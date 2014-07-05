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
#region NewsGroupCollection Object
    /// <summary>
    /// Represents a collection of newsgroups.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class NewsGroupCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Adds the provided newsgroup to the collection.
        /// </summary>
        /// <param name="group"></param>
        public void Add(ActiveUp.Net.Mail.NewsGroup group)
        {
            this.List.Add(group);
        }
        /// <summary>
        /// Retrieves the newsgroup at the specified index in the collection.
        /// </summary>
        public ActiveUp.Net.Mail.NewsGroup this[int index]
        {
            get
            {
                return (ActiveUp.Net.Mail.NewsGroup)this.List[index];
            }
        }
    }
    #endregion
}