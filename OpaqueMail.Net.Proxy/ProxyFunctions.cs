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
        /// Process special characters in a log file name.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        public static string GetLogFileName(string fileName)
        {
            // If the log file location doesn't contain a directory, make it relative to where the service lives.
            if (fileName.Length < 2 || (fileName[1] != ':' && !fileName.StartsWith("\\")))
                fileName = AppDomain.CurrentDomain.BaseDirectory + fileName;

            // Unless this is a UNC path, make sure the specified directory exists.
            if (!fileName.StartsWith("\\"))
            {
                string[] pathParts = fileName.Split('\\');
                string path = pathParts[0];

                for (int i = 1; i < pathParts.Length - 1; i++)
                {
                    path += "\\" + pathParts[i];
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
            }

            StringBuilder logFileNameBuilder = new StringBuilder();

            DateTime now = DateTime.Now;

            int pos = 0, lastPos = 0;
            while (pos > -1)
            {
                lastPos = pos;
                pos = fileName.IndexOf("{", pos);
                if (pos > -1)
                {
                    logFileNameBuilder.Append(fileName.Substring(lastPos, pos - lastPos));

                    int endPos = fileName.IndexOf("}", pos);
                    if (endPos > -1)
                    {
                        try
                        {
                            // Attempt to append the .NET DateTime.ToString() representation of the variable.
                            logFileNameBuilder.Append(DateTime.Now.ToString(fileName.Substring(pos + 1, endPos - pos - 1)));
                        }
                        catch (Exception)
                        {
                        }

                        pos = endPos + 1;
                    }
                    else
                        pos = -1;
                }
            }

            if (lastPos > -1)
                logFileNameBuilder.Append(fileName.Substring(lastPos));

            return logFileNameBuilder.ToString();
        }

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
            {
                lock (LogWriter)
                {
                    LogWriter.WriteLine("[" + DateTime.Now + "]\t" + sessionId + "\t\t" + message);
                    LogWriter.Flush();
                }
            }
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
            {
                lock (LogWriter)
                {
                    LogWriter.WriteLine("[" + DateTime.Now + "]\t" + sessionId + "\t" + connectionId + "\t" + message);
                    LogWriter.Flush();
                }
            }
        }

        /// <summary>
        /// Validate that an IP address is within an acceptable range.
        /// </summary>
        /// <param name="acceptedIPs">Range of accepted IPs.</param>
        /// <param name="ipAddress">IP address to check.</param>
        public static bool ValidateIP(string acceptedIPs, string ipAddress)
        {
            // If there's no accepted IP range string or if all are accepted, return true.
            if (string.IsNullOrEmpty(acceptedIPs) || acceptedIPs == "*" || acceptedIPs.ToUpper() == "ANY")
                return true;

            string[] acceptedIPparts = acceptedIPs.Split(',');
            foreach (string acceptedIPpart in acceptedIPparts)
            {
                string canonicalAcceptedIPpart = acceptedIPpart.Trim().ToUpper();

                // If all are accepted, return true;
                if (canonicalAcceptedIPpart == "*" || canonicalAcceptedIPpart == "ANY")
                    return true;

                int[] ipAddressOctets = ExplodeIPAddress(ipAddress);

                bool matchedPart = true;

                // If this is an IP range, check that the address falls between them.
                if (canonicalAcceptedIPpart.IndexOf("-") > -1)
                {
                    string[] ipRangeParts = canonicalAcceptedIPpart.Split(new char[] { '-' }, 2);

                    int[] ipRangeMinOctets = ExplodeIPAddress(ipRangeParts[0]);
                    int[] ipRangeMaxOctets = ExplodeIPAddress(ipRangeParts[1]);

                    // Make sure we're above the range's minimum value.
                    int octetsToCompare = ipAddressOctets.Length <= ipRangeMinOctets.Length ? ipAddressOctets.Length : ipRangeMinOctets.Length;
                    for (int i = 0; i < ipAddressOctets.Length; i++)
                    {
                        // If the octet is a wildcard, we've successfully matched.
                        if (ipRangeMinOctets[i] == -1)
                            break;

                        // If the octet is below the minimum value, we haven't matched.
                        if (ipAddressOctets[i] < ipRangeMinOctets[i])
                        {
                            matchedPart = false;
                            break;
                        }
                    }

                    // Only check the range's maximum value if we're above the range's minimum value.
                    if (matchedPart)
                    {
                        octetsToCompare = ipAddressOctets.Length <= ipRangeMaxOctets.Length ? ipAddressOctets.Length : ipRangeMaxOctets.Length;
                        for (int i = 0; i < ipAddressOctets.Length; i++)
                        {
                            // If the octet is a wildcard, we've successfully matched.
                            if (ipRangeMaxOctets[i] == -1)
                                break;

                            // If the octet is above the maximum value, we haven't matched.
                            if (ipAddressOctets[i] > ipRangeMaxOctets[i])
                            {
                                matchedPart = false;
                                break;
                            }
                        }

                        // If we've made it this far, we've successfully matched the IP.
                        if (matchedPart)
                            return true;
                    }
                }
                else
                {
                    // At this point, we're matching against a specific IP.
                    int[] ipTargetOctets = ExplodeIPAddress(canonicalAcceptedIPpart);

                    // Compare to the target value.
                    int octetsToCompare = ipAddressOctets.Length <= ipTargetOctets.Length ? ipAddressOctets.Length : ipTargetOctets.Length;
                    for (int i = 0; i < ipAddressOctets.Length; i++)
                    {
                        // If the octet is a wildcard, we've successfully matched.
                        if (ipTargetOctets[i] == -1)
                            break;

                        // If the octet isn't equal to the target value, we haven't matched.
                        if (ipAddressOctets[i] != ipTargetOctets[i])
                        {
                            matchedPart = false;
                            break;
                        }
                    }

                    // If we've made it this far, we've successfully matched the IP.
                    if (matchedPart)
                        return true;
                }
            }

            return false;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Divide an IP address string into its discrete components, representing wildcards with -1.
        /// </summary>
        /// <param name="ipAddress">The IP address string to process.</param>
        /// <returns></returns>
        private static int[] ExplodeIPAddress(string ipAddress)
        {
            string[] ipAddressParts = ipAddress.Split('.');

            int[] ipAddressOctets = new int[ipAddressParts.Length];

            for (int i = 0; i < ipAddressParts.Length; i++)
            {
                ipAddressOctets[i] = -1;
                int.TryParse(ipAddressParts[i], out ipAddressOctets[i]);
            }

            return ipAddressOctets;
        }
        #endregion Private Methods
    }
}
