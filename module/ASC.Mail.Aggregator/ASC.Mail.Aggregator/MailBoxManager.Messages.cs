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


using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Utils;
using ActiveUp.Net.Mail;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
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

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public methods

        public string CreateNewStreamId()
        {
            var streamId = Guid.NewGuid().ToString("N").ToLower();
            return streamId;
        }

        public void SetMessagesFolder(int tenant, string user, int folder, List<int> ids)
        {
            if (!MailFolder.IsIdOk(folder))
                throw new ArgumentException("can't set folder to none system folder");

            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var mailInfoList = GetMessagesInfo(db, tenant, user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder, MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                    });

                if (!mailInfoList.Any()) return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesFolder(db, tenant, user, mailInfoList, folder);
                    tx.Commit();
                }
            }
        }

        public void RestoreMessages(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var mailInfoList = GetMessagesInfo(db, tenant, user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder,
                        MailTable.Columns.folder_restore, MailTable.Columns.chain_id,
                        MailTable.Columns.id_mailbox
                    });

                if (!mailInfoList.Any()) return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    RestoreMessages(db, tenant, user, mailInfoList);
                    tx.Commit();
                }
            }
        }

        public void DeleteMessages(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");
            long usedQuota = 0;
            using (var db = GetDb())
            {
                var mailInfoList = GetMessagesInfo(db, tenant, user, ids, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.folder, MailTable.Columns.unread
                    });

                if (mailInfoList.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        usedQuota = DeleteMessages(db, tenant, user, mailInfoList, false);
                        tx.Commit();
                    }
                }
            }

            QuotaUsedDelete(tenant, usedQuota);
        }

        public void SetMessagesReadFlags(int tenant, string user, List<int> ids, bool isRead)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var mailInfoList = GetMessagesInfo(db, tenant, user, ids, MessageInfoToSetUnread.Fields)
                    .ConvertAll(x => new MessageInfoToSetUnread(x));

                if (!mailInfoList.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, tenant, user, mailInfoList, isRead);
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

            public int Id
            {
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

            public MessageInfoToSetUnread(IList<object> fieldsValues)
            {
                _id = Convert.ToInt32(fieldsValues[0]);
                _folder = Convert.ToInt32(fieldsValues[1]);
                _unread = Convert.ToBoolean(fieldsValues[2]);
                _chainId = (string)fieldsValues[3];
                _mailbox = Convert.ToInt32(fieldsValues[4]);
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
        /// <param name="mailInfoList">Info about messages to be update</param>
        /// <param name="isRead">New state to be set</param>
        /// <returns>List ob objects array</returns>
        /// <short>Get chains messages info</short>
        /// <category>Mail</category>
        private void SetMessagesReadFlags(IDbManager db, int tenant, string user, List<MessageInfoToSetUnread> mailInfoList, bool isRead)
        {
            if (!mailInfoList.Any())
                return;

            var messageInfoToSetUnreads = mailInfoList.Where(x => x.Unread == isRead).ToList();

            var ids = messageInfoToSetUnreads.Select(x => (object)x.Id).ToArray();

            if (!ids.Any())
                return;

            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Where(Exp.In(MailTable.Columns.id, ids))
                    .Where(GetUserWhere(user, tenant))
                    .Set(MailTable.Columns.unread, !isRead));

            var foldersMessCounterDiff = new Dictionary<int, int>();

            foreach (var mess in messageInfoToSetUnreads)
            {
                if (foldersMessCounterDiff.Keys.Contains(mess.Folder))
                    foldersMessCounterDiff[mess.Folder] += 1;
                else
                {
                    foldersMessCounterDiff[mess.Folder] = 1;
                }
            }

            var distinct = messageInfoToSetUnreads.Distinct(new MessageInfoToSetUnreadEqualityComparer()).ToList();

            foreach (var folder in foldersMessCounterDiff.Keys)
            {
                var sign = isRead ? -1 : 1;
                var messDiff = sign * foldersMessCounterDiff[folder];
                var convDiff = sign * distinct.Count(x => x.Folder == folder);

                ChangeFolderCounters(db, tenant, user, folder, messDiff, 0, convDiff, 0);
            }

            foreach (var messageId in distinct.Select(x => x.Id))
                UpdateMessageChainUnreadFlag(db, tenant, user, Convert.ToInt32(messageId));
        }

        public bool SetMessagesImportanceFlags(int tenant, string user, bool importance, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var affected = db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(Exp.In(MailTable.Columns.id, ids.ToArray()))
                        .Where(GetUserWhere(user, tenant))
                        .Set(MailTable.Columns.importance, importance));

                foreach (var messageId in ids)
                    UpdateMessageChainImportanceFlag(db, tenant, user, messageId);

                return affected > 0;
            }
        }

        public void UpdateChainFields(DbManager db, int tenant, string user, List<int> ids)
        {
            // Get additional information about mails
            var mailInfoList = GetMessagesInfo(db, tenant, user, ids, new[]
                {
                    MailTable.Columns.id_mailbox, MailTable.Columns.chain_id, MailTable.Columns.folder
                })
                .ConvertAll(x => new
                    {
                        id_mailbox = Convert.ToInt32(x[0]),
                        chain_id = (string)x[1],
                        folder = Convert.ToInt32(x[2])
                    });

            foreach (var info in mailInfoList.GroupBy(t => new { t.id_mailbox, t.chain_id, t.folder }))
            {
                UpdateChain(db, info.Key.chain_id, info.Key.folder, info.Key.id_mailbox, tenant, user);
            }
        }

        public void DeleteFoldersMessages(int tenant, string user, int folder)
        {
            long usedQuota = 0;

            using (var db = GetDb())
            {
                // Get message ids stored in the  folder.
                var ids = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.id)
                        .Where(MailTable.Columns.is_removed, false)
                        .Where(MailTable.Columns.folder, folder)
                        .Where(GetUserWhere(user, tenant)))
                    .Select(x => Convert.ToInt32(x[0])).ToArray();

                if (ids.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        // Mark messages stored in the folder as is_removed.
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.name)
                                .Set(MailTable.Columns.is_removed, true)
                                .Where(GetUserWhere(user, tenant))
                                .Where(MailTable.Columns.folder, folder));

                        // Recalculate used quota
                        usedQuota = MarkAttachmetsNeedRemove(db, tenant, Exp.In(AttachmentTable.Columns.id_mail, ids));

                        // delete tag/message cross references
                        db.ExecuteNonQuery(
                            new SqlDelete(TagMailTable.name)
                                .Where(Exp.In(TagMailTable.Columns.id_mail, ids))
                                .Where(GetUserWhere(user, tenant)));

                        // update tags counters
                        var tags = GetMailTags(db, tenant, user, Exp.Empty);
                        UpdateTagsCount(db, tenant, user, tags.Select(x => x.Id));

                        // delete chains stored in the folder
                        db.ExecuteNonQuery(new SqlDelete(ChainTable.name)
                                               .Where(GetUserWhere(user, tenant))
                                               .Where(ChainTable.Columns.folder, folder));

                        // reset folder counters
                        db.ExecuteNonQuery(new SqlUpdate(FolderTable.name)
                            .Where(GetUserWhere(user, tenant))
                            .Where(FolderTable.Columns.folder, folder)
                            .Set(FolderTable.Columns.total_conversations_count, 0)
                            .Set(FolderTable.Columns.total_messages_count, 0)
                            .Set(FolderTable.Columns.unread_conversations_count, 0)
                            .Set(FolderTable.Columns.unread_messages_count, 0));

                        tx.Commit();
                    }
                }
            }

            if (usedQuota > 0)
                QuotaUsedDelete(tenant, usedQuota);
        }

        public List<string> CheckUidlExistance(int mailboxId, List<string> uidlList)
        {
            using (var db = GetDb())
            {
                var existingUidls = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.uidl)
                        .Where(MailTable.Columns.id_mailbox, mailboxId)
                        .Where(Exp.In(MailTable.Columns.uidl, uidlList)))
                                         .ConvertAll(r => Convert.ToString(r[0]))
                                         .ToList();
                return existingUidls;
            }
        }

        public int MailSave(MailBox mailbox, MailMessageItem mail, int messageId, int folder, int folderRestore,
            string uidl, string md5, bool saveAttachments)
        {
            var idMail = 0;
            const int max_attempts = 2;
            var countAttachments = 0;
            long usedQuota = 0;
            var address = GetAddress(mailbox.EMail);

            using (var db = GetDb())
            {
                var iAttempt = 0;
                do
                {
                    try
                    {
                        using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            if (messageId != 0)
                                countAttachments = GetCountAttachments(db, messageId);

                            #region SQL query construction

                            if (messageId == 0)
                            {
                                // This case is for first time saved draft and received message.
                                var insert = new SqlInsert(MailTable.name, true)
                                    .InColumnValue(MailTable.Columns.id, messageId)
                                    .InColumnValue(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                                    .InColumnValue(MailTable.Columns.id_tenant, mailbox.TenantId)
                                    .InColumnValue(MailTable.Columns.id_user, mailbox.UserId)
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
                                                   !saveAttachments
                                                       ? countAttachments
                                                       : (mail.Attachments != null ? mail.Attachments.Count : 0))
                                    .InColumnValue(MailTable.Columns.unread, mail.IsNew)
                                    .InColumnValue(MailTable.Columns.is_answered, mail.IsAnswered)
                                    .InColumnValue(MailTable.Columns.is_forwarded, mail.IsForwarded)
                                    .InColumnValue(MailTable.Columns.stream, mail.StreamId)
                                    .InColumnValue(MailTable.Columns.folder, folder)
                                    .InColumnValue(MailTable.Columns.folder_restore, folderRestore)
                                    .InColumnValue(MailTable.Columns.is_from_crm, mail.IsFromCRM)
                                    .InColumnValue(MailTable.Columns.is_from_tl, mail.IsFromTL)
                                    .InColumnValue(MailTable.Columns.spam, 0)
                                    .InColumnValue(MailTable.Columns.mime_message_id, mail.MimeMessageId)
                                    .InColumnValue(MailTable.Columns.mime_in_reply_to, mail.MimeReplyToId)
                                    .InColumnValue(MailTable.Columns.chain_id, mail.ChainId)
                                    .InColumnValue(MailTable.Columns.introduction, mail.Introduction)
                                    .InColumnValue(MailTable.Columns.chain_date, mail.Date.ToUniversalTime())
                                    .InColumnValue(MailTable.Columns.is_text_body_only, mail.TextBodyOnly)
                                    .Identity(0, 0, true);

                                if (mail.HasParseError)
                                    insert.InColumnValue(MailTable.Columns.has_parse_error, mail.HasParseError);

                                idMail = db.ExecuteScalar<int>(insert);

                                ChangeFolderCounters(db, mailbox.TenantId, mailbox.UserId, folder,
                                    mail.IsNew ? 1 : 0, 1, false);
                            }
                            else
                            {
                                // This case is for already saved draft only.
                                var update = new SqlUpdate(MailTable.name)
                                    .Where(MailTable.Columns.id, messageId)
                                    .Set(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
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
                                         !saveAttachments
                                             ? countAttachments
                                             : (mail.Attachments != null ? mail.Attachments.Count : 0))
                                    .Set(MailTable.Columns.unread, mail.IsNew)
                                    .Set(MailTable.Columns.is_answered, mail.IsAnswered)
                                    .Set(MailTable.Columns.is_forwarded, mail.IsForwarded)
                                    .Set(MailTable.Columns.stream, mail.StreamId)
                                    .Set(MailTable.Columns.folder, folder)
                                    .Set(MailTable.Columns.folder_restore, folderRestore)
                                    .Set(MailTable.Columns.is_from_crm, mail.IsFromCRM)
                                    .Set(MailTable.Columns.is_from_tl, mail.IsFromTL)
                                    .Set(MailTable.Columns.is_text_body_only, mail.TextBodyOnly)
                                    .Set(MailTable.Columns.spam, 0)
                                    .Set(MailTable.Columns.mime_message_id, mail.MimeMessageId)
                                    .Set(MailTable.Columns.mime_in_reply_to, mail.MimeReplyToId)
                                    .Set(MailTable.Columns.chain_id, mail.ChainId);

                                db.ExecuteNonQuery(update);
                                idMail = messageId;
                            }

                            #endregion

                            if (saveAttachments &&
                                mail.Attachments != null &&
                                mail.Attachments.Count > 0)
                            {
                                usedQuota = MarkAttachmetsNeedRemove(db, mailbox.TenantId,
                                                                          Exp.Eq(AttachmentTable.Columns.id_mail, idMail));
                                SaveAttachments(db, mailbox.TenantId, idMail, mail.Attachments);
                            }

                            if (mail.FromEmail.Length != 0)
                            {
                                db.ExecuteNonQuery(
                                    new SqlDelete(TagMailTable.name)
                                        .Where(TagMailTable.Columns.id_mail, idMail));

                                var tagList = new List<int>();

                                if (mail.TagIds != null)
                                {
                                    tagList.AddRange(mail.TagIds);
                                }

                                var tagAddressesTagIds = db.ExecuteList(
                                    new SqlQuery(TagAddressTable.name)
                                        .Distinct()
                                        .Select(TagAddressTable.Columns.id_tag)
                                        .Where(TagAddressTable.Columns.address, mail.FromEmail)
                                        .Where(Exp.In(TagAddressTable.Columns.id_tag,
                                                      new SqlQuery(TagTable.name)
                                                          .Select(TagTable.Columns.id)
                                                          .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId)))))
                                                              .ConvertAll(r => Convert.ToInt32(r[0]));

                                tagAddressesTagIds.ForEach(tagId =>
                                    {
                                        if (!tagList.Contains(tagId))
                                            tagList.Add(tagId);
                                    });

                                if (tagList.Any())
                                {
                                    SetMessageTags(db, mailbox.TenantId, mailbox.UserId, idMail, tagList);
                                }
                            }

                            UpdateMessagesChains(db, mailbox, mail.MimeMessageId, mail.ChainId, folder);

                            tx.Commit();
                        }

                        iAttempt = max_attempts; // The transaction was succeded
                    }
                    catch (DbException ex)
                    {
                        if (!ex.Message.StartsWith("Deadlock found when trying to get lock; try restarting transaction"))
                            throw;

                        // Need to restart the transaction
                        iAttempt++;
                        if (iAttempt >= max_attempts)
                            throw;

                        _log.Warn("MailSave -> [!!!DEADLOCK!!!] Restarting transaction at {0}(of {1}) attempt",
                                  iAttempt, max_attempts);
                    }
                } while (iAttempt < max_attempts);
            }

            if (usedQuota > 0)
                QuotaUsedDelete(mailbox.TenantId, usedQuota);

            _log.Debug("MailSave() tenant='{0}', user_id='{1}', email='{2}', from='{3}', id_mail='{4}'",
                mailbox.TenantId, mailbox.UserId, mailbox.EMail, mail.From, idMail);

            return idMail;
        }

        public void UpdateMessageChainId(MailBox mailbox, long messageId, int folder, string oldChainId, string newChainId)
        {
            using (var db = GetDb())
            {
                if (mailbox == null)
                    throw new ArgumentNullException("mailbox");

                if (messageId == 0)
                    throw new ArgumentException("message_id");

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    if (!oldChainId.Equals(newChainId))
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.name)
                                .Set(MailTable.Columns.chain_id, newChainId)
                                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                                .Where(MailTable.Columns.id, messageId));

                        UpdateChain(db, oldChainId, folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);
                    }

                    UpdateChain(db, newChainId, folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);

                    tx.Commit();
                }
            }
        }

        private List<object[]> GetInfoOnExistingMessages(int mailboxId, string md5, string mimeMessageId, string[] columns)
        {
            if ((string.IsNullOrEmpty(md5) || md5.Equals(Defines.MD5_EMPTY)) && string.IsNullOrEmpty(mimeMessageId))
            {
                return new List<object[]>();
            }

            var messagesInfo = (!string.IsNullOrEmpty(mimeMessageId)
                                     ? GetMessagesInfoByMimeMessageId(
                                         mailboxId,
                                         mimeMessageId,
                                         columns)
                                     : GetMessagesInfoByMd5(
                                         mailboxId,
                                         md5,
                                         columns));
            return messagesInfo;
        }

        public int MailReceive(MailBox mailbox, Message message, int folderId, string uidl, string md5, bool hasParseError,
                               bool unread, int[] tagsIds, out MailMessageItem messageItem)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox");

            if (message == null)
                throw new ArgumentNullException("message");

            _log.Info("MailReceive() tenant='{0}', user_id='{1}', folder_id='{2}', uidl='{3}'",
                mailbox.TenantId, mailbox.UserId, folderId, uidl);

            var skipSaveMessage = false;
            var mailId = 0;

            if (mailbox.Imap)
            {
                // checks, whether message is already stored
                var messagesInfo = GetInfoOnExistingMessages(mailbox.MailBoxId, md5, message.MessageId, new[]
                    {
                        MailTable.Columns.id, MailTable.Columns.folder_restore, MailTable.Columns.uidl, MailTable.Columns.is_removed
                    })
                    .ConvertAll(i => new
                        {
                            id = Convert.ToInt32(i[0]),
                            folder_orig = Convert.ToInt32(i[1]),
                            uidl = Convert.ToString(i[2]),
                            is_removed = Convert.ToBoolean(i[3])
                        });

                if (messagesInfo.Any())
                {
                    var idList = messagesInfo.Where(m => !m.is_removed).Select(m => m.id).ToList();

                    if (idList.Any())
                    {
                        if (tagsIds != null) // Add new tags to existing messages
                        {
                            foreach (var tagId in tagsIds)
                                SetMessagesTag(mailbox.TenantId, mailbox.UserId, tagId, idList);
                        }

                        var fromThisMailBox =
                            message.From.Email.ToLowerInvariant().Equals(mailbox.Account.ToLowerInvariant());

                        var toThisMailBox =
                            message.To.Select(addr => addr.Email.ToLower()).Contains(mailbox.EMail.Address.ToLower());

                        if (fromThisMailBox && toThisMailBox)
                        {
                            if (messagesInfo.Exists(m => m.folder_orig == folderId))
                            {
                                var messageForUpdateInfo =
                                    messagesInfo.Find(i => (folderId == i.folder_orig) && (string.IsNullOrEmpty(i.uidl)));

                                if (null != messageForUpdateInfo) // Found message clone in the Sent folder
                                {
                                    mailId = messageForUpdateInfo.id;

                                    if (!messageForUpdateInfo.is_removed)
                                        UpdateMessageUidl(mailbox.TenantId, mailbox.UserId, mailId, uidl);

                                    _log.Debug("Message already exists: mailId={0}. Outbox clone", mailId);
                                }
                                else
                                {
                                    mailId = messagesInfo.First(m => m.folder_orig == folderId).id;
                                }

                                skipSaveMessage = true;
                            }
                        }
                        else
                        {
                            skipSaveMessage = true;
                            mailId = messagesInfo.First().id;

                            _log.Debug("Message already exists and it was removed from portal.");
                        }
                    }
                    else
                    {
                        skipSaveMessage = true;
                        mailId = messagesInfo.First().id;

                        _log.Debug("Message already exists and it was removed from portal.");
                    }
                }
            }
            else
            {
                var fromThisMailBox = message.From.Email.ToLowerInvariant().Equals(mailbox.Account.ToLowerInvariant());

                if (fromThisMailBox)
                {
                    var toThisMailBox =
                        message.To.Select(addr => addr.Email.ToLower()).Contains(mailbox.EMail.Address.ToLower());

                    var messagesInfo = GetInfoOnExistingMessages(mailbox.MailBoxId, md5, message.MessageId, new[]
                        {
                            MailTable.Columns.id, MailTable.Columns.folder_restore, MailTable.Columns.uidl, MailTable.Columns.is_removed
                        })
                        .ConvertAll(i => new
                            {
                                id = Convert.ToInt32(i[0]),
                                folder_orig = Convert.ToInt32(i[1]),
                                uidl = Convert.ToString(i[2]),
                                is_removed = Convert.ToBoolean(i[3])
                            });

                    if (!toThisMailBox || messagesInfo.Count > 1)
                    {
                        var messageForUpdateInfo = messagesInfo.Find(i => (MailFolder.Ids.sent == i.folder_orig) && (string.IsNullOrEmpty(i.uidl)));

                        if (null != messageForUpdateInfo) // Found message clone in the Sent folder
                        {
                            skipSaveMessage = true;
                            mailId = messageForUpdateInfo.id;

                            if (!messageForUpdateInfo.is_removed)
                                UpdateMessageUidl(mailbox.TenantId, mailbox.UserId, mailId, uidl);

                            _log.Debug("Message already exists: mailId={0}. Outbox clone", mailId);
                        }
                    }
                }
            }

            messageItem = null;

            if (!skipSaveMessage)
            {
                messageItem = ProcessMail(message, mailbox, folderId, hasParseError);

                tagsIds = AddTagIdsForSelfSendedMessages(mailbox, tagsIds, messageItem);

                if (null != tagsIds)
                {
                    if (null != messageItem.TagIds)
                        messageItem.TagIds.AddRange(tagsIds);
                    else
                        messageItem.TagIds = new ItemList<int>(tagsIds);
                }


                try
                {
                    _log.Debug("StoreAttachments(Account:{0})", mailbox.EMail);
                    messageItem.StreamId = CreateNewStreamId();
                    if (messageItem.Attachments.Any())
                    {
                        var index = 0;
                        messageItem.Attachments.ForEach(att => att.fileNumber = ++index);
                        StoreAttachments(mailbox.TenantId, mailbox.UserId, messageItem.Attachments, messageItem.StreamId);
                    }

                    _log.Debug("StoreMailBody(Account:{0})", mailbox.EMail);

                    StoreMailBody(mailbox.TenantId, mailbox.UserId, messageItem);

                    if (SaveOriginalMessage)
                        StoreMailEml(mailbox.TenantId, mailbox.UserId, messageItem.StreamId, message);

                    _log.Debug("MailSave(Account:{0})", mailbox.EMail);

                    messageItem.IsNew = unread;

                    var folderRestore = MailFolder.Ids.spam == folderId || MailFolder.Ids.trash == folderId
                                             ? MailFolder.Ids.inbox
                                             : folderId;
                    mailId = MailSave(mailbox, messageItem, 0, folderId, folderRestore, uidl, md5, true);
                    messageItem.Id = mailId;

                    _log.Info("MailSave(Account:{0}) returned mailId = {1}\r\n", mailbox.EMail, mailId);
                }
                catch (Exception)
                {
                    //Trying to delete all attachments and mailbody
                    var storage = MailDataStore.GetDataStore(mailbox.TenantId);
                    try
                    {
                        storage.DeleteDirectory(string.Empty, MailStoragePathCombiner.GetMessageDirectory(mailbox.UserId, messageItem.StreamId));
                    }
                    catch (Exception ex)
                    {
                        _log.Debug("Problems with mail_directory deleting. Account: {0}. Folder: {1}/{2}/{3}. Exception: {4}", mailbox.EMail, mailbox.TenantId, mailbox.UserId, messageItem.StreamId, ex.ToString());
                    }

                    _log.Debug("Problem with mail proccessing(Account:{0}). Body and attachment was deleted", mailbox.EMail);
                    throw;
                }

            }

            bool isMailboxRemoved;
            bool isMailboxDeactivated;
            DateTime beginDate;

            // checks mailbox state to delete message 
            // If account was removed during saving process then message retrieve will stop.
            GetMailBoxState(mailbox.MailBoxId, out isMailboxRemoved, out isMailboxDeactivated, out beginDate);

            if (mailbox.BeginDate != beginDate)
            {
                mailbox.BeginDateChanged = true;
                mailbox.BeginDate = beginDate;
            }

            if (isMailboxRemoved)
            {
                using (var db = GetDb())
                {
                    DeleteMessages(db,
                                   mailbox.TenantId,
                                   mailbox.UserId,
                                   new List<object[]>
                                       {
                                           new object[] {mailId, folderId, 1}
                                       },
                                   true);
                }

                throw new MailBoxOutException(MailBoxOutException.Types.Removed,
                                              string.Format("MailBox with id={0} is removed.\r\n", mailbox.MailBoxId));
            }

            if (isMailboxDeactivated)
                throw new MailBoxOutException(MailBoxOutException.Types.Deactivated,
                                              string.Format("MailBox with id={0} is deactivated.\r\n",
                                                            mailbox.MailBoxId));

            return mailId;
        }

        public void AddRelationshipEventForLinkedAccounts(MailBox mailbox, MailMessageItem messageItem, ILogger log)
        {
            try
            {
                messageItem.LinkedCrmEntityIds = GetLinkedCrmEntitiesId(messageItem.ChainId, mailbox.MailBoxId,
                                                                         mailbox.TenantId);

                if(!messageItem.LinkedCrmEntityIds.Any()) return;

                var crmDal = new CrmHistoryDal(mailbox.TenantId, mailbox.UserId);
                crmDal.AddRelationshipEvents(messageItem);
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.WarnException(
                        String.Format("Problem with adding history event to CRM. mailId={0}", messageItem.Id), ex);
            }
        }

        /// <summary>
        /// Creates Rfc 2822 3.6.4 message-id. Syntax: '&lt;' id-left '@' id-right '&gt;'.
        /// </summary>
        public static string CreateMessageId()
        {
            return "<" + Guid.NewGuid().ToString().Replace("-", "").Substring(16) + "@" +
                   Guid.NewGuid().ToString().Replace("-", "").Substring(16) + ">";
        }

        /// <summary>
        /// Updates mail_mail chain's references and mail_chains records when new message was saved
        /// </summary>
        /// <param name="db">Db manager</param>
        /// <param name="mailbox">New message mailbox</param>
        /// <param name="mimeMessageId">New message mime message id</param>
        /// <param name="chainId">New message chain id</param>
        /// <param name="folder">New message folder id</param>
        /// <returns>Nothing</returns>
        /// <short>Updates mail_mail chain's references and mail_chains records when new message was saved</short>
        /// <category>Mail</category>
        public void UpdateMessagesChains(IDbManager db, MailBox mailbox, string mimeMessageId, string chainId, int folder)
        {
            var chainsForUpdate = new[] { new { id = chainId, folder } };

            // if mime_message_id == chain_id - message is first in chain, because it isn't reply
            if (!string.IsNullOrEmpty(mimeMessageId) && mimeMessageId != chainId)
            {
                // Get chains which has our newly saved message as root.
                var chains = db.ExecuteList(
                    new SqlQuery(ChainTable.name)
                        .Select(ChainTable.Columns.id, ChainTable.Columns.folder)
                        .Where(ChainTable.Columns.id, mimeMessageId)
                        .Where(ChainTable.Columns.id_mailbox, mailbox.MailBoxId)
                        .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    ).Select(x => new { id = (string)x[0], folder = Convert.ToInt32(x[1]) }).ToArray();

                if (chains.Any())
                {
                    db.ExecuteNonQuery(
                        new SqlUpdate(MailTable.name)
                            .Set(MailTable.Columns.chain_id, chainId)
                            .Where(MailTable.Columns.chain_id, mimeMessageId)
                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.is_removed, false));

                    chainsForUpdate = chains.Concat(chainsForUpdate).ToArray();

                    var newChainsForUpdate = db.ExecuteList(
                        new SqlQuery(MailTable.name)
                            .Select(MailTable.Columns.folder)
                            .Where(MailTable.Columns.chain_id, chainId)
                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.is_removed, false)
                            .Distinct())
                            .ConvertAll(x => new
                            {
                                id = chainId,
                                folder = Convert.ToInt32(x[0]),
                            });

                    chainsForUpdate = chainsForUpdate.Concat(newChainsForUpdate).ToArray();
                }
            }

            foreach (var c in chainsForUpdate.Distinct())
            {
                UpdateChain(db, c.id, c.folder, mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId);
            }
        }

        public string DetectChainId(MailBox mailbox, MailMessageItem messageItem)
        {
            var chainId = messageItem.MimeMessageId; //Chain id is equal to root conversataions message - MimeMessageId
            if (!string.IsNullOrEmpty(messageItem.MimeMessageId) && !string.IsNullOrEmpty(messageItem.MimeReplyToId))
            {
                chainId = messageItem.MimeReplyToId;
                try
                {
                    using (var db = GetDb())
                    {
                        var detectChainByInReplyToQuery = new SqlQuery(MailTable.name)
                            .Select(MailTable.Columns.chain_id)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.is_removed, false)
                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                            .Where(Exp.Eq(MailTable.Columns.mime_message_id, messageItem.MimeReplyToId))
                            .Distinct();

                        var chainIdsDetectedWithInReplyTo = db.ExecuteList(detectChainByInReplyToQuery);

                        if (chainIdsDetectedWithInReplyTo.Any())
                        {
                            chainId = chainIdsDetectedWithInReplyTo.Select(r => Convert.ToString(r[0])).First();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("DetectChainId() params tenant={0}, user_id='{1}', mailbox_id={2}, mime_message_id='{3}' Exception:\r\n{4}",
                        mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, messageItem.MimeMessageId, ex.ToString());
                }
            }

            _log.Debug("DetectChainId() tenant='{0}', user_id='{1}', mailbox_id='{2}', mime_message_id='{3}' Result: {4}",
                        mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, messageItem.MimeMessageId, chainId);

            return chainId;
        }

        public MailMessageItem GetMailInfo(int tenant, string user, int messageId, bool loadImages, bool loadBody, bool unremoved = true)
        {
            using (var db = GetDb())
            {
                var messageItem = GetMailInfo(db, tenant, user, messageId, loadImages, loadBody, unremoved);
                return messageItem;
            }
        }

        public MailMessageItem GetMailInfo(IDbManager db, int tenant, string user, int messageId, bool loadImages, bool loadBody, bool unremoved = true)
        {
            var selectQuery = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.address,
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
                        MailTable.Columns.is_from_tl,
                        MailTable.Columns.folder,
                        MailTable.Columns.unread,
                        MailTable.Columns.introduction,
                        MailTable.Columns.is_text_body_only,
                        MailTable.Columns.id_mailbox,
                        MailTable.Columns.folder_restore,
                        MailTable.Columns.has_parse_error,
                        MailTable.Columns.mime_message_id,
                        MailTable.Columns.mime_in_reply_to
                )
                .Where(GetUserWhere(user, tenant))                
                .Where(MailTable.Columns.id, messageId);

            if (unremoved)
                selectQuery
                    .Where(MailTable.Columns.is_removed, false);

            var mailDbInfo = db.ExecuteList(selectQuery)
                               .ConvertAll(x => new
                                   {
                                       address = (string) x[0],
                                       chain_id = (string) x[1],
                                       chain_date = Convert.ToDateTime(x[2]),
                                       importance = Convert.ToBoolean(x[3]),
                                       datesent = Convert.ToDateTime(x[4]),
                                       from = (string) x[5],
                                       to = (string) x[6],
                                       cc = (string) x[7],
                                       bcc = (string) x[8],
                                       reply_to = (string) x[9],
                                       stream = (string) x[10],
                                       isanswered = Convert.ToBoolean(x[11]),
                                       isforwarded = Convert.ToBoolean(x[12]),
                                       subject = (string) x[13],
                                       hasAttachments = Convert.ToBoolean(x[14]),
                                       size = Convert.ToInt64(x[15]),
                                       is_from_tl = Convert.ToBoolean(x[16]),
                                       folder = Convert.ToInt32(x[17]),
                                       unread = Convert.ToBoolean(x[18]),
                                       introduction = (string) x[19],
                                       is_text_body_only = Convert.ToBoolean(x[20]),
                                       id_mailbox = Convert.ToInt32(x[21]),
                                       folder_restore = Convert.ToInt32(x[22]),
                                       has_parse_error = Convert.ToBoolean(x[23]),
                                       mime_message_id = (string) x[24],
                                       mime_in_reply_to = (string) x[25]
                                   })
                               .SingleOrDefault();

            if (mailDbInfo == null)
                return null;

            var tags = new ItemList<int>(
                db.ExecuteList(
                    new SqlQuery(TagMailTable.name)
                        .Select(TagMailTable.Columns.id_tag)
                        .Where(TagMailTable.Columns.id_mail, messageId))
                  .ConvertAll(x => (int)x[0]));

            var now = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant).TimeZone, DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant).TimeZone, mailDbInfo.datesent);
            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            var item = new MailMessageItem
                {
                    Id = messageId,
                    ChainId = mailDbInfo.chain_id,
                    ChainDate = mailDbInfo.chain_date,
                    Attachments = null,
                    Address = mailDbInfo.address,
                    Bcc = mailDbInfo.bcc,
                    Cc = mailDbInfo.cc,
                    Date = date,
                    From = mailDbInfo.@from,
                    HasAttachments = mailDbInfo.hasAttachments,
                    Important = mailDbInfo.importance,
                    IsAnswered = mailDbInfo.isanswered,
                    IsForwarded = mailDbInfo.isforwarded,
                    IsNew = false,
                    TagIds = tags,
                    ReplyTo = mailDbInfo.reply_to,
                    Size = mailDbInfo.size,
                    Subject = mailDbInfo.subject,
                    To = mailDbInfo.to,
                    StreamId = mailDbInfo.stream,
                    IsFromTL = mailDbInfo.is_from_tl,
                    Folder = mailDbInfo.folder,
                    WasNew = mailDbInfo.unread,
                    IsToday = isToday,
                    IsYesterday = isYesterday,
                    Introduction = mailDbInfo.introduction,
                    TextBodyOnly = mailDbInfo.is_text_body_only,
                    MailboxId = mailDbInfo.id_mailbox,
                    RestoreFolderId = mailDbInfo.folder_restore,
                    HasParseError = mailDbInfo.has_parse_error,
                    MimeMessageId = mailDbInfo.mime_message_id,
                    MimeReplyToId = mailDbInfo.mime_in_reply_to
                };

            //Reassemble paths
            if (loadBody)
            {
                var htmlBody = "";

                if (!item.HasParseError)
                {
                    var dataStore = MailDataStore.GetDataStore(tenant);

                    var key =  MailStoragePathCombiner.GetBodyKey(user, item.StreamId);

                    try
                    {
                        using (var s = dataStore.GetReadStream(string.Empty, key))
                        {
                            htmlBody = Encoding.UTF8.GetString(s.GetCorrectBuffer());
                        }

                        if (item.Folder != MailFolder.Ids.drafts && !item.From.Equals(MAIL_DAEMON_EMAIL))
                        {
                            bool imagesAreBlocked;
                            htmlBody = HtmlSanitizer.Sanitize(htmlBody, loadImages, out imagesAreBlocked);
                            item.ContentIsBlocked = imagesAreBlocked;
                        }
                    }
                    catch (Exception ex)
                    {
                        item.IsBodyCorrupted = true;
                        htmlBody = "";
                        _log.Error(ex, "Load stored body error: tenant={0} user=\"{1}\" messageId={2} key=\"{3}\"",
                                   tenant, user, messageId, key);
                    }
                }

                item.HtmlBody = htmlBody;

                if (string.IsNullOrEmpty(mailDbInfo.introduction) && !string.IsNullOrEmpty(item.HtmlBody))
                {
                    // if introduction wasn't saved, it will be save.
                    var introduction = MailMessageItem.GetIntroduction(htmlBody);

                    if (!string.IsNullOrEmpty(introduction))
                    {
                        item.Introduction = introduction;

                        db.ExecuteNonQuery(new SqlUpdate(MailTable.name)
                            .Set(MailTable.Columns.introduction, item.Introduction)
                            .Where(GetUserWhere(user, tenant))
                            .Where(MailTable.Columns.id, messageId));
                    }
                }
            }

            var attachments = GetMessageAttachments(db, tenant, user, messageId);

            item.Attachments = attachments.Count != 0 ? attachments : new List<MailAttachment>();

            Address from;
            if (Parser.TryParseAddress(mailDbInfo.@from, out from))
            {
                var ids = GetCrmContactsId(tenant, user, from.Email);
                if (ids.Count > 0)
                {
                    item.IsFromCRM = true;
                    item.ParticipantsCrmContactsId = ids;
                }
            }

            return item;
        }

        public List<MailAttachment> GetMessageAttachments(int tenant, string user, int messageId)
        {
            using (var db = GetDb())
            {
                var mailDbInfo =
                    db.ExecuteList(
                        new SqlQuery(MailTable.name)
                            .Select(
                                MailTable.Columns.stream,
                                MailTable.Columns.attach_count
                            )
                            .Where(GetUserWhere(user, tenant))
                            .Where(MailTable.Columns.id, messageId))
                      .ConvertAll(x => new
                          {
                              stream = x[0].ToString(),
                              attachments_count = Convert.ToInt32(x[1])
                          })
                          .FirstOrDefault();

                if (mailDbInfo != null && mailDbInfo.attachments_count > 0)
                {
                    return GetMessageAttachments(db, tenant, user, messageId);
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
                        AttachmentTable.Columns.content_id.Prefix(AttachmentTable.name),
                        AttachmentTable.Columns.id_mailbox.Prefix(AttachmentTable.name));
        }

        private List<MailAttachment> GetMessageAttachments(IDbManager db, int tenant, string user, int messageId)
        {
            var attachmentsSelectQuery = GetAttachmentsSelectQuery()
                .Where(MailTable.Columns.id.Prefix(MailTable.name), messageId)
                .Where(AttachmentTable.Columns.need_remove.Prefix(AttachmentTable.name), false)
                .Where(AttachmentTable.Columns.content_id, Exp.Empty)
                .Where(GetUserWhere(user, tenant, MailTable.name));

            var attachments =
                db.ExecuteList(attachmentsSelectQuery)
                  .ConvertAll(ToMailItemAttachment);

            return attachments;
        }

        public List<MailMessageItem> GetMailsFiltered(int tenant, string user, MailFilter filter, int page,
                                                      int pageSize, out long totalMessagesCount)
        {
            return GetSingleMailsFiltered(tenant, user, filter, page, pageSize, out totalMessagesCount);
        }

        public List<MailMessageItem> GetSingleMailsFiltered(int tenant, string user, MailFilter filter, int page,
                                                            int pageSize, out long totalMessagesCount)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            if (page <= 0)
                throw new ArgumentOutOfRangeException("page", "Can't be less than one.");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException("pageSize", "Can't be less than one.");

            var concatTagIds =
                String.Format(
                    "(SELECT CAST(group_concat(tm.{0} ORDER BY tm.{3} SEPARATOR ',') AS CHAR) from {1} as tm WHERE tm.{2} = `id`) tagIds",
                    TagMailTable.Columns.id_tag, TagMailTable.name, TagMailTable.Columns.id_mail, TagMailTable.Columns.time_created);


            using (var db = GetDb())
            {
                var filtered = new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id, MailTable.Columns.chain_id)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(user, tenant))
                    .ApplyFilter(filter);

                // Filter and sort all existing messages and get their ids and chain_ids
                var filteredIds = db.ExecuteList(filtered)
                                     .ConvertAll(r => new { id = Convert.ToInt32(r[0]), chain_id = (string)r[1] });

                var idsSet = filteredIds.Select(m => m.id).ToList();

                if (!idsSet.Any())
                {
                    totalMessagesCount = 0;
                    return new List<MailMessageItem>();
                }

                const string select_chain_length = "1";

                totalMessagesCount = idsSet.Count();
                page = Math.Min(page, (int)Math.Ceiling((double)totalMessagesCount / pageSize));

                var queryMessages = new SqlQuery(MailTable.name + " as outer_mail")
                    .Select(MailTable.Columns.id, MailTable.Columns.from, MailTable.Columns.to,
                            MailTable.Columns.reply, MailTable.Columns.subject, MailTable.Columns.importance,
                            "1", MailTable.Columns.date_sent, MailTable.Columns.size,
                            MailTable.Columns.attach_count, MailTable.Columns.unread, MailTable.Columns.is_answered,
                            MailTable.Columns.is_forwarded, MailTable.Columns.is_from_crm, MailTable.Columns.is_from_tl,
                            concatTagIds, MailTable.Columns.folder_restore, MailTable.Columns.chain_id, select_chain_length,
                            MailTable.Columns.folder)
                    .Where(Exp.In(MailTable.Columns.id, idsSet.ToArray()))
                    .ApplySorting(filter)
                    .SetFirstResult((page - 1) * pageSize)
                    .SetMaxResults(pageSize);

                var list = db.ExecuteList(queryMessages)
                             .ConvertAll(r =>
                                         ConvertToMailMessageItem(r, tenant));

                return list;
            }
        }

        public long GetNextMessageId(int tenant, string user, int messageId, MailFilter filter)
        {
            using (var db = GetDb())
            {
                var dateSent = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.date_sent)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.id, messageId));

                var sortOrder = filter.SortOrder == "ascending";

                return db.ExecuteScalar<long>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.id)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(user, tenant))
                    .ApplyFilter(filter)
                    .Where(sortOrder ? Exp.Ge(MailTable.Columns.date_sent, dateSent) : Exp.Le(MailTable.Columns.date_sent, dateSent))
                    .SetFirstResult(1)
                    .SetMaxResults(1)
                    .OrderBy(MailTable.Columns.date_sent, sortOrder));
            }
        }

        public List<MailMessageItem> GetConversationMessages(int tenant, string user, int messageId,
                                                             bool loadAllContent, bool markRead = false)
        {
            var mailDbInfoQuery = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id, MailTable.Columns.id_mailbox, MailTable.Columns.folder)
                .Where(MailTable.Columns.is_removed, false)
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.id, messageId);

            using (var db = GetDb())
            {
                var messageInfo = db.ExecuteList(mailDbInfoQuery)
                                     .ConvertAll(
                                         r =>
                                         new
                                             {
                                                 chain_id = (string)r[0],
                                                 mailbox_id = Convert.ToInt32(r[1]),
                                                 folder = Convert.ToInt32(r[2])
                                             })
                                     .FirstOrDefault();

                if (messageInfo == null) throw new ArgumentException("Message Id not found");

                var searchFolders = new List<int>();

                if (messageInfo.folder == MailFolder.Ids.inbox || messageInfo.folder == MailFolder.Ids.sent)
                    searchFolders.AddRange(new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
                else
                    searchFolders.Add(messageInfo.folder);

                var getMessagesIdsQuery = GetQueryForChainMessagesSelection(messageInfo.mailbox_id, messageInfo.chain_id, searchFolders)
                                       .OrderBy(MailTable.Columns.date_sent, true);


                var queryResult = db.ExecuteList(getMessagesIdsQuery);

                var mailInfoList =
                    queryResult.Select(
                        (item, i) =>
                        GetMailInfo(db, tenant, user, Convert.ToInt32(item[0]), false,
                                    loadAllContent || (i == queryResult.Count - 1)))
                               .Where(mailInfo => mailInfo != null)
                               .ToList();

                if(!markRead)
                    return mailInfoList;

                var unreadMessages = mailInfoList.Where(message => message.WasNew).ToList();
                if (!unreadMessages.Any())
                    return mailInfoList;

                var unreadMessagesCountByFolder = new Dictionary<int, int>();

                foreach (var message in unreadMessages)
                {
                    if (unreadMessagesCountByFolder.ContainsKey(message.Folder))
                        unreadMessagesCountByFolder[message.Folder] += 1;
                    else
                        unreadMessagesCountByFolder.Add(message.Folder, 1);
                }

                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(
                        new SqlUpdate(MailTable.name)
                            .Where(Exp.In(MailTable.Columns.id, unreadMessages.Select(x => (object)x.Id).ToArray()))
                            .Where(GetUserWhere(user, tenant))
                            .Set(MailTable.Columns.unread, false));

                    foreach (var keyPair in unreadMessagesCountByFolder)
                    {
                        ChangeFolderCounters(db, tenant, user, keyPair.Key, keyPair.Value * (-1), 0, -1, 0);

                        db.ExecuteNonQuery(
                            new SqlUpdate(ChainTable.name)
                                .Set(ChainTable.Columns.unread, false)
                                .Where(GetUserWhere(user, tenant))
                                .Where(ChainTable.Columns.id, messageInfo.chain_id)
                                .Where(ChainTable.Columns.id_mailbox, messageInfo.mailbox_id)
                                .Where(ChainTable.Columns.folder, keyPair.Key));
                    }

                    tx.Commit();
                }

                return mailInfoList;
            }
        }

        public void SetMessageFolderRestore(int tenant, string user, int toFolder, int messageId)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Set(MailTable.Columns.folder_restore, toFolder)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.id, messageId));
            }
        }

        public void UpdateMessageUidl(int tenant, string user, int messageId, string newUidl)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.id, messageId)
                        .Set(MailTable.Columns.uidl, newUidl));
            }
        }

        #endregion

        #region private methods

        private void SetMessagesFolder(DbManager db, int tenant, string user, List<object[]> mailsInfo, int toFolder)
        {
            if (!mailsInfo.Any())
                return;

            var uniqueChainInfo = mailsInfo
                .ConvertAll(x => new
                    {
                        folder = Convert.ToInt32(x[2]),
                        chain_id = (string)x[3],
                        id_mailbox = Convert.ToInt32(x[4])
                    })
                .Distinct();

            var prevInfo = mailsInfo.ConvertAll(x => new
                {
                    id = Convert.ToInt32(x[0]),
                    unread = Convert.ToBoolean(x[1]),
                    folder = Convert.ToInt32(x[2]),
                    chain_id = (string)x[3],
                    id_mailbox = Convert.ToInt32(x[4])
                });

            var ids = mailsInfo.ConvertAll(x => Convert.ToInt32(x[0]));

            var query = new SqlUpdate(MailTable.name)
                .Set(MailTable.Columns.folder, toFolder)
                .Where(GetUserWhere(user, tenant))
                .Where(Exp.In(MailTable.Columns.id, ids));

            db.ExecuteNonQuery(query);

            foreach (var info in uniqueChainInfo)
                UpdateChain(db, info.chain_id, info.folder, info.id_mailbox, tenant, user);

            var unreadMessagesCountCollection = new Dictionary<int, int>();
            var totalMessagesCountCollection = new Dictionary<int, int>();

            foreach (var info in prevInfo)
            {
                if (totalMessagesCountCollection.ContainsKey(info.folder))
                    totalMessagesCountCollection[info.folder] += 1;
                else
                    totalMessagesCountCollection.Add(info.folder, 1);

                if (!info.unread) continue;
                if (unreadMessagesCountCollection.ContainsKey(info.folder))
                    unreadMessagesCountCollection[info.folder] += 1;
                else
                    unreadMessagesCountCollection.Add(info.folder, 1);
            }

            UpdateChainFields(db, tenant, user, ids);

            var movedTotalUnreadCount = 0;
            var movedTotalCount = 0;

            foreach (var keyPair in totalMessagesCountCollection)
            {
                var sourceFolder = keyPair.Key;
                var totalMove = keyPair.Value;
                int unreadMove;
                unreadMessagesCountCollection.TryGetValue(sourceFolder, out unreadMove);
                ChangeFolderCounters(db, tenant, user, sourceFolder, unreadMove != 0 ? unreadMove * (-1) : 0,
                                     totalMove * (-1), false);
                movedTotalUnreadCount += unreadMove;
                movedTotalCount += totalMove;
            }

            if (movedTotalUnreadCount != 0 || movedTotalCount != 0)
            {
                ChangeFolderCounters(db, tenant, user, toFolder,
                                     movedTotalUnreadCount, movedTotalCount, false);
            }
        }

        private void RestoreMessages(DbManager db, int tenant, string user, List<object[]> mailsInfo)
        {
            if (!mailsInfo.Any())
                return;

            var uniqueChainInfo = mailsInfo
                .ConvertAll(x => new
                {
                    folder = Convert.ToInt32(x[2]),
                    chain_id = (string)x[4],
                    id_mailbox = Convert.ToInt32(x[5])
                })
                .Distinct();

            var prevInfo = mailsInfo.ConvertAll(x => new
            {
                id = Convert.ToInt32(x[0]),
                unread = Convert.ToBoolean(x[1]),
                folder = Convert.ToInt32(x[2]),
                folder_restore = Convert.ToInt32(x[3]),
                chain_id = (string)x[4],
                id_mailbox = Convert.ToInt32(x[5])
            });

            var ids = mailsInfo.ConvertAll(x => Convert.ToInt32(x[0]));

            var updateQuery =
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.folder + " = " + MailTable.Columns.folder_restore)
                    .Where(MailTable.Columns.is_removed, false)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.id, ids));

            db.ExecuteNonQuery(updateQuery);

            // Update chains in old folder
            foreach (var info in uniqueChainInfo)
                UpdateChain(db, info.chain_id, info.folder, info.id_mailbox, tenant, user);

            var unreadMessagesCountCollection = new Dictionary<int, int>();
            var totalMessagesCountCollection = new Dictionary<int, int>();

            foreach (var info in prevInfo)
            {
                if (totalMessagesCountCollection.ContainsKey(info.folder_restore))
                    totalMessagesCountCollection[info.folder_restore] += 1;
                else
                    totalMessagesCountCollection.Add(info.folder_restore, 1);

                if (!info.unread) continue;
                if (unreadMessagesCountCollection.ContainsKey(info.folder_restore))
                    unreadMessagesCountCollection[info.folder_restore] += 1;
                else
                    unreadMessagesCountCollection.Add(info.folder_restore, 1);
            }

            // Update chains in new restored folder
            UpdateChainFields(db, tenant, user, ids);

            var prevTotalUnreadCount = 0;
            var prevTotalCount = 0;

            foreach (var keyPair in totalMessagesCountCollection)
            {
                var folderRestore = keyPair.Key;
                var totalRestore = keyPair.Value;
                int unreadRestore;
                unreadMessagesCountCollection.TryGetValue(folderRestore, out unreadRestore);
                ChangeFolderCounters(db, tenant, user, folderRestore, unreadRestore, totalRestore, false);
                prevTotalUnreadCount -= unreadRestore;
                prevTotalCount -= totalRestore;
            }

            // Subtract the restored number of messages in the previous folder
            if (prevTotalUnreadCount != 0 || prevTotalCount != 0)
                ChangeFolderCounters(db, tenant, user, prevInfo[0].folder, prevTotalUnreadCount,
                                     prevTotalCount, false);
        }

        private long DeleteMessages(DbManager db, int tenant, string user,
                                   List<object[]> deleteInfo, bool reCountFolders)
        {
            if (!deleteInfo.Any())
                return 0;

            var messageFieldsInfo = deleteInfo
                .ConvertAll(r =>
                            new
                                {
                                    id = Convert.ToInt32(r[0]),
                                    folder = Convert.ToInt32(r[1]),
                                    unread = Convert.ToInt32(r[2])
                                });

            var messageIds = messageFieldsInfo.Select(m => m.id).ToArray();

            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.is_removed, true)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.id, messageIds)));

            var usedQuota = MarkAttachmetsNeedRemove(db, tenant, Exp.In(AttachmentTable.Columns.id_mail, messageIds));

            var affectedTags = db.ExecuteList(
                new SqlQuery(TagMailTable.name)
                    .Select(TagMailTable.Columns.id_tag)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(TagMailTable.Columns.id_mail, messageIds)))
                                  .ConvertAll(r => Convert.ToInt32(r[0]));

            db.ExecuteNonQuery(
                new SqlDelete(TagMailTable.name)
                    .Where(Exp.In(TagMailTable.Columns.id_mail, messageIds))
                    .Where(GetUserWhere(user, tenant)));

            UpdateTagsCount(db, tenant, user, affectedTags.Distinct());

            if (!reCountFolders)
            {
                var totalCollection = (from row in messageFieldsInfo
                                       group row by row.folder
                                           into g
                                           select new { id = Convert.ToInt32(g.Key), diff = -g.Count() })
                    .ToList();

                var unreadCollection = (from row in messageFieldsInfo.Where(m => m.unread == 1)
                                        group row by row.folder
                                            into g
                                            select new { id = Convert.ToInt32(g.Key), diff = -g.Count() })
                    .ToList();

                foreach (var folder in totalCollection)
                {
                    var unreadInFolder = unreadCollection
                        .FirstOrDefault(f => f.id == folder.id);

                    ChangeFolderCounters(db, tenant, user, folder.id,
                                         unreadInFolder != null ? unreadInFolder.diff : 0, folder.diff, false);
                }
            }
            else
            {
                RecalculateFolders(db, tenant, user, false);
            }

            UpdateChainFields(db, tenant, user, messageFieldsInfo.Select(m => Convert.ToInt32(m.id)).ToList());

            return usedQuota;
        }

        private List<object[]> GetMessagesInfoByMd5(int mailboxId, string md5, string[] columns)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(columns)
                        .Where(MailTable.Columns.md5, md5)
                        .Where(MailTable.Columns.id_mailbox, mailboxId));
            }
        }

        private List<object[]> GetMessagesInfoByMimeMessageId(int mailboxId, string mimeMessageId, string[] columns)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(columns)
                        .Where(MailTable.Columns.mime_message_id, mimeMessageId)
                        .Where(MailTable.Columns.id_mailbox, mailboxId));
            }
        }

        private int[] AddTagIdsForSelfSendedMessages(MailBox mailbox, int[] tagsIds, MailMessageItem messageItem)
        {
            try
            {
                const string mm_alias = "mm";
                const string mtm_alias = "mt";
                
                var fromAddressEmail = new MailAddress(messageItem.From).Address;
                var toAddressEmail = new MailAddress(messageItem.To).Address;
                if (fromAddressEmail == toAddressEmail)
                {
                    List<object[]> selfSendedMessageTagsIds;
                    using (var db = GetDb())
                    {
                        selfSendedMessageTagsIds = db.ExecuteList(
                            new SqlQuery(MailTable.name.Alias(mm_alias))
                                .InnerJoin(TagMailTable.name.Alias(mtm_alias),
                                           Exp.EqColumns(MailTable.Columns.id.Prefix(mm_alias), TagMailTable.Columns.id_mail.Prefix(mtm_alias)))
                                .Select(TagMailTable.Columns.id_tag.Prefix(mtm_alias))
                                .Where(MailTable.Columns.id_mailbox.Prefix(mm_alias), mailbox.MailBoxId)
                                .Where(MailTable.Columns.folder.Prefix(mm_alias), MailFolder.Ids.sent)
                                .Where(MailTable.Columns.mime_message_id.Prefix(mm_alias), messageItem.MimeMessageId)
                            );
                    }

                    if (selfSendedMessageTagsIds.Count > 0)
                    {
                        var temp = tagsIds != null ? tagsIds.ToList() : new List<int>();
                        temp.AddRange(selfSendedMessageTagsIds.Select(x => int.Parse(x[0].ToString())));
                        tagsIds = temp.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("AddTagIdsForSelfSendedMessages() Exception:\r\n{0}\r\n", ex.ToString());
            }
            return tagsIds;
        }

        private MailMessageItem ProcessMail(Message message, MailBox mailbox, int folder, bool hasParseError = false)
        {
            _log.Debug("Parse Message -> MailMessageItem");

            var messageItem = new MailMessageItem(message) { HasParseError = hasParseError };

            if (folder == MailFolder.Ids.inbox)
            {
                var ids = GetCrmContactsId(mailbox.TenantId, mailbox.UserId, messageItem.FromEmail);
                if (ids.Count > 0)
                {
                    messageItem.IsFromCRM = true;
                    messageItem.ParticipantsCrmContactsId = ids;
                }
            }

            _log.Debug("SetCrmTags()");

            SetCrmTags(messageItem, mailbox.TenantId, mailbox.UserId);

            _log.Debug("DetectChain()");

            if (string.IsNullOrEmpty(messageItem.MimeMessageId))
                messageItem.MimeMessageId = CreateMessageId();

            messageItem.ChainId = DetectChainId(mailbox, messageItem);

            return messageItem;
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

        private static SqlQuery GetQueryForChainMessagesSelection(int mailboxId, string chainId, List<int> searchFolders)
        {
            return new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.id)
                .Where(MailTable.Columns.id_mailbox, mailboxId)
                .Where(MailTable.Columns.chain_id, chainId)
                .Where(Exp.In(MailTable.Columns.folder, searchFolders.ToArray()))
                .Where(MailTable.Columns.is_removed, 0);
        }
        #endregion
    }
}
