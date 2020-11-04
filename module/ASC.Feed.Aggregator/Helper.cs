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