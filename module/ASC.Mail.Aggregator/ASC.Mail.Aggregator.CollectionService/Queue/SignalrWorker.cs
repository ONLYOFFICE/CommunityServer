/*
 *
 * (c) Copyright Ascensio System Limited 2010-2017
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
using System.Linq;
using System.Threading;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Core.Users;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.CollectionService.Queue
{
    public class SignalrWorker : IDisposable
    {
        private readonly Queue<MailBox> _processingQueue;
        private Thread _worker;
        private volatile bool _workerTerminateSignal;
        private readonly EventWaitHandle _waitHandle;
        private readonly ILogger _log;
        private static SignalrServiceClient _signalrServiceClient;
        private readonly TimeSpan _timeSpan;

        public SignalrWorker(bool startImmediately = true)
        {
            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "SignalrWorker");
            _workerTerminateSignal = false;
            _signalrServiceClient = new SignalrServiceClient("mail");
            _processingQueue = new Queue<MailBox>();
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

                var mailbox = NextQueuedMailBox;
                if (mailbox == null)
                    continue;

                try
                {
                    _log.Debug("signalrServiceClient.SendUnreadUser(UserId = {0} TenantId = {1})", mailbox.UserId,
                        mailbox.TenantId);

                    SendUnreadUser(mailbox.TenantId, mailbox.UserId);
                }
                catch (Exception ex)
                {
                    _log.Error("signalrServiceClient.SendUnreadUser(UserId = {0} TenantId = {1}) Exception: {2}", mailbox.UserId,
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

        public MailBox NextQueuedMailBox
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

        public void AddMailbox(MailBox item)
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
            var log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "SignalrService");
            try
            {
                var count = 0;
                var mailBoxManager = new MailBoxManager(log);

                var mailFolderInfos = mailBoxManager.GetFolders(tenant, userId);

                foreach (var mailFolderInfo in mailFolderInfos)
                {
                    if (mailFolderInfo.id != MailFolder.Ids.inbox) continue;
                    count = mailFolderInfo.unreadMessages;
                    break;
                }
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
                log.Error("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }
    }
}
