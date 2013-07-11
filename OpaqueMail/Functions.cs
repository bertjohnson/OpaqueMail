using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Provides common functions used by other OpaqueMail classes.
    /// </summary>
    public class Functions
    {
        #region Public Methods
        /// <summary>
        /// Create HTML links for embedded URLs.
        /// Useful for text/plain messages.
        /// </summary>
        /// <param name="html">HTML block to process.</param>
        public static string ConvertPlainTextToHTML(string html)
        {
            // Handle the special case of e-mail starting or endding with a link by padding with spaces on either side, which will be removed at the end.
            html = " " + html + " ";

            // Treat all whitespace equivalently and ignore case.
            string canonicalHtml = html.Replace("\t", " ").Replace("\r\n", "     ").Replace("\r", " ").Replace("\n", " ").ToLower();

            // Convert line breaks to BR tags.
            html = html.Replace("\r\n", "<br/>");
            
            // Build a new string using the following buffer.
            StringBuilder htmlBuilder = new StringBuilder();

            int pos = 0, lastPos = 0;
            while (pos > -1)
            {
                lastPos = pos;

                // Find the next link, whether using the HTTP or HTTPS protocol.
                int httpPos = canonicalHtml.IndexOf(" http://", pos);
                int httpsPos = canonicalHtml.IndexOf(" https://", pos);
            
                if (httpPos > -1)
                {
                    if (httpsPos > -1)
                        pos = httpPos < httpsPos ? httpPos : httpsPos;
                    else
                        pos = httpPos;
                }
                else if (httpsPos > -1)
                    pos = httpsPos;
                else
                    pos = -1;

                // A URL was found, so insert a link.
                if (pos > -1)
                {
                    int endPos = canonicalHtml.IndexOf(" ", pos + 1);
                    if (endPos > -1)
                    {
                        string link = html.Substring(pos + 1, endPos - pos - 1);

                        htmlBuilder.Append(html.Substring(lastPos, pos + 1 - lastPos));
                        htmlBuilder.Append("<a href=\"" + link + "\">" + link + "</a>");
                        pos = endPos;
                    }
                    else
                        pos = -1;
                }
            }

            // Add any text remaining after our last link.
            htmlBuilder.Append(html.Substring(lastPos));

            // Remove spaces padded to the beginning and end.
            string returnValue = htmlBuilder.ToString();
            return returnValue.Substring(1, returnValue.Length - 2);
        }

        /// <summary>
        /// Escapes embedded encoding of e-mail headers.
        /// </summary>
        /// <param name="header">E-mail header to be decoded.</param>
        public static string DecodeMailHeader(string header)
        {
            // Build a new string using the following buffer.
            StringBuilder headerBuilder = new StringBuilder();

            int cursor = 0, lastCursor = 0;
            while (cursor > -1)
            {
                lastCursor = cursor;
                cursor = header.IndexOf("=?", cursor);
                if (cursor > -1)
                {
                    int middleCursor = header.IndexOf("?", cursor + 2);
                    if (middleCursor > -1 && middleCursor < header.Length - 2)
                    {
                        int endCursor = header.IndexOf("?=", middleCursor + 2);
                        if (endCursor > -1)
                        {
                            headerBuilder.Append(header.Substring(lastCursor, cursor - lastCursor));

                            // Try to create a decoder for the encoding.
                            string encodingName = header.Substring(cursor + 2, middleCursor - cursor - 2);
                            Encoding encoding = Encoding.GetEncoding(encodingName);

                            byte[] encodedBytes = null;
                            switch (header.Substring(middleCursor + 1, 2))
                            {
                                case "B?":
                                    encodedBytes = Convert.FromBase64String(header.Substring(middleCursor + 3, endCursor - middleCursor - 3));
                                    break;
                                case "Q?":
                                    encodedBytes = Encoding.UTF8.GetBytes(Functions.FromQuotedPrintable(header.Substring(middleCursor + 3, endCursor - middleCursor - 3)));
                                    break;
                                default:
                                    encodedBytes = Encoding.UTF8.GetBytes(header.Substring(middleCursor, endCursor - middleCursor - 2));
                                    break;
                            }

                            // Append the decoded string.
                            headerBuilder.Append(encoding.GetString(encodedBytes));

                            cursor = endCursor + 2;
                        }
                        else
                            cursor = -1;
                    }
                    else
                        cursor = -1;
                }
            }

            // Append any remaining characters.
            headerBuilder.Append(header.Substring(lastCursor));

            return headerBuilder.ToString();
        }            

        /// <summary>
        /// Convert CID: object references to Base-64 encoded versions.
        /// Useful for displaying text/html messages with image references.
        /// </summary>
        /// <param name="html">HTML block to process.</param>
        /// <param name="attachments">Collection of attachments available to be embedded.</param>
        public static string EmbedAttachments(string html, AttachmentCollection attachments)
        {
            // Build a new string using the following buffer.
            StringBuilder htmlBuilder = new StringBuilder();

            int srcStartPos = 0, lastPos = 0;
            while (srcStartPos > -1)
            {
                // Find the next SRC= attribute and handle either single or double quotes.
                int srcStartQuotePos = html.IndexOf("src=\"cid:", srcStartPos);
                int srcStartApostrophePos = html.IndexOf("src='cid:", srcStartPos);

                if (srcStartQuotePos > -1)
                {
                    if (srcStartApostrophePos > -1)
                        srcStartPos = srcStartQuotePos < srcStartApostrophePos ? srcStartQuotePos : srcStartApostrophePos;
                    else
                        srcStartPos = srcStartQuotePos;
                }
                else if (srcStartApostrophePos > -1)
                    srcStartPos = srcStartApostrophePos;
                else
                    srcStartPos = -1;

                string srcEndDelimiter = (srcStartQuotePos == srcStartPos) ? "\"" : "'";

                if (srcStartPos > -1)
                {
                    int srcEndPos = html.IndexOf(srcEndDelimiter, srcStartPos + 9);
                    if (srcEndPos > 0)
                    {
                        htmlBuilder.Append(html.Substring(lastPos, srcStartPos + 5 - lastPos));

                        string cid = html.Substring(srcStartPos + 9, srcEndPos - srcStartPos - 9);

                        // Check for attachments with matching Content-IDs.
                        bool matchingAttachmentFound = false;
                        foreach (Attachment attachment in attachments)
                        {
                            if (attachment.ContentId == cid)
                            {
                                htmlBuilder.Append("data:" + attachment.ContentType.MediaType + ";base64,");

                                matchingAttachmentFound = true;
                                byte[] contentStreamBytes = ((MemoryStream)attachment.ContentStream).ToArray();

                                htmlBuilder.Append(Convert.ToBase64String(contentStreamBytes, 0, contentStreamBytes.Length, Base64FormattingOptions.InsertLineBreaks));
                            }
                        }

                        // If the current object hasn't been matched, look for a matching file name.
                        if (!matchingAttachmentFound)
                        {
                            // Ignore the non-file name portion of this Content-ID.
                            int cidAtPos = cid.IndexOf("@");
                            if (cidAtPos > -1)
                                cid = cid.Substring(0, cidAtPos);

                            foreach (Attachment attachment in attachments)
                            {
                                if (attachment.Name == cid)
                                {
                                    htmlBuilder.Append("data:" + attachment.ContentType.MediaType + ";base64,");

                                    matchingAttachmentFound = true;
                                    byte[] contentStreamBytes = ((MemoryStream)attachment.ContentStream).ToArray();

                                    htmlBuilder.Append(Convert.ToBase64String(contentStreamBytes, 0, contentStreamBytes.Length, Base64FormattingOptions.InsertLineBreaks));
                                }
                            }

                            if (!matchingAttachmentFound)
                                htmlBuilder.Append(cid);
                        }

                        srcStartPos = srcEndPos;
                        lastPos = srcStartPos;
                    }
                    else
                        srcStartPos = -1;
                }
                else
                    srcStartPos = -1;
            }

            htmlBuilder.Append(html.Substring(lastPos));

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Returns a base-64 string representing the original input.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        public static string FromBase64(string input)
        {
            return Encoding.UTF8.GetString(System.Convert.FromBase64String(input));
        }

        /// <summary>
        /// Parse a text representation of e-mail addresses into a collection of MailAddress objects.
        /// </summary>
        /// <param name="addresses">String representation of e-mail addresses to parse.</param>
        public static MailAddressCollection FromMailAddressString(string addresses)
        {
            // Escape embedded encoding.
            addresses = DecodeMailHeader(addresses);

            // Create a new collection of MailAddresses to be returned.
            MailAddressCollection addressCollection = new MailAddressCollection();

            int cursor = 0, lastCursor = 0;
            string displayName = "";
            while (cursor < addresses.Length)
            {
                int quoteCursor = addresses.IndexOf("\"", cursor);
                if (quoteCursor == -1)
                    quoteCursor = addresses.Length + 1;
                int aposCursor = addresses.IndexOf("'", cursor);
                if (aposCursor == -1)
                    aposCursor = addresses.Length + 1;
                int angleCursor = addresses.IndexOf("<", cursor);
                if (angleCursor == -1)
                    angleCursor = addresses.Length + 1;
                int bracketCursor = addresses.IndexOf("[", cursor);
                if (bracketCursor == -1)
                    bracketCursor = addresses.Length + 1;
                int commaCursor = addresses.IndexOf(",", cursor);
                if (commaCursor == -1)
                    commaCursor = addresses.Length + 1;
                int semicolonCursor = addresses.IndexOf(";", cursor);
                if (semicolonCursor == -1)
                    semicolonCursor = addresses.Length + 1;

                if (quoteCursor < aposCursor && quoteCursor < angleCursor && quoteCursor < bracketCursor && quoteCursor < commaCursor && quoteCursor < semicolonCursor)
                {
                    // The address display name is enclosed in quotes.
                    int endQuoteCursor = addresses.IndexOf("\"", quoteCursor + 1);
                    if (endQuoteCursor > -1)
                    {
                        displayName = addresses.Substring(quoteCursor + 1, endQuoteCursor - quoteCursor - 1);
                        cursor = endQuoteCursor + 1;
                    }
                    else
                        cursor = addresses.Length;
                }
                else if (aposCursor < quoteCursor && aposCursor < angleCursor && aposCursor < bracketCursor && aposCursor < commaCursor && aposCursor < semicolonCursor)
                {
                    // The address display name may be enclosed in apostrophes.
                    int endAposCursor = addresses.IndexOf("'", aposCursor + 1);
                    if (endAposCursor > -1)
                    {
                        displayName = addresses.Substring(aposCursor + 1, endAposCursor - aposCursor - 1);
                        cursor = endAposCursor + 1;
                    }
                }
                else if (angleCursor < quoteCursor && angleCursor < aposCursor && angleCursor < bracketCursor && angleCursor < commaCursor && angleCursor < semicolonCursor)
                {
                    // The address is enclosed in angle brackets.
                    int endAngleCursor = addresses.IndexOf(">", angleCursor + 1);
                    if (endAngleCursor > -1)
                    {
                        // If we didn't find a display name between quotes or apostrophes, look at all characters prior to the angle bracket.
                        if (displayName.Length < 1)
                            displayName = addresses.Substring(lastCursor, angleCursor - lastCursor).Trim();

                        string address = addresses.Substring(angleCursor + 1, endAngleCursor - angleCursor - 1);
                        if (displayName.Length > 0)
                            addressCollection.Add(new MailAddress(address, displayName));
                        else
                            addressCollection.Add(new MailAddress(address));

                        displayName = "";
                        cursor = endAngleCursor + 1;
                    }
                    else
                        cursor = addresses.Length;
                }
                else if (bracketCursor < quoteCursor && angleCursor < aposCursor && bracketCursor < angleCursor && bracketCursor < commaCursor && bracketCursor < semicolonCursor)
                {
                    // The address is enclosed in brackets.
                    int endBracketCursor = addresses.IndexOf(">", bracketCursor + 1);
                    if (endBracketCursor > -1)
                    {
                        // If we didn't find a display name between quotes or apostrophes, look at all characters prior to the bracket.
                        if (displayName.Length < 1)
                            displayName = addresses.Substring(lastCursor, bracketCursor - lastCursor).Trim();

                        string address = addresses.Substring(bracketCursor + 1, endBracketCursor - bracketCursor - 1);
                        if (displayName.Length > 0)
                            addressCollection.Add(new MailAddress(address, displayName));
                        else
                            addressCollection.Add(new MailAddress(address));

                        displayName = "";
                        cursor = endBracketCursor + 1;
                    }
                    else
                        cursor = addresses.Length;
                }
                else if (commaCursor < quoteCursor && commaCursor < aposCursor && commaCursor < angleCursor && commaCursor < bracketCursor && commaCursor < semicolonCursor)
                {
                    // We've found the next address, delimited by a comma.
                    cursor = commaCursor + 1;
                }
                else if (semicolonCursor > -1)
                {
                    // We've found the next address, delimited by a semicolon.
                    cursor = commaCursor + 1;
                }
                else
                    cursor = addresses.Length;

                lastCursor = cursor;
            }

            // If no encoded email address was parsed, try adding the entire string.
            if (addressCollection.Count < 1 && IsValidEmailAddress(addresses))
                addressCollection.Add(addresses);

            return addressCollection;
        }

        /// <summary>
        /// Decode modified UTF-7, as used for IMAP mailbox names.
        /// </summary>
        /// <param name="input">String to decode</param>
        public static string FromModifiedUTF7(string input)
        {
            StringBuilder outputBuilder = new StringBuilder();

            int ampCursor = 0, lastAmpCursor = 0;
            while (ampCursor > -1)
            {
                lastAmpCursor = ampCursor;
                ampCursor = input.IndexOf("&", ampCursor);
                if (ampCursor > -1)
                {
                    outputBuilder.Append(input.Substring(lastAmpCursor, ampCursor - lastAmpCursor));
                    int minusCursor = input.IndexOf("-", ampCursor + 1);
                    if (minusCursor > -1)
                    {
                        // Check if this is an encoded ampersand.
                        if (minusCursor == ampCursor + 1)
                            outputBuilder.Append("&");
                        else
                        {
                            // Unpack the encoded substring.
                            string base64String = "+" + input.Substring(ampCursor + 1, minusCursor - ampCursor - 1).Replace(',', '/');
                            outputBuilder.Append(Encoding.UTF7.GetString(Encoding.UTF8.GetBytes(base64String)));
                        }
                        ampCursor = minusCursor + 1;
                    }
                    else
                        ampCursor = -1;
                }
            }

            // Add the remaining portion after the last escape sequenece.
            outputBuilder.Append(input.Substring(lastAmpCursor));

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Returns a quoted-printable string representing the original input.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        public static string FromQuotedPrintable(string input)
        {
            // Remove carriage returns because they'll be added back in for line breaks (=0A).
            input = input.Replace("=0D", "");

            // Build a new string using the following buffer.
            StringBuilder outputBuilder = new StringBuilder();

            // Buffer for holding UTF-8 encoded characters.
            byte[] utf8Buffer = new byte[8];

            // Loop through and process quoted-printable strings, denoted by equals signs.
            int equalsPos = 0, lastPos = 0;
            while (equalsPos > -1)
            {
                lastPos = equalsPos;
                equalsPos = input.IndexOf('=', equalsPos);
                if (equalsPos > -1 && equalsPos < input.Length - 2)
                {
                    outputBuilder.Append(input.Substring(lastPos, equalsPos - lastPos));

                    string afterEquals = input.Substring(equalsPos + 1, 2);

                    switch (afterEquals)
                    {
                        case "\r\n":
                            break;
                        case "09":
                            outputBuilder.Append("\t");
                            break;
                        case "0A":
                            outputBuilder.Append("\r\n");
                            break;
                        case "20":
                            outputBuilder.Append(" ");
                            break;
                        default:
                            int highByte = int.Parse(afterEquals, System.Globalization.NumberStyles.HexNumber);

                            // Handle values above 7F as UTF-8 encoded character sequences.
                            bool processed = false;
                            if (highByte > 127 && equalsPos < input.Length - 2)
                            {
                                utf8Buffer[0] = (byte)highByte;
                                int utf8ByteCount = 1;

                                string encodedString = afterEquals;
                                equalsPos += 3;

                                while (input.Substring(equalsPos, 1) == "=")
                                {
                                    // Step over a line break if that breaks up our encoded string.
                                    if (input.Substring(equalsPos + 1, 2) != "\r\n")
                                        utf8Buffer[utf8ByteCount++] = (byte)int.Parse(input.Substring(equalsPos + 1, 2), NumberStyles.HexNumber);

                                    equalsPos += 3;
                                }

                                outputBuilder.Append(Utf8toUnicode(utf8Buffer, utf8ByteCount));

                                processed = true;
                                equalsPos -= 3;
                            }

                            // Continue if we didn't run into a UTF-8 encoded character sequence.
                            if (!processed)
                                outputBuilder.Append((char)highByte);
                            break;
                    }

                    equalsPos += 3;
                }
                else
                {
                    outputBuilder.Append(input.Substring(lastPos));
                    equalsPos = -1;
                }
            }
            return outputBuilder.ToString();
        }

        /// <summary>
        /// Provide a best guess of a file's content type based on its filename.
        /// </summary>
        /// <param name="fileExtension">File extension to interpret.</param>
        public static string GetDefaultContentTypeForExtension(string fileExtension)
        {
            // If a full filename pas been supplied, extract the extension.
            int lastPeriod = fileExtension.LastIndexOf(".");
            if (lastPeriod > -1)
                fileExtension = fileExtension.Substring(lastPeriod + 1);

            // Default to "application/octet-string" for unknown content types.
            string contentType = "application/octet-string";

            switch (fileExtension.ToLower())
            {
                case "gif":
                    contentType = "image/gif";
                    break;
                case "htm":
                case "html":
                    contentType = "text/html";
                    break;
                case "jpg":
                case "jpeg":
                    contentType = "image/jpeg";
                    break;
                case "pdf":
                    contentType = "application/pdf";
                    break;
                case "png":
                    contentType = "image/png";
                    break;
                case "tif":
                case "tiff":
                    contentType = "image/tiff";
                    break;
                case "txt":
                    contentType = "text/plain";
                    break;
                case "vcard":
                case "vcf":
                    contentType = "text/vcard";
                    break;
                case "xml":
                    contentType = "application/xml";
                    break;
            }

            return contentType;
        }

        /// <summary>
        /// Check if the specified e-mail address validates. 
        /// </summary>
        /// <param name="address">Address to validate.</param>
        private static bool IsValidEmailAddress(string address)
        {
            return Regex.IsMatch(address, @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        /// <summary>
        /// Calculates an MD5 has of the string provided.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        public static string MD5(string input)
        {
            // Compute the hash into a byte array.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hb = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the byte array into a hexadecimal string representation.
            StringBuilder hashBuilder = new StringBuilder();
            for (int i = 0; i < hb.Length; i++)
                hashBuilder.Append(Convert.ToString(hb[i], 16).PadLeft(2, '0'));

            return hashBuilder.ToString();
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        public static string ReadStreamString(Stream stream, byte[] buffer)
        {
            int bytesRead = stream.Read(buffer, 0, Constants.BUFFERSIZE);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream, up the maximum number of bytes specified.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="maximumBytes">Maximum number of bytes to receive.</param>
        public static string ReadStreamString(Stream stream, byte[] buffer, int maximumBytes)
        {
            int bytesRead = stream.Read(buffer, 0, maximumBytes);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns the string between the first two instances of specified start and end strings.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="start">First string boundary.</param>
        /// <param name="end">Second string boundary.</param>
        public static string ReturnBetween(string haystack, string start, string end)
        {
            int pos = haystack.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);
            if (pos > -1)
            {
                int pos2 = haystack.IndexOf(end, pos + start.Length, StringComparison.InvariantCultureIgnoreCase);
                if (pos2 > -1)
                    return haystack.Substring(pos + start.Length, pos2 - pos - start.Length);
            }
            return "";
        }

        /// <summary>
        /// Sends a string message over stream.
        /// </summary>
        /// <param name="stream">Stream to send message to.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="message">Text to transmit.</param>
        public static void SendStreamString(Stream stream, byte[] buffer, string message)
        {
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(message), 0, buffer, 0, message.Length);
            stream.Write(buffer, 0, message.Length);
            stream.Flush();
        }

        /// <summary>
        /// Encodes a message as a 7-bit string, spanned over lines of 100 base-64 characters each.
        /// </summary>
        /// <param name="message">The message to be 7-bit encoded.</param>
        public static string To7BitString(string message)
        {
            return To7BitString(message, 100);
        }

        /// <summary>
        /// Encodes a message as a 7-bit string.
        /// </summary>
        /// <param name="message">The message to be 7-bit encoded.</param>
        /// <param name="lineLength">The number of base-64 characters per line.</param>
        public static string To7BitString(string message, int lineLength)
        {
            StringBuilder sevenBitBuilder = new StringBuilder();

            // Loop through every lineLength # of characters, adding a new line to our StringBuilder.
            int position = 0;
            int chunkSize = lineLength;
            while (position < message.Length)
            {
                if (message.Length - (position + chunkSize) < 0)
                    chunkSize = message.Length - position;

                sevenBitBuilder.Append(message.Substring(position, chunkSize) + "\r\n");
                position += chunkSize;
            }

            return sevenBitBuilder.ToString();
        }

        /// <summary>
        /// Provides a string representation of one or more e-mail addresses.
        /// </summary>
        /// <param name="addresses">Collection of MailAddresses to display.</param>
        public static string ToMailAddressString(MailAddressCollection addresses)
        {
            StringBuilder addressString = new StringBuilder();

            foreach (MailAddress address in addresses)
            {
                if (!string.IsNullOrEmpty(address.DisplayName))
                    addressString.Append(address.DisplayName + " <" + address.Address + ">; ");
                else
                    addressString.Append(address.Address + "; ");
            }
            if (addressString.Length > 0)
                addressString.Remove(addressString.Length - 2, 2);

            return addressString.ToString();
        }

        /// <summary>
        /// Encode modified UTF-7, as used for IMAP mailbox names.
        /// </summary>
        /// <param name="input">String to encode</param>
        public static string ToModifiedUTF7(string input)
        {
            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder encodedOutputBuilder = new StringBuilder();

            // Loop through each character, adding the encoded character to our output.
            foreach (char inputChar in input)
            {
                // Check if the character is printable ASCII.
                if (inputChar >= '\x20' && inputChar <= '\x7e')
                {
                    if (encodedOutputBuilder.Length > 0)
                    {
                        // Add the encoded string to our output and clear the buffer.
                        outputBuilder.Append(Encoding.UTF8.GetString(Encoding.UTF7.GetBytes(encodedOutputBuilder.ToString())).Replace('/', ',').Replace('+', '&'));
                        encodedOutputBuilder.Clear();
                    }

                    // Encode ampersands.
                    if (inputChar == '&')
                        outputBuilder.Append("&-");
                    else
                        outputBuilder.Append(inputChar);
                }
                else
                    encodedOutputBuilder.Append(inputChar);
            }

            // Add the final encoded string to our output.
            if (encodedOutputBuilder.Length > 0)
                outputBuilder.Append(Encoding.UTF8.GetString(Encoding.UTF7.GetBytes(encodedOutputBuilder.ToString())).Replace('/', ',').Replace('+', '&'));

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Convert a UTF-8 byte array into a Unicode string.
        /// </summary>
        /// <param name="utf8Bytes">Array of UTF-8 encoded characters.</param>
        /// <param name="byteCount">Number of characters to process.</param>
        public static string Utf8toUnicode(byte[] utf8Bytes, int byteCount)
        {
            string outputString = "";
            int counter = 0, compoundValue = 0;

            for (int i = 0; i < byteCount; i++)
            {
                int value = (int)utf8Bytes[i];

                switch (counter)
                {
                    case 0:
                        if (0 <= value && value <= 0x7F)                            // 0xxxxxxx
                            outputString += Utf8toUnicodeNumberToString(value);
                        else if (0xC0 <= value && value <= 0xDF)                    // 110xxxxx
                        {
                            counter = 1;
                            compoundValue = value & 0x1F;
                        }
                        else if (0xE0 <= value && value <= 0xEF)                    // 1110xxxx
                        {
                            counter = 2;
                            compoundValue = value & 0xF;
                        }
                        else if (0xF0 <= value && value <= 0xF7)                    // 11110xxx
                        {
                            counter = 3;
                            compoundValue = value & 0x7;
                        }
                        break;
                    case 1:
                        counter--;
                        outputString += Utf8toUnicodeNumberToString((compoundValue << 6) | (value - 0x80));
                        compoundValue = 0;
                        break;
                    case 2:
                    case 3:
                        if (!(value < 0x80 || value > 0xBF))
                            compoundValue = (compoundValue << 6) | (value - 0x80);
                        counter--;
                        break;
                }
            }

            return outputString;
        }

        /// <summary>
        /// Escape UUEncoded (Unix-to-Unix encoded) message.
        /// </summary>
        /// <param name="input">Block of input to decode to plain-text.</param>
        public static string UUDecode(string input)
        {
            StringBuilder outputBuilder = new StringBuilder();

            // Process each line.
            string[] lines = input.Replace("\r", "").Split('\n');
            foreach (string line in lines)
                outputBuilder.Append(Functions.UUDecodeLine(line));

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Convert a message to its UUEncoded (Unix-to-Unix encoding) representation.
        /// </summary>
        /// <param name="input">Block of input to encode.</param>
        public static string UUEncode(string input)
        {
            StringBuilder outputBuilder = new StringBuilder();

            // UUEncoding requires the message's byte count to be a multiple of 3.
            if (input.Length % 3 != 0)
                input += new String(' ', 3 - input.Length % 3);

            // Process every three bytes according to the UUEncode algorithm.
            for (int i = 1; i <= input.Length; i += 3)
            {
                outputBuilder.Append(Convert.ToString((char)((int)Convert.ToChar(input.Substring(i - 1, 1)) / 4 + 32)));
                outputBuilder.Append(Convert.ToString((char)((int)Convert.ToChar(input.Substring(i - 1, 1)) % 4 * 16 + (int)Convert.ToChar(input.Substring(i, 1)) / 16 + 32)));
                outputBuilder.Append(Convert.ToString((char)((int)Convert.ToChar(input.Substring(i, 1)) % 16 * 4 + (int)Convert.ToChar(input.Substring(i + 1, 1)) / 64 + 32)));
                outputBuilder.Append(Convert.ToString((char)((int)Convert.ToChar(input.Substring(i + 1, 1)) % 64 + 32)));
            }

            return outputBuilder.ToString();
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Helper function for converting UTF-8 bytes to Unicode strings.
        /// </summary>
        /// <param name="characterCode"></param>
        /// <returns></returns>
        private static string Utf8toUnicodeNumberToString(int characterCode)
        {
            string output = "";

            if (characterCode <= 0xFFFF)
                output += (char)characterCode;
            else if (characterCode <= 0x10FFFF)
            {
                characterCode -= 0x10000;
                output += (char)(0xD800 | (characterCode >> 10)) + (char)(0xDC00 | (characterCode & 0x3FF));
            }

            return output;
        }

        /// <summary>
        /// Helper function to UUDecode a single line of input.
        /// </summary>
        /// <param name="inputLine">Line of input to decode to plain-text.</param>
        private static string UUDecodeLine(string inputLine)
        {
            StringBuilder outputBuilder = new StringBuilder();

            char[] lineAsCharacterArray = inputLine.ToCharArray();

            if (lineAsCharacterArray.Length < 1)
                return "";
            if (lineAsCharacterArray[0] == '`')
                return "";

            uint[] lineAsUintArray = new uint[lineAsCharacterArray.Length];
            for (int ii = 0; ii < lineAsCharacterArray.Length; ii++)
                lineAsUintArray[ii] = (uint)lineAsCharacterArray[ii] - 32 & 0x3f;

            int length = (int)lineAsUintArray[0];
            if ((int)(length / 3.0 + 0.999999999) * 4 > lineAsCharacterArray.Length - 1)
                return "";

            int i = 1;
            int j = 0;
            while (length > j + 3)
            {
                outputBuilder.Append((char)((lineAsUintArray[i] << 2 & 0xfc | lineAsUintArray[i + 1] >> 4 & 0x3) & 0xff));
                outputBuilder.Append((char)((lineAsUintArray[i + 1] << 4 & 0xf0 | lineAsUintArray[i + 2] >> 2 & 0xf) & 0xff));
                outputBuilder.Append((char)((lineAsUintArray[i + 2] << 6 & 0xc0 | lineAsUintArray[i + 3] & 0x3f) & 0xff));
                i += 4;
                j += 3;
            }

            if (length > j)
                outputBuilder.Append((char)((lineAsUintArray[i] << 2 & 0xfc | lineAsUintArray[i + 1] >> 4 & 0x3) & 0xff));
            if (length > j + 1)
                outputBuilder.Append((char)((lineAsUintArray[i + 1] << 4 & 0xf0 | lineAsUintArray[i + 2] >> 2 & 0xf) & 0xff));
            if (length > j + 2)
                outputBuilder.Append((char)((lineAsUintArray[i + 2] << 6 & 0xc0 | lineAsUintArray[i + 3] & 0x3f) & 0xff));

            return outputBuilder.ToString();
        }
        #endregion
    }
}
