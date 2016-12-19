/*
 * OpaqueMail (https://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (https://bertjohnson.com/) of Allcloud Inc. (https://allcloud.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System.Security.Cryptography.X509Certificates;

namespace OpaqueMail
{
    /// <summary>
    /// Represents an email message that was either received using the ImapClient or Pop3Client classes or will be sent using the SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate handling of secure PGP and S/MIME messages.
    /// </summary>
    public partial class MailMessage
    {
        #region Public Members
        /// <summary>Encrypt the email's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.</summary>
        public bool SmimeEncryptedEnvelope { get; set; }
        /// <summary>Determine how the S/MIME envelope will be encrypted.</summary>
        /// <summary>Type of subject identifier to use.</summary>
        /// <remarks>The default of "IssuerAndSerialNumber" is recommended for most use cases.</remarks>
        public SmimeEncryptionOptionFlags SmimeEncryptionOptionFlags { get; set; }
        /// <summary>Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</summary>
        public SmimeSettingsMode SmimeSettingsMode { get; set; }
        /// <summary>Sign the email.  When true, signing is the first S/MIME operation.</summary>
        public bool SmimeSigned { get; set; }
        /// <summary>
        /// Certificate used when signing messages.
        /// Requires private key.
        /// </summary>
        public X509Certificate2 SmimeSigningCertificate { get; set; }
        /// <summary>Certificate chain used to sign the message.</summary>
        public X509Certificate2Collection SmimeSigningCertificateChain { get; set; }
        /// <summary>Determine how the S/MIME message will be signed.</summary>
        public SmimeSigningOptionFlags SmimeSigningOptionFlags { get; set; }
        /// <summary>Triple-wrap the email by signing, then encrypting the envelope, then signing the encrypted envelope.</summary>
        public bool SmimeTripleWrapped { get; set; }
        #endregion Public Members

        #region Protected Members
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        protected string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        protected string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        #endregion Protected Members
    }
}
