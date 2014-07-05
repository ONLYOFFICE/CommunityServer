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

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Core.Caching;

namespace ASC.Core
{
    internal class ClientUserManager : IUserManagerClient, IGroupManagerClient
    {
        private readonly IUserService userService;

        private readonly IDictionary<Guid, UserInfo> systemUsers;


        public ClientUserManager(IUserService service)
        {
            userService = service;

            systemUsers = Configuration.Constants.SystemAccounts.ToDictionary(a => a.ID, a => new UserInfo {ID = a.ID, LastName = a.Name});
            systemUsers[Constants.LostUser.ID] = Constants.LostUser;
        }

        #region Users

        public UserInfo[] GetUsers()
        {
            return GetUsers(EmployeeStatus.Default);
        }

        public UserInfo[] GetUsers(EmployeeStatus status)
        {
            return GetUsers(status, EmployeeType.All);
        }

        public UserInfo[] GetUsers(EmployeeStatus status, EmployeeType type)
        {
            var users = GetUsersInternal().Where(u => (u.Status & status) == u.Status);
            switch (type)
            {
                case EmployeeType.User:
                    users = users.Where(u => !u.IsVisitor());
                    break;
                case EmployeeType.Visitor:
                    users = users.Where(u => u.IsVisitor());
                    break;
            }
            return users.ToArray();
        }

        public DateTime GetMaxUsersLastModified()
        {
            return userService.GetUsers(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime))
                              .Values
                              .Select(g => g.LastModified)
                              .DefaultIfEmpty()
                              .Max();
        }

        public string[] GetUserNames(EmployeeStatus status)
        {
            return GetUsers(status)
                .Select(u => u.UserName)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }

