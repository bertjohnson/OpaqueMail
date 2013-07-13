using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Allows applications to send e-mail by using the Simple Mail Transport Protocol (SMTP).
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public class SmtpClient : System.Net.Mail.SmtpClient
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class by using configuration file settings.
        /// </summary>
        public SmtpClient() : base() { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends e-mail by using the specified SMTP server.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        public SmtpClient(string host) : base(host) { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends e-mail by using the specified SMTP server and port.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        /// <param name="port">Port to be used by the host.</param>
        public SmtpClient(string host, int port) : base(host, port) { }
        #endregion Constructors

        #region Public Members

        /// <summary>Text delimiting S/MIME message parts.</summary>
        public string SmimeBoundaryName = "OpaqueMail-boundary";
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        public string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        public string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        /// <summary>
        /// Collection of certificates to be used when searching for recipient public keys.
        /// If not specified, SmtpClient will use the current Windows user's certificate store.
        /// </summary>
        public X509Certificate2Collection SmimeValidCertificates = null;
        #endregion Public Members

        #region Private Members
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] buffer = new byte[Constants.BUFFERSIZE];
        /// <summary>Cache of recipient public keys to speed up subsequent usage of this SmtpClient.</summary>
        private Dictionary<string, X509Certificate2> SmimeCertificateCache = new Dictionary<string, X509Certificate2>();
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// Performs requested S/MIME signing and encryption.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public async Task Send(MailMessage message)
        {
            // If not performing any S/MIME encryption or signing, use the default System.Net.Mail.SmtpClient Send() method.
            if (!message.SmimeSigned && !message.SmimeEncryptedEnvelope && !message.SmimeTripleWrapped)
                base.Send(message);
            else
                await SmimeSend(message);
        }

        /// <summary>
        /// Check whether all recipients on the message have valid public keys and will be able to receive S/MIME encrypted envelopes.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public bool SmimeVerifyAllRecipientsHavePublicKeys(MailMessage message)
        {
            // Prepare recipient keys if this message will be encrypted.
            Dictionary<string, MailAddress> addressesNeedingPublicKeys;
            HashSet<string> addressesWithPublicKeys;
            ResolvePublicKeys(message, out addressesWithPublicKeys, out addressesNeedingPublicKeys);

            return addressesNeedingPublicKeys.Count < 1;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Helper function to look up and validate public keys for each recipient.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        /// <param name="addressesWithPublicKeys">Collection containing recipients with valid public keys.</param>
        /// <param name="addressesNeedingPublicKeys">Collection containing recipients without valid public keys.</param>
        private void ResolvePublicKeys(MailMessage message, out HashSet<string> addressesWithPublicKeys, out Dictionary<string, MailAddress> addressesNeedingPublicKeys)
        {
            // Initialize collections for all recipients.
            addressesWithPublicKeys = new HashSet<string>();
            addressesNeedingPublicKeys = new Dictionary<string, MailAddress>();
            foreach (MailAddress toAddress in message.To)
            {
                string canonicalToAddress = toAddress.Address.ToUpper();
                if (SmimeCertificateCache.ContainsKey(canonicalToAddress))
                    addressesWithPublicKeys.Add(canonicalToAddress);
                else
                    addressesNeedingPublicKeys.Add(canonicalToAddress, toAddress);
            }
            foreach (MailAddress ccAddress in message.CC)
            {
                string canonicalCCAddress = ccAddress.Address.ToUpper();
                if (SmimeCertificateCache.ContainsKey(canonicalCCAddress))
                    addressesWithPublicKeys.Add(canonicalCCAddress);
                else
                    addressesNeedingPublicKeys.Add(canonicalCCAddress, ccAddress);
            }
            foreach (MailAddress bccAddress in message.Bcc)
            {
                string canonicalBccAddress = bccAddress.Address.ToUpper();
                if (SmimeCertificateCache.ContainsKey(canonicalBccAddress))
                    addressesWithPublicKeys.Add(canonicalBccAddress);
                else
                    addressesNeedingPublicKeys.Add(canonicalBccAddress, bccAddress);
            }

            // If any addresses haven't been mapped to public keys, map them.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                // Read from the Windows certificate store if valid certificates aren't specified.
                if (SmimeValidCertificates == null)
                {
                    X509Store store = new X509Store(StoreLocation.CurrentUser);
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                    SmimeValidCertificates = store.Certificates;
                    store.Close();
                }

                // Loop through certificates and check for matching recipients.
                /// TODO: Allow for users to choose when there are multiple certificate options and/or define default criteria.
                foreach (X509Certificate2 cert in SmimeValidCertificates)
                {
                    // Only look at certificates with e-mail subject names.
                    string canonicalCertSubject = "";
                    if (cert.Subject.StartsWith("E="))
                    {
                        canonicalCertSubject = cert.Subject.Substring(2).ToUpper();
                        int certSubjectComma = canonicalCertSubject.IndexOf(",");
                        if (certSubjectComma > -1)
                            canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                        // Only proceed if the key is for a recipient of this e-mail.
                        if (!addressesNeedingPublicKeys.ContainsKey(canonicalCertSubject))
                            continue;
                    }
                    else
                        continue;

                    // Verify the certificate chain.
                    if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireCertificateVerification) > 0)
                    {
                        if (!cert.Verify())
                            continue;
                    }

                    // Ensure valid key usage scenarios.
                    if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireKeyUsageOfDataEncipherment) > 0 || (message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0)
                    {
                        bool keyDataEncipherment = false, enhancedKeySecureEmail = false;
                        foreach (X509Extension extension in cert.Extensions)
                        {
                            if (!keyDataEncipherment && extension.Oid.FriendlyName == "Key Usage")
                            {
                                X509KeyUsageExtension ext = (X509KeyUsageExtension)extension;
                                if ((ext.KeyUsages & X509KeyUsageFlags.DataEncipherment) != X509KeyUsageFlags.None)
                                {
                                    keyDataEncipherment = true;

                                    if (!((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0))
                                        break;
                                }
                            }
                            if (!enhancedKeySecureEmail && extension.Oid.FriendlyName == "Enhanced Key Usage")
                            {
                                X509EnhancedKeyUsageExtension ext = (X509EnhancedKeyUsageExtension)extension;
                                OidCollection oids = ext.EnhancedKeyUsages;
                                foreach (Oid oid in oids)
                                {
                                    if (oid.FriendlyName == "Secure Email")
                                    {
                                        enhancedKeySecureEmail = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireKeyUsageOfDataEncipherment) > 0 && !keyDataEncipherment)
                            continue;
                        if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0 && !enhancedKeySecureEmail)
                            continue;
                    }

                    // If we've made it this far, we can use the certificate for a recipient.
                    MailAddress originalAddress = addressesNeedingPublicKeys[canonicalCertSubject];
                    SmimeCertificateCache.Add(canonicalCertSubject, cert);
                    addressesWithPublicKeys.Add(canonicalCertSubject);
                    addressesNeedingPublicKeys.Remove(canonicalCertSubject);

                    // Shortcut to abort processing of additional certificates if all recipients are accounted for.
                    if (addressesNeedingPublicKeys.Count < 1)
                        break;
                }
            }
        }

        /// <summary>
        /// Create a byte array containing an encrypted S/MIME envelope.
        /// </summary>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="contentBytes">The contents of the envelope to be encrypted.</param>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        private byte[] SmimeEncryptEnvelope(byte[] buffer, byte[] contentBytes, MailMessage message)
        {
            // Prepare the encryption envelope.
            ContentInfo contentInfo = new ContentInfo(contentBytes);

            // Default to using the AES 256-bit alogorithm with CBC.
            EnvelopedCms envelope = new EnvelopedCms(contentInfo, new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42")));

            // Resolve recipient public keys.
            Dictionary<string, MailAddress> addressesNeedingPublicKeys;
            HashSet<string> addressesWithPublicKeys;
            ResolvePublicKeys(message, out addressesWithPublicKeys, out addressesNeedingPublicKeys);

            // Throw an error if we're unable to encrypt the message for one or more recipients.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                StringBuilder exceptionMessage = new StringBuilder();
                exceptionMessage.Append("Trying to send encrypted message to one or more recipients without a trusted public key.\r\nRecipients without public keys: ");
                foreach (string addressNeedingPublicKey in addressesNeedingPublicKeys.Keys)
                    exceptionMessage.Append(addressNeedingPublicKey + ", ");
                exceptionMessage.Remove(exceptionMessage.Length - 2, 2);

                throw new SmtpException(exceptionMessage.ToString());
            }

            // Encrypt the symmetric session key using each recipient's public key.
            foreach (string addressWithPublicKey in addressesWithPublicKeys)
            {
                CmsRecipient recipient = new CmsRecipient(SmimeCertificateCache[addressWithPublicKey]);
                envelope.Encrypt(recipient);
            }

            return envelope.Encode();
        }

        /// <summary>
        /// Helper function for sending the specified message to an SMTP server for delivery with S/MIME encoding.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        private async Task SmimeSend(MailMessage message)
        {
            // Require one or more recipients.
            if (message.To.Count + message.CC.Count + message.Bcc.Count < 1)
                throw new SmtpException("One or more recipients must be specified via the '.To', '.CC', or '.Bcc' collections.");

            // Require a signing certificate to be specified.
            if ((message.SmimeSigned || message.SmimeTripleWrapped) && message.SmimeSigningCertificate == null)
                throw new SmtpException("A signing certificate must be passed prior to signing.");

            // Ensure the rendering engine expects MIME encoding.
            message.Headers["MIME-Version"] = "1.0";

            // OpaqueMail optional setting for protecting the subject.
            // Note: This is not part of the current RFC specifcation and should only be used when sending to other OpaqueMail agents.
            if (message.SmimeEncryptedEnvelope && (message.SmimeEncryptionOptionFlags & (SmimeEncryptionOptionFlags.EncryptSubject)) > 0)
            {
                message.Headers["X-Subject-Encryption"] = "true";
                message.Body = "Subject: " + message.Subject + "\r\n" + message.Body;
                message.Subject = Guid.NewGuid().ToString();
            }

            // Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
            byte[] MIMEMessageBytes = await message.MIMEEncode(buffer, SmimeBoundaryName);

            // Helper variables for tracking the outermost content type and encoding.
            string alternateViewContentType = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeSignedCmsBoundaryName + "\"";
            TransferEncoding alternativeViewTransferEncoding = TransferEncoding.SevenBit;

            // Handle S/MIME signing.
            if (message.SmimeSigned || message.SmimeTripleWrapped)
                MIMEMessageBytes = SmimeSign(buffer, MIMEMessageBytes, message, false);

            // Handle S/MIME envelope encryption.
            if (message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped)
            {
                alternateViewContentType = "application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"";
                alternativeViewTransferEncoding = TransferEncoding.Base64;
                MIMEMessageBytes = SmimeEncryptEnvelope(buffer, MIMEMessageBytes, message);
            }

            // Handle S/MIME triple wrapping (i.e. signing, envelope encryption, then signing again).
            if (message.SmimeTripleWrapped)
            {
                alternateViewContentType = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeTripleSignedCmsBoundaryName + "\"";
                alternativeViewTransferEncoding = TransferEncoding.SevenBit;
                MIMEMessageBytes = SmimeSign(buffer, MIMEMessageBytes, message, true);
            }

            // The alternate view will deliver all content, so empty any Body that was passed in.
            message.Body = "";

            // Send our MIME encoded message using the alternate view.
            using (MemoryStream avStream = new MemoryStream(MIMEMessageBytes))
            {
                AlternateView newAlternateView = new AlternateView(avStream, alternateViewContentType);
                newAlternateView.TransferEncoding = alternativeViewTransferEncoding;
                message.AlternateViews.Add(newAlternateView);

                base.Send(message);
            }
        }

        /// <summary>
        /// Create a byte array containing a signed S/MIME message.
        /// </summary>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="contentBytes">The contents of the envelope to be encrypted.</param>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        /// <param name="alreadyEncrypted">Whether a portion of the message has previously been signed, as when triple wrapping.</param>
        private byte[] SmimeSign(byte[] buffer, byte[] contentBytes, MailMessage message, bool alreadyEncrypted)
        {
            // Prepare the signing parameters.
            ContentInfo contentInfo = new ContentInfo(contentBytes);
            SignedCms signedCms = new SignedCms(contentInfo, false);

            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, message.SmimeSigningCertificate);
            signer.IncludeOption = X509IncludeOption.WholeChain;   // TODO: provide options

            // Sign the current time.
            if ((message.SmimeSigningOptionFlags & SmimeSigningOptionFlags.SignTime) > 0)
            {
                Pkcs9SigningTime signingTime = new Pkcs9SigningTime();
                signer.SignedAttributes.Add(signingTime);
            }
            
            // Encode the signed message.
            signedCms.ComputeSignature(signer);
            byte[] encodedBytes = signedCms.Encode();

            // Embed the signed and original version of the message using MIME.
            StringBuilder messageBuilder = new StringBuilder();
            
            // If this is a signed message only, the alternate view will have the appropriate Content Type.  Otherwise, embed it.
            if ((message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped) && !alreadyEncrypted)
                messageBuilder.Append("Content-Type: multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeSignedCmsBoundaryName + "\"\r\n\r\n");

            messageBuilder.Append("This is a multi-part S/MIME signed message.\r\n\r\n");
            messageBuilder.Append("--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "\r\n");

            // If triple wrapping, the previous layer was an encrypted envelope and needs to be Base64 encoded.
            if (alreadyEncrypted)
            {
                messageBuilder.Append("Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"\r\n");
                messageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                messageBuilder.Append("Content-Description: \"S/MIME Cryptographic envelopedCms\"\r\n");
                messageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7m\"\r\n\r\n");

                messageBuilder.Append(Functions.ToBase64String(contentBytes, 0, contentBytes.Length));
            }
            else
                messageBuilder.Append(Encoding.UTF8.GetString(contentBytes));

            messageBuilder.Append("\r\n\r\n--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "\r\n");
            messageBuilder.Append("Content-Type: application/x-pkcs7-signature; name=\"smime.p7s\"\r\n");
            messageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
            messageBuilder.Append("Content-Description: \"S/MIME Cryptographic signedCms\"\r\n");
            messageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7s\"\r\n\r\n");
            messageBuilder.Append(Functions.ToBase64String(encodedBytes, 0, encodedBytes.Length));
            messageBuilder.Append("\r\n--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "--\r\n");

            return Encoding.UTF8.GetBytes(messageBuilder.ToString());
        }
        #endregion Private Methods
    }
}
