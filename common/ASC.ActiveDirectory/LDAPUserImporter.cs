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


﻿using ASC.ActiveDirectory.BuiltIn;
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Novell;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;
using System;
using System.Collections.Generic;
﻿using System.IO;
﻿using System.Linq;
﻿using System.Runtime.Serialization.Formatters.Binary;
﻿using ASC.ActiveDirectory.DirectoryServices.LDAP;

namespace ASC.ActiveDirectory
{
    public class LDAPUserImporter
    {
        private const int MAX_NUMBER_OF_SYMBOLS = 64;
        private const string MOB_PHONE = "mobphone";
        private const string MAIL = "mail";

        public List<LDAPObject> AllDomainUsers { get; private set; }
        public List<LDAPObject> AllDomainGroups { get; private set; }
        public string LDAPDomain { get; private set; }

        public LDAPSupportSettings Settings
        {
            get { return _settings; }
        }

        private readonly LDAPSupportSettings _settings;

        private readonly LdapHelper _ldapHelper;
        private readonly ILog _log = LogManager.GetLogger(typeof(LDAPUserImporter));

        public LDAPUserImporter(LDAPSupportSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _ldapHelper = !WorkContext.IsMono ? (LdapHelper) new SystemLdapHelper() : new NovellLdapHelper();

            _settings = DeepClone(settings);

            if (_settings.EnableLdapAuthentication)
            {
                if (string.IsNullOrEmpty(_settings.UserFilter) ||
                    _settings.GroupMembership && string.IsNullOrEmpty(_settings.GroupFilter))
                {
                    var defaultSettings = settings.GetDefault() as LDAPSupportSettings;

                    if (defaultSettings != null)
                    {
                        if (string.IsNullOrEmpty(_settings.UserFilter))
                        {
                            _settings.UserFilter = defaultSettings.UserFilter;
                        }

                        if (_settings.GroupMembership && string.IsNullOrEmpty(_settings.GroupFilter))
                        {
                            _settings.GroupFilter = defaultSettings.GroupFilter;
                        }
                    }
                }
            }

            AllDomainUsers = new List<LDAPObject>();
            AllDomainGroups = new List<LDAPObject>();
        }

        private static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public string GetSidOfCurrentUser(string login)
        {
            // if login with domain
            var loginLocalPart = login;

            if (login.Contains("\\"))
            {
                var splited = login.Split('\\');
                loginLocalPart = splited[1];
            }
            else if (login.Contains("@"))
            {
                var splited = login.Split('@');
                loginLocalPart = splited[0];
            }

            var users = _ldapHelper
                .GetUsersByAttributesAndFilter(
                    _settings,
                    string.Format("({0}={1})",
                                  _settings.LoginAttribute,
                                  loginLocalPart));

            return users.Any()
                       ? (from user in users
                          where user != null
                          select user.Sid)
                             .FirstOrDefault()
                       : null;
        }

        public List<UserInfo> GetDiscoveredUsersByAttributes()
        {
            var users = new List<UserInfo>();

            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return users;

            var usersToAdd = AllDomainUsers.Select(CreateUserInfo);

            users.AddRange(usersToAdd);

            return users;
        }

        public List<GroupInfo> GetDiscoveredGroupsByAttributes()
        {
            if (!_settings.GroupMembership)
                return new List<GroupInfo>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return new List<GroupInfo>();

            var groups = new List<GroupInfo>();

            var groupsToAdd = from g in AllDomainGroups
                              select new GroupInfo
                                  {
                                      Name = g.InvokeGet(_settings.GroupNameAttribute) as string,
                                      Sid = g.Sid
                                  };

            groups.AddRange(groupsToAdd);

            return groups;
        }

        public UserInfo GetDiscoveredUser(string sid)
        {
            var domainUser = _ldapHelper.GetUserBySid(_settings, sid);

            if (domainUser == null)
                return Core.Users.Constants.LostUser;

            var userInfo = CreateUserInfo(domainUser);
            return userInfo;
        }

        public List<UserInfo> GetGroupUsers(GroupInfo groupInfo)
        {
            var users = new List<UserInfo>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return users;

            var domainGroup = AllDomainGroups.FirstOrDefault(lg => lg.Sid.Equals(groupInfo.Sid));

            if (domainGroup == null)
                return users;

            if (domainGroup.Sid.EndsWith("-513"))
            {
                // Domain Users found

               //var ldapUsers = _ldapHelper.GetUsersByAttributesAndFilter(_settings, "(&(objectCategory=person)(objectClass=user)(primaryGroupID=513))");

                var ldapUsers = _ldapHelper.GetUsersFromPrimaryGroup(_settings, "513");

               if (ldapUsers == null)
                   return users;

                foreach (var ldapUser in ldapUsers)
                {
                    var userInfo = CreateUserInfo(ldapUser);

                    if (!users.Exists(u => u.Sid == userInfo.Sid))
                        users.Add(userInfo);
                }
            }
            else
            {
                var members = _ldapHelper.GetGroupAttribute(domainGroup, _settings.GroupAttribute);

                if (members == null)
                    return users;

                foreach (var member in members)
                {
                    var ldapUser = FindUserByMember(member);

                    if (ldapUser != null)
                    {
                        var userInfo = CreateUserInfo(ldapUser);

                        if (!users.Exists(u => u.Sid == userInfo.Sid))
                            users.Add(userInfo);
                    }
                }                
            }

            return users;
        }

