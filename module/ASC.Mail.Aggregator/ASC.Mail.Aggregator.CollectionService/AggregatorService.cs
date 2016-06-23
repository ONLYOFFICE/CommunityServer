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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASC.Core;
using ASC.Mail.Aggregator.CollectionService.Queue;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Clients;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using DotNetOpenAuth.Messaging;
using MailKit.Security;
using ASC.Core.Notify.Signalr;
using ASC.Mail.Aggregator.Common.DataStorage;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;

namespace ASC.Mail.Aggregator.CollectionService
{
    public sealed class AggregatorService: ServiceBase
    {
        public const string AscMailCollectionServiceName = "ASC Mail Collection Service";
        private readonly ILogger _log;

        private readonly CancellationTokenSource _cancelTokenSource;
        readonly ManualResetEvent _resetEvent;
        private Timer _workTimer;
        private Timer _gcCleanerTimer;

        private readonly TasksConfig _tasksConfig;

        private readonly QueueManager _queueManager;
        readonly TaskFactory _taskFactory;
        private readonly TimeSpan _tsTaskStateCheckInterval;

        private bool _isFirstTime = true;

        private static SignalrServiceClient _signalrServiceClient;

        private const int SIGNALR_WAIT_SECONDS = 30;

        private TimeSpan _gcCleanTimerInterval = TimeSpan.FromMinutes(5); 

        private static readonly object Locker = new object();

        private readonly TimeSpan _taskSecondsLifetime;

        public AggregatorService(Options options)
        {
            ServiceName = AscMailCollectionServiceName;
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            // type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = false;
            CanHandleSessionChangeEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            try
            {
                _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "MainThread");

                Environment.SetEnvironmentVariable("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

                _tasksConfig = TasksConfig.FromConfig;

                if (options.OnlyUsers != null)
                    _tasksConfig.WorkOnUsersOnly.AddRange(options.OnlyUsers.ToList());

                if (options.NoMessagesLimit)
                    _tasksConfig.MaxMessagesPerSession = -1;

                _taskSecondsLifetime =
                    TimeSpan.FromSeconds(ConfigurationManager.AppSettings["mail.task-process-lifetime-seconds"] != null
                        ? Convert.ToInt32(ConfigurationManager.AppSettings["mail.task-process-lifetime-seconds"])
                        : 300);

                _queueManager = new QueueManager(_tasksConfig, _log);

                _resetEvent = new ManualResetEvent(false);

                _cancelTokenSource = new CancellationTokenSource();

                _taskFactory = new TaskFactory();

                _tsTaskStateCheckInterval = ConfigurationManager.AppSettings["mail.task-check-state-seconds"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.task-check-state-seconds"]))
                        : TimeSpan.FromSeconds(30);

                if(_tasksConfig.EnableSignalr)
                    _signalrServiceClient = new SignalrServiceClient();

                _workTimer = new Timer(workTimer_Elapsed, _cancelTokenSource.Token, Timeout.Infinite, Timeout.Infinite);

                _gcCleanerTimer = new Timer(gcCleanerTimer_Elapsed, _cancelTokenSource.Token, _gcCleanTimerInterval, _gcCleanTimerInterval);

                _log.Info("Service is ready.");
            }
            catch (Exception ex)
            {
                _log.Fatal("CollectorService error under construct: {0}", ex.ToString());
            }
        }

        private void gcCleanerTimer_Elapsed(object state)
        {
            _log.Info("gcCleanerTimer_Elapsed: GC.Collect()");
            _gcCleanerTimer.Change(Timeout.Infinite, Timeout.Infinite);
            GC.Collect();
            _gcCleanerTimer.Change(_gcCleanTimerInterval, _gcCleanTimerInterval);
        }

        #region - Overrides base -

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Start service\r\n");

                base.OnStart(args);

                StartTimer(true);
            }
            catch (Exception)
            {
                OnStop();
            }
        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            _cancelTokenSource.Cancel();

            _log.Info("Stop service\r\n");

            _queueManager.CancelHandler.WaitOne();

            StopTimer();

            if (_workTimer != null)
            {
                _workTimer.Dispose();
                _workTimer = null;
            }

            if (_gcCleanerTimer != null)
            {
                _gcCleanerTimer.Dispose();
                _gcCleanerTimer = null;
            }

            _resetEvent.Set();

            _queueManager.Dispose();

