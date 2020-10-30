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


// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using ASC.Mail.Core.DbSchema.Interfaces;

namespace ASC.Mail.Core.DbSchema.Tables
{
    public class MailTable : ITable
    {
        public const string TABLE_NAME = "mail_mail";

        public static class Columns
        {
            public const string Id = "id";
            public const string MailboxId = "id_mailbox";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Address = "address";
            public const string Uidl = "uidl";
            public const string Md5 = "md5";
            public const string From = "from_text";
            public const string To = "to_text";
            public const string Reply = "reply_to";
            public const string Cc = "cc";
            public const string Bcc = "bcc";
            public const string Subject = "subject";
            public const string Introduction = "introduction";
            public const string Importance = "importance";
            public const string DateReceived = "date_received";
            public const string DateSent = "date_sent";
            public const string Size = "size";
            public const string AttachCount = "attachments_count";
            public const string Unread = "unread";
            public const string IsAnswered = "is_answered";
            public const string IsForwarded = "is_forwarded";
            public const string IsTextBodyOnly = "is_text_body_only";
            public const string HasParseError = "has_parse_error";
            public const string CalendarUid = "calendar_uid";
            public const string Stream = "stream";
            public const string Folder = "folder";
            public const string FolderRestore = "folder_restore";
            public const string Spam = "spam"; //TODO: Need remove?
            public const string TimeModified = "time_modified";
            public const string IsRemoved = "is_removed";
            public const string MimeMessageId = "mime_message_id";
            public const string MimeInReplyTo = "mime_in_reply_to";
            public const string ChainId = "chain_id";
            public const string ChainDate = "chain_date";
            public const string LastModified = "time_modified";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public MailTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Id,
                Columns.MailboxId,
                Columns.User,
                Columns.Tenant,
                Columns.Address,
                Columns.Uidl,
                Columns.Md5,
                Columns.From,
                Columns.To,
                Columns.Reply,
                Columns.Cc,
                Columns.Bcc,
                Columns.Subject,
                Columns.Introduction,
                Columns.Importance,
                Columns.DateReceived,
                Columns.DateSent,
                Columns.Size,
                Columns.AttachCount,
                Columns.Unread,
                Columns.IsAnswered,
                Columns.IsForwarded,
                Columns.Stream,
                Columns.Folder,
                Columns.FolderRestore, 
                Columns.Spam,
                Columns.IsRemoved,
                Columns.TimeModified,
                Columns.MimeMessageId, 
                Columns.MimeInReplyTo, 
                Columns.ChainId,
                Columns.ChainDate,
                Columns.IsTextBodyOnly,
                Columns.HasParseError, 
                Columns.CalendarUid
            };
        }
    }
}
