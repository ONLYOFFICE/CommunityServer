/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.ComplexOperations;
using ASC.Mail.Aggregator.ComplexOperations.Base;
using ASC.Mail.Aggregator.DbSchema;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region structures defines

        public class MailFolderInfo
        {
            public int id;
            public DateTime timeModified;
            public int unread;
            public int unreadMessages;
            public int total;
            public int totalMessages;
        }

        #endregion

        #region public methods

        public List<MailFolderInfo> GetFolders(int tenant, string user)
        {
            Func<DbManager, List<MailFolderInfo>> getFoldersList = (db) =>
            {
                var query = new SqlQuery(FolderTable.Name)
                    .Select(FolderTable.Columns.Folder,
                        FolderTable.Columns.TimeModified,
                        FolderTable.Columns.UnreadConversationsCount,
                        FolderTable.Columns.UnreadMessagesCount,
                        FolderTable.Columns.TotalConversationsCount,
                        FolderTable.Columns.TotalMessagesCount)
                    .Where(GetUserWhere(user, tenant));

                // Try catch needed for resolve issue with folder's counter overflow.
                try
                {
                    var queryRes = db.ExecuteList(query)
                        .ConvertAll(x => new MailFolderInfo
                        {
                            id = Convert.ToInt32(x[0]),
                            timeModified = Convert.ToDateTime(x[1]),
                            unread = Convert.ToInt32(x[2]),
                            unreadMessages = Convert.ToInt32(x[3]),
                            total = Convert.ToInt32(x[4]),
                            totalMessages = Convert.ToInt32(x[5])
                        });

                    if (queryRes.Any())
                        return queryRes;
                }
                catch (Exception ex)
                {
                    _log.Error("GetFoldersList( \r\nException:{0}\r\n", ex.ToString());
                }

                return new List<MailFolderInfo>();
            };

            using (var db = GetDb())
            {
                var list = getFoldersList(db);

                if (!list.Any())
                    RecalculateFolders();

                return list;
            }
        }

        #endregion

        #region private methods

        private const string INCR_VALUE_FORMAT = "{0}={0}+({1})";

        private void ChangeFolderCounters(
            IDbManager db,
            int tenant,
            string user,
            int folder,
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
                return;
            }

            var updateQuery = new SqlUpdate(FolderTable.Name)
                .Where(GetUserWhere(user, tenant))
                .Where(FolderTable.Columns.Folder, folder);

            if (unreadMessDiff.HasValue)
            {
                var setValue = string.Format(INCR_VALUE_FORMAT,
                    FolderTable.Columns.UnreadMessagesCount, unreadMessDiff.Value);

                updateQuery.Set(setValue);
            }

            if (totalMessDiff.HasValue)
            {
                var setValue = string.Format(INCR_VALUE_FORMAT,
                    FolderTable.Columns.TotalMessagesCount, totalMessDiff.Value);

                updateQuery.Set(setValue);
            }

            if (unreadConvDiff.HasValue)
            {
                var setValue = string.Format(INCR_VALUE_FORMAT,
                    FolderTable.Columns.UnreadConversationsCount, unreadConvDiff.Value);

                updateQuery.Set(setValue);
            }

            if (totalConvDiff.HasValue)
            {
                var setValue = string.Format(INCR_VALUE_FORMAT,
                    FolderTable.Columns.TotalConversationsCount, totalConvDiff.Value);

                updateQuery.Set(setValue);
            }

            try
            {
                if (0 == db.ExecuteNonQuery(updateQuery))
                {
                    throw new Exception("Need recalculation");
                }
            }
            catch
            {
                RecalculateFolders();
            }
        }

        public MailOperationStatus RecalculateFolders(Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RecalculateFolders;
                });

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                return GetMailOperationStatus(runningOperation.Id, translateMailOperationStatus);

            var op = new MailRecalculateFoldersOperation(tenant, user, this);

            return QueueTask(op, translateMailOperationStatus);

        }

        public void RecalculateFolders(int tenant, string user, Action<MailOperationRecalculateMailboxProgress> callback = null)
        {
            using (var db = GetDb(RecalculateFoldersTimeout))
            {
                RecalculateFolders(db, tenant, user, callback);
            }
        }

        private void RecalculateFolders(IDbManager db, int tenant, string user, Action<MailOperationRecalculateMailboxProgress> callback = null)
        {
            var folderIds = new[]
                {
                    MailFolder.Ids.temp,
                    MailFolder.Ids.inbox,
                    MailFolder.Ids.sent,
                    MailFolder.Ids.drafts,
                    MailFolder.Ids.trash,
                    MailFolder.Ids.spam
                };

            var unreadMessagesCountByFolderQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.Folder, "count(*)")
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.Unread, true)
                .Where(MailTable.Columns.IsRemoved, false)
                .GroupBy(MailTable.Columns.Folder);

            if (callback != null)
                callback(MailOperationRecalculateMailboxProgress.CountUnreadMessages);

            var unreadMessagesCountByFolder =
                db.ExecuteList(unreadMessagesCountByFolderQuery)
                    .ConvertAll(
                        x =>
                            new KeyValuePair<int, int>(
                                Convert.ToInt32(x[0]),
                                Convert.ToInt32(x[1])));

            var totalMessagesCountByFolderQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.Folder, "count(*)")
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.IsRemoved, false)
                .GroupBy(MailTable.Columns.Folder);

            if (callback != null)
                callback(MailOperationRecalculateMailboxProgress.CountTotalMessages);

            var totalMessagesCountByFolder =
                db.ExecuteList(totalMessagesCountByFolderQuery)
                    .ConvertAll(
                        x =>
                            new KeyValuePair<int, int>(Convert.ToInt32(x[0]),
                                Convert.ToInt32(x[1])));

            var unreadConversationsCountByFolderQuery = new SqlQuery(ChainTable.Name)
                .Select(ChainTable.Columns.Folder, "count(*)")
                .Where(GetUserWhere(user, tenant))
                .Where(ChainTable.Columns.Unread, true)
                .GroupBy(ChainTable.Columns.Folder);

            if (callback != null)
                callback(MailOperationRecalculateMailboxProgress.CountUreadConversation);

            var unreadConversationsCountByFolder =
                db.ExecuteList(unreadConversationsCountByFolderQuery)
                    .ConvertAll(
                        x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

            var totalConversationsCountByFolderQuery = new SqlQuery(ChainTable.Name)
                .Select(ChainTable.Columns.Folder, "count(*)")
                .Where(GetUserWhere(user, tenant))
                .GroupBy(ChainTable.Columns.Folder);

            if (callback != null)
                callback(MailOperationRecalculateMailboxProgress.CountTotalConversation);

            var totalConversationsCountByFolder =
                db.ExecuteList(totalConversationsCountByFolderQuery)
                    .ConvertAll(
                        x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

            if (callback != null)
                callback(MailOperationRecalculateMailboxProgress.UpdateFoldersCounters);

            var foldersInfo = from folderId in folderIds
                               let unreadMessCount = unreadMessagesCountByFolder.Find(c => c.Key == folderId).Value
                               let totalMessCount = totalMessagesCountByFolder.Find(c => c.Key == folderId).Value
                               let unreadConvCount = unreadConversationsCountByFolder.Find(c => c.Key == folderId).Value
                               let totalConvCount = totalConversationsCountByFolder.Find(c => c.Key == folderId).Value
                               select new 
                                   {
                                       id = folderId,
                                       unread_messages_count = unreadMessCount,
                                       total_messages_count = totalMessCount,
                                       unread_conversations_count = unreadConvCount,
                                       total_conversations_count = totalConvCount,
                                       time_modified = DateTime.UtcNow
                                   };

            foreach (var info in foldersInfo)
            {
                var insert = new SqlInsert(FolderTable.Name, true)
                    .InColumnValue(FolderTable.Columns.Tenant, tenant)
                    .InColumnValue(FolderTable.Columns.User, user)
                    .InColumnValue(FolderTable.Columns.Folder, info.id)
                    .InColumnValue(FolderTable.Columns.UnreadMessagesCount, info.unread_messages_count)
                    .InColumnValue(FolderTable.Columns.UnreadConversationsCount, info.unread_conversations_count)
                    .InColumnValue(FolderTable.Columns.TotalMessagesCount, info.total_messages_count)
                    .InColumnValue(FolderTable.Columns.TotalConversationsCount, info.total_conversations_count);

                db.ExecuteNonQuery(insert);
            }
        }

        #endregion
    }
}
