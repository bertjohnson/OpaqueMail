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

// Depending on environment, the statement below may need to be changed to "using CERTCLIENTLib".
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OpaqueMail
{
    /// <summary>
    /// Helper class for creating, importing, and exporting X509 certiticates.
    /// </summary>
    public class CertHelper
    {
        #region Constants
        // Constants used for certificate signing requests.
        public const int CC_DEFAULTCONFIG = 0;
        public const int CC_UIPICKCONFIG = 0x1;
        public const int CR_IN_BASE64 = 0x1;
        public const int CR_IN_FORMATANY = 0;
        public const int CR_IN_PKCS10 = 0x100;
        public const int CR_OUT_BASE64 = 0x1;
        public const int CR_OUT_CHAIN = 0x100;
        #endregion Constants

        #region Enums
        /// <summary>
        /// Represents the certificate signing request response from a certificate authority.
        /// </summary>
        public enum CsrResponse
        {
            CR_DISP_ISSUED = 3,
            CR_DISP_UNDER_SUBMISSION = 5,
            CR_DISP_FAILED = -1
        }
        #endregion Enums

        #region Public Methods
        /// <summary>
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            // If the subject name is well-formed, strip CN= and any other arguments.
            string friendlyName = subjectName;
            int equalsPos = friendlyName.IndexOf("=");
            if (equalsPos > -1)
                friendlyName = friendlyName.Substring(equalsPos + 1);
            int commaPos = friendlyName.IndexOf(",");
            if (commaPos > -1)
                friendlyName = friendlyName.Substring(0, commaPos);

            return CreateSelfSignedCertificate(subjectName, friendlyName, StoreLocation.LocalMachine, false, 4096, 1);
        }

        /// <summary>
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName)
        {
            return CreateSelfSignedCertificate(subjectName, friendlyName, StoreLocation.LocalMachine , false, 4096, 1);
        }

        /// <summary>
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addToTrustedRoot)
        {
            return CreateSelfSignedCertificate(subjectName, friendlyName, location, addToTrustedRoot, 4096, 1);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addToTrustedRoot, int keyLength)
        {
            return CreateSelfSignedCertificate(subjectName, friendlyName, location, addToTrustedRoot, keyLength, 1);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addToTrustedRoot, int keyLength, int durationYears)
        {
            List<DerObjectIdentifier> oids = new List<DerObjectIdentifier>();
            oids.Add(new DerObjectIdentifier("1.3.6.1.5.5.7.3.4"));        // Secure Email.
            oids.Add(new DerObjectIdentifier("1.3.6.1.4.1.6449.1.3.5.2"));   // Email Protection.

            return CreateSelfSignedCertificate(subjectName, friendlyName, location, addToTrustedRoot, keyLength, durationYears, oids);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="addAsTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        /// <param name="oids">Collection of OIDs identifying certificate usage.</param>
        /// <returns>Self-signed certificate.</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addAsTrustedRoot, int keyLength, int durationYears, List<DerObjectIdentifier> oids)
        {
            // Prepare random number generation.
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // Create asymmetric key.
            AsymmetricCipherKeyPair subjectKeyPair;
            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyLength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            // Generate a serial number.
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Assign the subject name.
            X509Name subjectDN = new X509Name("CN=" + subjectName);
            X509Name issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Set valid dates.
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(durationYears);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Set key usage for digital signatures and key encipherment.
            KeyUsage usage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment);
            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, usage);

            // Load the OIDs passed in and specify enhanced key usages.
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(oids));

            // Assign the public key.
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Self-sign the certificate using SHA-512.
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", subjectKeyPair.Private, random);
            Org.BouncyCastle.X509.X509Certificate bouncyCastleCertificate = certificateGenerator.Generate(signatureFactory);

            // Convert from BouncyCastle private key format to System.Security.Cryptography format.
            AsymmetricAlgorithm privateKey = ToDotNetKey((RsaPrivateCrtKeyParameters)subjectKeyPair.Private);
            X509Certificate2 x509certificate = new X509Certificate2(DotNetUtilities.ToX509Certificate(bouncyCastleCertificate));
            x509certificate.PrivateKey = privateKey;
            x509certificate.FriendlyName = subjectName;

            return x509certificate;
        }

        /// <summary>
        /// Retrieve a certificate from the Windows certificate store based on its serial number.
        /// </summary>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="serialNumber">Serial number of the certificate.</param>
        public static X509Certificate2 GetCertificateBySerialNumber(StoreLocation location, string serialNumber)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber.Replace(" ", "").ToLower(), true);
            store.Close();

            // If no certificate is found, return null;
            if (certs.Count < 1)
                return null;
            else
                return certs[0];
        }

        /// <summary>
        /// Retrieve a certificate from the Windows certificate store based on its subject name.
        /// </summary>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="subjectName">Subject name of the certificate.</param>
        public static X509Certificate2 GetCertificateBySubjectName(StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
            store.Close();

            // If no certificate is found, return null;
            if (certs.Count < 1)
                return null;
            else
                return certs[0];
        }

        /// <summary>
        /// Retrieve a certificate from the Windows certificate store based on its thumbprint.
        /// </summary>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="thumbprint">Thumbprint of the certificate.</param>
        public static X509Certificate2 GetCertificateByThumbprint(StoreLocation location, string thumbprint)
        {
            // Sanitize the input.
            thumbprint = Regex.Replace(thumbprint, @"[^\da-zA-z]", string.Empty).ToUpper();

            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
            store.Close();

            // If no certificate is found, return null;
            if (certs.Count < 1)
                return null;
            else
                return certs[0];
        }

        /// <summary>
        /// Retrieve a collection of all certificates from the Windows certificate store.
        /// </summary>
        /// <param name="location">Location of the certificates; either the Current User or Local Machine.</param>
        public static X509Certificate2Collection GetWindowsCertificates(StoreLocation location)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection collection = store.Certificates;
            store.Close();

            return collection;
        }

        /// <summary>
        /// Install a certificate to the Windows certificate store in the specified location.
        /// </summary>
        /// <param name="cert">The certificate to install.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        public static void InstallWindowsCertificate(System.Security.Cryptography.X509Certificates.X509Certificate cert, StoreLocation location)
        {
            InstallWindowsCertificate(new X509Certificate2(cert), location);
        }

        /// <summary>
        /// Install a certificate to the Windows certificate store in the specified location.
        /// </summary>
        /// <param name="cert">The certificate to install.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        public static void InstallWindowsCertificate(X509Certificate2 cert, StoreLocation location)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }

        /// <summary>
        /// Install a root certificate to the Windows certificate store in the specified location.
        /// </summary>
        /// <param name="cert">The certificate to install.</param>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        public static void InstallWindowsRootCertificate(X509Certificate2 cert, StoreLocation location)
        {
            X509Store store = new X509Store(StoreName.Root, location);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Convert from BouncyCastle private key format to System.Security.Cryptography format.
        /// </summary>
        /// <param name="privateKey">BouncyCastle private key.</param>
        /// <returns>System.Security.Cryptography representation of the key.</returns>
        private static AsymmetricAlgorithm ToDotNetKey(RsaPrivateCrtKeyParameters privateKey)
        {
            CspParameters cspParams = new CspParameters()
            {
                KeyContainerName = Guid.NewGuid().ToString(),
                KeyNumber = (int)KeyNumber.Exchange,
                Flags = CspProviderFlags.UseMachineKeyStore
            };

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            RSAParameters parameters = new RSAParameters()
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
            };

            rsaProvider.ImportParameters(parameters);
            return rsaProvider;
        }
        #endregion Private Methods
    }
}