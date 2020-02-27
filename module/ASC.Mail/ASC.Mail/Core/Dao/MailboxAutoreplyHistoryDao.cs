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