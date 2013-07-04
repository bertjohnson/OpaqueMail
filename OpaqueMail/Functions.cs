using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Provides common functions used by other OpaqueMail classes.
    /// </summary>
    public class Functions
    {
        #region Public Functions
        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in inArray.</param>
        /// <param name="length">The number of elements of inArray to convert.</param>
        public static string ToBase64String(ref byte[] inArray, int offset, int length)
        {
            // Span lines over 100 characters by default.
            return ToBase64String(ref inArray, offset, length, 100);
        }

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in inArray.</param>
        /// <param name="length">The number of elements of inArray to convert.</param>
        /// <param name="length">The number of base-64 digits per line.</param>
        public static string ToBase64String(ref byte[] inArray, int offset, int length, int lineLength)
        {
            StringBuilder base64Builder = new StringBuilder();
            string base64Value = Convert.ToBase64String(inArray, 0, length);

            // Loop through every lineLength # of characters, adding a new line to our StringBuilder.
            int position = 0;
            int chunkSize = lineLength;
            while (position < base64Value.Length)
            {
                if (base64Value.Length - (position + chunkSize) < 0)
                    chunkSize = base64Value.Length - position;
                base64Builder.Append(base64Value.Substring(position, chunkSize));
                base64Builder.Append("\r\n");
                position += chunkSize;
            }

            return base64Builder.ToString();
        }
        #endregion Public Functions

        /// <summary>
        /// Encodes a message as a 7-bit string.
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
        /// <param name="lineLength">The number of base-64 digits per line.</param>
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
                sevenBitBuilder.Append(message.Substring(position, chunkSize));
                sevenBitBuilder.Append("\r\n");
                position += chunkSize;
            }

            return sevenBitBuilder.ToString();
        }
    }
}
