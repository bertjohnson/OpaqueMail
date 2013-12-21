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
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Represents an e-mail message that can be sent using the OpaqueMail.SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public class MailMessage : System.Net.Mail.MailMessage
    {
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
        /// <summary>Type of subject identifier to use.</summary>
        /// <remarks>The default of "IssuerAndSerialNumber" is recommended for most use cases.</remarks>
        public SubjectIdentifierType SubjectIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
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

        #region Public Methods
        /// <summary>
        /// Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
        /// </summary>
        /// <param name="SmimeBoundaryName">Text delimiting S/MIME message parts.</param>
        /// <param name="SmimeAlternativeViewBoundaryName">Text delimiting S/MIME message alternative view parts.</param>
        public async Task<byte[]> MIMEEncode(string SmimeBoundaryName, string SmimeAlternativeViewBoundaryName)
        {
            // Write out body of the message.
            StringBuilder MIMEBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            MIMEBuilder.Append("Content-Type: multipart/mixed; boundary=\"" + SmimeBoundaryName + "\"\r\n");
            MIMEBuilder.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");
            MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");

            if (!string.IsNullOrEmpty(Body))
            {
                MIMEBuilder.Append("--" + SmimeBoundaryName + "\r\n");

                // Handle alternative views by encapsulating them in a multipart/alternative content type.
                if (AlternateViews.Count > 0)
                {
                    MIMEBuilder.Append("Content-Type: multipart/alternative; boundary=\"" + SmimeAlternativeViewBoundaryName + "\"\r\n");
                    MIMEBuilder.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");
                    MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");

                    foreach (AlternateView alternateView in this.AlternateViews)
                    {
                        MIMEBuilder.Append("--" + SmimeAlternativeViewBoundaryName + "\r\n");
                        MIMEBuilder.Append("Content-Type: " + alternateView.ContentType + "\r\n");
                        MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n\r\n");

                        Stream dataStream = alternateView.ContentStream;
                        byte[] binaryData = new byte[dataStream.Length];
                        await dataStream.ReadAsync(binaryData, 0, binaryData.Length);

                        MIMEBuilder.Append(Functions.ToBase64String(binaryData));
                        MIMEBuilder.Append("\r\n");
                    }
                    MIMEBuilder.Append("--" + SmimeAlternativeViewBoundaryName + "\r\n");
                }
                
                if (this.IsBodyHtml)
                    MIMEBuilder.Append("Content-Type: text/html\r\n");
                else
                    MIMEBuilder.Append("Content-Type: text/plain\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n\r\n");

                MIMEBuilder.Append(Functions.ToBase64String(Body));
                MIMEBuilder.Append("\r\n");

                // If there are alternative views, close the multipart/alternative envelope.
                if (AlternateViews.Count > 0)
                    MIMEBuilder.Append("--" + SmimeAlternativeViewBoundaryName + "--\r\n");
            }
            // Since we've processed the alternate views, they shouldn't be rendered again.
            this.AlternateViews.Clear();

            // MIME encode attachments.
            foreach (Attachment attachment in this.Attachments)
            {
                MIMEBuilder.Append("--" + SmimeBoundaryName + "\r\n");
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

            MIMEBuilder.Append("--" + SmimeBoundaryName + "--\r\n");

            // Determine the body encoding, defaulting to UTF-8.
            Encoding bodyEncoding = BodyEncoding != null ? BodyEncoding : new UTF8Encoding();
            Encoder bodyEncoder = bodyEncoding.GetEncoder();

            // Encode and return the message.
            char[] chars = MIMEBuilder.ToString().ToCharArray();
            byte[] MIMEMessageBytes = new byte[bodyEncoder.GetByteCount(chars, 0, chars.Length, false)];
            int byteCount = bodyEncoder.GetBytes(chars, 0, chars.Length, MIMEMessageBytes, 0, true);

            return MIMEMessageBytes;
        }
        #endregion Public Methods
    }
}
