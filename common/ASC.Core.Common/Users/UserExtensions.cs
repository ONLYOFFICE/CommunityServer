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


using System.Collections.Generic;

namespace ASC.Core.Users
{
    public static class UserExtensions
    {
        public static bool IsOwner(this UserInfo ui)
        {
            if (ui == null) return false;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            return tenant != null && tenant.OwnerId.Equals(ui.ID);
        }

        public static bool IsMe(this UserInfo ui)
        {
            return ui != null && ui.ID == SecurityContext.CurrentAccount.ID;
        }

        public static bool IsAdmin(this UserInfo ui)
        {
            return ui != null && CoreContext.UserManager.IsUserInGroup(ui.ID, Constants.GroupAdmin.ID);
        }

        public static bool IsVisitor(this UserInfo ui)
        {
            return ui != null && CoreContext.UserManager.IsUserInGroup(ui.ID, Constants.GroupVisitor.ID);
        }

        public static bool IsOutsider(this UserInfo ui)
        {
            return IsVisitor(ui) && ui.ID == Constants.OutsideUser.ID;
        }

        public static bool IsLDAP(this UserInfo ui)
        {
            if (ui == null) return false;

            return !string.IsNullOrEmpty(ui.Sid);
        }

        // ReSharper disable once InconsistentNaming
        public static bool IsSSO(this UserInfo ui)
        {
            if (ui == null) return false;

            return !string.IsNullOrEmpty(ui.SsoNameId);
        }

        private const string EXT_MOB_PHONE = "extmobphone";
        private const string MOB_PHONE = "mobphone";
        private const string EXT_MAIL = "extmail";
        private const string MAIL = "mail";

        public static void ConvertExternalContactsToOrdinary(this UserInfo ui)
        {
            var ldapUserContacts = ui.Contacts;

            var newContacts = new List<string>();

            for (int i = 0, m = ldapUserContacts.Count; i < m; i += 2)
            {
                if (i + 1 >= ldapUserContacts.Count)
                    continue;

                var type = ldapUserContacts[i];
                var value = ldapUserContacts[i + 1];

                switch (type)
                {
                    case EXT_MOB_PHONE:
                        newContacts.Add(MOB_PHONE);
                        newContacts.Add(value);
                        break;
                    case EXT_MAIL:
                        newContacts.Add(MAIL);
                        newContacts.Add(value);
                        break;
                    default:
                        newContacts.Add(type);
                        newContacts.Add(value);
                        break;
                }
            }

            ui.Contacts = newContacts;
        }
    }
}
