/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Web.Community.News.Code
{
    [Serializable]
    public class FeedComment : ISecurityObject
    {
        public long Id { get; set; }

        public long FeedId { get; set; }

        public string Comment { get; set; }

        public long ParentId { get; set; }

        public DateTime Date { get; set; }

        public string Creator { get; set; }

        public bool Inactive { get; set; }

        public Feed Feed { get; set; }

        public FeedComment(long feedId)
        {
            FeedId = feedId;
            Creator = SecurityContext.CurrentAccount.ID.ToString();
            Date = TenantUtil.DateTimeNow();
            Inactive = false;
        }

        public FeedComment(long feedId, Feed feed)
        {
            FeedId = feedId;
            Creator = SecurityContext.CurrentAccount.ID.ToString();
            Date = TenantUtil.DateTimeNow();
            Inactive = false;
            Feed = feed;
        }

        public FeedComment()
        {
        }

        public bool IsRoot()
        {
            return ParentId == 0;
        }

        public List<FeedComment> SelectChildLevel(List<FeedComment> from)
        {
            return SelectChildLevel(Id, from);
        }

        public static List<FeedComment> SelectRootLevel(List<FeedComment> from)
        {
            return SelectChildLevel(0, from);
        }

        public static List<FeedComment> SelectChildLevel(long forParentId, List<FeedComment> from)
        {
            return from.FindAll(comm => comm.ParentId == forParentId);
        }

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId
        {
            get { return Id; }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId,
                                                 SecurityCallContext callContext)
        {
            var roles = new List<IRole>();

            if (Equals(account.ID, new Guid(Creator)))
            {
                roles.Add(Constants.Owner);
            }

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public bool InheritSupported
        {
            get { return false; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var c = obj as FeedComment;
            return c != null && c.Id == Id;
        }
    }
}