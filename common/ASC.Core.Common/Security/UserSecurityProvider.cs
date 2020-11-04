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

namespace ASC.Core.Users
{
    public class UserSecurityProvider : ISecurityObject
    {
        public Type ObjectType
        {
            get;
            private set;
        }

        public object SecurityId
        {
            get;
            private set;
        }


        public UserSecurityProvider(Guid userId)
        {
            SecurityId = userId;
            ObjectType = typeof(UserInfo);
        }


        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (account.ID.Equals(objectId.SecurityId))
            {
                roles.Add(ASC.Common.Security.Authorizing.Constants.Self);
            }
            return roles;
        }

        public bool InheritSupported
        {
            get { return false; }
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }
    }
}