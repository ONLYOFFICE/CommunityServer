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
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web;

using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.ActiveDirectory.ComplexOperations.Data;
using ASC.ActiveDirectory.Novell;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;

namespace ASC.ActiveDirectory
{
    public class LdapUserManager
    {
        private readonly ILog _log = LogManager.GetLogger("ASC");

        public LdapLocalization Resource { get; private set; }

        public PasswordSettings PasswordSettings { get; private set; }

        public LdapUserManager(LdapLocalization resource = null)
        {
            Resource = resource ?? new LdapLocalization();
        }

        private static bool TestUniqueUserName(string uniqueName)
        {
            return !string.IsNullOrEmpty(uniqueName) && Equals(CoreContext.UserManager.GetUserByUserName(uniqueName), Constants.LostUser);
        }

        private string MakeUniqueName(UserInfo userInfo)
        {
            if (string.IsNullOrEmpty(userInfo.Email))
                throw new ArgumentException(Resource.ErrorEmailEmpty, "userInfo");

            var uniqueName = new MailAddress(userInfo.Email).User;
            var startUniqueName = uniqueName;
            var i = 0;
            while (!TestUniqueUserName(uniqueName))
            {
                uniqueName = string.Format("{0}{1}", startUniqueName, (++i).ToString(CultureInfo.InvariantCulture));
            }
            return uniqueName;
        }

        private static bool CheckUniqueEmail(Guid userId, string email)
        {
            var foundUser = CoreContext.UserManager.GetUserByEmail(email);
            return Equals(foundUser, Constants.LostUser) || foundUser.ID == userId;
        }

        public bool TryAddLDAPUser(UserInfo ldapUserInfo, bool onlyGetChanges, out UserInfo portalUserInfo)
        {
            portalUserInfo = Constants.LostUser;

            try
            {
                if (ldapUserInfo == null)
                    throw new ArgumentNullException("ldapUserInfo");

                _log.DebugFormat("TryAddLDAPUser(SID: {0}): Email '{1}' UserName: {2}", ldapUserInfo.Sid,
                    ldapUserInfo.Email, ldapUserInfo.UserName);

                if (!CheckUniqueEmail(ldapUserInfo.ID, ldapUserInfo.Email))
                {
                    _log.DebugFormat("TryAddLDAPUser(SID: {0}): Email '{1}' already exists.",
                        ldapUserInfo.Sid, ldapUserInfo.Email);

                    return false;
                }

                if (!TryChangeExistingUserName(ldapUserInfo.UserName, onlyGetChanges))
                {
                    _log.DebugFormat("TryAddLDAPUser(SID: {0}): Username '{1}' already exists.",
                        ldapUserInfo.Sid, ldapUserInfo.UserName);

                    return false;
                }

                if (!ldapUserInfo.WorkFromDate.HasValue)
                {
                    ldapUserInfo.WorkFromDate = TenantUtil.DateTimeNow();
                }

                if (onlyGetChanges)
                {
                    portalUserInfo = ldapUserInfo;
                    return true;
                }

                _log.DebugFormat("CoreContext.UserManager.SaveUserInfo({0})", ldapUserInfo.GetUserInfoString());

                portalUserInfo = CoreContext.UserManager.SaveUserInfo(ldapUserInfo);

                if (PasswordSettings == null)
                {
                    _log.DebugFormat("PasswordSettings.Load()");

                    PasswordSettings = PasswordSettings.Load();
                }

                var password = LdapUtils.GeneratePassword(PasswordSettings);

                _log.DebugFormat("SecurityContext.SetUserPassword(ID:{0})", portalUserInfo.ID);

                SecurityContext.SetUserPassword(portalUserInfo.ID, password);

                return true;
            }
            catch (Exception ex)
            {
                if (ldapUserInfo != null)
                    _log.ErrorFormat("TryAddLDAPUser(UserName='{0}' Sid='{1}') failed: Error: {2}", ldapUserInfo.UserName,
                        ldapUserInfo.Sid, ex);
            }

            return false;
        }