        public UserInfo GetUserByUserName(string username)
        {
            return GetUsersInternal()
                       .FirstOrDefault(u => string.Compare(u.UserName, username, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }

        public UserInfo GetUserBySid(string sid)
        {
            return GetUsersInternal()
                       .FirstOrDefault(u => u.Sid != null && string.Compare(u.Sid, sid, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }

        public bool IsUserNameExists(string username)
        {
            return GetUserNames(EmployeeStatus.All)
                .Contains(username, StringComparer.CurrentCultureIgnoreCase);
        }

        public UserInfo GetUsers(Guid id)
        {
            if (systemUsers.ContainsKey(id)) return systemUsers[id];
            var u = userService.GetUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public IEnumerable<UserInfo> GetUsers(IEnumerable<Guid> ids)
        {
            var susers = systemUsers.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList();
            if (susers.Any()) return susers;

            var users = userService.GetUsers(CoreContext.TenantManager.GetCurrentTenant().TenantId, ids);

            var result = users.Select(x => x != null && !x.Removed ? x : Constants.LostUser);
            return result;
        }

        public UserInfo GetUsers(int tenant, string login, string passwordHash)
        {
            var u = userService.GetUser(tenant, login, passwordHash);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public bool UserExists(Guid id)
        {
            return !GetUsers(id).Equals(Constants.LostUser);
        }

        public UserInfo GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return Constants.LostUser;

            return GetUsersInternal()
                       .FirstOrDefault(u => string.Compare(u.Email, email, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }

        public UserInfo[] Search(string text, EmployeeStatus status)
        {
            return Search(text, status, Guid.Empty);
        }

        public UserInfo[] Search(string text, EmployeeStatus status, Guid groupId)
        {
            if (text == null || text.Trim() == string.Empty) return new UserInfo[0];

            var words = text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return new UserInfo[0];

            var users = groupId == Guid.Empty ?
                            GetUsers(status) :
                            GetUsersByGroup(groupId).Where(u => (u.Status & status) == status);

            var findUsers = new List<UserInfo>();
            foreach (var user in users)
            {
                var properties = new[]
                    {
                        user.LastName ?? string.Empty,
                        user.FirstName ?? string.Empty,
                        user.Title ?? string.Empty,
                        user.Location ?? string.Empty,
                        user.Email ?? string.Empty
                    };
                if (IsPropertiesContainsWords(properties, words))
                {
                    findUsers.Add(user);
                }
            }
            return findUsers.ToArray();
        }

        public UserInfo SaveUserInfo(UserInfo u)
        {
            if (systemUsers.ContainsKey(u.ID)) return systemUsers[u.ID];
            if (u.ID == Guid.Empty) SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            else SecurityContext.DemandPermissions(new UserSecurityProvider(u.ID), Constants.Action_EditUser);

            if (u.Status == EmployeeStatus.Active)
            {
                var q = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                if (q.ActiveUsers < GetUsersByGroup(Constants.GroupUser.ID).Length)
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum active users ({0})", q.ActiveUsers));
                }
            }

            return userService.SaveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, u);
        }

        public void DeleteUser(Guid id)
        {
            if (systemUsers.ContainsKey(id)) return;
            SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            if (id == CoreContext.TenantManager.GetCurrentTenant().OwnerId)
            {
                throw new InvalidOperationException("Can not remove tenant owner.");
            }

            userService.RemoveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public void SaveUserPhoto(Guid id, Guid notused, byte[] photo)
        {
            if (systemUsers.ContainsKey(id)) return;
            SecurityContext.DemandPermissions(new UserSecurityProvider(id), Constants.Action_EditUser);

            userService.SetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id, photo);
        }

        public byte[] GetUserPhoto(Guid id, Guid notused)
        {
            if (systemUsers.ContainsKey(id)) return null;
            return userService.GetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public GroupInfo[] GetUserGroups(Guid id)
        {
            return GetUserGroups(id, Guid.Empty);
        }

        public GroupInfo[] GetUserGroups(Guid id, Guid categoryID)
        {
            return GetUserGroups(id, IncludeType.Distinct, categoryID);
        }

        public GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType)
        {
            return GetUserGroups(userID, includeType, null);
        }

        private GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType, Guid? categoryId)
        {
            var result = new List<GroupInfo>();
            var distinctUserGroups = new List<GroupInfo>();

            var refs = GetRefsInternal();
            IEnumerable<UserGroupRef> userRefs = null;
            if (refs is UserGroupRefStore)
            {
                userRefs = ((UserGroupRefStore)refs).GetRefsByUser(userID);
            }

            foreach (var g in GetGroupsInternal().Where(g => !categoryId.HasValue || g.CategoryID == categoryId))
            {
                if (((g.CategoryID == Constants.SysGroupCategoryId || userRefs == null) && IsUserInGroupInternal(userID, g.ID, refs)) ||
                    (userRefs != null && userRefs.Any(r => !r.Removed && r.RefType == UserGroupRefType.Contains && r.GroupId == g.ID)))
                {
                    distinctUserGroups.Add(g);
                }
            }

            if (IncludeType.Distinct == (includeType & IncludeType.Distinct))
            {
                result.AddRange(distinctUserGroups);
            }

            return result.ToArray();
        }

        public bool IsUserInGroup(Guid userId, Guid groupId)
        {
            return IsUserInGroupInternal(userId, groupId, GetRefsInternal());
        }

        public UserInfo[] GetUsersByGroup(Guid groupId)
        {
            var refs = GetRefsInternal();
            return GetUsers().Where(u => IsUserInGroupInternal(u.ID, groupId, refs)).ToArray();
        }

        public void AddUserIntoGroup(Guid userId, Guid groupId)
        {
            if (Constants.LostUser.ID == userId || Constants.LostGroupInfo.ID == groupId)
            {
                return;
            }
            SecurityContext.DemandPermissions(Constants.Action_EditGroups);

            userService.SaveUserGroupRef(
                CoreContext.TenantManager.GetCurrentTenant().TenantId,
                new UserGroupRef(userId, groupId, UserGroupRefType.Contains));
        }

        public void RemoveUserFromGroup(Guid userId, Guid groupId)
        {
            if (Constants.LostUser.ID == userId || Constants.LostGroupInfo.ID == groupId) return;
            SecurityContext.DemandPermissions(Constants.Action_EditGroups);

            userService.RemoveUserGroupRef(CoreContext.TenantManager.GetCurrentTenant().TenantId, userId, groupId, UserGroupRefType.Contains);
        }

        #endregion Users

        #region Company

        public GroupInfo[] GetDepartments()
        {
            return CoreContext.GroupManager.GetGroups();
        }

        public Guid GetDepartmentManager(Guid deparmentID)
        {
            return GetRefsInternal()
                .Values
                .Where(r => r.RefType == UserGroupRefType.Manager && r.GroupId == deparmentID && !r.Removed)
                .Select(r => r.UserId)
                .SingleOrDefault();
        }

        public void SetDepartmentManager(Guid deparmentID, Guid userID)
        {
            var managerId = GetDepartmentManager(deparmentID);
            if (managerId != Guid.Empty)
            {
                userService.RemoveUserGroupRef(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    managerId, deparmentID, UserGroupRefType.Manager);
            }
            if (userID != Guid.Empty)
            {
                userService.SaveUserGroupRef(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    new UserGroupRef(userID, deparmentID, UserGroupRefType.Manager));
            }
        }

        public UserInfo GetCompanyCEO()
        {
            var id = GetDepartmentManager(Guid.Empty);
            return id != Guid.Empty ? GetUsers(id) : null;
        }

        public void SetCompanyCEO(Guid userId)
        {
            SetDepartmentManager(Guid.Empty, userId);
        }

        #endregion Company

        #region Groups

        public GroupInfo[] GetGroups()
        {
            return GetGroups(Guid.Empty);
        }

        public GroupInfo[] GetGroups(Guid categoryID)
        {
            return GetGroupsInternal()
                .Where(g => g.CategoryID == categoryID)
                .ToArray();
        }

        public GroupInfo GetGroupInfo(Guid groupID)
        {
            return GetGroupsInternal()
                       .SingleOrDefault(g => g.ID == groupID) ?? Constants.LostGroupInfo;
        }

        public GroupInfo GetGroupInfoBySid(string sid)
        {
            return GetGroupsInternal()
                       .SingleOrDefault(g => g.Sid == sid) ?? Constants.LostGroupInfo;
        }

        public DateTime GetMaxGroupsLastModified()
        {
            return userService.GetGroups(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime))
                              .Values
                              .Select(g => g.LastModified)
                              .DefaultIfEmpty()
                              .Max();
        }

        public GroupInfo SaveGroupInfo(GroupInfo g)
        {
            if (Constants.LostGroupInfo.Equals(g)) return Constants.LostGroupInfo;
            if (Constants.BuildinGroups.Any(b => b.ID == g.ID)) return Constants.BuildinGroups.Single(b => b.ID == g.ID);
            SecurityContext.DemandPermissions(Constants.Action_EditGroups);

            var newGroup = userService.SaveGroup(CoreContext.TenantManager.GetCurrentTenant().TenantId, ToGroup(g));
            return GetGroupInfo(newGroup.Id);
        }

        public void DeleteGroup(Guid id)
        {
            if (Constants.LostGroupInfo.Equals(id)) return;
            if (Constants.BuildinGroups.Any(b => b.ID == id)) return;
            SecurityContext.DemandPermissions(Constants.Action_EditGroups);

            userService.RemoveGroup(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        #endregion Groups

        private bool IsPropertiesContainsWords(IEnumerable<string> properties, IEnumerable<string> words)
        {
            foreach (var w in words)
            {
                var find = false;
                foreach (var p in properties)
                {
                    find = (2 <= w.Length) && (0 <= p.IndexOf(w, StringComparison.CurrentCultureIgnoreCase));
                    if (find) break;
                }
                if (!find) return false;
            }
            return true;
        }


        private IEnumerable<UserInfo> GetUsersInternal()
        {
            return userService.GetUsers(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime))
                              .Values
                              .Where(u => !u.Removed);
        }

        private IEnumerable<GroupInfo> GetGroupsInternal()
        {
            return userService.GetGroups(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime))
                              .Values
                              .Where(g => !g.Removed)
                              .Select(g => new GroupInfo(g.CategoryId) {ID = g.Id, Name = g.Name, Sid = g.Sid})
                              .Concat(Constants.BuildinGroups);
        }

        private IDictionary<string, UserGroupRef> GetRefsInternal()
        {
            return userService.GetUserGroupRefs(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
        }

        private bool IsUserInGroupInternal(Guid userId, Guid groupId, IDictionary<string, UserGroupRef> refs)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            if (groupId == Constants.GroupEveryone.ID)
            {
                return true;
            }
            if (groupId == Constants.GroupAdmin.ID && (tenant.OwnerId == userId || userId == Configuration.Constants.CoreSystem.ID))
            {
                return true;
            }

            UserGroupRef r;
            if (groupId == Constants.GroupUser.ID || groupId == Constants.GroupVisitor.ID)
            {
                var visitor = refs.TryGetValue(UserGroupRef.CreateKey(tenant.TenantId, userId, Constants.GroupVisitor.ID, UserGroupRefType.Contains), out r) && !r.Removed;
                if (groupId == Constants.GroupVisitor.ID)
                {
                    return visitor;
                }
                if (groupId == Constants.GroupUser.ID)
                {
                    return !visitor;
                }
            }
            return refs.TryGetValue(UserGroupRef.CreateKey(tenant.TenantId, userId, groupId, UserGroupRefType.Contains), out r) && !r.Removed;
        }

        private Group ToGroup(GroupInfo g)
        {
            if (g == null) return null;
            return new Group
                {
                    Id = g.ID,
                    Name = g.Name,
                    ParentId = g.Parent != null ? g.Parent.ID : Guid.Empty,
                    CategoryId = g.CategoryID,
                    Sid = g.Sid
                };
        }
    }
}