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
    public class MailboxTable : ITable
    {
        public const string TABLE_NAME = "mail_mailbox";

        public static class Columns
        {
            public const string Id = "id";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Address = "address";
            public const string Enabled = "enabled";
            public const string Password = "pop3_password";
            public const string MsgCountLast = "msg_count_last";
            public const string SizeLast = "size_last";
            public const string SmtpPassword = "smtp_password";
            public const string Name = "name";
            public const string LoginDelay = "login_delay";
            public const string IsProcessed = "is_processed";
            public const string IsRemoved = "is_removed";
            public const string IsDefault = "is_default";
            public const string QuotaError = "quota_error";
            public const string Imap = "imap";
            public const string BeginDate = "begin_date";
            public const string OAuthType = "token_type";
            public const string OAuthToken = "token";
            public const string ImapIntervals = "imap_intervals";
            public const string SmtpServerId = "id_smtp_server";
            public const string ServerId = "id_in_server";
            public const string EmailInFolder = "email_in_folder";
            public const string IsServerMailbox = "is_server_mailbox";
            public const string DateCreated = "date_created ";
            public const string DateChecked = "date_checked ";
            public const string DateUserChecked = "date_user_checked ";
            public const string UserOnline = "user_online ";
            public const string DateLoginDelayExpires = "date_login_delay_expires ";
            public const string DateAuthError = "date_auth_error ";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public MailboxTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Id,
                Columns.User,
                Columns.Tenant,
                Columns.Address,
                Columns.Enabled,
                Columns.Password,
                Columns.MsgCountLast,
                Columns.SizeLast,
                Columns.SmtpPassword,
                Columns.Name,
                Columns.LoginDelay,
                Columns.IsProcessed,
                Columns.IsRemoved,
                Columns.IsDefault,
                Columns.QuotaError,
                Columns.Imap,
                Columns.BeginDate,
                Columns.OAuthType,
                Columns.OAuthToken,
                Columns.ImapIntervals,
                Columns.SmtpServerId,
                Columns.ServerId,
                Columns.EmailInFolder,
                Columns.IsServerMailbox,
                Columns.DateCreated,
                Columns.DateChecked,
                Columns.DateUserChecked,
                Columns.UserOnline,
                Columns.DateLoginDelayExpires,
                Columns.DateAuthError
            };
        }
    }
}
