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

namespace ASC.Mail.Core.Dao
{
    public class UserFolderXMailDao : BaseDao, IUserFolderXMailDao
    {
        protected static ITable table = new MailTableFactory().Create<UserFoldertXMailTable>();

        protected string CurrentUserId { get; private set; }

        public UserFolderXMailDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public UserFolderXMail Get(int mailId)
        {
            var query = Query()
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(UserFoldertXMailTable.Columns.MailId, mailId);

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolderXMail)
                .SingleOrDefault();

            return result;
        }

        public List<UserFolderXMail> GetList(uint? folderId = null, List<int> mailIds = null)
        {
            var query = Query()
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId);

            if (folderId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.FolderId, folderId.Value);
            }

            if (mailIds != null && mailIds.Any())
            {
                query.Where(Exp.In(UserFoldertXMailTable.Columns.MailId, mailIds));
            }

            var list = Db.ExecuteList(query)
                .ConvertAll(ToUserFolderXMail);

            return list;
        }

        public List<int> GetMailIds(uint folderId)
        {
            var query = new SqlQuery(UserFoldertXMailTable.TABLE_NAME)
                .Select(UserFoldertXMailTable.Columns.MailId)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(UserFoldertXMailTable.Columns.FolderId, folderId);

            var list = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]));

            return list;
        }

        private delegate SqlInsert CreateInsertDelegate();

        public void SetMessagesFolder(IEnumerable<int> messageIds, uint folderId)
        {
            var idMessages = messageIds as IList<int> ?? messageIds.ToList();
            if (!idMessages.Any())
                return;

            CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(UserFoldertXMailTable.TABLE_NAME)
                    .IgnoreExists(true)
                    .InColumns(UserFoldertXMailTable.Columns.Tenant,
                        UserFoldertXMailTable.Columns.User,
                        UserFoldertXMailTable.Columns.MailId,
                        UserFoldertXMailTable.Columns.FolderId);

            var insertQuery = createInsertQuery();

            int i, messagessLen;
            for (i = 0, messagessLen = idMessages.Count; i < messagessLen; i++)
            {
                var messageId = idMessages[i];

                insertQuery
                    .Values(Tenant, CurrentUserId, messageId, folderId);

                if ((i % 100 != 0 || i == 0) && i + 1 != messagessLen)
                    continue;

                Db.ExecuteNonQuery(insertQuery);

                insertQuery = createInsertQuery();
            }
        }

        public int Save(UserFolderXMail item)
        {
            var query = new SqlInsert(UserFoldertXMailTable.TABLE_NAME, true)
                .InColumnValue(UserFoldertXMailTable.Columns.Tenant, item.Tenant)
                .InColumnValue(UserFoldertXMailTable.Columns.User, item.User)
                .InColumnValue(UserFoldertXMailTable.Columns.MailId, item.MailId)
                .InColumnValue(UserFoldertXMailTable.Columns.FolderId, item.FolderId);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(int? mailId = null, uint? folderId = null)
        {
            var query = new SqlDelete(UserFoldertXMailTable.TABLE_NAME)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId);

            if (mailId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.MailId, mailId.Value);
            }

            if (folderId.HasValue)
            {
                query.Where(UserFoldertXMailTable.Columns.FolderId, folderId.Value);
            }

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(List<int> mailIds)
        {
            var query = new SqlDelete(UserFoldertXMailTable.TABLE_NAME)
                .Where(UserFoldertXMailTable.Columns.Tenant, Tenant)
                .Where(UserFoldertXMailTable.Columns.User, CurrentUserId)
                .Where(Exp.In(UserFoldertXMailTable.Columns.MailId, mailIds));

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        private static readonly string QueryDeleteFormat =
                string.Format(
                    "delete t from {0} t inner join {1} m " +
                    "on t.{2} = m.{3} and t.{4} = m.{5} and t.{6} = m.{7} " +
                    "where m.{8} = @mailbox_id and m.{5} = @tenant and m.{7} = @user",
                    UserFoldertXMailTable.TABLE_NAME, MailTable.TABLE_NAME,
                    UserFoldertXMailTable.Columns.MailId, MailTable.Columns.Id,
                    UserFoldertXMailTable.Columns.Tenant, MailTable.Columns.Tenant,
                    UserFoldertXMailTable.Columns.User, MailTable.Columns.User,
                    MailTable.Columns.MailboxId);

        public int RemoveByMailbox(int mailboxId)
        {
            return Db.ExecuteNonQuery(QueryDeleteFormat, new {mailbox_id = mailboxId, tenant = Tenant, user = CurrentUserId});
        }

        protected UserFolderXMail ToUserFolderXMail(object[] r)
        {
            var folderXMail = new UserFolderXMail
            {
                Tenant = Convert.ToInt32(r[0]),
                User = Convert.ToString(r[1]),
                MailId = Convert.ToInt32(r[2]),
                FolderId = Convert.ToUInt32(r[3]),
                TimeModified = Convert.ToDateTime(r[4])
            };

            return folderXMail;
        }
    }
}