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
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Enums;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Dao
{
    public class MailDao : BaseDao, IMailDao
    {
        protected static ITable table = new MailTableFactory().Create<MailTable>();

        protected string CurrentUserId { get; private set; }

        public MailDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public int Save(Entities.Mail mail)
        {
            var query = new SqlInsert(MailTable.TABLE_NAME, true)
                .InColumnValue(MailTable.Columns.Id, mail.Id)
                .InColumnValue(MailTable.Columns.MailboxId, mail.MailboxId)
                .InColumnValue(MailTable.Columns.Tenant, mail.Tenant)
                .InColumnValue(MailTable.Columns.User, mail.User)
                .InColumnValue(MailTable.Columns.Address, mail.Address)
                .InColumnValue(MailTable.Columns.Uidl, mail.Uidl)
                .InColumnValue(MailTable.Columns.Md5, mail.Md5)
                .InColumnValue(MailTable.Columns.From, MailUtil.NormalizeStringForMySql(mail.From))
                .InColumnValue(MailTable.Columns.To, MailUtil.NormalizeStringForMySql(mail.To))
                .InColumnValue(MailTable.Columns.Reply, mail.Reply)
                .InColumnValue(MailTable.Columns.Subject, MailUtil.NormalizeStringForMySql(mail.Subject))
                .InColumnValue(MailTable.Columns.Cc, MailUtil.NormalizeStringForMySql(mail.Cc))
                .InColumnValue(MailTable.Columns.Bcc, MailUtil.NormalizeStringForMySql(mail.Bcc))
                .InColumnValue(MailTable.Columns.Importance, mail.Importance)
                .InColumnValue(MailTable.Columns.DateReceived, mail.DateReceived)
                .InColumnValue(MailTable.Columns.DateSent, mail.DateSent)
                .InColumnValue(MailTable.Columns.Size, mail.Size)
                .InColumnValue(MailTable.Columns.AttachCount, mail.AttachCount)
                .InColumnValue(MailTable.Columns.Unread, mail.Unread)
                .InColumnValue(MailTable.Columns.IsAnswered, mail.IsAnswered)
                .InColumnValue(MailTable.Columns.IsForwarded, mail.IsForwarded)
                .InColumnValue(MailTable.Columns.Stream, mail.Stream)
                .InColumnValue(MailTable.Columns.Folder, (int) mail.Folder)
                .InColumnValue(MailTable.Columns.FolderRestore, (int) mail.FolderRestore)
                .InColumnValue(MailTable.Columns.Spam, mail.Spam)
                .InColumnValue(MailTable.Columns.MimeMessageId, mail.MimeMessageId)
                .InColumnValue(MailTable.Columns.MimeInReplyTo, mail.MimeInReplyTo)
                .InColumnValue(MailTable.Columns.ChainId, mail.ChainId)
                .InColumnValue(MailTable.Columns.Introduction, MailUtil.NormalizeStringForMySql(mail.Introduction))
                .InColumnValue(MailTable.Columns.ChainDate, mail.DateSent)
                .InColumnValue(MailTable.Columns.IsTextBodyOnly, mail.IsTextBodyOnly)
                .Identity(0, 0, true);

            if (mail.HasParseError)
                query.InColumnValue(MailTable.Columns.HasParseError, mail.HasParseError);

            if (!string.IsNullOrEmpty(mail.CalendarUid))
                query.InColumnValue(MailTable.Columns.CalendarUid, mail.CalendarUid);

            return Db.ExecuteScalar<int>(query);
        }

        public Entities.Mail GetMail(IMessageExp exp)
        {
            var query = Query()
                .Where(exp.GetExpression());

            return Db.ExecuteList(query)
                .ConvertAll(ToMail)
                .SingleOrDefault();
        }

        public Entities.Mail GetNextMail(IMessageExp exp)
        {
            var query = Query()
                .Where(exp.GetExpression())
                .OrderBy(MailTable.Columns.Id, true)
                .SetMaxResults(1);

            var mail = Db.ExecuteList(query)
                .ConvertAll(ToMail)
                .SingleOrDefault();

            return mail;
        }

        public List<string> GetExistingUidls(int mailboxId, List<string> uidlList)
        {
            var query = new SqlQuery(MailTable.TABLE_NAME)
                .Select(MailTable.Columns.Uidl)
                .Where(MailTable.Columns.MailboxId, mailboxId)
                .Where(Exp.In(MailTable.Columns.Uidl, uidlList));

            var existingUidls = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToString(r[0]))
                .ToList();

            return existingUidls;
        }

        private const string SET_LAST_MODIFIED = MailTable.Columns.LastModified + " = UTC_TIMESTAMP()";

        public int SetMessagesChanged(List<int> ids)
        {
            var update = new SqlUpdate(MailTable.TABLE_NAME)
                .Set(SET_LAST_MODIFIED)
                .Where(Exp.In(MailTable.Columns.Id, ids));

            var result = Db.ExecuteNonQuery(update);

            return result;
        }

        protected Entities.Mail ToMail(object[] r)
        {
            var mail = new Entities.Mail
            {
                Id = Convert.ToInt32(r[0]),
                MailboxId = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                Tenant = Convert.ToInt32(r[3]),
                Address = Convert.ToString(r[4]),
                Uidl = Convert.ToString(r[5]),
                Md5 = Convert.ToString(r[6]),
                From = Convert.ToString(r[7]),
                To = Convert.ToString(r[8]),
                Reply = Convert.ToString(r[9]),
                Cc = Convert.ToString(r[10]),
                Bcc = Convert.ToString(r[11]),
                Subject = Convert.ToString(r[12]),
                Introduction = Convert.ToString(r[13]),
                Importance = Convert.ToBoolean(r[14]),
                DateReceived = Convert.ToDateTime(r[15]),
                DateSent = Convert.ToDateTime(r[16]),
                Size = Convert.ToInt32(r[17]),
                AttachCount = Convert.ToInt32(r[18]),
                Unread = Convert.ToBoolean(r[19]),
                IsAnswered = Convert.ToBoolean(r[20]),
                IsForwarded = Convert.ToBoolean(r[21]),
                Stream = Convert.ToString(r[22]),
                Folder = (FolderType) Convert.ToInt32(r[23]),
                FolderRestore = (FolderType) Convert.ToInt32(r[24]),
                Spam = Convert.ToBoolean(r[25]),
                IsRemoved = Convert.ToBoolean(r[26]),
                TimeModified = Convert.ToDateTime(r[27]),
                MimeMessageId = Convert.ToString(r[28]),
                MimeInReplyTo = Convert.ToString(r[29]),
                ChainId = Convert.ToString(r[30]),
                ChainDate = Convert.ToDateTime(r[31]),
                IsTextBodyOnly = Convert.ToBoolean(r[32]),
                HasParseError = Convert.ToBoolean(r[33]),
                CalendarUid = Convert.ToString(r[34])
            };

            return mail;
        }
    }
}