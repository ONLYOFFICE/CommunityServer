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
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    ///<inherited>ASC.Api.Projects.Wrappers.ObjectWrapperBase, ASC.Api.Projects</inherited>
    [DataContract(Namespace = "")]
    public class ObjectWrapperFullBase : ObjectWrapperBase, IApiSortableDate
    {
        ///<example>2020-12-22T04:11:56.5658524Z</example>
        ///<order>50</order>
        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>51</order>
        [DataMember(Order = 51, EmitDefaultValue = false)]
        public EmployeeWraper CreatedBy { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>51</order>
        [DataMember(Order = 51, EmitDefaultValue = false)]
        public Guid CreatedById { get; set; }

        private ApiDateTime updated;

        ///<example>2020-12-22T04:11:56.5658524Z</example>
        ///<order>52</order>
        [DataMember(Order = 52, EmitDefaultValue = false)]
        public ApiDateTime Updated
        {
            get { return updated < Created ? Created : updated; }
            set { updated = value; }
        }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>41</order>
        [DataMember(Order = 41, EmitDefaultValue = false)]
        public EmployeeWraper UpdatedBy { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>41</order>
        [DataMember(Order = 41, EmitDefaultValue = false)]
        public Guid UpdatedById { get; set; }
    }
}