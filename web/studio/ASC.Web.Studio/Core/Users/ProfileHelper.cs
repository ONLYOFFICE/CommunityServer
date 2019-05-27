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