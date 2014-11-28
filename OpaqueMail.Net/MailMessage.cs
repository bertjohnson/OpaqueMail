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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Represents an email message that was received using the ImapClient or Pop3Client classes.
    /// Includes OpaqueMail extensions to facilitate handling of secure S/MIME messages.
    /// </summary>
    public class MailMessage
    {
        #region Public Members
        /// <summary>Collection of all recipients of this message, based on To, CC, and Bcc paramaters.</summary>
        public List<string> AllRecipients = new List<string>();
        public AlternateViewCollection AlternateViews = new AlternateViewCollection();
        /// <summary>Gets the attachment collection used to store data attached to this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.AttachmentCollection" />.</returns>
        public AttachmentCollection Attachments = new AttachmentCollection();
        /// <summary>Gets the address collection that contains the blind carbon copy (BCC) recipients for this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.MailAddressCollection" /> object.</returns>
        public MailAddressCollection Bcc = new MailAddressCollection();
        /// <summary>Gets or sets the message body.</summary>
        public string Body = "";
        /// <summary>Content type of the message's body.</summary>
        public string BodyContentType = "";
        /// <summary>Whether the body's contents were decoded from their transfer encoding.</summary>
        public bool BodyDecoded = false;
        /// <summary>Gets or sets the encoding used to encode the message body.</summary>
        public Encoding BodyEncoding;
        public TransferEncoding BodyTransferEncoding;
        /// <summary>Gets the address collection that contains the carbon copy (CC) recipients for this email message.</summary>
        public MailAddressCollection CC = new MailAddressCollection();
        /// <summary>Character set encoding of the message.</summary>
        public string CharSet = "";
        /// <summary>Language of message content.</summary>
        public string ContentLanguage = "";
        /// <summary>Content transfer encoding of the message.</summary>
        public string ContentTransferEncoding = "";
        /// <summary>Primary content type of the message.</summary>
        public string ContentType = "";
        /// <summary>Date sent.</summary>
        public DateTime Date;
        /// <summary>Delivered-To header.</summary>
        public string DeliveredTo = "";
        /// <summary>Gets or sets the delivery notifications for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.DeliveryNotificationOptions" /> value that contains the delivery notifications for this message.</returns>
        public DeliveryNotificationOptions DeliveryNotificationOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.deliveryStatusNotification;
            }
            set
            {
                if ((DeliveryNotificationOptions.OnSuccess | DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay) < value && value != DeliveryNotificationOptions.Never)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.deliveryStatusNotification = value;
            }
        }
        /// <summary>
        /// Extended email headers.
        /// Only populated when the ReadOnlyMailMessage is instantiated with a parseExtendedHeaders setting of true.
        /// </summary>
        public ExtendedProperties ExtendedProperties;
        /// <summary>Flags representing the processed state of the message.</summary>
        public Flags Flags;
        /// <summary>Gets or sets the from address for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailAddress" /> that contains the from address information.</returns>
        public MailAddress From;
        /// <summary>Whether the message contains a body.</summary>
        public bool HasBody;
        /// <summary>Whether the message contains headers.</summary>
        public bool HasHeaders;
        /// <summary>Gets the email headers that are transmitted with this email message.</summary>
        /// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> that contains the email headers.</returns>
        public NameValueCollection Headers = new NameValueCollection();
        /// <summary>Gets or sets the encoding used for the user-defined custom headers for this email message.</summary>
        /// <returns>The encoding used for user-defined custom headers for this email message.</returns>
        public Encoding HeadersEncoding;
        /// <summary>Gets or sets a value indicating whether the mail message body is in Html.</summary>
        public bool IsBodyHtml;
        /// <summary>Mailbox the message was read from.</summary>
        public string Mailbox;
        /// <summary>Gets or sets the priority of this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailPriority" /> that contains the priority of this message.</returns>
        public MailPriority Priority;
        /// <summary>UID as specified by the IMAP server.</summary>
        public int ImapUid;
        /// <summary>Importance header.</summary>
        public string Importance = "";
        /// <summary>Index as specified by the IMAP or POP3 server.</summary>
        public int Index;
        /// <summary>In-Reply-To header.</summary>
        public string InReplyTo = "";
        /// <summary>Message ID header.</summary>
        public string MessageId;
        /// <summary>Text delimiting MIME message parts.</summary>
        public string MimeBoundaryName = "OpaqueMail-boundary";
        /// <summary>List of MIME parts from a multipart/* MIME encoded message.</summary>
        public List<MimePart> MimeParts;
        /// <summary>Partial message unique ID.</summary>
        public string PartialMessageId;
        /// <summary>Partial message unique number.</summary>
        public int PartialMessageNumber;
        /// <summary>UIDL as specified by the POP3 server.</summary>
        public string Pop3Uidl;
        /// <summary>Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</summary>
        public ReadOnlyMailMessageProcessingFlags ProcessingFlags = ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody;
        /// <summary>String representation of the raw body received.</summary>
        public string RawBody;
        /// <summary>Raw flags returned with the message.</summary>
        public HashSet<string> RawFlags = new HashSet<string>();
        /// <summary>String representation of the raw headers received.</summary>
        public string RawHeaders;
        /// <summary>String representation of the entire raw message received.</summary>
        public string RawMessage;
        /// <summary>Array of values of Received and X-Received headers.</summary>
        public string[] ReceivedChain;
        /// <summary>Message IDs of previously referenced messages.</summary>
        public string[] References;
        /// <summary>Gets or sets the ReplyTo address for the mail message.</summary>
        /// <returns>A MailAddress that indicates the value of the <see cref="P:System.Net.Mail.MailMessage.ReplyTo" /> field.</returns>
        [Obsolete("ReplyTo is obsoleted for this type.  Please use ReplyToList instead which can accept multiple addresses. http://go.microsoft.com/fwlink/?linkid=14202")]
        public MailAddress ReplyTo;
        /// <summary>Gets or sets the list of addresses to reply to for the mail message.</summary>
        /// <returns>The list of the addresses to reply to for the mail message.</returns>
        public MailAddressCollection ReplyToList = new MailAddressCollection();
        /// <summary>Return-Path header.</summary>
        public string ReturnPath = "";
        /// <summary>Gets or sets the sender's address for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailAddress" /> that contains the sender's address information.</returns>
        public MailAddress Sender;
        /// <summary>Certificate chain used to sign the message.</summary>
        public X509Certificate2Collection SmimeSigningCertificateChain = new X509Certificate2Collection();
        /// <summary>Gets or sets the subject line for this email message.</summary>
        /// <returns>A <see cref="T:System.String" /> that contains the subject content.</returns>
        public string Subject;
        /// <summary>Gets or sets the encoding used for the subject content for this email message.</summary>
        /// <returns>An <see cref="T:System.Text.Encoding" /> that was used to encode the <see cref="P:System.Net.Mail.MailMessage.Subject" /> property.</returns>
        public Encoding SubjectEncoding;
        /// <summary>X-Subject-Encryption header, as optionally used by OpaqueMail.</summary>
        public bool SubjectEncryption;
        /// <summary>
        /// Size of the entire message.
        /// When sending email, this is a rough estimate only.
        /// </summary>
        public long Size
        {
            get
            {
                // If a size has been set when loading the message, return that size.
                if (loadedSize > -1)
                    return loadedSize;
                else
                {
                    // If this message wasn't loaded via ReadOnlyMailMessage, calculate a rough estimate of its size.
                    long size = Body.Length;

                    foreach (Attachment attachment in Attachments)
                    {
                        using (Stream dataStream = attachment.ContentStream)
                        {
                            size += dataStream.Length;
                        }
                    }

                    return Body.Length;
                }
            }
            set
            {
                loadedSize = value;
            }
        }
        /// <summary>Encrypt the email's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.</summary>
        public bool SmimeEncryptedEnvelope = false;
        /// <summary>Type of subject identifier to use.</summary>
        /// <remarks>The default of "IssuerAndSerialNumber" is recommended for most use cases.</remarks>
        public SmimeEncryptionOptionFlags SmimeEncryptionOptionFlags = SmimeEncryptionOptionFlags.RequireCertificateVerification;
        /// <summary>Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</summary>
        public SmimeSettingsMode SmimeSettingsMode = SmimeSettingsMode.RequireExactSettings;
        /// <summary>Sign the email.  When true, signing is the first S/MIME operation.</summary>
        public bool SmimeSigned = false;
        /// <summary>
        /// Certificate used when signing messages.
        /// Requires private key.
        /// </summary>
        public X509Certificate2 SmimeSigningCertificate;
        /// <summary>Determine how the S/MIME message will be signed.</summary>
        public SmimeSigningOptionFlags SmimeSigningOptionFlags = SmimeSigningOptionFlags.SignTime;
        /// <summary>Triple-wrap the email by signing, then encrypting the envelope, then signing the encrypted envelope.</summary>
        public bool SmimeTripleWrapped = false;
        /// <summary>Determine how the S/MIME envelope will be encrypted.</summary>
        public SubjectIdentifierType SubjectIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
        /// <summary>Gets the address collection that contains the recipients of this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.MailAddressCollection" /> object.</returns>
        public MailAddressCollection To = new MailAddressCollection();
        #endregion Public Members

        #region Protected Members
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        protected string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        protected string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        #endregion Protected Members

        #region Private Members
        /// <summary>Size of the loaded message, as calculated in ReadOnlyMailMessage's constructor.</summary>
        private long loadedSize = -1;
        private DeliveryNotificationOptions deliveryStatusNotification;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        public MailMessage()
        {
        }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class by using the specified OpaqueMail.MailAddress class objects.
        /// </summary>
        /// <param name="from">A System.Net.Mail.MailAddress that contains the address of the sender of the email message.</param>
        /// <param name="to">A System.Net.Mail.MailAddress that contains the address of the recipient of the email message.</param>
        public MailMessage(MailAddress from, MailAddress to)
        {
            From = from;
            To.Add(to);
        }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        /// <param name="from">A System.String that contains the address of the sender of the email message.</param>
        /// <param name="to">A System.String that contains the address of the recipient of the email message.</param>
        /// <param name="subject">A System.String that contains the subject text.</param>
        /// <param name="body">A System.String that contains the message body.</param>
        public MailMessage(string from, string to, string subject, string body)
        {
            From = new MailAddress(from);
            To.Add(new MailAddress(to));
            Subject = subject;
            Body = body;
        }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        public MailMessage(string messageText) : this(messageText, ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody | ReadOnlyMailMessageProcessingFlags.IncludeMIMEParts, false) { }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        public MailMessage(string messageText, ReadOnlyMailMessageProcessingFlags processingFlags) : this(messageText, processingFlags, false) { }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        /// <param name="parseExtendedHeaders">Whether to populate the ExtendedHeaders object.</param>
        public MailMessage(string messageText, ReadOnlyMailMessageProcessingFlags processingFlags, bool parseExtendedHeaders)
        {
            // Default to no MIME boundary.
            MimeBoundaryName = null;

            if (((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders) > 0)
                && (processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawBody) > 0)
                RawMessage = messageText;

            // Prepare an object to track advanced headers.
            if (parseExtendedHeaders)
                ExtendedProperties = new ExtendedProperties();

            // Remember which specialized attachments to include.
            ProcessingFlags = processingFlags;

            // Fix messages whose carriage returns have been stripped.
            if (messageText.IndexOf("\r") < 0)
                messageText = messageText.Replace("\n", "\r\n");

            // Separate the headers for processing.
            string headers;
            int cutoff = messageText.IndexOf("\r\n\r\n");
            if (cutoff > -1)
                headers = messageText.Substring(0, cutoff);
            else
                headers = messageText;

            // Calculate the size of the message.
            Size = messageText.Length;

            // Temporary header variables to be processed by Functions.FromMailAddressString() later.
            string fromText = "";
            string toText = "";
            string ccText = "";
            string bccText = "";
            string replyToText = "";

            // Temporary header variables to be processed later.
            List<string> receivedChain = new List<string>();
            string receivedText = "";

            // Unfold any unneeded whitespace, then loop through each line of the headers.
            string[] headersList = Functions.UnfoldWhitespace(headers).Replace("\r", "").Split('\n');
            foreach (string header in headersList)
            {
                // Split header {name:value} pairs by the first colon found.
                int colonPos = header.IndexOf(":");
                if (colonPos > -1 && colonPos < header.Length - 1)
                {
                    string[] headerParts = new string[] { header.Substring(0, colonPos), header.Substring(colonPos + 1).TrimStart(new char[] { ' ' }) };
                    string headerType = headerParts[0].ToLower();
                    string headerValue = headerParts[1];

                    // Set header variables for common headers.
                    if (!string.IsNullOrEmpty(headerType) && !string.IsNullOrEmpty(headerValue))
                        Headers[headerType] = headerValue;

                    switch (headerType)
                    {
                        case "cc":
                            if (ccText.Length > 0)
                                ccText += ", ";
                            ccText = headerValue;
                            break;
                        case "content-transfer-encoding":
                            ContentTransferEncoding = headerValue;
                            switch (headerValue.ToLower())
                            {
                                case "base64":
                                    BodyTransferEncoding = TransferEncoding.Base64;
                                    break;
                                case "quoted-printable":
                                    BodyTransferEncoding = TransferEncoding.QuotedPrintable;
                                    break;
                                case "7bit":
                                    BodyTransferEncoding = TransferEncoding.SevenBit;
                                    break;
                                case "8bit":
                                    BodyTransferEncoding = TransferEncoding.EightBit;
                                    break;
                                default:
                                    BodyTransferEncoding = TransferEncoding.Unknown;
                                    break;
                            }
                            break;
                        case "content-language":
                            ContentLanguage = headerValue;
                            break;
                        case "content-type":
                            // If multiple content-types are passed, only process the first.
                            if (string.IsNullOrEmpty(ContentType))
                            {
                                ContentType = headerValue.Trim();
                                CharSet = Functions.ExtractMimeParameter(ContentType, "charset");
                                MimeBoundaryName = Functions.ExtractMimeParameter(ContentType, "boundary");

                                BodyContentType = ContentType.ToLower();
                                if (BodyContentType.IndexOf(";") > -1)
                                    BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                                IsBodyHtml = BodyContentType.StartsWith("text/html");
                            }
                            break;
                        case "date":
                            string dateString = headerValue;

                            // Ignore extraneous datetime information.
                            int dateStringParenthesis = dateString.IndexOf("(");
                            if (dateStringParenthesis > -1)
                                dateString = dateString.Substring(0, dateStringParenthesis - 1);

                            // Remove timezone suffix.
                            if (dateString.Substring(dateString.Length - 4, 1) == " ")
                                dateString = dateString.Substring(0, dateString.Length - 4);

                            DateTime.TryParse(dateString, out Date);
                            break;
                        case "delivered-to":
                            DeliveredTo = headerValue;
                            break;
                        case "from":
                            fromText = headerValue;
                            break;
                        case "importance":
                            Importance = headerValue;
                            break;
                        case "in-reply-to":
                            // Ignore opening and closing <> characters.
                            InReplyTo = headerValue;
                            if (InReplyTo.StartsWith("<"))
                                InReplyTo = InReplyTo.Substring(1);
                            if (InReplyTo.EndsWith(">"))
                                InReplyTo = InReplyTo.Substring(0, InReplyTo.Length - 1);
                            break;
                        case "message-id":
                            // Ignore opening and closing <> characters.
                            MessageId = headerValue;
                            if (MessageId.StartsWith("<"))
                                MessageId = MessageId.Substring(1);
                            if (MessageId.EndsWith(">"))
                                MessageId = MessageId.Substring(0, MessageId.Length - 1);
                            break;
                        case "received":
                        case "x-received":
                            if (!string.IsNullOrEmpty(receivedText))
                                receivedChain.Add(receivedText);

                            receivedText = headerValue;
                            break;
                        case "references":
                            // Loop through references.
                            List<string> references = new List<string>();
                            int referencesPos = 0;
                            while (referencesPos > -1)
                            {
                                referencesPos = headerValue.IndexOf("<", referencesPos);
                                if (referencesPos > -1)
                                {
                                    int referencesPos2 = headerValue.IndexOf(">", referencesPos);
                                    if (referencesPos2 > -1)
                                    {
                                        references.Add(headerValue.Substring(referencesPos + 1, referencesPos2 - referencesPos - 1));
                                        referencesPos = referencesPos2 + 1;
                                    }
                                    else
                                        referencesPos = -1;
                                }
                            }
                            // If any references were found, apply to the message's array.
                            if (references.Count > 0)
                                References = references.ToArray();

                            break;
                        case "replyto":
                        case "reply-to":
                            replyToText = headerValue;
                            break;
                        case "return-path":
                            // Ignore opening and closing <> characters.
                            ReturnPath = headerValue;
                            if (ReturnPath.StartsWith("<"))
                                ReturnPath = ReturnPath.Substring(1);
                            if (ReturnPath.EndsWith(">"))
                                ReturnPath = ReturnPath.Substring(0, ReturnPath.Length - 1);
                            break;
                        case "sender":
                        case "x-sender":
                            if (headerValue.Length > 0)
                            {
                                MailAddressCollection senderCollection = MailAddressCollection.Parse(headerValue);
                                if (senderCollection.Count > 0)
                                    this.Sender = senderCollection[0];
                            }
                            break;
                        case "subject":
                            // Decode international strings and remove escaped linebreaks.
                            Subject = Functions.DecodeMailHeader(headerValue).Replace("\r", "").Replace("\n", "");
                            break;
                        case "to":
                            if (toText.Length > 0)
                                toText += ", ";
                            toText += headerValue;
                            break;
                        case "x-priority":
                            switch (headerValue.ToUpper())
                            {
                                case "LOW":
                                    Priority = MailPriority.Low;
                                    break;
                                case "NORMAL":
                                    Priority = MailPriority.Normal;
                                    break;
                                case "HIGH":
                                    Priority = MailPriority.High;
                                    break;
                            }
                            break;
                        case "x-subject-encryption":
                            bool.TryParse(headerValue, out SubjectEncryption);
                            break;
                        default:
                            break;
                    }

                    // Set header variables for advanced headers.
                    if (parseExtendedHeaders)
                    {
                        switch (headerType)
                        {
                            case "acceptlanguage":
                            case "accept-language":
                                ExtendedProperties.AcceptLanguage = headerValue;
                                break;
                            case "authentication-results":
                                ExtendedProperties.AuthenticationResults = headerValue;
                                break;
                            case "bounces-to":
                            case "bounces_to":
                                ExtendedProperties.BouncesTo = headerValue;
                                break;
                            case "content-description":
                                ExtendedProperties.ContentDescription = headerValue;
                                break;
                            case "dispositionnotificationto":
                            case "disposition-notification-to":
                                ExtendedProperties.DispositionNotificationTo = headerValue;
                                break;
                            case "dkim-signature":
                            case "domainkey-signature":
                            case "x-google-dkim-signature":
                                ExtendedProperties.DomainKeySignature = headerValue;
                                break;
                            case "domainkey-status":
                                ExtendedProperties.DomainKeyStatus = headerValue;
                                break;
                            case "errors-to":
                                ExtendedProperties.ErrorsTo = headerValue;
                                break;
                            case "list-unsubscribe":
                            case "x-list-unsubscribe":
                                ExtendedProperties.ListUnsubscribe = headerValue;
                                break;
                            case "mailer":
                            case "x-mailer":
                                ExtendedProperties.Mailer = headerValue;
                                break;
                            case "organization":
                            case "x-originator-org":
                            case "x-originatororg":
                            case "x-organization":
                                ExtendedProperties.OriginatorOrg = headerValue;
                                break;
                            case "original-messageid":
                            case "x-original-messageid":
                                ExtendedProperties.OriginalMessageId = headerValue;
                                break;
                            case "originating-email":
                            case "x-originating-email":
                                ExtendedProperties.OriginatingEmail = headerValue;
                                break;
                            case "precedence":
                                ExtendedProperties.Precedence = headerValue;
                                break;
                            case "received-spf":
                                ExtendedProperties.ReceivedSPF = headerValue;
                                break;
                            case "references":
                                ExtendedProperties.References = headerValue;
                                break;
                            case "resent-date":
                                string dateString = headerValue;

                                // Ignore extraneous datetime information.
                                int dateStringParenthesis = dateString.IndexOf("(");
                                if (dateStringParenthesis > -1)
                                    dateString = dateString.Substring(0, dateStringParenthesis - 1);

                                // Remove timezone suffix.
                                if (dateString.Substring(dateString.Length - 4) == " ")
                                    dateString = dateString.Substring(0, dateString.Length - 4);

                                DateTime.TryParse(dateString, out ExtendedProperties.ResentDate);
                                break;
                            case "resent-from":
                                ExtendedProperties.ResentFrom = headerValue;
                                break;
                            case "resent-message-id":
                                ExtendedProperties.ResentMessageID = headerValue;
                                break;
                            case "thread-index":
                                ExtendedProperties.ThreadIndex = headerValue;
                                break;
                            case "thread-topic":
                                ExtendedProperties.ThreadTopic = headerValue;
                                break;
                            case "user-agent":
                            case "useragent":
                                ExtendedProperties.UserAgent = headerValue;
                                break;
                            case "x-auto-response-suppress":
                                ExtendedProperties.AutoResponseSuppress = headerValue;
                                break;
                            case "x-campaign":
                            case "x-campaign-id":
                            case "x-campaignid":
                            case "x-mllistcampaign":
                            case "x-rpcampaign":
                                ExtendedProperties.CampaignID = headerValue;
                                break;
                            case "x-delivery-context":
                                ExtendedProperties.DeliveryContext = headerValue;
                                break;
                            case "x-maillist-id":
                                ExtendedProperties.MailListId = headerValue;
                                break;
                            case "x-msmail-priority":
                                ExtendedProperties.MSMailPriority = headerValue;
                                break;
                            case "x-originalarrivaltime":
                            case "x-original-arrival-time":
                                dateString = headerValue;

                                // Ignore extraneous datetime information.
                                dateStringParenthesis = dateString.IndexOf("(");
                                if (dateStringParenthesis > -1)
                                    dateString = dateString.Substring(0, dateStringParenthesis - 1);

                                // Remove timezone suffix.
                                if (dateString.Substring(dateString.Length - 4) == " ")
                                    dateString = dateString.Substring(0, dateString.Length - 4);

                                DateTime.TryParse(dateString, out ExtendedProperties.OriginalArrivalTime);
                                break;
                            case "x-originating-ip":
                                ExtendedProperties.OriginatingIP = headerValue;
                                break;
                            case "x-rcpt-to":
                                if (headerValue.Length > 1)
                                    ExtendedProperties.RcptTo = headerValue.Substring(1, headerValue.Length - 2);
                                break;
                            case "x-csa-complaints":
                            case "x-complaints-to":
                            case "x-reportabuse":
                            case "x-report-abuse":
                            case "x-mail_abuse_inquiries":
                                ExtendedProperties.ReportAbuse = headerValue;
                                break;
                            case "x-spam-score":
                                ExtendedProperties.SpamScore = headerValue;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Track all Received and X-Received headers.
            if (!string.IsNullOrEmpty(receivedText))
                receivedChain.Add(receivedText);
            ReceivedChain = receivedChain.ToArray();

            // Process the body if it's passed in.
            string body = "";

            // If common headers are unsent, we're parsing the body only.
            if (string.IsNullOrEmpty(fromText) && string.IsNullOrEmpty(Subject))
                body = messageText;
            else
            {
                HasHeaders = true;

                // Set the raw headers property if requested.
                if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders) > 0)
                    RawHeaders = headers;

                if (cutoff > -1)
                {
                    body = messageText.Substring(cutoff + 4);

                    if (body.Length > 0)
                    {
                        // Ignore closing message indicators.
                        int closePos = body.Length;
                        if (body.EndsWith(")"))
                            closePos--;

                        // Ignore trailing linebreaks.
                        while (closePos > 1 && body.Substring(closePos - 2, 2) == "\r\n")
                            closePos -= 2;

                        if (closePos != body.Length)
                            body = body.Substring(0, closePos);
                    }
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                HasBody = true;

                // Only process MIME parts if a boundary name is passed in.
                if (!string.IsNullOrEmpty(MimeBoundaryName))
                {
                    // Parse body into MIME parts and process them.
                    MimeParts = MimePart.ExtractMIMEParts(ContentType, CharSet, ContentTransferEncoding, body, ProcessingFlags, 0);
                    if (MimeParts.Count > 0)
                        Task.Run(() => ProcessMimeParts()).Wait();
                    else
                    {
                        // We can no longer trust the external encoding, so infer if this body is Base-64 or Quoted-Printable encoded.
                        if (Functions.AppearsBase64(body))
                            ContentTransferEncoding = "base64";
                        else if (Functions.AppearsQuotedPrintable(body))
                            ContentTransferEncoding = "quoted-printable";

                        string encodedBody = body;
                        Body = Functions.Decode(body, ContentTransferEncoding, CharSet, BodyEncoding);
                        if (Body != encodedBody)
                            BodyDecoded = true;

                        BodyContentType = ContentType;
                        if (BodyContentType.IndexOf(";") > -1)
                            BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                        // Infer if text/html when no content type is specified.
                        if (BodyContentType != "text/html" && Body.ToUpper().Contains("<BODY"))
                            BodyContentType = "text/html";
                    }

                    if (!((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeMIMEParts) > 0))
                        MimeParts = null;
                }
                else
                {
                    // If no encoding is specified (such as when requesting partial headers only), infer if this body is Base-64 or Quoted-Printable encoded.
                    if (string.IsNullOrEmpty(ContentTransferEncoding))
                    {
                        if (Functions.AppearsBase64(body))
                            ContentTransferEncoding = "base64";
                        else if (Functions.AppearsQuotedPrintable(body))
                            ContentTransferEncoding = "quoted-printable";
                    }

                    string encodedBody = body;
                    Body = Functions.Decode(body, ContentTransferEncoding, CharSet, BodyEncoding);
                    if (Body != encodedBody)
                        BodyDecoded = true;

                    BodyContentType = ContentType;
                    if (BodyContentType.IndexOf(";") > -1)
                        BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                    // Infer if text/html when no content type is specified.
                    if (BodyContentType != "text/html" && Body.ToUpper().Contains("<BODY"))
                        BodyContentType = "text/html";
                }

                // Set the raw body property if requested.
                if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawBody) > 0)
                    RawBody = body;
            }

            // Parse String representations of addresses into MailAddress objects.
            if (fromText.Length > 0)
            {
                MailAddressCollection fromAddresses = MailAddressCollection.Parse(fromText);
                if (fromAddresses.Count > 0)
                    From = fromAddresses[0];
            }

            if (toText.Length > 0)
            {
                To = MailAddressCollection.Parse(toText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress toAddress in To)
                {
                    if (!AllRecipients.Contains(toAddress.Address))
                        AllRecipients.Add(toAddress.Address);
                }
            }

            if (ccText.Length > 0)
            {
                CC = MailAddressCollection.Parse(ccText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress ccAddress in CC)
                {
                    if (!AllRecipients.Contains(ccAddress.Address))
                        AllRecipients.Add(ccAddress.Address);
                }
            }

            if (bccText.Length > 0)
            {
                Bcc = MailAddressCollection.Parse(bccText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress bccAddress in Bcc)
                {
                    if (!AllRecipients.Contains(bccAddress.Address))
                        AllRecipients.Add(bccAddress.Address);
                }
            }

            if (replyToText.Length > 0)
            {
                ReplyToList = MailAddressCollection.Parse(replyToText);
            }
        }

        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="header">The raw headers of the email message.</param>
        /// <param name="body">The raw body of the email message.</param>
        public MailMessage(string headers, string body)
        {
            new MailMessage(headers + "\r\n" + body);
        }
        #endregion Constructors

        #region Destructor
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion Destructor

        #region Public Methods
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message in the specified file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        public static MailMessage LoadFile(string path)
        {
            if (File.Exists(path))
                return new MailMessage(File.ReadAllText(path));
            else
                return null;
        }

        /// <summary>
        /// Generate a multipart/mixed message containing the email's body, alternate views, and attachments.
        /// </summary>
        /// <param name="ContentTransferEncoding">Transfer encoding to use.</param>
        /// <param name="MimeBoundaryName">Text delimiting MIME message parts.</param>
        /// <param name="MimeAlternativeViewBoundaryName">Text delimiting MIME message alternative view parts.</param>
        public async Task<string> MIMEEncode(string ContentTransferEncoding, string MimeBoundaryName)
        {
            // If no Content Transfer Encoding is specified, default to quoted-printable.
            if (string.IsNullOrEmpty(ContentTransferEncoding))
                ContentTransferEncoding = "quoted-printable";

            // Write out body of the message.
            StringBuilder MIMEBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");

            if (!string.IsNullOrEmpty(Body))
            {
                // Handle alternative views by encapsulating them in a multipart/alternative content type.
                if (AlternateViews.Count > 0)
                {
                    foreach (AlternateView alternateView in AlternateViews)
                    {
                        string mimePartContentTransferEncoding = "quoted-printable";
                        if (alternateView.TransferEncoding == TransferEncoding.Base64)
                            mimePartContentTransferEncoding = "base64";

                        MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");
                        MIMEBuilder.Append("Content-Type: " + alternateView.MediaType + "\r\n");
                        MIMEBuilder.Append("Content-Transfer-Encoding: " + mimePartContentTransferEncoding + "\r\n\r\n");

                        byte[] binaryData = new byte[alternateView.ContentStream.Length];
                        await alternateView.ContentStream.ReadAsync(binaryData, 0, binaryData.Length);

                        MIMEBuilder.Append(Functions.Encode(binaryData, mimePartContentTransferEncoding));
                        MIMEBuilder.Append("\r\n");
                    }
                }

                MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");

                if (this.IsBodyHtml)
                    MIMEBuilder.Append("Content-Type: text/html\r\n");
                else
                    MIMEBuilder.Append("Content-Type: text/plain\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: " + ContentTransferEncoding + "\r\n\r\n");

                MIMEBuilder.Append(Functions.Encode(Body, ContentTransferEncoding));
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the alternate views, they shouldn't be rendered again.
            this.AlternateViews.Clear();

            // MIME encode attachments.
            foreach (Attachment attachment in this.Attachments)
            {
                MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");
                if (attachment.Name.ToLower() == "smime.p7m")
                    MIMEBuilder.Append("Content-Type: application/pkcs7-mime; name=smime.p7m; smime-type=enveloped-data\r\n");
                else
                    MIMEBuilder.Append("Content-Type: application/octet-stream; file=" + attachment.Name + "\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                if (!string.IsNullOrEmpty(attachment.ContentId))
                    MIMEBuilder.Append("Content-ID: <" + attachment.ContentId + ">\r\n");
                MIMEBuilder.Append("Content-Disposition: attachment; filename=" + attachment.Name + "\r\n\r\n");

                byte[] binaryData = new byte[attachment.ContentStream.Length];
                await attachment.ContentStream.ReadAsync(binaryData, 0, (int)attachment.ContentStream.Length);

                // Base-64 encode the attachment.
                MIMEBuilder.Append(Functions.ToBase64String(binaryData, 0, binaryData.Length));
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the attachments, they shouldn't be rendered again.
            this.Attachments.Clear();

            MIMEBuilder.Append("--" + MimeBoundaryName + "--");

            return MIMEBuilder.ToString();
        }

        /// <summary>
        /// Process a list of flags as returned by the IMAP server.
        /// </summary>
        /// <param name="flagsString">List of space-separated flags.</param>
        public int ParseFlagsString(string flagsString)
        {
            if (!string.IsNullOrEmpty(flagsString))
            {
                int flagCounter = 0;

                string[] flags = flagsString.Split(' ');
                foreach (string flag in flags)
                {
                    if (!string.IsNullOrEmpty(flag) && !RawFlags.Contains(flag))
                        RawFlags.Add(flag);

                    flagCounter++;

                    switch (flag.ToUpper())
                    {
                        case "\\ANSWERED":
                            Flags = Flags | Flags.Answered;
                            break;
                        case "\\DELETED":
                            Flags = Flags | Flags.Deleted;
                            break;
                        case "\\DRAFT":
                            Flags = Flags | Flags.Draft;
                            break;
                        case "\\FLAGGED":
                            Flags = Flags | Flags.Flagged;
                            break;
                        case "\\RECENT":
                            Flags = Flags | Flags.Recent;
                            break;
                        case "\\SEEN":
                            Flags = Flags | Flags.Seen;
                            break;
                    }
                }

                return flagCounter;
            }
            else
                return 0;
        }

        /// <summary>
        /// Ensure boundary names are unique.
        /// </summary>
        public void RandomizeBoundaryNames()
        {
            MimeBoundaryName = "OpaqueMail-boundary";

            string boundaryRandomness = "";
            Random randomGenerator = new Random();

            // Append 10 random characters.
            for (int i = 0; i < 10; i++)
            {
                int nextCharacter = randomGenerator.Next(1, 36);
                if (nextCharacter > 26)
                    boundaryRandomness += (char)(47 + nextCharacter);
                else
                    boundaryRandomness += (char)(64 + nextCharacter);
            }

            MimeBoundaryName += "-" + boundaryRandomness;
            SmimeSignedCmsBoundaryName += "-" + boundaryRandomness;
            SmimeTripleSignedCmsBoundaryName += "-" + boundaryRandomness;
        }

        /// <summary>
        /// Saves a text representation of the message to the file specified.
        /// </summary>
        /// <param name="path">The file to save to.</param>
        public void SaveFile(string path)
        {
            File.WriteAllText(path, RawHeaders + "\r\n\r\n" + RawBody);
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        private void ProcessMimeParts()
        {
            // Keep track of S/MIME signing and envelope encryption.
            bool allMimePartsSigned = true, allMimePartsEncrypted = true, allMimePartsTripleWrapped = true;

            // Process each MIME part.
            for (int j = 0; j < MimeParts.Count; j++)
            {
                MimePart mimePart = MimeParts[j];

                int semicolon = mimePart.ContentType.IndexOf(";");
                if (semicolon > -1)
                {
                    string originalContentType = mimePart.ContentType;
                    mimePart.ContentType = mimePart.ContentType.Substring(0, semicolon);

                    if (mimePart.ContentType.ToUpper() == "MESSAGE/PARTIAL")
                    {
                        PartialMessageId = Functions.ExtractMimeParameter(originalContentType, "id");
                        int partialMessageNumber = 0;
                        if (int.TryParse(Functions.ExtractMimeParameter(originalContentType, "number"), out partialMessageNumber))
                            PartialMessageNumber = partialMessageNumber;
                    }
                }

                // Extract any signing certificates.  If this MIME part isn't signed, the overall message isn't signed.
                if (mimePart.SmimeSigned)
                {
                    if (mimePart.SmimeSigningCertificates.Count > 0 && SmimeSigningCertificate == null)
                    {
                        foreach (X509Certificate2 signingCert in mimePart.SmimeSigningCertificates)
                        {
                            if (!SmimeSigningCertificateChain.Contains(signingCert))
                            {
                                SmimeSigningCertificateChain.Add(signingCert);
                                SmimeSigningCertificate = signingCert;
                            }
                        }
                    }
                }
                else
                    allMimePartsSigned = false;

                // If this MIME part isn't marked as being in an encrypted envelope, the overall message isn't encrypted.
                if (!mimePart.SmimeEncryptedEnvelope)
                {
                    // Ignore signatures and encryption blocks when determining if everything is encrypted.
                    if (!mimePart.ContentType.StartsWith("application/pkcs7-signature") && !mimePart.ContentType.StartsWith("application/x-pkcs7-signature") && !mimePart.ContentType.StartsWith("application/pkcs7-mime"))
                        allMimePartsEncrypted = false;
                }

                // If this MIME part isn't marked as being triple wrapped, the overall message isn't triple wrapped.
                if (!mimePart.SmimeTripleWrapped)
                {
                    // Ignore signatures and encryption blocks when determining if everything is triple wrapped.
                    if (!mimePart.ContentType.StartsWith("application/pkcs7-signature") && !mimePart.ContentType.StartsWith("application/x-pkcs7-signature") && !mimePart.ContentType.StartsWith("application/pkcs7-mime"))
                        allMimePartsTripleWrapped = false;
                }

                // Set the default primary body, defaulting to text/html and falling back to any text/*.
                string contentTypeToUpper = mimePart.ContentType.ToUpper();
                if (Body.Length < 1)
                {
                    // If the MIME part is of type text/*, set it as the intial message body.
                    if (string.IsNullOrEmpty(mimePart.ContentType) || contentTypeToUpper.StartsWith("TEXT/"))
                    {
                        IsBodyHtml = contentTypeToUpper.StartsWith("TEXT/HTML");
                        Body = mimePart.Body;
                        BodyContentType = mimePart.ContentType;
                        CharSet = mimePart.CharSet;
                    }
                    else
                    {
                        // If the MIME part isn't of type text/*, treat it as an attachment.
                        MemoryStream attachmentStream = new MemoryStream(mimePart.BodyBytes);
                        Attachment attachment;
                        if (mimePart.ContentType.IndexOf("/") > -1)
                            attachment = new Attachment(attachmentStream, mimePart.Name, mimePart.ContentType);
                        else
                            attachment = new Attachment(attachmentStream, mimePart.Name);

                        attachment.ContentId = mimePart.ContentID;
                        Attachments.Add(attachment);
                    }
                }
                else
                {
                    // If the current body isn't text/html and this is, replace the default body with the current MIME part.
                    if (!ContentType.ToUpper().StartsWith("TEXT/HTML") && contentTypeToUpper.StartsWith("TEXT/HTML"))
                    {
                        // Add the previous default body as an alternate view.
                        MemoryStream alternateViewStream = new MemoryStream(Encoding.UTF8.GetBytes(Body));
                        AlternateView alternateView = new AlternateView(alternateViewStream, BodyContentType);
                        if (BodyTransferEncoding != TransferEncoding.Unknown)
                            alternateView.TransferEncoding = BodyTransferEncoding;
                        AlternateViews.Add(alternateView);

                        IsBodyHtml = true;
                        Body = mimePart.Body;
                        BodyContentType = mimePart.ContentType;
                        CharSet = mimePart.CharSet;
                    }
                    else
                    {
                        // If the MIME part isn't of type text/*, treat is as an attachment.
                        MemoryStream attachmentStream = new MemoryStream(mimePart.BodyBytes);
                        Attachment attachment;
                        if (mimePart.ContentType.IndexOf("/") > -1)
                            attachment = new Attachment(attachmentStream, mimePart.Name, mimePart.ContentType);
                        else
                            attachment = new Attachment(attachmentStream, mimePart.Name);
                        attachment.ContentId = mimePart.ContentID;
                        Attachments.Add(attachment);
                    }
                }
            }

            // OpaqueMail optional setting for protecting the subject.
            if (SubjectEncryption && Body.StartsWith("Subject: "))
            {
                int linebreakPosition = Body.IndexOf("\r\n");
                if (linebreakPosition > -1)
                {
                    Body = Body.Substring(linebreakPosition + 2);

                    // Decode international strings and remove escaped linebreaks.
                    string subjectText = Body.Substring(9, linebreakPosition - 9);
                    Subject = Functions.DecodeMailHeader(subjectText).Replace("\r", "").Replace("\n", "");
                }
            }

            // Set the message's S/MIME attributes.
            SmimeSigned = allMimePartsSigned;
            SmimeEncryptedEnvelope = allMimePartsEncrypted;
            SmimeTripleWrapped = allMimePartsTripleWrapped;
        }
        #endregion Private Methods
    }
}