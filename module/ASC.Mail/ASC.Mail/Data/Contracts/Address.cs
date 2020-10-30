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
