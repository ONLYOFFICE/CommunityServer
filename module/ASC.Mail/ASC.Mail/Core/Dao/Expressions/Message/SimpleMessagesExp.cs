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

            if (Exp != null)
            {
                exp &= Exp;
            }

            return exp;
        }
    }
}