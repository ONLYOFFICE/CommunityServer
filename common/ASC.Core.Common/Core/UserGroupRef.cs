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
using System.Diagnostics;

namespace ASC.Core
{
    [DebuggerDisplay("{UserId} - {GroupId}")]
    public class UserGroupRef
    {
        public Guid UserId
        {
            get;
            set;
        }

        public Guid GroupId
        {
            get;
            set;
        }

        public bool Removed
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }

        public UserGroupRefType RefType
        {
            get;
            set;
        }

        public int Tenant
        {
            get;
            set;
        }


        public UserGroupRef()
        {
        }

        public UserGroupRef(Guid userId, Guid groupId, UserGroupRefType refType)
        {
            UserId = userId;
            GroupId = groupId;
            RefType = refType;
        }

        public static string CreateKey(int tenant, Guid userId, Guid groupId, UserGroupRefType refType)
        {
            return tenant.ToString() + userId.ToString("N") + groupId.ToString("N") + ((int)refType).ToString();
        }

        public string CreateKey()
        {
            return CreateKey(Tenant, UserId, GroupId, RefType);
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode() ^ GroupId.GetHashCode() ^ Tenant.GetHashCode() ^ RefType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var r = obj as UserGroupRef;
            return r != null && r.Tenant == Tenant && r.UserId == UserId && r.GroupId == GroupId && r.RefType == RefType;
        }
    }
}