            base.OnStop();
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            _log.Info("Service shutdown.");
            OnStop();
            _log.Flush();
            base.OnShutdown();
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _log.Info("Service power event detected = {0}.", powerStatus.ToString());
            _log.Flush();
            if (powerStatus == PowerBroadcastStatus.Suspend)
            {
                OnStop();
            }
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// Start service in console mode.
        /// </summary>
        public void StartConsole()
        {
            _log.Info("Service Start in console-daemon mode");
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
            _resetEvent.WaitOne();
        }

        #endregion

        #region - Timer_Elapsed -

        private void workTimer_Elapsed(object state)
        {
            _log.Debug("Timer->workTimer_Elapsed");

            var cancelToken = state as CancellationToken? ?? new CancellationToken();

            try
            {
                if (_isFirstTime)
                {
                    _queueManager.LoadQueueFromDump();

                    if (_queueManager.ProcessingCount > 0)
                    {
                        _log.Info("Found {0} tasks to release", _queueManager.ProcessingCount);

                        _queueManager.ReleaseAllProcessingMailboxes();

                        _queueManager.SaveQueueToDump();
                    }

                    _isFirstTime = false;
                }

                if (cancelToken.IsCancellationRequested)
                {
                    _log.Debug("Timer->workTimer_Elapsed: IsCancellationRequested. Quit.");
                    return;
                }

                if (_workTimer == null)
                {
                    _log.Debug("Timer->workTimer_Elapsed: _workTimer == null. Quit.");
                    OnStop();
                    return;
                }

                StopTimer();

                var tasks = CreateTasks(_tasksConfig.MaxTasksAtOnce, cancelToken);

                if (tasks.Any())
                    _queueManager.SaveQueueToDump();

                // ***Add a loop to process the tasks one at a time until none remain. 
                while (tasks.Any())
                {
                    // Identify the first task that completes.
                    var indexTask = Task.WaitAny(tasks.ToArray<Task>(), (int)_tsTaskStateCheckInterval.TotalMilliseconds, cancelToken);
                    if (indexTask > -1)
                    {
                        // ***Remove the selected task from the list so that you don't 
                        // process it more than once.
                        var outTask = tasks[indexTask];

                        FreeTask(outTask, tasks);

                        _queueManager.SaveQueueToDump();
                    }
                    else
                    {
                        _log.Info("Task.WaitAny timeout. Tasks count = {0}\r\nTasks:\r\n{1}", tasks.Count,
                                  string.Join("\r\n",
                                              tasks.Select(t => string.Format("Id: {0} Status: {1}", t.Id, t.Status))));
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

                        tasks2Free.ForEach(task => FreeTask(task, tasks));
                    }

                    var difference = _tasksConfig.MaxTasksAtOnce - tasks.Count;

                    if (difference <= 0) continue;

                    var newTasks = CreateTasks(difference, cancelToken);

                    tasks.AddRange(newTasks);

                    _queueManager.SaveQueueToDump();

                    _log.Info("Total tasks count = {0} ({1}).", tasks.Count,
                              string.Join(",", tasks.Select(t => t.Id)));
                }

                _log.Info("All mailboxes were processed. Go back to timer.");
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    ex = ((AggregateException)ex).GetBaseException();
                }

                if (ex is TaskCanceledException || ex is OperationCanceledException)
                {
                    _log.Info("Execution was canceled.");

                    _queueManager.ReleaseAllProcessingMailboxes();

                    _queueManager.SaveQueueToDump();

                    _queueManager.CancelHandler.Set();

                    return;
                }

                _log.Error("Timer->workTimer_Elapsed. Exception:\r\n{0}\r\n", ex.ToString());

                if (_queueManager.ProcessingCount != 0)
                {
                    _queueManager.ReleaseAllProcessingMailboxes();
                }
            }

            _queueManager.SaveQueueToDump();

            _queueManager.CancelHandler.Set();

            StartTimer();
        }

        private void StartTimer(bool immediately = false)
        {
            if (_workTimer == null)
                return;

            _log.Debug("Setup _workTimer to {0} seconds", _tasksConfig.CheckTimerInterval.TotalSeconds);
            if (immediately)
            {
                _workTimer.Change(0, Timeout.Infinite);
            }
            else
            {
                _workTimer.Change(_tasksConfig.CheckTimerInterval, _tasksConfig.CheckTimerInterval);
            }
        }

