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
using System.ComponentModel;
using System.Linq;
using ASC.Common.Logging;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.ActiveDirectory.ComplexOperations.Data
{
    public class LdapChangeCollection : List<LdapChange>
    {
        public Tenant Tenant { get; set; }

        #region User

        public void SetSkipUserChange(UserInfo user)
        {
            var change = new LdapChange(user.Sid,
                UserFormatter.GetUserName(user, DisplayUserNameFormat.Default),
                user.Email,
                LdapChangeType.User, LdapChangeAction.Skip);

            Add(change);
        }

        public void SetSaveAsPortalUserChange(UserInfo user)
        {
            var fieldChanges = new List<LdapItemChange>
            {
                new LdapItemChange(LdapItemChangeKey.Sid, user.Sid, null)
            };

            var change = new LdapChange(user.Sid,
                UserFormatter.GetUserName(user, DisplayUserNameFormat.Default),
                user.Email, LdapChangeType.User, LdapChangeAction.SaveAsPortal, fieldChanges);

            Add(change);
        }

        public void SetNoneUserChange(UserInfo user)
        {
            var change = new LdapChange(user.Sid,
                        UserFormatter.GetUserName(user, DisplayUserNameFormat.Default), user.Email,
                        LdapChangeType.User, LdapChangeAction.None);

            Add(change);
        }

        public void SetUpdateUserChange(UserInfo beforeUserInfo, UserInfo afterUserInfo, ILog log = null)
        {
            var fieldChanges =
                            LdapUserMapping.Fields.Select(field => GetPropChange(field, beforeUserInfo, afterUserInfo, log))
                                .Where(pch => pch != null)
                                .ToList();

            var change = new LdapChange(beforeUserInfo.Sid,
                UserFormatter.GetUserName(afterUserInfo, DisplayUserNameFormat.Default), afterUserInfo.Email,
                LdapChangeType.User, LdapChangeAction.Update, fieldChanges);

            Add(change);
        }

        public void SetAddUserChange(UserInfo user, ILog log = null)
        {
            var fieldChanges =
                        LdapUserMapping.Fields.Select(field => GetPropChange(field, after: user, log: log))
                            .Where(pch => pch != null)
                            .ToList();

            var change = new LdapChange(user.Sid,
                UserFormatter.GetUserName(user, DisplayUserNameFormat.Default), user.Email,
                LdapChangeType.User, LdapChangeAction.Add, fieldChanges);

            Add(change);
        }

        public void SetRemoveUserChange(UserInfo user)
        {
            var change = new LdapChange(user.Sid,
                                UserFormatter.GetUserName(user, DisplayUserNameFormat.Default), user.Email,
                                LdapChangeType.User, LdapChangeAction.Remove);

            Add(change);
        }
        #endregion

        #region Group

        public void SetAddGroupChange(GroupInfo group, ILog log = null)
        {
            var fieldChanges = new List<LdapItemChange>
                                    {
                                        new LdapItemChange(LdapItemChangeKey.Name, null, group.Name),
                                        new LdapItemChange(LdapItemChangeKey.Sid, null, group.Sid)
                                    };

            var change = new LdapChange(group.Sid, group.Name,
                LdapChangeType.Group, LdapChangeAction.Add, fieldChanges);

            Add(change);
        }

        public void SetAddGroupMembersChange(GroupInfo group,
            List<UserInfo> members)
        {
            var fieldChanges =
                members.Select(
                    member =>
                        new LdapItemChange(LdapItemChangeKey.Member, null,
                            UserFormatter.GetUserName(member, DisplayUserNameFormat.Default))).ToList();

            var change = new LdapChange(group.Sid, group.Name,
                LdapChangeType.Group, LdapChangeAction.AddMember, fieldChanges);

            Add(change);
        }

        public void SetSkipGroupChange(GroupInfo group)
        {
            var change = new LdapChange(group.Sid, group.Name, LdapChangeType.Group,
                LdapChangeAction.Skip);

            Add(change);
        }

        public void SetUpdateGroupChange(GroupInfo group)
        {
            var fieldChanges = new List<LdapItemChange>
                                {
                                    new LdapItemChange(LdapItemChangeKey.Name, group.Name, group.Name)
                                };

            var change = new LdapChange(group.Sid, group.Name,
                LdapChangeType.Group, LdapChangeAction.Update, fieldChanges);

            Add(change);
        }

        public void SetRemoveGroupChange(GroupInfo group, ILog log = null)
        {
            var change = new LdapChange(group.Sid, group.Name,
                            LdapChangeType.Group, LdapChangeAction.Remove);

            Add(change);
        }

        public void SetRemoveGroupMembersChange(GroupInfo group,
            List<UserInfo> members)
        {
            var fieldChanges =
                members.Select(
                    member =>
                        new LdapItemChange(LdapItemChangeKey.Member, null,
                            UserFormatter.GetUserName(member, DisplayUserNameFormat.Default))).ToList();

            var change = new LdapChange(group.Sid, group.Name,
                LdapChangeType.Group, LdapChangeAction.RemoveMember, fieldChanges);

            Add(change);
        }

        #endregion

        private static LdapItemChange GetPropChange(string propName, UserInfo before = null, UserInfo after = null, ILog log = null)
        {
            try
            {
                var valueSrc = before != null
                    ? before.GetType().GetProperty(propName).GetValue(before, null) as string
                    : "";
                var valueDst = after != null
                    ? after.GetType().GetProperty(propName).GetValue(before, null) as string
                    : "";

                LdapItemChangeKey key;
                if (!Enum.TryParse(propName, out key))
                    throw new InvalidEnumArgumentException(propName);

                var change = new LdapItemChange(key, valueSrc, valueDst);

                return change;
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.ErrorFormat("GetPropChange({0}) error: {1}", propName, ex);
            }

            return null;
        }
    }
}