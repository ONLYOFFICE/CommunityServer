/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class MailboxSignatureDao : BaseDao, IMailboxSignatureDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxSignatureTable>();

        protected string CurrentUserId { get; private set; }

        private const string SIG_ALIAS = "s";
        private const string MB_ALIAS = "mb";

        public MailboxSignatureDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public MailboxSignature GetSignature(int mailboxId)
        {
            var query = Query(SIG_ALIAS)
                .InnerJoin(MailboxTable.TABLE_NAME.Alias(MB_ALIAS),
                    Exp.EqColumns(MailboxTable.Columns.Id.Prefix(MB_ALIAS),
                        MailboxSignatureTable.Columns.MailboxId.Prefix(SIG_ALIAS)))
                .Where(MailboxSignatureTable.Columns.Tenant.Prefix(SIG_ALIAS), Tenant)
                .Where(MailboxSignatureTable.Columns.MailboxId.Prefix(SIG_ALIAS), mailboxId);

            return Db.ExecuteList(query)
                .ConvertAll(ToSignature)
                .DefaultIfEmpty(new MailboxSignature
                {
                    MailboxId = mailboxId,
                    Tenant = Tenant,
                    Html = "",
                    IsActive = false
                })
                .Single();
        }

        public List<MailboxSignature> GetSignatures(List<int> mailboxIds)
        {
            var query = Query(SIG_ALIAS)
                .InnerJoin(MailboxTable.TABLE_NAME.Alias(MB_ALIAS),
                    Exp.EqColumns(MailboxTable.Columns.Id.Prefix(MB_ALIAS),
                        MailboxSignatureTable.Columns.MailboxId.Prefix(SIG_ALIAS)))
                .Where(MailboxSignatureTable.Columns.Tenant.Prefix(SIG_ALIAS), Tenant)
                .Where(Exp.In(MailboxSignatureTable.Columns.MailboxId.Prefix(SIG_ALIAS), mailboxIds));

            var list = Db.ExecuteList(query)
                .ConvertAll(ToSignature);

            return (from mailboxId in mailboxIds
                let sig = list.FirstOrDefault(s => s.MailboxId == mailboxId)
                select sig ?? new MailboxSignature
                {
                    MailboxId = mailboxId,
                    Tenant = Tenant,
                    Html = "",
                    IsActive = false
                })
                .ToList();
        }

        public int SaveSignature(MailboxSignature signature)
        {
            var query = new SqlInsert(MailboxSignatureTable.TABLE_NAME, true)
                .InColumnValue(MailboxSignatureTable.Columns.Html, signature.Html)
                .InColumnValue(MailboxSignatureTable.Columns.IsActive, signature.IsActive)
                .InColumnValue(MailboxSignatureTable.Columns.Tenant, signature.Tenant)
                .InColumnValue(MailboxSignatureTable.Columns.MailboxId, signature.MailboxId);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteSignature(int mailboxId)
        {
            var query = new SqlDelete(MailboxSignatureTable.TABLE_NAME)
                .Where(MailboxSignatureTable.Columns.MailboxId, mailboxId)
                .Where(MailboxSignatureTable.Columns.Tenant, Tenant);
            return Db.ExecuteNonQuery(query);
        }

        protected MailboxSignature ToSignature(object[] r)
        {
            var obj = new MailboxSignature
            {
                MailboxId = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                Html = Convert.ToString(r[2]),
                IsActive = Convert.ToBoolean(r[3])
            };

            return obj;
        }
    }
}