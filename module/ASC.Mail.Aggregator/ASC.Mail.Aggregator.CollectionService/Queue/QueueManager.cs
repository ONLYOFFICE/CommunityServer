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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using ASC.Core;
using ASC.Mail.Aggregator.CollectionService.Queue.Data;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using LiteDB;

namespace ASC.Mail.Aggregator.CollectionService.Queue
{
    public class QueueManager : IDisposable
    {
        private readonly MailBoxManager _manager;
        private readonly int _maxItemsLimit;
        private readonly Queue<MailBox> _mailBoxQueue;
        private List<MailBox> _lockedMailBoxList;
        private readonly TasksConfig _tasksConfig;
        private readonly ILogger _log;
        private DateTime _loadQueueTime;
        private MemoryCache _tenantMemCache;

        private const string DBC_MAILBOXES = "mailboxes";
        private const string DBC_TENANTS = "tenants";
        private const string DBC_FILE = "dump.db";

        private readonly object _locker = new object();

        private LiteDatabase _db;
        private LiteCollection<MailboxData> _mailboxes;
        private LiteCollection<TenantData> _tenants;

        public ManualResetEvent CancelHandler { get; set; }

        public QueueManager(TasksConfig tasksConfig, ILogger log = null)
        {           
            _maxItemsLimit = tasksConfig.MaxTasksAtOnce;
            _mailBoxQueue = new Queue<MailBox>();
            _lockedMailBoxList = new List<MailBox>();
            _tasksConfig = tasksConfig;
            _log = log ?? new NullLogger();
            _loadQueueTime = DateTime.UtcNow;
            _tenantMemCache = new MemoryCache("QueueManagerTenantCache");

            CancelHandler = new ManualResetEvent(false);

            _manager = new MailBoxManager(_log)
            {
                AuthErrorWarningTimeout = _tasksConfig.AuthErrorWarningTimeout,
                AuthErrorDisableTimeout = _tasksConfig.AuthErrorDisableMailboxTimeout
            };

            LoadDump();
        }

        #region - public methods -

        public IEnumerable<MailBox> GetLockedMailboxes(int needTasks)
        {
            var mbList = new List<MailBox>();
            do
            {
                var mailBox = GetLockedMailbox();
                if (mailBox == null)
                    break;

                mbList.Add(mailBox);

            } while (mbList.Count < needTasks);

            return mbList;
        }

        public MailBox GetLockedMailbox()
        {
            MailBox mailBox;

            do
            {
                mailBox = GetQueuedMailbox();

            } while (mailBox != null && !TryLockMailbox(mailBox));

            if (mailBox == null)
                return null;

            _lockedMailBoxList.Add(mailBox);

            CancelHandler.Reset();

            AddMailboxToDumpDb(mailBox.ToMailboxData());

            return mailBox;
        }

        public void ReleaseAllProcessingMailboxes()
        {
            if (!_lockedMailBoxList.Any())
                return;

            var cloneCollection = new List<MailBox>(_lockedMailBoxList);

            _log.Info("QueueManager->ReleaseAllProcessingMailboxes()");

            foreach (var mailbox in cloneCollection)
            {
                ReleaseMailbox(mailbox);
            }
        }

        public void ReleaseMailbox(MailBox mailBox)
        {
            try
            {
                if (!_lockedMailBoxList.Contains(mailBox))
                {
                    _log.Warn("QueueManager->ReleaseMailbox(Tenant = {0} MailboxId = {1}, Address = '{2}') mailbox not found",
                               mailBox.TenantId, mailBox.MailBoxId, mailBox.EMail);
                    return;
                }

                _log.Info("QueueManager->ReleaseMailbox(MailboxId = {0} Address '{1}')", mailBox.MailBoxId, mailBox.EMail);

                CoreContext.TenantManager.SetCurrentTenant(mailBox.TenantId);

                _manager.SetMailboxProcessed(mailBox);

                _lockedMailBoxList.Remove(mailBox);

                DeleteMailboxFromDumpDb(mailBox.MailBoxId);
            }
            catch (Exception ex)
            {
                _log.Error("QueueManager->ReleaseMailbox(Tenant = {0} MailboxId = {1}, Address = '{2}')\r\nException: {3} \r\n",
                    mailBox.TenantId, mailBox.MailBoxId, mailBox.Account, ex.ToString());
            }
        }

        public int ProcessingCount
        {
            get { return _lockedMailBoxList.Count; }
        }

        public void LoadMailboxesFromDump()
        {
            if (_lockedMailBoxList.Any())
                return;

            try
            {
                _log.Debug("LoadMailboxesFromDump()");

                lock (_locker)
                {
                    var list = _mailboxes.FindAll().ToList();

                    _lockedMailBoxList = list.ConvertAll(m => m.ToMailbox()).ToList();
                }
            }
            catch (Exception ex)
            {
                _log.Error("LoadMailboxesFromDump: {0}", ex.ToString());

                ReCreateDump();
            }
        }

