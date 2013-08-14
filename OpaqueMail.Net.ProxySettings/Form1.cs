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
using System.Net.NetworkInformation;
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

            // Handle settings file linking.
            int settingsFilePosition = AboutLabel.Text.IndexOf("[SETTINGSFILE]", StringComparison.Ordinal);

            AboutLabel.Links.Clear();
            AboutLabel.Text = AboutLabel.Text.Replace("[SETTINGSFILE]", SettingsFileName);
            AboutLabel.Links.Add(AboutLabel.Text.IndexOf("S/MIME"), 6, "https://en.wikipedia.org/wiki/S/MIME");
            AboutLabel.Links.Add(settingsFilePosition, SettingsFileName.Length, SettingsFileName);
            AboutLabel.LinkClicked += Label_LinkClicked;

            CertificateLabel.Links.Clear();
            CertificateLabel.Links.Add(CertificateLabel.Text.IndexOf("Comodo"), 6, "http://www.instantssl.com/ssl-certificate-products/free-email-certificate.html");
            CertificateLabel.Links.Add(CertificateLabel.Text.IndexOf("StartCom"), 8, "https://cert.startcom.org/");
            CertificateLabel.LinkClicked += Label_LinkClicked;

            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);

            // Load the e-mail accounts list and certificate choices.
            PopulateAccounts();

            SmimeSettingsMode.SelectedIndex = 0;
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

            // Check which Outlook registry keys and Thunderbird configs have proxies associated.
            XPathDocument document;
            HashSet<string> outlookRegistryKeys = new HashSet<string>();
            HashSet<string> thunderbirdKeys = new HashSet<string>();
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
                }
            }
            catch { }

            // Correlate Outlook registry keys with accounts.
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
                                                    matchingCert = cert.SerialNumber;
                                            }
                                            else if (cert.Subject.StartsWith("CN="))
                                            {
                                                canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
                                                int certSubjectComma = canonicalCertSubject.IndexOf(",");
                                                if (certSubjectComma > -1)
                                                    canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                                                if (accountName.ToUpper() == canonicalCertSubject)
                                                    matchingCert = cert.SerialNumber;
                                            }
                                        }

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

            // Correlate Thunderbird config keys with accounts.
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
                                            matchingCert = cert.SerialNumber;
                                    }
                                    else if (cert.Subject.StartsWith("CN="))
                                    {
                                        canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
                                        int certSubjectComma = canonicalCertSubject.IndexOf(",");
                                        if (certSubjectComma > -1)
                                            canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                                        if (accountName.ToUpper() == canonicalCertSubject)
                                            matchingCert = cert.SerialNumber;
                                    }
                                }

                                if (!activeProxy && thunderbirdKeys.Contains(thunderbirdKey))
                                    activeProxy = true;

                                AccountGrid.Rows.Add("Thunderbird", accountName, thunderbirdKeys.Contains(thunderbirdKey), matchingCert, thunderbirdKey);
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
        /// Handle the save event and update OpaqueMail Proxy's settings..
        /// </summary>
        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            XPathDocument document = null;

            if (File.Exists(SettingsFileName))
            {
                DialogResult dr = MessageBox.Show("A settings file already exists for OpaqueMail Proxy.  Overwrite with these settings?", "Overwrite OpaqueMail Proxy Settings?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return;

                try
                {
                    document = new XPathDocument(SettingsFileName);
                }
                catch { }
            }

            // If the service is running, stop it before proceeding.
            if (ServiceExists("OpaqueMailProxy"))
            {
                ServiceController serviceContoller = new ServiceController("OpaqueMailProxy");
                if (serviceContoller.Status != ServiceControllerStatus.Stopped && serviceContoller.Status != ServiceControllerStatus.StopPending)
                    serviceContoller.Stop();
            }

            List<ProxyAccount> accounts = new List<ProxyAccount>();

            // First, account for any settings in the existing XML file.
            string fqdn = Functions.GetLocalFQDN();

            int smtpServiceCount = 0, imapServiceCount = 0, pop3ServiceCount = 0;
            if (document != null)
            {
                XPathNavigator navigator = document.CreateNavigator();

                smtpServiceCount = GetXmlIntValue(navigator, "/Settings/SMTP/ServiceCount") ?? 0;
                for (int i = 1; i <= smtpServiceCount; i++)
                {
                    ProxyAccount account = new ProxyAccount();
                    account.LocalSmtpIPAddress = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LocalIPAddress") ?? "";
                    account.LocalSmtpPort = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/LocalPort") ?? 587;
                    account.LocalSmtpEnableSsl = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/LocalEnableSsl") ?? true;
                    account.RemoteSmtpServer = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerHostName");
                    account.RemoteSmtpPort = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerPort") ?? 587;
                    account.RemoteSmtpEnableSsl = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerEnableSSL") ?? true;
                    account.RemoteSmtpUsername = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerUsername");
                    account.RemoteSmtpPassword = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/RemoteServerPassword");
                    account.SmtpAcceptedIPs = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/AcceptedIPs");
                    account.SmtpCertificateLocation = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/Location");
                    account.SmtpCertificateSerialNumber = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/SerialNumber");
                    account.SmtpCertificateSubjectName = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/Certificate/SubjectName");
                    account.SmtpLogFile = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/LogFile");

                    string logLevel = ProxyFunctions.GetXmlStringValue(navigator, "Settings/SMTP/Service" + i + "/LogLevel");
                    switch (logLevel.ToUpper())
                    {
                        case "NONE":
                            account.SmtpLogLevel = LogLevel.None;
                            break;
                        case "CRITICAL":
                            account.SmtpLogLevel = LogLevel.Critical;
                            break;
                        case "ERROR":
                            account.SmtpLogLevel = LogLevel.Error;
                            break;
                        case "RAW":
                            account.SmtpLogLevel = LogLevel.Raw;
                            break;
                        case "VERBOSE":
                            account.SmtpLogLevel = LogLevel.Verbose;
                            break;
                        case "WARNING":
                            account.SmtpLogLevel = LogLevel.Warning;
                            break;
                        case "INFORMATION":
                        default:
                            account.SmtpLogLevel = LogLevel.Information;
                            break;
                    }

                    account.SendCertificateReminders = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/SendCertificateReminders") ?? true;
                    account.SmimeEncrypt = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/SMIMEEncrypt") ?? true;
                    account.SmimeRemovePreviousOperations = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/SMIMERemovePreviousOperations") ?? true;
                    account.SmimeSign = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/SMIMESign") ?? true;
                    account.SmimeTripleWrap = GetXmlBoolValue(navigator, "/Settings/SMTP/Service" + i + "/SMIMETripleWrap") ?? true;

                    int? registryKeyCount = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/OutlookRegistryKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string registryKey = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/OutlookRegistryKey" + j);
                        if (!account.OutlookRegistryKeys.Contains(registryKey))
                            account.OutlookRegistryKeys.Add(registryKey);
                    }

                    int? thunderbirdKeyCount = GetXmlIntValue(navigator, "/Settings/SMTP/Service" + i + "/ThunderbirdKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string thunderbirdKey = GetXmlStringValue(navigator, "/Settings/SMTP/Service" + i + "/ThunderbirdKey" + j);
                        if (!string.IsNullOrEmpty(thunderbirdKey) && !account.ThunderbirdKeys.Contains(thunderbirdKey))
                            account.ThunderbirdKeys.Add(thunderbirdKey);
                    }

                    accounts.Add(account);
                }

                imapServiceCount = GetXmlIntValue(navigator, "/Settings/IMAP/ServiceCount") ?? 0;
                for (int i = 1; i <= imapServiceCount; i++)
                {
                    ProxyAccount account = new ProxyAccount();
                    bool accountMatched = false;

                    // Check if a matching Outlook account already exists.
                    int? registryKeyCount = GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/OutlookRegistryKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string registryKey = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/OutlookRegistryKey" + j);
                        if (!account.OutlookRegistryKeys.Contains(registryKey))
                            account.OutlookRegistryKeys.Add(registryKey);

                        foreach (ProxyAccount existingAccount in accounts)
                        {
                            if (existingAccount.OutlookRegistryKeys.Contains(registryKey) && !accountMatched)
                            {
                                account = existingAccount;
                                j = 0;
                                accountMatched = true;
                            }
                        }
                    }

                    // Check if a matching Thunderbird account already exists.
                    int? thunderbirdKeyCount = GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/ThunderbirdKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string thunderbirdKey = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/ThunderbirdKey" + j);
                        if (!string.IsNullOrEmpty(thunderbirdKey) && !account.ThunderbirdKeys.Contains(thunderbirdKey))
                            account.ThunderbirdKeys.Add(thunderbirdKey);

                        foreach (ProxyAccount existingAccount in accounts)
                        {
                            if (existingAccount.ThunderbirdKeys.Contains(thunderbirdKey) && !accountMatched)
                            {
                                account = existingAccount;
                                j = 0;
                                accountMatched = true;
                            }
                        }
                    }

                    account.LocalImapIPAddress = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LocalIPAddress") ?? "";
                    account.LocalImapPort = GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/LocalPort") ?? 587;
                    account.LocalImapEnableSsl = GetXmlBoolValue(navigator, "/Settings/IMAP/Service" + i + "/LocalEnableSsl") ?? true;

                    account.RemoteImapServer = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerHostName");
                    account.RemoteImapPort = GetXmlIntValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerPort") ?? 993;
                    account.RemoteImapEnableSsl = GetXmlBoolValue(navigator, "/Settings/IMAP/Service" + i + "/RemoteServerEnableSSL") ?? true;
                    account.ImapAcceptedIPs = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/AcceptedIPs");
                    account.ImapCertificateLocation = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/Location");
                    account.ImapCertificateSerialNumber = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/SerialNumber");
                    account.ImapCertificateSubjectName = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/Certificate/SubjectName");
                    account.ImapLogFile = GetXmlStringValue(navigator, "/Settings/IMAP/Service" + i + "/LogFile");

                    string logLevel = ProxyFunctions.GetXmlStringValue(navigator, "Settings/IMAP/Service" + i + "/LogLevel");
                    switch (logLevel.ToUpper())
                    {
                        case "NONE":
                            account.ImapLogLevel = LogLevel.None;
                            break;
                        case "CRITICAL":
                            account.ImapLogLevel = LogLevel.Critical;
                            break;
                        case "ERROR":
                            account.ImapLogLevel = LogLevel.Error;
                            break;
                        case "RAW":
                            account.ImapLogLevel = LogLevel.Raw;
                            break;
                        case "VERBOSE":
                            account.ImapLogLevel = LogLevel.Verbose;
                            break;
                        case "WARNING":
                            account.ImapLogLevel = LogLevel.Warning;
                            break;
                        case "INFORMATION":
                        default:
                            account.ImapLogLevel = LogLevel.Information;
                            break;
                    }
                }

                // Handle POP3 settings third.
                pop3ServiceCount = GetXmlIntValue(navigator, "/Settings/POP3/ServiceCount") ?? 0;
                for (int i = 1; i <= pop3ServiceCount; i++)
                {
                    ProxyAccount account = new ProxyAccount();
                    bool accountMatched = false;

                    // Check if a matching Outlook account already exists.
                    int? registryKeyCount = GetXmlIntValue(navigator, "/Settings/POP3/Service" + i + "/OutlookRegistryKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string registryKey = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/OutlookRegistryKey" + j);
                        if (!account.OutlookRegistryKeys.Contains(registryKey))
                            account.OutlookRegistryKeys.Add(registryKey);

                        foreach (ProxyAccount existingAccount in accounts)
                        {
                            if (existingAccount.OutlookRegistryKeys.Contains(registryKey) && !accountMatched)
                            {
                                account = existingAccount;
                                j = 0;
                                accountMatched = true;
                            }
                        }
                    }

                    // Check if a matching Thunderbird account already exists.
                    int? thunderbirdKeyCount = GetXmlIntValue(navigator, "/Settings/POP3/Service" + i + "/ThunderbirdKeyCount") ?? 0;
                    for (int j = 1; j <= registryKeyCount; j++)
                    {
                        string thunderbirdKey = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/ThunderbirdKey" + j);
                        if (!string.IsNullOrEmpty(thunderbirdKey) && !account.ThunderbirdKeys.Contains(thunderbirdKey))
                            account.ThunderbirdKeys.Add(thunderbirdKey);

                        foreach (ProxyAccount existingAccount in accounts)
                        {
                            if (existingAccount.ThunderbirdKeys.Contains(thunderbirdKey) && !accountMatched)
                            {
                                account = existingAccount;
                                j = 0;
                                accountMatched = true;
                            }
                        }
                    }

                    account.LocalPop3IPAddress = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/LocalIPAddress") ?? "";
                    account.LocalPop3Port = GetXmlIntValue(navigator, "/Settings/POP3/Service" + i + "/LocalPort") ?? 995;
                    account.LocalPop3EnableSsl = GetXmlBoolValue(navigator, "/Settings/POP3/Service" + i + "/LocalEnableSsl") ?? true;

                    account.RemotePop3Server = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerHostName");
                    account.RemotePop3Port = GetXmlIntValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerPort") ?? 995;
                    account.RemotePop3EnableSsl = GetXmlBoolValue(navigator, "/Settings/POP3/Service" + i + "/RemoteServerEnableSSL") ?? true;
                    account.Pop3AcceptedIPs = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/AcceptedIPs");
                    account.Pop3CertificateLocation = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/Certificate/Location");
                    account.Pop3CertificateSerialNumber = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/Certificate/SerialNumber");
                    account.Pop3CertificateSubjectName = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/Certificate/SubjectName");
                    account.Pop3LogFile = GetXmlStringValue(navigator, "/Settings/POP3/Service" + i + "/LogFile");

                    string logLevel = ProxyFunctions.GetXmlStringValue(navigator, "Settings/POP3/Service" + i + "/LogLevel");
                    switch (logLevel.ToUpper())
                    {
                        case "NONE":
                            account.Pop3LogLevel = LogLevel.None;
                            break;
                        case "CRITICAL":
                            account.Pop3LogLevel = LogLevel.Critical;
                            break;
                        case "ERROR":
                            account.Pop3LogLevel = LogLevel.Error;
                            break;
                        case "RAW":
                            account.Pop3LogLevel = LogLevel.Raw;
                            break;
                        case "VERBOSE":
                            account.Pop3LogLevel = LogLevel.Verbose;
                            break;
                        case "WARNING":
                            account.Pop3LogLevel = LogLevel.Warning;
                            break;
                        case "INFORMATION":
                        default:
                            account.Pop3LogLevel = LogLevel.Information;
                            break;
                    }
                }
            }

            // Second, gather existing Outlook account settings from the registry.
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
                                    bool matched = false;
                                    foreach (ProxyAccount existingAccount in accounts)
                                    {
                                        if (existingAccount.OutlookRegistryKeys.Contains(subKey.Name))
                                            matched = true;
                                    }

                                    if (!matched)
                                    {
                                        ProxyAccount account = new ProxyAccount();
                                        account.ClientType = "Outlook";
                                        account.ClientVersion = outlookVersion;

                                        account.RemoteImapEnableSsl = GetOutlookRegistryValue(subKey, "IMAP Use SSL") == "1";
                                        int.TryParse(GetOutlookRegistryValue(subKey, "IMAP Port"), out account.RemoteImapPort);
                                        account.RemoteImapServer = GetOutlookRegistryValue(subKey, "IMAP Server") ?? "";

                                        account.RemotePop3EnableSsl = GetOutlookRegistryValue(subKey, "POP3 Use SSL") == "1";
                                        int.TryParse(GetOutlookRegistryValue(subKey, "POP3 Port"), out account.RemotePop3Port);
                                        account.RemotePop3Server = GetOutlookRegistryValue(subKey, "POP3 Server") ?? "";

                                        account.RemoteSmtpEnableSsl = GetOutlookRegistryValue(subKey, "SMTP Use SSL") == "1";
                                        int.TryParse(GetOutlookRegistryValue(subKey, "SMTP Port"), out account.RemoteSmtpPort);
                                        account.RemoteSmtpServer = GetOutlookRegistryValue(subKey, "SMTP Server") ?? "";

                                        // Only proceed if a server is found.
                                        if (!string.IsNullOrEmpty(account.RemoteImapServer) || !string.IsNullOrEmpty(account.RemotePop3Server) || !string.IsNullOrEmpty(account.RemoteSmtpServer))
                                        {
                                            string username = GetOutlookRegistryValue(subKey, "IMAP User");
                                            if (string.IsNullOrEmpty(username))
                                                username = GetOutlookRegistryValue(subKey, "POP3 User");

                                            if (!string.IsNullOrEmpty(username))
                                            {
                                                account.Usernames.Add(username);
                                                account.OutlookRegistryKeys.Add(subKey.Name);

                                                accounts.Add(account);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Third, gather existing Thunderbird account settings.
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
                            string thunderbirdKey = directory + "~" + i.ToString();

                            bool matched = false;
                            foreach (ProxyAccount existingAccount in accounts)
                            {
                                if (existingAccount.ThunderbirdKeys.Contains(thunderbirdKey))
                                    matched = true;
                            }

                            if (!matched)
                            {
                                ProxyAccount account = new ProxyAccount();
                                account.ClientType = "Thunderbird";

                                int sslValue = 0;
                                int.TryParse(Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".try_ssl\", ", ")"), out sslValue);
                                account.RemoteSmtpEnableSsl = sslValue > 0;
                                int.TryParse(Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".port\", ", ")"), out account.RemoteSmtpPort);
                                account.RemoteSmtpServer = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".hostname\", \"", "\"") ?? "";

                                if (Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".type\", \"", "\"") == "pop3")
                                {
                                    int.TryParse(Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")"), out account.RemotePop3Port);
                                    account.RemotePop3EnableSsl = (account.RemotePop3Port == 995);
                                    account.RemotePop3Server = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"") ?? "";
                                }
                                else
                                {
                                    int.TryParse(Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")"), out account.RemoteImapPort);
                                    account.RemoteImapEnableSsl = (account.RemoteImapPort == 993);
                                    account.RemoteImapServer = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"") ?? "";
                                }

                                // Only proceed if a server is found.
                                if (!string.IsNullOrEmpty(account.RemoteImapServer) || !string.IsNullOrEmpty(account.RemotePop3Server) || !string.IsNullOrEmpty(account.RemoteSmtpServer))
                                {
                                    string username = Functions.ReturnBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".username\", \"", "\"");

                                    if (!string.IsNullOrEmpty(username))
                                    {
                                        account.Usernames.Add(username);
                                        account.ThunderbirdKeys.Add(thunderbirdKey);

                                        accounts.Add(account);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Fourth, check which accounts the user chooses to encrypt.
            smtpServiceCount = 0;
            imapServiceCount = 0;
            pop3ServiceCount = 0;

            HashSet<int> portsReserved = new HashSet<int>();
            int nextPortToTry = 1000;
            
            foreach (DataGridViewRow row in AccountGrid.Rows)
            {
                if ((bool)row.Cells[2].Value == true)
                {
                    foreach (ProxyAccount account in accounts)
                    {
                        if ((account.OutlookRegistryKeys.Contains((string)row.Cells[4].Value) || account.ThunderbirdKeys.Contains((string)row.Cells[4].Value)) && !account.Matched)
                        {
                            account.Matched = true;

                            // Ensure the SMTP proxy connection has a unique port.
                            if (!string.IsNullOrEmpty(account.RemoteSmtpServer))
                            {
                                smtpServiceCount++;

                                if (portsReserved.Contains(account.LocalSmtpPort))
                                {
                                    nextPortToTry = GetNextAvailablePort(++nextPortToTry);
                                    account.LocalSmtpPort = nextPortToTry;
                                }

                                portsReserved.Add(account.LocalSmtpPort);
                            }

                            // Ensure the IMAP proxy connection has a unique port.
                            if (!string.IsNullOrEmpty(account.RemoteImapServer))
                            {
                                imapServiceCount++;

                                if (portsReserved.Contains(account.LocalImapPort))
                                {
                                    nextPortToTry = GetNextAvailablePort(++nextPortToTry);
                                    account.LocalImapPort = nextPortToTry;
                                }

                                portsReserved.Add(account.LocalImapPort);
                            }

                            // Ensure the POP3 proxy connection has a unique port.
                            if (!string.IsNullOrEmpty(account.RemotePop3Server))
                            {
                                pop3ServiceCount++;

                                if (portsReserved.Contains(account.LocalPop3Port))
                                {
                                    nextPortToTry = GetNextAvailablePort(++nextPortToTry);
                                    account.LocalPop3Port = nextPortToTry;
                                }

                                portsReserved.Add(account.LocalPop3Port);
                            }
                        }
                    }
                }
            }

            // Fifth, write out the XML setting values.
            XmlWriterSettings streamWriterSettings = new XmlWriterSettings();
            streamWriterSettings.Indent = true;
            streamWriterSettings.IndentChars = "  ";
            streamWriterSettings.NewLineChars = "\r\n";
            streamWriterSettings.NewLineHandling = NewLineHandling.Replace;

            using (XmlWriter streamWriter = XmlWriter.Create(SettingsFileName, streamWriterSettings))
            {
                streamWriter.WriteStartDocument();

                streamWriter.WriteStartElement("Settings");

                streamWriter.WriteStartElement("SMTP");

                streamWriter.WriteComment("The number of SMTP proxy services to run.  Each proxy's settings will be outlined in subsequent <Service#/> blocks.");
                streamWriter.WriteElementString("ServiceCount", smtpServiceCount.ToString());

                int smtpServiceId = 0;
                foreach (ProxyAccount account in accounts)
                {
                    if (!string.IsNullOrEmpty(account.RemoteSmtpServer) && account.Matched)
                    {
                        streamWriter.WriteStartElement("Service" + (++smtpServiceId).ToString());

                        streamWriter.WriteComment("IP addresses to accept connections from.  Delete or set value to \"*\" to accept connections from any IP.");
                        streamWriter.WriteComment("Individual IPs can be specified, separated by commas, or ranges can be specified.  The \"*\" wildcard character is supported.");
                        streamWriter.WriteComment("By default, connections are only accepted from the localhost.");
                        streamWriter.WriteElementString("AcceptedIPs", account.ImapAcceptedIPs ?? "0.0.0.0");

                        streamWriter.WriteComment("Local IP address to listen on.  \"Any\" means listen on all IPs.");
                        streamWriter.WriteElementString("LocalIPAddress", account.LocalSmtpIPAddress ?? "Any");
                        streamWriter.WriteComment("Local port to listen on.");
                        streamWriter.WriteElementString("LocalPort", account.LocalSmtpPort > 0 ? account.LocalSmtpPort.ToString() : "587");
                        streamWriter.WriteComment("Whether local connections support TLS/SSL protection.");
                        streamWriter.WriteElementString("LocalEnableSSL", account.LocalSmtpEnableSsl.ToString());

                        streamWriter.WriteComment("Remote SMTP server hostname to connect to.  Common values: smtp.gmail.com, smtp.live.com, smtp.mail.yahoo.com");
                        streamWriter.WriteElementString("RemoteServerHostName", account.RemoteSmtpServer ?? "Any");
                        streamWriter.WriteComment("Remote SMTP server port to connect to.  587 is recommended, but 465 or 25 may be required.");
                        streamWriter.WriteElementString("RemoteServerPort", account.RemoteSmtpPort > 0 ? account.RemoteSmtpPort.ToString() : "587");
                        streamWriter.WriteComment("Whether the remote SMTP server supports TLS/SSL protection.");
                        streamWriter.WriteElementString("RemoteServerEnableSSL", account.RemoteSmtpEnableSsl.ToString());
                        
                        streamWriter.WriteComment("(Optional) Username used when authenticating to the remote SMTP server.  When supplied, it will override any values sent from the client.");
                        streamWriter.WriteElementString("RemoteServerUsername", account.RemoteSmtpUsername);
                        streamWriter.WriteComment("(Optional) Password used when authenticating to the remote SMTP server.  When supplied, it will override any values sent from the client.");
                        streamWriter.WriteElementString("RemoteServerPassword", account.RemoteSmtpPassword);
                        
                        streamWriter.WriteStartElement("Certificate");
                        streamWriter.WriteComment("Where certificates should be stored and retrieved from by default.  \"LocalMachine\" or \"CurrentUser\" only.");
                        streamWriter.WriteElementString("Location", account.SmtpCertificateLocation ?? "LocalMachine");
                        streamWriter.WriteComment("(Optional) The serial number of an X509 certificate to be used for server identification.  If left blank, one will be autogenerated.");
                        streamWriter.WriteElementString("SerialNumber", account.SmtpCertificateSerialNumber);
                        streamWriter.WriteComment("(Optional) The subject name of an X509 certificate to be used for server identification.  If left blank, one will be autogenerated.");
                        streamWriter.WriteElementString("SubjectName", account.SmtpCertificateSubjectName);
                        streamWriter.WriteEndElement();
                        
                        streamWriter.WriteComment("Send e-mail reminders when a signing certificate is due to expire within 30 days.");
                        streamWriter.WriteElementString("SendCertificateReminders", account.SendCertificateReminders.ToString());
                        
                        streamWriter.WriteComment("Whether all outgoing messages require the S/MIME settings specified below.");
                        streamWriter.WriteComment("When set to \"RequireExactSettings\", any messages that can't be signed or encrypted will be dropped, unsent.");
                        streamWriter.WriteComment("When set to \"BestEffort\", OpaqueMail Proxy will attempt to sign and/or encrypt messages but still forward any that can't be.");
                        streamWriter.WriteElementString("SMIMESettingsMode", SmimeSettingsMode.SelectedIndex > 0 ? "RequireExactSettings" : "BestEffort");
                        
                        streamWriter.WriteComment("Whether to sign the e-mail.  When true, signing is the first S/MIME operation.");
                        streamWriter.WriteElementString("SMIMESign", account.SmimeSign.ToString());
                        streamWriter.WriteComment("Whether to encrypt the e-mail's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.");
                        streamWriter.WriteElementString("SMIMEEncrypt", account.SmimeEncrypt.ToString());
                        streamWriter.WriteComment("Triple-wrap the e-mail by signing, then encrypting the envelope, then signing the encrypted envelope.");
                        streamWriter.WriteElementString("SMIMETripleWrap", account.SmimeTripleWrap.ToString());
                        
                        streamWriter.WriteComment("Remove envelope encryption and signatures from passed-in messages.  If true and SmimeSigned or SmimeEncryptEnvelope is also true, new S/MIME operations will be applied.");
                        streamWriter.WriteElementString("SMIMERemovePreviousOperations", account.SmimeRemovePreviousOperations.ToString());
                        
                        streamWriter.WriteComment("Where log files should be stored, if any.  Leave blank to avoid logging.");
                        streamWriter.WriteComment("Date and instance variables can be encased in angle braces.  For example, \"Logs\\SMTPProxy{#}-{yyyy-MM-dd}.log\".");
                        streamWriter.WriteElementString("LogFile", account.SmtpLogFile ?? "Logs\\SMTPProxy{#}-{yyyy-MM-dd}.log");
                        
                        streamWriter.WriteComment("Proxy logging level, determining how much information is logged.  Possible values: None, Critical, Error, Warning, Information, Verbose, Raw");
                        streamWriter.WriteElementString("LogLevel", account.SmtpLogLevel.ToString());

                        if (account.OutlookRegistryKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Outlook registry keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("OutlookRegistryKeyCount", account.OutlookRegistryKeys.Count.ToString());

                            int registryKeyId = 0;
                            foreach (string registryKey in account.OutlookRegistryKeys)
                                streamWriter.WriteElementString("OutlookRegistryKey" + (++registryKeyId).ToString(), registryKey);
                        }

                        if (account.ThunderbirdKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Thunderbird keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("ThunderbirdKeyCount", account.ThunderbirdKeys.Count.ToString());

                            int thunderbirdKeyId = 0;
                            foreach (string thunderbirdKey in account.ThunderbirdKeys)
                                streamWriter.WriteElementString("ThunderbirdKey" + (++thunderbirdKeyId).ToString(), thunderbirdKey);
                        }

                        streamWriter.WriteEndElement();
                    }
                }

                streamWriter.WriteEndElement();

                streamWriter.WriteStartElement("IMAP");

                streamWriter.WriteComment("The number of IMAP proxy services to run.  Each proxy's settings will be outlined in subsequent <Service#/> blocks.");
                streamWriter.WriteElementString("ServiceCount", imapServiceCount.ToString());

                int imapServiceId = 0;
                foreach (ProxyAccount account in accounts)
                {
                    if (!string.IsNullOrEmpty(account.RemoteImapServer) && account.Matched)
                    {
                        streamWriter.WriteStartElement("Service" + (++imapServiceId).ToString());

                        streamWriter.WriteComment("IP addresses to accept connections from.  Delete or set value to \"*\" to accept connections from any IP.");
                        streamWriter.WriteComment("Individual IPs can be specified, separated by commas, or ranges can be specified.  The \"*\" wildcard character is supported.");
                        streamWriter.WriteComment("By default, connections are only accepted from the localhost.");
                        streamWriter.WriteElementString("AcceptedIPs", account.ImapAcceptedIPs ?? "0.0.0.0");
                        
                        streamWriter.WriteComment("Local IP address to listen on.  \"Any\" means listen on all IPs.");
                        streamWriter.WriteElementString("LocalIPAddress", account.LocalImapIPAddress ?? "Any");
                        streamWriter.WriteComment("Local port to listen on.");
                        streamWriter.WriteElementString("LocalPort", account.LocalImapPort > 0 ? account.LocalImapPort.ToString() : "993");
                        streamWriter.WriteComment("Whether local connections support TLS/SSL protection.");
                        streamWriter.WriteElementString("LocalEnableSSL", account.LocalImapEnableSsl.ToString());
                        
                        streamWriter.WriteComment("Remote IMAP server hostname to connect to.  Common values: imap.gmail.com, imap.mail.yahoo.com");
                        streamWriter.WriteElementString("RemoteServerHostName", account.RemoteImapServer ?? "Any");
                        streamWriter.WriteComment("Remote IMAP server port to connect to.  993 is recommended, but 143 may be required.");
                        streamWriter.WriteElementString("RemoteServerPort", account.RemoteImapPort > 0 ? account.RemoteImapPort.ToString() : "993");
                        streamWriter.WriteComment("Whether the remote IMAP server supports TLS/SSL protection.");
                        streamWriter.WriteElementString("RemoteServerEnableSSL", account.RemoteImapEnableSsl.ToString());
                       
                        streamWriter.WriteComment("Where log files should be stored, if any.  Leave blank to avoid logging.");
                        streamWriter.WriteComment("Date and instance variables can be encased in angle braces.  For example, \"Logs\\IMAPProxy{#}-{yyyy-MM-dd}.log\".");
                        streamWriter.WriteElementString("LogFile", account.ImapLogFile ?? "Logs\\IMAPProxy{#}-{yyyy-MM-dd}.log");

                        streamWriter.WriteComment("Proxy logging level, determining how much information is logged.  Possible values: None, Critical, Error, Warning, Information, Verbose, Raw");
                        streamWriter.WriteElementString("LogLevel", account.ImapLogLevel.ToString());

                        if (account.OutlookRegistryKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Outlook registry keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("OutlookRegistryKeyCount", account.OutlookRegistryKeys.Count.ToString());

                            int registryKeyId = 0;
                            foreach (string registryKey in account.OutlookRegistryKeys)
                                streamWriter.WriteElementString("OutlookRegistryKey" + (++registryKeyId).ToString(), registryKey);
                        }

                        if (account.ThunderbirdKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Thunderbird keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("ThunderbirdKeyCount", account.ThunderbirdKeys.Count.ToString());

                            int thunderbirdKeyId = 0;
                            foreach (string thunderbirdKey in account.ThunderbirdKeys)
                                streamWriter.WriteElementString("ThunderbirdKey" + (++thunderbirdKeyId).ToString(), thunderbirdKey);
                        }

                        streamWriter.WriteEndElement();
                    }
                }

                streamWriter.WriteEndElement();

                streamWriter.WriteStartElement("POP3");

                streamWriter.WriteComment("The number of POP3 proxy services to run.  Each proxy's settings will be outlined in subsequent <Service#/> blocks.");
                streamWriter.WriteElementString("ServiceCount", pop3ServiceCount.ToString());

                int pop3ServiceId = 0;
                foreach (ProxyAccount account in accounts)
                {
                    if (!string.IsNullOrEmpty(account.RemotePop3Server) && account.Matched)
                    {
                        streamWriter.WriteStartElement("Service" + (++pop3ServiceId).ToString());

                        streamWriter.WriteComment("IP addresses to accept connections from.  Delete or set value to \"*\" to accept connections from any IP.");
                        streamWriter.WriteComment("Individual IPs can be specified, separated by commas, or ranges can be specified.  The \"*\" wildcard character is supported.");
                        streamWriter.WriteComment("By default, connections are only accepted from the localhost.");
                        streamWriter.WriteElementString("AcceptedIPs", account.Pop3AcceptedIPs ?? "0.0.0.0");
                        
                        streamWriter.WriteComment("Local IP address to listen on.  \"Any\" means listen on all IPs.");
                        streamWriter.WriteElementString("LocalIPAddress", account.LocalPop3IPAddress ?? "Any");
                        streamWriter.WriteComment("Local port to listen on.");
                        streamWriter.WriteElementString("LocalPort", account.LocalPop3Port > 0 ? account.LocalPop3Port.ToString() : "995");
                        streamWriter.WriteComment("Whether local connections support TLS/SSL protection.");
                        streamWriter.WriteElementString("LocalEnableSSL", account.LocalPop3EnableSsl.ToString());
                        
                        streamWriter.WriteComment("Remote POP3 server hostname to connect to.  Common values: pop.gmail.com, pop3.live.com, pop.mail.yahoo.com");
                        streamWriter.WriteElementString("RemoteServerHostName", account.RemotePop3Server ?? "Any");
                        streamWriter.WriteComment("Remote POP3 server port to connect to.  995 is recommended, but 110 may be required.");
                        streamWriter.WriteElementString("RemoteServerPort", account.RemotePop3Port > 0 ? account.RemotePop3Port.ToString() : "995");
                        streamWriter.WriteComment("Whether the remote POP3 server supports TLS/SSL protection.");
                        streamWriter.WriteElementString("RemoteServerEnableSSL", account.RemotePop3EnableSsl.ToString());
                        
                        streamWriter.WriteComment("Where log files should be stored, if any.  Leave blank to avoid logging.");
                        streamWriter.WriteComment("Date and instance variables can be encased in angle braces.  For example, \"Logs\\POP3Proxy{#}-{yyyy-MM-dd}.log\".");
                        streamWriter.WriteElementString("LogFile", account.Pop3LogFile ?? "Logs\\POP3Proxy{#}-{yyyy-MM-dd}.log");

                        streamWriter.WriteComment("Proxy logging level, determining how much information is logged.  Possible values: None, Critical, Error, Warning, Information, Verbose, Raw");
                        streamWriter.WriteElementString("LogLevel", account.Pop3LogLevel.ToString());

                        if (account.OutlookRegistryKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Outlook registry keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("OutlookRegistryKeyCount", account.OutlookRegistryKeys.Count.ToString());

                            int registryKeyId = 0;
                            foreach (string registryKey in account.OutlookRegistryKeys)
                                streamWriter.WriteElementString("OutlookRegistryKey" + (++registryKeyId).ToString(), registryKey);
                        }

                        if (account.ThunderbirdKeys.Count > 0)
                        {
                            streamWriter.WriteComment("Thunderbird keys for accounts configured through the OpaqueMail Proxy settings app.");
                            streamWriter.WriteElementString("ThunderbirdKeyCount", account.ThunderbirdKeys.Count.ToString());

                            int thunderbirdKeyId = 0;
                            foreach (string thunderbirdKey in account.ThunderbirdKeys)
                                streamWriter.WriteElementString("ThunderbirdKey" + (++thunderbirdKeyId).ToString(), thunderbirdKey);
                        }

                        streamWriter.WriteEndElement();
                    }
                }

                streamWriter.WriteEndElement();

                streamWriter.WriteEndElement();
            }

            // Sixth, restart the OpaqueMail service.
            InstallService();
            StartService();

            // Seventh, rewrite the Outlook registry values.
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
                                using (RegistryKey subKey = key.OpenSubKey(subkeyName, true))
                                {
                                    string smtpServer = GetOutlookRegistryValue(subKey, "SMTP Server");
                                    if (!string.IsNullOrEmpty(smtpServer))
                                    {
                                        foreach (ProxyAccount account in accounts)
                                        {
                                            // If matched, set to use the local proxy.  If not matched and we previously used the local proxy, switch back to the original value.
                                            if (account.OutlookRegistryKeys.Contains(subKey.Name))
                                            {
                                                if (account.Matched)
                                                {
                                                    subKey.SetValue("SMTP Server", Encoding.Unicode.GetBytes(fqdn + "\0"));
                                                    subKey.SetValue("SMTP Port", account.LocalSmtpPort);
                                                    subKey.SetValue("SMTP Use SSL", account.LocalSmtpEnableSsl ? 1 : 0);
                                                }
                                                else
                                                {
                                                    subKey.SetValue("SMTP Server", Encoding.Unicode.GetBytes(account.RemoteSmtpServer));
                                                    subKey.SetValue("SMTP Port", account.RemoteSmtpPort);
                                                    subKey.SetValue("SMTP Use SSL", account.RemoteSmtpEnableSsl ? 1 : 0);
                                                }
                                            }
                                        }
                                    }

                                    string imapServer = GetOutlookRegistryValue(subKey, "IMAP Server");
                                    if (!string.IsNullOrEmpty(imapServer))
                                    {
                                        foreach (ProxyAccount account in accounts)
                                        {
                                            // If matched, set to use the local proxy.  If not matched and we previously used the local proxy, switch back to the original value.
                                            if (account.OutlookRegistryKeys.Contains(subKey.Name))
                                            {
                                                if (account.Matched)
                                                {
                                                    subKey.SetValue("IMAP Server", Encoding.Unicode.GetBytes(fqdn + "\0"));
                                                    subKey.SetValue("IMAP Port", account.LocalImapPort);
                                                    subKey.SetValue("IMAP Use SSL", account.LocalImapEnableSsl ? 1 : 0);
                                                }
                                                else
                                                {
                                                    subKey.SetValue("IMAP Server", Encoding.Unicode.GetBytes(account.RemoteImapServer + "\0"));
                                                    subKey.SetValue("IMAP Port", account.RemoteImapPort);
                                                    subKey.SetValue("IMAP Use SSL", account.RemoteImapEnableSsl ? 1 : 0);
                                                }
                                            }
                                        }
                                    }

                                    string pop3Server = GetOutlookRegistryValue(subKey, "POP3 Server");
                                    if (!string.IsNullOrEmpty(pop3Server))
                                    {
                                        foreach (ProxyAccount account in accounts)
                                        {
                                            // If matched, set to use the local proxy.  If not matched and we previously used the local proxy, switch back to the original value.
                                            if (account.OutlookRegistryKeys.Contains(subKey.Name))
                                            {
                                                if (account.Matched)
                                                {
                                                    subKey.SetValue("POP3 Server", Encoding.Unicode.GetBytes(fqdn + "\0"));
                                                    subKey.SetValue("POP3 Port", account.LocalPop3Port);
                                                    subKey.SetValue("POP3 Use SSL", account.LocalPop3EnableSsl ? 1 : 0);
                                                }
                                                else
                                                {
                                                    subKey.SetValue("POP3 Server", Encoding.Unicode.GetBytes(account.RemotePop3Server + "\0"));
                                                    subKey.SetValue("POP3 Port", account.RemotePop3Port);
                                                    subKey.SetValue("POP3 Use SSL", account.RemotePop3EnableSsl ? 1 : 0);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Eighth, rewrite the Thunderbird registry values.
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
                            string thunderbirdKey = directory + "~" + i.ToString();

                            foreach (ProxyAccount account in accounts)
                            {
                                // If matched, set to use the local proxy.  If not matched and we previously used the local proxy, switch back to the original value.
                                if (account.ThunderbirdKeys.Contains(thunderbirdKey))
                                {
                                    if (account.Matched)
                                    {
                                        if (Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".type\", \"", "\"").ToLower() == "pop3")
                                        {
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"", fqdn);
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")", account.LocalPop3Port.ToString());
                                        }
                                        else
                                        {
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"", fqdn);
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")", account.LocalImapPort.ToString());
                                        }

                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".hostname\", \"", "\"", fqdn);
                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".port\", ", ")", account.LocalSmtpPort.ToString());
                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".is_gmail\", ", ")", "false");
                                    }
                                    else
                                    {
                                        if (Functions.ReturnBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".type\", \"", "\"").ToLower() == "pop3")
                                        {
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"", account.RemotePop3Server);
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")", account.RemotePop3Port.ToString());
                                        }
                                        else
                                        {
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".hostname\", \"", "\"", account.RemoteImapServer);
                                            prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".port\", ", ")", account.RemoteImapPort.ToString());
                                        }

                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".hostname\", \"", "\"", account.RemoteSmtpServer);
                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.smtpserver.smtp" + i.ToString() + ".port\", ", ")", account.RemoteSmtpPort.ToString());

                                        bool isGmail = account.RemoteSmtpServer.ToUpper() == "SMTP.GMAIL.COM" || account.RemoteSmtpServer.ToUpper() == "SMTP.GOOGLEMAIL.COM";
                                        prefsFile = Functions.ReplaceBetween(prefsFile, "user_pref(\"mail.server.server" + i.ToString() + ".is_gmail\", ", ")", isGmail ? "true" : "false");
                                    }
                                }
                            }
                        }

                        // Write the settings file back.
                        File.WriteAllBytes(directory + "\\prefs.js", Encoding.UTF8.GetBytes(prefsFile));
                    }
                }
            }

            // Finally, prompt to restart Outlook or Thunderbird.
            Process[] processes = Process.GetProcessesByName("OUTLOOK");
            if (processes.Length > 0)
            {
                DialogResult dr = MessageBox.Show("Outlook is currently running and will need to be restarted before these changes will take effect.  Would you like to restart Outlook now?", "Restart Outlook?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    // Stop Outlook.
                    foreach (Process process in processes)
                        process.Kill();

                    // Try to start Outlook.
                    for (int i = 15; i >= 8; i--)
                    {
                        if (File.Exists("C:\\Program Files (x86)\\Microsoft Office\\Office" + i.ToString() + "\\Outlook.exe"))
                        {
                            Process.Start("C:\\Program Files (x86)\\Microsoft Office\\Office" + i.ToString() + "\\Outlook.exe");
                            break;
                        }
                        else if (File.Exists("C:\\Program Files\\Microsoft Office\\Office" + i.ToString() + "\\Outlook.exe"))
                        {
                            Process.Start("C:\\Program Files\\Microsoft Office\\Office" + i.ToString() + "\\Outlook.exe");
                            break;
                        }
                    }
                }
            }
            processes = Process.GetProcessesByName("THUNDERBIRD");
            if (processes.Length > 0)
            {
                DialogResult dr = MessageBox.Show("Thunderbird is currently running and will need to be restarted before these changes will take effect.  Would you like to restart Thunderbird now?", "Restart Thunderbird?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    // Stop Thunderbird.
                    foreach (Process process in processes)
                        process.Kill();

                    // Try to start Thunderbird.
                    if (File.Exists("C:\\Program Files (x86)\\Mozilla Thunderbird\\Thunderbird.exe"))
                        Process.Start("C:\\Program Files (x86)\\Mozilla Thunderbird\\Thunderbird.exe");
                }
            }

            MessageBox.Show("OpaqueMail Proxy has been successfully configured and the Windows Service is now running.\r\n\r\nYou may close this program and the proxy will continue to run in the background.", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        #endregion Private Methods

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
            public List<string> Usernames = new List<string>();

            public string ImapAcceptedIPs = "0.0.0.0";
            public string Pop3AcceptedIPs = "0.0.0.0";
            public string SmtpAcceptedIPs = "0.0.0.0";

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

            public string ImapLogFile = "Logs\\IMAPProxy{#}-{yyyy-MM-dd}.log";
            public LogLevel ImapLogLevel = LogLevel.Verbose;
            public string Pop3LogFile = "Logs\\POP3Proxy{#}-{yyyy-MM-dd}.log";
            public LogLevel Pop3LogLevel = LogLevel.Verbose;
            public string SmtpLogFile = "Logs\\SMTPProxy{#}-{yyyy-MM-dd}.log";
            public LogLevel SmtpLogLevel = LogLevel.Verbose;
        }
    }
}
