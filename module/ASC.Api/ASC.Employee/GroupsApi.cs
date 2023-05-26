/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;

namespace ASC.Api.Employee
{
    ///<summary>
    /// Groups API.
    ///</summary>
    ///<name>group</name>
    public class GroupsApi : IApiEntryPoint
    {
        public string Name
        {
            get { return "group"; }
        }

        public GroupsApi(ApiContext context)
        {
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        ///<summary>
        ///Returns the general information about all the groups, such as group ID and group manager.
        ///</summary>
        ///<short>
        ///Get groups
        ///</short>
        ///<returns type="ASC.Api.Employee.GroupWrapperSummary, ASC.Api.Employee">List of groups</returns>
        /// <remarks>
        /// This method returns partial group information.
        /// </remarks>
        /// <path>api/2.0/group</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("")]
        public IEnumerable<GroupWrapperSummary> GetAll()
        {
            return CoreContext.UserManager.GetDepartments().Select(x => new GroupWrapperSummary(x));
        }

        ///<summary>
        ///Returns a list of all the groups by the group name specified in the request.
        ///</summary>
        ///<short>
        ///Get groups by a group name
        ///</short>
        ///<param type="System.String, System" name="groupName">Group name</param>
        ///<returns>List of groups</returns>
        ///<path>api/2.0/group/search</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("search")]
        public IEnumerable<GroupWrapperSummary> GetTagsByName(string groupName)
        {
            groupName = (groupName ?? "").Trim();

            if (string.IsNullOrEmpty(groupName)) return new List<GroupWrapperSummary>();

            return CoreContext.UserManager.GetDepartments()
                .Where(x => x.Name.Contains(groupName))
                .Select(x => new GroupWrapperSummary(x));
        }

        ///<summary>
        ///Returns the detailed information about the selected group: group name, category, description, manager, members, and parent group if it exists.
        ///</summary>
        ///<short>
        ///Get a group
        ///</short>
        ///<param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        ///<returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group</returns>
        /// <remarks>
        /// This method returns full group information.
        /// </remarks>
        /// <path>api/2.0/group/{groupid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{groupid}")]
        public GroupWrapperFull GetById(Guid groupid)
        {
            return new GroupWrapperFull(GetGroupInfo(groupid), true);
        }

        ///<summary>
        ///Returns a list of groups for the user with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get user groups
        ///</short>
        ///<param type="System.Guid, System" method="url" name="userid">User ID</param>
        ///<returns type="ASC.Api.Employee.GroupWrapperSummary, ASC.Api.Employee">Group</returns>
        /// <path>api/2.0/group/user/{userid}</path>
        /// <httpMethod>GET</httpMethod>
        ///  <collection>list</collection>
        [Read("user/{userid}")]
        public IEnumerable<GroupWrapperSummary> GetByUserId(Guid userid)
        {
            return CoreContext.UserManager.GetUserGroups(userid).Select(x => new GroupWrapperSummary(x));
        }

        /// <summary>
        /// Adds a new group with the group manager, name, and members specified in the request.
        /// </summary>
        /// <short>
        /// Add a new group
        /// </short>
        /// <param type="System.Guid, System" name="groupManager">Group manager</param>
        /// <param type="System.String, System" name="groupName">Group name</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="members">List of group members</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Newly created group</returns>
        /// <path>api/2.0/group</path>
        /// <httpMethod>POST</httpMethod>
        [Create("")]
        public GroupWrapperFull AddGroup(Guid groupManager, string groupName, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);

            var group = CoreContext.UserManager.SaveGroupInfo(new GroupInfo { Name = groupName });

            TransferUserToDepartment(groupManager, @group, true);
            if (members != null)
            {
                foreach (var member in members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(Request, MessageAction.GroupCreated, MessageTarget.Create(group.ID), group.Name);

            return new GroupWrapperFull(group, true);
        }

        /// <summary>
        /// Updates the existing group changing the group manager, name, and/or members.
        /// </summary>
        /// <short>
        /// Update a group
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <param type="System.Guid, System" name="groupManager">New group manager</param>
        /// <param type="System.String, System" name="groupName">New group name</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="members">New list of group members</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Updated group</returns>
        /// <path>api/2.0/group/{groupid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{groupid}")]
        public GroupWrapperFull UpdateGroup(Guid groupid, Guid groupManager, string groupName, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = CoreContext.UserManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");
            if (group.ID == Core.Users.Constants.LostGroupInfo.ID)
            {
                throw new ItemNotFoundException("group not found");
            }

            group.Name = groupName ?? group.Name;
            CoreContext.UserManager.SaveGroupInfo(group);

            RemoveMembersFrom(groupid, CoreContext.UserManager.GetUsersByGroup(groupid, EmployeeStatus.All).Select(u => u.ID).Where(id => !members.Contains(id)));

            TransferUserToDepartment(groupManager, @group, true);
            if (members != null)
            {
                foreach (var member in members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(Request, MessageAction.GroupUpdated, MessageTarget.Create(group.ID), group.Name);

            return GetById(groupid);
        }

        /// <summary>
        /// Deletes a group with the ID specified in the request from the list of groups on the portal.
        /// </summary>
        /// <short>
        /// Delete a group
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group</returns>
        /// <path>api/2.0/group/{groupid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("{groupid}")]
        public GroupWrapperFull DeleteGroup(Guid groupid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var @group = GetGroupInfo(groupid);
            var groupWrapperFull = new GroupWrapperFull(group, false);

            CoreContext.UserManager.DeleteGroup(groupid);

            MessageService.Send(Request, MessageAction.GroupDeleted, MessageTarget.Create(group.ID), group.Name);

            return groupWrapperFull;
        }

        private static GroupInfo GetGroupInfo(Guid groupid)
        {
            var group =
                CoreContext.UserManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull(
                    "group not found");
            if (group.ID == Core.Users.Constants.LostGroupInfo.ID)
                throw new ItemNotFoundException("group not found");
            return @group;
        }

        /// <summary>
        /// Moves all the members from the selected group to another one specified in the request.
        /// </summary>
        /// <short>
        /// Move group members
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID to move from</param>
        /// <param type="System.Guid, System" method="url" name="newgroupid">Group ID to move to</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">New group information</returns>
        /// <path>api/2.0/group/{groupid}/members/{newgroupid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{groupid}/members/{newgroupid}")]
        public GroupWrapperFull TransferMembersTo(Guid groupid, Guid newgroupid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var oldgroup = GetGroupInfo(groupid);

            var newgroup = GetGroupInfo(newgroupid);

            var users = CoreContext.UserManager.GetUsersByGroup(oldgroup.ID);
            foreach (var userInfo in users)
            {
                TransferUserToDepartment(userInfo.ID, newgroup, false);
            }
            return GetById(newgroupid);
        }

        /// <summary>
        /// Replaces the group members with those specified in the request.
        /// </summary>
        /// <short>
        /// Replace group members
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="members">List of new members</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group information</returns>
        /// <path>api/2.0/group/{groupid}/members</path>
        /// <httpMethod>POST</httpMethod>
        [Create("{groupid}/members")]
        public GroupWrapperFull SetMembersTo(Guid groupid, IEnumerable<Guid> members)
        {
            RemoveMembersFrom(groupid, CoreContext.UserManager.GetUsersByGroup(groupid).Select(x => x.ID));
            AddMembersTo(groupid, members);
            return GetById(groupid);
        }

        /// <summary>
        /// Adds new group members to the group with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Add group members
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="members">List of new members</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group information</returns>
        /// <path>api/2.0/group/{groupid}/members</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{groupid}/members")]
        public GroupWrapperFull AddMembersTo(Guid groupid, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in members)
            {
                TransferUserToDepartment(userId, group, false);
            }
            return GetById(group.ID);
        }

        /// <summary>
        /// Sets a user with the ID specified in the request as a group manager.
        /// </summary>
        /// <short>
        /// Set a group manager
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group information</returns>
        /// <path>api/2.0/group/{groupid}/manager</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update("{groupid}/manager")]
        public GroupWrapperFull SetManager(Guid groupid, Guid userid)
        {
            var group = GetGroupInfo(groupid);
            if (CoreContext.UserManager.UserExists(userid))
            {
                CoreContext.UserManager.SetDepartmentManager(group.ID, userid);
            }
            else
            {
                throw new ItemNotFoundException("user not found");
            }
            return GetById(groupid);
        }

        /// <summary>
        /// Removes the group members specified in the request from the selected group.
        /// </summary>
        /// <short>
        /// Remove group members
        /// </short>
        /// <param type="System.Guid, System" method="url" name="groupid">Group ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="members">List of group members</param>
        /// <path>api/2.0/group/{groupid}/members</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <returns type="ASC.Api.Employee.GroupWrapperFull, ASC.Api.Employee">Group information</returns>
        [Delete("{groupid}/members")]
        public GroupWrapperFull RemoveMembersFrom(Guid groupid, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in members)
            {
                RemoveUserFromDepartment(userId, group);
            }
            return GetById(group.ID);
        }

        private static void RemoveUserFromDepartment(Guid userId, GroupInfo @group)
        {
            if (!CoreContext.UserManager.UserExists(userId)) return;

            var user = CoreContext.UserManager.GetUsers(userId);
            CoreContext.UserManager.RemoveUserFromGroup(user.ID, @group.ID);
            CoreContext.UserManager.SaveUserInfo(user);
        }

        private static void TransferUserToDepartment(Guid userId, GroupInfo group, bool setAsManager)
        {
            if (!CoreContext.UserManager.UserExists(userId) && userId != Guid.Empty) return;

            if (setAsManager)
            {
                CoreContext.UserManager.SetDepartmentManager(@group.ID, userId);
            }
            CoreContext.UserManager.AddUserIntoGroup(userId, @group.ID);
        }
    }
}