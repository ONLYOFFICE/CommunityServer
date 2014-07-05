/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            if (SecurityContext.IsAuthenticated)
            {
                userNameOrUserId = String.IsNullOrEmpty(userNameOrUserId) ? SecurityContext.CurrentAccount.ID.ToString() : userNameOrUserId;
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
                    case "mobphone":
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
                    case "gmail":

                    case "phone":
                    case "mobphone":

                    case "jabber":
                    case "skype":
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