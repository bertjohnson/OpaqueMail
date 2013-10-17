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
    /// Define constants used by other OpaqueMail classes.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Extra large buffer size for bit shuffling.
        /// </summary>
        public const int HUGEBUFFERSIZE = 16777216;

        /// <summary>
        /// Large buffer size for bit shuffling.
        /// </summary>
        public const int LARGEBUFFERSIZE = 1048576;

        /// <summary>
        /// Small buffer size for bit shuffling.
        /// </summary>
        public const int SMALLBUFFERSIZE = 65536;

        /// <summary>
        /// Tiny buffer size for bit shuffling.
        /// </summary>
        public const int TINYBUFFERSIZE = 2048;

        /// <summary>
        /// Tiny buffer size for string concatenation.
        /// </summary>
        public const int TINYSBSIZE = 256;

        /// <summary>
        /// Small buffer size for string concatenation.
        /// </summary>
        public const int SMALLSBSIZE = 2048;

        /// <summary>
        /// Medium buffer size for string concatenation.
        /// </summary>
        public const int MEDIUMSBSIZE = 32768;

        /// <summary>
        /// Large buffer size for string concatenation.
        /// </summary>
        public const int LARGESBSIZE = 1048576;
    }
}
