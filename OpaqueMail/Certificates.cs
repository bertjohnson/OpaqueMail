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
    public class Certificates
    {
        public bool ImportCertificate(X509Certificate2 cert)
        {
            return false;
        }

        public bool ImportCertificate(string certFileName)
        {
            return false;
        }
    }
}
