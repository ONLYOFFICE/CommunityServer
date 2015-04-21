/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<ContactBaseWrapper> Members { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWrapper Contact { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public BidType BidType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal BidValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public CurrencyInfoWrapper BidCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int PerPeriodValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DealMilestoneBaseWrapper Stage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SuccessProbability { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime ActualCloseDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime ExpectedCloseDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsPrivate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<EmployeeWraper> AccessList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

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