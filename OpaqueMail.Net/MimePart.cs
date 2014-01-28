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
using System.Net.Mime;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    /// <summary>
    /// Represents a node in a MIME encoded message tree.
    /// </summary>
    public class MimePart
    {
        #region Public Members
        /// <summary>Return the string representation of the body.</summary>
        public string Body
        {
            get
            {
                if (!string.IsNullOrEmpty(CharSet))
                    return Encoding.GetEncoding(CharSet).GetString(BodyBytes);
                else
                    return Encoding.UTF8.GetString(BodyBytes);
            }
        }
        /// <summary>Raw contents of the MIME part's body.</summary>
        public byte[] BodyBytes;
        /// <summary>Character Set used to encode the MIME part.</summary>
        public string CharSet = "";
        /// <summary>ID of the MIME part.</summary>
        public string ContentID = "";
        /// <summary>Content Type of the MIME part.</summary>
        public string ContentType = "";
        /// <summary>Content Transfer Encoding of the MIME part.</summary>
        public TransferEncoding ContentTransferEncoding = TransferEncoding.Unknown;
        /// <summary>Filename of the MIME part.</summary>
        public string Name = "";
        /// <summary>Whether the MIME part is part of an S/MIME encrypted envelope.</summary>
        public bool SmimeEncryptedEnvelope = false;
        /// <summary>Whether the MIME part is S/MIME signed.</summary>
        public bool SmimeSigned = false;
        /// <summary>Certificates used when signing messages.</summary>
        public X509Certificate2Collection SmimeSigningCertificates = null;
        /// <summary>Whether the MIME part was S/MIME signed, had its envelope encrypted, and was then signed again.</summary>
        public bool SmimeTripleWrapped = false;
        #endregion Public Members

        #region Constructors
        /// <summary>
        /// Instantiate a MIME part based on the string representation of its body.
        /// </summary>
        /// <param name="name">Filename of the MIME part.</param>
        /// <param name="contentType">Content Type of the MIME part.</param>
        /// <param name="charset">Character Set used to encode the MIME part.</param>
        /// <param name="contentID">ID of the MIME part.</param>
        /// <param name="contentTransferEncoding">Content Transfer Encoding string of the MIME part.</param>
        /// <param name="body">String representation of the MIME part's body.</param>
        public MimePart(string name, string contentType, string charset, string contentID, string contentTransferEncoding, string body)
            : this(name, contentType, charset, contentID, contentTransferEncoding, Encoding.UTF8.GetBytes(body)) { }

        /// <summary>
        /// Instantiate a MIME part based on its body's byte array.
        /// </summary>
        /// <param name="name">Filename of the MIME part.</param>
        /// <param name="contentType">Content Type of the MIME part.</param>
        /// <param name="charset">Character Set used to encode the MIME part.</param>
        /// <param name="contentID">ID of the MIME part.</param>
        /// <param name="contentTransferEncoding">Content Transfer Encoding string of the MIME part.</param>
        /// <param name="bodyBytes">The MIME part's raw bytes.</param>
        public MimePart(string name, string contentType, string charset, string contentID, string contentTransferEncoding, byte[] bodyBytes)
        {
            BodyBytes = bodyBytes;
            ContentType = contentType;
            ContentID = contentID;
            CharSet = charset;
            Name = Functions.DecodeMailHeader(name).Replace("\r", "").Replace("\n", "");

            switch (contentTransferEncoding.ToLower())
            {
                case "base64":
                    ContentTransferEncoding = TransferEncoding.Base64;
                    break;
                case "quoted-printable":
                    ContentTransferEncoding = TransferEncoding.QuotedPrintable;
                    break;
                case "7bit":
                    ContentTransferEncoding = TransferEncoding.SevenBit;
                    break;
                case "8bit":
                    ContentTransferEncoding = TransferEncoding.EightBit;
                    break;
                default:
                    ContentTransferEncoding = TransferEncoding.Unknown;
                    break;
            }
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Extract a list of MIME parts from a multipart/* MIME encoded message.
        /// </summary>
        /// <param name="contentType">Content Type of the outermost MIME part.</param>
        /// <param name="charSet">Character set of the outermost MIME part.</param>
        /// <param name="contentTransferEncoding">Encoding of the outermost MIME part.</param>
        /// <param name="body">The outermost MIME part's contents.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        public static List<MimePart> ExtractMIMEParts(string contentType, string charSet, string contentTransferEncoding, string body, ReadOnlyMailMessageProcessingFlags processingFlags)
        {
            List<MimePart> mimeParts = new List<MimePart>();

            string contentTypeToUpper = contentType.ToUpper();
            if (contentTypeToUpper.StartsWith("MULTIPART/"))
            {
                // Prepare to process each part of the multipart/* message.
                int cursor = 0;

                // Prepend the body with a carriage return and linefeed for consistent boundary matching.
                body = "\r\n" + body;

                // Determine the outermost boundary name.
                string boundaryName = Functions.ReturnBetween(contentType, "boundary=\"", "\"");
                if (boundaryName.Length < 1)
                {
                    cursor = contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
                    if (cursor > -1)
                        boundaryName = contentType.Substring(cursor + 9);
                    cursor = boundaryName.IndexOf(";");
                    if (cursor > -1)
                        boundaryName = boundaryName.Substring(0, cursor);

                    if (boundaryName.StartsWith("\"") && boundaryName.EndsWith("\""))
                        boundaryName = boundaryName.Substring(1, boundaryName.Length - 2);
                    else
                    {
                        // Remove linear whitespace from boundary names.
                        boundaryName = boundaryName.Trim();
                    }
                }
                int boundaryNameLength = boundaryName.Length;

                // Variables used for record keeping with signed S/MIME parts.
                int signatureBlock = -1;
                List<string> mimeBlocks = new List<string>();

                cursor = body.IndexOf("\r\n--" + boundaryName, 0, StringComparison.Ordinal);
                while (cursor > -1)
                {
                    // Calculate the end boundary of the current MIME part.
                    int mimeStartPosition = cursor + boundaryNameLength + 4;
                    int mimeEndPosition = body.IndexOf("\r\n--" + boundaryName, mimeStartPosition, StringComparison.Ordinal);
                    if (mimeEndPosition > -1)
                    {
                        string afterBoundaryEnd = body.Substring(mimeEndPosition + 4 + boundaryNameLength, 2);
                        if (afterBoundaryEnd == "\r\n" || afterBoundaryEnd == "--")
                        {
                            string mimeContents = body.Substring(mimeStartPosition, mimeEndPosition - mimeStartPosition);

                            // Extract the header portion of the current MIME part.
                            int mimeDivider = mimeContents.IndexOf("\r\n\r\n");
                            string mimeHeaders, mimeBody;
                            if (mimeDivider > -1)
                            {
                                mimeHeaders = mimeContents.Substring(0, mimeDivider);
                                mimeBody = mimeContents.Substring(mimeDivider + 4);
                            }
                            else
                            {
                                // The following is a workaround to handle malformed MIME bodies.
                                mimeHeaders = mimeContents;
                                mimeBody = "";

                                int linePos = 0, lastLinePos = 0;
                                while (linePos > -1)
                                {
                                    lastLinePos = linePos;
                                    linePos = mimeHeaders.IndexOf("\r\n", lastLinePos);
                                    if (linePos > -1)
                                    {
                                        string currentLine = mimeContents.Substring(lastLinePos, linePos - lastLinePos);
                                        if (currentLine.Length > 0 && currentLine.IndexOf(":") < 0)
                                        {
                                            mimeBody = mimeContents.Substring(lastLinePos + 2, mimeContents.Length - lastLinePos - 4);
                                            linePos = -1;
                                        }
                                        else
                                            linePos += 2;
                                    }
                                }
                            }

                            mimeBlocks.Add(mimeContents);

                            // Divide the MIME part's headers into its components.
                            string mimeCharSet = "", mimeContentDisposition = "", mimeContentID = "", mimeContentType = "", mimeContentTransferEncoding = "", mimeFileName = "";
                            ExtractMimeHeaders(mimeHeaders, out mimeContentType, out mimeCharSet, out mimeContentTransferEncoding, out mimeContentDisposition, out mimeFileName, out mimeContentID);

                            string mimeContentTypeToUpper = mimeContentType.ToUpper();
                            if (mimeContentTypeToUpper.StartsWith("MULTIPART/"))
                            {
                                // Recurse through embedded MIME parts.
                                List<MimePart> returnedMIMEParts = ExtractMIMEParts(mimeContentType, mimeCharSet, mimeContentTransferEncoding, mimeBody, processingFlags);
                                foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                                    mimeParts.Add(returnedMIMEPart);
                            }
                            else
                            {
                                // Keep track of whether this MIME part's body has already been processed.
                                bool processed = false;

                                if (mimeContentTypeToUpper.StartsWith("APPLICATION/PKCS7-SIGNATURE") || mimeContentTypeToUpper.StartsWith("APPLICATION/X-PKCS7-SIGNATURE"))
                                {
                                    // Unless a flag has been set to include this *.p7s block, exclude it from attachments.
                                    if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeSmimeSignedData) == 0)
                                        processed = true;

                                    // Remember the signature block to use for later verification.
                                    signatureBlock = mimeBlocks.Count() - 1;
                                }
                                else if (mimeContentTypeToUpper.StartsWith("APPLICATION/PKCS7-MIME") || mimeContentTypeToUpper.StartsWith("APPLICATION/X-PKCS7-MIME"))
                                {
                                    // Unless a flag has been set to include this *.p7m block, exclude it from attachments.
                                    processed = (processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeSmimeEncryptedEnvelopeData) == 0;

                                    // Decrypt the MIME part and recurse through embedded MIME parts.
                                    List<MimePart> returnedMIMEParts = ReturnDecryptedMimeParts(mimeContentType, mimeContentTransferEncoding, mimeBody, processingFlags);
                                    if (returnedMIMEParts != null)
                                    {
                                        foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                                            mimeParts.Add(returnedMIMEPart);
                                    }
                                    else
                                    {
                                        // If we were unable to decrypt, return this MIME part as-is.
                                        processed = false;
                                    }
                                }
                                else if (mimeContentTypeToUpper.StartsWith("APPLICATION/MS-TNEF") || mimeFileName.ToUpper() == "WINMAIL.DAT")
                                {
                                    // Process the TNEF encoded message.
                                    processed = true;
                                    TnefEncoding tnef = new TnefEncoding(Convert.FromBase64String(mimeBody));

                                    // If we were unable to extract content from this MIME, include it as an attachment.
                                    if ((tnef.Body.Length < 1 && tnef.MimeAttachments.Count < 1) || (processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeWinMailData) > 0)
                                        processed = false;
                                    else
                                    {
                                        // Unless a flag has been set to include this winmail.dat block, exclude it from attachments.
                                        if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeWinMailData) > 0)
                                        {
                                            if (!string.IsNullOrEmpty(tnef.Body))
                                                mimeParts.Add(new MimePart("winmail.dat", tnef.ContentType, "", "", mimeContentTransferEncoding, Encoding.UTF8.GetBytes(tnef.Body)));
                                        }

                                        foreach (MimePart mimePart in tnef.MimeAttachments)
                                            mimeParts.Add(mimePart);
                                    }
                                }
                                else if (mimeContentTypeToUpper == "MESSAGE/RFC822")
                                {
                                    if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeNestedRFC822Messages) > 0)
                                    {
                                        // Recurse through the RFC822 container.
                                        processed = true;

                                        mimeDivider = mimeBody.IndexOf("\r\n\r\n");
                                        if (mimeDivider > -1)
                                        {
                                            mimeHeaders = Functions.UnfoldWhitespace(mimeBody.Substring(0, mimeDivider));
                                            mimeBody = mimeBody.Substring(mimeDivider + 4);

                                            mimeContentType = Functions.ReturnBetween(mimeHeaders, "Content-Type:", "\r\n").Trim();
                                            mimeContentTransferEncoding = Functions.ReturnBetween(mimeHeaders, "Content-Transfer-Encoding:", "\r\n").Trim();
                                            mimeCharSet = Functions.ExtractMimeParameter(mimeContentType, "charset").Replace("\"", "");

                                            List<MimePart> returnedMIMEParts = ExtractMIMEParts(mimeContentType, mimeCharSet, mimeContentTransferEncoding, mimeBody, processingFlags);
                                            foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                                                mimeParts.Add(returnedMIMEPart);
                                        }
                                    }
                                }

                                if (!processed)
                                {
                                    // Decode and add the message to the MIME parts collection.
                                    switch (mimeContentTransferEncoding.ToLower())
                                    {
                                        case "base64":
                                            mimeBody = mimeBody.Replace("\r\n", "");
                                            if (mimeBody.Length % 4 != 0)
                                                mimeBody += new String('=', 4 - (mimeBody.Length % 4));

                                            mimeParts.Add(new MimePart(mimeFileName, mimeContentType, mimeCharSet, mimeContentID, mimeContentTransferEncoding, Convert.FromBase64String(mimeBody)));
                                            break;
                                        case "quoted-printable":
                                            mimeParts.Add(new MimePart(mimeFileName, mimeContentType, mimeCharSet, mimeContentID, mimeContentTransferEncoding, Functions.FromQuotedPrintable(mimeBody, mimeCharSet, null)));
                                            break;
                                        case "binary":
                                        case "7bit":
                                        case "8bit":
                                        default:
                                            mimeParts.Add(new MimePart(mimeFileName, mimeContentType, mimeCharSet, mimeContentID, mimeContentTransferEncoding, mimeBody));
                                            break;
                                    }
                                }
                            }
                        }
                        cursor = mimeEndPosition;
                    }
                    else
                        cursor = -1;
                }

                // If a PKCS signature was found and there's one other MIME part, verify the signature.
                if (signatureBlock > -1 && mimeBlocks.Count == 2)
                {
                    // Verify the signature and track the signing certificates.
                    X509Certificate2Collection signingCertificates;
                    if (VerifySignature(mimeBlocks[signatureBlock], mimeBlocks[1 - signatureBlock], out signingCertificates))
                    {
                        // Stamp each MIME part found so far as signed, and if relevant, triple wrapped.
                        foreach (MimePart mimePart in mimeParts)
                        {
                            mimePart.SmimeSigningCertificates = signingCertificates;

                            if (mimePart.SmimeSigned && mimePart.SmimeEncryptedEnvelope)
                                mimePart.SmimeTripleWrapped = true;

                            mimePart.SmimeSigned = true;
                        }
                    }
                }
            }
            else if (contentTypeToUpper.StartsWith("APPLICATION/MS-TNEF"))
            {
                // Process the TNEF encoded message.
                TnefEncoding tnef = new TnefEncoding(Convert.FromBase64String(body));

                // Unless a flag has been set to include this winmail.dat block, exclude it from attachments.
                if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeWinMailData) > 0)
                {
                    if (!string.IsNullOrEmpty(tnef.Body))
                        mimeParts.Add(new MimePart("winmail.dat", tnef.ContentType, "", "", "", Encoding.UTF8.GetBytes(tnef.Body)));
                }

                foreach (MimePart mimePart in tnef.MimeAttachments)
                    mimeParts.Add(mimePart);
            }
            else if (contentTypeToUpper.StartsWith("APPLICATION/PKCS7-MIME") || contentTypeToUpper.StartsWith("APPLICATION/X-PKCS7-MIME"))
            {
                // Don't attempt to decrypt if this is a signed message only.
                if (contentType.IndexOf("smime-type=signed-data") < 0)
                {
                    // Unless a flag has been set to include this *.p7m block, exclude it from attachments.
                    if ((processingFlags & ReadOnlyMailMessageProcessingFlags.IncludeSmimeEncryptedEnvelopeData) > 0)
                        mimeParts.Add(new MimePart("smime.p7m", contentType, "", "", "", body));

                    // Decrypt the MIME part and recurse through embedded MIME parts.
                    List<MimePart> returnedMIMEParts = ReturnDecryptedMimeParts(contentType, contentTransferEncoding, body, processingFlags);
                    if (returnedMIMEParts != null)
                    {
                        foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                            mimeParts.Add(returnedMIMEPart);
                    }
                    else
                    {
                        // If we were unable to decrypt the message, pass it along as-is.
                        mimeParts.Add(new MimePart(Functions.ReturnBetween(contentType + ";", "name=", ";").Replace("\"", ""), contentType, "", "", contentTransferEncoding, body));
                    }
                }
                else
                {
                    // Hydrate the signature CMS object.
                    SignedCms signedCms = new SignedCms();

                    try
                    {
                        // Attempt to decode the signature block and verify the passed in signature.
                        signedCms.Decode(Convert.FromBase64String(body));
                        signedCms.CheckSignature(true);

                        string mimeContents = Encoding.UTF8.GetString(signedCms.ContentInfo.Content);

                        int mimeDivider = mimeContents.IndexOf("\r\n\r\n");
                        string mimeHeaders;
                        if (mimeDivider > -1)
                            mimeHeaders = mimeContents.Substring(0, mimeDivider);
                        else
                            mimeHeaders = mimeContents;

                        if (mimeHeaders.Length > 0)
                        {
                            // Extract the body portion of the current MIME part.
                            string mimeBody = mimeContents.Substring(mimeDivider + 4);

                            string mimeCharSet = "", mimeContentDisposition = "", mimeContentID = "", mimeContentType = "", mimeContentTransferEncoding = "", mimeFileName = "";
                            ExtractMimeHeaders(mimeHeaders, out mimeContentType, out mimeCharSet, out mimeContentTransferEncoding, out mimeContentDisposition, out mimeFileName, out mimeContentID);

                            List<MimePart> returnedMIMEParts = ExtractMIMEParts(mimeContentType, mimeCharSet, mimeContentTransferEncoding, mimeBody, processingFlags);
                            foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                                mimeParts.Add(returnedMIMEPart);
                        }
                    }
                    catch
                    {
                        // If an exception occured, the signature could not be verified.
                    }
                }
            }
            else if (contentTypeToUpper == "MESSAGE/RFC822")
            {
                int mimeDivider = body.IndexOf("\r\n\r\n");
                if (mimeDivider > -1)
                {
                    string mimeHeaders = Functions.UnfoldWhitespace(body.Substring(0, mimeDivider));
                    string mimeBody = body.Substring(mimeDivider + 4);

                    string mimeContentType = Functions.ReturnBetween(mimeHeaders, "Content-Type:", "\r\n").Trim();
                    string mimeContentTransferEncoding = Functions.ReturnBetween(mimeHeaders, "Content-Transfer-Encoding:", "\r\n").Trim();
                    string mimeCharSet = Functions.ExtractMimeParameter(mimeContentType, "charset");

                    List<MimePart> returnedMIMEParts = ExtractMIMEParts(mimeContentType, mimeCharSet, mimeContentTransferEncoding, mimeBody, processingFlags);
                    foreach (MimePart returnedMIMEPart in returnedMIMEParts)
                        mimeParts.Add(returnedMIMEPart);
                }
            }
            else
            {
                // Decode the message.
                switch (contentTransferEncoding.ToLower())
                {
                    case "base64":
                        body = Functions.FromBase64(body);
                        break;
                    case "quoted-printable":
                        body = Functions.FromQuotedPrintable(body, charSet, null);
                        break;
                    case "binary":
                    case "7bit":
                    case "8bit":
                        break;
                }

                // Extract the headers from this MIME part.
                string mimeHeaders;
                int mimeDivider = body.IndexOf("\r\n\r\n");
                if (mimeDivider > -1 )
                    mimeHeaders = body.Substring(0, mimeDivider);
                else
                    mimeHeaders = body;

                // Divide the MIME part's headers into its components.
                string mimeCharSet = "", mimeContentDisposition = "", mimeContentID = "", mimeContentType = "", mimeContentTransferEncoding = "", mimeFileName = "";
                ExtractMimeHeaders(mimeHeaders, out mimeContentType, out mimeCharSet, out mimeContentTransferEncoding, out mimeContentDisposition, out mimeFileName, out mimeContentID);

                // If this MIME part's content type is null, fall back to the overall content type.
                if ((string.IsNullOrEmpty(mimeContentType) && !string.IsNullOrEmpty(contentType)) || (contentTypeToUpper.StartsWith("MESSAGE/PARTIAL")))
                {
                    mimeCharSet = charSet;
                    mimeContentType = contentType;
                }
                else
                {
                    if (body.Length > (mimeDivider + 4))
                        body = body.Substring(mimeDivider + 4);
                    else
                        body = "";
                }

                // Add the message to the MIME parts collection.
                mimeParts.Add(new MimePart(mimeFileName, mimeContentType, mimeCharSet, mimeContentID, mimeContentTransferEncoding, body));
            }
            
            return mimeParts;
        }

        /// <summary>
        /// Decrypt the encrypted S/MIME envelope.
        /// </summary>
        /// <param name="contentType">Content Type of the outermost MIME part.</param>
        /// <param name="contentTransferEncoding">Encoding of the outermost MIME part.</param>
        /// <param name="envelopeText">The MIME envelope.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a ReadOnlyMailMessage.</param>
        public static List<MimePart> ReturnDecryptedMimeParts(string contentType, string contentTransferEncoding, string envelopeText, ReadOnlyMailMessageProcessingFlags processingFlags)
        {
            try
            {
                // Hydrate the envelope CMS object.
                EnvelopedCms envelope = new EnvelopedCms();

                // Attempt to decrypt the envelope.
                envelope.Decode(Convert.FromBase64String(envelopeText));
                envelope.Decrypt();

                string body = Encoding.UTF8.GetString(envelope.ContentInfo.Content);
                int divider = body.IndexOf("\r\n\r\n");
                string mimeHeaders = body.Substring(0, divider);
                body = body.Substring(divider + 4);

                // Divide the MIME part's headers into its components.
                string mimeContentType = "", mimeCharSet = "", mimeContentTransferEncoding = "", mimeFileName = "", mimeContentDisposition = "", mimeContentID = "";
                ExtractMimeHeaders(mimeHeaders, out mimeContentType, out mimeCharSet, out mimeContentTransferEncoding, out mimeContentDisposition, out mimeFileName, out mimeContentID);

                // Recurse through embedded MIME parts.
                List<MimePart> mimeParts = ExtractMIMEParts(mimeContentType, mimeCharSet, mimeContentTransferEncoding, body, processingFlags);
                foreach (MimePart mimePart in mimeParts)
                    mimePart.SmimeEncryptedEnvelope = true;

                return mimeParts;
            }
            catch (Exception)
            {
                // If unable to decrypt the body, return null.
                return null;
            }
        }

        /// <summary>
        /// Verify the S/MIME signature.
        /// </summary>
        /// <param name="signatureBlock">The S/MIME signature block.</param>
        /// <param name="body">The message's raw body.</param>
        /// <param name="signingCertificates">Collection of certificates to be used when signing.</param>
        public static bool VerifySignature(string signatureBlock, string body, out X509Certificate2Collection signingCertificates)
        {
            // Ignore MIME headers for the signature block.
            signatureBlock = signatureBlock.Substring(signatureBlock.IndexOf("\r\n\r\n") + 4);

            // Bypass any leading carriage returns and line feeds in the body.
            int bodyOffset = 0;
            while (body.Substring(bodyOffset).StartsWith("\r\n"))
                bodyOffset += 2;

            // Hydrate the signature CMS object.
            ContentInfo contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(body.Substring(bodyOffset)));
            SignedCms signedCms = new SignedCms(contentInfo, true);

            try
            {
                // Attempt to decode the signature block and verify the passed in signature.
                signedCms.Decode(Convert.FromBase64String(signatureBlock));
                signedCms.CheckSignature(true);
                signingCertificates = signedCms.Certificates;
                
                return true;
            }
            catch
            {
                // If an exception occured, the signature could not be verified.
                signingCertificates = null;
                return false;
            }
        }

        /// <summary>
        /// Divide a MIME part's headers into its components.
        /// </summary>
        /// <param name="mimeHeaders">The raw headers portion of the MIME part.</param>
        /// <param name="mimeContentType">Content Type of the MIME part.</param>
        /// <param name="mimeCharset">Character Set used to encode the MIME part.</param>
        /// <param name="mimeContentDisposition">Content disposition, such as file metadata, of the MIME part.</param>
        /// <param name="mimeFileName">Filename of the MIME part.</param>
        /// <param name="mimeContentID">ID of the MIME part.</param>
        private static void ExtractMimeHeaders(string mimeHeaders, out string mimeContentType, out string mimeCharSet, out string mimeContentTransferEncoding, out string mimeContentDisposition, out string mimeFileName, out string mimeContentID)
        {
            // Initialize all headers as blank strings.
            mimeContentType = mimeCharSet = mimeContentTransferEncoding = mimeContentDisposition = mimeFileName = mimeContentID = "";

            // Record keeping variable to handle headers that spawn multiple lines.
            string lastMimeHeaderType = "";

            // Unfold any unneeded whitespace, then Loop through each line of the headers.
            string[] mimeHeaderLines = Functions.UnfoldWhitespace(mimeHeaders).Replace("\r", "").Split('\n');
            foreach (string header in mimeHeaderLines)
            {
                // Split header {name:value} pairs by the first colon found.
                int colonPos = header.IndexOf(":");
                if (colonPos > -1 && colonPos < header.Length - 1)
                {
                    string[] headerParts = new string[] { header.Substring(0, colonPos), header.Substring(colonPos + 1).Trim() };
                    string headerType = headerParts[0].ToLower();
                    string headerValue = headerParts[1];

                    // Process each header's value based on its name.
                    switch (headerType)
                    {
                        case "content-disposition":
                            mimeContentDisposition += headerValue.Trim();
                            break;
                        case "content-id":
                            // Ignore opening and closing <> characters.
                            mimeContentID = headerValue.Trim();
                            if (mimeContentID.StartsWith("<"))
                                mimeContentID = mimeContentID.Substring(1);
                            if (mimeContentID.EndsWith(">"))
                                mimeContentID = mimeContentID.Substring(0, mimeContentID.Length - 1);
                            break;
                        case "content-transfer-encoding":
                            mimeContentTransferEncoding = headerValue.ToLower();
                            break;
                        case "content-type":
                            if (string.IsNullOrEmpty(mimeContentType))
                                mimeContentType = headerValue;
                            break;
                        default:
                            break;
                    }
                    lastMimeHeaderType = headerType;
                }
            }

            // If a content disposition has been specified, extract the filename.
            if (mimeContentDisposition.Length > 0)
                mimeFileName = Functions.ExtractMimeParameter(mimeContentDisposition, "name");

            // If a content disposition has not been specified, search elsewhere in the content type string for the filename.
            if (string.IsNullOrEmpty(mimeFileName))
                mimeFileName = Functions.ExtractMimeParameter(mimeContentType, "name");

            mimeCharSet = Functions.ExtractMimeParameter(mimeContentType, "charset");
        }
        #endregion Public Methods
    }
}
