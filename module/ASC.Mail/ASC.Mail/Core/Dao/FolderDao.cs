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
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao
{
    public class FolderDao : BaseDao, IFolderDao
    {
        protected static ITable table = new MailTableFactory().Create<FolderCountersTable>();

        protected string CurrentUserId { get; private set; }

        public FolderDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public Folder GetFolder(FolderType folder)
        {
            var query = Query()
                .Where(FolderCountersTable.Columns.Folder, (int) folder)
                .Where(FolderCountersTable.Columns.Tenant, Tenant)
                .Where(FolderCountersTable.Columns.User, CurrentUserId);

            return Db.ExecuteList(query)
                .ConvertAll(ToFolder)
                .SingleOrDefault();
        }

        public List<Folder> GetFolders()
        {
            var query = Query()
                .Where(FolderCountersTable.Columns.Tenant, Tenant)
                .Where(FolderCountersTable.Columns.User, CurrentUserId);

            return Db.ExecuteList(query)
                .ConvertAll(ToFolder);
        }

        public int Save(Folder folder)
        {
            var query = new SqlInsert(FolderCountersTable.TABLE_NAME, true)
                .InColumnValue(FolderCountersTable.Columns.Tenant, folder.Tenant)
                .InColumnValue(FolderCountersTable.Columns.User, folder.UserId)
                .InColumnValue(FolderCountersTable.Columns.Folder, (int) folder.FolderType)
                .InColumnValue(FolderCountersTable.Columns.UnreadMessagesCount, folder.UnreadCount)
                .InColumnValue(FolderCountersTable.Columns.UnreadConversationsCount, folder.UnreadChainCount)
                .InColumnValue(FolderCountersTable.Columns.TotalMessagesCount, folder.TotalCount)
                .InColumnValue(FolderCountersTable.Columns.TotalConversationsCount, folder.TotalChainCount);

            return Db.ExecuteNonQuery(query);
        }

        private const string INCR_VALUE_FORMAT = "{0}={0}+({1})";
        private const string SET_VALUE_FORMAT = "{0}={1}";

        public int ChangeFolderCounters(
            FolderType folder,
            int? unreadMessDiff = null,
            int? totalMessDiff = null,
            int? unreadConvDiff = null,
            int? totalConvDiff = null)
        {
            if (!unreadMessDiff.HasValue
                && !totalMessDiff.HasValue
                && !unreadConvDiff.HasValue
                && !totalConvDiff.HasValue)
            {
                return -1;
            }

            var updateQuery = new SqlUpdate(FolderCountersTable.TABLE_NAME)
                .Where(FolderCountersTable.Columns.Tenant, Tenant)
                .Where(FolderCountersTable.Columns.User, CurrentUserId)
                .Where(FolderCountersTable.Columns.Folder, (int) folder);

            Action<string, int?> setColumnValue = (column, item) =>
            {
                if (!item.HasValue)
                    return;

                updateQuery.Set(item.Value != 0
                    ? string.Format(INCR_VALUE_FORMAT, column, item.Value)
                    : string.Format(SET_VALUE_FORMAT, column, 0));
            };

            setColumnValue(FolderCountersTable.Columns.UnreadMessagesCount, unreadMessDiff);

            setColumnValue(FolderCountersTable.Columns.TotalMessagesCount, totalMessDiff);

            setColumnValue(FolderCountersTable.Columns.UnreadConversationsCount, unreadConvDiff);

            setColumnValue(FolderCountersTable.Columns.TotalConversationsCount, totalConvDiff);

            var result = Db.ExecuteNonQuery(updateQuery);

            return result;
        }

        public int Delete()
        {
            var query = new SqlDelete(FolderCountersTable.TABLE_NAME)
                .Where(MailTable.Columns.Tenant, Tenant)
                .Where(MailTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(query);
        }

        protected Folder ToFolder(object[] r)
        {
            var f = new Folder
            {
                Tenant = Convert.ToInt32(r[0]),
                UserId = Convert.ToString(r[1]),
                FolderType = (FolderType) Convert.ToInt32(r[2]),
                TimeModified = Convert.ToDateTime(r[3]),
                UnreadCount = Convert.ToInt32(r[4]),
                TotalCount = Convert.ToInt32(r[5]),
                UnreadChainCount = Convert.ToInt32(r[6]),
                TotalChainCount = Convert.ToInt32(r[7])
            };

            return f;
        }
    }
}
