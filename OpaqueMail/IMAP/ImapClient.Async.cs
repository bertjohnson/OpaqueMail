using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    public partial class ImapClient : IDisposable
    {
        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, string message)
        {
            return await AppendMessageAsync(mailboxName, message, new string[] { }, null);
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public async Task<bool> AppendMessageAsync(string mailboxName, string message, string[] flags, DateTime? date)
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
            await SendCommandAsync(commandTag, commandBuilder.ToString());

            // Confirm the server is ready to accept our raw data.
            string response = await Functions.ReadStreamStringAsync(ImapStream, InternalBuffer);
            if (response.StartsWith("+"))
            {
                await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, message + "\r\n");
                response = await ReadDataAsync(commandTag);

                return LastCommandResult;
            }

            return false;
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
        /// <param name="uid">The index of the message to load.</param>
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
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
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
                return null;

            if (mailboxName != CurrentMailboxName)
                SelectMailbox(mailboxName);

            List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();
            int numMessages = GetMessageCount();

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
        /// Read the last response from the IMAP server.
        /// </summary>
        public async Task<string> ReadDataAsync()
        {
            return await ReadDataAsync(SessionCommandTag);
        }

        /// <summary>
        /// Read the last response from the IMAP server tied to a specific command tag.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response</param>
        public async Task<string> ReadDataAsync(string commandTag)
        {
            string response = "";

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                response += await Functions.ReadStreamStringAsync(ImapStream, InternalBuffer);

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
        /// Perform a search in the current mailbox and return all matching messages.
        /// </summary>
        /// <param name="searchQuery">Well-formatted IMAP search criteria.</param>
        public async Task<List<ReadOnlyMailMessage>> SearchAsync(string searchQuery)
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

            await SendCommandAsync(commandTag, "SEARCH " + searchQuery + "\r\n");
            string response = await ReadDataAsync(commandTag);

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
        /// Send a message to the IMAP server.
        /// Should always be followed by GetImapStreamString.
        /// </summary>
        /// <param name="command">Text to transmit.</param>
        public async void SendCommandAsync(string command)
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
            LastCommandIssued = commandTag + " " + command;
            await Functions.SendStreamStringAsync(ImapStream, InternalBuffer, commandTag + " " + command);
        }

    }
}
