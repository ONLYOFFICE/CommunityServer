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


using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using System;

namespace ASC.Core
{
    [Serializable]
    public class AzRecord
    {
        public Guid SubjectId { get; set; }

        public Guid ActionId { get; set; }

        public string ObjectId { get; set; }

        public AceType Reaction { get; set; }

        public int Tenant { get; set; }

        
        public AzRecord()
        {
        }

        public AzRecord(Guid subjectId, Guid actionId, AceType reaction)
            : this(subjectId, actionId, reaction, default(string))
        {
        }

        public AzRecord(Guid subjectId, Guid actionId, AceType reaction, ISecurityObjectId objectId)
            : this(subjectId, actionId, reaction, AzObjectIdHelper.GetFullObjectId(objectId))
        {
        }


        internal AzRecord(Guid subjectId, Guid actionId, AceType reaction, string objectId)
        {
            SubjectId = subjectId;
            ActionId = actionId;
            Reaction = reaction;
            ObjectId = objectId;
        }


        public override bool Equals(object obj)
        {
            var r = obj as AzRecord;
            return r != null &&
                r.Tenant == Tenant &&
                r.SubjectId == SubjectId &&
                r.ActionId == ActionId &&
                r.ObjectId == ObjectId &&
                r.Reaction == Reaction;
        }

        public override int GetHashCode()
        {
            return Tenant.GetHashCode() ^
                SubjectId.GetHashCode() ^
                ActionId.GetHashCode() ^
                (ObjectId ?? string.Empty).GetHashCode() ^
                Reaction.GetHashCode();
        }
    }
}
