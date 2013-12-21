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

namespace OpaqueMail.Net
{
    /// <summary>
    /// Allows applications to send e-mail by using the Simple Mail Transport Protocol (SMTP).
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public class SmtpClient : System.Net.Mail.SmtpClient
    {
        #region Public Members
        /// <summary>
        /// OID representing the encryption algorthim(s) to be used.
        /// </summary>
        /// <remarks>Defaults to using the AES 256-bit algorithm with CBC.</remarks>
        public AlgorithmIdentifier SmimeAlgorithmIdentifier = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"));
        /// <summary>
        /// Collection of certificates to be used when searching for recipient public keys.
        /// If not specified, SmtpClient will use the current Windows user's certificate store.
        /// </summary>
        public X509Certificate2Collection SmimeValidCertificates = null;
        #endregion Public Members

        #region Protected Members
        /// <summary>Text delimiting S/MIME alternative view message parts.</summary>
        protected string SmimeAlternativeViewBoundaryName = "OpaqueMail-alternative-boundary";
        /// <summary>Text delimiting S/MIME message parts.</summary>
        protected string SmimeBoundaryName = "OpaqueMail-boundary";
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        protected string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        protected string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        #endregion Protected Members

        #region Private Members
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] buffer = new byte[Constants.HUGEBUFFERSIZE];
        /// <summary>Cache of recipient public keys to speed up subsequent usage of this SmtpClient.</summary>
        private Dictionary<string, X509Certificate2> SmimeCertificateCache = new Dictionary<string, X509Certificate2>();
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class by using configuration file settings.
        /// </summary>
        public SmtpClient()
            : base()
        {
            RandomizeBoundaryNames();
        }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends e-mail by using the specified SMTP server.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        public SmtpClient(string host)
            : base(host)
        {
            RandomizeBoundaryNames();
        }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends e-mail by using the specified SMTP server and port.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        /// <param name="port">Port to be used by the host.</param>
        public SmtpClient(string host, int port)
            : base(host, port)
        {
            RandomizeBoundaryNames();
        }
        #endregion Constructors

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
            {
                // If this is a read-only message, it was forwarded by the Proxy and should be sent raw.
                if (message is ReadOnlyMailMessage)
                    Task.Run(() => SmimeSendRawAsync(message)).Wait();
                else
                    base.Send(message);
            }
            else
            {
                Task.Run(() => SmimeSendAsync(message)).Wait();
            }
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// Performs requested S/MIME signing and encryption.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public async Task SendAsync(MailMessage message)
        {
            // If this is a read-only message, it was forwarded by the Proxy and should be sent raw.
            if (message is ReadOnlyMailMessage)
                await SmimeSendRawAsync(message);
            else
                await SmimeSendAsync(message);
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
        /// Ensure boundary names are unique.
        /// </summary>
        private void RandomizeBoundaryNames()
        {
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

            SmimeAlternativeViewBoundaryName += "-" + boundaryRandomness;
            SmimeBoundaryName += "-" + boundaryRandomness;
            SmimeSignedCmsBoundaryName += "-" + boundaryRandomness;
            SmimeTripleSignedCmsBoundaryName += "-" + boundaryRandomness;
        }

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

            MailAddressCollection[] addressRanges = new MailAddressCollection[] { message.To, message.CC, message.Bcc };
            foreach (MailAddressCollection addressRange in addressRanges)
            {
                foreach (MailAddress toAddress in addressRange)
                {
                    string canonicalToAddress = toAddress.Address.ToUpper();
                    if (SmimeCertificateCache.ContainsKey(canonicalToAddress))
                    {
                        if (!addressesWithPublicKeys.Contains(canonicalToAddress))
                            addressesWithPublicKeys.Add(canonicalToAddress);
                    }
                    else
                    {
                        if (!addressesNeedingPublicKeys.ContainsKey(canonicalToAddress))
                            addressesNeedingPublicKeys.Add(canonicalToAddress, toAddress);
                    }
                }
            }

            // If any addresses haven't been mapped to public keys, map them.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                // Read from the Windows certificate store if valid certificates aren't specified.
                if (SmimeValidCertificates == null || SmimeValidCertificates.Count < 1)
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
                foreach (X509Certificate2 cert in SmimeValidCertificates)
                {
                    // Look at certificates with e-mail subject names.
                    string canonicalCertSubject = "";
                    if (cert.Subject.StartsWith("E="))
                        canonicalCertSubject = cert.Subject.Substring(2).ToUpper();
                    else if (cert.Subject.StartsWith("CN="))
                        canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
                    else
                        canonicalCertSubject = cert.Subject.ToUpper();

                    int certSubjectComma = canonicalCertSubject.IndexOf(",");
                    if (certSubjectComma > -1)
                        canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                    // Only proceed if the key is for a recipient of this e-mail.
                    if (!addressesNeedingPublicKeys.ContainsKey(canonicalCertSubject))
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
        private async Task SmimeSendRawAsync(MailMessage message)
        {
            // Connect to the SMTP server.
            TcpClient SmtpTcpClient = new TcpClient();
            SmtpTcpClient.Connect(Host, Port);
            Stream SmtpStream = SmtpTcpClient.GetStream();

            // Use stream readers and writers to simplify I/O.
            StreamReader reader = new StreamReader(SmtpStream);
            StreamWriter writer = new StreamWriter(SmtpStream);
            writer.AutoFlush = true;

            // Read the welcome message.
            string response = await reader.ReadLineAsync();

            // Send EHLO and find out server capabilities.
            await writer.WriteLineAsync("EHLO " + Host);
            char[] charBuffer = new char[Constants.SMALLBUFFERSIZE];
            int bytesRead = await reader.ReadAsync(charBuffer, 0, Constants.SMALLBUFFERSIZE);
            response = new string(charBuffer, 0, bytesRead);
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to connect to remote server '" + Host + "'.  Sent 'EHLO' and received '" + response + "'.");

            // Stand up a TLS/SSL stream.
            if (EnableSsl)
            {
                await writer.WriteLineAsync("STARTTLS");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to start TLS/SSL protection with '" + Host + "'.  Received '" + response + "'.");

                SmtpStream = new SslStream(SmtpStream);
                ((SslStream)SmtpStream).AuthenticateAsClient(Host);

                reader = new StreamReader(SmtpStream);
                writer = new StreamWriter(SmtpStream);
                writer.AutoFlush = true;
            }

            // Authenticate using the AUTH LOGIN command.
            if (Credentials != null)
            {
                NetworkCredential cred = (NetworkCredential)Credentials;
                await writer.WriteLineAsync("AUTH LOGIN");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("3"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
                await writer.WriteLineAsync(Functions.ToBase64String(cred.UserName));
                response = await reader.ReadLineAsync();
                await writer.WriteLineAsync(Functions.ToBase64String(cred.Password));
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
            }

            // Build our raw headers block.
            StringBuilder rawHeaders = new StringBuilder(Constants.SMALLSBSIZE);

            // Specify who the message is from.
            rawHeaders.Append(Functions.SpanHeaderLines("From: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.From))) + "\r\n");
            await writer.WriteLineAsync("MAIL FROM:<" + message.From.Address + ">");
            response = await reader.ReadLineAsync();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");

            // Identify all recipients of the message.
            if (message.To.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("To: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.To))) + "\r\n");
            foreach (MailAddress address in message.To)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            if (message.CC.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("CC: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.CC))) + "\r\n");
            foreach (MailAddress address in message.CC)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            foreach (MailAddress address in message.Bcc)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            // Send the raw message.
            await writer.WriteLineAsync("DATA");
            response = await reader.ReadLineAsync();
            if (!response.StartsWith("3"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'DATA' and received '" + response + "'.");

            // If a read-only mail message is passed in with its raw headers and body, save a few steps by sending that directly.
            if (message is ReadOnlyMailMessage)
                await writer.WriteAsync(((ReadOnlyMailMessage)message).RawHeaders + "\r\n" + ((ReadOnlyMailMessage)message).RawBody + "\r\n.\r\n");
            else
            {
                rawHeaders.Append(Functions.SpanHeaderLines("Subject: " + Functions.EncodeMailHeader(message.Subject)) + "\r\n");
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
                            rawHeaders.Append(Functions.SpanHeaderLines(rawHeader + ": " + message.Headers[rawHeader]) + "\r\n");
                            break;
                    }
                }

                await writer.WriteAsync(rawHeaders.ToString() + "\r\n" + message.Body + "\r\n.\r\n");
            }

            response = await reader.ReadLineAsync();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent message and received '" + response + "'.");

            // Clean up this connection.
            await writer.WriteLineAsync("QUIT");

            writer.Dispose();
            reader.Dispose();

            SmtpStream.Dispose();
            SmtpTcpClient.Close();
        }

        /// <summary>
        /// Create a byte array containing an encrypted S/MIME envelope.
        /// </summary>
        /// <param name="contentBytes">The contents of the envelope to be encrypted.</param>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        private byte[] SmimeEncryptEnvelope(byte[] contentBytes, MailMessage message, bool alreadySigned)
        {
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
                    StringBuilder exceptionMessage = new StringBuilder(Constants.TINYSBSIZE);
                    exceptionMessage.Append("Trying to send encrypted message to one or more recipients without a trusted public key.\r\nRecipients without public keys: ");
                    foreach (string addressNeedingPublicKey in addressesNeedingPublicKeys.Keys)
                        exceptionMessage.Append(addressNeedingPublicKey + ", ");
                    exceptionMessage.Remove(exceptionMessage.Length - 2, 2);

                    throw new SmtpException(exceptionMessage.ToString());
                }
                else
                    return contentBytes;
            }

            if (alreadySigned)
            {
                // If already signed, prepend S/MIME headers.
                StringBuilder contentBuilder = new StringBuilder(Constants.TINYSBSIZE);
                contentBuilder.Append("Content-Type: multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + SmimeSignedCmsBoundaryName + "\"\r\n");
                contentBuilder.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");

                contentBytes = Encoding.UTF8.GetBytes(contentBuilder.ToString() + Encoding.UTF8.GetString(contentBytes));
            }

            // Prepare the encryption envelope.
            ContentInfo contentInfo = new ContentInfo(contentBytes);
            EnvelopedCms envelope;

            // If a specific algorithm is specified, choose that.  Otherwise, negotiate which algorithm to use.
            if (SmimeAlgorithmIdentifier != null)
                envelope = new EnvelopedCms(contentInfo, SmimeAlgorithmIdentifier);
            else
                envelope = new EnvelopedCms(contentInfo);

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
        private async Task SmimeSendAsync(MailMessage message)
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
            byte[] MIMEMessageBytes = await message.MIMEEncode(SmimeBoundaryName, SmimeAlternativeViewBoundaryName);
            message.Headers["Content-Type"] = "multipart/mixed; boundary=\"" + SmimeBoundaryName + "\"";
            message.Headers["Content-Transfer-Encoding"] = "7bit";

            // Skip the MIME header.
            message.Body = Encoding.UTF8.GetString(MIMEMessageBytes);
            message.Body = message.Body.Substring(message.Body.IndexOf("\r\n\r\n") + 4);

            // Handle S/MIME signing.
            bool successfullySigned = false;
            if (message.SmimeSigned || message.SmimeTripleWrapped)
            {
                int unsignedSize = MIMEMessageBytes.Length;
                MIMEMessageBytes = SmimeSign(buffer, MIMEMessageBytes, message, false);
                successfullySigned = MIMEMessageBytes.Length != unsignedSize;

                if (successfullySigned)
                {
                    // Remove any prior content dispositions.
                    if (message.Headers["Content-Disposition"] != null)
                        message.Headers.Remove("Content-Disposition");

                    message.Headers["Content-Type"] = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + SmimeSignedCmsBoundaryName + "\"";
                    message.Headers["Content-Transfer-Encoding"] = "7bit";
                    message.Body = Encoding.UTF8.GetString(MIMEMessageBytes);
                }
            }

            // Handle S/MIME envelope encryption.
            bool successfullyEncrypted = false;
            if (message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped)
            {
                int unencryptedSize = MIMEMessageBytes.Length;
                MIMEMessageBytes = SmimeEncryptEnvelope(MIMEMessageBytes, message, successfullySigned);
                successfullyEncrypted = MIMEMessageBytes.Length != unencryptedSize;

                // If the message won't be triple-wrapped, wrap the encrypted message with MIME.
                if (successfullyEncrypted && (!successfullySigned || !message.SmimeTripleWrapped))
                {
                    message.Headers["Content-Type"] = "application/pkcs7-mime; name=smime.p7m;\r\n\tsmime-type=enveloped-data";
                    message.Headers["Content-Transfer-Encoding"] = "base64";

                    message.Body = Functions.ToBase64String(MIMEMessageBytes) + "\r\n";
                }
            }

            // Handle S/MIME triple wrapping (i.e. signing, envelope encryption, then signing again).
            if (successfullyEncrypted)
            {
                if (message.SmimeTripleWrapped)
                {
                    message.Headers["Content-Type"] = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + SmimeTripleSignedCmsBoundaryName + "\"";
                    message.Headers["Content-Transfer-Encoding"] = "7bit";

                    message.Body = Encoding.UTF8.GetString(SmimeSign(buffer, MIMEMessageBytes, message, true));
                }
                else
                    message.Headers["Content-Disposition"] = "attachment; filename=smime.p7m";
            }

            await SmimeSendRawAsync(message);
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
            StringBuilder unsignedMessageBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            // If triple wrapping, the previous layer was an encrypted envelope and needs to be Base64 encoded.
            if (alreadyEncrypted)
            {
                unsignedMessageBuilder.Append("Content-Type: application/pkcs7-mime; smime-type=enveloped-data;\r\n\tname=\"smime.p7m\"\r\n");
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

            CmsSigner signer = new CmsSigner(message.SubjectIdentifierType, message.SmimeSigningCertificate);
            signer.IncludeOption = X509IncludeOption.WholeChain;

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
            StringBuilder messageBuilder = new StringBuilder(Constants.SMALLSBSIZE);

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
