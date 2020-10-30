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

namespace ASC.Mail.Core.Entities
{
    public class UserFolder : IEquatable<UserFolder>
    {
        public uint Id { get; set; }
        public uint ParentId { get; set; }
        public string User { get; set; }
        public int Tenant { get; set; }
        public string Name { get; set; }
        public int FolderCount { get; set; }
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public int UnreadChainCount { get; set; }
        public int TotalChainCount { get; set; }
        public DateTime TimeModified { get; set; }

        public bool Equals(UserFolder other)
        {
            if (other == null) return false;
            return string.Equals(User, other.User, StringComparison.InvariantCultureIgnoreCase)
                   && Tenant == other.Tenant
                   && Id == other.Id
                   && ParentId == other.ParentId
                   && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
                   && FolderCount == other.FolderCount
                   && UnreadCount == other.UnreadCount
                   && TotalCount == other.TotalCount
                   && UnreadChainCount == other.UnreadChainCount
                   && TotalChainCount == other.TotalChainCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as UserFolder);
        }

        public override int GetHashCode()
        {
            return Tenant.GetHashCode()
                   ^ User.GetHashCode()
                   ^ Id.GetHashCode()
                   ^ ParentId.GetHashCode()
                   ^ Name.GetHashCode()
                   ^ FolderCount.GetHashCode()
                   ^ UnreadCount.GetHashCode()
                   ^ TotalCount.GetHashCode()
                   ^ UnreadChainCount.GetHashCode()
                   ^ TotalChainCount.GetHashCode();
        }
    }
}
