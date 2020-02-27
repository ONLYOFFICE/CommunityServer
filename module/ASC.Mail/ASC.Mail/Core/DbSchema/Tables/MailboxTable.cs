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
