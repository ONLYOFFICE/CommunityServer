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
    public class MailboxAutoreplyDao : BaseDao, IMailboxAutoreplyDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxAutoreplyTable>();

        protected string CurrentUserId { get; private set; }

        private const string AUTOREPLY_ALIAS = "r";
        private const string MB_ALIAS = "mb";

        public MailboxAutoreplyDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public MailboxAutoreply GetAutoreply(int mailboxId)
        {
            var query = Query(AUTOREPLY_ALIAS)
                .InnerJoin(MailboxTable.TABLE_NAME.Alias(MB_ALIAS),
                    Exp.EqColumns(MailboxTable.Columns.Id.Prefix(MB_ALIAS),
                        MailboxAutoreplyTable.Columns.MailboxId.Prefix(AUTOREPLY_ALIAS)))
                .Where(MailboxAutoreplyTable.Columns.Tenant.Prefix(AUTOREPLY_ALIAS), Tenant)
                .Where(MailboxAutoreplyTable.Columns.MailboxId.Prefix(AUTOREPLY_ALIAS), mailboxId);

            return Db.ExecuteList(query)
                .ConvertAll(ToAutoreply)
                .DefaultIfEmpty(new MailboxAutoreply
                {
                    MailboxId = mailboxId,
                    Tenant = Tenant,
                    TurnOn = false,
                    OnlyContacts = false,
                    TurnOnToDate = false,
                    FromDate = DateTime.MinValue,
                    ToDate = DateTime.MinValue,
                    Subject = string.Empty,
                    Html = string.Empty
                })
                .Single();
        }

        public List<MailboxAutoreply> GetAutoreplies(List<int> mailboxIds)
        {
            var query = Query(AUTOREPLY_ALIAS)
                .InnerJoin(MailboxTable.TABLE_NAME.Alias(MB_ALIAS),
                    Exp.EqColumns(MailboxTable.Columns.Id.Prefix(MB_ALIAS),
                        MailboxAutoreplyTable.Columns.MailboxId.Prefix(AUTOREPLY_ALIAS)))
                .Where(MailboxAutoreplyTable.Columns.Tenant.Prefix(AUTOREPLY_ALIAS), Tenant)
                .Where(Exp.In(MailboxAutoreplyTable.Columns.MailboxId.Prefix(AUTOREPLY_ALIAS), mailboxIds));

            var list = Db.ExecuteList(query)
                .ConvertAll(ToAutoreply);

            return (from mailboxId in mailboxIds
                let autoreply = list.FirstOrDefault(s => s.MailboxId == mailboxId)
                select autoreply ?? new MailboxAutoreply
                {
                    MailboxId = mailboxId,
                    Tenant = Tenant,
                    TurnOn = false,
                    OnlyContacts = false,
                    TurnOnToDate = false,
                    FromDate = DateTime.MinValue,
                    ToDate = DateTime.MinValue,
                    Subject = string.Empty,
                    Html = string.Empty
                })
                .ToList();
        }

        public int SaveAutoreply(MailboxAutoreply autoreply)
        {
            var query = new SqlInsert(MailboxAutoreplyTable.TABLE_NAME, true)
                .InColumnValue(MailboxAutoreplyTable.Columns.MailboxId, autoreply.MailboxId)
                .InColumnValue(MailboxAutoreplyTable.Columns.Tenant, autoreply.Tenant)
                .InColumnValue(MailboxAutoreplyTable.Columns.TurnOn, autoreply.TurnOn)
                .InColumnValue(MailboxAutoreplyTable.Columns.OnlyContacts, autoreply.OnlyContacts)
                .InColumnValue(MailboxAutoreplyTable.Columns.TurnOnToDate, autoreply.TurnOnToDate)
                .InColumnValue(MailboxAutoreplyTable.Columns.FromDate, autoreply.FromDate)
                .InColumnValue(MailboxAutoreplyTable.Columns.ToDate, autoreply.ToDate)
                .InColumnValue(MailboxAutoreplyTable.Columns.Subject, autoreply.Subject)
                .InColumnValue(MailboxAutoreplyTable.Columns.Html, autoreply.Html);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteAutoreply(int mailboxId)
        {
            var query = new SqlDelete(MailboxAutoreplyTable.TABLE_NAME)
                .Where(MailboxAutoreplyTable.Columns.MailboxId, mailboxId)
                .Where(MailboxAutoreplyTable.Columns.Tenant, Tenant);

            return Db.ExecuteNonQuery(query);
        }

        protected MailboxAutoreply ToAutoreply(object[] r)
        {
            var obj = new MailboxAutoreply
            {
                MailboxId = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                TurnOn = Convert.ToBoolean(r[2]),
                OnlyContacts = Convert.ToBoolean(r[3]),
                TurnOnToDate = Convert.ToBoolean(r[4]),
                FromDate = Convert.ToDateTime(r[5]),
                ToDate = Convert.ToDateTime(r[6]),
                Subject = Convert.ToString(r[7]),
                Html = Convert.ToString(r[8])
            };

            return obj;
        }
    }
}