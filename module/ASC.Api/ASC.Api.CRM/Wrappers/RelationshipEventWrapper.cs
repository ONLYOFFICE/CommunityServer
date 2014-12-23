/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;
using ASC.Api.Documents;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "entity", Namespace = "")]
    public class EntityWrapper
    {
        [DataMember]
        public String EntityType { get; set; }

        [DataMember]
        public int EntityId { get; set; }

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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Content { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public HistoryCategoryBaseWrapper Category { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWrapper Contact { get; set; }

        [DataMember]
        public EntityWrapper Entity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CanEdit { get; set; }

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
                    Files = new[] {FileWrapper.GetSample()},
                    Content = @"Agreed to meet at lunch and discuss the client commercial offer"
                };
        }
    }
}