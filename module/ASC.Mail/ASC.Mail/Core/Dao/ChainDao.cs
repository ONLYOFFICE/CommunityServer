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
using ASC.Mail.Core.Dao.Expressions.Conversation;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao
{
    public class ChainDao : BaseDao, IChainDao
    {
        protected static ITable table = new MailTableFactory().Create<ChainTable>();

        protected string CurrentUserId { get; private set; }

        public ChainDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<Chain> GetChains(IConversationsExp exp)
        {
            var query = Query()
                .Where(exp.GetExpression());

            return Db.ExecuteList(query)
                .ConvertAll(ToChain);
        }

        public Dictionary<int, int> GetChainCount(IConversationsExp exp)
        {
            var query = new SqlQuery(ChainTable.TABLE_NAME)
                .Select(ChainTable.Columns.Folder)
                .SelectCount()
                .Where(exp.GetExpression())
                .GroupBy(ChainTable.Columns.Folder);

            return Db.ExecuteList(query)
                .ConvertAll(r => new
                {
                    folder = Convert.ToInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);
        }

        private const string QUERY_COUNT_FORMAT = "SELECT chains.{3}, COUNT(*) FROM " +
                                                  "(select t.{3}, c.{4} from {0} t " +
                                                  "inner join {1} m on t.{5} = m.{6} " +
                                                  "inner join {2} c on m.{7} = c.{4} AND " +
                                                                      "m.id_mailbox = c.id_mailbox AND " +
                                                                      "m.tenant = c.tenant AND " +
                                                                      "m.id_user = c.id_user AND " +
                                                                      "m.folder = c.folder " +
                                                  "where t.{8} = @tenant and t.{9} = @user {10}" +
                                                  "group by t.{3}, c.{4}) as chains " + 
                                                  "GROUP BY chains.{3};";

        public Dictionary<uint, int> GetChainUserFolderCount(bool? unread = null)
        {
            var query = string.Format(QUERY_COUNT_FORMAT,
                UserFoldertXMailTable.TABLE_NAME,
                MailTable.TABLE_NAME,
                ChainTable.TABLE_NAME,
                UserFoldertXMailTable.Columns.FolderId,
                ChainTable.Columns.Id,
                UserFoldertXMailTable.Columns.MailId,
                MailTable.Columns.Id,
                MailTable.Columns.ChainId,
                UserFoldertXMailTable.Columns.Tenant,
                UserFoldertXMailTable.Columns.User,
                unread.HasValue ? string.Format("and m.{0} = {1} ", MailTable.Columns.Unread, unread.Value ? 1 : 0) : "");

            var result = Db.ExecuteList(query, new { tenant = Tenant, user = CurrentUserId })
                .ConvertAll(r => new
                {
                    folder = Convert.ToUInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);

            return result;
        }

        public Dictionary<uint, int> GetChainUserFolderCount(List<int> userFolderIds, bool? unread = null)
        {
            var query = string.Format(QUERY_COUNT_FORMAT,
                UserFoldertXMailTable.TABLE_NAME,
                MailTable.TABLE_NAME,
                ChainTable.TABLE_NAME,
                UserFoldertXMailTable.Columns.FolderId,
                ChainTable.Columns.Id,
                UserFoldertXMailTable.Columns.MailId,
                MailTable.Columns.Id,
                MailTable.Columns.ChainId,
                UserFoldertXMailTable.Columns.Tenant,
                UserFoldertXMailTable.Columns.User,
                string.Format("and t.{0} in ({1}) {2}", UserFoldertXMailTable.Columns.FolderId, string.Join(",", userFolderIds), 
                    unread.HasValue 
                        ? string.Format("and m.{0} = {1} ", MailTable.Columns.Unread, unread.Value ? 1 : 0) 
                        : "")
                );

            var result = Db.ExecuteList(query, new { tenant = Tenant, user = CurrentUserId })
                .ConvertAll(r => new
                {
                    folder = Convert.ToUInt32(r[0]),
                    count = Convert.ToInt32(r[1])
                })
                .ToDictionary(o => o.folder, o => o.count);

            return result;
        }

        public int SaveChain(Chain chain)
        {
            var query = new SqlInsert(ChainTable.TABLE_NAME, true)
                .InColumnValue(ChainTable.Columns.Id, chain.Id)
                .InColumnValue(ChainTable.Columns.MailboxId, chain.MailboxId)
                .InColumnValue(ChainTable.Columns.Tenant, chain.Tenant)
                .InColumnValue(ChainTable.Columns.User, chain.User)
                .InColumnValue(ChainTable.Columns.Folder, chain.Folder)
                .InColumnValue(ChainTable.Columns.Length, chain.Length)
                .InColumnValue(ChainTable.Columns.Unread, chain.Unread)
                .InColumnValue(ChainTable.Columns.HasAttachments, chain.HasAttachments)
                .InColumnValue(ChainTable.Columns.Importance, chain.Importance)
                .InColumnValue(ChainTable.Columns.Tags, chain.Tags);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Delete(IConversationsExp exp)
        {
            var query = new SqlDelete(ChainTable.TABLE_NAME)
                .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int SetFieldValue<T>(IConversationsExp exp, string field, T value)
        {
            var query =
                new SqlUpdate(ChainTable.TABLE_NAME)
                    .Set(field, value)
                    .Where(exp.GetExpression());

            return Db.ExecuteNonQuery(query);
        }

        protected Chain ToChain(object[] r)
        {
            var chain = new Chain
            {
                Id = Convert.ToString(r[0]),
                MailboxId = Convert.ToInt32(r[1]),
                Tenant = Convert.ToInt32(r[2]),
                User = Convert.ToString(r[3]),
                Folder = (FolderType) Convert.ToInt32(r[4]),
                Length = Convert.ToInt32(r[5]),
                Unread = Convert.ToBoolean(r[6]),
                HasAttachments = Convert.ToBoolean(r[7]),
                Importance = Convert.ToBoolean(r[8]),
                Tags = Convert.ToString(r[9])
            };

            return chain;
        }
    }
}