        private void StopTimer()
        {
            if (_workTimer == null)
                return;

            _log.Debug("Setup _workTimer to Timeout.Infinite");
            _workTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void NotifySignalr(MailBox mailbox, ILogger log)
        {
            log.Debug("signalrServiceClient.SendUnreadUser(UserId = {0} TenantId = {1})", mailbox.UserId,
                mailbox.TenantId);

            lock (Locker)
            {
                _signalrServiceClient.SendUnreadUser(mailbox.TenantId, mailbox.UserId);
            }
        }

        private List<Task<MailBox>> CreateTasks(int needCount, CancellationToken cancelToken)
        {
            _log.Info("CreateTasks(need {0} tasks).", needCount);

            var mailboxes = _queueManager.GetLockedMailboxes(needCount);

            var tasks =
                mailboxes.Select(
                    mailbox =>
                    {
                        var timeoutCancel = new CancellationTokenSource(_taskSecondsLifetime);

                        var commonCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutCancel.Token).Token;

                        var task = _taskFactory.StartNew(() => ProcessMailbox(mailbox, _tasksConfig, commonCancelToken),
                            commonCancelToken);

                        return task;
                    }).ToList();

            if (tasks.Any())
                _log.Info("Created {0} tasks.", tasks.Count);
            else
                _log.Info("No more mailboxes for processing.");

            return tasks;
        }

        private MailBox ProcessMailbox(MailBox mailbox, TasksConfig tasksConfig, CancellationToken cancelToken)
        {
            var taskLogger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "Task_" + Task.CurrentId);

            taskLogger.Info(
                "ProcessMailbox(Tenant = {0}, MailboxId = {1} Address = '{2}') Is {3}",
                mailbox.TenantId, mailbox.MailBoxId,
                mailbox.EMail, mailbox.Active ? "Active" : "Inactive");

            var manager = new MailBoxManager(taskLogger)
            {
                AuthErrorWarningTimeout = tasksConfig.AuthErrorWarningTimeout,
                AuthErrorDisableTimeout = tasksConfig.AuthErrorDisableMailboxTimeout
            };

            try
            {
                using (var client = new MailClient(mailbox, cancelToken, _tasksConfig.TcpTimeout, _tasksConfig.SslCertificateErrorsPermit, taskLogger))
                {
                    client.Authenticated += ClientOnAuthenticated;
                    client.GetMessage += ClientOnGetMessage;

                    client.Aggregate(tasksConfig, tasksConfig.MaxMessagesPerSession);

                    client.GetMessage -= ClientOnGetMessage;
                    client.Authenticated -= ClientOnAuthenticated;
                }

                taskLogger.Info("Mailbox '{0}' has been processed.", mailbox.EMail);
            }
            catch (OperationCanceledException)
            {
                taskLogger.Info("Task canceled.");
                if (_tasksConfig.EnableSignalr)
                {
                    NotifySignalr(mailbox, taskLogger);
                }
            }
            catch (AuthenticationException authEx)
            {
                taskLogger.Error(
                    "ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, authEx.ToStringDescriptive());

                if (!mailbox.AuthErrorDate.HasValue)
                    manager.SetMailboxAuthError(mailbox, true);
            }
            catch (Exception ex)
            {
                taskLogger.Error(
                    "ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail,
                    ex is ImapProtocolException || ex is Pop3ProtocolException ? ex.Message : ex.ToString());
            }

            return mailbox;
        }

        private void ClientOnAuthenticated(object sender, MailClientEventArgs mailClientEventArgs)
        {
            if (!mailClientEventArgs.Mailbox.AuthErrorDate.HasValue)
                return;

            var manager = new MailBoxManager();
            manager.SetMailboxAuthError(mailClientEventArgs.Mailbox, false);
        }

