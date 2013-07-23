using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpaqueMail.Net.Proxy
{
    public class SmtpProxy
    {
        /// <summary>Welcome message to be displayed when connecting.</summary>
        public string WelcomeMessage = "OpaqueMail Proxy";

        #region Structs
        /// <summary>
        /// Arguments passed in when instantiating a new SMTP proxy instance.
        /// </summary>
        public struct SmtpProxyArguments
        {
            /// <summary>TCP connection to the client.</summary>
            public TcpClient TcpClient;
            /// <summary>Certificate to authenticate the server.</summary>
            public X509Certificate Certificate;
            /// <summary>Local IP address to bind to.</summary>
            public IPAddress LocalIpAddress;
            /// <summary>Local IP address to listen on.</summary>
            public int LocalPort;
            /// <summary>Whether the local server supports TLS/SSL.</summary>
            public bool LocalEnableSsl;
            /// <summary>Destination hostname to forward all SMTP messages to.</summary>
            public string DestinationHostName;
            /// <summary>Destination port to connect to.</summary>
            public int DestinationPort;
            /// <summary>Whether the destination SMTP server requires TLS/SSL.</summary>
            public bool DestinationEnableSsl;

            /// <summary>Encrypt the e-mail's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.</summary>
            public bool SmimeEncryptedEnvelope;
            /// <summary>Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</summary>
            public SmimeSettingsMode SmimeSettingsMode;
            /// <summary>Sign the e-mail.  When true, signing is the first S/MIME operation.</summary>
            public bool SmimeSigned;
            /// <summary>Triple-wrap the e-mail by signing, then encrypting the envelope, then signing the encrypted envelope.</summary>
            public bool SmimeTripleWrapped;
        }
        #endregion Structs

        public void StartSmtpProxy(IPAddress localIPAddress, int localPort, bool localEnableSsl, string destinationHostName, int destinationPort, bool destinationEnableSsl)
        {
            StartSmtpProxy(localIPAddress, localPort, localEnableSsl, destinationHostName, destinationPort, destinationEnableSsl, SmimeSettingsMode.PreferSettingsOnly, true, true, true);
        }

        public void StartSmtpProxy(IPAddress localIPAddress, int localPort, bool localEnableSsl, string destinationHostName, int destinationPort, bool destinationEnableSsl, SmimeSettingsMode smimeSettingsMode, bool smimeSigned, bool smimeEncryptedEnvelope, bool smimeTripleWrapped)
        {
            X509Certificate serverCertificate = null;

            // If local SSL is supported via STARTTLS, ensure we have a valid server certificate.
            if (localEnableSsl)
            {
                string fqdn = Functions.FQDN();
                serverCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.LocalMachine, fqdn);

                // If no certificate was found, generate a self-signed certificate.
                if (serverCertificate == null)
                {
                    List<string> oids = new List<string>();
                    oids.Add("1.3.6.1.5.5.7.3.1");    // Server Authentication.

                    // Generate the certificate with a duration of 10 years, 4096-bits, and a key usage of server authentication.
                    serverCertificate = CertHelper.CreateSelfSignedCertificate(fqdn, true, 4096, 10, oids);
                }
            }

            // Start listening on the specified port and IP address.
            TcpListener listener = new TcpListener(localIPAddress, localPort);
            listener.Start();

            // Accept client requests, forking each into its own thread.
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                // Prepare the arguments for our new thread.
                SmtpProxyArguments arguments = new SmtpProxyArguments();
                arguments.TcpClient = client;
                arguments.Certificate = serverCertificate;
                arguments.LocalIpAddress = localIPAddress;
                arguments.LocalPort = localPort;
                arguments.LocalEnableSsl = localEnableSsl;
                arguments.DestinationHostName = destinationHostName;
                arguments.DestinationPort = destinationPort;
                arguments.DestinationEnableSsl = destinationEnableSsl;

                arguments.SmimeSettingsMode = smimeSettingsMode;
                arguments.SmimeSigned = smimeSigned;
                arguments.SmimeEncryptedEnvelope = smimeEncryptedEnvelope;
                arguments.SmimeTripleWrapped = smimeTripleWrapped;

                // Fork the thread and continue listening for new connections.
                Thread t = new Thread(new ParameterizedThreadStart(HandleSocket));
                t.Start(arguments);
            }
        }

        /// <summary>
        /// Handle an incoming SMTP connection, from connection to completion.
        /// </summary>
        /// <param name="parameters">SmtpProxyArguments object containing all parameters for this connection.</param>
        private void HandleSocket(object parameters)
        {
            // Cast the passed-in parameters back to their original objects.
            SmtpProxyArguments arguments = (SmtpProxyArguments)parameters;
            TcpClient client = arguments.TcpClient;
            Stream clientStream = client.GetStream();

            // Placeholder variables to be populated throughout the client session.
            NetworkCredential credential = new NetworkCredential();
            string fromAddress = "";
            string identity = "";
            string ip = "";
            List<string> toList = new List<string>();
            bool sending = false, inAuth = false;

            // A byte array to streamline bit shuffling.
            byte[] buffer = new byte[Constants.BUFFERSIZE];

            // Capture the client's IP information.
            PropertyInfo pi = clientStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
            ip = ((Socket)pi.GetValue(clientStream, null)).RemoteEndPoint.ToString();
            if (ip.IndexOf(":") > -1)
                ip = ip.Substring(0, ip.IndexOf(":"));

            // Send our welcome message.
            Functions.SendStreamString(clientStream, buffer, "220 " + WelcomeMessage + "\r\n");

            // Instantiate an SmtpClient for sending messages to the destination server.
            using (SmtpClient smtpClient = new SmtpClient(arguments.DestinationHostName, arguments.DestinationPort))
            {
                smtpClient.EnableSsl = arguments.DestinationEnableSsl;
                smtpClient.Credentials = new NetworkCredential();

                // Loop through each received command.
                string command = "";
                while (client.Connected)
                {
                    command += Functions.ReadStreamString(clientStream, buffer, Constants.BUFFERSIZE);

                    if (command.EndsWith("\r\n"))
                    {
                        // Handle continuations of current "DATA" commands.
                        if (sending)
                        {
                            // Handle the finalization of a "DATA" command.
                            if (command.EndsWith("\r\n.\r\n"))
                            {
                                sending = false;

                                try
                                {
                                    ReadOnlyMailMessage message = new ReadOnlyMailMessage(command.Substring(0, command.Length - 5));

                                    foreach (string toListAddress in toList)
                                    {
                                        if (!message.AllRecipients.Contains(toListAddress))
                                        {
                                            message.AllRecipients.Add(toListAddress);
                                            message.Bcc.Add(toListAddress);
                                        }
                                    }

                                    // Attempt to sign and encrypt the envelopes of all messages, but still send if unable to.
                                    message.SmimeSettingsMode = SmimeSettingsMode.PreferSettingsOnly;

                                    // Apply S/MIME settings.
                                    message.SmimeSigned = arguments.SmimeSigned;
                                    message.SmimeEncryptedEnvelope = arguments.SmimeEncryptedEnvelope;
                                    message.SmimeTripleWrapped = arguments.SmimeTripleWrapped;

                                    // Look up the S/MIME signing certificate for the current sender.  If it doesn't exist, create one.
                                    message.SmimeSigningCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.LocalMachine, message.From.Address);
                                    if (message.SmimeSigningCertificate == null)
                                        message.SmimeSigningCertificate = CertHelper.CreateSelfSignedCertificate(message.From.Address, true, 4096, 10);

                                    // Send the message.
                                    MailMessage mailMessage = message.AsMailMessage();
                                    smtpClient.Send(mailMessage);
                                    Functions.SendStreamString(clientStream, buffer, "250 Forwarded\r\n");
                                }
                                catch (Exception)
                                {
                                    // Report if an exception was encountering sending the message.
                                    Functions.SendStreamString(clientStream, buffer, "500 Error occurred when forwarding\r\n");
                                }
                                command = "";
                            }
                        }
                        // Handle continuations of current "AUTH" commands.
                        else if (inAuth)
                        {
                            inAuth = false;
                            // Split up an AUTH PLAIN handshake into its components.
                            string authString = Encoding.UTF8.GetString(Convert.FromBase64String(command));
                            string[] authStringParts = authString.Split(new char[] { '\0' }, 3);
                            if (authStringParts.Length > 2)
                                smtpClient.Credentials = new NetworkCredential(authStringParts[1], authStringParts[2]);

                            Functions.SendStreamString(clientStream, buffer, "235 OK\r\n");
                            command = "";
                        }
                        else
                        {
                            // Otherwise, look at the verb of the incoming command.
                            string[] commandParts = command.Substring(0, command.Length - 2).Replace("\r", "").Split(new char[] { ' ' }, 2);
                            switch (commandParts[0].ToUpper())
                            {
                                case "AUTH":
                                    // Support authentication.
                                    if (commandParts.Length > 1)
                                    {
                                        // Only support PLAIN authentication.
                                        if (commandParts[1].ToUpper() == "PLAIN")
                                        {
                                            // Prepare to handle a continuation command.
                                            inAuth = true;
                                            Functions.SendStreamString(clientStream, buffer, "334 Proceed\r\n");
                                        }
                                        else
                                        {
                                            // Split up an AUTH PLAIN handshake into its components.
                                            string authString = Encoding.UTF8.GetString(Convert.FromBase64String(commandParts[1].Substring(6)));
                                            string[] authStringParts = authString.Split(new char[] { '\0' }, 3);
                                            if (authStringParts.Length > 2)
                                                smtpClient.Credentials = new NetworkCredential(authStringParts[1], authStringParts[2]);

                                            Functions.SendStreamString(clientStream, buffer, "235 OK\r\n");
                                        }
                                    }
                                    else
                                        Functions.SendStreamString(clientStream, buffer, "500 Unknown verb\r\n");
                                    break;
                                case "DATA":
                                    // Prepare to handle continuation data.
                                    sending = true;
                                    command = command.Substring(6);
                                    Functions.SendStreamString(clientStream, buffer, "354 Send message content; end with <CRLF>.<CRLF>\r\n");
                                    break;
                                case "EHLO":
                                    // Proceed with the login and send a list of supported commands.
                                    if (commandParts.Length > 1)
                                        identity = commandParts[1];
                                    if (arguments.LocalEnableSsl)
                                        Functions.SendStreamString(clientStream, buffer, "250-Hello " + identity + " [" + ip + "], please proceed\r\n250-AUTH PLAIN\r\n250-RSET\r\n250 STARTTLS\r\n");
                                    else
                                        Functions.SendStreamString(clientStream, buffer, "250-Hello " + identity + " [" + ip + "], please proceed\r\n250-AUTH PLAIN\r\n250 RSET\r\n");
                                    break;
                                case "HELO":
                                    // Proceed with the login.
                                    if (commandParts.Length > 1)
                                        identity = commandParts[1];
                                    Functions.SendStreamString(clientStream, buffer, "250 Hello " + identity + " [" + ip + "], please proceed\r\n");
                                    break;
                                case "MAIL":
                                case "SAML":
                                case "SEND":
                                case "SOML":
                                    // Accept the from address.
                                    if (commandParts.Length > 1)
                                        fromAddress = commandParts[1].Substring(5);
                                    Functions.SendStreamString(clientStream, buffer, "250 OK\r\n");
                                    break;
                                case "NOOP":
                                    // Prolong the current session.
                                    Functions.SendStreamString(clientStream, buffer, "250 Still here\r\n");
                                    break;
                                case "PASS":
                                    // Support authentication.
                                    if (commandParts.Length > 1)
                                        ((NetworkCredential)smtpClient.Credentials).Password = commandParts[1];
                                    Functions.SendStreamString(clientStream, buffer, "235 OK\r\n");
                                    break;
                                case "QUIT":
                                    // Wait one second then force the current connection closed.
                                    Functions.SendStreamString(clientStream, buffer, "221 Bye\r\n");
                                    Thread.Sleep(1000);
                                    if (clientStream != null)
                                        clientStream.Dispose();
                                    if (client != null)
                                        client.Close();
                                    break;
                                case "RCPT":
                                    // Acknolwedge recipients.
                                    if (commandParts.Length > 1)
                                        toList.Add(commandParts[1].Substring(5, commandParts[1].Length - 6));
                                    Functions.SendStreamString(clientStream, buffer, "250 OK\r\n");
                                    break;
                                case "RSET":
                                    // Reset the current message arguments.
                                    fromAddress = "";
                                    toList.Clear();

                                    Functions.SendStreamString(clientStream, buffer, "250 OK\r\n");
                                    break;
                                case "STARTTLS":
                                    // If supported, upgrade the session's security through a TLS handshake.
                                    if (arguments.LocalEnableSsl)
                                    {
                                        Functions.SendStreamString(clientStream, buffer, "250 Go ahead\r\n");
                                        if (!(clientStream is SslStream))
                                        {
                                            clientStream = new SslStream(clientStream);
                                            ((SslStream)clientStream).AuthenticateAsServer(arguments.Certificate);
                                        }
                                    }
                                    else
                                        Functions.SendStreamString(clientStream, buffer, "500 Unknown verb\r\n");
                                    break;
                                case "USER":
                                    // Support authentication.
                                    if (commandParts.Length > 1)
                                        ((NetworkCredential)smtpClient.Credentials).UserName = commandParts[1];
                                    Functions.SendStreamString(clientStream, buffer, "235 OK\r\n");
                                    break;
                                case "VRFY":
                                    // Notify that we can't verify addresses.
                                    Functions.SendStreamString(clientStream, buffer, "252 I'm just a proxy\r\n");
                                    break;
                                default:
                                    Functions.SendStreamString(clientStream, buffer, "500 Unknown verb\r\n");
                                    break;
                            }

                            command = "";
                        }
                    }
                }
            }
            // Clean up after any unexpectedly closed connections.
            if (clientStream != null)
                clientStream.Dispose();
            if (client != null)
                client.Close();
        }
    }
}
