OpaqueMail .NET E-mail Library
==============================

.NET e-mail library with full support for IMAP, POP3, and SMTP.  Provides S/MIME message signing, encryption, and decryption to foster better e-mail security and privacy.

Follows IETF standards, implementing all IMAP4rev1, POP3, SMTP, and S/MIME 3.2 commands plus common extensions such as IDLE.  Supports MIME, Unicode, and TNEF encoding.

Includes a fully-featured test client that allows browsing and searching of IMAP and POP3 messages as well as sending of SMTP messages with encryption.  Automatically embeds images into Text/HTML messages and strips Script tags.

Inherits from System.Net.Mail.MailMessage and System.Net.Mail.SmtpClient for simplified upgrades of existing code.  Implements .NET 4.5 async and await.

Thoroughly documented.  Designed for security, portability, and performance.

Licensed according to the MIT License (http://mit-license.org/).

Created by Bert Johnson (http://bertjohnson.net) of Bkip Inc. (http://bkip.com).

OpaqueMail S/MIME E-mail Proxy
==============================

SMTP proxy to add or remove S/MIME message signing, encryption, and authentication for outbound messages.

Can also serve as a passthrough IMAP and POP3 proxy to import S/MIME certificates from incoming messages.

Simplifies e-mail protection for Outlook, Thunderbird, and other Windows mail clients.

Can be used to secure and authenticate mail programs that connect to SMTP servers anonymously (e.g. SharePoint).

Runs as a Windows service.  Inbound and outbound IPs, ports, and TLS / SSL settings are all configurable via XML.

Licensed according to the MIT License (http://mit-license.org/).

Created by Bert Johnson (http://bertjohnson.net) of Bkip Inc. (http://bkip.com).

License
=======

Copyright © 2013 Bert Johnson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.