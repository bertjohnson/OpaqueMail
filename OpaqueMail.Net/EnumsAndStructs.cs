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
    /// <summary>
    /// Supported authentication methods.
    /// </summary>
    public enum AuthenticationMode
    {
        Login,
        CramMD5,
        Plain
    }

    /// <summary>
    /// Extended e-mail headers for a ReadOnlyMailMessage.
    /// Only populated when the ReadOnlyMailMessage is instantiated with a parseExtendedHeaders setting of true.
    /// </summary>
    public struct ExtendedProperties
    {
        /// <summary>Accept-Language header.</summary>
        public string AcceptLanguage;
        /// <summary>Authentication Results header.</summary>
        public string AuthenticationResults;
        /// <summary>X-Auto-Responses-Suppress header.</summary>
        public string AutoResponseSuppress;
        /// <summary>Bounces-To header.</summary>
        public string BouncesTo;
        /// <summary>X-Campaign-ID header, used by marketers.</summary>
        public string CampaignID;
        /// <summary>Content-Description header.</summary>
        public string ContentDescription;
        /// <summary>X-Delivery-Context header.</summary>
        public string DeliveryContext;
        /// <summary>Disposition-Notification-To header.</summary>
        public string DispositionNotificationTo;
        /// <summary>DKIM signature message header.</summary>
        public string DomainKeySignature;
        /// <summary>DKIM status header.</summary>
        public string DomainKeyStatus;
        /// <summary>Errors-To header.</summary>
        public string ErrorsTo;
        /// <summary>List-Unsubscribe header.</summary>
        public string ListUnsubscribe;
        /// <summary>X-Mailer header.</summary>
        public string Mailer;
        /// <summary>X-MailList-ID header.</summary>
        public string MailListId;
        /// <summary>X-MSMail-Priority header.</summary>
        public string MSMailPriority;
        /// <summary>X-Original-Arrival-Time header.</summary>
        public DateTime OriginalArrivalTime;
        /// <summary>X-Original-Message-ID header</summary>
        public string OriginalMessageId;
        /// <summary>X-Originating-Email header.</summary>
        public string OriginatingEmail;
        /// <summary>X-Originating-IP header.</summary>
        public string OriginatingIP;
        /// <summary>X-Originator-Org header.</summary>
        public string OriginatorOrg;
        /// <summary>Precedence header.</summary>
        public string Precedence;
        /// <summary>Rcpt-To header.</summary>
        public string RcptTo;
        /// <summary>Received SPF header.</summary>
        public string ReceivedSPF;
        /// <summary>X-Report-Abuse header.</summary>
        public string ReportAbuse;
        /// <summary>References header.</summary>
        public string References;
        /// <summary>Resent-Date header.</summary>
        public DateTime ResentDate;
        /// <summary>Resent-From header.</summary>
        public string ResentFrom;
        /// <summary>Resent-Message-ID header.</summary>
        public string ResentMessageID;
        /// <summary>Spam Score header.</summary>
        public string SpamScore;
        /// <summary>Thread Index header.</summary>
        public string ThreadIndex;
        /// <summary>Thread Topic header.</summary>
        public string ThreadTopic;
        /// <summary>UserAgent header.</summary>
        public string UserAgent;
    }

    /// <summary>
    /// Flags representing the processed state of the ReadOnlyMailMessage.
    /// </summary>
    [Flags]
    public enum Flags
    {
        None = 0,
        Seen = 1,
        Answered = 2,
        Flagged = 4,
        Deleted = 8,
        Draft = 16,
        Recent = 32
    }

    /// <summary>
    /// Values to be sent when using the IMAP "ID" command.
    /// </summary>
    public struct ImapIdentification
    {
        /// <summary>Name of the IMAP client.</summary>
        public string Name;
        /// <summary>Version of the IMAP client</summary>
        public string Version;
        /// <summary>Operating system.</summary>
        public string OS;
        /// <summary>Operating system vendor.</summary>
        public string OSVendor;
        /// <summary>IMAP client vendor.</summary>
        public string Vendor;
        /// <summary>IMAP client support URL</summary>
        public string SupportURL;
        /// <summary>IMAP client address.</summary>
        public string Address;
        /// <summary>Current date.</summary>
        public string Date;
        /// <summary>Command issued.</summary>
        public string Command;
        /// <summary>Command arguments.</summary>
        public string Arguments;
        /// <summary>Environment information.</summary>
        public string Environment;
    }

    /// <summary>
    /// Quota and usage info to be used by the "GETQUOTA", "GETQUOTAROOT", and "SETQUOTA" commands.
    /// </summary>
    public struct QuotaUsage
    {
        /// <summary>Quota used.</summary>
        public int Usage;
        /// <summary>Maximum quota size.</summary>
        public int QuotaMaximum;
    }

    /// <summary>
    /// Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.
    /// </summary>
    [Flags]
    public enum ReadOnlyMailMessageProcessingFlags
    {
        None = 0,
        IncludeRawHeaders = 1,
        IncludeRawBody = 2,
        IncludeNestedRFC822Messages = 128,
        IncludeWinMailData = 256,
        IncludeSmimeSignedData = 512,
        IncludeSmimeEncryptedEnvelopeData = 1024
    }

    /// <summary>
    /// Bit flags to determine how an S/MIME envelope is encrypted.
    /// </summary>
    [Flags]
    public enum SmimeEncryptionOptionFlags
    {
        None = 0,
        RequireCertificateVerification = 1,
        RequireKeyUsageOfDataEncipherment = 2,
        RequireEnhancedKeyUsageofSecureEmail = 4,

        // OpaqueMail optional setting for protecting the subject.
        // Note: This is not part of the current RFC specifcation and should only be used when sending to other OpaqueMail agents.
        EncryptSubject = 256
    }

    /// <summary>
    /// Whether S/MIME settings for encryption and signing are explicitly required or only preferred.
    /// </summary>
    public enum SmimeSettingsMode
    {
        BestEffort = 0,
        RequireExactSettings = 1
    }

    /// <summary>
    /// Bit flags to determine how an S/MIME message is signed.
    /// </summary>
    public enum SmimeSigningOptionFlags
    {
        None = 0,
        SignTime = 1
    }
}
