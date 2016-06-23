/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Linq;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Feed.Aggregator
{
    public static class Helper
    {
        public static UserInfo GetUser(Guid id)
        {
            return CoreContext.UserManager.GetUsers(id);
        }

        public static string GetUsersString(List<Guid> users)
        {
            var usersString = users
                .Select(GetUser)
                .Aggregate(string.Empty, (current, user) => current + (user.DisplayUserName() + ", "));
            
            if (!string.IsNullOrEmpty(usersString))
            {
                usersString = usersString.Remove(usersString.Length - 2);
            }

            return usersString;
        }

        public static string GetContactsString(HashSet<int> contacts, ContactDao dao)
        {
            if (contacts == null || dao == null)
            {
                return null;
            }

            var contactsString = contacts
                .Select(dao.GetByID)
                .Aggregate(string.Empty, (current, contact) => current + (contact.GetTitle() + ", "));

            if (!string.IsNullOrEmpty(contactsString))
            {
                contactsString = contactsString.Remove(contactsString.Length - 2);
            }

            return contactsString;
        }

        public static string GetText(string html)
        {
            return HtmlUtil.GetText(html);
        }

        public static string GetHtmlDescription(string description)
        {
            return !string.IsNullOrEmpty(description) ? description.Replace("\n", "<br/>") : description;
        }
    }
}