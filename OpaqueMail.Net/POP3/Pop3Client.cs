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
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Allows applications to retrieve and manage e-mail by using the Post Office Protocol (POP3).
    /// </summary>
    /// <remarks>Includes OpaqueMail extensions to facilitate processing of secure S/MIME messages.</remarks>
    public class Pop3Client : IDisposable
    {
        #region Public Members
        /// <summary>Optional shared secret for securing authentication via the APOP command.</summary>
        public string APOPSharedSecret = "";
        /// <summary>Gets or sets the credentials used to authenticate.</summary>
        public NetworkCredential Credentials;
        /// <summary>Specify whether the OpaqueMail.Pop3Client uses Secure Sockets Layer (SSL).</summary>
        public bool EnableSsl;
        /// <summary>Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</summary>
        public ReadOnlyMailMessageProcessingFlags ProcessingFlags = ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody;
        /// <summary>Set of extended POP3 capabilities.</summary>
        public HashSet<string> Capabilities = new HashSet<string>();
        /// <summary>Gets or sets the name or IP address of the host used for POP3 transactions.</summary>
        public string Host;
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
                if (Pop3TcpClient != null)
                    return Pop3TcpClient.Connected;
                else
                    return false;
            }
        }
        /// <summary>The authentication state when capabilities were last queried.</summary>
        private bool LastCapabilitiesCheckAuthenticationState = false;
        /// <summary>The last command issued to the POP3 server.</summary>
        public string LastCommandIssued;
        /// <summary>Whether the last POP3 command was successful.</summary>
        public bool LastCommandResult = false;
        /// <summary>The last error message returned by the POP3 server.</summary>
        public string LastErrorMessage;
        /// <summary>Gets or sets the port used for POP3 transactions.</summary>
        public int Port;
        /// <summary>Gets or sets a value, in milliseconds, that determines how long the stream will attempt to read before timing out.</summary>
        public int ReadTimeout
        {
            get
            {
                if (Pop3Stream != null)
                    return Pop3Stream.ReadTimeout;
                else
                    return -1;
            }
            set
            {
                if (Pop3Stream != null)
                    Pop3Stream.ReadTimeout = value;
            }
        }
        /// <summary>The welcome message provided by the POP3 server.</summary>
        public string WelcomeMessage
        {
            get { return SessionWelcomeMessage; }
        }
        /// <summary>Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.</summary>
        public int WriteTimeout
        {
            get
            {
                if (Pop3Stream != null)
                    return Pop3Stream.WriteTimeout;
                else
                    return -1;
            }
            set
            {
                if (Pop3Stream != null)
                    Pop3Stream.WriteTimeout = value;
            }
        }
        #endregion Public Members

        #region Private Members
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] InternalBuffer = new byte[Constants.LARGEBUFFERSIZE];
        /// <summary>Connection to the remote POP3 server.</summary>
        private TcpClient Pop3TcpClient;
        /// <summary>Stream for communicating with the POP3 server.</summary>
        private Stream Pop3Stream;
        /// <summary>The welcome message provided by the POP3 server.</summary>
        private String SessionWelcomeMessage = "";
        /// <summary>Returns the POP3 server's expiration policy as found from the CAPA command.</summary>
        private string ServerExpirationPolicy = "";
        /// <summary>Returns the POP3 server's implementation string as found from the CAPA command.</summary>
        private string ServerImplementation = "";
        /// <summary>Returns the POP3 server's login delay as found from the CAPA command.</summary>
        private int ServerLoginDelay = 0;
        /// <summary>Whether the session has successfully been authenticated.</summary>
        private bool SessionIsAuthenticated = false;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.Pop3Client class by using the specified settings.
        /// </summary>
        /// <param name="host">Name or IP of the host used for POP3 transactions.</param>
        /// <param name="port">Port to be used by the host.</param>
        /// <param name="userName">The username associated with this connection.</param>
        /// <param name="password">The password associated with this connection.</param>
        /// <param name="enableSSL">Whether the POP3 connection uses TLS / SSL protection.</param>
        public Pop3Client(string host, int port, string userName, string password, bool enableSSL)
        {
            Host = host;
            Port = port;
            Credentials = new NetworkCredential(userName, password);
            EnableSsl = enableSSL;
        }

        /// <summary>
        /// Default destructor.
        /// </summary>
        ~Pop3Client()
        {
            if (Pop3Stream != null)
                Pop3Stream.Dispose();
            if (Pop3TcpClient != null)
                Pop3TcpClient.Close();
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Perform an authentication handshake with the POP3 server.
        /// </summary>
        public bool Authenticate()
        {
            string response = "";

            // If an APOP shared secret has been established between the client and server, require that authentication.
            if (!string.IsNullOrEmpty(APOPSharedSecret))
            {
                string[] welcomeMessageComponents = WelcomeMessage.Split(' ');
                SendCommand("APOP " + Credentials.UserName + " " + Functions.MD5(Credentials.Password + welcomeMessageComponents[welcomeMessageComponents.Length - 1] + APOPSharedSecret) + "\r\n");
                response = ReadData();

                SessionIsAuthenticated = LastCommandResult;
                return LastCommandResult;
            }
            else
            {
                SendCommand("USER " + Credentials.UserName + "\r\n");
                response = ReadData();
                if (!LastCommandResult)
                    return false;

                SendCommand("PASS " + Credentials.Password + "\r\n");
                response = ReadData();

                SessionIsAuthenticated = LastCommandResult;
                return LastCommandResult;
            }
        }

        /// <summary>
        /// Connect to the remote POP3 server.
        /// </summary>
        public bool Connect()
        {
            try
            {
                Pop3TcpClient = new TcpClient();
                Pop3TcpClient.Connect(Host, Port);
                Pop3Stream = Pop3TcpClient.GetStream();

                if (EnableSsl)
                    StartTLS();

                // Remember the welcome message.
                SessionWelcomeMessage = ReadData();
                if (!LastCommandResult)
                    return false;

                return true;
            }
            catch
            {
                if (Pop3Stream != null)
                    Pop3Stream.Dispose();
                if (Pop3TcpClient != null)
                    Pop3TcpClient.Close();

                return false;
            }
        }

        /// <summary>
        /// Delete a message from the server based on its index.
        /// </summary>
        /// <param name="index">Index of the message to delete.</param>
        public bool DeleteMessage(int index)
        {
            return Task.Run(() => DeleteMessageAsync(index)).Result;
        }

        /// <summary>
        /// Delete a message from the server based on its index.
        /// </summary>
        /// <param name="uid">UID of the message to delete.</param>
        public bool DeleteMessageUid(string uid)
        {
            return Task.Run(() => DeleteMessageAsync(uid)).Result;
        }

        /// <summary>
        /// Delete a message from the server based on its index.
        /// </summary>
        /// <param name="index">Index of the message to delete.</param>
        public async Task<bool> DeleteMessageAsync(int index)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            await SendCommandAsync("DELE " + index.ToString() + "\r\n");
            string response = await ReadDataAsync();
            return LastCommandResult;
        }

        /// <summary>
        /// Delete a message from the server based on its UID.
        /// </summary>
        /// <param name="uid">UID of the message to delete.</param>
        public async Task<bool> DeleteMessageAsync(string uid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            await SendCommandAsync("DELE " + uid + "\r\n");
            string response = await ReadDataAsync();
            return LastCommandResult;
        }

        /// <summary>
        /// Delete a series of messages from the server based on their indices.
        /// </summary>
        /// <param name="indices">Array of message indices to delete.</param>
        public bool DeleteMessages(int[] indices)
        {
            return Task.Run(() => DeleteMessagesAsync(indices)).Result;
        }

        /// <summary>
        /// Delete a series of messages from the server based on their indices.
        /// </summary>
        /// <param name="indices">Array of message indices to delete.</param>
        public async Task<bool> DeleteMessagesAsync(int[] indices)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            bool returnValue = true;
            foreach (int index in indices)
            {
                await SendCommandAsync("DELE " + index.ToString() + "\r\n");
                string response = await ReadDataAsync();
                returnValue &= LastCommandResult;
            }

            return returnValue;
        }

        /// <summary>
        /// Delete a series of messages from the server based on their UIDs.
        /// </summary>
        /// <param name="uids">Array of message UIDs to delete.</param>
        public bool DeleteMessages(string[] uids)
        {
            return Task.Run(() => DeleteMessagesAsync(uids)).Result;
        }

        /// <summary>
        /// Delete a series of messages from the server based on their UIDs.
        /// </summary>
        /// <param name="uids">Array of message UIDs to delete.</param>
        public async Task<bool> DeleteMessagesAsync(string[] uids)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            bool returnValue = true;
            foreach (string uid in uids)
            {
                await SendCommandAsync("DELE " + uid + "\r\n");
                string response = await ReadDataAsync();
                returnValue &= LastCommandResult;
            }

            return returnValue;
        }

        /// <summary>
        /// Sends a QUIT message to the POP3 server, gracefully ends the TCP connection, and releases all resources used by the current instances of the OpaqueMail.Pop3Client class.
        /// </summary>
        public void Dispose()
        {
            if (Pop3TcpClient != null)
            {
                if (Pop3TcpClient.Connected)
                {
                    SendCommand("QUIT\r\n");
                    string response = ReadData();
                }
                Pop3TcpClient.Close();
                Pop3TcpClient = null;
            }
            if (Pop3Stream != null)
                Pop3Stream.Dispose();
        }

        /// <summary>
        /// Retrieve a list of the POP3's servers extended capabilities.
        /// </summary>
        public string[] GetCapabilities()
        {
            return Task.Run(() => GetCapabilitiesAsync()).Result;
        }

        /// <summary>
        /// Retrieve a list of the POP3's servers extended capabilities.
        /// </summary>
        public async Task<string[]> GetCapabilitiesAsync()
        {
            // If we've logged in or out since last checking capabilities, ignore the cache.
            if (LastCapabilitiesCheckAuthenticationState == IsAuthenticated)
            {
                // Send pre-cached capabilities if they exist.
                if (Capabilities.Count > 0)
                    return Capabilities.ToArray();
            }

            LastCapabilitiesCheckAuthenticationState = IsAuthenticated;

            await SendCommandAsync("CAPA\r\n");
            string response = await ReadDataAsync();

            if (LastCommandResult)
            {
                ServerSupportsTop = false;
                ServerSupportsUIDL = false;

                // Ignore the first and last line of the response.
                string[] capabilitiesLines = response.Replace("\r", "").Split('\n');
                string[] capabilitiesList = new string[capabilitiesLines.Length - 1];

                // Look for known capabilities to populate Pop3Client properties.
                for (int i = 1; i < capabilitiesLines.Length; i++)
                {
                    if (capabilitiesLines[i].StartsWith("EXPIRE "))
                        ServerExpirationPolicy = capabilitiesLines[i].Substring(7);
                    else if (capabilitiesLines[i].StartsWith("IMPLEMENTATION "))
                        ServerImplementation = capabilitiesLines[i].Substring(15);
                    else if (capabilitiesLines[i].StartsWith("LOGIN-DELAY "))
                        int.TryParse(capabilitiesLines[i].Substring(12), out ServerLoginDelay);
                    else if (capabilitiesLines[i].StartsWith("LOGIN-DELAY "))
                        int.TryParse(capabilitiesLines[i].Substring(12), out ServerLoginDelay);
                    else
                    {
                        switch (capabilitiesLines[i])
                        {
                            case "SASL":
                                ServerSupportsSASL = true;
                                break;
                            case "STLS":
                                ServerSupportsSTLS = true;
                                break;
                            case "TOP":
                                ServerSupportsTop = true;
                                break;
                            case "UIDL":
                                ServerSupportsUIDL = true;
                                break;
                        }
                    }

                    capabilitiesList[i - 1] = capabilitiesLines[i];
                    Capabilities.Add(capabilitiesLines[i]);
                }

                return capabilitiesList;
            }
            else
                return new string[] { };
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its index.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        public ReadOnlyMailMessage GetMessage(int index)
        {
            return Task.Run(() => GetMessageAsync(index)).Result;
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its UID.
        /// </summary>
        /// <param name="uid">The UID of the message, as returned by a UIDL command.</param>
        public ReadOnlyMailMessage GetMessageUid(string uid)
        {
            return Task.Run(() => GetMessageUidAsync(uid)).Result;
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its index, optionally returning only headers.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public ReadOnlyMailMessage GetMessage(int index, bool headersOnly)
        {
            return Task.Run(() => GetMessageAsync(index, headersOnly)).Result;
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its UID, optionally returning only headers.
        /// </summary>
        /// <param name="uid">The UID of the message, as returned by a UIDL command.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public ReadOnlyMailMessage GetMessageUid(string uid, bool headersOnly)
        {
            return Task.Run(() => GetMessageUidAsync(uid, headersOnly)).Result;
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its index.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        public async Task<ReadOnlyMailMessage> GetMessageAsync(int index)
        {
            return await GetMessageHelper(index, "", false);
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its UID.
        /// </summary>
        /// <param name="uid">The UID of the message, as returned by a UIDL command.</param>
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string uid)
        {
            return await GetMessageHelper(-1, uid, false);
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its index, optionally returning only headers.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public async Task<ReadOnlyMailMessage> GetMessageAsync(int index, bool headersOnly)
        {
            return await GetMessageHelper(index, "", headersOnly);
        }

        /// <summary>
        /// Retrieve a specific message from the server based on its UID, optionally returning only headers.
        /// </summary>
        /// <param name="uid">The UID of the message, as returned by a UIDL command.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string uid, bool headersOnly)
        {
            return await GetMessageHelper(-1, uid, headersOnly);
        }

        /// <summary>
        /// Return the number of messages on the POP3 server.
        /// </summary>
        public int GetMessageCount()
        {
            return Task.Run(() => GetMessageCountAsync()).Result;
        }

        /// <summary>
        /// Return the number of messages on the POP3 server.
        /// </summary>
        public async Task<int> GetMessageCountAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the STAT command.");

            await SendCommandAsync("STAT\r\n");
            string response = await ReadDataAsync();

            // Handle POP3 server errors.
            if (!LastCommandResult)
                return -1;

            string[] responseParts = response.Split(' ');
            int numMessages = 0;
            int.TryParse(responseParts[0], out numMessages);

            return numMessages;
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages on the POP3 server.
        /// </summary>
        public List<ReadOnlyMailMessage> GetMessages()
        {
            return Task.Run(() => GetMessagesAsync()).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count)
        {
            return Task.Run(() => GetMessagesAsync(count)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(count, headersOnly)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, starting at a specific index.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="startIndex">The relative 1-indexed message to start at.</param>
        /// <param name="reverseOrder">Whether to return messages in descending order.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count, int startIndex, bool reverseOrder)
        {
            return Task.Run(() => GetMessagesAsync(count, startIndex, reverseOrder)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="startIndex">The relative 1-indexed message to start with.</param>
        /// <param name="reverseOrder">Whether to return messages in descending order.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public List<ReadOnlyMailMessage> GetMessages(int count, int startIndex, bool reverseOrder, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(count, startIndex, reverseOrder, headersOnly)).Result;
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages on the POP3 server.
        /// </summary>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync()
        {
            return await GetMessagesAsync(25, 1, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count)
        {
            return await GetMessagesAsync(count, 1, false, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, bool headersOnly)
        {
            return await GetMessagesAsync(count, 1, false, headersOnly);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, starting at a specific index.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="startIndex">The relative 1-indexed message to start at.</param>
        /// <param name="reverseOrder">Whether to return messages in descending order.</param>
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, int startIndex, bool reverseOrder)
        {
            return await GetMessagesAsync(count, startIndex, reverseOrder, false);
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages on the POP3 server, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="startIndex">The relative 1-indexed message to start with.</param>
        /// <param name="reverseOrder">Whether to return messages in descending order.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, int startIndex, bool reverseOrder, bool headersOnly)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the TOP or RETR commands.");

            string response;
            List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();
            Dictionary<int, string> uidls = new Dictionary<int, string>();

            // Try to retrieve a list of unique IDs using the UIDL command.
            if (ServerSupportsUIDL != false)
            {
                await SendCommandAsync("UIDL\r\n");
                response = (await ReadDataAsync("\r\n.\r\n")).Replace("\r", "");
                string[] responseLines = response.Split('\n');
                if (responseLines.Length > 0)
                {
                    foreach (string responseLine in responseLines)
                    {
                        string[] responseParts = responseLine.Split(new char[] { ' ' }, 2);
                        if (!uidls.ContainsKey(int.Parse(responseParts[0])))
                            uidls.Add(int.Parse(responseParts[0]), responseParts[1]);
                    }
                }
                else
                    ServerSupportsUIDL = false;
            }
 
            // Retrieve the current message count.
            int numMessages = GetMessageCount();

            if (numMessages > 0)
            {
                response = "";
                int messagesReturned = 0;

                int loopStartIndex = reverseOrder ? numMessages + 1 - startIndex : startIndex;
                int loopIterateCount = reverseOrder ? -1 : 1;
                int loopIterations = 0;
                for (int i = loopStartIndex; loopIterations < numMessages; i+=loopIterateCount)
                {
                    ReadOnlyMailMessage message = await GetMessageHelper(i, "", headersOnly);
                    if (message != null)
                    {
                        message.Index = i;
                        if (ServerSupportsUIDL == true)
                        {
                            if (uidls.ContainsKey(i))
                                message.Pop3Uidl = uidls[i];
                        }
                        messages.Add(message);
                        messagesReturned++;
                    }

                    if (messagesReturned >= count)
                        break;
                    else
                        loopIterations++;
                }
            }

            return messages;
        }

        /// <summary>
        /// Determine the UID of a message according to the UIDL command.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        public string GetUidl(int index)
        {
            return Task.Run(() => GetUidlAsync(index)).Result;
        }

        /// <summary>
        /// Determine the UID of a message according to the UIDL command.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        public async Task<string> GetUidlAsync(int index)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the UIDL command.");

            await SendCommandAsync("UIDL " + index.ToString() + "\r\n");
            string response = await ReadDataAsync();

            // Validate the response is in the correct format.
            if (LastCommandResult){
                string[] responseSections = response.Split(new char[] {' '}, 2);
                if (responseSections.Length > 1)
                    return responseSections[1];
                else
                    return "";
            }
            else
            {
                ServerSupportsUIDL = false;
                return "";
            }
        }

        /// <summary>
        /// Log out and end the current session.
        /// </summary>
        public void LogOut()
        {
            SendCommand("QUIT\r\n");
            string response = ReadData();
            SessionIsAuthenticated = false;
        }

        /// <summary>
        /// Prolong the current session, but issue no command.
        /// </summary>
        public bool NoOp()
        {
            return Task.Run(() => NoOpAsync()).Result;
        }

        /// <summary>
        /// Prolong the current session, but issue no command.
        /// </summary>
        public async Task<bool> NoOpAsync()
        {
            await SendCommandAsync("NOOP\r\n");
            string response = await ReadDataAsync();

            return LastCommandResult;
        }

        /// <summary>
        /// Read the last response from the POP3 server.
        /// </summary>
        public string ReadData()
        {
            return ReadData("");
        }

        /// <summary>
        /// Read the last response from the POP3 server.
        /// </summary>
        /// <param name="endMarker">A string indicating the end of the current message.</param>
        public string ReadData(string endMarker)
        {
            string response = "";

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                int bytesRead = Pop3Stream.Read(InternalBuffer, 0, Constants.LARGEBUFFERSIZE);
                response += Encoding.UTF8.GetString(InternalBuffer, 0, bytesRead);

                // Deal with bad commands and responses with errors.
                if (firstResponse && response.StartsWith("-"))
                {
                    LastErrorMessage = response.Substring(0, response.Length - 2).Substring(response.IndexOf(" ") + 1);
                    return "";
                }

                // Check if the last sequence received ends with a line break, possibly indicating an end of message.
                if (response.EndsWith("\r\n"))
                {
                    if (endMarker.Length > 0)
                    {
                        if (response.EndsWith(endMarker))
                        {
                            receivingMessage = false;

                            // Strip start +OK message.
                            if (response.StartsWith("+OK\r\n"))
                                response = response.Substring(5);
                            else
                                response = response.Substring(4);

                            // Strip end POP3 padding.
                            response = response.Substring(0, response.Length - endMarker.Length);
                        }
                    }
                    else
                    {
                        // Check if the message includes a POP3 "OK" signature, signifying the message is complete.
                        // Eliminate POP3 message padding.
                        if (response.StartsWith("+OK\r\n"))
                        {
                            receivingMessage = false;
                            response = response.Substring(5);
                        }
                        else if (response.StartsWith("+OK"))
                        {
                            receivingMessage = false;
                            response = response.Substring(4, response.Length - 6);
                        }
                    }
                }

                firstResponse = false;
            }

            LastCommandResult = true;
            return response;
        }
        
        /// <summary>
        /// Read the last response from the POP3 server.
        /// </summary>
        public async Task<string> ReadDataAsync()
        {
            return await ReadDataAsync("");
        }

        /// <summary>
        /// Read the last response from the POP3 server.
        /// </summary>
        /// <param name="endMarker">A string indicating the end of the current message.</param>
        public async Task<string> ReadDataAsync(string endMarker)
        {
            string response = "";

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                int bytesRead = await Pop3Stream.ReadAsync(InternalBuffer, 0, Constants.LARGEBUFFERSIZE);
                response += Encoding.UTF8.GetString(InternalBuffer, 0, bytesRead);

                // Deal with bad commands and responses with errors.
                if (firstResponse && response.StartsWith("-"))
                {
                    LastErrorMessage = response.Substring(0, response.Length - 2).Substring(response.IndexOf(" ") + 1);
                    return "";
                }

                // Check if the last sequence received ends with a line break, possibly indicating an end of message.
                if (response.EndsWith("\r\n"))
                {
                    if (endMarker.Length > 0)
                    {
                        if (response.EndsWith(endMarker))
                        {
                            receivingMessage = false;

                            // Strip start +OK message.
                            if (response.StartsWith("+OK\r\n"))
                                response = response.Substring(5);
                            else
                                response = response.Substring(4);

                            // Strip end POP3 padding.
                            response = response.Substring(0, response.Length - endMarker.Length);
                        }
                    }
                    else
                    {
                        // Check if the message includes a POP3 "OK" signature, signifying the message is complete.
                        // Eliminate POP3 message padding.
                        if (response.StartsWith("+OK\r\n"))
                        {
                            receivingMessage = false;
                            response = response.Substring(5);
                        }
                        else if (response.StartsWith("+OK"))
                        {
                            receivingMessage = false;
                            response = response.Substring(4, response.Length - 6);
                        }
                    }
                }

                firstResponse = false;
            }

            LastCommandResult = true;
            return response;
        }

        /// <summary>
        /// Unmark any messages during this session so that they will not be deleted upon exiting.
        /// </summary>
        public bool Reset()
        {
            return Task.Run(() => ResetAsync()).Result;
        }

        /// <summary>
        /// Unmark any messages during this session so that they will not be deleted upon exiting.
        /// </summary>
        public async Task<bool> ResetAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the RSET command.");

            await SendCommandAsync("RSET\r\n");
            string response = await ReadDataAsync();

            return LastCommandResult;
        }

        /// <summary>
        /// Helper function to send a message to the POP3 server.
        /// Should always be followed by GetPop3StreamString.
        /// </summary>
        /// <param name="command">Text to transmit.</param>
        public void SendCommand(string command)
        {
            LastCommandIssued = command;
            Functions.SendStreamString(Pop3Stream, InternalBuffer, command);
        }

        /// <summary>
        /// Helper function to send a message to the POP3 server.
        /// Should always be followed by GetPop3StreamString.
        /// </summary>
        /// <param name="command">Text to transmit.</param>
        public async Task SendCommandAsync(string command)
        {
            LastCommandIssued = command;
            await Functions.SendStreamStringAsync(Pop3Stream, InternalBuffer, command);
        }

        /// <summary>
        /// Negotiate TLS security for the current session.
        /// </summary>
        public void StartTLS()
        {
            if (!(Pop3Stream is SslStream))
                Pop3Stream = new SslStream(Pop3TcpClient.GetStream());
            if (!((SslStream)Pop3Stream).IsAuthenticated)
                ((SslStream)Pop3Stream).AuthenticateAsClient(Host);
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Helper function to retrieve a specific message from the server based on its index or UID, optionally returning only headers.
        /// </summary>
        /// <param name="index">The index number of the message to return.</param>
        /// <param name="uid">The UID of the message, as returned by a UIDL command.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        private async Task<ReadOnlyMailMessage> GetMessageHelper(int index, string uid, bool headersOnly)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the RETR command.");

            bool processed = false;
            string response = "";

            // Determine whether we're using the index number or UID string.
            string messageID = index > -1 ? index.ToString() : uid;

            // If retrieving headers only, first try the POP3 TOP command.
            if (headersOnly && (ServerSupportsTop != false))
            {
                await SendCommandAsync("TOP " + messageID + " 0\r\n");
                response = await ReadDataAsync("\r\n.\r\n");

                if (LastCommandResult)
                    processed = true;
                ServerSupportsTop = processed;
            }
            if (!processed)
            {
                await SendCommandAsync("RETR " + messageID + "\r\n");
                response = await ReadDataAsync("\r\n.\r\n");
            }

            if (LastCommandResult && response.Length > 0)
            {
                ReadOnlyMailMessage message = new ReadOnlyMailMessage(response, ProcessingFlags);

                if (string.IsNullOrEmpty(uid) && ServerSupportsUIDL != null)
                {
                    message.Index = index;
                    message.Pop3Uidl = await GetUidlAsync(index);
                }
                else
                    message.Pop3Uidl = uid;

                return message;
            }

            // If unable to find or parse the message, return null.
            return null;
        }
        #endregion Private Methods

        #region Public Properties
        /// Returns the POP3 server's pipelining capability as found from the CAPA command.
        private bool ServerSupportsPipelining = false;
        /// <summary>Whether the POP3 server supports the "TOP" command for previewing headers.</summary>
        private bool? ServerSupportsTop = null;
        /// <summary>Whether the POP3 server supports SASL authentication, as found from the CAPA command.</summary>
        private bool ServerSupportsSASL = false;
        /// <summary>Whether the POP3 server supports TLS negotation, as found from the CAPA command.</summary>
        private bool ServerSupportsSTLS = false;
        /// <summary>Whether the POP3 server supports the "UIDL" command for uniquely identifying messages.</summary>
        private bool? ServerSupportsUIDL = null;

        /// <summary>
        /// Returns the POP3 server's expiration policy as found from the CAPA command.
        /// </summary>
        public string ExpirationPolicy
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerExpirationPolicy;
            }
        }

        /// <summary>
        /// Returns the POP3 server's implementation string as found from the CAPA command.
        /// </summary>
        public string Implementation
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerImplementation;
            }
        }

        /// <summary>
        /// Returns the POP3 server's login delay as found from the CAPA command.
        /// </summary>
        public int LoginDelay
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerLoginDelay;
            }
        }

        /// <summary>
        /// Returns the POP3 server's pipelining capability as found from the CAPA command.
        /// </summary>
        public bool SupportsPipelining
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsPipelining;
            }
        }

        /// <summary>
        /// Whether the POP3 server supports SASL authentication, as found from the CAPA command.
        /// </summary>
        public bool SupportsSASL
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsSASL;
            }
        }

        /// <summary>
        /// Whether the POP3 server supports TLS negotation, as found from the CAPA command.
        /// </summary>
        public bool SupportsSTLS
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsSTLS;
            }
        }

        /// <summary>
        /// Whether the POP3 server supports the "TOP" command, as found from the CAPA command.
        /// </summary>
        public bool SupportsTop
        {
            get
            {
                // Check if we've explicitly found this to be true.
                if (ServerSupportsTop != null)
                    return ServerSupportsTop == true ? true : false;

                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsTop == true ? true : false;
            }
        }

        /// <summary>
        /// Whether the POP3 server supports the "UIDL" command, as found from the CAPA command.
        /// </summary>
        public bool SupportsUIDL
        {
            get
            {
                // Check if we've explicitly found this to be true.
                if (ServerSupportsUIDL != null)
                    return ServerSupportsUIDL == true ? true : false;

                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsUIDL == true ? true : false;
            }
        }
        #endregion Public Properties
    }

    /// <summary>
    /// Represents the exception that is thrown when the OpaqueMail.Pop3Client is not able to complete an operation.
    /// </summary>
    public class Pop3Exception : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.Pop3Exception class.
        /// </summary>
        public Pop3Exception() : base() { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.Pop3Exception class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        public Pop3Exception(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.Pop3Exception class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public Pop3Exception(string message, Exception innerException) : base(message, innerException) { }
    }
}
