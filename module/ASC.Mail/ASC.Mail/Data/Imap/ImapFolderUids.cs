/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System.Collections.Generic;
using System.Linq;

namespace ASC.Mail.Data.Imap
{
    public class ImapFolderUids
    {
        public int BeginDateUid { get; set; }
        public uint? UidValidity { get; set; }
        public List<int> UnhandledUidIntervals { get; set; }

        public ImapFolderUids(IEnumerable<int> uidIntervals, int beginDateUid, uint? uidValidity = null)
        {
            if (uidIntervals == null)
                uidIntervals = new List<int>();

            UnhandledUidIntervals = new List<int>(uidIntervals);
            BeginDateUid = beginDateUid;
            UidValidity = uidValidity;
        }

        public static bool operator ==(ImapFolderUids a, ImapFolderUids b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(ImapFolderUids a, ImapFolderUids b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var p = obj as ImapFolderUids;
            return (object)p != null && Equals(p);
        }

        public bool Equals(ImapFolderUids p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return BeginDateUid == p.BeginDateUid && UnhandledUidIntervals.SequenceEqual(p.UnhandledUidIntervals) && UidValidity == p.UidValidity;
        }

        public override int GetHashCode()
        {
            return BeginDateUid ^ UnhandledUidIntervals.GetHashCode() ^ UidValidity.GetHashCode();
        }
    }
}
