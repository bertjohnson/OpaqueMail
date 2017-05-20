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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Provides common functions used by other OpaqueMail classes.
    /// </summary>
    public static class Functions
    {
        #region Public Methods
        /// <summary>
        /// Determine whether a message without no encoding specified appears to be Base-64.
        /// </summary>
        /// <param name="body">Message body to check.</param>
        /// <returns>Whether the message appears to be Base-64 encoded.</returns>
        public static bool AppearsBase64(string body)
        {
            body = body.Replace("\r\n", "");
            if (body.Length % 4 == 0 && (body.EndsWith("=") || body.IndexOf(" ") < 0))
                return true;
            else
            {
                // Experimental check for non-Base64 characters.
                if (body.IndexOf(".") < 0 && body.IndexOf(",") < 0 && body.IndexOf("<") < 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Determine whether the content type of a message appears to be HTML.
        /// </summary>
        /// <param name="body">Message body to check.</param>
        /// <returns>Whether the content type of the message appears to be HTML.</returns>
        public static bool AppearsHTML(string body)
        {
            int tagCursor = body.IndexOf("<");
            if (tagCursor > -1){
                int endTagCursor = body.IndexOf(">", tagCursor + 1);
                if (endTagCursor > -1 && (endTagCursor - tagCursor) < 128)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determine whether a message without no encoding specified appears to be Quoted-Printable.
        /// </summary>
        /// <param name="body">Message body to check.</param>
        /// <returns>Whether the message appears to be Quoted-Printable encoded.</returns>
        public static bool AppearsQuotedPrintable(string body)
        {
            if (body.IndexOf("=\r\n") > -1 || body.IndexOf("=20") > -1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Create HTML links for embedded URLs.
        /// </summary>
        /// <remarks>Useful for text/plain messages.</remarks>
        /// <param name="html">HTML block to process.</param>
        /// <returns>The HTML block with any URLs encapsulated in HTML links.</returns>
        public static string ConvertPlainTextToHTML(string html)
        {
            // Handle the special case of email starting or endding with a link by padding with spaces on either side, which will be removed at the end.
            html = " " + html + " ";

            // Treat all whitespace equivalently and ignore case.
            string canonicalHtml = html.Replace("\t", " ").Replace("\r\n", "     ").Replace("\r", " ").Replace("\n", " ").ToLower();

            // Convert angle brackets and change line breaks to BR tags.
            html = html.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "<br/>");
            
            // Build a new string using the following buffer.
            StringBuilder htmlBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            int pos = 0, lastPos = 0;
            while (pos > -1)
            {
                lastPos = pos;

                // Find the next link, whether using the HTTP or HTTPS protocol.
                int httpPos = canonicalHtml.IndexOf(" http://", pos, StringComparison.Ordinal);
                int httpsPos = canonicalHtml.IndexOf(" https://", pos, StringComparison.Ordinal);
            
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
                    int endPos = canonicalHtml.IndexOf(" ", pos + 1, StringComparison.Ordinal);
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
        /// Decode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to decode.</param>
        /// <param name="contentTransferEncoding">Transfer encoding to use.</param>
        /// <returns>Decoded representation of the message.</returns>
        public static string Decode(byte[] input, string contentTransferEncoding)
        {
            return Decode(input, contentTransferEncoding, "", Encoding.UTF8);
        }

        /// <summary>
        /// Decode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to decode.</param>
        /// <param name="contentTransferEncoding">Transfer encoding to use.</param>
        /// <param name="charSet">Character set to use.</param>
        /// <param name="encoding">Text encoding to use.</param>
        /// <returns>Decoded representation of the message.</returns>
        public static string Decode(byte[] input, string contentTransferEncoding, string charSet, Encoding encoding)
        {
            switch (contentTransferEncoding.ToUpper())
            {
                case "BASE64":
                    return FromBase64(encoding.GetString(input), encoding);
                case "QUOTED-PRINTABLE":
                    return FromQuotedPrintable(encoding.GetString(input));
                case "7BIT":
                case "8BIT":
                case "BINARY":
                default:
                    return encoding.GetString(input);
            }
        }

        /// <summary>
        /// Decode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to decode.</param>
        /// <param name="contentTransferEncoding">Transfer encoding to use.</param>
        /// <returns>Decoded representation of the message.</returns>
        public static string Decode(string input, string contentTransferEncoding)
        {
            return Decode(input, contentTransferEncoding, "", Encoding.UTF8);
        }

        /// <summary>
        /// Decode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to decode.</param>
        /// <param name="contentTransferEncoding">Transfer encoding to use.</param>
        /// <param name="charSet">Character set to use.</param>
        /// <param name="encoding">Text encoding to use.</param>
        /// <returns>Decoded representation of the message.</returns>
        public static string Decode(string input, string contentTransferEncoding, string charSet, Encoding encoding)
        {
            switch (contentTransferEncoding.ToUpper())
            {
                case "BASE64":
                    return FromBase64(input);
                case "QUOTED-PRINTABLE":
                    return FromQuotedPrintable(input, charSet, encoding);
                case "7BIT":
                case "8BIT":
                case "BINARY":
                default:
                    return input;
            }
        }

        /// <summary>
        /// Escapes embedded encoding of email headers.
        /// </summary>
        /// <param name="header">Email header to be decoded.</param>
        /// <returns>The decoded email header.</returns>
        public static string DecodeMailHeader(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                try
                {
                    // Build a new string using the following buffer.
                    StringBuilder headerBuilder = new StringBuilder(Constants.TINYSBSIZE);

                    int cursor = 0, lastCursor = 0;
                    int headerLength = header.Length;
                    while (cursor > -1)
                    {
                        lastCursor = cursor;
                        cursor = header.IndexOf("=?", cursor, StringComparison.Ordinal);
                        if (cursor > -1)
                        {
                            headerBuilder.Append(header.Substring(lastCursor, cursor - lastCursor));

                            int middleCursor = header.IndexOf("?", cursor + 2, StringComparison.Ordinal);

                            if (middleCursor > -1 && middleCursor < headerLength - 2)
                            {
                                int endCursor = header.IndexOf("?=", middleCursor + 3, StringComparison.Ordinal);
                                if (endCursor > -1 && endCursor > middleCursor + 1)
                                {
                                    // Try to create a decoder for the encoding.
                                    string charSet = NormalizeCharSet(header.Substring(cursor + 2, middleCursor - cursor - 2).ToUpper().Replace("\"", ""));

                                    Encoding encoding = Encoding.GetEncoding(charSet);

                                    byte[] encodedBytes = null;
                                    switch (header.Substring(middleCursor + 1, 2).ToUpper())
                                    {
                                        case "B?":
                                            encodedBytes = Convert.FromBase64String(header.Substring(middleCursor + 3, endCursor - middleCursor - 3));
                                            break;
                                        case "Q?":
                                            encodedBytes = encoding.GetBytes(FromQuotedPrintable(header.Substring(middleCursor + 3, endCursor - middleCursor - 3).Replace("_", " "), charSet, encoding));
                                            break;
                                        default:
                                            encodedBytes = encoding.GetBytes(header.Substring(middleCursor, endCursor - middleCursor - 2));
                                            break;
                                    }

                                    // Append the decoded string.
                                    headerBuilder.Append(Encoding.UTF8.GetString(Encoding.Convert(encoding, Encoding.UTF8, encodedBytes)));

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
                catch
                {
                    // If the header is malformed, return it as passed in.
                    return header;
                }
            }
            else
                return header;
        }

        /// <summary>
        /// Return a MIME parameter with languages and character set information extracted.
        /// </summary>
        /// <param name="mimeParameter">MIME parameter to process.</param>
        /// <returns>Decoded MIME parameter.</returns>
        private static string DecodeMimeParameter(string mimeParameter)
        {
            string characterSet, language;
            return DecodeMimeParameter(mimeParameter, out characterSet, out language);
        }

        /// <summary>
        /// Return a MIME parameter with languages and character set information extracted.
        /// </summary>
        /// <param name="mimeParameter">MIME parameter to process.</param>
        /// <param name="characterSet">Character set of the encoded MIME parameter.</param>
        /// <param name="language">Language of the encoded MIME parameter.</param>
        /// <returns>Decoded MIME parameter.</returns>
        private static string DecodeMimeParameter(string mimeParameter, out string characterSet, out string language)
        {
            string[] mimeHeaderParts = mimeParameter.Split(new char[] { '\'' }, 3);
            if (mimeHeaderParts.Length == 3)
            {
                // Split the MIME header parts into their components.
                characterSet = mimeHeaderParts[0];
                language = mimeHeaderParts[1];
                return mimeHeaderParts[2];
            }
            else
            {
                // If no valid encoding is found, return the header as-is.
                characterSet = null;
                language = null;
                return mimeParameter;
            }
        }

        /// <summary>
        /// Convert "CID: object" references to Base-64 encoded versions.
        /// </summary>
        /// <remarks>Useful for displaying text/html messages with image references.</remarks>
        /// <param name="html">HTML block to process.</param>
        /// <param name="attachments">Collection of attachments available to be embedded.</param>
        /// <returns>The HTML block with any CID: object references replaced by their Base-64 encoded bytes.</returns>
        public static string EmbedAttachments(string html, AttachmentCollection attachments)
        {
            // Build a new string using the following buffer.
            StringBuilder htmlBuilder = new StringBuilder(Constants.MEDIUMSBSIZE);

            int srcStartPos = 0, lastPos = 0;
            while (srcStartPos > -1)
            {
                // Find the next SRC= attribute and handle either single or double quotes.
                int srcStartQuotePos = html.IndexOf("src=\"cid:", srcStartPos, StringComparison.Ordinal);
                int srcStartApostrophePos = html.IndexOf("src='cid:", srcStartPos, StringComparison.Ordinal);

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
                    int srcEndPos = html.IndexOf(srcEndDelimiter, srcStartPos + 9, StringComparison.Ordinal);
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
                                htmlBuilder.Append("data:" + attachment.MediaType + ";base64,");

                                matchingAttachmentFound = true;
                                byte[] contentStreamBytes = ((MemoryStream)attachment.ContentStream).ToArray();

                                htmlBuilder.Append(ToBase64String(contentStreamBytes, 0, contentStreamBytes.Length));
                            }
                        }

                        // If the current object hasn't been matched, look for a matching file name.
                        if (!matchingAttachmentFound)
                        {
                            // Ignore the non-file name portion of this Content-ID.
                            int cidAtPos = cid.IndexOf("@", StringComparison.Ordinal);
                            if (cidAtPos > -1)
                                cid = cid.Substring(0, cidAtPos);

                            foreach (Attachment attachment in attachments)
                            {
                                if (attachment.Name == cid)
                                {
                                    htmlBuilder.Append("data:" + attachment.MediaType + ";base64,");

                                    matchingAttachmentFound = true;
                                    byte[] contentStreamBytes = ((MemoryStream)attachment.ContentStream).ToArray();

                                    htmlBuilder.Append(ToBase64String(contentStreamBytes, 0, contentStreamBytes.Length));
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
        /// Encode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to encode.</param>
        /// <param name="ContentTransferEncoding">Transfer encoding to use.</param>
        /// <returns>Encoded representation of the message.</returns>
        public static string Encode(byte[] input, string ContentTransferEncoding)
        {
            switch (ContentTransferEncoding.ToUpper())
            {
                case "7BIT":
                    return To7BitString(Encoding.UTF8.GetString(input));
                case "BASE64":
                    return ToBase64String(input);
                case "QUOTED-PRINTABLE":
                    return ToQuotedPrintable(Encoding.UTF8.GetString(input));
                case "8BIT":
                case "BINARY":
                default:
                    return Encoding.UTF8.GetString(input);
            }
        }

        /// <summary>
        /// Encode a message according to the Content Transfer Encoding specified.
        /// </summary>
        /// <param name="input">Message to encode.</param>
        /// <param name="ContentTransferEncoding">Transfer encoding to use.</param>
        /// <returns>Encoded representation of the message.</returns>
        public static string Encode(string input, string ContentTransferEncoding)
        {
            switch (ContentTransferEncoding.ToUpper())
            {
                case "7BIT":
                    return To7BitString(input);
                case "BASE64":
                    return ToBase64String(input);
                case "QUOTED-PRINTABLE":
                    return ToQuotedPrintable(input);
                case "8BIT":
                case "BINARY":
                default:
                    return input;
            }
        }

        /// <summary>
        /// Encodes email headers to escape extended characters.
        /// </summary>
        /// <param name="header">Email header to be encoded.</param>
        /// <returns>Base-64 encoded version of the email header.</returns>
        public static string EncodeMailHeader(string header)
        {
            return EncodeMailHeader(header, -1);
        }

        /// <summary>
        /// Encodes email headers to escape extended characters.
        /// </summary>
        /// <param name="header">Email header to be encoded.</param>
        /// <param name="maxChunkSize">Maximum number of bytes in each encoded chunk.</param>
        /// <returns>Base-64 encoded version of the email header.</returns>
        public static string EncodeMailHeader(string header, int maxChunkSize)
        {
            if (string.IsNullOrEmpty(header))
                return "";

            // If passed value is already folded, unfold it prior to encoding.
            header = header.Replace("\r\n ", " ").Replace("\r\n\t", " ");

            int lastCharacterExtendedStart = -1;
            StringBuilder headerBuilder = new StringBuilder();
            char[] headerCharacters = header.ToCharArray();
            int max = headerCharacters.Length;
            for (int i = 0; i < max; i++)
            {
                char headerCharacter = headerCharacters[i];
                if (headerCharacter > 127)
                {
                    if (lastCharacterExtendedStart < 0)
                        lastCharacterExtendedStart = i;
                }
                else
                {
                    if (lastCharacterExtendedStart > -1)
                    {
                        int chunkSize = i - lastCharacterExtendedStart;
                        if (chunkSize > maxChunkSize)
                            chunkSize = maxChunkSize;

                        for (int j = lastCharacterExtendedStart; j < i; j += maxChunkSize)
                        {
                            if (j + chunkSize <= i)
                                headerBuilder.Append("=?UTF-8?B?" + ToBase64String(header.Substring(j, chunkSize)) + "?=");
                            else
                                headerBuilder.Append("=?UTF-8?B?" + ToBase64String(header.Substring(j)) + "?=");
                        }

                        lastCharacterExtendedStart = -1;
                    }

                    headerBuilder.Append(headerCharacter);
                }
            }

            if (lastCharacterExtendedStart > -1)
            {
                for (int j = lastCharacterExtendedStart; j < max; j += maxChunkSize)
                {
                    if (j + maxChunkSize <= max)
                        headerBuilder.Append("=?UTF-8?B?" + ToBase64String(header.Substring(j, maxChunkSize)) + "?=");
                    else
                        headerBuilder.Append("=?UTF-8?B?" + ToBase64String(header.Substring(j)) + "?=");
                }

                lastCharacterExtendedStart = 0;
            }

            return headerBuilder.ToString();
        }

        /// <summary>
        /// Quotes the name of the mailbox if it contains spaces.
        /// </summary>
        /// <param name="mailboxName">Name of the mailbox.</param>
        /// <returns></returns>
        public static string EscapeMailboxName(string mailboxName)
        {
            // Encode ampersands and Unicode characters.
            string encodedMailboxName = Functions.ToModifiedUTF7(mailboxName);
            if (encodedMailboxName != mailboxName || mailboxName.IndexOf(" ") > -1)
                return "\"" + encodedMailboxName.Replace("\"", "\\\"") + "\"";
            else
                return mailboxName;
        }

        /// <summary>
        /// Return an embedded MIME parameter, observing character set encoding.
        /// </summary>
        /// <param name="mimeHeader">MIME header containing the parameter to extract.</param>
        /// <param name="mimeParameter">MIME parameter to extract.</param>
        /// <returns>Extracted MIME parameter, properly formatted.</returns>
        public static string ExtractMimeParameter(string mimeHeader, string mimeParameter)
        {
            int parameterPos = mimeHeader.IndexOf(mimeParameter, StringComparison.OrdinalIgnoreCase);
            if (parameterPos > -1)
            {
                int asteriskPos = mimeHeader.IndexOf("*", parameterPos + mimeParameter.Length);
                int equalsPos = mimeHeader.IndexOf("=", parameterPos + mimeParameter.Length);

                if (equalsPos > -1 && (asteriskPos == -1 || equalsPos < asteriskPos))
                {
                    // The parameter isn't encoded, so return it as-is.
                    string closedMimeHeader = mimeHeader + ";";
                    string returnValue = ReturnBefore(closedMimeHeader.Substring(equalsPos + 1), ";").Trim();

                    if (returnValue.StartsWith("\"") && returnValue.EndsWith("\""))
                        return returnValue.Substring(1, returnValue.Length - 2);
                    else
                        return returnValue;
                }
                else if (asteriskPos > -1)
                {
                    // The parameter is language-encoded, per RFC2184.
                    if (asteriskPos == equalsPos - 1)
                    {
                        // The parameter is encoded on one line.
                        string closedMimeHeader = mimeHeader + ";";

                        string returnValue = ReturnBefore(closedMimeHeader.Substring(equalsPos + 1), ";");
                        if (returnValue.StartsWith("\"") && returnValue.EndsWith("\""))
                            return DecodeMimeParameter(returnValue.Substring(1, returnValue.Length - 2));
                        else
                            return DecodeMimeParameter(returnValue);
                    }
                    else
                    {
                        // The parameter is encoded on one or more lines.
                        string closedMimeHeader = mimeHeader + "\r\n";

                        int index = 0;
                        int.TryParse(mimeHeader.Substring(asteriskPos + 1, equalsPos - asteriskPos - 1).Replace("*", ""), out index);

                        StringBuilder outputBuilder = new StringBuilder();

                        // Loop through each component of the parameter.
                        bool incrementing = true;
                        while (incrementing)
                        {
                            string indexString = index.ToString();

                            asteriskPos = mimeHeader.IndexOf(mimeParameter + "*" + indexString + "*=");
                            equalsPos = mimeHeader.IndexOf(mimeParameter + "*" + indexString + "=");

                            if (asteriskPos > -1 && (asteriskPos < equalsPos || equalsPos == -1))
                            {
                                string encodedMimeHeader = DecodeMimeParameter(ReturnBefore(closedMimeHeader.Substring(asteriskPos + mimeParameter.Length + indexString.Length + 3), "\r\n"));
                                if (encodedMimeHeader.StartsWith("\"") && encodedMimeHeader.EndsWith("\""))
                                    outputBuilder.Append(encodedMimeHeader.Substring(1, encodedMimeHeader.Length - 2));
                                else
                                    outputBuilder.Append(encodedMimeHeader);
                            }
                            else if (equalsPos > -1)
                            {
                                string unencodedMimeHeader = ReturnBefore(closedMimeHeader.Substring(equalsPos + mimeParameter.Length + indexString.Length + 2), "\r\n");
                                if (unencodedMimeHeader.StartsWith("\"") && unencodedMimeHeader.EndsWith("\""))
                                    outputBuilder.Append(unencodedMimeHeader.Substring(1, unencodedMimeHeader.Length - 2));
                                else
                                    outputBuilder.Append(unencodedMimeHeader);
                            }
                            else
                                incrementing = false;

                            index++;
                        }

                        return outputBuilder.ToString();
                    }
                }                
            }

            return "";
        }
        
        /// <summary>
        /// Returns a base-64 string representing the original input.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>Base-64 representation of the input string.</returns>
        public static string FromBase64(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                    return Encoding.UTF8.GetString(System.Convert.FromBase64String(input));
                else
                    return "";
            }
            catch (Exception)
            {
                return input;
            }
        }

        /// <summary>
        /// Returns a base-64 string representing the original input.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <param name="encoding">Encoding to use.</param>
        /// <returns>Base-64 representation of the input string.</returns>
        public static string FromBase64(string input, Encoding encoding)
        {
            try
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;

                if (!string.IsNullOrEmpty(input))
                    return encoding.GetString(System.Convert.FromBase64String(input));
                else
                    return "";
            }
            catch (Exception)
            {
                return input;
            }
        }

        /// <summary>
        /// Decode modified UTF-7, as used for IMAP mailbox names.
        /// </summary>
        /// <param name="input">String to decode</param>
        /// <returns>Decoded version of the input string.</returns>
        public static string FromModifiedUTF7(string input)
        {
            StringBuilder outputBuilder = new StringBuilder(Constants.TINYSBSIZE);

            int ampCursor = 0, lastAmpCursor = 0;
            while (ampCursor > -1)
            {
                lastAmpCursor = ampCursor;
                ampCursor = input.IndexOf("&", ampCursor, StringComparison.Ordinal);
                if (ampCursor > -1)
                {
                    outputBuilder.Append(input.Substring(lastAmpCursor, ampCursor - lastAmpCursor));
                    int minusCursor = input.IndexOf("-", ampCursor + 1, StringComparison.Ordinal);
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
        /// Escapes quoted-printable encoding.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The decoded version of the quoted-printable string passed in.</returns>
        public static string FromQuotedPrintable(string input)
        {
            return FromQuotedPrintable(input, "", Encoding.Default);
        }

            /// <summary>
        /// Escapes quoted-printable encoding.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <param name="charSet">Character set used for encoding.</param>
        /// <param name="encoding">Encoding to use.</param>
        /// <returns>The decoded version of the quoted-printable string passed in.</returns>
        public static string FromQuotedPrintable(string input, string charSet, Encoding encoding)
        {
            try {
                // Build a new string using the following buffer.
                StringBuilder outputBuilder = new StringBuilder(Constants.SMALLSBSIZE);

                // Buffer for holding bytes of non-ASCII characters.
                byte[] byteBuffer = new byte[Constants.SMALLBUFFERSIZE];
                // Total count of bytes stored in byteBuffer.
                int byteBufferLength = 0;

                // If no encoding is passed in, but a character set is specified, create the encoding.
                if (encoding == null)
                {
                    if (!string.IsNullOrEmpty(charSet))
                    {
                        encoding = Encoding.GetEncoding(NormalizeCharSet(charSet));
                    }
                    else
                        encoding = Encoding.UTF8;
                }

                // Loop through and process quoted-printable strings, denoted by equals signs.
                int equalsPos = 0, lastPos = 0;
                while (equalsPos > -1)
                {
                    lastPos = equalsPos;
                    equalsPos = input.IndexOf("=", equalsPos, StringComparison.Ordinal);
                    if (equalsPos > -1 && equalsPos < input.Length - 2)
                    {
                        outputBuilder.Append(input.Substring(lastPos, equalsPos - lastPos));

                        string afterEquals = input.Substring(equalsPos + 1, 2);
                        // When byteBufferEnd, decode string from byteBuffer.
                        bool byteBufferEnd = true;

                        switch (afterEquals)
                        {
                            case "\r\n":
                                byteBufferEnd = false;
                                break;
                            default:
                                byte theByte;
                                if (Byte.TryParse(afterEquals, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out theByte))
                                {
                                    byteBuffer[byteBufferLength++] = theByte;
                                    if (byteBufferLength < byteBuffer.Length && equalsPos < input.Length - 5)
                                    {
                                        if (input[equalsPos + 3] == '=' && input[equalsPos + 4] != '\r')
                                            byteBufferEnd = false;
                                    }
                                }
                                break;
                        }

                        if (byteBufferEnd && byteBufferLength > 0)
                        {
                            outputBuilder.Append(encoding.GetString(byteBuffer, 0, byteBufferLength));
                            byteBufferLength = 0;
                        }

                        equalsPos += 3;
                    }
                    else
                    {
                        outputBuilder.Append(input.Substring(lastPos));
                        equalsPos = -1;
                    }
                }

                // Remove hanging equals.
                if (outputBuilder[outputBuilder.Length - 1] == '=')
                    outputBuilder.Remove(outputBuilder.Length - 1, 1);

                return outputBuilder.ToString();
            }
            catch (Exception)
            {
                // If the input is malformed, return it as passed in.
                return input;
            }
        }

        /// <summary>
        /// Provide a best guess of a file's content type based on its filename.
        /// </summary>
        /// <param name="fileExtension">File extension to interpret.</param>
        /// <returns>A string representing the best guess for the file extension's content type.</returns>
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
                case "rtf":
                    contentType = "text/richtext";
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
                case "zip":
                    contentType = "application/zip";
                    break;
            }

            return contentType;
        }

        /// <summary>
        /// Return the machine's fully-qualified domain name.
        /// </summary>
        /// <returns>The machine's fully-qualified domain name.</returns>
        public static string GetLocalFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            return hostName.IndexOf(domainName, StringComparison.Ordinal) > -1 ? hostName : hostName + "." + domainName;
        }

        /// <summary>
        /// Calculates an MD5 has of the string provided.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>A hexadecimal representation of the MD5 hash.</returns>
        public static string MD5(string input)
        {
            // Compute the hash into a byte array.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hb = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the byte array into a hexadecimal string representation.
            StringBuilder hashBuilder = new StringBuilder(Constants.TINYSBSIZE);
            for (int i = 0; i < hb.Length; i++)
                hashBuilder.Append(Convert.ToString(hb[i], 16).PadLeft(2, '0'));

            return hashBuilder.ToString();
        }

        /// <summary>
        /// Replace aliased character sets with their more common representation.
        /// </summary>
        /// <param name="charSet">Character set alias to check.</param>
        /// <returns>Normalized character set name.</returns>
        public static string NormalizeCharSet(string charSet)
        {
            switch (charSet.ToUpper())
            {
                case "ANSI_X3.110-1983":
                    return "iso-8859-1";
                case "CP1252":
                    return "windows-1252";
            }
            return charSet;
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public static string ReadStreamString(Stream stream, byte[] buffer)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="streamReader">StreamReader to receive message from.</param>
        /// <param name="buffer">A character array to streamline bit shuffling.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public static string ReadStreamString(StreamReader streamReader, char[] buffer)
        {
            int bytesRead = streamReader.Read(buffer, 0, buffer.Length);
            return new string(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream, up the maximum number of bytes specified.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="maximumBytes">Maximum number of bytes to receive.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public static string ReadStreamString(Stream stream, byte[] buffer, int maximumBytes)
        {
            int bytesRead = stream.Read(buffer, 0, maximumBytes);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="streamReader">StreamReader to receive message from.</param>
        /// <param name="buffer">A character array to streamline bit shuffling.</param>
        /// <param name="maximumBytes">The maximum number of bytes to return.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public static string ReadStreamString(StreamReader streamReader, char[] buffer, int maximumBytes)
        {
            int bytesRead = streamReader.Read(buffer, 0, maximumBytes);
            return new string(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public async static Task<string> ReadStreamStringAsync(Stream stream, byte[] buffer)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="streamReader">StreamReader to receive message from.</param>
        /// <param name="buffer">A character array to streamline bit shuffling.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public async static Task<string> ReadStreamStringAsync(StreamReader streamReader, char[] buffer)
        {
            int bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length);
            return new string(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream, up the maximum number of bytes specified.
        /// </summary>
        /// <param name="stream">Stream to receive message from.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="maximumBytes">Maximum number of bytes to receive.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public async static Task<string> ReadStreamStringAsync(Stream stream, byte[] buffer, int maximumBytes)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, maximumBytes);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Returns string representation of message sent over stream.
        /// </summary>
        /// <param name="streamReader">StreamReader to receive message from.</param>
        /// <param name="buffer">A character array to streamline bit shuffling.</param>
        /// <param name="maximumBytes">Maximum number of bytes to receive.</param>
        /// <returns>Any text read from the stream connection.</returns>
        public async static Task<string> ReadStreamStringAsync(StreamReader streamReader, char[] buffer, int maximumBytes)
        {
            int bytesRead = await streamReader.ReadAsync(buffer, 0, maximumBytes);
            return new string(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Remove <script/> blocks from HTML.
        /// </summary>
        /// <param name="html">An HTML block whose Javascript code should be removed.</param>
        /// <returns>The HTML block with Javscript removed.</returns>
        public static string RemoveScriptTags(string html)
        {
            // Treat all whitespace equivalently and ignore case.
            string canonicalHtml = html.ToLower();

            // Build a new string using the following buffer.
            StringBuilder htmlBuilder = new StringBuilder(Constants.SMALLSBSIZE);
            
            // First, process <script> blocks.
            int pos = 0, lastPos = 0;
            while (pos > -1)
            {
                lastPos = pos;

                // Find the next link, whether using the HTTP or HTTPS protocol.
                pos = canonicalHtml.IndexOf("<script", pos, StringComparison.Ordinal);

                if (pos > -1)
                {
                    // If another <script> tag is found, add everything since the last one.
                    htmlBuilder.Append(html.Substring(lastPos, pos - lastPos));

                    // Find where the <script> tag is closed, properly handling attributes.
                    bool tagClosed = false;
                    while (!tagClosed)
                    {
                        int quotePos = canonicalHtml.IndexOf("\"", pos, StringComparison.Ordinal);
                        if (quotePos < 0)
                            quotePos = canonicalHtml.Length + 1;
                        int aposPos = canonicalHtml.IndexOf("'", pos, StringComparison.Ordinal);
                        if (aposPos < 0)
                            aposPos = canonicalHtml.Length + 1;
                        int anglePos = canonicalHtml.IndexOf(">", pos, StringComparison.Ordinal);
                        if (anglePos < 0)
                            anglePos = canonicalHtml.Length + 1;

                        if (quotePos < aposPos && quotePos < anglePos)
                        {
                            int endQuotePos = canonicalHtml.IndexOf("\"", quotePos + 1, StringComparison.Ordinal);
                            if (endQuotePos > -1)
                                pos = endQuotePos + 1;
                            else
                                pos = -1;
                        }
                        else if (aposPos < quotePos && aposPos < anglePos)
                        {
                            int endAposPos = canonicalHtml.IndexOf("'", aposPos + 1, StringComparison.Ordinal);
                            if (endAposPos > -1)
                                pos = endAposPos + 1;
                            else
                                pos = -1;
                        }
                        else if (anglePos <= canonicalHtml.Length)
                        {
                            if (canonicalHtml[anglePos - 1] == '/')
                            {
                                tagClosed = true;
                                pos = anglePos + 1;
                            }
                            else
                            {
                                int endScriptPos = canonicalHtml.IndexOf("</script", anglePos, StringComparison.Ordinal);
                                if (endScriptPos > -1)
                                {
                                    int closeEndScriptPos = canonicalHtml.IndexOf(">", endScriptPos + 7, StringComparison.Ordinal);
                                    if (closeEndScriptPos > -1)
                                    {
                                        tagClosed = true;
                                        pos = closeEndScriptPos + 1;
                                    }
                                    else
                                        pos = -1;
                                }
                                else
                                    pos = -1;
                            }
                        }
                        else
                            pos = -1;
                    }
                }
                else
                    htmlBuilder.Append(html.Substring(lastPos));
            }

            // Finally, remove any onclick, onmouseover, etc. event handlers.
            html = htmlBuilder.ToString();
            canonicalHtml = html.ToLower();

            string[] eventHandlers = new string[] { "onabort", "onblur", "onclick", "ondblclick",
                "onerror", "onfocus", "onfocusin", "onfocusout", "onkeydown", "onkeypress", "onkeyup",
                "onload", "onmousedown", "onmouseenter", "onmouseleave", "onmousemove", "onmouseover",
                "onmouseout", "onmouseup", "onresize", "onscroll", "onselect", "onunload", "onwheel" };
            foreach (string eventHandler in eventHandlers)
            {
                // Only proceed if the event handler name is found.
                if (canonicalHtml.IndexOf(eventHandler, StringComparison.Ordinal) > -1)
                {
                    htmlBuilder.Clear();

                    pos = lastPos = 0;
                    while (pos > -1)
                    {
                        lastPos = pos;
                        pos = canonicalHtml.IndexOf(eventHandler, pos, StringComparison.Ordinal);

                        if (pos > -1)
                        {
                            // Check if we're currently within a tag.
                            int lastOpenAngle = canonicalHtml.LastIndexOf("<", pos);
                            int lastCloseAngle = canonicalHtml.LastIndexOf(">", pos);
                            if (lastOpenAngle > lastCloseAngle)
                            {
                                // We're currently in a tag.
                                htmlBuilder.Append(html.Substring(lastPos, pos - lastPos));

                                int equalsPos = canonicalHtml.IndexOf("=", pos, StringComparison.Ordinal);
                                int nextCloseAnglePos = canonicalHtml.IndexOf(">", pos, StringComparison.Ordinal);

                                // Only continue processing if there's an equals sign after the event handler name and before the tag closes.
                                if (equalsPos > -1 && nextCloseAnglePos > equalsPos)
                                {
                                    int quotePos = canonicalHtml.IndexOf("\"", equalsPos, StringComparison.Ordinal);
                                    if (quotePos < 0)
                                        quotePos = canonicalHtml.Length + 1;
                                    int aposPos = canonicalHtml.IndexOf("'", equalsPos, StringComparison.Ordinal);
                                    if (aposPos < 0)
                                        aposPos = canonicalHtml.Length + 1;

                                    if (quotePos < aposPos && quotePos < nextCloseAnglePos)
                                    {
                                        int endQuotePos = canonicalHtml.IndexOf("\"", quotePos + 1, StringComparison.Ordinal);
                                        if (endQuotePos > -1 && endQuotePos < nextCloseAnglePos)
                                            pos = endQuotePos + 1;
                                        else
                                            pos = nextCloseAnglePos;
                                    }
                                    else if (aposPos <= canonicalHtml.Length)
                                    {
                                        int endAposPos = canonicalHtml.IndexOf("'", aposPos + 1, StringComparison.Ordinal);
                                        if (endAposPos > -1)
                                            pos = endAposPos + 1;
                                        else
                                            pos = nextCloseAnglePos;
                                    }
                                    else
                                        pos = nextCloseAnglePos;
                                }
                                else
                                    pos = nextCloseAnglePos;
                            }
                        }
                        else
                            htmlBuilder.Append(html.Substring(lastPos));
                    }

                    html = htmlBuilder.ToString();
                    canonicalHtml = html.ToLower();
                }
            }

            return html;
        }

        /// <summary>
        /// Returns the string before the specified end string.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="endString">String boundary.</param>
        /// <returns>Any text found in the haystack before the specified end string.</returns>
        public static string ReturnBefore(string haystack, string endString)
        {
            int pos = haystack.IndexOf(endString, StringComparison.Ordinal);
            if (pos > -1)
                return haystack.Substring(0, pos);
            return "";
        }

        /// <summary>
        /// Replace the string between the first two instances of specified start and end strings.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="startString">First string boundary.</param>
        /// <param name="endString">Second string boundary.</param>
        /// <param name="value">String to replace the substring with.</param>
        /// <returns>The haystack with value replacing any text between the specified start and end strings.</returns>
        public static string ReplaceBetween(string haystack, string startString, string endString, string value)
        {
            return ReplaceBetween(haystack, startString, endString, 0, value);
        }

        /// <summary>
        /// Replace the string between the first two instances of specified start and end strings.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="startString">First string boundary.</param>
        /// <param name="endString">Second string boundary.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="value">String to replace the substring with.</param>
        /// <returns>The haystack with value replacing any text between the specified start and end strings.</returns>
        public static string ReplaceBetween(string haystack, string startString, string endString, int startIndex, string value)
        {
            int pos = haystack.IndexOf(startString, startIndex, StringComparison.Ordinal);
            if (pos > -1)
            {
                int pos2 = haystack.IndexOf(endString, pos + startString.Length, StringComparison.Ordinal);
                if (pos2 > -1)
                    return haystack.Substring(0, pos + startString.Length) + value + haystack.Substring(pos2);
            }
            return haystack;
        }

        /// <summary>
        /// Returns the string between the first two instances of specified start and end strings.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="startString">First string boundary.</param>
        /// <param name="endString">Second string boundary.</param>
        /// <returns>Any text found in the haystack between the specified start and end strings.</returns>
        public static string ReturnBetween(string haystack, string startString, string endString)
        {
            return ReturnBetween(haystack, startString, endString, 0);
        }

        /// <summary>
        /// Returns the string between the first two instances of specified start and end strings.
        /// </summary>
        /// <param name="haystack">Container string to search within.</param>
        /// <param name="startString">First string boundary.</param>
        /// <param name="endString">Second string boundary.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>Any text found in the haystack between the specified start and end strings.</returns>
        public static string ReturnBetween(string haystack, string startString, string endString, int startIndex)
        {
            int pos = haystack.IndexOf(startString, startIndex, StringComparison.Ordinal);
            if (pos > -1)
            {
                int pos2 = haystack.IndexOf(endString, pos + startString.Length, StringComparison.Ordinal);
                if (pos2 > -1)
                    return haystack.Substring(pos + startString.Length, pos2 - pos - startString.Length);
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
        /// Sends a string message over stream.
        /// </summary>
        /// <param name="streamWriter">StreamWriter to send message to.</param>
        /// <param name="message">Text to transmit.</param>
        public static void SendStreamString(StreamWriter streamWriter, string message)
        {
            streamWriter.Write(message);
            streamWriter.Flush();
        }

        /// <summary>
        /// Sends a string message over stream.
        /// </summary>
        /// <param name="stream">Stream to send message to.</param>
        /// <param name="buffer">A byte array to streamline bit shuffling.</param>
        /// <param name="message">Text to transmit.</param>
        public async static Task SendStreamStringAsync(Stream stream, byte[] buffer, string message)
        {
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(message), 0, buffer, 0, message.Length);
            await stream.WriteAsync(buffer, 0, message.Length);
            await stream.FlushAsync();
        }

        /// <summary>
        /// Sends a string message over stream.
        /// </summary>
        /// <param name="streamWriter">StreamWriter to send message to.</param>
        /// <param name="message">Text to transmit.</param>
        public async static Task SendStreamStringAsync(StreamWriter streamWriter, string message)
        {
            await streamWriter.WriteAsync(message);
            await streamWriter.FlushAsync();
        }

        /// <summary>
        /// Attempt to keep the number of characters per subject line to 78 or fewer.
        /// </summary>
        /// <param name="header">Email header to be spanned.</param>
        /// <returns>The original email header spread over lines no longer than 78 characters each.</returns>
        public static string SpanHeaderLines(string header)
        {
            return SpanHeaderLines(header, 78);
        }

        /// <summary>
        /// Attempt to keep the number of characters per subject line to a certain number of characters.
        /// </summary>
        /// <param name="header">Email header to be spanned.</param>
        /// <param name="maxLineLength">Maximum length for a line.</param>
        /// <returns>The original email header spread over lines no longer than the number of characters specified.</returns>
        public static string SpanHeaderLines(string header, int maxLineLength)
        {
            if (header.Length > maxLineLength)
            {
                StringBuilder headerBuilder = new StringBuilder(Constants.TINYSBSIZE);

                int pos = 0, spacePos = 0, endEncodingPos = 0, lastPos = 0;
                while (pos > -1 && pos < header.Length - maxLineLength)
                {
                    if (lastPos + maxLineLength >= header.Length)
                    {
                        spacePos = header.LastIndexOf(" ");
                        endEncodingPos = header.LastIndexOf("?=");
                    }
                    else
                    {
                        spacePos = header.LastIndexOf(" ", lastPos + maxLineLength);
                        endEncodingPos = header.LastIndexOf("?=", lastPos + maxLineLength);
                    }
                    if (endEncodingPos == -1 || spacePos > endEncodingPos)
                        pos = spacePos;
                    else
                        pos = endEncodingPos;

                    if (pos < 0 || pos == (lastPos - 1))
                        pos = header.IndexOf(" ", lastPos + maxLineLength);

                    if (pos > -1)
                    {
                        while (pos - lastPos > maxLineLength)
                        {
                            headerBuilder.Append(header.Substring(lastPos, maxLineLength) + "\r\n ");

                            lastPos += maxLineLength;
                        }

                        headerBuilder.Append(header.Substring(lastPos, pos - lastPos) + "\r\n ");

                        lastPos = pos;
                    }
                }

                pos = header.Length - 1;
                while (pos - lastPos > maxLineLength)
                {
                    headerBuilder.Append(header.Substring(lastPos, maxLineLength) + "\r\n ");
                    lastPos += maxLineLength;
                }

                headerBuilder.Append(header.Substring(lastPos));

                return headerBuilder.ToString();
            }
            else
                return header;
        }

        /// <summary>
        /// Encodes a message as a 7-bit string, spanned over lines of 100 base-64 characters each.
        /// </summary>
        /// <param name="input">The message to be 7-bit encoded.</param>
        /// <returns>A 7-bit encoded representation of the message.</returns>
        public static string To7BitString(string input)
        {
            return To7BitString(input, 998);
        }

        /// <summary>
        /// Encodes a message as a 7-bit string.
        /// </summary>
        /// <param name="input">The message to be 7-bit encoded.</param>
        /// <param name="lineLength">The number of base-64 characters per line.</param>
        /// <returns>A 7-bit encoded representation of the message.</returns>
        public static string To7BitString(string input, int lineLength)
        {
            StringBuilder sevenBitBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            int position = 0, lastPosition = 0;
            while (position > 0 && position < (input.Length - lineLength))
            {
                lastPosition = position;
                // Find the next linebreak, and move on if it's within lineLength characters.
                position = input.IndexOf("\r\n", position, StringComparison.Ordinal);
                if (position > -1 && (position - lastPosition) <= lineLength)
                    position = position + 2;
                else
                {
                    // If there's no linebreak within lineLength characters, break on the last space within lineLength characters.
                    int endPosition = position + lineLength;
                    if (endPosition > input.Length)
                        endPosition = input.Length;

                    position = input.LastIndexOf(" ", endPosition);
                    if (position > -1 && position > lastPosition)
                    {
                        sevenBitBuilder.Append(input.Substring(lastPosition, position - lastPosition));
                        sevenBitBuilder.Append("\r\n");

                        position = position + 1;
                    }
                    else
                    {
                        // If there's no whitespace within lineLength characters, force a break.
                        sevenBitBuilder.Append(input.Substring(lastPosition, endPosition - lastPosition));
                        position = -1;
                    }
                }
            }

            sevenBitBuilder.Append(input.Substring(lastPosition));

            return sevenBitBuilder.ToString();
        }

        /// <summary>
        /// Converts a string to its equivalent string representation with base-64 digits.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>Base-64 string representation of the input.</returns>
        public static string ToBase64String(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input), Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation with base-64 digits.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>Base-64 string representation of the input.</returns>
        public static string ToBase64String(byte[] inArray)
        {
            return Convert.ToBase64String(inArray, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation with base-64 digits.
        /// Parameters specify the subset as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in inArray.</param>
        /// <param name="length">The number of elements of inArray to convert.</param>
        /// <returns>Base-64 string representation of the input.</returns>
        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            return Convert.ToBase64String(inArray, offset, length, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Provides a string representation of one or more email addresses.
        /// </summary>
        /// <param name="address">MailAddress to display.</param>
        /// <returns>A string listing all email addresses in the collection, with their display names and address.</returns>
        public static string ToMailAddressString(MailAddress address)
        {
            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.DisplayName))
                    return "\"" + address.DisplayName + "\" <" + address.Address + ">";
                else
                    return address.Address;
            }
            return "";
        }

        /// <summary>
        /// Encode modified UTF-7, as used for IMAP mailbox names.
        /// </summary>
        /// <param name="input">String to encode.</param>
        /// <returns>Modified UTF-7 representation of the input.</returns>
        public static string ToModifiedUTF7(string input)
        {
            StringBuilder outputBuilder = new StringBuilder(Constants.TINYSBSIZE);
            StringBuilder encodedOutputBuilder = new StringBuilder(Constants.TINYSBSIZE);

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
        /// Converts a string to its equivalent quoted printable representation.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>Quoted printable string representation of the input.</returns>
        public static string ToQuotedPrintable(string input)
        {
            // If no string is passed in, return.
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder lineBuilder = new StringBuilder();

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            foreach (byte inputByte in inputBytes)
            {
                // Ignore tabs, spaces, and 7-bit characters.
                if ((inputByte == 9) || ((inputByte >= 32) && (inputByte <= 60)) || ((inputByte >= 62) && (inputByte <= 126)))
                    lineBuilder.Append(Convert.ToChar(inputByte));
                else
                {
                    lineBuilder.Append('=');
                    lineBuilder.Append(inputByte.ToString("X2"));
                }

                if (lineBuilder.Length > 74)
                {
                    int cutoff = 75;
                    char lastChar = lineBuilder[cutoff - 1];

                    // Avoid cutting an escape sequence in the middle.
                    if (lastChar == '=')
                        cutoff--;
                    else if (lineBuilder[cutoff - 2] == '=')
                        cutoff-=2;
                    
                    // End every line with a soft break.
                    outputBuilder.AppendLine(lineBuilder.ToString(0, cutoff) + "=");
                    lineBuilder.Remove(0, cutoff);
                }
            }

            outputBuilder.Append(lineBuilder.ToString());
            if (char.IsWhiteSpace(outputBuilder[outputBuilder.Length - 1]))
                outputBuilder.Append('=');

            string output = outputBuilder.ToString();

            // Prevent lines from ending with whitespace.
            return output.Replace(" =\r\n", "=20=\r\n").Replace("\t=\r\n", "=09=\r\n");
        }
        
        /// <summary>
        /// Remove unneeded line breaks from headers, as were added to avoid excessive line lengths.
        /// </summary>
        /// <param name="input">String to unfold.</param>
        /// <returns>Unfolded representation of input.</returns>
        public static string UnfoldWhitespace(string input)
        {
            StringBuilder outputBuilder = new StringBuilder();
            int pos = 0, lastPos = 0, inputLength = input.Length;
            while (pos > -1)
            {
                pos = input.IndexOf("\r\n", lastPos);
                if (pos > -1 && pos < (inputLength - 2))
                {
                    char evaluatedChar = input[pos + 2];

                    // If the line break is followed by whitespace, ignore it.  Otherwise, include the line break.
                    if (evaluatedChar == ' ' || evaluatedChar == '\t')
                    {
                        outputBuilder.Append(input.Substring(lastPos, pos - lastPos));

                        pos += 2;
                        bool inWhitespace = true;
                        while (inWhitespace && pos <= inputLength)
                        {
                            evaluatedChar = input[pos];
                            if (evaluatedChar == ' ' || evaluatedChar == '\t')
                                pos++;
                            else
                                inWhitespace = false;
                        }
                    }
                    else
                    {
                        outputBuilder.Append(input.Substring(lastPos, pos + 2 - lastPos));
                        pos += 2;
                    }

                    lastPos = pos;
                }
                else
                    pos = -1;
            }
            outputBuilder.Append(input.Substring(lastPos));

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Convert a UTF-8 byte array into a Unicode string.
        /// </summary>
        /// <param name="utf8Bytes">Array of UTF-8 encoded characters.</param>
        /// <param name="byteCount">Number of characters to process.</param>
        /// <returns>Unicode string representing the UTF-8 bytes passed in.</returns>
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
                        if (value <= 0x7F)                            // 0xxxxxxx
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
        /// <returns>UUDecoded version of input.</returns>
        public static string UUDecode(string input)
        {
            StringBuilder outputBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            // Process each line.
            string[] lines = input.Replace("\r", "").Split('\n');
            foreach (string line in lines)
                outputBuilder.Append(UUDecodeLine(line));

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Convert a message to its UUEncoded (Unix-to-Unix encoding) representation.
        /// </summary>
        /// <param name="input">Block of input to encode.</param>
        /// <returns>A UUEncoded representation of input.</returns>
        public static string UUEncode(string input)
        {
            StringBuilder outputBuilder = new StringBuilder(Constants.SMALLSBSIZE);

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
        /// <param name="characterCode">The Unicode character's ordinal.</param>
        /// <returns>Unicode string representation of the UTF-8 sequence.</returns>
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
        /// <returns>The single line of UUDecoded input.</returns>
        private static string UUDecodeLine(string inputLine)
        {
            StringBuilder outputBuilder = new StringBuilder(Constants.TINYSBSIZE);

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
