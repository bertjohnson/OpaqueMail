using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    public enum Flags
    {
        None = 0,
        Seen = 1,
        Answered = 2,
        Flagged = 4,
        Deleted = 8,
        Draft = 16
    }

    /// <summary>
    /// Represents an e-mail message that can be send using the SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public partial class MailMessage
    {
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="messageText">The raw contents of the e-mail message.</param>
        public MailMessage(string messageText)
        {
            string headers = messageText.Substring(messageText.IndexOf("\r\n") + 2);

            #region Process Headers

            // TODO: Process headers
            string[] headersList = headers.Replace("\r", "").Split('\n');

            string lastHeaderType = "";
            foreach (string header in headersList)
            {
                // TODO: Add content type
                /*                if (lastHeaderType == "content-type")
                                {
                                    if (header.StartsWith(" boundary="))
                                        BodyEncoding += header;
                                }*/
                if (lastHeaderType == "from")
                {
                    if (header.StartsWith(" <"))
                    {
                        message.FromName = message.FromAddress;
                        message.FromAddress = header.Substring(2, header.Length - 3);
                    }
                }

                int splitterPos = header.IndexOf(":");
                if (splitterPos > -1 && splitterPos < header.Length - 1)
                {
                    string[] headerParts = new string[] { header.Substring(0, splitterPos), header.Substring(splitterPos + 2) };
                    string headerType = headerParts[0].ToLower();
                    switch (headerType)
                    {
                        case "authentication-results":
                            message.AuthenticationResults = headerParts[1];
                            break;
                        case "cc":
                            message.CC = headerParts[1];
                            break;
                        case "content-transfer-encoding":
                            message.ContentTransferEncoding = headerParts[1];
                            break;
                        case "content-type":
                            message.ContentType = headerParts[1];
                            break;
                        case "date":
                            string dateString = headerParts[1];
                            int dateStringParenthesis = dateString.IndexOf("(");
                            if (dateStringParenthesis > -1)
                                dateString = dateString.Substring(0, dateStringParenthesis - 1);
                            DateTime.TryParse(dateString, out message.Date);
                            break;
                        case "delivered-to":
                            message.DeliveredTo = headerParts[1];
                            break;
                        case "domainkey-signature":
                            message.DomainKeySignature = headerParts[1];
                            break;
                        case "domainkey-status":
                            message.DomainKeyStatus = headerParts[1];
                            break;
                        case "from":
                            string from = Functions.EscapeMailHeader(headerParts[1]);

                            int startAddress = from.LastIndexOf("<");
                            if (startAddress > 0)
                            {
                                message.FromName = from.Substring(0, startAddress - 1).Replace("\"", "");
                                message.FromAddress = from.Substring(startAddress + 1, from.Length - startAddress - 2);
                            }
                            else if (startAddress > -1)
                                message.FromAddress = from.Substring(startAddress + 1, from.Length - startAddress - 2);
                            else
                                message.FromAddress = from;
                            break;
                        case "importance":
                            message.Importance = headerParts[1];
                            break;
                        case "in-reply-to":
                            if (headerParts[1].Length > 1)
                                message.InReplyTo = headerParts[1].Substring(1, headerParts[1].Length - 2);
                            break;
                        case "message-id":
                            if (headerParts[1].Length > 1)
                                message.MessageID = headerParts[1].Substring(1, headerParts[1].Length - 2);
                            break;
                        case "mime-version":
                            message.MIMEVersion = headerParts[1];
                            break;
                        case "received-spf":
                            message.ReceivedSPF = headerParts[1];
                            break;
                        case "return-path":
                            message.ReturnPath = headerParts[1];
                            break;
                        case "sender":
                            message.Sender = headerParts[1];
                            break;
                        case "subject":
                            message.Subject = Functions.EscapeMailHeader(headerParts[1]);
                            break;
                        case "to":
                            message.To = Functions.EscapeMailHeader(headerParts[1]);
                            break;
                        case "x-mailer":
                            message.Mailer = headerParts[1];
                            break;
                        case "x-maillist-id":
                            message.MailListID = headerParts[1];
                            break;
                        case "x-msmail-priority":
                            message.MSMailPriority = headerParts[1];
                            break;
                        case "x-priority":
                            message.Priority = headerParts[1];
                            break;
                        case "x-rcpt-to":
                            if (headerParts[1].Length > 1)
                                message.RcptTo = headerParts[1].Substring(1, headerParts[1].Length - 2);
                            break;
                        case "x-spam-score":
                            message.SpamScore = headerParts[1];
                            break;
                    }
                    lastHeaderType = headerType;
                }
                else
                {
                    // Handle spillover
                    switch (lastHeaderType)
                    {
                        default:
                            break;
                    }
                }
            }
            #endregion Process Headers
        }
    }
}