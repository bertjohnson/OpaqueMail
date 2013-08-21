/*
 * OpaqueMail Proxy (http://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (http://bertjohnson.net) of Bkip Inc. (http://bkip.com).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using OpaqueMail.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace OpaqueMail.Proxy
{
    /// <summary>
    /// Provides common functions used by other OpaqueMail Proxy classes.
    /// </summary>
    public class ProxyFunctions
    {
        #region Public Methods
        /// <summary>
        /// Ensure that a given directory exists by creating its components if needed.
        /// </summary>
        /// <param name="directory">Directory to ensure.</param>
        public static void EnsureDirectory(string directory)
        {
            // Unless this is a UNC path, make sure the specified directory exists.
            if (!directory.StartsWith("\\"))
            {
                string[] pathParts = directory.Split('\\');
                string path = pathParts[0];

                for (int i = 1; i < pathParts.Length - 1; i++)
                {
                    path += "\\" + pathParts[i];
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// Process special characters in an exported e-mail file name.
        /// </summary>
        /// <param name="directory">The base directory to export messages to.</param>
        /// <param name="messageId">ID of the message.</param>
        /// <param name="instanceId">The instance number of the proxy.</param>
        /// <param name="userName">The user transmitting this message.</param>
        /// <returns>The final log file name, with full path, to be used.</returns>
        public static string GetExportFileName(string directory, string messageId, int instanceId, string userName)
        {
            // Replace the {#} token with the proxy instance number.
            if (instanceId == 1)
                directory = directory.Replace("{#}", "");
            else
                directory = directory.Replace("{#}", instanceId.ToString());

            // Replace server and port {} tokens.
            directory = directory.Replace("{userName}", userName);

            // If the log file location doesn't contain a directory, make it relative to where the service lives.
            if (directory.Length < 2 || (directory[1] != ':' && !directory.StartsWith("\\")))
                directory = AppDomain.CurrentDomain.BaseDirectory + directory;

            if (!directory.EndsWith("\\"))
                directory += "\\";

            // Make sure the specified directory exists.
            EnsureDirectory(directory);

            while (File.Exists(directory + messageId + ".eml"))
                messageId = Guid.NewGuid().ToString();

            return directory + messageId + ".eml";
        }

        /// <summary>
        /// Process special characters in a log file name.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        /// <param name="instanceId">The instance number of the proxy.</param>
        /// <param name="localIPAddress">Local IP address to bind to.</param>
        /// <param name="localPort">Local port to listen on.</param>
        /// <param name="remoteServerHostName">Remote server hostname to forward all POP3 messages to.</param>
        /// <param name="remoteServerPort">Remote server port to connect to.</param>
        /// <returns>The final log file name, with full path, to be used.</returns>
        public static string GetLogFileName(string fileName, int instanceId, string localIPAddress, string remoteServerHostName, int localPort, int remoteServerPort)
        {
            // Replace the {#} token with the proxy instance number.
            if (instanceId == 1)
                fileName = fileName.Replace("{#}", "");
            else
                fileName = fileName.Replace("{#}", instanceId.ToString());

            // Replace server and port {} tokens.
            fileName = fileName.Replace("{localIpAddress}", localIPAddress);
            fileName = fileName.Replace("{remoteServerHostName}", remoteServerHostName);
            fileName = fileName.Replace("{localPort}", localPort.ToString());
            fileName = fileName.Replace("{remotePort}", remoteServerPort.ToString());

            // If the log file location doesn't contain a directory, make it relative to where the service lives.
            if (fileName.Length < 2 || (fileName[1] != ':' && !fileName.StartsWith("\\")))
                fileName = AppDomain.CurrentDomain.BaseDirectory + fileName;

            // Make sure the specified directory exists.
            EnsureDirectory(fileName);

            StringBuilder logFileNameBuilder = new StringBuilder(Constants.TINYSBSIZE);

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
                        catch
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
        /// <returns>A boolean representation of the setting, or false if none was found.</returns>
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
        /// <returns>An integer representation of the setting, or 0 if none was found.</returns>
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
        /// <returns>A string representation of the setting, or an empty string if none was found.</returns>
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
        /// <param name="LogWriter">Log writer object used for documenting events.</param>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="minimalLogLevel">The minimal log level needed for the message to be logged.</param>
        /// <param name="currentLogLevel">The session's current log level.</param>
        public static void Log(StreamWriter LogWriter, string sessionId, string message, LogLevel minimalLogLevel, LogLevel currentLogLevel)
        {
            if ((int)currentLogLevel >= (int)minimalLogLevel)
            {
                if (LogWriter != null)
                {
                    lock (LogWriter)
                    {
                        LogWriter.Write("[" + DateTime.Now + "]\t" + sessionId + "\t\t" + minimalLogLevel.ToString().ToUpper() + "\t" + message + (message.EndsWith("\r\n") ? "" : "\r\n"));
                        LogWriter.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Log an event or exception.
        /// </summary>
        /// <param name="LogWriter">Log writer object used for documenting events.</param>
        /// <param name="sessionId">The current session's unique ID.</param>
        /// <param name="connectionId">The current connection's unique ID.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="minimalLogLevel">The minimal log level needed for the message to be logged.</param>
        /// <param name="currentLogLevel">The session's current log level.</param>
        public static void Log(StreamWriter LogWriter, string sessionId, string connectionId, string message, LogLevel minimalLogLevel, LogLevel currentLogLevel)
        {
            if ((int)currentLogLevel >= (int)minimalLogLevel)
            {
                if (LogWriter != null)
                {
                    lock (LogWriter)
                    {
                        LogWriter.Write("[" + DateTime.Now + "]\t" + sessionId + "\t" + connectionId + "\t" + minimalLogLevel.ToString().ToUpper() + "\t" + message + (message.EndsWith("\r\n") ? "" : "\r\n"));
                        LogWriter.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Validate that an IP address is within an acceptable range.
        /// </summary>
        /// <param name="acceptedIPs">Range of accepted IPs.</param>
        /// <param name="ipAddress">IP address to check.</param>
        /// <returns>True if the IP provided falls within the accepted IP range..</returns>
        public static bool ValidateIP(string acceptedIPs, string ipAddress)
        {
            // Remove leading or trailing whitespace.
            acceptedIPs = acceptedIPs.Trim();

            // If there's no accepted IP range string or if all are accepted, return true.
            if (string.IsNullOrEmpty(acceptedIPs) || acceptedIPs == "*" || acceptedIPs.ToUpper() == "ANY")
                return true;

            int[] ipAddressOctets = ExplodeIPAddress(ipAddress);

            string[] acceptedIPparts = acceptedIPs.Split(',');
            foreach (string acceptedIPpart in acceptedIPparts)
            {
                string canonicalAcceptedIPpart = acceptedIPpart.Trim().ToUpper();

                // If all are accepted, return true;
                if (canonicalAcceptedIPpart == "*" || canonicalAcceptedIPpart == "ANY")
                    return true;

                // Don't process blank strings.
                if (!string.IsNullOrEmpty(canonicalAcceptedIPpart))
                {
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
            }

            return false;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Divide an IP address string into its discrete components, representing wildcards with -1.
        /// </summary>
        /// <param name="ipAddress">The IP address string to process.</param>
        /// <returns>An array of integers representing the IP octets, with a -1 specifying a wildcard character.</returns>
        private static int[] ExplodeIPAddress(string ipAddress)
        {
            string[] ipAddressParts = ipAddress.Split('.');

            int[] ipAddressOctets = new int[ipAddressParts.Length];

            for (int i = 0; i < ipAddressParts.Length; i++)
            {
                if (!int.TryParse(ipAddressParts[i], out ipAddressOctets[i]))
                    ipAddressOctets[i] = -1;
            }

            return ipAddressOctets;
        }
        #endregion Private Methods
    }
}
