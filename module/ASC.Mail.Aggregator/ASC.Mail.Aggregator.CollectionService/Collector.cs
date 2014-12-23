/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.CollectionService
{
    public class Collector
    {
        private readonly MailBoxManager _manager;
        private readonly MailItemManager _itemManger;
        private readonly MailQueueSettings _settings;
        private readonly MailWorkerQueue _queue;
        private readonly ILogger _log;
        private bool _noTasks;
        private readonly TasksConfig _tasksConfig;

        public Collector(MailBoxManager manager, MailQueueSettings settings, List<MessageHandlerBase> message_handlers) 
        {
            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "Collector");

            Environment.SetEnvironmentVariable("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

            _manager = manager;
            _settings = settings;
            _itemManger = new MailItemManager(_manager, message_handlers);
            _queue = new MailWorkerQueue(settings.ConcurrentThreadCount, settings.CheckInterval, this);

            _log.Info("MailWorkerQueue: ConcurrentThreadCount = {0} and CheckInterval = {1}", 
                settings.ConcurrentThreadCount, settings.CheckInterval);

            var config_builder = new TasksConfig.Builder();

            if (settings.WorkOnUsersOnly != null && settings.WorkOnUsersOnly.Any())
            {
                var i = 0;
                var users = string.Empty;
                settings.WorkOnUsersOnly.ForEach(user => users += string.Format("\r\n\t\t\t\t{0}. \"{1}\"", ++i, user));

                _log.Info("Aggreagtor will get tasks for this users only:" + users);
            }

            config_builder.SetUsersToWorkOn(settings.WorkOnUsersOnly);
            config_builder.SetOnlyTeamlabTasks(settings.OnlyTeamlabTasks);
            config_builder.SetActiveInterval(settings.ActivityTimeout);

            _tasksConfig = config_builder.Build();
        }

        public int ItemsPerSession { 
            get { 
                return _settings.MaxMessagesPerSession; 
            } 
        }

        public void Start()
        {
            try
            {
                _log.Info("Collector.Start() -> _queue.IsStarted = {0}", _queue.IsStarted);
                if (!_queue.IsStarted)
                {
                    _log.Debug("Starting collector\r\n");
                    AggregatorLogger.Instance.Start();
                    _queue.Start();
                }
            }
            catch (Exception e)
            {
                _log.Fatal("Collector.Start() Exception: {0}", e.ToString());
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _log.Info("Collector.Stop() -> _queue.IsStarted = {0}", _queue.IsStarted);
                if (_queue.IsStarted)
                {
                    _log.Debug("Stopping collector");
                    _queue.Stop();
                    AggregatorLogger.Instance.Stop();
                }
            }
            catch (Exception e)
            {
                _log.Fatal("Collector.Stop() Exception: {0}", e.ToString());
                throw;
            }
        }

        private static readonly object SyncObject = new object();
        private static bool _isGetMailboxRunning = false;

        public MailQueueItem GetItem()
        {
            try
            {
                while(true)
                {
                    if(!_noTasks) _log.Info("Getting new Item...");

                    MailBox mbox = null;
                    var locked_in_this_thread = false;
                    lock (SyncObject)
                    {
                        if (!_isGetMailboxRunning)
                        {
                            _isGetMailboxRunning = true;
                            locked_in_this_thread = true;
                        }
                    }
                    try
                    {
                        if (locked_in_this_thread && _isGetMailboxRunning)
                            mbox = _manager.GetMailboxForProcessing(_tasksConfig);
                    }
                    finally
                    {
                        if (locked_in_this_thread && _isGetMailboxRunning)
                            _isGetMailboxRunning = false;
                    }


                    if (mbox == null)
                    {
                        if (!_noTasks) _log.Info("Nothing to do.");
                        _noTasks = true;
                        break;
                    }

                    var absence = false;
                    var type = HttpRuntime.Cache.Get(mbox.TenantId.ToString(CultureInfo.InvariantCulture));
                    if (type == null)
                    {
                        _log.Info("Tenant {0} isn't in cache", mbox.TenantId);
                        absence = true;
                        try
                        {
                            type = _manager.GetTariffType(mbox.TenantId);
                        }
                        catch (Exception e)
                        {
                            _log.Error("Collector.GetItem() -> GetTariffType Exception: {0}", e.ToString());
                            type = MailBoxManager.TariffType.Active;
                        }
                    }
                    else
                    {
                        _log.Info("Tenant {0} is in cache", mbox.TenantId);
                    }

                    _noTasks = false;
                    _log.Info("MailboxId: {0} is processing. EMail: '{1}'  User: '{2}' TenantId: {3} ",
                              mbox.MailBoxId, 
                              mbox.EMail.Address, 
                              mbox.UserId, 
                              mbox.TenantId);

                    switch ((MailBoxManager.TariffType) type)
                    {
                        case MailBoxManager.TariffType.LongDead:
                            _log.Info("Tenant {0} is not paid. Disable mailboxes.", mbox.TenantId);
                            _manager.DisableMailboxesForTenant(mbox.TenantId);
                            break;
                        case MailBoxManager.TariffType.Overdue:
                            _log.Info("Tenant {0} is not paid. Stop processing mailboxes.", mbox.TenantId);
                            _manager.SetNextLoginDelayedForTenant(mbox.TenantId, _settings.OverdueAccountDelay);
                            break;
                        default:
                            var user_terminated = false;
                            try
                            {
                                CoreContext.TenantManager.SetCurrentTenant(mbox.TenantId);
                                var user = CoreContext.UserManager.GetUsers(new Guid(mbox.UserId));
                                if (user.Status == EmployeeStatus.Terminated)
                                {
                                    user_terminated = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex, "Cannot get current user = '{0}'", mbox.UserId);
                            }

                            if (user_terminated)
                            {
                                _log.Info("User '{0}' was terminated. TenantId: {1}. Stop processing mailboxes.", mbox.UserId, mbox.TenantId);
                                _manager.DisableMailboxesForUser(mbox.TenantId, mbox.UserId);
                                return null;
                            }

                            if (absence)
                            {
                                var mboxKey = mbox.TenantId.ToString(CultureInfo.InvariantCulture);
                                HttpRuntime.Cache.Remove(mboxKey);
                                HttpRuntime.Cache.Insert(mboxKey, type, null,
                                                         DateTime.UtcNow.Add(_settings.TenantCachingPeriod), Cache.NoSlidingExpiration);
                            }
                            _log.Debug("CreateItemForAccount()...");
                            return MailItemQueueFactory.CreateItemForAccount(mbox, _itemManger);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Collector.GetItem() Exception: {0}", e.ToString());
            }

            return null;
        }

        public void ItemCompleted(MailQueueItem item)
        {
            try
            {
                _log.Info("MailboxProcessingCompleted(MailBoxId: {0} Email: '{1}')\r\n", 
                    item.Account.MailBoxId, 
                    item.Account.EMail.Address);

                _manager.MailboxProcessingCompleted(item.Account);
            }
            catch (Exception e)
            {
                _log.Fatal("Collector.ItemCompleted(MailBoxId: {0} Email: '{1}') Exception: {2}\r\n", 
                    item.Account.MailBoxId, 
                    item.Account.EMail.Address, 
                    e.ToString());
            }
        }

        public void ItemError(MailQueueItem item, Exception exception)
        {
            try
            {
                _log.Info("MailboxProcessingError(MailBoxId: {0} Email: '{1}' Tenant: {2} User: '{3}' Exception:\r\n{4})\r\n",
                    item.Account.MailBoxId, 
                    item.Account.EMail.Address, 
                    item.Account.TenantId, 
                    item.Account.UserId, 
                    exception.ToString());

                _manager.MailboxProcessingError(item.Account, exception);
            }
            catch (Exception e)
            {
                _log.Fatal("Collector.ItemError(MailBoxId: {0} Email: '{1}' Tenant: {2} User: '{3}' ExceptionMessage: {4}) Exception:\r\n{5}\r\n",
                    item.Account.MailBoxId, 
                    item.Account.EMail.Address, 
                    item.Account.TenantId, 
                    item.Account.UserId, 
                    exception.ToString(), 
                    e.ToString());
            }
        }
    }
}