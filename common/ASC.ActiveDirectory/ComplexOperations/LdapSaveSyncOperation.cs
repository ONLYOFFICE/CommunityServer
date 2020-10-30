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
using System.Security.Cryptography;
using System.Text;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations.Data;
using ASC.ActiveDirectory.Novell.Exceptions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Users;

using Newtonsoft.Json;
// ReSharper disable RedundantToStringCall

namespace ASC.ActiveDirectory.ComplexOperations
{
    public class LdapSaveSyncOperation : LdapOperation
    {
        private readonly LdapChangeCollection _ldapChanges;
        private readonly UserInfo _currentUser;

        public LdapSaveSyncOperation(LdapSettings settings, Tenant tenant, LdapOperationType operation, LdapLocalization resource = null, string userId = null)
            : base(settings, tenant, operation, resource)
        {
            _ldapChanges = new LdapChangeCollection { Tenant = tenant };
            _currentUser = userId != null ? CoreContext.UserManager.GetUsers(Guid.Parse(userId)) : null;
        }

        protected override void Do()
        {
            try
            {
                if (OperationType == LdapOperationType.Save)
                {
                    SetProgress(10, Resource.LdapSettingsStatusSavingSettings);

                    LDAPSettings.IsDefault = LDAPSettings.Equals(LDAPSettings.GetDefault());

                    if (!LDAPSettings.Save())
                    {
                        Logger.Error("Can't save LDAP settings.");
                        Error = Resource.LdapSettingsErrorCantSaveLdapSettings;
                        return;
                    }
                }

                if (LDAPSettings.EnableLdapAuthentication)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("SyncLDAP()");
                        sb.AppendLine(string.Format("Server: {0}:{1}", LDAPSettings.Server, LDAPSettings.PortNumber));
                        sb.AppendLine("UserDN: " + LDAPSettings.UserDN);
                        sb.AppendLine("LoginAttr: " + LDAPSettings.LoginAttribute);
                        sb.AppendLine("UserFilter: " + LDAPSettings.UserFilter);
                        sb.AppendLine("Groups: " + LDAPSettings.GroupMembership);
                        if (LDAPSettings.GroupMembership) {
                            sb.AppendLine("GroupDN: " + LDAPSettings.GroupDN);
                            sb.AppendLine("UserAttr: " + LDAPSettings.UserAttribute);
                            sb.AppendLine("GroupFilter: " + LDAPSettings.GroupFilter);
                            sb.AppendLine("GroupName: " + LDAPSettings.GroupNameAttribute);
                            sb.AppendLine("GroupMember: " + LDAPSettings.GroupAttribute);
                        }

                        Logger.Debug(sb.ToString());
                    }

                    SyncLDAP();

                    if (!string.IsNullOrEmpty(Error))
                        return;
                }
                else
                {
                    Logger.Debug("TurnOffLDAP()");

                    TurnOffLDAP();

                    ((LdapCurrentUserPhotos)LdapCurrentUserPhotos.Load().GetDefault()).Save();

                    ((LdapCurrentAcccessSettings)LdapCurrentAcccessSettings.Load().GetDefault()).Save();
                    //не снимать права при выключении
                    //var rights = new List<LdapSettings.AccessRight>();
                    //TakeUsersRights(rights);

                    //if (rights.Count > 0)
                    //{
                    //    Warning = Resource.LdapSettingsErrorLostRights;
                    //}
                }
            }
            catch (NovellLdapTlsCertificateRequestedException ex)
            {
                Logger.ErrorFormat(
                    "CheckSettings(acceptCertificate={0}, cert thumbprint: {1}): NovellLdapTlsCertificateRequestedException: {2}",
                    LDAPSettings.AcceptCertificate, LDAPSettings.AcceptCertificateHash, ex.ToString());
                Error = Resource.LdapSettingsStatusCertificateVerification;

                //TaskInfo.SetProperty(CERT_REQUEST, ex.CertificateConfirmRequest);
            }
            catch (TenantQuotaException e)
            {
                Logger.ErrorFormat("TenantQuotaException. {0}", e.ToString());
                Error = Resource.LdapSettingsTenantQuotaSettled;
            }
            catch (FormatException e)
            {
                Logger.ErrorFormat("FormatException error. {0}", e.ToString());
                Error = Resource.LdapSettingsErrorCantCreateUsers;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Internal server error. {0}", e.ToString());
                Error = Resource.LdapSettingsInternalServerError;
            }
            finally
            {
                SetProgress(99, Resource.LdapSettingsStatusDisconnecting, "");
                Dispose();
            }

