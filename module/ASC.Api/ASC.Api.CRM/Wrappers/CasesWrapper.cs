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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "case", Namespace = "")]
    public class CasesWrapper : ObjectWrapperBase
    {
        public CasesWrapper()
            : base(0)
        {
        }

        public CasesWrapper(Cases cases)
            : base(cases.ID)
        {
            CreateBy = EmployeeWraper.Get(cases.CreateBy);
            Created = (ApiDateTime)cases.CreateOn;
            Title = cases.Title;
            IsClosed = cases.IsClosed;

            IsPrivate = CRMSecurity.IsPrivate(cases);

            if (IsPrivate)
            {
                AccessList = CRMSecurity.GetAccessSubjectTo(cases)
                                        .SkipWhile(item => item.Key == Core.Users.Constants.GroupEveryone.ID)
                                        .Select(item => EmployeeWraper.Get(item.Key));
            }
            CanEdit = CRMSecurity.CanEdit(cases);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<ContactBaseWrapper> Members { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsClosed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsPrivate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<EmployeeWraper> AccessList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<CustomFieldBaseWrapper> CustomFields { get; set; }

        public static CasesWrapper GetSample()
        {
            return new CasesWrapper
                {
                    IsClosed = false,
                    Title = "Exhibition organization",
                    Created = ApiDateTime.GetSample(),
                    CreateBy = EmployeeWraper.GetSample(),
                    IsPrivate = false,
                    CustomFields = new[] {CustomFieldBaseWrapper.GetSample()},
                    CanEdit = true
                };
        }
    }
}