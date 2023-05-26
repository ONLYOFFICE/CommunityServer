/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Users;

namespace ASC.Web.Core.Utility
{
    [Serializable]
    [DataContract]
    public class ImpersonationSettings : BaseSettings<ImpersonationSettings>
    {
        [DataMember(Name = "Enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "EnableType")]
        public ImpersonateEnableType EnableType { get; set; }

        [DataMember(Name = "OnlyForOwnGroups")]
        public bool OnlyForOwnGroups { get; set; }

        [DataMember(Name = "AllowedAdmins")]
        public List<Guid> AllowedAdmins { get; set; }

        [DataMember(Name = "RestrictionUsers")]
        public List<Guid> RestrictionUsers { get; set; }

        [DataMember(Name = "RestrictionGroups")]
        public List<Guid> RestrictionGroups { get; set; }

        public override Guid ID => new Guid("{843C6381-E900-437F-B153-16DAAD3C7AC5}");

        private static bool? _impersonateSettingsAvailable;

        public static bool Available
        {
            get
            {
                if (_impersonateSettingsAvailable.HasValue)
                {
                    return _impersonateSettingsAvailable.Value;
                }

                _impersonateSettingsAvailable = Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["core.user.impersonate"]);
                return _impersonateSettingsAvailable.Value;
            }
        }

        public override ISettings GetDefault()
        {
            return new ImpersonationSettings();
        }

        public static bool CanImpersonate(UserInfo currentUser, out ImpersonationSettings settings)
        {
            settings = null;

            if (!Available) return false;

            settings = Load();

            if (!settings.Enabled) return false;

            if (currentUser.IsOwner()) return true;

            if (settings.EnableType == ImpersonateEnableType.DisableForAdmins) return false;

            if (!currentUser.IsAdmin()) return false;

            if (settings.EnableType == ImpersonateEnableType.EnableForAllFullAdmins) return true;

            if (settings.AllowedAdmins == null || !settings.AllowedAdmins.Contains(currentUser.ID)) return false;

            return true;
        }

        public static bool CanImpersonateUser(Guid userId)
        {
            var currentUserId = SecurityContext.CurrentAccount.ID;

            if (userId == currentUserId || CoreContext.UserManager.IsSystemUser(userId)) return false;

            var currentUser = CoreContext.UserManager.GetUsers(currentUserId);

            if (!CanImpersonate(currentUser, out ImpersonationSettings settings))
            {
                return false;
            }

            var impersonatedUser = CoreContext.UserManager.GetUsers(userId);

            if (impersonatedUser.ID == Constants.LostUser.ID || impersonatedUser.IsOwner()) return false;

            if (currentUser.IsOwner()) return true;

            if (settings.RestrictionUsers != null && settings.RestrictionUsers.Contains(userId)) return false;

            var admins = WebItemSecurity.GetProductAdministrators(Guid.Empty).Select(admin => admin.ID).Distinct();

            if (admins.Contains(userId)) return false;

            var impersonatedUserGroups = CoreContext.UserManager.GetUserGroups(userId);

            if (settings.RestrictionGroups != null && settings.RestrictionGroups.Any())
            {
                foreach (var group in impersonatedUserGroups)
                {
                    if (settings.RestrictionGroups.Contains(group.ID)) return false;
                }
            }

            if (settings.OnlyForOwnGroups)
            {
                var currentUserGroups = CoreContext.UserManager.GetUserGroups(currentUserId);

                foreach (var group in currentUserGroups)
                {
                    if (impersonatedUserGroups.Contains(group)) return true;
                }

                return false;
            }

            return true;
        }

        public static bool IsImpersonator()
        {
            var cookiesForComeback = CookiesManager.GetCookies(CookiesType.ComebackAuthKey);

            return !string.IsNullOrEmpty(cookiesForComeback);
        }

        public static ImpersonationSettings LoadAndRefresh()
        {
            var settings = Load();
            var updatedFlag = false;

            if (settings.AllowedAdmins != null)
            {
                settings.AllowedAdmins.RemoveAll(adminId =>
                {
                    var admin = CoreContext.UserManager.GetUsers(adminId);

                    if (admin.ID == Constants.LostUser.ID || admin.Status == EmployeeStatus.Terminated || !admin.IsAdmin())
                    {
                        updatedFlag = true;
                        return true;
                    }

                    return false;
                });
            }

            if (settings.RestrictionUsers != null)
            {
                var admins = WebItemSecurity.GetProductAdministrators(Guid.Empty).Select(admin => admin.ID).Distinct();

                settings.RestrictionUsers.RemoveAll(userId =>
                {
                    var user = CoreContext.UserManager.GetUsers(userId);

                    if (user.ID == Constants.LostUser.ID || user.Status == EmployeeStatus.Terminated || admins.Contains(userId))
                    {
                        updatedFlag = true;
                        return true;
                    }
                    return false;
                });
            }

            if (settings.RestrictionGroups != null)
            {
                settings.RestrictionGroups.RemoveAll(groupId =>
                {
                    var groupInfo = CoreContext.UserManager.GetGroupInfo(groupId);

                    if (groupInfo.ID == Constants.LostGroupInfo.ID)
                    {
                        updatedFlag = true;
                        return true;
                    }

                    return false;
                });
            }

            if (updatedFlag)
            {
                settings.Save();
            }

            return settings;
        }
    }

    public enum ImpersonateEnableType
    {
        DisableForAdmins,
        EnableForAllFullAdmins,
        EnableWithLimits
    }
}
