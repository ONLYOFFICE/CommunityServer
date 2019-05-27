/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Mail.Core.Dao;
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

            Log.Debug("End ClearMailGarbage()\r\n");
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