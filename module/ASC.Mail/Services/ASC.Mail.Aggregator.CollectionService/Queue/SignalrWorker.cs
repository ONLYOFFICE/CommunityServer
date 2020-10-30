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
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Core.Users;
using ASC.Mail.Core;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;

namespace ASC.Mail.Aggregator.CollectionService.Queue
{
    public class SignalrWorker : IDisposable
    {
        private readonly Queue<MailBoxData> _processingQueue;
        private Thread _worker;
        private volatile bool _workerTerminateSignal;
        private readonly EventWaitHandle _waitHandle;
        private readonly ILog _log;
        private static SignalrServiceClient _signalrServiceClient;
        private readonly TimeSpan _timeSpan;

        public SignalrWorker(bool startImmediately = true)
        {
            _log = LogManager.GetLogger("ASC.Mail.SignalrWorker");
            _workerTerminateSignal = false;
            _signalrServiceClient = new SignalrServiceClient("mail");
            _processingQueue = new Queue<MailBoxData>();
            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _timeSpan = TimeSpan.FromSeconds(10);

            _worker = new Thread(ProcessQueue);

            if (startImmediately)
                _worker.Start();
        }

        private void ProcessQueue()
        {
            while (!_workerTerminateSignal)
            {
                if (!HasQueuedMailbox)
                {
                    _log.Debug("No items, waiting.");
                    _waitHandle.WaitOne();
                    _log.Debug("Waking up...");
                }

                var mailbox = NextQueuedMailBoxData;
                if (mailbox == null)
                    continue;

                try
                {
                    _log.DebugFormat("signalrServiceClient.SendUnreadUser(UserId = {0} TenantId = {1})", mailbox.UserId,
                        mailbox.TenantId);

                    SendUnreadUser(mailbox.TenantId, mailbox.UserId);
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("signalrServiceClient.SendUnreadUser(UserId = {0} TenantId = {1}) Exception: {2}", mailbox.UserId,
                        mailbox.TenantId, ex.ToString());
                }

                _waitHandle.Reset();
            }
        }

        public int QueueCount
        {
            get
            {
                lock (_processingQueue)
                {
                    return _processingQueue.Count;
                }
            }
        }

        public bool HasQueuedMailbox
        {
            get
            {
                lock (_processingQueue)
                {
                    return _processingQueue.Any();
                }
            }
        }

        public MailBoxData NextQueuedMailBoxData
        {
            get
            {
                if (!HasQueuedMailbox)
                    return null;

                lock (_processingQueue)
                {
                    return _processingQueue.Dequeue();
                }
            }
        }

        public void Start()
        {
            if(!_worker.IsAlive)
                _worker.Start();
        }

        public void AddMailbox(MailBoxData item)
        {
            lock (_processingQueue)
            {
                if(!_processingQueue.Contains(item))
                    _processingQueue.Enqueue(item);
            }
            _waitHandle.Set();
        }

        public void Dispose()
        {
            if (_worker == null)
                return;

            _workerTerminateSignal = true;
            _waitHandle.Set();

            if (_worker.IsAlive)
            {
                _log.Info("Stop SignalrWorker.");

                if (!_worker.Join(_timeSpan))
                {
                    _log.Info("SignalrWorker busy, aborting the thread.");
                    _worker.Abort();
                }
            }

            _worker = null;
        }

        private static void SendUnreadUser(int tenant, string userId)
        {
            var log = LogManager.GetLogger("ASC.Mail.SignalrWorker");
            try
            {
                var engineFactory = new EngineFactory(tenant, userId);

                var mailFolderInfos = engineFactory.FolderEngine.GetFolders();

                var count = (from mailFolderInfo in mailFolderInfos
                    where mailFolderInfo.id == FolderType.Inbox
                    select mailFolderInfo.unreadMessages)
                    .FirstOrDefault();

                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var userInfo = CoreContext.UserManager.GetUsers(Guid.Parse(userId));
                if (userInfo.ID != Constants.LostUser.ID)
                {
                    // sendMailsCount
                    _signalrServiceClient.SendUnreadUser(tenant, userId, count);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }
    }
}
