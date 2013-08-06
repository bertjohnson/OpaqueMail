using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net.Proxy
{
    public class ProxyBase
    {
        #region Public Members
        /// <summary>Welcome message to be displayed when connecting.</summary>
        public string WelcomeMessage = "OpaqueMail Proxy";
        #endregion Public Members

        #region Protected Members
        /// <summary>Whether the proxy has been started.</summary>
        protected bool Started = false;
        /// <summary>A TcpListener to accept incoming connections.</summary>
        protected TcpListener Listener;
        /// <summary>A unique session identifier for logging.</summary>
        protected string SessionId = "";
        /// <summary>A unique connection identifier for logging.</summary>
        protected int ConnectionId = 0;
        /// <summary>StreamWriter object to output event logs and exception information.</summary>
        protected StreamWriter LogWriter = null;
        /// <summary>A collection of all S/MIME signing certificates imported during this session.</summary>
        protected X509Certificate2Collection SmimeCertificatesReceived = new X509Certificate2Collection();
        /// <summary>The last command received from the client.</summary>
        protected string LastCommandReceived = "";
        #endregion Protected Members

        #region Public Methods
        /// <summary>
        /// Handle service continuations following pauses.
        /// </summary>
        public void ProcessContinuation()
        {
            ProxyFunctions.Log(LogWriter, SessionId, "Service continuing after pause.");
            Started = true;
        }

        /// <summary>
        /// Handle pause event.
        /// </summary>
        public void ProcessPause()
        {
            ProxyFunctions.Log(LogWriter, SessionId, "Service pausing.");
            Started = false;
        }

        /// <summary>
        /// Handle power events, such as hibernation.
        /// </summary>
        /// <param name="powerStatus">Indicates the system's power status.</param>
        public void ProcessPowerEvent(int powerStatus)
        {
            if (LogWriter != null)
            {
                switch (powerStatus)
                {
                    case 0:
                        ProxyFunctions.Log(LogWriter, SessionId, "Computer has asked permission to enter the suspended state.");
                        break;
                    case 2:
                        ProxyFunctions.Log(LogWriter, SessionId, "Computer was denied permission to enter the suspended state.");
                        break;
                    case 4:
                        ProxyFunctions.Log(LogWriter, SessionId, "Computer is about to enter the suspended state.");
                        break;
                    case 6:
                        ProxyFunctions.Log(LogWriter, SessionId, "Computer has resumed operation after a critical suspension caused by a failing battery.");
                        break;
                    case 7:
                        ProxyFunctions.Log(LogWriter, SessionId, "The computer has resumed operation after being suspsended.");
                        break;
                    case 8:
                        ProxyFunctions.Log(LogWriter, SessionId, "Computer's battery power is low.");
                        break;
                    case 10:
                    case 11:
                        ProxyFunctions.Log(LogWriter, SessionId, "The computer's power status has changed.");
                        break;
                    case 18:
                        ProxyFunctions.Log(LogWriter, SessionId, "The computer has resumed operation to handle an event.");
                        break;
                }
            }
        }
        #endregion Public Methods
    }
}
