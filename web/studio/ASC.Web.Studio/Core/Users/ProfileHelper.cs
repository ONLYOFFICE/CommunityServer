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


using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Studio.Core.Users
{
    public sealed class ProfileHelper
    {
        public UserInfo UserInfo { get; private set; }

        public ProfileHelper(string userNameOrUserId)
        {
            if (string.IsNullOrEmpty(userNameOrUserId) && SecurityContext.IsAuthenticated)
            {
                UserInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            }

            if (!String.IsNullOrEmpty(userNameOrUserId))
            {
                UserInfo = CoreContext.UserManager.GetUserByUserName(userNameOrUserId);
            }

            if (UserInfo == null || UserInfo.Equals(Constants.LostUser))
            {
                var userID = Guid.Empty;
                if (!String.IsNullOrEmpty(userNameOrUserId))
                {
                    try
                    {
                        userID = new Guid(userNameOrUserId);
                    }
                    catch
                    {
                        userID = SecurityContext.CurrentAccount.ID;
                    }
                }

                if (!CoreContext.UserManager.UserExists(userID))
                {
                    userID = SecurityContext.CurrentAccount.ID;
                }

                UserInfo = CoreContext.UserManager.GetUsers(userID);
            }
        }

        public List<MyContact> Phones
        {
            get
            {
                if (_phones != null) return _phones;
                _phones = GetPhones();
                return _phones;
            }
        }

        public List<MyContact> Emails
        {
            get
            {
                if (_emails != null) return _emails;
                _emails = GetEmails();
                return _emails;
            }
        }

        public List<MyContact> Messengers
        {
            get
            {
                if (_messengers != null) return _messengers;
                _messengers = GetMessengers();
                return _messengers;
            }
        }

        public List<MyContact> Contacts
        {
            get
            {
                if (_socialContacts != null) return _socialContacts;
                _socialContacts = GetSocialContacts();
                return _socialContacts;
            }
        }

        private List<MyContact> _phones;
        private List<MyContact> _emails;
        private List<MyContact> _messengers;
        private List<MyContact> _socialContacts;
        private List<MyContact> _contacts;

        private static MyContact GetSocialContact(String type, String value)
        {
            var node = SocialContactsManager.xmlSocialContacts.GetElementById(type);
            var title = node != null ? node.Attributes["title"].Value : type;
            var template = node != null ? node.GetElementsByTagName("template")[0].InnerXml : "{0}";

            return new MyContact
                {
                    type = type,
                    classname = type,
                    label = title,
                    text = HttpUtility.HtmlEncode(value),
                    link = String.Format(template, HttpUtility.HtmlEncode(value))
                };
        }

        private List<MyContact> GetContacts()
        {
            if (_contacts != null) return _contacts;

            var userInfoContacts = UserInfo.Contacts;
            _contacts = new List<MyContact>();
            for (int i = 0, n = userInfoContacts.Count; i < n; i += 2)
            {
                if (i + 1 < userInfoContacts.Count)
                {
                    _contacts.Add(GetSocialContact(userInfoContacts[i], userInfoContacts[i + 1]));
                }
            }
            return _contacts;
        }

        private List<MyContact> GetPhones()
        {
            var contacts = GetContacts();
            var phones = new List<MyContact>();

            for (int i = 0, n = contacts.Count; i < n; i++)
            {
                switch (contacts[i].type)
                {
                    case "phone":
                    case "extphone":
                    case "mobphone":
                    case "extmobphone":
                        phones.Add(contacts[i]);
                        break;
                }
            }

            return phones;
        }

        private List<MyContact> GetEmails()
        {
            var contacts = GetContacts();
            var emails = new List<MyContact>();

            for (int i = 0, n = contacts.Count; i < n; i++)
            {
                switch (contacts[i].type)
                {
                    case "mail":
                    case "extmail":
                    case "gmail":
                        emails.Add(contacts[i]);
                        break;
                }
            }

            return emails;
        }

        private List<MyContact> GetMessengers()
        {
            var contacts = GetContacts();
            var messengers = new List<MyContact>();

            for (int i = 0, n = contacts.Count; i < n; i++)
            {
                switch (contacts[i].type)
                {
                    case "jabber":
                    case "skype":
                    case "extskype":
                    case "msn":
                    case "aim":
                    case "icq":
                    case "gtalk":
                        messengers.Add(contacts[i]);
                        break;
                }
            }

            return messengers;
        }

        private List<MyContact> GetSocialContacts()
        {
            var contacts = GetContacts();
            var soccontacts = new List<MyContact>();

            for (int i = 0, n = contacts.Count; i < n; i++)
            {
                switch (contacts[i].type)
                {
                    case "mail":
                    case "extmail":
                    case "gmail":

                    case "phone":
                    case "extphone":
                    case "mobphone":
                    case "extmobphone":

                    case "jabber":
                    case "skype":
                    case "extskype":
                    case "msn":
                    case "aim":
                    case "icq":
                    case "gtalk":
                        continue;
                }
                soccontacts.Add(contacts[i]);
            }

            return soccontacts;
        }
    }
}