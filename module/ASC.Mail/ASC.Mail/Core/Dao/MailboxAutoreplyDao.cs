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