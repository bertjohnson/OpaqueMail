using CERTENROLLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Helper class for creating, importing, and exporting X509 certiticates.
    /// </summary>
    public class CertHelper
    {
        /// <summary>
        /// Retrieve a certificate from the Windows certificate store based on its serial number.
        /// </summary>
        /// <param name="location">Location of the certificate; either the Current User or Local Machine.</param>
        /// <param name="serialNumber">Serial number of the certificate.</param>
        public static X509Certificate2 GetCertificateBySerialNumber(StoreLocation location, string serialNumber)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, true);
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
        /// <param name="serialNumber">Serial number of the certificate.</param>
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
        /// Retrieve a collection of all certificates from the Windows certificate store..
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
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            return CreateSelfSignedCertificate(subjectName, false, 4096, 1);
        }

        /// <summary>
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, bool addToTrustedRoot)
        {
            return CreateSelfSignedCertificate(subjectName, addToTrustedRoot, 4096, 1);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, bool addToTrustedRoot, int keyLength)
        {
            return CreateSelfSignedCertificate(subjectName, addToTrustedRoot, keyLength, 1);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="addToTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, bool addToTrustedRoot, int keyLength, int durationYears)
        {
            List<string> oids = new List<string>();
            oids.Add("1.3.6.1.5.5.7.3.4");        // Secure Email.
            oids.Add("1.3.6.1.4.1.311.10.3.4");   // Data Encipherment.

            return CreateSelfSignedCertificate(subjectName, addToTrustedRoot, keyLength, durationYears, oids);
        }

        /// <summary>
        /// Generate a self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="addAsTrustedRoot">Whether to add the generated certificate as a trusted root.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        /// <param name="oids">Collection of OIDs identifying certificate usage.</param>
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, bool addAsTrustedRoot, int keyLength, int durationYears, List<string> oids)
        {
            // Generate a distinguished name.
            CX500DistinguishedName distinguishedName = new CX500DistinguishedName();
            distinguishedName.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            // Generate a private key.
            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
            privateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
            privateKey.Length = keyLength;
            privateKey.MachineContext = true;
            privateKey.ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0";
            privateKey.Create();

            // Use the SHA-512 hashing algorithm.
            CObjectId hashAlgorithm = new CObjectId();
            hashAlgorithm.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY, AlgorithmFlags.AlgorithmFlagsNone, "SHA512");

            // Load the OIDs passed in and specify enhanced key usages.
            CObjectIds oidCollection = new CObjectIds();
            foreach (string oidID in oids)
            {
                CObjectId oid = new CObjectId();
                oid.InitializeFromValue(oidID);
                oidCollection.Add(oid);
            }
            CX509ExtensionEnhancedKeyUsage enhancedKeyUsages = new CX509ExtensionEnhancedKeyUsage();
            enhancedKeyUsages.InitializeEncode(oidCollection);

            // Create the self-signing request.
            CX509CertificateRequestCertificate cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = distinguishedName;
            cert.Issuer = distinguishedName;
            cert.NotBefore = DateTime.Now;
            cert.NotAfter = DateTime.Now.AddYears(1);
            cert.X509Extensions.Add((CX509Extension)enhancedKeyUsages);
            cert.HashAlgorithm = hashAlgorithm;
            cert.Encode();

            // Enroll based on the certificate signing request.
            CX509Enrollment enrollment = new CX509Enrollment();
            enrollment.InitializeFromRequest(cert);
            enrollment.CertificateFriendlyName = subjectName;
            string csrText = enrollment.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);            

            // Install the certificate chain.  Note that no password is specified.
            enrollment.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csrText, EncodingType.XCN_CRYPT_STRING_BASE64, "");

            // Base-64 encode the PKCS#12 certificate in order to re-import it.
            string pfx = enrollment.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);

            // Instantiate the PKCS#12 certificate.
            X509Certificate2 certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Convert.FromBase64String(pfx), "", System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);

            // If specified, also install the certificate to the trusted root store.
            if (addAsTrustedRoot)
            {
                X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                rootStore.Open(OpenFlags.ReadWrite);
                rootStore.Add(certificate);
                rootStore.Close();
            }

            return certificate;
        }
    }
}
