using CERTCLILib;
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
        /// Create a certificate signing request.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        public static CX509CertificateRequestCertificate CreateCertificateSigningRequest(string subjectName)
        {
            return CreateCertificateSigningRequest(subjectName, 4096, 1);
        }

        /// <summary>
        /// Create a certificate signing request.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        public static CX509CertificateRequestCertificate CreateCertificateSigningRequest(string subjectName, int keyLength)
        {
            return CreateCertificateSigningRequest(subjectName, keyLength, 1);
        }

        /// <summary>
        /// Create a certificate signing request.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        public static CX509CertificateRequestCertificate CreateCertificateSigningRequest(string subjectName, int keyLength, int durationYears)
        {
            List<string> oids = new List<string>();
            oids.Add("1.3.6.1.5.5.7.3.4");          // Secure Email.
            oids.Add("1.3.6.1.4.1.6449.1.3.5.2");   // E-mail Protection.

            return CreateCertificateSigningRequest(subjectName, keyLength, durationYears, oids);
        }

        /// <summary>
        /// Create a certificate signing request.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
        /// <param name="keyLength">Size of the key in bits.</param>
        /// <param name="durationYears">Duration of the certificate, specified in years.</param>
        /// <param name="oids">Collection of OIDs identifying certificate usage.</param>
        public static CX509CertificateRequestCertificate CreateCertificateSigningRequest(string subjectName, int keyLength, int durationYears, List<string> oids)
        {
            // Prepend the subject name with CN= if it doesn't begin with CN=, E=, etc..
            if (subjectName.IndexOf("=") < 0)
                subjectName = "CN=" + subjectName;

            // Generate a distinguished name.
            CX500DistinguishedName distinguishedName = new CX500DistinguishedName();
            distinguishedName.Encode(subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

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

            CX509ExtensionKeyUsage keyUsage = new CX509ExtensionKeyUsage();
            keyUsage.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE);

            CX509ExtensionEnhancedKeyUsage enhancedKeyUsages = new CX509ExtensionEnhancedKeyUsage();
            enhancedKeyUsages.InitializeEncode(oidCollection);

            string sanSubjectName = subjectName.Substring(subjectName.IndexOf("=") + 1);

            CAlternativeName sanAlternateName = new CAlternativeName();
            sanAlternateName.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_RFC822_NAME, sanSubjectName);

            CAlternativeNames sanAlternativeNames = new CAlternativeNames();
            sanAlternativeNames.Add(sanAlternateName);

            CX509ExtensionAlternativeNames alternativeNamesExtension = new CX509ExtensionAlternativeNames();
            alternativeNamesExtension.InitializeEncode(sanAlternativeNames);

            // Create the self-signing request.
            CX509CertificateRequestCertificate cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = distinguishedName;
            cert.Issuer = distinguishedName;
            cert.NotBefore = DateTime.Now;
            cert.NotAfter = DateTime.Now.AddYears(1);
            cert.X509Extensions.Add((CX509Extension)keyUsage);
            cert.X509Extensions.Add((CX509Extension)enhancedKeyUsages);
            cert.X509Extensions.Add((CX509Extension)alternativeNamesExtension);
            cert.HashAlgorithm = hashAlgorithm;
            cert.Encode();

            return cert;
        }

        /// <summary>
        /// Generate a 4096-bit self-signed certificate and add it to the Windows certificate store.
        /// </summary>
        /// <param name="subjectName">The subject name of the certificate.</param>
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
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addToTrustedRoot, int keyLength, int durationYears)
        {
            List<string> oids = new List<string>();
            oids.Add("1.3.6.1.5.5.7.3.4");        // Secure Email.
            oids.Add("1.3.6.1.4.1.6449.1.3.5.2");   // E-mail Protection.

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
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName, string friendlyName, StoreLocation location, bool addAsTrustedRoot, int keyLength, int durationYears, List<string> oids)
        {
            // Create the self-signing request.
            CX509CertificateRequestCertificate cert = CreateCertificateSigningRequest(subjectName, keyLength, durationYears, oids);

            // Enroll based on the certificate signing request.
            CX509Enrollment enrollment = new CX509Enrollment();
            enrollment.InitializeFromRequest(cert);
            enrollment.CertificateFriendlyName = friendlyName;
            string csrText = enrollment.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);

            // Install the certificate chain.  Note that no password is specified.
            enrollment.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csrText, EncodingType.XCN_CRYPT_STRING_BASE64, "");

            // Base-64 encode the PKCS#12 certificate in order to re-import it.
            string pfx = enrollment.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);

            // Instantiate the PKCS#12 certificate.
            X509Certificate2 certificate = new X509Certificate2(System.Convert.FromBase64String(pfx), "", X509KeyStorageFlags.Exportable);

            // If specified, also install the certificate to the trusted root store.
            if (addAsTrustedRoot)
            {
                X509Store rootStore = new X509Store(StoreName.Root, location);
                rootStore.Open(OpenFlags.ReadWrite);
                rootStore.Add(certificate);
                rootStore.Close();
            }

            return certificate;
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
        public static void InstallWindowsCertificate(X509Certificate cert, StoreLocation location)
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

        /// <summary>
        /// Submit a certificate signing request to a certificate authority, such as a server running Active Directory Certificate Services, and return the certificate or response.
        /// </summary>
        /// <param name="csr">Certificate signing request to be submitted.</param>
        /// <param name="friendlyName">The friendly name of the certificate.</param>
        /// <param name="caServer">The certificate authority server instance.</param>
        /// <param name="csrResponse">Response from the certificate signing request, represented as a CsrResponse enum.</param>
        /// <param name="dispositionMessage">Message returned when a certificate signing fails.</param>
        public X509Certificate2 SubmitCertificateSigningRequest(CX509CertificateRequestCertificate csr, string friendlyName, string caServer, out CsrResponse csrResponse, out string dispositionMessage)
        {
            // Convert the certificate signing request to base-64..
            CX509Enrollment enrollment = new CX509Enrollment();
            enrollment.InitializeFromRequest(csr);
            enrollment.CertificateFriendlyName = friendlyName;
            string csrText = enrollment.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);

            // Submit the request to the certificate authority.
            CCertRequest certRequest = new CCertRequest();
            int csrResponseCode = certRequest.Submit(CR_IN_BASE64 | CR_IN_FORMATANY, csrText, string.Empty, caServer);

            // React to our response response from the certificate authority.
            switch (csrResponseCode)
            {
                case 3:     // Issued.
                    csrResponse = CsrResponse.CR_DISP_ISSUED;
                    dispositionMessage = "";
                    return new X509Certificate2(Encoding.UTF8.GetBytes(certRequest.GetCertificate(CR_OUT_BASE64 | CR_OUT_CHAIN)));
                case 5:     // Pending.
                    csrResponse = CsrResponse.CR_DISP_UNDER_SUBMISSION;
                    dispositionMessage = "";
                    return null;
                default:    // Failure.
                    csrResponse = CsrResponse.CR_DISP_FAILED;
                    dispositionMessage = certRequest.GetDispositionMessage();
                    return null;
            }
        }
        #endregion Public Methods
    }
}
