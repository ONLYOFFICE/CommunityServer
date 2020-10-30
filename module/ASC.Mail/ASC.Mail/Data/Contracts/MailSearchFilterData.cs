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
using ASC.Mail.Enums;

namespace ASC.Mail.Data.Contracts
{
    [DataContract(Namespace = "")]
    public class MailSearchFilterData : ICloneable
    {
        [DataMember(Name = "PrimaryFolder")]
        public FolderType PrimaryFolder { get; set; }

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

        [DataMember(Name = "FromAddress")]
        public string FromAddress { get; set; }

        [DataMember(Name = "ToAddress")]
        public string ToAddress { get; set; }

        [DataMember(Name = "MailboxId")]
        public int? MailboxId { get; set; }

        [DataMember(Name = "CustomLabels")]
        public List<int> CustomLabels { get; set; }

        [DataMember(Name = "Sort")]
        public string Sort { get; set; }

        [DataMember(Name = "SortOrder")]
        public string SortOrder { get; set; }

        [DataMember(Name = "SearchFilter")]
        public string SearchText { get; set; }

        [DataMember(Name = "Page")]
        public int? Page { get; set; }

        [DataMember(Name = "PageSize")]
        public int? PageSize { get; set; }

        public int? SetLabel { get; set; }

        [DataMember(Name = "WithCalendar")]
        public bool? WithCalendar { get; set; }

        [DataMember(Name = "UserFolderId")]
        public int? UserFolderId { get; set; }

        [DataMember(Name = "FromDate")]
        public DateTime? FromDate { get; set; }

        [DataMember(Name = "FromMessage")]
        public int? FromMessage { get; set; }

        [DataMember(Name = "PrevFlag")]
        public bool? PrevFlag { get; set; }

        public MailSearchFilterData()
        {
            CustomLabels = new List<int>();
        }

        public bool IsDefault()
        {
            return !Attachments.HasValue &&
                   !PeriodFrom.HasValue &&
                   !PeriodTo.HasValue &&
                   !Important.HasValue &&
                   string.IsNullOrEmpty(FromAddress) &&
                   string.IsNullOrEmpty(ToAddress) &&
                   !MailboxId.HasValue &&
                   (CustomLabels == null || !CustomLabels.Any()) &&
                   string.IsNullOrEmpty(SearchText) &&
                   !WithCalendar.HasValue &&
                   !Unread.HasValue;
        }

        public override int GetHashCode()
        {
            return PrimaryFolder.GetHashCode() ^ Unread.GetHashCode() ^ Attachments.GetHashCode() ^
                   PeriodFrom.GetHashCode() ^ PeriodTo.GetHashCode() ^ Important.GetHashCode() ^
                   FromAddress.GetHashCode() ^ MailboxId.GetHashCode() ^ CustomLabels.GetHashCode() ^
                   Sort.GetHashCode() ^ SortOrder.GetHashCode() ^ SearchText.GetHashCode() ^
                   Page.GetValueOrDefault() ^ PageSize.GetHashCode() ^ SetLabel.GetHashCode() ^
                   WithCalendar.GetHashCode() ^ UserFolderId.GetHashCode() ^ FromDate.GetHashCode() ^
                   FromMessage.GetHashCode() ^ PrevFlag.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var searchFilterData = obj as MailSearchFilterData;
            return searchFilterData != null && searchFilterData.PrimaryFolder == PrimaryFolder &&
                   searchFilterData.Unread == Unread && searchFilterData.Attachments == Attachments &&
                   searchFilterData.PeriodFrom == PeriodFrom && searchFilterData.PeriodTo == PeriodTo &&
                   searchFilterData.Important == Important && searchFilterData.Sort == Sort &&
                   searchFilterData.SortOrder == SortOrder && searchFilterData.SearchText == SearchText &&
                   searchFilterData.Page == Page && searchFilterData.PageSize == PageSize &&
                   searchFilterData.SetLabel == SetLabel && searchFilterData.WithCalendar == WithCalendar &&
                   searchFilterData.WithCalendar == WithCalendar && searchFilterData.UserFolderId == UserFolderId &&
                   searchFilterData.FromDate == FromDate && searchFilterData.FromMessage == FromMessage &&
                   searchFilterData.PrevFlag == PrevFlag;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}