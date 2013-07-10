using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Imap
{
    /// <summary>
    /// Allows applications to retrieve and manage e-mail by using the Internet Message Access Protocol (IMAP).
    /// Includes OpaqueMail extensions to facilitate processing of secure S/MIME messages.
    /// </summary>
    public partial class ImapClient : IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.ImapClient class by using the specified settings.
        /// </summary>
        public ImapClient(string host, int port, string userName, string password, bool enableSSL)
        {
            Host = host;
            Port = port;
            Credentials = new NetworkCredential(userName, password);
            EnableSsl = enableSSL;
        }
        #endregion Constructors

        #region Public Members
        /// <summary>Set of extended IMAP capabilities.</summary>
        public HashSet<string> Capabilities = new HashSet<string>();
        /// <summary>Gets or sets the credentials used to authenticate.</summary>
        public NetworkCredential Credentials;
        /// <summary>Specify whether the OpaqueMail.ImapClient uses Secure Sockets Layer (SSL).</summary>
        public bool EnableSsl;
        /// <summary>Gets or sets the name or IP address of the host used for IMAP transactions.</summary>
        public string Host;
        /// <summary>Version of IMAP reported by the server.</summary>
        public string ImapVersion = "";
        /// <summary>Whether the session has successfully been authenticated.</summary>
        public bool IsAuthenticated
        {
            get
            {
                if (IsConnected)
                    return SessionIsAuthenticated;
                else
                    return false;
            }
        }
        /// <summary>Determine whether the current session is still alive.</summary>
        public bool IsConnected
        {
            get
            {
                if (ImapTcpClient != null)
                    return ImapTcpClient.Connected;
                else
                    return false;
            }
        }
        /// <summary>Whether the session has explicitly entered the IDLE state.</summary>
        public bool IsIdle
        {
            get
            {
                return SessionIsIdle;
            }
        }
        /// <summary>Whether a mailbox has been selected in the session.</summary>
        public bool IsMailboxSelected
        {
            get
            {
                return SessionIsMailboxSelected;
            }
        }
        /// <summary>Whether to keep the session open through periodic NOOP messages.</summary>
        public bool KeepAlive
        {
            get
            {
                return SessionKeepAlive;
            }
            set
            {
                SessionKeepAlive = value;
            }
        }
        /// <summary>The last command issued to the IMAP server.</summary>
        public string LastCommandIssued;
        /// <summary>Whether the last IMAP command was successful.</summary>
        public bool LastCommandResult = false;
        /// <summary>The last error message returned by the IMAP server.</summary>
        public string LastErrorMessage;
        /// <summary>Gets or sets the port used for IMAP transactions.</summary>
        public int Port;
        /// <summary>Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</summary>
        public ReadOnlyMailMessageProcessingFlags ProcessingFlags = ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody;
        /// <summary>A unique string used to tag commands in the current IMAP session.</summary>
        public string SessionCommandTag = ".";
        /// <summary>Gets or sets a value, in milliseconds, that determines how long the stream will attempt to read before timing out.</summary>
        public int ReadTimeout
        {
            get
            {
                if (ImapStream != null)
                    return ImapStream.ReadTimeout;
                else
                    return -1;
            }
            set
            {
                if (ImapStream != null)
                    ImapStream.ReadTimeout = value;
            }
        }
        /// <summary>List of unprocessed messages returned by the server.</summary>
        public List<string> UnexpectedServerMessages = new List<string>();
        /// <summary>The welcome message provided by the IMAP server.</summary>
        public string WelcomeMessage
        {
            get { return SessionWelcomeMessage; }
        }
        /// <summary>Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.</summary>
        public int WriteTimeout
        {
            get
            {
                if (ImapStream != null)
                    return ImapStream.WriteTimeout;
                else
                    return -1;
            }
            set
            {
                if (ImapStream != null)
                    ImapStream.WriteTimeout = value;
            }
        }
        #endregion Public Members

        #region Private Members
        /// <summary>Command counter for uniquely identifying IMAP commands.</summary>
        private int CommandTagCounter = 0;
        /// <summary>Remote IMAP mailbox currently being accessed.</summary>
        private Mailbox CurrentMailbox;
        /// <summary>Name of the remote IMAP mailbox currently being accessed.</summary>
        private string CurrentMailboxName = "INBOX";
        /// <summary>Connection to the remote IMAP server.</summary>
        private TcpClient ImapTcpClient;
        /// <summary>Stream for communicating with the IMAP server.</summary>
        private Stream ImapStream;
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] InternalBuffer = new byte[Constants.BUFFERSIZE];
        /// <summary>The authentication state when capabilities were last queried.</summary>
        private bool LastCapabilitiesCheckAuthenticationState = false;
        /// <summary>Whether the session has successfully been authenticated.</summary>
        private bool SessionIsAuthenticated = false;
        /// <summary>Whether the session has explicitly entered the IDLE state.</summary>
        private bool SessionIsIdle = false;
        /// <summary>Whether a mailbox has been selected in the session.</summary>
        private bool SessionIsMailboxSelected = false;
        /// <summary>Whether to keep the session open through periodic NOOP messages.</summary>
        private bool SessionKeepAlive = false;
        /// <summary>The welcome message provided by the IMAP server.</summary>
        private String SessionWelcomeMessage = "";
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Add one or more flags to a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public bool AddFlagsToMessage(string mailboxName, int index, string[] flags)
        {
            return AddFlagsToMessageHelper(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Add one or more flags to a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public bool AddFlagsToMessageUid(string mailboxName, int uid, string[] flags)
        {
            return AddFlagsToMessageHelper(mailboxName, uid, flags, true);
        }

        /// <summary>
        /// Perform an authentication handshake with the IMAP server.
        /// </summary>
        public bool Authenticate()
        {
            string response = ReadData("*");
            if (!LastCommandResult)
                return false;

            SessionWelcomeMessage = response.Substring(5, response.Length - 7);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "LOGIN " + Credentials.UserName + " " + Credentials.Password + "\r\n");
            response = ReadData(commandTag);
            if (!LastCommandResult)
                return false;

            // If capabilities are returned while logging in, remember them.
            if (response.StartsWith("* CAPABILITY "))
            {
                // Strip the capability prefix.
                response = response.Substring(13);

                int firstSpace = response.IndexOf(" ");
                string[] capabilitiesList = response.Substring(firstSpace + 1).Split(' ');

                // Cache the capabilities.
                ImapVersion = response.Substring(0, firstSpace);
                InitializeCapabilities(capabilitiesList);
                LastCapabilitiesCheckAuthenticationState = true;

                foreach (string capability in capabilitiesList)
                    Capabilities.Add(capability);
            }

            SessionIsAuthenticated = true;
            return true;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        public bool AppendMessage(string mailboxName, string message)
        {
            return AppendMessage(mailboxName, message, new string[] { }, null);
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public bool AppendMessage(string mailboxName, string message, string[] flags, DateTime? date)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Ensure the message has an ending carriage return and line feed.
            if (!message.EndsWith("\r\n"))
                message += "\r\n";

            // Create the initial APPEND command.
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append("APPEND " + mailboxName + " ");

            // If flags are specified, add them as parameters.
            if (flags != null)
            {
                if (flags.Length > 0)
                {
                    commandBuilder.Append("(");
                    bool firstFlag = true;
                    foreach (string flag in flags)
                    {
                        if (!firstFlag)
                            commandBuilder.Append(" ");
                        commandBuilder.Append(flag);
                        firstFlag = false;
                    }
                    commandBuilder.Append(") ");
                }
            }

            // If a date is specified, add it as a parameter.
            if (date != null)
                commandBuilder.Append("\"" + ((DateTime)date).ToString("dd-MM-yyyy hh:mm:ss") + " " + ((DateTime)date).ToString("zzzz").Replace(":", "") + "\" ");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            // Complete the initial command send it.
            commandBuilder.Append("{" + message.Length + "}\r\n");
            SendCommand(commandTag, commandBuilder.ToString());

            // Confirm the server is ready to accept our raw data.
            string response = Functions.ReadStreamString(ImapStream, InternalBuffer);
            if (response.StartsWith("+"))
            {
                Functions.SendStreamString(ImapStream, InternalBuffer, message + "\r\n");
                response = ReadData(commandTag);

                return LastCommandResult;
            }

            return false;
        }

        /// <summary>
        /// Request a checkpoint of the currently selected mailbox.
        /// </summary>
        public bool Check()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "CHECK\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Close the currently selected mailbox and remove all messages with the "\Deleted" flag.
        /// </summary>
        /// <returns></returns>
        public bool CloseMailbox()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "CLOSE\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Connect to the remote IMAP server.
        /// </summary>
        public bool Connect()
        {
            try
            {
                ImapTcpClient = new TcpClient();
                ImapTcpClient.Connect(Host, Port);

                if (EnableSsl)
                {
                    ImapStream = new SslStream(ImapTcpClient.GetStream());
                    StartTLS();
                }
                else
                    ImapStream = ImapTcpClient.GetStream();

                return true;
            }
            catch (Exception)
            {
                if (ImapStream != null)
                    ImapStream.Dispose();
                if (ImapTcpClient != null)
                    ImapTcpClient.Close();

                return false;
            }
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public bool CopyMessage(string sourceMailboxName, int index, string destMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "COPY " + index.ToString() + " " + destMailboxName + "\r\n");
            ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public bool CopyMessageUid(string sourceMailboxName, int uid, string destMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "UID COPY " + uid.ToString() + " " + destMailboxName + "\r\n");
            ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Create a mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The given name for the new mailbox.</param>
        public bool CreateMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Disallow wildcard characters in mailbox names;
            if (mailboxName.Contains("*") || mailboxName.Contains("%"))
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "CREATE " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="index">Index of the message to delete.</param>
        public bool DeleteMessage(string mailboxName, int index)
        {
            return AddFlagsToMessageHelper(mailboxName, index, new string[] { "\\Deleted" }, false);
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="uid">UID of the message to delete.</param>
        public bool DeleteMessageUid(string mailboxName, int uid)
        {
            return AddFlagsToMessageHelper(mailboxName, uid, new string[] { "\\Deleted" }, true);
        }

        /// <summary>
        /// Delete a mailbox from the server.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to delete.</param>
        public bool DeleteMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "DELETE " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Sends a LOGOUT message to the IMAP server, gracefully ends the TCP connection, and releases all resources used by the current instances of the OpaqueMail.ImapClient class.
        /// </summary>
        public void Dispose()
        {
            if (ImapTcpClient != null)
            {
                if (ImapTcpClient.Connected)
                    LogOut();
                ImapTcpClient.Close();
            }
            if (ImapStream != null)
                ImapStream.Dispose();
        }

        /// <summary>
        /// Notify the IMAP server that the client supports the specified capability.
        /// </summary>
        /// <param name="capabilityName">Name of the capability to enable.</param>
        public bool EnableCapability(string capabilityName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "ENABLE " + capabilityName + "\r\n");
            string response = ReadData(commandTag);
            return LastCommandResult;
        }

        /// <summary>
        /// Examine a mailbox, returning its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        public Mailbox ExamineMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return null;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "EXAMINE " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            if (LastCommandResult)
                return new Mailbox(mailboxName, response);
            else
                return null;
        }

        /// <summary>
        /// Remove all messages from the current mailbox that have the "\Deleted" flag.
        /// </summary>
        public bool ExpungeMailbox()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "EXPUNGE\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Retrieve a list of the IMAP's servers extended capabilities.
        /// </summary>
        /// <param name="imapVersion">String representing the server's IMAP version.</param>
        public string[] GetCapabilities(out string imapVersion)
        {
            // If we've logged in or out since last checking capabilities, ignore the cache.
            if (LastCapabilitiesCheckAuthenticationState == IsAuthenticated)
            {
                // Send pre-cached capabilities if they exist.
                if (Capabilities.Count > 0)
                {
                    imapVersion = ImapVersion;
                    return Capabilities.ToArray();
                }
            }

            LastCapabilitiesCheckAuthenticationState = IsAuthenticated;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "CAPABILITY\r\n");
            string response = ReadData(commandTag);

            imapVersion = "";
            if (response.StartsWith("* CAPABILITY "))
            {
                // Strip the capability prefix.
                response = response.Substring(13);

                int firstSpace = response.IndexOf(" ");
                imapVersion = response.Substring(0, firstSpace);
                string[] capabilitiesList = response.Substring(firstSpace + 1).Split(' ');

                // Cache the capabilities.
                ImapVersion = imapVersion;
                InitializeCapabilities(capabilitiesList);

                return capabilitiesList;
            }
            else
                return new string[] { };
        }

        /// <summary>
        /// Load an instance of a message based on its index.
        /// </summary>
        /// <param name="index">The index of the message to load.</param>
        public ReadOnlyMailMessage GetMessage(int index)
        {
            return GetMessageHelper(CurrentMailboxName, index, false, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The index of the message to load.</param>
        public ReadOnlyMailMessage GetMessage(string mailboxName, int index)
        {
            return GetMessageHelper(mailboxName, index, false, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public ReadOnlyMailMessage GetMessage(string mailboxName, int index, bool headersOnly)
        {
            return GetMessageHelper(mailboxName, index, headersOnly, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        public ReadOnlyMailMessage GetMessage(string mailboxName, int index, bool headersOnly, bool setSeenFlag)
        {
            return GetMessageHelper(mailboxName, index, headersOnly, setSeenFlag, false);
        }

        /// <summary>
        /// Load an instance of a message based on its UID.
        /// </summary>
        /// <param name="uid">The UID of the message to load.</param>
        public ReadOnlyMailMessage GetMessageUid(int uid)
        {
            return GetMessageHelper(CurrentMailboxName, uid, false, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        public ReadOnlyMailMessage GetMessageUid(string mailboxName, int uid)
        {
            return GetMessageHelper(mailboxName, uid, false, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public ReadOnlyMailMessage GetMessageUid(string mailboxName, int uid, bool headersOnly)
        {
            return GetMessageHelper(mailboxName, uid, headersOnly, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        public ReadOnlyMailMessage GetMessageUid(string mailboxName, int uid, bool headersOnly, bool setSeenFlag)
        {
            return GetMessageHelper(mailboxName, uid, headersOnly, setSeenFlag, true);
        }

        /// <summary>
        /// Return the number of messages in the current mailbox.
        /// </summary>
        public int GetMessageCount()
        {
            return GetMessageCount(CurrentMailboxName);
        }

        /// <summary>
        /// Return the number of messages in a specific mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox to examine.</param>
        public int GetMessageCount(string mailbox)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return -1;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "STATUS " + mailbox + " (messages)\r\n");
            string response = ReadData(commandTag);
            response = Functions.ReturnBetween(response, "(MESSAGES ", ")");
            
            int numMessages = -1;
            int.TryParse(response, out numMessages);

            return numMessages;
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the current mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        public List<ReadOnlyMailMessage> GetMessages()
        {
            return GetMessages(CurrentMailboxName, 25, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName)
        {
            return GetMessages(mailboxName, 25, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count)
        {
            return GetMessages(CurrentMailboxName, count, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName, int count)
        {
            return GetMessages(mailboxName, count, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count, bool headersOnly)
        {
            return GetMessages(CurrentMailboxName, count, 1, false, headersOnly, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailbox, int count, bool headersOnly)
        {
            return GetMessages(mailbox, count, 1, false, headersOnly, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the IMAP server, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="startIndex">The relative 1-indexed message to start with.</param>
        /// <param name="reverseOrder">Whether to return messages in descending order.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        /// <param name="setSeenFlag">Whether to update the message's flag as having been seen.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName, int count, int startIndex, bool reverseOnly, bool headersOnly, bool setSeenFlag)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated || string.IsNullOrEmpty(mailboxName))
                return null;

            if (mailboxName != CurrentMailboxName)
                SelectMailbox(mailboxName);

            List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();
            int numMessages = GetMessageCount();

            int messagesReturned = 0;
            for (int i = numMessages; i >= 1; i--)
            {
                ReadOnlyMailMessage message = GetMessageHelper(mailboxName, i, headersOnly, setSeenFlag, false);

                if (message != null)
                {
                    messages.Add(message);
                    messagesReturned++;
                }

                if (messagesReturned >= count)
                    break;
            }

            return messages;
        }

        /// <summary>
        /// Get the current quota and usage for the specified root.
        /// </summary>
        /// <param name="quotaRoot">The quota root to work with.</param>
        public QuotaUsage GetQuota(string quotaRoot)
        {
            QuotaUsage quota = new QuotaUsage();
            quota.TotalQuota = -1;
            quota.Usage = -1;

            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    return quota;

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "GETQUOTA \"" + quotaRoot + "\"\r\n");
                string response = ReadData(commandTag);

                if (LastCommandResult)
                {
                    // Attempt to parse the response.
                    string[] responseParts = response.Substring(0, response.Length - 1).Split(' ');
                    int.TryParse(responseParts[responseParts.Length - 2], out quota.Usage);
                    int.TryParse(responseParts[responseParts.Length - 1], out quota.TotalQuota);
                }

                return quota;
            }
            else
                return quota;
        }

        /// <summary>
        /// Get the current quota and usage at the root level.
        /// </summary>
        /// <param name="mailboxName">The mailbox to work with.</param>
        public QuotaUsage GetQuotaRoot(string mailboxName)
        {
            QuotaUsage quota = new QuotaUsage();
            quota.TotalQuota = -1;
            quota.Usage = -1;

            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    return quota;

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "GETQUOTAROOT \"" + mailboxName + "\"\r\n");
                string response = ReadData(commandTag);

                if (LastCommandResult)
                {
                    // Attempt to parse the response.
                    string[] responseParts = response.Substring(0, response.Length - 1).Split(' ');
                    int.TryParse(responseParts[responseParts.Length - 2], out quota.Usage);
                    int.TryParse(responseParts[responseParts.Length - 1], out quota.TotalQuota);
                }

                return quota;
            }
            else
                return quota;
        }

        /// <summary>
        /// Send a list of identifying characteristics to the server.
        /// </summary>
        /// <param name="identification">Values to be sent.</param>
        public void Identify(ImapIdentification identification)
        {
            StringBuilder identificationBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(identification.Name))
                identificationBuilder.Append("\"name\" \"" + identification.Name + "\" ");
            if (!string.IsNullOrEmpty(identification.Version))
                identificationBuilder.Append("\"version\" \"" + identification.Version + "\" ");
            if (!string.IsNullOrEmpty(identification.OS))
                identificationBuilder.Append("\"os\" \"" + identification.OS + "\" ");
            if (!string.IsNullOrEmpty(identification.OSVendor))
                identificationBuilder.Append("\"os-vendor\" \"" + identification.OSVendor + "\" ");
            if (!string.IsNullOrEmpty(identification.Vendor))
                identificationBuilder.Append("\"vendor\" \"" + identification.Vendor + "\" ");
            if (!string.IsNullOrEmpty(identification.SupportURL))
                identificationBuilder.Append("\"support-url\" \"" + identification.SupportURL + "\" ");
            if (!string.IsNullOrEmpty(identification.Address))
                identificationBuilder.Append("\"address\" \"" + identification.Address + "\" ");
            if (!string.IsNullOrEmpty(identification.Date))
                identificationBuilder.Append("\"date\" \"" + identification.Date + "\" ");
            if (!string.IsNullOrEmpty(identification.Command))
                identificationBuilder.Append("\"command\" \"" + identification.Command + "\" ");
            if (!string.IsNullOrEmpty(identification.Arguments))
                identificationBuilder.Append("\"arguments\" \"" + identification.Arguments + "\" ");
            if (!string.IsNullOrEmpty(identification.Environment))
                identificationBuilder.Append("\"environment\" \"" + identification.Environment + "\" ");

            string identificationString = identificationBuilder.ToString();
            if (identificationString.Length > 0)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "ID (" + identificationString.Substring(0, identificationString.Length - 1) + ")\r\n");
                string response = ReadData(commandTag);
            }
        }

        /// <summary>
        /// Notify the server that the session is going IDLE, while continuing to receive notifications from the server.
        /// </summary>
        public bool IdleStart()
        {
            // Ensure that the server supports IDLE.
            if (ServerSupportsIdle)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "IDLE\r\n");
                string response = ReadData(commandTag);

                SessionIsIdle = LastCommandResult;
                return LastCommandResult;
            }
            else
            return false;
        }

        /// <summary>
        /// Notify the server that the session is no longer IDLE.
        /// </summary>
        public bool IdleStop()
        {
            // Ensure that we've already entered the IDLE state.
            if (SessionIsIdle)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "DONE\r\n");
                string response = ReadData(commandTag);

                SessionIsIdle = false;
            }

            return true;
        }

        /// <summary>
        /// Return an array of all root mailboxes, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListMailboxes(bool includeFullHierarchy)
        {
            return ListMailboxes("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of all mailboxes below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListMailboxes(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return new Mailbox[] { };

            List<Mailbox> mailboxes = new List<Mailbox>();

            // If the server supports XLIST, prefer that over LIST.
            string xListPrefix = "";
            if (ServerSupportsXlist)
                xListPrefix = "X";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %\r\n");
                else
                    SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %/%\r\n");
            }

            string response = ReadData(commandTag);

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                Mailbox mailbox = Mailbox.CreateFromList(responseLine);
                if (mailbox != null)
                    mailboxes.Add(mailbox);
            }

            return mailboxes.ToArray();
        }

        /// <summary>
        /// Return an array of all root mailbox names, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListMailboxNames(bool includeFullHierarchy)
        {
            return ListMailboxNames("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of all mailbox names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListMailboxNames(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return new string[] { };

            List<string> mailboxNames = new List<string>();

            // If the server supports XLIST, prefer that over LIST.
            string xListPrefix = "";
            if (ServerSupportsXlist)
                xListPrefix = "X";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %\r\n");
                else
                    SendCommand(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %/%\r\n");
            }

            string response = ReadData(commandTag);

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                string[] responseLineParts = responseLine.Split(space, 5);
                if (responseLineParts.Length > 4)
                    mailboxNames.Add(responseLineParts[4].Replace("\"", ""));
            }

            return mailboxNames.ToArray();
        }

        /// <summary>
        /// Return an array of subscriptions, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListSubscriptions(bool includeFullHierarchy)
        {
            return ListSubscriptions("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of subscriptions below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListSubscriptions(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return new Mailbox[] { };

            List<Mailbox> mailboxes = new List<Mailbox>();

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                SendCommand(commandTag, "LSUB \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    SendCommand(commandTag, "LSUB \"" + mailboxName + "\" %\r\n");
                else
                    SendCommand(commandTag, "LSUB \"" + mailboxName + "\" %/%\r\n");
            }

            string response = ReadData(commandTag);

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                Mailbox mailbox = Mailbox.CreateFromList(responseLine);
                if (mailbox != null)
                    mailboxes.Add(mailbox);
            }

            return mailboxes.ToArray();
        }
        
        /// <summary>
        /// Return an array of subscription names, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListSubscriptionNames(bool includeFullHierarchy)
        {
            return ListSubscriptionNames("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of subscription names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListSubscriptionNames(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return new string[] { };

            List<string> mailboxNames = new List<string>();

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                SendCommand(commandTag, "LSUB \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    SendCommand(commandTag, "LSUB \"" + mailboxName + "\" %\r\n");
                else
                    SendCommand(commandTag, "LSUB \"" + mailboxName + "\" %/%\r\n");
            }

            string response = ReadData(commandTag);

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                string[] responseLineParts = responseLine.Split(space, 5);
                if (responseLineParts.Length > 4)
                    mailboxNames.Add(responseLineParts[4].Replace("\"", ""));
            }

            return mailboxNames.ToArray();
        }

        /// <summary>
        /// Log out and end the current session.
        /// </summary>
        public void LogOut()
        {
            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "LOGOUT\r\n");
            string response = ReadData(commandTag);
            SessionIsAuthenticated = false;
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public bool MoveMessage(string sourceMailboxName, int index, string destMailboxName)
        {
            return MoveMessageHelper(sourceMailboxName, index, destMailboxName, false);
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public bool MoveMessageUid(string sourceMailboxName, int uid, string destMailboxName)
        {
            return MoveMessageHelper(sourceMailboxName, uid, destMailboxName, true);
        }

        /// <summary>
        /// Prolong the current session and poll for new messages, but issue no command.
        /// </summary>
        public bool NoOp()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "NOOP\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Read the last response from the IMAP server.
        /// </summary>
        public string ReadData()
        {
            return ReadData(SessionCommandTag);
        }
        
        /// <summary>
        /// Read the last response from the IMAP server tied to a specific command tag.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response</param>
        public string ReadData(string commandTag)
        {
            string response = "";

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                int bytesRead = ImapStream.Read(InternalBuffer, 0, Constants.BUFFERSIZE);
                response += Encoding.UTF8.GetString(InternalBuffer, 0, bytesRead);

                // Deal with bad commands and responses with errors.
                if (firstResponse)
                {
                    if (response.StartsWith(commandTag + " BAD"))
                    {
                        LastErrorMessage = response.Substring(commandTag.Length + 5);
                        return "";
                    }
                    else if (firstResponse && (response.StartsWith(commandTag + " NO")))
                    {
                        LastErrorMessage = response.Substring(commandTag.Length + 4);
                        return "";
                    }
                }

                // Check if the last sequence received ends with a line break, possibly indicating an end of message.
                if (response.EndsWith("\r\n"))
                {
                    // Check if the message includes an IMAP "OK" signature, signifying the message is complete.
                    int lastLineBreak = response.LastIndexOf("\r\n", response.Length - 2);
                    if (lastLineBreak > 0)
                    {
                        if (response.Substring(lastLineBreak + 2).StartsWith(commandTag + " OK"))
                        {
                            receivingMessage = false;
                            response = response.Substring(0, lastLineBreak);
                        }
                    }
                    else
                    {
                        if (response.StartsWith(commandTag + " OK\r\n"))
                        {
                            receivingMessage = false;
                            response = response.Substring(commandTag.Length + 5, response.Length - commandTag.Length - 7);
                        }
                        else if (response.StartsWith(commandTag + " OK"))
                        {
                            receivingMessage = false;
                            response = response.Substring(commandTag.Length + 3, response.Length - commandTag.Length - 5);
                        }
                    }
                }
                firstResponse = false;
            }

            LastCommandResult = true;
            return response;
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public bool RemoveFlagsFromMessage(string mailboxName, int index, string[] flags)
        {
            return RemoveFlagsFromMessageHelper(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public bool RemoveFlagsFromMessageUid(string mailboxName, int uid, string[] flags)
        {
            return RemoveFlagsFromMessageHelper(mailboxName, uid, flags, true);
        }
        
        /// <summary>
        /// Rename a mailbox.
        /// </summary>
        /// <param name="currentMailboxName">The name of the current mailbox to be renamed.</param>
        /// <param name="newMailboxName">The new name of the mailbox.</param>
        public bool RenameMailbox(string currentMailboxName, string newMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Disallow wildcard characters in mailbox names;
            if (newMailboxName.Contains("*") || newMailboxName.Contains("%"))
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "RENAME " + currentMailboxName + " " + newMailboxName + "\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Perform a search in the current mailbox and return all matching messages.
        /// </summary>
        /// <param name="searchQuery">Well-formatted IMAP search criteria.</param>
        public List<ReadOnlyMailMessage> Search(string searchQuery)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return null;

            // Strip the command if it was passed since we'll be adding it.
            if (searchQuery.StartsWith("SEARCH "))
                searchQuery = searchQuery.Substring(7);
            else if (searchQuery.StartsWith("UID SEARCH "))
                searchQuery = searchQuery.Substring(11);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "UID SEARCH " + searchQuery + "\r\n");
            string response = ReadData(commandTag);

            if (LastCommandResult)
            {
                List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();

                if (response.StartsWith("* SEARCH "))
                {
                    string[] messageIDs = response.Substring(9).Split(' ');
                    foreach (string messageID in messageIDs)
                    {
                        int numericMessageID = -1;
                        if (int.TryParse(messageID, out numericMessageID))
                        {
                            ReadOnlyMailMessage message = GetMessageUid(int.Parse(messageID));
                            if (message != null)
                                messages.Add(message);
                        }
                    }
                }

                return messages;
            }

            return new List<ReadOnlyMailMessage>();
        }

        /// <summary>
        /// Select a mailbox for subsequent operations and return its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        public Mailbox SelectMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return null;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "SELECT " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            if (LastCommandResult)
            {
                CurrentMailboxName = mailboxName;
                SessionIsMailboxSelected = true;

                CurrentMailbox = new Mailbox(mailboxName, response);
                return CurrentMailbox;
            }
            else
                return null;
        }

        /// <summary>
        /// Send a message to the IMAP server.
        /// Should always be followed by GetImapStreamString.
        /// </summary>
        /// <param name="command">Text to transmit.</param>
        public void SendCommand(string command)
        {
            SendCommand(SessionCommandTag, command);
        }

        /// <summary>
        /// Send a message to the IMAP server, specifying a unique command tag.
        /// Should always be followed by GetImapStreamString.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response</param>
        /// <param name="command">Text to transmit.</param>
        public void SendCommand(string commandTag, string command)
        {
            LastCommandIssued = commandTag + " " + command;
            Functions.SendStreamString(ImapStream, InternalBuffer, commandTag + " " + command);
        }

        /// <summary>
        /// Set a quota for the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">Name of the mailbox to work with.</param>
        /// <param name="quotaSize">Size (in MB) of the quota.</param>
        public bool SetQuota(string mailboxName, int quotaSize)
        {
            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    return false;

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "SETQUOTA \"" + mailboxName + "\" (STORAGE " + quotaSize.ToString() + ")\r\n");
                string response = ReadData(commandTag);

                return LastCommandResult;
            }
            else
                return false;
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public bool StoreFlags(string mailboxName, int index, string[] flags)
        {
            return StoreFlagsHelper(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public bool StoreFlagsUid(string mailboxName, int uid, string[] flags)
        {
            return StoreFlagsHelper(mailboxName, uid, flags, true);
        }

        /// <summary>
        /// Subscribe to a mailbox to monitor changes.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public bool SubscribeMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "SUBSCRIBE " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Stop subscribing to a mailbox.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public bool UnsubscribeMailbox(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            SendCommand(commandTag, "UNSUBSCRIBE " + mailboxName + "\r\n");
            string response = ReadData(commandTag);

            return LastCommandResult;
        }

        /// <summary>
        /// Negotiate TLS security for the current session.
        /// </summary>
        public void StartTLS()
        {
            if (!((SslStream)ImapStream).IsAuthenticated)
                ((SslStream)ImapStream).AuthenticateAsClient(Host);
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Add one or more flags to a message.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="id">Identifier of the message to update, either index or UID.</param>
        /// <param name="flags">List of flags to add.</param>
        /// <param name="isUid">Whether the ID was passed as a UID.</param>
        private bool AddFlagsToMessageHelper(string mailbox, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Ensure that at least one flag will be added.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                SendCommand(commandTag, "UID STORE " + id.ToString() + " +Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                SendCommand(commandTag, "STORE " + id.ToString() + " +Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = ReadData(commandTag);
            return LastCommandResult;
        }

        /// <summary>
        /// Helper function to load an instance of a message in a specified mailbox based on its index, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="id">The identifier of the message to load, either its index or UID.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        /// <param name="isUid">Whether the identifer is an UID.</param>
        private ReadOnlyMailMessage GetMessageHelper(string mailbox, int id, bool headersOnly, bool setSeenFlag, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return null;

            if (mailbox != CurrentMailboxName)
                SelectMailbox(mailbox);

            string uidPrefix = isUid ? "UID " : "";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            // Format the command depending on whether we want headers only and whether we want to mark messages as seen.
            if (headersOnly)
                SendCommand(commandTag, uidPrefix + "FETCH " + id + " (BODY[HEADER] UID)\r\n");
            else
            {
                if (setSeenFlag)
                    SendCommand(commandTag, uidPrefix + "FETCH " + id + " (BODY[] UID FLAGS)\r\n");
                else
                    SendCommand(commandTag, uidPrefix + "FETCH " + id + " (BODY.PEEK[] UID FLAGS)\r\n");
            }

            string response = ReadData(commandTag);

            // Ensure the message was actually found.
            if (response.IndexOf("\r\n") > -1)
            {
                // Read the message's UID and flags.
                int uid = 0;
                int.TryParse(Functions.ReturnBetween(response, "UID ", " "), out uid);
                string flagsString = Functions.ReturnBetween(response, "FLAGS (", ")");

                // Strip IMAP response padding.
                response = response.Substring(response.IndexOf("\r\n") + 2);
                response = response.Substring(0, response.Length - 3);

                ReadOnlyMailMessage message = new ReadOnlyMailMessage(response, ProcessingFlags);
                message.ImapUid = uid;
                message.Mailbox = mailbox;
                message.ParseFlagsString(flagsString);
                return message;
            }
            else
                return null;
        }

        /// <summary>
        /// Process the list of capabilities returned by the IMAP server and remember them for future retrieval.
        /// </summary>
        /// <param name="capabilitiesList">Array of capability strings.</param>
        private void InitializeCapabilities(string[] capabilitiesList)
        {
            ServerAuthSupport = new List<string>();
            ServerCompressionSupport = new List<string>();
            ServerContextSupport = new List<string>();
            ServerRights = new List<string>();
            ServerSearchSupport = new List<string>();
            ServerSortingSupport = new List<string>();
            ServerThreadingSupport = new List<string>();

            foreach (string capability in capabilitiesList)
            {
                if (capability.StartsWith("AUTH="))
                {
                    string authMethod = capability.Substring(5);
                    if (!ServerAuthSupport.Contains(authMethod))
                        ServerAuthSupport.Add(authMethod);
                }
                else if (capability.StartsWith("COMPRESS="))
                {
                    string compresssionAlgorithm = capability.Substring(9);
                    if (!ServerCompressionSupport.Contains(compresssionAlgorithm))
                        ServerCompressionSupport.Add(compresssionAlgorithm);
                }
                else if (capability.StartsWith("CONTEXT="))
                {
                    string context = capability.Substring(8);
                    if (!ServerContextSupport.Contains(context))
                        ServerContextSupport.Add(context);
                }
                else if (capability.StartsWith("IMAP-SIEVE="))
                    ServerImapSieveServer = capability.Substring(11);
                else if (capability.StartsWith("RIGHTS="))
                {
                    string rights = capability.Substring(7);
                    if (!ServerRights.Contains(rights))
                        ServerRights.Add(rights);
                }
                if (capability.StartsWith("SEARCH="))
                {
                    string searchOption = capability.Substring(7);
                    if (!ServerSearchSupport.Contains(searchOption))
                        ServerSearchSupport.Add(searchOption);
                }
                if (capability.StartsWith("SORT="))
                {
                    string sortingOption = capability.Substring(5);
                    if (!ServerSortingSupport.Contains(sortingOption))
                        ServerSortingSupport.Add(sortingOption);
                }
                if (capability.StartsWith("THREAD="))
                {
                    string threadingOption = capability.Substring(7);
                    if (!ServerThreadingSupport.Contains(threadingOption))
                        ServerThreadingSupport.Add(threadingOption);
                }
                else
                {
                    switch (capability)
                    {
                        case "ACL":
                            ServerSupportsACL = true;
                            break;
                        case "BINARY":
                            ServerSupportsBinary = true;
                            break;
                        case "CATENATE":
                            ServerSupportsCatenate = true;
                            break;
                        case "CHILDREN":
                            ServerSupportsChildren = true;
                            break;
                        case "CONDSTORE":
                            ServerSupportsCondStore = true;
                            break;
                        case "CONVERT":
                            ServerSupportsConvert = true;
                            break;
                        case "CREATE-SPECIAL-USE":
                            ServerSupportsCreateSpecialUse = true;
                            break;
                        case "ENABLE":
                            ServerSupportsEnable = true;
                            break;
                        case "ESEARCH":
                            ServerSupportsESearch = true;
                            break;
                        case "ESORT":
                            ServerSupportsESort = true;
                            break;
                        case "FILTERS":
                            ServerSupportsFilters = true;
                            break;
                        case "X-GM-EXT-1":
                            ServerSupportsGoogleExtensions = true;
                            break;
                        case "ID":
                            ServerSupportsID = true;
                            break;
                        case "IDLE":
                            ServerSupportsIdle = true;
                            break;
                        case "LANGUAGE":
                            ServerSupportsLanguage = true;
                            break;
                        case "LISTEXT":
                            ServerSupportsListExt = true;
                            break;
                        case "LIST-STATUS":
                            ServerSupportsListStatus = true;
                            break;
                        case "LITERAL+":
                            ServerSupportsLiteralPlus = true;
                            break;
                        case "LOGINDISABLED":
                            ServerSupportsLoginDisabled = true;
                            break;
                        case "LOGIN-REFERRALS":
                            ServerSupportsLoginReferrals = true;
                            break;
                        case "MAILBOX-REFERRALS":
                            ServerSupportsMailboxReferrals = true;
                            break;
                        case "METADATA":
                            ServerSupportsMove = true;
                            break;
                        case "MOVE":
                            ServerSupportsMove = true;
                            break;
                        case "MULTIAPPEND":
                            ServerSupportsMultiAppend = true;
                            break;
                        case "MULTISEARCH":
                            ServerSupportsMultiSearch = true;
                            break;
                        case "NAMESPACE":
                            ServerSupportsNamespace = true;
                            break;
                        case "NOTIFY":
                            ServerSupportsNotify = true;
                            break;
                        case "QUOTA":
                            ServerSupportsQuota = true;
                            break;
                        case "QRESYNC":
                            ServerSupportsQResync = true;
                            break;
                        case "SEARCHRES":
                            ServerSupportsSearchRes = true;
                            break;
                        case "SORT":
                            ServerSupportsSort = true;
                            break;
                        case "SPECIAL-USE":
                            ServerSupportsSpecialUse = true;
                            break;
                        case "STARTTLS":
                            ServerSupportsSort = true;
                            break;
                        case "UIDPLUS":
                            ServerSupportsUIDPlus = true;
                            break;
                        case "UNSELECT":
                            ServerSupportsUnselect = true;
                            break;
                        case "WITHIN":
                            ServerSupportsWithin = true;
                            break;
                        case "XLIST":
                            ServerSupportsXlist = true;
                            break;
                    }
                }
                Capabilities.Add(capability);
            }
        }

        /// <summary>
        /// Helper function to ove a message to the destination mailbox.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="id">Identifier of the message to move, either index or UID.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        /// <param name="isUid">Whether the ID was passed as a UID.</param>
        private bool MoveMessageHelper(string sourceMailboxName, int id, string destMailboxName, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Ensure we're working with the right mailbox.
            if (sourceMailboxName != CurrentMailboxName)
                SelectMailbox(sourceMailboxName);

            string uidPrefix = isUid ? "UID " : "";

            // If server supports the MOVE command, use that.  Otherwise, copy it to its destination then delete the original.
            if (ServerSupportsMove)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, uidPrefix + "MOVE " + id.ToString() + " " + destMailboxName + "\r\n");
                string response = ReadData(commandTag);

                return LastCommandResult;
            }
            else
            {
                if (CopyMessage(sourceMailboxName, id, destMailboxName))
                {
                    if (DeleteMessage(sourceMailboxName, id))
                        return ExpungeMailbox();
                }
            }

            return false;
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="id">Identifier of the message to update, either index or UID.</param>
        /// <param name="flags">List of flags to update.</param>
        /// <param name="isUid">Whether the ID was passed as a UID.</param>
        private bool StoreFlagsHelper(string mailbox, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Ensure that at least one flag will be removed.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                SendCommand(commandTag, "UID STORE " + id.ToString() + " Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                SendCommand(commandTag, "STORE " + id.ToString() + " Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = ReadData(commandTag);
            return LastCommandResult;
        }

        /// <summary>
        /// Remove one or more flags from a message.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="id">Identifier of the message to update, either index or UID.</param>
        /// <param name="flags">List of flags to remove.</param>
        /// <param name="isUid">Whether the ID was passed as a UID.</param>
        private bool RemoveFlagsFromMessageHelper(string mailbox, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                return false;

            // Ensure that at least one flag will be removed.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                SendCommand(commandTag, "UID STORE " + id.ToString() + " -Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                SendCommand(commandTag, "STORE " + id.ToString() + " -Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = ReadData(commandTag);
            return LastCommandResult;
        }

        /// <summary>
        /// Iterates the command tag counter and returns a new ID for uniquely identifying commands.
        /// </summary>
        private string UniqueCommandTag()
        {
            return (++CommandTagCounter).ToString();
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Represents the exception that is thrown when the OpaqueMail.ImapClient is not able to complete an operation.
    /// </summary>
    public class ImapException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.ImapException class.
        /// </summary>
        public ImapException() : base() { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.ImapException class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        public ImapException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.ImapException class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImapException(string message, Exception innerException) : base(message, innerException) { }
    }
}
