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
