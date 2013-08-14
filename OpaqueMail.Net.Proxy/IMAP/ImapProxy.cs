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
    public class ImapProxy : ProxyBase
    {
        #region Public Methods
        /// <summary>
        /// Start a IMAP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all IMAP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote IMAP server requires TLS/SSL.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl)
        {
            Start(acceptedIPs, localIPAddress, localPort, localEnableSsl, remoteServerHostName, remoteServerPort, remoteServerEnableSsl, null, "", LogLevel.None, 0);
        }

        /// <summary>
        /// Start a IMAP proxy instance.
        /// </summary>
        /// <param name="acceptedIPs">IP addresses to accept connections from.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="localEnableSsl">Whether the local server supports TLS/SSL.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all IMAP messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <param name="remoteServerEnableSsl">Whether the remote IMAP server requires TLS/SSL.</param>
        /// <param name="remoteServerCredential">(Optional) Credentials to be used for all connections to the remote IMAP server.  When set, this overrides any credentials passed locally.</param>
        /// <param name="logFile">File where event logs and exception information will be written.</param>
        /// <param name="logLevel">Proxy logging level, determining how much information is logged.</param>
        /// <param name="instanceId">The instance number of the proxy.</param>
        public void Start(string acceptedIPs, IPAddress localIPAddress, int localPort, bool localEnableSsl, string remoteServerHostName, int remoteServerPort, bool remoteServerEnableSsl, NetworkCredential remoteServerCredential, string logFile, LogLevel logLevel, int instanceId)
        {
            // Create the log writer.
            string logFileName = "";
            if (!string.IsNullOrEmpty(logFile))
            {
                logFileName = ProxyFunctions.GetLogFileName(logFile, instanceId, localIPAddress.ToString(), remoteServerHostName, localPort, remoteServerPort);
                LogWriter = new StreamWriter(logFileName, true, Encoding.UTF8, Constants.SMALLBUFFERSIZE);
                LogWriter.AutoFlush = true;

                LogLevel = logLevel;
            }

            // Make sure the remote server isn't an infinite loop back to this server.
            string fqdn = Functions.GetLocalFQDN();
            if (remoteServerHostName.ToUpper() == fqdn.ToUpper() && remoteServerPort == localPort)
            {
                ProxyFunctions.Log(LogWriter, SessionId, "Cannot start service because the remote server host name {" + remoteServerHostName + "} and port {" + remoteServerPort.ToString() + "} is the same as this proxy, which would cause an infinite loop.", Proxy.LogLevel.Critical, LogLevel);
                return;
            }
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress hostIP in hostEntry.AddressList)
            {
                if (remoteServerHostName == hostIP.ToString() && remoteServerPort == localPort)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Cannot start service because the remote server hostname {" + remoteServerHostName + "} and port {" + remoteServerPort.ToString() + "} is the same as this proxy, which would cause an infinite loop.", Proxy.LogLevel.Critical, LogLevel);
                    return;
                }
            }

            ProxyFunctions.Log(LogWriter, SessionId, "Starting service.", Proxy.LogLevel.Information, LogLevel);

            // Attempt to start up to 3 times in case another service using the port is shutting down.
            int startAttempts = 0;
            while (startAttempts < 3)
            {
                startAttempts++;

                // If we've failed to start once, wait an extra 10 seconds.
                if (startAttempts > 1)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Attempting to start for the " + (startAttempts == 2 ? "2nd" : "3rd") + " time.", Proxy.LogLevel.Information, LogLevel);
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
                        serverCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.LocalMachine, fqdn);
                        // In case the service as running as the current user, check the Current User certificate store as well.
                        if (serverCertificate == null)
                            serverCertificate = CertHelper.GetCertificateBySubjectName(StoreLocation.CurrentUser, fqdn);

                        // If no certificate was found, generate a self-signed certificate.
                        if (serverCertificate == null)
                        {
                            ProxyFunctions.Log(LogWriter, SessionId, "No signing certificate found, so generating new certificate.", Proxy.LogLevel.Warning, LogLevel);

                            List<string> oids = new List<string>();
                            oids.Add("1.3.6.1.5.5.7.3.1");    // Server Authentication.

                            // Generate the certificate with a duration of 10 years, 4096-bits, and a key usage of server authentication.
                            serverCertificate = CertHelper.CreateSelfSignedCertificate(fqdn, fqdn, true, 4096, 10, oids);

                            ProxyFunctions.Log(LogWriter, SessionId, "Certificate generated with Serial Number {" + serverCertificate.GetSerialNumberString() + "}.", Proxy.LogLevel.Information, LogLevel);
                        }
                    }

                    Listener = new TcpListener(localIPAddress, localPort);
                    Listener.Start();

                    ProxyFunctions.Log(LogWriter, SessionId, "Service started.", Proxy.LogLevel.Information, LogLevel);
                    ProxyFunctions.Log(LogWriter, SessionId, "Listening on address {" + localIPAddress.ToString() + "}, port {" + localPort + "}.", Proxy.LogLevel.Information, LogLevel);

                    Started = true;

                    // Accept client requests, forking each into its own thread.
                    while (Started)
                    {
                        TcpClient client = Listener.AcceptTcpClient();

                        string newLogFileName = ProxyFunctions.GetLogFileName(logFile, instanceId, localIPAddress.ToString(), remoteServerHostName, localPort, remoteServerPort);
                        if (newLogFileName != logFileName)
                        {
                            LogWriter.Close();
                            LogWriter = new StreamWriter(newLogFileName, true, Encoding.UTF8, Constants.SMALLBUFFERSIZE);
                            LogWriter.AutoFlush = true;
                        }

                        // Prepare the arguments for our new thread.
                        ImapProxyConnectionArguments arguments = new ImapProxyConnectionArguments();
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

                        // Increment the connection counter;
                        arguments.ConnectionId = (unchecked(++ConnectionId)).ToString();

                        // Fork the thread and continue listening for new connections.
                        Thread processThread = new Thread(new ParameterizedThreadStart(ProcessConnection));
                        processThread.Name = "OpaqueMail IMAP Proxy Connection";
                        processThread.Start(arguments);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Exception when starting proxy: " + ex.Message, Proxy.LogLevel.Critical, LogLevel);
                }
            }
        }

        /// <summary>
        /// Stop the IMAP proxy and close all existing connections.
        /// </summary>
        public void Stop()
        {
            ProxyFunctions.Log(LogWriter, SessionId, "Stopping service.", Proxy.LogLevel.Information, LogLevel);

            Started = false;

            if (Listener != null)
                Listener.Stop();

            ProxyFunctions.Log(LogWriter, SessionId, "Service stopped.", Proxy.LogLevel.Information, LogLevel);
        }

        /// <summary>
        /// Start all IMAP proxy instances from the specified settings file.
        /// </summary>
        /// <param name="fileName">File containing the IMAP proxy settings.</param>
        public static List<ImapProxy> StartProxiesFromSettingsFile(string fileName)
        {
            List<ImapProxy> imapProxies = new List<ImapProxy>();

            try
            {
                if (File.Exists(fileName))
                {
                    XPathDocument document = new XPathDocument(fileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    int imapServiceCount = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/IMAP/ServiceCount");
                    for (int i = 1; i <= imapServiceCount; i++)
                    {
                        ImapProxyArguments arguments = new ImapProxyArguments();
                        arguments.AcceptedIPs = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/AcceptedIPs");

                        string localIpAddress = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LocalIPAddress").ToUpper();
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

                        arguments.LocalPort = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/LocalPort");
                        // If the port is invalid, proceed to the next service instance.
                        if (arguments.LocalPort < 1)
                            continue;

                        arguments.LocalEnableSsl = ProxyFunctions.GetXmlBoolValue(navigator, "/Settings/IMAP/Service" + i + "/LocalEnableSSL");

                        arguments.RemoteServerHostName = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerHostName");
                        // If the host name is invalid, proceed to the next service instance.
                        if (string.IsNullOrEmpty(arguments.RemoteServerHostName))
                            continue;

                        arguments.RemoteServerPort = ProxyFunctions.GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerPort");
                        // If the port is invalid, proceed to the next service instance.
                        if (arguments.RemoteServerPort < 1)
                            continue;

                        arguments.RemoteServerEnableSsl = ProxyFunctions.GetXmlBoolValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerEnableSSL");

                        string remoteServerUsername = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerUsername");
                        if (!string.IsNullOrEmpty(remoteServerUsername))
                        {
                            arguments.RemoteServerCredential = new NetworkCredential();
                            arguments.RemoteServerCredential.UserName = remoteServerUsername;
                            arguments.RemoteServerCredential.Password = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerPassword");
                        }

                        string certificateLocationValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/Location");
                        StoreLocation certificateLocation = StoreLocation.LocalMachine;
                        if (certificateLocationValue.ToUpper() == "CURRENTUSER")
                            certificateLocation = StoreLocation.CurrentUser;

                        // Try to load the signing certificate based on its serial number first, then fallback to its subject name.
                        string certificateValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/SerialNumber");
                        if (!string.IsNullOrEmpty(certificateValue))
                            arguments.Certificate = CertHelper.GetCertificateBySerialNumber(certificateLocation, certificateValue);
                        else
                        {
                            certificateValue = ProxyFunctions.GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/SubjectName");
                            if (!string.IsNullOrEmpty(certificateValue))
                                arguments.Certificate = CertHelper.GetCertificateBySubjectName(certificateLocation, certificateValue);
                        }

                        arguments.LogFile = ProxyFunctions.GetXmlStringValue(navigator, "Settings/IMAP/Service" + i + "/LogFile");
                        
                        string logLevel = ProxyFunctions.GetXmlStringValue(navigator, "Settings/IMAP/Service" + i + "/LogLevel");
                        switch (logLevel.ToUpper())
                        {
                            case "NONE":
                                arguments.LogLevel = LogLevel.None;
                                break;
                            case "CRITICAL":
                                arguments.LogLevel = LogLevel.Critical;
                                break;
                            case "ERROR":
                                arguments.LogLevel = LogLevel.Error;
                                break;
                            case "RAW":
                                arguments.LogLevel = LogLevel.Raw;
                                break;
                            case "VERBOSE":
                                arguments.LogLevel = LogLevel.Verbose;
                                break;
                            case "WARNING":
                                arguments.LogLevel = LogLevel.Warning;
                                break;
                            case "INFORMATION":
                            default:
                                arguments.LogLevel = LogLevel.Information;
                                break;
                        }

                        arguments.InstanceId = i;

                        // Remember the proxy in order to close it when the service stops.
                        arguments.Proxy = new ImapProxy();
                        imapProxies.Add(arguments.Proxy);

                        Thread proxyThread = new Thread(new ParameterizedThreadStart(StartProxy));
                        proxyThread.Name = "OpaqueMail IMAP Proxy";
                        proxyThread.Start(arguments);
                    }
                }
            }
            catch
            {
                // Ignore errors if the XML settings file is malformed.
            }

            return imapProxies;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Handle an incoming IMAP connection, from connection to completion.
        /// </summary>
        /// <param name="parameters">ImapProxyConnectionArguments object containing all parameters for this connection.</param>
        private void ProcessConnection(object parameters)
        {
            // Cast the passed-in parameters back to their original objects.
            ImapProxyConnectionArguments arguments = (ImapProxyConnectionArguments)parameters;

            try
            {
                TcpClient client = arguments.TcpClient;
                Stream clientStream = client.GetStream();

                // Capture the client's IP information.
                PropertyInfo pi = clientStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
                string ip = ((Socket)pi.GetValue(clientStream, null)).RemoteEndPoint.ToString();
                if (ip.IndexOf(":") > -1)
                    ip = ip.Substring(0, ip.IndexOf(":"));

                // If the IP address range filter contains the localhost entry 0.0.0.0, check if the client IP is a local address and update it to 0.0.0.0 if so.
                if (arguments.AcceptedIPs.IndexOf("0.0.0.0") > -1)
                {
                    if (ip == "127.0.0.1")
                        ip = "0.0.0.0";
                    else
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
                }

                // Validate that the IP address is within an accepted range.
                if (!ProxyFunctions.ValidateIP(arguments.AcceptedIPs, ip))
                {
                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Connection rejected from {" + ip + "} due to its IP address.", Proxy.LogLevel.Warning, LogLevel);

                    Functions.SendStreamString(clientStream, new byte[Constants.SMALLBUFFERSIZE], "500 IP address [" + ip + "] rejected.\r\n");

                    if (clientStream != null)
                        clientStream.Dispose();
                    if (client != null)
                        client.Close();

                    return;
                }

                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "New connection established from {" + ip + "}.", Proxy.LogLevel.Information, LogLevel);

                // If supported, upgrade the session's security through a TLS handshake.
                if (arguments.LocalEnableSsl)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Starting local TLS/SSL protection for {" + ip + "}.", Proxy.LogLevel.Information, LogLevel);
                    clientStream = new SslStream(clientStream);
                    ((SslStream)clientStream).AuthenticateAsServer(arguments.Certificate);
                }

                // Connect to the remote server.
                TcpClient remoteServerClient = new TcpClient(arguments.RemoteServerHostName, arguments.RemoteServerPort);
                Stream remoteServerStream = remoteServerClient.GetStream();

                // If supported, upgrade the session's security through a TLS handshake.
                if (arguments.RemoteServerEnableSsl)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Starting remote TLS/SSL protection with {" + arguments.RemoteServerHostName + "}.", Proxy.LogLevel.Information, LogLevel);
                    remoteServerStream = new SslStream(remoteServerStream);
                    ((SslStream)remoteServerStream).AuthenticateAsClient(arguments.RemoteServerHostName);
                }

                // Relay server data to the client.
                TransmitArguments remoteServerToClientArguments = new TransmitArguments();
                remoteServerToClientArguments.ClientStream = remoteServerStream;
                remoteServerToClientArguments.RemoteServerStream = clientStream;
                remoteServerToClientArguments.IsClient = false;
                remoteServerToClientArguments.ConnectionId = ConnectionId.ToString();
                remoteServerToClientArguments.IPAddress = ip;
                Thread remoteServerToClientThread = new Thread(new ParameterizedThreadStart(RelayData));
                remoteServerToClientThread.Name = "OpaqueMail IMAP Proxy Server to Client";
                remoteServerToClientThread.Start(remoteServerToClientArguments);

                // Relay client data to the remote server.
                TransmitArguments clientToRemoteServerArguments = new TransmitArguments();
                clientToRemoteServerArguments.ClientStream = clientStream;
                clientToRemoteServerArguments.RemoteServerStream = remoteServerStream;
                clientToRemoteServerArguments.IsClient = true;
                clientToRemoteServerArguments.ConnectionId = ConnectionId.ToString();
                clientToRemoteServerArguments.IPAddress = ip;
                clientToRemoteServerArguments.Credential = arguments.RemoteServerCredential;
                Thread clientToRemoteServerThread = new Thread(new ParameterizedThreadStart(RelayData));
                clientToRemoteServerThread.Name = "OpaqueMail IMAP Proxy Client to Server";
                clientToRemoteServerThread.Start(clientToRemoteServerArguments);
            }
            catch (SocketException ex)
            {
                ProxyFunctions.Log(LogWriter, SessionId, "Exception communicating with {" + arguments.RemoteServerHostName + "} on port {" + arguments.RemoteServerPort + "}: " + ex.Message, Proxy.LogLevel.Error, LogLevel);
            }
            catch (Exception ex)
            {
                ProxyFunctions.Log(LogWriter, SessionId, "Exception: " + ex.Message, Proxy.LogLevel.Error, LogLevel);
            }
        }

        /// <summary>
        /// Relay data read from one connection to another.
        /// </summary>
        /// <param name="o">A TransmitArguments object containing local and remote server parameters.</param>
        private async void RelayData(object o)
        {
            // Cast the passed-in parameters back to their original objects.
            TransmitArguments arguments = (TransmitArguments)o;
            Stream clientStream = arguments.ClientStream;
            Stream remoteServerStream = arguments.RemoteServerStream;

            // A byte array to streamline bit shuffling.
            char[] buffer = new char[Constants.SMALLBUFFERSIZE];

            // Placeholder variables to track the current message being transmitted.
            bool inMessage = false;
            int messageLength = 0;
            StringBuilder messageBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            // The overall number of bytes transmitted on this connection.
            ulong bytesTransmitted = 0;
            int appendLength = 0, appendBytesTransmitted = 0;

            //  When a "[THROTTLED]" notice was last received.
            DateTime lastThrottleTime = new DateTime(1900, 1, 1);

            bool stillReceiving = true;
            try
            {
                using (StreamReader clientStreamReader = new StreamReader(clientStream))
                {
                    using (StreamWriter remoteServerStreamWriter = new StreamWriter(remoteServerStream))
                    {
                        remoteServerStreamWriter.AutoFlush = true;

                        while (Started && stillReceiving)
                        {
                            // Read data from the source and send it to its destination.
                            int bytesRead = await clientStreamReader.ReadAsync(buffer, 0, Constants.SMALLBUFFERSIZE);

                            if (bytesRead > 0)
                            {
                                // Cast the bytes received to a string.
                                string stringRead = new string(buffer, 0, bytesRead);
                                bytesTransmitted += (ulong)bytesRead;

                                // If this data comes from the client, log it.  Otherwise, process it.
                                if (arguments.IsClient)
                                {
                                    bool messageRelayed = false;

                                    string[] commandParts = stringRead.Split(new char[] { ' ' }, 4);
                                    if (commandParts.Length > 2)
                                    {
                                        // If we're in an APPEND command, wait until we've completed the append before logging another command.
                                        if (appendLength > 0)
                                        {
                                            appendBytesTransmitted += stringRead.Length;
                                            if (appendBytesTransmitted >= appendLength)
                                                LastCommandReceived = "";
                                        }
                                        else
                                        {
                                            // Optionally replace credentials with those from our settings file.
                                            if (arguments.Credential != null && commandParts[1] == "LOGIN" && commandParts.Length == 4)
                                            {
                                                await remoteServerStreamWriter.WriteAsync(commandParts[0] + " " + commandParts[1] + " " + arguments.Credential.UserName + " " + arguments.Credential.Password + "\r\n");

                                                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId.ToString(), "C: " + commandParts[0] + " " + commandParts[1] + " " + arguments.Credential.UserName + " " + arguments.Credential.Password, Proxy.LogLevel.Raw, LogLevel);

                                                messageRelayed = true;
                                            }

                                            if (commandParts[1] == "UID")
                                                LastCommandReceived = commandParts[1] + " " + commandParts[2];
                                            else
                                                LastCommandReceived = commandParts[1];

                                            if (LogLevel == Proxy.LogLevel.Verbose)
                                                ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId.ToString(), "Command {" + LastCommandReceived + "} processed.", Proxy.LogLevel.Verbose, LogLevel);

                                            // If we're in an APPEND command, remember how many bytes we should receive.
                                            if (LastCommandReceived == "APPEND")
                                            {
                                                appendLength = 0;
                                                int.TryParse(Functions.ReturnBetween(stringRead, "{", "}"), out appendLength);
                                                appendBytesTransmitted = stringRead.Length - stringRead.IndexOf("\r\n") - 2;
                                            }
                                        }
                                    }

                                    if (!messageRelayed)
                                    {
                                        await remoteServerStreamWriter.WriteAsync(buffer, 0, bytesRead);

                                        ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId.ToString(), "C: " + stringRead, Proxy.LogLevel.Raw, LogLevel);
                                    }
                                }
                                else
                                {
                                    await remoteServerStreamWriter.WriteAsync(buffer, 0, bytesRead);

                                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId.ToString(), "S: " + stringRead, Proxy.LogLevel.Raw, LogLevel);

                                    // If we're currently receiving a message, check to see if it's completed.
                                    if (inMessage)
                                    {
                                        messageBuilder.Append(stringRead);
                                        if (messageBuilder.Length >= messageLength)
                                        {
                                            // If the message has been completed and it contains a signature, process it.
                                            string message = messageBuilder.ToString(0, messageLength);
                                            if (message.IndexOf("application/x-pkcs7-signature") > -1 || message.IndexOf("application/pkcs7-mime") > -1)
                                            {
                                                Thread processThread = new Thread(new ParameterizedThreadStart(ProcessMessage));
                                                processThread.Name = "OpaqueMail IMAP Proxy Signature Processor";
                                                ProcessMessageArguments processMessageArguments = new ProcessMessageArguments();
                                                processMessageArguments.MessageText = message;
                                                processMessageArguments.ConnectionId = ConnectionId.ToString();
                                                processThread.Start(processMessageArguments);
                                            }

                                            // We're no longer receiving a message, so continue.
                                            inMessage = false;
                                            stringRead = messageBuilder.ToString(messageLength, messageBuilder.Length - messageLength);
                                            messageBuilder.Clear();
                                        }
                                    }

                                    if (!inMessage)
                                    {
                                        if (stringRead.IndexOf("[THROTTLED]\r\n", StringComparison.Ordinal) > -1)
                                        {
                                            if (DateTime.Now - lastThrottleTime >= new TimeSpan(0, 20, 0))
                                            {
                                                ProxyFunctions.Log(LogWriter, SessionId, ConnectionId.ToString(), "Connection speed throttled by the remote server.", Proxy.LogLevel.Warning, LogLevel);
                                                lastThrottleTime = DateTime.Now;
                                            }
                                        }

                                        int pos = 0;
                                        while (pos > -1)
                                        {
                                            // Messages are denoted by FETCH headers with their lengths in curly braces.
                                            pos = stringRead.IndexOf(" FETCH ", pos);
                                            if (pos > -1)
                                            {
                                                int openBrace = stringRead.IndexOf("{", pos);
                                                int lineBreak = stringRead.IndexOf("\r", pos);
                                                if (lineBreak < -1)
                                                    lineBreak = stringRead.Length + 1;

                                                if (openBrace > -1 && openBrace < lineBreak)
                                                {
                                                    int closeBrace = stringRead.IndexOf("}", openBrace);
                                                    if (closeBrace > -1)
                                                    {
                                                        // Only proceed if we can parse the size of the message.
                                                        if (int.TryParse(stringRead.Substring(openBrace + 1, closeBrace - openBrace - 1), out messageLength))
                                                        {
                                                            int messageBytesRead = stringRead.Length - closeBrace - 3;

                                                            if (messageBytesRead > messageLength)
                                                            {
                                                                string message = stringRead.Substring(closeBrace + 3, messageLength);
                                                                if (message.IndexOf("application/x-pkcs7-signature") > -1 || message.IndexOf("application/pkcs7-mime") > -1)
                                                                {
                                                                    Thread processThread = new Thread(new ParameterizedThreadStart(ProcessMessage));
                                                                    processThread.Name = "OpaqueMail IMAP Proxy Signature Processor";
                                                                    ProcessMessageArguments processMessageArguments = new ProcessMessageArguments();
                                                                    processMessageArguments.MessageText = message;
                                                                    processMessageArguments.ConnectionId = ConnectionId.ToString();
                                                                    processThread.Start(processMessageArguments);
                                                                }
                                                                pos = closeBrace + 3 + messageLength;
                                                            }
                                                            else
                                                            {
                                                                inMessage = true;
                                                                messageBuilder.Clear();
                                                                if (stringRead.Length > closeBrace + 3)
                                                                    messageBuilder.Append(stringRead.Substring(closeBrace + 3));
                                                                pos = -1;
                                                            }
                                                        }
                                                        else
                                                            pos = -1;
                                                    }
                                                    else
                                                        pos = -1;
                                                }
                                                else
                                                    pos = -1;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                stillReceiving = false;
                        }
                    }
                }
            }
/*            catch (IOException)
            {
                // Ignore either stream being closed.
            }*/
            catch (ObjectDisposedException)
            {
                // Ignore either stream being closed.
            }
            catch (Exception ex)
            {
                // Log other exceptions.
                ProxyFunctions.Log(LogWriter, SessionId, "Exception while transmitting data: " + ex.Message, Proxy.LogLevel.Error, LogLevel);
            }
            finally
            {
                // If sending to the local client, log the connection being closed.
                if (!arguments.IsClient)
                    ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Connection from {" + arguments.IPAddress + "} closed after transmitting {" + bytesTransmitted.ToString("N0") + "} bytes.", Proxy.LogLevel.Information, LogLevel);

                if (clientStream != null)
                    clientStream.Dispose();
                if (remoteServerStream != null)
                    remoteServerStream.Dispose();
            }
        }

        /// <summary>
        /// Process a transmitted message to import any signing certificates for subsequent S/MIME encryption.
        /// </summary>
        /// <param name="o">A ProcessMessageArguments object containing message parameters.</param>
        private void ProcessMessage(object o)
        {
            ProcessMessageArguments arguments = (ProcessMessageArguments)o;

            // Only parse the message if it contains a known S/MIME content type.
            string canonicalMessageText = arguments.MessageText.ToLower();
            if (canonicalMessageText.IndexOf("application/x-pkcs7-signature") > -1 || canonicalMessageText.IndexOf("application/pkcs7-mime") > -1)
            {
                try
                {
                    // Parse the message.
                    ReadOnlyMailMessage message = new ReadOnlyMailMessage(arguments.MessageText);

                    // If the message contains a signing certificate that we haven't processed on this session, import it.
                    if (message.SmimeSigningCertificate != null && !SmimeCertificatesReceived.Contains(message.SmimeSigningCertificate))
                    {
                        // Import the certificate to the Local Machine store.
                        ProxyFunctions.Log(LogWriter, SessionId, arguments.ConnectionId, "Importing certificate with Serial Number {" + message.SmimeSigningCertificate.SerialNumber + "}.", Proxy.LogLevel.Information, LogLevel);
                        CertHelper.InstallWindowsCertificate(message.SmimeSigningCertificate, StoreLocation.LocalMachine);

                        // Remember this ceriticate to avoid importing it again this session.
                        SmimeCertificatesReceived.Add(message.SmimeSigningCertificate);
                    }
                }
                catch (Exception ex)
                {
                    ProxyFunctions.Log(LogWriter, SessionId, "Exception while processing message: " + ex.Message, Proxy.LogLevel.Error, LogLevel);
                }
            }
        }

        /// <summary>
        /// Start an individual IMAP proxy on its own thread.
        /// </summary>
        /// <param name="parameters">ImapProxyArguments object containing all parameters for this connection.</param>
        private static void StartProxy(object parameters)
        {
            ImapProxyArguments arguments = (ImapProxyArguments)parameters;

            // Start the proxy using passed-in settings.
            arguments.Proxy.Start(arguments.AcceptedIPs, arguments.LocalIpAddress, arguments.LocalPort, arguments.LocalEnableSsl, arguments.RemoteServerHostName, arguments.RemoteServerPort, arguments.RemoteServerEnableSsl, arguments.RemoteServerCredential, arguments.LogFile, arguments.LogLevel, arguments.InstanceId);
        }
        #endregion Private Methods
    }
}
