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