        private void ClientOnGetMessage(object sender, MailClientMessageEventArgs mailClientMessageEventArgs)
        {
            var log = _log;

            try
            {
                var mimeMessage = mailClientMessageEventArgs.Message;
                var uid = mailClientMessageEventArgs.MessageUid;
                var folder = mailClientMessageEventArgs.Folder;
                var mailbox = mailClientMessageEventArgs.Mailbox;
                var unread = mailClientMessageEventArgs.Unread;
                var fromEmail = mimeMessage.From.Mailboxes.FirstOrDefault();
                log = mailClientMessageEventArgs.Logger;

                var manager = new MailBoxManager(log);

                var md5 = string.Format("{0}|{1}|{2}|{3}",
                    mimeMessage.From.Mailboxes.Any() ? mimeMessage.From.Mailboxes.First().Address : "",
                    mimeMessage.Subject,
                    mimeMessage.Date.UtcDateTime,
                    mimeMessage.MessageId)
                    .GetMd5();

                var uidl = mailbox.Imap ? string.Format("{0}-{1}", uid, folder.FolderId) : uid;

                var fromThisMailBox =
                    fromEmail != null &&
                    fromEmail.Address.ToLowerInvariant().Equals(mailbox.EMail.Address.ToLowerInvariant());

                var toThisMailBox =
                    mimeMessage.To.Mailboxes.Select(addr => addr.Address.ToLowerInvariant())
                        .Contains(mailbox.EMail.Address.ToLowerInvariant());

                log.Info(@"Message:
    MimeId:   '{0}'
    Subject:  '{1}'
    Date:     {2}
    FolderId: {3} ('{4}')
    Unread:   {5}
    Uidl:     '{6}'
    Md5:      '{7}'
    To->From: {8}
    From->To: {9}",
                    mimeMessage.MessageId,
                    mimeMessage.Subject,
                    mimeMessage.Date,
                    folder.FolderId,
                    folder.Name,
                    unread,
                    uidl,
                    md5,
                    fromThisMailBox,
                    toThisMailBox);

                var tagsIds = folder.Tags.Any() ? manager.GetOrCreateTags(mailbox.TenantId, mailbox.UserId, folder.Tags) : null;

                var found = manager
                    .SearchExistingMessagesAndUpdateIfNeeded(
                        mailbox,
                        folder.FolderId,
                        uidl,
                        md5,
                        mimeMessage.MessageId,
                        fromThisMailBox,
                        toThisMailBox,
                        tagsIds);

                var needSave = !found;
                if (!needSave)
                    return;

                var chainId = manager.DetectChainId(mailbox, mimeMessage.MessageId, mimeMessage.InReplyTo,
                    mimeMessage.Subject);

                var streamId = MailUtil.CreateStreamId();

                log.Debug("MimeMessage->MailMessage");

                var message = new MailMessage(mimeMessage, folder.FolderId, unread, chainId, streamId);

                if (!TryStoreMailData(message, mailbox, log))
                    return;

                log.Debug("MailSave()");

                var folderRestoreId = folder.FolderId == MailFolder.Ids.spam ? MailFolder.Ids.inbox : folder.FolderId;

                message.Id = manager.MailSave(mailbox, message, 0, folder.FolderId, folderRestoreId, uidl, md5, true);

                log.Info("MailSave(Account:{0}) returned mailId = {1}\r\n", mailbox.EMail, message.Id);

                log.Debug("DoOptionalOperations()");

                DoOptionalOperations(message, mimeMessage, mailbox, tagsIds, log);

                bool isMailboxRemoved;
                bool isMailboxDeactivated;
                DateTime beginDate;

                manager.GetMailBoxState(mailbox.MailBoxId, out isMailboxRemoved, out isMailboxDeactivated, out beginDate);

                if (mailbox.BeginDate != beginDate)
                {
                    mailbox.BeginDateChanged = true;
                    mailbox.BeginDate = beginDate;
                }

                var client = sender as MailClient;

                if (isMailboxRemoved)
                {
                    if (client != null)
                        client.Cancel();

                    manager.DeleteMessages(
                        mailbox.TenantId,
                        mailbox.UserId,
                        new List<int>
                        {
                            (int) message.Id
                        });

                    log.Info("MailBox with id={0} is removed.\r\n", mailbox.MailBoxId);
                }

                if (isMailboxDeactivated)
                {
                    if (client != null)
                        client.Cancel();

                    log.Info("MailBox with id={0} is deactivated.\r\n", mailbox.MailBoxId);
                }

            }
            catch (Exception ex)
            {
                log.Error("[ClientOnGetMessage] Exception:\r\n{0}\r\n", ex.ToString());
            }
        }

        private void DoOptionalOperations(MailMessage message, MimeMessage mimeMessage, MailBox mailbox, int[] tagIds, ILogger log)
        {
            var manager = new MailBoxManager(log);

            CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);

            SecurityContext.AuthenticateMe(new Guid(mailbox.UserId));

