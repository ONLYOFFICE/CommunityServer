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

using System.Web;
using ASC.Data.Storage;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;
using ActiveUp.Net.Mail;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Mail.Aggregator.Extension;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines
        public const int time_check_minutes = 15;
        #endregion

        #region public methods

        public string CreateNewStreamId()
        {
            var stream_id = Guid.NewGuid().ToString("N").ToLower();
            return stream_id;
        }

        public void SetMessagesFolder(int id_tenant, string id_user, int to_folder, List<int> ids)
        {
            if (!MailFolder.IsIdOk(to_folder))
                throw new ArgumentException("can't set folder to none system folder");

            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var list_objects = GetMessagesInfo(db, id_tenant, id_user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder, MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                    });

                if (list_objects.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        SetMessagesFolder(db, id_tenant, id_user, list_objects, to_folder);
                        tx.Commit();
                    }
                }
            }
        }

        public void SetMessagesFolder(int id_tenant, string id_user, int to_folder, MailFilter filter)
        {
            if (!MailFolder.IsIdOk(to_folder))
                throw new ArgumentException("can't set folder to none system folder");

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesFolder(db, id_tenant, id_user, to_folder, filter);
                    tx.Commit();
                }
            }
        }

        public void RestoreMessages(int id_tenant, string id_user, MailFilter filter)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    RestoreMessages(db, id_tenant, id_user, filter);
                    tx.Commit();
                }
            }
        }

        public void RestoreMessages(int id_tenant, string id_user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var mails_info = GetMessagesInfo(db, id_tenant, id_user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder,
                        MailTable.Columns.folder_restore, MailTable.Columns.chain_id,
                        MailTable.Columns.id_mailbox
                    });

                if (mails_info.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        RestoreMessages(db, id_tenant, id_user, mails_info);
                        tx.Commit();
                    }
                }

            }
        }

        public void DeleteMessages(int id_tenant, string id_user, MailFilter filter)
        {
            long used_quota = 0;
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    used_quota = DeleteMessages(db, id_tenant, id_user, filter);
                    tx.Commit();
                }
            }

            QuotaUsedDelete(id_tenant, used_quota);
        }

        private long DeleteMessages(DbManager db, int id_tenant, string id_user, MailFilter filter)
        {
            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.is_removed, true)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(MailTable.Columns.is_removed, false)
                    .ApplyFilter(filter));

            var filtered_messages =
                new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(MailTable.Columns.is_removed, false)
                    .ApplyFilter(filter);

            var used_quota = db.ExecuteScalar<long>(
                new SqlQuery(AttachmentTable.name + " AS a")
                    .InnerJoin(filtered_messages, "m",
                                   Exp.Sql(string.Format("a.{0} = m.{1}", AttachmentTable.Columns.id_mail, MailTable.Columns.id)))
                    .SelectSum("a." + AttachmentTable.Columns.size)
                    .Where("a." + AttachmentTable.Columns.need_remove, false));

            if (used_quota > 0)
            {
                var delete_attachments_query =
                    string.Format("UPDATE {0} AS a " +
                                  "INNER JOIN (" +
                                  filtered_messages.GetSqlWithParameters() +
                                  ") AS m ON " +
                                  "a.{1} = m.{2} " +
                                  "SET a.{3} = {4} " +
                                  "WHERE a.{3} = 0",
                                  AttachmentTable.name,
                                  AttachmentTable.Columns.id_mail, MailTable.Columns.id,
                                  AttachmentTable.Columns.need_remove, true);

                db.ExecuteNonQuery(delete_attachments_query);
            }

            var affected_tags =
                db.ExecuteList(
                    new SqlQuery(MAIL_TAG_MAIL + " AS tm")
                        .InnerJoin(filtered_messages, "m",
                                   Exp.Sql(string.Format("tm.{0} = m.{1}", TagMailFields.id_mail, MailTable.Columns.id)))
                        .Select("tm." + TagMailFields.id_tag))
                  .ConvertAll(r => Convert.ToInt32(r[0]));

            db.ExecuteNonQuery(
                new SqlDelete(MAIL_TAG_MAIL)
                    .Where(Exp.In(TagMailFields.id_mail, filtered_messages)));

            UpdateTagsCount(db, id_tenant, id_user, affected_tags.Distinct());

            RecalculateFolders(db, id_tenant, id_user, false);

            return used_quota;
        }

        public void DeleteMessages(int id_tenant, string id_user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");
            long used_quota = 0;
            using (var db = GetDb())
            {
                var delete_messages_info = GetMessagesInfo(db, id_tenant, id_user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.folder, MailTable.Columns.unread
                    });

                if (delete_messages_info.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        used_quota = DeleteMessages(db, id_tenant, id_user, delete_messages_info, false);
                        tx.Commit();
                    }
                }
            }

            QuotaUsedDelete(id_tenant, used_quota);
        }

        public void SetMessagesReadFlags(int tenant, string user, List<int> ids, bool is_read)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var messages_info = GetMessagesInfo(db, tenant, user, ids, MessageInfoToSetUnread.Fields)
                    .ConvertAll(x => new MessageInfoToSetUnread(x));

                if (!messages_info.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, tenant, user, messages_info, is_read);
                    tx.Commit();
                }
            }
        }


        private struct MessageInfoToSetUnread
        {
            public static readonly string[] Fields =
                {
                    MailTable.Columns.id,
                    MailTable.Columns.folder,
                    MailTable.Columns.unread,
                    MailTable.Columns.chain_id,
                    MailTable.Columns.id_mailbox
                };

            private readonly int _id;
            private readonly int _folder;
            private readonly bool _unread;
            private readonly string _chainId;
            private readonly int _mailbox;

            public int Id{
                get { return _id; }
            }

            public int Folder
            {
                get { return _folder; }
            }

            public bool Unread
            {
                get { return _unread; }
            }

            public string ChainId
            {
                get { return _chainId; }
            }

            public int Mailbox
            {
                get { return _mailbox; }
            }

            public MessageInfoToSetUnread(IList<object> fields_values)
            {
                _id = Convert.ToInt32(fields_values[0]);
                _folder = Convert.ToInt32(fields_values[1]);
                _unread = Convert.ToBoolean(fields_values[2]);
                _chainId = (string) fields_values[3];
                _mailbox = Convert.ToInt32(fields_values[4]);
            }
        }

        class MessageInfoToSetUnreadEqualityComparer : IEqualityComparer<MessageInfoToSetUnread>
        {
            public bool Equals(MessageInfoToSetUnread m1, MessageInfoToSetUnread m2)
            {
                return m1.ChainId == m2.ChainId && m1.Mailbox == m2.Mailbox && m1.Folder == m2.Folder;
            }

            public int GetHashCode(MessageInfoToSetUnread m)
            {
                return (m.ChainId + m.Mailbox + m.Folder).GetHashCode();
            }

        }

        /// <summary>
        ///    Set messages read/unread flag and update chains state & folders counters
        /// </summary>
        /// <param name="db">db manager instance</param>
        /// <param name="tenant">Tenant Id</param>
        /// <param name="user">User Id</param>
        /// <param name="messages_info">Info about messages to be update</param>
        /// <param name="is_read">New state to be set</param>
        /// <returns>List ob objects array</returns>
        /// <short>Get chains messages info</short>
        /// <category>Mail</category>
        private void SetMessagesReadFlags(IDbManager db, int tenant, string user, IEnumerable<MessageInfoToSetUnread> messages_info, bool is_read)
        {
            var changing_messages = messages_info.Where(x => x.Unread == is_read);
            var message_info_to_set_unreads = changing_messages as IList<MessageInfoToSetUnread> ?? changing_messages.ToList();
            var ids = message_info_to_set_unreads.Select(x => (object)x.Id).ToArray();

            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Where(Exp.In(MailTable.Columns.id, ids))
                    .Where(GetUserWhere(user, tenant))
                    .Set(MailTable.Columns.unread, !is_read));

            var folders_mess_counter_diff = new Dictionary<int, int>();

            foreach (var mess in message_info_to_set_unreads)
            {
                if (folders_mess_counter_diff.Keys.Contains(mess.Folder))
                    folders_mess_counter_diff[mess.Folder] += 1;
                else
                {
                    folders_mess_counter_diff[mess.Folder] = 1;
                }
            }

            var distinct = message_info_to_set_unreads.Distinct(new MessageInfoToSetUnreadEqualityComparer()).ToList();

            foreach (var folder in folders_mess_counter_diff.Keys)
            {
                var sign = is_read ? -1 : 1;
                var mess_diff = sign*folders_mess_counter_diff[folder];
                var conv_diff = sign*distinct.Where(x => x.Folder == folder).Count();

                ChangeFolderCounters(db, tenant, user, folder, mess_diff, 0, conv_diff, 0);
            }

            foreach (var message_id in distinct.Select(x => x.Id))
                UpdateMessageChainUnreadFlag(db, tenant, user, Convert.ToInt32(message_id));
        }

        public void SetMessagesReadFlags(int tenant, string user, MailFilter filter, bool is_read)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, tenant, user, filter, is_read);
                    tx.Commit();
                }
            }
        }

        public void SetMessagesReadFlags(IDbManager db, int tenant, string user, MailFilter filter, bool is_read)
        {
            var condition_by_filter = ASC.Mail.Aggregator.Extension.SqlQueryExtensions.GetMailFilterConditions(filter, true, "");

            if (filter.PrimaryFolder == MailFolder.Ids.inbox || filter.PrimaryFolder == MailFolder.Ids.sent)
                condition_by_filter &= Exp.In(MailTable.Columns.folder, new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
            else
                condition_by_filter &= Exp.Eq(MailTable.Columns.folder, filter.PrimaryFolder);
            
            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.unread, !is_read)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(condition_by_filter));

            RecalculateFolders(db, tenant, user, false);
        }

        public bool SetMessagesImportanceFlags(int id_tenant, string id_user, bool importance, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var affected = db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(Exp.In(MailTable.Columns.id, ids.ToArray()))
                        .Where(GetUserWhere(id_user, id_tenant))
                        .Set(MailTable.Columns.importance, importance));

                foreach (var message_id in ids)
                    UpdateMessageChainImportanceFlag(db, id_tenant, id_user, message_id);

                return affected > 0;
            }
        }

        public void UpdateChainFields(DbManager db, int id_tenant, string id_user, List<int> ids)
        {
            // Get additional information about mails
            var mail_info = GetMessagesInfo(db, id_tenant, id_user, ids, new[]
                {
                    MailTable.Columns.id_mailbox, MailTable.Columns.chain_id, MailTable.Columns.folder
                })
                .ConvertAll(x => new
                    {
                        id_mailbox = Convert.ToInt32(x[0]),
                        chain_id = (string) x[1],
                        folder = Convert.ToInt32(x[2])
                    });

            foreach (var info in mail_info.GroupBy(t => new { t.chain_id, t.folder, t.id_mailbox }))
            {
                UpdateChain(db, info.Key.chain_id, info.Key.folder, info.Key.id_mailbox, id_tenant, id_user);
            }
        }

        public void UpdateCrmMessages(int tenant, IEnumerable<string> emails, IEnumerable<string> users)
        {
            var emails_array = emails.Distinct().ToArray();
            var users_array = users.Select(x => (object) x).ToArray();

            if (emails_array.Length == 0) return;

            var query = new SqlUpdate(MailTable.name)
                .Where(MailTable.Columns.id_tenant, tenant)
                .Where(MailTable.Columns.folder, 1)
                .Where(MailTable.Columns.is_removed, false)
                .Where(MailTable.Columns.is_from_crm, false);

            if (users_array.Length != 0) query.Where(Exp.In(MailTable.Columns.id_user, users_array));

            var exp = Exp.Like(MailTable.Columns.from, emails_array[0], SqlLike.AnyWhere);
            for (var i = 1; i < emails_array.Length; i++)
            {
                exp = Exp.Or(exp, Exp.Like(MailTable.Columns.from, emails_array[i], SqlLike.AnyWhere));
            }

            using (var db = GetDb())
            {
                if (FullTextSearch.SupportModule(FullTextSearch.MailFromTextModule))
                {
                    var ids = new List<string>();
                    ids = emails_array.Aggregate(ids, (current, t) =>
                                                      current.Concat(
                                                          FullTextSearch.Search(t, FullTextSearch.MailFromTextModule)
                                                                        .GetIdentifiers()).Distinct().ToList());

                    var last_ids = db.ExecuteList(
                            new SqlQuery(MailTable.name)
                              .Select(MailTable.Columns.id)
                              .Where(MailTable.Columns.id_tenant, tenant)
                              .Where(MailTable.Columns.folder, 1)
                              .Where(MailTable.Columns.is_removed, false)
                              .Where(MailTable.Columns.is_from_crm, false)
                              .Where(Exp.Gt(MailTable.Columns.chain_date,
                                            DateTime.Now.AddMinutes((-1)*time_check_minutes)
                                                    .ToUniversalTime()))
                              .Where(exp)).ConvertAll(x => (string) x[0]);

                    ids = ids.Concat(last_ids).Distinct().ToList();

                    query.Where(Exp.In(MailTable.Columns.id, ids));
                }
                else
                {
                    query.Where(exp);
                }

                query.Set(MailTable.Columns.is_from_crm, true);
                db.ExecuteNonQuery(query);
            }
        }

        public void DeleteFoldersMessages(int id_tenant, string id_user, int folder)
        {
            long used_quota = 0;

            using (var db = GetDb())
            {
                // Get message ids stored in the  folder.
                var ids = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.id)
                        .Where(MailTable.Columns.is_removed, false)
                        .Where(MailTable.Columns.folder, folder)
                        .Where(GetUserWhere(id_user, id_tenant)))
                    .Select(x => Convert.ToInt32(x[0])).ToArray();

                if (ids.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        // Mark messages stored in the folder as is_removed.
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.name)
                                .Set(MailTable.Columns.is_removed, true)
                                .Where(GetUserWhere(id_user, id_tenant))
                                .Where(MailTable.Columns.folder, folder));

                        // Recalculate used quota
                        used_quota = MarkAttachmetsNeedRemove(db, id_tenant, Exp.In(AttachmentTable.Columns.id_mail, ids));

                        // delete tag/message cross references
                        db.ExecuteNonQuery(
                            new SqlDelete(MAIL_TAG_MAIL)
                                .Where(Exp.In(TagMailFields.id_mail, ids))
                                .Where(GetUserWhere(id_user, id_tenant)));

                        // update tags counters
                        var tags = GetMailTags(db, id_tenant, id_user, Exp.Empty);
                        UpdateTagsCount(db, id_tenant, id_user, tags.Select(x => x.Id));

                        // delete chains stored in the folder
                        db.ExecuteNonQuery(new SqlDelete(ChainTable.name)
                                               .Where(GetUserWhere(id_user, id_tenant))
                                               .Where(ChainTable.Columns.folder, folder));

                        // reset folder counters
                        db.ExecuteNonQuery(new SqlUpdate(MAIL_FOLDER)
                            .Where(GetUserWhere(id_user, id_tenant))
                            .Where(FolderFields.folder, folder)
                            .Set(FolderFields.total_conversations_count, 0)
                            .Set(FolderFields.total_messages_count, 0)
                            .Set(FolderFields.unread_conversations_count, 0)
                            .Set(FolderFields.unread_messages_count, 0));

                        tx.Commit();
                    }
                }
            }

            if (used_quota > 0)
                QuotaUsedDelete(id_tenant, used_quota);
        }

        public void GetStoredMessagesUIDL_MD5(int mailbox_id, Dictionary<int, string> uidl_list,
                                              Dictionary<int, string> md5_list)
        {
            using (var db = GetDb())
            {
                db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.id, MailTable.Columns.uidl, MailTable.Columns.md5)
                        .Where(MailTable.Columns.id_mailbox, mailbox_id))
                  .ForEach(r =>
                      {
                          uidl_list.Add(Convert.ToInt32(r[0]), (string) r[1]);
                          md5_list.Add(Convert.ToInt32(r[0]), (string) r[2]);
                      });
            }
        }

        public int MailSave(MailBox mail_box, MailMessageItem mail, int mail_id, int folder, int folder_restore, 
            string uidl, string md5, bool save_attachments)
        {
            var id_mail = 0;
            const int max_attempts = 2;
            var count_attachments = 0;
            long used_quota = 0;
            var address = GetAddress(mail_box.EMail);

            using (var db = GetDb())
            {
                var i_attempt = 0;
                do
                {
                    try
                    {
                        using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            if (mail_id != 0)
                                count_attachments = GetCountAttachments(db, mail_id);

                            #region SQL query construction

                            if (mail_id == 0)
                            {
                                // This case is for first time saved draft and received message.
                                var insert = new SqlInsert(MailTable.name, true)
                                    .InColumnValue(MailTable.Columns.id, mail_id)
                                    .InColumnValue(MailTable.Columns.id_mailbox, mail_box.MailBoxId)
                                    .InColumnValue(MailTable.Columns.id_tenant, mail_box.TenantId)
                                    .InColumnValue(MailTable.Columns.id_user, mail_box.UserId)
                                    .InColumnValue(MailTable.Columns.address, address)
                                    .InColumnValue(MailTable.Columns.uidl, !string.IsNullOrEmpty(uidl) ? uidl : null)
                                    .InColumnValue(MailTable.Columns.md5, !string.IsNullOrEmpty(md5) ? md5 : null)
                                    .InColumnValue(MailTable.Columns.from, mail.From)
                                    .InColumnValue(MailTable.Columns.to, mail.To)
                                    .InColumnValue(MailTable.Columns.reply, mail.ReplyTo)
                                    .InColumnValue(MailTable.Columns.subject, mail.Subject)
                                    .InColumnValue(MailTable.Columns.cc, mail.Cc)
                                    .InColumnValue(MailTable.Columns.bcc, mail.Bcc)
                                    .InColumnValue(MailTable.Columns.importance, mail.Important)
                                    .InColumnValue(MailTable.Columns.date_received, DateTime.UtcNow)
                                    .InColumnValue(MailTable.Columns.date_sent, mail.Date.ToUniversalTime())
                                    .InColumnValue(MailTable.Columns.size, mail.Size)
                                    .InColumnValue(MailTable.Columns.attach_count,
                                                   !save_attachments
                                                       ? count_attachments
                                                       : (mail.Attachments != null ? mail.Attachments.Count : 0))
                                    .InColumnValue(MailTable.Columns.unread, mail.IsNew)
                                    .InColumnValue(MailTable.Columns.is_answered, mail.IsAnswered)
                                    .InColumnValue(MailTable.Columns.is_forwarded, mail.IsForwarded)
                                    .InColumnValue(MailTable.Columns.stream, mail.StreamId)
                                    .InColumnValue(MailTable.Columns.folder, folder)
                                    .InColumnValue(MailTable.Columns.folder_restore, folder_restore)
                                    .InColumnValue(MailTable.Columns.is_from_crm, mail.IsFromCRM)
                                    .InColumnValue(MailTable.Columns.is_from_tl, mail.IsFromTL)
                                    .InColumnValue(MailTable.Columns.spam, 0)
                                    .InColumnValue(MailTable.Columns.mime_message_id, mail.MessageId)
                                    .InColumnValue(MailTable.Columns.mime_in_reply_to, mail.InReplyTo)
                                    .InColumnValue(MailTable.Columns.chain_id, mail.ChainId)
                                    .InColumnValue(MailTable.Columns.introduction, mail.Introduction)
                                    .InColumnValue(MailTable.Columns.chain_date, mail.Date.ToUniversalTime())
                                    .InColumnValue(MailTable.Columns.is_text_body_only, mail.TextBodyOnly)
                                    .Identity(0, 0, true);

                                if (mail.HasParseError)
                                    insert.InColumnValue(MailTable.Columns.has_parse_error, mail.HasParseError);

                                id_mail = db.ExecuteScalar<int>(insert);

                                ChangeFolderCounters(db, mail_box.TenantId, mail_box.UserId, folder, 
                                    mail.IsNew ? 1 : 0, 1, false);
                            }
                            else
                            {
                                // This case is for already saved draft only.
                                var update = new SqlUpdate(MailTable.name)
                                    .Where(MailTable.Columns.id, mail_id)
                                    .Set(MailTable.Columns.id_mailbox, mail_box.MailBoxId)
                                    .Set(MailTable.Columns.from, mail.From)
                                    .Set(MailTable.Columns.to, mail.To)
                                    .Set(MailTable.Columns.reply, mail.ReplyTo)
                                    .Set(MailTable.Columns.subject, mail.Subject)
                                    .Set(MailTable.Columns.cc, mail.Cc)
                                    .Set(MailTable.Columns.bcc, mail.Bcc)
                                    .Set(MailTable.Columns.importance, mail.Important)
                                    .Set(MailTable.Columns.date_received, DateTime.UtcNow)
                                    .Set(MailTable.Columns.date_sent, mail.Date.ToUniversalTime())
                                    .Set(MailTable.Columns.size, mail.Size)
                                    .Set(MailTable.Columns.attach_count,
                                         !save_attachments
                                             ? count_attachments
                                             : (mail.Attachments != null ? mail.Attachments.Count : 0))
                                    .Set(MailTable.Columns.unread, mail.IsNew)
                                    .Set(MailTable.Columns.is_answered, mail.IsAnswered)
                                    .Set(MailTable.Columns.is_forwarded, mail.IsForwarded)
                                    .Set(MailTable.Columns.stream, mail.StreamId)
                                    .Set(MailTable.Columns.folder, folder)
                                    .Set(MailTable.Columns.folder_restore, folder_restore)
                                    .Set(MailTable.Columns.is_from_crm, mail.IsFromCRM)
                                    .Set(MailTable.Columns.is_from_tl, mail.IsFromTL)
                                    .Set(MailTable.Columns.is_text_body_only, mail.TextBodyOnly)
                                    .Set(MailTable.Columns.spam, 0);

                                if (!string.IsNullOrEmpty(mail.MessageId))
                                    update.Set(MailTable.Columns.mime_message_id, mail.MessageId);
                                if (!string.IsNullOrEmpty(mail.InReplyTo))
                                    update.Set(MailTable.Columns.mime_in_reply_to, mail.InReplyTo);
                                if (!string.IsNullOrEmpty(mail.ChainId)) update.Set(MailTable.Columns.chain_id, mail.ChainId);

                                db.ExecuteNonQuery(update);
                                id_mail = mail_id;
                            }

                            #endregion

                            if (save_attachments &&
                                mail.Attachments != null &&
                                mail.Attachments.Count > 0)
                            {
                                used_quota = MarkAttachmetsNeedRemove(db, mail_box.TenantId,
                                                                          Exp.Eq(AttachmentTable.Columns.id_mail, id_mail));
                                SaveAttachments(db, mail_box.TenantId, id_mail, mail.Attachments);
                            }

                            if (mail.FromEmail.Length != 0)
                            {
                                db.ExecuteNonQuery(
                                    new SqlDelete(MAIL_TAG_MAIL)
                                        .Where(TagMailFields.id_mail, id_mail));

                                var tag_collection = new List<int>();

                                if (mail.TagIds != null)
                                {
                                    tag_collection.AddRange(mail.TagIds);
                                }

                                var tag_addresses_tag_ids = db.ExecuteList(
                                    new SqlQuery(MAIL_TAG_ADDRESSES)
                                        .Distinct()
                                        .Select(TagAddressFields.id_tag)
                                        .Where(TagAddressFields.address, mail.FromEmail)
                                        .Where(Exp.In(TagAddressFields.id_tag,
                                                      new SqlQuery(MAIL_TAG)
                                                          .Select(TagFields.id)
                                                          .Where(GetUserWhere(mail_box.UserId, mail_box.TenantId)))))
                                                              .ConvertAll(r => Convert.ToInt32(r[0]));

                                tag_addresses_tag_ids.ForEach(tag_id =>
                                    {
                                        if (!tag_collection.Contains(tag_id))
                                            tag_collection.Add(tag_id);
                                    });

                                if (tag_collection.Any())
                                {
                                    SetMessageTags(db, mail_box.TenantId, mail_box.UserId, id_mail, tag_collection);
                                }
                            }

                            UpdateMessagesChains(db, mail_box, mail.MessageId, mail.ChainId, folder);

                            // ToDo: implement MailContactsSave as Message handler extension
                            // MailContactsSave(db, mail_box.TenantId, mail_box.UserId, mail);

                            tx.Commit();
                        }

                        i_attempt = max_attempts; // The transaction was succeded
                    }
                    catch (DbException ex)
                    {
                        if (!ex.Message.StartsWith("Deadlock found when trying to get lock; try restarting transaction"))
                            throw;

                        // Need to restart the transaction
                        i_attempt++;
                        if (i_attempt >= max_attempts)
                            throw;

                        _log.Warn("MailSave -> [!!!DEADLOCK!!!] Restarting transaction at {0}(of {1}) attempt",
                                  i_attempt, max_attempts);
                    }
                } while (i_attempt < max_attempts);
            }

            if(used_quota > 0)
                QuotaUsedDelete(mail_box.TenantId, used_quota);

            _log.Debug("MailSave() tenant='{0}', user_id='{1}', email='{2}', from='{3}', id_mail='{4}'",
                mail_box.TenantId, mail_box.UserId, mail_box.EMail, mail.From, id_mail);

            return id_mail;
        }

        public void UpdateMessageChainId(MailBox mailbox, long message_id, int folder, string old_chain_id, string new_chain_id)
        {
            using (var db = GetDb())
            {
                if (mailbox == null)
                    throw new ArgumentNullException("mailbox");

                if (message_id == 0)
                    throw new ArgumentException("message_id");

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    if (!old_chain_id.Equals(new_chain_id))
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.name)
                                .Set(MailTable.Columns.chain_id, new_chain_id)
                                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                                .Where(MailTable.Columns.id, message_id));

                        UpdateChain(db, old_chain_id, folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);
                    }

                    UpdateChain(db, new_chain_id, folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);

                    tx.Commit();
                }
            }
        }

        public int MailReceive(MailBox mail_box, Message message, int folder_id, string uidl, string md5, bool has_parse_error, 
            bool unread, int[] tags_ids, out MailMessageItem message_item)
        {
            if (mail_box == null)
                throw new ArgumentNullException("mail_box");

            if (message == null)
                throw new ArgumentNullException("message");

            _log.Debug("MailReceive() tenant='{0}', user_id='{1}', folder_id='{2}', uidl='{3}'",
                mail_box.TenantId, mail_box.UserId, folder_id, uidl);

            var skip_save_message = false;

            var mail_id = 0;

            var from_this_mail_box = message.From.Email.ToLowerInvariant().Equals(mail_box.Account.ToLowerInvariant());

            // checks for sent message existance
            // Folder double check needed for  the case.
            // Case: Mailbox added to TLMail collects messages from the another account. In that account sender may be different.
            if (MailFolder.Ids.sent == folder_id || from_this_mail_box)
            {
                var to_this_mail_box = message.To.Select(addr => addr.Email.ToLower()).Contains(mail_box.EMail.Address.ToLower());

                var messages_info = GetMessagesInfoByMimeMessageId(
                    mail_box.MailBoxId,
                    mail_box.TenantId,
                    mail_box.UserId,
                    message.MessageId,
                    new[] {MailTable.Columns.id, MailTable.Columns.folder_restore, MailTable.Columns.uidl})
                    .ConvertAll(i => new
                        {
                            id = Convert.ToInt32(i[0]),
                            folder = Convert.ToInt32(i[1]),
                            uidl = (string) i[2]
                        });

                // Check place for message saving
                // If it is sent folder or mailbox connected by POP3 protocol
                // than message will be saved to inbox first
                if (MailFolder.Ids.sent == folder_id ||
                    (!mail_box.Imap && (!to_this_mail_box || messages_info.Count > 1)))
                {
                    var message_for_update_info = messages_info.Find(i => (MailFolder.Ids.sent == i.folder) && (null == i.uidl));

                    if (null != message_for_update_info) // Found message clone in  the Sent folder
                    {
                        mail_id = message_for_update_info.id;
                        UpdateMessageUidl(mail_box.TenantId, mail_box.UserId, mail_id, uidl);
                        skip_save_message = true;
                        _log.Debug("Found outbox clone in sent folder, mailId={0}. Saveing message will skiped!\r\n",
                                   mail_id);
                    }
                }
            }

            message_item = null;

            if (!skip_save_message)
            {
                message_item = ProcessMail(message, mail_box, folder_id, has_parse_error);

                tags_ids = AddTagIdsForSelfSendedMessages(mail_box, tags_ids, message_item);

                if (null != tags_ids)
                {
                    if (null != message_item.TagIds)
                        message_item.TagIds.AddRange(tags_ids);
                    else
                        message_item.TagIds = new ItemList<int>(tags_ids);
                }


                try
                {
                    _log.Debug("StoreAttachments(Account:{0})", mail_box.EMail);
                    message_item.StreamId = CreateNewStreamId();
                    if (message_item.Attachments.Any())
                    {
                        var index = 0;
                        message_item.Attachments.ForEach(att => att.fileNumber = ++index);
                        StoreAttachments(mail_box.TenantId, mail_box.UserId, message_item.Attachments, message_item.StreamId);
                    }

                    _log.Debug("StoreMailBody(Account:{0})", mail_box.EMail);

                    StoreMailBody(mail_box.TenantId, mail_box.UserId, message_item);

                    _log.Debug("MailSave(Account:{0})", mail_box.EMail);

                    message_item.IsNew = unread;

                    var folder_restore = MailFolder.Ids.spam == folder_id || MailFolder.Ids.trash == folder_id
                                             ? MailFolder.Ids.inbox
                                             : folder_id;
                    mail_id = MailSave(mail_box, message_item, 0, folder_id, folder_restore, uidl, md5, true);
                    AddRelationshipEventForLinkedAccounts(mail_box, message_item, mail_id, _log);

                    _log.Debug("MailSave(Account:{0}) returned mailId = {1}\r\n", mail_box.EMail, mail_id);
                }
                catch (Exception)
                {
                    //Trying to delete all attachments and mailbody
                    var storage = MailDataStore.GetDataStore(mail_box.TenantId);
                    try
                    {
                        storage.DeleteDirectory(string.Empty, string.Format("{0}/{1}", mail_box.UserId, message_item.StreamId));
                    }
                    catch (Exception ex)
                    {
                        _log.Debug("Problems with mail_directory deleting. Account: {0}. Folder: {1}/{2}/{3}. Exception: {4}", mail_box.EMail, mail_box.TenantId, mail_box.UserId, message_item.StreamId, ex.ToString());
                    }

                    _log.Debug("Problem with mail proccessing(Account:{0}). Body and attachment was deleted", mail_box.EMail);
                    throw;
                }

            }

            bool is_mailbox_removed;
            bool is_mailbox_deactivated;
            DateTime begin_date;

            // checks mailbox state to delete message 
            // If account was removed during saving process then message retrieve will stop.
            GetMailBoxState(mail_box.MailBoxId, out is_mailbox_removed, out is_mailbox_deactivated, out begin_date);

            if (mail_box.BeginDate != begin_date)
            {
                mail_box.BeginDateChanged = true;
                mail_box.BeginDate = begin_date;
            }

            if (is_mailbox_removed)
            {
                using (var db = GetDb())
                {
                    DeleteMessages(db,
                                   mail_box.TenantId,
                                   mail_box.UserId,
                                   new List<object[]>
                                       {
                                           new object[] {mail_id, folder_id, 1}
                                       },
                                   true);
                }

                throw new MailBoxOutException(MailBoxOutException.Types.REMOVED,
                                              string.Format("MailBox with id={0} is removed.\r\n", mail_box.MailBoxId));
            }

            if (is_mailbox_deactivated)
                throw new MailBoxOutException(MailBoxOutException.Types.DEACTIVATED,
                                              string.Format("MailBox with id={0} is deactivated.\r\n",
                                                            mail_box.MailBoxId));

            return mail_id;
        }


        public void AddRelationshipEventForLinkedAccounts(MailBox mail_box, MailMessageItem message_item)
        {
            AddRelationshipEventForLinkedAccounts(mail_box, message_item, message_item.Id, null);
        }


        private void AddRelationshipEventForLinkedAccounts(MailBox mail_box, MailMessageItem message_item, long mail_id, ILogger log)
        {
            try
            {
                message_item.LinkedCrmEntityIds = GetLinkedCrmEntitiesId(message_item.ChainId, mail_box.MailBoxId,mail_box.TenantId);
                var crm_dal = new CrmHistoryDal(this, mail_box.TenantId, mail_box.UserId);
                message_item.Id = mail_id;
                crm_dal.AddRelationshipEvents(message_item);
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.WarnException(String.Format("Problem with adding history event to CRM. mailId={0}", mail_id), ex);
            }
        }

        /// <summary>
        /// Creates Rfc 2822 3.6.4 message-id. Syntax: '&lt;' id-left '@' id-right '&gt;'.
        /// </summary>
        public string CreateMessageId()
        {
            return "<" + Guid.NewGuid().ToString().Replace("-", "").Substring(16) + "@" +
                   Guid.NewGuid().ToString().Replace("-", "").Substring(16) + ">";
        }

        /// <summary>
        /// Updates mail_mail chain's references and mail_chains records when new message was saved
        /// </summary>
        /// <param name="db">Db manager</param>
        /// <param name="mailbox">New message mailbox</param>
        /// <param name="mime_message_id">New message mime message id</param>
        /// <param name="chain_id">New message chain id</param>
        /// <param name="folder">New message folder id</param>
        /// <returns>Nothing</returns>
        /// <short>Updates mail_mail chain's references and mail_chains records when new message was saved</short>
        /// <category>Mail</category>
        public void UpdateMessagesChains(IDbManager db, MailBox mailbox, string mime_message_id, string chain_id,
                                         int folder)
        {
            var chains_for_update = new[] {new {id = chain_id, folder = folder}};

            // if mime_message_id == chain_id - message is first in chain, because it isn't reply
            if (!string.IsNullOrEmpty(mime_message_id) && mime_message_id != chain_id)
            {
                // Get chains which has our newly saved message as root.
                var chains = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.id, ChainTable.Columns.folder)
                        .Where(ChainTable.Columns.id, mime_message_id)
                        .Where(ChainTable.Columns.id_mailbox, mailbox.MailBoxId)
                        .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    ).Select(x => new { id = (string) x[0], folder = Convert.ToInt32(x[1])}).ToArray();

                if (chains.Any())
                {
                    db.ExecuteNonQuery(
                        new SqlUpdate(MailTable.name)
                            .Set(MailTable.Columns.chain_id, chain_id)
                            .Where(MailTable.Columns.chain_id, mime_message_id)
                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.is_removed, false));

                    chains_for_update = chains.Concat(chains_for_update).ToArray();

                    var new_chains_for_update = db.ExecuteList(
                        new SqlQuery(MailTable.name)
                            .Select(MailTable.Columns.folder)
                            .Where(MailTable.Columns.chain_id, chain_id)
                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.is_removed, false)
                            .Distinct())
                            .ConvertAll(x => new
                            {
                                id = chain_id,
                                folder = Convert.ToInt32(x[0]),
                            });

                    chains_for_update = chains_for_update.Concat(new_chains_for_update).ToArray();
                }
            }

            foreach (var c in chains_for_update.Distinct())
            {
                UpdateChain(db, c.id, c.folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);
            }
        }

        public string DetectChainId(MailBox mailbox, MailMessageItem message_item)
        {
            var chain_id = message_item.MessageId; //Chain id is equal to root conversataions message - messageId
            if (!string.IsNullOrEmpty(message_item.MessageId) && !string.IsNullOrEmpty(message_item.InReplyTo))
            {
                chain_id = message_item.InReplyTo;
                try
                {
                    using (var db = GetDb())
                    {
                        var base_query = GetDetectChainBaseQuery(mailbox);
                        var detect_chain_by_in_reply_to_query = GetDetectChainByInReplyToIdQuery(base_query, message_item);

                        var chain_ids_detected_with_in_reply_to = db.ExecuteList(detect_chain_by_in_reply_to_query);

                        if (chain_ids_detected_with_in_reply_to.Any())
                        {
                            chain_id = chain_ids_detected_with_in_reply_to.Select(r => Convert.ToString(r[0])).First();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("DetectChainId() params tenant={0}, user_id='{1}', mailbox_id={2}, mime_message_id='{3}' Exception:\r\n{4}",
                        mailbox.TenantId, mailbox.UserId, message_item.MailboxId, message_item.MessageId,ex.ToString());
                }
            }

            _log.Debug("DetectChainId() tenant='{0}', user_id='{1}', mailbox_id='{2}', mime_message_id='{3}' Result: {4}",
                        mailbox.TenantId, mailbox.UserId, message_item.MailboxId, message_item.MessageId, chain_id);

            return chain_id;
        }

        private static SqlQuery GetDetectChainByInReplyToIdQuery(SqlQuery base_query, MailMessageItem message_item)
        {
            return base_query
                .Where(Exp.Eq(MailTable.Columns.mime_message_id, message_item.InReplyTo));
        }

        private SqlQuery GetDetectChainBaseQuery(MailBox mailbox)
        {
            var base_query = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id)
                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                .Where(MailTable.Columns.is_removed, false)
                .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                .Distinct();
            return base_query;
        }


        public MailMessageItem GetMailInfo(int tenant, string user, int id_mail, bool load_images, bool load_body)
        {
            using (var db = GetDb())
            {
                var message_item = GetMailInfo(db, tenant, user, id_mail, load_images, load_body);

                if (message_item == null) return null;

                if (message_item.WasNew)
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.name)
                                .Where(MailTable.Columns.id, id_mail)
                                .Where(GetUserWhere(user, tenant))
                                .Set(MailTable.Columns.unread, false));
                        UpdateChain(db, message_item.ChainId, message_item.Folder, message_item.MailboxId, tenant, user);
                        tx.Commit();
                    }
                }

                return message_item;
            }
        }

        public MailMessageItem GetMailInfo(IDbManager db, int id_tenant, string id_user, int id_mail, bool load_images, bool load_body)
        {
            var db_info = db.ExecuteList(
                new SqlQuery(MailTable.name)
                    .Select(
                        MailTable.Columns.address,
                        MailTable.Columns.chain_id,
                        MailTable.Columns.chain_date,
                        MailTable.Columns.importance,
                        MailTable.Columns.date_sent,
                        MailTable.Columns.from,
                        MailTable.Columns.to,
                        MailTable.Columns.cc,
                        MailTable.Columns.bcc,
                        MailTable.Columns.reply,
                        MailTable.Columns.stream,
                        MailTable.Columns.is_answered,
                        MailTable.Columns.is_forwarded,
                        MailTable.Columns.subject,
                        MailTable.Columns.attach_count,
                        MailTable.Columns.size,
                        MailTable.Columns.is_from_crm,
                        MailTable.Columns.is_from_tl,
                        MailTable.Columns.folder,
                        MailTable.Columns.unread,
                        MailTable.Columns.introduction,
                        MailTable.Columns.is_text_body_only,
                        MailTable.Columns.id_mailbox,
                        MailTable.Columns.folder_restore,
                        MailTable.Columns.has_parse_error
                    )
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(MailTable.Columns.id, id_mail))
                            .ConvertAll(x => new
                            {
                                address = (string)x[0],
                                chain_id = (string)x[1],
                                chain_date = Convert.ToDateTime(x[2]),
                                importance = Convert.ToBoolean(x[3]),
                                datesent = Convert.ToDateTime(x[4]),
                                from = (string)x[5],
                                to = (string)x[6],
                                cc = (string)x[7],
                                bcc = (string)x[8],
                                reply_to = (string)x[9],
                                stream = (string)x[10],
                                isanswered = Convert.ToBoolean(x[11]),
                                isforwarded = Convert.ToBoolean(x[12]),
                                subject = (string)x[13],
                                hasAttachments = Convert.ToBoolean(x[14]),
                                size = Convert.ToInt64(x[15]),
                                is_from_crm = Convert.ToBoolean(x[16]),
                                is_from_tl = Convert.ToBoolean(x[17]),
                                folder = Convert.ToInt32(x[18]),
                                unread = Convert.ToBoolean(x[19]),
                                introduction = (string)x[20],
                                is_text_body_only = Convert.ToBoolean(x[21]),
                                id_mailbox = Convert.ToInt32(x[22]),
                                folder_restore = Convert.ToInt32(x[23]),
                                has_parse_error = Convert.ToBoolean(x[24])
                            })
                            .SingleOrDefault();

            if (db_info == null)
                return null;

            var tags = new ItemList<int>(
                db.ExecuteList(
                    new SqlQuery(MAIL_TAG_MAIL)
                        .Select(TagMailFields.id_tag)
                        .Where(TagMailFields.id_mail, id_mail))
                  .ConvertAll(x => (int) x[0]));

            var now = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(id_tenant), DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(id_tenant), db_info.datesent);
            var is_today = (now.Year == date.Year && now.Date == date.Date);
            var is_yesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            var item = new MailMessageItem
                {
                    Id = id_mail,
                    ChainId = db_info.chain_id,
                    ChainDate = db_info.chain_date,
                    Attachments = null,
                    Address = db_info.address,
                    Bcc = db_info.bcc,
                    Cc = db_info.cc,
                    Date = date,
                    From = db_info.@from,
                    HasAttachments = db_info.hasAttachments,
                    Important = db_info.importance,
                    IsAnswered = db_info.isanswered,
                    IsForwarded = db_info.isforwarded,
                    IsNew = false,
                    TagIds = tags,
                    ReplyTo = db_info.reply_to,
                    Size = db_info.size,
                    Subject = db_info.subject,
                    To = db_info.to,
                    StreamId = db_info.stream,
                    IsFromCRM = db_info.is_from_crm,
                    IsFromTL = db_info.is_from_tl,
                    Folder = db_info.folder,
                    WasNew = db_info.unread,
                    IsToday = is_today,
                    IsYesterday = is_yesterday,
                    Introduction = db_info.introduction,
                    TextBodyOnly = db_info.is_text_body_only,
                    MailboxId = db_info.id_mailbox,
                    RestoreFolderId = db_info.folder_restore,
                    HasParseError = db_info.has_parse_error
                };

            //Reassemble paths
            if (load_body)
            {
                var html_body = "";

                if (!item.HasParseError)
                {
                    var data_store = MailDataStore.GetDataStore(id_tenant);

                    var key = string.Format("{0}/{1}/body.html", id_user, item.StreamId);

                    try
                    {
                        using (var s = data_store.GetReadStream(string.Empty, key))
                        {
                            html_body = Encoding.UTF8.GetString(s.GetCorrectBuffer());
                        }

                        if (item.Folder != MailFolder.Ids.drafts && !item.From.Equals(MAIL_DAEMON_EMAIL))
                        {
                            bool images_are_blocked;
                            html_body = HtmlSanitizer.Sanitize(html_body, load_images, out images_are_blocked);
                            item.ContentIsBlocked = images_are_blocked;
                        }
                    }
                    catch (Exception ex)
                    {
                        item.IsBodyCorrupted = true;
                        html_body = "";
                        _log.Error(ex, "Load stored body error: tenant={0} user=\"{1}\" messageId={2} key=\"{3}\"",
                                   id_tenant, id_user, id_mail, key);
                    }
                }

                item.HtmlBody = html_body;

                if (string.IsNullOrEmpty(db_info.introduction) && !string.IsNullOrEmpty(item.HtmlBody))
                {
                    // if introduction wasn't saved, it will be save.
                    var introduction = MailMessageItem.GetIntroduction(html_body);

                    if (!string.IsNullOrEmpty(introduction))
                    {
                        item.Introduction = introduction;

                        db.ExecuteNonQuery( new SqlUpdate(MailTable.name)
                            .Set(MailTable.Columns.introduction, item.Introduction)
                            .Where(GetUserWhere(id_user, id_tenant))
                            .Where(MailTable.Columns.id, id_mail));
                    }
                }
            }

            var attachments = GetMessageAttachments(db, id_tenant, id_user, id_mail);

            item.Attachments = attachments.Count != 0 ? attachments : new List<MailAttachment>();

            return item;
        }

        public List<MailAttachment> GetMessageAttachments(int id_tenant, string id_user, int id_mail)
        {
            using (var db = GetDb())
            {
                var db_info =
                    db.ExecuteList(
                        new SqlQuery(MailTable.name)
                            .Select(
                                MailTable.Columns.stream,
                                MailTable.Columns.attach_count
                            )
                            .Where(GetUserWhere(id_user, id_tenant))
                            .Where(MailTable.Columns.id, id_mail))
                      .ConvertAll(x => new
                          {
                              stream = x[0].ToString(),
                              attachments_count = Convert.ToInt32(x[1])
                          })
                          .FirstOrDefault();

                if (db_info != null && db_info.attachments_count > 0)
                {
                    return GetMessageAttachments(db, id_tenant, id_user, id_mail);
                }
            }

            return new List<MailAttachment>();
        }

        private SqlQuery GetAttachmentsSelectQuery()
        {
            return new SqlQuery(AttachmentTable.name)
                .InnerJoin(MailTable.name,
                           Exp.EqColumns(MailTable.Columns.id.Prefix(MailTable.name),
                                         AttachmentTable.Columns.id_mail.Prefix(AttachmentTable.name)))
                .Select(AttachmentTable.Columns.id.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.name.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.stored_name.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.type.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.size.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.file_number.Prefix(AttachmentTable.name),
                        MailTable.Columns.stream.Prefix(MailTable.name),
                        MailTable.Columns.id_tenant.Prefix(MailTable.name),
                        MailTable.Columns.id_user.Prefix(MailTable.name),
                        AttachmentTable.Columns.content_id.Prefix(AttachmentTable.name));
        }


        private List<MailAttachment> GetMessageAttachments(IDbManager db, int id_tenant, string id_user, int id_mail)
        {
            var attachments_select_query = GetAttachmentsSelectQuery()
                .Where(MailTable.Columns.id.Prefix(MailTable.name), id_mail)
                .Where(AttachmentTable.Columns.need_remove.Prefix(AttachmentTable.name), false)
                .Where(AttachmentTable.Columns.content_id, Exp.Empty)
                .Where(GetUserWhere(id_user, id_tenant, MailTable.name));

            var attachments =
                db.ExecuteList(attachments_select_query)
                  .ConvertAll(ToMailItemAttachment);

            return attachments;
        }

        public List<MailMessageItem> GetMailsFiltered(int id_tenant, string id_user, MailFilter filter, int page,
                                                      int page_size, out long total_messages_count)
        {
            return GetSingleMailsFiltered(id_tenant, id_user, filter, page, page_size, out total_messages_count);
        }

        public List<MailMessageItem> GetSingleMailsFiltered(int id_tenant, string id_user, MailFilter filter, int page,
                                                            int page_size, out long total_messages_count)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            if (page <= 0)
                throw new ArgumentOutOfRangeException("page", "Can't be less than one.");

            if (page_size <= 0)
                throw new ArgumentOutOfRangeException("page_size", "Can't be less than one.");

            var concat_tag_ids =
                String.Format(
                    "(SELECT CAST(group_concat(tm.{0} ORDER BY tm.{3} SEPARATOR ',') AS CHAR) from {1} as tm WHERE tm.{2} = `id`) tagIds",
                    TagMailFields.id_tag, MAIL_TAG_MAIL, TagMailFields.id_mail, TagMailFields.time_created);


            using (var db = GetDb())
            {
                var q_filtered = new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id, MailTable.Columns.chain_id)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .ApplyFilter(filter);

                // Filter and sort all existing messages and get their ids and chain_ids
                var filtered_ids = db.ExecuteList(q_filtered)
                                     .ConvertAll(r => new {id = Convert.ToInt32(r[0]), chain_id = (string) r[1]});

                var final_ids_set = filtered_ids.Select(id_chid => id_chid.id);
                const string select_chain_length = "1";

                var ids_set = final_ids_set as IList<int> ?? final_ids_set.ToList();
                total_messages_count = ids_set.Count();
                page = Math.Min(page, (int) Math.Ceiling((double) total_messages_count/page_size));

                var query_messages = new SqlQuery(MailTable.name + " as outer_mail")
                    .Select(MailTable.Columns.id, MailTable.Columns.from, MailTable.Columns.to,
                            MailTable.Columns.reply, MailTable.Columns.subject, MailTable.Columns.importance,
                            "1", MailTable.Columns.date_sent, MailTable.Columns.size,
                            MailTable.Columns.attach_count, MailTable.Columns.unread, MailTable.Columns.is_answered,
                            MailTable.Columns.is_forwarded, MailTable.Columns.is_from_crm, MailTable.Columns.is_from_tl,
                            concat_tag_ids, MailTable.Columns.folder_restore, MailTable.Columns.chain_id, select_chain_length, 
                            MailTable.Columns.folder)
                    // Select by final ids set
                    .Where(Exp.In(MailTable.Columns.id, ids_set.ToArray()))
                    // The following two lines could be skipped. Because we have already applied this filtering two times beforehand
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(id_user, id_tenant))
                    // Filtering is unnecessary here. Only sorting must be applied
                    .ApplySorting(filter)
                    // Paging
                    .SetFirstResult((page - 1)*page_size)
                    .SetMaxResults(page_size);

                var list = db.ExecuteList(query_messages)
                             .ConvertAll(r =>
                                         ConvertToMailMessageItem(r, id_tenant));

                return list;
            }
        }

        public long GetNextMessageId(int tenant, string user, int id, MailFilter filter)
        {
            using (var db = GetDb())
            {
                var date_sent = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.date_sent)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.id, id));

                var sort_order = filter.SortOrder == "ascending";

                return db.ExecuteScalar<long>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(user, tenant))
                    .ApplyFilter(filter)
                    .Where(sort_order ? Exp.Ge(MailTable.Columns.date_sent, date_sent) : Exp.Le(MailTable.Columns.date_sent, date_sent))
                    .SetFirstResult(1)
                    .SetMaxResults(1)
                    .OrderBy(MailTable.Columns.date_sent, sort_order));
            }
        }

        public List<MailMessageItem> GetConversationMessages(int tenant, string user, int message_id,
                                                             bool load_all_content)
        {
            var get_message_info = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id, MailTable.Columns.id_mailbox, MailTable.Columns.folder)
                .Where(MailTable.Columns.is_removed, false)
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.id, message_id);

            using (var db = GetDb())
            {
                var message_info = db.ExecuteList(get_message_info)
                                     .ConvertAll(
                                         r =>
                                         new
                                             {
                                                 chain_id = (string) r[0],
                                                 mailbox_id = Convert.ToInt32(r[1]),
                                                 folder = Convert.ToInt32(r[2])
                                             })
                                     .FirstOrDefault();

                if (message_info == null) throw new ArgumentException("Message Id not found");

                var search_folders = new List<int>();

                if (message_info.folder == MailFolder.Ids.inbox || message_info.folder == MailFolder.Ids.sent)
                    search_folders.AddRange(new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
                else
                    search_folders.Add(message_info.folder);

                var get_messages_ids = GetQueryForChainMessagesSelection(message_info.mailbox_id, message_info.chain_id, search_folders)
                                       .OrderBy(MailTable.Columns.date_sent, true);


                var query_result = db.ExecuteList(get_messages_ids);
                var list_messages = query_result.Select(
                    (item, i) =>
                    GetMailInfo(db, tenant, user, Convert.ToInt32(item[0]), false,
                                load_all_content || (i == query_result.Count - 1)))
                                                .ToList();

                var unread_messages = list_messages.Where(message => message.WasNew).ToList();
                if (!unread_messages.Any())
                    return list_messages;

                var unread_messages_count_by_folder = new Dictionary<int, int>();

                foreach (var message in unread_messages)
                {
                    if (unread_messages_count_by_folder.ContainsKey(message.Folder))
                        unread_messages_count_by_folder[message.Folder] += 1;
                    else
                        unread_messages_count_by_folder.Add(message.Folder, 1);
                }

                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(
                        new SqlUpdate(MailTable.name)
                            .Where(Exp.In(MailTable.Columns.id, unread_messages.Select(x => (object)x.Id).ToArray() ))
                            .Where(GetUserWhere(user, tenant))
                            .Set(MailTable.Columns.unread, false));

                    foreach (var key_pair in unread_messages_count_by_folder)
                    {
                        ChangeFolderCounters(db, tenant, user, key_pair.Key, key_pair.Value*(-1), 0, -1, 0);

                        db.ExecuteNonQuery(
                            new SqlUpdate(ChainTable.name)
                                .Set(ChainTable.Columns.unread, false)
                                .Where(GetUserWhere(user, tenant))
                                .Where(ChainTable.Columns.id, message_info.chain_id)
                                .Where(ChainTable.Columns.id_mailbox, message_info.mailbox_id)
                                .Where(ChainTable.Columns.folder, key_pair.Key));
                    }

                    tx.Commit();
                }

                return list_messages;
            }
        }



        public void SetMessageFolderRestore(int id_tenant, string id_user, int to_folder, int message_id)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Set(MailTable.Columns.folder_restore, to_folder)
                        .Where(GetUserWhere(id_user, id_tenant))
                        .Where(MailTable.Columns.id, message_id));
            }
        }

        public string GetMimeMessageIdByMessageId(int message_id)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<string>(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.mime_message_id)
                        .Where(MailTable.Columns.id, message_id)) ?? "";
            }
        }

        public void UpdateMessageUidl(int id_tenant, string id_user, int id_message, string new_uidl)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(GetUserWhere(id_user, id_tenant))
                        .Where(MailTable.Columns.id, id_message)
                        .Set(MailTable.Columns.uidl, new_uidl));
            }
        }

        #endregion

        #region private methods

        private void SetMessagesFolder(DbManager db, int id_tenant, string id_user, List<object[]> mails_info,
                                       int id_folder)
        {
            if (mails_info == null || mails_info.Count <= 0)
                return;

            var unique_chain_info = mails_info
                .ConvertAll(x => new
                    {
                        folder = Convert.ToInt32(x[2]),
                        chain_id = (string) x[3],
                        id_mailbox = Convert.ToInt32(x[4])
                    })
                .Distinct();

            var prev_info = mails_info.ConvertAll(x => new
                {
                    id = Convert.ToInt32(x[0]),
                    unread = Convert.ToBoolean(x[1]),
                    folder = Convert.ToInt32(x[2]),
                    chain_id = (string) x[3],
                    id_mailbox = Convert.ToInt32(x[4])
                });

            var ids = mails_info.ConvertAll(x => Convert.ToInt32(x[0]));

            var query = new SqlUpdate(MailTable.name)
                .Set(MailTable.Columns.folder, id_folder)
                .Where(GetUserWhere(id_user, id_tenant))
                .Where(Exp.In(MailTable.Columns.id, ids));

            db.ExecuteNonQuery(query);

            foreach (var info in unique_chain_info)
                UpdateChain(db, info.chain_id, info.folder, info.id_mailbox, id_tenant, id_user);

            var unread_messages_count_collection = new Dictionary<int, int>();
            var total_messages_count_collection = new Dictionary<int, int>();

            foreach (var info in prev_info)
            {
                if (total_messages_count_collection.ContainsKey(info.folder))
                    total_messages_count_collection[info.folder] += 1;
                else
                    total_messages_count_collection.Add(info.folder, 1);

                if (!info.unread) continue;
                if (unread_messages_count_collection.ContainsKey(info.folder))
                    unread_messages_count_collection[info.folder] += 1;
                else
                    unread_messages_count_collection.Add(info.folder, 1);
            }

            UpdateChainFields(db, id_tenant, id_user, ids);

            var moved_total_unread_count = 0;
            var moved_total_count = 0;

            foreach (var key_pair in total_messages_count_collection)
            {
                var source_folder = key_pair.Key;
                var total_move = key_pair.Value;
                int unread_move;
                unread_messages_count_collection.TryGetValue(source_folder, out unread_move);
                ChangeFolderCounters(db, id_tenant, id_user, source_folder, unread_move != 0 ? unread_move*(-1) : 0,
                                     total_move*(-1), false);
                moved_total_unread_count += unread_move;
                moved_total_count += total_move;
            }

            if (moved_total_unread_count != 0 || moved_total_count != 0)
                ChangeFolderCounters(db, id_tenant, id_user, id_folder,
                                     moved_total_unread_count, moved_total_count, false);
        }

        private void SetMessagesFolder(DbManager db, int id_tenant, string id_user, int folder, MailFilter filter)
        {
            var condition_by_filter = ASC.Mail.Aggregator.Extension.SqlQueryExtensions.GetMailFilterConditions(filter, true, "");

            if (filter.PrimaryFolder == MailFolder.Ids.inbox || filter.PrimaryFolder == MailFolder.Ids.sent)
                condition_by_filter &= Exp.In(MailTable.Columns.folder, new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
            else
                condition_by_filter &= Exp.Eq(MailTable.Columns.folder, filter.PrimaryFolder);

            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.folder, folder)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(condition_by_filter));

            RecalculateFolders(db, id_tenant, id_user, false);
        }

        private void RestoreMessages(DbManager db, int id_tenant, string id_user, MailFilter filter)
        {
            var condition_by_filter = ASC.Mail.Aggregator.Extension.SqlQueryExtensions.GetMailFilterConditions(filter, true, "");
                condition_by_filter &= Exp.Eq(MailTable.Columns.folder, filter.PrimaryFolder);
            
            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.folder + " = " + MailTable.Columns.folder_restore)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(condition_by_filter));

            RecalculateFolders(db, id_tenant, id_user, false);
        }

        private void RestoreMessages(DbManager db, int id_tenant, string id_user, List<object[]> mails_info)
        {
            if (mails_info == null || mails_info.Count <= 0)
                return;

            var unique_chain_info = mails_info
                .ConvertAll(x => new
                {
                    folder = Convert.ToInt32(x[2]),
                    chain_id = (string)x[4],
                    id_mailbox = Convert.ToInt32(x[5])
                })
                .Distinct();

            var prev_info = mails_info.ConvertAll(x => new
            {
                id = Convert.ToInt32(x[0]),
                unread = Convert.ToBoolean(x[1]),
                folder = Convert.ToInt32(x[2]),
                folder_restore = Convert.ToInt32(x[3]),
                chain_id = (string)x[4],
                id_mailbox = Convert.ToInt32(x[5])
            });

            var ids_array = mails_info.ConvertAll(x => Convert.ToInt32(x[0]));

            var update_query =
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.folder + " = " + MailTable.Columns.folder_restore)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(Exp.In(MailTable.Columns.id, ids_array));

            db.ExecuteNonQuery(update_query);

            // Update chains in old folder
            foreach (var info in unique_chain_info)
                UpdateChain(db, info.chain_id, info.folder, info.id_mailbox, id_tenant, id_user);

            var unread_messages_count_collection = new Dictionary<int, int>();
            var total_messages_count_collection = new Dictionary<int, int>();

            foreach (var info in prev_info)
            {
                if (total_messages_count_collection.ContainsKey(info.folder_restore))
                    total_messages_count_collection[info.folder_restore] += 1;
                else
                    total_messages_count_collection.Add(info.folder_restore, 1);

                if (!info.unread) continue;
                if (unread_messages_count_collection.ContainsKey(info.folder_restore))
                    unread_messages_count_collection[info.folder_restore] += 1;
                else
                    unread_messages_count_collection.Add(info.folder_restore, 1);
            }

            // Update chains in new restored folder
            UpdateChainFields(db, id_tenant, id_user, ids_array);

            var prev_total_unread_count = 0;
            var prev_total_count = 0;

            foreach (var key_pair in total_messages_count_collection)
            {
                var folder_restore = key_pair.Key;
                var total_restore = key_pair.Value;
                int unread_restore;
                unread_messages_count_collection.TryGetValue(folder_restore, out unread_restore);
                ChangeFolderCounters(db, id_tenant, id_user, folder_restore, unread_restore, total_restore, false);
                prev_total_unread_count -= unread_restore;
                prev_total_count -= total_restore;
            }

            // Subtract the restored number of messages in the previous folder
            if (prev_total_unread_count != 0 || prev_total_count != 0)
                ChangeFolderCounters(db, id_tenant, id_user, prev_info[0].folder, prev_total_unread_count,
                                     prev_total_count, false);
        }

        private long DeleteMessages(DbManager db, int id_tenant, string id_user,
                                   List<object[]> delete_info, bool re_count_folders)
        {
            var message_fields_info = delete_info
                .ConvertAll(r =>
                            new
                                {
                                    id = Convert.ToInt32(r[0]),
                                    folder = Convert.ToInt32(r[1]),
                                    unread = Convert.ToInt32(r[2])
                                });

            var message_ids = message_fields_info.Select(m => m.id).ToArray();

            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.is_removed, true)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(Exp.In(MailTable.Columns.id, message_ids)));

            var used_quota = MarkAttachmetsNeedRemove(db, id_tenant, Exp.In(AttachmentTable.Columns.id_mail, message_ids));

            var affected_tags = db.ExecuteList(
                new SqlQuery(MAIL_TAG_MAIL)
                    .Select(TagMailFields.id_tag)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(Exp.In(TagMailFields.id_mail, message_ids)))
                                  .ConvertAll(r => Convert.ToInt32(r[0]));

            db.ExecuteNonQuery(
                new SqlDelete(MAIL_TAG_MAIL)
                    .Where(Exp.In(TagMailFields.id_mail, message_ids))
                    .Where(GetUserWhere(id_user, id_tenant)));

            UpdateTagsCount(db, id_tenant, id_user, affected_tags.Distinct());

            if (!re_count_folders)
            {
                var total_collection = (from row in message_fields_info
                                        group row by row.folder
                                        into g
                                        select new {id = Convert.ToInt32(g.Key), diff = -g.Count()})
                    .ToList();

                var unread_collection = (from row in message_fields_info.Where(m => m.unread == 1)
                                         group row by row.folder
                                         into g
                                         select new {id = Convert.ToInt32(g.Key), diff = -g.Count()})
                    .ToList();

                foreach (var folder in total_collection)
                {
                    var unread_in_folder = unread_collection
                        .FirstOrDefault(f => f.id == folder.id);

                    ChangeFolderCounters(db, id_tenant, id_user, folder.id,
                                         unread_in_folder != null ? unread_in_folder.diff : 0, folder.diff, false);
                }
            }
            else
            {
                RecalculateFolders(db, id_tenant, id_user, false);
            }

            UpdateChainFields(db, id_tenant, id_user, message_fields_info.Select(m => Convert.ToInt32(m.id)).ToList());

            return used_quota;
        }

        private List<object[]> GetMessagesInfoByMimeMessageId(
            int id_mailbox,
            int id_tenant,
            string id_user,
            string mime_message_id,
            string[] columns)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(columns)
                        .Where(MailTable.Columns.mime_message_id, mime_message_id)
                        .Where(MailTable.Columns.id_mailbox, id_mailbox)
                        .Where(GetUserWhere(id_user, id_tenant)));
            }
        }

        private int[] AddTagIdsForSelfSendedMessages(MailBox mail_box, int[] tags_ids, MailMessageItem message_item)
        {
            try
            {
                var from_address_email = new MailAddress(message_item.From).Address;
                var to_address_email = new MailAddress(message_item.To).Address;
                if (from_address_email == to_address_email)
                {
                    List<object[]> self_sended_message_tags_ids;
                    using (var db = GetDb())
                    {
                        self_sended_message_tags_ids = db.ExecuteList(
                            new SqlQuery(MailTable.name + " mm")
                                .InnerJoin(MAIL_TAG_MAIL + " mt",
                                           Exp.EqColumns(MailTable.Columns.id.Prefix("mm"), TagMailFields.id_mail.Prefix("mt")))
                                .Select(TagMailFields.id_tag.Prefix("mt"))
                                .Where(MailTable.Columns.id_mailbox.Prefix("mm"), mail_box.MailBoxId)
                                .Where(MailTable.Columns.folder.Prefix("mm"), MailFolder.Ids.sent)
                                .Where(MailTable.Columns.mime_message_id.Prefix("mm"), message_item.MessageId)
                            );
                    }

                    if (self_sended_message_tags_ids.Count > 0)
                    {
                        var temp = tags_ids != null ? tags_ids.ToList() : new List<int>();
                        temp.AddRange(self_sended_message_tags_ids.Select(x => int.Parse(x[0].ToString())));
                        tags_ids = temp.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                // if we appears in that section than to address contains more than one mailadreses
            }
            return tags_ids;
        }


        private MailMessageItem ProcessMail(Message message, MailBox mail_box, int folder_id, bool has_parse_error = false)
        {
            _log.Debug("Parse Message -> MailMessageItem");

            var message_item = new MailMessageItem(message) {HasParseError = has_parse_error};

            if (folder_id == MailFolder.Ids.inbox)
            {
                var ids = GetCrmContactsId(mail_box.TenantId, mail_box.UserId, message_item.FromEmail);
                if (ids.Count > 0)
                {
                    message_item.IsFromCRM = true;
                    message_item.ParticipantsCrmContactsId = ids;
                }
            }

            _log.Debug("SetCrmTags()");

            SetCrmTags(message_item, mail_box.TenantId, mail_box.UserId);

            _log.Debug("DetectChain()");

            if (string.IsNullOrEmpty(message_item.MessageId))
                message_item.MessageId = CreateMessageId();

            message_item.ChainId = DetectChainId(mail_box, message_item);

            return message_item;
        }

        private List<object[]> GetMessagesInfo(IDbManager db, int tenant, string user,
                                               List<int> ids, string[] columns)
        {
            if (!ids.Any()) 
                throw new ArgumentException("ids are empty");

            return db.ExecuteList(
                new SqlQuery(MailTable.name)
                    .Select(columns)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.id, ids)));
        }

        private static SqlQuery GetQueryForChainMessagesSelection(int id_mailbox, string id_chain, List<int> search_folders)
        {
            return new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.id)
                .Where(MailTable.Columns.id_mailbox, id_mailbox)
                .Where(MailTable.Columns.chain_id, id_chain)
                .Where(Exp.In(MailTable.Columns.folder, search_folders.ToArray()))
                .Where(MailTable.Columns.is_removed, 0);
        }
        #endregion
    }
}
