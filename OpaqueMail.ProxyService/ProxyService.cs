/*
 * OpaqueMail Proxy (http://opaquemail.org/).
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

using OpaqueMail.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace OpaqueMail.Proxy
{
    /// <summary>
    /// Service to proxy SMTP, IMAP, and POP3 traffic.
    /// </summary>
    public partial class ProxyService : ServiceBase
    {
        #region Private Members
        /// <summary>List of all proxies that have been started.</summary>
        private List<ImapProxy> imapProxies = null;
        private List<Pop3Proxy> pop3Proxies = null;
        private List<SmtpProxy> smtpProxies = null;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProxyService()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Protected Methods
        /// <summary>
        /// Handle the service start event by reading the settings file and starting all specified proxy instances.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            imapProxies = ImapProxy.StartProxiesFromSettingsFile(GetSettingsFileName());
            pop3Proxies = Pop3Proxy.StartProxiesFromSettingsFile(GetSettingsFileName());
            smtpProxies = SmtpProxy.StartProxiesFromSettingsFile(GetSettingsFileName());
        }

        /// <summary>
        /// Handle the service stop event by stopping all proxies.
        /// </summary>
        protected override void OnStop()
        {
            if (imapProxies != null)
            {
                foreach (ImapProxy imapProxy in imapProxies)
                    imapProxy.Stop();

                imapProxies.Clear();
            }
            if (pop3Proxies != null)
            {
                foreach (Pop3Proxy pop3Proxy in pop3Proxies)
                    pop3Proxy.Stop();

                pop3Proxies.Clear();
            }
            if (smtpProxies != null)
            {
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.Stop();

                smtpProxies.Clear();
            }
        }

        /// <summary>
        /// Handle service continuations following pauses.
        /// </summary>
        protected override void OnContinue()
        {
            if (imapProxies != null)
            {
                foreach (ImapProxy imapProxy in imapProxies)
                    imapProxy.ProcessContinuation();

                imapProxies.Clear();
            }
            if (pop3Proxies != null)
            {
                foreach (Pop3Proxy pop3Proxy in pop3Proxies)
                    pop3Proxy.ProcessContinuation();

                pop3Proxies.Clear();
            }
            if (smtpProxies != null)
            {
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.ProcessContinuation();

                smtpProxies.Clear();
            }
        }

        /// <summary>
        /// Handle pause event.
        /// </summary>
        protected override void OnPause()
        {
            if (imapProxies != null)
            {
                foreach (ImapProxy imapProxy in imapProxies)
                    imapProxy.ProcessPause();

                imapProxies.Clear();
            }
            if (pop3Proxies != null)
            {
                foreach (Pop3Proxy pop3Proxy in pop3Proxies)
                    pop3Proxy.ProcessPause();

                pop3Proxies.Clear();
            }
            if (smtpProxies != null)
            {
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.ProcessPause();

                smtpProxies.Clear();
            }
        }

        /// <summary>
        /// Handle power events, such as hibernation.
        /// </summary>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (imapProxies != null)
            {
                foreach (ImapProxy imapProxy in imapProxies)
                    imapProxy.ProcessPowerEvent((int)powerStatus);

                imapProxies.Clear();
            }
            if (pop3Proxies != null)
            {
                foreach (Pop3Proxy pop3Proxy in pop3Proxies)
                    pop3Proxy.ProcessPowerEvent((int)powerStatus);

                pop3Proxies.Clear();
            }
            if (smtpProxies != null)
            {
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.ProcessPowerEvent((int)powerStatus);

                smtpProxies.Clear();
            }

            return base.OnPowerEvent(powerStatus);
        }
        #endregion Protected Methods

        #region Private Methods
        /// <summary>
        /// Return the path where the service's settings should be saved and loaded.
        /// </summary>
        private static string GetSettingsFileName()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "\\OpaqueMail.Proxy.xml";
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Sets the service account to the local system.
    /// </summary>
    [RunInstaller(true)]
    public sealed class ProxyServiceProcessInstaller : ServiceProcessInstaller
    {
        public ProxyServiceProcessInstaller()
        {
            Account = ServiceAccount.LocalSystem;
        }
    }

    /// <summary>
    /// Handles OpaqueMail Proxy service installation.
    /// </summary>
    [RunInstaller(true)]
    public sealed class ProxyServiceInstaller : ServiceInstaller
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProxyServiceInstaller()
        {
            Description = "Serves as a local SMTP, IMAP, and POP3 proxy, optionally adding authentication and S/MIME signing or encryption to all outbound email.";
            DisplayName = "OpaqueMail Proxy";
            ServiceName = "OpaqueMailProxy";
            StartType = ServiceStartMode.Automatic;
        }

        /// <summary>
        /// Handle installation and uninstallation.
        /// </summary>
        /// <param name="uninstall">Whether we're uninstalling.  False if installing, true if uninstalling</param>
        /// <param name="args">Any service installation arguments.</param>
        public void Install(bool uninstall, string[] args)
        {
            try
            {
                using (AssemblyInstaller installer = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    installer.UseNewContext = true;
                    try
                    {
                        // Attempt to install or uninstall.
                        if (uninstall)
                            installer.Uninstall(state);
                        else
                        {
                            installer.Install(state);
                            installer.Commit(state);
                        }
                    }
                    catch
                    {
                        // If an error is encountered, attempt to roll back.
                        try
                        {
                            installer.Rollback(state);
                        }
                        catch { }

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
