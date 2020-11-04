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
using System.IO;
using ASC.ElasticSearch;
using ASC.ElasticSearch.Core;
using ASC.Mail.Core;
using ASC.Mail.Resources;

namespace ASC.Mail.Data.Search
{
    public sealed class MailWrapper : WrapperWithDoc
    {
        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("time_modified")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("from_text", 1, Analyzer = Analyzer.uax_url_email)]
        public string FromText { get; set; }

        [Column("to_text", 2, Analyzer = Analyzer.uax_url_email)]
        public string ToText { get; set; }

        [Column("cc", 3, Analyzer = Analyzer.uax_url_email)]
        public string Cc { get; set; }

        [Column("bcc", 4, Analyzer = Analyzer.uax_url_email)]
        public string Bcc { get; set; }

        [Column("subject", 5)]
        public string Subject { get; set; }

        [ColumnMeta("id_user", 6)]
        public Guid UserId { get; set; }

        [ColumnMeta("date_sent", 7)]
        public DateTime DateSent { get; set; }

        [ColumnMeta("folder", 8)]
        public byte Folder { get; set; }

        [ColumnMeta("chain_id", 9)]
        public string ChainId { get; set; }

        [ColumnMeta("chain_date", 10)]
        public DateTime ChainDate { get; set; }

        [ColumnMeta("id_mailbox", 11)]
        public int MailboxId { get; set; }

        [ColumnCondition("is_removed", 12, false)]
        public bool IsRemoved { get; set; }

        [ColumnMeta("unread", 13)]
        public bool Unread { get; set; }

        [ColumnMeta("importance", 14)]
        public bool Importance { get; set; }

        [ColumnMeta("attachments_count", 15)]
        public bool HasAttachments { get; set; }

        [Join(JoinTypeEnum.Sub, "id:id_mail")]
        public List<UserFolderWrapper> UserFolders { get; set; }

        [ColumnMeta("calendar_uid", 16)]
        public bool WithCalendar { get; set; }

        [Join(JoinTypeEnum.Sub, "id:id_mail")]
        public List<TagWrapper> Tags { get; set; }

        [Column("introduction", 17)]
        public string Introduction { get; set; }

        protected override string Table
        {
            get { return "mail_mail"; }
        }

        protected override Stream GetDocumentStream()
        {
            var factory = new EngineFactory(TenantId, UserId.ToString());
            var messageEngine = factory.MessageEngine;
            return messageEngine.GetMessageStream(Id);
        }

        public override string SettingsTitle
        {
            get { return MailCoreResource.IndexTitle; }
        }
    }

    public sealed class UserFolderWrapper : Wrapper
    {
        [ColumnId("id_folder")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("time_created")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table
        {
            get { return "mail_user_folder_x_mail"; }
        }
    }

    public sealed class TagWrapper : Wrapper
    {
        [ColumnId("id_tag")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("time_created")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table
        {
            get { return "mail_tag_mail"; }
        }
    }
}