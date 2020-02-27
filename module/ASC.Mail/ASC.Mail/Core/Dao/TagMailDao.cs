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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao
{
    public class TagMailDao : BaseDao, ITagMailDao
    {
        protected static ITable table = new MailTableFactory().Create<TagMailTable>();

        protected string CurrentUserId { get; private set; }

        public TagMailDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        private delegate SqlInsert CreateInsertDelegate();

        public void SetMessagesTag(IEnumerable<int> messageIds, int tagId)
        {
            var idMessages = messageIds as IList<int> ?? messageIds.ToList();
            if (!idMessages.Any())
                return;

            CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(TagMailTable.TABLE_NAME)
                    .IgnoreExists(true)
                    .InColumns(TagMailTable.Columns.MailId,
                        TagMailTable.Columns.TagId,
                        TagMailTable.Columns.Tenant,
                        TagMailTable.Columns.User);

            var insertQuery = createInsertQuery();

            int i, messagessLen;
            for (i = 0, messagessLen = idMessages.Count; i < messagessLen; i++)
            {
                var messageId = idMessages[i];

                insertQuery
                    .Values(messageId, tagId, Tenant, CurrentUserId);

                if ((i % 100 != 0 || i == 0) && i + 1 != messagessLen) 
                    continue;

                Db.ExecuteNonQuery(insertQuery);

                insertQuery = createInsertQuery();
            }
        }

        public int CalculateTagCount(int id)
        {
            var query = new SqlQuery(TagMailTable.TABLE_NAME)
                .SelectCount()
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId)
                .Where(TagMailTable.Columns.TagId, id);

            return Db.ExecuteScalar<int>(query);
        }

        public Dictionary<int, List<int>> GetMailTagsDictionary(List<int> mailIds)
        {
            var query = new SqlQuery(TagMailTable.TABLE_NAME)
                .Select(TagMailTable.Columns.MailId, TagMailTable.Columns.TagId)
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId)
                .Where(Exp.In(TagMailTable.Columns.MailId, mailIds));

            var dictionary = Db.ExecuteList(query)
                .ConvertAll(r => new
                {
                    MailId = Convert.ToInt32(r[0]),
                    TagId = Convert.ToInt32(r[1])
                })
                .GroupBy(t => t.MailId)
                .ToDictionary(g => g.Key, g => g.Select(t => t.TagId).ToList());

            return dictionary;
        }

        public List<int> GetTagIds(List<int> mailIds)
        {
            var query = new SqlQuery(TagMailTable.TABLE_NAME)
                .Select(TagMailTable.Columns.TagId)
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId)
                .Where(Exp.In(TagMailTable.Columns.MailId, mailIds));

            var tagIds = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .Distinct()
                .ToList();

            return tagIds;
        }

        public List<int> GetTagIds(int mailboxId)
        {
            var query = string.Format("select t.{0} from {1} t inner join {2} m on t.{3} = m.{4} where m.{5} = @mailbox_id",
                    TagMailTable.Columns.TagId, TagMailTable.TABLE_NAME, MailTable.TABLE_NAME, TagMailTable.Columns.MailId,
                    MailTable.Columns.Id, MailTable.Columns.MailboxId);

            var tagIds = Db.ExecuteList(query, new { mailbox_id = mailboxId })
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .Distinct()
                .ToList();

            return tagIds;
        }

        public int Delete(int tagId, List<int> mailIds)
        {
            var query = new SqlDelete(TagMailTable.TABLE_NAME)
                .Where(TagMailTable.Columns.TagId, tagId)
                .Where(Exp.In(TagMailTable.Columns.MailId, mailIds))
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteByTagId(int tagId)
        {
            var query = new SqlDelete(TagMailTable.TABLE_NAME)
                .Where(TagMailTable.Columns.TagId, tagId)
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(query);
        }

        private static readonly string QueryDeleteFormat =
            string.Format("delete t from {0} t inner join {1} m on t.{2} = m.{3} where m.{4} = @mailbox_id",
                TagMailTable.TABLE_NAME, MailTable.TABLE_NAME, TagMailTable.Columns.MailId, MailTable.Columns.Id,
                MailTable.Columns.MailboxId);

        public int DeleteByMailboxId(int mailboxId)
        {
            return Db.ExecuteNonQuery(QueryDeleteFormat, new { mailbox_id = mailboxId });
        }

        public int DeleteByMailIds(List<int> mailIds)
        {
            var query = new SqlDelete(TagMailTable.TABLE_NAME)
                .Where(TagMailTable.Columns.Tenant, Tenant)
                .Where(TagMailTable.Columns.User, CurrentUserId)
                .Where(Exp.In(TagMailTable.Columns.MailId, mailIds));

            return Db.ExecuteNonQuery(query);
        }
    }
}