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