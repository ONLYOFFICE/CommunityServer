/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Runtime.Serialization;

using ASC.Api.Documents;
using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "entity", Namespace = "")]
    public class EntityWrapper
    {
        ///<example>opportunity</example>
        [DataMember]
        public String EntityType { get; set; }

        ///<example type="int">123445</example>
        [DataMember]
        public int EntityId { get; set; }

        ///<example>Household appliances internet shop</example>
        [DataMember]
        public String EntityTitle { get; set; }

        public static EntityWrapper GetSample()
        {
            return new EntityWrapper
            {
                EntityId = 123445,
                EntityType = "opportunity",
                EntityTitle = "Household appliances internet shop"
            };
        }
    }

    ///<inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "historyEvent", Namespace = "")]
    public class RelationshipEventWrapper :
        ObjectWrapperBase
    {
        public RelationshipEventWrapper() :
            base(0)
        {
        }

        public RelationshipEventWrapper(RelationshipEvent relationshipEvent)
            : base(relationshipEvent.ID)
        {
            CreateBy = EmployeeWraper.Get(relationshipEvent.CreateBy);
            Created = (ApiDateTime)relationshipEvent.CreateOn;
            Content = relationshipEvent.Content;
            Files = new List<FileWrapper>();
            CanEdit = CRMSecurity.CanEdit(relationshipEvent);
        }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        ///<example>2020-12-13T17:13:31.5902727Z</example>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        ///<example>Agreed to meet at lunch and discuss the client commercial offer</example>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Content { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.HistoryCategoryBaseWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public HistoryCategoryBaseWrapper Category { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWrapper Contact { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.EntityWrapper, ASC.Api.CRM</type>
        [DataMember]
        public EntityWrapper Entity { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CanEdit { get; set; }

        ///<type>ASC.Api.Documents.FileWrapper, ASC.Api.Documents</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<FileWrapper> Files { get; set; }

        public static RelationshipEventWrapper GetSample()
        {
            return new RelationshipEventWrapper
            {
                CanEdit = true,
                Category = HistoryCategoryBaseWrapper.GetSample(),
                Entity = EntityWrapper.GetSample(),
                Contact = ContactBaseWrapper.GetSample(),
                Created = ApiDateTime.GetSample(),
                CreateBy = EmployeeWraper.GetSample(),
                Files = new[] { FileWrapper.GetSample() },
                Content = @"Agreed to meet at lunch and discuss the client commercial offer"
            };
        }
    }
}