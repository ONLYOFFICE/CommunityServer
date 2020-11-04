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
using ASC.Core.Users;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using System.Collections.Generic;
using ASC.Core;

namespace ASC.Blogs.Core.Security
{
    public class CorporateBlogSecObject : SecurityObjectId, ISecurityObject
    {

        private GroupInfo groupInfo;

        public CorporateBlogSecObject(GroupInfo groupInfo)
            : base(groupInfo.ID, typeof(CorporateBlogSecObject))
        {
            this.groupInfo = groupInfo;
        }

        public override string ToString()
        {
            return string.Format("corpblog for {0}", groupInfo.Name);
        }

        #region ISecurityObjectProvider Members

        public bool InheritSupported
        {
            get { return false; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();

            var userGroups = new List<GroupInfo>(
                CoreContext.UserManager.GetUserGroups(account.ID, IncludeType.Distinct | IncludeType.InChild)
            );

            if (userGroups.Contains(groupInfo))
            {
                roles.Add(ASC.Common.Security.Authorizing.Constants.Member);
            }

            return roles;
        }

        #endregion
    }
}