            try
            {
                if (mailbox.Imap)
                {
                    if (tagIds != null) // Add new tags to existing messages
                    {
                        foreach (var tagId in tagIds)
                            manager.SetMessagesTag(mailbox.TenantId, mailbox.UserId, tagId, new[] {(int) message.Id});
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("SetMessagesTag(tenant={0}, userId='{1}', messageId={2}, tagid = {3}) Exception:\r\n{4}\r\n",
                    mailbox.TenantId, mailbox.UserId, message.Id, e.ToString(), tagIds != null ? string.Join(",", tagIds) : "null");
            }

            manager.AddRelationshipEventForLinkedAccounts(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            manager.SaveEmailInData(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            manager.SendAutoreply(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            manager.UploadIcsToCalendar(
                mailbox, 
                message.CalendarId,
                message.CalendarUid,
                message.CalendarEventIcs,
                message.CalendarEventCharset,
                message.CalendarEventMimeType,
                mailbox.EMail.Address, 
                _tasksConfig.DefaultApiSchema, log);

            if (_tasksConfig.SaveOriginalMessage)
                StoreMailEml(mailbox.TenantId, mailbox.UserId, message.StreamId, mimeMessage, log);

            if (!_tasksConfig.EnableSignalr)
                return;

            var now = DateTime.UtcNow;

            if (mailbox.LastSignalrNotify.HasValue &&
                !((now - mailbox.LastSignalrNotify.Value).TotalSeconds > SIGNALR_WAIT_SECONDS))
            {
                mailbox.LastSignalrNotifySkipped = true;
                return;
            }

            NotifySignalr(mailbox, log);

            mailbox.LastSignalrNotify = now;
            mailbox.LastSignalrNotifySkipped = false;
        }

        public string StoreMailEml(int tenant, string user, string streamId, MimeMessage message, ILogger logger)
        {
            if (message == null)
                return string.Empty;

            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var savePath = MailStoragePathCombiner.GetEmlKey(user, streamId);
            var storage = MailDataStore.GetDataStore(tenant);

            try
            {
                using (var stream = new MemoryStream())
                {
                    message.WriteTo(stream);

                    var res = storage
                        .UploadWithoutQuota(string.Empty, savePath, stream, "message/rfc822", string.Empty)
                        .ToString();

                    logger.Debug("StoreMailEml() tenant='{0}', user_id='{1}', save_eml_path='{2}' Result: {3}",
                        tenant, user, savePath, res);

                    return res;
                }
            }
            catch (Exception ex)
            {
                logger.Error("StoreMailEml Exception: {0}", ex.ToString());
            }

            return string.Empty;
        }

        private bool TryStoreMailData(MailMessage message, MailBox mailbox, ILogger log)
        {
            var manager = new MailBoxManager(log);

            try
            {
                if (message.Attachments.Any())
                {
                    log.Debug("StoreAttachments()");
                    var index = 0;
                    message.Attachments.ForEach(att =>
                    {
                        att.fileNumber = ++index;
                        att.mailboxId = mailbox.MailBoxId;
                    });
                    manager.StoreAttachments(mailbox.TenantId, mailbox.UserId, message.Attachments,
                        message.StreamId);

                    log.Debug("MailMessage.ReplaceEmbeddedImages()");
                    message.ReplaceEmbeddedImages(_log);
                }

                log.Debug("StoreMailBody()");
                manager.StoreMailBody(mailbox.TenantId, mailbox.UserId, message);
            }
            catch (Exception ex)
            {
                log.Error("TryStoreMailData(Account:{0}): Exception:\r\n{1}\r\n",
                        mailbox.EMail, ex.ToString());

                //Trying to delete all attachments and mailbody
                if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
                {
                    log.Info("Problem with mail proccessing(Account:{0}). Body and attachment was deleted",
                        mailbox.EMail);
                }

                return false;
            }

            return true;
        }

        private void FreeTask(Task<MailBox> task, ICollection<Task<MailBox>> tasks)
        {
            _log.Debug("End Task {0} with status = '{1}'.", task.Id, task.Status);

            if (!tasks.Remove(task))
                _log.Error("Task not exists in tasks array.");

            var mailbox = task.Result;

            if (mailbox != null)
            {
                if(mailbox.LastSignalrNotifySkipped)
                    NotifySignalr(mailbox, _log);

                ReleaseMailbox(mailbox);
            }

            task.Dispose();
        }

        private void ReleaseMailbox(MailBox mailbox)
        {
            _queueManager.ReleaseMailbox(mailbox);
        }

        #endregion
    }
}