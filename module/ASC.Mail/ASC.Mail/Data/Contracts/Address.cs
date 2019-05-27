/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Mail.Utils;

namespace ASC.Mail.Data.Contracts
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
                var getString = string.Empty;

                if (Name.Length > 0)
                {
                    getString += "\"" + Name + "\" ";
                    getString += "<" + Email + ">";
                }
                else
                {
                    getString += Email;
                }

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
            return Merged;
        }

    }
}
