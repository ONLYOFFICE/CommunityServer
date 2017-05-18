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
using System.Linq;
using ASC.ActiveDirectory;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Users;
using Resources;

namespace ASC.Web.Studio.Core.Import.LDAP
{
    public class LDAPSaveSyncOperation : LDAPOperation
    {
        private readonly bool _syncOnly;

        public override LDAPOperationType OperationType
        {
            get { return _syncOnly ? LDAPOperationType.Sync : LDAPOperationType.Save; }
        }

        public LDAPSaveSyncOperation(LDAPSupportSettings settings, Tenant tenant = null, bool syncOnly = false, bool? acceptCertificate = null)
            : base(settings, tenant, acceptCertificate)
        {
            _syncOnly = syncOnly;
        }

        protected override void Do()
        {
            try
            {
                RemoveOldWorkaroundOnLogoutLDAPUsers();

                if (LDAPSettings.EnableLdapAuthentication)
                {
                    SyncLDAP();

                    if (!string.IsNullOrEmpty(Error))
                        return;
                }
                else
                {
                    SetProgress(50, Resource.LdapSettingsModifyLdapUsers);

                    var existingLDAPUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToList();
                    foreach (var existingLDAPUser in existingLDAPUsers)
                    {
                        existingLDAPUser.Sid = null;
                        CoreContext.UserManager.SaveUserInfo(existingLDAPUser);
                    }
                }

                SetProgress(100);
            }
            catch (TenantQuotaException e)
            {
                Logger.ErrorFormat("TenantQuotaException. {0}", e);
                Error = Resource.LdapSettingsTenantQuotaSettled;
            }
            catch (FormatException e)
            {
                Logger.ErrorFormat("FormatException error. {0}", e);
                Error = Resource.LdapSettingsErrorCantCreateUsers;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Internal server error. {0}", e);
                Error = Resource.LdapSettingsInternalServerError;
            }
        }

        /// <summary>
        /// Remove old workaround: Add L to sid for invalidate cookie
        /// </summary>
        private static void RemoveOldWorkaroundOnLogoutLDAPUsers()
        {
            var users = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null && u.Sid.StartsWith("l")).ToArray();
            foreach (var userInfo in users)
            {
                userInfo.Sid = userInfo.Sid.Remove(0, 1);
                CoreContext.UserManager.SaveUserInfo(userInfo);
            }
        }

        private void SyncLDAP()
        {
            if (!LDAPSettings.GroupMembership)
            {
                SyncLDAPUsers();
            }
            else
            {
                SyncLDAPUsersInGroups();
            }
        }

        private void SyncLDAPUsers()
        {
            SetProgress(15, Resource.LdapSettingsStatusGettingUsersFromLdap);

            var ldapUsers = Importer.GetDiscoveredUsersByAttributes()
                                 .ConvertAll(u =>
                                 {
                                     if (string.IsNullOrEmpty(u.FirstName))
                                         u.FirstName = Resource.FirstName;

                                     if (string.IsNullOrEmpty(u.LastName))
                                         u.LastName = Resource.LastName;

                                     return u;
                                 })
                                 .SortByUserName()
                                 .ToList();

            if (!ldapUsers.Any())
            {
                Error = Resource.LdapSettingsErrorUsersNotFound;
                return;
            }

            SetProgress(20, Resource.LdapSettingsStatusRemovingOldUsers);

            ldapUsers = RemoveOldDbUsers(ldapUsers);

            SetProgress(30, Resource.LdapSettingsStatusSavingUsers);

            SyncDbUsers(ldapUsers);

            SetProgress(70, Resource.LdapSettingsStatusRemovingOldGroups);

            RemoveOldDbGroups(new List<GroupInfo>()); // Remove all db groups with sid
        }

        private void SyncLDAPUsersInGroups()
        {
            SetProgress(15, Resource.LdapSettingsStatusGettingGroupsFromLdap);

            var ldapGroups = Importer.GetDiscoveredGroupsByAttributes();

            if (!ldapGroups.Any())
            {
                Error = Resource.LdapSettingsErrorGroupsNotFound;
                return;
            }

            SetProgress(20, Resource.LdapSettingsStatusGettingUsersFromLdap);

            //Get All found groups users
            List<UserInfo> uniqueLdapGroupUsers;

            var ldapGroupsUsers = GetGroupsUsers(ldapGroups, out uniqueLdapGroupUsers);

            uniqueLdapGroupUsers = uniqueLdapGroupUsers
                .ConvertAll(u =>
                {
                    if (u.FirstName == string.Empty)
                        u.FirstName = Resource.FirstName;

                    if (u.LastName == string.Empty)
                        u.LastName = Resource.LastName;

                    return u;
                })
                .SortByUserName()
                .ToList();

            if (!uniqueLdapGroupUsers.Any())
            {
                Error = Resource.LdapSettingsErrorUsersNotFound;
                return;
            }

            SetProgress(30, Resource.LdapSettingsStatusSavingUsers);

            SyncGroupsUsers(uniqueLdapGroupUsers);

            SetProgress(60, Resource.LdapSettingsStatusSavingGroups);

            SyncDbGroups(ldapGroupsUsers);

            SetProgress(80, Resource.LdapSettingsStatusRemovingOldGroups);

            RemoveOldDbGroups(ldapGroups);

            SetProgress(90, Resource.LdapSettingsStatusRemovingOldUsers);

            RemoveOldDbUsers(uniqueLdapGroupUsers);
        }

