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

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpaqueMail
{
    /// <summary>
    /// Represents an email message that was either received using the ImapClient or Pop3Client classes or will be sent using the SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate handling of secure PGP and S/MIME messages.
    /// </summary>
    public partial class MailMessage
    {
        #region Public Members
        /// <summary>Whether the message is encrypted using PGP.</summary>
        public bool PgpEncryptedEnvelope
        {
            get
            {
                return pgpEncrypted;
            }
        }
        /// <summary>Whether the message is PGP signed.</summary>
        public bool PgpSigned
        {
            get
            {
                return pgpSigned;
            }
        }
        #endregion Public Members

        #region Private Members
        /// <summary>Whether the message is encrypted using PGP.</summary>
        private bool pgpEncrypted = false;
        /// <summary>Whether the message is PGP signed.</summary>
        private bool pgpSigned = false;
        #endregion Private Members

        #region Public Methods
        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="decryptedMessage">If successful, the decrypted message.</param>
        /// <returns>Whether the decryption completed successfully.</returns>
        public bool PgpDecrypt(PgpPrivateKey recipientPrivateKey, out byte[] decryptedMessage)
        {
            string encryptedBody = "";

            // Process each MIME part.
            if (MimeParts != null)
            {
                for (int i = 0; i < MimeParts.Count; i++)
                {
                    MimePart mimePart = MimeParts[i];

                    // Check if the MIME part is encrypted or signed using PGP.
                    if (mimePart.Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                    {
                        encryptedBody = Functions.ReturnBetween(mimePart.Body, "-----BEGIN PGP MESSAGE-----\r\n", "\r\n-----END PGP MESSAGE-----");
                        break;
                    }
                }
            }
            else
            {
                if (Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                    encryptedBody = Functions.ReturnBetween(Body, "-----BEGIN PGP MESSAGE-----\r\n", "\r\n-----END PGP MESSAGE-----");
            }

            // Process an encrypted body if found.
            if (!string.IsNullOrEmpty(encryptedBody))
            {
                // Ignore the PGP headers.
                int doubleLineBreak = encryptedBody.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    encryptedBody = encryptedBody.Substring(doubleLineBreak + 4);

                // Attempt to decrypt the message and set the body if successful.
                if (Pgp.Decrypt(Encoding.UTF8.GetBytes(encryptedBody), out decryptedMessage, recipientPrivateKey))
                {
                    // Ensure a valid encoding.
                    if (BodyEncoding == null)
                        BodyEncoding = Encoding.UTF8;

                    // Convert the byte array back to a string.
                    Body = BodyEncoding.GetString(decryptedMessage);

                    // If the body was successfully decrypted, attempt to decrypt attachments.
                    foreach (Attachment attachment in Attachments)
                    {
                        // Only process attachments with names ending in ".pgp".
                        if (attachment.Name.ToLower().EndsWith(".pgp"))
                        {
                            if (Pgp.Decrypt(attachment.ContentStream, out decryptedMessage, recipientPrivateKey))
                            {
                                attachment.ContentStream = new MemoryStream(decryptedMessage);
                                attachment.Name = attachment.Name.Substring(0, attachment.Name.Length - 4);
                            }
                        }
                    }

                    return true;
                }
                else
                    return false;
            }
            else
            {
                decryptedMessage = null;
                return false;
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified private key.
        /// </summary>
        /// <param name="recipientPublicKey">BouncyCastle public key to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public bool PgpEncrypt(PgpPublicKey recipientPublicKey, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes)
        {
            return PgpEncrypt(new List<PgpPublicKey>() { recipientPublicKey }, symmetricKeyAlgorithmTag);
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified private key.
        /// </summary>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public bool PgpEncrypt(IEnumerable<PgpPublicKey> recipientPublicKeys, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes)
        {
            // Ensure a valid encoding.
            if (BodyEncoding == null)
                BodyEncoding = Encoding.UTF8;

            // Attempt to encrypt.
            bool encrypted;
            using (MemoryStream encryptedMessageStream = new MemoryStream())
            {
                // Attempt to encrypt the message.
                encrypted = Pgp.Encrypt(BodyEncoding.GetBytes(Body), encryptedMessageStream, "", recipientPublicKeys, symmetricKeyAlgorithmTag, true);
                
                if (encrypted)
                {
                    rawBody = BodyEncoding.GetString(encryptedMessageStream.ToArray());
                }
            }

            // If the body was successfully encrypted, attempt to encrypt attachments.
            if (encrypted)
            {
                foreach (Attachment attachment in Attachments)
                {
                    // Don't process attachments with names ending in ".pgp".
                    if (!attachment.Name.ToLower().EndsWith(".pgp"))
                    {
                        using (MemoryStream attachmentStream = new MemoryStream())
                        {
                            encrypted = Pgp.Encrypt(attachment.ContentStream, attachmentStream, "", recipientPublicKeys);

                            if (encrypted)
                            {
                                attachment.ContentStream = attachmentStream;
                                attachment.Name += ".pgp";
                            }
                        }
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public bool PgpSign(PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256)
        {
            // Ensure a valid encoding.
            if (BodyEncoding == null)
                BodyEncoding = Encoding.UTF8;

            byte[] signatureBytes;
            if (Pgp.Sign(BodyEncoding.GetBytes(Body), out signatureBytes, senderPublicKey, senderPrivateKey, hashAlgorithmTag))
            {
                // Fix up a formatting bug in BouncyCastle.
                rawBody = Encoding.UTF8.GetString(signatureBytes).Replace("-----BEGIN PGP SIGNATURE-----", "\r\n-----BEGIN PGP SIGNATURE-----");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempt to sign then encrypt a message using PGP with the specified private and public keys.
        /// </summary>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKey">BouncyCastle public key to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public bool PgpSignAndEncrypt(PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, PgpPublicKey recipientPublicKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes)
        {
            return PgpSignAndEncrypt(senderPublicKey, senderPrivateKey, new List<PgpPublicKey>() { recipientPublicKey }, hashAlgorithmTag, symmetricKeyAlgorithmTag);
        }

        /// <summary>
        /// Attempt to sign then encrypt a message using PGP with the specified private and public keys.
        /// </summary>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public bool PgpSignAndEncrypt(PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, IEnumerable<PgpPublicKey> recipientPublicKeys, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes)
        {
            // Ensure a valid encoding.
            if (BodyEncoding == null)
                BodyEncoding = Encoding.UTF8;

            // Attempt to sign.
            bool signedAndEncrypted = false;
            using (MemoryStream signedAndEncryptedMessageStream = new MemoryStream())
            {
                // Attempt to encrypt the message.
                signedAndEncrypted = Pgp.SignAndEncrypt(BodyEncoding.GetBytes(Body), "", signedAndEncryptedMessageStream, senderPublicKey, senderPrivateKey, recipientPublicKeys, hashAlgorithmTag, symmetricKeyAlgorithmTag, true);

                if (signedAndEncrypted)
                {
                    signedAndEncrypted = true;

                    rawBody = BodyEncoding.GetString(signedAndEncryptedMessageStream.ToArray());
                }
            }

            return signedAndEncrypted;
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="senderPublicKey">BouncyCastle public key to be used for verification.</param>
        /// <returns>Whether the message's signature is verified.</returns>
        public bool PgpVerifySignature(PgpPublicKey senderPublicKey)
        {
            string pgpSignedMessage = "";
            string pgpSignature = "";

            // Process each MIME part.
            if (MimeParts != null)
            {
                int pgpSignedMessageIndex = -1;
                int pgpSignatureIndex = -1;
                for (int i = 0; i < MimeParts.Count; i++)
                {
                    MimePart mimePart = MimeParts[i];

                    // Check if the MIME part is encrypted or signed using PGP.
                    if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNED MESSAGE-----"))
                        pgpSignedMessageIndex = i;
                    else if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNATURE-----"))
                        pgpSignatureIndex = i;
                }

                // Verify PGP signatures.
                if (pgpSignedMessageIndex > -1 && pgpSignatureIndex > -1)
                {
                    pgpSignedMessage = MimeParts[pgpSignedMessageIndex].Body;
                    pgpSignature = MimeParts[pgpSignatureIndex].Body;
                }
            }

            // If the signature isn't embedded as its own MIME part, extract from the body.
            if (string.IsNullOrEmpty(pgpSignedMessage) && string.IsNullOrEmpty(pgpSignature))
            {
                pgpSignedMessage = Functions.ReturnBetween(Body, "-----BEGIN PGP SIGNED MESSAGE-----\r\n", "\r\n-----BEGIN PGP SIGNATURE-----");
                pgpSignature = Functions.ReturnBetween(Body, "-----BEGIN PGP SIGNATURE-----\r\n", "\r\n-----END PGP SIGNATURE-----");
            }

            // If a signature is embedded, attempt to verify.
            if (!string.IsNullOrEmpty(pgpSignedMessage) && !string.IsNullOrEmpty(pgpSignature))
            {
                // Skip over PGP headers.
                int doubleLineBreak = pgpSignedMessage.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    pgpSignedMessage = pgpSignedMessage.Substring(doubleLineBreak + 4);
                doubleLineBreak = pgpSignature.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    pgpSignature = pgpSignature.Substring(doubleLineBreak + 4);

                // Verify the message with its signature.
                if (!string.IsNullOrEmpty(pgpSignedMessage) && !string.IsNullOrEmpty(pgpSignature))
                {
                    bool pgpSignatureVerified = Pgp.VerifySignature(Encoding.UTF8.GetBytes(pgpSignedMessage),
                        Encoding.UTF8.GetBytes(Functions.FromBase64(pgpSignature)), senderPublicKey);

                    if (pgpSignatureVerified)
                        Body = pgpSignedMessage;

                    return pgpSignatureVerified;
                }
            }

            return false;
        }
        #endregion Public Methods
    }
}
