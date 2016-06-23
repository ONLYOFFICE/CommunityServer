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
using System.Linq;

namespace ASC.ActiveDirectory
{
    public class LDAPUserImporter
    {
        private const int maxNumberOfSymbols = 64;
        private const string unknownDomain = "unknowndomain";
        private const string mobPhone = "mobphone";
        private const string primaryGroupId = "513";
        private const char hyphen = '-';
        public List<LDAPObject> AllDomainUsers { get; set; }
        public List<LDAPObject> DomainGroups { get; set; }
        private List<LDAPObject> domainUsers;
        private GroupInfo primaryGroup;
        private readonly RelationGroupCache relationGroupCache = new RelationGroupCache();
        private readonly LdapHelper ldapHelper;
        private readonly ILog log = LogManager.GetLogger(typeof(LDAPUserImporter));

        public LDAPUserImporter()
        {
            ldapHelper = !WorkContext.IsMono ? (LdapHelper)new SystemLdapHelper() : new NovellLdapHelper();
        }

        public string GetSidOfCurrentUser(string login, LDAPSupportSettings settings)
        {
            var users = ldapHelper.GetUsersByAttributesAndFilter(settings, "(" + settings.LoginAttribute + "=" + login + ")");
            if (users.Count != 0)
            {
                foreach (var user in users)
                {
                    if (user != null)
                    {
                        return user.Sid;
                    }
                }
            }
            return null;
        }

        public List<UserInfo> GetDiscoveredUsersByAttributes(LDAPSupportSettings settings)
        {
            var users = new List<UserInfo>();
            if (AllDomainUsers == null)
            {
                AllDomainUsers = ldapHelper.GetUsersByAttributes(settings);
            }
            domainUsers = new List<LDAPObject>();
            if (AllDomainUsers != null)
            {
                foreach (var user in AllDomainUsers)
                {
                    if (user != null && !user.IsDisabled && IsUserExistsInGroup(user, settings))
                    {
                        domainUsers.Add(user);
                        var userInfo = CreateUserInfo(user, settings);
                        if (CoreContext.UserManager.GetUserBySid("l" + userInfo.Sid).ID == Core.Users.Constants.LostUser.ID &&
                            CoreContext.UserManager.GetUserBySid(userInfo.Sid).ID == Core.Users.Constants.LostUser.ID)
                        {
                            users.Add(userInfo);
                        }
                    }
                }
            }
            return users;
        }

        public UserInfo GetDiscoveredUser(LDAPSupportSettings settings, string sid)
        {
            var domainUser = ldapHelper.GetUserBySid(settings, sid);
            if (domainUser != null && !domainUser.IsDisabled && IsUserExistsInGroup(domainUser, settings))
            {
                domainUsers = new List<LDAPObject> { domainUser };
                var userInfo = CreateUserInfo(domainUser, settings);
                CheckEmailIsNew(userInfo);
                return userInfo;
            }
            return Core.Users.Constants.LostUser;
        }

        public List<GroupInfo> GetDiscoveredGroupsByAttributes(LDAPSupportSettings settings, out List<GroupInfo> existingGroups)
        {
            existingGroups = new List<GroupInfo>();
            if (settings.GroupMembership)
            {
                if (DomainGroups == null)
                {
                    DomainGroups = ldapHelper.GetGroupsByAttributes(settings);
                }
                if (DomainGroups != null)
                {
                    var groups = new List<GroupInfo>(DomainGroups.Count);
                    var removedGroups = new List<LDAPObject>();
                    foreach (var domainGroup in DomainGroups)
                    {
                        var lastId = domainGroup.Sid.Split(hyphen).Last();
                        if (lastId != primaryGroupId)
                        {
                            var members = ldapHelper.GetGroupAttribute(domainGroup, settings.GroupAttribute);
                            if (members == null)
                            {
                                removedGroups.Add(domainGroup);
                                continue;
                            }
                        }
                        string sid = domainGroup.Sid;
                        var groupInfo = new GroupInfo
                        {
                            Name = domainGroup.InvokeGet(settings.GroupNameAttribute) as string,
                            Sid = sid
                        };
                        // Domain Users - primary group
                        if (sid.Split(hyphen).Last() == primaryGroupId)
                        {
                            primaryGroup = groupInfo;
                        }
                        if (CoreContext.UserManager.GetGroupInfoBySid(groupInfo.Sid).ID == Core.Users.Constants.LostGroupInfo.ID)
                        {
                            groups.Add(groupInfo);
                        }
                        else
                        {
                            existingGroups.Add(groupInfo);
                        }
                    }
                    foreach (var domainGroup in removedGroups)
                    {
                        if (DomainGroups.Contains(domainGroup))
                        {
                            DomainGroups.Remove(domainGroup);
                        }
                    }
                    return groups;
                }
            }
            return null;
        }

