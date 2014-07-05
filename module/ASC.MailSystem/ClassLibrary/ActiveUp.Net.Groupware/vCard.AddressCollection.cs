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

namespace ActiveUp.Net.Groupware.vCard
{
    /// <summary>
    /// Contains one or more Address object(s).
    /// </summary>
    #if !PocketPC
    [System.Serializable]
    #endif
    public class AddressCollection : System.Collections.CollectionBase
    {
        public AddressCollection()
        {
            
        }
        public void Add(ActiveUp.Net.Groupware.vCard.Address address)
        {
            this.List.Add(address);
        }
        public ActiveUp.Net.Groupware.vCard.Address this[int index]
        {
            get
            {
                return (ActiveUp.Net.Groupware.vCard.Address)this.List[index];
            }
        }
        /// <summary>
        /// Returns the first object in the collection whose Type is Prefered.
        /// </summary>
        /// <returns>The first object in the collection whose Type is Prefered.</returns>
        public ActiveUp.Net.Groupware.vCard.Address GetPrefered()
        {
            foreach(ActiveUp.Net.Groupware.vCard.Address address in this) if(address.IsPrefered) return address;
            return null;
        }
    }
}
