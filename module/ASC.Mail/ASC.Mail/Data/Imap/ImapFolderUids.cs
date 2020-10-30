/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
