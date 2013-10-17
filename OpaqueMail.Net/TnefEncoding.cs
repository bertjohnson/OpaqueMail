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
    /// Represents an e-mail encoded using Microsoft's TNEF format.
    /// </summary>
    public class TnefEncoding
    {
        #region Public Members
        /// <summary>Raw body of the TNEF-encoded message.</summary>
        public string Body;
        /// <summary>Calculated content type of the TNEF-encoded message.</summary>
        public string ContentType;
        /// <summary>Collection of files attached to the TNEF-encoded message.</summary>
        public List<MimePart> MimeAttachments = new List<MimePart>();
        #endregion Public Members

        #region Private Members
        /// <summary>Position of internal buffer cursor.</summary>
        private int Cursor = 0;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Process a TNEF-encoded message and return an object with the message's body, content type, and attachments.
        /// </summary>
        public TnefEncoding(byte[] TnefEncodedBytes)
        {
            // Confirm the TNEF signature.
            uint signature = ReadUint32(TnefEncodedBytes);
            if (signature == 0x223E9F78)
            {
                uint referenceKey = ReadUint16(TnefEncodedBytes);

                // Variables to track the last attachment properties discovered.
                string currentAttachmentFileName = "";
                byte[] currentAttachmentBody = null;

                // Loop through each TNEF-encoded component.
                while (Cursor < TnefEncodedBytes.Length)
                {
                    byte headerComponent = TnefEncodedBytes[Cursor++];
                    uint headerType = ReadUint16(TnefEncodedBytes) & 0x0000FFFF;
                    uint headerId = ReadUint16(TnefEncodedBytes);
                    uint headerSize = ReadUint32(TnefEncodedBytes);

                    if (headerSize > 0)
                    {
                        // Load the data of this TNEF component into its own array.
                        byte[] data = new byte[headerSize];
                        Buffer.BlockCopy(TnefEncodedBytes, Cursor, data, 0, (int)headerSize);
                        Cursor += (int)headerSize;

                        // Require valid checksums.
                        uint readChecksum = ReadUint16(TnefEncodedBytes);
                        uint calculatedChecksum = CalculateChecksum(data);
                        if (readChecksum == calculatedChecksum)
                        {
                            // Process only the TNEF components related to the body and attachments.
                            switch (headerType)
                            {
                                case 0x0000:    // Owner
                                case 0x0001:    // Sent For
                                case 0x0002:    // Delegate
                                case 0x0006:    // Date Start
                                case 0x0007:    // Date End
                                case 0x0008:    // Owner Appt ID
                                case 0x0009:    // Response Requested
                                case 0x8000:    // From
                                case 0x8004:    // Subject
                                case 0x8005:    // Date Sent
                                case 0x8006:    // Date Received
                                case 0x8007:    // Flags
                                case 0x8008:    // Message Class
                                case 0x8009:    // Message ID
                                case 0x800A:    // Parent ID
                                case 0x800B:    // Conversation ID
                                case 0x800C:    // Body
                                case 0x800D:    // Priority
                                    break;
                                case 0x800F:    // Attachment Data
                                    // Check if we've already found this attachment's filename, and if so, record it.
                                    if (!string.IsNullOrEmpty(currentAttachmentFileName))
                                    {
                                        MimeAttachments.Add(new MimePart(currentAttachmentFileName, Functions.GetDefaultContentTypeForExtension(currentAttachmentFileName), "", "", "", data));
                                        currentAttachmentFileName = "";
                                    }
                                    else
                                        currentAttachmentBody = data;
                                    break;
                                case 0x8010:    // Attachment Title
                                    // Check if we've already found this attachment's contents, and if so, record it.
                                    if (currentAttachmentBody != null)
                                    {
                                        MimeAttachments.Add(new MimePart(ParseNullTerminatedString(Encoding.UTF8.GetString(data)), Functions.GetDefaultContentTypeForExtension(ParseNullTerminatedString(Encoding.UTF8.GetString(data))), "", "", "", currentAttachmentBody));
                                        currentAttachmentBody = null;
                                    }
                                    else
                                        currentAttachmentFileName = ParseNullTerminatedString(Encoding.UTF8.GetString(data));
                                    break;
                                case 0x8011:    // Attachment Rendering
                                case 0x8012:    // Attachment Creation Date
                                case 0x8013:    // Attachment Modified Date
                                case 0x8020:    // Modified Date
                                case 0x9001:    // Attachment Transport Name
                                case 0x9002:    // Attachment Rendering Data
                                case 0x9003:    // MAPI Properties
                                    if (data.Length > 16)
                                    {
                                        // Attempt to unpack the MAPI properties.
                                        MapiProperties props = new MapiProperties(data);
                                        if (!string.IsNullOrEmpty(props.Body))
                                        {
                                            Body = props.Body;
                                            ContentType = props.ContentType;
                                        }
                                    }

                                    break;
                                case 0x9004:    // Recipients Table
                                case 0x9005:    // Attachment
                                case 0x9006:    // TNEF Version
                                case 0x9007:    // OEM Code Page
                                case 0x9008:    // Original Message Class
                                    break;
                            }
                        }
                    }
                }
            }
        }
        #endregion Constructors

        #region Private Methods
        /// <summary>
        /// Calculate a checksum to validate TNEF headers.
        /// </summary>
        /// <param name="inputBytes">Bytes over which the checksum should be calculated.</param>
        private uint CalculateChecksum(byte[] inputBytes)
        {
            ushort checksum = 0;
            for (int i = 0; i < inputBytes.Length; i++)
                checksum += inputBytes[i];

            return checksum;
        }

        /// <summary>
        /// Convert a C-style null-terminated string to a .NET System.String object.
        /// </summary>
        /// <param name="input">String to convert.</param>
        private string ParseNullTerminatedString(string input)
        {
            int nullTerminator = input.IndexOf('\0');
            if (nullTerminator > -1)
                return input.Substring(0, nullTerminator);
            else
                return input;
        }

        /// <summary>
        /// Return the next WORD in the TNEF-encoded message.
        /// </summary>
        /// <param name="TNEFEncodedBytes">Reference to the original TNEF-encoded message array.</param>
        private uint ReadUint16(byte[] TNEFEncodedBytes)
        {
            uint value = BitConverter.ToUInt16(TNEFEncodedBytes, Cursor);
            Cursor += 2;
            return value;
        }

        /// <summary>
        /// Return the next DWORD in the TNEF-encoded message.
        /// </summary>
        /// <param name="TNEFEncodedBytes">Reference to the original TNEF-encoded message array.</param>
        private uint ReadUint32(byte[] TNEFEncodedBytes)
        {
            uint value = BitConverter.ToUInt32(TNEFEncodedBytes, Cursor);
            Cursor += 4;
            return value;
        }
    }
    #endregion Private Methods

    /// <summary>
    /// Represents a subset of MAPI properties to support reading TNEF-encoded messages.
    /// </summary>
    public class MapiProperties
    {
        #region Public Members
        /// <summary>Raw body of the TNEF-encoded message.</summary>
        public string Body;
        /// <summary>Calculated content type of the TNEF-encoded message.</summary>
        public string ContentType;
        #endregion Private Members

        #region Private Members
        /// <summary>Position of internal buffer cursor.</summary>
        private int Cursor = 0;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Process relevant contents of a MAPI properties block and return an object with the message's body and content typ.
        /// </summary>
        public MapiProperties(byte[] MapiPropertiesBytes)
        {
            // Number of internal MAPI properties.
            uint propertyCount = ReadUint32(MapiPropertiesBytes);

            // Ignore malformed messages.
            if (propertyCount > 1000)
                return;

            // Loop through all MAPI properties.
            for (int i = 0; i < propertyCount; i++)
            {
                uint propertyType = ReadUint16(MapiPropertiesBytes);
                uint propertyID = ReadUint16(MapiPropertiesBytes);

                // Process and remove the multi-value flag.
                bool isPropertyMultiValue = (propertyType & 0x1000) > 0;
                propertyType = (uint)(propertyType & ~0x1000);

                // Handle explicitly variable-length value types.
                switch (propertyType)
                {
                    case 0x000d:    // Object
                    case 0x001e:    // String
                    case 0x001f:    // Unicode String
                    case 0x0102:    // Binary
                        isPropertyMultiValue = true;
                        break;
                }

                // Handle named properties.
                if (propertyID >= 0x8000 && propertyID <= 0xFFFE)
                {
                    Guid propertyGuid = ReadGuid(MapiPropertiesBytes);
                    uint propertyLength = ReadUint32(MapiPropertiesBytes);

                    if (propertyLength == 1)
                    {
                        // Handle strings.
                        uint propertyNameLength = ReadUint32(MapiPropertiesBytes);
                        byte[] propertyNameBytes = new byte[propertyNameLength];

                        Buffer.BlockCopy(MapiPropertiesBytes, (int)Cursor, propertyNameBytes, 0, (int)propertyNameLength);
                        Cursor += (int)propertyNameLength;

                        // Deal with non-uniform values by proceeding to the next DWORD.
                        if (propertyNameLength % 4 > 0)
                            Cursor += 4 - (int)(propertyNameLength % 4);
                    }
                    else if (propertyLength == 0)
                        propertyLength = ReadUint32(MapiPropertiesBytes);
                }

                // If there are multiple properties, read how many.
                uint propertyValueCount = 1;
                if (isPropertyMultiValue)
                    propertyValueCount = ReadUint32(MapiPropertiesBytes);

                // Process only MAPI property types we care about.
                for (int j = 0; j < propertyValueCount; j++)
                {
                    switch (propertyType)
                    {
                        case 0X0000:    // Unspecified
                        case 0X0001:    // Null
                            break;
                        case 0X0002:    // Short
                        case 0X0003:    // Int
                        case 0X0004:    // Float
                        case 0X000a:    // Error
                        case 0X000b:    // Boolean
                            Cursor += 4;
                            break;
                        case 0X0005:    // Double
                        case 0X0006:    // Currency
                        case 0X0007:    // Application Time
                        case 0X0014:    // Int64
                        case 0X0040:    // System Time
                            Cursor += 8;
                            break;
                        case 0X0048:    // CLSID
                            Cursor += 16;
                            break;
                        case 0x000d:    // Object
                        case 0x001e:    // String
                        case 0x001f:    // Unicode String
                        case 0x0102:    // Binary
                            // Read the length of the current value.
                            int propertyValueLength = (int)ReadUint32(MapiPropertiesBytes);
                            byte[] propertyValue;

                            // Process only MAPI properties with IDs we care about.
                            int lastCursor = Cursor;
                            switch (propertyID)
                            {
                                case 0x1000:    // Body
                                    if (string.IsNullOrEmpty(Body))
                                    {
                                        // Read the raw body.
                                        propertyValue = new byte[propertyValueLength];
                                        Buffer.BlockCopy(MapiPropertiesBytes, Cursor, propertyValue, 0, propertyValueLength);

                                        Body = Encoding.UTF8.GetString(propertyValue);
                                        ContentType = "text/plain";
                                    }
                                    break;
                                case 0x1009:    // RTF Compressed
                                    // Read the RTF compressed body.
                                    Body = DecompressRTF(MapiPropertiesBytes);
                                    ContentType = "text/rtf";
                                    break;
                                case 0x1013:    // Body HTML
                                    // Read the HTML body.
                                    propertyValue = new byte[propertyValueLength];
                                    Buffer.BlockCopy(MapiPropertiesBytes, Cursor, propertyValue, 0, propertyValueLength);

                                    Body = Encoding.UTF8.GetString(propertyValue);
                                    ContentType = "text/html";
                                    break;
                            }

                            Cursor = lastCursor + propertyValueLength;
                            // Deal with non-uniform values by proceeding to the next DWORD.
                            if (propertyValueLength % 4 > 0)
                                Cursor += 4 - (propertyValueLength % 4);

                            break;
                    }
                }
            }
        }
        #endregion Constructors

        #region Private Methods
        /// <summary>
        /// Process compressed RTF properties and return the underlying string.
        /// </summary>
        /// <param name="MAPIPropertiesBytes">Reference to the original MAPI properties array.</param>
        private string DecompressRTF(byte[] MAPIPropertiesBytes)
        {
            // Read the compressed string's compressed size, decompressed size, signature, and CRC checksum.
            uint compressedSize = ReadUint32(MAPIPropertiesBytes);
            uint decompressedSize = ReadUint32(MAPIPropertiesBytes);
            uint magicFlags = ReadUint32(MAPIPropertiesBytes);
            uint crc = ReadUint32(MAPIPropertiesBytes);

            // Ensure the message is compressed by checking its signature.
            if (magicFlags == 0x414c454d)
            {
                // The string isn't compressed, so return the raw bytes.
                return Encoding.UTF8.GetString(MAPIPropertiesBytes, Cursor, (int)decompressedSize);
            }
            else if (magicFlags == 0x75465a4c)
            {
                // Note: we ignore the CRC.

                // Prepopulate a buffer with RTF tags to speed up decryption.
                string rtfPreBufferString = "{\\rtf1\\ansi\\mac\\deff0\\deftab720{\\fonttbl;}{\\f0\\fnil \\froman \\fswiss \\fmodern \\fscript \\fdecor MS Sans SerifSymbolArialTimes New RomanCourier{\\colortbl\\red0\\green0\\blue0\r\n\\par \\pard\\plain\\f0\\fs20\\b\\i\\u\\tab\\tx";
                byte[] rtfPreBuffer = Encoding.UTF8.GetBytes(rtfPreBufferString);

                // Populate the initial output buffer the RTF tags from above.
                int outputCursor = rtfPreBuffer.Length;
                byte[] outputBuffer = new byte[outputCursor + decompressedSize];
                Buffer.BlockCopy(rtfPreBuffer, 0, outputBuffer, 0, rtfPreBuffer.Length);

                // Keep track of decompression flags.
                int flags = 0, flagCount = 0;

                // Loop through all compressed bytes according to Microsoft's compressed RTF algorithm.
                bool processing = true;
                while (processing)
                {
                    if (flagCount++ % 8 == 0)
                        flags = MAPIPropertiesBytes[Cursor++];
                    else
                        flags = flags >> 1;

                    if ((flags & 1) > 0)
                    {
                        int offset = MAPIPropertiesBytes[Cursor++] & 0xFF;
                        int length = MAPIPropertiesBytes[Cursor++] & 0xFF;
                        offset = (offset << 4) | (length >> 4);
                        length = (length & 0xF) + 2;

                        offset = ((int)(outputCursor & 0xFFFFF000)) | offset;

                        if (offset >= outputCursor)
                        {
                            if (offset == outputCursor)
                            {
                                processing = false;
                                continue;
                            }

                            offset -= 4096;
                        }

                        int end = offset + length;
                        while (offset < end)
                            outputBuffer[outputCursor++] = outputBuffer[offset++];
                    }
                    else
                        outputBuffer[outputCursor++] = MAPIPropertiesBytes[Cursor++];
                }

                // Return the successfully-decompressed string.
                return Encoding.UTF8.GetString(outputBuffer, rtfPreBuffer.Length, (int)decompressedSize);
            }

            return "";
        }

        /// <summary>
        /// Return the next WORD in the MAPI properties.
        /// </summary>
        /// <param name="MAPIPropertiesBytes">Reference to the original MAPI properties array.</param>
        private uint ReadUint16(byte[] MAPIPropertiesBytes)
        {
            uint value = BitConverter.ToUInt16(MAPIPropertiesBytes, Cursor);
            Cursor += 2;
            return value;
        }

        /// <summary>
        /// Return the next DWORD in the MAPI properties.
        /// </summary>
        /// <param name="MAPIPropertiesBytes">Reference to the original MAPI properties array.</param>
        private uint ReadUint32(byte[] MAPIPropertiesBytes)
        {
            uint value = BitConverter.ToUInt32(MAPIPropertiesBytes, Cursor);
            Cursor += 4;
            return value;
        }

        /// <summary>
        /// Return the next 16 bytes in the MAPI properties as a Guid.
        /// </summary>
        /// <param name="MAPIPropertiesBytes">Reference to the original MAPI properties array.</param>
        private Guid ReadGuid(byte[] MAPIPropertiesBytes)
        {
            byte[] guidBytes = new byte[16];
            Buffer.BlockCopy(MAPIPropertiesBytes, Cursor, guidBytes, 0, 16);
            Cursor += 16;

            return new Guid(guidBytes);
        }
        #endregion Private Methods
    }
}
