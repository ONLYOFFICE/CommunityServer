/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Api.Employee;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Namespace = "")]
    public class ObjectWrapperBase
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 10)]
        public string Title { get; set; }

        [DataMember(Order = 11)]
        public string Description { get; set; }

        [DataMember(Order = 20)]
        public int Status { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public Guid ResponsibleId { get; set; }
    }
}