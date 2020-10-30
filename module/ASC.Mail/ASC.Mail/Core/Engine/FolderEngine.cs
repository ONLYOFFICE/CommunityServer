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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Mail.Core.Dao.Expressions.Conversation;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Expressions.UserFolder;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Engine
{
    public class FolderEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public class MailFolderInfo
        {
            public FolderType id;
            public DateTime timeModified;
            public int unread;
            public int unreadMessages;
            public int total;
            public int totalMessages;
        }

        public EngineFactory Factory { get; private set; }

        public FolderEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.FolderEngine");

            Factory = new EngineFactory(Tenant, User, Log);
        }

        public List<MailFolderInfo> GetFolders()
        {
            List<MailFolderInfo> folders;
            var needRecalculation = false;

            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateFolderDao(Tenant, User);

                var folderList = dao.GetFolders();

                foreach (var folder in DefaultFolders)
                {
                    if (folderList.Exists(f => f.FolderType == folder)) 
                        continue;

                    needRecalculation = true;

                    var newFolder = new Folder
                    {
                        FolderType = folder,
                        Tenant = Tenant,
                        UserId = User,
                        TotalCount = 0,
                        UnreadCount = 0,
                        UnreadChainCount = 0,
                        TotalChainCount = 0,
                        TimeModified = DateTime.UtcNow
                    };

                    dao.Save(newFolder);

                    folderList.Add(newFolder);
                }

                folders = folderList
                    .ConvertAll(x => new MailFolderInfo
                    {
                        id = x.FolderType,
                        timeModified = x.TimeModified,
                        unread = x.UnreadChainCount,
                        unreadMessages = x.UnreadCount,
                        total = x.TotalChainCount,
                        totalMessages = x.TotalCount
                    });
            }

            if (!needRecalculation)
                return folders;

            Factory.OperationEngine.RecalculateFolders();

            return folders;
        }

        public void ChangeFolderCounters(
            IDaoFactory daoFactory,
            FolderType folder,
            uint? userFolder = null,
            int? unreadMessDiff = null,
            int? totalMessDiff = null,
            int? unreadConvDiff = null,
            int? totalConvDiff = null)
        {
            if (folder == FolderType.UserFolder && !userFolder.HasValue)
                throw new ArgumentException(@"ChangeFolderCounters failed", "userFolder");

            try
            {
                var dao = daoFactory.CreateFolderDao(Tenant, User);

                var res = dao 
                    .ChangeFolderCounters(folder, unreadMessDiff, totalMessDiff, unreadConvDiff, totalConvDiff);

                if (res == 0)
                {
                    var totalCount = totalMessDiff.GetValueOrDefault(0);
                    var unreadCount = unreadMessDiff.GetValueOrDefault(0);
                    var unreadChainCount = unreadConvDiff.GetValueOrDefault(0);
                    var totalChainCount = totalConvDiff.GetValueOrDefault(0);

                    if (totalCount < 0 || unreadCount < 0 || unreadChainCount < 0 || totalChainCount < 0)
                        throw new Exception("Need recalculation");

                    var f = dao.GetFolder(folder);

                    if (f == null)
                    {
                        // Folder is not found

                        res = dao.Save(new Folder
                        {
                            FolderType = folder,
                            Tenant = Tenant,
                            UserId = User,
                            TotalCount = totalCount,
                            UnreadCount = unreadCount,
                            UnreadChainCount = unreadChainCount,
                            TotalChainCount = totalChainCount,
                            TimeModified = DateTime.UtcNow
                        });

                        if(res == 0)
                            throw new Exception("Need recalculation");
                    }
                    else
                    {
                        throw new Exception("Need recalculation");
                    }

                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("ChangeFolderCounters() Exception: {0}", ex.ToString());
                var engine = new EngineFactory(Tenant, User);
                engine.OperationEngine.RecalculateFolders();
            }

            if (!userFolder.HasValue)
                return;

            Factory.UserFolderEngine.ChangeFolderCounters(daoFactory, userFolder.Value, unreadMessDiff, totalMessDiff, unreadConvDiff, totalConvDiff);
        }

        public void RecalculateFolders(Action<MailOperationRecalculateMailboxProgress> callback = null)
        {
            using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RecalculateFoldersTimeout))
            {
                var daoFactory = new DaoFactory(db);

                var daoFolder = daoFactory.CreateFolderDao(Tenant, User);

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var folderTypes = Enum.GetValues(typeof(FolderType)).Cast<int>();

                    var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                    if (callback != null)
                        callback(MailOperationRecalculateMailboxProgress.CountUnreadMessages);

                    var unreadMessagesCountByFolder =
                        daoMailInfo.GetMailCount(
                            SimpleMessagesExp.CreateBuilder(Tenant, User)
                                .SetUnread(true)
                                .Build());

                    if (callback != null)
                        callback(MailOperationRecalculateMailboxProgress.CountTotalMessages);

                    var totalMessagesCountByFolder = daoMailInfo.GetMailCount(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .Build());

                    var daoChain = daoFactory.CreateChainDao(Tenant, User);

                    if (callback != null)
                        callback(MailOperationRecalculateMailboxProgress.CountUreadConversation);

                    var unreadConversationsCountByFolder = daoChain.GetChainCount(
                        SimpleConversationsExp.CreateBuilder(Tenant, User)
                            .SetUnread(true)
                            .Build());

                    if (callback != null)
                        callback(MailOperationRecalculateMailboxProgress.CountTotalConversation);

                    var totalConversationsCountByFolder = daoChain.GetChainCount(
                        SimpleConversationsExp.CreateBuilder(Tenant, User)
                            .Build());

                    if (callback != null)
                        callback(MailOperationRecalculateMailboxProgress.UpdateFoldersCounters);

                    var now = DateTime.UtcNow;

                    var folders = (from folderId in folderTypes
                        let unreadMessCount =
                            unreadMessagesCountByFolder.ContainsKey(folderId)
                                ? unreadMessagesCountByFolder[folderId]
                                : 0
                        let totalMessCount =
                            totalMessagesCountByFolder.ContainsKey(folderId)
                                ? totalMessagesCountByFolder[folderId]
                                : 0
                        let unreadConvCount =
                            unreadConversationsCountByFolder.ContainsKey(folderId)
                                ? unreadConversationsCountByFolder[folderId]
                                : 0
                        let totalConvCount =
                            totalConversationsCountByFolder.ContainsKey(folderId)
                                ? totalConversationsCountByFolder[folderId]
                                : 0
                        select new Folder
                        {
                            FolderType = (FolderType) folderId,
                            Tenant = Tenant,
                            UserId = User,
                            UnreadCount = unreadMessCount,
                            UnreadChainCount = unreadConvCount,
                            TotalCount = totalMessCount,
                            TotalChainCount = totalConvCount,
                            TimeModified = now
                        })
                        .ToList();

                    foreach (var folder in folders)
                    {
                        daoFolder.Save(folder);
                    }

                    var userFolder = folders.FirstOrDefault(f => f.FolderType == FolderType.UserFolder);

                    if (userFolder != null)
                    {
                        var daoUserFolder = daoFactory.CreateUserFolderDao(Tenant, User);

                        var userFolders =
                            daoUserFolder.GetList(
                                SimpleUserFoldersExp.CreateBuilder(Tenant, User)
                                    .Build());

                        if (userFolders.Any())
                        {
                            var totalMessagesCountByUserFolder = daoMailInfo.GetMailUserFolderCount();

                            if (callback != null)
                                callback(MailOperationRecalculateMailboxProgress.CountTotalUserFolderMessages);

                            var unreadMessagesCountByUserFolder = daoMailInfo.GetMailUserFolderCount(true);

                            if (callback != null)
                                callback(MailOperationRecalculateMailboxProgress.CountUnreadUserFolderMessages);

                            var totalConversationsCountByUserFolder = daoChain.GetChainUserFolderCount();

                            if (callback != null)
                                callback(MailOperationRecalculateMailboxProgress.CountTotalUserFolderConversation);

                            var unreadConversationsCountByUserFolder = daoChain.GetChainUserFolderCount(true);

                            if (callback != null)
                                callback(MailOperationRecalculateMailboxProgress.CountUreadUserFolderConversation);

                            var newUserFolders = (from folder in userFolders
                                let unreadMessCount =
                                    unreadMessagesCountByUserFolder.ContainsKey(folder.Id)
                                        ? unreadMessagesCountByUserFolder[folder.Id]
                                        : 0
                                let totalMessCount =
                                    totalMessagesCountByUserFolder.ContainsKey(folder.Id)
                                        ? totalMessagesCountByUserFolder[folder.Id]
                                        : 0
                                let unreadConvCount =
                                    unreadConversationsCountByUserFolder.ContainsKey(folder.Id)
                                        ? unreadConversationsCountByUserFolder[folder.Id]
                                        : 0
                                let totalConvCount =
                                    totalConversationsCountByUserFolder.ContainsKey(folder.Id)
                                        ? totalConversationsCountByUserFolder[folder.Id]
                                        : 0
                                select new UserFolder
                                {
                                    Id = folder.Id,
                                    ParentId = folder.ParentId,
                                    Name = folder.Name,
                                    FolderCount = folder.FolderCount,
                                    Tenant = Tenant,
                                    User = User,
                                    UnreadCount = unreadMessCount,
                                    UnreadChainCount = unreadConvCount,
                                    TotalCount = totalMessCount,
                                    TotalChainCount = totalConvCount,
                                    TimeModified = now
                                })
                                .ToList();

                            if (callback != null)
                                callback(MailOperationRecalculateMailboxProgress.UpdateUserFoldersCounters);

                            foreach (var folder in newUserFolders)
                            {
                                daoUserFolder.Save(folder);
                            }
                        }
                    }

                    tx.Commit();
                }
            }
        }

        public Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>> GetSpecialDomainFolders()
        {
            var specialDomainFolders = new Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>>();

            try
            {
                using (var daoFactory = new DaoFactory())
                {
                    var imapSpecialMailboxes = daoFactory
                        .CreateImapSpecialMailboxDao()
                        .GetImapSpecialMailboxes();

                    imapSpecialMailboxes.ForEach(r =>
                    {
                        var mb = new MailBoxData.MailboxInfo
                        {
                            folder_id = r.FolderId,
                            skip = r.Skip
                        };
                        if (specialDomainFolders.Keys.Contains(r.Server))
                            specialDomainFolders[r.Server][r.MailboxName] = mb;
                        else
                            specialDomainFolders[r.Server] = new Dictionary<string, MailBoxData.MailboxInfo>
                            {
                                {r.MailboxName, mb}
                            };
                    });
                }

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetSpecialDomainFolders() Exception: {0}", ex.ToString());
            }

            return specialDomainFolders;
        }

        public static List<FolderType> DefaultFolders
        {
            get
            {
                return ((FolderType[]) Enum.GetValues(typeof(FolderType)))
                    .Where(folderType => folderType != FolderType.Sending && folderType != FolderType.UserFolder)
                    .ToList();
            }
        }
    }
}
