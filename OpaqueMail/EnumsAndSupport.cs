/*
 * OpaqueMail (https://opaquemail.org/).
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

namespace OpaqueMail
{
    /// <summary>
    /// Supported authentication methods.
    /// </summary>
    public enum AuthenticationMode
    {
        Login,
        CramMD5,
        DigestMD5,
        Negotiate,
        Plain,
        XOAuth,
        XOAuth2
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ContentType
    {
		/// <summary>Gets or sets the value of the boundary parameter included in the Content-Type header represented by this instance.</summary>
	    public string Boundary;
		/// <summary>Gets or sets the value of the charset parameter included in the Content-Type header represented by this instance.</summary>
        public string CharSet;
		/// <summary>Gets or sets the media type value included in the Content-Type header represented by this instance.</summary>
		public string MediaType;
		/// <summary>Gets or sets the value of the name parameter included in the Content-Type header represented by this instance.</summary>
		public string Name;
    }

	/// <summary>
    /// Describes the delivery notification options for email.
    /// </summary>
	[Flags]
	public enum DeliveryNotificationOptions
	{
		/// <summary>No notification information will be sent. The mail server will utilize its configured behavior to determine whether it should generate a delivery notification.</summary>
		None = 0,
		/// <summary>Notify if the delivery is successful.</summary>
		OnSuccess = 1,
		/// <summary>Notify if the delivery is unsuccessful.</summary>
		OnFailure = 2,
		/// <summary>Notify if the delivery is delayed.</summary>
		Delay = 4,
		/// <summary>A notification should not be generated under any circumstances.</summary>
		Never = 134217728
	}

    /// <summary>
    /// Extended email headers for a MailMessage.
    /// Only populated when the MailMessage is instantiated with a parseExtendedHeaders setting of true.
    /// </summary>
    public class ExtendedProperties
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
    /// Flags representing the processed state of the MailMessage.
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
        /// <summary>
        /// Values to be sent when using the IMAP "ID" command.
        /// </summary>
        /// <param name="name">Name of the IMAP client.</param>
        /// <param name="version">Version of the IMAP client.</param>
        /// <param name="os">Operating system.</param>
        /// <param name="osvendor">Operating system vendor.</param>
        /// <param name="vendor">IMAP client vendor.</param>
        /// <param name="supportURL">IMAP client support URL.</param>
        /// <param name="address">IMAP client address.</param>
        /// <param name="date">Current date.</param>
        /// <param name="command">Command issued.</param>
        /// <param name="arguments">Command arguments.</param>
        /// <param name="environment">Environment information.</param>
        public ImapIdentification(string name, string version, string os, string osvendor, string vendor, string supportURL, string address, string date, string command, string arguments, string environment)
        {
            Name = name;
            Version = version;
            OS = os;
            OSVendor = osvendor;
            Vendor = vendor;
            SupportURL = supportURL;
            Address = address;
            Date = date;
            Command = command;
            Arguments = arguments;
            Environment = environment;
        }

        /// <summary>Name of the IMAP client.</summary>
        public string Name;
        /// <summary>Version of the IMAP client.</summary>
        public string Version;
        /// <summary>Operating system.</summary>
        public string OS;
        /// <summary>Operating system vendor.</summary>
        public string OSVendor;
        /// <summary>IMAP client vendor.</summary>
        public string Vendor;
        /// <summary>IMAP client support URL.</summary>
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

    /// <summary>Specifies the priority of a <see cref="T:System.Net.Mail.MailMessage" />.</summary>
    public enum MailPriority
    {
        /// <summary>The email has normal priority.</summary>
        Normal,
        /// <summary>The email has low priority.</summary>
        Low,
        /// <summary>The email has high priority.</summary>
        High
    }

    /// <summary>Helper class to pass message contents with associated metadata.</summary>
    public class MessagePartialHelper
    {
        /// <summary>Message's raw contents.</summary>
        public string MessageString;
        /// <summary>UID of the message.</summary>
        public int ImapUid;
        /// <summary>Name of the mailbox.</summary>
        public string Mailbox;
        /// <summary>String representation of list of flags.</summary>
        public string FlagsString;
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
    /// Flags determining whether specialized properties are returned with a MailMessage.
    /// </summary>
    [Flags]
    public enum MailMessageProcessingFlags
    {
        None = 0,
        IncludeRawHeaders = 1,
        IncludeRawBody = 2,
        IncludeMIMEParts = 4,
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

	/// <summary>Specifies the outcome of sending email by using the SmtpClient class.</summary>
	public enum SmtpStatusCode
	{
		/// <summary>A system status or system Help reply.</summary>
		SystemStatus = 211,
		/// <summary>A Help message was returned by the service.</summary>
		HelpMessage = 214,
		/// <summary>The SMTP service is ready.</summary>
		ServiceReady = 220,
		/// <summary>The SMTP service is closing the transmission channel.</summary>
		ServiceClosingTransmissionChannel,
		/// <summary>The email was successfully sent to the SMTP service.</summary>
		Ok = 250,
		/// <summary>The user mailbox is not located on the receiving server; the server forwards the email.</summary>
		UserNotLocalWillForward,
		/// <summary>The specified user is not local, but the receiving SMTP service accepted the message and attempted to deliver it. This status code is defined in RFC 1123, which is available at http://www.ietf.org.</summary>
		CannotVerifyUserWillAttemptDelivery,
		/// <summary>The SMTP service is ready to receive the email content.</summary>
		StartMailInput = 354,
		/// <summary>The SMTP service is not available; the server is closing the transmission channel.</summary>
		ServiceNotAvailable = 421,
		/// <summary>The destination mailbox is in use.</summary>
		MailboxBusy = 450,
		/// <summary>The SMTP service cannot complete the request. This error can occur if the client's IP address cannot be resolved (that is, a reverse lookup failed). You can also receive this error if the client domain has been identified as an open relay or source for unsolicited email (spam). For details, see RFC 2505, which is available at http://www.ietf.org.</summary>
		LocalErrorInProcessing,
		/// <summary>The SMTP service does not have sufficient storage to complete the request.</summary>
		InsufficientStorage,
		/// <summary>The client was not authenticated or is not allowed to send mail using the specified SMTP host.</summary>
		ClientNotPermitted = 454,
		/// <summary>The SMTP service does not recognize the specified command.</summary>
		CommandUnrecognized = 500,
		/// <summary>The syntax used to specify a command or parameter is incorrect.</summary>
		SyntaxError,
		/// <summary>The SMTP service does not implement the specified command.</summary>
		CommandNotImplemented,
		/// <summary>The commands were sent in the incorrect sequence.</summary>
		BadCommandSequence,
		/// <summary>The SMTP server is configured to accept only TLS connections, and the SMTP client is attempting to connect by using a non-TLS connection. The solution is for the user to set EnableSsl=true on the SMTP Client.</summary>
		MustIssueStartTlsFirst = 530,
		/// <summary>The SMTP service does not implement the specified command parameter.</summary>
		CommandParameterNotImplemented = 504,
		/// <summary>The destination mailbox was not found or could not be accessed.</summary>
		MailboxUnavailable = 550,
		/// <summary>The user mailbox is not located on the receiving server. You should resend using the supplied address information.</summary>
		UserNotLocalTryAlternatePath,
		/// <summary>The message is too large to be stored in the destination mailbox.</summary>
		ExceededStorageAllocation,
		/// <summary>The syntax used to specify the destination mailbox is incorrect.</summary>
		MailboxNameNotAllowed,
		/// <summary>The transaction failed.</summary>
		TransactionFailed,
		/// <summary>The transaction could not occur. You receive this error when the specified SMTP host cannot be found.</summary>
		GeneralFailure = -1
	}

    /// <summary>Specifies the Content-Transfer-Encoding header information for an email message attachment.</summary>
    public enum TransferEncoding
    {
        /// <summary>Encodes data that consists of printable characters in the US-ASCII character set. See RFC 2406 Section 6.7.</summary>
        QuotedPrintable,
        /// <summary>Encodes stream-based data. See RFC 2406 Section 6.8.</summary>
        Base64,
        /// <summary>Used for data that is not encoded. The data is in 7-bit US-ASCII characters with a total line length of no longer than 1000 characters. See RFC2406 Section 2.7.</summary>
        SevenBit,
        EightBit,
        /// <summary>Indicates that the transfer encoding is unknown.</summary>
        Unknown = -1
    }
}
