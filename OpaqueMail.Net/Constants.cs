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
        /// Large buffer size for bit shuffling.
        /// </summary>
        public const int LARGEBUFFERSIZE = 16777216;

        /// <summary>
        /// Small buffer size for bit shuffling.
        /// </summary>
        public const int SMALLBUFFERSIZE = 65536;
    }
}
