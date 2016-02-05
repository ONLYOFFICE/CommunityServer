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


using ASC.Common.Threading;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Iterators;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.PostfixAdministration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using ServerType = ASC.Mail.Server.Dal.ServerType;

namespace ASC.Mail.GarbageEraser
{
    public class MailGarbageEraser
    {
        private readonly ILogger _log;
        private readonly MailBoxManager _mailBoxManager;
        private readonly MailGarbageCleanDal _garbageManager;
        private readonly MemoryCache _tenantMemCache;
        private readonly int _tenantCacheDays = 1;
        private readonly int _tenantOverdueDays = 30;
        private readonly int _garbageOverdueDays = 30;
        private readonly int _maxTasksAtOnce = 10;
        private readonly int _maxFilesToRemoveAtOnce = 100;

        readonly LimitedConcurrencyLevelTaskScheduler _lcts;
        readonly TaskFactory _taskFactory;

        public MailGarbageEraser(int maxTasksAtOnce, int maxFilesToRemoveAtOnce, int tenantCacheDays, int tenantOverdueDays, int garbageOverdueDays,
                                 ILogger log = null)
        {
            _maxTasksAtOnce = maxTasksAtOnce;
            _maxFilesToRemoveAtOnce = maxFilesToRemoveAtOnce;
            _tenantCacheDays = tenantCacheDays;
            _tenantOverdueDays = tenantOverdueDays;
            _garbageOverdueDays = garbageOverdueDays;

            _log = log ?? new NullLogger();

            _mailBoxManager = new MailBoxManager();

            _garbageManager = new MailGarbageCleanDal();

            _tenantMemCache = new MemoryCache("GarbageEraserTenantCache");

            _lcts = new LimitedConcurrencyLevelTaskScheduler(_maxTasksAtOnce);

            _taskFactory = new TaskFactory(_lcts);
        }

        #region - Public methods -

        public void ClearMailGarbage(ManualResetEvent resetEvent = null)
        {
            _log.Debug("Begin ClearMailGarbage()");

            var mailboxIterator = new MailboxIterator(_mailBoxManager);

            var mailbox = mailboxIterator.First();

            var tasks = new List<Task>();

            while (!mailboxIterator.IsDone)
            {
                try
                {
                    var mb = mailbox;
                    var task = _taskFactory.StartNew(() => ClearGarbage(mb), TaskCreationOptions.LongRunning);

                    _log.Debug("Start Task {0}", task.Id);

                    tasks.Add(task);

                    if (tasks.Count == _maxTasksAtOnce)
                    {
                        _log.Info("Wait any task to complete");

                        var indexTask = Task.WaitAny(tasks.ToArray());

                        if (indexTask > -1)
                        {
                            var outTask = tasks[indexTask];
                            FreeTask(outTask, tasks);
                        }

                        var tasks2Free =
                            tasks.Where(
                                t =>
                                t.Status == TaskStatus.Canceled || t.Status == TaskStatus.Faulted ||
                                t.Status == TaskStatus.RanToCompletion).ToList();

                        if (tasks2Free.Any())
                        {
                            _log.Info("Need free next tasks = {0}: ({1})", tasks2Free.Count,
                                      string.Join(",",
                                                  tasks2Free.Select(t => t.Id.ToString(CultureInfo.InvariantCulture))));

                            tasks2Free.ForEach(tsk => FreeTask(tsk, tasks));
                        }

                    }

                    mailbox = mailboxIterator.Next();
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }

                if (resetEvent == null || !resetEvent.WaitOne(0)) continue;

                _log.Debug("ClearMailGarbage() is canceled");
            }

            _log.Debug("End ClearMailGarbage()\r\n");
        }

        #endregion

        #region - Private methods -

        private void FreeTask(Task task, ICollection<Task> tasks)
        {
            _log.Debug("End Task {0} with status = '{1}'.", task.Id, task.Status);

            if (!tasks.Remove(task))
                _log.Error("Task not exists in tasks array.");

            task.Dispose();
        }

        private void ClearGarbage(MailBox mailbox)
        {
            var taskLog = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "Task_" + Task.CurrentId);

            taskLog.Info("Processing MailboxId = {0}, email = '{1}', tenant = '{2}', user = '{3}'",
                              mailbox.MailBoxId, mailbox.EMail.Address, mailbox.TenantId, mailbox.UserId);