        private bool TryChangeExistingUserName(string ldapUserName, bool onlyGetChanges)
        {
            try
            {
                if (string.IsNullOrEmpty(ldapUserName))
                    return false;

                var otherUser = CoreContext.UserManager.GetUserByUserName(ldapUserName);

                if (Equals(otherUser, Constants.LostUser))
                    return true;

                if (otherUser.IsLDAP())
                    return false;

                otherUser.UserName = MakeUniqueName(otherUser);

                if (onlyGetChanges)
                    return true;

                _log.Debug("TryChangeExistingUserName()");

                _log.DebugFormat("CoreContext.UserManager.SaveUserInfo({0})", otherUser.GetUserInfoString());

                CoreContext.UserManager.SaveUserInfo(otherUser);

                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("TryChangeOtherUserName({0}) failed. Error: {1}", ldapUserName, ex);
            }

            return false;
        }

        public UserInfo GetLDAPSyncUserChange(UserInfo ldapUserInfo, List<UserInfo> ldapUsers, out LdapChangeCollection changes)
        {
            return SyncLDAPUser(ldapUserInfo, ldapUsers, out changes, true);
        }

        public UserInfo SyncLDAPUser(UserInfo ldapUserInfo, List<UserInfo> ldapUsers = null)
        {
            LdapChangeCollection changes;
            return SyncLDAPUser(ldapUserInfo, ldapUsers, out changes);
        }

        private UserInfo SyncLDAPUser(UserInfo ldapUserInfo, List<UserInfo> ldapUsers, out LdapChangeCollection changes, bool onlyGetChanges = false)
        {
            UserInfo result;

            changes = new LdapChangeCollection();

            UserInfo userToUpdate;

            var userBySid = CoreContext.UserManager.GetUserBySid(ldapUserInfo.Sid);

            if (Equals(userBySid, Constants.LostUser))
            {
                var userByEmail = CoreContext.UserManager.GetUserByEmail(ldapUserInfo.Email);

                if (Equals(userByEmail, Constants.LostUser))
                {
                    if (ldapUserInfo.Status != EmployeeStatus.Active)
                    {
                        if (onlyGetChanges)
                            changes.SetSkipUserChange(ldapUserInfo);

                        _log.DebugFormat("SyncUserLDAP(SID: {0}, Username: '{1}') ADD failed: Status is {2}",
                            ldapUserInfo.Sid, ldapUserInfo.UserName,
                            Enum.GetName(typeof(EmployeeStatus), ldapUserInfo.Status));

                        return Constants.LostUser;
                    }

                    if (!TryAddLDAPUser(ldapUserInfo, onlyGetChanges, out result))
                    {
                        if (onlyGetChanges)
                            changes.SetSkipUserChange(ldapUserInfo);

                        return Constants.LostUser;
                    }

                    if (onlyGetChanges)
                        changes.SetAddUserChange(result, _log);

                    if (!onlyGetChanges && LdapSettings.Load().SendWelcomeEmail &&
                        (ldapUserInfo.ActivationStatus != EmployeeActivationStatus.AutoGenerated))
                    {
                        var client = LdapNotifyHelper.StudioNotifyClient;

                        var confirmLink = CommonLinkUtility.GetConfirmationUrl(ldapUserInfo.Email, ConfirmType.EmailActivation);

                        client.SendNoticeToAsync(
                            NotifyConstants.ActionLdapActivation,
                            null,
                            new[] { new DirectRecipient(ldapUserInfo.Email, null, new[] { ldapUserInfo.Email }, false) },
                            new[] { ASC.Core.Configuration.Constants.NotifyEMailSenderSysName },
                            null,
                            new TagValue(NotifyConstants.TagUserName, ldapUserInfo.DisplayUserName()),
                            new TagValue(NotifyConstants.TagUserEmail, ldapUserInfo.Email),
                            new TagValue(NotifyConstants.TagMyStaffLink, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetMyStaff())),
                            NotifyConstants.TagGreenButton(Resource.NotifyButtonJoin, confirmLink),
                            new TagValue(NotifyCommonTags.WithoutUnsubscribe, true));
                    }

                    return result;
                }

                if (userByEmail.IsLDAP())
                {
                    if (ldapUsers == null || ldapUsers.Any(u => u.Sid.Equals(userByEmail.Sid)))
                    {
                        if (onlyGetChanges)
                            changes.SetSkipUserChange(ldapUserInfo);

                        _log.DebugFormat(
                            "SyncUserLDAP(SID: {0}, Username: '{1}') ADD failed: Another ldap user with email '{2}' already exists",
                            ldapUserInfo.Sid, ldapUserInfo.UserName, ldapUserInfo.Email);

                        return Constants.LostUser;
                    }
                }

                userToUpdate = userByEmail;
            }
            else
            {
                userToUpdate = userBySid;
            }

