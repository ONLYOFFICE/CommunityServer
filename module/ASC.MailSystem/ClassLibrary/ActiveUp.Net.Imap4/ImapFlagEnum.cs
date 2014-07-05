using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Imap4
{
    [Flags]
    public enum ImapFlagEnum
    {
        Seen = 1,
        Answered = 2,
        Flagged = 4,
        Deleted = 8,
        Draft = 16,
        Recent = 32
    }
}
