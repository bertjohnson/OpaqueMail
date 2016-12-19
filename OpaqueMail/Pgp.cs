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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace OpaqueMail
{
    /// <summary>
    /// Provides supporting functions for Pretty Good Privacy (PGP).
    /// </summary>
    public static class Pgp
    {
        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="message">Byte array containing the message to decrypt.</param>
        /// <param name="decryptedMessage">If successful, the decrypted message.</param>
        /// <param name="recipientPrivateKey">The BouncyCastle private key to be used for decryption.</param>
        /// <remarks>The message should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the decryption completed successfully.</returns>
        public static bool Decrypt(byte[] message, out byte[] decryptedMessage, PgpPrivateKey recipientPrivateKey)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Decrypt(messageStream, out decryptedMessage, recipientPrivateKey);
            }
        }

        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="message">Byte array containing the message to decrypt.</param>
        /// <param name="decryptedMessageStream">Stream to write the decrypted message into.</param>
        /// <param name="recipientPrivateKey">The BouncyCastle private key to be used for decryption.</param>
        /// <remarks>The message should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the decryption completed successfully.</returns>
        public static bool Decrypt(byte[] message, Stream decryptedMessageStream, PgpPrivateKey recipientPrivateKey)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Decrypt(messageStream, decryptedMessageStream, recipientPrivateKey);
            }
        }

        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="messageStream">Stream containing the message to decrypt.</param>
        /// <param name="decryptedMessage">If successful, the decrypted message.</param>
        /// <param name="recipientPrivateKey">The BouncyCastle private key to be used for decryption.</param>
        /// <remarks>The message should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the decryption completed successfully.</returns>
        public static bool Decrypt(Stream messageStream, out byte[] decryptedMessage, PgpPrivateKey recipientPrivateKey)
        {
            using (MemoryStream decryptedMessageStream = new MemoryStream())
            {
                bool decrypted = Decrypt(messageStream, decryptedMessageStream, recipientPrivateKey);
                if (decrypted)
                    decryptedMessage = decryptedMessageStream.ToArray();
                else
                    decryptedMessage = null;

                return decrypted;
            }
        }

        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="messageStream">Stream containing the message to decrypt.</param>
        /// <param name="decryptedMessageStream">Stream to write the decrypted message into.</param>
        /// <param name="recipientPrivateKey">The BouncyCastle private key to be used for decryption.</param>
        /// <remarks>The message should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the decryption completed successfully.</returns>
        public static bool Decrypt(Stream messageStream, Stream decryptedMessageStream, PgpPrivateKey recipientPrivateKey)
        {
            // Decode from Base-64.
            using (Stream decoderStream = PgpUtilities.GetDecoderStream(messageStream))
            {
                // Extract the encrypted data list.
                PgpObjectFactory pgpObjectFactory = new PgpObjectFactory(decoderStream);
                PgpObject pgpObject = pgpObjectFactory.NextPgpObject();
                while (!(pgpObject is PgpEncryptedDataList))
                {
                    pgpObject = pgpObjectFactory.NextPgpObject();
                    if (pgpObject == null)
                        return false;
                }
                PgpEncryptedDataList pgpEncryptedDataList = pgpObject as PgpEncryptedDataList;

                // Attempt to extract the encrypted data stream.
                Stream decryptedStream = null;
                foreach (PgpPublicKeyEncryptedData pgpEncryptedData in pgpEncryptedDataList.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>()){
                    if (pgpEncryptedData.KeyId == recipientPrivateKey.KeyId)
                        decryptedStream = pgpEncryptedData.GetDataStream(recipientPrivateKey);
                }

                // If we're unable to decrypt any of the streams, fail.
                if (decryptedStream == null)
                    return false;

                PgpObjectFactory clearPgpObjectFactory = new PgpObjectFactory(decryptedStream);
                PgpObject message = clearPgpObjectFactory.NextPgpObject();

                // Deal with compression.
                if (message is PgpCompressedData)
                {
                    PgpCompressedData compressedMessage = (PgpCompressedData)message;
                    using (Stream compressedDataStream = compressedMessage.GetDataStream())
                    {
                        PgpObjectFactory compressedPgpObjectFactory = new PgpObjectFactory(compressedDataStream);

                        pgpObject = compressedPgpObjectFactory.NextPgpObject();
                        while (!(pgpObject is PgpLiteralData))
                        {
                            pgpObject = compressedPgpObjectFactory.NextPgpObject();
                            if (pgpObject == null)
                                return false;
                        }
                    }
                }
                else if (message is PgpLiteralData)
                {
                    pgpObject = message;
                }
                else
                {
                    // If not compressed and the following object isn't literal data, fail.
                    decryptedStream.Dispose();
                    return false;
                }

                // If a literal data stream was found, extract the decrypted message.
                PgpLiteralData literalData = pgpObject as PgpLiteralData;
                if (literalData != null)
                {
                    using (Stream literalDataStream = literalData.GetDataStream())
                    {
                        literalDataStream.CopyTo(decryptedMessageStream);
                    }

                    decryptedStream.Dispose();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="encryptedMessage">If successful, the encrypted message.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKey">BouncyCastle public key to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(byte[] message, out byte[] encryptedMessage, string fileName, PgpPublicKey recipientPublicKey, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            return Encrypt(message, out encryptedMessage, fileName, new List<PgpPublicKey>() { recipientPublicKey }, symmetricKeyAlgorithmTag, armor);
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="encryptedMessage">If successful, the encrypted message.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(byte[] message, out byte[] encryptedMessage, string fileName, IEnumerable<PgpPublicKey> recipientPublicKeys, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Encrypt(messageStream, out encryptedMessage, fileName, recipientPublicKeys, symmetricKeyAlgorithmTag, armor);
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="encryptedMessageStream">Stream to write the encrypted message into.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKey">BouncyCastle public key to be used for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(byte[] message, Stream encryptedMessageStream, string fileName, PgpPublicKey recipientPublicKey, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            return Encrypt(message, encryptedMessageStream, fileName, new List<PgpPublicKey>() { recipientPublicKey }, symmetricKeyAlgorithmTag, armor);
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="encryptedMessageStream">Stream to write the encrypted message into.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(byte[] message, Stream encryptedMessageStream, string fileName, IEnumerable<PgpPublicKey> recipientPublicKeys, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Encrypt(messageStream, encryptedMessageStream, fileName, recipientPublicKeys, symmetricKeyAlgorithmTag, armor);
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="encryptedMessage">If successful, the encrypted message.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKey">BouncyCastle public key to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(Stream messageStream, out byte[] encryptedMessage, string fileName, PgpPublicKey recipientPublicKey, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            return Encrypt(messageStream, out encryptedMessage, fileName, new List<PgpPublicKey>() { recipientPublicKey }, symmetricKeyAlgorithmTag, armor);
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="encryptedMessage">If successful, the encrypted message.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(Stream messageStream, out byte[] encryptedMessage, string fileName, IEnumerable<PgpPublicKey> recipientPublicKeys, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream encryptedMessageStream = new MemoryStream())
            {
                bool encrypted = Encrypt(messageStream, encryptedMessageStream, fileName, recipientPublicKeys, symmetricKeyAlgorithmTag, armor);
                if (encrypted)
                    encryptedMessage = encryptedMessageStream.ToArray();
                else
                    encryptedMessage = null;

                return encrypted;
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="encryptedMessageStream">Stream to write the encrypted message into.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKey">BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(Stream messageStream, Stream encryptedMessageStream, string fileName, PgpPublicKey recipientPublicKey, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            return Encrypt(messageStream, encryptedMessageStream, fileName, new List<PgpPublicKey>() { recipientPublicKey }, symmetricKeyAlgorithmTag, armor);
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="encryptedMessageStream">Stream to write the encrypted message into.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool Encrypt(Stream messageStream, Stream encryptedMessageStream, string fileName, IEnumerable<PgpPublicKey> recipientPublicKeys, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            // Allow any of the corresponding keys to be used for decryption.
            PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(symmetricKeyAlgorithmTag, true, new SecureRandom());
            foreach (PgpPublicKey publicKey in recipientPublicKeys)
            {
                encryptedDataGenerator.AddMethod(publicKey);
            }

            // Handle optional ASCII armor.
            if (armor)
            {
                using (Stream armoredStream = new ArmoredOutputStream(encryptedMessageStream))
                {
                    using (Stream encryptedStream = encryptedDataGenerator.Open(armoredStream, new byte[Constants.LARGEBUFFERSIZE]))
                    {
                        PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
                        using (Stream compressedStream = compressedDataGenerator.Open(encryptedStream))
                        {
                            PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
                            using (Stream literalDataStream = literalDataGenerator.Open(encryptedStream, PgpLiteralData.Binary, fileName, DateTime.Now, new byte[Constants.LARGEBUFFERSIZE]))
                            {
                                messageStream.Seek(0, SeekOrigin.Begin);
                                messageStream.CopyTo(literalDataStream);
                            }
                        }

                    }
                }
            }
            else
            {
                using (Stream encryptedStream = encryptedDataGenerator.Open(encryptedMessageStream, new byte[Constants.LARGEBUFFERSIZE]))
                {
                    PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
                    using (Stream compressedStream = compressedDataGenerator.Open(encryptedStream))
                    {
                        PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
                        using (Stream literalDataStream = literalDataGenerator.Open(encryptedStream, PgpLiteralData.Binary, fileName, DateTime.Now, new byte[Constants.LARGEBUFFERSIZE]))
                        {
                            messageStream.Seek(0, SeekOrigin.Begin);
                            messageStream.CopyTo(literalDataStream);
                        }
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="message">Byte array containing the message to sign.</param>
        /// <param name="signature">If successful, the signature.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public static bool Sign(byte[] message, out byte[] signature, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Sign(messageStream, out signature, senderPublicKey, senderPrivateKey, hashAlgorithmTag, armor);
            }
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="message">Byte array containing the message to sign.</param>
        /// <param name="signatureStream">Stream to write the signature into.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public static bool Sign(byte[] message, Stream signatureStream, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return Sign(messageStream, signatureStream, senderPublicKey, senderPrivateKey, hashAlgorithmTag, armor);
            }
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="messageStream">Stream containing the message to sign.</param>
        /// <param name="signature">If successful, the signature.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public static bool Sign(Stream messageStream, out byte[] signature, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, bool armor = true)
        {
            using (MemoryStream signatureStream = new MemoryStream())
            {
                bool signed = Sign(messageStream, signatureStream, senderPublicKey, senderPrivateKey, hashAlgorithmTag, armor);
                if (signed)
                    signature = signatureStream.ToArray();
                else
                    signature = null;

                return signed;
            }
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="messageStream">Stream containing the message to sign.</param>
        /// <param name="signatureStream">Stream to write the signature into.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public static bool Sign(Stream messageStream, Stream signatureStream, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, bool armor = true)
        {
            // Create a signature generator.
            PgpSignatureGenerator signatureGenerator = new PgpSignatureGenerator(senderPublicKey.Algorithm, hashAlgorithmTag);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, senderPrivateKey);

            // Add the public key user ID.
            foreach (string userId in senderPublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator signatureSubGenerator = new PgpSignatureSubpacketGenerator();
                signatureSubGenerator.SetSignerUserId(false, userId);
                signatureGenerator.SetHashedSubpackets(signatureSubGenerator.Generate());
                break;
            }

            // Handle ASCII armor.
            if (armor)
            {
                using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(signatureStream))
                {
                    armoredStream.BeginClearText(hashAlgorithmTag);

                    // Process each character in the message.
                    int messageChar;
                    while ((messageChar = messageStream.ReadByte()) >= 0)
                    {
                        armoredStream.WriteByte((byte)messageChar);
                        signatureGenerator.Update((byte)messageChar);
                    }

                    armoredStream.EndClearText();

                    using (BcpgOutputStream bcpgStream = new BcpgOutputStream(armoredStream))
                    {
                        signatureGenerator.Generate().Encode(bcpgStream);
                    }
                }
            }
            else
            {
                // Process each character in the message.
                int messageChar;
                while ((messageChar = messageStream.ReadByte()) >= 0)
                {
                    signatureGenerator.Update((byte)messageChar);
                }

                signatureGenerator.Generate().Encode(signatureStream);
            }

            return true;
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="signedAndEncryptedMessage">If successful, the encrypted message.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool SignAndEncrypt(byte[] message, string fileName, out byte[] signedAndEncryptedMessage, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, IEnumerable<PgpPublicKey> recipientPublicKeys, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                using (MemoryStream signedAndEncryptedMessageStream = new MemoryStream())
                {
                    if (SignAndEncrypt(messageStream, fileName, signedAndEncryptedMessageStream, senderPublicKey, senderPrivateKey, recipientPublicKeys, hashAlgorithmTag, symmetricKeyAlgorithmTag, armor))
                    {
                        signedAndEncryptedMessage = signedAndEncryptedMessageStream.ToArray();
                        return true;
                    }
                    else
                    {
                        signedAndEncryptedMessage = null;
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="message">Byte array containing the message to encrypt.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="signedAndEncryptedMessageStream">Stream to write the signed and encrypted message into.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool SignAndEncrypt(byte[] message, string fileName, Stream signedAndEncryptedMessageStream, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, IEnumerable<PgpPublicKey> recipientPublicKeys, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream messageStream = new MemoryStream(message))
            {
                return SignAndEncrypt(messageStream, fileName, signedAndEncryptedMessageStream, senderPublicKey, senderPrivateKey, recipientPublicKeys, hashAlgorithmTag, symmetricKeyAlgorithmTag, armor);
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="signedAndEncryptedMessage">If successful, the encrypted message.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool SignAndEncrypt(Stream messageStream, string fileName, out byte[] signedAndEncryptedMessage, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, IEnumerable<PgpPublicKey> recipientPublicKeys, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            using (MemoryStream signedAndEncryptedMessageStream = new MemoryStream())
            {
                if (SignAndEncrypt(messageStream, fileName, signedAndEncryptedMessageStream, senderPublicKey, senderPrivateKey, recipientPublicKeys, hashAlgorithmTag, symmetricKeyAlgorithmTag, armor))
                {
                    signedAndEncryptedMessage = signedAndEncryptedMessageStream.ToArray();
                    return true;
                }
                else
                {
                    signedAndEncryptedMessage = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified public key(s).
        /// </summary>
        /// <param name="messageStream">Stream containing the message to encrypt.</param>
        /// <param name="fileName">File name of for the message.</param>
        /// <param name="signedAndEncryptedMessageStream">Stream to write the encrypted message into.</param>
        /// <param name="senderPublicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="senderPrivateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="recipientPublicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <param name="symmetricKeyAlgorithmTag">The symmetric key algorithm tag to use for encryption.</param>
        /// <param name="armor">Whether to wrap the message with ASCII armor.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public static bool SignAndEncrypt(Stream messageStream, string fileName, Stream signedAndEncryptedMessageStream, PgpPublicKey senderPublicKey, PgpPrivateKey senderPrivateKey, IEnumerable<PgpPublicKey> recipientPublicKeys, HashAlgorithmTag hashAlgorithmTag = HashAlgorithmTag.Sha256, SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = SymmetricKeyAlgorithmTag.TripleDes, bool armor = true)
        {
            // Create a signature generator.
            PgpSignatureGenerator signatureGenerator = new PgpSignatureGenerator(senderPublicKey.Algorithm, hashAlgorithmTag);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, senderPrivateKey);

            // Add the public key user ID.
            foreach (string userId in senderPublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator signatureSubGenerator = new PgpSignatureSubpacketGenerator();
                signatureSubGenerator.SetSignerUserId(false, userId);
                signatureGenerator.SetHashedSubpackets(signatureSubGenerator.Generate());
                break;
            }

            // Allow any of the corresponding keys to be used for decryption.
            PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, true, new SecureRandom());
            foreach (PgpPublicKey publicKey in recipientPublicKeys)
            {
                encryptedDataGenerator.AddMethod(publicKey);
            }

            // Handle optional ASCII armor.
            if (armor)
            {
                using (Stream armoredStream = new ArmoredOutputStream(signedAndEncryptedMessageStream))
                {
                    using (Stream encryptedStream = encryptedDataGenerator.Open(armoredStream, new byte[Constants.LARGEBUFFERSIZE]))
                    {
                        PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
                        using (Stream compressedStream = compressedDataGenerator.Open(encryptedStream))
                        {
                            signatureGenerator.GenerateOnePassVersion(false).Encode(compressedStream);

                            PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
                            using (Stream literalStream = literalDataGenerator.Open(compressedStream, PgpLiteralData.Binary,
                                fileName, DateTime.Now, new byte[Constants.LARGEBUFFERSIZE]))
                            {
                                // Process each character in the message.
                                int messageChar;
                                while ((messageChar = messageStream.ReadByte()) >= 0)
                                {
                                    literalStream.WriteByte((byte)messageChar);
                                    signatureGenerator.Update((byte)messageChar);
                                }
                            }

                            signatureGenerator.Generate().Encode(compressedStream);
                        }
                    }
                }
            }
            else
            {
                using (Stream encryptedStream = encryptedDataGenerator.Open(signedAndEncryptedMessageStream, new byte[Constants.LARGEBUFFERSIZE]))
                {
                    PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
                    using (Stream compressedStream = compressedDataGenerator.Open(encryptedStream))
                    {
                        signatureGenerator.GenerateOnePassVersion(false).Encode(compressedStream);

                        PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
                        using (Stream literalStream = literalDataGenerator.Open(compressedStream, PgpLiteralData.Binary,
                            fileName, DateTime.Now, new byte[Constants.LARGEBUFFERSIZE]))
                        {
                            // Process each character in the message.
                            int messageChar;
                            while ((messageChar = messageStream.ReadByte()) >= 0)
                            {
                                literalStream.WriteByte((byte)messageChar);
                                signatureGenerator.Update((byte)messageChar);
                            }
                        }

                        signatureGenerator.Generate().Encode(compressedStream);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="signedMessage">Byte array containing the signed message.</param>
        /// <param name="signature">Byte array containing the signature.</param>
        /// <param name="publicKey">BouncyCastle public key to be used for verification.</param>
        /// <remarks>The message and signature should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the message's signature is verified.</returns>
        public static bool VerifySignature(byte[] signedMessage, byte[] signature, PgpPublicKey publicKey)
        {
            using (MemoryStream signedMessageStream = new MemoryStream(signedMessage))
            {
                using (MemoryStream signatureStream = new MemoryStream(signature))
                {
                    return VerifySignature(signedMessageStream, signatureStream, publicKey);
                }
            }
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="signedMessage">Byte array containing the signed message.</param>
        /// <param name="signatureStream">Stream containing the signature.</param>
        /// <param name="publicKey">BouncyCastle public key to be used for verification.</param>
        /// <remarks>The message and signature should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the message's signature is verified.</returns>
        public static bool VerifySignature(byte[] signedMessage, Stream signatureStream, PgpPublicKey publicKey)
        {
            using (MemoryStream signedMessageStream = new MemoryStream(signedMessage))
            {
                return VerifySignature(signedMessageStream, signatureStream, publicKey);
            }
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="signedMessageStream">Stream containing the signed message.</param>
        /// <param name="signature">Byte array containing the signature.</param>
        /// <param name="publicKey">BouncyCastle public key to be used for verification.</param>
        /// <remarks>The message and signature should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the message's signature is verified.</returns>
        public static bool VerifySignature(Stream signedMessageStream, byte[] signature, PgpPublicKey publicKey)
        {
            using (MemoryStream signatureStream = new MemoryStream(signature))
            {
                return VerifySignature(signedMessageStream, signatureStream, publicKey);
            }
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="signedMessageStream">Stream containing the signed message.</param>
        /// <param name="signatureStream">Stream containing the signature.</param>
        /// <param name="publicKey">BouncyCastle public key to be used for verification.</param>
        /// <remarks>The message and signature should be passed in without ASCII Armor.</remarks>
        /// <returns>Whether the message's signature is verified.</returns>
        public static bool VerifySignature(Stream signedMessageStream, Stream signatureStream, PgpPublicKey publicKey)
        {
            // Decode from Base-64.
            using (Stream decoderStream = PgpUtilities.GetDecoderStream(signatureStream))
            {
                // Extract the signature list.
                PgpObjectFactory pgpObjectFactory = new PgpObjectFactory(decoderStream);

                PgpObject pgpObject = pgpObjectFactory.NextPgpObject();
                if (pgpObject is PgpSignatureList)
                {
                    PgpSignatureList signatureList = pgpObject as PgpSignatureList;

                    // Hydrate the signature object with the message to be verified.
                    PgpSignature signature = signatureList[0];
                    signature.InitVerify(publicKey);
                    signedMessageStream.Seek(0, SeekOrigin.Begin);
                    for (int i = 0; i < signedMessageStream.Length; i++)
                    {
                        signature.Update((byte)signedMessageStream.ReadByte());
                    }

                    // Return the result.
                    return signature.Verify();
                }
                else
                    return false;
            }
        }
    }
}
