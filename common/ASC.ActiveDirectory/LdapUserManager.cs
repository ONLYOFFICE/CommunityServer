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
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.ActiveDirectory.ComplexOperations.Data;
using ASC.ActiveDirectory.Novell;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Utility;
using log4net;

namespace ASC.ActiveDirectory
{
    public class LdapUserManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(LdapUserManager));
        
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
                if (onlyGetChanges)
                    changes.SetNoneUserChange(ldapUserInfo);

                return userBySid;
            }

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
        private const string MOB_PHONE = "mobphone";
        private const string EXT_MAIL = "extmail";
        private const string MAIL = "mail";

        private static void UpdateLdapUserContacts(UserInfo ldapUser, List<string> portalUserContacts)
        {
            if (!portalUserContacts.Any())
                return;

            var ldapUserContacts = ldapUser.Contacts;

            var newContacts = new List<string>();

            var phones = new List<string>();
            var mails = new List<string>();
            var otherContacts = new List<string>();

            for (int i = 0, n = portalUserContacts.Count; i < n; i += 2)
            {
                if (i + 1 >= portalUserContacts.Count)
                    continue;

                var type = portalUserContacts[i];
                var value = portalUserContacts[i + 1];

                switch (type)
                {
                    case EXT_MOB_PHONE:
                    case EXT_MAIL:
                        break;
                    case MOB_PHONE:
                        phones.Add(value);
                        break;
                    case MAIL:
                        mails.Add(value);
                        break;
                    default:
                        otherContacts.Add(type);
                        otherContacts.Add(value);
                        break;
                }
            }

            string phone;
            string mail;

            for (int i = 0, m = ldapUserContacts.Count; i < m; i += 2)
            {
                if (i + 1 >= ldapUserContacts.Count) 
                    continue;

                switch (ldapUserContacts[i])
                {
                    case EXT_MOB_PHONE:
                        phone = ldapUserContacts[i + 1];

                        if (phones.Exists(p => p.Equals(phone)))
                        {
                            phones.Remove(phone);
                        }

                        newContacts.Add(EXT_MOB_PHONE);
                        newContacts.Add(phone);

                        break;
                    case EXT_MAIL:
                        mail = ldapUserContacts[i + 1];

                        if (mails.Exists(p => p.Equals(mail)))
                        {
                            mails.Remove(mail);
                        }

                        newContacts.Add(EXT_MAIL);
                        newContacts.Add(mail);

                        break;
                    default:
                        continue;
                }
            }

            phones.ForEach(p =>
            {
                newContacts.Add(MOB_PHONE);
                newContacts.Add(p);
            });

            mails.ForEach(m =>
            {
                newContacts.Add(MAIL);
                newContacts.Add(m);
            });

            newContacts.AddRange(otherContacts);

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

                if (notEqual(portalUser.Email, ldapUser.Email))
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

                if (portalUser.ActivationStatus != ldapUser.ActivationStatus)
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

                userToUpdate.UserName = updateInfo.UserName;
                userToUpdate.Email = updateInfo.Email;
                userToUpdate.FirstName = updateInfo.FirstName;
                userToUpdate.LastName = updateInfo.LastName;
                userToUpdate.Sid = updateInfo.Sid;
                userToUpdate.ActivationStatus = updateInfo.ActivationStatus;
                userToUpdate.Contacts = updateInfo.Contacts;
                userToUpdate.Title = updateInfo.Title;
                userToUpdate.Location = updateInfo.Location;

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

            try
            {
                var settings = LdapSettings.Load();

                if (!settings.EnableLdapAuthentication)
                    return false;

                _log.DebugFormat("TryGetAndSyncLdapUserInfo(login: \"{0}\")", login);
                
                using (var importer = new NovellLdapUserImporter(settings, Resource))
                {
                    var ldapUserInfo = importer.Login(login, password);

                    if (ldapUserInfo == null || ldapUserInfo.Item1.Equals(Constants.LostUser))
                    {
                        _log.DebugFormat("NovellLdapUserImporter.Login('{0}') failed.", login);
                        return false;
                    }

                    _log.DebugFormat("TryCheckAndSyncToLdapUser(Username: '{0}', Email: {1}, DN: {2})",
                        ldapUserInfo.Item1.UserName, ldapUserInfo.Item1.Email, ldapUserInfo.Item2.DistinguishedName);

                    if (!TryCheckAndSyncToLdapUser(ldapUserInfo, importer, out userInfo))
                    {
                        _log.Debug("TryCheckAndSyncToLdapUser() failed");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
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