        private void SyncDbGroups(Dictionary<GroupInfo, List<UserInfo>> ldapGroupsWithUsers)
        {
            const double percents = 20;

            var step = percents / ldapGroupsWithUsers.Count;

            var percentage = (double)GetProgress();

            if (!ldapGroupsWithUsers.Any())
                return;

            foreach (var ldapGroupWithUsers in ldapGroupsWithUsers)
            {
                var ldapGroup = ldapGroupWithUsers.Key;

                var ldapGroupUsers = ldapGroupWithUsers.Value;

                SetProgress(Convert.ToInt32(percentage), currentSource: ldapGroup.Name);

                var dbLdapGroup = CoreContext.UserManager.GetGroupInfoBySid(ldapGroup.Sid);

                IEnumerable<UserInfo> groupMembersToAdd;

                if (Equals(dbLdapGroup, ASC.Core.Users.Constants.LostGroupInfo))
                {
                    if (ldapGroupUsers.Any()) // Skip empty groups
                    {
                        groupMembersToAdd = ldapGroupUsers.Select(ldapGroupUser => SearchDbUserBySid(ldapGroupUser.Sid))
                                                          .Where(
                                                              userBySid =>
                                                              !Equals(userBySid, ASC.Core.Users.Constants.LostUser))
                                                              .ToList();

                        if (groupMembersToAdd.Any())
                        {
                            ldapGroup = CoreContext.UserManager.SaveGroupInfo(ldapGroup);

                            foreach (var userBySid in groupMembersToAdd)
                            {
                                SetProgress(currentSource: string.Format("Group: {0} \\ Adding user: {1}", ldapGroup.Name, userBySid.DisplayUserName()));

                                CoreContext.UserManager.AddUserIntoGroup(userBySid.ID, ldapGroup.ID);
                            }
                        }
                    }
                }
                else
                {
                    if (!dbLdapGroup.Name.Equals(ldapGroup.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Update group name
                        dbLdapGroup.Name = ldapGroup.Name;

                        SetProgress(currentSource: string.Format("Saving group: {0}", ldapGroup.Name));

                        CoreContext.UserManager.SaveGroupInfo(dbLdapGroup);
                    }

                    var dbGroupMembers =
                        CoreContext.UserManager.GetUsersByGroup(dbLdapGroup.ID).Where(u => u.Sid != null).ToList();

                    RemoveOldDbGroupMembers(dbLdapGroup, dbGroupMembers, ldapGroupUsers);

                    groupMembersToAdd = from ldapGroupUser in ldapGroupUsers
                                        let dbUser = dbGroupMembers.FirstOrDefault(
                                            u =>
                                            u.Sid.Equals(ldapGroupUser.Sid))
                                        where dbUser == null
                                        select SearchDbUserBySid(ldapGroupUser.Sid)
                                        into userBySid
                                        where !Equals(userBySid, ASC.Core.Users.Constants.LostUser)
                                        select userBySid;

                    foreach (var userInfo in groupMembersToAdd)
                    {
                        SetProgress(currentSource: string.Format("Group: {0} \\ Adding user: {1}", ldapGroup.Name, userInfo.DisplayUserName()));

                        CoreContext.UserManager.AddUserIntoGroup(userInfo.ID, dbLdapGroup.ID);
                    }

                    RemoveDbGroupIfEmpty(dbLdapGroup);
                }

                percentage += step;
            }
        }

        private void RemoveOldDbGroupMembers(GroupInfo groupInfo, IEnumerable<UserInfo> dbGroupMembers, IEnumerable<UserInfo> ldapGroupMembers)
        {
            var removedGroupMembers =
                    dbGroupMembers.Where(
                        dbUser => ldapGroupMembers
                                      .FirstOrDefault(lu => dbUser.Sid.Equals(lu.Sid)) == null);

            foreach (var dbUser in removedGroupMembers)
            {
                SetProgress(currentSource: string.Format("Group: {0} \\ Removing user: {1}", groupInfo.Name, dbUser.DisplayUserName()));

                CoreContext.UserManager.RemoveUserFromGroup(dbUser.ID, groupInfo.ID);
            }
        }

        private void RemoveDbGroupIfEmpty(GroupInfo groupInfo)
        {
            var dbGroupMembers = CoreContext.UserManager.GetUsersByGroup(groupInfo.ID).ToList();

            if (dbGroupMembers.Any())
                return;

            SetProgress(currentSource: string.Format("Removing group: {0}", groupInfo.Name));

            CoreContext.UserManager.DeleteGroup(groupInfo.ID);
        }

        private static UserInfo SearchDbUserBySid(string sid)
        {
            if (string.IsNullOrEmpty(sid))
                return ASC.Core.Users.Constants.LostUser;

            var foundUser = CoreContext.UserManager.GetUserBySid(sid);

            return foundUser;
        }

        private void SyncDbUsers(List<UserInfo> ldapUsers)
        {
            const double percents = 35;

            var step = percents / ldapUsers.Count;

            var percentage = (double)GetProgress();

            if (!ldapUsers.Any())
                return;

            foreach (var userInfo in ldapUsers)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource: string.Format("Syncing user: {0} {1}", userInfo.FirstName, userInfo.LastName));

                UserManagerWrapper.SyncUserLDAP(userInfo);

                percentage += step;

                SetProgress(Convert.ToInt32(percentage));
            }
        }

        /// <summary>
        /// Remove old LDAP users from db
        /// </summary>
        /// <param name="ldapUsers">list of actual LDAP users</param>
        /// <returns>New list of actual LDAP users</returns>
        private List<UserInfo> RemoveOldDbUsers(List<UserInfo> ldapUsers)
        {
            var dbLdapUsers = CoreContext.UserManager.GetUsers().Where(u => u.Sid != null).ToList();

            if (!dbLdapUsers.Any())
                return ldapUsers;

            var removedUsers =
                dbLdapUsers
                    .Where(u => ldapUsers
                                    .FirstOrDefault(lu =>
                                                    u.Sid.Equals(lu.Sid)) == null)
                    .ToList();

            if (!removedUsers.Any())
                return ldapUsers;

            const double percents = 10;

            var step = percents / removedUsers.Count;

            var percentage = (double) GetProgress();

            foreach (var removedUser in removedUsers)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource: string.Format("Removing user: {0}", removedUser.DisplayUserName()));

                if (!removedUser.IsOwner())
                {
                    CoreContext.UserManager.DeleteUser(removedUser.ID);
                }
                else
                {
                    removedUser.Sid = null;
                    CoreContext.UserManager.SaveUserInfo(removedUser);
                }

                percentage += step;

                SetProgress(Convert.ToInt32(percentage));
            }

            dbLdapUsers.RemoveAll(removedUsers.Contains);

            var newLdapUsers = ldapUsers.Where(u => !removedUsers.Exists(ru => ru.ID.Equals(u.ID))).ToList();

            return newLdapUsers;
        }

        private void RemoveOldDbGroups(List<GroupInfo> ldapGroups)
        {
            var percentage = (double)GetProgress();

            var removedDbLdapGroups =
                CoreContext.UserManager.GetGroups()
                    .Where(g => g.Sid != null && ldapGroups.FirstOrDefault(lg => g.Sid.Equals(lg.Sid)) == null)
                    .ToList();

            if (!removedDbLdapGroups.Any())
                return;

            const double percents = 10;

            var step = percents / removedDbLdapGroups.Count;

            foreach (var groupInfo in removedDbLdapGroups)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource: string.Format("Removing group: {0}", groupInfo.Name));

                CoreContext.UserManager.DeleteGroup(groupInfo.ID);

                percentage += step;

                SetProgress(Convert.ToInt32(percentage));    
            }
        }

        private void SyncGroupsUsers(List<UserInfo> uniqueLdapGroupUsers)
        {
            const double percents = 30;

            var step = percents / uniqueLdapGroupUsers.Count;

            var percentage = (double)GetProgress();

            int i, len;
            for (i = 0, len = uniqueLdapGroupUsers.Count; i < len; i++)
            {
                var ldapGroupUser = uniqueLdapGroupUsers[i];

                SetProgress(Convert.ToInt32(percentage),
                    currentSource: string.Format("Syncing user: {0} {1}", ldapGroupUser.FirstName, ldapGroupUser.LastName));

                uniqueLdapGroupUsers[i] = UserManagerWrapper.SyncUserLDAP(ldapGroupUser);

                percentage += step;
            }
        }

        private Dictionary<GroupInfo, List<UserInfo>> GetGroupsUsers(List<GroupInfo> ldapGroups, out List<UserInfo> uniqueLdapGroupUsers)
        {
            uniqueLdapGroupUsers = new List<UserInfo>();

            var listGroupsUsers = new Dictionary<GroupInfo, List<UserInfo>>();

            int i, len;
            for (i = 0, len = ldapGroups.Count; i < len; i++)
            {
                var ldapGroup = ldapGroups[i];

                var ldapGroupUsers = Importer.GetGroupUsers(ldapGroup);

                listGroupsUsers.Add(ldapGroup, ldapGroupUsers);

                List<UserInfo> groupUsers = uniqueLdapGroupUsers;
                var users = ldapGroupUsers.Where(u => !groupUsers.Contains(u)).ToList();
                if (users.Any())
                    uniqueLdapGroupUsers.AddRange(users);
            }

            return listGroupsUsers;
        }
    }
}