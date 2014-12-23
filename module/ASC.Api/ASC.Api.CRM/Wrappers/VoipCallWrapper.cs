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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Specific;
using ASC.VoipService;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "voipCall", Namespace = "")]
    public class VoipCallWrapper
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public string From { get; set; }

        [DataMember(Order = 3)]
        public string To { get; set; }

        [DataMember(Order = 4)]
        public VoipCallStatus Status { get; set; }

        [DataMember(Order = 5)]
        public EmployeeWraper AnsweredBy { get; set; }

        [DataMember(Order = 6)]
        public ApiDateTime DialDate { get; set; }

        [DataMember(Order = 7)]
        public int DialDuration { get; set; }

        [DataMember(Order = 10)]
        public decimal Cost { get; set; }

        [DataMember(Order = 11)]
        public ContactWrapper Contact { get; set; }

        [DataMember(Order = 12)]
        public IEnumerable<VoipCallHistoryWrapper> History { get; set; }

        public VoipCallWrapper(VoipCall call, ContactWrapper contact = null)
        {
            Id = call.Id;
            From = call.From;
            To = call.To;
            Status = call.Status;
            AnsweredBy = EmployeeWraper.Get(call.AnsweredBy);
            DialDate = new ApiDateTime(call.DialDate);
            DialDuration = call.DialDuration;
            Cost = call.TotalPrice;
            Contact = contact;
            History = call.History.Select(h => new VoipCallHistoryWrapper(h));
        }
    }
}