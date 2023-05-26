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
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core.Users;

namespace ASC.Api.Employee
{
    [DataContract(Name = "group", Namespace = "")]
    public class GroupWrapperFull
    {
        public GroupWrapperFull(GroupInfo group, bool includeMembers)
        {
            Id = group.ID;
            Category = group.CategoryID;
            Parent = group.Parent != null ? group.Parent.ID : Guid.Empty;
            Name = group.Name;
            Manager = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(Core.CoreContext.UserManager.GetDepartmentManager(group.ID)));

            if (includeMembers)
            {
                Members = new List<EmployeeWraper>(Core.CoreContext.UserManager.GetUsersByGroup(group.ID).Select(EmployeeWraper.Get));
            }
        }

        private GroupWrapperFull()
        {
        }
        ///<example>Description</example>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public string Description { get; set; }

        ///<example>Sample group</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string Name { get; set; }

        ///<example>852d4b63-997a-4bae-aa7b-89bb87d85dbf</example>
        ///<order>4</order>
        [DataMember(Order = 4, EmitDefaultValue = true)]
        public Guid? Parent { get; set; }

        ///<example>74f31a85-991b-4e9b-b9e8-ae8e80d468f5</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public Guid Category { get; set; }

        ///<example>fadd9f49-2431-4610-a518-3ca9b3843c88</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9, EmitDefaultValue = true)]
        public EmployeeWraper Manager { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>10</order>
        ///<collection>list</collection>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public List<EmployeeWraper> Members { get; set; }

        public static GroupWrapperFull GetSample()
        {
            return new GroupWrapperFull
            {
                Id = Guid.NewGuid(),
                Manager = EmployeeWraper.GetSample(),
                Category = Guid.NewGuid(),
                Name = "Sample group",
                Parent = Guid.NewGuid(),
                Members = new List<EmployeeWraper> { EmployeeWraper.GetSample() }
            };
        }
    }
}