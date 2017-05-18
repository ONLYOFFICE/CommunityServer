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


using System.Linq;
using System.Runtime.Serialization;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;

namespace ASC.Mail.Aggregator.Filter
{
    [DataContract(Namespace = "")]
    public class MailFilter
    {
        public int Id { get; set; }
        public string FilterName { get; set; }
        public int TennantId { get; set; }
        public string UserName { get; set; }

        [DataMember(Name = "PrimaryFolder")]
        public int PrimaryFolder { get; set; }

        [DataMember(Name = "Unread")]
        public bool? Unread { get; set; }

        [DataMember(Name = "Attachments")]
        public bool? Attachments { get; set; }

        [DataMember(Name = "Period_from")]
        public long? PeriodFrom { get; set; }

        [DataMember(Name = "Period_to")]
        public long? PeriodTo { get; set; }

        [DataMember(Name = "Important")]
        public bool? Important { get; set; }

        [DataMember(Name = "FindAddress")]
        public string FindAddress { get; set; }

        [DataMember(Name = "MailboxId")]
        public int? MailboxId { get; set; }

        [DataMember(Name = "CustomLabels")]
        public ItemList<int> CustomLabels { get; set; }

        [DataMember(Name = "Sort")]
        public string Sort { get; set; }

        [DataMember(Name = "SortOrder")]
        public string SortOrder { get; set; }

        [DataMember(Name = "SearchFilter")]
        public string SearchText { get; set; }

        [DataMember(Name = "Page")]
        public int? Page { get; set; }

        [DataMember(Name = "PageSize")]
        public int PageSize { get; set; }

        public int? SetLabel { get; set; }

        [DataMember(Name = "WithCalendar")]
        public bool? WithCalendar { get; set; }

        public MailFilter()
        {
            CustomLabels = new ItemList<int>();
        }

        public bool IsDefault()
        {
            return !Unread.HasValue && 
                   !Attachments.HasValue &&
                   !PeriodFrom.HasValue &&
                   !PeriodTo.HasValue &&
                   !Important.HasValue &&
                   string.IsNullOrEmpty(FindAddress) &&
                   !MailboxId.HasValue && 
                   (CustomLabels == null || !CustomLabels.Any()) && 
                   string.IsNullOrEmpty(SearchText) &&
                   !WithCalendar.HasValue;
        }
    }
}