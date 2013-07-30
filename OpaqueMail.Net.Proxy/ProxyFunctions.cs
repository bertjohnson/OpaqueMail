using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace OpaqueMail.Net.Proxy
{
    public class ProxyFunctions
    {
        #region Public Methods
        /// <summary>
        /// Return a boolean value from an XML document.
        /// </summary>
        /// <param name="navigator">An XPathNavigator within the current XmlDocument.</param>
        /// <param name="xpathExpression">The XPath expression to evaluate.</param>
        public static bool GetXmlBoolValue(XPathNavigator navigator, string xpathExpression)
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
        public static int GetXmlIntValue(XPathNavigator navigator, string xpathExpression)
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
        public static string GetXmlStringValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
                return valueNavigator.Value;
            else
                return "";
        }

        /// <summary>
        /// Log an event or exception.
        /// </summary>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="message">The message to log.</param>
        public static void Log(StreamWriter LogWriter, string sessionId, string message)
        {
            if (LogWriter != null)
                LogWriter.WriteLine("[" + DateTime.Now + "]\t" + sessionId + "\t\t" + message);
        }

        /// <summary>
        /// Log an event or exception.
        /// </summary>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="connectionId">The current connection's unique ID.</param>
        /// <param name="message">The message to log.</param>
        public static void Log(StreamWriter LogWriter, string sessionId, string connectionId, string message)
        {
            if (LogWriter != null)
                LogWriter.WriteLine("[" + DateTime.Now + "]\t" + sessionId + "\t" + connectionId + "\t" + message);
        }

        /// <summary>
        /// Log an event or exception.
        /// </summary>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="message">The message to log.</param>
        public static async Task LogAsync(StreamWriter LogWriter, string sessionId, string message)
        {
            if (LogWriter != null)
                await LogWriter.WriteLineAsync("[" + DateTime.Now + "]\t" + sessionId + "\t\t" + message);
        }

        /// <summary>
        /// Log an event or exception.
        /// </summary>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="connectionId">The current connection's unique ID.</param>
        /// <param name="message">The message to log.</param>
        public static async Task LogAsync(StreamWriter LogWriter, string sessionId, string connectionId, string message)
        {
            if (LogWriter != null)
                await LogWriter.WriteLineAsync("[" + DateTime.Now + "]\t" + sessionId + "\t" + connectionId + "\t" + message);
        }
        #endregion Public Methods
    }
}
