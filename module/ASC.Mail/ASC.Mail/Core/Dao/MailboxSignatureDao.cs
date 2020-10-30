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