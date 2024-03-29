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

using ASC.Api.Employee;

namespace ASC.Api.Settings
{
    [DataContract(Name = "security", Namespace = "")]
    public class SecurityWrapper
    {
        ///<example>00000000-0000-0000-0000-000000000000</example>
        [DataMember]
        public string WebItemId { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<collection>list</collection>
        [DataMember]
        public IEnumerable<EmployeeWraper> Users { get; set; }

        ///<type>ASC.Api.Employee.GroupWrapperSummary, ASC.Api.Employee</type>
        ///<collection>list</collection>
        [DataMember]
        public IEnumerable<GroupWrapperSummary> Groups { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool Enabled { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsSubItem { get; set; }

        public static SecurityWrapper GetSample()
        {
            return new SecurityWrapper
            {
                WebItemId = Guid.Empty.ToString(),
                Enabled = true,
                IsSubItem = false,
                Groups = new List<GroupWrapperSummary>
                        {
                            GroupWrapperSummary.GetSample()
                        },
                Users = new List<EmployeeWraper>
                        {
                            EmployeeWraper.GetSample()
                        }
            };
        }
    }
}