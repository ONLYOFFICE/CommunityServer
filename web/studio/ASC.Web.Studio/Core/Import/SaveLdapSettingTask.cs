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

using ASC.ActiveDirectory;
using ASC.ActiveDirectory.BuiltIn;
using ASC.ActiveDirectory.Novell;
using ASC.Common.Caching;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using log4net;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using ASC.ActiveDirectory.DirectoryServices;

namespace ASC.Web.Studio.Core.Import
{
    public class SaveLdapSettingTask : IProgressItem
    {
        #region .Fields
        private readonly LDAPSupportSettings _settings;
        private readonly ILog _log = LogManager.GetLogger(typeof(SaveLdapSettingTask));
        private LDAPUserImporter _importer;
        private readonly LdapSettingsChecker _ldapSettingsChecker;
        private static readonly TimeSpan CacheExpired = TimeSpan.FromMinutes(15);
        private readonly bool _skipSaveSettings;
        #endregion

        #region .Public

        #region .Properties
        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }
        #endregion

        #region .Constuctor

        public SaveLdapSettingTask(string serializeSettings, int tenantId, string status,
                                   bool acceptCertificate = false, bool skipSaveSettings = false)
            : this(JsonConvert.DeserializeObject<LDAPSupportSettings>(serializeSettings),
                   tenantId, status, acceptCertificate, skipSaveSettings)
        {
        }

