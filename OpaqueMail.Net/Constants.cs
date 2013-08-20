using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
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
