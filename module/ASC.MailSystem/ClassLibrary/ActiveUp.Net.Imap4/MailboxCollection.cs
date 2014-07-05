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
#region MailboxCollection object
    /// <summary>
    /// Represents a collection of Mailboxes.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MailboxCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Adds the provided mailbox to the collection.
        /// </summary>
        /// <param name="mailbox">The mailbox to be added.</param>
        public void Add(ActiveUp.Net.Mail.Mailbox mailbox)
        {
            this.List.Add(mailbox);
        }
        /// <summary>
        /// Returns the mailbox at index [index] in the collection.
        /// </summary>
        public ActiveUp.Net.Mail.Mailbox this[int index]
        {
            get
            {
                return (ActiveUp.Net.Mail.Mailbox)this.List[index];
            }
        }
        /// <summary>
        /// Returns the mailbox with the specified name in the collection.
        /// </summary>
        public ActiveUp.Net.Mail.Mailbox this[string mailboxName]
        {
            get
            {
                for(int i=0;i<this.List.Count;i++) if(((ActiveUp.Net.Mail.Mailbox)this.List[i]).Name==mailboxName) return this[i];
                return null;
            }
        }
    }
    #endregion
}