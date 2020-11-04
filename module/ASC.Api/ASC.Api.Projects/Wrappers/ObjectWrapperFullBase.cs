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
using ASC.Api.Employee;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Namespace = "")]
    public class ObjectWrapperFullBase : ObjectWrapperBase, IApiSortableDate
    {
        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 51, EmitDefaultValue = false)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 51, EmitDefaultValue = false)]
        public Guid CreatedById { get; set; }

        private ApiDateTime updated;

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public ApiDateTime Updated
        {
            get { return updated < Created ? Created : updated; }
            set { updated = value; }
        }

        [DataMember(Order = 41, EmitDefaultValue = false)]
        public EmployeeWraper UpdatedBy { get; set; }

        [DataMember(Order = 41, EmitDefaultValue = false)]
        public Guid UpdatedById { get; set; }
    }
}