/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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