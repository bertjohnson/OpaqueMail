/*
 * OpaqueMail (http://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (http://bertjohnson.net/) of Bkip Inc. (http://bkip.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Allows applications to retrieve and manage e-mail by using the Internet Message Access Protocol (IMAP).
    /// </summary>
    /// <remarks>Includes OpaqueMail extensions to facilitate processing of secure S/MIME messages.</remarks>
    public partial class ImapClient : IDisposable
    {
        #region Public Members
        /// <summary>Set of extended IMAP capabilities.</summary>
        public HashSet<string> Capabilities = new HashSet<string>();
        /// <summary>Gets or sets the credentials used to authenticate.</summary>
        public NetworkCredential Credentials;
        /// <summary>Specify whether the OpaqueMail.ImapClient uses Secure Sockets Layer (SSL).</summary>
        public bool EnableSsl;
        /// <summary>Gets or sets the name or IP address of the host used for IMAP transactions.</summary>
        public string Host;
        /// <summary>Frequency of sending NOOP commands to check for mailbox notifications.</summary>
        public TimeSpan IdleFrequency = new TimeSpan(0, 1, 0);
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
        /// <summary>When our idle loop started.</summary>
        private DateTime IdleInitializedTime;
        /// <summary>Thread where we listen for messages from the server.</summary>
        private Thread IdleThread;
        /// <summary>Connection to the remote IMAP server.</summary>
        private TcpClient ImapTcpClient;
        /// <summary>Stream for communicating with the IMAP server.</summary>
        private Stream ImapStream;
        /// <summary>Lock to coordinate asynchronous stream reading.</summary>
        private readonly AsyncLock ImapStreamLock = new AsyncLock();
        /// <summary>StreamReader for receiving data from the IMAP server.</summary>
        private StreamReader ImapStreamReader;
        /// <summary>StreamWriter for sending data to the IMAP server</summary>
        private StreamWriter ImapStreamWriter;
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] InternalBuffer = new byte[Constants.LARGEBUFFERSIZE];
        /// <summary>The authentication state when capabilities were last queried.</summary>
        private bool LastCapabilitiesCheckAuthenticationState = false;
        /// <summary>When a "[THROTTLED]" notice was last received.</summary>
        private DateTime LastThrottleTime = new DateTime(1900, 1, 1);
        /// <summary>Collection of server responses that haven't been processed yet.</summary>
        /// <remarks>Used when multiple requests are issued concurrently on one connection.</remarks>
        private Dictionary<string, string> SwitchBoard = new Dictionary<string, string>();
        /// <summary>Whether the session has successfully been authenticated.</summary>
        private bool SessionIsAuthenticated = false;
        /// <summary>Whether the session has explicitly entered the IDLE state.</summary>
        private bool SessionIsIdle = false;
        /// <summary>Whether a mailbox has been selected in the session.</summary>
        private bool SessionIsMailboxSelected = false;
        /// <summary>The welcome message provided by the IMAP server.</summary>
        private String SessionWelcomeMessage = "";
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.ImapClient class by using the specified settings.
        /// </summary>
        /// <param name="host">Name or IP of the host used for IMAP transactions.</param>
        /// <param name="port">Port to be used by the host.</param>
        /// <param name="userName">The username associated with this connection.</param>
        /// <param name="password">The password associated with this connection.</param>
        /// <param name="enableSSL">Whether the IMAP connection uses TLS / SSL protection.</param>
        public ImapClient(string host, int port, string userName, string password, bool enableSSL)
        {
            Host = host;
            Port = port;
            Credentials = new NetworkCredential(userName, password);
            EnableSsl = enableSSL;
        }

        /// <summary>
        /// Default destructor.
        /// </summary>
        ~ImapClient()
        {
            if (ImapStreamWriter != null)
                ImapStreamWriter.Dispose();
            if (ImapStreamReader != null)
                ImapStreamReader.Dispose();
            if (ImapStream != null)
                ImapStream.Dispose();
            if (ImapTcpClient != null)
                ImapTcpClient.Close();
        }
        #endregion Constructors

        #region Events
        /// <summary>
        /// Represents the event handler that will handle ImapClientDisconnectEvents.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        public delegate void ImapClientDisconnectEventHandler(object sender);

        /// <summary>
        /// Represents the event handler that will handle ImapClientMessageExpungeEvents.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        /// <param name="e">An ImapClientEventArgs that contains the event data.</param>
        public delegate void ImapClientMessageExpungeHandler(object sender, ImapClientMessageExpungeEventArgs e);

        /// <summary>
        /// Represents the event handler that will handle ImapClientNewMessageEvents.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        /// <param name="e">An ImapClientEventArgs that contains the event data.</param>
        public delegate void ImapClientNewMessageHandler(object sender, ImapClientNewMessageEventArgs e);

        /// <summary>
        /// Represents the event handler that will handle ImapClientThrottleEvents.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        public delegate void ImapClientThrottleHandler(object sender);

        /// <summary>
        /// Represents the event handler that will handle an ImapClientDisconnectEvent.
        /// </summary>
        public event ImapClientDisconnectEventHandler ImapClientDisconnectEvent;

        /// <summary>
        /// Represents the event handler that will handle an ImapClientMessageExpungeEvent.
        /// </summary>
        public event ImapClientMessageExpungeHandler ImapClientMessageExpungeEvent;

        /// <summary>
        /// Represents the event handler that will handle an ImapClientNewMessageEvent.
        /// </summary>
        public event ImapClientNewMessageHandler ImapClientNewMessageEvent;

        /// <summary>
        /// Represents the event handler that will handle an ImapClientThrottleEvent.
        /// </summary>
        public event ImapClientThrottleHandler ImapClientThrottleEvent;

        /// <summary>
        /// Raises an event indicating the IMAP server disconnecting this connection due to a "BYE" command.
        /// </summary>
        protected void OnImapClientDisconnectEvent()
        {
            ImapClientDisconnectEvent(this);
        }

        /// <summary>
        /// Raises an event indicating a message being expunged.
        /// </summary>
        /// <param name="e">An ImapClientEventArgs that contains the event data.</param>
        protected void OnImapClientMessageExpungeEvent(ImapClientMessageExpungeEventArgs e)
        {
            ImapClientMessageExpungeEvent(this, e);
        }

        /// <summary>
        /// Raises an event indicating a new message being received.
        /// </summary>
        /// <param name="e">An ImapClientEventArgs that contains the event data.</param>
        protected void OnImapClientNewMessageEvent(ImapClientNewMessageEventArgs e)
        {
            ImapClientNewMessageEvent(this, e);
        }

        /// <summary>
        /// Raises an event indicating a connection being throttled by the server.
        /// </summary>
        protected void OnImapClientThrottleEvent()
        {
            ImapClientThrottleEvent(this);
        }
        #endregion Events

        #region Public Methods
        /// <summary>
        /// Add one or more flags to a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public async Task<bool> AddFlagsToMessageAsync(string mailboxName, int index, string[] flags)
        {
            return await AddFlagsToMessageHelperAsync(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Add one or more flags to a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public async Task<bool> AddFlagsToMessageUidAsync(string mailboxName, int uid, string[] flags)
        {
            return await AddFlagsToMessageHelperAsync(mailboxName, uid, flags, true);
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message text to append.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, string message)
        {
            return await AppendMessageAsync(mailboxName, message, new string[] { }, null);
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, ReadOnlyMailMessage message)
        {
            return await AppendMessageAsync(mailboxName, message.RawMessage, message.RawFlags.ToArray(), message.Date);
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message text to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, string message, string[] flags, DateTime? date)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the APPEND command.");

            // Ensure the message has an ending carriage return and line feed.
            if (!message.EndsWith("\r\n"))
                message += "\r\n";

            // Create the initial APPEND command.
            StringBuilder commandBuilder = new StringBuilder(Constants.SMALLSBSIZE);
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
            await SendCommandAsync(commandTag, commandBuilder.ToString());

            // Confirm the server is ready to accept our raw data.
            string response = await ReadDataAsync("+", "APPEND");
            if (LastCommandResult)
            {
                await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, message + "\r\n");
                response = await ReadDataAsync(commandTag, "APPEND");

                return LastCommandResult;
            }

            return false;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, ReadOnlyMailMessage message, string[] flags, DateTime? date)
        {
            return await AppendMessageAsync(mailboxName, message.RawMessage, flags, date);
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages to append.</param>
        public async Task<bool> AppendMessagesAsync(string mailboxName, string[] messages)
        {
            return await AppendMessagesAsync(mailboxName, messages, new string[] { }, null);
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public async Task<bool> AppendMessagesAsync(string mailboxName, string[] messages, string[] flags, DateTime? date)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the APPEND command.");

            // If the server supports MULTIAPPEND, pipeline the append statements.
            if (ServerSupportsMultiAppend)
            {
                bool firstMessage = true;
                for (int i = 0; i < messages.Length; i++)
                {
                    string message = messages[i];

                    // Ensure the message has an ending carriage return and line feed.
                    if (!message.EndsWith("\r\n"))
                        message += "\r\n";

                    // Create the initial APPEND command.
                    StringBuilder commandBuilder = new StringBuilder(Constants.SMALLSBSIZE);

                    string commandTag = "";

                    if (firstMessage)
                    {
                        commandBuilder.Append("APPEND " + mailboxName);

                        // Generate a unique command tag for tracking this command and its response.
                        commandTag = UniqueCommandTag();

                        firstMessage = false;
                    }

                    commandBuilder.Append(" ");

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

                    // Complete the initial command send it.
                    commandBuilder.Append("{" + message.Length + "}\r\n");
                    await SendCommandAsync(commandTag, commandBuilder.ToString());

                    // Confirm the server is ready to accept our raw data.
                    string response = await ReadDataAsync("+", "APPEND");
                    if (LastCommandResult)
                    {
                        // If on the last message, add an extra line break and return the result.
                        if (i == messages.Length - 1)
                        {
                            await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, message + "\r\n");
                            response = await ReadDataAsync(commandTag, "APPEND");

                            return LastCommandResult;
                        }
                        else
                            await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, message);
                    }
                }

                return false;
            }
            else
            {
                bool returnValue = true;

                foreach (string message in messages)
                    returnValue = returnValue && await AppendMessageAsync(mailboxName, message, new string[] { }, null);

                return returnValue;
            }
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public async Task<bool> AppendMessagesAsync(string mailboxName, ReadOnlyMailMessage[] messages, string[] flags, DateTime? date)
        {
            string[] rawMessages = new string[messages.Length];
            for (int i = 0; i < messages.Length; i++)
                rawMessages[i] = messages[i].RawMessage;

            return await AppendMessagesAsync(mailboxName, rawMessages, flags, date);
        }

        /// <summary>
        /// Perform an authentication handshake with the IMAP server.
        /// </summary>
        public bool Authenticate()
        {
            return Authenticate(AuthenticationMode.Login);
        }

        /// <summary>
        /// Perform an authentication handshake with the IMAP server with the specified method.
        /// </summary>
        /// <param name="authMode">The authentication method to use.</param>
        public bool Authenticate(AuthenticationMode authMode)
        {
            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();
            string response = "";

            switch (authMode)
            {
                case AuthenticationMode.CramMD5:
                    SendCommand(commandTag, "AUTHENTICATE CRAM-MD5\r\n");
                    response = ReadData(commandTag, "AUTHENTICATE");

                    // If handshake started successfully, respond to the challenge.
                    if (LastCommandResult)
                    {
                        using (HMACMD5 hmacMd5 = new HMACMD5(System.Text.Encoding.UTF8.GetBytes(Credentials.Password)))
                        {
                            byte[] hmacMd5Hash = hmacMd5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(response));
                            string key = Encoding.UTF8.GetString(hmacMd5Hash).ToLower().Replace("-", "");
                            string challengeResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName + " " + key));

                            Functions.SendStreamString(ImapStream, InternalBuffer, challengeResponse + "\r\n");
                            response = ReadData(commandTag, "AUTHENTICATE");
                        }
                    }
                    else
                        return false;

                    break;
                case AuthenticationMode.Login:
                    SendCommand(commandTag, "LOGIN " + Credentials.UserName + " " + Credentials.Password + "\r\n");
                    response = ReadData(commandTag, "LOGIN");
                    break;
                case AuthenticationMode.Plain:
                    SendCommand(commandTag, "AUTHENTICATE PLAIN " + Convert.ToBase64String(Encoding.UTF8.GetBytes("\0" + Credentials.UserName + "\0" + Credentials.Password)) + "\r\n");
                    response = ReadData(commandTag, "AUTHENTICATE");
                    break;
            }

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
        /// Request a checkpoint of the currently selected mailbox.
        /// </summary>
        public async Task<bool> CheckAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the CHECK command.");

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                throw new ImapException("A mailbox must be selected prior to calling the CHECK command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "CHECK\r\n");
            string response = await ReadDataAsync(commandTag, "CHECK");

            return LastCommandResult;
        }

        /// <summary>
        /// Close the currently selected mailbox and remove all messages with the "\Deleted" flag.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CloseMailboxAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the CLOSE command.");

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                throw new ImapException("A mailbox must be selected prior to calling the CLOSE command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "CLOSE\r\n");
            string response = await ReadDataAsync(commandTag, "CLOSE");

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
                ImapStream = ImapTcpClient.GetStream();

                if (EnableSsl)
                    StartTLS();

                ImapStreamReader = new StreamReader(ImapStream);
                ImapStreamWriter = new StreamWriter(ImapStream);

                // Remember the welcome message.
                SessionWelcomeMessage = ReadData("*", "");
                if (LastCommandResult)
                    return true;
            }
            catch { }

            // If we didn't successfully receive the welcome message, dispose all objects.
            if (ImapStreamWriter != null)
                ImapStreamWriter.Dispose();
            if (ImapStreamReader != null)
                ImapStreamReader.Dispose();
            if (ImapStream != null)
                ImapStream.Dispose();
            if (ImapTcpClient != null)
                ImapTcpClient.Close();

            return false;
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public async Task<bool> CopyMessageAsync(string sourceMailboxName, int index, string destMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the COPY command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "COPY " + index.ToString() + " " + destMailboxName + "\r\n");
            string result = await ReadDataAsync(commandTag, "COPY");

            return LastCommandResult;
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public async Task<bool> CopyMessageUidAsync(string sourceMailboxName, int uid, string destMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the COPY command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "UID COPY " + uid.ToString() + " " + destMailboxName + "\r\n");
            await ReadDataAsync(commandTag, "COPY");

            return LastCommandResult;
        }

        /// <summary>
        /// Create a mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The given name for the new mailbox.</param>
        public async Task<bool> CreateMailboxAsync(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the CREATE command.");

            // Disallow wildcard characters in mailbox names;
            if (mailboxName.IndexOf("*", StringComparison.Ordinal) > -1 || mailboxName.IndexOf("%", StringComparison.Ordinal) > -1)
                throw new ImapException("A mailbox must be selected prior to calling the CREATE command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "CREATE " + mailboxName + "\r\n");
            string response = await ReadDataAsync(commandTag, "CREATE");

            return LastCommandResult;
        }

        /// <summary>
        /// Delete a mailbox from the server.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to delete.</param>
        public async Task<bool> DeleteMailboxAsync(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the DELETE command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "DELETE " + mailboxName + "\r\n");
            string response = await ReadDataAsync(commandTag, "DELETE");

            return LastCommandResult;
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="index">Index of the message to delete.</param>
        public async Task<bool> DeleteMessageAsync(string mailboxName, int index)
        {
            return await AddFlagsToMessageHelperAsync(mailboxName, index, new string[] { "\\Deleted" }, false);
        }

        /// <summary>
        /// Delete a series of messages from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the messages to delete.</param>
        /// <param name="indices">Array of message indices to delete.</param>
        public async Task<bool> DeleteMessagesAsync(string mailboxName, int[] indices)
        {
            bool returnValue = true;
            foreach (int index in indices)
                returnValue = returnValue && await DeleteMessageAsync(mailboxName, index);

            return returnValue;
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="uid">UID of the message to delete.</param>
        public async Task<bool> DeleteMessageUidAsync(string mailboxName, int uid)
        {
            return await AddFlagsToMessageHelperAsync(mailboxName, uid, new string[] { "\\Deleted" }, true);
        }

        /// <summary>
        /// Delete a series of messages from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the messages to delete.</param>
        /// <param name="indices">Array of message indices to delete.</param>
        /// <param name="uids">Array of message UIDs to delete.</param>
        public async Task<bool> DeleteMessagesUidAsync(string mailboxName, int[] uids)
        {
            bool returnValue = true;
            foreach (int uid in uids)
                returnValue = returnValue && await DeleteMessageUidAsync(mailboxName, uid);

            return returnValue;
        }

        /// <summary>
        /// Ends the TCP connection and releases all resources used by the current instances of the OpaqueMail.ImapClient class.
        /// </summary>
        public void Disconnect()
        {
            if (ImapStreamWriter != null)
                ImapStreamWriter.Dispose();
            if (ImapStreamReader != null)
                ImapStreamReader.Dispose();
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
        /// Ends the TCP connection and releases all resources used by the current instances of the OpaqueMail.ImapClient class.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Notify the IMAP server that the client supports the specified capability.
        /// </summary>
        /// <param name="capabilityName">Name of the capability to enable.</param>
        public async Task<bool> EnableCapabilityAsync(string capabilityName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the ENABLE command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "ENABLE " + capabilityName + "\r\n");
            string response = await ReadDataAsync(commandTag, "ENABLE");
            return LastCommandResult;
        }

        /// <summary>
        /// Examine a mailbox, returning its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        public async Task<Mailbox> ExamineMailboxAsync(string mailboxName)
        {
            return await ExamineMailboxAsync(mailboxName, "");
        }

        /// <summary>
        /// Examine a mailbox, returning its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        /// <param name="qResyncParameters">Quick Resynchronization parameters, such as UIDValidity, the last known modification sequence, known UIDs, and/or known sequence ranges.</param>
        public async Task<Mailbox> ExamineMailboxAsync(string mailboxName, string qResyncParameters)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the EXAMINE command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (!string.IsNullOrEmpty(qResyncParameters))
                await SendCommandAsync(commandTag, "EXAMINE " + mailboxName + " (QRESYNC (" + qResyncParameters + "))\r\n");
            else
                await SendCommandAsync(commandTag, "EXAMINE " + mailboxName + "\r\n");
            
            string response = await ReadDataAsync(commandTag, "EXAMINE");

            if (LastCommandResult)
                return new Mailbox(mailboxName, response);
            else
                return null;
        }

        /// <summary>
        /// Remove all messages from the current mailbox that have the "\Deleted" flag.
        /// </summary>
        public async Task<bool> ExpungeMailboxAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the EXPUNGE command.");

            // Require a mailbox to be selected first.
            if (string.IsNullOrEmpty(CurrentMailboxName))
                throw new ImapException("A mailbox must be selected prior to calling the EXPUNGE command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "EXPUNGE\r\n");
            string response = await ReadDataAsync(commandTag, "EXPUNGE");

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
            string response = ReadData(commandTag, "CAPABILITY");

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
        public async Task<ReadOnlyMailMessage> GetMessageAsync(int index)
        {
            return await GetMessageHelper(CurrentMailboxName, index, false, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        public async Task<ReadOnlyMailMessage> GetMessageAsync(string mailboxName, int index)
        {
            return await GetMessageHelper(mailboxName, index, false, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public async Task<ReadOnlyMailMessage> GetMessageAsync(string mailboxName, int index, bool headersOnly)
        {
            return await GetMessageHelper(mailboxName, index, headersOnly, false, false);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        public async Task<ReadOnlyMailMessage> GetMessageAsync(string mailboxName, int index, bool headersOnly, bool setSeenFlag)
        {
            return await GetMessageHelper(mailboxName, index, headersOnly, setSeenFlag, false);
        }

        /// <summary>
        /// Return the number of messages in the current mailbox.
        /// </summary>
        public async Task<int> GetMessageCountAsync()
        {
            return await GetMessageCountAsync(CurrentMailboxName);
        }

        /// <summary>
        /// Return the number of messages in a specific mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to examine.</param>
        public async Task<int> GetMessageCountAsync(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the STATUS command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "STATUS " + mailboxName + " (messages)\r\n");
            string response = await ReadDataAsync(commandTag, "STATUS");
            response = Functions.ReturnBetween(response, "(MESSAGES ", ")");

            int numMessages = -1;
            int.TryParse(response, out numMessages);

            return numMessages;
        }

        /// <summary>
        /// Load an instance of a message based on its UID.
        /// </summary>
        /// <param name="uid">The UID of the message to load.</param>
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(int uid)
        {
            return await GetMessageHelper(CurrentMailboxName, uid, false, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string mailboxName, int uid)
        {
            return await GetMessageHelper(mailboxName, uid, false, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string mailboxName, int uid, bool headersOnly)
        {
            return await GetMessageHelper(mailboxName, uid, headersOnly, false, true);
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string mailboxName, int uid, bool headersOnly, bool setSeenFlag)
        {
            return await GetMessageHelper(mailboxName, uid, headersOnly, setSeenFlag, true);
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the current mailbox.
        /// </summary>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync()
        {
            return await GetMessagesAsync(CurrentMailboxName, 25, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(string mailboxName)
        {
            return await GetMessagesAsync(mailboxName, 25, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count)
        {
            return await GetMessagesAsync(CurrentMailboxName, count, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(string mailboxName, int count)
        {
            return await GetMessagesAsync(mailboxName, count, 1, false, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, bool headersOnly)
        {
            return await GetMessagesAsync(CurrentMailboxName, count, 1, false, headersOnly, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(string mailboxName, int count, bool headersOnly)
        {
            return await GetMessagesAsync(mailboxName, count, 1, false, headersOnly, false);
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
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(string mailboxName, int count, int startIndex, bool reverseOrder, bool headersOnly, bool setSeenFlag)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated || string.IsNullOrEmpty(mailboxName))
                throw new ImapException("Must be connected to the server and authenticated prior to calling the FETCH command.");

            if (mailboxName != CurrentMailboxName)
                await SelectMailboxAsync(mailboxName);

            List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();
            int numMessages = await GetMessageCountAsync();

            int messagesReturned = 0;

            int loopStartIndex = reverseOrder ? numMessages + 1 - startIndex : startIndex;
            int loopIterateCount = reverseOrder ? -1 : 1;
            int loopIterations = 0;
            for (int i = loopStartIndex; loopIterations < numMessages; i += loopIterateCount)
            {
                ReadOnlyMailMessage message = await GetMessageHelper(mailboxName, i, headersOnly, setSeenFlag, false);

                if (message != null)
                {
                    messages.Add(message);
                    messagesReturned++;
                }

                if (messagesReturned >= count)
                    break;
                else
                    loopIterations++;
            }

            return messages;
        }

        /// <summary>
        /// Get the current quota and usage for the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to work with.</param>
        public async Task<QuotaUsage> GetQuotaAsync(string mailboxName)
        {
            QuotaUsage quota = new QuotaUsage();
            quota.QuotaMaximum = -1;
            quota.Usage = -1;

            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    throw new ImapException("Must be connected to the server and authenticated prior to calling the GETQUOTA command.");

                // Encode ampersands and Unicode characters.
                mailboxName = Functions.ToModifiedUTF7(mailboxName);

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, "GETQUOTA \"" + mailboxName + "\"\r\n");
                string response = await ReadDataAsync(commandTag, "GETQUOTA");

                if (LastCommandResult)
                {
                    // Attempt to parse the response.
                    string[] responseParts = response.Substring(0, response.Length - 1).Split(' ');
                    int.TryParse(responseParts[responseParts.Length - 2], out quota.Usage);
                    int.TryParse(responseParts[responseParts.Length - 1], out quota.QuotaMaximum);
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
        public async Task<QuotaUsage> GetQuotaRootAsync(string mailboxName)
        {
            QuotaUsage quota = new QuotaUsage();
            quota.QuotaMaximum = -1;
            quota.Usage = -1;

            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    throw new ImapException("Must be connected to the server and authenticated prior to calling the GETQUOTAROOT command.");

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, "GETQUOTAROOT \"" + mailboxName + "\"\r\n");
                string response = await ReadDataAsync(commandTag, "GETQUOTAROOT");

                if (LastCommandResult)
                {
                    // Attempt to parse the response.
                    string[] responseParts = response.Substring(0, response.Length - 1).Split(' ');
                    int.TryParse(responseParts[responseParts.Length - 2], out quota.Usage);
                    int.TryParse(responseParts[responseParts.Length - 1], out quota.QuotaMaximum);
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
        public async Task IdentifyAsync(ImapIdentification identification)
        {
            StringBuilder identificationBuilder = new StringBuilder(Constants.SMALLSBSIZE);

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

                await SendCommandAsync(commandTag, "ID (" + identificationString.Substring(0, identificationString.Length - 1) + ")\r\n");
                string response = await ReadDataAsync(commandTag, "ID");
            }
        }

        /// <summary>
        /// Notify the server that the session is going IDLE, while continuing to receive notifications from the server.
        /// </summary>
        public async Task<bool> IdleStartAsync()
        {
            // Ensure that the server supports IDLE.
            if (ServerSupportsIdle)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, "IDLE\r\n");
                string response = await ReadDataAsync(commandTag, "IDLE");

                SessionIsIdle = LastCommandResult;

                IdleInitializedTime = DateTime.Now;
                IdleThread = new Thread(new ParameterizedThreadStart(CheckIdle));
                IdleThread.Start(IdleInitializedTime);

                return LastCommandResult;
            }
            else
                return false;
        }

        /// <summary>
        /// Notify the server that the session is no longer IDLE.
        /// </summary>
        public async Task<bool> IdleStopAsync()
        {
            // Ensure that we've already entered the IDLE state.
            if (SessionIsIdle)
            {
                IdleInitializedTime = DateTime.Now;
                SessionIsIdle = false;

                await SendCommandAsync("", "DONE\r\n");
                string response = await ReadDataAsync("", "DONE");
            }

            return true;
        }

        /// <summary>
        /// Return an array of all root mailboxes, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<Mailbox[]> ListMailboxesAsync(bool includeFullHierarchy)
        {
            return await ListMailboxesAsync("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of all mailboxes below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<Mailbox[]> ListMailboxesAsync(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the LIST command.");

            List<Mailbox> mailboxes = new List<Mailbox>();

            // If the server supports XLIST, prefer that over LIST.
            string xListPrefix = "";
            if (ServerSupportsXlist)
                xListPrefix = "X";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %\r\n");
                else
                    await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %/%\r\n");
            }

            string response = await ReadDataAsync(commandTag, "LIST");

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
        public async Task<string[]> ListMailboxNamesAsync(bool includeFullHierarchy)
        {
            return await ListMailboxNamesAsync("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of all mailbox names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<string[]> ListMailboxNamesAsync(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the LIST command.");

            List<string> mailboxNames = new List<string>();

            // If the server supports XLIST, prefer that over LIST.
            string xListPrefix = "";
            if (ServerSupportsXlist)
                xListPrefix = "X";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (includeFullHierarchy)
                await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %\r\n");
                else
                    await SendCommandAsync(commandTag, xListPrefix + "LIST \"" + mailboxName + "\" %/%\r\n");
            }

            string response = await ReadDataAsync(commandTag, "LIST");

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                int lastParenthesis = responseLine.IndexOf(") ");
                if (lastParenthesis > -1)
                {
                    string[] responseLineParts = responseLine.Substring(lastParenthesis + 2).Split(space, 2);
                    if (responseLineParts.Length > 1)
                        mailboxNames.Add(Functions.FromModifiedUTF7(responseLineParts[1].Replace("\"", "")));
                }
            }

            return mailboxNames.ToArray();
        }

        /// <summary>
        /// Return an array of subscriptions, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<Mailbox[]> ListSubscriptionsAsync(bool includeFullHierarchy)
        {
            return await ListSubscriptionsAsync("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of subscriptions below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<Mailbox[]> ListSubscriptionsAsync(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the LSUB command.");

            List<Mailbox> mailboxes = new List<Mailbox>();

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            if (includeFullHierarchy)
                await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" %\r\n");
                else
                    await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" %/%\r\n");
            }

            string response = await ReadDataAsync(commandTag, "LSUB");

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
        public async Task<string[]> ListSubscriptionNamesAsync(bool includeFullHierarchy)
        {
            return await ListSubscriptionNamesAsync("", includeFullHierarchy);
        }

        /// <summary>
        /// Return an array of subscription names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public async Task<string[]> ListSubscriptionNamesAsync(string mailboxName, bool includeFullHierarchy)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the LSUB command.");

            List<string> mailboxNames = new List<string>();

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            if (includeFullHierarchy)
                await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" *\r\n");
            else
            {
                if (string.IsNullOrEmpty(mailboxName))
                    await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" %\r\n");
                else
                    await SendCommandAsync(commandTag, "LSUB \"" + mailboxName + "\" %/%\r\n");
            }

            string response = await ReadDataAsync(commandTag, "LSUB");

            string[] responseLines = response.Replace("\r", "").Split('\n');
            char[] space = " ".ToCharArray();
            foreach (string responseLine in responseLines)
            {
                string[] responseLineParts = responseLine.Split(space, 5);
                if (responseLineParts.Length > 4)
                    mailboxNames.Add(Functions.FromModifiedUTF7(responseLineParts[4].Replace("\"", "")));
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
            string response = ReadData(commandTag, "LOGOUT");
            SessionIsAuthenticated = false;
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public async Task<bool> MoveMessageAsync(string sourceMailboxName, int index, string destMailboxName)
        {
            return await MoveMessageHelperAsync(sourceMailboxName, index, destMailboxName, false);
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public async Task<bool> MoveMessageUidAsync(string sourceMailboxName, int uid, string destMailboxName)
        {
            return await MoveMessageHelperAsync(sourceMailboxName, uid, destMailboxName, true);
        }

        /// <summary>
        /// Prolong the current session and poll for new messages, but issue no command.
        /// </summary>
        public async Task<bool> NoOpAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the NOOP command.");

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "NOOP\r\n");
            string response = await ReadDataAsync(commandTag, "NOOP");

            return LastCommandResult;
        }

        /// <summary>
        /// Request that the server forwards notifications matching the specified criteria.
        /// </summary>
        /// <param name="notificationCriteria">Well-formatted notification criteria string.</param>
        public async Task<bool> Notify(string notificationCriteria)
        {
            if (ServerSupportsNotify)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    throw new ImapException("Must be connected to the server and authenticated prior to calling the NOTIFY command.");

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, "NOTIFY " + notificationCriteria + "\r\n");
                string response = await ReadDataAsync(commandTag, "NOTIFY");

                return LastCommandResult;
            }
            else
                return false;
        }

        /// <summary>
        /// Read the last response from the IMAP server tied to a specific command tag.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response.</param>
        /// <param name="previousCommand">The previous command issued to the server.</param>
        public async Task<string> ReadDataAsync(string commandTag, string previousCommand)
        {
            StringBuilder responseBuilder = new StringBuilder();

            // Exclusively lock the IMAP stream reader.
            using (await ImapStreamLock.LockAsync())
            {
                // Loop until the anticipated result is received.
                while (true)
                {
                    // Check if another reader instance already found this command tag's response.
                    if (SwitchBoard.ContainsKey(commandTag))
                    {
                        string returnValue = SwitchBoard[commandTag];
                        SwitchBoard.Remove(commandTag);
                        return returnValue;
                    }

                    // If this is a poll during an IDLE command, time out after 5 seconds.
                    if (previousCommand == "IDLE")
                    {
                        if (!ImapTcpClient.Client.Poll(5000, SelectMode.SelectRead))
                            return "";
                    }

                    string responseLine = await ImapStreamReader.ReadLineAsync();

                    // Split the line into its components and check if this is a tagged response.
                    string[] responseParts = responseLine.Split(new char[] { ' ' }, 3);

                    if (responseParts.Length > 1)
                    {
                        // If it's a tagged response and it matches the tag we're expecting, return the proper result.
                        switch (responseParts[1])
                        {
                            // Handle successes.
                            case "OK":
                                string result = "";
                                if (responseBuilder.Length > 0)
                                    result = responseBuilder.ToString();
                                else if (responseParts.Length > 2)
                                    result = responseParts[2];

                                // If the result matches our expected tag, return it; otherwise, save it for later and continue with the next result.
                                if (responseParts[0] == commandTag || previousCommand == "DONE")
                                {
                                    LastCommandResult = true;
                                    return result;
                                }
                                else if (responseParts[0] != "*")
                                {
                                    SwitchBoard[responseParts[0]] = result;
                                    continue;
                                }
                                else
                                {
                                    responseBuilder.Append(responseLine + "\r\n");
                                    continue;
                                }

                            // Handle errors.
                            case "BAD":
                            case "NO":
                                if (responseParts.Length > 2)
                                    LastErrorMessage = responseParts[2];

                                if (responseParts[0] == commandTag)
                                {
                                    LastCommandResult = false;
                                    return "";
                                }
                                else
                                {
                                    SwitchBoard[responseParts[0]] = "";
                                    continue;
                                }

                            // Handle explicit server disconnections by raising the "Disconnect" event.
                            case "BYE":
                                LastCommandResult = false;
                                if (ImapClientDisconnectEvent != null)
                                    OnImapClientDisconnectEvent();
                                Disconnect();
                                return "";

                            // If not a known response, return it verbatim.
                            default:
                                result = responseLine.Substring(responseLine.IndexOf(" ") + 1); ;

                                if (responseParts[0] == commandTag || (responseParts[0] == "+" && responseBuilder.Length == 0))
                                    return result;
                                else
                                    responseBuilder.Append(responseLine + "\r\n");

                                break;
                        }
                    }
                    else
                        responseBuilder.Append(responseLine + "\r\n");

                    // Handle status notifications.
                    if (SessionIsIdle)
                    {
                        if (responseLine.StartsWith("* "))
                        {
                            responseParts = responseLine.Split(new char[] { ' ' }, 4);

                            if (responseParts.Length > 2)
                            {
                                switch (responseParts[2])
                                {
                                    case "EXISTS":
                                        if (ImapClientNewMessageEvent != null)
                                            OnImapClientNewMessageEvent(new ImapClientNewMessageEventArgs(CurrentMailboxName, int.Parse(responseParts[1])));
                                        break;
                                    case "EXPUNGE":
                                        if (ImapClientMessageExpungeEvent != null)
                                            OnImapClientMessageExpungeEvent(new ImapClientMessageExpungeEventArgs(CurrentMailboxName, int.Parse(responseParts[1])));
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public async Task<bool> RemoveFlagsFromMessageAsync(string mailboxName, int index, string[] flags)
        {
            return await RemoveFlagsFromMessageHelperAsync(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public async Task<bool> RemoveFlagsFromMessageUidAsync(string mailboxName, int uid, string[] flags)
        {
            return await RemoveFlagsFromMessageHelperAsync(mailboxName, uid, flags, true);
        }
        
        /// <summary>
        /// Rename a mailbox.
        /// </summary>
        /// <param name="currentMailboxName">The name of the current mailbox to be renamed.</param>
        /// <param name="newMailboxName">The new name of the mailbox.</param>
        public async Task<bool> RenameMailboxAsync(string currentMailboxName, string newMailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the RENAME command.");

            // Encode ampersands and Unicode characters.
            currentMailboxName = Functions.ToModifiedUTF7(currentMailboxName);
            newMailboxName = Functions.ToModifiedUTF7(newMailboxName);

            // Disallow wildcard characters in mailbox names;
            if (newMailboxName.IndexOf("*", StringComparison.Ordinal) > -1 || newMailboxName.IndexOf("%", StringComparison.Ordinal) > -1)
                return false;

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "RENAME " + currentMailboxName + " " + newMailboxName + "\r\n");
            string response = await ReadDataAsync(commandTag, "RENAME");

            return LastCommandResult;
        }

        /// <summary>
        /// Perform a search in the current mailbox and return all matching messages.
        /// </summary>
        /// <param name="searchQuery">Well-formatted IMAP search criteria.</param>
        public async Task<List<ReadOnlyMailMessage>> SearchAsync(string searchQuery)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the SEARCH command.");

            // Strip the command if it was passed since we'll be adding it.
            if (searchQuery.StartsWith("SEARCH "))
                searchQuery = searchQuery.Substring(7);
            else if (searchQuery.StartsWith("UID SEARCH "))
                searchQuery = searchQuery.Substring(11);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "SEARCH " + searchQuery + "\r\n");
            string response = await ReadDataAsync(commandTag, "SEARCH");

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
                            ReadOnlyMailMessage message = await GetMessageAsync(int.Parse(messageID));
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
        public async Task<Mailbox> SelectMailboxAsync(string mailboxName)
        {
            return await SelectMailboxAsync(mailboxName, "");
        }

        /// <summary>
        /// Select a mailbox for subsequent operations and return its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        /// <param name="qResyncParameters">Quick Resynchronization parameters, such as UIDValidity, the last known modification sequence, known UIDs, and/or known sequence ranges.</param>
        public async Task<Mailbox> SelectMailboxAsync(string mailboxName, string qResyncParameters)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the SELECT command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (!string.IsNullOrEmpty(qResyncParameters))
                await SendCommandAsync(commandTag, "SELECT " + mailboxName + " (QRESYNC (" + qResyncParameters + "))\r\n");
            else
                await SendCommandAsync(commandTag, "SELECT " + mailboxName + "\r\n");

            string response = await ReadDataAsync(commandTag, "SELECT");

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
        public async Task SendCommandAsync(string command)
        {
            await SendCommandAsync(SessionCommandTag, command);
        }

        /// <summary>
        /// Send a message to the IMAP server, specifying a unique command tag.
        /// Should always be followed by GetImapStreamString.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response</param>
        /// <param name="command">Text to transmit.</param>
        public async Task SendCommandAsync(string commandTag, string command)
        {
            if (!string.IsNullOrEmpty(commandTag))
            {
                LastCommandIssued = commandTag + " " + command;
                await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, commandTag + " " + command);
            }
            else
                await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, command);
        }

        /// <summary>
        /// Set a quota for the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">Name of the mailbox to work with.</param>
        /// <param name="quotaSize">Size (in MB) of the quota.</param>
        public async Task<bool> SetQuotaAsync(string mailboxName, int quotaSize)
        {
            // Ensure that the server supports Quota extensions.
            if (ServerSupportsQuota)
            {
                // Protect against commands being called out of order.
                if (!IsAuthenticated)
                    throw new ImapException("Must be connected to the server and authenticated prior to calling the SETQUOTA command.");

                // Encode ampersands and Unicode characters.
                mailboxName = Functions.ToModifiedUTF7(mailboxName);

                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, "SETQUOTA \"" + mailboxName + "\" (STORAGE " + quotaSize.ToString() + ")\r\n");
                string response = await ReadDataAsync(commandTag, "SETQUOTA");

                return LastCommandResult;
            }
            else
                return false;
        }

        /// <summary>
        /// Negotiate DEFLATE stream compression for the current session.
        /// </summary>
        public void StartDeflateCompression()
        {
            if (ServerCompressionSupport.Contains("DEFLATE"))
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "COMPRESS DEFLATE\r\n");
                string response = ReadData(commandTag, "COMPRESS");

                if (!(ImapStream is DeflateStream))
                    ImapStream = new DeflateStream(ImapStream, CompressionLevel.Fastest);
            }
        }

        /// <summary>
        /// Negotiate TLS security for the current session.
        /// </summary>
        public void StartTLS()
        {
            StartTLS(false);
        }

        /// <summary>
        /// Negotiate TLS security for the current session.
        /// </summary>
        public void StartTLS(bool sendCommand)
        {
            if (sendCommand)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                SendCommand(commandTag, "STARTTLS\r\n");
                string response = ReadData(commandTag, "STARTTLS");
            }

            if (!(ImapStream is SslStream))
                ImapStream = new SslStream(ImapStream);

            if (!((SslStream)ImapStream).IsAuthenticated)
                ((SslStream)ImapStream).AuthenticateAsClient(Host);
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public async Task<bool> StoreFlagsAsync(string mailboxName, int index, string[] flags)
        {
            return await StoreFlagsHelperAsync(mailboxName, index, flags, false);
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public async Task<bool> StoreFlagsUidAsync(string mailboxName, int uid, string[] flags)
        {
            return await StoreFlagsHelperAsync(mailboxName, uid, flags, true);
        }

        /// <summary>
        /// Subscribe to a mailbox to monitor changes.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public async Task<bool> SubscribeMailboxAsync(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the SUBSCRIBE command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "SUBSCRIBE " + mailboxName + "\r\n");
            string response = await ReadDataAsync(commandTag, "SUBSCRIBE");

            return LastCommandResult;
        }

        /// <summary>
        /// Iterates the command tag counter and returns a new ID for uniquely identifying commands.
        /// </summary>
        public string UniqueCommandTag()
        {
            return (++CommandTagCounter).ToString();
        }

        /// <summary>
        /// Stop subscribing to a mailbox.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public async Task<bool> UnsubscribeMailboxAsync(string mailboxName)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the UNSUBSCRIBE command.");

            // Encode ampersands and Unicode characters.
            mailboxName = Functions.ToModifiedUTF7(mailboxName);

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            await SendCommandAsync(commandTag, "UNSUBSCRIBE " + mailboxName + "\r\n");
            string response = await ReadDataAsync(commandTag, "UNSUSBSCRIBE");

            return LastCommandResult;
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
        private async Task<bool> AddFlagsToMessageHelperAsync(string mailboxName, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the STORE command.");

            // Ensure that at least one flag will be added.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                await SendCommandAsync(commandTag, "UID STORE " + id.ToString() + " +Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                await SendCommandAsync(commandTag, "STORE " + id.ToString() + " +Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = await ReadDataAsync(commandTag, "STORE");
            return LastCommandResult;
        }

        /// <summary>
        /// Periodically check for message notifications.
        /// </summary>
        private async void CheckIdle(object idleInitializedTimeObject)
        {
            DateTime idleInitializedTime = (DateTime)idleInitializedTimeObject;
            
            while (IdleInitializedTime == idleInitializedTime)
                await ReadDataAsync("", "IDLE");
        }

        /// <summary>
        /// Helper function to load an instance of a message in a specified mailbox based on its index, optionally returning only headers and/or setting the "Seen" flag.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="id">The identifier of the message to load, either its index or UID.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        /// <param name="setSeenFlag">Whether to touch the message and set its "Seen" flag.</param>
        /// <param name="isUid">Whether the identifer is an UID.</param>
        private async Task<ReadOnlyMailMessage> GetMessageHelper(string mailboxName, int id, bool headersOnly, bool setSeenFlag, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the FETCH command.");

            if (mailboxName != CurrentMailboxName)
                await SelectMailboxAsync(mailboxName);

            string uidPrefix = isUid ? "UID " : "";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            // Format the command depending on whether we want headers only and whether we want to mark messages as seen.
            if (headersOnly)
                await SendCommandAsync(commandTag, uidPrefix + "FETCH " + id + " (BODY[HEADER] UID)\r\n");
            else
            {
                if (setSeenFlag)
                    await SendCommandAsync(commandTag, uidPrefix + "FETCH " + id + " (BODY[] UID FLAGS)\r\n");
                else
                    await SendCommandAsync(commandTag, uidPrefix + "FETCH " + id + " (BODY.PEEK[] UID FLAGS)\r\n");
            }

            string response = await ReadDataAsync(commandTag, "FETCH");

            // Ensure the message was actually found.
            if (response.IndexOf("\r\n", StringComparison.Ordinal) > -1)
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
                message.Mailbox = mailboxName;
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
        private async Task<bool> MoveMessageHelperAsync(string sourceMailboxName, int id, string destMailboxName, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the MOVE command.");

            // Encode ampersands and Unicode characters.
            sourceMailboxName = Functions.ToModifiedUTF7(sourceMailboxName);
            destMailboxName = Functions.ToModifiedUTF7(destMailboxName);

            // Ensure we're working with the right mailbox.
            if (sourceMailboxName != CurrentMailboxName)
                await SelectMailboxAsync(sourceMailboxName);

            string uidPrefix = isUid ? "UID " : "";

            // If server supports the MOVE command, use that.  Otherwise, copy it to its destination then delete the original.
            if (ServerSupportsMove)
            {
                // Generate a unique command tag for tracking this command and its response.
                string commandTag = UniqueCommandTag();

                await SendCommandAsync(commandTag, uidPrefix + "MOVE " + id.ToString() + " " + destMailboxName + "\r\n");
                string response = await ReadDataAsync(commandTag, "MOVE");

                return LastCommandResult;
            }
            else
            {
                if (await CopyMessageAsync(sourceMailboxName, id, destMailboxName))
                {
                    if (await DeleteMessageAsync(sourceMailboxName, id))
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
        private async Task<bool> StoreFlagsHelperAsync(string mailboxName, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the STORE command.");

            // Ensure that at least one flag will be removed.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                await SendCommandAsync(commandTag, "UID STORE " + id.ToString() + " Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                await SendCommandAsync(commandTag, "STORE " + id.ToString() + " Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = await ReadDataAsync(commandTag, "STORE");
            return LastCommandResult;
        }

        /// <summary>
        /// Remove one or more flags from a message.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="id">Identifier of the message to update, either index or UID.</param>
        /// <param name="flags">List of flags to remove.</param>
        /// <param name="isUid">Whether the ID was passed as a UID.</param>
        private async Task<bool> RemoveFlagsFromMessageHelperAsync(string mailboxName, int id, string[] flags, bool isUid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new ImapException("Must be connected to the server and authenticated prior to calling the STORE command.");

            // Ensure that at least one flag will be removed.
            if (flags.Length < 1)
                return false;

            string flagsString = "";
            foreach (string flag in flags)
                flagsString += flag + " ";

            // Generate a unique command tag for tracking this command and its response.
            string commandTag = UniqueCommandTag();

            if (isUid)
                await SendCommandAsync(commandTag, "UID STORE " + id.ToString() + " -Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");
            else
                await SendCommandAsync(commandTag, "STORE " + id.ToString() + " -Flags (" + flagsString.Substring(0, flagsString.Length - 1) + ")\r\n");

            string response = await ReadDataAsync(commandTag, "STORE");
            return LastCommandResult;
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Provides data for ImapClient "EXISTS" events.
    /// </summary>
    public class ImapClientNewMessageEventArgs : EventArgs
    {
        /// <summary>Name of the mailbox containing the message to be processed.</summary>
        public string MailboxName;
        /// <summary>ID of the message to be processed.</summary>
        public int MessageId;
        /// <summary>Number of messages in the mailbox.</summary>
        public int MessageCount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mailboxName">Name of the mailbox containing the message to be processed.</param>
        /// <param name="messageCount">Number of messages in the mailbox.</param>
        public ImapClientNewMessageEventArgs(string mailboxName, int messageCount)
        {
            MailboxName = mailboxName;
            MessageCount = messageCount;
        }
    }

    /// <summary>
    /// Provides data for ImapClient "EXPUNGE" events.
    /// </summary>
    public class ImapClientMessageExpungeEventArgs : EventArgs
    {
        /// <summary>Name of the mailbox containing the message to be processed.</summary>
        public string MailboxName;
        /// <summary>ID of the message to be processed.</summary>
        public int MessageId;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mailboxName">Name of the mailbox containing the message to be processed.</param>
        /// <param name="messageCount">ID of the message to be processed.</param>
        public ImapClientMessageExpungeEventArgs(string mailboxName, int messageId)
        {
            MailboxName = mailboxName;
            MessageId = messageId;
        }
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
