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
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace OpaqueMail
{
    /// <summary>Represents the address of an electronic mail sender or recipient.</summary>
    public class MailAddress
    {
        #region Public Members
        /// <summary>Gets the email address specified when this instance was created.</summary>
        public string Address
        {
            get
            {
                return mailAddress;
            }
            set
            {
                Parse(value);
            }
        }
        /// <summary>Display name of the mail address</summary>
        public string DisplayName
        {
            get
            {
                return mailDisplayName;
            }
            set
            {
                mailDisplayName = value;
            }
        }
        /// <summary>Host portion of the address specified.</summary>
        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
                mailAddress = userName + "@" + value;
            }
        }
        /// <summary>Whether the specified email address validates.</summary>
        public bool IsValid
        {
            get
            {
                return IsValidAddress(mailAddress);
            }
        }
        /// <summary>User information from the address specified.</summary>
        public string User
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                mailAddress = value + "@" + host;
            }
        }
        #endregion Public Members

        #region Private Members
        /// <summary>Raw email address.</summary>
        private string mailAddress;
        /// <summary>Display name of the mail address.</summary>
        private string mailDisplayName;
        /// <summary>Host portion of the address specified.</summary>
        private string host;
        /// <summary>User information from the address specified.</summary>
        private string userName;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes an empty instance of the MailAddress class.
        /// </summary>
        public MailAddress()
        {
            userName = "";
            host = "";
        }

        /// <summary>
        /// Initializes a new instance of the MailAddress class using the specified address.
        /// </summary>
        /// <param name="address">A string that contains an email address.</param>
        public MailAddress(string address)
        {
            mailAddress = address;

            // Split the address into its components.
            int atPos = address.IndexOf('@');
            if (atPos > -1)
            {
                userName = address.Substring(0, atPos);
                host = address.Substring(atPos + 1);
            }
            else
            {
                userName = address;
                host = "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the MailAddress class using the specified address and display name.
        /// </summary>
        /// <param name="address">A string that contains an email address.</param>
        /// <param name="displayName">A string that contains the display name associated with the address.</param>
        public MailAddress(string address, string displayName)
        {
            mailAddress = address;
            mailDisplayName = displayName;

            // Split the address into its components.
            int atPos = address.IndexOf('@');
            if (atPos > -1)
            {
                userName = address.Substring(0, atPos);
                host = address.Substring(atPos + 1);
            }
            else
            {
                userName = address;
                host = "";
            }
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Check if the specified email address validates.
        /// </summary>
        /// <param name="address">Address to validate.</param>
        /// <returns>True if the email address provided passes validation.</returns>
        public static bool IsValidAddress(string address)
        {
            return Regex.IsMatch(address, @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        /// <summary>Parse a text representation of email addresses into a collection of MailAddress objects.</summary>
        /// <param name="addresses">String representation of email addresses to parse.</param>
        public static MailAddress Parse(string address)
        {
            MailAddressCollection addressCollection = MailAddressCollection.Parse(address);
            if (addressCollection.Count > 0)
            {
                MailAddress mailAddress = addressCollection[0];
                return mailAddress;
            }
            else
                return null;
        }

        /// <summary>
        /// Provides a string representation of the email address.
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.DisplayName))
                return Functions.EncodeMailHeader(this.DisplayName) + " <" + this.Address + ">";
            else
                return this.Address;
        }
        #endregion Public Methods
    }

    /// <summary>Store email addresses that are associated with an email message.</summary>
    public class MailAddressCollection : Collection<MailAddress>
    {
        #region Public Methods
        public static MailAddressCollection Parse(string addresses)
        {
            // Escape embedded encoding.
            addresses = Functions.DecodeMailHeader(addresses);

            // Create a new collection of MailAddresses to be returned.
            MailAddressCollection addressCollection = new MailAddressCollection();

            int cursor = 0, lastCursor = 0;
            string displayName = "";
            while (cursor < addresses.Length)
            {
                int quoteCursor = addresses.IndexOf("\"", cursor, StringComparison.Ordinal);
                if (quoteCursor == -1)
                    quoteCursor = addresses.Length + 1;
                int aposCursor = addresses.IndexOf("'", cursor, StringComparison.Ordinal);
                if (aposCursor == -1)
                    aposCursor = addresses.Length + 1;
                int angleCursor = addresses.IndexOf("<", cursor, StringComparison.Ordinal);
                if (angleCursor == -1)
                    angleCursor = addresses.Length + 1;
                int bracketCursor = addresses.IndexOf("[", cursor, StringComparison.Ordinal);
                if (bracketCursor == -1)
                    bracketCursor = addresses.Length + 1;
                int parenthesisCursor = addresses.IndexOf("(", cursor, StringComparison.Ordinal);
                if (parenthesisCursor == -1)
                    parenthesisCursor = addresses.Length + 1;
                int commaCursor = addresses.IndexOf(",", cursor, StringComparison.Ordinal);
                if (commaCursor == -1)
                    commaCursor = addresses.Length + 1;
                int semicolonCursor = addresses.IndexOf(";", cursor, StringComparison.Ordinal);
                if (semicolonCursor == -1)
                    semicolonCursor = addresses.Length + 1;

                bool processed = false;
                if (quoteCursor < aposCursor && quoteCursor < angleCursor && quoteCursor < bracketCursor && quoteCursor < parenthesisCursor && quoteCursor < commaCursor && quoteCursor < semicolonCursor)
                {
                    // The address display name is enclosed in quotes.
                    int endQuoteCursor = addresses.IndexOf("\"", quoteCursor + 1, StringComparison.Ordinal);
                    if (endQuoteCursor > -1)
                    {
                        displayName = addresses.Substring(quoteCursor + 1, endQuoteCursor - quoteCursor - 1).Replace("\\(", "(").Replace("\\)", ")");
                        cursor = endQuoteCursor + 1;
                    }
                    else
                        cursor = addresses.Length;

                    processed = true;
                }
                else if (aposCursor < angleCursor && aposCursor < bracketCursor && aposCursor < parenthesisCursor && aposCursor < commaCursor && aposCursor < semicolonCursor)
                {
                    // The address display name may be enclosed in apostrophes.
                    int endAposCursor = addresses.IndexOf("'", aposCursor + 1, StringComparison.Ordinal);
                    if (endAposCursor > -1)
                    {
                        displayName = addresses.Substring(aposCursor + 1, endAposCursor - aposCursor - 1).Replace("\\(", "(").Replace("\\)", ")");
                        cursor = endAposCursor + 1;
                    }
                    else
                    {
                        // The address contains an apostophe, but it's not enclosed in apostrophes.
                        angleCursor = addresses.IndexOf("<", cursor, StringComparison.Ordinal);
                        if (angleCursor == -1)
                            angleCursor = addresses.Length + 1;
                        bracketCursor = addresses.IndexOf("[", cursor, StringComparison.Ordinal);
                        if (bracketCursor == -1)
                            bracketCursor = addresses.Length + 1;

                        if (angleCursor < bracketCursor)
                        {
                            displayName = addresses.Substring(lastCursor, angleCursor - lastCursor).Trim().Replace("\\(", "(").Replace("\\)", ")");
                            cursor = angleCursor;
                        }
                        else if (bracketCursor > -1)
                        {
                            displayName = addresses.Substring(lastCursor, bracketCursor - lastCursor).Trim().Replace("\\(", "(").Replace("\\)", ")");
                            cursor = angleCursor;
                        }
                        else
                            cursor = addresses.Length;
                    }

                    processed = true;
                }
                else if (angleCursor < bracketCursor && angleCursor < parenthesisCursor && angleCursor < commaCursor && angleCursor < semicolonCursor)
                {
                    // The address is enclosed in angle brackets.
                    int endAngleCursor = addresses.IndexOf(">", angleCursor + 1, StringComparison.Ordinal);
                    if (endAngleCursor > -1)
                    {
                        // If we didn't find a display name between quotes or apostrophes, look at all characters prior to the angle bracket.
                        if (displayName.Length < 1)
                            displayName = addresses.Substring(lastCursor, angleCursor - lastCursor).Trim().Replace("\\(", "(").Replace("\\)", ")");

                        string address = addresses.Substring(angleCursor + 1, endAngleCursor - angleCursor - 1);
                        addressCollection.Add(new MailAddress(address, displayName));

                        displayName = "";
                        cursor = endAngleCursor + 1;
                    }
                    else
                        cursor = addresses.Length;

                    processed = true;
                }
                else if (bracketCursor < parenthesisCursor && bracketCursor < commaCursor && bracketCursor < semicolonCursor)
                {
                    // The address is enclosed in brackets.
                    int endBracketCursor = addresses.IndexOf("]", bracketCursor + 1, StringComparison.Ordinal);
                    if (endBracketCursor > -1)
                    {
                        // If we didn't find a display name between quotes or apostrophes, look at all characters prior to the bracket.
                        if (displayName.Length < 1)
                            displayName = addresses.Substring(lastCursor, bracketCursor - lastCursor).Trim().Replace("\\(", "(").Replace("\\)", ")");

                        string address = addresses.Substring(bracketCursor + 1, endBracketCursor - bracketCursor - 1);
                        if (displayName.Length > 0)
                            addressCollection.Add(new MailAddress(address, displayName));
                        else
                            addressCollection.Add(new MailAddress(address));

                        displayName = "";
                        cursor = endBracketCursor + 1;

                        processed = true;
                    }
                    else
                        cursor = addresses.Length;
                }
                else if (parenthesisCursor < commaCursor && parenthesisCursor < semicolonCursor)
                {
                    if ((parenthesisCursor == 0) || (addresses[parenthesisCursor - 1] != '\\'))
                    {
                        // The display name is enclosed in parentheses.
                        int endParenthesisCursor = 0;
                        while (endParenthesisCursor > -1)
                        {
                            endParenthesisCursor = addresses.IndexOf(")", parenthesisCursor + 1, StringComparison.Ordinal);
                            if (endParenthesisCursor > 0)
                            {
                                if (addresses[endParenthesisCursor - 1] != '\\')
                                    break;
                            }
                        }

                        string address = addresses.Substring(lastCursor, parenthesisCursor - lastCursor).Trim();
                        displayName = addresses.Substring(parenthesisCursor + 1, endParenthesisCursor - parenthesisCursor - 1).Replace("\\(", "(").Replace("\\)", ")");
                        addressCollection.Add(new MailAddress(address, displayName));

                        cursor = parenthesisCursor + 1;

                        processed = true;
                    }
                }

                if (!processed)
                {
                    if (commaCursor < semicolonCursor)
                    {
                        if (commaCursor > lastCursor)
                        {
                            // We've found the next address, delimited by a comma.
                            string address = addresses.Substring(cursor, commaCursor - cursor).Trim();
                            addressCollection.Add(new MailAddress(address));
                        }

                        cursor = commaCursor + 1;
                    }
                    else if (semicolonCursor < addresses.Length)
                    {
                        if (semicolonCursor > lastCursor)
                        {
                            // We've found the next address, delimited by a semicolon.
                            string address = addresses.Substring(cursor, semicolonCursor - cursor).Trim();
                            addressCollection.Add(new MailAddress(address));
                        }

                        cursor = semicolonCursor + 1;
                    }
                    else
                    {
                        // Process any remaining address.
                        string address = addresses.Substring(cursor).Trim();
                        addressCollection.Add(new MailAddress(address));

                        cursor = addresses.Length;
                    }
                }

                lastCursor = cursor;
            }

            // If no encoded email address was parsed, try adding the entire string.
            if (addressCollection.Count < 1)
            {
                addressCollection.Add(addresses);
            }

            return addressCollection;
        }

        /// <summary>
        /// Provides a string representation of one or more email addresses.
        /// </summary>
        public override string ToString()
        {
            StringBuilder addressString = new StringBuilder(Constants.TINYSBSIZE);

            foreach (MailAddress address in this)
            {
                if (!string.IsNullOrEmpty(address.DisplayName))
                    addressString.Append(address.DisplayName + " <" + address.Address + ">, ");
                else
                    addressString.Append(address.Address + ", ");
            }
            if (addressString.Length > 1)
                addressString.Remove(addressString.Length - 2, 2);

            return addressString.ToString();
        }

        /// <summary>Add a list of email addresses to the collection.</summary>
        /// <param name="addresses">The email addresses to add to the <see cref="T:System.Net.Mail.MailAddressCollection" />. Multiple email addresses must be separated with a comma character (","). </param>
        public void Add(string addresses)
        {
        }
        #endregion Public Methods

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The index of the email address element to be replaced.</param>
        /// <param name="item">An email address that will replace the element in the collection.</param>
        /// <exception cref="T:System.ArgumentNullException">The<paramref name=" item" /> parameter is null.</exception>
        protected override void SetItem(int index, MailAddress item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
        /// <summary>Inserts an email address into the <see cref="T:System.Net.Mail.MailAddressCollection" />, at the specified location.</summary>
        /// <param name="index">The location at which to insert the email address that is specified by <paramref name="item" />.</param>
        /// <param name="item">The email address to be inserted into the collection.</param>
        /// <exception cref="T:System.ArgumentNullException">The<paramref name=" item" /> parameter is null.</exception>
        protected override void InsertItem(int index, MailAddress item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }
    }
}
