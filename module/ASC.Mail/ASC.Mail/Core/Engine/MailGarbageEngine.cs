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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Mail.Core.Dao;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Extensions;
using ASC.Mail.Iterators;

namespace ASC.Mail.Core.Engine
{
    public class MailGarbageEngine : IDisposable
    {
        public ILog Log { get; private set; }

        private static MemoryCache TenantMemCache { get; set; }

        private static MailGarbageEraserConfig Config { get; set; }

        private static TaskFactory TaskFactory { get; set; }

        private static object Locker { get; set; }

        public MailGarbageEngine(ILog log = null)
            : this(MailGarbageEraserConfig.FromConfig(), log)
        {
        }

        public MailGarbageEngine(MailGarbageEraserConfig config, ILog log = null)
        {
            Config = config;

            Log = log ?? LogManager.GetLogger("ASC.Mail.GarbageEngine");

            TenantMemCache = new MemoryCache("GarbageEraserTenantCache");

            var scheduler = new LimitedConcurrencyLevelTaskScheduler(Config.MaxTasksAtOnce);

            TaskFactory = new TaskFactory(scheduler);

            Locker = new object();
        }

        #region - Public methods -

        public void ClearMailGarbage(CancellationToken cancelToken)
        {
            Log.Debug("Begin ClearMailGarbage()");

            var tasks = new List<Task>();

            var mailboxIterator = new MailboxIterator(isRemoved: null, log: Log);

            var mailbox = mailboxIterator.First();

            while (!mailboxIterator.IsDone)
            {
                try
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    var mb = mailbox;

                    var task = Queue(() => ClearGarbage(mb), cancelToken);

                    tasks.Add(task);

                    if (tasks.Count == Config.MaxTasksAtOnce)
                    {
                        Log.Info("Wait all tasks to complete");

                        Task.WaitAll(tasks.ToArray());

                        tasks = new List<Task>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                if (!cancelToken.IsCancellationRequested)
                {
                    mailbox = mailboxIterator.Next();
                    continue;
                }

                Log.Debug("ClearMailGarbage: IsCancellationRequested. Quit.");
                break;
            }

            RemoveUselessMsDomains();

            Log.Debug("End ClearMailGarbage()\r\n");
        }

        public void RemoveUselessMsDomains() {
            Log.Debug("Start RemoveUselessMsDomains()\r\n");

            try
            {
                var engineFactory = new EngineFactory(-1, ASC.Core.Configuration.Constants.CoreSystem.ID.ToString());

                var domains = engineFactory.ServerDomainEngine.GetAllDomains();

                foreach (var domain in domains)
                {
                    if (domain.Tenant == -1)
                        continue;

                    var status = GetTenantStatus(domain.Tenant);

                    if (status != TenantStatus.RemovePending)
                        continue;

                    var exp = new TenantServerMailboxesExp(domain.Tenant, null);

                    var mailboxes = engineFactory.MailboxEngine.GetMailboxDataList(exp);

                    if (mailboxes.Any())
                    {
                        Log.WarnFormat("Domain's '{0}' Tenant={1} is removed, but it has unremoved server mailboxes (count={2}). Skip it.", 
                            domain.Name, domain.Tenant, mailboxes.Count);

                        continue;
                    }

                    Log.InfoFormat("Domain's '{0}' Tenant={1} is removed. Lets remove domain.", domain.Name, domain.Tenant);

                    var count = domains.Count(d => d.Name.Equals(domain.Name, StringComparison.InvariantCultureIgnoreCase));

                    var skipMS = count > 1;

                    if (skipMS)
                    {
                        Log.InfoFormat("Domain's '{0}' has duplicated entry for another tenant. Remove only current entry.", domain.Name);
                    }

                    RemoveDomain(domain, skipMS);
                }

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("RemoveUselessMsDomains failed. Exception: {0}", ex.ToString()));
            }

            Log.Debug("End RemoveUselessMsDomains()\r\n");
        }

        public TenantStatus GetTenantStatus(int tenant) {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);

                var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