            var type = _tenantMemCache.Get(mailbox.TenantId.ToString(CultureInfo.InvariantCulture));

            if (type == null)
            {
                taskLog.Info("Tenant {0} isn't in cache", mailbox.TenantId);

                taskLog.Debug("GetTenantStatus(OverdueDays={0})", _tenantOverdueDays);

                type = mailbox.GetTenantStatus(_tenantOverdueDays, _log);

                var cacheItem = new CacheItem(mailbox.TenantId.ToString(CultureInfo.InvariantCulture), type);

                var cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration =
                        DateTimeOffset.UtcNow.AddDays(_tenantCacheDays)
                };

                _tenantMemCache.Add(cacheItem, cacheItemPolicy);
            }
            else
            {
                taskLog.Info("Tenant {0} is in cache", mailbox.TenantId);
            }

            taskLog.Info("Tenant {0} has status '{1}'", mailbox.TenantId, type.ToString());

            if (Defines.TariffType.LongDead == (Defines.TariffType)type)
            {
                RemoveMailboxData(mailbox, true, taskLog);
            }
            else if (mailbox.IsUserRemoved())
            {
                taskLog.Info("User has been terminated.");
                RemoveMailboxData(mailbox, true, taskLog);
            }
            else if (mailbox.IsRemoved)
            {
                taskLog.Info("Mailbox is removed.");
                RemoveMailboxData(mailbox, false, taskLog);
            }
            else
            {
                RemoveGarbageMailData(mailbox, _garbageOverdueDays, taskLog);
            }

