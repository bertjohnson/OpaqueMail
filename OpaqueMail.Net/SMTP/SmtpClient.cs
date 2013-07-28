using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Net.Sockets;
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
        /// <summary>
        /// Collection of certificates to be used when searching for recipient public keys.
        /// If not specified, SmtpClient will use the current Windows user's certificate store.
        /// </summary>
        public X509Certificate2Collection SmimeValidCertificates = null;
        #endregion Public Members

        #region Protected Members
        /// <summary>Text delimiting S/MIME message parts.</summary>
        protected string SmimeBoundaryName = "OpaqueMail-boundary";
        /// <summary>Text delimiting S/MIME message parts related to encryption.</summary>
        protected string SmimeEncryptedCmsBoundaryName = "OpaqueMail-encryption-boundary";
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        protected string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        protected string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        #endregion Protected Members

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
        public void Send(MailMessage message)
        {
            // If not performing any S/MIME encryption or signing, use the default System.Net.Mail.SmtpClient Send() method.
            if (!message.SmimeSigned && !message.SmimeEncryptedEnvelope && !message.SmimeTripleWrapped)
                base.Send(message);
            else
            {
                Task.Run(() => SmimeSend(message)).Wait();
            }
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// Performs requested S/MIME signing and encryption.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public async Task SendAsync(MailMessage message)
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
                    // Load from the current user.
                    X509Store store = new X509Store(StoreLocation.CurrentUser);
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                    SmimeValidCertificates = store.Certificates;
                    store.Close();

                    // Add any tied to the local machine.
                    store = new X509Store(StoreLocation.LocalMachine);
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                    SmimeValidCertificates.AddRange(store.Certificates);
                    store.Close();
                }

                // Loop through certificates and check for matching recipients.
                /// TODO: Allow for users to choose when there are multiple certificate options and/or define default criteria.
                foreach (X509Certificate2 cert in SmimeValidCertificates)
                {
                    // Look at certificates with e-mail subject names.
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
                    else if (cert.Subject.StartsWith("SN="))
                    {
                        canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
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
        /// Sends the specified message to an SMTP server for delivery without making modifications to the body.
        /// Necessary because the standard SmtpClient.Send() may slightly alter messages, invalidating signatures.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        private async Task SmimeSendRaw(MailMessage message)
        {
            // Connect to the SMTP server.
            TcpClient SmtpTcpClient = new TcpClient();
            SmtpTcpClient.Connect(Host, Port);
            Stream SmtpStream = SmtpTcpClient.GetStream();

            // Read the welcome message.
            string response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);

            // Send EHLO and find out server capabilities.
            await Functions.SendStreamStringAsync(SmtpStream, buffer, "EHLO " + Host + "\r\n");
            response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to connect to remote server '" + Host + "'.  Sent 'EHLO' and received '" + response + "'.");

            // Stand up a TLS/SSL stream.
            if (EnableSsl)
            {
                await Functions.SendStreamStringAsync(SmtpStream, buffer, "STARTTLS\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to start TLS/SSL protection with '" + Host + "'.  Received '" + response + "'.");

                SmtpStream = new SslStream(SmtpStream);

                if (!((SslStream)SmtpStream).IsAuthenticated)
                    ((SslStream)SmtpStream).AuthenticateAsClient(Host);
            }
    
            // Authenticate using the AUTH LOGIN command.
            if (Credentials != null)
            {
                NetworkCredential cred = (NetworkCredential)Credentials;
                await Functions.SendStreamStringAsync(SmtpStream, buffer, "AUTH LOGIN\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("3"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
                await Functions.SendStreamStringAsync(SmtpStream, buffer, Functions.ToBase64String(cred.UserName) + "\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                await Functions.SendStreamStringAsync(SmtpStream, buffer, Functions.ToBase64String(cred.Password) + "\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
            }

            // Build our raw headers block.
            StringBuilder rawHeaders = new StringBuilder();

            // Specify who the message is from.
            rawHeaders.Append("From: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.From)) + "\r\n");
            await Functions.SendStreamStringAsync(SmtpStream, buffer, "MAIL FROM:<" + message.From.Address + ">\r\n");
            response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");

            // Identify all recipients of the message.
            rawHeaders.Append("To: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.To)) + "\r\n");
            foreach (MailAddress address in message.To)
            {
                await Functions.SendStreamStringAsync(SmtpStream, buffer, "RCPT TO:<" + address.Address + ">\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            rawHeaders.Append("CC: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.CC)) + "\r\n");
            foreach (MailAddress address in message.CC)
            {
                await Functions.SendStreamStringAsync(SmtpStream, buffer, "RCPT TO:<" + address.Address + ">\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");
            }

            foreach (MailAddress address in message.Bcc)
            {
                await Functions.SendStreamStringAsync(SmtpStream, buffer, "RCPT TO:<" + address.Address + ">\r\n");
                response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");
            }

            // Send the raw message.
            await Functions.SendStreamStringAsync(SmtpStream, buffer, "DATA\r\n");
            response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
            if (!response.StartsWith("3"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'DATA' and received '" + response + "'.");

            rawHeaders.Append("Subject: " + Functions.EncodeMailHeader(message.Subject) + "\r\n");
            foreach (string rawHeader in message.Headers)
            {
                switch (rawHeader.ToUpper())
                {
                    case "BCC":
                    case "CC":
                    case "FROM":
                    case "SUBJECT":
                    case "TO":
                        break;
                    default:
                        rawHeaders.Append(rawHeader + ": " + message.Headers[rawHeader] + "\r\n");
                        break;
                }
            }

            await Functions.SendStreamStringAsync(SmtpStream, buffer, rawHeaders.ToString() + "\r\n" + message.Body + "\r\n.\r\n");

            response = await Functions.ReadStreamStringAsync(SmtpStream, buffer);
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent message and received '" + response + "'.");

            // Clean up this connection.
            await Functions.SendStreamStringAsync(SmtpStream, buffer, "QUIT\r\n");
            SmtpStream.Dispose();
            SmtpTcpClient.Close();
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

            // Throw an error if we're unable to encrypt the message for one or more recipients and encryption is explicitly required.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                // If the implementation requires S/MIME encryption (the default), throw an error if there's no certificate.
                if ((message.SmimeSettingsMode & SmimeSettingsMode.RequireExactSettings) > 0)
                {
                    StringBuilder exceptionMessage = new StringBuilder();
                    exceptionMessage.Append("Trying to send encrypted message to one or more recipients without a trusted public key.\r\nRecipients without public keys: ");
                    foreach (string addressNeedingPublicKey in addressesNeedingPublicKeys.Keys)
                        exceptionMessage.Append(addressNeedingPublicKey + ", ");
                    exceptionMessage.Remove(exceptionMessage.Length - 2, 2);

                    throw new SmtpException(exceptionMessage.ToString());
                }
                else
                    return contentBytes;
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
            if ((message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped) && (message.SmimeEncryptionOptionFlags & (SmimeEncryptionOptionFlags.EncryptSubject)) > 0)
            {
                message.Headers["X-Subject-Encryption"] = "true";
                message.Body = "Subject: " + message.Subject + "\r\n" + message.Body;
                message.Subject = Guid.NewGuid().ToString();
            }

            // Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
            byte[] MIMEMessageBytes = await message.MIMEEncode(buffer, SmimeBoundaryName);

            // Handle S/MIME signing.
            bool successfullySigned = false;
            if (message.SmimeSigned || message.SmimeTripleWrapped)
            {
                int unsignedSize = MIMEMessageBytes.Length;
                MIMEMessageBytes = SmimeSign(buffer, MIMEMessageBytes, message, false);
                successfullySigned = MIMEMessageBytes.Length != unsignedSize;

                if (successfullySigned)
                {
                    message.Headers["Content-Type"] = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeSignedCmsBoundaryName + "\"";
                    message.Headers["Content-Transfer-Encoding"] = "7bit";

                    if (!message.SmimeTripleWrapped)
                        message.Body = Encoding.UTF8.GetString(MIMEMessageBytes);
                }
            }

            // Handle S/MIME envelope encryption.
            bool successfullyEncrypted = false;
            if (message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped)
            {
                int unencryptedSize = MIMEMessageBytes.Length;
                MIMEMessageBytes = SmimeEncryptEnvelope(buffer, MIMEMessageBytes, message);
                successfullyEncrypted = MIMEMessageBytes.Length != unencryptedSize;

                // If the message won't be triple-wrapped, wrap the encrypted message with MIME.
                if (successfullyEncrypted && !message.SmimeTripleWrapped)
                {
                    message.Headers["Content-Type"] = "application/pkcs7-mime; name=smime.p7m; smime-type=enveloped-data";
                    message.Headers["Content-Transfer-Encoding"] = "base64";
                    message.Headers["Content-Disposition"] = "attachment; filename=smime.p7m";

                    message.Body = Functions.ToBase64String(MIMEMessageBytes) + "\r\n";
                }
            }

            // Handle S/MIME triple wrapping (i.e. signing, envelope encryption, then signing again).
            if (successfullyEncrypted && message.SmimeTripleWrapped)
            {
                message.Headers["Content-Type"] = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeTripleSignedCmsBoundaryName + "\"";
                message.Headers["Content-Transfer-Encoding"] = "7bit";

                message.Body = Encoding.UTF8.GetString(SmimeSign(buffer, MIMEMessageBytes, message, true));
            }

            // Only repackage the message if it's either been signed or encrypted.
            if (successfullySigned || successfullyEncrypted)
                await SmimeSendRaw(message);
            else
                base.Send(message);
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
            if (message.SmimeSigningCertificate == null)
            {
                // If the implementation requires S/MIME signing (the default), throw an error if there's no certificate.
                if ((message.SmimeSettingsMode & SmimeSettingsMode.RequireExactSettings) > 0)
                    throw new SmtpException("Trying to send a signed message, but no signing certificate has been assigned.");
                else
                    return contentBytes;
            }

            // First, create a buffer for tracking the unsigned portion of this message.
            StringBuilder unsignedMessageBuilder = new StringBuilder();

            // If triple wrapping, the previous layer was an encrypted envelope and needs to be Base64 encoded.
            if (alreadyEncrypted)
            {
                unsignedMessageBuilder.Append("Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"\r\n");
                unsignedMessageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                unsignedMessageBuilder.Append("Content-Description: \"S/MIME Cryptographic envelopedCms\"\r\n");
                unsignedMessageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7m\"\r\n\r\n");

                unsignedMessageBuilder.Append(Functions.ToBase64String(contentBytes));
            }
            else
                unsignedMessageBuilder.Append(Encoding.UTF8.GetString(contentBytes));

            // Prepare the signing parameters.
            ContentInfo contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(unsignedMessageBuilder.ToString()));
            SignedCms signedCms = new SignedCms(contentInfo, true);

            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, message.SmimeSigningCertificate);
            signer.IncludeOption = X509IncludeOption.EndCertOnly;

            // Sign the current time.
            if ((message.SmimeSigningOptionFlags & SmimeSigningOptionFlags.SignTime) > 0)
            {
                Pkcs9SigningTime signingTime = new Pkcs9SigningTime();
                signer.SignedAttributes.Add(signingTime);
            }
            
            // Encode the signed message.
            signedCms.ComputeSignature(signer);
            byte[] signedBytes = signedCms.Encode();

            // Embed the signed and original version of the message using MIME.
            StringBuilder messageBuilder = new StringBuilder();

            // If this is a signed message only, the alternate view will have the appropriate Content Type.  Otherwise, embed it.
            if ((message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped) && !alreadyEncrypted)
                messageBuilder.Append("Content-Type: multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1; boundary=\"" + SmimeSignedCmsBoundaryName + "\"\r\n\r\n");

            // Build the MIME message by embedding the unsigned and signed portions.
            messageBuilder.Append("This is a multi-part S/MIME signed message.\r\n\r\n");
            messageBuilder.Append("--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "\r\n");
            messageBuilder.Append(unsignedMessageBuilder.ToString());
            messageBuilder.Append("\r\n--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "\r\n");
            messageBuilder.Append("Content-Type: application/x-pkcs7-signature; smime-type=signed-data; name=\"smime.p7s\"\r\n");
            messageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
            messageBuilder.Append("Content-Description: \"S/MIME Cryptographic signedCms\"\r\n");
            messageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7s\"\r\n\r\n");
            messageBuilder.Append(Functions.ToBase64String(signedBytes, 0, signedBytes.Length));
            messageBuilder.Append("\r\n--" + (alreadyEncrypted ? SmimeTripleSignedCmsBoundaryName : SmimeSignedCmsBoundaryName) + "--\r\n");

            return Encoding.UTF8.GetBytes(messageBuilder.ToString());
        }
        #endregion Private Methods
    }
}
