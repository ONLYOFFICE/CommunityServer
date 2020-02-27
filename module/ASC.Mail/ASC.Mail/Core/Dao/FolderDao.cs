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