        public void AddUserIntoGroups(UserInfo user, LDAPSupportSettings settings)
        {
            if (user == null || !settings.GroupMembership || DomainGroups == null)
            {
                return;
            }

            var domainUser = FindDomainUser(user.Sid);
            if (domainUser == null)
            {
                return;
            }

            var userAttributeValue = ldapHelper.GetUserAttribute(domainUser, settings.UserAttribute);

            foreach (var domainGroup in DomainGroups)
            {
                string sid = domainGroup.Sid;
                var members = ldapHelper.GetGroupAttribute(domainGroup, settings.GroupAttribute);
                if (members != null)
                {
                    foreach (string member in members)
                    {
                        if (IsUser(member, settings.UserAttribute))
                        {
                            if (userAttributeValue.Equals(member, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var group = CoreContext.UserManager.GetGroupInfoBySid(sid);
                                if (group != Core.Users.Constants.LostGroupInfo)
                                {
                                    CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                                }
                            }
                        }
                        else if (!relationGroupCache.Exists(member, sid) && IsGroup(member, settings.UserAttribute))
                        {
                            relationGroupCache.Add(member, sid);
                        }
                    }
                }
            }
            if (primaryGroup != null)
            {
                var getPrimaryGroup = CoreContext.UserManager.GetGroupInfoBySid(primaryGroup.Sid);
                if (getPrimaryGroup != Core.Users.Constants.LostGroupInfo)
                {
                    CoreContext.UserManager.AddUserIntoGroup(user.ID, getPrimaryGroup.ID);
                }
            }
        }

        public void AddUserInCacheGroups(UserInfo user)
        {
            if (relationGroupCache != null && relationGroupCache.Value.Count != 0)
            {
                foreach (var pair in relationGroupCache.Value)
                {
                    var childDomainGroup = GetLDAPGroup(pair.Key);
                    if (childDomainGroup != null)
                    {
                        string childDomainGroupSid = childDomainGroup.Sid;
                        var childGroup = CoreContext.UserManager.GetGroupInfoBySid(childDomainGroupSid);
                        if (childGroup != Core.Users.Constants.LostGroupInfo)
                        {
                            foreach (var sid in pair.Value)
                            {
                                var group = CoreContext.UserManager.GetGroupInfoBySid(sid);
                                if (group != Core.Users.Constants.LostGroupInfo)
                                {
                                    CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<string, IList<string>> GetCache()
        {
            return relationGroupCache.Value;
        }

        public void CheckEmailIsNew(UserInfo userInfo)
        {
            if (CoreContext.UserManager.GetUserByEmail(userInfo.Email).ID != Core.Users.Constants.LostUser.ID)
            {
                int count = 0;
                while (true)
                {
                    var email = userInfo.Email + (++count);
                    if (CoreContext.UserManager.GetUserByEmail(email).ID == Core.Users.Constants.LostUser.ID)
                    {
                        userInfo.Email = email;
                        break;
                    }
                }
            }
        }

        private LDAPObject GetLDAPGroup(string distinguishedName)
        {
            for (int i = 0; i < DomainGroups.Count; i++)
            {
                if (DomainGroups[i].DistinguishedName.Equals(distinguishedName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return DomainGroups[i];
                }
            }
            return null;
        }

        private bool IsUser(string userAttributeValue, string userAttribute)
        {
            foreach (var user in domainUsers)
            {
                var userAttrVal = user.InvokeGet(userAttribute) as string;
                if (userAttributeValue.Equals(userAttrVal, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsGroup(string userAttributeValue, string userAttribute)
        {
            string userAttrVal;
            foreach (var group in DomainGroups)
            {
                userAttrVal = group.InvokeGet(userAttribute) as string;
                if (userAttributeValue.Equals(userAttrVal, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private LDAPObject GetGroup(string dn)
        {
            for (int i = 0; i < DomainGroups.Count; i++)
            {
                if (DomainGroups[i].DistinguishedName.Equals(dn, StringComparison.InvariantCultureIgnoreCase))
                {
                    return DomainGroups[i];
                }
            }
            return null;
        }

        private bool IsUserExistsInGroup(LDAPObject domainUser, LDAPSupportSettings settings)
        {
            if (!settings.GroupMembership || DomainGroups == null || DomainGroups.Count == 0 || primaryGroup != null)
            {
                return true;
            }

            var distinguishedName = ldapHelper.GetUserAttribute(domainUser, settings.UserAttribute);

            foreach (var domainGroup in DomainGroups)
            {
                if (ldapHelper.UserExistsInGroup(domainGroup, distinguishedName, settings.GroupAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        private UserInfo CreateUserInfo(LDAPObject domainUser, LDAPSupportSettings settings)
        {
            string userName = GetAttributeFromUser(domainUser, settings.LoginAttribute);
            string firstName = GetAttributeFromUser(domainUser, settings.FirstNameAttribute);
            string secondName = GetAttributeFromUser(domainUser, settings.SecondNameAttribute);
            string mail = GetAttributeFromUser(domainUser, settings.MailAttribute);
            string mobilePhone = GetAttributeFromUser(domainUser, settings.MobilePhoneAttribute);
            string title = GetAttributeFromUser(domainUser, settings.TitleAttribute);
            string location = GetAttributeFromUser(domainUser, settings.LocationAttribute);
            List<string> contacts = new List<string>(2);

            if (!string.IsNullOrEmpty(mobilePhone))
            {
                contacts.Add(mobPhone);
                contacts.Add(mobilePhone);
            }

            var user = new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = userName,
                Sid = domainUser.Sid,
                ActivationStatus = (!string.IsNullOrEmpty(mail) ? EmployeeActivationStatus.Activated : EmployeeActivationStatus.NotActivated),
                Email = (!string.IsNullOrEmpty(mail) ? mail : string.Empty),
                Title = (!string.IsNullOrEmpty(title) ? title : string.Empty),
                Location = (!string.IsNullOrEmpty(location) ? location : string.Empty),
                WorkFromDate = TenantUtil.DateTimeNow(),
                Contacts = contacts
            };

            if (!string.IsNullOrEmpty(firstName))
            {
                if (firstName.Length > maxNumberOfSymbols)
                {
                    user.FirstName = firstName.Substring(0, maxNumberOfSymbols);
                }
                else
                {
                    user.FirstName = firstName;
                }
            }
            else
            {
                user.FirstName = string.Empty;
            }

            if (!string.IsNullOrEmpty(secondName))
            {
                if (secondName.Length > maxNumberOfSymbols)
                {
                    user.LastName = secondName.Substring(0, maxNumberOfSymbols);
                }
                else
                {
                    user.LastName = secondName;
                }
            }
            else
            {
                user.LastName = string.Empty;
            }

            if (user.Email == string.Empty)
            {
                var domain = ldapHelper.GetDomain(settings);
                //DC= or dc=
                var domainName = domain != null && domain.DistinguishedName != null ?
                    domain.DistinguishedName.Remove(0, 3).Replace(",DC=", ".").Replace(",dc=", ".") : unknownDomain;
                string loginName = domainUser.InvokeGet(settings.LoginAttribute).ToString();
                string email = loginName.Contains("@") ? loginName : loginName + "@" + domainName;
                user.Email = email.Replace(" ", string.Empty);
            }

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
                log.ErrorFormat("Can't get attribute from user: attr = {0}, dn = {1}, {2}",
                    attribute, domainUser.DistinguishedName, e);
                return String.Empty;
            }
        }

        private LDAPObject FindDomainUser(string sid)
        {
            if (domainUsers != null)
            {
                foreach (var user in domainUsers)
                {
                    string userSid = user.Sid;
                    if (userSid == sid || "l" + userSid == sid)
                    {
                        return user;
                    }
                }
            }
            return null;
        }
    }
}
