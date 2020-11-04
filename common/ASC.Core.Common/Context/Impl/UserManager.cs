/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Core.Caching;

namespace ASC.Core
{
    public class UserManager
    {
        private readonly IUserService userService;

        private readonly IDictionary<Guid, UserInfo> systemUsers;


        public UserManager(IUserService service)
        {
            userService = service;

            systemUsers = Configuration.Constants.SystemAccounts.ToDictionary(a => a.ID, a => new UserInfo { ID = a.ID, LastName = a.Name });
            systemUsers[Constants.LostUser.ID] = Constants.LostUser;
            systemUsers[Constants.OutsideUser.ID] = Constants.OutsideUser;
            systemUsers[Constants.NamingPoster.ID] = Constants.NamingPoster;
        }


        public void ClearCache()
        {
            if (userService is ICachedService)
            {
                ((ICachedService)userService).InvalidateCache();
            }
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

        public UserInfo GetSsoUserByNameId(string nameId)
        {
            return GetUsersInternal()
                .FirstOrDefault(u => !string.IsNullOrEmpty(u.SsoNameId) && string.Compare(u.SsoNameId, nameId, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }
        public bool IsUserNameExists(string username)
        {
            return GetUserNames(EmployeeStatus.All)
                .Contains(username, StringComparer.CurrentCultureIgnoreCase);
        }

        public UserInfo GetUsers(Guid id)
        {
            if (IsSystemUser(id)) return systemUsers[id];
            var u = userService.GetUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public UserInfo GetUsersByPasswordHash(int tenant, string login, string passwordHash)
        {
            var u = userService.GetUserByPasswordHash(tenant, login, passwordHash);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public bool UserExists(Guid id)
        {
            return !GetUsers(id).Equals(Constants.LostUser);
        }

        public bool IsSystemUser(Guid id)
        {
            return systemUsers.ContainsKey(id);
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

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return new UserInfo[0];

            var users = groupId == Guid.Empty ?
                GetUsers(status) :
                GetUsersByGroup(groupId).Where(u => (u.Status & status) == status);

            var findUsers = new List<UserInfo>();
            foreach (var user in users)
            {
                var properties = new string[]
                {
                    user.LastName ?? string.Empty,
                    user.FirstName ?? string.Empty,
                    user.Title ?? string.Empty,
                    user.Location ?? string.Empty,
                    user.Email ?? string.Empty,
                };
                if (IsPropertiesContainsWords(properties, words))
                {
                    findUsers.Add(user);
                }
            }
            return findUsers.ToArray();
        }

        public UserInfo SaveUserInfo(UserInfo u, bool isVisitor = false)
        {
            if (IsSystemUser(u.ID)) return systemUsers[u.ID];
            if (u.ID == Guid.Empty) SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            else SecurityContext.DemandPermissions(new UserSecurityProvider(u.ID), Constants.Action_EditUser);

            if (Constants.MaxEveryoneCount <= GetUsersByGroup(Constants.GroupEveryone.ID).Length)
            {
                throw new TenantQuotaException("Maximum number of users exceeded");
            }

            if (u.Status == EmployeeStatus.Active)
            {
                var q = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                if (q.ActiveUsers < GetUsersByGroup(Constants.GroupUser.ID).Length)
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum active users ({0})", q.ActiveUsers));
                }
            }

            if (u.Status == EmployeeStatus.Terminated && u.ID == CoreContext.TenantManager.GetCurrentTenant().OwnerId)
            {
                throw new InvalidOperationException("Can not disable tenant owner.");
            }

            var newUser = userService.SaveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, u);

            return newUser;
        }

        public void DeleteUser(Guid id)
        {
            if (IsSystemUser(id)) return;
            SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            if (id == CoreContext.TenantManager.GetCurrentTenant().OwnerId)
            {
                throw new InvalidOperationException("Can not remove tenant owner.");
            }

            userService.RemoveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public void SaveUserPhoto(Guid id, byte[] photo)
        {
            if (IsSystemUser(id)) return;
            SecurityContext.DemandPermissions(new UserSecurityProvider(id), Constants.Action_EditUser);

            userService.SetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id, photo);
        }

        public byte[] GetUserPhoto(Guid id)
        {
            if (IsSystemUser(id)) return null;
            return userService.GetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public IEnumerable<Guid> GetUserGroupsId(Guid id)
        {
            return GetUsers(id).GetUserGroupsId();
        }

        public GroupInfo[] GetUserGroups(Guid id)
        {
            return GetUsers(id).GetGroups(IncludeType.Distinct, Guid.Empty);
        }

        public GroupInfo[] GetUserGroups(Guid id, Guid categoryID)
        {
            return GetUsers(id).GetGroups(IncludeType.Distinct, categoryID);
        }

        public GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType)
        {
            return GetUsers(userID).GetGroups(includeType, null);
        }

        internal GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType, Guid? categoryId)
        {
            var result = new List<GroupInfo>();
            var distinctUserGroups = new List<GroupInfo>();

            var refs = GetRefsInternal();
            IEnumerable<UserGroupRef> userRefs = null;
            var store = refs as UserGroupRefStore;
            if (store != null)
            {
                userRefs = store.GetRefsByUser(userID);
            }

            var userRefsContainsNotRemoved = userRefs != null ? userRefs.Where(r => !r.Removed && r.RefType == UserGroupRefType.Contains).ToList() : null;

            foreach (var g in GetGroupsInternal().Where(g => !categoryId.HasValue || g.CategoryID == categoryId))
            {
                if (((g.CategoryID == Constants.SysGroupCategoryId || userRefs == null) && IsUserInGroupInternal(userID, g.ID, refs)) ||
                    (userRefsContainsNotRemoved != null && userRefsContainsNotRemoved.Any(r => r.GroupId == g.ID)))
                {
                    distinctUserGroups.Add(g);
                }
            }

            if (IncludeType.Distinct == (includeType & IncludeType.Distinct))
            {
                result.AddRange(distinctUserGroups);
            }

            result.Sort((group1, group2) => String.Compare(group1.Name, group2.Name, StringComparison.Ordinal));

            return result.ToArray();
        }

        internal IEnumerable<Guid> GetUserGroupsGuids(Guid userID)
        {
            var result = new List<Guid>();

            var refs = GetRefsInternal();

            var store = refs as UserGroupRefStore;
            if (store != null)
            {
                var userRefs = store.GetRefsByUser(userID);

                if (userRefs != null)
                {
                    var toAdd = userRefs.Where(r => !r.Removed && 
                        r.RefType == UserGroupRefType.Contains && 
                        !Constants.BuildinGroups.Any(g => g.ID.Equals(r.GroupId)))
                        .Select(r => r.GroupId);
                    result.AddRange(toAdd);
                }
            }

            return result;
        }

        public bool IsUserInGroup(Guid userId, Guid groupId)
        {
            return IsUserInGroupInternal(userId, groupId, GetRefsInternal());
        }

        public UserInfo[] GetUsersByGroup(Guid groupId, EmployeeStatus employeeStatus = EmployeeStatus.Default)
        {
            var refs = GetRefsInternal();
            return GetUsers(employeeStatus).Where(u => IsUserInGroupInternal(u.ID, groupId, refs)).ToArray();
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

            GetUsers(userId).ResetGroupCache();
        }

        public void RemoveUserFromGroup(Guid userId, Guid groupId)
        {
            if (Constants.LostUser.ID == userId || Constants.LostGroupInfo.ID == groupId) return;
            SecurityContext.DemandPermissions(Constants.Action_EditGroups);

            userService.RemoveUserGroupRef(CoreContext.TenantManager.GetCurrentTenant().TenantId, userId, groupId, UserGroupRefType.Contains);

            GetUsers(userId).ResetGroupCache();
        }

        #endregion Users


        #region Company

        public GroupInfo[] GetDepartments()
        {
            return CoreContext.UserManager.GetGroups();
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
            return new GroupInfo(newGroup.CategoryId) { ID = newGroup.Id, Name = newGroup.Name, Sid = newGroup.Sid };
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
                .Select(g => new GroupInfo(g.CategoryId) { ID = g.Id, Name = g.Name, Sid = g.Sid })
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
            if (groupId == Constants.GroupAdmin.ID && (tenant.OwnerId == userId || userId == Configuration.Constants.CoreSystem.ID || userId == Constants.NamingPoster.ID))
            {
                return true;
            }
            if (groupId == Constants.GroupVisitor.ID && userId == Constants.OutsideUser.ID)
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