            UpdateLdapUserContacts(ldapUserInfo, userToUpdate.Contacts);

            if (!NeedUpdateUser(userToUpdate, ldapUserInfo))
            {
                _log.DebugFormat("SyncUserLDAP(SID: {0}, Username: '{1}') No need to update, skipping", ldapUserInfo.Sid, ldapUserInfo.UserName);
                if (onlyGetChanges)
                    changes.SetNoneUserChange(ldapUserInfo);

                return userBySid;
            }

            _log.DebugFormat("SyncUserLDAP(SID: {0}, Username: '{1}') Userinfo is outdated, updating", ldapUserInfo.Sid, ldapUserInfo.UserName);
            if (!TryUpdateUserWithLDAPInfo(userToUpdate, ldapUserInfo, onlyGetChanges, out result))
            {
                if (onlyGetChanges)
                    changes.SetSkipUserChange(ldapUserInfo);

                return Constants.LostUser;
            }

            if (onlyGetChanges)
                changes.SetUpdateUserChange(ldapUserInfo, result, _log);

            return result;
        }

        private const string EXT_MOB_PHONE = "extmobphone";
        private const string EXT_MAIL = "extmail";
        private const string EXT_PHONE = "extphone";
        private const string EXT_SKYPE = "extskype";

        private static void UpdateLdapUserContacts(UserInfo ldapUser, List<string> portalUserContacts)
        {
            if (!portalUserContacts.Any())
                return;

            var ldapUserContacts = ldapUser.Contacts;

            var newContacts = new List<string>(ldapUser.Contacts);

            for (int i = 0; i < portalUserContacts.Count; i += 2)
            {
                if (portalUserContacts[i] == EXT_MOB_PHONE || portalUserContacts[i] == EXT_MAIL
                    || portalUserContacts[i] == EXT_PHONE || portalUserContacts[i] == EXT_SKYPE)
                    continue;
                if (i + 1 >= portalUserContacts.Count)
                    continue;

                newContacts.Add(portalUserContacts[i]);
                newContacts.Add(portalUserContacts[i + 1]);
            }

            ldapUser.Contacts = newContacts;
        }

