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

using AjaxPro;
using ASC.ActiveDirectory;
using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.LdapSettings, Location)]
    [AjaxNamespace("LdapSettingsController")]
    public partial class LdapSettings : UserControl
    {
        private const int ONE_THREAD = 1;
        protected LDAPSupportSettings settings;
        private static ProgressQueue _tasks = new ProgressQueue(ONE_THREAD, TimeSpan.FromMinutes(15), true);
        private static ILog _log = LogManager.GetLogger(typeof(LdapSettings));

        public const string Location = "~/userControls/Management/LdapSettings/LdapSettings.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(LdapSettings), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/ldapsettings/js/ldapsettings.js"));
            ldapSettingsConfirmContainerId.Options.IsPopup = true;
            ldapSettingsLimitPanelId.Options.IsPopup = true;
            settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void SaveSettings(string serializeSettings)
        {
            if (Context.User != null && Context.User.Identity != null)
            {
                var userInfo = CoreContext.UserManager.GetUsers(((IUserAccount)Context.User.Identity).ID);
                if (userInfo != ASC.Core.Users.Constants.LostUser && userInfo.IsOwner())
                {
                    lock (_tasks.SynchRoot)
                    {
                        var task = _tasks.GetItems().OfType<SaveSettingTask>().
                            FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                        if (task != null && task.IsCompleted)
                        {
                            _tasks.Remove(task);
                            task = null;
                        }
                        if (task == null)
                        {
                            task = new SaveSettingTask(serializeSettings)
                            {
                                Id = TenantProvider.CurrentTenantID,
                                Status = string.Empty
                            };
                            _tasks.Add(task);
                        }
                        return;
                    }
                }
            }
            _log.ErrorFormat("Insufficient Access Rights by saving ldap settings!");
            throw new SecurityException();
        }

        [AjaxMethod(HttpSessionStateRequirement.None)]
        public LDAPSupportSettingsResult GetStatus()
        {
            lock (_tasks.SynchRoot)
            {
                return ToResult(_tasks.GetItems().OfType<SaveSettingTask>().
                    FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID));
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.None)]
        public LDAPSupportSettings RestoreDefaultSettings()
        {
            return new LDAPSupportSettings().GetDefault() as LDAPSupportSettings;
        }

        private LDAPSupportSettingsResult ToResult(SaveSettingTask task)
        {
            lock (_tasks.SynchRoot)
            {
                if (task == null)
                {
                    return null;
                }
                return new LDAPSupportSettingsResult
                {
                    Id = task.Id.ToString(),
                    Completed = task.IsCompleted,
                    Percents = (int)task.Percentage,
                    Status = (string)task.Status,
                    Error = (string)task.Error
                };
            }
        }

        private class SaveSettingTask : IProgressItem
        {
            private const int WAITING_TIMEOUT = 600;
            private static ILog _log = LogManager.GetLogger(typeof(SaveSettingTask));
            private string _serializeSettings;
            private LDAPUserImporter _importer = new LDAPUserImporter();

            public object Error { get; set; }

            public object Id { get; set; }

            public bool IsCompleted { get; set; }

            public double Percentage { get; set; }

            public object Status { get; set; }

            public SaveSettingTask(string serializeSettings)
            {
                _serializeSettings = serializeSettings;
            }

            public void RunJob()
            {
                Percentage = 1;
                Status = Resource.LdapSettingsStatusCheckingLdapSettings;
                var tenantID = (int)Id;
                CoreContext.TenantManager.SetCurrentTenant(tenantID);
                if (_serializeSettings == null)
                {
                    _log.ErrorFormat("Can't save default LDAP settings.");
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    IsCompleted = true;
                    return;
                }
                try
                {
                    var settings = (LDAPSupportSettings)JavaScriptDeserializer.DeserializeFromJson(_serializeSettings, typeof(LDAPSupportSettings));
                    if (settings == null)
                    {
                        _log.ErrorFormat("Wrong LDAP settings were received from client.");
                        Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                        IsCompleted = true;
                        return;
                    }
                    if (!settings.Server.StartsWith("LDAP://"))
                    {
                        settings.Server = "LDAP://" + settings.Server;
                    }
                    if (!string.IsNullOrEmpty(settings.Password))
                    {
                        settings.PasswordBytes = InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(settings.Password));
                    }
                    settings.Password = string.Empty;
                    var error = GetError(ADDomain.CheckSettings(settings, _importer));
                    if (error == string.Empty)
                    {
                        Status = Resource.LdapSettingsStatusSavingSettings;
                        Percentage = 3;
                        if (!SettingsManager.Instance.SaveSettings<LDAPSupportSettings>(settings, tenantID))
                        {
                            _log.ErrorFormat("Can't save LDAP settings.");
                            Error = Resource.LdapSettingsErrorCantSaveLdapSettings;
                            IsCompleted = true;
                            return;
                        }
                        // for logout old ldap users
                        AddLToSids();

                        if (settings.EnableLdapAuthentication)
                        {
                             CreateUsersAndGroups(settings);
                        }
                        Percentage = 100;
                    }
                    else
                    {
                        Error = error;
                    }
                }
                catch (TenantQuotaException e)
                {
                    _log.ErrorFormat("TenantQuotaException.", e);
                    Error = Resource.LdapSettingsTenantQuotaSettled;
                }
                catch (FormatException)
                {
                    Error = Resource.LdapSettingsErrorCantCreateUsers;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Internal server error.", e);
                    Error = Resource.LdapSettingsInternalServerError;
                }
                IsCompleted = true;
            }

            private string GetError(byte result)
            {
                switch (result)
                {
                    case ADDomain.OPERATION_OK:
                        return string.Empty;
                    case ADDomain.WRONG_SERVER_OR_PORT:
                        return Resource.LdapSettingsErrorWrongServerOrPort;
                    case ADDomain.WRONG_USER_DN:
                        return Resource.LdapSettingsErrorWrongUserDN;
                    case ADDomain.INCORRECT_LDAP_FILTER:
                        return Resource.LdapSettingsErrorIncorrectLdapFilter;
                    case ADDomain.USERS_NOT_FOUND:
                        return Resource.LdapSettingsErrorUsersNotFound;
                    case ADDomain.WRONG_LOGIN_ATTRIBUTE:
                        return Resource.LdapSettingsErrorWrongLoginAttribute;
                    case ADDomain.WRONG_GROUP_DN_OR_GROUP_NAME:
                        return Resource.LdapSettingsErrorWrongGroupDNOrGroupName;
                    case ADDomain.GROUPS_NOT_FOUND:
                        return Resource.LdapSettingsErrorGroupsNotFound;
                    case ADDomain.WRONG_GROUP_ATTRIBUTE:
                        return Resource.LdapSettingsErrorWrongGroupAttribute;
                    case ADDomain.WRONG_USER_ATTRIBUTE:
                        return Resource.LdapSettingsErrorWrongUserAttribute;
                    case ADDomain.CREDENTIALS_NOT_VALID:
                        return Resource.LdapSettingsErrorCredentialsNotValid;
                    default:
                        return Resource.LdapSettingsErrorUnknownError;
                }
            }

            private void AddLToSids()
            {
                var users = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToArray();
                for (int i = 0; i < users.Length; i++)
                {
                    if (!users[i].Sid.StartsWith("l"))
                    {
                        users[i].Sid = "l" + users[i].Sid;
                        CoreContext.UserManager.SaveUserInfo(users[i]);
                    }
                }
            }

            private void CreateUsersAndGroups(LDAPSupportSettings settings)
            {
                Status = Resource.LdapSettingsStatusGettingGroupsFromLdap;
                var groups = _importer.GetDiscoveredGroupsByAttributes(settings);
                Percentage = 8;
                Status = Resource.LdapSettingsStatusGettingUsersFromLdap;
                var users = _importer.GetDiscoveredUsersByAttributes(settings);
                Percentage = 15;
                Status = Resource.LdapSettingsStatusSavingGroups;
                AddGroupsToCore(groups);
                Percentage = 35;
                Status = Resource.LdapSettingsStatusSavingUsers;
                double percents = 35;
                var step = percents / users.Count;
                if (users != null && users.Count != 0)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].FirstName == string.Empty)
                        {
                            users[i].FirstName = Resource.FirstName;
                        }
                        if (users[i].LastName == string.Empty)
                        {
                            users[i].LastName = Resource.LastName;
                        }
                    }
                    users = users.SortByUserName();
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                        {
                            users[i] = UserManagerWrapper.AddUser(users[i], UserManagerWrapper.GeneratePassword(), true, false);
                        }
                        else
                        {
                            users[i] = UserManagerWrapper.AddUser(users[i], UserManagerWrapper.GeneratePassword(), true, false, true);
                        }
                        Percentage += step;
                    }
                }
                Percentage = 70;
                var allLdapUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToArray();
                percents = 20;
                step = percents / allLdapUsers.Length;
                for (int i = 0; i < allLdapUsers.Length; i++)
                {
                    _importer.AddUserIntoGroups(allLdapUsers[i], settings);
                    Percentage += step;
                }
                Percentage = 90;
                AddUsersInCacheGroups();
                RemoveEmptyGroups();
            }

            private void RemoveEmptyGroups()
            {
                foreach (var group in CoreContext.GroupManager.GetGroups().Where(g => !string.IsNullOrEmpty(g.Sid)).ToList())
                {
                    if (CoreContext.UserManager.GetUsersByGroup(group.ID).Length == 0)
                    {
                        CoreContext.GroupManager.DeleteGroup(group.ID);
                    }
                }
            }

            private void AddUsersInCacheGroups()
            {
                var cache = _importer.GetCache();
                if (cache != null && cache.Count != 0)
                {
                    double percents = 10;
                    double step = percents / cache.Count;
                    foreach (var pair in cache)
                    {
                        var childDomainGroup = GetLDAPGroup(pair.Key);
                        if (childDomainGroup != null)
                        {
                            var childGroup = CoreContext.GroupManager.GetGroupInfoBySid(childDomainGroup.Sid.Value);
                            if (childGroup != ASC.Core.Users.Constants.LostGroupInfo)
                            {
                                foreach (var user in CoreContext.UserManager.GetUsersByGroup(childGroup.ID))
                                {
                                    foreach (var sid in pair.Value)
                                    {
                                        var group = CoreContext.GroupManager.GetGroupInfoBySid(sid);
                                        if (group != ASC.Core.Users.Constants.LostGroupInfo)
                                        {
                                            CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                                        }
                                    }
                                }
                            }
                        }
                        Percentage += step;
                    }
                }
            }

            private LDAPGroup GetLDAPGroup(string dn)
            {
                if (_importer.DomainGroups != null)
                {
                    for (int i = 0; i < _importer.DomainGroups.Count; i++)
                    {
                        if (_importer.DomainGroups[i].DistinguishedName.Equals(dn, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return _importer.DomainGroups[i];
                        }
                    }
                }
                return null;
            }

            private void AddGroupsToCore(List<GroupInfo> groups)
            {
                var percents = 30;
                if (groups != null && groups.Count != 0)
                {
                    var step = percents / groups.Count;
                    foreach (var group in groups)
                    {
                        if (CoreContext.GroupManager.GetGroupInfoBySid(group.Sid).ID == ASC.Core.Users.Constants.LostGroupInfo.ID)
                        {
                            CoreContext.GroupManager.SaveGroupInfo(group);
                        }
                        Percentage += step;
                    }
                }
                Percentage = 45;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}