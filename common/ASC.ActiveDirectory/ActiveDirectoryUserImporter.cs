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
using System;
using System.Collections.Generic;

namespace ASC.ActiveDirectory
{
    public sealed class ActiveDirectoryUserImporter
    {
        const int ONE_USER = 1;
        private IList<LDAPUser> _domainUsers;

        public IEnumerable<UserInfo> GetDiscoveredUsers()
        {
            _domainUsers = ADDomain.GetUsers();
            IList<UserInfo> users = new List<UserInfo>();
            foreach (var domainUser in _domainUsers)
            {
                if (domainUser.IsDisabled && domainUser.AccountName != string.Empty)
                {
                    continue;
                }
                users.Add(CreateUserInfo(domainUser));
            }
            return users;
        }

        public UserInfo GetDiscoveredUser(string sid)
        {
            var domainUser = ADDomain.GetUserBySid(sid);
            _domainUsers = new List<LDAPUser>(ONE_USER);
            _domainUsers.Add(domainUser);
            if(domainUser != null)
            {
                return CreateUserInfo(domainUser);
            }
            return ASC.Core.Users.Constants.LostUser;
        }

        public IEnumerable<GroupInfo> GetDiscoveredGroups()
        {
            var domainGroups = ADDomain.GetGroups();
            var groups = new List<GroupInfo>(domainGroups.Count);
            foreach(var domainGroup in domainGroups)
            {
                groups.Add(new GroupInfo
                {
                    Name = domainGroup.Name,
                    Sid = domainGroup.Sid.Value
                });
            }
            return groups;
        }

        public void AddGroupsToCore(IEnumerable<GroupInfo> groups)
        {
            foreach (var group in groups)
            {
                CoreContext.GroupManager.SaveGroupInfo(group);
            }
        }

        public void AddUsersToCore(IEnumerable<UserInfo> users)
        {
            foreach (var user in users)
            {
                AddUserToCore(user);
            }
        }

        public void AddUserToCore(UserInfo user)
        {
            var domainUser = FindDomainUser(user.Sid);
            if (domainUser != null)
            {
                CoreContext.UserManager.SaveUserInfo(user);
                var domainGroups = domainUser.GetGroups();
                foreach (var domainGroup in domainGroups)
                {
                    var group = CoreContext.GroupManager.GetGroupInfoBySid(domainGroup.Sid.Value);
                    if (group != ASC.Core.Users.Constants.LostGroupInfo)
                    {
                        CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
                    }
                }
            }
        }

        private UserInfo CreateUserInfo(LDAPUser domainUser)
        {
            return new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = domainUser.AccountName,
                Sid = domainUser.Sid.Value,
                ActivationStatus = (!string.IsNullOrEmpty(domainUser.Mail) ? EmployeeActivationStatus.Activated : EmployeeActivationStatus.NotActivated),
                Email = (!string.IsNullOrEmpty(domainUser.Mail) ? domainUser.Mail : domainUser.AccountName + "@" + ADDomain.Domain.Name + ".net"),
                FirstName = (!string.IsNullOrEmpty(domainUser.FirstName) ? domainUser.FirstName : domainUser.AccountName),
                LastName = (!string.IsNullOrEmpty(domainUser.SecondName) ? domainUser.SecondName : domainUser.AccountName),
                MobilePhone = (!string.IsNullOrEmpty(domainUser.Mobile) ? domainUser.Mobile : string.Empty),
                Title = (!string.IsNullOrEmpty(domainUser.Title) ? domainUser.Title : string.Empty),
                Location = (!string.IsNullOrEmpty(domainUser.Street) ? domainUser.Street : string.Empty),
                WorkFromDate = TenantUtil.DateTimeNow()
            };
        }

        private LDAPUser FindDomainUser(string sid)
        {
            foreach (var user in _domainUsers)
            {
                if (user.Sid.Value == sid)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
