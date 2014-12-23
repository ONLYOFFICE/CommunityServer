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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.People.Classes
{
    public sealed class MyUserProfile
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string GroupId { get; set; }
        public string Group { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }
        public DateTime? RegDate { get; set; }
        public string RegDateValue { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthDateValue { get; set; }
        public string Place { get; set; }
        public string Comment { get; set; }
        public List<MyContact> Phones { get; set; }
        public List<MyContact> Emails { get; set; }
        public List<MyContact> Messengers { get; set; }
        public List<MyContact> Contacts { get; set; }

        private MyContact GetSocialContact(String type, String value)
        {
            XmlElement node = SocialContactsManager.xmlSocialContacts.GetElementById(type);
            String title = node != null ? node.Attributes["title"].Value : type;
            String template = node != null ? node.GetElementsByTagName("template")[0].InnerXml : "{0}";

            return new MyContact()
            {
                type = type,
                classname = type,
                label = title,
                text = System.Web.HttpUtility.HtmlEncode(value),
                link = String.Format(template, System.Web.HttpUtility.HtmlEncode(value))
            };
        }

        private List<MyContact> GetContacts(List<string> userInfoContacts)
        {
            List<MyContact> contacts = new List<MyContact>();
            for (int i = 0, n = userInfoContacts.Count; i < n; i += 2)
            {
                if (i + 1 < userInfoContacts.Count)
                {
                    contacts.Add(GetSocialContact(userInfoContacts[i], userInfoContacts[i + 1]));
                }
            }
            return contacts;
        }

        private List<MyContact> GetPhones(List<string> userInfoContacts)
        {
            List<MyContact> contacts = GetContacts(userInfoContacts);
            List<MyContact> phones = new List<MyContact>();

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

        private List<MyContact> GetEmails(List<string> userInfoContacts)
        {
            List<MyContact> contacts = GetContacts(userInfoContacts);
            List<MyContact> emails = new List<MyContact>();

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

        private List<MyContact> GetMessengers(List<string> userInfoContacts)
        {
            List<MyContact> contacts = GetContacts(userInfoContacts);
            List<MyContact> messengers = new List<MyContact>();

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

        private List<MyContact> GetSocialContacts(List<string> userInfoContacts)
        {
            List<MyContact> contacts = GetContacts(userInfoContacts);
            List<MyContact> soccontacts = new List<MyContact>();

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

        public MyUserProfile(Guid Id)
        {
            GroupInfo[] userGroups = CoreContext.UserManager.GetUserGroups(Id);
            var _userInfo = CoreContext.UserManager.GetUsers(Id);

            this.Id = Id;
            this.UserName = _userInfo.UserName;
            this.DisplayName = _userInfo.DisplayUserName(true);
            this.GroupId = userGroups.Count() > 0 ? userGroups.First().ID.ToString() : String.Empty;
            this.Group = HttpUtility.HtmlEncode(_userInfo.Department);
            this.Title = HttpUtility.HtmlEncode(_userInfo.Title);
            this.Email = HttpUtility.HtmlEncode(_userInfo.Email);
            this.Phone = HttpUtility.HtmlEncode(_userInfo.MobilePhone);
            this.Avatar = _userInfo.GetPhotoURL();
            this.Gender = _userInfo.Sex != null && _userInfo.Sex.HasValue ? _userInfo.Sex == true ? Resources.Resource.LblMaleSexStatus : Resources.Resource.LblFemaleSexStatus : String.Empty;
            this.RegDate = _userInfo.WorkFromDate;
            this.RegDateValue = _userInfo.WorkFromDate.HasValue ? _userInfo.WorkFromDate.Value.ToShortDateString() : String.Empty;
            this.BirthDate = _userInfo.BirthDate;
            this.BirthDateValue = _userInfo.BirthDate.HasValue ? _userInfo.BirthDate.Value.ToShortDateString() : String.Empty;
            this.Place = HttpUtility.HtmlEncode(_userInfo.Location);
            this.Comment = HttpUtility.HtmlEncode(_userInfo.Notes);

            this.Phones = GetPhones(_userInfo.Contacts);
            this.Emails = GetEmails(_userInfo.Contacts);
            this.Messengers = GetMessengers(_userInfo.Contacts);
            this.Contacts = GetSocialContacts(_userInfo.Contacts);
        }

    }
}
