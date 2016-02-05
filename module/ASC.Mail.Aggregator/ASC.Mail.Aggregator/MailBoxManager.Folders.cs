/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region structures defines

        public class MailFolderInfo
        {
            public int id;
            public int unread;
            public DateTime time_modified;
            public int total_count;
        }

        #endregion

        #region public methods

        public List<MailFolderInfo> GetFolders(int tenant, string user, bool isConversation)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(FolderTable.name)
                    .Select(FolderTable.Columns.folder,
                            FolderTable.Columns.time_modified,
                            isConversation
                                ? FolderTable.Columns.unread_conversations_count
                                : FolderTable.Columns.unread_messages_count,
                            isConversation ? FolderTable.Columns.total_conversations_count : FolderTable.Columns.total_messages_count)
                    .Where(GetUserWhere(user, tenant));

                // Try catch needed for resolve issue with folder's counter overflow.
                try
                {
                    var queryRes = db.ExecuteList(query)
                                      .ConvertAll(x => new MailFolderInfo
                                          {
                                              id = Convert.ToInt32(x[0]),
                                              time_modified = Convert.ToDateTime(x[1]),
                                              unread = Convert.ToInt32(x[2]),
                                              total_count = Convert.ToInt32(x[3])
                                          });

                    if (queryRes.Any())
                        return queryRes;
                }
                catch(Exception ex)
                {
                    _log.Error("GetFoldersList( \r\nException:{0}\r\n", ex.ToString());
                }

                RecalculateFolders(db, tenant, user, !isConversation);

                return RecalculateFolders(db, tenant, user, isConversation);
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
                var updateQuery = new SqlUpdate(FolderTable.name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderTable.Columns.folder, folder);

                if (unreadDiff != 0)
                {
                    if (isConversation)
                        updateQuery.Set(FolderTable.Columns.unread_conversations_count + "=" +
                                         FolderTable.Columns.unread_conversations_count + "+(" + unreadDiff + ")");
                    else
                        updateQuery.Set(FolderTable.Columns.unread_messages_count + "=" + FolderTable.Columns.unread_messages_count +
                                         "+(" + unreadDiff + ")");
                }

                if (totalDiff != 0)
                {
                    if (isConversation)
                        updateQuery.Set(FolderTable.Columns.total_conversations_count + "=" +
                                         FolderTable.Columns.total_conversations_count + "+(" + totalDiff + ")");
                    else
                    {
                        updateQuery.Set(FolderTable.Columns.total_messages_count + "=" +
                                         FolderTable.Columns.total_messages_count + "+(" + totalDiff + ")");
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

            var updateQuery = new SqlUpdate(FolderTable.name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderTable.Columns.folder, folder);

            if (0 != unreadMessDiff)
                updateQuery.Set(FolderTable.Columns.unread_messages_count + "=" +
                                         FolderTable.Columns.unread_messages_count + "+(" + unreadMessDiff + ")");

            if (0 != totalMessDiff)
                updateQuery.Set(FolderTable.Columns.total_messages_count + "=" +
                                         FolderTable.Columns.total_messages_count + "+(" + totalMessDiff + ")");

            if (0 != unreadConvDiff)
                updateQuery.Set(FolderTable.Columns.unread_conversations_count + "=" +
                                         FolderTable.Columns.unread_conversations_count + "+(" + unreadConvDiff + ")");

            if (0 != totalConvDiff)
                updateQuery.Set(FolderTable.Columns.total_conversations_count + "=" +
                                         FolderTable.Columns.total_conversations_count + "+(" + totalConvDiff + ")");


            if (0 == db.ExecuteNonQuery(updateQuery))
                RecalculateFolders(db, tenant, user);
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
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.unread, true)
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(
                          Convert.ToInt32(x[0]),
                          Convert.ToInt32(x[1])));

            var totalMessagesCountByFolder =
                db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(Convert.ToInt32(x[0]),
                                                 Convert.ToInt32(x[1])));

            var unreadConversationsCountByFolder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.unread, true)
                        .GroupBy(ChainTable.Columns.folder))
                  .ConvertAll(
                      x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

            var totalConversationsCountByFolder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.folder))
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
                    new SqlInsert(FolderTable.name, true)
                        .InColumnValue(FolderTable.Columns.id_tenant, tenant)
                        .InColumnValue(FolderTable.Columns.id_user, user)
                        .InColumnValue(FolderTable.Columns.folder, info.id)
                        .InColumnValue(FolderTable.Columns.unread_messages_count, info.unread_messages_count)
                        .InColumnValue(FolderTable.Columns.unread_conversations_count, info.unread_conversations_count)
                        .InColumnValue(FolderTable.Columns.total_messages_count, info.total_messages_count)
                        .InColumnValue(FolderTable.Columns.total_conversations_count, info.total_conversations_count));
            }
        }

        private List<MailFolderInfo> RecalculateFolders(IDbManager db, int tenant, string user, bool isConversation)
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
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.unread, true)
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                totalCount = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                                .ConvertAll(
                                    x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));
            }
            else
            {
                unreadCount = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.unread, true)
                        .GroupBy(ChainTable.Columns.folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                totalCount = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(*)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.folder))
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
                        total_count = total,
                        time_modified = DateTime.UtcNow
                    })
            {
                folders.Add(folder);

                var upsertQuery =
                    string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5})" +
                                  "VALUES(@tid, @user, @folder, @count1, @count2)" +
                                  "ON DUPLICATE KEY UPDATE {4} = @count1, {5} = @count2",
                                  FolderTable.name,
                                  FolderTable.Columns.id_tenant,
                                  FolderTable.Columns.id_user,
                                  FolderTable.Columns.folder,
                                  isConversation
                                      ? FolderTable.Columns.unread_conversations_count
                                      : FolderTable.Columns.unread_messages_count,
                                  isConversation
                                      ? FolderTable.Columns.total_conversations_count
                                      : FolderTable.Columns.total_messages_count);

                db.ExecuteNonQuery(upsertQuery, new { tid = tenant, user, folder = folder.id, count1 = folder.unread, count2 = folder.total_count });
            }

            return folders;
        }

        #endregion
    }
}