        public bool IsUserExistsInGroups(UserInfo ldapUser)
        {
            try
            {
                if (!_settings.GroupMembership)
                    return false;

                if (ldapUser == null ||
                    Equals(ldapUser, Core.Users.Constants.LostUser) ||
                    string.IsNullOrEmpty(ldapUser.Sid))
                {
                    return false;
                }

                if(!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                    return false;

                var domainUser = _ldapHelper.GetUserBySid(_settings, ldapUser.Sid);

                if (domainUser == null)
                    return false;

                var distinguishedName = _ldapHelper.GetUserAttribute(domainUser, _settings.UserAttribute);

                foreach (var domainGroup in AllDomainGroups)
                {
                    if (_ldapHelper.UserExistsInGroup(domainGroup, distinguishedName, _settings.GroupAttribute))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ldapUser != null)
                    _log.ErrorFormat("IsUserExistInGroups(login: '{0}' sid: '{1}') error {2}", ldapUser.UserName, ldapUser.Sid, ex);
            }

            return false;
        }

        public void SyncUserGroupMembership(UserInfo user)
        {
            if (user == null ||
                !_settings.GroupMembership ||
                AllDomainGroups == null ||
                !AllDomainGroups.Any() && !TryLoadLDAPGroups() ||
                !AllDomainUsers.Any() && !TryLoadLDAPUsers())
            {
                return;
            }

            var domainUser = AllDomainUsers.FirstOrDefault(u => u.Sid.Equals(user.Sid));
            if (domainUser == null)
            {
                return;
            }

            var userAttributeValue = _ldapHelper.GetUserAttribute(domainUser, _settings.UserAttribute);

            foreach (var domainGroup in AllDomainGroups)
            {
                var sid = domainGroup.Sid;

                var members = _ldapHelper.GetGroupAttribute(domainGroup, _settings.GroupAttribute);

                if (members == null)
                    continue;

                foreach (var member in members)
                {
                    var ldapUser = FindUserByMember(member);

                    if (ldapUser == null) 
                        continue;

                    if (!userAttributeValue.Equals(member, StringComparison.InvariantCultureIgnoreCase)) 
                        continue;

                    var groupInfo = CoreContext.UserManager.GetGroupInfoBySid(sid);

                    if (!Equals(groupInfo, Core.Users.Constants.LostGroupInfo))
                    {
                        CoreContext.UserManager.AddUserIntoGroup(user.ID, groupInfo.ID);
                    }
                }
            }

            var primaryGroup = AllDomainGroups.FirstOrDefault(g => g.Sid.EndsWith("-513"));

            if (primaryGroup == null)
                return;

            var getPrimaryGroup = CoreContext.UserManager.GetGroupInfoBySid(primaryGroup.Sid);

            if (!Equals(getPrimaryGroup, Core.Users.Constants.LostGroupInfo))
            {
                CoreContext.UserManager.AddUserIntoGroup(user.ID, getPrimaryGroup.ID);
            }
        }

        public bool TryLoadLDAPUsers()
        {
            try
            {
                if (!_settings.EnableLdapAuthentication)
                    return false;

                AllDomainUsers = _ldapHelper.GetUsersByAttributes(_settings) ?? new List<LDAPObject>();

                return true;
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("Incorrect filter. userFilter = {0}", _settings.UserFilter);
            }

            return false;
        }

        public bool TryLoadLDAPGroups()
        {
            try
            {
                if (!_settings.EnableLdapAuthentication || !_settings.GroupMembership)
                    return false;

                AllDomainGroups = _ldapHelper.GetGroupsByAttributes(_settings) ?? new List<LDAPObject>();

                return true;
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("Incorrect group filter. groupFilter = {0}", _settings.GroupFilter);
            }

            return false;
        }

        public bool TryLoadLDAPDomain()
        {
            try
            {
                if (!_settings.EnableLdapAuthentication)
                    return false;

                var domain = _ldapHelper.GetDomain(_settings);

                if (domain == null || string.IsNullOrEmpty(domain.DistinguishedName))
                    return false;

                LDAPDomain = domain.DistinguishedName.Remove(0, 3).Replace(",DC=", ".").Replace(",dc=", ".");

                return true;
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("Incorrect filter. domainFilter = {0}", _settings.UserFilter);
            }

            return false;
        }

        private LDAPObject FindUserByMember(string userAttributeValue)
        {
            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return null;

            return AllDomainUsers.FirstOrDefault(u =>
                                                  Convert.ToString(u.InvokeGet(_settings.UserAttribute))
                                                         .Equals(userAttributeValue,
                                                                 StringComparison.InvariantCultureIgnoreCase));
        }

        public UserInfo CreateUserInfo(LDAPObject domainUser)
        {
            var userName = GetAttributeFromUser(domainUser, _settings.LoginAttribute);
            var firstName = GetAttributeFromUser(domainUser, _settings.FirstNameAttribute);
            var secondName = GetAttributeFromUser(domainUser, _settings.SecondNameAttribute);
            var mail = GetAttributeFromUser(domainUser, _settings.MailAttribute);
            var mobilePhone = GetAttributeFromUser(domainUser, _settings.MobilePhoneAttribute);
            var title = GetAttributeFromUser(domainUser, _settings.TitleAttribute);
            var location = GetAttributeFromUser(domainUser, _settings.LocationAttribute);
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(mobilePhone))
            {
                contacts.Add(MOB_PHONE);
                contacts.Add(mobilePhone);
            }

            if (!string.IsNullOrEmpty(mail))
            {
                contacts.Add(MAIL);
                contacts.Add(mail);
            }

            var user = new UserInfo
                {
                    ID = Guid.NewGuid(),
                    UserName = userName,
                    Sid = domainUser.Sid,
                    ActivationStatus = EmployeeActivationStatus.Activated,
                    Status = domainUser.IsDisabled ? EmployeeStatus.Terminated : EmployeeStatus.Active,
                    Title = (!string.IsNullOrEmpty(title) ? title : string.Empty),
                    Location = (!string.IsNullOrEmpty(location) ? location : string.Empty),
                    WorkFromDate = TenantUtil.DateTimeNow(),
                    Contacts = contacts
                };

            if (!string.IsNullOrEmpty(firstName))
            {
                user.FirstName = firstName.Length > MAX_NUMBER_OF_SYMBOLS
                                     ? firstName.Substring(0, MAX_NUMBER_OF_SYMBOLS)
                                     : firstName;
            }
            else
            {
                user.FirstName = string.Empty;
            }

            if (!string.IsNullOrEmpty(secondName))
            {
                user.LastName = secondName.Length > MAX_NUMBER_OF_SYMBOLS
                                    ? secondName.Substring(0, MAX_NUMBER_OF_SYMBOLS)
                                    : secondName;
            }
            else
            {
                user.LastName = string.Empty;
            }

            if(string.IsNullOrEmpty(LDAPDomain) && !TryLoadLDAPDomain())
                throw new Exception("LDAP domain not found");

            var email = userName.Contains("@") ? userName : userName + "@" + LDAPDomain;

            user.Email = email;

            return user;
        }

