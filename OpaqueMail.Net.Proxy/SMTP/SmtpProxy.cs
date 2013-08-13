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
using System.Xml.XPath;

namespace OpaqueMail.Net.Proxy
{
    public class SmtpProxy : ProxyBase
    {
        #region Private Members
        private Dictionary<X509Certificate2, DateTime> CertificateReminders = new Dictionary<X509Certificate2, DateTime>();
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Start an SMTP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all SMTP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote SMTP server requires TLS/SSL.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl)
        {
            Start(acceptedIPs, localIPAddress, localPort, localEnableSsl, remoteServerHostName, remoteServerPort, remoteServerEnableSsl, null, SmimeSettingsMode.BestEffort, true, true, true, true, true, "", 0);
        }

        /// <summary>
        /// Start an SMTP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all SMTP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote SMTP server requires TLS/SSL.</param>
        /// <param name="remoteServerCredential">(Optional) Credentials to be used for all connections to the remote SMTP server.  When set, this overrides any credentials passed locally.</param>
        /// <param name="logFile">File where event logs and exception information will be written.</param>
        /// <param name="instanceId">The instance number of the proxy.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl, NetworkCredential remoteServerCredential, string logFile, int instanceId)
        {
            Start(acceptedIPs, localIPAddress, localPort, localEnableSsl, remoteServerHostName, remoteServerPort, remoteServerEnableSsl, remoteServerCredential, SmimeSettingsMode.BestEffort, true, true, true, true, true, logFile, instanceId);
        }

        /// <summary>
        /// Start an SMTP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all SMTP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote SMTP server requires TLS/SSL.</param>
        /// <param name="remoteServerCredential">(Optional) Credentials to be used for all connections to the remote SMTP server.  When set, this overrides any credentials passed locally.</param>
        /// <param name="smimeSettingsMode">Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</param>
        /// <param name="smimeEncryptedEnvelope">Whether the e-mail's envelope should be encrypted.  When SmimeSign is true, encryption is the second S/MIME operation.</param>
        /// <param name="smimeSigned">Whether the e-mail should be signed.  When true, signing is the first S/MIME operation</param>
        /// <param name="smimeTripleWrapped">Whether the e-mail should be triple-wrapped by signing, then encrypting the envelope, then signing the encrypted envelope.</param>
        /// <param name="smimeRemovePreviousOperations">Remove envelope encryption and signatures from passed-in messages.  If true and SmimeSigned or SmimeEncryptEnvelope is also true, new S/MIME operations will be applied.</param>
        /// <param name="sendCertificateReminders">Send e-mail reminders when a signing certificate is due to expire within 30 days.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl, NetworkCredential remoteServerCredential, SmimeSettingsMode smimeSettingsMode, bool smimeSigned, bool smimeEncryptedEnvelope, bool smimeTripleWrapped, bool smimeRemovePreviousOperations, bool sendCertificateReminders)
        {
            Start(acceptedIPs, localIPAddress, localPort, localEnableSsl, remoteServerHostName, remoteServerPort, remoteServerEnableSsl, remoteServerCredential, smimeSettingsMode, smimeSigned, smimeEncryptedEnvelope, smimeTripleWrapped, smimeRemovePreviousOperations, sendCertificateReminders, "", 0);
        }

