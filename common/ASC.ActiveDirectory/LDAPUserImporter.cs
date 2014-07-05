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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace ASC.ActiveDirectory
{
    public sealed class LDAPUserImporter
    {
        private const int MAX_NUMBER_OF_SYMBOLS = 64;
        private const string UNKNOWN_DOMAIN = "unknowndomain";
        private const string PRIMARY_GROUP = "513";
        const char LAST_HYPHEN = '-';
        public List<LDAPUser> AllDomainUsers { get; set; }
        public List<LDAPGroup> DomainGroups { get; set; }
        private List<LDAPUser> _domainUsers;
        private GroupInfo _primaryGroup;
        public RelationGroupCache _cache;
        private static ILog _log = LogManager.GetLogger(typeof(LDAPUserImporter));

        public LDAPUserImporter()
        {
            _cache = new RelationGroupCache();
        }

        public string GetSidOfCurrentUser(string login, LDAPSupportSettings settings)
        {
            var users = ADDomain.GetUsersByAttributesAndFilter(settings, "(" + settings.LoginAttribute + "=" + login + ")");
            if (users.Count != 0 && users[0] != null && users[0].Sid != null)
            {
                return users[0].Sid.Value;
            }
            return null;
        }

        public List<UserInfo> GetDiscoveredUsersByAttributes(LDAPSupportSettings settings)
        {
            var users = new List<UserInfo>();
            if (AllDomainUsers != null)
            {
                AllDomainUsers = ADDomain.GetUsersByAttributes(settings);
            }
            _domainUsers = new List<LDAPUser>();
            if (AllDomainUsers != null)
            {
                foreach (var user in AllDomainUsers)
                {
                    if (user != null && !user.IsDisabled && IsUserExistsInGroup(user, settings))
                    {
                        _domainUsers.Add(user);
                        var userInfo = CreateUserInfo(user, settings);
                        if (CoreContext.UserManager.GetUserBySid("l" + userInfo.Sid).ID == Core.Users.Constants.LostUser.ID &&
                            CoreContext.UserManager.GetUserBySid(userInfo.Sid).ID == Core.Users.Constants.LostUser.ID)
                        {
                            if (CheckEmail(userInfo))
                            {
                                users.Add(userInfo);
                            }
                        }
                    }
                }
            }
            return users;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckEmail(UserInfo userInfo)
        {
            if (CoreContext.UserManager.GetUserByEmail(userInfo.Email).ID != Core.Users.Constants.LostUser.ID)
            {
                int count = 0;
                string email;
                while (true)
                {
                    email = userInfo.Email + (++count);
                    if (CoreContext.UserManager.GetUserByEmail(email).ID == Core.Users.Constants.LostUser.ID)
                    {
                        if (IsValidEmail(email))
                        {
                            userInfo.Email = email;
                        }
                        else
                        {
                            _log.ErrorFormat("Wrong email address format: {0}", email);
                            return false;
                        }
                        break;
                    }
                }
            }
            return true;
        }

        public UserInfo GetDiscoveredUser(LDAPSupportSettings settings, string sid)
        {
            var domainUser = ADDomain.GetUserBySid(settings, sid);
            if (domainUser != null && !domainUser.IsDisabled && IsUserExistsInGroup(domainUser, settings))
            {
                _domainUsers = new List<LDAPUser>();
                _domainUsers.Add(domainUser);
                var userInfo = CreateUserInfo(domainUser, settings);
                if (CheckEmail(userInfo))
                {
                    return userInfo;
                }
            }
            return Core.Users.Constants.LostUser;
        }

        public List<GroupInfo> GetDiscoveredGroupsByAttributes(LDAPSupportSettings settings)
        {
            if (settings.GroupMembership)
            {
                if (DomainGroups == null)
                {
                    DomainGroups = ADDomain.GetGroupsByParameter(settings);
                }
                if (DomainGroups != null)
                {
                    var groups = new List<GroupInfo>(DomainGroups.Count);
                    var removedGroups = new List<LDAPGroup>();
                    foreach (var domainGroup in DomainGroups)
                    {
                        var lastId = domainGroup.Sid.Value.Split(LAST_HYPHEN).Last();
                        if (lastId != PRIMARY_GROUP)
                        {
                            var members = ADDomain.GetGroupAttribute(domainGroup, settings.GroupAttribute);
                            if (members == null || members.Value == null)
                            {
                                removedGroups.Add(domainGroup);
                                continue;
                            }
                        }
                        var groupInfo = new GroupInfo
                        {
                            Name = domainGroup.Name,
                            Sid = domainGroup.Sid.Value
                        };
                        // Domain Users - primary group
                        if (domainGroup.Sid.Value.Split(LAST_HYPHEN).Last() == PRIMARY_GROUP)
                        {
                            _primaryGroup = groupInfo;
                        }
                        if (CoreContext.GroupManager.GetGroupInfoBySid(groupInfo.Sid).ID == Core.Users.Constants.LostGroupInfo.ID)
                        {
                            groups.Add(groupInfo);
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

            var dn = ADDomain.GetUserAttribute(domainUser, settings.UserAttribute);

            foreach (var domainGroup in DomainGroups)
            {
                var members = ADDomain.GetGroupAttribute(domainGroup, settings.GroupAttribute);
                if (members != null)
                {
                    if (members.Value != null)
                    {
                        foreach (var member in members)
                        {
                            var memberString = member.ToString();
                            if (IsUser(memberString))
                            {
                                if (dn.Equals(memberString, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var group = CoreContext.GroupManager.GetGroupInfoBySid(domainGroup.Sid.Value);
                                    if (group != Core.Users.Constants.LostGroupInfo)
                                    {
                                        CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                                    }
                                }
                            }
                            else if (!_cache.Exists(memberString, domainGroup.Sid.Value) && IsGroup(memberString))
                            {
                                _cache.Add(memberString, domainGroup.Sid.Value);
                            }
                        }
                    }
                }
            }
            if (_primaryGroup != null)
            {
                var primaryGroup = CoreContext.GroupManager.GetGroupInfoBySid(_primaryGroup.Sid);
                if (primaryGroup != Core.Users.Constants.LostGroupInfo)
                {
                    CoreContext.UserManager.AddUserIntoGroup(user.ID, primaryGroup.ID);
                }
            }
        }

        public void AddUserInCacheGroups(UserInfo user)
        {
            if (_cache != null && _cache.Value.Count != 0)
            {
                foreach (var pair in _cache.Value)
                {
                    var childDomainGroup = GetLDAPGroup(pair.Key);
                    if (childDomainGroup != null)
                    {
                        var childGroup = CoreContext.GroupManager.GetGroupInfoBySid(childDomainGroup.Sid.Value);
                        if (childGroup != Core.Users.Constants.LostGroupInfo)
                        {
                            foreach (var sid in pair.Value)
                            {
                                var group = CoreContext.GroupManager.GetGroupInfoBySid(sid);
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

        private LDAPGroup GetLDAPGroup(string dn)
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

        public Dictionary<string, IList<string>> GetCache()
        {
            return _cache.Value;
        }

        private bool IsUser(string dn)
        {
            foreach (var user in _domainUsers)
            {
                if (user.DistinguishedName.Equals(dn, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsGroup(string dn)
        {
            for (int i = 0; i < DomainGroups.Count; i++)
            {
                if (DomainGroups[i].DistinguishedName.Equals(dn, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private LDAPGroup GetGroup(string dn)
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

        private bool IsUserExistsInGroup(LDAPUser domainUser, LDAPSupportSettings settings)
        {
            if (!settings.GroupMembership || DomainGroups == null || DomainGroups.Count == 0 || _primaryGroup != null)
            {
                return true;
            }

            var dn = ADDomain.GetUserAttribute(domainUser, settings.UserAttribute);

            foreach (var domainGroup in DomainGroups)
            {
                if (ADDomain.UserExistsInGroup(domainGroup, dn, settings.GroupAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        private UserInfo CreateUserInfo(LDAPUser domainUser, LDAPSupportSettings settings)
        {
            var user = new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = domainUser.AccountName,
                Sid = domainUser.Sid.Value,
                ActivationStatus = (!string.IsNullOrEmpty(domainUser.Mail) ? EmployeeActivationStatus.Activated : EmployeeActivationStatus.NotActivated),
                Email = (!string.IsNullOrEmpty(domainUser.Mail) ? domainUser.Mail : string.Empty),
                MobilePhone = (!string.IsNullOrEmpty(domainUser.Mobile) ? domainUser.Mobile : string.Empty),
                Title = (!string.IsNullOrEmpty(domainUser.Title) ? domainUser.Title : string.Empty),
                Location = (!string.IsNullOrEmpty(domainUser.Street) ? domainUser.Street : string.Empty),
                WorkFromDate = TenantUtil.DateTimeNow()
            };

            if (!string.IsNullOrEmpty(domainUser.FirstName))
            {
                if (domainUser.FirstName.Length > MAX_NUMBER_OF_SYMBOLS)
                {
                    user.FirstName = domainUser.FirstName.Substring(0, MAX_NUMBER_OF_SYMBOLS);
                }
                else
                {
                    user.FirstName = domainUser.FirstName;
                }
            }
            else
            {
                user.FirstName = string.Empty;
            }

            if (!string.IsNullOrEmpty(domainUser.SecondName))
            {
                if (domainUser.SecondName.Length > MAX_NUMBER_OF_SYMBOLS)
                {
                    user.LastName = domainUser.SecondName.Substring(0, MAX_NUMBER_OF_SYMBOLS);
                }
                else
                {
                    user.LastName = domainUser.SecondName;
                }
            }
            else
            {
                user.LastName = string.Empty;
            }

            if (user.Email == string.Empty)
            {
                var domain = ADDomain.GetDomain(settings);
                var domainName = domain != null ? domain.Name : UNKNOWN_DOMAIN;
                string loginName = domainUser.InvokeGet(settings.LoginAttribute).ToString();
                user.Email = loginName.Contains("@") ? loginName : loginName + "@" + domainName;
            }
            return user;
        }

        private LDAPUser FindDomainUser(string sid)
        {
            if (_domainUsers != null)
            {
                foreach (var user in _domainUsers)
                {
                    if (user.Sid.Value == sid || "l" + user.Sid.Value == sid)
                    {
                        return user;
                    }
                }
            }
            return null;
        }
    }
}
