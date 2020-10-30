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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Mail.Core.Dao.Expressions.Conversation;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Engine
{
    public class ChainEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public EngineFactory Factory { get; private set; }

        private const int CHUNK_SIZE = 3;

        public ChainEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ChainEngine");

            Factory = new EngineFactory(Tenant, User, Log);
        }

        public List<Chain> GetChainsById(string id) {
            using (var daoFactory = new DaoFactory())
            {
                var daoChain = daoFactory.CreateChainDao(Tenant, User);

                var exp = SimpleConversationsExp.CreateBuilder(Tenant, User)
                    .SetChainIds(new List<string> { id })
                    .Build();

                return daoChain.GetChains(exp);
            }
        }

        public long GetNextConversationId(int id, MailSearchFilterData filter)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetMail(new ConcreteUserMessageExp(id, Tenant, User));

                if (mail == null)
                    return 0;

                filter.FromDate = mail.ChainDate;
                filter.FromMessage = id;
                filter.PageSize = 1;

                bool hasMore;
                var messages = GetFilteredConversations(daoFactory, filter, out hasMore);
                return messages.Any() ? messages.First().Id : 0;
            }
        }

        public List<MailMessageData> GetConversations(MailSearchFilterData filterData, out bool hasMore)
        {
            if (filterData == null)
                throw new ArgumentNullException("filterData");

            var filter = (MailSearchFilterData) filterData.Clone();

            if (filter.UserFolderId.HasValue && Factory.UserFolderEngine.Get((uint) filter.UserFolderId.Value) == null)
                throw new ArgumentException("Folder not found");

            using (var daoFactory = new DaoFactory())
            {
                var filteredConversations = GetFilteredConversations(daoFactory, filter, out hasMore);

                if (!filteredConversations.Any())
                    return filteredConversations;

                var chainIds = new List<string>();
                filteredConversations.ForEach(x => chainIds.Add(x.ChainId));

                var daoChain = daoFactory.CreateChainDao(Tenant, User);

                var exp = SimpleConversationsExp.CreateBuilder(Tenant, User)
                    .SetChainIds(chainIds)
                    .SetFoldersIds(
                        filter.PrimaryFolder == FolderType.Inbox ||
                        filter.PrimaryFolder == FolderType.Sent
                            ? new List<int> {(int) FolderType.Inbox, (int) FolderType.Sent}
                            : new List<int> {(int) filter.PrimaryFolder})
                    .Build();

                var extendedInfo = daoChain.GetChains(exp);

                foreach (var chain in filteredConversations)
                {
                    var chainMessages = extendedInfo.FindAll(x => x.MailboxId == chain.MailboxId && x.Id == chain.ChainId);
                    if (!chainMessages.Any()) continue;
                    chain.IsNew = chainMessages.Any(x => x.Unread);
                    chain.HasAttachments = chainMessages.Any(x => x.HasAttachments);
                    chain.Important = chainMessages.Any(x => x.Importance);
                    chain.ChainLength = chainMessages.Sum(x => x.Length);
                    var firstOrDefault = chainMessages.FirstOrDefault(x => !string.IsNullOrEmpty(x.Tags));
                    chain.LabelsString = firstOrDefault != null ? firstOrDefault.Tags : "";
                }

                return filteredConversations;
            }
        }

        public List<MailMessageData> GetConversationMessages(int tenant, string user, int messageId,
            bool loadAllContent, bool needProxyHttp, bool needMailSanitazer, bool markRead = false)
        {
            var engine = new EngineFactory(tenant, user);

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

                var messageInfo = daoMailInfo.GetMailInfoList(
                    SimpleMessagesExp.CreateBuilder(tenant, user)
                        .SetMessageId(messageId)
                        .Build())
                    .SingleOrDefault();

                if (messageInfo == null)
                    throw new ArgumentException("Message Id not found");

                var searchFolders = new List<int>(); 

                if (messageInfo.Folder == FolderType.Inbox || messageInfo.Folder == FolderType.Sent)
                    searchFolders.AddRange(new[] {(int) FolderType.Inbox, (int) FolderType.Sent});
                else
                    searchFolders.Add((int) messageInfo.Folder);

                var exp = SimpleMessagesExp.CreateBuilder(tenant, user)
                    .SetMailboxId(messageInfo.MailboxId)
                    .SetChainId(messageInfo.ChainId)
                    .SetFoldersIds(searchFolders)
                    .Build();

                var mailInfoList = daoMailInfo.GetMailInfoList(exp);

                var ids = mailInfoList.Select(m => m.Id).ToList();

                var messages =
                    ids.Select(
                        (id, i) =>
                            engine.MessageEngine.GetMessage(daoFactory, id,
                                new MailMessageData.Options
                                {
                                    LoadImages = false,
                                    LoadBody = loadAllContent || (id == messageId),
                                    NeedProxyHttp = needProxyHttp,
                                    NeedSanitizer = needMailSanitazer
                                }))
                        .Where(mailInfo => mailInfo != null)
                        .OrderBy(m => m.Date)
                        .ToList();

                if (!markRead)
                    return messages;

                var unreadMessages = messages.Where(message => message.WasNew).ToList();
                if (!unreadMessages.Any())
                    return messages;

                var unreadMessagesCountByFolder = new Dictionary<FolderType, int>();

                foreach (var message in unreadMessages)
                {
                    if (unreadMessagesCountByFolder.ContainsKey(message.Folder))
                        unreadMessagesCountByFolder[message.Folder] += 1;
                    else
                        unreadMessagesCountByFolder.Add(message.Folder, 1);
                }

                var userFolderXmailDao = daoFactory.CreateUserFolderXMailDao(tenant, user);

                uint? userFolder = null;

                if (unreadMessagesCountByFolder.Keys.Any(k => k == FolderType.UserFolder))
                {
                    var item = userFolderXmailDao.Get(ids.First());
                    userFolder = item == null ? (uint?)null : item.FolderId;
                }

                List<int> ids2Update;

                using (var tx = db.BeginTransaction())
                {
                    ids2Update = unreadMessages.Select(x => x.Id).ToList();

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(tenant, user)
                            .SetMessageIds(ids2Update)
                            .Build(),
                        MailTable.Columns.Unread,
                        false);

                    var daoChain = daoFactory.CreateChainDao(tenant, user);

                    foreach (var keyPair in unreadMessagesCountByFolder)
                    {
                        var folderType = keyPair.Key;

                        var unreadMessDiff = keyPair.Value != 0 ? keyPair.Value * (-1) : (int?) null;

                        engine.FolderEngine.ChangeFolderCounters(daoFactory, folderType, userFolder,
                            unreadMessDiff, unreadConvDiff: -1);

                        daoChain.SetFieldValue(
                            SimpleConversationsExp.CreateBuilder(tenant, user)
                                .SetChainId(messageInfo.ChainId)
                                .SetMailboxId(messageInfo.MailboxId)
                                .SetFolder((int)keyPair.Key)
                                .Build(),
                            ChainTable.Columns.Unread,
                            false);
                    }

                    if (userFolder.HasValue)
                    {
                        var userFoldersIds = userFolderXmailDao.GetList(mailIds: ids)
                            .Select(ufxm => (int)ufxm.FolderId)
                            .Distinct()
                            .ToList();

                        engine.UserFolderEngine.RecalculateCounters(daoFactory, userFoldersIds);
                    }

                    tx.Commit();
                }

                var data = new MailWrapper
                {
                    Unread = false
                };

                engine.IndexEngine.Update(data, s => s.In(m => m.Id, ids2Update.ToArray()), wrapper => wrapper.Unread);

                return messages;
            }
        }

        public List<MailInfo> GetChainedMessagesInfo(DaoFactory daoFactory, List<int> ids)
        {
            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            var chainsInfo = daoMailInfo.GetMailInfoList(
                SimpleMessagesExp.CreateBuilder(Tenant, User)
                    .SetMessageIds(ids)
                    .Build());

            var chainArray = chainsInfo.Select(r => r.ChainId).Distinct().ToArray();

            const int max_query_count = 25;
            var i = 0;
            var unsortedMessages = new List<MailInfo>();

            do
            {
                var partChains = chainArray.Skip(i).Take(max_query_count).ToList();

                if (!partChains.Any())
                    break;

                var selectedMessages = daoMailInfo.GetMailInfoList(
                    SimpleMessagesExp.CreateBuilder(Tenant, User)
                        .SetChainIds(partChains)
                        .Build());

                unsortedMessages.AddRange(selectedMessages);

                i += max_query_count;

            } while (true);

            var result = unsortedMessages
                .Where(r => chainsInfo.FirstOrDefault(c =>
                    c.ChainId == r.ChainId &&
                    c.MailboxId == r.MailboxId &&
                    ((r.Folder == FolderType.Inbox || r.Folder == FolderType.Sent)
                        ? c.Folder == FolderType.Inbox || c.Folder == FolderType.Sent
                        : c.Folder == r.Folder)) != null)
                .ToList();

            return result;
        }

        public void SetConversationsFolder(List<int> ids, FolderType folder, uint? userFolderId = null)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            var engine = new EngineFactory(Tenant, User, Log);

            List<MailInfo> listObjects;

            using (var daoFactory = new DaoFactory())
            {
                listObjects = GetChainedMessagesInfo(daoFactory, ids);

                if (!listObjects.Any())
                    return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    engine.MessageEngine.SetFolder(daoFactory, listObjects, folder, userFolderId);
                    tx.Commit();
                }
            }

            if (folder == FolderType.Inbox || folder == FolderType.Sent || folder == FolderType.Spam)
                engine.OperationEngine.ApplyFilters(listObjects.Select(o => o.Id).ToList());

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            var data = new MailWrapper
            {
                Folder = (byte) folder,
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
                s => s.In(m => m.Id, listObjects.Select(o => o.Id).ToArray());

            engine.IndexEngine.Update(data, exp, w => w.Folder);

            engine.IndexEngine.Update(data, exp, UpdateAction.Replace, w => w.UserFolders);
        }

        public void RestoreConversations(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            var engine = new EngineFactory(tenant, user, Log);

            List<MailInfo> listObjects;

            using (var daoFactory = new DaoFactory())
            {
                listObjects = GetChainedMessagesInfo(daoFactory, ids);

                if (!listObjects.Any())
                    return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    engine.MessageEngine.Restore(daoFactory, listObjects);
                    tx.Commit();
                }
            }

            var filterApplyIds =
                listObjects.Where(
                    m =>
                        m.FolderRestore == FolderType.Inbox || m.FolderRestore == FolderType.Sent ||
                        m.FolderRestore == FolderType.Spam).Select(m => m.Id).ToList();

            if (filterApplyIds.Any())
                engine.OperationEngine.ApplyFilters(filterApplyIds);

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            var mails = listObjects.ConvertAll(m => new MailWrapper
            {
                Id = m.Id,
                Folder = (byte) m.FolderRestore
            });

            engine.IndexEngine.Update(mails, wrapper => wrapper.Folder);
        }

        public void DeleteConversations(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            long usedQuota;

            var engine = new EngineFactory(tenant, user);

            List<MailInfo> listObjects;

            using (var daoFactory = new DaoFactory())
            {
                listObjects = GetChainedMessagesInfo(daoFactory, ids);

                if (!listObjects.Any())
                    return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    usedQuota = engine.MessageEngine.SetRemoved(daoFactory, listObjects);
                    tx.Commit();
                }
            }

            engine.QuotaEngine.QuotaUsedDelete(usedQuota);

            if (!FactoryIndexer<MailWrapper>.Support)
                return;

            engine.IndexEngine.Remove(listObjects.Select(info => info.Id).ToList(), Tenant, new Guid(User));
        }

        private const string MM_ALIAS = "mm";

        public void SetConversationsImportanceFlags(int tenant, string user, bool important, List<int> ids)
        {
            List<MailInfo> mailInfos;

            using (var daoFactory = new DaoFactory())
            {
                mailInfos = GetChainedMessagesInfo(daoFactory, ids);

                var chainsInfo = mailInfos
                    .Select(m => new
                    {
                        m.ChainId,
                        m.MailboxId,
                        m.Folder
                    })
                    .Distinct().ToList();

                if (!chainsInfo.Any())
                    throw new Exception("no chain messages belong to current user");

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

                    var exp = Exp.Empty;
                    var сhains = new List<Tuple<int, string>>();

                    foreach (var chain in chainsInfo)
                    {
                        var key = new Tuple<int, string>(chain.MailboxId, chain.ChainId);

                        if (сhains.Any() &&
                            сhains.Contains(key) &&
                            (chain.Folder == FolderType.Inbox || chain.Folder == FolderType.Sent))
                        {
                            continue;
                        }

                        var innerWhere = Exp.And(
                            Exp.Eq(MailTable.Columns.ChainId.Prefix(MM_ALIAS), chain.ChainId),
                            Exp.Eq(MailTable.Columns.MailboxId.Prefix(MM_ALIAS), chain.MailboxId));

                        if (chain.Folder == FolderType.Inbox || chain.Folder == FolderType.Sent)
                        {
                            innerWhere &= Exp.Or(
                                Exp.Eq(MailTable.Columns.Folder.Prefix(MM_ALIAS), (int)FolderType.Inbox),
                                Exp.Eq(MailTable.Columns.Folder.Prefix(MM_ALIAS), (int)FolderType.Sent));

                            сhains.Add(key);
                        }
                        else
                        {
                            innerWhere &= Exp.Eq(MailTable.Columns.Folder.Prefix(MM_ALIAS), (int)chain.Folder);
                        }

                        exp |= innerWhere;
                    }

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(tenant, user)
                            .SetExp(exp)
                            .Build(),
                        MailTable.Columns.Importance,
                        important);

                    var daoChain = daoFactory.CreateChainDao(tenant, user);

                    foreach (var chain in chainsInfo)
                    {
                        daoChain.SetFieldValue(
                            SimpleConversationsExp.CreateBuilder(tenant, user)
                                .SetChainId(chain.ChainId)
                                .SetMailboxId(chain.MailboxId)
                                .SetFolder((int)chain.Folder)
                                .Build(),
                            ChainTable.Columns.Importance,
                            important);
                    }

                    tx.Commit();
                }
            }

            var factory = new EngineFactory(Tenant, User);

            var data = new MailWrapper
            {
                Importance = important
            };

            factory.IndexEngine.Update(data, s => s.In(m => m.Id, mailInfos.Select(o => o.Id).ToArray()),
                wrapper => wrapper.Importance);
        }

        public void UpdateMessageChainAttachmentsFlag(IDaoFactory daoFactory, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(daoFactory, tenant, user, messageId, MailTable.Columns.AttachCount, ChainTable.Columns.HasAttachments);
        }

        public void UpdateMessageChainUnreadFlag(IDaoFactory daoFactory, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(daoFactory, tenant, user, messageId, MailTable.Columns.Unread, ChainTable.Columns.Unread);
        }

        public void UpdateMessageChainImportanceFlag(IDaoFactory daoFactory, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(daoFactory, tenant, user, messageId, MailTable.Columns.Importance, ChainTable.Columns.Importance);
        }

        private static void UpdateMessageChainFlag(IDaoFactory daoFactory, int tenant, string user, int messageId, string fieldFrom, string fieldTo)
        {
            var daoMail = daoFactory.CreateMailDao(tenant, user);

            var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, tenant, user));

            if (mail == null)
                return;

            var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

            var maxValue = daoMailInfo.GetFieldMaxValue<bool>(
                SimpleMessagesExp.CreateBuilder(tenant, user)
                    .SetChainId(mail.ChainId)
                    .SetMailboxId(mail.MailboxId)
                    .SetFolder((int) mail.Folder)
                    .Build(),
                fieldFrom);

            var daoChain = daoFactory.CreateChainDao(tenant, user);

            daoChain.SetFieldValue(
                SimpleConversationsExp.CreateBuilder(tenant, user)
                    .SetChainId(mail.ChainId)
                    .SetMailboxId(mail.MailboxId)
                    .SetFolder((int) mail.Folder)
                    .Build(),
                fieldTo,
                maxValue);
        }

        public void UpdateChain(string chainId, FolderType folder, uint? userFolderId, int mailboxId, int tenant, string user)
        {
            using (var daoFactory = new DaoFactory())
            {
                UpdateChain(daoFactory, chainId, folder, userFolderId, mailboxId, tenant, user);
            }
        }

        public void UpdateChainFields(IDaoFactory daoFactory, int tenant, string user, List<int> ids)
        {
            var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

            var mailInfoList = daoMailInfo.GetMailInfoList(
                SimpleMessagesExp.CreateBuilder(tenant, user, null)
                    .SetMessageIds(ids)
                    .Build())
                .ConvertAll(x => new
                {
                    id_mailbox = x.MailboxId,
                    chain_id = x.ChainId,
                    folder = x.Folder
                });

            if (!mailInfoList.Any()) return;

            foreach (var info in mailInfoList.GroupBy(t => new { t.id_mailbox, t.chain_id, t.folder }))
            {
                uint? userFolder = null;

                if (info.Key.folder == FolderType.UserFolder)
                {
                    var userFolderXmailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);
                    var item = userFolderXmailDao.Get(ids.First());
                    userFolder = item == null ? (uint?)null : item.FolderId;
                }

                UpdateChain(daoFactory, info.Key.chain_id, info.Key.folder, userFolder, info.Key.id_mailbox, tenant, user);
            }
        }

        // Method for updating chain flags, date and length.
        public void UpdateChain(IDaoFactory daoFactory, string chainId, FolderType folder, uint? userFolderId, int mailboxId, 
            int tenant, string user)
        {
            if (string.IsNullOrEmpty(chainId)) return;

            var db = daoFactory.DbManager;
            var engine = new EngineFactory(tenant, user, Log);

            var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

            const string m_alias = "m";

            var getChainQuery = new SqlQuery(MailTable.TABLE_NAME.Alias(m_alias))
                .SelectCount()
                .SelectMax(MailTable.Columns.DateSent.Prefix(m_alias))
                .SelectMax(MailTable.Columns.Unread.Prefix(m_alias))
                .SelectMax(MailTable.Columns.AttachCount.Prefix(m_alias))
                .SelectMax(MailTable.Columns.Importance.Prefix(m_alias))
                .Where(MailTable.Columns.Tenant.Prefix(m_alias), tenant)
                .Where(MailTable.Columns.User.Prefix(m_alias), user)
                .Where(MailTable.Columns.IsRemoved.Prefix(m_alias), 0)
                .Where(MailTable.Columns.ChainId.Prefix(m_alias), chainId)
                .Where(MailTable.Columns.MailboxId.Prefix(m_alias), mailboxId)
                .Where(MailTable.Columns.Folder.Prefix(m_alias), (int)folder);

            var chainInfo = db.ExecuteList(getChainQuery)
                .ConvertAll(x => new
                {
                    length = Convert.ToInt32(x[0]),
                    date = Convert.ToDateTime(x[1]),
                    unread = Convert.ToBoolean(x[2]),
                    attach_count = Convert.ToInt32(x[3]),
                    importance = Convert.ToBoolean(x[4])
                })
                .FirstOrDefault();

            if (chainInfo == null)
                throw new InvalidDataException("Conversation is absent in MAIL_MAIL");

            var daoChain = daoFactory.CreateChainDao(tenant, user);

            var query = SimpleConversationsExp.CreateBuilder(tenant, user)
                .SetMailboxId(mailboxId)
                .SetChainId(chainId)
                .SetFolder((int)folder)
                .Build();

            var storedChainInfo = daoChain.GetChains(query);

            var chainUnreadFlag = storedChainInfo.Any(c => c.Unread);

            if (0 == chainInfo.length)
            {
                var deletQuery = SimpleConversationsExp.CreateBuilder(tenant, user)
                    .SetFolder((int)folder)
                    .SetMailboxId(mailboxId)
                    .SetChainId(chainId)
                    .Build();

                var result = daoChain.Delete(deletQuery);

                Log.DebugFormat(
                    "UpdateChain() row deleted from chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}' result={5}",
                    tenant, user, mailboxId, folder, chainId, result);

                var unreadConvDiff = chainUnreadFlag ? -1 : (int?) null;

                engine.FolderEngine.ChangeFolderCounters(daoFactory, folder, userFolderId,
                    unreadConvDiff: unreadConvDiff, totalConvDiff: -1);
            }
            else
            {
                //var updateMailQuery = new SqlUpdate(MailTable.TABLE_NAME)
                //    .Where(MailTable.Columns.Tenant, tenant)
                //    .Where(MailTable.Columns.User, user)
                //    .Where(MailTable.Columns.IsRemoved, 0)
                //    .Where(MailTable.Columns.ChainId, chainId)
                //    .Where(MailTable.Columns.MailboxId, mailboxId)
                //    .Where(MailTable.Columns.Folder, (int) folder)
                //    // Folder condition important because chain has different dates in different folders(Ex: Sent and Inbox).
                //    .Set(MailTable.Columns.ChainDate, chainInfo.date);

                //db.ExecuteNonQuery(updateMailQuery);

                var updateQuery = SimpleMessagesExp.CreateBuilder(tenant, user)
                        .SetChainId(chainId)
                        .SetMailboxId(mailboxId)
                        .SetFolder((int)folder)
                        .Build();

                daoMailInfo.SetFieldValue(updateQuery,
                    MailTable.Columns.ChainDate,
                    chainInfo.date);

                var tags = GetChainTags(daoFactory, chainId, folder, mailboxId, tenant, user);

                var chain = new Chain
                {
                    Id = chainId,
                    Tenant = tenant,
                    User = user,
                    MailboxId = mailboxId,
                    Folder = folder,
                    Length = chainInfo.length,
                    Unread = chainInfo.unread,
                    HasAttachments = chainInfo.attach_count > 0,
                    Importance = chainInfo.importance,
                    Tags = tags
                };

                var result = daoChain.SaveChain(chain);

                if (result <= 0)
                    throw new InvalidOperationException(string.Format("Invalid insert into {0}", ChainTable.TABLE_NAME));

                Log.DebugFormat(
                    "UpdateChain() row inserted to chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}'",
                    tenant, user, mailboxId, folder, chainId);

                var unreadConvDiff = (int?) null;
                var totalConvDiff = (int?) null;

                if (!storedChainInfo.Any())
                {
                    totalConvDiff = 1;
                    unreadConvDiff = chainInfo.unread ? 1 : (int?) null;
                }
                else
                {
                    if (chainUnreadFlag != chainInfo.unread)
                    {
                        unreadConvDiff = chainInfo.unread ? 1 : -1;
                    }
                }

                engine.FolderEngine.ChangeFolderCounters(daoFactory, folder, userFolderId,
                    unreadConvDiff: unreadConvDiff, totalConvDiff: totalConvDiff);
            }
        }

        public void UpdateChainTags(IDaoFactory daoFactory, string chainId, FolderType folder, int mailboxId, int tenant, string user)
        {
            var tags = GetChainTags(daoFactory, chainId, folder, mailboxId, tenant, user);

            var daoChain = daoFactory.CreateChainDao(tenant, user);

            var updateQuery = SimpleConversationsExp.CreateBuilder(tenant, user)
                    .SetChainId(chainId)
                    .SetMailboxId(mailboxId)
                    .SetFolder((int)folder)
                    .Build();

            daoChain.SetFieldValue(
                updateQuery,
                ChainTable.Columns.Tags,
                tags);
        }

        private static string GetChainTags(IDaoFactory daoFactory, string chainId, FolderType folder, int mailboxId, int tenant, string user)
        {
            const string mm_alias = "ch";
            const string mtm_alias = "tm";

            var db = daoFactory.DbManager;

            var newQuery = new SqlQuery(TagMailTable.TABLE_NAME.Alias(mtm_alias))
                .Select(TagMailTable.Columns.TagId.Prefix(mtm_alias))
                .InnerJoin(MailTable.TABLE_NAME.Alias(mm_alias),
                           Exp.EqColumns(MailTable.Columns.Id.Prefix(mm_alias), TagMailTable.Columns.MailId.Prefix(mtm_alias)))
                .Where(MailTable.Columns.ChainId.Prefix(mm_alias), chainId)
                .Where(MailTable.Columns.IsRemoved.Prefix(mm_alias), 0)
                .Where(TagMailTable.Columns.Tenant.Prefix(mtm_alias), tenant)
                .Where(TagMailTable.Columns.User.Prefix(mtm_alias), user)
                .Where(MailTable.Columns.Folder.Prefix(mm_alias), (int) folder)
                .Where(MailTable.Columns.MailboxId.Prefix(mm_alias), mailboxId)
                .GroupBy(1)
                .OrderBy(TagMailTable.Columns.TimeCreated.Prefix(mtm_alias), true);

            var tags = db.ExecuteList(newQuery)
                         .ConvertAll(x => x[0].ToString());

            return string.Join(",", tags.ToArray());
        }

        private List<MailMessageData> GetFilteredConversations(IDaoFactory daoFactory, MailSearchFilterData filter, out bool hasMore)
        {
            var conversations = new List<MailMessageData>();
            var skipFlag = false;
            var chunkIndex = 0;

            if (filter.FromDate.HasValue && filter.FromMessage.HasValue && filter.FromMessage.Value > 0)
            {
                skipFlag = true;
            }

            var prevFlag = filter.PrevFlag.GetValueOrDefault(false);
            var tenantInfo = CoreContext.TenantManager.GetTenant(Tenant);
            var utcNow = DateTime.UtcNow;
            var pageSize = filter.PageSize.GetValueOrDefault(25);

            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            while (conversations.Count < pageSize + 1)
            {
                filter.PageSize = CHUNK_SIZE*pageSize;

                IMessagesExp exp = null;

                if (!filter.IsDefault() && FactoryIndexer<MailWrapper>.Support && FactoryIndexer.CheckState(false))
                {
                    filter.Page = chunkIndex*CHUNK_SIZE*pageSize; // Elastic Limit from {index of last message} to {count of messages}

                    List<MailWrapper> mailWrappers;
                    if (FilterChainMessagesExp.TryGetFullTextSearchChains(filter, User, out mailWrappers))
                    {
                        if (!mailWrappers.Any())
                            break;

                        var ids = mailWrappers.Select(c => c.Id).ToList();

                        var query = SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageIds(ids)
                            .SetOrderBy(filter.Sort);

                        if (prevFlag)
                        {
                            query.SetOrderAsc(!(filter.SortOrder == Defines.ASCENDING));
                        }
                        else {
                            query.SetOrderAsc(filter.SortOrder == Defines.ASCENDING);
                        }

                        exp = query
                            .Build();
                    }
                }
                else
                {
                    filter.Page = chunkIndex; // MySQL Limit from {page by size} to {size}

                    exp = new FilterChainMessagesExp(filter, Tenant, User);
                }

                chunkIndex++;

                var listMessages = daoMailInfo.GetMailInfoList(exp, true)
                    .ConvertAll(m => MessageEngine.ToMailMessage(m, tenantInfo, utcNow));

                if (0 == listMessages.Count)
                    break;

                if (skipFlag && filter.FromMessage.HasValue)
                {
                    var messageData = listMessages.FirstOrDefault(m => m.Id == filter.FromMessage.Value);

                    if (messageData != null)
                    {
                        // Skip chain messages by FromMessage.
                        listMessages =
                            listMessages.Where(
                                m => !(m.ChainId.Equals(messageData.ChainId) && m.MailboxId == messageData.MailboxId))
                                .ToList();
                    }

                    skipFlag = false;
                }

                foreach (var messageData in listMessages)
                {
                    var existingChainIndex =
                        conversations.FindIndex(
                            c => c.ChainId == messageData.ChainId && c.MailboxId == messageData.MailboxId);

                    if (existingChainIndex > -1)
                    {
                        if (conversations[existingChainIndex].Date < messageData.Date)
                            conversations[existingChainIndex] = messageData;
                    }
                    else
                    {
                        conversations.Add(messageData);
                    }
                }

                if (conversations.Count > pageSize)
                    break;
            }

            hasMore = conversations.Count > pageSize;

            if (hasMore)
            {
                conversations = conversations.Take(pageSize).ToList();
            }

            if (prevFlag) {
                conversations.Reverse();
            }

            return conversations;
        }
    }
}
