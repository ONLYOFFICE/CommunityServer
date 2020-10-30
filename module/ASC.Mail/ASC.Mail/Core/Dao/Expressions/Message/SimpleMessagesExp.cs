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
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Message
{
    public class SimpleMessagesExp : IMessagesExp
    {
        public int Tenant { get; private set; }

        public string User { get; set; }
        public bool? IsRemoved { get; set; }
        public int? Folder { get; set; }
        public int? MailboxId { get; set; }
        public string ChainId { get; set; }
        public string Md5 { get; set; }
        public string MimeMessageId { get; set; }
        public int? MessageId { get; set; }
        public bool? Unread { get; set; }

        public List<int> MessageIds { get; set; }
        public List<int> FoldersIds { get; set; }
        public List<string> ChainIds { get; set; }
        public List<int> TagIds { get; set; }
        public int? UserFolderId { get; set; }

        public string OrderBy { get; set; }
        public bool? OrderAsc { get; set; }
        public int? StartIndex { get; set; }
        public int? Limit { get; set; }

        public string Subject { get; set; }

        public DateTime? DateSent { get; set; }

        public Exp Exp { get; set; }

        public SimpleMessagesExp(int tenant)
        {
            Tenant = tenant;
        }

        public SimpleMessagesExp(int tenant, string user, bool? isRemoved = false)
        {
            Tenant = tenant;
            User = user;
            IsRemoved = isRemoved;
        }

        public static MessagesExpBuilder CreateBuilder(int tenant, string user, bool? isRemoved = false)
        {
            return new MessagesExpBuilder(tenant, user, isRemoved);
        }

        public static MessagesExpBuilder CreateBuilder(int tenant)
        {
            return new MessagesExpBuilder(tenant);
        }

        private const string MM_ALIAS = "mm";

        public Exp GetExpression()
        {
            var exp = Exp.Eq(MailTable.Columns.Tenant.Prefix(MM_ALIAS), Tenant);

            if (!string.IsNullOrEmpty(User))
            {
                exp &= Exp.Eq(MailTable.Columns.User.Prefix(MM_ALIAS), User);
            }

            if (MessageId.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.Id.Prefix(MM_ALIAS), MessageId.Value);
            }

            if (MessageIds != null)
            {
                exp &= Exp.In(MailTable.Columns.Id.Prefix(MM_ALIAS), MessageIds);
            }

            if (ChainIds != null)
            {
                exp &= Exp.In(MailTable.Columns.ChainId.Prefix(MM_ALIAS), ChainIds);
            }

            if (Folder.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.Folder.Prefix(MM_ALIAS), Folder.Value);
            }

            if (IsRemoved.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.IsRemoved.Prefix(MM_ALIAS), IsRemoved.Value);
            }

            if (MailboxId.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.MailboxId.Prefix(MM_ALIAS), MailboxId.Value);
            }

            if (!string.IsNullOrEmpty(Md5))
            {
                exp &= Exp.Eq(MailTable.Columns.Md5.Prefix(MM_ALIAS), Md5);
            }

            if (!string.IsNullOrEmpty(MimeMessageId))
            {
                exp &= Exp.Eq(MailTable.Columns.MimeMessageId.Prefix(MM_ALIAS), MimeMessageId);
            }

            if (!string.IsNullOrEmpty(ChainId))
            {
                exp &= Exp.Eq(MailTable.Columns.ChainId.Prefix(MM_ALIAS), ChainId);
            }

            if (FoldersIds != null && FoldersIds.Any())
            {
                exp &= Exp.In(MailTable.Columns.Folder.Prefix(MM_ALIAS), FoldersIds.ToArray());
            }

            if (Unread.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.Unread.Prefix(MM_ALIAS), Unread.Value);
            }

            if (!string.IsNullOrEmpty(Subject)) {
                exp &= Exp.Eq(MailTable.Columns.Subject.Prefix(MM_ALIAS), Subject);
            }

            if (DateSent.HasValue)
            {
                exp &= Exp.Eq(MailTable.Columns.DateSent.Prefix(MM_ALIAS), DateSent.Value);
            }

            if (Exp != null)
            {
                exp &= Exp;
            }

            return exp;
        }
    }
}