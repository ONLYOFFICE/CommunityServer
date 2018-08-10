/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


#region Usings

using System;
using System.Runtime.Serialization;
using ASC.Common.Security;

#endregion

namespace ASC.CRM.Core.Entities
{
    [DataContract]
    public class Deal : DomainObject, ISecurityObjectId
    {
        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid? LastModifedBy { get; set; }

        public DateTime? LastModifedOn { get; set; }

        [DataMember(Name = "contact_id")]
        public int ContactID { get; set; }

        [DataMember(Name = "contact")]
        public Contact Contact { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "responsible_id")]
        public Guid ResponsibleID { get; set; }

        [DataMember(Name = "bid_type")]
        public BidType BidType { get; set; }

        [DataMember(Name = "bid_value")]
        public decimal BidValue { get; set; }

        [DataMember(Name = "bid_currency")]
        public string BidCurrency { get; set; }
        
        [DataMember(Name = "per_period_value")]
        public int PerPeriodValue { get; set; }

        [DataMember(Name = "deal_milestone")]
        public int DealMilestoneID { get; set; }

        [DataMember(Name = "deal_milestone_probability")]
        public int DealMilestoneProbability { get; set; }

        public DateTime ActualCloseDate { get; set; }

        [DataMember(Name = "actual_close_date")]
        private String ActualCloseDateStr
        {
            get
            {
                return ActualCloseDate.Date == DateTime.MinValue.Date
                           ? string.Empty : ActualCloseDate.ToString(DateTimeExtension.DateFormatPattern);
            }
            set { ; }
        }

        
        public DateTime ExpectedCloseDate { get; set; }

        [DataMember(Name = "expected_close_date")]
        private String ExpectedCloseDateStr
        {
            get
            {
                return ExpectedCloseDate.Date == DateTime.MinValue.Date
                           ? string.Empty : ExpectedCloseDate.ToString(DateTimeExtension.DateFormatPattern);
            }
            set { ; }
        }
        
        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }
    }
}
