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
using ASC.Mail.Core.Dao.Expressions.UserFolder;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class UserFolderDao : BaseDao, IUserFolderDao
    {
        protected static ITable table = new MailTableFactory().Create<UserFolderTable>();

        protected string CurrentUserId { get; private set; }

        public UserFolderDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public UserFolder Get(uint id)
        {
            var query = Query()
                .Where(UserFolderTable.Columns.Tenant, Tenant)
                .Where(UserFolderTable.Columns.User, CurrentUserId)
                .Where(UserFolderTable.Columns.Id, id);

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder)
                .SingleOrDefault();

            return result;
        }

        public UserFolder GetByMail(uint mailId)
        {
            var subQuery = new SqlQuery(UserFoldertXMailTable.TABLE_NAME)
                .Select(UserFoldertXMailTable.Columns.FolderId)
                .Where(UserFoldertXMailTable.Columns.MailId, mailId)
                .Distinct();

            var query = Query()
                .Where(Exp.EqColumns(UserFolderTable.Columns.Id, subQuery));

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder)
                .SingleOrDefault();

            return result;
        }

        public List<UserFolder> GetList(IUserFoldersExp exp)
        {
            var query = Query()
                .Where(exp.GetExpression());

            if (exp.StartIndex.HasValue)
            {
                query.SetFirstResult(exp.StartIndex.Value);
            }

            if (exp.Limit.HasValue)
            {
                query.SetMaxResults(exp.Limit.Value);
            }

            if (!string.IsNullOrEmpty(exp.OrderBy))
            {
                var sortField = UserFolderTable.Columns.Name;

                if (exp.OrderBy == "timeModified")
                {
                    sortField = UserFolderTable.Columns.TimeModified;
                }

                query.OrderBy(sortField, exp.OrderAsc != null && exp.OrderAsc.Value);
            }

            var list = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder);

            return list;
        }

        public UserFolder GetRootFolder(uint folderId)
        {
            var subQuery = new SqlQuery(UserFolderTreeTable.TABLE_NAME)
                .Select(UserFolderTreeTable.Columns.ParentId)
                .Where(UserFolderTreeTable.Columns.FolderId, folderId)
                .SetMaxResults(1)
                .OrderBy(UserFolderTreeTable.Columns.Level, false);

            var query = Query()
                .Where(Exp.EqColumns(UserFolderTable.Columns.Id, subQuery));

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder)
                .SingleOrDefault();

            return result;
        }

        public UserFolder GetRootFolderByMailId(int mailId)
        {
            var subSubQuery = new SqlQuery(UserFoldertXMailTable.TABLE_NAME)
                .Select(UserFoldertXMailTable.Columns.FolderId)
                .Where(UserFoldertXMailTable.Columns.MailId, mailId)
                .Distinct();

            var subQuery = new SqlQuery(UserFolderTreeTable.TABLE_NAME)
                .Select(UserFolderTreeTable.Columns.ParentId)
                .Where(Exp.EqColumns(UserFolderTreeTable.Columns.FolderId, subSubQuery))
                .SetMaxResults(1)
                .OrderBy(UserFolderTreeTable.Columns.Level, false);

            var query = Query()
                .Where(Exp.EqColumns(UserFolderTable.Columns.Id, subQuery));

            var result = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder)
                .SingleOrDefault();

            return result;
        }

        public List<UserFolder> GetParentFolders(uint folderId)
        {
            const string folder_alias = "f";
            const string folder_tree_alias = "ft";

            var query = Query(folder_alias)
                .InnerJoin(UserFolderTreeTable.TABLE_NAME.Alias(folder_tree_alias),
                    Exp.EqColumns(UserFolderTable.Columns.Id.Prefix(folder_alias),
                        UserFolderTreeTable.Columns.ParentId.Prefix(folder_tree_alias)))
                .Where(UserFolderTreeTable.Columns.FolderId.Prefix(folder_tree_alias), folderId)
                .OrderBy(UserFolderTreeTable.Columns.Level.Prefix(folder_tree_alias), false);

            var list = Db.ExecuteList(query)
                .ConvertAll(ToUserFolder);

            return list;
        }

        public uint Save(UserFolder folder)
        {
            var query = new SqlInsert(UserFolderTable.TABLE_NAME, true)
                .InColumnValue(UserFolderTable.Columns.Id, folder.Id)
                .InColumnValue(UserFolderTable.Columns.ParentId, folder.ParentId)
                .InColumnValue(UserFolderTable.Columns.Tenant, folder.Tenant)
                .InColumnValue(UserFolderTable.Columns.User, folder.User)
                .InColumnValue(UserFolderTable.Columns.Name, folder.Name)
                .InColumnValue(UserFolderTable.Columns.FolderCount, folder.FolderCount)
                .InColumnValue(UserFolderTable.Columns.UnreadMessagesCount, folder.UnreadCount)
                .InColumnValue(UserFolderTable.Columns.TotalMessagesCount, folder.TotalCount)
                .InColumnValue(UserFolderTable.Columns.UnreadConversationsCount, folder.UnreadChainCount)
                .InColumnValue(UserFolderTable.Columns.TotalConversationsCount, folder.TotalChainCount)
                .InColumnValue(UserFolderTable.Columns.TimeModified, folder.TimeModified)
                .Identity(0, (uint) 0, true);

            return Db.ExecuteScalar<uint>(query);
        }

        public int Remove(uint id)
        {
            var query = new SqlDelete(UserFolderTable.TABLE_NAME)
                .Where(UserFolderTable.Columns.Tenant, Tenant)
                .Where(UserFolderTable.Columns.User, CurrentUserId)
                .Where(UserFolderTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(IUserFoldersExp exp)
        {
            var query = new SqlDelete(UserFolderTable.TABLE_NAME)
                .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        private static readonly string SetFolderCount =
            string.Format("{0} = (select count(*) - 1 from {1} where {2} = {3})",
                UserFolderTable.Columns.FolderCount, UserFolderTreeTable.TABLE_NAME,
                UserFolderTreeTable.Columns.ParentId, UserFolderTable.Columns.Id);

        public void RecalculateFoldersCount(uint id)
        {
            var subQuery = new SqlQuery(UserFolderTreeTable.TABLE_NAME)
                .Select(UserFolderTreeTable.Columns.ParentId)
                .Where(UserFolderTreeTable.Columns.FolderId, id);

            var query = new SqlUpdate(UserFolderTable.TABLE_NAME)
                .Set(SetFolderCount)
                .Where(Exp.In(UserFolderTable.Columns.Id, subQuery));

            // ReSharper disable once UnusedVariable
            var result =  Db.ExecuteNonQuery(query);
        }

        private const string INCR_VALUE_FORMAT = "{0}={0}+({1})";
        private const string SET_VALUE_FORMAT = "{0}={1}";

        public int SetFolderCounters(uint folderId, int? unreadMess = null, int? totalMess = null,
            int? unreadConv = null, int? totalConv = null)
        {
            if (!unreadMess.HasValue
                && !totalMess.HasValue
                && !unreadConv.HasValue
                && !totalConv.HasValue)
            {
                return -1;
            }

            var updateQuery = new SqlUpdate(UserFolderTable.TABLE_NAME)
                .Where(UserFolderTable.Columns.Tenant, Tenant)
                .Where(UserFolderTable.Columns.User, CurrentUserId)
                .Where(UserFolderTable.Columns.Id, folderId);

            Action<string, int?> setColumnValue = (column, item) =>
            {
                if (!item.HasValue)
                    return;

                updateQuery.Set(string.Format(SET_VALUE_FORMAT, column, item.Value));
            };

            setColumnValue(UserFolderTable.Columns.UnreadMessagesCount, unreadMess);

            setColumnValue(UserFolderTable.Columns.TotalMessagesCount, totalMess);

            setColumnValue(UserFolderTable.Columns.UnreadConversationsCount, unreadConv);

            setColumnValue(UserFolderTable.Columns.TotalConversationsCount, totalConv);

            var result = Db.ExecuteNonQuery(updateQuery);

            return result;
        }

        public int ChangeFolderCounters(uint folderId, int? unreadMessDiff = null, int? totalMessDiff = null,
            int? unreadConvDiff = null, int? totalConvDiff = null)
        {
            if (!unreadMessDiff.HasValue
                && !totalMessDiff.HasValue
                && !unreadConvDiff.HasValue
                && !totalConvDiff.HasValue)
            {
                return -1;
            }

            var updateQuery = new SqlUpdate(UserFolderTable.TABLE_NAME)
                .Where(UserFolderTable.Columns.Tenant, Tenant)
                .Where(UserFolderTable.Columns.User, CurrentUserId)
                .Where(UserFolderTable.Columns.Id, folderId);

            Action<string, int?> setColumnValue = (column, item) =>
            {
                if (!item.HasValue)
                    return;

                updateQuery.Set(item.Value != 0
                    ? string.Format(INCR_VALUE_FORMAT, column, item.Value)
                    : string.Format(SET_VALUE_FORMAT, column, 0));
            };

            setColumnValue(UserFolderTable.Columns.UnreadMessagesCount, unreadMessDiff);

            setColumnValue(UserFolderTable.Columns.TotalMessagesCount, totalMessDiff);

            setColumnValue(UserFolderTable.Columns.UnreadConversationsCount, unreadConvDiff);

            setColumnValue(UserFolderTable.Columns.TotalConversationsCount, totalConvDiff);

            var result = Db.ExecuteNonQuery(updateQuery);

            return result;
        }

        protected UserFolder ToUserFolder(object[] r)
        {
            var folder = new UserFolder
            {
                Id = Convert.ToUInt32(r[0]),
                ParentId = Convert.ToUInt32(r[1]),
                
                Tenant = Convert.ToInt32(r[2]),
                User = Convert.ToString(r[3]),
                
                Name = Convert.ToString(r[4]),
                FolderCount = Convert.ToInt32(r[5]),

                UnreadCount = Convert.ToInt32(r[6]),
                TotalCount = Convert.ToInt32(r[7]),

                UnreadChainCount = Convert.ToInt32(r[8]),
                TotalChainCount = Convert.ToInt32(r[9]),

                TimeModified = Convert.ToDateTime(r[10])
            };

            return folder;
        }
    }
}