            taskLog.Info("Mailbox {0} processing complete.", mailbox.MailBoxId);
        }

        private void RemoveMailboxData(MailBox mailbox, bool totalMailRemove, ILogger log)
        {
            log.Debug("RemoveMailboxData()");

            try
            {
                if (!mailbox.IsRemoved)
                {
                    log.Debug("Mailbox is't removed.");

                    var needRecalculateFolders = !totalMailRemove;

                    if (!mailbox.IsTeamlab)
                    {
                        log.Debug("RemoveMailBox()");
                        _mailBoxManager.RemoveMailBox(mailbox, needRecalculateFolders);
                    }
                    else
                    {
                        log.Debug("RemoveTeamlabMailbox()");

                        CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);

                        var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

                        SecurityContext.AuthenticateMe(tenantInfo.OwnerId);

                        RemoveTeamlabMailbox(mailbox);

                        _mailBoxManager.RemoveMailBox(mailbox, needRecalculateFolders);

                    }

                    mailbox.IsRemoved = true;
                }

                log.Debug("MailDataStore.GetDataStore(Tenant = {0})", mailbox.TenantId);

                var dataStorage = MailDataStore.GetDataStore(mailbox.TenantId);

                log.Debug("GetMailboxAttachsCount()");

                var countAttachs = _garbageManager.GetMailboxAttachsCount(mailbox);

                log.Info("Found {0} garbage attachments", countAttachs);

                if (countAttachs > 0)
                {
                    var sumCount = 0;

                    log.Debug("GetMailboxAttachsGarbage(limit = {0})", _maxFilesToRemoveAtOnce);

                    var attachGrbgList = _garbageManager.GetMailboxAttachs(mailbox, _maxFilesToRemoveAtOnce);

                    sumCount += attachGrbgList.Count;

                    log.Info("Clearing {0} garbage attachments ({1}/{2})", attachGrbgList.Count, sumCount, countAttachs);

                    while (attachGrbgList.Any())
                    {
                        dataStorage.QuotaController = null;

                        log.Debug("dataStorage.DeleteFiles()");

                        dataStorage.DeleteFiles(string.Empty, attachGrbgList.Select(a => a.Path).ToList());

                        log.Debug("RemoveMailboxAttachsGarbage()");

                        _garbageManager.CleanupMailboxAttachs(attachGrbgList);

                        log.Debug("GetMailboxAttachsGarbage()");

                        attachGrbgList = _garbageManager.GetMailboxAttachs(mailbox, _maxFilesToRemoveAtOnce);

                        if (!attachGrbgList.Any()) continue;

                        sumCount += attachGrbgList.Count;

                        log.Info("Found {0} garbage attachments ({1}/{2})", attachGrbgList.Count, sumCount,
                                 countAttachs);
                    }
                }

                log.Debug("GetMailboxMessagesCount()");

                var countMessages = _garbageManager.GetMailboxMessagesCount(mailbox);

                log.Info("Found {0} garbage messages", countMessages);

                if (countMessages > 0)
                {
                    var sumCount = 0;

                    log.Debug("GetMailboxMessagesGarbage(limit = {0})", _maxFilesToRemoveAtOnce);

                    var messageGrbgList = _garbageManager.GetMailboxMessages(mailbox, _maxFilesToRemoveAtOnce);

                    sumCount += messageGrbgList.Count;

                    log.Info("Clearing {0} garbage messages ({1}/{2})", messageGrbgList.Count, sumCount, countMessages);

                    while (messageGrbgList.Any())
                    {
                        dataStorage.QuotaController = null;

                        log.Debug("dataStorage.DeleteFiles()");

                        dataStorage.DeleteFiles(string.Empty, messageGrbgList.Select(a => a.Path).ToList());

                        log.Debug("RemoveMailboxMessagesGarbage()");

                        _garbageManager.CleanupMailboxMessages(messageGrbgList);

                        log.Debug("GetMailboxMessagesGarbage()");

                        messageGrbgList = _garbageManager.GetMailboxMessages(mailbox, _maxFilesToRemoveAtOnce);

                        if (!messageGrbgList.Any()) continue;

                        sumCount += messageGrbgList.Count;

                        log.Info("Found {0} garbage messages ({1}/{2})", messageGrbgList.Count, sumCount,
                                 countMessages);
                    }
                }

                log.Debug("ClearMailboxData()");

                _garbageManager.CleanupMailboxData(mailbox, totalMailRemove);

                log.Debug("Garbage mailbox '{0}' was totaly removed.", mailbox.EMail.Address);
            }
            catch (Exception ex)
            {
                log.Error("RemoveMailboxData(mailboxId = {0}) Failure\r\nException: {1}", mailbox.MailBoxId, ex.ToString());
            }
        }

        public bool RemoveGarbageMailData(MailBox mailbox, int garbageDaysLimit, ILogger log)
        {
            //TODO: Implement cleanup data marked as removed and trash messages exceeded garbageDaysLimit

            return true;
        }

        private void RemoveTeamlabMailbox(MailBox mailbox)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox");

            if (!mailbox.IsTeamlab)
                return;

            try
            {
                var serverDal = new ServerDal(mailbox.TenantId);

                var serverData = serverDal.GetTenantServer();

                if ((ServerType) serverData.type != ServerType.Postfix)
                    throw new NotSupportedException();

                var mailserverfactory = new PostfixFactory();

                var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(2)
                        .Build();

                var dnsPresets = new DnsPresets.Builder()
                    .SetMx(serverData.mx_record, 0)
                    .SetSpfValue("Spf")
                    .SetDkimSelector("Dkim")
                    .SetDomainCheckPrefix("DomainCheck")
                    .Build();

                var setup = new ServerSetup
                    .Builder(serverData.id, mailbox.TenantId, mailbox.UserId)
                    .SetConnectionString(serverData.connection_string)
                    .SetLogger(_log)
                    .SetServerLimits(limits)
                    .SetDnsPresets(dnsPresets)
                    .Build();

                var mailServer = mailserverfactory.CreateServer(setup);

                var tlMailbox = mailServer.GetMailbox(mailbox.MailBoxId, mailserverfactory);

                if (tlMailbox == null)
                    return;

                var groups = mailServer.GetMailGroups(mailserverfactory);

                var groupsContainsMailbox = groups.Where(g => g.InAddresses.Contains(tlMailbox.Address))
                                                  .Select(g => g);

                foreach (var mailGroup in groupsContainsMailbox)
                {
                    if (mailGroup.InAddresses.Count == 1)
                    {
                        mailServer.DeleteMailGroup(mailGroup.Id, mailserverfactory);
                    }
                    else
                    {
                        mailGroup.RemoveMember(tlMailbox.Address.Id);
                    }
                }

                mailServer.DeleteMailbox(tlMailbox);

            }
            catch (Exception ex)
            {
                _log.Error("RemoveTeamlabMailbox(mailboxId = {0}) Failure\r\nException: {1}", mailbox.MailBoxId,
                           ex.ToString());
            }
        }

        #endregion
    }
}