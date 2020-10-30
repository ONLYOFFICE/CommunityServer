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
    public class ContactInfo : IEquatable<ContactInfo>
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
        public string User { get; set; }
        public int ContactId { get; set; }
        public string Data { get; set; }
        public int Type { get; set; }
        public bool IsPrimary { get; set; }

        public bool Equals(ContactInfo other)
        {
            if (other == null) return false;
            return string.Equals(User, other.User, StringComparison.InvariantCultureIgnoreCase)
                   && Tenant == other.Tenant
                   && ContactId == other.ContactId
                   && string.Equals(Data, other.Data, StringComparison.InvariantCultureIgnoreCase)
                   && Type == other.Type
                   && IsPrimary == other.IsPrimary;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as ContactInfo);
        }

        public override int GetHashCode()
        {
            return Tenant.GetHashCode() ^ User.GetHashCode() ^ ContactId.GetHashCode() ^ Data.GetHashCode() ^
                   Type.GetHashCode() ^ IsPrimary.GetHashCode();
        }
    }
}