        private string GetAttributeFromUser(LDAPObject domainUser, string attribute)
        {
            if (String.IsNullOrEmpty(attribute))
            {
                return String.Empty;
            }
            try
            {
                return domainUser.InvokeGet(attribute) as string;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can't get attribute from user: attr = {0}, dn = {1}, {2}",
                                 attribute, domainUser.DistinguishedName, e);
                return String.Empty;
            }
        }

        public List<KeyValuePair<UserInfo, LDAPObject>> FindLdapUsers(LDAPLogin ldapLogin)
        {
            var settings = Settings;

            var listResults = new List<KeyValuePair<UserInfo, LDAPObject>>();

            var ldapHelper = WorkContext.IsMono
                        ? new NovellLdapHelper()
                        : new SystemLdapHelper() as LdapHelper;

            var fullLogin = ldapLogin.ToString();

            var searchTerm = ldapLogin.Username.Equals(fullLogin)
                ? string.Format("({0}={1})", settings.LoginAttribute, ldapLogin.Username)
                : string.Format("(|({0}={1})({0}={2}))", settings.LoginAttribute, ldapLogin.Username, fullLogin);

            var users = ldapHelper.GetUsersByAttributesAndFilter(
                settings, searchTerm)
                .Where(user => user != null)
                .ToList();

            if (!users.Any())
            {
                return listResults;
            }

            var loginString = ldapLogin.ToString();

            foreach (var ldapObject in users)
            {
                var currentUser = CreateUserInfo(ldapObject);

                if (Equals(currentUser, Core.Users.Constants.LostUser))
                    continue;

                if (string.IsNullOrEmpty(ldapLogin.Domain))
                {
                    listResults.Add(new KeyValuePair<UserInfo, LDAPObject>(currentUser, ldapObject));
                    continue;
                }

                string accessableLogin = null;
                var dotIndex = currentUser.Email.LastIndexOf(".", StringComparison.Ordinal);
                if (dotIndex > -1)
                {
                    accessableLogin = currentUser.Email.Remove(dotIndex);
                }

                if (!currentUser.Email.Equals(loginString, StringComparison.InvariantCultureIgnoreCase) &&
                    (accessableLogin == null || !accessableLogin.Equals(loginString, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                listResults.Add(new KeyValuePair<UserInfo, LDAPObject>(currentUser, ldapObject));
            }

            return listResults;
        }
    }
}
