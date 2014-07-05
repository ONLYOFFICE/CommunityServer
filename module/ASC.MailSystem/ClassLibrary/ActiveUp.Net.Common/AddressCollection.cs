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

using System.Collections.Generic;

namespace ActiveUp.Net.Mail
{
#region AddressCollection Object
    /// <summary>
    /// A collection of Internet Email addresses.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class AddressCollection : List<Address>
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public AddressCollection()
        {
            //
        }
        
        /// <summary>
        /// Allows the developer to add a collection of Address objects in another one.
        /// </summary>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <returns>The concatened collection.</returns>
        public static AddressCollection operator +(AddressCollection first, AddressCollection second) 
        {
            AddressCollection newAddresses = first;
            foreach(Address address in second)
                newAddresses.Add(address);

            return newAddresses;
        }

        /// <summary>
        /// Add an Address object.
        /// </summary>
        /// <param name="address">The Address object</param>
        public new void Add(Address address)
        {
            base.Add(address);
        }

        /// <summary>
        /// Add an Address object using the specified Internet email (RFC 2822 addr-spec).
        /// </summary>
        /// <param name="email">The email</param>
        public void Add(string email)
        {
            base.Add(new ActiveUp.Net.Mail.Address(email));
        }

        /// <summary>
        /// Add an Address using the specified Internet email (RFC 2822 addr-spec) and fullname.
        /// </summary>
        /// <param name="email">The email</param>
        /// <param name="name">The name</param>
        public void Add(string email, string name)
        {
            base.Add(new ActiveUp.Net.Mail.Address(email, name));
        }

        /// <summary>
        /// Remove the Address at the specified index position.
        /// </summary>
        /// <param name="index">The index position.</param>
        public void Remove(int index)
        {
            // Check to see if there is a Address at the supplied index.
            if (index < Count || index >= 0)
            {
                base.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns a string reprensentation of the Addresses.
        /// </summary>
        /// <returns>The addresses separated with commas, in format compliant with RFC2822's address specification.</returns>
        public string Merged
        {
            get
            {
                string _addresses="";
            
                foreach(Address address in this)
                {
                    _addresses += address.Merged+",";
                }
                return _addresses.TrimEnd(',');
            }
        }
        /// <summary>
        /// Returns a string reprensentation of the Addresses as HTML formatted links, separated by semicolons.
        /// </summary>
        /// <returns>The addresses as HTML formatted links, separated by semicolons.</returns>
        public string Links
        {
            get
            {
                string _addresses="";
            
                foreach(Address address in this)
                {
                    _addresses += address.Link+";";
                }
                return _addresses.TrimEnd(';');
            }
        }
    }
    #endregion
}