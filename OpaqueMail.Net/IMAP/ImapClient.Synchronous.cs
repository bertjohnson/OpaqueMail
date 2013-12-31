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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    public partial class ImapClient
    {
        #region Public Methods
        /// <summary>
        /// Add one or more flags to a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public bool AddFlagsToMessage(string mailboxName, int index, string[] flags)
        {
            return Task.Run(() => AddFlagsToMessageHelperAsync(mailboxName, index, flags, false)).Result;
        }

        /// <summary>
        /// Add one or more flags to a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to add.</param>
        public bool AddFlagsToMessageUid(string mailboxName, int uid, string[] flags)
        {
            return Task.Run(() => AddFlagsToMessageHelperAsync(mailboxName, uid, flags, true)).Result;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message text to append.</param>
        public bool AppendMessage(string mailboxName, string message)
        {
            return Task.Run(() => AppendMessageAsync(mailboxName, message)).Result;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        public bool AppendMessage(string mailboxName, ReadOnlyMailMessage message)
        {
            return Task.Run(() => AppendMessageAsync(mailboxName, message)).Result;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message text to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public bool AppendMessage(string mailboxName, string message, string[] flags, DateTime? date)
        {
            return Task.Run(() => AppendMessageAsync(mailboxName, message, flags, date)).Result;
        }

        /// <summary>
        /// Appends a message to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="message">The raw message to append.</param>
        /// <param name="flags">Optional flags to be applied for the message.</param>
        /// <param name="date">Optional date for the message.</param>
        public bool AppendMessage(string mailboxName, ReadOnlyMailMessage message, string[] flags, DateTime? date)
        {
            return Task.Run(() => AppendMessageAsync(mailboxName, message, flags, date)).Result;
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages text to append.</param>
        public bool AppendMessages(string mailboxName, string[] messages)
        {
            return Task.Run(() => AppendMessagesAsync(mailboxName, messages, new string[] { }, null)).Result;
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages to append.</param>
        public bool AppendMessages(string mailboxName, ReadOnlyMailMessage[] messages)
        {
            return Task.Run(() => AppendMessagesAsync(mailboxName, messages, new string[] { }, null)).Result;
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages text to append.</param>
        /// <param name="flags">Optional flags to be applied for all messages.</param>
        /// <param name="date">Optional date for all messages.</param>
        public bool AppendMessages(string mailboxName, string[] messages, string[] flags, DateTime? date)
        {
            return Task.Run(() => AppendMessagesAsync(mailboxName, messages, flags, date)).Result;
        }

        /// <summary>
        /// Appends messages to the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to append to.</param>
        /// <param name="messages">The raw messages to append.</param>
        /// <param name="flags">Optional flags to be applied for all messages.</param>
        /// <param name="date">Optional date for all messages.</param>
        public bool AppendMessages(string mailboxName, ReadOnlyMailMessage[] messages, string[] flags, DateTime? date)
        {
            return Task.Run(() => AppendMessagesAsync(mailboxName, messages, flags, date)).Result;
        }

        /// <summary>
        /// Request a checkpoint of the currently selected mailbox.
        /// </summary>
        public bool Check()
        {
            return Task.Run(() => CheckAsync()).Result;        
        }

        /// <summary>
        /// Close the currently selected mailbox and remove all messages with the "\Deleted" flag.
        /// </summary>
        /// <returns></returns>
        public bool CloseMailbox()
        {
            return Task.Run(() => CloseMailboxAsync()).Result;
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public bool CopyMessage(string sourceMailboxName, int index, string destMailboxName)
        {
            return Task.Run(() => CopyMessageAsync(sourceMailboxName, index, destMailboxName)).Result;
        }

        /// <summary>
        /// Copy a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="destMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to copy.</param>
        /// <param name="sourceMailboxName">Name of the mailbox to copy to.</param>
        public bool CopyMessageUid(string sourceMailboxName, int uid, string destMailboxName)
        {
            return Task.Run(() => CopyMessageUidAsync(sourceMailboxName, uid, destMailboxName)).Result;
        }

        /// <summary>
        /// Create a mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The given name for the new mailbox.</param>
        public bool CreateMailbox(string mailboxName)
        {
            return Task.Run(() => CreateMailboxAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Delete a mailbox from the server.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to delete.</param>
        public bool DeleteMailbox(string mailboxName)
        {
            return Task.Run(() => DeleteMailboxAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="index">Index of the message to delete.</param>
        public bool DeleteMessage(string mailboxName, int index)
        {
            return Task.Run(() => DeleteMessageAsync(mailboxName, index)).Result;
        }

        /// <summary>
        /// Delete a series of messages from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the messages to delete.</param>
        /// <param name="indices">Array of message indices to delete.</param>
        public bool DeleteMessages(string mailboxName, int[] indices)
        {
            return Task.Run(() => DeleteMessagesAsync(mailboxName, indices)).Result;
        }

        /// <summary>
        /// Delete a message from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to delete.</param>
        /// <param name="uid">UID of the message to delete.</param>
        public bool DeleteMessageUid(string mailboxName, int uid)
        {
            return Task.Run(() => DeleteMessageUidAsync(mailboxName, uid)).Result;
        }

        /// <summary>
        /// Delete a series of messages from the server.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the messages to delete.</param>
        /// <param name="indices">Array of message indices to delete.</param>
        /// <param name="uids">Array of message UIDs to delete.</param>
        public bool DeleteMessagesUid(string mailboxName, int[] uids)
        {
            return Task.Run(() => DeleteMessagesUidAsync(mailboxName, uids)).Result;
        }

        /// <summary>
        /// Notify the IMAP server that the client supports the specified capability.
        /// </summary>
        /// <param name="capabilityName">Name of the capability to enable.</param>
        public bool EnableCapability(string capabilityName)
        {
            return Task.Run(() => EnableCapabilityAsync(capabilityName)).Result;
        }

        /// <summary>
        /// Examine a mailbox, returning its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        public Mailbox ExamineMailbox(string mailboxName)
        {
            return Task.Run(() => ExamineMailboxAsync(mailboxName, "")).Result;
        }

        /// <summary>
        /// Examine a mailbox, returning its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        /// <param name="qResyncParameters">Quick Resynchronization parameters, such as UIDValidity, the last known modification sequence, known UIDs, and/or known sequence ranges.</param>
        public Mailbox ExamineMailbox(string mailboxName, string qResyncParameters)
        {
            return Task.Run(() => ExamineMailboxAsync(mailboxName, qResyncParameters)).Result;
        }

        /// <summary>
        /// Remove all messages from the current mailbox that have the "\Deleted" flag.
        /// </summary>
        public bool ExpungeMailbox()
        {
            return Task.Run(() => ExpungeMailboxAsync()).Result;
        }

        /// <summary>
        /// Load an instance of a message based on its index.
        /// </summary>
        /// <param name="index">The index of the message to load.</param>
        public ReadOnlyMailMessage GetMessage(int index)
        {
            return Task.Run(() => GetMessageAsync(index)).Result;
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        public ReadOnlyMailMessage GetMessage(string mailboxName, int index)
        {
            return Task.Run(() => GetMessageAsync(mailboxName, index)).Result;
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its index, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="index">The index of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public ReadOnlyMailMessage GetMessage(string mailboxName, int index, bool headersOnly)
        {
            return Task.Run(() => GetMessageAsync(mailboxName, index, headersOnly)).Result;
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
            return Task.Run(() => GetMessageAsync(mailboxName, index, headersOnly, setSeenFlag)).Result;
        }

        /// <summary>
        /// Return the number of messages in the current mailbox.
        /// </summary>
        public int GetMessageCount()
        {
            return Task.Run(() => GetMessageCountAsync()).Result;
        }

        /// <summary>
        /// Return the number of messages in a specific mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to examine.</param>
        public int GetMessageCount(string mailboxName)
        {
            return Task.Run(() => GetMessageCountAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Load an instance of a message based on its UID.
        /// </summary>
        /// <param name="uid">The UID of the message to load.</param>
        public ReadOnlyMailMessage GetMessageUid(int uid)
        {
            return Task.Run(() => GetMessageUidAsync(uid)).Result;
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        public ReadOnlyMailMessage GetMessageUid(string mailboxName, int uid)
        {
            return Task.Run(() => GetMessageUidAsync(mailboxName, uid)).Result;
        }

        /// <summary>
        /// Load an instance of a message in a specified mailbox based on its UID, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The mailbox to load from.</param>
        /// <param name="uid">The UID of the message to load.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>        
        public ReadOnlyMailMessage GetMessageUid(string mailboxName, int uid, bool headersOnly)
        {
            return Task.Run(() => GetMessageUidAsync(mailboxName, uid, headersOnly)).Result;
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
            return Task.Run(() => GetMessageUidAsync(mailboxName, uid, headersOnly, setSeenFlag)).Result;
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the current mailbox.
        /// </summary>
        public List<ReadOnlyMailMessage> GetMessages()
        {
            return Task.Run(() => GetMessagesAsync()).Result;
        }

        /// <summary>
        /// Retrieve up to 25 of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName)
        {
            return Task.Run(() => GetMessagesAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count)
        {
            return Task.Run(() => GetMessagesAsync(count)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName, int count)
        {
            return Task.Run(() => GetMessagesAsync(mailboxName, count)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the current mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public List<ReadOnlyMailMessage> GetMessages(int count, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(count, headersOnly)).Result;
        }

        /// <summary>
        /// Retrieve up to count of the most recent messages from the specified mailbox, optionally returning only headers.
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox to fetch from.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="headersOnly">Return only the message's headers when true; otherwise, return the message and body.</param>
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName, int count, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(mailboxName, count, headersOnly)).Result;
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
        public List<ReadOnlyMailMessage> GetMessages(string mailboxName, int count, int startIndex, bool reverseOrder, bool headersOnly, bool setSeenFlag)
        {
            return Task.Run(() => GetMessagesAsync(mailboxName, count, startIndex, reverseOrder, headersOnly, setSeenFlag)).Result;
        }

        /// <summary>
        /// Get the current quota and usage for the specified mailbox.
        /// </summary>
        /// <param name="mailboxName">The mailbox to work with.</param>
        public QuotaUsage GetQuota(string mailboxName)
        {
            return Task.Run(() => GetQuotaAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Get the current quota and usage at the root level.
        /// </summary>
        /// <param name="mailboxName">The mailbox to work with.</param>
        public QuotaUsage GetQuotaRoot(string mailboxName)
        {
            return Task.Run(() => GetQuotaRootAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Send a list of identifying characteristics to the server.
        /// </summary>
        /// <param name="identification">Values to be sent.</param>
        public void Identify(ImapIdentification identification)
        {
            Task.Run(() => IdentifyAsync(identification));
        }

        /// <summary>
        /// Notify the server that the session is going IDLE, while continuing to receive notifications from the server.
        /// </summary>
        public bool IdleStart()
        {
            return Task.Run(() => IdleStartAsync()).Result;
        }

        /// <summary>
        /// Notify the server that the session is no longer IDLE.
        /// </summary>
        public bool IdleStop()
        {
            return Task.Run(() => IdleStopAsync()).Result;
        }

        /// <summary>
        /// Return an array of all root mailboxes, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListMailboxes(bool includeFullHierarchy)
        {
            return Task.Run(() => ListMailboxesAsync("", includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of all mailboxes below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListMailboxes(string mailboxName, bool includeFullHierarchy)
        {
            return Task.Run(() => ListMailboxesAsync(mailboxName, includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of all root mailbox names, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListMailboxNames(bool includeFullHierarchy)
        {
            return Task.Run(() => ListMailboxNamesAsync("", includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of all mailbox names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListMailboxNames(string mailboxName, bool includeFullHierarchy)
        {
            return Task.Run(() => ListMailboxNamesAsync(mailboxName, includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of subscriptions, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListSubscriptions(bool includeFullHierarchy)
        {
            return Task.Run(() => ListSubscriptionsAsync("", includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of subscriptions below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public Mailbox[] ListSubscriptions(string mailboxName, bool includeFullHierarchy)
        {
            return Task.Run(() => ListSubscriptionsAsync(mailboxName, includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of subscription names, and optionally, all children.
        /// </summary>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListSubscriptionNames(bool includeFullHierarchy)
        {
            return Task.Run(() => ListSubscriptionNamesAsync("", includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Return an array of subscription names below a specified mailbox, and optionally, all children.
        /// </summary>
        /// <param name="mailboxName">The mailbox to search below.</param>
        /// <param name="includeFullHierarchy">Whether to use wildcards and return all descendants</param>
        public string[] ListSubscriptionNames(string mailboxName, bool includeFullHierarchy)
        {
            return Task.Run(() => ListSubscriptionNamesAsync(mailboxName, includeFullHierarchy)).Result;
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its index.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="index">Index of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public bool MoveMessage(string sourceMailboxName, int index, string destMailboxName)
        {
            return Task.Run(() => MoveMessageHelperAsync(sourceMailboxName, index, destMailboxName, false)).Result;
        }

        /// <summary>
        /// Move a message to the destination mailbox, referenced by its UID.
        /// </summary>
        /// <param name="sourceMailboxName">Name of the mailbox containing the original message.</param>
        /// <param name="uid">UID of the message to move.</param>
        /// <param name="destMailboxName">Name of the mailbox to move to.</param>
        public bool MoveMessageUid(string sourceMailboxName, int uid, string destMailboxName)
        {
            return Task.Run(() => MoveMessageHelperAsync(sourceMailboxName, uid, destMailboxName, true)).Result;
        }

        /// <summary>
        /// Prolong the current session and poll for new messages, but issue no command.
        /// </summary>
        public bool NoOp()
        {
            return Task.Run(() => NoOpAsync()).Result;
        }

        /// <summary>
        /// Read the last response from the IMAP server tied to a specific command tag.
        /// </summary>
        /// <param name="commandTag">Command tag identifying the command and its response</param>
        /// <param name="previousCommand">The previous command issued to the server.</param>
        public string ReadData(string commandTag, string previousCommand)
        {
            return Task.Run(() => ReadDataAsync(commandTag, previousCommand)).Result;
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public bool RemoveFlagsFromMessage(string mailboxName, int index, string[] flags)
        {
            return Task.Run(() => RemoveFlagsFromMessageHelperAsync(mailboxName, index, flags, false)).Result;
        }

        /// <summary>
        /// Remove one or more flags from a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags to remove.</param>
        public bool RemoveFlagsFromMessageUid(string mailboxName, int uid, string[] flags)
        {
            return Task.Run(() => RemoveFlagsFromMessageHelperAsync(mailboxName, uid, flags, true)).Result;
        }

        /// <summary>
        /// Rename a mailbox.
        /// </summary>
        /// <param name="currentMailboxName">The name of the current mailbox to be renamed.</param>
        /// <param name="newMailboxName">The new name of the mailbox.</param>
        public bool RenameMailbox(string currentMailboxName, string newMailboxName)
        {
            return Task.Run(() => RenameMailboxAsync(currentMailboxName, newMailboxName)).Result;
        }

        /// <summary>
        /// Perform a search in the current mailbox and return all matching messages.
        /// </summary>
        /// <param name="searchQuery">Well-formatted IMAP search criteria.</param>
        public List<ReadOnlyMailMessage> Search(string searchQuery)
        {
            return Task.Run(() => SearchAsync(searchQuery)).Result;
        }

        /// <summary>
        /// Select a mailbox for subsequent operations and return its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        public Mailbox SelectMailbox(string mailboxName)
        {
            return Task.Run(() => SelectMailboxAsync(mailboxName, "")).Result;
        }

        /// <summary>
        /// Select a mailbox for subsequent operations and return its properties.
        /// </summary>
        /// <param name="mailboxName">Mailbox to work with.</param>
        /// <param name="qResyncParameters">Quick Resynchronization parameters, such as UIDValidity, the last known modification sequence, known UIDs, and/or known sequence ranges.</param>
        public Mailbox SelectMailbox(string mailboxName, string qResyncParameters)
        {
            return Task.Run(() => SelectMailboxAsync(mailboxName, qResyncParameters)).Result;
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
            return Task.Run(() => SetQuotaAsync(mailboxName, quotaSize)).Result;
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its index.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="index">Index of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public bool StoreFlags(string mailboxName, int index, string[] flags)
        {
            return Task.Run(() => StoreFlagsHelperAsync(mailboxName, index, flags, false)).Result;
        }

        /// <summary>
        /// Update the flags associated with a message, referenced by its UID.
        /// </summary>
        /// <param name="mailboxName">Mailbox containing the message to update.</param>
        /// <param name="uid">UID of the message to update.</param>
        /// <param name="flags">List of flags.</param>
        public bool StoreFlagsUid(string mailboxName, int uid, string[] flags)
        {
            return Task.Run(() => StoreFlagsHelperAsync(mailboxName, uid, flags, true)).Result;
        }

        /// <summary>
        /// Subscribe to a mailbox to monitor changes.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public bool SubscribeMailbox(string mailboxName)
        {
            return Task.Run(() => SubscribeMailboxAsync(mailboxName)).Result;
        }

        /// <summary>
        /// Stop subscribing to a mailbox.
        /// </summary>
        /// <param name="mailboxName">Name of mailbox to subscribe to.</param>
        public bool UnsubscribeMailbox(string mailboxName)
        {
            return Task.Run(() => UnsubscribeMailboxAsync(mailboxName)).Result;
        }
        #endregion Public Methods
    }
}
