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
using ASC.ElasticSearch;
using ASC.Mail.Core.Dao.Expressions.UserFolder;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;

namespace ASC.Mail.Core.Engine
{
    public class UserFolderEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public EngineFactory Factory { get; private set; }

        public UserFolderEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.UserFolderEngine");

            Factory = new EngineFactory(Tenant, User, Log);
        }

        public MailUserFolderData Get(uint id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var userFolder = userFolderDao.Get(id);

                return ToMailUserFolderData(userFolder);
            }
        }

        public MailUserFolderData GetByMail(uint mailId)
        {
            using (var daoFactory = new DaoFactory())
            {
                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var userFolder = userFolderDao.GetByMail(mailId);

                return ToMailUserFolderData(userFolder);
            }
        }

        public List<MailUserFolderData> GetList(List<uint> ids = null, uint? parentId = null)
        {
            using (var daoFactory = new DaoFactory())
            {
                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var builder = SimpleUserFoldersExp.CreateBuilder(Tenant, User);

                if (ids != null && ids.Any())
                {
                    builder.SetIds(ids);
                }

                if (parentId.HasValue)
                {
                    builder.SetParent(parentId.Value);
                }

                var exp = builder.Build();

                var userFolderDataList = userFolderDao.GetList(exp)
                    .ConvertAll(ToMailUserFolderData);

                return userFolderDataList;
            }
        }

        public MailUserFolderData Create(string name, uint parentId = 0)
        {
            if (string.IsNullOrEmpty(name))
                throw new EmptyFolderException(@"Name is empty");

            var utsNow = DateTime.UtcNow;

            var newUserFolder = new UserFolder
            {
                Id = 0,
                ParentId = parentId,
                Name = name,
                User = User,
                Tenant = Tenant,
                TimeModified = utsNow
            };

            using (var daoFactory = new DaoFactory())
            {
                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var userFolderTreeDao = daoFactory.CreateUserFolderTreeDao(Tenant, User);

                if (parentId > 0)
                {
                    var parentUserFolder = userFolderDao.Get(parentId);

                    if (parentUserFolder == null)
                        throw new ArgumentException(@"Parent folder not found", "parentId");
                }

                if (IsFolderNameAlreadyExists(userFolderDao, newUserFolder))
                {
                    throw new AlreadyExistsFolderException(
                        string.Format("Folder with name \"{0}\" already exists", newUserFolder.Name));
                }

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    newUserFolder.Id = userFolderDao.Save(newUserFolder);

                    if (newUserFolder.Id <= 0)
                        throw new Exception("Save user folder failed");

                    var userFolderTreeItem = new UserFolderTreeItem
                    {
                        FolderId = newUserFolder.Id,
                        ParentId = newUserFolder.Id,
                        Level = 0
                    };

                    //itself link
                    userFolderTreeDao.Save(userFolderTreeItem);

                    //full path to root
                    userFolderTreeDao.InsertFullPathToRoot(newUserFolder.Id, newUserFolder.ParentId);

                    tx.Commit();
                }

                userFolderDao.RecalculateFoldersCount(newUserFolder.Id);
            }

            return ToMailUserFolderData(newUserFolder);
        }

        public MailUserFolderData Update(uint id, string name, uint? parentId = null)
        {
            if (id < 0)
                throw new ArgumentException("id");

            if (string.IsNullOrEmpty(name))
                throw new EmptyFolderException(@"Name is empty");

            if(parentId.HasValue && id == parentId.Value)
                throw new ArgumentException(@"id equals to parentId", "parentId");

            using (var daoFactory = new DaoFactory())
            {
                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var oldUserFolder = userFolderDao.Get(id);

                if (oldUserFolder == null)
                    throw new ArgumentException("Folder not found");

                var newUserFolder = new UserFolder
                {
                    Id = id,
                    ParentId = parentId ?? oldUserFolder.ParentId,
                    Name = name,
                    User = User,
                    Tenant = Tenant,
                    FolderCount = oldUserFolder.FolderCount,
                    UnreadCount = oldUserFolder.UnreadCount,
                    TotalCount = oldUserFolder.TotalCount,
                    UnreadChainCount = oldUserFolder.UnreadChainCount,
                    TotalChainCount = oldUserFolder.TotalChainCount,
                    TimeModified = oldUserFolder.TimeModified
                };

                if (newUserFolder.Equals(oldUserFolder))
                    return ToMailUserFolderData(oldUserFolder);

                if (IsFolderNameAlreadyExists(userFolderDao, newUserFolder))
                {
                    throw new AlreadyExistsFolderException(
                        string.Format("Folder with name \"{0}\" already exists", newUserFolder.Name));
                }

                var utsNow = DateTime.UtcNow;

                var userFolderTreeDao = daoFactory.CreateUserFolderTreeDao(Tenant, User);

                if (newUserFolder.ParentId != oldUserFolder.ParentId)
                {
                    if (!CanMoveFolderTo(userFolderDao, newUserFolder))
                    {
                        throw new MoveFolderException(
                            string.Format("Can't move folder with id=\"{0}\" into the folder with id=\"{1}\"",
                                newUserFolder.Id, newUserFolder.ParentId));
                    }
                }

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    newUserFolder.TimeModified = utsNow;
                    userFolderDao.Save(newUserFolder);

                    if (newUserFolder.ParentId != oldUserFolder.ParentId)
                    {
                        userFolderTreeDao.Move(newUserFolder.Id, newUserFolder.ParentId);
                    }

                    tx.Commit();
                }

                var recalcFolders = new List<uint> { newUserFolder.ParentId };

                if (newUserFolder.ParentId > 0)
                {
                    recalcFolders.Add(newUserFolder.Id);
                }

                if (oldUserFolder.ParentId != 0 && !recalcFolders.Contains(oldUserFolder.ParentId))
                {
                    recalcFolders.Add(oldUserFolder.ParentId);
                }

                recalcFolders.ForEach(fid =>
                {
                    userFolderDao.RecalculateFoldersCount(fid);
                });

                return ToMailUserFolderData(newUserFolder);
            }
        }

        public void Delete(uint folderId)
        {
            var affectedIds = new List<int>();

            using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RecalculateFoldersTimeout))
            {
                var daoFactory = new DaoFactory(db);

                var userFolderXmailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);

                var userFolderTreeDao = daoFactory.CreateUserFolderTreeDao(Tenant, User);

                var userFolderDao = daoFactory.CreateUserFolderDao(Tenant, User);

                var folder = userFolderDao.Get(folderId);
                if (folder == null)
                    return;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    //Find folder sub-folders
                    var expTree = SimpleUserFoldersTreeExp.CreateBuilder()
                        .SetParent(folder.Id)
                        .Build();

                    var removeFolderIds = userFolderTreeDao.Get(expTree)
                        .ConvertAll(f => f.FolderId);

                    if(!removeFolderIds.Contains(folderId))
                        removeFolderIds.Add(folderId);

                    //Remove folder with subfolders
                    var expFolders = SimpleUserFoldersExp.CreateBuilder(Tenant, User)
                        .SetIds(removeFolderIds)
                        .Build();

                    userFolderDao.Remove(expFolders);

                    //Remove folder tree info
                    expTree = SimpleUserFoldersTreeExp.CreateBuilder()
                        .SetIds(removeFolderIds)
                        .Build();

                    userFolderTreeDao.Remove(expTree);

                    //Move mails to trash
                    foreach (var id in removeFolderIds)
                    {
                        var listMailIds = userFolderXmailDao.GetMailIds(id);

                        if (!listMailIds.Any()) continue;

                        affectedIds.AddRange(listMailIds);

                        //Move mails to trash
                        Factory.MessageEngine.SetFolder(daoFactory, listMailIds, FolderType.Trash);

                        //Remove listMailIds from 'mail_user_folder_x_mail'
                        userFolderXmailDao.Remove(listMailIds);
                    }

                    tx.Commit();
                }

                userFolderDao.RecalculateFoldersCount(folder.ParentId);
            }

            if (!FactoryIndexer<MailWrapper>.Support || !affectedIds.Any())
                return;

            var data = new MailWrapper
            {
                Folder = (byte)FolderType.Trash
            };

            Factory.IndexEngine.Update(data, s => s.In(m => m.Id, affectedIds.ToArray()), wrapper => wrapper.Unread);
        }

        public void SetFolderMessages(IDaoFactory daoFactory, uint userFolderId, List<int> ids)
        {
            var userFolderXMailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);

            userFolderXMailDao.Remove(ids);

            userFolderXMailDao.SetMessagesFolder(ids, userFolderId);
        }

        public void DeleteFolderMessages(IDaoFactory daoFactory, List<int> ids)
        {
            var userFolderXMailDao = daoFactory.CreateUserFolderXMailDao(Tenant, User);

            userFolderXMailDao.Remove(ids);
        }

        public void RecalculateCounters(IDaoFactory daoFactory, List<int> userFolderIds)
        {
            var chainDao = daoFactory.CreateChainDao(Tenant, User);
            var mailInfoDao = daoFactory.CreateMailInfoDao(Tenant, User);

            var totalUfMessList = mailInfoDao.GetMailUserFolderCount(userFolderIds);
            var unreadUfMessUfList = mailInfoDao.GetMailUserFolderCount(userFolderIds, true);
            var totalUfConvList = chainDao.GetChainUserFolderCount(userFolderIds);
            var unreadUfConvUfList = chainDao.GetChainUserFolderCount(userFolderIds, true);

            foreach (var id in userFolderIds.Select(id => (uint)id))
            {
                int totalMess;
                totalUfMessList.TryGetValue(id, out totalMess);

                int unreadMess;
                unreadUfMessUfList.TryGetValue(id, out unreadMess);

                int totalConv;
                totalUfConvList.TryGetValue(id, out totalConv);

                int unreadConv;
                unreadUfConvUfList.TryGetValue(id, out unreadConv);

                SetFolderCounters(daoFactory, id,
                    unreadMess, totalMess, unreadConv, totalConv);
            }
        }

        public void SetFolderCounters(
            IDaoFactory daoFactory,
            uint userFolderId,
            int? unreadMess = null,
            int? totalMess = null,
            int? unreadConv = null,
            int? totalConv = null)
        {
            try
            {
                var res = daoFactory.CreateUserFolderDao(Tenant, User)
                    .SetFolderCounters(userFolderId, unreadMess, totalMess, unreadConv,
                        totalConv);

                if (res == 0)
                    throw new Exception("Need recalculation");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("UserFolderEngine->SetFolderCounters() Exception: {0}", ex.ToString());
                //TODO: Think about recalculation
                //var engine = new EngineFactory(Tenant, User);
                //engine.OperationEngine.RecalculateFolders();
            }
        }

        public void ChangeFolderCounters(
            IDaoFactory daoFactory, 
            uint userFolderId,
            int? unreadMessDiff = null,
            int? totalMessDiff = null,
            int? unreadConvDiff = null,
            int? totalConvDiff = null)
        {
            try
            {
                var res = daoFactory.CreateUserFolderDao(Tenant, User)
                    .ChangeFolderCounters(userFolderId, unreadMessDiff, totalMessDiff, unreadConvDiff,
                        totalConvDiff);

                if (res == 0)
                    throw new Exception("Need recalculation");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("UserFolderEngine->ChangeFolderCounters() Exception: {0}", ex.ToString());
                //TODO: Think about recalculation
                //var engine = new EngineFactory(Tenant, User);
                //engine.OperationEngine.RecalculateFolders();
            }
        }

        private bool IsFolderNameAlreadyExists(IUserFolderDao userFolderDao, UserFolder newUserFolder)
        {
            //Find folder sub-folders
            var exp = SimpleUserFoldersExp.CreateBuilder(Tenant, User)
                .SetParent(newUserFolder.ParentId)
                .Build();

            var listExistinFolders = userFolderDao.GetList(exp);

            return listExistinFolders.Any(existinFolder => existinFolder.Name.Equals(newUserFolder.Name,
                StringComparison.InvariantCultureIgnoreCase));
        }

        private bool CanMoveFolderTo(IUserFolderDao userFolderDao, UserFolder newUserFolder)
        {
            //Find folder sub-folders
            var exp = SimpleUserFoldersExp.CreateBuilder(Tenant, User)
                .SetParent(newUserFolder.Id)
                .SetIds(new List<uint> { newUserFolder.ParentId})
                .Build();

            var listExistinFolders = userFolderDao.GetList(exp);

            return !listExistinFolders.Any();
        }

        // ReSharper disable once UnusedMember.Local
        private static UserFolder ToUserFolder(MailUserFolderData folder, int tenant, string user)
        {
            if (folder == null)
                return null;

            var utcNow = DateTime.UtcNow;

            var userFolder = new UserFolder
            {
                Tenant = tenant,
                User = user,
                Id = folder.Id,
                ParentId = folder.ParentId,
                Name = folder.Name,
                FolderCount = folder.FolderCount,
                UnreadCount = folder.UnreadCount,
                TotalCount = folder.TotalCount,
                UnreadChainCount = folder.UnreadChainCount,
                TotalChainCount = folder.TotalChainCount,
                TimeModified = utcNow
            };

            return userFolder;
        }

        private static MailUserFolderData ToMailUserFolderData(UserFolder folder)
        {
            if (folder == null)
                return null;

            var userFolderData = new MailUserFolderData
            {
                Id = folder.Id,
                ParentId = folder.ParentId,
                Name = folder.Name,
                FolderCount = folder.FolderCount,
                UnreadCount = folder.UnreadCount,
                TotalCount = folder.TotalCount,
                UnreadChainCount = folder.UnreadChainCount,
                TotalChainCount = folder.TotalChainCount
            };

            return userFolderData;
        }
    }
}
