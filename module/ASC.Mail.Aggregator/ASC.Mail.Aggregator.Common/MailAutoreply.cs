/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailAutoreply
    {
        public MailAutoreply(int mailboxId, int tenant, bool turnOn,
            bool onlyContacts, bool turnOnToDate, DateTime from, DateTime to, string subject, string html)
        {
            Tenant = tenant;
            MailboxId = mailboxId;
            TurnOn = turnOn;
            TurnOnToDate = turnOnToDate;
            OnlyContacts = onlyContacts;
            FromDate = from;
            ToDate = to;
            Subject = subject;
            Html = html;
        }

        public int Tenant { get; private set; }

        [DataMember(Name = "mailboxId")]
        public int MailboxId { get; private set; }

        [DataMember(Name = "turnOn")]
        public bool TurnOn { get; set; }

        [DataMember(Name = "onlyContacts")]
        public bool OnlyContacts { get; private set; }

        [DataMember(Name = "turnOnToDate")]
        public bool TurnOnToDate { get; private set; }

        [DataMember(Name = "fromDate")]
        public DateTime FromDate { get; private set; }

        [DataMember(Name = "toDate")]
        public DateTime ToDate { get; private set; }

        [DataMember(Name = "subject")]
        public string Subject { get; private set; }

        [DataMember(Name = "html")]
        public string Html { get; private set; }
    }
}