            SetProgress(100, OperationType == LdapOperationType.SaveTest ||
                             OperationType == LdapOperationType.SyncTest
                ? JsonConvert.SerializeObject(_ldapChanges)
                : "", "");
        }

        private void TurnOffLDAP()
        {
            const double percents = 48;

            SetProgress((int)percents, Resource.LdapSettingsModifyLdapUsers);

            var existingLDAPUsers = CoreContext.UserManager.GetUsers(EmployeeStatus.All).Where(u => u.Sid != null).ToList();

            var step = percents / existingLDAPUsers.Count;

            var percentage = (double)GetProgress();

            var index = 0;
            var count = existingLDAPUsers.Count;

            foreach (var existingLDAPUser in existingLDAPUsers)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource:
                        string.Format("({0}/{1}): {2}", ++index, count,
                            UserFormatter.GetUserName(existingLDAPUser, DisplayUserNameFormat.Default)));

                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        existingLDAPUser.Sid = null;
                        existingLDAPUser.ConvertExternalContactsToOrdinary();

                        Logger.DebugFormat("CoreContext.UserManager.SaveUserInfo({0})", existingLDAPUser.GetUserInfoString());

                        CoreContext.UserManager.SaveUserInfo(existingLDAPUser);
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        _ldapChanges.SetSaveAsPortalUserChange(existingLDAPUser);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                percentage += step;
            }
        }

        private void SyncLDAP()
        {
            var t = CoreContext.TenantManager.GetCurrentTenant();

            var currentDomainSettings = LdapCurrentDomain.Load();

            if (string.IsNullOrEmpty(currentDomainSettings.CurrentDomain) || currentDomainSettings.CurrentDomain != Importer.LDAPDomain)
            {
                currentDomainSettings.CurrentDomain = Importer.LDAPDomain;
                currentDomainSettings.Save();
            }

            if (!LDAPSettings.GroupMembership)
            {
                Logger.Debug("SyncLDAPUsers()");

                SyncLDAPUsers();
            }
            else
            {
                Logger.Debug("SyncLDAPUsersInGroups()");

                SyncLDAPUsersInGroups();
            }

            SyncLdapAvatar();

            SyncLdapAccessRights();
        }

        private void SyncLdapAvatar()
        {
            SetProgress(90, Resource.LdapSettingsStatusUpdatingUserPhotos);

            if (!LDAPSettings.LdapMapping.ContainsKey(LdapSettings.MappingFields.AvatarAttribute))
            {
                var ph = LdapCurrentUserPhotos.Load();

                if (ph.CurrentPhotos == null || !ph.CurrentPhotos.Any())
                {
                    return;
                }

                foreach (var guid in ph.CurrentPhotos.Keys)
                {
                    Logger.InfoFormat("SyncLdapAvatar() Removing photo for '{0}'", guid);
                    UserPhotoManager.RemovePhoto(guid);
                    UserPhotoManager.ResetThumbnailSettings(guid);
                }

                ph.CurrentPhotos = null;
                ph.Save();
                return;
            }

            var photoSettings = LdapCurrentUserPhotos.Load();

            if (photoSettings.CurrentPhotos == null)
            {
                photoSettings.CurrentPhotos = new Dictionary<Guid, string>();
            }

            var ldapUsers = Importer.AllDomainUsers.Where(x => !x.IsDisabled);
            var step = 5.0 / ldapUsers.Count();
            var currentPercent = 90.0;
            foreach (var ldapUser in ldapUsers)
            {
                var image = ldapUser.GetValue(LDAPSettings.LdapMapping[LdapSettings.MappingFields.AvatarAttribute], true);

                if (image == null || image.GetType() != typeof(byte[]))
                {
                    continue;
                }

                string hash;
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    hash = Convert.ToBase64String(md5.ComputeHash((byte[])image));
                }

                var user = CoreContext.UserManager.GetUserBySid(ldapUser.Sid);

                Logger.DebugFormat("SyncLdapAvatar() Found photo for '{0}'", ldapUser.Sid);

                if (photoSettings.CurrentPhotos.ContainsKey(user.ID) && photoSettings.CurrentPhotos[user.ID] == hash)
                {
                    Logger.Debug("SyncLdapAvatar() Same hash, skipping.");
                    continue;
                }

                try
                {
                    SetProgress((int)(currentPercent += step),
                        string.Format("{0}: {1}", Resource.LdapSettingsStatusSavingUserPhoto, UserFormatter.GetUserName(user, DisplayUserNameFormat.Default)));
                    UserPhotoManager.ResetThumbnailSettings(user.ID);
                    UserPhotoManager.SaveOrUpdatePhoto(user.ID, (byte[])image);

                    if (photoSettings.CurrentPhotos.ContainsKey(user.ID))
                    {
                        photoSettings.CurrentPhotos[user.ID] = hash;
                    }
                    else
                    {
                        photoSettings.CurrentPhotos.Add(user.ID, hash);
                    }
                }
                catch
                {
                    Logger.DebugFormat("SyncLdapAvatar() Couldn't save photo for '{0}'", user.ID);
                    if (photoSettings.CurrentPhotos.ContainsKey(user.ID))
                    {
                        photoSettings.CurrentPhotos.Remove(user.ID);
                    }
                }
            }

            photoSettings.Save();
        }

        private void SyncLdapAccessRights()
        {
            SetProgress(95, Resource.LdapSettingsStatusUpdatingAccessRights);

            var currentUserRights = new List<LdapSettings.AccessRight>();
            TakeUsersRights(_currentUser != null ? currentUserRights : null);

            if (LDAPSettings.GroupMembership && LDAPSettings.AccessRights != null && LDAPSettings.AccessRights.Count > 0)
            {
                GiveUsersRights(LDAPSettings.AccessRights, _currentUser != null ? currentUserRights : null);
            }

            if (currentUserRights.Count > 0)
            {
                Warning = Resource.LdapSettingsErrorLostRights;
            }

            LDAPSettings.Save();
        }

        private void TakeUsersRights(List<LdapSettings.AccessRight> currentUserRights)
        {
            var current = LdapCurrentAcccessSettings.Load();

            if (current.CurrentAccessRights == null || !current.CurrentAccessRights.Any())
            {
                Logger.Debug("TakeUsersRights() CurrentAccessRights is empty, skipping");
                return;
            }

            SetProgress(95, Resource.LdapSettingsStatusRemovingOldRights);
            foreach (var right in current.CurrentAccessRights)
            {
                foreach (var user in right.Value)
                {
                    var userId = Guid.Parse(user);
                    if (_currentUser != null && _currentUser.ID == userId)
                    {
                        Logger.DebugFormat("TakeUsersRights() Attempting to take admin rights from yourself `{0}`, skipping", user);
                        if (currentUserRights != null)
                        {
                            currentUserRights.Add(right.Key);
                        }
                    }
                    else
                    {
                        Logger.DebugFormat("TakeUsersRights() Taking admin rights ({0}) from '{1}'", right.Key, user);
                        Web.Core.WebItemSecurity.SetProductAdministrator(LdapSettings.AccessRightsGuids[right.Key], userId, false);
                    }
                }
            }

            current.CurrentAccessRights = null;
            current.Save();
        }

        private void GiveUsersRights(Dictionary<LdapSettings.AccessRight, string> accessRightsSettings, List<LdapSettings.AccessRight> currentUserRights)
        {
            var current = LdapCurrentAcccessSettings.Load();
            var currentAccessRights = new Dictionary<LdapSettings.AccessRight, List<string>>();
            var usersWithRightsFlat = current.CurrentAccessRights == null ? new List<string>() : current.CurrentAccessRights.SelectMany(x => x.Value).Distinct().ToList();

            var step = 3.0 / accessRightsSettings.Count();
            var currentPercent = 95.0;
            foreach (var access in accessRightsSettings)
            {
                currentPercent += step;
                var ldapGroups = Importer.FindGroupsByAttribute(LDAPSettings.GroupNameAttribute, access.Value.Split(',').Select(x => x.Trim()));

                if (!ldapGroups.Any())
                {
                    Logger.DebugFormat("GiveUsersRights() No ldap groups found for ({0}) access rights, skipping", access.Key);
                    continue;
                }

                foreach (var ldapGr in ldapGroups)
                {
                    var gr = CoreContext.UserManager.GetGroupInfoBySid(ldapGr.Sid);

                    if (gr == null)
                    {
                        Logger.DebugFormat("GiveUsersRights() Couldn't find portal group for '{0}'", ldapGr.Sid);
                        continue;
                    }

                    var users = CoreContext.UserManager.GetUsersByGroup(gr.ID);

                    Logger.DebugFormat("GiveUsersRights() Found '{0}' users for group '{1}' ({2})", users.Count(), gr.Name, gr.ID);


                    foreach (var user in users)
                    {
                        if (!user.Equals(Constants.LostUser) && !user.IsVisitor())
                        {
                            if (!usersWithRightsFlat.Contains(user.ID.ToString()))
                            {
                                usersWithRightsFlat.Add(user.ID.ToString());

                                var cleared = false;

                                foreach (var r in Enum.GetValues(typeof(LdapSettings.AccessRight)).Cast<LdapSettings.AccessRight>())
                                {
                                    var prodId = LdapSettings.AccessRightsGuids[r];

                                    if (Web.Core.WebItemSecurity.IsProductAdministrator(prodId, user.ID))
                                    {
                                        cleared = true;
                                        Web.Core.WebItemSecurity.SetProductAdministrator(prodId, user.ID, false);
                                    }
                                }

                                if (cleared) {
                                    Logger.DebugFormat("GiveUsersRights() Cleared manually added user rights for '{0}'", user.DisplayUserName());
                                }
                            }

                            if (!currentAccessRights.ContainsKey(access.Key))
                            {
                                currentAccessRights.Add(access.Key, new List<string>());
                            }
                            currentAccessRights[access.Key].Add(user.ID.ToString());

                            SetProgress((int)currentPercent,
                                string.Format(Resource.LdapSettingsStatusGivingRights, UserFormatter.GetUserName(user, DisplayUserNameFormat.Default), access.Key));
                            Web.Core.WebItemSecurity.SetProductAdministrator(LdapSettings.AccessRightsGuids[access.Key], user.ID, true);

                            if (currentUserRights != null && currentUserRights.Contains(access.Key))
                            {
                                currentUserRights.Remove(access.Key);
                            }
                        }
                    }
                }
            }

            current.CurrentAccessRights = currentAccessRights;
            current.Save();
        }

        private void SyncLDAPUsers()
        {
            SetProgress(15, Resource.LdapSettingsStatusGettingUsersFromLdap);

            var ldapUsers = Importer.GetDiscoveredUsersByAttributes();

            if (!ldapUsers.Any())
            {
                Error = Resource.LdapSettingsErrorUsersNotFound;
                return;
            }

            Logger.DebugFormat("Importer.GetDiscoveredUsersByAttributes() Success: Users count: {0}",
                Importer.AllDomainUsers.Count);

            SetProgress(20, Resource.LdapSettingsStatusRemovingOldUsers, "");

            ldapUsers = RemoveOldDbUsers(ldapUsers);

            SetProgress(30,
                OperationType == LdapOperationType.Save || OperationType == LdapOperationType.SaveTest
                    ? Resource.LdapSettingsStatusSavingUsers
                    : Resource.LdapSettingsStatusSyncingUsers,
                "");

            SyncDbUsers(ldapUsers);

            SetProgress(70, Resource.LdapSettingsStatusRemovingOldGroups, "");

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

            Logger.DebugFormat("Importer.GetDiscoveredGroupsByAttributes() Success: Groups count: {0}",
                Importer.AllDomainGroups.Count);

            SetProgress(20, Resource.LdapSettingsStatusGettingUsersFromLdap);

            //Get All found groups users
            List<UserInfo> uniqueLdapGroupUsers;

            var ldapGroupsUsers = GetGroupsUsers(ldapGroups, out uniqueLdapGroupUsers);

            if (!uniqueLdapGroupUsers.Any())
            {
                Error = Resource.LdapSettingsErrorUsersNotFound;
                return;
            }

            Logger.DebugFormat("GetGroupsUsers() Success: Users count: {0}",
                Importer.AllDomainUsers.Count);

            SetProgress(30,
                OperationType == LdapOperationType.Save || OperationType == LdapOperationType.SaveTest
                    ? Resource.LdapSettingsStatusSavingUsers
                    : Resource.LdapSettingsStatusSyncingUsers,
                "");

            var newUniqueLdapGroupUsers = SyncGroupsUsers(uniqueLdapGroupUsers);

            SetProgress(60, Resource.LdapSettingsStatusSavingGroups, "");

            SyncDbGroups(ldapGroupsUsers);

            SetProgress(80, Resource.LdapSettingsStatusRemovingOldGroups, "");

            RemoveOldDbGroups(ldapGroups);

            SetProgress(90, Resource.LdapSettingsStatusRemovingOldUsers, "");

            RemoveOldDbUsers(newUniqueLdapGroupUsers);
        }

        private void SyncDbGroups(Dictionary<GroupInfo, List<UserInfo>> ldapGroupsWithUsers)
        {
            const double percents = 20;

            var step = percents / ldapGroupsWithUsers.Count;

            var percentage = (double)GetProgress();

            if (!ldapGroupsWithUsers.Any())
                return;

            var gIndex = 0;
            var gCount = ldapGroupsWithUsers.Count;

            foreach (var ldapGroupWithUsers in ldapGroupsWithUsers)
            {
                var ldapGroup = ldapGroupWithUsers.Key;

                var ldapGroupUsers = ldapGroupWithUsers.Value;

                ++gIndex;

                SetProgress(Convert.ToInt32(percentage),
                    currentSource:
                        string.Format("({0}/{1}): {2}", gIndex,
                            gCount, ldapGroup.Name));

                var dbLdapGroup = CoreContext.UserManager.GetGroupInfoBySid(ldapGroup.Sid);

                if (Equals(dbLdapGroup, Constants.LostGroupInfo))
                {
                    AddNewGroup(ldapGroup, ldapGroupUsers, gIndex, gCount);
                }
                else
                {
                    UpdateDbGroup(dbLdapGroup, ldapGroup, ldapGroupUsers, gIndex, gCount);
                }

                percentage += step;
            }
        }

        private void AddNewGroup(GroupInfo ldapGroup, List<UserInfo> ldapGroupUsers, int gIndex, int gCount)
        {
            if (!ldapGroupUsers.Any()) // Skip empty groups
            {
                if (OperationType == LdapOperationType.SaveTest ||
                    OperationType == LdapOperationType.SyncTest)
                {
                    _ldapChanges.SetSkipGroupChange(ldapGroup);
                }

                return;
            }

            var groupMembersToAdd =
                ldapGroupUsers.Select(ldapGroupUser => SearchDbUserBySid(ldapGroupUser.Sid))
                    .Where(userBySid => !Equals(userBySid, Constants.LostUser))
                    .ToList();

            if (groupMembersToAdd.Any())
            {
                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        ldapGroup = CoreContext.UserManager.SaveGroupInfo(ldapGroup);

                        var index = 0;
                        var count = groupMembersToAdd.Count;

                        foreach (var userBySid in groupMembersToAdd)
                        {
                            SetProgress(
                                currentSource:
                                    string.Format("({0}/{1}): {2}, {3} ({4}/{5}): {6}", gIndex,
                                        gCount, ldapGroup.Name,
                                        Resource.LdapSettingsStatusAddingGroupUser,
                                        ++index, count,
                                        UserFormatter.GetUserName(userBySid, DisplayUserNameFormat.Default)));

                            CoreContext.UserManager.AddUserIntoGroup(userBySid.ID, ldapGroup.ID);
                        }
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        _ldapChanges.SetAddGroupChange(ldapGroup);
                        _ldapChanges.SetAddGroupMembersChange(ldapGroup, groupMembersToAdd);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (OperationType == LdapOperationType.SaveTest ||
                    OperationType == LdapOperationType.SyncTest)
                {
                    _ldapChanges.SetSkipGroupChange(ldapGroup);
                }
            }
        }

        private static bool NeedUpdateGroup(GroupInfo portalGroup, GroupInfo ldapGroup)
        {
            var needUpdate =
                !portalGroup.Name.Equals(ldapGroup.Name, StringComparison.InvariantCultureIgnoreCase) ||
                !portalGroup.Sid.Equals(ldapGroup.Sid, StringComparison.InvariantCultureIgnoreCase);

            return needUpdate;
        }

        private void UpdateDbGroup(GroupInfo dbLdapGroup, GroupInfo ldapGroup, List<UserInfo> ldapGroupUsers, int gIndex,
            int gCount)
        {
            SetProgress(currentSource:
                string.Format("({0}/{1}): {2}", gIndex, gCount, ldapGroup.Name));

            var dbGroupMembers =
                        CoreContext.UserManager.GetUsersByGroup(dbLdapGroup.ID, EmployeeStatus.All)
                            .Where(u => u.Sid != null)
                            .ToList();

            var groupMembersToRemove =
                dbGroupMembers.Where(
                    dbUser => ldapGroupUsers.FirstOrDefault(lu => dbUser.Sid.Equals(lu.Sid)) == null).ToList();

            var groupMembersToAdd = (from ldapGroupUser in ldapGroupUsers
                                     let dbUser = dbGroupMembers.FirstOrDefault(u => u.Sid.Equals(ldapGroupUser.Sid))
                                     where dbUser == null
                                     select SearchDbUserBySid(ldapGroupUser.Sid)
                                         into userBySid
                                         where !Equals(userBySid, Constants.LostUser)
                                         select userBySid)
                .ToList();

            switch (OperationType)
            {
                case LdapOperationType.Save:
                case LdapOperationType.Sync:
                    if (NeedUpdateGroup(dbLdapGroup, ldapGroup))
                    {
                        dbLdapGroup.Name = ldapGroup.Name;
                        dbLdapGroup.Sid = ldapGroup.Sid;

                        dbLdapGroup = CoreContext.UserManager.SaveGroupInfo(dbLdapGroup);
                    }

                    var index = 0;
                    var count = groupMembersToRemove.Count;

                    foreach (var dbUser in groupMembersToRemove)
                    {
                        SetProgress(
                            currentSource:
                                string.Format("({0}/{1}): {2}, {3} ({4}/{5}): {6}", gIndex, gCount,
                                    dbLdapGroup.Name,
                                    Resource.LdapSettingsStatusRemovingGroupUser,
                                    ++index, count,
                                    UserFormatter.GetUserName(dbUser, DisplayUserNameFormat.Default)));

                        CoreContext.UserManager.RemoveUserFromGroup(dbUser.ID, dbLdapGroup.ID);
                    }

                    index = 0;
                    count = groupMembersToAdd.Count;

                    foreach (var userInfo in groupMembersToAdd)
                    {
                        SetProgress(
                            currentSource:
                                string.Format("({0}/{1}): {2}, {3} ({4}/{5}): {6}", gIndex, gCount,
                                    ldapGroup.Name,
                                    Resource.LdapSettingsStatusAddingGroupUser,
                                    ++index, count,
                                    UserFormatter.GetUserName(userInfo, DisplayUserNameFormat.Default)));

                        CoreContext.UserManager.AddUserIntoGroup(userInfo.ID, dbLdapGroup.ID);
                    }

                    if (dbGroupMembers.All(dbUser => groupMembersToRemove.Exists(u => u.ID.Equals(dbUser.ID)))
                        && !groupMembersToAdd.Any())
                    {
                        SetProgress(currentSource:
                            string.Format("({0}/{1}): {2}", gIndex, gCount, dbLdapGroup.Name));

                        CoreContext.UserManager.DeleteGroup(dbLdapGroup.ID);
                    }

                    break;
                case LdapOperationType.SaveTest:
                case LdapOperationType.SyncTest:
                    if (NeedUpdateGroup(dbLdapGroup, ldapGroup))
                        _ldapChanges.SetUpdateGroupChange(ldapGroup);

                    if (groupMembersToRemove.Any())
                        _ldapChanges.SetRemoveGroupMembersChange(dbLdapGroup, groupMembersToRemove);

                    if (groupMembersToAdd.Any())
                        _ldapChanges.SetAddGroupMembersChange(dbLdapGroup, groupMembersToAdd);

                    if (dbGroupMembers.All(dbUser => groupMembersToRemove.Exists(u => u.ID.Equals(dbUser.ID)))
                        && !groupMembersToAdd.Any())
                    {
                        _ldapChanges.SetRemoveGroupChange(dbLdapGroup, Logger);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static UserInfo SearchDbUserBySid(string sid)
        {
            if (string.IsNullOrEmpty(sid))
                return Constants.LostUser;

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

            var index = 0;
            var count = ldapUsers.Count;

            foreach (var userInfo in ldapUsers)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource:
                        string.Format("({0}/{1}): {2}", ++index, count,
                            UserFormatter.GetUserName(userInfo, DisplayUserNameFormat.Default)));

                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        LDAPUserManager.SyncLDAPUser(userInfo, ldapUsers);
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        LdapChangeCollection changes;
                        LDAPUserManager.GetLDAPSyncUserChange(userInfo, ldapUsers, out changes);
                        _ldapChanges.AddRange(changes);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                percentage += step;
            }
        }

        /// <summary>
        /// Remove old LDAP users from db
        /// </summary>
        /// <param name="ldapUsers">list of actual LDAP users</param>
        /// <returns>New list of actual LDAP users</returns>
        private List<UserInfo> RemoveOldDbUsers(List<UserInfo> ldapUsers)
        {
            var dbLdapUsers = CoreContext.UserManager.GetUsers(EmployeeStatus.All).Where(u => u.Sid != null).ToList();

            if (!dbLdapUsers.Any())
                return ldapUsers;

            var removedUsers =
                dbLdapUsers.Where(u => ldapUsers.FirstOrDefault(lu => u.Sid.Equals(lu.Sid)) == null).ToList();

            if (!removedUsers.Any())
                return ldapUsers;

            const double percents = 8;

            var step = percents / removedUsers.Count;

            var percentage = (double)GetProgress();

            var index = 0;
            var count = removedUsers.Count;

            foreach (var removedUser in removedUsers)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource:
                        string.Format("({0}/{1}): {2}", ++index, count,
                            UserFormatter.GetUserName(removedUser, DisplayUserNameFormat.Default)));

                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        removedUser.Sid = null;
                        if (!removedUser.IsOwner() && !(_currentUser != null && _currentUser.ID == removedUser.ID && removedUser.IsAdmin()))
                        {
                            removedUser.Status = EmployeeStatus.Terminated; // Disable user on portal
                        }
                        else
                        {
                            Warning = Resource.LdapSettingsErrorRemovedYourself;
                            Logger.DebugFormat("RemoveOldDbUsers() Attempting to exclude yourself `{0}` from group or user filters, skipping.", removedUser.ID);
                        }

                        removedUser.ConvertExternalContactsToOrdinary();

                        Logger.DebugFormat("CoreContext.UserManager.SaveUserInfo({0})", removedUser.GetUserInfoString());

                        CoreContext.UserManager.SaveUserInfo(removedUser);
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        _ldapChanges.SetSaveAsPortalUserChange(removedUser);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                percentage += step;
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

            var index = 0;
            var count = removedDbLdapGroups.Count;

            foreach (var groupInfo in removedDbLdapGroups)
            {
                SetProgress(Convert.ToInt32(percentage),
                    currentSource: string.Format("({0}/{1}): {2}", ++index, count, groupInfo.Name));

                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        CoreContext.UserManager.DeleteGroup(groupInfo.ID);
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        _ldapChanges.SetRemoveGroupChange(groupInfo);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                percentage += step;
            }
        }

        private List<UserInfo> SyncGroupsUsers(List<UserInfo> uniqueLdapGroupUsers)
        {
            const double percents = 30;

            var step = percents / uniqueLdapGroupUsers.Count;

            var percentage = (double)GetProgress();

            var newUniqueLdapGroupUsers = new List<UserInfo>();

            var index = 0;
            var count = uniqueLdapGroupUsers.Count;

            int i, len;
            for (i = 0, len = uniqueLdapGroupUsers.Count; i < len; i++)
            {
                var ldapGroupUser = uniqueLdapGroupUsers[i];

                SetProgress(Convert.ToInt32(percentage),
                    currentSource:
                        string.Format("({0}/{1}): {2}", ++index, count,
                            UserFormatter.GetUserName(ldapGroupUser, DisplayUserNameFormat.Default)));

                UserInfo user;
                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.Sync:
                        user = LDAPUserManager.SyncLDAPUser(ldapGroupUser, uniqueLdapGroupUsers);
                        if (!Equals(user, Constants.LostUser))
                            newUniqueLdapGroupUsers.Add(user);
                        break;
                    case LdapOperationType.SaveTest:
                    case LdapOperationType.SyncTest:
                        LdapChangeCollection changes;
                        user = LDAPUserManager.GetLDAPSyncUserChange(ldapGroupUser, uniqueLdapGroupUsers, out changes);
                        if (!Equals(user, Constants.LostUser))
                        {
                            newUniqueLdapGroupUsers.Add(user);
                        }
                        _ldapChanges.AddRange(changes);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                percentage += step;
            }

            return newUniqueLdapGroupUsers;
        }

        private Dictionary<GroupInfo, List<UserInfo>> GetGroupsUsers(List<GroupInfo> ldapGroups,
            out List<UserInfo> uniqueLdapGroupUsers)
        {
            uniqueLdapGroupUsers = new List<UserInfo>();

            var listGroupsUsers = new Dictionary<GroupInfo, List<UserInfo>>();

            foreach (var ldapGroup in ldapGroups)
            {
                var ldapGroupUsers = Importer.GetGroupUsers(ldapGroup);

                listGroupsUsers.Add(ldapGroup, ldapGroupUsers);

                foreach (var ldapGroupUser in ldapGroupUsers)
                {
                    if (!uniqueLdapGroupUsers.Any(u => u.Sid.Equals(ldapGroupUser.Sid)))
                    {
                        uniqueLdapGroupUsers.Add(ldapGroupUser);
                    }
                }
            }

            return listGroupsUsers;
        }
    }
}