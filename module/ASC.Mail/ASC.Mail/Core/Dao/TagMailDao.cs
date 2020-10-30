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