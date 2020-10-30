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
using System.Globalization;
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
    public class UserFolderTreeDao : BaseDao, IUserFolderTreeDao
    {
        protected static ITable table = new MailTableFactory().Create<UserFolderTreeTable>();

        protected string CurrentUserId { get; private set; }

        public UserFolderTreeDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<UserFolderTreeItem> Get(IUserFoldersTreeExp exp)
        {
            var query = Query()
                .Where(exp.GetExpression());

            var list = Db.ExecuteList(query)
                .ConvertAll(ToUserFolderTreeItem);

            return list;
        }

        public int Save(UserFolderTreeItem item)
        {
            var query = new SqlInsert(UserFolderTreeTable.TABLE_NAME, true)
                .InColumnValue(UserFolderTreeTable.Columns.FolderId, item.FolderId)
                .InColumnValue(UserFolderTreeTable.Columns.ParentId, item.ParentId)
                .InColumnValue(UserFolderTreeTable.Columns.Level, item.Level);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        private readonly string _levelUp = string.Format("{0} + 1", UserFolderTreeTable.Columns.Level);

        private const string UFT_ALIAS = "uft";

        public int InsertFullPathToRoot(uint folderId, uint parentId)
        {
            var subQuery = new SqlQuery(UserFolderTreeTable.TABLE_NAME.Alias(UFT_ALIAS))
                .Select(folderId.ToString(),
                    UserFolderTreeTable.Columns.ParentId.Prefix(UFT_ALIAS),
                    _levelUp.Prefix(UFT_ALIAS))
                .Where(UserFolderTreeTable.Columns.FolderId.Prefix(UFT_ALIAS), parentId);

            var query = new SqlInsert(UserFolderTreeTable.TABLE_NAME, true)
                .InColumns(UserFolderTreeTable.Columns.FolderId, UserFolderTreeTable.Columns.ParentId,
                    UserFolderTreeTable.Columns.Level)
                .Values(subQuery);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Remove(IUserFoldersTreeExp exp)
        {
            var query = new SqlDelete(UserFolderTreeTable.TABLE_NAME)
                .Where(exp.GetExpression());

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public void Move(uint folderId, uint toFolderId)
        {
            var exp = SimpleUserFoldersTreeExp.CreateBuilder()
                .SetParent(folderId)
                .Build();

            var subFolders = Get(exp)
                .ToDictionary(r => r.FolderId, r => r.Level);

            if (!subFolders.Any())
            {
                return;
            }

            var query = new SqlDelete(UserFolderTreeTable.TABLE_NAME)
                .Where(Exp.In(UserFolderTreeTable.Columns.FolderId, subFolders.Keys) &
                       !Exp.In(UserFolderTreeTable.Columns.ParentId, subFolders.Keys));

            // ReSharper disable once NotAccessedVariable
            var result = Db.ExecuteNonQuery(query);

            foreach (var subFolder in subFolders)
            {
                var subQuery = new SqlQuery(UserFolderTreeTable.TABLE_NAME)
                    .Select(subFolder.Key.ToString(CultureInfo.InvariantCulture),
                        UserFolderTreeTable.Columns.ParentId,
                        string.Format("{0} + {1}", _levelUp, subFolder.Value.ToString(CultureInfo.InvariantCulture)))
                    .Where(UserFolderTreeTable.Columns.FolderId, toFolderId);

                var insertQuery = new SqlInsert(UserFolderTreeTable.TABLE_NAME, true)
                    .InColumns(UserFolderTreeTable.Columns.FolderId,
                        UserFolderTreeTable.Columns.ParentId,
                        UserFolderTreeTable.Columns.Level)
                    .Values(subQuery);

                // ReSharper disable once RedundantAssignment
                result = Db.ExecuteNonQuery(insertQuery);
            }
        }

        protected UserFolderTreeItem ToUserFolderTreeItem(object[] r)
        {
            var folder = new UserFolderTreeItem
            {
                FolderId = Convert.ToUInt32(r[0]),
                ParentId = Convert.ToUInt32(r[1]),
                Level = Convert.ToUInt32(r[2])
            };

            return folder;
        }
    }
}