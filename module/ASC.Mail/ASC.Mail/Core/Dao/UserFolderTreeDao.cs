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