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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Expressions.Conversation;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using MimeKit;
using MySql.Data.MySqlClient;

namespace ASC.Mail.Core.Engine
{
    public class MessageEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public EngineFactory Factory { get; private set; }

        public MessageEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.MessageEngine");

            Factory = new EngineFactory(Tenant, User, Log);
        }

        public MailMessageData GetMessage(int messageId, MailMessageData.Options options)
        {
            using (var daoFactory = new DaoFactory())
            {
                return GetMessage(daoFactory, messageId, options);
            }
        }

        public MailMessageData GetMessage(IDaoFactory daoFactory, int messageId, MailMessageData.Options options)
        {
            var daoMail = daoFactory.CreateMailDao(Tenant, User);

            var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User, !options.OnlyUnremoved));

            return GetMessage(daoFactory, mail, options);
        }

        public MailMessageData GetNextMessage(int messageId, MailMessageData.Options options)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetNextMail(new ConcreteNextUserMessageExp(messageId, Tenant, User, !options.OnlyUnremoved));

                return GetMessage(daoFactory, mail, options);
            }
        }

        public Stream GetMessageStream(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetMail(new ConcreteUserMessageExp(id, Tenant, User, false));

                if (mail == null)
                    throw new ArgumentException("Message not found with id=" + id);

                var dataStore = MailDataStore.GetDataStore(Tenant);

                var key = MailStoragePathCombiner.GetBodyKey(User, mail.Stream);

                return dataStore.GetReadStream(string.Empty, key);
            }
        }

        public Tuple<int, int> GetRangeMessages(IMessagesExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                return daoMailInfo.GetRangeMails(exp);
            }
        }

        private MailMessageData GetMessage(IDaoFactory daoFactory, Entities.Mail mail, MailMessageData.Options options)
        {
            if (mail == null)
                return null;

            var daoTagMail = daoFactory.CreateTagMailDao(Tenant, User);

            var tagIds = daoTagMail.GetTagIds(new List<int> { mail.Id });

            var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

            var attachments = daoAttachment.GetAttachments(
                new ConcreteMessageAttachmentsExp(mail.Id, Tenant, User,
                    onlyEmbedded: options.LoadEmebbedAttachements));

            return ToMailMessage(mail, tagIds, attachments, options);
        }

        public List<MailMessageData> GetFilteredMessages(MailSearchFilterData filter, out long totalMessagesCount)
        {
            var res = new List<MailMessageData>();

            var ids = new List<int>();

            long total = 0;

            if (filter.UserFolderId.HasValue && Factory.UserFolderEngine.Get((uint)filter.UserFolderId.Value) == null)
                throw new ArgumentException("Folder not found");

            if (!filter.IsDefault() && FactoryIndexer<MailWrapper>.Support && FactoryIndexer.CheckState(false))
            {
                if (FilterMessagesExp.TryGetFullTextSearchIds(filter, User, out ids, out total))
                {
                    if (!ids.Any())
                    {
                        totalMessagesCount = 0;
                        return res;
                    }
                }
            }

            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                IMessagesExp exp;

                var tenantInfo = CoreContext.TenantManager.GetTenant(Tenant);
                var utcNow = DateTime.UtcNow;

                if (ids.Any())
                {
                    var pageSize = filter.PageSize.GetValueOrDefault(25);
                    var page = filter.Page.GetValueOrDefault(1);

                    exp = SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(ids)
                            .SetOrderBy(filter.Sort)
                            .SetOrderAsc(filter.SortOrder == Defines.ASCENDING)
                            .SetLimit(pageSize)
                            .Build();

                    var list = daoMailInfo.GetMailInfoList(exp)
                        .ConvertAll(m => ToMailMessage(m, tenantInfo, utcNow));

                    var pagedCount = (list.Count + page * pageSize);

                    totalMessagesCount = page == 0 ? total : total - pagedCount;

                    return list;
                }
                else
                {
                    exp = new FilterMessagesExp(ids, Tenant, User, filter);

                    if (filter.IsDefault())
                    {
                        var folders = daoFactory.CreateFolderDao(Tenant, User).GetFolders();

                        var currentFolder =
                            folders.FirstOrDefault(f => f.FolderType == filter.PrimaryFolder);

                        if (currentFolder != null && currentFolder.FolderType == FolderType.UserFolder)
                        {
                            totalMessagesCount = daoMailInfo.GetMailInfoTotal(exp);
                        }
                        else
                        {
                            totalMessagesCount = currentFolder == null
                                ? 0
                                : filter.Unread.HasValue
                                    ? filter.Unread.Value
                                        ? currentFolder.UnreadCount
                                        : currentFolder.TotalCount - currentFolder.UnreadCount
                                    : currentFolder.TotalCount;
                        }
                    }
                    else
                    {
                        totalMessagesCount = daoMailInfo.GetMailInfoTotal(exp);
                    }

                    if (totalMessagesCount == 0)
                        return res;

                    var list = daoMailInfo.GetMailInfoList(exp)
                        .ConvertAll(m => ToMailMessage(m, tenantInfo, utcNow));

                    return list;
                }
            }
        }

        public List<MailMessageData> GetFilteredMessages(MailSieveFilterData filter, int page, int pageSize, out long totalMessagesCount)
        {
            if(filter == null)
                throw new ArgumentNullException("filter");

            var res = new List<MailMessageData>();
            long total = 0;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                List<int> ids;
                if (FilterSieveMessagesExp.TryGetFullTextSearchIds(filter, User, out ids, out total))
                {
                    if (!ids.Any())
                    {
                        totalMessagesCount = 0;
                        return res;
                    }
                }

                var exp = new FilterSieveMessagesExp(ids, Tenant, User, filter, page, pageSize);

                totalMessagesCount = ids.Any() ? total : daoMailInfo.GetMailInfoTotal(exp);

                if (totalMessagesCount == 0)
                {
                    return res;
                }

                var tenantInfo = CoreContext.TenantManager.GetTenant(Tenant);
                var utcNow = DateTime.UtcNow;

                var list = daoMailInfo.GetMailInfoList(exp)
                    .ConvertAll(m => ToMailMessage(m, tenantInfo, utcNow));

                return list;
            }
        }

        public int GetNextFilteredMessageId(int messageId, MailSearchFilterData filter)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User, false));

                if (mail == null)
                    return -1;

                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                if (FactoryIndexer<MailWrapper>.Support && FactoryIndexer.CheckState(false))
                {
                    List<int> ids;
                    long total = 0;
                    if (FilterMessagesExp.TryGetFullTextSearchIds(filter, User, out ids, out total, mail.DateSent))
                    {
                        if (!ids.Any())
                            return -1;

                        return ids.Where(id => id != messageId)
                            .DefaultIfEmpty(-1)
                            .First();
                    }
                }

                var exp = new FilterNextMessageExp(mail.DateSent, Tenant, User, filter);

                var list = daoMailInfo.GetMailInfoList(exp);

                return list.Where(m => m.Id != messageId)
                    .Select(m => m.Id)
                    .DefaultIfEmpty(-1)
                    .First();
            }
        }

        //TODO: Simplify
        public bool SetUnread(List<int> ids, bool unread, bool allChain = false)
        {
            var factory = new EngineFactory(Tenant, User);

            var ids2Update = new List<int>();
            List<MailInfo> chainedMessages;

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                    chainedMessages = factory.ChainEngine.GetChainedMessagesInfo(daoFactory, ids);

                    if (!chainedMessages.Any())
                        return true;

                    var listIds = allChain
                        ? chainedMessages.Where(x => x.IsNew == !unread).Select(x => x.Id).ToList()
                        : ids;

                    if (!listIds.Any())
                        return true;

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(listIds)
                            .Build(),
                        MailTable.Columns.Unread,
                        unread);

                    var sign = unread ? 1 : -1;

                    var folderConvMessCounters = new List<Tuple<FolderType, int, int>>();

                    var fGroupedChains = chainedMessages.GroupBy(m => new {m.ChainId, m.Folder, m.MailboxId});

                    uint? userFolder = null;

                    var userFolderXmailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);

                    if (chainedMessages.Any(m => m.Folder == FolderType.UserFolder))
                    {
                        var item = userFolderXmailDao.Get(ids.First());
                        userFolder = item == null ? (uint?) null : item.FolderId;
                    }

                    foreach (var fChainMessages in fGroupedChains)
                    {
                        var chainUnreadBefore = fChainMessages.Any(m => m.IsNew);

                        var firstFlag = true;

                        var unreadMessDiff = 0;

                        foreach (var m in fChainMessages.Where(m => listIds.Contains(m.Id) && m.IsNew != unread))
                        {
                            m.IsNew = unread;

                            unreadMessDiff++;

                            if (!firstFlag)
                                continue;

                            ids2Update.Add(m.Id);

                            firstFlag = false;
                        }

                        var chainUnreadAfter = fChainMessages.Any(m => m.IsNew);

                        var unreadConvDiff = chainUnreadBefore == chainUnreadAfter ? 0 : 1;

                        var tplFolderIndex =
                            folderConvMessCounters.FindIndex(tpl => tpl.Item1 == fChainMessages.Key.Folder);

                        if (tplFolderIndex == -1)
                        {
                            folderConvMessCounters.Add(
                                Tuple.Create(fChainMessages.Key.Folder,
                                    unreadMessDiff,
                                    unreadConvDiff));
                        }
                        else
                        {
                            var tplFolder = folderConvMessCounters[tplFolderIndex];

                            folderConvMessCounters[tplFolderIndex] = Tuple.Create(fChainMessages.Key.Folder,
                                tplFolder.Item2 + unreadMessDiff,
                                tplFolder.Item3 + unreadConvDiff);
                        }
                    }

                    foreach (var f in folderConvMessCounters)
                    {
                        var folder = f.Item1;

                        var unreadMessDiff = f.Item2 != 0 ? sign*f.Item2 : (int?) null;
                        var unreadConvDiff = f.Item3 != 0 ? sign*f.Item3 : (int?) null;

                        factory.FolderEngine.ChangeFolderCounters(daoFactory, folder, userFolder,
                            unreadMessDiff, unreadConvDiff: unreadConvDiff);
                    }

                    foreach (var id in ids2Update)
                        factory.ChainEngine.UpdateMessageChainUnreadFlag(daoFactory, Tenant, User, id);

                    if (userFolder.HasValue)
                    {
                        var userFoldersIds = userFolderXmailDao.GetList(mailIds: chainedMessages.Select(m => m.Id).ToList())
                            .Select(ufxm => (int)ufxm.FolderId)
                            .Distinct()
                            .ToList();

                        factory.UserFolderEngine.RecalculateCounters(daoFactory, userFoldersIds);
                    }

                    tx.Commit();

                }
            }

            var data = new MailWrapper
            {
                Unread = unread
            };

            ids2Update = allChain ? chainedMessages.Select(m => m.Id).ToList() : ids;

            factory.IndexEngine.Update(data, s => s.In(m => m.Id, ids2Update.ToArray()), wrapper => wrapper.Unread);

            return true;
        }

        public bool SetImportant(List<int> ids, bool importance)
        {
            var factory = new EngineFactory(Tenant, User);

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(ids)
                            .Build(),
                        MailTable.Columns.Importance,
                        importance);

                    foreach (var messageId in ids)
                        factory.ChainEngine.UpdateMessageChainImportanceFlag(daoFactory, Tenant, User, messageId);

                    tx.Commit();
                }
            }

            var data = new MailWrapper
            {
                Importance = importance
            };

            factory.IndexEngine.Update(data, s => s.In(m => m.Id, ids.ToArray()), wrapper => wrapper.Importance);

            return true;
        }

        public void Restore(List<int> ids)
        {
            List<MailInfo> mailInfoList;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                mailInfoList = daoMailInfo.GetMailInfoList(
                    SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(ids)
                            .Build());

                if (!mailInfoList.Any())
                    return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    Restore(daoFactory, mailInfoList);
                    tx.Commit();
                }
            }

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            var mails = mailInfoList.ConvertAll(m => new MailWrapper
            {
                Id = m.Id,
                Folder = (byte) m.FolderRestore
            });

            Factory.IndexEngine.Update(mails, wrapper => wrapper.Folder);
        }

        //TODO: Simplify
        public void Restore(IDaoFactory daoFactory, List<MailInfo> mailsInfo)
        {
            if (!mailsInfo.Any())
                return;

            var engine = new EngineFactory(Tenant, User);

            var uniqueChainInfo = mailsInfo
                .ConvertAll(x => new
                {
                    folder = x.Folder,
                    chain_id = x.ChainId,
                    id_mailbox = x.MailboxId
                })
                .Distinct();

            var prevInfo = mailsInfo.ConvertAll(x => new
            {
                id = x.Id,
                unread = x.IsNew,
                folder = x.Folder,
                folder_restore = x.FolderRestore,
                chain_id = x.ChainId,
                id_mailbox = x.MailboxId
            });

            var ids = mailsInfo.ConvertAll(x => x.Id);

            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            daoMailInfo.SetFieldsEqual(
                SimpleMessagesExp.CreateBuilder(Tenant, User)
                    .SetMessageIds(ids)
                    .Build(),
                MailTable.Columns.FolderRestore,
                MailTable.Columns.Folder);

            // Update chains in old folder
            foreach (var info in uniqueChainInfo)
                engine.ChainEngine.UpdateChain(daoFactory, info.chain_id, info.folder, null, info.id_mailbox, Tenant, User);

            var unreadMessagesCountCollection = new Dictionary<FolderType, int>();
            var totalMessagesCountCollection = new Dictionary<FolderType, int>();

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
            engine.ChainEngine.UpdateChainFields(daoFactory, Tenant, User, ids);

            var prevTotalUnreadCount = 0;
            var prevTotalCount = 0;

            int? totalMessDiff;
            int? unreadMessDiff;
            foreach (var keyPair in totalMessagesCountCollection)
            {
                var folderRestore = keyPair.Key;
                var totalRestore = keyPair.Value;

                totalMessDiff = totalRestore != 0 ? totalRestore : (int?) null;

                int unreadRestore;
                unreadMessagesCountCollection.TryGetValue(folderRestore, out unreadRestore);

                unreadMessDiff = unreadRestore != 0 ? unreadRestore : (int?) null;

                engine.FolderEngine.ChangeFolderCounters(daoFactory, folderRestore, null,
                    unreadMessDiff, totalMessDiff);

                prevTotalUnreadCount -= unreadRestore;
                prevTotalCount -= totalRestore;
            }

            // Subtract the restored number of messages in the previous folder

            unreadMessDiff = prevTotalUnreadCount != 0 ? prevTotalUnreadCount : (int?) null;
            totalMessDiff = prevTotalCount != 0 ? prevTotalCount : (int?) null;

            engine.FolderEngine.ChangeFolderCounters(daoFactory, prevInfo[0].folder, null,
                unreadMessDiff, totalMessDiff);
        }

        public void SetFolder(List<int> ids, FolderType folder, uint? userFolderId = null)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            if (userFolderId.HasValue && folder != FolderType.UserFolder)
            {
                folder = FolderType.UserFolder;
            }

            using (var daoFactory = new DaoFactory())
            {

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetFolder(daoFactory, ids, folder, userFolderId);
                    tx.Commit();
                }
            }

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            var data = new MailWrapper
            {
                Folder = (byte)folder,
                UserFolders = userFolderId.HasValue
                    ? new List<UserFolderWrapper>
                    {
                        new UserFolderWrapper
                        {
                            Id = (int) userFolderId.Value
                        }
                    }
                    : new List<UserFolderWrapper>()
            };

            Expression<Func<Selector<MailWrapper>, Selector<MailWrapper>>> exp =
                s => s.In(m => m.Id, ids.ToArray());

            Factory.IndexEngine.Update(data, exp, w => w.Folder);

            Factory.IndexEngine.Update(data, exp, UpdateAction.Replace, w => w.UserFolders);
        }

        public void SetFolder(IDaoFactory daoFactory, List<int> ids, FolderType toFolder,
            uint? toUserFolderId = null)
        {
            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            var query = SimpleMessagesExp.CreateBuilder(Tenant, User)
                        .SetMessageIds(ids)
                        .Build();

            var mailInfoList = daoMailInfo.GetMailInfoList(query);

            if (!mailInfoList.Any()) return;

            SetFolder(daoFactory, mailInfoList, toFolder, toUserFolderId);
        }

        public void SetFolder(IDaoFactory daoFactory, List<MailInfo> mailsInfo, FolderType toFolder,
            uint? toUserFolderId = null)
        {
            if (!mailsInfo.Any())
                return;

            var engine = new EngineFactory(Tenant, User);
            
            if(toUserFolderId.HasValue && engine.UserFolderEngine.Get(toUserFolderId.Value) == null)
                throw new ArgumentException("Folder not found");

            var userFolderXmailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);

            var messages = mailsInfo.ConvertAll(x =>
            {
                var srcUserFolderId = (uint?)null;

                if (x.Folder == FolderType.UserFolder)
                {
                    var item = userFolderXmailDao.Get(x.Id);
                    srcUserFolderId = item == null ? (uint?)null : item.FolderId;
                }

                return new
                {
                    id = x.Id,
                    unread = x.IsNew,
                    folder = x.Folder,
                    userFolderId = srcUserFolderId,
                    chain_id = x.ChainId,
                    id_mailbox = x.MailboxId
                };
            })
            .Where(m => m.folder != toFolder || m.userFolderId != toUserFolderId)
            .ToList();

            if(!messages.Any())
                return;

            var uniqueChainInfo = messages
                .ConvertAll(x => new
                {
                    x.folder,
                    x.userFolderId,
                    x.chain_id,
                    x.id_mailbox
                })
                .Distinct();

            var prevInfo = messages.ConvertAll(x => new
            {
                x.id,
                x.unread,
                x.folder,
                x.userFolderId,
                x.chain_id,
                x.id_mailbox
            });

            var ids = messages.Select(x => x.id).ToList();

            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            var updateQuery = SimpleMessagesExp.CreateBuilder(Tenant, User)
                    .SetMessageIds(ids)
                    .Build();

            daoMailInfo.SetFieldValue(updateQuery,
                MailTable.Columns.Folder,
                toFolder);

            if (toUserFolderId.HasValue)
            {
                engine.UserFolderEngine.SetFolderMessages(daoFactory, toUserFolderId.Value, ids);
            }
            else if (prevInfo.Any(x => x.userFolderId.HasValue))
            {
                var prevIds = prevInfo.Where(x => x.userFolderId.HasValue).Select(x => x.id).ToList();

                engine.UserFolderEngine.DeleteFolderMessages(daoFactory, prevIds);
            }

            foreach (var info in uniqueChainInfo)
            {
                engine.ChainEngine.UpdateChain(daoFactory,
                    info.chain_id,
                    info.folder,
                    info.userFolderId,
                    info.id_mailbox,
                    Tenant, User);
            }

            var totalMessages = prevInfo.GroupBy(x => new {x.folder, x.userFolderId})
                .Select(group => new {group.Key, Count = group.Count()});

            var unreadMessages = prevInfo.Where(x => x.unread)
                .GroupBy(x => new {x.folder, x.userFolderId})
                .Select(group => new {group.Key, Count = group.Count()})
                .ToList();

            engine.ChainEngine.UpdateChainFields(daoFactory, Tenant, User, ids);

            var movedTotalUnreadCount = 0;
            var movedTotalCount = 0;
            int? totalMessDiff;
            int? unreadMessDiff;

            foreach (var keyPair in totalMessages)
            {
                var srcFolder = keyPair.Key.folder;
                var srcUserFolder = keyPair.Key.userFolderId;
                var totalMove = keyPair.Count;

                var unreadItem = unreadMessages.FirstOrDefault(
                        x => x.Key.folder == srcFolder && x.Key.userFolderId == srcUserFolder);

                var unreadMove = unreadItem != null ? unreadItem.Count : 0;  

                unreadMessDiff = unreadMove != 0 ? unreadMove*(-1) : (int?) null;
                totalMessDiff = totalMove != 0 ? totalMove*(-1) : (int?) null;

                engine.FolderEngine.ChangeFolderCounters(daoFactory, srcFolder, srcUserFolder,
                    unreadMessDiff, totalMessDiff);

                movedTotalUnreadCount += unreadMove;
                movedTotalCount += totalMove;
            }

            unreadMessDiff = movedTotalUnreadCount != 0 ? movedTotalUnreadCount : (int?) null;
            totalMessDiff = movedTotalCount != 0 ? movedTotalCount : (int?) null;

            engine.FolderEngine.ChangeFolderCounters(daoFactory, toFolder, toUserFolderId,
                unreadMessDiff, totalMessDiff);

            // Correction of UserFolders counters

            var userFolderIds = prevInfo.Where(x => x.folder == FolderType.UserFolder)
                .Select(x => (int)x.userFolderId.Value)
                .Distinct()
                .ToList();

            if (userFolderIds.Count() == 0 && !toUserFolderId.HasValue) // Only for movement from/to UserFolders
                return;

            if(toUserFolderId.HasValue)
                userFolderIds.Add((int)toUserFolderId.Value);

            engine.UserFolderEngine.RecalculateCounters(daoFactory, userFolderIds);
        }

        public void SetRemoved(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            long usedQuota;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                var mailInfoList =
                    daoMailInfo.GetMailInfoList(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(ids)
                            .Build());

                if (!mailInfoList.Any()) return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    usedQuota = SetRemoved(daoFactory, mailInfoList);
                    tx.Commit();
                }
            }

            var engine = new EngineFactory(Tenant);
            engine.QuotaEngine.QuotaUsedDelete(usedQuota);

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            Factory.IndexEngine.Remove(ids, Tenant, new Guid(User));
        }

        public long SetRemoved(IDaoFactory daoFactory, List<MailInfo> deleteInfo)
        {
            if (!deleteInfo.Any())
                return 0;

            var messageFieldsInfo = deleteInfo
                .ConvertAll(r =>
                    new
                    {
                        id = r.Id,
                        folder = r.Folder,
                        unread = r.IsNew
                    });

            var ids = messageFieldsInfo.Select(m => m.id).ToList();

            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            daoMailInfo.SetFieldValue(
                SimpleMessagesExp.CreateBuilder(Tenant, User)
                    .SetMessageIds(ids)
                    .Build(),
                MailTable.Columns.IsRemoved,
                true);

            var exp = new ConcreteMessagesAttachmentsExp(ids, Tenant, User, onlyEmbedded: null);

            var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

            var usedQuota = daoAttachment.GetAttachmentsSize(exp);

            daoAttachment.SetAttachmnetsRemoved(exp);

            var tagDao = daoFactory.CreateTagDao(Tenant, User);
            var tagMailDao = daoFactory.CreateTagMailDao(Tenant, User);

            var tagIds = tagMailDao.GetTagIds(ids.ToList());

            tagMailDao.DeleteByMailIds(tagIds);

            foreach (var tagId in tagIds)
            {
                var tag = tagDao.GetTag(tagId);

                if (tag == null)
                    continue;

                var count = tagMailDao.CalculateTagCount(tag.Id);

                tag.Count = count;

                tagDao.SaveTag(tag);
            }

            var totalCollection = (from row in messageFieldsInfo
                group row by row.folder
                into g
                select new {id = g.Key, diff = -g.Count()})
                .ToList();

            var unreadCollection = (from row in messageFieldsInfo.Where(m => m.unread)
                group row by row.folder
                into g
                select new {id = g.Key, diff = -g.Count()})
                .ToList();

            var engine = new EngineFactory(Tenant, User);

            foreach (var folder in totalCollection)
            {
                var unreadInFolder = unreadCollection
                    .FirstOrDefault(f => f.id == folder.id);

                var unreadMessDiff = unreadInFolder != null ? unreadInFolder.diff : (int?) null;
                var totalMessDiff = folder.diff != 0 ? folder.diff : (int?) null;

                engine.FolderEngine.ChangeFolderCounters(daoFactory, folder.id, null,
                    unreadMessDiff, totalMessDiff);
            }

            engine.ChainEngine.UpdateChainFields(daoFactory, Tenant, User,
                messageFieldsInfo.Select(m => Convert.ToInt32(m.id)).ToList());

            return usedQuota;
        }

        public void SetRemoved(FolderType folder)
        {
            long usedQuota;

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                var mailInfoList = daoMailInfo.GetMailInfoList(
                    SimpleMessagesExp.CreateBuilder(Tenant, User)
                        .SetFolder((int) folder)
                        .Build());

                if (!mailInfoList.Any()) return;

                var ids = mailInfoList.Select(m => m.Id).ToList();

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetFolder((int) folder)
                            .Build(),
                        MailTable.Columns.IsRemoved,
                        true);

                    var exp = new ConcreteMessagesAttachmentsExp(ids, Tenant, User, onlyEmbedded: null);

                    var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

                    usedQuota = daoAttachment.GetAttachmentsSize(exp);

                    daoAttachment.SetAttachmnetsRemoved(exp);

                    var tagDao = daoFactory.CreateTagDao(Tenant, User);
                    var tagMailDao = daoFactory.CreateTagMailDao(Tenant, User);

                    var tagIds = tagMailDao.GetTagIds(ids.ToList());

                    tagMailDao.DeleteByMailIds(tagIds);

                    foreach (var tagId in tagIds)
                    {
                        var tag = tagDao.GetTag(tagId);

                        if (tag == null)
                            continue;

                        var count = tagMailDao.CalculateTagCount(tag.Id);

                        tag.Count = count;

                        tagDao.SaveTag(tag);
                    }

                    var daoChain = daoFactory.CreateChainDao(Tenant, User);

                    daoChain.Delete(SimpleConversationsExp.CreateBuilder(Tenant, User)
                        .SetFolder((int) folder)
                        .Build());

                    var daoFolder = daoFactory.CreateFolderDao(Tenant, User);

                    daoFolder.ChangeFolderCounters(folder, 0, 0, 0, 0);

                    tx.Commit();
                }
            }

            if (usedQuota <= 0)
                return;

            var engine = new EngineFactory(Tenant);
            engine.QuotaEngine.QuotaUsedDelete(usedQuota);
        }

        public int MailSave(MailBoxData mailbox, MailMessageData message,
            int messageId, FolderType folder, FolderType folderRestore, uint? userFolderId,
            string uidl, string md5, bool saveAttachments)
        {
            int id;

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    long usedQuota;

                    id = MailSave(daoFactory, mailbox, message, messageId,
                        folder, folderRestore, userFolderId,
                        uidl, md5, saveAttachments, out usedQuota);

                    tx.Commit();
                }
            }

            return id;
        }

        public int MailSave(IDaoFactory daoFactory, MailBoxData mailbox, MailMessageData message,
            int messageId, FolderType folder, FolderType folderRestore, uint? userFolderId,
            string uidl, string md5, bool saveAttachments, out long usedQuota)
        {
            var countAttachments = 0;
            usedQuota = 0;

            var daoMail = daoFactory.CreateMailDao(mailbox.TenantId, mailbox.UserId);
            var daoAttachment = daoFactory.CreateAttachmentDao(mailbox.TenantId, mailbox.UserId);

            if (messageId != 0)
            {
                countAttachments = daoAttachment.GetAttachmentsCount(
                    new ConcreteMessageAttachmentsExp(messageId, mailbox.TenantId, mailbox.UserId));
            }

            var address = mailbox.EMail.Address.ToLowerInvariant();

            var mail = new Entities.Mail
            {
                Id = messageId,
                Tenant = Tenant,
                User = User,
                MailboxId = mailbox.MailBoxId,
                Address = address,
                From = message.From,
                To = message.To,
                Reply = message.ReplyTo,
                Subject = message.Subject,
                Cc = message.Cc,
                Bcc = message.Bcc,
                Importance = message.Important,
                DateReceived = DateTime.UtcNow,
                DateSent = message.Date.ToUniversalTime(),
                Size = message.Size,
                AttachCount = !saveAttachments
                    ? countAttachments
                    : (message.Attachments != null ? message.Attachments.Count : 0),
                Unread = message.IsNew,
                IsAnswered = message.IsAnswered,
                IsForwarded = message.IsForwarded,
                Stream = message.StreamId,
                Folder = folder,
                FolderRestore = folderRestore,
                Spam = false,
                MimeMessageId = message.MimeMessageId,
                MimeInReplyTo = message.MimeReplyToId,
                ChainId = message.ChainId,
                Introduction = message.Introduction,
                HasParseError = message.HasParseError,
                CalendarUid = message.CalendarUid,
                Uidl = uidl,
                Md5 = md5
            };

            var mailId = daoMail.Save(mail);

            if (messageId == 0)
            {
                var engine = new EngineFactory(Tenant, User);

                var unreadMessDiff = message.IsNew ? 1 : (int?) null;
                engine.FolderEngine.ChangeFolderCounters(daoFactory, folder, userFolderId, unreadMessDiff, 1);

                if (userFolderId.HasValue)
                {
                    engine.UserFolderEngine.SetFolderMessages(daoFactory, userFolderId.Value, new List<int> {mailId});
                }
            }

            if (saveAttachments &&
                message.Attachments != null &&
                message.Attachments.Count > 0)
            {
                var exp = new ConcreteMessageAttachmentsExp(mailId, mailbox.TenantId, mailbox.UserId, onlyEmbedded: null);

                usedQuota = daoAttachment.GetAttachmentsSize(exp);

                daoAttachment.SetAttachmnetsRemoved(exp);

                foreach (var attachment in message.Attachments)
                {
                    var newId = daoAttachment.SaveAttachment(attachment.ToAttachmnet(mailId));
                    attachment.fileId = newId;
                }

                var count = daoAttachment.GetAttachmentsCount(
                                new ConcreteMessageAttachmentsExp(mailId, mailbox.TenantId, mailbox.UserId));

                var daoMailInfo = daoFactory.CreateMailInfoDao(mailbox.TenantId, mailbox.UserId);

                daoMailInfo.SetFieldValue(
                    SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId)
                        .SetMessageId(mailId)
                        .Build(),
                    MailTable.Columns.AttachCount,
                    count);
            }

            if (!string.IsNullOrEmpty(message.FromEmail) && message.FromEmail.Length > 0)
            {
                var tagDao = daoFactory.CreateTagDao(mailbox.TenantId, mailbox.UserId);
                var tagMailDao = daoFactory.CreateTagMailDao(mailbox.TenantId, mailbox.UserId);
                var tagAddressDao = daoFactory.CreateTagAddressDao(mailbox.TenantId, mailbox.UserId);

                if(messageId > 0)
                    tagMailDao.DeleteByMailIds(new List<int> { mailId });

                if (message.TagIds == null)
                    message.TagIds = new List<int>();

                var tagAddressesTagIds = tagAddressDao.GetTagIds(message.FromEmail);

                tagAddressesTagIds.ForEach(tagId =>
                {
                    if (!message.TagIds.Contains(tagId))
                        message.TagIds.Add(tagId);
                });

                if (message.TagIds.Any())
                {
                    foreach (var tagId in message.TagIds)
                    {
                        var tag = tagDao.GetTag(tagId);

                        if (tag == null)
                            continue;

                        tagMailDao.SetMessagesTag(new[] { mailId }, tag.Id);

                        var count = tagMailDao.CalculateTagCount(tag.Id);

                        tag.Count = count;

                        tagDao.SaveTag(tag);
                    }
                }
            }

            UpdateMessagesChains(daoFactory, mailbox, message.MimeMessageId, message.ChainId, folder, userFolderId);

            Log.DebugFormat("MailSave() tenant='{0}', user_id='{1}', email='{2}', from='{3}', id_mail='{4}'",
                mailbox.TenantId, mailbox.UserId, mailbox.EMail, message.From, mailId);

            return mailId;
        }

        public ChainInfo DetectChain(MailBoxData mailbox, string mimeMessageId, string mimeReplyToId, string subject)
        {
            using (var daoFactory = new DaoFactory())
            {
                return DetectChain(daoFactory, mailbox, mimeMessageId, mimeReplyToId, subject);
            }
        }

        public ChainInfo DetectChain(IDaoFactory daoFactory, MailBoxData mailbox, string mimeMessageId,
            string mimeReplyToId, string subject)
        {
            var chainId = mimeMessageId; //Chain id is equal to root conversataions message - MimeMessageId
            var chainDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(mimeMessageId) && !string.IsNullOrEmpty(mimeReplyToId))
            {
                chainId = mimeReplyToId;

                try
                {
                    var daoMailInfo = daoFactory.CreateMailInfoDao(mailbox.TenantId, mailbox.UserId);

                    var chainAndSubject =
                        daoMailInfo.GetMailInfoList(
                            SimpleMessagesExp.CreateBuilder(Tenant, User)
                                .SetMailboxId(mailbox.MailBoxId)
                                .SetMimeMessageId(mimeReplyToId)
                                .Build())
                            .ConvertAll(x => new
                            {
                                chain_id = x.ChainId,
                                subject = x.Subject,
                                chainDate = x.ChainDate
                            })
                            .Distinct()
                            .FirstOrDefault()
                        ?? daoMailInfo.GetMailInfoList(
                            SimpleMessagesExp.CreateBuilder(Tenant, User)
                                .SetMailboxId(mailbox.MailBoxId)
                                .SetChainId(mimeReplyToId)
                                .Build())
                            .ConvertAll(x => new
                            {
                                chain_id = x.ChainId,
                                subject = x.Subject,
                                chainDate = x.ChainDate
                            })
                            .Distinct()
                            .FirstOrDefault();

                    if (chainAndSubject != null)
                    {
                        var chainSubject = MailUtil.NormalizeSubject(chainAndSubject.subject);
                        var messageSubject = MailUtil.NormalizeSubject(subject);

                        if (chainSubject.Equals(messageSubject))
                        {
                            chainId =  chainAndSubject.chain_id;
                            chainDate = chainAndSubject.chainDate;
                        }
                        else
                        {
                            chainId = mimeMessageId;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.WarnFormat(
                        "DetectChainId() params tenant={0}, user_id='{1}', mailbox_id={2}, mime_message_id='{3}' Exception:\r\n{4}",
                        mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, mimeMessageId, ex.ToString());
                }
            }

            Log.DebugFormat(
                "DetectChainId() tenant='{0}', user_id='{1}', mailbox_id='{2}', mime_message_id='{3}' Result: {4}",
                mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, mimeMessageId, chainId);

            return new ChainInfo
            {
                Id = chainId,
                MailboxId = mailbox.MailBoxId,
                ChainDate = chainDate
            };
        }

        //TODO: Need refactoring
        public static MailMessageData Save(MailBoxData mailbox, MimeMessage mimeMessage, string uidl, MailFolder folder, uint? userFolderId, bool unread = true, ILog log = null)
        {
            if (mailbox == null)
                throw new ArgumentException(@"mailbox is null", "mailbox");

            if (mimeMessage == null)
                throw new ArgumentException(@"message is null", "mimeMessage");

            if (uidl == null)
                throw new ArgumentException(@"uidl is null", "uidl");

            if (log == null)
                log = new NullLog();

            var fromEmail = mimeMessage.From.Mailboxes.FirstOrDefault();

            var md5 =
                    string.Format("{0}|{1}|{2}|{3}",
                        mimeMessage.From.Mailboxes.Any() ? mimeMessage.From.Mailboxes.First().Address : "",
                        mimeMessage.Subject, mimeMessage.Date.UtcDateTime, mimeMessage.MessageId).GetMd5();

            var fromThisMailBox = fromEmail != null &&
                                  fromEmail.Address.ToLowerInvariant()
                                      .Equals(mailbox.EMail.Address.ToLowerInvariant());

            var toThisMailBox =
                mimeMessage.To.Mailboxes.Select(addr => addr.Address.ToLowerInvariant())
                    .Contains(mailbox.EMail.Address.ToLowerInvariant());

            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId, log);

            List<int> tagsIds = null;

            if (folder.Tags.Any())
            {
                log.Debug("GetOrCreateTags()");
                tagsIds = engine.TagEngine.GetOrCreateTags(mailbox.TenantId, mailbox.UserId, folder.Tags);
            }

            log.Debug("UpdateExistingMessages()");

            var found = UpdateExistingMessages(mailbox, folder.Folder, uidl, md5,
                mimeMessage.MessageId, mimeMessage.Subject, mimeMessage.Date.UtcDateTime, fromThisMailBox, toThisMailBox, tagsIds, log);

            var needSave = !found;
            if (!needSave)
                return null;

            log.Debug("DetectChainId()");

            var chainInfo = engine.MessageEngine.DetectChain(mailbox, mimeMessage.MessageId, mimeMessage.InReplyTo,
                mimeMessage.Subject);

            var streamId = MailUtil.CreateStreamId();

            log.Debug("Convert MimeMessage->MailMessage");

            var message = mimeMessage.ConvertToMailMessage(folder, unread, chainInfo.Id, chainInfo.ChainDate, streamId,
                mailbox.MailBoxId, true, log);

            log.Debug("TryStoreMailData()");

            if (!TryStoreMailData(message, mailbox, log))
            {
                throw new Exception("Failed to save message");
            }

            log.Debug("MailSave()");

            if (TrySaveMail(mailbox, message, folder, userFolderId, uidl, md5, log))
            {
                return message;
            }

            if (TryRemoveMailDirectory(mailbox, message.StreamId, log))
            {
                log.InfoFormat("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
            }
            else
            {
                throw new Exception("Can't delete mail folder with data");
            }

            return null;
        }
        //TODO: Need refactoring
        public static string StoreMailBody(MailBoxData mailBoxData, MailMessageData messageItem, ILog log)
        {
            if (string.IsNullOrEmpty(messageItem.HtmlBody) && (messageItem.HtmlBodyStream == null || messageItem.HtmlBodyStream.Length == 0))
                return string.Empty;

            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var savePath = MailStoragePathCombiner.GetBodyKey(mailBoxData.UserId, messageItem.StreamId);
            var storage = MailDataStore.GetDataStore(mailBoxData.TenantId);

            storage.QuotaController = null;

            try
            {
                string response;

                if (messageItem.HtmlBodyStream != null && messageItem.HtmlBodyStream.Length > 0)
                {
                    messageItem.HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                    response = storage
                            .Save(savePath, messageItem.HtmlBodyStream, MailStoragePathCombiner.BODY_FILE_NAME)
                            .ToString();
                }
                else
                {
                    using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(messageItem.HtmlBody)))
                    {
                        response = storage
                            .Save(savePath, reader, MailStoragePathCombiner.BODY_FILE_NAME)
                            .ToString();
                    }
                }

                log.DebugFormat("StoreMailBody() tenant='{0}', user_id='{1}', save_body_path='{2}' Result: {3}",
                            mailBoxData.TenantId, mailBoxData.UserId, savePath, response);

                return response;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "StoreMailBody() Problems with message saving in messageId={0}. \r\n Exception: \r\n {1}\r\n",
                    messageItem.MimeMessageId, ex.ToString());

                storage.Delete(string.Empty, savePath);
                throw;
            }
        }
        //TODO: Need refactoring
        public static Dictionary<int, string> GetPop3NewMessagesIDs(MailBoxData mailBox, Dictionary<int, string> uidls,
            int chunk)
        {
            var newMessages = new Dictionary<int, string>();

            if (!uidls.Any() || uidls.Count == mailBox.MessagesCount)
                return newMessages;

            var i = 0;

            var chunkUidls = uidls.Skip(i).Take(chunk).ToList();

            using (var daoFactory = new DaoFactory())
            {
                var daoMail = daoFactory.CreateMailDao(mailBox.TenantId, mailBox.UserId);

                do
                {
                    var checkList = chunkUidls.Select(u => u.Value).Distinct().ToList();

                    var existingUidls = daoMail.GetExistingUidls(mailBox.MailBoxId, checkList);

                    if (!existingUidls.Any())
                    {
                        var messages = newMessages;
                        foreach (var item in
                            chunkUidls.Select(uidl => new KeyValuePair<int, string>(uidl.Key, uidl.Value))
                                .Where(item => !messages.Contains(item)))
                        {
                            newMessages.Add(item.Key, item.Value);
                        }
                    }
                    else if (existingUidls.Count != chunkUidls.Count)
                    {
                        var messages = newMessages;
                        foreach (var item in (from uidl in chunkUidls
                                              where !existingUidls.Contains(uidl.Value)
                                              select new KeyValuePair<int, string>(uidl.Key, uidl.Value)).Where(
                                item => !messages.Contains(item)))
                        {
                            newMessages.Add(item.Key, item.Value);
                        }
                    }

                    i += chunk;

                    chunkUidls = uidls.Skip(i).Take(chunk).ToList();

                } while (chunkUidls.Any());
            }

            return newMessages;
        }

        private void UpdateMessagesChains(IDaoFactory daoFactory, MailBoxData mailbox, string mimeMessageId, string chainId, FolderType folder, uint? userFolderId)
        {
            var chainsForUpdate = new[] {new {id = chainId, folder}};

            // if mime_message_id == chain_id - message is first in chain, because it isn't reply
            if (!string.IsNullOrEmpty(mimeMessageId) && mimeMessageId != chainId)
            {
                var daoChain = daoFactory.CreateChainDao(mailbox.TenantId, mailbox.UserId);

                var chains = daoChain.GetChains(SimpleConversationsExp.CreateBuilder(mailbox.TenantId, mailbox.UserId)
                    .SetMailboxId(mailbox.MailBoxId)
                    .SetChainId(mimeMessageId)
                    .Build())
                    .Select(x => new {id = x.Id, folder = x.Folder})
                    .ToArray();

                if (chains.Any())
                {
                    var daoMailInfo = daoFactory.CreateMailInfoDao(mailbox.TenantId, mailbox.UserId);

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId)
                            .SetChainId(mimeMessageId)
                            .Build(),
                        MailTable.Columns.ChainId,
                        chainId);

                    chainsForUpdate = chains.Concat(chainsForUpdate).ToArray();

                    var newChainsForUpdate =
                        daoMailInfo.GetMailInfoList(
                            SimpleMessagesExp.CreateBuilder(Tenant, User)
                                .SetMailboxId(mailbox.MailBoxId)
                                .SetChainId(chainId)
                                .Build())
                            .ConvertAll(x => new
                            {
                                id = chainId,
                                folder = x.Folder
                            })
                            .Distinct();

                    chainsForUpdate = chainsForUpdate.Concat(newChainsForUpdate).ToArray();
                }
            }

            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);

            foreach (var c in chainsForUpdate.Distinct())
            {
                engine.ChainEngine.UpdateChain(daoFactory, c.id, c.folder, userFolderId, mailbox.MailBoxId,
                    mailbox.TenantId, mailbox.UserId);
            }
        }

        //TODO: Need refactoring
        private static bool TrySaveMail(MailBoxData mailbox, MailMessageData message, MailFolder folder, uint? userFolderId, string uidl, string md5, ILog log)
        {
            try
            {
                var folderRestoreId = folder.Folder == FolderType.Spam ? FolderType.Inbox : folder.Folder;

                var attempt = 1;

                var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId, log);

                while (attempt < 3)
                {
                    try
                    {
                        message.Id = engine.MessageEngine.MailSave(mailbox, message, 0,
                            folder.Folder, folderRestoreId, userFolderId, uidl, md5, true);

                        break;
                    }
                    catch (MySqlException exSql)
                    {
                        if (!exSql.Message.StartsWith("Deadlock found"))
                            throw;

                        if (attempt > 2)
                            throw;

                        log.WarnFormat("[DEADLOCK] MailSave() try again (attempt {0}/2)", attempt);

                        attempt++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TrySaveMail Exception:\r\n{0}\r\n", ex.ToString());
            }

            return false;
        }

        //TODO: Need refactoring
        public static bool TryStoreMailData(MailMessageData message, MailBoxData mailbox, ILog log)
        {
            try
            {
                if (message.Attachments.Any())
                {
                    log.Debug("StoreAttachments()");
                    var index = 0;
                    message.Attachments.ForEach(att =>
                    {
                        att.fileNumber = ++index;
                        att.mailboxId = mailbox.MailBoxId;
                    });

                    var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId, log);

                    engine.AttachmentEngine.StoreAttachments(mailbox, message.Attachments, message.StreamId);

                    log.Debug("MailMessage.ReplaceEmbeddedImages()");
                    message.ReplaceEmbeddedImages(log);
                }

                log.Debug("StoreMailBody()");
                StoreMailBody(mailbox, message, log);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TryStoreMailData(Account:{0}): Exception:\r\n{1}\r\n", mailbox.EMail, ex.ToString());

                //Trying to delete all attachments and mailbody
                if (TryRemoveMailDirectory(mailbox, message.StreamId, log))
                {
                    log.InfoFormat("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
                }

                return false;
            }

            return true;
        }
        //TODO: Need refactoring
        private static bool TryRemoveMailDirectory(MailBoxData mailbox, string streamId, ILog log)
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
                log.DebugFormat(
                    "Problems with mail_directory deleting. Account: {0}. Folder: {1}/{2}/{3}. Exception: {4}",
                    mailbox.EMail, mailbox.TenantId, mailbox.UserId, streamId, ex.ToString());

                return false;
            }
        }
        //TODO: Need refactoring
        private static bool UpdateExistingMessages(MailBoxData mailbox, FolderType folder, string uidl, string md5,
            string mimeMessageId, string subject, DateTime dateSent, bool fromThisMailBox, bool toThisMailBox, List<int> tagsIds, ILog log)
        {
            if ((string.IsNullOrEmpty(md5) || md5.Equals(Defines.MD5_EMPTY)) && string.IsNullOrEmpty(mimeMessageId))
            {
                return false;
            }

            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId, log);

            using (var daoFactory = new DaoFactory())
            {
                var daoMailInfo = daoFactory.CreateMailInfoDao(mailbox.TenantId, mailbox.UserId);

                var builder = SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId, null)
                    .SetMailboxId(mailbox.MailBoxId);

                var exp = (string.IsNullOrEmpty(mimeMessageId)
                    ? builder.SetMd5(md5)
                    : builder.SetMimeMessageId(mimeMessageId))
                    .Build();

                var messagesInfo = daoMailInfo.GetMailInfoList(exp);

                if (!messagesInfo.Any() && folder == FolderType.Sent)
                {
                    exp = SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId, null)
                        .SetMailboxId(mailbox.MailBoxId)
                        .SetFolder((int)FolderType.Sent)
                        .SetSubject(subject)
                        .SetDateSent(dateSent)
                        .Build();

                    messagesInfo = daoMailInfo.GetMailInfoList(exp);
                }

                if (!messagesInfo.Any())
                {
                    return false;
                }

                var idList = messagesInfo.Where(m => !m.IsRemoved).Select(m => m.Id).ToList();
                if (!idList.Any())
                {
                    log.Info("Message already exists and it was removed from portal.");
                    return true;
                }

                if (mailbox.Imap)
                {
                    if (tagsIds != null) // Add new tags to existing messages
                    {
                        using (var tx = daoFactory.DbManager.BeginTransaction())
                        {
                            if (tagsIds.Any(tagId => !engine.TagEngine.SetMessagesTag(daoFactory, idList, tagId)))
                            {
                                tx.Rollback();
                                return false;
                            }

                            tx.Commit();
                        }
                    }

                    if ((!fromThisMailBox || !toThisMailBox) && messagesInfo.Exists(m => m.FolderRestore == folder))
                    {
                        var clone = messagesInfo.FirstOrDefault(m => m.FolderRestore == folder && m.Uidl == uidl);
                        if (clone != null)
                            log.InfoFormat("Message already exists: mailId={0}. Clone", clone.Id);
                        else
                        {
                            var existMessage = messagesInfo.First();

                            if (!existMessage.IsRemoved)
                            {
                                if (string.IsNullOrEmpty(existMessage.Uidl))
                                {
                                    daoMailInfo.SetFieldValue(
                                    SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId)
                                        .SetMessageId(existMessage.Id)
                                        .Build(),
                                    MailTable.Columns.Uidl,
                                    uidl);
                                }
                            }

                            log.Info("Message already exists by MD5|MimeMessageId|Subject|DateSent");
                        }

                        return true;
                    }
                }
                else
                {
                    if (!fromThisMailBox && toThisMailBox && messagesInfo.Count == 1)
                    {
                        log.InfoFormat("Message already exists: mailId={0}. Outbox clone", messagesInfo.First().Id);
                        return true;
                    }
                }

                if (folder == FolderType.Sent)
                {
                    var sentCloneForUpdate =
                        messagesInfo.FirstOrDefault(
                            m => m.FolderRestore == FolderType.Sent && string.IsNullOrEmpty(m.Uidl));

                    if (sentCloneForUpdate != null)
                    {
                        if (!sentCloneForUpdate.IsRemoved)
                        {
                            daoMailInfo.SetFieldValue(
                                SimpleMessagesExp.CreateBuilder(mailbox.TenantId, mailbox.UserId)
                                    .SetMessageId(sentCloneForUpdate.Id)
                                    .Build(),
                                MailTable.Columns.Uidl,
                                uidl);
                        }

                        log.InfoFormat("Message already exists: mailId={0}. Outbox clone", sentCloneForUpdate.Id);

                        return true;
                    }
                }

                if (folder == FolderType.Spam)
                {
                    var first = messagesInfo.First();

                    log.InfoFormat("Message already exists: mailId={0}. It was moved to spam on server", first.Id);

                    return true;
                }

                var fullClone = messagesInfo.FirstOrDefault(m => m.FolderRestore == folder && m.Uidl == uidl);
                if (fullClone == null)
                    return false;

                log.InfoFormat("Message already exists: mailId={0}. Full clone", fullClone.Id);
                return true;

            }
        }

        public static MailMessageData ToMailMessage(MailInfo mailInfo, Tenant tenantInfo, DateTime utcNow)
        {
            var now = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, utcNow);
            var date = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, mailInfo.DateSent);
            var chainDate = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, mailInfo.ChainDate);

            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            return new MailMessageData
            {
                Id = mailInfo.Id,
                From = mailInfo.From,
                To = mailInfo.To,
                Cc = mailInfo.Cc,
                ReplyTo = mailInfo.ReplyTo,
                Subject = mailInfo.Subject,
                Important = mailInfo.Importance,
                Date = date,
                Size = mailInfo.Size,
                HasAttachments = mailInfo.HasAttachments,
                IsNew = mailInfo.IsNew,
                IsAnswered = mailInfo.IsAnswered,
                IsForwarded = mailInfo.IsForwarded,
                LabelsString = mailInfo.LabelsString,
                RestoreFolderId = mailInfo.FolderRestore,
                Folder = mailInfo.Folder,
                ChainId = mailInfo.ChainId ?? "",
                ChainLength = 1,
                ChainDate = chainDate,
                IsToday = isToday,
                IsYesterday = isYesterday,
                MailboxId = mailInfo.MailboxId,
                CalendarUid = mailInfo.CalendarUid,
                Introduction = mailInfo.Intoduction
            };
        }

        protected MailMessageData ToMailMessage(Entities.Mail mail, List<int> tags, List<Attachment> attachments,
            MailMessageData.Options options)
        {
            var now = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(Tenant).TimeZone, DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(Tenant).TimeZone, mail.DateSent);
            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            var item = new MailMessageData
            {
                Id = mail.Id,
                ChainId = mail.ChainId,
                ChainDate = mail.ChainDate,
                Attachments = null,
                Address = mail.Address,
                Bcc = mail.Bcc,
                Cc = mail.Cc,
                Date = date,
                From = mail.From,
                HasAttachments = mail.AttachCount > 0,
                Important = mail.Importance,
                IsAnswered = mail.IsAnswered,
                IsForwarded = mail.IsForwarded,
                IsNew = false,
                TagIds = tags,
                ReplyTo = mail.Reply,
                Size = mail.Size,
                Subject = mail.Subject,
                To = mail.To,
                StreamId = mail.Stream,
                Folder = mail.Folder,
                WasNew = mail.Unread,
                IsToday = isToday,
                IsYesterday = isYesterday,
                Introduction = !string.IsNullOrEmpty(mail.Introduction) ? mail.Introduction.Trim() : "",
                TextBodyOnly = mail.IsTextBodyOnly,
                MailboxId = mail.MailboxId,
                RestoreFolderId = mail.FolderRestore,
                HasParseError = mail.HasParseError,
                MimeMessageId = mail.MimeMessageId,
                MimeReplyToId = mail.MimeInReplyTo,
                CalendarUid = mail.CalendarUid,
                Uidl = mail.Uidl
            };

            //Reassemble paths
            if (options.LoadBody)
            {
                var htmlBody = "";

                if (!item.HasParseError)
                {
#if DEBUG
                    var watch = new Stopwatch();
                    double swtGetBodyMilliseconds;
                    double swtSanitazeilliseconds = 0;
#endif

                    var dataStore = MailDataStore.GetDataStore(Tenant);
                    var key = MailStoragePathCombiner.GetBodyKey(User, item.StreamId);

                    try
                    {
#if DEBUG
                        Log.DebugFormat(
                            "Mail->GetMailInfo(id={0})->Start Body Load tenant: {1}, user: '{2}', key='{3}'",
                            mail.Id, Tenant, User, key);

                        watch.Start();
#endif
                        using (var s = dataStore.GetReadStream(string.Empty, key))
                        {
                            htmlBody = Encoding.UTF8.GetString(s.ReadToEnd());
                        }
#if DEBUG
                        watch.Stop();
                        swtGetBodyMilliseconds = watch.Elapsed.TotalMilliseconds;
                        watch.Reset();
#endif
                        if (options.NeedSanitizer && item.Folder != FolderType.Draft &&
                            !item.From.Equals(Defines.MailDaemonEmail))
                        {
#if DEBUG
                            watch.Start();
#endif
                            bool imagesAreBlocked;

                            Log.DebugFormat(
                                "Mail->GetMailInfo(id={0})->Start Sanitize Body tenant: {1}, user: '{2}', BodyLength: {3} bytes",
                                mail.Id, Tenant, User, htmlBody.Length);

                            htmlBody = HtmlSanitizer.Sanitize(htmlBody, out imagesAreBlocked,
                                new HtmlSanitizer.Options(options.LoadImages, options.NeedProxyHttp));

#if DEBUG
                            watch.Stop();
                            swtSanitazeilliseconds = watch.Elapsed.TotalMilliseconds;
#endif
                            item.ContentIsBlocked = imagesAreBlocked;
                        }
#if DEBUG
                        Log.DebugFormat(
                            "Mail->GetMailInfo(id={0})->Elapsed: BodyLoad={1}ms, Sanitaze={2}ms (NeedSanitizer={3}, NeedProxyHttp={4})",
                            mail.Id, swtGetBodyMilliseconds, swtSanitazeilliseconds, options.NeedSanitizer,
                            options.NeedProxyHttp);
#endif
                    }
                    catch (Exception ex)
                    {
                        item.IsBodyCorrupted = true;
                        htmlBody = "";
                        Log.Error(
                            string.Format("Mail->GetMailInfo(tenant={0} user=\"{1}\" messageId={2} key=\"{3}\")",
                                Tenant, User, mail.Id, key), ex);
#if DEBUG
                        watch.Stop();
                        swtGetBodyMilliseconds = watch.Elapsed.TotalMilliseconds;
                        Log.DebugFormat(
                            "Mail->GetMailInfo(id={0})->Elapsed [BodyLoadFailed]: BodyLoad={1}ms, Sanitaze={2}ms (NeedSanitizer={3}, NeedProxyHttp={4})",
                            mail.Id, swtGetBodyMilliseconds, swtSanitazeilliseconds, options.NeedSanitizer,
                            options.NeedProxyHttp);
#endif
                    }
                }

                item.HtmlBody = htmlBody;
            }
            item.Attachments = attachments.Count != 0
                ? attachments.ConvertAll(AttachmentEngine.ToAttachmentData)
                : new List<MailAttachmentData>();

            return item;
        }
    }
}
