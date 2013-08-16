using Microsoft.Win32;
using OpaqueMail.Net.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace OpaqueMail.Net.ProxySettings
{
    public partial class Form1 : Form
    {
        #region Private Members
        /// <summary>Outlook versions to check for in the registry.</summary>
        Dictionary<string, string> OutlookVersions = new Dictionary<string, string>();
        /// <summary>The path where settings should be saved and loaded.</summary>
        private string SettingsFileName = "";
        /// <summary>Timer to check the service status.</summary>
        private System.Threading.Timer StatusTimer;
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

        #region Protected Methods
        /// <summary>
        /// Handle the F5 keypress by reloading the e-mail accounts list and certificate choices.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
                PopulateAccounts();

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion Protected Methods

        #region Private Methods
        /// <summary>
        /// Load event handler.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            OutlookVersions.Add("8.0", "Outlook 97");
            OutlookVersions.Add("9.0", "Outlook 2000");
            OutlookVersions.Add("10.0", "Outlook XP");
            OutlookVersions.Add("11.0", "Outlook 2003");
            OutlookVersions.Add("12.0", "Outlook 2007");
            OutlookVersions.Add("14.0", "Outlook 2010");
            OutlookVersions.Add("15.0", "Outlook 2013");
            OutlookVersions.Add("16.0", "Outlook v16");
            OutlookVersions.Add("17.0", "Outlook v17");
            OutlookVersions.Add("18.0", "Outlook v18");
            OutlookVersions.Add("19.0", "Outlook v19");
            OutlookVersions.Add("20.0", "Outlook v20");

            SettingsFileName = (AppDomain.CurrentDomain.BaseDirectory + "\\OpaqueMail.Proxy.xml").Replace("\\\\", "\\");

            AboutLabel.Links.Clear();
            AboutLabel.Links.Add(AboutLabel.Text.IndexOf("S/MIME"), 6, "https://en.wikipedia.org/wiki/S/MIME");
            AboutLabel.LinkClicked += Label_LinkClicked;

            GettingStartedLabel.Links.Clear();
            int settingsFileIndex = GettingStartedLabel.Text.IndexOf("[SETTINGSFILE]");
            GettingStartedLabel.Text = GettingStartedLabel.Text.Replace("[SETTINGSFILE]", SettingsFileName);
            GettingStartedLabel.Links.Add(settingsFileIndex, SettingsFileName.Length, SettingsFileName);
            GettingStartedLabel.Links.Add(GettingStartedLabel.Text.IndexOf("http://opaquemail.org/"), 22, "http://opaquemail.org/");
            GettingStartedLabel.LinkClicked += Label_LinkClicked;

            CertificateLabel.Links.Clear();
            CertificateLabel.Links.Add(CertificateLabel.Text.IndexOf("Comodo"), 6, "http://www.instantssl.com/ssl-certificate-products/free-email-certificate.html");
            CertificateLabel.Links.Add(CertificateLabel.Text.IndexOf("StartCom"), 8, "https://cert.startcom.org/");
            CertificateLabel.LinkClicked += Label_LinkClicked;

            AccountsLabel.Links.Clear();
            AccountsLabel.Links.Add(AboutLabel.Text.IndexOf("http://opaquemail.org/"), 22, "http://opaquemail.org/");
            AccountsLabel.LinkClicked += Label_LinkClicked;

            // Load the e-mail accounts list and certificate choices.
            PopulateAccounts();

            SmimeOperations.SelectedIndex = 3;
            SmimeOperations.SelectedIndexChanged += SmimeOperations_SelectedIndexChanged;

            SmimeSettingsMode.SelectedIndex = 0;

            string ip = "192.168.*";
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress hostIP in hostEntry.AddressList)
            {
                string[] ipParts = hostIP.ToString().Split('.');
                if (ipParts.Length > 2)
                {
                    ip = ipParts[0] + "." + ipParts[1] + ".*";
                    break;
                }
            }
            NetworkAccess.Items[1] = ((string)NetworkAccess.Items[1]).Replace("192.168.*", ip);

            NetworkAccess.SelectedIndex = 0;

            UpdateServiceStatus(null);

            StatusTimer = new System.Threading.Timer(new TimerCallback(UpdateServiceStatus), null, 15000, 15000);
        }

        /// <summary>
        /// Determine the serial number of a certificate matching the account name.
        /// </summary>
        /// <param name="certs">Collection of certificates to search within.</param>
        /// <param name="accountName">Account name to match.</param>
        /// <returns></returns>
        private string GetMatchingCert(X509Certificate2Collection certs, string accountName)
        {
            string matchingCert = "self-signed";

            // Check if there's a matching certificate.
            foreach (X509Certificate2 cert in certs)
            {
                string canonicalCertSubject = "";
                if (cert.Subject.StartsWith("E="))
                {
                    canonicalCertSubject = cert.Subject.Substring(2).ToUpper();
                    int certSubjectComma = canonicalCertSubject.IndexOf(",");
                    if (certSubjectComma > -1)
                        canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                    if (accountName.ToUpper() == canonicalCertSubject)
                    {
                        if (cert.Verify())
                            matchingCert = cert.SerialNumber;
                    }
                }
                else if (cert.Subject.StartsWith("CN="))
                {
                    canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
                    int certSubjectComma = canonicalCertSubject.IndexOf(",");
                    if (certSubjectComma > -1)
                        canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                    if (accountName.ToUpper() == canonicalCertSubject)
                    {
                        if (cert.Verify())
                            matchingCert = cert.SerialNumber;
                    }
                }
            }

            return matchingCert;
        }

        /// <summary>
        /// Determine the next available port.
        /// </summary>
        /// <param name="nextPortToTry">The first port to check.</param>
        private int GetNextAvailablePort(int nextPortToTry)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipGlobalProperties.GetActiveTcpConnections();

            while (true)
            {
                bool isAvailable = true;
                foreach (TcpConnectionInformation tcpInfo in tcpConnections)
                {
                    if (tcpInfo.LocalEndPoint.Port == nextPortToTry)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                    return nextPortToTry;
                else
                    nextPortToTry++;
            }
        }

        /// <summary>
        /// Retrieve an Outlook registry setting.
        /// </summary>
        /// <param name="key">The registry key to read within.</param>
        /// <param name="name">The name of the value to read.</param>
        private string GetOutlookRegistryValue(RegistryKey key, string name)
        {
            object value = key.GetValue(name);
            if (value is byte[])
                return Encoding.Unicode.GetString((byte[])value).Replace("\0", "");
            if (value != null)
                return value.ToString();
            else
                return null;
        }

        /// <summary>
        /// Return a boolean value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        private static bool? GetXmlBoolValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
            {
                if (!string.IsNullOrEmpty(valueNavigator.Value))
                {
                    bool value;
                    bool.TryParse(valueNavigator.Value, out value);
                    return value;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Return an integer value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        private static int? GetXmlIntValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
            {
                int value;
                int.TryParse(valueNavigator.Value, out value);
                return value;
            }
            else
                return null;
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
                return null;
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
        /// Open the default application to view clicked links.
        /// </summary>
        private void Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString().Replace("\r\n", ""));
        }

        /// <summary>
        /// Populate the gridview with appropriate e-mail account and certificate choices.
        /// </summary>
        private void PopulateAccounts()
        {
            // Prepare the certificate choices.
            List<Choice> certChoices = new List<Choice>();

            X509Certificate2Collection certs = CertHelper.GetWindowsCertificates(StoreLocation.CurrentUser);
            certs.AddRange(CertHelper.GetWindowsCertificates(StoreLocation.LocalMachine));

            HashSet<string> certificatesSeen = new HashSet<string>();
            foreach (X509Certificate2 cert in certs)
            {
                // Avoid duplicate certificates.
                if (!certificatesSeen.Contains(cert.SerialNumber))
                {
                    // Ensure that the certificate has a valid subject name.
                    if (cert.Subject.IndexOf("@") > -1 && (cert.Subject.StartsWith("E=") || (cert.Subject.StartsWith("CN="))))
                    {
                        if (cert.Verify())
                        {
                            certChoices.Add(new Choice(cert.Subject + " (SN: " + cert.SerialNumber + ")", cert.SerialNumber));
                            certificatesSeen.Add(cert.SerialNumber);
                        }
                    }
                }
            }

            certChoices.Add(new Choice("New self-signed certificate", "self-signed"));
            CertificateColumn.DataSource = certChoices;
            CertificateColumn.DisplayMember = "Name";
            CertificateColumn.ValueMember = "Value";

            // Check which Outlook registry keys, Thunderbird configs, and Windows Live Mail configs have proxies associated.
            XPathDocument document;
            HashSet<string> outlookRegistryKeys = new HashSet<string>();
            HashSet<string> thunderbirdKeys = new HashSet<string>();
            HashSet<string> liveMailKeys = new HashSet<string>();
            try
            {
                document = new XPathDocument(SettingsFileName);
                XPathNavigator navigator = document.CreateNavigator();

                int smtpServiceCount = GetXmlIntValue(navigator, "/Settings/SMTP/ServiceCount") ?? 0;
                for (int i = 1; i <= smtpServiceCount; i++)
                {
                    int? registryKeyCount = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/OutlookRegistryKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string registryKey = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/OutlookRegistryKey" + j);
                        if (!string.IsNullOrEmpty(registryKey))
                            outlookRegistryKeys.Add(registryKey);
                    }

                    int? thunderbirdKeyCount = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/ThunderbirdKeyCount") ?? 0;
                    for (int j = 1; j <= thunderbirdKeyCount; j++)
                    {
                        string thunderbirdKey = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/ThunderbirdKey" + j);
                        if (!string.IsNullOrEmpty(thunderbirdKey))
                            thunderbirdKeys.Add(thunderbirdKey);
                    }

                    int? liveMailKeyCount = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/LiveMailKeyCount") ?? 0;
                    for (int j = 1; j <= liveMailKeyCount; j++)
                    {
                        string liveMailKey = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LiveMailKey" + j);
                        if (!string.IsNullOrEmpty(liveMailKey))
                            liveMailKeys.Add(liveMailKey);
                    }
                }
            }
            catch { }

            // First, correlate Outlook registry keys with accounts.
            AccountGrid.Rows.Clear();
            bool activeProxy = false;
            foreach (string outlookVersion in OutlookVersions.Keys)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\" + outlookVersion + @"\Outlook\Profiles\Outlook\9375CFF0413111d3B88A00104B2A6676", false))
                {
                    if (key != null)
                    {
                        string[] subkeyNames = key.GetSubKeyNames();
                        if (subkeyNames != null)
                        {
                            foreach (string subkeyName in subkeyNames)
                            {
                                using (RegistryKey subKey = key.OpenSubKey(subkeyName, false))
                                {
                                    string smtpServer = GetOutlookRegistryValue(subKey, "SMTP Server");

                                    if (!string.IsNullOrEmpty(smtpServer))
                                    {
                                        string accountName = GetOutlookRegistryValue(subKey, "Account Name");

                                        string matchingCert = GetMatchingCert(certs, accountName);

                                        if (!activeProxy && outlookRegistryKeys.Contains(subKey.Name))
                                            activeProxy = true;

                                        AccountGrid.Rows.Add(OutlookVersions[outlookVersion], accountName, outlookRegistryKeys.Contains(subKey.Name), matchingCert, subKey.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Second, correlate Thunderbird config keys with accounts.
            activeProxy = false;
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Thunderbird\\Profiles"))
            {
                foreach (string directory in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Thunderbird\\Profiles"))
                {
                    if (File.Exists(directory + "\\prefs.js"))
                    {
                        string prefsFile = File.ReadAllText(directory + "\\prefs.js");

                        int keyCount;
                        int.TryParse(Functions.ReturnBetween(prefsFile, "user_pref(\"mail.account.lastKey\", ", ")"), out keyCount);
                        for (int i = 1; i <= keyCount; i++)
                        {
                            string smtpServer = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".hostname\", \"", "\"");
                            string accountName = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".username\", \"", "\"");

                            if (!string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(accountName))
                            {
                                string thunderbirdKey = directory + "~" + i.ToString();

                                string matchingCert = GetMatchingCert(certs, accountName);

                                if (!activeProxy && thunderbirdKeys.Contains(thunderbirdKey))
                                    activeProxy = true;

                                AccountGrid.Rows.Add("Thunderbird", accountName, thunderbirdKeys.Contains(thunderbirdKey), matchingCert, thunderbirdKey);
                            }
                        }
                    }
                }
            }

            // Third, correlate Windows Live Mail config keys with accounts.
            activeProxy = false;
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows Live Mail"))
            {
                foreach (string directory in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows Live Mail"))
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".oeaccount"))
                        {
                            string settingsFile = File.ReadAllText(file);

                            int pos = 0;
                            while (pos > -1)
                            {
                                pos = settingsFile.IndexOf("<MessageAccount>", pos);
                                if (pos > -1)
                                {
                                    int pos2 = settingsFile.IndexOf("</MessageAccount>", pos);
                                    if (pos2 > -1)
                                    {
                                        string accountSettings = settingsFile.Substring(pos + 16, pos2 - pos - 16);

                                        string accountName = Functions.ReturnBetween(accountSettings, "<Account_Name type=\"SZ\">", "</Account_Name>");
                                        string smtpServer = Functions.ReturnBetween(accountSettings, "<SMTP_Server type=\"SZ\">", "</SMTP_Server>");
                                        string address = Functions.ReturnBetween(accountSettings, "<SMTP_Email_Address type=\"SZ\">", "</SMTP_Email_Address>");

                                        if (!string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(address))
                                        {
                                            string liveMailKey = file + "~" + accountName;

                                            string matchingCert = GetMatchingCert(certs, address);

                                            if (!activeProxy && liveMailKeys.Contains(liveMailKey))
                                                activeProxy = true;

                                            AccountGrid.Rows.Add("Live Mail", address, liveMailKeys.Contains(liveMailKey), matchingCert, liveMailKey);
                                        }

                                        pos = pos2 + 17;
                                    }
                                    else
                                        pos = -1;
                                }
                            }
                        }
                    }
                }
            }

            // If there's at least one active proxy, ensure the service is running.
            if (activeProxy)
                StartService();
        }

        /// <summary>
        /// Save an embedded resource file to the file system.
        /// </summary>
        /// <param name="resourceFileName">Identifier of the embedded resource.</param>
        /// <param name="resourcePath">Full path to the embedded resource.</param>
        /// <param name="filePath">File system location to save the file.</param>
        private async void SaveResourceFile(string resourceFileName, string resourcePath, string filePath)
        {
            if (!File.Exists(filePath + "\\" + resourceFileName))
            {
                using (StreamReader resourceReader = new StreamReader(Assembly.GetAssembly(GetType()).GetManifestResourceStream(resourcePath + "." + resourceFileName)))
                {
                    using (StreamWriter fileWriter = new StreamWriter(filePath + "\\" + resourceFileName))
                    {
                        char[] buffer = new char[Constants.SMALLBUFFERSIZE];

                        int bytesRead;
                        while ((bytesRead = await resourceReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            await fileWriter.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
        }

        /// <summary>
        /// Confirm the Windows service exists.
        /// </summary>
        /// <param name="serviceName">Name of the WIndows service</param>
        private bool ServiceExists(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.FirstOrDefault(s => s.ServiceName == serviceName) != null;
        }

        /// <summary>
        /// Update Outlook signatures to reference OpaqueMail.
        /// </summary>
        /// <param name="registryKeys">List of registry entries for Outlook accounts to update.</param>
        private void SetOutlookSignatures(List<string> registryKeyLocations)
        {
            string microsoftFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft";
            if (Directory.Exists(microsoftFolder))
            {
                if (!Directory.Exists(microsoftFolder + "\\Signatures"))
                    Directory.CreateDirectory(microsoftFolder + "\\Signatures");
                if (!Directory.Exists(microsoftFolder + "\\Signatures\\OpaqueMail_files"))
                    Directory.CreateDirectory(microsoftFolder + "\\Signatures\\OpaqueMail_files");

                SaveResourceFile("OpaqueMail.htm", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures");
                SaveResourceFile("OpaqueMail.rtf", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures");
                SaveResourceFile("OpaqueMail.txt", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures");
                SaveResourceFile("colorschememapping.xml", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures\\OpaqueMail_files");
                SaveResourceFile("filelist.xml", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures\\OpaqueMail_files");
                SaveResourceFile("themedata.thmx", "OpaqueMail.Net.ProxySettings.Signature.Resources", microsoftFolder + "\\Signatures\\OpaqueMail_files");

                foreach (string keyLocation in registryKeyLocations)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyLocation.Replace("HKEY_CURRENT_USER\\", ""), true))
                    {
                        if (key != null)
                        {
                            string signatureValue = GetOutlookRegistryValue(key, "New Signature");

                            // If there's no signature for this account, set it to OpaqueMail.
                            if (string.IsNullOrEmpty(signatureValue))
                                key.SetValue("New Signature", Encoding.Unicode.GetBytes("OpaqueMail\0"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle S/MIME operation option changes.
        /// </summary>
        private void SmimeOperations_SelectedIndexChanged(object sender, EventArgs e)
        {
            SmimeSettingsMode.Enabled = SmimeOperations.SelectedIndex > 0;
        }

        /// <summary>
        /// Start the OpaqueMail Proxy Windows service.
        /// </summary>
        private void StartService()
        {
            StartService("OpaqueMailProxy");
        }

        /// <summary>
        /// Start a Windows service.
        /// </summary>
        /// <param name="serviceName">Name of the service to start.</param>
        private void StartService(string serviceName)
        {
            if (ServiceExists(serviceName))
            {
                ServiceController serviceContoller = new ServiceController(serviceName);
                if (serviceContoller.Status != ServiceControllerStatus.Running && serviceContoller.Status != ServiceControllerStatus.StartPending)
                    serviceContoller.Start();
            }
        }

        /// <summary>
        /// Stop the OpaqueMail Proxy Windows service.
        /// </summary>
        private void StopService()
        {
            StopService("OpaqueMailProxy");
        }

        /// <summary>
        /// Stop a Windows service.
        /// </summary>
        /// <param name="serviceName">Name of the service to start.</param>
        private void StopService(string serviceName)
        {
            if (ServiceExists(serviceName))
            {
                ServiceController serviceContoller = new ServiceController(serviceName);
                if (serviceContoller.Status != ServiceControllerStatus.Stopped && serviceContoller.Status != ServiceControllerStatus.StopPending)
                    serviceContoller.Stop();
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
        /// Update the service status message.
        /// </summary>
        private void UpdateServiceStatus(object o)
        {
            this.BeginInvoke(new MethodInvoker(delegate
                {
                    if (ServiceExists("OpaqueMailProxy"))
                    {
                        ServiceController serviceContoller = new ServiceController("OpaqueMailProxy");
                        switch (serviceContoller.Status)
                        {
                            case ServiceControllerStatus.ContinuePending:
                            case ServiceControllerStatus.Running:
                                ServiceStatusLabel.Text = "OpaqueMailProxy service running successfully.";
                                ServiceStatusLabel.ForeColor = Color.DarkGreen;
                                break;
                            case ServiceControllerStatus.Paused:
                            case ServiceControllerStatus.PausePending:
                                ServiceStatusLabel.Text = "OpaqueMailProxy service paused.";
                                ServiceStatusLabel.ForeColor = Color.Black;
                                break;
                            case ServiceControllerStatus.StartPending:
                                ServiceStatusLabel.Text = "OpaqueMailProxy service starting.";
                                ServiceStatusLabel.ForeColor = Color.Black;
                                break;
                            case ServiceControllerStatus.Stopped:
                            case ServiceControllerStatus.StopPending:
                                ServiceStatusLabel.Text = "OpaqueMailProxy service stopped.";
                                ServiceStatusLabel.ForeColor = Color.Black;
                                break;
                        }
                    }
                    else
                    {
                        ServiceStatusLabel.Text = "OpaqueMailProxy service not installed.";
                        ServiceStatusLabel.ForeColor = Color.DarkRed;
                    }
                }));
        }
        #endregion Private Methods

        #region Support Classes
        /// <summary>
        /// Represent certificate choices in a grid view.
        /// </summary>
        private class Choice
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public Choice(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        /// <summary>
        /// Track e-mail account settings.
        /// </summary>
        private class ProxyAccount
        {
            public bool Matched = false;

            public string ClientType;
            public string ClientVersion;

            public List<string> OutlookRegistryKeys = new List<string>();
            public List<string> ThunderbirdKeys = new List<string>();
            public List<string> LiveMailKeys = new List<string>();
            public List<string> Usernames = new List<string>();

            public string ImapAcceptedIPs;
            public string Pop3AcceptedIPs;
            public string SmtpAcceptedIPs;

            public bool LocalImapEnableSsl = true;
            public int LocalImapPort = 993;
            public string LocalImapIPAddress = "Any";

            public bool LocalPop3EnableSsl = true;
            public int LocalPop3Port = 995;
            public string LocalPop3IPAddress = "Any";

            public bool LocalSmtpEnableSsl = true;
            public int LocalSmtpPort = 587;
            public string LocalSmtpIPAddress = "Any";

            public bool RemoteImapEnableSsl;
            public int RemoteImapPort;
            public string RemoteImapServer = "";

            public bool RemotePop3EnableSsl;
            public int RemotePop3Port;
            public string RemotePop3Server = "";

            public bool RemoteSmtpEnableSsl;
            public int RemoteSmtpPort;
            public string RemoteSmtpServer = "";
            public string RemoteSmtpUsername;
            public string RemoteSmtpPassword;
            public string FixedFrom;
            public string FixedTo;
            public string FixedCC;
            public string FixedBcc;
            public string Signature;

            public string ImapCertificateLocation;
            public string ImapCertificateSerialNumber;
            public string ImapCertificateSubjectName;
            public string Pop3CertificateLocation;
            public string Pop3CertificateSerialNumber;
            public string Pop3CertificateSubjectName;
            public string SmtpCertificateLocation;
            public string SmtpCertificateSerialNumber;
            public string SmtpCertificateSubjectName;

            public bool SendCertificateReminders = true;
            public string SmimeSettingsModeValue = "BestEffort";

            public bool SmimeSign = true;
            public bool SmimeEncrypt = true;
            public bool SmimeTripleWrap = true;

            public bool SmimeRemovePreviousOperations = true;

            public string ImapExportDirectory;
            public string Pop3ExportDirectory;

            public string ImapLogFile = "Logs\\IMAPProxy{#}-{yyyy-MM-dd}.log";
            public LogLevel ImapLogLevel = LogLevel.Verbose;
            public string Pop3LogFile = "Logs\\POP3Proxy{#}-{yyyy-MM-dd}.log";
            public LogLevel Pop3LogLevel = LogLevel.Verbose;
            public string SmtpLogFile = "Logs\\SMTPProxy{#}-{yyyy-MM-dd}.log";
            public LogLevel SmtpLogLevel = LogLevel.Verbose;
        }
        #endregion Support Classes
    }
}
