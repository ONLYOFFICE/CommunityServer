/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        // ReSharper disable InconsistentNaming
        public const string MAIL_FOLDER = "mail_folder";
        // ReSharper restore InconsistentNaming

        public struct FolderFields
        {
            public static string id_user = "id_user";
            public static string id_tenant = "tenant";
            public static string folder = "folder";
            public static string time_modified = "time_modified";
            public static string unread_messages_count = "unread_messages_count";
            public static string total_messages_count = "total_messages_count";
            public static string unread_conversations_count = "unread_conversations_count";
            public static string total_conversations_count = "total_conversations_count";
        };

        #endregion

        #region structures defines

        public struct MailFolderInfo
        {
            public int id;
            public int unread;
            public DateTime time_modified;
            public int total_count;
        }

        #endregion

        #region public methods

        public List<MailFolderInfo> GetFoldersList(int tenant, string user, bool is_conversation)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(MAIL_FOLDER)
                    .Select(FolderFields.folder,
                            FolderFields.time_modified,
                            is_conversation
                                ? FolderFields.unread_conversations_count
                                : FolderFields.unread_messages_count,
                            is_conversation ? FolderFields.total_conversations_count : FolderFields.total_messages_count)
                    .Where(GetUserWhere(user, tenant));

                // Try catch needed for resolve issue with folder's counter overflow.
                try
                {
                    var query_res = db.ExecuteList(query)
                                      .ConvertAll(x => new MailFolderInfo
                                          {
                                              id = Convert.ToInt32(x[0]),
                                              time_modified = Convert.ToDateTime(x[1]),
                                              unread = Convert.ToInt32(x[2]),
                                              total_count = Convert.ToInt32(x[3])
                                          });
                    
                    const int total_folder_count = 5;
                    if (total_folder_count == query_res.Count)
                        return query_res;
                }
                catch
                {
                }

                RecalculateFolders(db, tenant, user, !is_conversation);

                return RecalculateFolders(db, tenant, user, is_conversation);
            }
        }

        public DateTime GetMessagesModifyDate(int tenant, string user)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(MAIL_FOLDER)
                    .SelectMax(FolderFields.time_modified)
                    .Where(GetUserWhere(user, tenant));

                var date_string = db.ExecuteScalar<string>(query);

                DateTime date_time;
                return DateTime.TryParse(date_string, out date_time) ? date_time.ToUniversalTime() : DateTime.MinValue;
            }
        }

        public DateTime GetFolderModifyDate(int tenant, string user, int folder)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(MAIL_FOLDER)
                    .Select(FolderFields.time_modified)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderFields.folder, folder);

                var date_string = db.ExecuteScalar<string>(query);

                DateTime date_time;
                return DateTime.TryParse(date_string, out date_time) ? date_time.ToUniversalTime() : DateTime.MinValue;
            }
        }

        #endregion

        #region private methods

        private void ChangeFolderCounters(IDbManager db, int tenant, string user, int folder, int unread_diff,
                                          int total_diff, bool is_conversation)
        {
            var res = 0;
            if (unread_diff != 0 || total_diff != 0)
            {
                var update_query = new SqlUpdate(MAIL_FOLDER)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderFields.folder, folder);

                if (unread_diff != 0)
                {
                    if (is_conversation)
                        update_query.Set(FolderFields.unread_conversations_count + "=" +
                                         FolderFields.unread_conversations_count + "+(" + unread_diff + ")");
                    else
                        update_query.Set(FolderFields.unread_messages_count + "=" + FolderFields.unread_messages_count +
                                         "+(" + unread_diff + ")");
                }

                if (total_diff != 0)
                {
                    if (is_conversation)
                        update_query.Set(FolderFields.total_conversations_count + "=" +
                                         FolderFields.total_conversations_count + "+(" + total_diff + ")");
                    else
                    {
                        update_query.Set(FolderFields.total_messages_count + "=" +
                                         FolderFields.total_messages_count + "+(" + total_diff + ")");
                    }
                }

                res = db.ExecuteNonQuery(update_query);
            }

            if (0 == res)
                RecalculateFolders(db, tenant, user);
        }

        private void ChangeFolderCounters(
            IDbManager db,
            int tenant,
            string user,
            int folder,
            int unread_mess_diff,
            int total_mess_diff,
            int unread_conv_diff,
            int total_conv_diff)
        {
            if (0 == unread_mess_diff && 0 == total_mess_diff && 0 == unread_conv_diff && 0 == total_conv_diff)
                return;

            var update_query = new SqlUpdate(MAIL_FOLDER)
                    .Where(GetUserWhere(user, tenant))
                    .Where(FolderFields.folder, folder);

            if (0 != unread_mess_diff)
                update_query.Set(FolderFields.unread_messages_count + "=" +
                                         FolderFields.unread_messages_count + "+(" + unread_mess_diff + ")");

            if (0 != total_mess_diff)
                update_query.Set(FolderFields.total_messages_count + "=" +
                                         FolderFields.total_messages_count + "+(" + total_mess_diff + ")");

            if (0 != unread_conv_diff)
                update_query.Set(FolderFields.unread_conversations_count + "=" +
                                         FolderFields.unread_conversations_count + "+(" + unread_conv_diff + ")");

            if (0 != total_conv_diff)
                update_query.Set(FolderFields.total_conversations_count + "=" +
                                         FolderFields.total_conversations_count + "+(" + total_conv_diff + ")");


            if (0 == db.ExecuteNonQuery(update_query))
                RecalculateFolders(db, tenant, user);
        }

        private void RecalculateFolders(IDbManager db, int tenant, string user)
        {
            var folder_ids = new[]
                {
                    MailFolder.Ids.inbox,
                    MailFolder.Ids.sent,
                    MailFolder.Ids.drafts,
                    MailFolder.Ids.trash,
                    MailFolder.Ids.spam
                };

            var unread_messages_count_by_folder =
                db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.unread, true)
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(
                          Convert.ToInt32(x[0]),
                          Convert.ToInt32(x[1])));

            var total_messages_count_by_folder =
                db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                  .ConvertAll(
                      x =>
                      new KeyValuePair<int, int>(Convert.ToInt32(x[0]),
                                                 Convert.ToInt32(x[1])));

            var unread_conversations_count_by_folder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.unread, true)
                        .GroupBy(ChainTable.Columns.folder))
                  .ConvertAll(
                      x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

            var total_conversations_count_by_folder =
                db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.folder))
                  .ConvertAll(
                      x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));


            var folders_info = from folder_id in folder_ids
                               let unread_mess_count = unread_messages_count_by_folder.Find(c => c.Key == folder_id).Value
                               let total_mess_count = total_messages_count_by_folder.Find(c => c.Key == folder_id).Value
                               let unread_conv_count = unread_conversations_count_by_folder.Find(c => c.Key == folder_id).Value
                               let total_conv_count = total_conversations_count_by_folder.Find(c => c.Key == folder_id).Value
                               select new 
                                   {
                                       id = folder_id,
                                       unread_messages_count = unread_mess_count,
                                       total_messages_count = total_mess_count,
                                       unread_conversations_count = unread_conv_count,
                                       total_conversations_count = total_conv_count,
                                       time_modified = DateTime.Now
                                   };

            foreach (var info in folders_info)
            {
                db.ExecuteNonQuery(
                    new SqlInsert(MAIL_FOLDER, true)
                        .InColumnValue(FolderFields.id_tenant, tenant)
                        .InColumnValue(FolderFields.id_user, user)
                        .InColumnValue(FolderFields.folder, info.id)
                        .InColumnValue(FolderFields.unread_messages_count, info.unread_messages_count)
                        .InColumnValue(FolderFields.unread_conversations_count, info.unread_conversations_count)
                        .InColumnValue(FolderFields.total_messages_count, info.total_messages_count)
                        .InColumnValue(FolderFields.total_conversations_count, info.total_conversations_count));
            }
        }

        private List<MailFolderInfo> RecalculateFolders(IDbManager db, int tenant, string user, bool is_conversation)
        {
            List<KeyValuePair<int, int>> unread_count;
            List<KeyValuePair<int, int>> total_count;
            var folders = new List<MailFolderInfo>();
            var folder_ids = new[]
                {
                    MailFolder.Ids.inbox,
                    MailFolder.Ids.sent,
                    MailFolder.Ids.drafts,
                    MailFolder.Ids.trash,
                    MailFolder.Ids.spam
                };

            if (!is_conversation)
            {
                unread_count = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.unread, true)
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                total_count = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, false)
                        .GroupBy(MailTable.Columns.folder))
                                .ConvertAll(
                                    x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));
            }
            else
            {
                unread_count = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.unread, true)
                        .GroupBy(ChainTable.Columns.folder))
                                 .ConvertAll(
                                     x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));

                total_count = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.folder, "count(1)")
                        .Where(GetUserWhere(user, tenant))
                        .GroupBy(ChainTable.Columns.folder))
                                .ConvertAll(
                                    x => new KeyValuePair<int, int>(Convert.ToInt32(x[0]), Convert.ToInt32(x[1])));
            }

            foreach (var folder in
                from folder_id in folder_ids
                let unread = unread_count.Find(c => c.Key == folder_id).Value
                let total = total_count.Find(c => c.Key == folder_id).Value
                select new MailFolderInfo
                    {
                        id = folder_id,
                        unread = unread,
                        total_count = total,
                        time_modified = DateTime.Now
                    })
            {
                folders.Add(folder);

                var upsert_query =
                    string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5})" +
                                  "VALUES({6}, '{7}', {8}, {9}, {10})" +
                                  "ON DUPLICATE KEY UPDATE {4} = {9}, {5} = {10}",
                                  MAIL_FOLDER,
                                  FolderFields.id_tenant,
                                  FolderFields.id_user,
                                  FolderFields.folder,
                                  is_conversation
                                      ? FolderFields.unread_conversations_count
                                      : FolderFields.unread_messages_count,
                                  is_conversation
                                      ? FolderFields.total_conversations_count
                                      : FolderFields.total_messages_count,
                                  tenant,
                                  user,
                                  folder.id,
                                  folder.unread,
                                  folder.total_count);

                db.ExecuteNonQuery(upsert_query);
            }

            return folders;
        }

        #endregion
    }
}