        /// <summary>
        /// Start an SMTP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all SMTP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote SMTP server requires TLS/SSL.</param>
        /// <param name="remoteServerCredential">(Optional) Credentials to be used for all connections to the remote SMTP server.  When set, this overrides any credentials passed locally.</param>
        /// <param name="smimeSettingsMode">Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</param>
        /// <param name="smimeEncryptedEnvelope">Whether the e-mail's envelope should be encrypted.  When SmimeSign is true, encryption is the second S/MIME operation.</param>
        /// <param name="smimeSigned">Whether the e-mail should be signed.  When true, signing is the first S/MIME operation</param>
        /// <param name="smimeTripleWrapped">Whether the e-mail should be triple-wrapped by signing, then encrypting the envelope, then signing the encrypted envelope.</param>
        /// <param name="smimeRemovePreviousOperations">Remove envelope encryption and signatures from passed-in messages.  If true and SmimeSigned or SmimeEncryptEnvelope is also true, new S/MIME operations will be applied.</param>
        /// <param name="sendCertificateReminders">Send e-mail reminders when a signing certificate is due to expire within 30 days.</param>
        /// <param name="logFile">File where event logs and exception information will be written.</param>
        /// <param name="instanceId">The instance number of the proxy.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl, NetworkCredential remoteServerCredential, SmimeSettingsMode smimeSettingsMode, bool smimeSigned, bool smimeEncryptedEnvelope, bool smimeTripleWrapped, bool smimeRemovePreviousOperations, bool sendCertificateReminders, string logFile, int instanceId)
        {
            // Create the log writer.
            string logFileName = "";
            if (!string.IsNullOrEmpty(logFile))
            {
                logFileName = ProxyFunctions.GetLogFileName(logFile, instanceId);
                LogWriter = new StreamWriter(logFileName, true, Encoding.UTF8, Constants.SMALLBUFFERSIZE);
            }

            ProxyFunctions.Log(LogWriter, SessionId, "Starting service.");

            // Attempt to start up to 3 times in case another service using the port is shutting down.
            int startAttempts = 0;
            while (startAttempts < 3)
            {
                startAttempts++;

                // If we've failed to start once, wait an extra 10 seconds.
                if (startAttempts > 1)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Attempting to start for the " + (startAttempts == 2 ? "2nd" : "3rd") + " time.");
                    Thread.Sleep(10000 * startAttempts);
                }

                try
                {
                    X509Certificate serverCertificate = null;

                    // Generate a unique session ID for logging.
                    SessionId = Guid.NewGuid().ToString();
                    ConnectionId = 0;

                    // If local SSL is supported via STARTTLS, ensure we have a valid server certificate.
                    if (localEnableSsl)
                    {
                        string fqdn = Functions.GetLocalFQDN();
                        serverCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.LocalMachine, fqdn);
                        // In case the service as running as the current user, check the Current User certificate store as well.
                        if (serverCertificate == null)
                            serverCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.CurrentUser, fqdn);

                        // If no certificate was found, generate a self-signed certificate.
                        if (serverCertificate == null)
                        {
                            ProxyFunctions.Log(LogWriter, SessionId, "No signing certificate found, so generating new certificate.");

                            List<string> oids = new List<string>();
                            oids.Add("1.3.6.1.5.5.7.3.1");    // Server Authentication.

                            // Generate the certificate with a duration of 10 years, 4096-bits, and a key usage of server authentication.
                            serverCertificate = CertHelper.CreateSelfSignedCertificate(fqdn, fqdn, true, 4096, 10, oids);

                            ProxyFunctions.Log(LogWriter, SessionId, "New certificate generated with Serial Number {" + Encoding.UTF8.GetString(serverCertificate.GetSerialNumber()) + "}.");
                        }
                    }

                    // Start listening on the specified port and IP address.
                    Listener = new TcpListener(localIPAddress, localPort);
                    Listener.Start();

                    ProxyFunctions.Log(LogWriter, SessionId, "Service started.");
                    ProxyFunctions.Log(LogWriter, SessionId, "Listening on address {" + localIPAddress.ToString() + "}, port {" + localPort + "}.");

                    Started = true;

