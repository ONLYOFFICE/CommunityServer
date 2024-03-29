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
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;

namespace ASC.Api.CRM.Wrappers
{
    /// <summary>
    ///  Opportunity
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "opportunity", Namespace = "")]
    public class OpportunityWrapper : ObjectWrapperBase
    {
        public OpportunityWrapper(Deal deal)
            : base(deal.ID)
        {
            CreateBy = EmployeeWraper.Get(deal.CreateBy);
            Created = (ApiDateTime)deal.CreateOn;
            Title = deal.Title;
            Description = deal.Description;
            Responsible = EmployeeWraper.Get(deal.ResponsibleID);
            BidType = deal.BidType;
            BidValue = deal.BidValue;
            PerPeriodValue = deal.PerPeriodValue;
            SuccessProbability = deal.DealMilestoneProbability;
            ActualCloseDate = (ApiDateTime)deal.ActualCloseDate;
            ExpectedCloseDate = (ApiDateTime)deal.ExpectedCloseDate;
            CanEdit = CRMSecurity.CanEdit(deal);
        }

        public OpportunityWrapper(int id)
            : base(id)
        {
        }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        ///<example>2020-12-11T03:36:09.7011881Z</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<ContactBaseWrapper> Members { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWrapper Contact { get; set; }

        ///<example>Hotel catalogue</example>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Title { get; set; }

        ///<example>description</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        ///<example type="int">0</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public BidType BidType { get; set; }

        ///<example type="double">1,1</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal BidValue { get; set; }

        ///<type>ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public CurrencyInfoWrapper BidCurrency { get; set; }

        ///<example type="int">1</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int PerPeriodValue { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.DealMilestoneBaseWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DealMilestoneBaseWrapper Stage { get; set; }

        ///<example type="int">65</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SuccessProbability { get; set; }

        ///<example>2020-12-11T03:36:09.7011881Z</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime ActualCloseDate { get; set; }

        ///<example>2020-12-11T03:36:09.7011881Z</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime ExpectedCloseDate { get; set; }

        ///<example>false</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsPrivate { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<EmployeeWraper> AccessList { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.CustomFieldBaseWrapper, ASC.Api.CRM</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<CustomFieldBaseWrapper> CustomFields { get; set; }

        public static OpportunityWrapper GetSample()
        {
            return new OpportunityWrapper(0)
            {
                CreateBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Responsible = EmployeeWraper.GetSample(),
                Title = "Hotel catalogue",
                Description = "",
                ExpectedCloseDate = ApiDateTime.GetSample(),
                Contact = ContactBaseWrapper.GetSample(),
                IsPrivate = false,
                SuccessProbability = 65,
                BidType = BidType.FixedBid,
                Stage = DealMilestoneBaseWrapper.GetSample()
            };
        }
    }
}