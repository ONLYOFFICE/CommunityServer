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

namespace ASC.Bookmarking.Business.Permissions
{
    public class BookmarkPermissionSecurityObject : ISecurityObject
    {
        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId { get; set; }

        public Guid CreatorID { get; set; }


        public BookmarkPermissionSecurityObject(Guid userID, Guid id)
        {
            CreatorID = userID;
            SecurityId = id;
        }

        public BookmarkPermissionSecurityObject(Guid userID)
            : this(userID, Guid.NewGuid())
        {
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        public IEnumerable<IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            return account.ID == CreatorID ? new[] { Constants.Owner } : new IRole[0];
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public bool InheritSupported
        {
            get { return false; }
        }
    }
}