                    // Accept client requests, forking each into its own thread.
                    while (Started)
                    {
                        TcpClient client = Listener.AcceptTcpClient();

                        string newLogFileName = ProxyFunctions.GetLogFileName(logFile, instanceId);
                        if (newLogFileName != logFileName)
                        {
                            LogWriter.Close();
                            LogWriter = new StreamWriter(newLogFileName, true, Encoding.UTF8, Constants.SMALLBUFFERSIZE);
                            LogWriter.AutoFlush = true;
                        }

                        // Prepare the arguments for our new thread.
                        SmtpProxyConnectionArguments arguments = new SmtpProxyConnectionArguments();
                        arguments.AcceptedIPs = acceptedIPs;
                        arguments.TcpClient = client;
                        arguments.Certificate = serverCertificate;
                        arguments.LocalIpAddress = localIPAddress;
                        arguments.LocalPort = localPort;
                        arguments.LocalEnableSsl = localEnableSsl;
                        arguments.RemoteServerHostName = remoteServerHostName;
                        arguments.RemoteServerPort = remoteServerPort;
                        arguments.RemoteServerEnableSsl = remoteServerEnableSsl;
                        arguments.RemoteServerCredential = remoteServerCredential;

                        arguments.SmimeSettingsMode = smimeSettingsMode;
                        arguments.SmimeSigned = smimeSigned;
                        arguments.SmimeEncryptedEnvelope = smimeEncryptedEnvelope;
                        arguments.SmimeTripleWrapped = smimeTripleWrapped;
                        arguments.SmimeRemovePreviousOperations = smimeRemovePreviousOperations;

                        arguments.SendCertificateReminders = sendCertificateReminders;

                        // Increment the connection counter;
                        arguments.ConnectionId = (unchecked(++ConnectionId)).ToString();

                        // Fork the thread and continue listening for new connections.
                        Thread processThread = new Thread(new ParameterizedThreadStart(ProcessConnection));
                        processThread.Name = "OpaqueMail SMTP Proxy Connection";
                        processThread.Start(arguments);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Exception when starting proxy: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Stop the SMTP proxy and close all existing connections.
        /// </summary>
        public void Stop()
        {
            ProxyFunctions.Log(LogWriter, SessionId, "Stopping service.");

            Started = false;

            if (Listener != null)
                Listener.Stop();

            ProxyFunctions.Log(LogWriter, SessionId, "Service stopped.");
        }

        /// <summary>
        /// Start all SMTP proxy instances from the specified settings file.
        /// </summary>
        /// <param name="fileName">File containing the SMTP proxy settings.</param>
        public static List<SmtpProxy> StartProxiesFromSettingsFile(string fileName)
        {
            List<SmtpProxy> smtpProxies = new List<SmtpProxy>();

            try
            {
                if (File.Exists(fileName))
                {
                    XPathDocument document = new XPathDocument(fileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    int smtpServiceCount = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/SMTP/ServiceCount");
                    for (int i = 1; i <= smtpServiceCount; i++)
                    {
                        SmtpProxyArguments arguments = new SmtpProxyArguments();
                        arguments.AcceptedIPs = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/AcceptedIPs");

                        string localIpAddress = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LocalIPAddress").ToUpper();
                        switch (localIpAddress)
                        {
                            // Treat blank values as "Any".
                            case "":
                            case "ANY":
                                arguments.LocalIpAddress = IPAddress.Any;
                                break;
                            case "BROADCAST":
                                arguments.LocalIpAddress = IPAddress.Broadcast;
                                break;
                            case "IPV6ANY":
                                arguments.LocalIpAddress = IPAddress.IPv6Any;
                                break;
                            case "IPV6LOOPBACK":
                                arguments.LocalIpAddress = IPAddress.IPv6Loopback;
                                break;
                            case "LOOPBACK":
                                arguments.LocalIpAddress = IPAddress.Loopback;
                                break;
                            default:
                                // Try to parse the local IP address.  If unable to, proceed to the next service instance.
                                if (!IPAddress.TryParse(localIpAddress, out arguments.LocalIpAddress))
                                    continue;
                                break;
                        }

                        arguments.LocalPort = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/LocalPort");
                        // If the port is invalid, proceed to the next service instance.
                        if (arguments.LocalPort < 1)
                            continue;

                        arguments.LocalEnableSsl = ProxyFunctions.GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/LocalEnableSSL");

                        arguments.RemoteServerHostName = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerHostName");
                        // If the host name is invalid, proceed to the next service instance.
                        if (string.IsNullOrEmpty(arguments.RemoteServerHostName))
                            continue;

                        arguments.RemoteServerPort = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerPort");
                        // If the port is invalid, proceed to the next service instance.
                        if (arguments.RemoteServerPort < 1)
                            continue;

                        arguments.RemoteServerEnableSsl = ProxyFunctions.GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerEnableSSL");

                        string remoteServerUsername = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerUsername");
                        if (!string.IsNullOrEmpty(remoteServerUsername))
                        {
                            arguments.RemoteServerCredential = new NetworkCredential();
                            arguments.RemoteServerCredential.UserName = remoteServerUsername;
                            arguments.RemoteServerCredential.Password = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerPassword");
                        }

                        string certificateLocationValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/Location");
                        StoreLocation certificateLocation = StoreLocation.LocalMachine;
                        if (certificateLocationValue.ToUpper() == "CURRENTUSER")
                            certificateLocation = StoreLocation.CurrentUser;

                        // Try to load the signing certificate based on its serial number first, then fallback to its subject name.
                        string certificateValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/SerialNumber");
                        if (!string.IsNullOrEmpty(certificateValue))
                            arguments.Certificate = CertHelper.GetCertificateBySerialNumber(certificateLocation, certificateValue);
                        else
                        {
                            certificateValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/SubjectName");
                            if (!string.IsNullOrEmpty(certificateValue))
                                arguments.Certificate = CertHelper.GetCertificateBySubjectName(certificateLocation, certificateValue);
                        }

                        arguments.SendCertificateReminders = ProxyFunctions.GetXmlBoolValue(navigator, "Settings/SMTP/Service" + i + "/SendCertificateReminders");

                        arguments.SmimeEncryptedEnvelope = ProxyFunctions.GetXmlBoolValue(navigator, "Settings/SMTP/Service" + i + "/SMIMEEncrypt");
                        arguments.SmimeRemovePreviousOperations = ProxyFunctions.GetXmlBoolValue(navigator, "Settings/SMTP/Service" + i + "/SMIMERemovePreviousOperations");
                        arguments.SmimeSigned = ProxyFunctions.GetXmlBoolValue(navigator, "Settings/SMTP/Service" + i + "/SMIMESign");
                        arguments.SmimeTripleWrapped = ProxyFunctions.GetXmlBoolValue(navigator, "Settings/SMTP/Service" + i + "/SMIMETripleWrap");

                        // Look up the S/MIME settings mode, defaulting to requiring exact settings.
                        string smimeSettingsMode = ProxyFunctions.GetXmlStringValue(navigator, "Settings/SMTP/Service" + i + "/SMIMESettingsMode");
                        if (smimeSettingsMode.ToUpper() == "BESTEFFORT")
                            arguments.SmimeSettingsMode = SmimeSettingsMode.BestEffort;
                        else
                            arguments.SmimeSettingsMode = SmimeSettingsMode.RequireExactSettings;

                        arguments.LogFile = ProxyFunctions.GetXmlStringValue(navigator, "Settings/SMTP/Service" + i + "/LogFile");
                        arguments.InstanceId = i;

                        // Remember the proxy in order to close it when the service stops.
                        arguments.Proxy = new SmtpProxy();
                        smtpProxies.Add(arguments.Proxy);

                        Thread proxyThread = new Thread(new ParameterizedThreadStart(StartProxy));
                        proxyThread.Name = "OpaqueMail SMTP Proxy";
                        proxyThread.Start(arguments);
                    }
                }
            }
            catch
            {
            }

            return smtpProxies;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Handle an incoming SMTP connection, from connection to completion.
        /// </summary>
        /// <param name="parameters">SmtpProxyConnectionArguments object containing all parameters for this connection.</param>
        private async void ProcessConnection(object parameters)
        {
            // Cast the passed-in parameters back to their original objects.
            SmtpProxyConnectionArguments arguments = (SmtpProxyConnectionArguments)parameters;

            // The overall number of bytes transmitted on this connection.
            ulong bytesTransmitted = 0;
            try
            {
                TcpClient client = arguments.TcpClient;
                Stream clientStream = client.GetStream();

                // Placeholder variables to be populated throughout the client session.
                NetworkCredential credential = arguments.RemoteServerCredential;
                string fromAddress = "";
                string identity = "";
                string ip = "";
                List<string> toList = new List<string>();
                bool sending = false, inPlainAuth = false, inLoginAuth = false;

                // A byte array to streamline bit shuffling.
                byte[] buffer = new byte[Constants.SMALLBUFFERSIZE];

                // Capture the client's IP information.
                PropertyInfo pi = clientStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
                ip = ((Socket)pi.GetValue(clientStream, null)).RemoteEndPoint.ToString();
                if (ip.IndexOf(":") > -1)
                    ip = ip.Substring(0, ip.IndexOf(":"));

                // If the IP address range filter contains the localhost entry 0.0.0.0, check if the client IP is a local address and update it to 0.0.0.0 if so.
                if (arguments.AcceptedIPs.IndexOf("0.0.0.0") > -1)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress hostIP in hostEntry.AddressList)
                    {
                        if (hostIP.ToString() == ip)
                        {
                            ip = "0.0.0.0";
                            break;
                        }
                    }
                }

                // Validate that the IP address is within an accepted range.
                if (!ProxyFunctions.ValidateIP(arguments.AcceptedIPs, ip))
                {
                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Connection rejected from {" + ip + "} due to its IP address.");

                    await Functions.SendStreamStringAsync(clientStream, buffer, "500 IP address [" + ip + "] rejected.\r\n");

                    if (clientStream != null)
                        clientStream.Dispose();
                    if (client != null)
                        client.Close();

                    return;
                }

                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "New connection established from {" + ip + "}.");

                // Send our welcome message.
                await Functions.SendStreamStringAsync(clientStream, buffer, "220 " + WelcomeMessage + "\r\n");

                // Instantiate an SmtpClient for sending messages to the remote server.
                using (SmtpClient smtpClient = new SmtpClient(arguments.RemoteServerHostName, arguments.RemoteServerPort))
                {
                    smtpClient.EnableSsl = arguments.RemoteServerEnableSsl;
                    smtpClient.Credentials = arguments.RemoteServerCredential;

                    // Loop through each received command.
                    string command = "";
                    while (client.Connected)
                    {
                        int bytesRead = await clientStream.ReadAsync(buffer, 0, Constants.SMALLBUFFERSIZE);
                        bytesTransmitted += (ulong)bytesRead;

                        command += Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (command.EndsWith("\r\n"))
                        {
                            // Handle continuations of current "DATA" commands.
                            if (sending)
                            {
                                // Handle the finalization of a "DATA" command.
                                if (command.EndsWith("\r\n.\r\n"))
                                {
                                    sending = false;

                                    string messageFrom = "", messageSubject = "", messageSize = "";
                                    try
                                    {
                                        ReadOnlyMailMessage message = new ReadOnlyMailMessage(command.Substring(0, command.Length - 5), ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody);

                                        // If the received message is already signed or encrypted and we don't want to remove previous S/MIME operations, forward it as-is.
                                        string contentType = message.ContentType;
                                        if ((contentType.StartsWith("application/pkcs7-mime") || contentType.StartsWith("application/x-pkcs7-mime") || contentType.StartsWith("application/x-pkcs7-signature")) && !arguments.SmimeRemovePreviousOperations)
                                        {
                                            message.SmimeSigned = message.SmimeEncryptedEnvelope = message.SmimeTripleWrapped = false;
                                            await smtpClient.SendAsync(message);
                                        }
                                        else
                                        {
                                            messageFrom = message.From.Address;
                                            messageSubject = message.Subject;
                                            messageSize = message.Size.ToString();

                                            ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Forwarding message from {" + message.From.Address + "} with subject {" + message.Subject + "} and size of {" + message.Size + "}.");

                                            foreach (string toListAddress in toList)
                                            {
                                                if (!message.AllRecipients.Contains(toListAddress))
                                                {
                                                    message.AllRecipients.Add(toListAddress);
                                                    message.Bcc.Add(toListAddress);
                                                }
                                            }

                                            // Attempt to sign and encrypt the envelopes of all messages, but still send if unable to.
                                            message.SmimeSettingsMode = SmimeSettingsMode.BestEffort;

                                            // Apply S/MIME settings.
                                            message.SmimeSigned = arguments.SmimeSigned;
                                            message.SmimeEncryptedEnvelope = arguments.SmimeEncryptedEnvelope;
                                            message.SmimeTripleWrapped = arguments.SmimeTripleWrapped;

                                            // Look up the S/MIME signing certificate for the current sender.  If it doesn't exist, create one.
                                            message.SmimeSigningCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.LocalMachine, message.From.Address);
                                            if (message.SmimeSigningCertificate == null)
                                                message.SmimeSigningCertificate = CertHelper.CreateSelfSignedCertificate("E=" + message.From.Address, message.From.Address, true, 4096, 10);

                                            // Send the message.
                                            await smtpClient.SendAsync(message.AsMailMessage());

                                            // Check the signing certificate's expiration to determine if we should send a reminder.
                                            if (arguments.SendCertificateReminders && message.SmimeSigningCertificate != null)
                                            {
                                                string expirationDateString = message.SmimeSigningCertificate.GetExpirationDateString();
                                                TimeSpan expirationTime = DateTime.Parse(expirationDateString) - DateTime.Now;
                                                if (expirationTime.TotalDays < 30)
                                                {
                                                    bool sendReminder = true;
                                                    if (CertificateReminders.ContainsKey(message.SmimeSigningCertificate))
                                                    {
                                                        TimeSpan timeSinceLastReminder = DateTime.Now - CertificateReminders[message.SmimeSigningCertificate];
                                                        if (timeSinceLastReminder.TotalHours < 24)
                                                            sendReminder = false;
                                                    }

                                                    // Send the reminder message.
                                                    if (sendReminder)
                                                    {
                                                        MailMessage reminderMessage = new MailMessage(message.From, message.From);
                                                        reminderMessage.Subject = "OpaqueMail: S/MIME Certificate Expires " + expirationDateString;
                                                        reminderMessage.Body = "Your OpaqueMail S/MIME Certificate will expire in " + ((int)expirationTime.TotalDays) + " days on " + expirationDateString + ".\r\n\r\n" +
                                                            "Certificate Subject Name: " + message.SmimeSigningCertificate.Subject + "\r\n" +
                                                            "Certificate Serial Number: " + message.SmimeSigningCertificate.SerialNumber + "\r\n" +
                                                            "Certificate Issuer: " + message.SmimeSigningCertificate.Issuer + "\r\n\r\n" +
                                                            "Please renew or enroll a new certificate to continue protecting your e-mail privacy.\r\n\r\n" +
                                                            "This is an automated message sent from the OpaqueMail Proxy on " + Functions.GetLocalFQDN() + ".  " +
                                                            "For more information, visit http://opaquemail.org/.";

                                                        reminderMessage.SmimeEncryptedEnvelope = message.SmimeEncryptedEnvelope;
                                                        reminderMessage.SmimeEncryptionOptionFlags = message.SmimeEncryptionOptionFlags;
                                                        reminderMessage.SmimeSettingsMode = message.SmimeSettingsMode;
                                                        reminderMessage.SmimeSigned = message.SmimeSigned;
                                                        reminderMessage.SmimeSigningCertificate = message.SmimeSigningCertificate;
                                                        reminderMessage.SmimeSigningOptionFlags = message.SmimeSigningOptionFlags;
                                                        reminderMessage.SmimeTripleWrapped = message.SmimeTripleWrapped;

                                                        ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Certificate with Serial Number {" + message.SmimeSigningCertificate.SerialNumber + "} expiring.  Sending reminder to {" + message.From.Address + "}.");

                                                        await smtpClient.SendAsync(reminderMessage);

                                                        CertificateReminders[message.SmimeSigningCertificate] = DateTime.Now;
                                                    }
                                                }
                                            }
                                        }

                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 Forwarded\r\n");
                                        ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Message from {" + message.From.Address + "} with subject {" + message.Subject + "} and size of {" + message.Size + "} successfully forwarded.");
                                    }
                                    catch (Exception ex)
                                    {
                                        // Report if an exception was encountering sending the message.
                                        Functions.SendStreamString(clientStream, buffer, "500 Error occurred when forwarding\r\n");

                                        ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Error when forwarding message from {" + messageFrom + "} with subject {" + messageSubject + "} and size of {" + messageSize + "}.  Exception: " + ex.Message);
                                    }
                                    command = "";
                                }
                            }
                            // Handle continuations of current "AUTH PLAIN" commands.
                            else if (inPlainAuth)
                            {
                                inPlainAuth = false;
                                // Split up an AUTH PLAIN handshake into its components.
                                string authString = Encoding.UTF8.GetString(Convert.FromBase64String(command));
                                string[] authStringParts = authString.Split(new char[] { '\0' }, 3);
                                if (authStringParts.Length > 2 && arguments.RemoteServerCredential == null)
                                    smtpClient.Credentials = new NetworkCredential(authStringParts[1], authStringParts[2]);

                                await Functions.SendStreamStringAsync(clientStream, buffer, "235 OK\r\n");
                                command = "";
                            }
                            // Handle continuations of current "AUTH LOGIN" commands.
                            else if (inLoginAuth)
                            {
                                if (smtpClient.Credentials == null)
                                {
                                    // Handle the username being received for the first time.
                                    smtpClient.Credentials = new NetworkCredential();
                                    ((NetworkCredential)smtpClient.Credentials).UserName = Functions.FromBase64(command.Substring(0, command.Length - 2));
                                    await Functions.SendStreamStringAsync(clientStream, buffer, "334 UGFzc3dvcmQ6\r\n");
                                }
                                else
                                {
                                    // Handle the password.
                                    inLoginAuth = false;
                                    ((NetworkCredential)smtpClient.Credentials).Password = Functions.FromBase64(command.Substring(0, command.Length - 2));
                                    await Functions.SendStreamStringAsync(clientStream, buffer, "235 OK\r\n");
                                }
                                command = "";
                            }
                            else
                            {
                                // Otherwise, look at the verb of the incoming command.
                                string[] commandParts = command.Substring(0, command.Length - 2).Replace("\r", "").Split(new char[] { ' ' }, 2);

                                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Command {" + commandParts[0] + "} received.");
                                switch (commandParts[0].ToUpper())
                                {
                                    case "AUTH":
                                        // Support authentication.
                                        if (commandParts.Length > 1)
                                        {
                                            commandParts = command.Substring(0, command.Length - 2).Replace("\r", "").Split(new char[] { ' ' });
                                            switch (commandParts[1].ToUpper())
                                            {
                                                case "PLAIN":
                                                    // Prepare to handle a continuation command.
                                                    inPlainAuth = true;
                                                    await Functions.SendStreamStringAsync(clientStream, buffer, "334 Proceed\r\n");
                                                    break;
                                                case "LOGIN":
                                                    inLoginAuth = true;
                                                    if (commandParts.Length > 2)
                                                    {
                                                        // Parse the username and request a password.
                                                        smtpClient.Credentials = new NetworkCredential();
                                                        ((NetworkCredential)smtpClient.Credentials).UserName = Functions.FromBase64(commandParts[2]);
                                                        await Functions.SendStreamStringAsync(clientStream, buffer, "334 UGFzc3dvcmQ6\r\n");
                                                    }
                                                    else
                                                    {
                                                        // Request a username only.
                                                        await Functions.SendStreamStringAsync(clientStream, buffer, "334 VXNlcm5hbWU6\r\n");
                                                    }
                                                    break;
                                                default:
                                                    // Split up an AUTH PLAIN handshake into its components.
                                                    string authString = Encoding.UTF8.GetString(Convert.FromBase64String(commandParts[1].Substring(6)));
                                                    string[] authStringParts = authString.Split(new char[] { '\0' }, 3);
                                                    if (authStringParts.Length > 2 && arguments.RemoteServerCredential == null)
                                                        smtpClient.Credentials = new NetworkCredential(authStringParts[1], authStringParts[2]);

                                                    await Functions.SendStreamStringAsync(clientStream, buffer, "235 OK\r\n");
                                                    break;
                                            }
                                        }
                                        else
                                            await Functions.SendStreamStringAsync(clientStream, buffer, "500 Unknown verb\r\n");
                                        break;
                                    case "DATA":
                                        // Prepare to handle continuation data.
                                        sending = true;
                                        command = command.Substring(6);
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "354 Send message content; end with <CRLF>.<CRLF>\r\n");
                                        break;
                                    case "EHLO":
                                        // Proceed with the login and send a list of supported commands.
                                        if (commandParts.Length > 1)
                                            identity = commandParts[1] + " ";
                                        if (arguments.LocalEnableSsl)
                                            await Functions.SendStreamStringAsync(clientStream, buffer, "250-Hello " + identity + "[" + ip + "], please proceed\r\n250-AUTH LOGIN PLAIN\r\n250-RSET\r\n250 STARTTLS\r\n");
                                        else
                                            await Functions.SendStreamStringAsync(clientStream, buffer, "250-Hello " + identity + "[" + ip + "], please proceed\r\n250-AUTH LOGIN PLAIN\r\n250 RSET\r\n");
                                        break;
                                    case "HELO":
                                        // Proceed with the login.
                                        if (commandParts.Length > 1)
                                            identity = commandParts[1];
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 Hello " + identity + " [" + ip + "], please proceed\r\n");
                                        break;
                                    case "MAIL":
                                    case "SAML":
                                    case "SEND":
                                    case "SOML":
                                        // Accept the from address.
                                        if (commandParts.Length > 1 && commandParts[1].Length > 5)
                                            fromAddress = commandParts[1].Substring(5);
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 OK\r\n");
                                        break;
                                    case "NOOP":
                                        // Prolong the current session.
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 Still here\r\n");
                                        break;
                                    case "PASS":
                                        // Support authentication.
                                        if (commandParts.Length > 1 && arguments.RemoteServerCredential == null)
                                            ((NetworkCredential)smtpClient.Credentials).Password = commandParts[1];
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "235 OK\r\n");
                                        break;
                                    case "QUIT":
                                        // Wait one second then force the current connection closed.
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "221 Bye\r\n");
                                        Thread.Sleep(1000);
                                        if (clientStream != null)
                                            clientStream.Dispose();
                                        if (client != null)
                                            client.Close();
                                        break;
                                    case "RCPT":
                                        // Acknolwedge recipients.
                                        if (commandParts.Length > 1 && commandParts[1].Length > 6)
                                            toList.Add(commandParts[1].Substring(5, commandParts[1].Length - 6));
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 OK\r\n");
                                        break;
                                    case "RSET":
                                        // Reset the current message arguments.
                                        fromAddress = "";
                                        toList.Clear();

                                        await Functions.SendStreamStringAsync(clientStream, buffer, "250 OK\r\n");
                                        break;
                                    case "STARTTLS":
                                        // If supported, upgrade the session's security through a TLS handshake.
                                        if (arguments.LocalEnableSsl)
                                        {
                                            await Functions.SendStreamStringAsync(clientStream, buffer, "220 Go ahead\r\n");
                                            if (!(clientStream is SslStream))
                                            {
                                                clientStream = new SslStream(clientStream);
                                                ((SslStream)clientStream).AuthenticateAsServer(arguments.Certificate);
                                            }
                                        }
                                        else
                                            await Functions.SendStreamStringAsync(clientStream, buffer, "500 Unknown verb\r\n");
                                        break;
                                    case "USER":
                                        // Support authentication.
                                        if (commandParts.Length > 1 && arguments.RemoteServerCredential == null)
                                            ((NetworkCredential)smtpClient.Credentials).UserName = commandParts[1];
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "235 OK\r\n");
                                        break;
                                    case "VRFY":
                                        // Notify that we can't verify addresses.
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "252 I'm just a proxy\r\n");
                                        break;
                                    default:
                                        await Functions.SendStreamStringAsync(clientStream, buffer, "500 Unknown verb\r\n");
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

                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Connection from {" + ip + "} closed after transmitting {" + bytesTransmitted.ToString("N0") + "} bytes.");
            }
            catch (SocketException ex)
            {
                ProxyFunctions.Log(LogWriter, SessionId, "Exception communicating with {" + arguments.RemoteServerHostName + "} on port {" + arguments.RemoteServerPort + "}: " + ex.Message);
            }
            catch (Exception ex)
            {
                ProxyFunctions.Log(LogWriter, SessionId, "Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Start an individual SMTP proxy on its own thread.
        /// </summary>
        /// <param name="parameters">SmtpProxyArguments object containing all parameters for this connection.</param>
        private static void StartProxy(object parameters)
        {
            SmtpProxyArguments arguments = (SmtpProxyArguments)parameters;

            // Start the proxy using passed-in settings.
            arguments.Proxy.Start(arguments.AcceptedIPs, arguments.LocalIpAddress, arguments.LocalPort, arguments.LocalEnableSsl, arguments.RemoteServerHostName, arguments.RemoteServerPort, arguments.RemoteServerEnableSsl, arguments.RemoteServerCredential, arguments.SmimeSettingsMode, arguments.SmimeSigned, arguments.SmimeEncryptedEnvelope, arguments.SmimeTripleWrapped, arguments.SmimeRemovePreviousOperations, arguments.SendCertificateReminders, arguments.LogFile, arguments.InstanceId);
        }
        #endregion Private Methods
    }
}
