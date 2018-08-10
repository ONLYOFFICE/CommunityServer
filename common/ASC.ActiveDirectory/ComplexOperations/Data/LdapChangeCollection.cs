/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.ComponentModel;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;

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