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
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Represents an e-mail message that was received using the ImapClient or Pop3Client classes.
    /// Includes OpaqueMail extensions to facilitate handling of secure S/MIME messages.
    /// </summary>
    public class ReadOnlyMailMessage : OpaqueMail.Net.MailMessage
    {
        #region Public Members
        /// <summary>Collection of all recipients of this message, based on To, CC, and Bcc paramaters.</summary>
        public List<string> AllRecipients = new List<string>();
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
        /// <summary>
        /// Extended e-mail headers.
        /// Only populated when the ReadOnlyMailMessage is instantiated with a parseExtendedHeaders setting of true.
        /// </summary>
        public ExtendedProperties ExtendedProperties;
        /// <summary>Flags representing the processed state of the message.</summary>
        public Flags Flags;
        /// <summary>Mailbox the message was read from.</summary>
        public string Mailbox;
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
        /// <summary>Return-Path header.</summary>
        public string ReturnPath = "";
        /// <summary>Certificate chain used to sign the message.</summary>
        public X509Certificate2Collection SmimeSigningCertificateChain = new X509Certificate2Collection();
        /// <summary>X-Subject-Encryption header, as optionally used by OpaqueMail.</summary>
        public bool SubjectEncryption;
        #endregion Public Members

        #region Constructors
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="messageText">The raw contents of the e-mail message.</param>
        public ReadOnlyMailMessage(string messageText) : this(messageText, ReadOnlyMailMessageProcessingFlags.None, false) { }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the e-mail message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        public ReadOnlyMailMessage(string messageText, ReadOnlyMailMessageProcessingFlags processingFlags) : this(messageText, processingFlags, false) { }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.ReadOnlyMailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the e-mail message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        /// <param name="parseExtendedHeaders">Whether to populate the ExtendedHeaders object.</param>
        public ReadOnlyMailMessage(string messageText, ReadOnlyMailMessageProcessingFlags processingFlags, bool parseExtendedHeaders)
        {
            if (((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders) > 0)
                && (processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawBody) > 0)
                RawMessage = messageText;

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

            // Set the raw headers property if requested.
            if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders) > 0)
                RawHeaders = headers;

            // Calculate the size of the message.
            Size = messageText.Length;

            // Temporary header variables to be processed by Functions.FromMailAddressString() later.
            string fromText = "";
            string toText = "";
            string ccText = "";
            string bccText = "";
            string replyToText = "";
            string subjectText = "";

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
                        Headers[headerParts[0]] = headerValue;

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
                                MailAddressCollection senderCollection = Functions.FromMailAddressString(headerValue);
                                if (senderCollection.Count > 0)
                                    this.Sender = senderCollection[0];
                            }
                            break;
                        case "subject":
                            subjectText = headerValue;
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
                        ExtendedProperties = new ExtendedProperties();

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
            if (cutoff > -1)
                 body = messageText.Substring(cutoff + 2);
            if (!string.IsNullOrEmpty(body))
            {
                // Set the raw body property if requested.
                if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeRawBody) > 0)
                    RawBody = body;

                // Parse body into MIME parts.
                List<MimePart> mimeParts = MimePart.ExtractMIMEParts(ContentType, CharSet, ContentTransferEncoding, body, ProcessingFlags);

                // Process each MIME part.
                if (mimeParts.Count > 0)
                {
                    // Keep track of S/MIME signing and envelope encryption.
                    bool allMimePartsSigned = true, allMimePartsEncrypted = true, allMimePartsTripleWrapped = true;

                    // Process each MIME part.
                    for (int j = 0; j < mimeParts.Count; j++)
                    {
                        MimePart mimePart = mimeParts[j];

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
                                CharSet = mimePart.CharSet;
                                ContentType = mimePart.ContentType;
                                if (mimePart.ContentTransferEncoding != TransferEncoding.Unknown)
                                    BodyTransferEncoding = mimePart.ContentTransferEncoding;
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
                        else
                        {
                            // If the current body isn't text/html and this is, replace the default body with the current MIME part.
                            if (!ContentType.ToUpper().StartsWith("TEXT/HTML") && contentTypeToUpper.StartsWith("TEXT/HTML"))
                            {
                                // Add the previous default body as an alternate view.
                                MemoryStream alternateViewStream = new MemoryStream(Encoding.UTF8.GetBytes(Body));
                                AlternateView alternateView = new AlternateView(alternateViewStream, ContentType);
                                if (BodyTransferEncoding != TransferEncoding.Unknown)
                                    alternateView.TransferEncoding = BodyTransferEncoding;
                                AlternateViews.Add(alternateView);

                                IsBodyHtml = true;
                                Body = mimePart.Body;
                                CharSet = mimePart.CharSet;
                                ContentType = mimePart.ContentType;
                                if (mimePart.ContentTransferEncoding != TransferEncoding.Unknown)
                                    BodyTransferEncoding = mimePart.ContentTransferEncoding;
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
                            subjectText = Body.Substring(9, linebreakPosition - 9);
                            Body = Body.Substring(linebreakPosition + 2);
                        }
                    }

                    // Set the message's S/MIME attributes.
                    SmimeSigned = allMimePartsSigned;
                    SmimeEncryptedEnvelope = allMimePartsEncrypted;
                    SmimeTripleWrapped = allMimePartsTripleWrapped;
                }
                else
                {
                    // Process non-MIME messages.
                    Body = body;
                }
            }

            // Parse String representations of addresses into MailAddress objects.
            if (fromText.Length > 0)
            {
                MailAddressCollection fromAddresses = Functions.FromMailAddressString(fromText);
                if (fromAddresses.Count > 0)
                    From = fromAddresses[0];
            }

            if (toText.Length > 0)
            {
                To.Clear();
                MailAddressCollection toAddresses = Functions.FromMailAddressString(toText);
                foreach (MailAddress toAddress in toAddresses)
                    To.Add(toAddress);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress toAddress in toAddresses)
                {
                    if (!AllRecipients.Contains(toAddress.Address))
                        AllRecipients.Add(toAddress.Address);
                }
            }

            if (ccText.Length > 0)
            {
                CC.Clear();
                MailAddressCollection ccAddresses = Functions.FromMailAddressString(ccText);
                foreach (MailAddress ccAddress in ccAddresses)
                    CC.Add(ccAddress);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress ccAddress in ccAddresses)
                {
                    if (!AllRecipients.Contains(ccAddress.Address))
                        AllRecipients.Add(ccAddress.Address);
                }
            }

            if (bccText.Length > 0)
            {
                Bcc.Clear();
                MailAddressCollection bccAddresses = Functions.FromMailAddressString(bccText);
                foreach (MailAddress bccAddress in bccAddresses)
                    Bcc.Add(bccAddress);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress bccAddress in bccAddresses)
                {
                    if (!AllRecipients.Contains(bccAddress.Address))
                        AllRecipients.Add(bccAddress.Address);
                }
            }

            if (replyToText.Length > 0)
            {
                ReplyToList.Clear();
                MailAddressCollection replyToAddresses = Functions.FromMailAddressString(replyToText);
                foreach (MailAddress replyToAddress in replyToAddresses)
                    ReplyToList.Add(replyToAddress);
            }

            // Decode international strings and remove escaped linebreaks.
            Subject = Functions.DecodeMailHeader(subjectText).Replace("\r", "").Replace("\n", "");
        }

        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="header">The raw headers of the e-mail message.</param>
        /// <param name="body">The raw body of the e-mail message.</param>
        public ReadOnlyMailMessage(string header, string body)
        {
            new ReadOnlyMailMessage(header + "\r\n" + body);
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Cast a ReadOnlyMailMessage to a MaiLMessage.
        /// </summary>
        /// <returns>A MailMessage representation.</returns>
        public MailMessage AsMailMessage()
        {
            MailMessage castMessage = new MailMessage();

            // Process alternate views.
            foreach (AlternateView alternateView in AlternateViews)
                castMessage.AlternateViews.Add(alternateView);

            // Process attachments.
            foreach (Attachment attachment in Attachments)
                castMessage.Attachments.Add(attachment);

            // Process BCC.
            foreach (MailAddress address in Bcc)
                castMessage.Bcc.Add(address);

            castMessage.Body = Body;
            if (BodyEncoding != Encoding.ASCII)
                castMessage.BodyEncoding = BodyEncoding;
            if (BodyTransferEncoding != System.Net.Mime.TransferEncoding.Unknown)
                castMessage.BodyTransferEncoding = BodyTransferEncoding;
            else
                castMessage.BodyTransferEncoding = System.Net.Mime.TransferEncoding.SevenBit;

            // Process CC.
            foreach (MailAddress address in CC)
                castMessage.CC.Add(address);

            castMessage.DeliveryNotificationOptions = DeliveryNotificationOptions;
            castMessage.From = From;

            // Process headers.
            foreach (string headerKey in Headers.Keys)
                castMessage.Headers.Add(headerKey, Headers[headerKey]);

            if (HeadersEncoding != null)
                castMessage.HeadersEncoding = HeadersEncoding;
            castMessage.IsBodyHtml = IsBodyHtml || castMessage.Body.IndexOf("<HTML", StringComparison.OrdinalIgnoreCase) > -1;
            castMessage.Priority = Priority;

            // Process ReplyToList.
            foreach (MailAddress address in ReplyToList)
                castMessage.ReplyToList.Add(address);

            castMessage.Sender = Sender;
            castMessage.SmimeEncryptedEnvelope = SmimeEncryptedEnvelope;
            castMessage.SmimeEncryptionOptionFlags = SmimeEncryptionOptionFlags;
            castMessage.SmimeSettingsMode = SmimeSettingsMode;
            castMessage.SmimeSigned = SmimeSigned;
            castMessage.SmimeSigningCertificate = SmimeSigningCertificate;
            castMessage.SmimeSigningOptionFlags = SmimeSigningOptionFlags;
            castMessage.SmimeTripleWrapped = SmimeTripleWrapped;
            castMessage.Subject = Subject;
            if (SubjectEncoding != null)
                castMessage.SubjectEncoding = SubjectEncoding;

            // Process To.
            foreach (MailAddress address in To)
                castMessage.To.Add(address);

            return castMessage;
        }

        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message in the specified file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        public static ReadOnlyMailMessage LoadFile(string path)
        {
            if (File.Exists(path))
                return new ReadOnlyMailMessage(File.ReadAllText(path));
            else
                return null;
        }

        /// <summary>
        /// Process a list of flags as returned by the IMAP server.
        /// </summary>
        /// <param name="flagsString">List of space-separated flags.</param>
        public int ParseFlagsString(string flagsString)
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

        /// <summary>
        /// Saves a text representation of the message to the file specified.
        /// </summary>
        /// <param name="path">The file to save to.</param>
        public void SaveFile(string path)
        {
            File.WriteAllText(path, RawHeaders + "\r\n\r\n" + RawBody);
        }
        #endregion Public Methods
    }
}