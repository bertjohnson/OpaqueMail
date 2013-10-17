/*
 * OpaqueMail (http://opaquemail.org/).
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    public partial class ImapClient : IDisposable
    {
        #region Public Properties
        /// <summary>A list of authentication mechanisms supported by the server.</summary>
        private List<string> ServerAuthSupport;
        /// <summary>Whether the IMAP server supports the "ACL" extension.</summary>
        private bool ServerSupportsACL = false;
        /// <summary>Whether the IMAP server supports the "Binary" extension.</summary>
        private bool ServerSupportsBinary = false;
        /// <summary>Whether the IMAP server supports the "Catenate" extension.</summary>
        private bool ServerSupportsCatenate = false;
        /// <summary>Whether the IMAP server supports the "Children" mailbox extension.</summary>
        private bool ServerSupportsChildren = false;
        /// <summary>A list of compression algorithms supported by the server.</summary>
        private List<string> ServerCompressionSupport;
        /// <summary>Whether the IMAP server supports the Conditional STORE command.</summary>
        private bool ServerSupportsCondStore = false;
        /// <summary>A list of contexts supported by the server.</summary>
        private List<string> ServerContextSupport;
        /// <summary>Whether the IMAP server supports the "Convert" command.</summary>
        private bool ServerSupportsConvert = false;
        /// <summary>Whether the IMAP server supports the "Create-Special-Use" command.</summary>
        private bool ServerSupportsCreateSpecialUse = false;
        /// <summary>Whether the IMAP server supports the "Enable" command.</summary>
        private bool ServerSupportsEnable = false;
        /// <summary>Whether the IMAP server supports the "ESearch" extension.</summary>
        private bool ServerSupportsESearch = false;
        /// <summary>Whether the IMAP server supports the "ESort" extension.</summary>
        private bool ServerSupportsESort = false;
        /// <summary>Whether the IMAP server supports the "Filters" extension.</summary>
        private bool ServerSupportsFilters = false;
        /// <summary>Whether the IMAP server supports Google's X-GM-EXT-1 extensions.</summary>
        private bool ServerSupportsGoogleExtensions = false;
        /// <summary>Whether the IMAP server supports the "ID" command.</summary>
        private bool ServerSupportsID = false;
        /// <summary>Whether the IMAP server supports real-time notifications.</summary>
        private bool ServerSupportsIdle = false;
        /// <summary>Link to the "IMAP-Sieve" server.</summary>
        private string ServerImapSieveServer;
        /// <summary>Whether the IMAP server supports the "Language" extension.</summary>
        private bool ServerSupportsLanguage = false;
        /// <summary>Whether the IMAP server supports the "ListExt" extension.</summary>
        private bool ServerSupportsListExt = false;
        /// <summary>Whether the IMAP server supports the "List-Status" extension.</summary>
        private bool ServerSupportsListStatus = false;
        /// <summary>Whether the IMAP server supports non-synchronizing literals.</summary>
        private bool ServerSupportsLiteralPlus = false;
        /// <summary>Whether the IMAP server supports the "LoginDisabled" extension.</summary>
        private bool ServerSupportsLoginDisabled = false;
        /// <summary>Whether the IMAP server supports the "Login-Referrals" extension.</summary>
        private bool ServerSupportsLoginReferrals = false;
        /// <summary>Whether the IMAP server supports the "Mailbox-Referrals" extension.</summary>
        private bool ServerSupportsMailboxReferrals = false;
        /// <summary>Whether the IMAP server supports the "Metadata" extension.</summary>
        private bool ServerSupportsMetadata = false;
        /// <summary>Whether the IMAP server supports the "Move" command.</summary>
        private bool ServerSupportsMove = false;
        /// Whether the IMAP server supports the "MultiAppend" command.
        private bool ServerSupportsMultiAppend = false;
        /// Whether the IMAP server supports the "MultiSearch" extension.
        private bool ServerSupportsMultiSearch = false;
        /// <summary>Whether the IMAP server supports namespaces.</summary>
        private bool ServerSupportsNamespace = false;
        /// <summary>Whether the IMAP server supports the "Notify" command.</summary>
        private bool ServerSupportsNotify = false;
        /// <summary>Whether the IMAP server supports the "Quota" extension.</summary>
        private bool ServerSupportsQuota = false;
        /// <summary>Whether the IMAP server supports the "QResync" extension.</summary>
        private bool ServerSupportsQResync = false;
        /// <summary>A list of search options supported by the server.</summary>
        private List<string> ServerSearchSupport;
        /// <summary>Whether the IMAP server supports the "SearchRes" extension.</summary>
        private bool ServerSupportsSearchRes = false;
        /// <summary>A list of rights reported by the server.</summary>
        private List<string> ServerRights;
        /// <summary>Whether the IMAP server supports the "Sort" command.</summary>
        private bool ServerSupportsSort = false;
        /// <summary>A list of sorting options supported by the server.</summary>
        private List<string> ServerSortingSupport;
        /// <summary>Whether the IMAP server supports the "Special-Use" extension.</summary>
        private bool ServerSupportsSpecialUse = false;
        /// <summary>A list of threading options supported by the server.</summary>
        private List<string> ServerThreadingSupport;
        /// <summary>Whether the IMAP server supports the "StartTLS" command.</summary>
        private bool ServerSupportsStartTls = false;
        /// <summary>Whether the IMAP server supports the "UIDPlus" extension.</summary>
        private bool ServerSupportsUIDPlus = false;
        /// <summary>Whether the IMAP server supports the "Unselect" command.</summary>
        private bool ServerSupportsUnselect = false;
        /// <summary>Whether the IMAP server supports the "Within" search extension.</summary>
        private bool ServerSupportsWithin = false;
        /// <summary>Whether the IMAP server supports the "Xlist" command.</summary>
        private bool ServerSupportsXlist = false;

        /// <summary>
        /// Whether the IMAP server supports the "ACL" extension.
        /// </summary>
        public bool SupportsACL
        {
            get
            {
                return ServerSupportsACL;
            }
        }

        /// <summary>
        /// A list of authentication methods supported by the server.
        /// </summary>
        private string[] AuthSupport
        {
            get
            {
                return ServerAuthSupport.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Binary" extension.
        /// </summary>
        public bool SupportsBinary
        {
            get
            {
                return ServerSupportsBinary;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Catenate" extension.
        /// </summary>
        public bool SupportsCatenate
        {
            get
            {
                return ServerSupportsCatenate;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Children" mailbox extension.
        /// </summary>
        public bool SupportsChildren
        {
            get
            {
                return ServerSupportsChildren;
            }
        }

        /// <summary>
        /// A list of contexts supported by the server.
        /// </summary>
        private string[] ContextSupport
        {
            get
            {
                return ServerContextSupport.ToArray();
            }
        }

        /// <summary>
        /// A list of compression algorithms supported by the server.
        /// </summary>
        private string[] CompressionSupport
        {
            get
            {
                return ServerCompressionSupport.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the Conditional STORE command.</summary>
        /// </summary>
        public bool SupportsCondStore
        {
            get
            {
                return ServerSupportsCondStore;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Convert" command.
        /// </summary>
        public bool SupportsConvert
        {
            get
            {
                return ServerSupportsConvert;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Create-Special-Use" command.</summary>
        /// </summary>
        public bool SupportsCreateSpecialUse
        {
            get
            {
                return ServerSupportsCreateSpecialUse;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Enable" command.
        /// </summary>
        public bool SupportsEnable
        {
            get
            {
                return ServerSupportsEnable;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "ESearch" extension.
        /// </summary>
        public bool SupportsESearch
        {
            get
            {
                return ServerSupportsESearch;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "ESort" extension.
        /// </summary>
        public bool SupportsESort
        {
            get
            {
                return ServerSupportsESort;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Filters" extension.
        /// </summary>
        public bool SupportsFilters
        {
            get
            {
                return ServerSupportsFilters;
            }
        }

        /// <summary>
        /// <summary>Whether the IMAP server supports Google's X-GM-EXT-1 extensions.</summary>
        /// </summary>
        public bool SupportsGoogleExtensions
        {
            get
            {
                return ServerSupportsGoogleExtensions;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "ID" command.
        /// </summary>
        public bool SupportsID
        {
            get
            {
                return ServerSupportsID;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports real-time notifications.
        /// </summary>
        public bool SupportsIdle
        {
            get
            {
                return ServerSupportsIdle;
            }
        }

        /// <summary>
        /// Link to the "IMAP-Sieve" server.
        /// </summary>
        public string ImapSieveServer
        {
            get
            {
                return ServerImapSieveServer;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Language" extension.
        /// </summary>
        public bool SupportsLanguage
        {
            get
            {
                return ServerSupportsLanguage;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "ListExt" extension.
        /// </summary>
        public bool SupportsListExt
        {
            get
            {
                return ServerSupportsListExt;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "List-Status" extension.
        /// </summary>
        public bool SupportsListStatus
        {
            get
            {
                return ServerSupportsListStatus;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports non-synchronizing literals.
        /// </summary>
        public bool SupportsLiteralPlus
        {
            get
            {
                return ServerSupportsLiteralPlus;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "LoginDisabled" extension.
        /// </summary>
        public bool SupportsLoginDisabled
        {
            get
            {
                return ServerSupportsLoginDisabled;
            }
        }
        
        /// <summary>
        /// Whether the IMAP server supports the "Login-Referrals" extension.
        /// </summary>
        public bool SupportsLoginReferrals
        {
            get
            {
                return ServerSupportsLoginReferrals;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Mailbox-Referrals" extension.
        /// </summary>
        public bool SupportsMailboxReferrals
        {
            get
            {
                return ServerSupportsMailboxReferrals;
            }
        }

        /// <summary>
        /// A list of rights reported by the server.
        /// </summary>
        private string[] Rights
        {
            get
            {
                return ServerRights.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Metadata" extension.
        /// </summary>
        public bool SupportsMetadata
        {
            get
            {
                return ServerSupportsMetadata;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Move" command.
        /// </summary>
        public bool SupportsMove
        {
            get
            {
                return ServerSupportsMove;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "MultiAppend" command.
        /// </summary>
        public bool SupportsMultiAppend
        {
            get
            {
                return ServerSupportsMultiAppend;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "MultiSearch" extension.
        /// </summary>
        public bool SupportsMultiSearch
        {
            get
            {
                return ServerSupportsMultiSearch;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports namespaces.
        /// </summary>
        public bool SupportsNamespace
        {
            get
            {
                return ServerSupportsNamespace;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Notify" command.
        /// </summary>
        public bool SupportsNotify
        {
            get
            {
                return ServerSupportsNotify;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Quota" extension.
        /// </summary>
        public bool SupportsQuota
        {
            get
            {
                return ServerSupportsQuota;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "QResync" extension.
        /// </summary>
        public bool SupportsQResync
        {
            get
            {
                return ServerSupportsQResync;
            }
        }

        /// <summary>
        /// A list of search options supported by the server.
        /// </summary>
        private string[] SearchSupport
        {
            get
            {
                return ServerSearchSupport.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "SearchRes" extension.
        /// </summary>
        public bool SupportsSearchRes
        {
            get
            {
                return ServerSupportsSearchRes;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Sort" command.
        /// </summary>
        public bool SupportsSort
        {
            get
            {
                return ServerSupportsSort;
            }
        }

        /// <summary>
        /// A list of sorting options supported by the server.
        /// </summary>
        private string[] SortingSupport
        {
            get
            {
                return ServerSortingSupport.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Special-Use" extension.
        /// </summary>
        public bool SupportsSpecialUse
        {
            get
            {
                return ServerSupportsSpecialUse;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "StartTls" command.
        /// </summary>
        public bool SupportsStartTls
        {
            get
            {
                return ServerSupportsStartTls;
            }
        }

        /// <summary>
        /// A list of threading options supported by the server.
        /// </summary>
        private string[] ThreadingSupport
        {
            get
            {
                return ServerThreadingSupport.ToArray();
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "UIDPLUS" extension.
        /// </summary>
        public bool SupportsUIDPlus
        {
            get
            {
                return ServerSupportsUIDPlus;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Unselect" command.
        /// </summary>
        public bool SupportsUnselect
        {
            get
            {
                return ServerSupportsUnselect;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "Within" search extension.
        /// </summary>
        public bool SupportsWithin
        {
            get
            {
                return ServerSupportsWithin;
            }
        }

        /// <summary>
        /// Whether the IMAP server supports the "XLIST" command.
        /// </summary>
        public bool SupportsXlist
        {
            get
            {
                return ServerSupportsXlist;
            }
        }
        #endregion Public Properties
    }
}
