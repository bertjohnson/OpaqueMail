/*
 * OpaqueMail Proxy (https://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (https://bertjohnson.com/) of Allcloud Inc. (https://allcloud.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpaqueMail.Proxy.Settings
{
    /// <summary>
    /// Helper class to create a loopback exception for the Windows 8 Mail app.
    /// </summary>
    /// <remarks>See http://blogs.msdn.com/b/fiddler/archive/2011/09/14/fiddler-and-windows-8-metro-style-applications-https-and-private-network-capabilities.aspx for more information.</remarks>
    public class Windows8MailHelper
    {
        #region Structs
        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct SidAndAttributes
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct InetFirewallAcCapabilities
        {
            public uint Count;
            public IntPtr Capabilities;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct InetFirewallAcBinaries
        {
            public uint Count;
            public IntPtr Binaries;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct InetFirewallAppContainer
        {
            internal IntPtr AppContainerSid;
            internal IntPtr UserSid;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string AppContainerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Description;
            internal InetFirewallAcCapabilities Capabilities;
            internal InetFirewallAcBinaries Binaries;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string WorkingDirectory;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageFullName;
        }
        #endregion Structs

        #region Externs
        [DllImport("FirewallAPI.dll")]
        internal static extern void NetworkIsolationFreeAppContainers(IntPtr pACs);
        
        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationGetAppContainerConfig(out uint pdwCntACs, out IntPtr appContainerSids);

        [DllImport("FirewallAPI.dll")]
        private static extern uint NetworkIsolationSetAppContainerConfig(uint pdwCntACs, SidAndAttributes[] appContainerSids);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool ConvertStringSidToSid(string strSid, out IntPtr pSid);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ConvertSidToStringSid(IntPtr pSid, out string strSid);

        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationEnumAppContainers(uint Flags, out uint pdwCntPublicACs, out IntPtr ppACs);
        #endregion Externs

        #region Private Enums
        private enum NETISO_FLAG
        {
            NETISO_FLAG_FORCE_COMPUTE_BINARIES = 0x1,
            NETISO_FLAG_MAX = 0x2
        }
        #endregion Private Enums

        #region Internal Members
        internal List<AppContainer> Apps = new List<AppContainer>();
        internal List<InetFirewallAppContainer> AppList;
        internal List<SidAndAttributes> AppListConfig;
        internal IntPtr ACs;
        #endregion Internal Members

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Windows8MailHelper()
        {
            AppList = PINetworkIsolationEnumAppContainers();
            AppListConfig = PINetworkIsolationGetAppContainerConfig();
            foreach (InetFirewallAppContainer PIApp in AppList)
            {
                AppContainer app = new AppContainer(PIApp.AppContainerName, PIApp.DisplayName, PIApp.WorkingDirectory, PIApp.AppContainerSid);

                List<SidAndAttributes> AppCapabilities = GetCapabilites(PIApp.Capabilities);

                app.LoopbackEnabled = CheckLoopback(PIApp.AppContainerSid);
                Apps.Add(app);
            }
        }
        #endregion Constructors

        #region Helper Classes
        /// <summary>
        /// Object to track app container information.
        /// </summary>
        internal class AppContainer
        {
            public String AppContainerName { get; set; }
            public String DisplayName { get; set; }
            public String WorkingDirectory { get; set; }
            public String StringSid { get; set; }
            public List<uint> Capabilities { get; set; }
            public bool LoopbackEnabled { get; set; }

            public AppContainer(String _appContainerName, String _displayName, String _workingDirectory, IntPtr _sid)
            {
                this.AppContainerName = _appContainerName;
                this.DisplayName = _displayName;
                this.WorkingDirectory = _workingDirectory;
                String tempSid;
                ConvertSidToStringSid(_sid, out tempSid);
                this.StringSid = tempSid;
            }
        }
        #endregion Helper Classes

        #region Public Methods
        /// <summary>
        /// Enable Windows 8 Mail loopback exemptions.
        /// </summary>
        public void EnableWindows8MailLoopback()
        {
            // Loop through and enable loopbacks for any Windows 8 Mail applications.
            for (int i = 0; i < Apps.Count; i++)
            {
                // Strings relevant to the Windows 8 Mail app.
                if (Apps[i].AppContainerName.Contains("microsoft.windowscommunicationsapps_") || Apps[i].AppContainerName.Contains("microsoft.winjs."))
                    Apps[i].LoopbackEnabled = true;
            }

            // Count the number of exemptions.
            int countEnabled = Apps.Where(_ => _.LoopbackEnabled == true).Count();
            SidAndAttributes[] sidAndAttributesArray = new SidAndAttributes[countEnabled];
            int count = 0;

            // Save our exemptions.
            for (int i = 0; i < Apps.Count; i++)
            {
                if (Apps[i].LoopbackEnabled)
                {
                    sidAndAttributesArray[count].Attributes = 0;
                    IntPtr ptr;
                    ConvertStringSidToSid(Apps[i].StringSid, out ptr);
                    sidAndAttributesArray[count].Sid = ptr;
                    count++;
                }
            }
            NetworkIsolationSetAppContainerConfig((uint)countEnabled, sidAndAttributesArray);

            // Free our app container handles.
            NetworkIsolationFreeAppContainers(ACs);
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Check if an application is set up for the loopback exemption.
        /// </summary>
        /// <param name="intPtr">Pointer to the application container.</param>
        private bool CheckLoopback(IntPtr intPtr)
        {
            foreach (SidAndAttributes item in AppListConfig)
            {
                string before, after;
                ConvertSidToStringSid(item.Sid, out before);
                ConvertSidToStringSid(intPtr, out after);

                return before == after;
            }

            return false;
        }

        /// <summary>
        /// Return a list of capabilities of a Windows 8 application.
        /// </summary>
        private static List<SidAndAttributes> GetCapabilites(InetFirewallAcCapabilities cap)
        {
            List<SidAndAttributes> myCapabilities = new List<SidAndAttributes>();

            IntPtr arrayValue = cap.Capabilities;

            int structSize = Marshal.SizeOf(typeof(SidAndAttributes));
            for (var i = 0; i < cap.Count; i++)
            {
                SidAndAttributes currentSidAndAttributes = (SidAndAttributes)Marshal.PtrToStructure(arrayValue, typeof(SidAndAttributes));
                myCapabilities.Add(currentSidAndAttributes);
                arrayValue = new IntPtr((long)(arrayValue) + (long)(structSize));
            }

            return myCapabilities;
        }

        /// <summary>
        /// Return a list of Windows 8 app container configuration settings.
        /// </summary>
        private static List<SidAndAttributes> PINetworkIsolationGetAppContainerConfig()
        {
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;
            List<SidAndAttributes> myCapabilities = new List<SidAndAttributes>();

            GCHandle handlePdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);
            GCHandle handlePpACs = GCHandle.Alloc(arrayValue, GCHandleType.Pinned);

            uint retVal = NetworkIsolationGetAppContainerConfig(out size, out arrayValue);

            int structSize = Marshal.SizeOf(typeof(SidAndAttributes));
            for (var i = 0; i < size; i++)
            {
                SidAndAttributes currentSidAndAttributes = (SidAndAttributes)Marshal.PtrToStructure(arrayValue, typeof(SidAndAttributes));
                myCapabilities.Add(currentSidAndAttributes);
                arrayValue = new IntPtr((long)(arrayValue) + (long)(structSize));
            }

            handlePdwCntPublicACs.Free();
            handlePpACs.Free();

            return myCapabilities;
        }

        /// <summary>
        /// Return a list of Windows 8 app containers.
        /// </summary>
        private List<InetFirewallAppContainer> PINetworkIsolationEnumAppContainers()
        {
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;
            List<InetFirewallAppContainer> list = new List<InetFirewallAppContainer>();

            GCHandle handlePdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);
            GCHandle handlePpACs = GCHandle.Alloc(arrayValue, GCHandleType.Pinned);

            uint retVal = NetworkIsolationEnumAppContainers((Int32)NETISO_FLAG.NETISO_FLAG_MAX, out size, out arrayValue);
            ACs = arrayValue;

            int structSize = Marshal.SizeOf(typeof(InetFirewallAppContainer));
            for (var i = 0; i < size; i++)
            {
                InetFirewallAppContainer cur = (InetFirewallAppContainer)Marshal.PtrToStructure(arrayValue, typeof(InetFirewallAppContainer));
                list.Add(cur);
                arrayValue = new IntPtr((long)(arrayValue) + (long)(structSize));
            }

            handlePdwCntPublicACs.Free();
            handlePpACs.Free();

            return list;
        }
        #endregion Private Methods
    }
}