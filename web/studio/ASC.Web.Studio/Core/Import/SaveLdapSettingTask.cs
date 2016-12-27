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
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace ASC.Web.Studio.Core.Import
{
    public class SaveLdapSettingTask : IProgressItem
    {
        private const int WAITING_TIMEOUT = 600;
        private string _serializeSettings;
        private readonly ILog log = LogManager.GetLogger(typeof(SaveLdapSettingTask));
        private readonly LDAPUserImporter importer = new LDAPUserImporter();
        private readonly LdapSettingsChecker ldapSettingsChecker;

        public object Id { get; set; }

        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }

        public SaveLdapSettingTask(string serializeSettings, int tenantId, string status, bool acceptCertificate = false)
        {
            _serializeSettings = serializeSettings;
            Id = tenantId;
            AscCache.Default.Insert("SaveLdapSettingTaskId", Id, TimeSpan.FromMinutes(15));
            AscCache.Default.Insert("SaveLdapSettingTaskStatus", status, TimeSpan.FromMinutes(15));
            AscCache.Default.Insert("SaveLdapSettingTaskAcceptCertificate", acceptCertificate, TimeSpan.FromMinutes(15));
            ldapSettingsChecker = !WorkContext.IsMono ?
                (LdapSettingsChecker)new SystemLdapSettingsChecker() : new NovellLdapSettingsChecker();
        }

        public void RunJob()
        {
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                var tenantId = (int)Id;
                CoreContext.TenantManager.SetCurrentTenant(tenantId);
                ResolveCulture();
                AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 1, TimeSpan.FromMinutes(15));
                AscCache.Default.Insert("SaveLdapSettingTaskStatus",
                    Resource.LdapSettingsStatusCheckingLdapSettings, TimeSpan.FromMinutes(15));
                AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", false, TimeSpan.FromMinutes(15));

                if (_serializeSettings == null)
                {
                    log.Error("Can't save default LDAP settings.");
                    AscCache.Default.Insert("SaveLdapSettingTaskError",
                        Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                    AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                    return;
                }
                try
                {
                    var settings = JsonConvert.DeserializeObject<LDAPSupportSettings>(_serializeSettings);
                    PrepareSettings(settings);
                    bool acceptCertificate = (bool?)AscCache.Default.Get<object>("SaveLdapSettingTaskAcceptCertificate") ?? false;
                    byte result = ldapSettingsChecker.CheckSettings(settings, importer, acceptCertificate);
                    if (result == LdapSettingsChecker.CERTIFICATE_REQUEST)
                    {
                        AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 0, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskStatus",
                            Resource.LdapSettingsStatusCertificateVerification, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("NovellLdapCertificateConfirmRequest",
                            ((NovellLdapSettingsChecker)ldapSettingsChecker).CertificateConfirmRequest, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                    var error = GetError(result);
                    if (error == string.Empty)
                    {
                        AscCache.Default.Insert("SaveLdapSettingTaskStatus",
                            Resource.LdapSettingsStatusSavingSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 10, TimeSpan.FromMinutes(15));
                        if (!SettingsManager.Instance.SaveSettings<LDAPSupportSettings>(settings, tenantId))
                        {
                            log.Error("Can't save LDAP settings.");
                            AscCache.Default.Insert("SaveLdapSettingTaskError",
                                Resource.LdapSettingsErrorCantSaveLdapSettings, TimeSpan.FromMinutes(15));
                            AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                            return;
                        }
                        // for logout old ldap users
                        AddLToSids();

                        if (settings.EnableLdapAuthentication)
                        {
                            var wrongUser = CreateUsersAndGroups(settings);
                            if (wrongUser != null)
                            {
                                log.ErrorFormat("FormatException error. wrongUser.Email = {0}, wrongUser.FirstName = {1}, wrongUser.LastName = {2}",
                                    wrongUser.Email, wrongUser.FirstName, wrongUser.LastName);
                                AscCache.Default.Insert("SaveLdapSettingTaskError",
                                    String.Format(Resource.LdapSettingsWrongEmail, wrongUser.Email, wrongUser.FirstName ?? String.Empty,
                                    wrongUser.LastName ?? String.Empty), TimeSpan.FromMinutes(15));
                                AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                                return;
                            }
                        }
                        AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 100, TimeSpan.FromMinutes(15));
                    }
                    else
                    {
                        AscCache.Default.Insert("SaveLdapSettingTaskError", error, TimeSpan.FromMinutes(15));
                    }
                }
                catch (GroupsNotFoundException e)
                {
                    log.ErrorFormat("GroupsNotFoundException. {0}", e);
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsGroupsNotFound, TimeSpan.FromMinutes(15));
                }
                catch (TenantQuotaException e)
                {
                    log.ErrorFormat("TenantQuotaException. {0}", e);
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsTenantQuotaSettled, TimeSpan.FromMinutes(15));
                }
                catch (FormatException e)
                {
                    log.ErrorFormat("FormatException error. {0}", e);
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantCreateUsers, TimeSpan.FromMinutes(15));
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Internal server error. {0}", e);
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsInternalServerError, TimeSpan.FromMinutes(15));
                }
            }
            finally
            {
                AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                AscCache.Default.Remove("SaveLdapSettingTaskStarted");
                SecurityContext.Logout();
            }
        }

        private void PrepareSettings(LDAPSupportSettings settings)
        {
            if (settings == null)
            {
                log.Error("Wrong LDAP settings were received from client.");
                AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                return;
            }
            if (settings.EnableLdapAuthentication)
            {
                if (!string.IsNullOrWhiteSpace(settings.Server))
                {
                    settings.Server = settings.Server.Trim();
                }
                else
                {
                    log.Error("settings.Server is null or empty.");
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                    AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                    return;
                }
                if (!settings.Server.StartsWith("LDAP://"))
                {
                    settings.Server = "LDAP://" + settings.Server;
                }
                if (!string.IsNullOrWhiteSpace(settings.UserDN))
                {
                    settings.UserDN = settings.UserDN.Trim();
                }
                else
                {
                    log.Error("settings.UserDN is null or empty.");
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                    AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(settings.LoginAttribute))
                {
                    settings.LoginAttribute = settings.LoginAttribute.Trim();
                }
                else
                {
                    log.Error("settings.LoginAttribute is null or empty.");
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                    AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(settings.UserFilter))
                {
                    settings.UserFilter = settings.UserFilter.Trim();
                }
                else
                {
                    log.Error("settings.UserFilter is null or empty.");
                    AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                    AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(settings.FirstNameAttribute))
                {
                    settings.FirstNameAttribute = settings.FirstNameAttribute.Trim();
                }
                if (!string.IsNullOrWhiteSpace(settings.SecondNameAttribute))
                {
                    settings.SecondNameAttribute = settings.SecondNameAttribute.Trim();
                }
                if (!string.IsNullOrWhiteSpace(settings.MailAttribute))
                {
                    settings.MailAttribute = settings.MailAttribute.Trim();
                }
                if (!string.IsNullOrWhiteSpace(settings.TitleAttribute))
                {
                    settings.TitleAttribute = settings.TitleAttribute.Trim();
                }
                if (!string.IsNullOrWhiteSpace(settings.MobilePhoneAttribute))
                {
                    settings.MobilePhoneAttribute = settings.MobilePhoneAttribute.Trim();
                }
                if (settings.GroupMembership)
                {
                    if (!string.IsNullOrWhiteSpace(settings.GroupDN))
                    {
                        settings.GroupDN = settings.GroupDN.Trim();
                    }
                    else
                    {
                        log.Error("settings.GroupDN is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(settings.GroupFilter))
                    {
                        settings.GroupFilter = settings.GroupFilter.Trim();
                    }
                    else
                    {
                        log.Error("settings.GroupFilter is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(settings.GroupAttribute))
                    {
                        settings.GroupAttribute = settings.GroupAttribute.Trim();
                    }
                    else
                    {
                        log.Error("settings.GroupAttribute is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(settings.UserAttribute))
                    {
                        settings.UserAttribute = settings.UserAttribute.Trim();
                    }
                    else
                    {
                        log.Error("settings.UserAttribute is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                }
                if (WorkContext.IsMono)
                {
                    settings.Authentication = true;
                }
                if (settings.Authentication)
                {
                    if (!string.IsNullOrWhiteSpace(settings.Login))
                    {
                        settings.Login = settings.Login.Trim();
                    }
                    else
                    {
                        log.Error("settings.Login is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                    if (!string.IsNullOrEmpty(settings.Password))
                    {
                        settings.PasswordBytes = InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(settings.Password));
                    }
                    else
                    {
                        log.Error("settings.Password is null or empty.");
                        AscCache.Default.Insert("SaveLdapSettingTaskError", Resource.LdapSettingsErrorCantGetLdapSettings, TimeSpan.FromMinutes(15));
                        AscCache.Default.Insert("SaveLdapSettingTaskIsCompleted", true, TimeSpan.FromMinutes(15));
                        return;
                    }
                }
            }
            settings.Password = string.Empty;
        }

        private void ResolveCulture()
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

        private string GetError(byte result)
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

        private UserInfo CreateUsersAndGroups(LDAPSupportSettings settings)
        {
            AscCache.Default.Insert("SaveLdapSettingTaskStatus", Resource.LdapSettingsStatusGettingGroupsFromLdap, TimeSpan.FromMinutes(15));
            List<GroupInfo> existingGroups;
            var groups = importer.GetDiscoveredGroupsByAttributes(settings, out existingGroups);
            if (groups != null && groups.Count == 0 && existingGroups.Count == 0)
            {
                log.Error("Not found any group which would contain users. groups.Count == 0.");
                throw new GroupsNotFoundException();
            }
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 15, TimeSpan.FromMinutes(15));
            AscCache.Default.Insert("SaveLdapSettingTaskStatus", Resource.LdapSettingsStatusGettingUsersFromLdap, TimeSpan.FromMinutes(15));
            var users = importer.GetDiscoveredUsersByAttributes(settings);
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 20, TimeSpan.FromMinutes(15));
            AscCache.Default.Insert("SaveLdapSettingTaskStatus", Resource.LdapSettingsStatusSavingGroups, TimeSpan.FromMinutes(15));
            AddGroupsToCore(groups);
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 40, TimeSpan.FromMinutes(15));
            AscCache.Default.Insert("SaveLdapSettingTaskStatus", Resource.LdapSettingsStatusSavingUsers, TimeSpan.FromMinutes(15));
            double percents = 35;
            var step = percents / users.Count;
            double percentage = Convert.ToDouble(AscCache.Default.Get<string>("SaveLdapSettingTaskPercentage") ?? "0");
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
                    if (!CheckEmail(users[i].Email))
                    {
                        return users[i];
                    }
                }
                for (int i = 0; i < users.Count; i++)
                {
                    importer.CheckEmailIsNew(users[i]);
                    if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                    {
                        users[i] = UserManagerWrapper.AddUser(users[i], UserManagerWrapper.GeneratePassword(), true, false);
                    }
                    else
                    {
                        users[i] = UserManagerWrapper.AddUser(users[i], UserManagerWrapper.GeneratePassword(), true, false, true);
                    }
                    percentage += step;
                    AscCache.Default.Insert("SaveLdapSettingTaskPercentage", Convert.ToInt32(percentage), TimeSpan.FromMinutes(15));
                }
            }
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 75, TimeSpan.FromMinutes(15));
            var allLdapUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToArray();
            percents = 15;
            step = percents / allLdapUsers.Length;
            percentage = Convert.ToDouble(AscCache.Default.Get<string>("SaveLdapSettingTaskPercentage") ?? "0");
            for (int i = 0; i < allLdapUsers.Length; i++)
            {
                importer.AddUserIntoGroups(allLdapUsers[i], settings);
                percentage += step;
                AscCache.Default.Insert("SaveLdapSettingTaskPercentage", Convert.ToInt32(percentage), TimeSpan.FromMinutes(15));
            }
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 90, TimeSpan.FromMinutes(15));
            AddUsersInCacheGroups();
            RemoveEmptyGroups();
            return null;
        }

        private bool CheckEmail(string email)
        {
            try
            {
                new MailAddress(email);
                return true;
            }
            catch
            {
                log.ErrorFormat("Wrong email format: {0}", email);
                return false;
            }
        }

        private void RemoveEmptyGroups()
        {
            foreach (var group in CoreContext.UserManager.GetGroups().Where(g => !string.IsNullOrEmpty(g.Sid)).ToList())
            {
                if (CoreContext.UserManager.GetUsersByGroup(group.ID).Length == 0)
                {
                    CoreContext.UserManager.DeleteGroup(group.ID);
                }
            }
        }

        private void AddUsersInCacheGroups()
        {
            var cache = importer.GetCache();
            if (cache != null && cache.Count != 0)
            {
                double percents = 10;
                double step = percents / cache.Count;
                double percentage = Convert.ToDouble(AscCache.Default.Get<string>("SaveLdapSettingTaskPercentage") ?? "0");
                foreach (var pair in cache)
                {
                    var childDomainGroup = GetLDAPGroup(pair.Key);
                    if (childDomainGroup != null)
                    {
                        string childDomainGroupSid = childDomainGroup.Sid;
                        var childGroup = CoreContext.UserManager.GetGroupInfoBySid(childDomainGroupSid);
                        if (childGroup != ASC.Core.Users.Constants.LostGroupInfo)
                        {
                            foreach (var user in CoreContext.UserManager.GetUsersByGroup(childGroup.ID))
                            {
                                foreach (var sid in pair.Value)
                                {
                                    var group = CoreContext.UserManager.GetGroupInfoBySid(sid);
                                    if (group != ASC.Core.Users.Constants.LostGroupInfo)
                                    {
                                        CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                                    }
                                }
                            }
                        }
                    }
                    percentage += step;
                    AscCache.Default.Insert("SaveLdapSettingTaskPercentage", Convert.ToInt32(percentage), TimeSpan.FromMinutes(15));
                }
            }
        }

        private LDAPObject GetLDAPGroup(string distinguishedName)
        {
            if (importer.DomainGroups != null)
            {
                for (int i = 0; i < importer.DomainGroups.Count; i++)
                {
                    if (importer.DomainGroups[i].DistinguishedName.Equals(
                        distinguishedName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return importer.DomainGroups[i];
                    }
                }
            }
            return null;
        }

        private void AddGroupsToCore(List<GroupInfo> groups)
        {
            var percents = 30;
            double percentage = Convert.ToDouble(AscCache.Default.Get<string>("SaveLdapSettingTaskPercentage") ?? "0");
            if (groups != null && groups.Count != 0)
            {
                var step = percents / groups.Count;
                foreach (var group in groups)
                {
                    if (CoreContext.UserManager.GetGroupInfoBySid(group.Sid).ID == ASC.Core.Users.Constants.LostGroupInfo.ID)
                    {
                        CoreContext.UserManager.SaveGroupInfo(group);
                    }
                    percentage += step;
                    AscCache.Default.Insert("SaveLdapSettingTaskPercentage", Convert.ToInt32(percentage), TimeSpan.FromMinutes(15));
                }
            }
            AscCache.Default.Insert("SaveLdapSettingTaskPercentage", 45, TimeSpan.FromMinutes(15));
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private class GroupsNotFoundException : Exception
        {
        }
    }
}