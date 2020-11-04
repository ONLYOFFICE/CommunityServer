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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core.Users;
#endregion

namespace ASC.CRM.Core.Entities
{
    [DataContract]
    public class RelationshipEvent : DomainObject, ISecurityObjectId
    {
        [DataMember(Name = "createBy")]
        public Guid CreateBy { get; set; }

        [DataMember(Name = "createOn")]
        public DateTime CreateOn { get; set; }

        [DataMember(Name = "lastModifedBy")]
        public Guid? LastModifedBy { get; set; }

        [DataMember(Name = "lastModifedOn")]
        public DateTime? LastModifedOn { get; set; }

        [DataMember(Name = "content")]
        public String Content { get; set; }

        [DataMember(Name = "contactID")]
        public int ContactID { get; set; }

        [DataMember(Name = "entityType")]
        public EntityType EntityType { get; set; }

        [DataMember(Name = "entityID")]
        public int EntityID { get; set; }

        [DataMember(Name = "categoryID")]
        public int CategoryID { get; set; }

        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }
    }
}