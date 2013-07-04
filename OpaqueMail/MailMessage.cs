using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Represents an e-mail message that can be send using the SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public class MailMessage : System.Net.Mail.MailMessage
    {
        #region Constructors
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        public MailMessage() : base() { }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class by using the specified OpaqueMail.MailAddress class objects.
        /// </summary>
        /// <param name="from">A System.Net.Mail.MailAddress that contains the address of the sender of the e-mail message.</param>
        /// <param name="to">A System.Net.Mail.MailAddress that contains the address of the recipient of the e-mail message.</param>
        public MailMessage(MailAddress from, MailAddress to) : base(from, to) { }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class by using the specified System.String class objects.
        /// </summary>
        /// <param name="from">A System.String that contains the address of the sender of the e-mail message.</param>
        /// <param name="to">A System.String that contains the address of the recipient of the e-mail message.</param>
        public MailMessage(string from, string to) : base(from, to) { }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        /// <param name="from">A System.String that contains the address of the sender of the e-mail message.</param>
        /// <param name="to">A System.String that contains the address of the recipient of the e-mail message.</param>
        /// <param name="subject">A System.String that contains the subject text.</param>
        /// <param name="body">A System.String that contains the message body.</param>
        public MailMessage(string from, string to, string subject, string body) : base(from, to, subject, body) { }
        #endregion Constructors

        #region Public Members
        /// <summary>
        /// Require recipient public keys to be validated.
        /// </summary>
        public bool SMIMERequireCertificateVerification = true;
        /// <summary>
        /// Require recipient public keys to be have an Enhanced Key Usage value of "Secure Email" (1.3.6.1.5.5.7.3.4).
        /// </summary>
        public bool SMIMERequireEnhancedKeyUsageofSecureEmail = true;
        /// <summary>
        /// Require recipient public keys to be have a Key Usage value of "Key Encipherment".
        /// </summary>
        public bool SMIMERequireKeyUsageOfDataEncipherment = true;
        /// <summary>
        /// Exclude public keys with invalid "Valid from" and "Valid to" dates.
        /// Only used when SMIMERequireCertificateVerification is false.
        /// </summary>
        public bool SMIMERequireValidCertificateDates = true;

        /// <summary>
        /// Sign the e-mail.  When true, signing is the first S/MIME operation.
        /// </summary>
        public bool SMIMESign = false;
        /// <summary>
        /// Encrypt the e-mail's envelope.  When SMIMESign is true, encryption is the second S/MIME operation.
        /// </summary>
        public bool SMIMEEncryptEnvelope = false;
        /// <summary>
        /// Triple-wrap the e-mail by signing, then encrypting the envelope, then signing the encrypted envelope.
        /// </summary>
        public bool SMIMETripleWrap = false;

        /// <summary>
        /// Stamp the e-mail with a signed timestamp.
        /// Only applies if SMIMESign is true.
        /// </summary>
        public bool SMIMESignTime = true;
        /// <summary>
        /// Encrypt headers such as Subject, To, and From in addition to the message.
        /// Only applies if SMIMEEncryptEnvelope is true.
        /// </summary>
        public bool SMIMEEncryptHeaders = true;

        /// <summary>
        /// Text delimiting S/MIME message parts.
        /// </summary>
        public string SMIMEBoundaryName = "OpaqueMail-boundary";
        /// <summary>
        /// Text delimiting S/MIME message parts related to signatures.
        /// </summary>
        public string SMIMESignedCmsBoundaryName = "OpaqueMail-signed-Cmsboundary";
        /// <summary>
        /// Text delimiting MIME message parts in triple-wrapped messages.
        /// </summary>
        public string SMIMETripleSignedCmsBoundaryName = "OpaqueMail-triple-signed-Cmsboundary";

        /// <summary>
        /// Certificate used when signing messages.
        /// Requires private key.
        /// </summary>
        public X509Certificate2 SMIMESignerCertificate;
        #endregion Public Members

        /// <summary>
        /// Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
        /// </summary>
        /// <param name="buffer">Buffer used during various S/MIME operations.</param>
        public byte[] MIMEEncode(ref byte[] buffer)
        {
            StringBuilder MIMEBuilder = new StringBuilder();

            // Write out body of the message.
            MIMEBuilder.Append("Content-Type: multipart/mixed; boundary=\"" + SMIMEBoundaryName + "\"\r\n\r\n");
            MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");
            MIMEBuilder.Append("--" + SMIMEBoundaryName + "\r\n");
            if (this.IsBodyHtml)
                MIMEBuilder.Append("Content-Type: text/html; charset=\"UTF-8\"\r\n");
            else
                MIMEBuilder.Append("Content-Type: text/plain; charset=\"UTF-8\"\r\n");
            MIMEBuilder.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");

            // 7-bit encode the body with lines no longer than 100 characters each.
            MIMEBuilder.Append(Functions.To7BitString(this.Body));
            MIMEBuilder.Append("\r\n");

            // MIME encode alternate views.
            foreach (AlternateView alternateView in this.AlternateViews)
            {
                // Determine the alternate view encoding, defaulting to UTF-8.
                Encoding encoding = alternateView.ContentType.CharSet != null ? Encoding.GetEncoding(alternateView.ContentType.CharSet) : new UTF8Encoding();

                MIMEBuilder.Append("--" + SMIMEBoundaryName + "\r\n");
                MIMEBuilder.Append("Content-Type: " + alternateView.ContentType + "; charset=\"" + encoding.WebName + "\"\r\n\r\n");

                Stream dataStream = alternateView.ContentStream;
                byte[] binaryData = new byte[dataStream.Length];
                dataStream.Read(binaryData, 0, binaryData.Length);

                MIMEBuilder.Append(encoding.GetString(binaryData));
            }
            // Since we've processed the alternate views, they shouldn't be rendered again.
            this.AlternateViews.Clear();

            // MIME encode attachments.
            foreach (Attachment attachment in this.Attachments)
            {
                MIMEBuilder.Append("--" + SMIMEBoundaryName + "\r\n");
                MIMEBuilder.Append("Content-Type: application/octet-stream; file=" + attachment.Name + "\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                MIMEBuilder.Append("Content-Disposition: attachment; filename=" + attachment.Name + "\r\n\r\n");

                Stream dataStream = attachment.ContentStream;
                byte[] binaryData = new byte[dataStream.Length];
                dataStream.Read(binaryData, 0, binaryData.Length);

                // Base-64 encode the attachment.
                MIMEBuilder.Append(Functions.ToBase64String(ref binaryData, 0, binaryData.Length));
  
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the attachments, they shouldn't be rendered again.
            this.Attachments.Clear();

            MIMEBuilder.Append("--" + SMIMEBoundaryName + "--");

            // Determine the body encoding, defaulting to UTF-8.
            Encoding bodyEncoding = BodyEncoding != null ? BodyEncoding : new UTF8Encoding();
            Encoder bodyEncoder = bodyEncoding.GetEncoder();

            // Encode and return the message.
            string MIMEMessage = MIMEBuilder.ToString();
            int byteCount = bodyEncoder.GetBytes(MIMEMessage.ToCharArray(), 0, MIMEMessage.Length, buffer, 0, true);
            byte[] MIMEMessageBytes = new byte[byteCount];
            Buffer.BlockCopy(buffer, 0, MIMEMessageBytes, 0, byteCount);

            return MIMEMessageBytes;
        }
    }
}
