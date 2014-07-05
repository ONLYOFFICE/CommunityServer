// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

namespace ActiveUp.Net.Mail
{
#region FlagCollection object
    /// <summary>
    /// Represents a collection of Flags.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class FlagCollection : System.Collections.CollectionBase, IFlagCollection
    {
        /// <summary>
        /// Adds the provided flag to the collection.
        /// </summary>
        /// <param name="flag"></param>
        public void Add(ActiveUp.Net.Mail.IFlag flag)
        {
            this.List.Add(flag);
        }
        /// <summary>
        /// Adds a new flag to the collection with the specified name.
        /// </summary>
        /// <param name="flagName"></param>
        public void Add(string flagName)
        {
            this.List.Add(new Flag(flagName));
        }
        /// <summary>
        /// Retrieves the flag at index [index] in the collection.
        /// </summary>
        public ActiveUp.Net.Mail.Flag this[int index]
        {
            get
            {
                return (ActiveUp.Net.Mail.Flag)this.List[index];
            }
        }
        /// <summary>
        /// Retrieves the flag with the specified name in the collection.
        /// </summary>
        public ActiveUp.Net.Mail.Flag this[string flagName]
        {
            get
            {
                for(int i=0;i<this.List.Count;i++) 
                    if(((ActiveUp.Net.Mail.Flag)this.List[i]).Name.Equals(flagName, System.StringComparison.InvariantCultureIgnoreCase)) return this[i];
                return null;
            }
        }
        /// <summary>
        /// A string representing the collection (in IMAP4rev1 compatible format).
        /// </summary>
        public string Merged
        {
            get
            {
                string ret = "";
                foreach(ActiveUp.Net.Mail.Flag flag in this) ret += "\\"+flag.Name+" ";
                return "("+ret.Trim(' ')+")";
            }
        }
    }
    #endregion
}