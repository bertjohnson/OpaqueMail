using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Represents an IMAP mailbox.
    /// </summary>
    public class Mailbox
    {
        /// <summary>Standard IMAP flags associated with this mailbox.</summary>
        public HashSet<string> Flags = new HashSet<string>();
        /// <summary>Mailbox hierarchy delimiting string.</summary>
        public string HierarchyDelimiter;
        /// <summary>True if ModSeq is explicitly unavailable.</summary>
        public bool NoModSeq = false;
        /// <summary>Permanent IMAP flags associated with this mailbox.</summary>
        public HashSet<string> PermanentFlags = new HashSet<string>();
        /// <summary>Name of the mailbox.</summary>
        public string Name;

        /// <summary>Number of messages in the mailbox.  -1 if COUNT was not parsed.</summary>
        public int Count = -1;
        /// <summary>Highest ModSeq in the mailbox.  -1 if HIGHESTMODSEQ was not parsed.</summary>
        public int HighestModSeq = -1;
        /// <summary>Number of recent messages in the mailbox.  -1 if RECENT was not parsed.</summary>
        public int Recent = -1;
        /// <summary>Expected next UID for the mailbox.  -1 if UIDNEXT was not parsed.</summary>
        public int UidNext = -1;
        /// <summary>UID validity for the mailbox.  -1 if UIDVALIDITY was not parsed.</summary>
        public int UidValidity = -1;

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mailbox() { }

        /// <summary>
        /// Parse IMAP output from an EXAMINE or SELECT command.
        /// </summary>
        /// <param name="name">Name of the mailbox.</param>
        /// <param name="imapResponse">Raw IMAP output of an EXAMINE or SELECT command.</param>
        public Mailbox(string name, string imapResponse)
        {
            Name = name;

            string[] responseLines = imapResponse.Replace("\r", "").Split('\n');
            foreach (string responseLine in responseLines)
            {
                if (responseLine.StartsWith("* FLAGS ("))
                {
                    string[] flags = responseLine.Substring(9, responseLine.Length - 10).Split(' ');
                    foreach (string flag in flags)
                    {
                        if (!Flags.Contains(flag))
                            Flags.Add(flag);
                    }
                }
                else if (responseLine.StartsWith("* OK [NOMODSEQ]"))
                    NoModSeq = true;
                else if (responseLine.StartsWith("* OK [HIGHESTMODSEQ "))
                {
                    string highestModSeq = responseLine.Substring(20, responseLine.IndexOf("]") - 20);
                    int.TryParse(highestModSeq, out HighestModSeq);
                }
                else if (responseLine.StartsWith("* OK [PERMANENTFLAGS ("))
                {
                    string[] permanentFlags = responseLine.Substring(22, responseLine.IndexOf("]") - 22).Split(' ');
                    foreach (string permanentFlag in permanentFlags)
                    {
                        if (!PermanentFlags.Contains(permanentFlag))
                            PermanentFlags.Add(permanentFlag);
                    }
                }
                else if (responseLine.StartsWith("* OK [UIDNEXT "))
                {
                    string uidNext = responseLine.Substring(14, responseLine.IndexOf("]") - 14);
                    int.TryParse(uidNext, out UidNext);
                }
                else if (responseLine.StartsWith("* OK [UIDVALIDITY "))
                {
                    string uidValidity = responseLine.Substring(18, responseLine.IndexOf("]") - 18);
                    int.TryParse(uidValidity, out UidValidity);
                }
                else if (responseLine.EndsWith(" EXISTS"))
                {
                    string existsCount = responseLine.Substring(2, responseLine.Length - 9);
                    int.TryParse(existsCount, out Count);
                }
                else if (responseLine.EndsWith(" RECENT"))
                {
                    string recentCount = responseLine.Substring(2, responseLine.Length - 9);
                    int.TryParse(recentCount, out Recent);
                }
            }
        }

        /// <summary>
        /// Parse IMAP output from a LIST, LSUB, or XLIST command.
        /// </summary>
        /// <param name="lineFromListCommand">Raw output line from a LIST, LSUB, or XLIST command.</param>
        /// <returns></returns>
        public static Mailbox CreateFromList(string lineFromListCommand)
        {
            // Ensure the list of flags is contained on this line.
            int startsFlagList = lineFromListCommand.IndexOf("(");
            int endFlagList = lineFromListCommand.IndexOf(")", startsFlagList + 1);
            if (startsFlagList > -1 && endFlagList > -1)
            {
                Mailbox mailbox = new Mailbox();

                string[] flags = lineFromListCommand.Substring(startsFlagList + 1, endFlagList - startsFlagList - 1).Split(' ');
                foreach (string flag in flags)
                {
                    if (!mailbox.Flags.Contains(flag))
                        mailbox.Flags.Add(flag);
                }

                // Ensure the hierarchy delimiter and name are returned.
                string[] remainingParts = lineFromListCommand.Substring(endFlagList + 2).Split(new char[] { ' ' }, 2);
                if (remainingParts.Length == 2)
                {
                    mailbox.HierarchyDelimiter = remainingParts[0].Replace("\"", "");
                    mailbox.Name = remainingParts[1].Replace("\"", "");

                    return mailbox;
                }
            }

            // No valid mailbox listing found, so return null.
            return null;
        }
        #endregion Constructors
    }
}
