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
using System.Runtime.Serialization;
using ASC.Specific;
using ASC.VoipService;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "voipCallHistory", Namespace = "")]
    public class VoipCallHistoryWrapper
    {
        [DataMember(Order = 1)]
        public string ID { get; set; }

        [DataMember(Order = 2)]
        public Guid AnsweredBy { get; set; }

        [DataMember(Order = 3)]
        public int WaitTime { get; set; }

        [DataMember(Order = 4)]
        public int HoldTime { get; set; }

        [DataMember(Order = 5)]
        public int WrapUpTime { get; set; }

        [DataMember(Order = 8)]
        public string RecordUrl { get; set; }

        [DataMember(Order = 9)]
        public int RecordDuration { get; set; }

        [DataMember(Order = 10)]
        public decimal Cost { get; set; }

        public VoipCallHistoryWrapper(VoipCallHistory history)
        {
            ID = history.ID;
            AnsweredBy = history.AnsweredBy;
            WaitTime = history.AnswerDate.Subtract(history.QueueDate).Seconds;
            HoldTime = 0;
            WrapUpTime = history.EndDialDate.Subtract(history.AnswerDate).Seconds;
            RecordUrl = history.RecordUrl;
            RecordDuration = history.RecordDuration;
            Cost = history.Price;
        }
    }
}