                return tenantInfo.Status;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("GetTenantStatus(tenant='{0}') failed. Exception: {1}", tenant, ex.ToString()));
            }

            return TenantStatus.Active;
        }

        public void RemoveDomain(Entities.ServerDomain domain, bool skipMS = false)
        {
            try
            {
                using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RemoveDomainTimeout))
                {
                    var daoFactory = new DaoFactory(db);

                    using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        var serverDomainDao = daoFactory.CreateServerDomainDao(domain.Tenant);

                        serverDomainDao.Delete(domain.Id);

                        if (!skipMS)
                        {
                            var serverDao = daoFactory.CreateServerDao();

                            var server = serverDao.Get(domain.Tenant);

                            if (server == null)
                                throw new Exception(string.Format("Information for Tenant's Mail Server not found (Tenant = {0})", domain.Tenant));

                            var serverEngine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                            serverEngine.RemoveDomain(domain.Name, false);
                        }

                        tx.Commit();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("RemoveDomainIfUseless(Domain: '{0}', ID='{1}') failed. Exception: {2}", domain.Name, domain.Id, ex.ToString()));
            }
        }

        public void ClearUserMail(Guid userId, Tenant tenantId = null)
        {
            var log = LogManager.GetLogger("ASC.Mail.Cleaner");

            var tenant = tenantId != null ? tenantId.TenantId : CoreContext.TenantManager.GetCurrentTenant().TenantId;

            log.InfoFormat("ClearUserMail(userId: '{0}' tenant: {1})", userId, tenant);

            var user = userId.ToString();

            RemoveUserFolders(tenant, user, log);

            RemoveUserMailboxes(tenant, user, log);

            //TODO: RemoveUserTags

            //TODO: RemoveUserContacts

            //TODO: RemoveUserAlerts

            //TODO: RemoveUserDisplayImagesAddresses

            //TODO: RemoveUserFolderCounters
        }

        #endregion

        #region - Private methods -

        private Task Queue(Action action, CancellationToken cancelToken)
        {
            var task = TaskFactory.StartNew(action, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            task.ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    Log.DebugFormat("End Task {0} with status = '{1}'.", task.Id, task.Status);
                });

            return task;
        }

        private bool NeedRemove(MailBoxData mailbox, ILog taskLog)
        {
            var needRemove = false;

            lock (Locker)
            {
                Defines.TariffType type;

                var memTenantItem = TenantMemCache.Get(mailbox.TenantId.ToString(CultureInfo.InvariantCulture));

                if (memTenantItem == null)
                {
                    taskLog.InfoFormat("Tenant {0} isn't in cache", mailbox.TenantId);

                    taskLog.DebugFormat("GetTenantStatus(OverdueDays={0})", Config.TenantOverdueDays);

                    type = mailbox.GetTenantStatus(Config.TenantOverdueDays, Config.HttpContextScheme, Log);

                    var cacheItem = new CacheItem(mailbox.TenantId.ToString(CultureInfo.InvariantCulture), type);

                    var cacheItemPolicy = new CacheItemPolicy
                    {
                        AbsoluteExpiration =
                            DateTimeOffset.UtcNow.AddDays(Config.TenantCacheDays)
                    };

                    TenantMemCache.Add(cacheItem, cacheItemPolicy);
                }
                else
                {
                    taskLog.InfoFormat("Tenant {0} is in cache", mailbox.TenantId);

                    type = (Defines.TariffType)memTenantItem;
                }

                taskLog.InfoFormat("Tenant {0} has status '{1}'", mailbox.TenantId, type.ToString());

                if (type == Defines.TariffType.LongDead)
                {
                    needRemove = true;
                }
                else
                {
                    var isUserRemoved = mailbox.IsUserRemoved();

                    taskLog.InfoFormat("User '{0}' status is '{1}'", mailbox.UserId, isUserRemoved ? "Terminated" : "Not terminated");

                    if (isUserRemoved)
                    {
                        needRemove = true;
                    }
                }

            }

            return needRemove;
        }

        private void ClearGarbage(MailBoxData mailbox)
        {
            var taskLog =
                LogManager.GetLogger(string.Format("ASC.Mail Mbox_{0} Task_{1}", mailbox.MailBoxId, Task.CurrentId));

            taskLog.InfoFormat("Processing MailboxId = {0}, email = '{1}', tenant = '{2}', user = '{3}'",
                mailbox.MailBoxId, mailbox.EMail.Address, mailbox.TenantId, mailbox.UserId);

            try
            {
                if (NeedRemove(mailbox, taskLog))
                {
                    RemoveMailboxData(mailbox, true, taskLog);
                }
                else if (mailbox.IsRemoved)
                {
                    taskLog.Info("Mailbox is removed.");
                    RemoveMailboxData(mailbox, false, taskLog);
                }
                else
                {
                    RemoveGarbageMailData(mailbox, Config.GarbageOverdueDays, taskLog);
                }

                taskLog.InfoFormat("Mailbox {0} processing complete.", mailbox.MailBoxId);
            }
            catch (Exception ex)
            {
                taskLog.ErrorFormat("Mailbox {0} processed with error : {1}", mailbox.MailBoxId, ex.ToString());
            }
        }

        private static void RemoveMailboxData(MailBoxData mailbox, bool totalMailRemove, ILog log)
        {
            log.InfoFormat("RemoveMailboxData(id: {0} address: {1})", mailbox.MailBoxId, mailbox.EMail.ToString());

            try
            {
                if (!mailbox.IsRemoved)
                {
                    log.Info("Mailbox is't removed.");

                    var needRecalculateFolders = !totalMailRemove;

                    if (mailbox.IsTeamlab)
                    {
                        log.Info("RemoveTeamlabMailbox()");

                        CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                        RemoveTeamlabMailbox(mailbox, log);
                    }

                    log.Info("SetMailboxRemoved()");
                    var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);
                    engine.MailboxEngine.RemoveMailBox(mailbox, needRecalculateFolders);

                    mailbox.IsRemoved = true;
                }

                log.DebugFormat("MailDataStore.GetDataStore(Tenant = {0})", mailbox.TenantId);

                var dataStorage = MailDataStore.GetDataStore(mailbox.TenantId);

                dataStorage.QuotaController = null;

                log.Debug("GetMailboxAttachsCount()");

                var mailGarbageDao = new MailGarbageDao();

                var countAttachs = mailGarbageDao.GetMailboxAttachsCount(mailbox);

                log.InfoFormat("Found {0} garbage attachments", countAttachs);

                if (countAttachs > 0)
                {
                    var sumCount = 0;

                    log.DebugFormat("GetMailboxAttachsGarbage(limit = {0})", Config.MaxFilesToRemoveAtOnce);

                    var attachGrbgList = mailGarbageDao.GetMailboxAttachs(mailbox, Config.MaxFilesToRemoveAtOnce);

                    sumCount += attachGrbgList.Count;

                    log.InfoFormat("Clearing {0} garbage attachments ({1}/{2})", attachGrbgList.Count, sumCount, countAttachs);

                    while (attachGrbgList.Any())
                    {
                        foreach (var attachGrbg in attachGrbgList)
                        {
                            RemoveFile(dataStorage, attachGrbg.Path, log);
                        }

                        log.Debug("CleanupMailboxAttachs()");

                        mailGarbageDao.CleanupMailboxAttachs(attachGrbgList);

                        log.Debug("GetMailboxAttachs()");

                        attachGrbgList = mailGarbageDao.GetMailboxAttachs(mailbox, Config.MaxFilesToRemoveAtOnce);

                        if (!attachGrbgList.Any()) continue;

                        sumCount += attachGrbgList.Count;

                        log.InfoFormat("Found {0} garbage attachments ({1}/{2})", attachGrbgList.Count, sumCount,
                                 countAttachs);
                    }
                }

                log.Debug("GetMailboxMessagesCount()");

                var countMessages = mailGarbageDao.GetMailboxMessagesCount(mailbox);

                log.InfoFormat("Found {0} garbage messages", countMessages);

                if (countMessages > 0)
                {
                    var sumCount = 0;

                    log.DebugFormat("GetMailboxMessagesGarbage(limit = {0})", Config.MaxFilesToRemoveAtOnce);

                    var messageGrbgList = mailGarbageDao.GetMailboxMessages(mailbox, Config.MaxFilesToRemoveAtOnce);

                    sumCount += messageGrbgList.Count;

                    log.InfoFormat("Clearing {0} garbage messages ({1}/{2})", messageGrbgList.Count, sumCount, countMessages);

                    while (messageGrbgList.Any())
                    {
                        foreach (var mailMessageGarbage in messageGrbgList)
                        {
                            RemoveFile(dataStorage, mailMessageGarbage.Path, log);
                        }

                        log.Debug("CleanupMailboxMessages()");

                        mailGarbageDao.CleanupMailboxMessages(messageGrbgList);

                        log.Debug("GetMailboxMessages()");

                        messageGrbgList = mailGarbageDao.GetMailboxMessages(mailbox, Config.MaxFilesToRemoveAtOnce);

                        if (!messageGrbgList.Any()) continue;

                        sumCount += messageGrbgList.Count;

                        log.InfoFormat("Found {0} garbage messages ({1}/{2})", messageGrbgList.Count, sumCount,
                                 countMessages);
                    }
                }

                log.Debug("ClearMailboxData()");

                mailGarbageDao.CleanupMailboxData(mailbox, totalMailRemove);

                log.DebugFormat("Garbage mailbox '{0}' was totaly removed.", mailbox.EMail.Address);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveMailboxData(mailboxId = {0}) Failure\r\nException: {1}", mailbox.MailBoxId, ex.ToString());

                throw;
            }
        }

        private static void RemoveFile(IDataStore dataStorage, string path, ILog log)
        {
            try
            {
                log.DebugFormat("Removing file: {0}", path);

                dataStorage.Delete(string.Empty, path);

                log.InfoFormat("File: '{0}' removed successfully", path);
            }
            catch (FileNotFoundException)
            {
                log.WarnFormat("File: {0} not found", path);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveFile(path: {0}) failed. Error: {1}", path, ex.ToString());
            }
        }

        private static void RemoveUserMailDirectory(int tenant, string userId, ILog log)
        {
            log.DebugFormat("MailDataStore.GetDataStore(Tenant = {0})", tenant);

            var dataStorage = MailDataStore.GetDataStore(tenant);

            var userMailDir = MailStoragePathCombiner.GetUserMailsDirectory(userId);

            try
            {
                log.InfoFormat("RemoveUserMailDirectory(Path: {0}, Tenant = {1} User = '{2}')", userMailDir, tenant, userId);

                dataStorage.DeleteDirectory(userMailDir);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("MailDataStore.DeleteDirectory(path: {0}) failed. Error: {1}", userMailDir, ex.ToString());

                throw;
            }
        }

        public bool RemoveGarbageMailData(MailBoxData mailbox, int garbageDaysLimit, ILog log)
        {
            //TODO: Implement cleanup data marked as removed and trash messages exceeded garbageDaysLimit

            return true;
        }

        private static void RemoveTeamlabMailbox(MailBoxData mailbox, ILog log)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox");

            if (!mailbox.IsTeamlab)
                return;

            try
            {
                var engineFactory = new EngineFactory(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    SecurityContext.CurrentAccount.ID.ToString());

                engineFactory.ServerMailboxEngine.RemoveMailbox(mailbox);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveTeamlabMailbox(mailboxId = {0}) Failure\r\nException: {1}", mailbox.MailBoxId,
                    ex.ToString());
            }
        }

        private static void RemoveUserFolders(int tenant, string userId, ILog log)
        {
            try
            {
                var engineFactory = new EngineFactory(tenant, userId);

                var engine = engineFactory.UserFolderEngine;

                var folders = engine.GetList(parentId: 0);

                foreach (var folder in folders)
                {
                    engine.Delete(folder.Id);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveUserFolders() Failure\r\nException: {0}", ex.ToString());
            }
        }

        private static void RemoveUserMailboxes(int tenant, string user, ILog log)
        {
            var mailboxIterator = new MailboxIterator(tenant, user);

            var mailbox = mailboxIterator.First();

            if (mailboxIterator.IsDone)
            {
                log.Info("There are no user's mailboxes for deletion");
                return;
            }

            while (!mailboxIterator.IsDone)
            {
                try
                {
                    if (!mailbox.UserId.Equals(user))
                        throw new Exception(
                            string.Format("Mailbox (id:{0}) user '{1}' not equals to removed user: '{2}'",
                                mailbox.MailBoxId, mailbox.UserId, user));

                    RemoveMailboxData(mailbox, true, log);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("RemoveMailboxData(MailboxId: {0}) failed. Error: {1}", mailbox.MailBoxId, ex);
                }

                mailbox = mailboxIterator.Next();
            }

            RemoveUserMailDirectory(tenant, user, log);
        }

        #endregion

        public void Dispose()
        {
            if (TenantMemCache != null)
            {
                TenantMemCache.Dispose();
            }
        }
    }
}