        private bool NeedUpdateUser(UserInfo portalUser, UserInfo ldapUser)
        {
            var needUpdate = false;

            try
            {
                Func<string, string, bool> notEqual =
                    (f1, f2) =>
                        f1 == null && f2 != null ||
                        f1 != null && !f1.Equals(f2, StringComparison.InvariantCultureIgnoreCase);

                if (notEqual(portalUser.FirstName, ldapUser.FirstName))
                {
                    _log.DebugFormat("NeedUpdateUser by FirstName -> portal: '{0}', ldap: '{1}'",
                        portalUser.FirstName ?? "NULL",
                        ldapUser.FirstName ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.LastName, ldapUser.LastName))
                {
                    _log.DebugFormat("NeedUpdateUser by LastName -> portal: '{0}', ldap: '{1}'",
                        portalUser.LastName ?? "NULL",
                        ldapUser.LastName ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.UserName, ldapUser.UserName))
                {
                    _log.DebugFormat("NeedUpdateUser by UserName -> portal: '{0}', ldap: '{1}'",
                        portalUser.UserName ?? "NULL",
                        ldapUser.UserName ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.Email, ldapUser.Email) &&
                    (ldapUser.ActivationStatus != EmployeeActivationStatus.AutoGenerated
                        || ldapUser.ActivationStatus == EmployeeActivationStatus.AutoGenerated && portalUser.ActivationStatus== EmployeeActivationStatus.AutoGenerated))
                {
                    _log.DebugFormat("NeedUpdateUser by Email -> portal: '{0}', ldap: '{1}'",
                        portalUser.Email ?? "NULL",
                        ldapUser.Email ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.Sid, ldapUser.Sid))
                {
                    _log.DebugFormat("NeedUpdateUser by Sid -> portal: '{0}', ldap: '{1}'",
                        portalUser.Sid ?? "NULL",
                        ldapUser.Sid ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.Title, ldapUser.Title))
                {
                    _log.DebugFormat("NeedUpdateUser by Title -> portal: '{0}', ldap: '{1}'",
                        portalUser.Title ?? "NULL",
                        ldapUser.Title ?? "NULL");
                    needUpdate = true;
                }

                if (notEqual(portalUser.Location, ldapUser.Location))
                {
                    _log.DebugFormat("NeedUpdateUser by Location -> portal: '{0}', ldap: '{1}'",
                        portalUser.Location ?? "NULL",
                        ldapUser.Location ?? "NULL");
                    needUpdate = true;
                }

                if (portalUser.ActivationStatus != ldapUser.ActivationStatus &&
                    (!portalUser.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated) || portalUser.Email != ldapUser.Email) &&
                    ldapUser.ActivationStatus != EmployeeActivationStatus.AutoGenerated)
                {
                    _log.DebugFormat("NeedUpdateUser by ActivationStatus -> portal: '{0}', ldap: '{1}'",
                        portalUser.ActivationStatus,
                        ldapUser.ActivationStatus);
                    needUpdate = true;
                }

                if (portalUser.Status != ldapUser.Status)
                {
                    _log.DebugFormat("NeedUpdateUser by Status -> portal: '{0}', ldap: '{1}'",
                        portalUser.Status,
                        ldapUser.Status);
                    needUpdate = true;
                }

                if (ldapUser.Contacts.Count != portalUser.Contacts.Count ||
                    !ldapUser.Contacts.All(portalUser.Contacts.Contains))
                {
                    _log.DebugFormat("NeedUpdateUser by Contacts -> portal: '{0}', ldap: '{1}'",
                        string.Join("|", portalUser.Contacts),
                        string.Join("|", ldapUser.Contacts));
                    needUpdate = true;
                }

                if (notEqual(portalUser.MobilePhone, ldapUser.MobilePhone))
                {
                    _log.DebugFormat("NeedUpdateUser by PrimaryPhone -> portal: '{0}', ldap: '{1}'",
                        portalUser.MobilePhone ?? "NULL",
                        ldapUser.MobilePhone ?? "NULL");
                    needUpdate = true;
                }

                if (!portalUser.BirthDate.Equals(ldapUser.BirthDate))
                {
                    _log.DebugFormat("NeedUpdateUser by BirthDate -> portal: '{0}', ldap: '{1}'",
                        portalUser.BirthDate.ToString() ?? "NULL",
                        ldapUser.BirthDate.ToString() ?? "NULL");
                    needUpdate = true;
                }

                if (portalUser.Sex != ldapUser.Sex)
                {
                    _log.DebugFormat("NeedUpdateUser by Sex -> portal: '{0}', ldap: '{1}'",
                        portalUser.Sex.ToString() ?? "NULL",
                        ldapUser.Sex.ToString() ?? "NULL");
                    needUpdate = true;
                }
            }
            catch (Exception ex)
            {
                _log.DebugFormat("NeedUpdateUser failed: error: {0}", ex);
            }

