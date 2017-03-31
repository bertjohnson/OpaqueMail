### Changelog ###

2.5.3 - 2017-03-30
* Ensured that the last text/html MIME part is considered the body (in the case of multiple text/html MIME parts).

2.5.2 - 2017-03-22
* Added fix in POP3 `GetMessagesAsync()` method for servers that return blank lines for UIDL command.

2.5.1 - 2017-03-16
 * Fixed alternate view MIME wrapping.
 * In SmtpProxy, eliminated unneeded certificate generation when messages aren't signed.

2.5.0 - 2017-02-27
 * Cleaned up a few edge cases based on static code analysis.

2.4.4 - 2017-02-16
 * Removed UID variants of `Pop3Client.DeleteMessage`, as deletion only works based on index.

2.4.3 - 2017-01-09
 * Fixed bug when headers contained high-byte characters after the 78th index.

2.4.2 - 2017-01-08
 * Updated `ImapClient.GetMessagePartialHelper` to deal with situations where the body length is shorter than what is reported.

2.4.1 - 2016-12-23
 * Added `Functions.GetCertificateByThumbprint` helper method.

2.3.4 - 2016-12-18
 * Added `GetMessagePartial()`.
 * Fixed StartTLS.

2.3.3 - 2016-12-14
 * Minor safeguards for null headers.

2.3.2 - 2016-12-10
 * Fixed issues with MIME when content types aren't defined.

2.3.1 - 2016-11-25
 * Fixed private scoping for pgpEncypted and pgpSigned.

2.3.0 - 2016-11-24
 * Fixed SMTP handling of attachments.
 * Added connection timeouts.
 * Fixed "PEEK" behavior for headers-only reads.

2.2.6 - 2016-08-28
 * 2.26 Added ability to specify supported SSL protocols.
 * Fixed occasional POP3 error.

2.2.5 - 2016-04-16
 * Removed unused variables.
 * Fixed message size calculation when attachments are present.

2.2.4 - 2015-11-26
 * Signed assembly with a strong name.
 * Simplified handling of malformed quoted-printable encodings.

2.2.3 - 2015-06-14
 * Improved SMTP server compatibility by forcing "EHLO" after TLS negotation.

2.2.2 - 2015-06-14
 * Added PGP "sign then encrypt".
 * Improved PGP function overloads.

2.2.1 - 2015-06-13
 * Added PGP signing and encryption.

2.2.0 - 2015-06-08
 * Added initial PGP support through BouncyCastle (decryption and signature verification).

2.1.0 - 2015-05-24
 * Quoted-printable encoding fixes.

2.0.0 - 2014-11-28
 * Combined `MailMessage` and `ReadOnlyMailMessage` into one class.

1.6.5 - 2014-01-19
 * Fixed bug related to signature verification.
 * Fixed bug with character decoding.
 * Improved IMAP IDLE.

1.6.4.2 - 2013-12-30
 * Made ReadData and SendCommand methods public.

1.5.8.2 - 2013-10-17
 * Fixed TNEF parsing bug.

1.5.7 - 2013-08-22
 * Added DeleteMessage() method for ImapClient.
 * Cleaned up code documentation.

1.5.6 - 2013-08-21
 * Added proxy support for Opera Mail.

1.5.5 - 2013-08-20
 * Standardized namespace to OpaqueMail.Net and OpaqueMail.Proxy.

1.5.4 - 2013-08-16
 * Added optional export of proxied messages.
 * Made self-signed certificates more conformant.

1.5.3 - 2013-08-15
 * Added Windows Live Mail support.

1.4.2 - 2013-08-13
 * Added Thunderbird configuration wizard.

1.3.2 - 2013-08-07
 * Added MULTIAPPEND and QRESYNC.
 * Slightly improved performance.

1.3.1 - 2013-08-06
 * Allowed the SMTP proxy to remove and/or replace existing S/MIME operations on outbound messages.

1.3.0 - 2013-08-06
 * Made performance improvements.
 * Added IMAP events.
 * Improved async support.

1.0.0 - 2013-07-04
 * Initial release.