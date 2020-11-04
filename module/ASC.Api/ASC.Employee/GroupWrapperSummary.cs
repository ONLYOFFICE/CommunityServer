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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Api.Employee
{
    [DataContract(Name = "group", Namespace = "")]
    public class GroupWrapperSummary
    {
        public GroupWrapperSummary(GroupInfo group)
        {
            Id = group.ID;
            Name = group.Name;
            Manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(group.ID)).UserName;
        }

        protected GroupWrapperSummary()
        {
        }


        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 9, EmitDefaultValue = true)]
        public string Manager { get; set; }

        public static GroupWrapperSummary GetSample()
        {
            return new GroupWrapperSummary { Id = Guid.Empty, Manager = "Jake.Zazhitski", Name = "Group Name" };
        }
    }
}