        public void LoadTenantsFromDump()
        {
            try
            {
                _log.Debug("LoadTenantsFromDump()");

                lock (_locker)
                {
                    var list = _tenants.FindAll().ToList();

                    foreach (var tenantData in list)
                    {
                        AddTenantToCache(tenantData, false);
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Error("LoadTenantsFromDump: {0}", ex.ToString());

                ReCreateDump();
            }
        }

        #endregion

        #region - private methods -

        private void ReCreateDump()
        {
            try
            {
                if (File.Exists(DBC_FILE))
                {
                    _log.Debug("Dump file '{0}' exists, trying delete", DBC_FILE);

                    File.Delete(DBC_FILE);

                    _log.Debug("Dump file '{0}' deleted", DBC_FILE);
                }

                _db = new LiteDatabase(DBC_FILE);

                lock (_locker)
                {
                    _mailboxes = _db.GetCollection<MailboxData>(DBC_MAILBOXES);
                    _tenants = _db.GetCollection<TenantData>(DBC_TENANTS);
                }
            }
            catch (Exception ex)
            {
                _log.Error("ReCreateDump() failed Exception: {0}", ex.ToString());
            }
        }

        private void AddMailboxToDumpDb(MailboxData mailboxData)
        {
            try
            {
                lock (_locker)
                {
                    var mailbox = _mailboxes.FindOne(Query.EQ("MailboxId", mailboxData.MailboxId));

                    if (mailbox != null)
                        return;

                    _mailboxes.Insert(mailboxData);

                    // Create, if not exists, new index on Name field
                    _mailboxes.EnsureIndex(x => x.MailboxId);
                }
            }
            catch (Exception ex)
            {
                _log.Error("AddMailboxToDumpDb(id={0}) Exception: {1}", mailboxData.MailboxId, ex.ToString());

                ReCreateDump();
            }
        }

        private void DeleteMailboxFromDumpDb(int mailBoxId)
        {
            try
            {
                lock (_locker)
                {
                    _mailboxes.Delete(Query.EQ("MailboxId", mailBoxId));
                }
            }
            catch (Exception ex)
            {
                _log.Error("DeleteMailboxFromDumpDb(mailboxId={0}) Exception: {1}", mailBoxId, ex.ToString());

                ReCreateDump();
            }
        }

        private void LoadDump()
        {
            try
            {
                _db = new LiteDatabase(DBC_FILE);

                lock (_locker)
                {
                    _tenants = _db.GetCollection<TenantData>(DBC_TENANTS);
                    _mailboxes = _db.GetCollection<MailboxData>(DBC_MAILBOXES);
                }
            }
            catch (Exception ex)
            {
                _log.Error("LoadDump() failed Exception: {0}", ex.ToString());

                ReCreateDump();
            }
        }

        private void AddTenantToDumpDb(TenantData tenantData)
        {
            try
            {
                lock (_locker)
                {
                    var tenant = _tenants.FindOne(Query.EQ("Tenant", tenantData.Tenant));

                    if(tenant != null)
                        return;
                     
                    _tenants.Insert(tenantData);

                    // Create, if not exists, new index on Name field
                    _tenants.EnsureIndex(x => x.Tenant);
                }
            }
            catch (Exception ex)
            {
                _log.Error("AddTenantToDumpDb(tenantId={0}) Exception: {1}", tenantData.Tenant, ex.ToString());

                ReCreateDump();
            }
        }

        private void DeleteTenantFromDumpDb(int tenantId)
        {
            try
            {
                lock (_locker)
                {
                    _tenants.Delete(Query.EQ("Tenant", tenantId));
                }
            }
            catch (Exception ex)
            {
                _log.Error("DeleteTenantFromDumpDb(tenant={0}) Exception: {1}", tenantId, ex.ToString());

                ReCreateDump();
            }
        }

        private void AddTenantToCache(TenantData tenantData, bool needDump = true)
        {
            var now = DateTime.UtcNow;

            if (tenantData.Expired < now)
            {
                DeleteTenantFromDumpDb(tenantData.Tenant);
                return; // Skip Expired tenant
            }

            var cacheItem = new CacheItem(tenantData.Tenant.ToString(CultureInfo.InvariantCulture), tenantData);

            var nowOffset = tenantData.Expired - now;              

            var absoluteExpiration = DateTime.UtcNow.Add(nowOffset);

            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = CacheEntryRemove,
                AbsoluteExpiration = absoluteExpiration
            };

            _tenantMemCache.Add(cacheItem, cacheItemPolicy);

            if (!needDump)
                return;

            AddTenantToDumpDb(tenantData);
        }

        private bool QueueIsEmpty
        {
            get { return !_mailBoxQueue.Any(); }
        }

        private bool QueueLifetimeExpired
        {
            get { return DateTime.UtcNow - _loadQueueTime >= _tasksConfig.QueueLifetime; }
        }

        private void LoadQueue()
        {
            try
            {
                var mbList = _manager.GetMailboxesForProcessing(_tasksConfig, _maxItemsLimit);
                ReloadQueue(mbList);
            }
            catch (Exception ex)
            {
                _log.Error("QueueManager->LoadQueue()\r\nException: \r\n {0}", ex.ToString());
            }
        }

        private MailBox GetQueuedMailbox()
        {
            if (QueueIsEmpty || QueueLifetimeExpired)
            {
                _log.Debug("Queue is {0}. Load new queue.", QueueIsEmpty ? "EMPTY" : "EXPIRED");

                LoadQueue();
            }

            return !QueueIsEmpty ? _mailBoxQueue.Dequeue() : null;
        }

        private void RemoveFromQueue(int tenant)
        {
            var mbList = _mailBoxQueue.Where(mb => mb.TenantId != tenant).Select(mb => mb).ToList();
            ReloadQueue(mbList);
        }

        private void RemoveFromQueue(int tenant, string user)
        {
            var mbList = _mailBoxQueue.Where(mb => mb.TenantId != tenant && mb.UserId != user).Select(mb => mb).ToList();
            ReloadQueue(mbList);
        }

        private void ReloadQueue(IEnumerable<MailBox> mbList)
        {
            _mailBoxQueue.Clear();
            _mailBoxQueue.PushRange(mbList);
            _loadQueueTime = DateTime.UtcNow;
        }

        private bool TryLockMailbox(MailBox mailbox)
        {
            _log.Debug("TryLockMailbox(MailboxId={0} is {1})", mailbox.MailBoxId, mailbox.Active ? "active" : "inactive");

            try
            {
                var contains = _tenantMemCache.Contains(mailbox.TenantId.ToString(CultureInfo.InvariantCulture));

                if (!contains)
                {
                    _log.Debug("Tenant {0} isn't in cache", mailbox.TenantId);
                    try
                    {
                        var type = mailbox.GetTenantStatus(_tasksConfig.TenantOverdueDays, _tasksConfig.DefaultApiSchema, _log);

                        switch (type)
                        {
                            case Defines.TariffType.LongDead:
                                _log.Info("Tenant {0} is not paid. Disable mailboxes.", mailbox.TenantId);
                                _manager.DisableMailboxesForTenant(mailbox.TenantId);
                                RemoveFromQueue(mailbox.TenantId);
                                return false;
                            case Defines.TariffType.Overdue:
                                _log.Info("Tenant {0} is not paid. Stop processing mailboxes.", mailbox.TenantId);
                                _manager.SetNextLoginDelayedForTenant(mailbox.TenantId, _tasksConfig.OverdueAccountDelay);
                                RemoveFromQueue(mailbox.TenantId);
                                return false;
                            default:
                                _log.Info("Tenant {0} is paid.", mailbox.TenantId);

                                if (mailbox.IsUserTerminated() || mailbox.IsUserRemoved())
                                {
                                    _log.Info("User '{0}' was terminated. Tenant = {1}. Disable mailboxes for user.",
                                              mailbox.UserId,
                                              mailbox.TenantId);
                                    _manager.DisableMailboxesForUser(mailbox.TenantId, mailbox.UserId);
                                    RemoveFromQueue(mailbox.TenantId, mailbox.UserId);
                                    return false;
                                }

                                var expired = DateTime.UtcNow.Add(_tasksConfig.TenantCachingPeriod);

                                var tenantData = new TenantData
                                {
                                    Tenant = mailbox.TenantId,
                                    TariffType = type,
                                    Expired = expired
                                };

                                AddTenantToCache(tenantData);

                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error("TryLockMailbox() -> GetTariffType Exception:\r\n{0}\r\n", e.ToString());
                    }

                }
                else
                {
                    _log.Debug("Tenant {0} is in cache", mailbox.TenantId);
                }

                if (mailbox.IsTenantQuotaEnded(_tasksConfig.TenantMinQuotaBalance, _log))
                {
                    _log.Info("Tenant = {0} User = {1}. Quota is ended.", mailbox.TenantId, mailbox.UserId);
                    if (!mailbox.QuotaError)
                    {
                        _manager.CreateQuotaErrorWarningAlert(mailbox.TenantId, mailbox.UserId);
                    }
                    _manager.SetNextLoginDelayedForUser(mailbox.TenantId, mailbox.UserId, _tasksConfig.QuotaEndedDelay);
                    RemoveFromQueue(mailbox.TenantId, mailbox.UserId);
                    return false;
                }

                return _manager.LockMailbox(mailbox.MailBoxId, true);

            }
            catch (Exception ex)
            {
                _log.Error("TryLockMailbox(MailboxId={0} is {1}) Exception:\r\n{2}\r\n", mailbox.MailBoxId,
                           mailbox.Active ? "active" : "inactive", ex.ToString());

                return false;
            }

        }

        private void CacheEntryRemove(CacheEntryRemovedArguments arguments)
        {
            if (arguments.RemovedReason == CacheEntryRemovedReason.CacheSpecificEviction)
                return;

            var tenantId = Convert.ToInt32(arguments.CacheItem.Key);

            _log.Info("Tenant {0} payment cache is expired.", tenantId);

            DeleteTenantFromDumpDb(tenantId);
        }

        #endregion

        public void Dispose()
        {
            _tenantMemCache.Dispose();
            _tenantMemCache = null;

            _db.Dispose();
        }
    }

}
