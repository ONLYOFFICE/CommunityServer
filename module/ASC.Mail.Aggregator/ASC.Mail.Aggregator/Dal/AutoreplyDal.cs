/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.DbSchema;

namespace ASC.Mail.Aggregator.Dal
{
    public class AutoreplyDal
    {
        private readonly DbManager manager;

        public AutoreplyDal(DbManager manager)
        {
            this.manager = manager;
        }

        public MailAutoreply GetAutoreply(int mailboxId, int tenant)
        {
            var autoreply = GetAutoreplies(new List<int> { mailboxId }, tenant).FirstOrDefault();

            return autoreply ?? new MailAutoreply(mailboxId, tenant, false, false,
                false, DateTime.MinValue, DateTime.MinValue, String.Empty, String.Empty);
        }

        public List<MailAutoreply> GetAutoreplies(List<int> mailboxesIds, int tenant)
        {
            if (!mailboxesIds.Any())
                return new List<MailAutoreply>();

            var selectQuery = GetSelectAutorepliesQuery(mailboxesIds, tenant);
            var resultList = manager.ExecuteList(selectQuery)
                                      .ConvertAll(result =>
                                                  new MailAutoreply(Convert.ToInt32(result[0]), tenant,
                                                                    Convert.ToBoolean(result[1]),
                                                                    Convert.ToBoolean(result[2]),
                                                                    Convert.ToBoolean(result[3]),
                                                                    Convert.ToDateTime(result[4]),
                                                                    Convert.ToDateTime(result[5]),
                                                                    Convert.ToString(result[6]),
                                                                    Convert.ToString(result[7])));

            var autoreplies = new List<MailAutoreply>();

            mailboxesIds.ForEach(idMailbox =>
            {
                var autoreply = resultList.FirstOrDefault(s => s.MailboxId == idMailbox);
                autoreplies.Add(autoreply ?? new MailAutoreply(idMailbox, tenant, false,
                    false, false, DateTime.MinValue, DateTime.MinValue, String.Empty, String.Empty));
            });

            return autoreplies;
        }

        public void UpdateOrCreateAutoreply(int mailboxId, int tenant, MailAutoreply autoreply)
        {
            ISqlInstruction queryForExecution = new SqlInsert(AutoreplyTable.Name, true)
                .InColumnValue(AutoreplyTable.Columns.MailboxId, autoreply.MailboxId)
                .InColumnValue(AutoreplyTable.Columns.Tenant, autoreply.Tenant)
                .InColumnValue(AutoreplyTable.Columns.TurnOn, autoreply.TurnOn)
                .InColumnValue(AutoreplyTable.Columns.OnlyContacts, autoreply.OnlyContacts)
                .InColumnValue(AutoreplyTable.Columns.TurnOnToDate, autoreply.TurnOnToDate)
                .InColumnValue(AutoreplyTable.Columns.FromDate, autoreply.FromDate)
                .InColumnValue(AutoreplyTable.Columns.ToDate, autoreply.ToDate)
                .InColumnValue(AutoreplyTable.Columns.Subject, autoreply.Subject)
                .InColumnValue(AutoreplyTable.Columns.Html, autoreply.Html);

            var deleteAutoreplyHistoryQuery = new SqlDelete(AutoreplyHistoryTable.Name)
                .Where(AutoreplyHistoryTable.Columns.MailboxId, mailboxId)
                .Where(AutoreplyHistoryTable.Columns.Tenant, tenant);

            manager.ExecuteNonQuery(queryForExecution);
            manager.ExecuteNonQuery(deleteAutoreplyHistoryQuery);
        }

        public void DeleteAutoreply(int mailboxId, int tenant)
        {
            var deleteAutoreplyQuery = new SqlDelete(AutoreplyTable.Name)
                .Where(AutoreplyTable.Columns.MailboxId, mailboxId)
                .Where(AutoreplyTable.Columns.Tenant, tenant);

            var deleteAutoreplyHistoryQuery = new SqlDelete(AutoreplyHistoryTable.Name)
                .Where(AutoreplyHistoryTable.Columns.MailboxId, mailboxId)
                 .Where(AutoreplyHistoryTable.Columns.Tenant, tenant);

            manager.ExecuteNonQuery(deleteAutoreplyQuery);
            manager.ExecuteNonQuery(deleteAutoreplyHistoryQuery);
        }

        public void UpdateOrCreateAutoreplyHistory(int mailboxId, int tenant, string email)
        {
            ISqlInstruction queryForExecution = new SqlInsert(AutoreplyHistoryTable.Name, true)
                .InColumnValue(AutoreplyHistoryTable.Columns.MailboxId, mailboxId)
                .InColumnValue(AutoreplyHistoryTable.Columns.Tenant, tenant)
                .InColumnValue(AutoreplyHistoryTable.Columns.SendingEmail, email)
                .InColumnValue(AutoreplyHistoryTable.Columns.SendingDate, DateTime.UtcNow);
            manager.ExecuteNonQuery(queryForExecution);
        }

        public List<string> GetAutoreplyHistory(int mailBoxId, string email, int autoreplyDaysInterval)
        {
            ISqlInstruction queryForExecution = new SqlQuery(AutoreplyHistoryTable.Name)
                .Select(AutoreplyHistoryTable.Columns.SendingEmail)
                .Where(AutoreplyHistoryTable.Columns.MailboxId, mailBoxId)
                .Where(AutoreplyHistoryTable.Columns.SendingEmail, email)
                .Where(string.Format("TO_DAYS(UTC_TIMESTAMP) - TO_DAYS({0}) < {1}",
                    AutoreplyHistoryTable.Columns.SendingDate, autoreplyDaysInterval));
            var list = manager.ExecuteList(queryForExecution);
            List<string> emailList = new List<string>();
            list.ForEach(elem => emailList.Add((string)elem[0]));
            return emailList;
        }

        private SqlQuery GetSelectAutorepliesQuery(ICollection mailboxesIds, int tenant)
        {
            return new SqlQuery(AutoreplyTable.Name)
                .Select(AutoreplyTable.Columns.MailboxId)
                .Select(AutoreplyTable.Columns.TurnOn)
                .Select(AutoreplyTable.Columns.OnlyContacts)
                .Select(AutoreplyTable.Columns.TurnOnToDate)
                .Select(AutoreplyTable.Columns.FromDate)
                .Select(AutoreplyTable.Columns.ToDate)
                .Select(AutoreplyTable.Columns.Subject)
                .Select(AutoreplyTable.Columns.Html)
                .Where(AutoreplyTable.Columns.Tenant, tenant)
                .Where(Exp.In(AutoreplyTable.Columns.MailboxId, mailboxesIds));
        }
    }
}
