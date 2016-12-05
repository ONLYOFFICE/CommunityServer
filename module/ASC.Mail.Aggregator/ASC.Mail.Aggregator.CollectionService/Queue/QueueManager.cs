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
using System.Net.Mail;
using System.Runtime.Caching;
using System.Threading;
using ASC.Core;
using ASC.Mail.Aggregator.CollectionService.Queue.Data;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;

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
        private readonly string _queueDumpPath;
        private readonly string _tenantDumpPath;

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

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            _queueDumpPath = Path.Combine(dir, "dump.queue.json");
            _tenantDumpPath = Path.Combine(dir, "dump.tenants.json");
        }

        #region - public methods -

        public IEnumerable<MailBox> GetLockedMailboxes(int needTasks)
        {
            var mailboxes = new List<MailBox>();
            do
            {
                var mailBox = GetLockedMailbox();
                if (mailBox == null)
                    break;

                mailboxes.Add(mailBox);

            } while (mailboxes.Count < needTasks);

            return mailboxes;
        }

        public MailBox GetLockedMailbox()
        {
            MailBox mailBox;

            do
            {
                mailBox = GetQueuedMailbox();

            } while (mailBox != null && !TryLockMailbox(mailBox));

            if (mailBox != null)
            {
                _lockedMailBoxList.Add(mailBox);
                CancelHandler.Reset();
            }

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

        public void LoadQueueFromDump()
        {
            if (_lockedMailBoxList.Any() || !File.Exists(_queueDumpPath))
                return;

            try
            {
                _log.Debug("LoadQueueFromDump('{0}')", _queueDumpPath);

                var list = MailUtil.RestoreFromJson<List<MailboxData>>(_queueDumpPath);

                _lockedMailBoxList = list.ConvertAll(m => new MailBox
                    {
                        TenantId = m.TenantId,
                        UserId = m.UserId,
                        MailBoxId = m.MailBoxId,
                        EMail = new MailAddress(m.EMail),
                        Imap = m.Imap,
                        IsTeamlab = m.IsTeamlab,
                        Size = m.Size,
                        MessagesCount = m.MessagesCount
                    }).ToList();
            }
            catch (Exception ex)
            {
                _log.Error("LoadQueueFromDump: {0}", ex.ToString());
                RemoveDump(_queueDumpPath);
            }
        }

        public void SaveQueueToDump()
        {
            try
            {
                _log.Debug("SaveQueueToDump()");

                if (!_lockedMailBoxList.Any())
                {
                    RemoveDump(_queueDumpPath);
                    return;
                }

                var list = _lockedMailBoxList
                    .ConvertAll(m =>
                        new MailboxData
                        {
                            TenantId = m.TenantId,
                            UserId = m.UserId,
                            MailBoxId = m.MailBoxId,
                            EMail = m.EMail.Address,
                            Imap = m.Imap,
                            IsTeamlab = m.IsTeamlab,
                            MessagesCount = m.MessagesCount,
                            Size = m.Size
                        });

                MailUtil.SaveToJson(_queueDumpPath, list);
            }
            catch (Exception ex)
            {
                _log.Error("SaveQueueToDump: {0}", ex.ToString());
                RemoveDump(_queueDumpPath);
            }
        }

        public void LoadTenantsFromDump()
        {
            try
            {
                if (!File.Exists(_tenantDumpPath))
                    return;

                _log.Debug("LoadTenantsFromDump('{0}')", _tenantDumpPath);

                var list = MailUtil.RestoreFromJson<List<TenantData>>(_tenantDumpPath);

                foreach (var tenantData in list)
                {
                    AddTenantToCache(tenantData);
                }

                if (_tenantMemCache.Any())
                    return;

                RemoveDump(_tenantDumpPath);
            }
            catch (Exception ex)
            {
                _log.Error("LoadTenantsFromDump: {0}", ex.ToString());
                RemoveDump(_tenantDumpPath);
            }
        }

        #endregion

        #region - private methods -

        private void AddTenantToCache(TenantData tenantData)
        {
            var now = DateTime.UtcNow;

            if (tenantData.ExpiredDate < now)
                return; // Skip Expired tenant

            var cacheItem = new CacheItem(tenantData.TenantId.ToString(CultureInfo.InvariantCulture), tenantData);

            var nowOffset = tenantData.ExpiredDate - now;              

            var absoluteExpiration = DateTime.UtcNow.Add(nowOffset);

            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = CacheEntryRemove,
                AbsoluteExpiration = absoluteExpiration
            };

            _tenantMemCache.Add(cacheItem, cacheItemPolicy);
        }

        private void SaveTenantsToDump()
        {
            try
            {
                if (!_tenantMemCache.Any())
                {
                    RemoveDump(_tenantDumpPath);
                    return;
                }

                _log.Debug("SaveTenantsToDump()");

                var list = _tenantMemCache.ToList()
                    .ConvertAll(t => t.Value as TenantData);

                MailUtil.SaveToJson(_tenantDumpPath, list);
            }
            catch (Exception ex)
            {
                _log.Error("SaveTenantsToDump: {0}", ex.ToString());
                RemoveDump(_tenantDumpPath);
            }
        }

        private void RemoveDump(string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                _log.Debug("Dump file found. Removing '{0}'", path);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _log.Error("RemoveDump(\"{0}\"): {1}", path, ex.ToString());
            }
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
                var mailboxes = _manager.GetMailboxesForProcessing(_tasksConfig, _maxItemsLimit);
                ReloadQueue(mailboxes);
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
            var mailboxes = _mailBoxQueue.Where(mb => mb.TenantId != tenant).Select(mb => mb).ToList();
            ReloadQueue(mailboxes);
        }

        private void RemoveFromQueue(int tenant, string user)
        {
            var mailboxes = _mailBoxQueue.Where(mb => mb.TenantId != tenant && mb.UserId != user).Select(mb => mb).ToList();
            ReloadQueue(mailboxes);
        }

        private void ReloadQueue(IEnumerable<MailBox> mailboxes)
        {
            _mailBoxQueue.Clear();
            _mailBoxQueue.PushRange(mailboxes);
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
                                    TenantId = mailbox.TenantId,
                                    TariffType = type,
                                    ExpiredDate = expired
                                };

                                AddTenantToCache(tenantData);

                                SaveTenantsToDump();

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
            if (arguments.RemovedReason != CacheEntryRemovedReason.CacheSpecificEviction)
            {
                _log.Info("Tenant {0} payment cache is expired.", Convert.ToInt32(arguments.CacheItem.Key));

                SaveTenantsToDump();
            }
        }

        #endregion

        public void Dispose()
        {
            _tenantMemCache.Dispose();
            _tenantMemCache = null;
        }
    }

}
