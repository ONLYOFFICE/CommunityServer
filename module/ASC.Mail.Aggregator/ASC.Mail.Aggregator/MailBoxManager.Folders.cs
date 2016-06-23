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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Common;
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

        public List<MailFolderInfo> GetFolders(int tenant, string user, bool isConversation)
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

                if (list.Any())
                    return list;

                RecalculateFolders(db, tenant, user, !isConversation);

                RecalculateFolders(db, tenant, user, isConversation);

                list = getFoldersList(db);

                return list;
            }
        }

        #endregion

        #region private methods

        private void ChangeFolderCounters(IDbManager db, int tenant, string user, int folder, int unreadDiff,
                                          int totalDiff, bool isConversation)
        {
            var res = 0;
            if (unreadDiff != 0 || totalDiff != 0)
            {
                var updateQuery = new SqlUpdate(FolderTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderTable.Columns.Folder, folder);

                if (unreadDiff != 0)
                {
                    if (isConversation)
                        updateQuery.Set(FolderTable.Columns.UnreadConversationsCount + "=" +
                                         FolderTable.Columns.UnreadConversationsCount + "+(" + unreadDiff + ")");
                    else
                        updateQuery.Set(FolderTable.Columns.UnreadMessagesCount + "=" + FolderTable.Columns.UnreadMessagesCount +
                                         "+(" + unreadDiff + ")");
                }

                if (totalDiff != 0)
                {
                    if (isConversation)
                        updateQuery.Set(FolderTable.Columns.TotalConversationsCount + "=" +
                                         FolderTable.Columns.TotalConversationsCount + "+(" + totalDiff + ")");
                    else
                    {
                        updateQuery.Set(FolderTable.Columns.TotalMessagesCount + "=" +
                                         FolderTable.Columns.TotalMessagesCount + "+(" + totalDiff + ")");
                    }
                }

                res = db.ExecuteNonQuery(updateQuery);
            }

            if (0 == res)
                RecalculateFolders(db, tenant, user);
        }

        private void ChangeFolderCounters(
            IDbManager db,
            int tenant,
            string user,
            int folder,
            int unreadMessDiff,
            int totalMessDiff,
            int unreadConvDiff,
            int totalConvDiff)
        {
            if (0 == unreadMessDiff && 0 == totalMessDiff && 0 == unreadConvDiff && 0 == totalConvDiff)
                return;

            var updateQuery = new SqlUpdate(FolderTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderTable.Columns.Folder, folder);

            if (0 != unreadMessDiff)
                updateQuery.Set(FolderTable.Columns.UnreadMessagesCount + "=" +
                                         FolderTable.Columns.UnreadMessagesCount + "+(" + unreadMessDiff + ")");

            if (0 != totalMessDiff)
                updateQuery.Set(FolderTable.Columns.TotalMessagesCount + "=" +
                                         FolderTable.Columns.TotalMessagesCount + "+(" + totalMessDiff + ")");

            if (0 != unreadConvDiff)
                updateQuery.Set(FolderTable.Columns.UnreadConversationsCount + "=" +
                                         FolderTable.Columns.UnreadConversationsCount + "+(" + unreadConvDiff + ")");

            if (0 != totalConvDiff)
                updateQuery.Set(FolderTable.Columns.TotalConversationsCount + "=" +
                                         FolderTable.Columns.TotalConversationsCount + "+(" + totalConvDiff + ")");

            try
            {
                if (0 == db.ExecuteNonQuery(updateQuery))
                    throw new Exception("Need recalculation");
            }
            catch
            {
                RecalculateFolders(db, tenant, user);
            }
        }

        private void RecalculateFolders(IDbManager db, int tenant, string user)
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

            var unreadMessagesCountByFolder =
                db.ExecuteList(
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.Unread, true)
                        .Where(MailTable.Columns.IsRemoved, false)
                        .GroupBy(MailTable.Columns.Folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(
                          Convert.ToInt32(x[0]),
                          Convert.ToInt32(x[1])));

            var totalMessagesCountByFolder =
                db.ExecuteList(
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.IsRemoved, false)
                        .GroupBy(MailTable.Columns.Folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(Convert.ToInt32(x[0]),
                                                 Convert.ToInt32(x[1])));

            var unreadConversationsCountByFolder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.Name)
                        .Select(ChainTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.Unread, true)
                        .GroupBy(ChainTable.Columns.Folder))
                  .ConvertAll(
                      x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

            var totalConversationsCountByFolder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.Name)
                        .Select(ChainTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.Folder))
                  .ConvertAll(
                      x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));


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
                db.ExecuteNonQuery(
                    new SqlInsert(FolderTable.Name, true)
                        .InColumnValue(FolderTable.Columns.Tenant, tenant)
                        .InColumnValue(FolderTable.Columns.User, user)
                        .InColumnValue(FolderTable.Columns.Folder, info.id)
                        .InColumnValue(FolderTable.Columns.UnreadMessagesCount, info.unread_messages_count)
                        .InColumnValue(FolderTable.Columns.UnreadConversationsCount, info.unread_conversations_count)
                        .InColumnValue(FolderTable.Columns.TotalMessagesCount, info.total_messages_count)
                        .InColumnValue(FolderTable.Columns.TotalConversationsCount, info.total_conversations_count));
            }
        }

        private void RecalculateFolders(IDbManager db, int tenant, string user, bool isConversation)
        {
            List<KeyValuePair<int, int>> unreadCount;
            List<KeyValuePair<int, int>> totalCount;
            var folders = new List<MailFolderInfo>();
            var folderIds = new[]
                {
                    MailFolder.Ids.temp,
                    MailFolder.Ids.inbox,
                    MailFolder.Ids.sent,
                    MailFolder.Ids.drafts,
                    MailFolder.Ids.trash,
                    MailFolder.Ids.spam
                };

            if (!isConversation)
            {
                unreadCount = db.ExecuteList(
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.Unread, true)
                        .Where(MailTable.Columns.IsRemoved, false)
                        .GroupBy(MailTable.Columns.Folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                totalCount = db.ExecuteList(
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.IsRemoved, false)
                        .GroupBy(MailTable.Columns.Folder))
                                .ConvertAll(
                                    x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));
            }
            else
            {
                unreadCount = db.ExecuteList(
                    new SqlQuery(ChainTable.Name)
                        .Select(ChainTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.Unread, true)
                        .GroupBy(ChainTable.Columns.Folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                totalCount = db.ExecuteList(
                    new SqlQuery(ChainTable.Name)
                        .Select(ChainTable.Columns.Folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.Folder))
                                .ConvertAll(
                                    x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));
            }

            foreach (var folder in
                from folderId in folderIds
                let unread = unreadCount.Find(c => c.Key == folderId).Value
                let total = totalCount.Find(c => c.Key == folderId).Value
                select new MailFolderInfo
                    {
                        id = folderId,
                        unread = unread,
                        total = total,
                        timeModified = DateTime.UtcNow
                    })
            {
                folders.Add(folder);

                var upsertQuery =
                    string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5})" +
                                  "VALUES(@tid, @user, @folder, @count1, @count2)" +
                                  "ON DUPLICATE KEY UPDATE {4} = @count1, {5} = @count2",
                                  FolderTable.Name,
                                  FolderTable.Columns.Tenant,
                                  FolderTable.Columns.User,
                                  FolderTable.Columns.Folder,
                                  isConversation
                                      ? FolderTable.Columns.UnreadConversationsCount
                                      : FolderTable.Columns.UnreadMessagesCount,
                                  isConversation
                                      ? FolderTable.Columns.TotalConversationsCount
                                      : FolderTable.Columns.TotalMessagesCount);

                db.ExecuteNonQuery(upsertQuery, new { tid = tenant, user, folder = folder.id, count1 = folder.unread, count2 = folder.total });
            }
        }

        #endregion
    }
}
