/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Server.Utils
{
    public class Address
    {
        string _email, _name, _localPart, _domain;
        /// <summary>
        /// The default constructor. Set all properties to string.Empty.
        /// </summary>
        public Address()
        {
            Email = string.Empty;
            Name = string.Empty;
            LocalPart = string.Empty;
            Domain = string.Empty;
        }

        /// <summary>
        /// Creates the Address using the specified Internet email (RFC 2822 addr-spec).
        /// </summary>
        /// <param name="email">The email address to use.</param>
        public Address(string email)
        {
            /*this.Email = 
            this.Name = string.Empty;*/
            var addr = Parser.ParseAddress(email);
            Email = addr.Email;
            Name = addr.Name;
        }

        /// <summary>
        /// Creates the Address using the specified Internet email (RFC 2822 addr-spec) and fullname.
        /// </summary>
        /// <param name="email">The email address to use.</param>
        /// <param name="name">The owner's name.</param>
        public Address(string email, string name)
        {
            Email = email;
            Name = name;
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
                return _name;
            }
            set
            {
                _name = value.Trim();
            }
        }

        /// <summary>
        /// The Address owner's localpart.
        /// </summary>
        public string LocalPart
        {
            get
            {
                return _localPart;
            }
            set
            {
                _localPart = value.Trim();
            }
        }

        /// <summary>
        /// The Address owner's domain.
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain;
            }
            set
            {
                _domain = value.Trim();
            }
        }

        /// <summary>
        /// Gets a string compliant with RFC2822 address specification that represents the Address with the owner's fullname.
        /// </summary>
        public string Merged
        {
            get
            {
                var get_string = string.Empty;

                if (Name.Length > 0)
                {
                    get_string += "\"" + Name + "\" ";
                    get_string += "<" + Email + ">";
                }
                else
                {
                    get_string += Email;
                }

                return get_string;
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
            return Merged;
        }

    }
}
