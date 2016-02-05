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

using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail
{
    #region Address Object

    /// <summary>
    /// Represent an Internet Email address with the owner's fullname.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Address
    {
        string _email, _name;
        /// <summary>
        /// The default constructor. Set all properties to string.Empty.
        /// </summary>
        public Address()
        {
            this.Email = string.Empty;
            this.Name = string.Empty;
        }

        /// <summary>
        /// Creates the Address using the specified Internet email (RFC 2822 addr-spec).
        /// </summary>
        /// <param name="email">The email address to use.</param>
        public Address(string email)
        {
            /*this.Email = 
            this.Name = string.Empty;*/
            Address addr = Parser.ParseAddress(email);
            this.Email = addr.Email;
            this.Name = addr.Name;
        }

        /// <summary>
        /// Creates the Address using the specified Internet email (RFC 2822 addr-spec) and fullname.
        /// </summary>
        /// <param name="email">The email address to use.</param>
        /// <param name="name">The owner's name.</param>
        public Address(string email, string name)
        {
            this.Email = email;
            this.Name = name;
        }

        /// <summary>
        /// The Internet email address (RFC 2822 addr-spec).
        /// </summary>
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value.Trim();
            }
        }

        /// <summary>
        /// The Address owner's fullname.
        /// </summary>
        public string Name
        {
            get
            {
//#if TRIAL
//                return ProductHelper.GetTrialString(this._name, TrialStringType.LongText);
//#else
                return _name;
//#endif
            }
            set
            {
                _name = value.Trim();
            }
        }

        /// <summary>
        /// Gets a string compliant with RFC2822 address specification that represents the Address with the owner's fullname.
        /// </summary>
        public string Merged
        {
            get
            {
                var getString = this.Name.Length > 0 ? string.Format("\"{0}\" <{1}>", this.Name, this.Email) : this.Email;

                return getString;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.Merged;
        }

        ///<summary>
        /// Gets an HTML formatted link to the address (mailto: form).
        /// </summary>
        public string Link
        {
            get
            {
                var getString = string.Format("<a href=\"mailto:{0}\">{1}</a>", this.Email, this.Name.Length > 0 ? this.Name : this.Email);

                return getString;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (!(obj is Address)) return false;

            var other = (Address)obj;

            return Email.Equals(other.Email) && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return ((Email != null ? Email.GetHashCode() : 0)) ^ (Name != null ? Name.GetHashCode() : 0);
        }

    }
    #endregion
}