        public SaveLdapSettingTask(LDAPSupportSettings settings, int tenantId, string status, bool acceptCertificate = false, bool skipSaveSettings = false)
        {
            _settings = settings;
            
            Id = tenantId;

            AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_ID);
            AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_IS_COMPLETED);
            AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_PERCENTAGE);
            AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_STATUS);
            AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_ERROR);
            AscCache.Default.Remove(Constants.NOVELL_LDAP_CERTIFICATE_CONFIRM_REQUEST);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_ID, Id);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, status);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_ACCEPT_CERTIFICATE, acceptCertificate);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STARTED, "started");
            SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, false);
            
            _ldapSettingsChecker = !WorkContext.IsMono
                                       ? (LdapSettingsChecker) new SystemLdapSettingsChecker()
                                       : new NovellLdapSettingsChecker();

            _skipSaveSettings = skipSaveSettings;
        }

        #endregion

        public void RunJob()
        {
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                var tenantId = (int) Id;

                CoreContext.TenantManager.SetCurrentTenant(tenantId);

                ResolveCulture();

                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 1);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusCheckingLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, false);

                if (_settings == null)
                {
                    _log.Error("Can't save default LDAP settings.");
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                    return;
                }
                try
                {
                    PrepareSettings(_settings);

                    var acceptCertificate =
                        (bool?) AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_ACCEPT_CERTIFICATE) ?? false;

                    if (_importer == null)
                    {
                        _importer = new LDAPUserImporter(_settings);
                    }

                    SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 5);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusLoadingBaseInfo);

                    var result = _ldapSettingsChecker.CheckSettings(_importer, acceptCertificate);

                    if (result == LdapSettingsChecker.CERTIFICATE_REQUEST)
                    {
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 0);
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS,
                                         Resource.LdapSettingsStatusCertificateVerification);
                        SetProggressInfo(Constants.NOVELL_LDAP_CERTIFICATE_CONFIRM_REQUEST,
                                         ((NovellLdapSettingsChecker) _ldapSettingsChecker).CertificateConfirmRequest);
                        return;
                    }

                    var error = GetError(result);
                    if (error != string.Empty)
                    {
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, error);
                        return;
                    }

                    if (!_skipSaveSettings)
                    {
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusSavingSettings);
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 10);

                        _settings.IsDefault = _settings.Equals(_settings.GetDefault());

                        if (!SettingsManager.Instance.SaveSettings(_settings, tenantId))
                        {
                            _log.Error("Can't save LDAP settings.");
                            SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR,
                                             Resource.LdapSettingsErrorCantSaveLdapSettings);
                            return;
                        }
                    }

                    RemoveOldWorkaroundOnLogoutLDAPUsers();

                    if (_settings.EnableLdapAuthentication)
                    {
                        SyncLDAP();

                        var taskError = AscCache.Default.Get<string>(Constants.LDAP_SETTING_TASK_ERROR);
                        if (!string.IsNullOrEmpty(taskError))
                            return;
                    }
                    else
                    {
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsModifyLdapUsers);
                        SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 50);

                        var existingLDAPUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToList();
                        foreach (var existingLDAPUser in existingLDAPUsers)
                        {
                            existingLDAPUser.Sid = null;
                            CoreContext.UserManager.SaveUserInfo(existingLDAPUser);
                        }
                    }

                    SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 100);
                }
                catch (TenantQuotaException e)
                {
                    _log.ErrorFormat("TenantQuotaException. {0}", e);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsTenantQuotaSettled);
                }
                catch (FormatException e)
                {
                    _log.ErrorFormat("FormatException error. {0}", e);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantCreateUsers);
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Internal server error. {0}", e);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsInternalServerError);
                }
            }
            finally
            {
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                AscCache.Default.Remove(Constants.LDAP_SETTING_TASK_STARTED);
                SecurityContext.Logout();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region .Private

        private void PrepareSettings(LDAPSupportSettings settings)
        {
            if (settings == null)
            {
                _log.Error("Wrong LDAP settings were received from client.");
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                return;
            }

            if (!settings.EnableLdapAuthentication)
            {
                settings.Password = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.Server))
                settings.Server = settings.Server.Trim();
            else
            {
                _log.Error("settings.Server is null or empty.");
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                return;
            }

            if (!settings.Server.StartsWith("LDAP://"))
                settings.Server = "LDAP://" + settings.Server;

            if (!string.IsNullOrWhiteSpace(settings.UserDN))
                settings.UserDN = settings.UserDN.Trim();
            else
            {
                _log.Error("settings.UserDN is null or empty.");
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.LoginAttribute))
                settings.LoginAttribute = settings.LoginAttribute.Trim();
            else
            {
                _log.Error("settings.LoginAttribute is null or empty.");
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.UserFilter))
                settings.UserFilter = settings.UserFilter.Trim();

            if (!string.IsNullOrWhiteSpace(settings.FirstNameAttribute))
                settings.FirstNameAttribute = settings.FirstNameAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.SecondNameAttribute))
                settings.SecondNameAttribute = settings.SecondNameAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.MailAttribute))
                settings.MailAttribute = settings.MailAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.TitleAttribute))
                settings.TitleAttribute = settings.TitleAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.MobilePhoneAttribute))
                settings.MobilePhoneAttribute = settings.MobilePhoneAttribute.Trim();

            if (settings.GroupMembership)
            {
                if (!string.IsNullOrWhiteSpace(settings.GroupDN))
                    settings.GroupDN = settings.GroupDN.Trim();
                else
                {
                    _log.Error("settings.GroupDN is null or empty.");
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(settings.GroupFilter))
                    settings.GroupFilter = settings.GroupFilter.Trim();

                if (!string.IsNullOrWhiteSpace(settings.GroupAttribute))
                    settings.GroupAttribute = settings.GroupAttribute.Trim();
                else
                {
                    _log.Error("settings.GroupAttribute is null or empty.");
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(settings.UserAttribute))
                    settings.UserAttribute = settings.UserAttribute.Trim();
                else
                {
                    _log.Error("settings.UserAttribute is null or empty.");
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                    return;
                }
            }

            if (WorkContext.IsMono)
                settings.Authentication = true;

            if (!settings.Authentication)
            {
                settings.Password = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.Login))
                settings.Login = settings.Login.Trim();
            else
            {
                _log.Error("settings.Login is null or empty.");
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorCantGetLdapSettings);
                SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                return;
            }

            if (settings.PasswordBytes == null || !settings.PasswordBytes.Any())
            {
                if (!string.IsNullOrEmpty(settings.Password))
                {
                    settings.PasswordBytes =
                        InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(settings.Password));
                }
                else
                {
                    _log.Error("settings.Password is null or empty.");
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR,
                                     Resource.LdapSettingsErrorCantGetLdapSettings);
                    SetProggressInfo(Constants.LDAP_SETTING_TASK_IS_COMPLETED, true);
                    return;
                }
            }

            settings.Password = string.Empty;
        }

        private static void ResolveCulture()
        {
            CultureInfo culture = null;

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                culture = tenant.GetCulture();
            }

            if (culture != null && !Equals(Thread.CurrentThread.CurrentCulture, culture))
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            if (culture != null && !Equals(Thread.CurrentThread.CurrentUICulture, culture))
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        private static string GetError(byte result)
        {
            switch (result)
            {
                case LdapSettingsChecker.OPERATION_OK:
                    return string.Empty;
                case LdapSettingsChecker.WRONG_SERVER_OR_PORT:
                    return Resource.LdapSettingsErrorWrongServerOrPort;
                case LdapSettingsChecker.WRONG_USER_DN:
                    return Resource.LdapSettingsErrorWrongUserDN;
                case LdapSettingsChecker.INCORRECT_LDAP_FILTER:
                    return Resource.LdapSettingsErrorIncorrectLdapFilter;
                case LdapSettingsChecker.USERS_NOT_FOUND:
                    return Resource.LdapSettingsErrorUsersNotFound;
                case LdapSettingsChecker.WRONG_LOGIN_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongLoginAttribute;
                case LdapSettingsChecker.WRONG_GROUP_DN:
                    return Resource.LdapSettingsErrorWrongGroupDN;
                case LdapSettingsChecker.INCORRECT_GROUP_LDAP_FILTER:
                    return Resource.LdapSettingsErrorWrongGroupFilter;
                case LdapSettingsChecker.GROUPS_NOT_FOUND:
                    return Resource.LdapSettingsErrorGroupsNotFound;
                case LdapSettingsChecker.WRONG_GROUP_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongGroupAttribute;
                case LdapSettingsChecker.WRONG_USER_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongUserAttribute;
                case LdapSettingsChecker.WRONG_GROUP_NAME_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongGroupNameAttribute;
                case LdapSettingsChecker.CREDENTIALS_NOT_VALID:
                    return Resource.LdapSettingsErrorCredentialsNotValid;
                case LdapSettingsChecker.CONNECT_ERROR:
                    return Resource.LdapSettingsConnectError;
                case LdapSettingsChecker.STRONG_AUTH_REQUIRED:
                    return Resource.LdapSettingsStrongAuthRequired;
                case LdapSettingsChecker.WRONG_SID_ATTRIBUTE:
                    return Resource.LdapSettingsWrongSidAttribute;
                case LdapSettingsChecker.TLS_NOT_SUPPORTED:
                    return Resource.LdapSettingsTlsNotSupported;
                default:
                    return Resource.LdapSettingsErrorUnknownError;
            }
        }

        /// <summary>
        /// Remove old workaround: Add L to sid for invalidate cookie
        /// </summary>
        private static void RemoveOldWorkaroundOnLogoutLDAPUsers()
        {
            var users = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null && u.Sid.StartsWith("l")).ToArray();
            foreach (var userInfo in users)
            {
                userInfo.Sid = userInfo.Sid.Remove(0, 1);
                CoreContext.UserManager.SaveUserInfo(userInfo);
            }
        }

        private static void RemoveOldDbUsers(List<UserInfo> ldapUsers)
        {
            var dbLdapUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null && !u.IsOwner()).ToList();

            if (!dbLdapUsers.Any())
                return;

            var removedUsers =
                dbLdapUsers
                    .Where(u => ldapUsers
                                    .FirstOrDefault(lu =>
                                                    u.Sid.Equals(lu.Sid)) == null)
                    .ToList();

            if (!removedUsers.Any())
                return;

            const double percents = 10;

            var step = percents / removedUsers.Count;

            var percentage = Convert.ToDouble(AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_PERCENTAGE) ?? "0");

            foreach (var removedUser in removedUsers)
            {
                CoreContext.UserManager.DeleteUser(removedUser.ID);

                percentage += step;

                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, Convert.ToInt32(percentage));
            }

            dbLdapUsers.RemoveAll(removedUsers.Contains);

            ldapUsers.RemoveAll(u => removedUsers.Exists(ru => ru.Sid.Equals(u.Sid)));
        }

        private static void RemoveOldDbGroups(List<GroupInfo> ldapGroups)
        {
            var percentage = Convert.ToDouble(AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_PERCENTAGE) ?? "0");

            var removedDbLdapGroups =
                CoreContext.UserManager.GetGroups()
                    .Where(g => g.Sid != null && ldapGroups.FirstOrDefault(lg => g.Sid.Equals(lg.Sid)) == null)
                    .ToList();

            if (!removedDbLdapGroups.Any())
                return;

            const double percents = 10;

            var step = percents / removedDbLdapGroups.Count;

            foreach (var groupInfo in removedDbLdapGroups)
            {
                CoreContext.UserManager.DeleteGroup(groupInfo.ID);

                percentage += step;

                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, Convert.ToInt32(percentage));
            }
        }

        private static void SyncDbUsers(List<UserInfo> ldapUsers)
        {
            const double percents = 35;

            var step = percents / ldapUsers.Count;

            var percentage = Convert.ToDouble(AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_PERCENTAGE) ?? "0");

            if (!ldapUsers.Any()) 
                return;

            foreach (var userInfo in ldapUsers)
            {
                UserManagerWrapper.SyncUserLDAP(userInfo);

                percentage += step;

                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, Convert.ToInt32(percentage));
            }
        }

        private static void RemoveOldDbGroupMembers(GroupInfo groupInfo, IEnumerable<UserInfo> dbGroupMembers, IEnumerable<UserInfo> ldapGroupMembers)
        {
            var removedGroupMembers =
                    dbGroupMembers.Where(
                        dbUser => ldapGroupMembers
                                      .FirstOrDefault(lu => dbUser.Sid.Equals(lu.Sid)) == null);

            foreach (var dbUser in removedGroupMembers)
            {
                CoreContext.UserManager.RemoveUserFromGroup(dbUser.ID, groupInfo.ID);
            }
        }

        private static void RemoveDbGroupIfEmpty(GroupInfo groupInfo)
        {
            var dbGroupMembers = CoreContext.UserManager.GetUsersByGroup(groupInfo.ID).ToList();

            if (!dbGroupMembers.Any())
                CoreContext.UserManager.DeleteGroup(groupInfo.ID);
        }

        private static UserInfo SearchDbUserBySid(string sid)
        {
            if (string.IsNullOrEmpty(sid))
                return ASC.Core.Users.Constants.LostUser;
            
            var foundUser = CoreContext.UserManager.GetUserBySid(sid);

            return foundUser;
        }

        private void SyncDbGroups(Dictionary<GroupInfo, List<UserInfo>> ldapGroupsWithUsers)
        {
            const double percents = 20;

            var step = percents / ldapGroupsWithUsers.Count;

            var percentage = Convert.ToDouble(AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_PERCENTAGE) ?? "0");

            if (!ldapGroupsWithUsers.Any())
                return;

            foreach (var ldapGroupWithUsers in ldapGroupsWithUsers)
            {
                var ldapGroup = ldapGroupWithUsers.Key;

                var ldapGroupUsers = ldapGroupWithUsers.Value;

                var dbLdapGroup = CoreContext.UserManager.GetGroupInfoBySid(ldapGroup.Sid);

                IEnumerable<UserInfo> groupMembersToAdd;

                if (Equals(dbLdapGroup, ASC.Core.Users.Constants.LostGroupInfo))
                {
                    if (ldapGroupUsers.Any()) // Skip empty groups
                    {
                        groupMembersToAdd = ldapGroupUsers.Select(ldapGroupUser => SearchDbUserBySid(ldapGroupUser.Sid))
                                                          .Where(
                                                              userBySid =>
                                                              !Equals(userBySid, ASC.Core.Users.Constants.LostUser))
                                                              .ToList();

                        if (groupMembersToAdd.Any())
                        {
                            ldapGroup = CoreContext.UserManager.SaveGroupInfo(ldapGroup);

                            foreach (var userBySid in groupMembersToAdd)
                            {
                                CoreContext.UserManager.AddUserIntoGroup(userBySid.ID, ldapGroup.ID);
                            }
                        }
                    }
                }
                else
                {
                    if (!dbLdapGroup.Name.Equals(ldapGroup.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Update group name
                        dbLdapGroup.Name = ldapGroup.Name;

                        CoreContext.UserManager.SaveGroupInfo(dbLdapGroup);
                    }

                    var dbGroupMembers =
                        CoreContext.UserManager.GetUsersByGroup(dbLdapGroup.ID).Where(u => u.Sid != null).ToList();

                    RemoveOldDbGroupMembers(dbLdapGroup, dbGroupMembers, ldapGroupUsers);

                    groupMembersToAdd = from ldapGroupUser in ldapGroupUsers
                                        let dbUser = dbGroupMembers.FirstOrDefault(
                                            u =>
                                            u.Sid.Equals(ldapGroupUser.Sid))
                                        where dbUser == null
                                        select SearchDbUserBySid(ldapGroupUser.Sid)
                                        into userBySid
                                        where !Equals(userBySid, ASC.Core.Users.Constants.LostUser)
                                        select userBySid;

                    foreach (var userInfo in groupMembersToAdd)
                    {
                        CoreContext.UserManager.AddUserIntoGroup(userInfo.ID, dbLdapGroup.ID);
                    }

                    RemoveDbGroupIfEmpty(dbLdapGroup);
                }

                percentage += step;
                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, Convert.ToInt32(percentage));
            }
        }

        private void SyncLDAPUsers()
        {
            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 15);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusGettingUsersFromLdap);

            var ldapUsers = _importer.GetDiscoveredUsersByAttributes()
                                 .ConvertAll(u =>
                                 {
                                     if (u.FirstName == string.Empty)
                                         u.FirstName = Resource.FirstName;

                                     if (u.LastName == string.Empty)
                                         u.LastName = Resource.LastName;

                                     return u;
                                 })
                                 .SortByUserName()
                                 .ToList();

            if (!ldapUsers.Any())
            {
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorUsersNotFound);
                return;
            }

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 20);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusRemovingOldUsers);

            RemoveOldDbUsers(ldapUsers);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 30);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusSavingUsers);

            SyncDbUsers(ldapUsers);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 70);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusRemovingOldGroups);

            RemoveOldDbGroups(new List<GroupInfo>()); // Remove all db groups with sid
        }

        private void SyncLDAPUsersInGroups()
        {
            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 15);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusGettingGroupsFromLdap);

            var ldapGroups = _importer.GetDiscoveredGroupsByAttributes();

            if (!ldapGroups.Any())
            {
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorGroupsNotFound);
                return;
            }

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 20);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusGettingUsersFromLdap);

            //Get All found groups users
            List<UserInfo> uniqueLdapGroupUsers;

            var ldapGroupsUsers = GetGroupsUsers(ldapGroups, out uniqueLdapGroupUsers);

            uniqueLdapGroupUsers = uniqueLdapGroupUsers
                .ConvertAll(u =>
                {
                    if (u.FirstName == string.Empty)
                        u.FirstName = Resource.FirstName;

                    if (u.LastName == string.Empty)
                        u.LastName = Resource.LastName;

                    return u;
                })
                .SortByUserName()
                .ToList();

            if (!uniqueLdapGroupUsers.Any())
            {
                SetProggressInfo(Constants.LDAP_SETTING_TASK_ERROR, Resource.LdapSettingsErrorUsersNotFound);
                return;
            }

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 30);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusSavingUsers);

            SyncGroupsUsers(uniqueLdapGroupUsers);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 60);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusSavingGroups);

            SyncDbGroups(ldapGroupsUsers);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 80);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusRemovingOldGroups);

            RemoveOldDbGroups(ldapGroups);

            SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, 90);
            SetProggressInfo(Constants.LDAP_SETTING_TASK_STATUS, Resource.LdapSettingsStatusRemovingOldUsers);

            RemoveOldDbUsers(uniqueLdapGroupUsers);
        }

        private void SyncGroupsUsers(List<UserInfo> uniqueLdapGroupUsers)
        {
            const double percents = 30;

            var step = percents / uniqueLdapGroupUsers.Count;

            var percentage = Convert.ToDouble(AscCache.Default.Get<object>(Constants.LDAP_SETTING_TASK_PERCENTAGE) ?? "0");

            int i, len;
            for (i = 0, len = uniqueLdapGroupUsers.Count; i < len; i++)
            {
                var ldapGroupUser = uniqueLdapGroupUsers[i];
                uniqueLdapGroupUsers[i] = UserManagerWrapper.SyncUserLDAP(ldapGroupUser);

                percentage += step;
                SetProggressInfo(Constants.LDAP_SETTING_TASK_PERCENTAGE, Convert.ToInt32(percentage));
            }
        }

        private Dictionary<GroupInfo, List<UserInfo>> GetGroupsUsers(List<GroupInfo> ldapGroups, out List<UserInfo> uniqueLdapGroupUsers)
        {
            uniqueLdapGroupUsers = new List<UserInfo>();

            var listGroupsUsers = new Dictionary<GroupInfo, List<UserInfo>>();

            int i, len;
            for (i = 0, len = ldapGroups.Count; i < len; i++)
            {
                var ldapGroup = ldapGroups[i];

                var ldapGroupUsers = _importer.GetGroupUsers(ldapGroup);

                listGroupsUsers.Add(ldapGroup, ldapGroupUsers);

                List<UserInfo> groupUsers = uniqueLdapGroupUsers;
                var users = ldapGroupUsers.Where(u => !groupUsers.Contains(u)).ToList();
                if(users.Any())
                    uniqueLdapGroupUsers.AddRange(users);
            }

            return listGroupsUsers;
        }


        private void SyncLDAP()
        {
            if (!_settings.GroupMembership)
            {
                SyncLDAPUsers();
            }
            else
            {
                SyncLDAPUsersInGroups();
            }
        }

        public static class Constants
        {
            public const string LDAP_SETTING_TASK_ID = "SaveLdapSettingTaskId";
            public const string LDAP_SETTING_TASK_STATUS = "SaveLdapSettingTaskStatus";
            public const string LDAP_SETTING_TASK_ACCEPT_CERTIFICATE = "SaveLdapSettingTaskAcceptCertificate";
            public const string LDAP_SETTING_TASK_PERCENTAGE = "SaveLdapSettingTaskPercentage";
            public const string LDAP_SETTING_TASK_IS_COMPLETED = "SaveLdapSettingTaskIsCompleted";
            public const string LDAP_SETTING_TASK_ERROR = "SaveLdapSettingTaskError";
            public const string LDAP_SETTING_TASK_STARTED = "SaveLdapSettingTaskStarted";
            public const string NOVELL_LDAP_CERTIFICATE_CONFIRM_REQUEST = "NovellLdapCertificateConfirmRequest";
        }

        private static void SetProggressInfo<T>(string key, T value)
        {
            AscCache.Default.Insert(key, value, CacheExpired);
        }

        #endregion
    }
}