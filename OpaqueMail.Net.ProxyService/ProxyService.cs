using OpaqueMail.Net.Proxy;
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

namespace OpaqueMail
{
    public partial class ProxyService : ServiceBase
    {
        #region Private Members
        /// <summary>List of all proxies that have been started.</summary>
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
            smtpProxies = SmtpProxy.StartProxiesFromSettingsFile(GetSettingsFileName());
        }

        /// <summary>
        /// Handle the service stop event by stopping all proxies.
        /// </summary>
        protected override void OnStop()
        {
            if (smtpProxies != null)
            {
                foreach (SmtpProxy smtpProxy in smtpProxies)
                    smtpProxy.StopProxy();

                smtpProxies.Clear();
            }
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

    [RunInstaller(true)]
    public sealed class ProxyServiceProcessInstaller : ServiceProcessInstaller
    {
        public ProxyServiceProcessInstaller()
        {
            Account = ServiceAccount.LocalSystem;
        }
    }

    [RunInstaller(true)]
    public sealed class ProxyServiceInstaller : ServiceInstaller
    {
        public ProxyServiceInstaller()
        {
            Description = "Serves as a local SMTP proxy, optionally adding authentication and S/MIME signing or encryption to all outbound email.";
            DisplayName = "OpaqueMail Proxy";
            ServiceName = "OpaqueMailProxy";
            StartType = ServiceStartMode.Automatic;
        }

        public void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing"); 
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                            inst.Uninstall(state);
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
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
