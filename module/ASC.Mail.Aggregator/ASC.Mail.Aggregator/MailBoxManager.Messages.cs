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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.DataStorage;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Mail.Aggregator.Extension;
using ASC.Mail.Aggregator.Filter;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public methods

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
                        MailTable.Columns.Id, MailTable.Columns.Unread, MailTable.Columns.Folder, MailTable.Columns.ChainId, MailTable.Columns.MailboxId
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
                        MailTable.Columns.Id, MailTable.Columns.Unread, MailTable.Columns.Folder,
                        MailTable.Columns.FolderRestore, MailTable.Columns.ChainId,
                        MailTable.Columns.MailboxId
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
                        MailTable.Columns.Id, MailTable.Columns.Folder, MailTable.Columns.Unread
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
                    MailTable.Columns.Id,
                    MailTable.Columns.Folder,
                    MailTable.Columns.Unread,
                    MailTable.Columns.ChainId,
                    MailTable.Columns.MailboxId
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
                new SqlUpdate(MailTable.Name)
                    .Where(Exp.In(MailTable.Columns.Id, ids))
                    .Where(GetUserWhere(user, tenant))
                    .Set(MailTable.Columns.Unread, !isRead));

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
                    new SqlUpdate(MailTable.Name)
                        .Where(Exp.In(MailTable.Columns.Id, ids.ToArray()))
                        .Where(GetUserWhere(user, tenant))
                        .Set(MailTable.Columns.Importance, importance));

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
                    MailTable.Columns.MailboxId, MailTable.Columns.ChainId, MailTable.Columns.Folder
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
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Id)
                        .Where(MailTable.Columns.IsRemoved, false)
                        .Where(MailTable.Columns.Folder, folder)
                        .Where(GetUserWhere(user, tenant)))
                    .Select(x => Convert.ToInt32(x[0])).ToArray();

                if (ids.Any())
                {
                    using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        // Mark messages stored in the folder as is_removed.
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.Name)
                                .Set(MailTable.Columns.IsRemoved, true)
                                .Where(GetUserWhere(user, tenant))
                                .Where(MailTable.Columns.Folder, folder));

                        // Recalculate used quota
                        usedQuota = MarkAttachmetsNeedRemove(db, tenant, Exp.In(AttachmentTable.Columns.MailId, ids));

                        // delete tag/message cross references
                        db.ExecuteNonQuery(
                            new SqlDelete(TagMailTable.Name)
                                .Where(Exp.In(TagMailTable.Columns.MailId, ids))
                                .Where(GetUserWhere(user, tenant)));

                        // update tags counters
                        var tags = GetMailTags(db, tenant, user, Exp.Empty);
                        UpdateTagsCount(db, tenant, user, tags.Select(x => x.Id));

                        // delete chains stored in the folder
                        db.ExecuteNonQuery(new SqlDelete(ChainTable.Name)
                                               .Where(GetUserWhere(user, tenant))
                                               .Where(ChainTable.Columns.Folder, folder));

                        // reset folder counters
                        db.ExecuteNonQuery(new SqlUpdate(FolderTable.Name)
                            .Where(GetUserWhere(user, tenant))
                            .Where(FolderTable.Columns.Folder, folder)
                            .Set(FolderTable.Columns.TotalConversationsCount, 0)
                            .Set(FolderTable.Columns.TotalMessagesCount, 0)
                            .Set(FolderTable.Columns.UnreadConversationsCount, 0)
                            .Set(FolderTable.Columns.UnreadMessagesCount, 0));

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
                    new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.Uidl)
                        .Where(MailTable.Columns.MailboxId, mailboxId)
                        .Where(Exp.In(MailTable.Columns.Uidl, uidlList)))
                                         .ConvertAll(r => Convert.ToString(r[0]))
                                         .ToList();
                return existingUidls;
            }
        }

        public void SetDraftSending(MailDraft draft)
        {
            SetConversationsFolder(draft.Mailbox.TenantId, draft.Mailbox.UserId, MailFolder.Ids.temp,
                new List<int> {draft.Id});
        }

        public void ReleaseSendingDraftOnSuccess(MailDraft draft, MailMessage message)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    // message was correctly send - lets update its chains id
                    var draftChainId = message.ChainId;
                    // before moving message from draft to sent folder - lets recalculate its correct chain id
                    message.ChainId = DetectChainId(draft.Mailbox, message.MimeMessageId, message.MimeReplyToId,
                        message.Subject);

                    if (message.ChainId.Equals(message.MimeMessageId))
                        message.MimeReplyToId = null;

                    if (!draftChainId.Equals(message.ChainId))
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(MailTable.Name)
                                .Set(MailTable.Columns.ChainId, message.ChainId)
                                .Where(GetUserWhere(draft.Mailbox.UserId, draft.Mailbox.TenantId))
                                .Where(MailTable.Columns.Id, message.Id));

                        UpdateChain(db, draftChainId, MailFolder.Ids.temp, draft.Mailbox.MailBoxId,
                            draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        var updateOldChainIdQuery = new SqlUpdate(ChainXCrmContactEntity.Name)
                            .Set(ChainXCrmContactEntity.Columns.ChainId, message.ChainId)
                            .Where(ChainXCrmContactEntity.Columns.ChainId, draftChainId)
                            .Where(ChainXCrmContactEntity.Columns.MailboxId, draft.Mailbox.MailBoxId)
                            .Where(ChainXCrmContactEntity.Columns.Tenant, draft.Mailbox.TenantId);

                        db.ExecuteNonQuery(updateOldChainIdQuery);
                    }

                    UpdateChain(db, message.ChainId, MailFolder.Ids.temp, draft.Mailbox.MailBoxId,
                        draft.Mailbox.TenantId, draft.Mailbox.UserId);

                    var listObjects = GetChainedMessagesInfo(db, draft.Mailbox.TenantId, draft.Mailbox.UserId,
                        new List<int> {draft.Id},
                        new[]
                        {
                            MailTable.Columns.Id, MailTable.Columns.Unread, MailTable.Columns.Folder,
                            MailTable.Columns.ChainId, MailTable.Columns.MailboxId
                        });

                    if (!listObjects.Any())
                        return;

                    SetMessagesFolder(db, draft.Mailbox.TenantId, draft.Mailbox.UserId, listObjects,
                        MailFolder.Ids.sent);

                    SetMessageFolderRestore(db, draft.Mailbox.TenantId, draft.Mailbox.UserId, MailFolder.Ids.sent,
                        draft.Id);

                    RecalculateFolders(db, draft.Mailbox.TenantId, draft.Mailbox.UserId, true);

                    tx.Commit();
                }
            }
        }

        public void ReleaseSendingDraftOnFailure(MailDraft draft)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var listObjects = GetChainedMessagesInfo(db, draft.Mailbox.TenantId, draft.Mailbox.UserId,
                        new List<int> {draft.Id},
                        new[]
                        {
                            MailTable.Columns.Id, MailTable.Columns.Unread, MailTable.Columns.Folder,
                            MailTable.Columns.ChainId, MailTable.Columns.MailboxId
                        });

                    if (!listObjects.Any())
                        return;

                    SetMessagesFolder(db, draft.Mailbox.TenantId, draft.Mailbox.UserId, listObjects,
                        MailFolder.Ids.drafts);

                    RecalculateFolders(db, draft.Mailbox.TenantId, draft.Mailbox.UserId, true);

                    tx.Commit();
                }
            }
        }

        public MailMessage SaveDraft(MailDraft draft)
        {
            var embededAttachmentsForSaving = FixHtmlBodyWithEmbeddedAttachments(draft);

            var message = draft.ToMailMessage();

            var needRestoreAttachments = draft.Id == 0 && message.Attachments.Any();

            if (needRestoreAttachments)
            {
                message.Attachments.ForEach(attachment => StoreAttachmentCopy(draft.Mailbox.TenantId, draft.Mailbox.UserId, attachment, draft.StreamId));
            }

            StoreMailBody(draft.Mailbox, message);

            long usedQuota;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    draft.Id = MailSave(db, draft.Mailbox, message, draft.Id, message.Folder, message.Folder,
                        string.Empty, string.Empty, false, out usedQuota);

                    message.Id = draft.Id;

                    if (draft.AccountChanged)
                    {
                        UpdateChain(db, message.ChainId, message.Folder, draft.PreviousMailboxId,
                            draft.Mailbox.TenantId, draft.Mailbox.UserId);
                    }

                    if (draft.Id > 0 && needRestoreAttachments)
                    {
                        foreach (var attachment in message.Attachments)
                        {
                            var newId = SaveAttachment(db, draft.Mailbox.TenantId, draft.Id, attachment);
                            attachment.fileId = newId;
                        }
                    }

                    if (draft.Id > 0 && embededAttachmentsForSaving.Any())
                    {
                        SaveAttachments(db, draft.Mailbox.TenantId, draft.Id, embededAttachmentsForSaving);
                    }

                    UpdateChain(db, message.ChainId, message.Folder, draft.Mailbox.MailBoxId, draft.Mailbox.TenantId,
                        draft.Mailbox.UserId);

                    if (draft.AccountChanged)
                        UpdateCrmLinkedMailboxId(db, message.ChainId, draft.Mailbox.TenantId,
                            draft.PreviousMailboxId, draft.Mailbox.MailBoxId);

                    tx.Commit();

                }
            }

            if (usedQuota > 0)
                QuotaUsedDelete(draft.Mailbox.TenantId, usedQuota);

            return message;
        }

        public int MailSave(MailBox mailbox, MailMessage mail,
            int messageId, int folder, int folderRestore,
            string uidl, string md5, bool saveAttachments)
        {
            int id;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    long usedQuota;

                    id = MailSave(db, mailbox, mail, messageId, folder, folderRestore, uidl, md5, saveAttachments,
                        out usedQuota);

                    tx.Commit();
                }
            }

            return id;
        }

        public int MailSave(DbManager db, MailBox mailbox, MailMessage mail,
            int messageId, int folder, int folderRestore,
            string uidl, string md5, bool saveAttachments, out long usedQuota)
        {
            int idMail;
            var countAttachments = 0;
            usedQuota = 0;
            var address = GetAddress(mailbox.EMail);


            if (messageId != 0)
                countAttachments = GetCountAttachments(db, messageId);

            #region SQL query construction

            if (messageId == 0)
            {
                // This case is for first time saved draft and received message.
                var insert = new SqlInsert(MailTable.Name)
                    .InColumnValue(MailTable.Columns.Id, messageId)
                    .InColumnValue(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                    .InColumnValue(MailTable.Columns.Tenant, mailbox.TenantId)
                    .InColumnValue(MailTable.Columns.User, mailbox.UserId)
                    .InColumnValue(MailTable.Columns.Address, address)
                    .InColumnValue(MailTable.Columns.Uidl, !string.IsNullOrEmpty(uidl) ? uidl : null)
                    .InColumnValue(MailTable.Columns.Md5, !string.IsNullOrEmpty(md5) ? md5 : null)
                    .InColumnValue(MailTable.Columns.From, MailUtil.NormalizeStringForMySql(mail.From))
                    .InColumnValue(MailTable.Columns.To, MailUtil.NormalizeStringForMySql(mail.To))
                    .InColumnValue(MailTable.Columns.Reply, mail.ReplyTo)
                    .InColumnValue(MailTable.Columns.Subject, MailUtil.NormalizeStringForMySql(mail.Subject))
                    .InColumnValue(MailTable.Columns.Cc, MailUtil.NormalizeStringForMySql(mail.Cc))
                    .InColumnValue(MailTable.Columns.Bcc, MailUtil.NormalizeStringForMySql(mail.Bcc))
                    .InColumnValue(MailTable.Columns.Importance, mail.Important)
                    .InColumnValue(MailTable.Columns.DateReceived, DateTime.UtcNow)
                    .InColumnValue(MailTable.Columns.DateSent, mail.Date.ToUniversalTime())
                    .InColumnValue(MailTable.Columns.Size, mail.Size)
                    .InColumnValue(MailTable.Columns.AttachCount,
                        !saveAttachments
                            ? countAttachments
                            : (mail.Attachments != null ? mail.Attachments.Count : 0))
                    .InColumnValue(MailTable.Columns.Unread, mail.IsNew)
                    .InColumnValue(MailTable.Columns.IsAnswered, mail.IsAnswered)
                    .InColumnValue(MailTable.Columns.IsForwarded, mail.IsForwarded)
                    .InColumnValue(MailTable.Columns.Stream, mail.StreamId)
                    .InColumnValue(MailTable.Columns.Folder, folder)
                    .InColumnValue(MailTable.Columns.FolderRestore, folderRestore)
                    .InColumnValue(MailTable.Columns.Spam, 0)
                    .InColumnValue(MailTable.Columns.MimeMessageId, mail.MimeMessageId)
                    .InColumnValue(MailTable.Columns.MimeInReplyTo, mail.MimeReplyToId)
                    .InColumnValue(MailTable.Columns.ChainId, mail.ChainId)
                    .InColumnValue(MailTable.Columns.Introduction, MailUtil.NormalizeStringForMySql(mail.Introduction))
                    .InColumnValue(MailTable.Columns.ChainDate, mail.Date.ToUniversalTime())
                    .InColumnValue(MailTable.Columns.IsTextBodyOnly, mail.TextBodyOnly)
                    .Identity(0, 0, true);

                if (mail.HasParseError)
                    insert.InColumnValue(MailTable.Columns.HasParseError, mail.HasParseError);

                if (!string.IsNullOrEmpty(mail.CalendarUid))
                    insert.InColumnValue(MailTable.Columns.CalendarUid, mail.CalendarUid);

                idMail = db.ExecuteScalar<int>(insert);

                ChangeFolderCounters(db, mailbox.TenantId, mailbox.UserId, folder,
                    mail.IsNew ? 1 : 0, 1, false);
            }
            else
            {
                // This case is for already saved draft only.
                var update = new SqlUpdate(MailTable.Name)
                    .Where(MailTable.Columns.Id, messageId)
                    .Set(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                    .Set(MailTable.Columns.From, MailUtil.NormalizeStringForMySql(mail.From))
                    .Set(MailTable.Columns.To, MailUtil.NormalizeStringForMySql(mail.To))
                    .Set(MailTable.Columns.Reply, mail.ReplyTo)
                    .Set(MailTable.Columns.Subject, MailUtil.NormalizeStringForMySql(mail.Subject))
                    .Set(MailTable.Columns.Cc, MailUtil.NormalizeStringForMySql(mail.Cc))
                    .Set(MailTable.Columns.Bcc, MailUtil.NormalizeStringForMySql(mail.Bcc))
                    .Set(MailTable.Columns.Importance, mail.Important)
                    .Set(MailTable.Columns.DateReceived, DateTime.UtcNow)
                    .Set(MailTable.Columns.DateSent, mail.Date.ToUniversalTime())
                    .Set(MailTable.Columns.Size, mail.Size)
                    .Set(MailTable.Columns.AttachCount,
                        !saveAttachments
                            ? countAttachments
                            : (mail.Attachments != null ? mail.Attachments.Count : 0))
                    .Set(MailTable.Columns.Unread, mail.IsNew)
                    .Set(MailTable.Columns.IsAnswered, mail.IsAnswered)
                    .Set(MailTable.Columns.IsForwarded, mail.IsForwarded)
                    .Set(MailTable.Columns.Stream, mail.StreamId)
                    .Set(MailTable.Columns.Folder, folder)
                    .Set(MailTable.Columns.FolderRestore, folderRestore)
                    .Set(MailTable.Columns.IsTextBodyOnly, mail.TextBodyOnly)
                    .Set(MailTable.Columns.Spam, 0)
                    .Set(MailTable.Columns.MimeMessageId, mail.MimeMessageId)
                    .Set(MailTable.Columns.MimeInReplyTo, mail.MimeReplyToId)
                    .Set(MailTable.Columns.ChainId, mail.ChainId)
                    .Set(MailTable.Columns.Introduction, MailUtil.NormalizeStringForMySql(mail.Introduction));

                db.ExecuteNonQuery(update);
                idMail = messageId;
            }

            #endregion

            if (saveAttachments &&
                mail.Attachments != null &&
                mail.Attachments.Count > 0)
            {
                usedQuota = MarkAttachmetsNeedRemove(db, mailbox.TenantId,
                    Exp.Eq(AttachmentTable.Columns.MailId, idMail));
                SaveAttachments(db, mailbox.TenantId, idMail, mail.Attachments);
            }

            if (mail.FromEmail.Length != 0)
            {
                db.ExecuteNonQuery(
                    new SqlDelete(TagMailTable.Name)
                        .Where(TagMailTable.Columns.MailId, idMail));

                var tagList = new List<int>();

                if (mail.TagIds != null)
                    tagList.AddRange(mail.TagIds);

                var tagAddressesTagIds = db.ExecuteList(
                    new SqlQuery(TagAddressTable.Name)
                        .Distinct()
                        .Select(TagAddressTable.Columns.TagId)
                        .Where(TagAddressTable.Columns.Address, mail.FromEmail)
                        .Where(Exp.In(TagAddressTable.Columns.TagId,
                            new SqlQuery(TagTable.Name)
                                .Select(TagTable.Columns.Id)
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

            _log.Debug("MailSave() tenant='{0}', user_id='{1}', email='{2}', from='{3}', id_mail='{4}'",
                mailbox.TenantId, mailbox.UserId, mailbox.EMail, mail.From, idMail);

            return idMail;
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

        public bool SearchExistingMessagesAndUpdateIfNeeded(MailBox mailbox, int folderId, string uidl, string md5,
            string mimeMessageId, bool fromThisMailBox, bool toThisMailBox, List<int> tagsIds = null)
        {
            var messagesInfo = GetInfoOnExistingMessages(
                mailbox.MailBoxId, md5, mimeMessageId, new[]
                {
                    MailTable.Columns.Id,
                    MailTable.Columns.FolderRestore,
                    MailTable.Columns.Uidl,
                    MailTable.Columns.IsRemoved
                })
                .ConvertAll(i => new
                {
                    id = Convert.ToInt32(i[0]),
                    folder_orig = Convert.ToInt32(i[1]),
                    uidl = Convert.ToString(i[2]),
                    is_removed = Convert.ToBoolean(i[3])
                });

            if (!messagesInfo.Any())
                return false;

            var idList = messagesInfo.Where(m => !m.is_removed).Select(m => m.id).ToList();
            if (!idList.Any())
            {
                _log.Info("Message already exists and it was removed from portal.");
                return true;
            }

            if (mailbox.Imap)
            {
                if (tagsIds != null) // Add new tags to existing messages
                {
                    foreach (var tagId in tagsIds)
                        SetMessagesTag(mailbox.TenantId, mailbox.UserId, tagId, idList);
                }

                if ((!fromThisMailBox || !toThisMailBox) && messagesInfo.Exists(m => m.folder_orig == folderId))
                {
                    var clone = messagesInfo.FirstOrDefault(m => m.folder_orig == folderId && m.uidl == uidl);
                    if(clone != null)
                        _log.Info("Message already exists: mailId={0}. Clone", clone.id);
                    else
                        _log.Info("Message already exists. by MD5/MimeMessageId");

                    return true;
                }
            }
            else
            {
                if (!fromThisMailBox && toThisMailBox && messagesInfo.Count == 1)
                {
                    _log.Info("Message already exists: mailId={0}. Outbox clone", messagesInfo.First().id);
                    return true;
                }
            }

            if (folderId == MailFolder.Ids.sent)
            {
                var sentCloneForUpdate =
                    messagesInfo.FirstOrDefault(
                        m => m.folder_orig == MailFolder.Ids.sent && string.IsNullOrEmpty(m.uidl));

                if (sentCloneForUpdate != null)
                {
                    if (!sentCloneForUpdate.is_removed)
                        UpdateMessageUidl(mailbox.TenantId, mailbox.UserId, sentCloneForUpdate.id, uidl);

                    _log.Info("Message already exists: mailId={0}. Outbox clone", sentCloneForUpdate.id);

                    return true;
                }
            }

            if (folderId == MailFolder.Ids.spam)
            {
                var first = messagesInfo.First();

                _log.Info("Message already exists: mailId={0}. It was moved to spam on server", first.id);

                return true;
            }

            var fullClone = messagesInfo.FirstOrDefault(m => m.folder_orig == folderId && m.uidl == uidl);
            if (fullClone == null)
                return false;

            _log.Info("Message already exists: mailId={0}. Full clone", fullClone.id);
            return true;
        }

        public bool TryRemoveMailDirectory(MailBox mailbox, string streamId)
        {
            //Trying to delete all attachments and mailbody
            var storage = MailDataStore.GetDataStore(mailbox.TenantId);
            try
            {
                storage.DeleteDirectory(string.Empty,
                    MailStoragePathCombiner.GetMessageDirectory(mailbox.UserId, streamId));
                return true;
            }
            catch (Exception ex)
            {
                _log.Debug(
                    "Problems with mail_directory deleting. Account: {0}. Folder: {1}/{2}/{3}. Exception: {4}",
                    mailbox.EMail, mailbox.TenantId, mailbox.UserId, streamId, ex.ToString());

                return false;
            } 
        }

        public void AddRelationshipEventForLinkedAccounts(MailBox mailbox, MailMessage messageItem, string httpContextScheme, ILogger log)
        {
            if (log == null)
                log = new NullLogger();

            try
            {
                messageItem.LinkedCrmEntityIds = GetLinkedCrmEntitiesId(messageItem.ChainId, mailbox.MailBoxId,
                                                                         mailbox.TenantId);

                if(!messageItem.LinkedCrmEntityIds.Any()) return;

                var crmDal = new CrmHistoryDal(mailbox.TenantId, mailbox.UserId, httpContextScheme);
                crmDal.AddRelationshipEvents(messageItem);
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("Problem with adding history event to CRM. mailId={0}", messageItem.Id), ex);
            }
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
                    new SqlQuery(ChainTable.Name)
                        .Select(ChainTable.Columns.Id, ChainTable.Columns.Folder)
                        .Where(ChainTable.Columns.Id, mimeMessageId)
                        .Where(ChainTable.Columns.MailboxId, mailbox.MailBoxId)
                        .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    ).Select(x => new { id = (string)x[0], folder = Convert.ToInt32(x[1]) }).ToArray();

                if (chains.Any())
                {
                    db.ExecuteNonQuery(
                        new SqlUpdate(MailTable.Name)
                            .Set(MailTable.Columns.ChainId, chainId)
                            .Where(MailTable.Columns.ChainId, mimeMessageId)
                            .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.IsRemoved, false));

                    chainsForUpdate = chains.Concat(chainsForUpdate).ToArray();

                    var newChainsForUpdate = db.ExecuteList(
                        new SqlQuery(MailTable.Name)
                            .Select(MailTable.Columns.Folder)
                            .Where(MailTable.Columns.ChainId, chainId)
                            .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                            .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                            .Where(MailTable.Columns.IsRemoved, false)
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

        public string DetectChainId(MailBox mailbox, string mimeMessageId, string mimeReplyToId, string subject)
        {
            var chainId = mimeMessageId; //Chain id is equal to root conversataions message - MimeMessageId
            if (!string.IsNullOrEmpty(mimeMessageId) && !string.IsNullOrEmpty(mimeReplyToId))
            {
                chainId = mimeReplyToId;
                try
                {
                    Func<SqlQuery> getBaseQuery = () => new SqlQuery(MailTable.Name)
                        .Select(MailTable.Columns.ChainId)
                        .Select(MailTable.Columns.Subject)
                        .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                        .Where(MailTable.Columns.IsRemoved, false)
                        .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                        .Distinct();

                    using (var db = GetDb())
                    {
                        var detectChainByInReplyToQuery = getBaseQuery()
                            .Where(MailTable.Columns.MimeMessageId, mimeReplyToId);

                        var chainAndSubject = db.ExecuteList(detectChainByInReplyToQuery)
                               .ConvertAll(x => new
                               {
                                   chain_id = (string)x[0],
                                   subject = (string)x[1]
                               })
                               .FirstOrDefault();

                        if (chainAndSubject == null)
                        {
                            var detectChainByChainIdQuery = getBaseQuery()
                                .Where(MailTable.Columns.ChainId, mimeReplyToId);

                            chainAndSubject = db.ExecuteList(detectChainByChainIdQuery)
                                   .ConvertAll(x => new
                                   {
                                       chain_id = (string)x[0],
                                       subject = (string)x[1]
                                   })
                                   .FirstOrDefault();
                        }

                        if (chainAndSubject != null)
                        {
                            var chainSubject = MailUtil.NormalizeSubject(chainAndSubject.subject);
                            var messageSubject = MailUtil.NormalizeSubject(subject);
                            chainId = chainSubject.Equals(messageSubject) ? chainAndSubject.chain_id : mimeMessageId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("DetectChainId() params tenant={0}, user_id='{1}', mailbox_id={2}, mime_message_id='{3}' Exception:\r\n{4}",
                        mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, mimeMessageId, ex.ToString());
                }
            }

            _log.Debug("DetectChainId() tenant='{0}', user_id='{1}', mailbox_id='{2}', mime_message_id='{3}' Result: {4}",
                        mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, mimeMessageId, chainId);

            return chainId;
        }

        public MailMessage GetMailInfo(int tenant, string user, int messageId, MailMessage.Options options)
        {
            using (var db = GetDb())
            {
                var messageItem = GetMailInfo(db, tenant, user, messageId, options);
                return messageItem;
            }
        }

        public MailMessage GetMailInfo(IDbManager db, int tenant, string user, int messageId, MailMessage.Options options)
        {
            var selectQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.Address,
                        MailTable.Columns.ChainId,
                        MailTable.Columns.ChainDate,
                        MailTable.Columns.Importance,
                        MailTable.Columns.DateSent,
                        MailTable.Columns.From,
                        MailTable.Columns.To,
                        MailTable.Columns.Cc,
                        MailTable.Columns.Bcc,
                        MailTable.Columns.Reply,
                        MailTable.Columns.Stream,
                        MailTable.Columns.IsAnswered,
                        MailTable.Columns.IsForwarded,
                        MailTable.Columns.Subject,
                        MailTable.Columns.AttachCount,
                        MailTable.Columns.Size,
                        MailTable.Columns.Folder,
                        MailTable.Columns.Unread,
                        MailTable.Columns.Introduction,
                        MailTable.Columns.IsTextBodyOnly,
                        MailTable.Columns.MailboxId,
                        MailTable.Columns.FolderRestore,
                        MailTable.Columns.HasParseError,
                        MailTable.Columns.MimeMessageId,
                        MailTable.Columns.MimeInReplyTo,
                        MailTable.Columns.CalendarUid
                )
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.Id, messageId);

            if (options.OnlyUnremoved)
                selectQuery
                    .Where(MailTable.Columns.IsRemoved, false);

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
                                       folder = Convert.ToInt32(x[16]),
                                       unread = Convert.ToBoolean(x[17]),
                                       introduction = (string) x[18],
                                       is_text_body_only = Convert.ToBoolean(x[19]),
                                       id_mailbox = Convert.ToInt32(x[20]),
                                       folder_restore = Convert.ToInt32(x[21]),
                                       has_parse_error = Convert.ToBoolean(x[22]),
                                       mime_message_id = (string) x[23],
                                       mime_in_reply_to = (string) x[24],
                                       calendar_uid = (string)x[25]
                                   })
                               .SingleOrDefault();

            if (mailDbInfo == null)
                return null;

            var tags = new ItemList<int>(
                db.ExecuteList(
                    new SqlQuery(TagMailTable.Name)
                        .Select(TagMailTable.Columns.TagId)
                        .Where(TagMailTable.Columns.MailId, messageId))
                  .ConvertAll(x => (int)x[0]));

            var now = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant).TimeZone, DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant).TimeZone, mailDbInfo.datesent);
            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            var item = new MailMessage
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
                    MimeReplyToId = mailDbInfo.mime_in_reply_to,
                    CalendarUid = mailDbInfo.calendar_uid
                };

            //Reassemble paths
            if (options.LoadBody)
            {
                var htmlBody = "";

                if (!item.HasParseError)
                {
                    var watch = new Stopwatch();
                    double swtGetBodyMilliseconds = 0;
                    double swtSanitazeilliseconds = 0;

                    var dataStore = MailDataStore.GetDataStore(tenant);
                    var key =  MailStoragePathCombiner.GetBodyKey(user, item.StreamId);

                    try
                    {
                        watch.Start();
                        using (var s = dataStore.GetReadStream(string.Empty, key))
                        {
                            htmlBody = Encoding.UTF8.GetString(s.ReadToEnd());
                        }

                        watch.Stop();
                        swtGetBodyMilliseconds = watch.Elapsed.TotalMilliseconds;
                        watch.Reset();

                        if (options.NeedSanitizer && item.Folder != MailFolder.Ids.drafts && !item.From.Equals(MailDaemonEmail))
                        {
                            watch.Start();
                            bool imagesAreBlocked;
                            var htmlSanitizer =
                                new HtmlSanitizer(new HtmlSanitizer.Options(options.LoadImages, options.NeedProxyHttp));

                            htmlBody = htmlSanitizer.Sanitize(htmlBody, out imagesAreBlocked);
                            watch.Stop();
                            swtSanitazeilliseconds = watch.Elapsed.TotalMilliseconds;
                            item.ContentIsBlocked = imagesAreBlocked;
                        }

                        _log.Debug(
                            "Mail->GetMailInfo(id={0})->Elapsed: BodyLoad={1}ms, Sanitaze={2}ms (NeedSanitizer={3}, NeedProxyHttp={4})",
                            messageId, swtGetBodyMilliseconds, swtSanitazeilliseconds, options.NeedSanitizer, options.NeedProxyHttp);
                    }
                    catch (Exception ex)
                    {
                        item.IsBodyCorrupted = true;
                        htmlBody = "";
                        _log.Error(
                            string.Format("Load stored body error: tenant={0} user=\"{1}\" messageId={2} key=\"{3}\"",
                                tenant, user, messageId, key), ex);

                        watch.Stop();
                        swtGetBodyMilliseconds = watch.Elapsed.TotalMilliseconds;
                        _log.Debug(
                            "Mail->GetMailInfo(id={0})->Elapsed [BodyLoadFailed]: BodyLoad={1}ms, Sanitaze={2}ms (NeedSanitizer={3}, NeedProxyHttp={4})",
                            messageId, swtGetBodyMilliseconds, swtSanitazeilliseconds, options.NeedSanitizer, options.NeedProxyHttp);
                    }
                }

                item.HtmlBody = htmlBody;

                if (string.IsNullOrEmpty(mailDbInfo.introduction) && !string.IsNullOrEmpty(item.HtmlBody))
                {
                    // if introduction wasn't saved, it will be save.
                    var introduction = MailMessage.GetIntroduction(htmlBody);

                    if (!string.IsNullOrEmpty(introduction))
                    {
                        item.Introduction = introduction;

                        db.ExecuteNonQuery(new SqlUpdate(MailTable.Name)
                            .Set(MailTable.Columns.Introduction, item.Introduction)
                            .Where(GetUserWhere(user, tenant))
                            .Where(MailTable.Columns.Id, messageId));
                    }
                }
            }

            var attachments = GetMessageAttachments(db, tenant, user, messageId);

            item.Attachments = attachments.Count != 0 ? attachments : new List<MailAttachment>();

            return item;
        }

        public List<MailAttachment> GetMessageAttachments(int tenant, string user, int messageId)
        {
            using (var db = GetDb())
            {
                var mailDbInfo =
                    db.ExecuteList(
                        new SqlQuery(MailTable.Name)
                            .Select(
                                MailTable.Columns.Stream,
                                MailTable.Columns.AttachCount
                            )
                            .Where(GetUserWhere(user, tenant))
                            .Where(MailTable.Columns.Id, messageId))
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
            return new SqlQuery(AttachmentTable.Name)
                .InnerJoin(MailTable.Name,
                           Exp.EqColumns(MailTable.Columns.Id.Prefix(MailTable.Name),
                                         AttachmentTable.Columns.MailId.Prefix(AttachmentTable.Name)))
                .Select(AttachmentTable.Columns.Id.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.RealName.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.StoredName.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.Type.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.Size.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.FileNumber.Prefix(AttachmentTable.Name),
                        MailTable.Columns.Stream.Prefix(MailTable.Name),
                        MailTable.Columns.Tenant.Prefix(MailTable.Name),
                        MailTable.Columns.User.Prefix(MailTable.Name),
                        AttachmentTable.Columns.ContentId.Prefix(AttachmentTable.Name),
                        AttachmentTable.Columns.IdMailbox.Prefix(AttachmentTable.Name));
        }

        private List<MailAttachment> GetMessageAttachments(IDbManager db, int tenant, string user, int messageId)
        {
            var attachmentsSelectQuery = GetAttachmentsSelectQuery()
                .Where(MailTable.Columns.Id.Prefix(MailTable.Name), messageId)
                .Where(AttachmentTable.Columns.NeedRemove.Prefix(AttachmentTable.Name), false)
                .Where(AttachmentTable.Columns.ContentId, Exp.Empty)
                .Where(GetUserWhere(user, tenant, MailTable.Name));

            var attachments =
                db.ExecuteList(attachmentsSelectQuery)
                  .ConvertAll(ToMailItemAttachment);

            return attachments;
        }

        public List<MailMessage> GetSingleMailsFiltered(int tenant, string user, MailFilter filter, out long totalMessagesCount)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            var concatTagIds =
                String.Format(
                    "(SELECT CAST(group_concat(tm.{0} ORDER BY tm.{3} SEPARATOR ',') AS CHAR) from {1} as tm WHERE tm.{2} = `id`) tagIds",
                    TagMailTable.Columns.TagId, TagMailTable.Name, TagMailTable.Columns.MailId, TagMailTable.Columns.TimeCreated);

            using (var db = GetDb())
            {
                const string mm_alias = "mm";
                const string mtm_alias = "tm";

                var filtered = new SqlQuery(MailTable.Name.Alias(mm_alias))
                    .Select(
                        MailTable.Columns.Id.Prefix(mm_alias), 
                        MailTable.Columns.ChainId.Prefix(mm_alias));

                if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
                {
                    filtered = filtered
                        .InnerJoin(TagMailTable.Name.Alias(mtm_alias),
                                    Exp.EqColumns(MailTable.Columns.Id.Prefix(mm_alias), TagMailTable.Columns.MailId.Prefix(mtm_alias)))
                        .Where(Exp.In(TagMailTable.Columns.TagId.Prefix(mtm_alias), filter.CustomLabels));
                }

                filtered = filtered
                    .ApplyFilter(filter, mm_alias);

                if (filtered == null)
                {
                    totalMessagesCount = 0;
                    return new List<MailMessage>();
                }

                filtered
                    .Where(TagMailTable.Columns.Tenant.Prefix(mm_alias), tenant)
                    .Where(TagMailTable.Columns.User.Prefix(mm_alias), user)
                    .Where(MailTable.Columns.IsRemoved.Prefix(mm_alias), false);

                if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
                {
                    filtered = filtered
                        .GroupBy(1)
                        .Having(Exp.Eq(string.Format("count({0})", MailTable.Columns.Id.Prefix(mm_alias)), filter.CustomLabels.Count()));
                }

                // Filter and sort all existing messages and get their ids and chain_ids
                var filteredIds = db.ExecuteList(filtered)
                                     .ConvertAll(r => new { id = Convert.ToInt32(r[0]), chain_id = (string)r[1] });

                var idsSet = filteredIds.Select(m => m.id).ToList();

                if (!idsSet.Any())
                {
                    totalMessagesCount = 0;
                    return new List<MailMessage>();
                }

                const string select_chain_length = "1";

                totalMessagesCount = idsSet.Count();
                var page = Math.Min(filter.Page, (int)Math.Ceiling((double)totalMessagesCount / filter.PageSize));

                var queryMessages = new SqlQuery(MailTable.Name + " as outer_mail")
                    .Select(MailTable.Columns.Id, MailTable.Columns.From, MailTable.Columns.To,
                            MailTable.Columns.Reply, MailTable.Columns.Subject, MailTable.Columns.Importance,
                            "1", MailTable.Columns.DateSent, MailTable.Columns.Size,
                            MailTable.Columns.AttachCount, MailTable.Columns.Unread, MailTable.Columns.IsAnswered,
                            MailTable.Columns.IsForwarded, concatTagIds, MailTable.Columns.FolderRestore, 
                            MailTable.Columns.ChainId, select_chain_length, MailTable.Columns.Folder)
                    .Where(Exp.In(MailTable.Columns.Id, idsSet.ToArray()))
                    .ApplySorting(filter)
                    .SetFirstResult((page - 1) * filter.PageSize)
                    .SetMaxResults(filter.PageSize);

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
                var dateSent = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.Name)
                    .Select(MailTable.Columns.DateSent)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.Id, messageId));

                var sortOrder = filter.SortOrder == "ascending";

                const string mm_alias = "mm";
                const string mtm_alias = "tm";

                var queryMessages = new SqlQuery(MailTable.Name.Alias(mm_alias))
                    .Select(MailTable.Columns.Id.Prefix(mm_alias));

                if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
                {
                    queryMessages = queryMessages
                        .InnerJoin(TagMailTable.Name.Alias(mtm_alias),
                                    Exp.EqColumns(MailTable.Columns.Id.Prefix(mm_alias), TagMailTable.Columns.MailId.Prefix(mtm_alias)))
                        .Where(Exp.In(TagMailTable.Columns.TagId.Prefix(mtm_alias), filter.CustomLabels));
                }

                queryMessages = queryMessages
                    .ApplyFilter(filter, mm_alias);

                if (queryMessages == null)
                    return -1;

                queryMessages
                    .Where(TagMailTable.Columns.Tenant.Prefix(mm_alias), tenant)
                    .Where(TagMailTable.Columns.User.Prefix(mm_alias), user)
                    .Where(MailTable.Columns.IsRemoved.Prefix(mm_alias), false)
                    .Where(sortOrder
                               ? Exp.Ge(MailTable.Columns.DateSent.Prefix(mm_alias), dateSent)
                               : Exp.Le(MailTable.Columns.DateSent.Prefix(mm_alias), dateSent));

                if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
                {
                    queryMessages = queryMessages
                        .GroupBy(1)
                        .Having(Exp.Eq(string.Format("count({0})", MailTable.Columns.Id.Prefix(mm_alias)), filter.CustomLabels.Count()));
                }

                queryMessages = queryMessages
                    .SetFirstResult(1)
                    .SetMaxResults(1)
                    .OrderBy(MailTable.Columns.DateSent.Prefix(mm_alias), sortOrder);

                return db.ExecuteScalar<long>(queryMessages);
            }
        }

        public List<MailMessage> GetConversationMessages(int tenant, string user, int messageId,
                                                             bool loadAllContent, bool needProxyHttp, bool needMailSanitazer, bool markRead = false)
        {
            var mailDbInfoQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.ChainId, MailTable.Columns.MailboxId, MailTable.Columns.Folder)
                .Where(MailTable.Columns.IsRemoved, false)
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.Id, messageId);

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
                                       .OrderBy(MailTable.Columns.DateSent, true);

                var queryResult = db.ExecuteList(getMessagesIdsQuery);

                var mailInfoList =
                    queryResult.Select(
                        (item, i) =>
                            GetMailInfo(db, tenant, user, Convert.ToInt32(item[0]), new MailMessage.Options
                            {
                                LoadImages = false,
                                LoadBody = loadAllContent || (i == queryResult.Count - 1),
                                NeedProxyHttp = needProxyHttp,
                                NeedSanitizer = needMailSanitazer
                            }))
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
                        new SqlUpdate(MailTable.Name)
                            .Where(Exp.In(MailTable.Columns.Id, unreadMessages.Select(x => (object)x.Id).ToArray()))
                            .Where(GetUserWhere(user, tenant))
                            .Set(MailTable.Columns.Unread, false));

                    foreach (var keyPair in unreadMessagesCountByFolder)
                    {
                        ChangeFolderCounters(db, tenant, user, keyPair.Key, keyPair.Value * (-1), 0, -1, 0);

                        db.ExecuteNonQuery(
                            new SqlUpdate(ChainTable.Name)
                                .Set(ChainTable.Columns.Unread, false)
                                .Where(GetUserWhere(user, tenant))
                                .Where(ChainTable.Columns.Id, messageInfo.chain_id)
                                .Where(ChainTable.Columns.MailboxId, messageInfo.mailbox_id)
                                .Where(ChainTable.Columns.Folder, keyPair.Key));
                    }

                    tx.Commit();
                }

                return mailInfoList;
            }
        }

        public void SetMessageFolderRestore(DbManager db, int tenant, string user, int toFolder, int messageId)
        {
            var updateFolderRestore =
                new SqlUpdate(MailTable.Name)
                    .Set(MailTable.Columns.FolderRestore, toFolder)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.Id, messageId);

            db.ExecuteNonQuery(updateFolderRestore);
        }

        public void UpdateMessageUidl(int tenant, string user, int messageId, string newUidl)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.Name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.Id, messageId)
                        .Set(MailTable.Columns.Uidl, newUidl));
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

            var query = new SqlUpdate(MailTable.Name)
                .Set(MailTable.Columns.Folder, toFolder)
                .Where(GetUserWhere(user, tenant))
                .Where(Exp.In(MailTable.Columns.Id, ids));

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
                new SqlUpdate(MailTable.Name)
                    .Set(MailTable.Columns.Folder + " = " + MailTable.Columns.FolderRestore)
                    .Where(MailTable.Columns.IsRemoved, false)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.Id, ids));

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
                new SqlUpdate(MailTable.Name)
                    .Set(MailTable.Columns.IsRemoved, true)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.Id, messageIds)));

            var usedQuota = MarkAttachmetsNeedRemove(db, tenant, Exp.In(AttachmentTable.Columns.MailId, messageIds));

            var affectedTags = db.ExecuteList(
                new SqlQuery(TagMailTable.Name)
                    .Select(TagMailTable.Columns.TagId)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(TagMailTable.Columns.MailId, messageIds)))
                                  .ConvertAll(r => Convert.ToInt32(r[0]));

            db.ExecuteNonQuery(
                new SqlDelete(TagMailTable.Name)
                    .Where(Exp.In(TagMailTable.Columns.MailId, messageIds))
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
                    new SqlQuery(MailTable.Name)
                        .Select(columns)
                        .Where(MailTable.Columns.Md5, md5)
                        .Where(MailTable.Columns.MailboxId, mailboxId));
            }
        }

        private List<object[]> GetMessagesInfoByMimeMessageId(int mailboxId, string mimeMessageId, string[] columns)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(
                    new SqlQuery(MailTable.Name)
                        .Select(columns)
                        .Where(MailTable.Columns.MimeMessageId, mimeMessageId)
                        .Where(MailTable.Columns.MailboxId, mailboxId));
            }
        }

        private List<object[]> GetMessagesInfo(IDbManager db, int tenant, string user,
                                               List<int> ids, string[] columns)
        {
            if (!ids.Any())
                throw new ArgumentException("ids are empty");

            return db.ExecuteList(
                new SqlQuery(MailTable.Name)
                    .Select(columns)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailTable.Columns.Id, ids)));
        }

        private static SqlQuery GetQueryForChainMessagesSelection(int mailboxId, string chainId, List<int> searchFolders)
        {
            return new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.Id)
                .Where(MailTable.Columns.MailboxId, mailboxId)
                .Where(MailTable.Columns.ChainId, chainId)
                .Where(Exp.In(MailTable.Columns.Folder, searchFolders.ToArray()))
                .Where(MailTable.Columns.IsRemoved, 0);
        }

        private List<MailAttachment> FixHtmlBodyWithEmbeddedAttachments(MailDraft draft)
        {
            var embededAttachmentsForSaving = new List<MailAttachment>();

            var embeddedLinks = draft.GetEmbeddedAttachmentLinks();
            if (!embeddedLinks.Any())
                return embededAttachmentsForSaving;

            var fckStorage = StorageManager.GetDataStoreForCkImages(draft.Mailbox.TenantId);
            var attachmentStorage = StorageManager.GetDataStoreForAttachments(draft.Mailbox.TenantId);
            var currentMailFckeditorUrl = fckStorage.GetUri(StorageManager.CKEDITOR_IMAGES_DOMAIN, "").ToString();
            var currentUserStorageUrl = MailStoragePathCombiner.GetUserMailsDirectory(draft.Mailbox.UserId);

            StorageManager storage = null;
            foreach (var embeddedLink in embeddedLinks)
            {
                try
                {
                    var isFckImage = embeddedLink.StartsWith(currentMailFckeditorUrl);
                    var prefixLength = isFckImage
                        ? currentMailFckeditorUrl.Length
                        : embeddedLink.IndexOf(currentUserStorageUrl, StringComparison.Ordinal) +
                          currentUserStorageUrl.Length + 1;
                    var fileLink = HttpUtility.UrlDecode(embeddedLink.Substring(prefixLength));
                    var fileName = Path.GetFileName(fileLink);
                    var attach = new MailAttachment
                    {
                        fileName = fileName,
                        storedName = fileName,
                        contentId = embeddedLink.GetMd5(),
                        storedFileUrl = fileLink,
                        streamId = draft.StreamId,
                        user = draft.Mailbox.UserId,
                        tenant = draft.Mailbox.TenantId,
                        mailboxId = draft.Mailbox.MailBoxId
                    };

                    var savedAttachmentId = GetAttachmentId(draft.Id, attach.contentId);
                    var attachmentWasSaved = savedAttachmentId != 0;
                    var currentImgStorage = isFckImage ? fckStorage : attachmentStorage;
                    var domain = isFckImage ? StorageManager.CKEDITOR_IMAGES_DOMAIN : draft.Mailbox.UserId;

                    if (draft.Id == 0 || !attachmentWasSaved)
                    {
                        attach.data = StorageManager.LoadDataStoreItemData(domain, fileLink, currentImgStorage);

                        if (storage == null)
                        {
                            storage = new StorageManager(draft.Mailbox.TenantId, draft.Mailbox.UserId);
                        }

                        storage.StoreAttachmentWithoutQuota(attach);

                        embededAttachmentsForSaving.Add(attach);
                    }

                    if (attachmentWasSaved)
                    {
                        attach = GetMessageAttachment(savedAttachmentId, draft.Mailbox.TenantId, draft.Mailbox.UserId);
                        var path = MailStoragePathCombiner.GerStoredFilePath(attach);
                        currentImgStorage = attachmentStorage;
                        attach.storedFileUrl =
                            MailStoragePathCombiner.GetStoredUrl(currentImgStorage.GetUri(path));
                    }

                    draft.HtmlBody = draft.HtmlBody.Replace(embeddedLink, attach.storedFileUrl);

                }
                catch (Exception ex)
                {
                    _log.Error("ChangeEmbededAttachmentLinksForStoring() failed with exception: {0}", ex.ToString());
                }
            }

            return embededAttachmentsForSaving;
        }

        #endregion
    }
}
