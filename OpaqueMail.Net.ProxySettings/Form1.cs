using OpaqueMail.Net.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace OpaqueMail.Net.ProxySettings
{
    public partial class Form1 : Form
    {
        #region Private Members
        /// <summary>List of all proxies that have been started.</summary>
        private List<ImapProxy> imapProxies = new List<ImapProxy>();
        /// <summary>List of all proxies that have been started.</summary>
        private List<Pop3Proxy> pop3Proxies = new List<Pop3Proxy>();
        /// <summary>List of all proxies that have been started.</summary>
        private List<SmtpProxy> smtpProxies = new List<SmtpProxy>();
        /// <summary>The path where settings should be saved and loaded.</summary>
        private string SettingsFileName = "";
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Private Methods
        /// <summary>
        /// Proxy Settings load event handler.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            Tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            // Return the path where the service's settings should be saved and loaded.
            SettingsFileName = (AppDomain.CurrentDomain.BaseDirectory + "\\OpaqueMail.Proxy.xml").Replace("\\\\", "\\");

            // Check if the proxy should run as a Windows service.
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    XPathDocument document = new XPathDocument(SettingsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    OperationalModeWindowsService.Checked = GetXmlBoolValue(navigator, "/Settings/RunAsService");

                    // If the proxy is supposed to run as a Windows service, ensure it's started.
                    if (OperationalModeWindowsService.Checked)
                    {
                        InstallService();
                        StartService();
                    }
                }
                catch { }
            }

            // If the proxy isn't supposed to run as a Windows service, start all proxy instances from this application.
            if (!OperationalModeWindowsService.Checked)
            {
                imapProxies = ImapProxy.StartProxiesFromSettingsFile(SettingsFileName);
                pop3Proxies = Pop3Proxy.StartProxiesFromSettingsFile(SettingsFileName);
                smtpProxies = SmtpProxy.StartProxiesFromSettingsFile(SettingsFileName);
            }

            // Update the service status message.
            RefreshServiceStatus();

            // Embed links to relevant certificate authorities.
            FreeCertificateSourcesLabel.Text = @"Free personal S/MIME encryption and signing certificates are available from the following sources:

StartCom: https://cert.startcom.org/
Comodo http://www.instantssl.com/ssl-certificate-products/free-email-certificate.html
CACert: http://cacert.org/";

            FreeCertificateSourcesLabel.Links.Add(112, 26, "https://cert.startcom.org/");
            FreeCertificateSourcesLabel.Links.Add(147, 78, "http://www.instantssl.com/ssl-certificate-products/free-email-certificate.html");
            FreeCertificateSourcesLabel.Links.Add(235, 18, "http://cacert.org/");
            FreeCertificateSourcesLabel.LinkClicked += FreeCertificateSourcesLabel_LinkClicked;

            SmtpProxySettingsLabel.Text = "To configure OpaqueMail proxy settings, edit the following XML file:\r\n\r\n" + SettingsFileName + "\r\n\r\nOnce settings are changed, either restart the OpaqueMail Proxy service or this application.";
            ImapProxySettingsLabel.Text = SmtpProxySettingsLabel.Text;
            Pop3ProxySettingsLabel.Text = SmtpProxySettingsLabel.Text;
        }

        /// <summary>
        /// Open browsers to view clicked links.
        /// </summary>
        private void FreeCertificateSourcesLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        /// <summary>
        /// Return a boolean value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        private static bool GetXmlBoolValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
            {
                bool value = false;
                bool.TryParse(valueNavigator.Value, out value);
                return value;
            }
            else
                return false;
        }

        /// <summary>
        /// Return an integer value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        private static int GetXmlIntValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
            {
                int value = 0;
                int.TryParse(valueNavigator.Value, out value);
                return value;
            }
            else
                return 0;
        }

        /// <summary>
        /// Return a string value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        private static string GetXmlStringValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
                return valueNavigator.Value;
            else
                return "";
        }

        /// <summary>
        /// Install the OpaqueMail Proxy service.
        /// </summary>
        private void InstallService()
        {
            if (!ServiceExists("OpaqueMailProxy"))
            {
                ProxyServiceInstaller installer = new ProxyServiceInstaller();
                installer.Install(false, new string[] { });
            }
        }

        /// <summary>
        /// Switch to using the local application only instead of a Windows service.
        /// </summary>
        private void OperationalModeApplication_CheckedChanged(object sender, EventArgs e)
        {
            if (OperationalModeApplication.Checked)
            {
                StopService();
                UninstallService();

                Thread.Sleep(2500);

                // Start all proxy instances.
                imapProxies = ImapProxy.StartProxiesFromSettingsFile(SettingsFileName);
                pop3Proxies = Pop3Proxy.StartProxiesFromSettingsFile(SettingsFileName);
                smtpProxies = SmtpProxy.StartProxiesFromSettingsFile(SettingsFileName);

                UpdateSettingsFile(false);

                RefreshServiceStatus();
            }
        }

        /// <summary>
        /// Switch to using a Windows service.
        /// </summary>
        private void OperationalModeWindowsService_CheckedChanged(object sender, EventArgs e)
        {
            if (OperationalModeWindowsService.Checked)
            {
                foreach (ImapProxy imapProxy in imapProxies)
                    imapProxy.Stop();
                foreach (Pop3Proxy pop3Proxy in pop3Proxies)
                    pop3Proxy.Stop();
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.Stop();

                Thread.Sleep(2500);

                InstallService();
                StartService();

                UpdateSettingsFile(true);

                RefreshServiceStatus();
            }
        }

        /// <summary>
        /// Load and display all local certificates valid for e-mail signing.
        /// </summary>
        private void RefreshCertificates()
        {
            StringBuilder certificatesText = new StringBuilder(Constants.SMALLSBSIZE);

            // Load ceritificates for both the Current User and Local machine.
            X509Certificate2Collection certs = CertHelper.GetWindowsCertificates(StoreLocation.CurrentUser);
            certs.AddRange(CertHelper.GetWindowsCertificates(StoreLocation.LocalMachine));
            foreach (X509Certificate2 cert in certs)
            {
                // Ensure that the certificate has a valid subject name.
                if (cert.Subject.IndexOf("@") > -1 && (cert.Subject.StartsWith("E=") || (cert.Subject.StartsWith("CN="))))
                {
                    if (cert.Verify())
                        certificatesText.Append(cert.Subject + " [Serial #: " + cert.SerialNumber + "]\r\n");
                }
            }

            if (certificatesText.Length > 0)
                SigningCertificatesLabel.Text = certificatesText.ToString();
            else
                SigningCertificatesLabel.Text = "This computer has no valid e-mail S/MIME signing certificates.\r\n\r\nTo attain a valid certificate, please use one of the links below.";
        }

        /// <summary>
        /// Display configuration settings for local IMAP proxy service instances.
        /// </summary>
        private void RefreshImapServiceInstances()
        {
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    XPathDocument document = new XPathDocument(SettingsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    ImapProxyInstanceGrid.Items.Clear();

                    int imapServiceCount = GetXmlIntValue(navigator, "/Settings/IMAP/ServiceCount");
                    for (int i = 1; i <= imapServiceCount; i++)
                    {
                        string[] properties = new string[6];
                        properties[0] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LocalIPAddress");
                        properties[1] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LocalPort");
                        properties[2] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LocalEnableSSL");
                        properties[3] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerHostName");
                        properties[4] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerPort");
                        properties[5] = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerEnableSSL");

                        ImapProxyInstanceGrid.Items.Add(new ListViewItem(properties));
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Display configuration settings for local POP3 proxy service instances.
        /// </summary>
        private void RefreshPop3ServiceInstances()
        {
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    XPathDocument document = new XPathDocument(SettingsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    Pop3ProxyInstanceGrid.Items.Clear();

                    int pop3ServiceCount = GetXmlIntValue(navigator, "/Settings/POP3/ServiceCount");
                    for (int i = 1; i <= pop3ServiceCount; i++)
                    {
                        string[] properties = new string[6];
                        properties[0] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/LocalIPAddress");
                        properties[1] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/LocalPort");
                        properties[2] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/LocalEnableSSL");
                        properties[3] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerHostName");
                        properties[4] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerPort");
                        properties[5] = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerEnableSSL");

                        Pop3ProxyInstanceGrid.Items.Add(new ListViewItem(properties));
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Display configuration settings for local SMTP proxy service instances.
        /// </summary>
        private void RefreshSmtpServiceInstances()
        {
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    XPathDocument document = new XPathDocument(SettingsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    SmtpProxyInstanceGrid.Items.Clear();

                    int smtpServiceCount = GetXmlIntValue(navigator, "/Settings/SMTP/ServiceCount");
                    for (int i = 1; i <= smtpServiceCount; i++)
                    {
                        string[] properties = new string[6];
                        properties[0] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LocalIPAddress");
                        properties[1] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LocalPort");
                        properties[2] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LocalEnableSSL");
                        properties[3] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerHostName");
                        properties[4] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerPort");
                        properties[5] = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerEnableSSL");

                        SmtpProxyInstanceGrid.Items.Add(new ListViewItem(properties));
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Display the current status of the OpaqueMail Proxy service.
        /// </summary>
        private void RefreshServiceStatus()
        {
            if (OperationalModeWindowsService.Checked)
            {
                if (!ServiceExists("OpaqueMailProxy"))
                {
                    ProxyStatusLabel.Text = "OpaqueMail Proxy is configured to run as a Windows service, but the service could not be found.\r\n\r\nPlease try reinstalling the service.";
                    ProxyStatusLabel.ForeColor = Color.DarkRed;
                }
                else
                {
                    ProxyStatusLabel.ForeColor = Color.Black;

                    ServiceController serviceContoller = new ServiceController("OpaqueMailProxy");
                    switch (serviceContoller.Status)
                    {
                        case ServiceControllerStatus.ContinuePending:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is resuming.";
                            break;
                        case ServiceControllerStatus.Paused:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is paused.";
                            break;
                        case ServiceControllerStatus.PausePending:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is pausing.";
                            break;
                        case ServiceControllerStatus.Running:
                            ProxyStatusLabel.ForeColor = Color.DarkGreen;
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is running.";
                            break;
                        case ServiceControllerStatus.StartPending:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is starting.";
                            break;
                        case ServiceControllerStatus.Stopped:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is stopped.";
                            break;
                        case ServiceControllerStatus.StopPending:
                            ProxyStatusLabel.Text = "The OpaqueMail Proxy service is stopping.";
                            break;
                    }
                }
            }
            else
            {
                ProxyStatusLabel.ForeColor = Color.DarkGreen;
                ProxyStatusLabel.Text = "OpaqueMail Proxy is successfully running with " + smtpProxies.Count + " service instance" + (smtpProxies.Count != 1 ? "s" : "") + ".";
            }
        }

        /// <summary>
        /// Confirm the Windows service exists.
        /// </summary>
        /// <param name="name">Name of the WIndows service</param>
        private bool ServiceExists(string name)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.FirstOrDefault(s => s.ServiceName == name) != null;
        }

        /// <summary>
        /// Start the OpaqueMail Proxy Windows service.
        /// </summary>
        private void StartService()
        {
            if (ServiceExists("OpaqueMailProxy"))
            {
                ServiceController serviceContoller = new ServiceController("OpaqueMailProxy");
                if (serviceContoller.Status != ServiceControllerStatus.Running && serviceContoller.Status != ServiceControllerStatus.StartPending)
                    serviceContoller.Start();
            }
        }

        /// <summary>
        /// Stop the OpaqueMail Proxy Windows service.
        /// </summary>
        private void StopService()
        {
            if (ServiceExists("OpaqueMailProxy"))
            {
                ServiceController serviceContoller = new ServiceController("OpaqueMailProxy");
                if (serviceContoller.Status != ServiceControllerStatus.Stopped && serviceContoller.Status != ServiceControllerStatus.StopPending)
                    serviceContoller.Stop();
            }
        }
        
        /// <summary>
        /// Handle tab changes by refreshing relevant content.
        /// </summary>
        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Tabs.SelectedIndex)
            {
                case 0:
                    RefreshServiceStatus();
                    break;
                case 1:
                    RefreshSmtpServiceInstances();
                    break;
                case 2:
                    RefreshImapServiceInstances();
                    break;
                case 3:
                    RefreshPop3ServiceInstances();
                    break;
                case 4:
                    RefreshCertificates();
                    break;
            }
        }

        /// <summary>
        /// Uninstall the OpaqueMail Proxy service.
        /// </summary>
        private void UninstallService()
        {
            if (ServiceExists("OpaqueMailProxy"))
            {
                ProxyServiceInstaller installer = new ProxyServiceInstaller();
                installer.Install(true, new string[] { });
            }
        }

        /// <summary>
        /// Update the settings file to reflect whether OpaqueMail Proxy runs as a Windows service.
        /// </summary>
        private void UpdateSettingsFile(bool runAsService)
        {
            string configuration = File.ReadAllText(SettingsFileName);

            string canonicalConfiguration = configuration.ToUpper();
            int startPos = canonicalConfiguration.IndexOf("<RUNASSERVICE>");
            if (startPos > -1)
            {
                int endPos = canonicalConfiguration.IndexOf("</RUNASSERVICE>", startPos + 14);
                if (endPos > -1)
                    configuration = configuration.Substring(0, startPos + 14) + runAsService.ToString() + configuration.Substring(endPos);
            }

            File.WriteAllText(SettingsFileName, configuration);
        }
        #endregion Private Methods
    }
}
