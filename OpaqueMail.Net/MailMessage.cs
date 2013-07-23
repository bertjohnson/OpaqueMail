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
    /// <summary>
    /// Represents an e-mail message that can be sent using the OpaqueMail.SmtpClient class.
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
        /// <summary>
        /// Cast a ReadOnlyMailMessage as a regular MailMessage.
        /// </summary>
        /// <param name="message">ReadOnlyMailMessage to import properties from.</param>
        public MailMessage FromReadOnlyMailMessage(ReadOnlyMailMessage message)
        {
            return message as MailMessage;
        }
        #endregion Constructors

        #region Public Members
        /// <summary>
        /// Size of the entire message.
        /// When sending e-mail, this is a rough estimate only.
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

                    foreach (AlternateView alternateView in AlternateViews)
                    {
                        using (Stream dataStream = alternateView.ContentStream)
                        {
                            size += dataStream.Length;
                        }
                    }

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
        /// <summary>Encrypt the e-mail's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.</summary>
        public bool SmimeEncryptedEnvelope = false;
        /// <summary>Determine how the S/MIME envelope will be encrypted.</summary>
        public SmimeEncryptionOptionFlags SmimeEncryptionOptionFlags = SmimeEncryptionOptionFlags.RequireCertificateVerification;
        /// <summary>Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</summary>
        public SmimeSettingsMode SmimeSettingsMode = SmimeSettingsMode.RequireExactSettings;
        /// <summary>Sign the e-mail.  When true, signing is the first S/MIME operation.</summary>
        public bool SmimeSigned = false;
        /// <summary>
        /// Certificate used when signing messages.
        /// Requires private key.
        /// </summary>
        public X509Certificate2 SmimeSigningCertificate;
        /// <summary>Determine how the S/MIME message will be signed.</summary>
        public SmimeSigningOptionFlags SmimeSigningOptionFlags = SmimeSigningOptionFlags.SignTime;
        /// <summary>Triple-wrap the e-mail by signing, then encrypting the envelope, then signing the encrypted envelope.</summary>
        public bool SmimeTripleWrapped = false;
        #endregion Public Members

        #region Private Members
        /// <summary>Size of the loaded message, as calculated in ReadOnlyMailMessage's constructor.</summary>
        private long loadedSize = -1;
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
        /// </summary>
        /// <param name="buffer">Buffer used during various S/MIME operations.</param>
        /// <param name="SmimeBoundaryName">Text delimiting S/MIME message parts.</param>
        public async Task<byte[]> MIMEEncode(byte[] buffer, string SmimeBoundaryName)
        {
            StringBuilder MIMEBuilder = new StringBuilder();

            // Write out body of the message.
            MIMEBuilder.Append("Content-Type: multipart/mixed; boundary=\"" + SmimeBoundaryName + "\"\r\n\r\n");
            MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");
            MIMEBuilder.Append("--" + SmimeBoundaryName + "\r\n");
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

                MIMEBuilder.Append("--" + SmimeBoundaryName + "\r\n");
                MIMEBuilder.Append("Content-Type: " + alternateView.ContentType + "; charset=\"" + encoding.WebName + "\"\r\n\r\n");

                using (Stream dataStream = alternateView.ContentStream)
                {
                    byte[] binaryData = new byte[dataStream.Length];
                    await dataStream.ReadAsync(binaryData, 0, binaryData.Length);

                    MIMEBuilder.Append(encoding.GetString(binaryData));
                }
            }
            // Since we've processed the alternate views, they shouldn't be rendered again.
            this.AlternateViews.Clear();

            // MIME encode attachments.
            foreach (Attachment attachment in this.Attachments)
            {
                MIMEBuilder.Append("--" + SmimeBoundaryName + "\r\n");
                MIMEBuilder.Append("Content-Type: application/octet-stream; file=" + attachment.Name + "\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                MIMEBuilder.Append("Content-Disposition: attachment; filename=" + attachment.Name + "\r\n\r\n");

                using (Stream dataStream = attachment.ContentStream)
                {
                    byte[] binaryData = new byte[dataStream.Length];
                    await dataStream.ReadAsync(binaryData, 0, binaryData.Length);

                    // Base-64 encode the attachment.
                    MIMEBuilder.Append(Functions.ToBase64String(binaryData, 0, binaryData.Length));
                }
  
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the attachments, they shouldn't be rendered again.
            this.Attachments.Clear();

            MIMEBuilder.Append("--" + SmimeBoundaryName + "--");

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
        #endregion Public Methods
    }
}
