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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class MailboxAutoreplyHistoryDao : BaseDao, IMailboxAutoreplyHistoryDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxAutoreplyHistoryTable>();

        protected string CurrentUserId { get; private set; }

        public MailboxAutoreplyHistoryDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        private const string WHERE_SENDING_DATE_LESS =
            "TO_DAYS(UTC_TIMESTAMP) - TO_DAYS(" + MailboxAutoreplyHistoryTable.Columns.SendingDate + ")";

        public List<string> GetAutoreplyHistorySentEmails(int mailboxId, string email, int autoreplyDaysInterval)
        {
            var query = new SqlQuery(MailboxAutoreplyHistoryTable.TABLE_NAME)
                .Select(MailboxAutoreplyHistoryTable.Columns.SendingEmail)
                .Where(MailboxAutoreplyHistoryTable.Columns.MailboxId, mailboxId)
                .Where(MailboxAutoreplyHistoryTable.Columns.SendingEmail, email)
                .Where(Exp.Lt(WHERE_SENDING_DATE_LESS, autoreplyDaysInterval));

            return Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToString(r[0]));
        }

        public int SaveAutoreplyHistory(MailboxAutoreplyHistory autoreplyHistory)
        {
            var query = new SqlInsert(MailboxAutoreplyHistoryTable.TABLE_NAME, true)
                .InColumnValue(MailboxAutoreplyHistoryTable.Columns.MailboxId, autoreplyHistory.MailboxId)
                .InColumnValue(MailboxAutoreplyHistoryTable.Columns.Tenant, autoreplyHistory.Tenant)
                .InColumnValue(MailboxAutoreplyHistoryTable.Columns.SendingEmail, autoreplyHistory.SendingEmail)
                .InColumnValue(MailboxAutoreplyHistoryTable.Columns.SendingDate, autoreplyHistory.SendingDate);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteAutoreplyHistory(int mailboxId)
        {
            var query = new SqlDelete(MailboxAutoreplyHistoryTable.TABLE_NAME)
                .Where(MailboxAutoreplyHistoryTable.Columns.MailboxId, mailboxId)
                .Where(MailboxAutoreplyHistoryTable.Columns.Tenant, Tenant);

            return Db.ExecuteNonQuery(query);
        }

        protected MailboxAutoreplyHistory ToAutoreplyHistory(object[] r)
        {
            var obj = new MailboxAutoreplyHistory
            {
                MailboxId = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                SendingDate = Convert.ToDateTime(r[2]),
                SendingEmail = Convert.ToString(r[3])
            };

            return obj;
        }
    }
}