            return needUpdate;
        }

        private bool TryUpdateUserWithLDAPInfo(UserInfo userToUpdate, UserInfo updateInfo, bool onlyGetChanges, out UserInfo portlaUserInfo)
        {
            portlaUserInfo = Constants.LostUser;

            try
            {
                _log.Debug("TryUpdateUserWithLDAPInfo()");

                if (!userToUpdate.UserName.Equals(updateInfo.UserName, StringComparison.InvariantCultureIgnoreCase)
                    && !TryChangeExistingUserName(updateInfo.UserName, onlyGetChanges))
                {
                    _log.DebugFormat(
                        "UpdateUserWithLDAPInfo(ID: {0}): New username already exists. (Old: '{1})' New: '{2}'",
                        userToUpdate.ID, userToUpdate.UserName, updateInfo.UserName);

                    return false;
                }

                if (!userToUpdate.Email.Equals(updateInfo.Email, StringComparison.InvariantCultureIgnoreCase)
                    && !CheckUniqueEmail(userToUpdate.ID, updateInfo.Email))
                {
                    _log.DebugFormat(
                        "UpdateUserWithLDAPInfo(ID: {0}): New email already exists. (Old: '{1})' New: '{2}'",
                        userToUpdate.ID, userToUpdate.Email, updateInfo.Email);

                    return false;
                }

                if (!(updateInfo.ActivationStatus == EmployeeActivationStatus.AutoGenerated &&
                    userToUpdate.ActivationStatus == (EmployeeActivationStatus.AutoGenerated | EmployeeActivationStatus.Activated))) {

                    userToUpdate.ActivationStatus = updateInfo.ActivationStatus;
                    userToUpdate.Email = updateInfo.Email;
                }

                userToUpdate.UserName = updateInfo.UserName;
                userToUpdate.FirstName = updateInfo.FirstName;
                userToUpdate.LastName = updateInfo.LastName;
                userToUpdate.Sid = updateInfo.Sid;
                userToUpdate.Contacts = updateInfo.Contacts;
                userToUpdate.Title = updateInfo.Title;
                userToUpdate.Location = updateInfo.Location;
                userToUpdate.Sex = updateInfo.Sex;
                userToUpdate.BirthDate = updateInfo.BirthDate;
                userToUpdate.MobilePhone = updateInfo.MobilePhone;

                if (!userToUpdate.IsOwner()) // Owner must never be terminated by LDAP!
                {
                    userToUpdate.Status = updateInfo.Status;
                }

                if (!onlyGetChanges)
                {
                    _log.DebugFormat("CoreContext.UserManager.SaveUserInfo({0})", userToUpdate.GetUserInfoString());

                    portlaUserInfo = CoreContext.UserManager.SaveUserInfo(userToUpdate);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("UpdateUserWithLDAPInfo(Id='{0}' UserName='{1}' Sid='{2}') failed: Error: {3}",
                    userToUpdate.ID, userToUpdate.UserName,
                    userToUpdate.Sid, ex);
            }

            return false;
        }

        public bool TryGetAndSyncLdapUserInfo(string login, string password, out UserInfo userInfo)
        {
            userInfo = Constants.LostUser;

            NovellLdapUserImporter importer = null;

            try
            {
                var settings = LdapSettings.Load();

                if (!settings.EnableLdapAuthentication)
                    return false;

                _log.DebugFormat("TryGetAndSyncLdapUserInfo(login: \"{0}\")", login);

                importer = new NovellLdapUserImporter(settings, Resource);

                var ldapUserInfo = importer.Login(login, password);

                if (ldapUserInfo == null || ldapUserInfo.Item1.Equals(Constants.LostUser))
                {
                    _log.DebugFormat("NovellLdapUserImporter.Login('{0}') failed.", login);
                    return false;
                }

                var portalUser = CoreContext.UserManager.GetUserBySid(ldapUserInfo.Item1.Sid);

                if (portalUser.Status == EmployeeStatus.Terminated || portalUser.Equals(Constants.LostUser))
                {
                    if (!ldapUserInfo.Item2.IsDisabled)
                    {
                        _log.DebugFormat("TryCheckAndSyncToLdapUser(Username: '{0}', Email: {1}, DN: {2})",
                            ldapUserInfo.Item1.UserName, ldapUserInfo.Item1.Email, ldapUserInfo.Item2.DistinguishedName);

                        if (!TryCheckAndSyncToLdapUser(ldapUserInfo, importer, out userInfo))
                        {
                            importer.Dispose();
                            _log.Debug("TryCheckAndSyncToLdapUser() failed");
                            return false;
                        }
                        importer.Dispose();
                    }
                    else
                    {
                        importer.Dispose();
                        return false;
                    }
                }
                else
                {
                    _log.DebugFormat("TryCheckAndSyncToLdapUser(Username: '{0}', Email: {1}, DN: {2})",
                        ldapUserInfo.Item1.UserName, ldapUserInfo.Item1.Email, ldapUserInfo.Item2.DistinguishedName);

                    var tenant = CoreContext.TenantManager.GetCurrentTenant();

                    new System.Threading.Tasks.Task(() =>
                    {
                        try
                        {
                            CoreContext.TenantManager.SetCurrentTenant(tenant);
                            SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

                            var uInfo = SyncLDAPUser(ldapUserInfo.Item1);

                            var newLdapUserInfo = new Tuple<UserInfo, LdapObject>(uInfo, ldapUserInfo.Item2);

                            if (importer.Settings.GroupMembership)
                            {
                                importer.TrySyncUserGroupMembership(newLdapUserInfo);
                            }
                        }
                        finally
                        {
                            importer.Dispose();
                        }
                    }).Start();

                    if (ldapUserInfo.Item2.IsDisabled)
                    {
                        _log.DebugFormat("TryGetAndSyncLdapUserInfo(login: \"{0}\") failed, user is disabled in ldap", login);
                        return false;
                    }
                    else
                    {
                        userInfo = portalUser;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (importer != null)
                {
                    importer.Dispose();
                }

                _log.ErrorFormat("TryGetLdapUserInfo(login: '{0}') failed. Error: {1}", login, ex);
                userInfo = Constants.LostUser;
                return false;
            }
        }

        private bool TryCheckAndSyncToLdapUser(Tuple<UserInfo, LdapObject> ldapUserInfo, LdapUserImporter importer,
            out UserInfo userInfo)
        {
            try
            {
                SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

                userInfo = SyncLDAPUser(ldapUserInfo.Item1);

                if (userInfo == null || userInfo.Equals(Constants.LostUser))
                {
                    throw new Exception("The user did not pass the configuration check by ldap user settings");
                }

                var newLdapUserInfo = new Tuple<UserInfo, LdapObject>(userInfo, ldapUserInfo.Item2);

                if (!importer.Settings.GroupMembership)
                {
                    return true;
                }

                if (!importer.TrySyncUserGroupMembership(newLdapUserInfo))
                {
                    throw new Exception("The user did not pass the configuration check by ldap group settings");
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("TrySyncLdapUser(SID: '{0}', Email: {1}) failed. Error: {2}", ldapUserInfo.Item1.Sid,
                    ldapUserInfo.Item1.Email, ex);
            }
            finally
            {
                SecurityContext.Logout();
            }

            userInfo = Constants.LostUser;
            return false;
        }
    }
}
