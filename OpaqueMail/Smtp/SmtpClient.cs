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
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Allows applications to send email by using the Simple Mail Transport Protocol (SMTP).
    /// Includes OpaqueMail extensions to facilitate sending of secure S/MIME messages.
    /// </summary>
    public partial class SmtpClient : System.Net.Mail.SmtpClient
    {
        #region Public Members
        /// <summary>Allowed protocols when EnableSSL is true.</summary>
        public SslProtocols SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
        #endregion Public Members

        #region Private Members
        /// <summary>Buffer used during various S/MIME operations.</summary>
        private byte[] buffer = new byte[Constants.HUGEBUFFERSIZE];
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class by using configuration file settings.
        /// </summary>
        public SmtpClient()
            : base()
        {
            SmimeAlgorithmIdentifier = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"));
            SmimeValidCertificates = null;
            
            RandomizeBoundaryNames();

            EnableSsl = true;
        }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends email by using the specified SMTP server.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        public SmtpClient(string host)
            : base(host)
        {
            SmimeAlgorithmIdentifier = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"));
            SmimeValidCertificates = null;

            RandomizeBoundaryNames();
        }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpClient class that sends email by using the specified SMTP server and port.
        /// </summary>
        /// <param name="host">Name or IP of the host used for SMTP transactions.</param>
        /// <param name="port">Port to be used by the host.</param>
        public SmtpClient(string host, int port)
            : base(host, port)
        {
            SmimeAlgorithmIdentifier = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"));
            SmimeValidCertificates = null;

            RandomizeBoundaryNames();
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public void Send(MailMessage message)
        {
            Task.Run(() => SendAsync(message)).Wait();
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">An OpaqueMail.MailMessage that contains the message to send.</param>
        public async Task SendAsync(MailMessage message)
        {
            // If the message isn't encoded, do so now.
            if (string.IsNullOrEmpty(message.RawBody))
                message.Prepare();

            // Perform requested S/MIME signing and/or encryption.
            message.SmimePrepare(this);
            string rawBody = message.RawBody;

            // Connect to the SMTP server.
            TcpClient SmtpTcpClient = new TcpClient();
            SmtpTcpClient.Connect(Host, Port);
            Stream SmtpStream = SmtpTcpClient.GetStream();

            // Use stream readers and writers to simplify I/O.
            StreamReader reader = new StreamReader(SmtpStream);
            StreamWriter writer = new StreamWriter(SmtpStream);
            writer.AutoFlush = true;

            // Read the welcome message.
            string response = await reader.ReadLineAsync();

            // Send EHLO and find out server capabilities.
            await writer.WriteLineAsync("EHLO " + Host);
            char[] charBuffer = new char[Constants.SMALLBUFFERSIZE];
            int bytesRead = await reader.ReadAsync(charBuffer, 0, Constants.SMALLBUFFERSIZE);
            response = new string(charBuffer, 0, bytesRead);
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to connect to remote server '" + Host + "'.  Sent 'EHLO' and received '" + response + "'.");

            // Stand up a TLS/SSL stream.
            if (EnableSsl)
            {
                await writer.WriteLineAsync("STARTTLS");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to start TLS/SSL protection with '" + Host + "'.  Received '" + response + "'.");

                SmtpStream = new SslStream(SmtpStream);
                ((SslStream)SmtpStream).AuthenticateAsClient(Host, null, SslProtocols, true);

                reader = new StreamReader(SmtpStream);
                writer = new StreamWriter(SmtpStream);
                writer.AutoFlush = true;

                await writer.WriteLineAsync("EHLO " + Host);
                bytesRead = await reader.ReadAsync(charBuffer, 0, Constants.SMALLBUFFERSIZE);
                response = new string(charBuffer, 0, bytesRead);
            }

            // Authenticate using the AUTH LOGIN command.
            if (Credentials != null)
            {
                NetworkCredential cred = (NetworkCredential)Credentials;
                await writer.WriteLineAsync("AUTH LOGIN");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("3"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
                await writer.WriteLineAsync(Functions.ToBase64String(cred.UserName));
                string tempResponse = await reader.ReadLineAsync();
                await writer.WriteLineAsync(Functions.ToBase64String(cred.Password));
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
            }

            // Build our raw headers block.
            StringBuilder rawHeaders = new StringBuilder(Constants.SMALLSBSIZE);

            // Specify who the message is from.
            rawHeaders.Append(Functions.SpanHeaderLines("From: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.From))) + "\r\n");
            await writer.WriteLineAsync("MAIL FROM:<" + message.From.Address + ">");
            response = await reader.ReadLineAsync();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");

            // Identify all recipients of the message.
            if (message.To.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("To: " + Functions.EncodeMailHeader(message.To.ToString())) + "\r\n");
            foreach (MailAddress address in message.To)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            if (message.CC.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("CC: " + Functions.EncodeMailHeader(message.CC.ToString())) + "\r\n");
            foreach (MailAddress address in message.CC)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            foreach (MailAddress address in message.Bcc)
            {
                await writer.WriteLineAsync("RCPT TO:<" + address.Address + ">");
                response = await reader.ReadLineAsync();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }

            // Ensure a content type is set.
            if (string.IsNullOrEmpty(message.ContentType))
            {
                if (Functions.AppearsHTML(message.Body))
                    message.ContentType = "text/html";
                else
                    message.ContentType = "text/plain";
            }
            message.Headers["Content-Type"] = message.ContentType + (!string.IsNullOrEmpty(message.CharSet) ? "; charset=\"" + message.CharSet + "\"" : "");

            // If the body hasn't been processed, handle encoding of extended characters.
            if (string.IsNullOrEmpty(rawBody) && !string.IsNullOrEmpty(message.Body))
            {
                bool extendedCharacterFound = false;
                foreach (char headerCharacter in message.Body.ToCharArray())
                {
                    if (headerCharacter > 127)
                    {
                        extendedCharacterFound = true;
                        break;
                    }
                }

                if (extendedCharacterFound)
                {
                    message.ContentTransferEncoding = "base64";
                    message.Body = Functions.ToBase64String(message.Body);
                }

                rawBody = message.Body;
            }

            if (!string.IsNullOrEmpty(message.ContentTransferEncoding))
                message.Headers["Content-Transfer-Encoding"] = message.ContentTransferEncoding;

            // Send the raw message.
            await writer.WriteLineAsync("DATA");
            response = await reader.ReadLineAsync();
            if (!response.StartsWith("3"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'DATA' and received '" + response + "'.");

            rawHeaders.Append(Functions.SpanHeaderLines("Subject: " + Functions.EncodeMailHeader(message.Subject, 32)) + "\r\n");
            foreach (string rawHeader in message.Headers)
            {
                switch (rawHeader.ToUpper())
                {
                    case "BCC":
                    case "CC":
                    case "FROM":
                    case "SUBJECT":
                    case "TO":
                        break;
                    default:
                        rawHeaders.Append(Functions.SpanHeaderLines(rawHeader + ": " + message.Headers[rawHeader]) + "\r\n");
                        break;
                }
            }

            await writer.WriteAsync(rawHeaders.ToString() + "\r\n" + rawBody + "\r\n.\r\n");

            response = await reader.ReadLineAsync();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent message and received '" + response + "'.");

            // Clean up this connection.
            await writer.WriteLineAsync("QUIT");

            writer.Dispose();
            reader.Dispose();

            SmtpStream.Dispose();
            SmtpTcpClient.Close();
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Ensure boundary names are unique.
        /// </summary>
        private void RandomizeBoundaryNames()
        {
            string boundaryRandomness = "";
            Random randomGenerator = new Random();

            // Append 10 random characters.
            for (int i = 0; i < 10; i++)
            {
                int nextCharacter = randomGenerator.Next(1, 36);
                if (nextCharacter > 26)
                    boundaryRandomness += (char)(47 + nextCharacter);
                else
                    boundaryRandomness += (char)(64 + nextCharacter);
            }

            SmimeAlternativeViewBoundaryName += "-" + boundaryRandomness;
            SmimeBoundaryName += "-" + boundaryRandomness;
            SmimeSignedCmsBoundaryName += "-" + boundaryRandomness;
            SmimeTripleSignedCmsBoundaryName += "-" + boundaryRandomness;
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Represents the exception that is thrown when the OpaqueMail.ImapClient is not able to complete an operation.
    /// </summary>
    public class SmtpException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpException class.
        /// </summary>
        public SmtpException() : base() { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpException class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        public SmtpException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the OpaqueMail.SmtpException class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">A System.String that describes the error that occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SmtpException(string message, Exception innerException) : base(message, innerException) { }
    }
}
