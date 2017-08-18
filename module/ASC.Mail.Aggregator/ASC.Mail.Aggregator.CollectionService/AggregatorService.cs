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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ASC.Core;
using ASC.Mail.Aggregator.CollectionService.Queue;
using ASC.Mail.Aggregator.CollectionService.Queue.Data;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Clients;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using MailKit.Security;
using ASC.Mail.Aggregator.Common.DataStorage;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;
using log4net;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using MySql.Data.MySqlClient;

namespace ASC.Mail.Aggregator.CollectionService
{
    public sealed class AggregatorService: ServiceBase
    {
        public const string ASC_MAIL_COLLECTION_SERVICE_NAME = "ASC Mail Collection Service";

        private const string S_FAIL = "error";
        private const string S_OK = "success";

        private const string PROCESS_MESSAGE = "process message";
        private const string PROCESS_MAILBOX = "process mailbox";
        private const string CONNECT_MAILBOX = "connect mailbox";

        private readonly ILogger _log;
        private readonly ILog _logStat;
        private readonly CancellationTokenSource _cancelTokenSource;
        readonly ManualResetEvent _resetEvent;
        private Timer _workTimer;
        private readonly TasksConfig _tasksConfig;
        private readonly QueueManager _queueManager;
        readonly TaskFactory _taskFactory;
        private readonly TimeSpan _tsTaskStateCheckInterval;
        private bool _isFirstTime = true;
        private static SignalrWorker _signalrWorker;
        private const int SIGNALR_WAIT_SECONDS = 30;
        private readonly TimeSpan _taskSecondsLifetime;

        public AggregatorService(Options options)
        {
            ServiceName = ASC_MAIL_COLLECTION_SERVICE_NAME;
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

                _logStat = LogManager.GetLogger("ASC.MAIL.STAT");

                _tasksConfig = TasksConfig.FromConfig;

                _tasksConfig.DefaultFolders = MailQueueItemSettings.DefaultFolders;

                _tasksConfig.ImapFlags = MailQueueItemSettings.ImapFlags;

                _tasksConfig.PopUnorderedDomains = MailQueueItemSettings.PopUnorderedDomains;

                _tasksConfig.SkipImapFlags = MailQueueItemSettings.SkipImapFlags;

                _tasksConfig.SpecialDomainFolders = MailQueueItemSettings.SpecialDomainFolders;

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
                    _signalrWorker = new SignalrWorker();

                _workTimer = new Timer(workTimer_Elapsed, _cancelTokenSource.Token, Timeout.Infinite, Timeout.Infinite);

                _log.Info("Service is ready.");
            }
            catch (Exception ex)
            {
                _log.Fatal("CollectorService error under construct: {0}", ex.ToString());
            }
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
            try
            {
                _log.Info("Stoping service\r\n");

                if (_cancelTokenSource != null)
                    _cancelTokenSource.Cancel();

                if (_queueManager != null)
                    _queueManager.CancelHandler.WaitOne();

                StopTimer();

                if (_workTimer != null)
                {
                    _workTimer.Dispose();
                    _workTimer = null;
                }

                if (_resetEvent != null)
                    _resetEvent.Set();

                if (_queueManager != null)
                    _queueManager.Dispose();

                if (_signalrWorker != null)
                    _signalrWorker.Dispose();
            }
            catch (Exception ex)
            {
                _log.Error("Stop service Error: {0}\r\n", ex.ToString());
            }
            finally
            {
                base.OnStop();
            }

            _log.Info("Stop service\r\n");
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
                    _queueManager.LoadMailboxesFromDump();

                    if (_queueManager.ProcessingCount > 0)
                    {
                        _log.Info("Found {0} tasks to release", _queueManager.ProcessingCount);

                        _queueManager.ReleaseAllProcessingMailboxes();
                    }

                    _queueManager.LoadTenantsFromDump();

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

                // ***Add a loop to process the tasks one at a time until none remain. 
                while (tasks.Any())
                {
                    // Identify the first task that completes.
                    var indexTask = Task.WaitAny(tasks.Select(t => t.Task).ToArray(), (int)_tsTaskStateCheckInterval.TotalMilliseconds, cancelToken);
                    if (indexTask > -1)
                    {
                        // ***Remove the selected task from the list so that you don't 
                        // process it more than once.
                        var outTask = tasks[indexTask];

                        FreeTask(outTask, tasks);
                    }
                    else
                    {
                        _log.Info("Task.WaitAny timeout. Tasks count = {0}\r\nTasks:\r\n{1}", tasks.Count,
                            string.Join("\r\n",
                                tasks.Select(
                                    t =>
                                        string.Format("Id: {0} Status: {1}, MailboxId: {2} Address: '{3}'",
                                            t.Task.Id, t.Task.Status, t.Mailbox.MailBoxId, t.Mailbox.EMail))));
                    }

                    var tasks2Free =
                        tasks.Where(
                            t =>
                            t.Task.Status == TaskStatus.Canceled || t.Task.Status == TaskStatus.Faulted ||
                            t.Task.Status == TaskStatus.RanToCompletion).ToList();

                    if (tasks2Free.Any())
                    {
                        _log.Info("Need free next tasks = {0}: ({1})", tasks2Free.Count,
                                  string.Join(",",
                                              tasks2Free.Select(t => t.Task.Id.ToString(CultureInfo.InvariantCulture))));

                        tasks2Free.ForEach(task => FreeTask(task, tasks));
                    }

                    var difference = _tasksConfig.MaxTasksAtOnce - tasks.Count;

                    if (difference <= 0) continue;

                    var newTasks = CreateTasks(difference, cancelToken);

                    tasks.AddRange(newTasks);

                    _log.Info("Total tasks count = {0} ({1}).", tasks.Count,
                              string.Join(",", tasks.Select(t => t.Task.Id)));
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

                    _queueManager.CancelHandler.Set();

                    return;
                }

                _log.Error("Timer->workTimer_Elapsed. Exception:\r\n{0}\r\n", ex.ToString());

                if (_queueManager.ProcessingCount != 0)
                {
                    _queueManager.ReleaseAllProcessingMailboxes();
                }
            }

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

        private void NotifySignalrIfNeed(MailBox mailbox, ILogger log)
        {
            if (!_tasksConfig.EnableSignalr)
                return;

            try
            {
                if (_signalrWorker != null)
                    _signalrWorker.AddMailbox(mailbox);
            }
            catch (Exception ex)
            {
                log.Error("_signalrWorker.AddMailbox(UserId = {0} TenantId = {1}) Exception: {2}", mailbox.UserId,
                mailbox.TenantId, ex.ToString());
            }
        }

        private List<TaskData> CreateTasks(int needCount, CancellationToken cancelToken)
        {
            _log.Info("CreateTasks(need {0} tasks).", needCount);

            var mailboxes = _queueManager.GetLockedMailboxes(needCount);

            var tasks = new List<TaskData>();

            foreach (var mailbox in mailboxes)
            {
                var timeoutCancel = new CancellationTokenSource(_taskSecondsLifetime);

                var commonCancelToken =
                    CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutCancel.Token).Token;

                var taskLogger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, string.Format("Mbox_{0}", mailbox.MailBoxId));

                var client = CreateMailClient(mailbox, taskLogger, commonCancelToken);

                if (client == null)
                {
                    taskLogger.Info("ReleaseMailbox(Tenant = {0} MailboxId = {1}, Address = '{2}')",
                               mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);
                    ReleaseMailbox(mailbox);
                    continue;
                }

                var task = _taskFactory.StartNew(() => ProcessMailbox(client, _tasksConfig),
                    commonCancelToken);

                tasks.Add(new TaskData(mailbox, task));
            }

            if (tasks.Any())
                _log.Info("Created {0} tasks.", tasks.Count);
            else
                _log.Info("No more mailboxes for processing.");

            return tasks;
        }

        private MailClient CreateMailClient(MailBox mailbox, ILogger log, CancellationToken cancelToken)
        {
            MailClient client = null;

            var connectError = false;
            var stopClient = false;

            Stopwatch watch = null;

            if (_tasksConfig.CollectStatistics)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            var manager = new MailBoxManager(log)
            {
                AuthErrorWarningTimeout = _tasksConfig.AuthErrorWarningTimeout,
                AuthErrorDisableTimeout = _tasksConfig.AuthErrorDisableMailboxTimeout
            };

            try
            {
                client = new MailClient(mailbox, cancelToken, _tasksConfig.TcpTimeout,
                    _tasksConfig.SslCertificateErrorsPermit, _tasksConfig.ProtocolLogPath, log, true);

                log.Debug("MailClient.LoginImapPop(Tenant = {0}, MailboxId = {1} Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);

                client.Authenticated += ClientOnAuthenticated;

                client.LoginImapPop();
            }
            catch (System.TimeoutException exTimeout)
            {
                log.Warn(
                    "[TIMEOUT] CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exTimeout.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (OperationCanceledException)
            {
                log.Info(
                    "[CANCEL] CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);
                stopClient = true;
            }
            catch (AuthenticationException authEx)
            {
                log.Error(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, authEx.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (WebException webEx)
            {
                log.Error(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, webEx.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (Exception ex)
            {
                log.Error(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail,
                    ex is ImapProtocolException || ex is Pop3ProtocolException ? ex.Message : ex.ToString());

                stopClient = true;
            }
            finally
            {
                if (connectError)
                {
                    SetMailboxAuthError(manager, mailbox, log);
                }

                if (stopClient)
                {
                    CloseMailClient(client, mailbox, log);
                }

                if (_tasksConfig.CollectStatistics && watch != null)
                {
                    watch.Stop();

                    LogStat(CONNECT_MAILBOX, mailbox, watch.Elapsed, connectError);
                }
            }

            return client;
        }

        private static void SetMailboxAuthError(MailBoxManager manager, MailBox mailbox, ILogger log)
        {
            try
            {
                if (!mailbox.AuthErrorDate.HasValue)
                    manager.SetMailboxAuthError(mailbox, true);
            }
            catch (Exception ex)
            {
                log.Error(
                    "CreateTasks->SetMailboxAuthError(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, ex.Message);
            }
        }

        private void CloseMailClient(MailClient client, MailBox mailbox, ILogger log)
        {
            if (client == null)
                return;

            try
            {
                client.Authenticated -= ClientOnAuthenticated;
                client.GetMessage -= ClientOnGetMessage;

                client.Cancel();
                client.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(
                    "CloseMailClient(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, ex.Message);
            }
        }

        private void ProcessMailbox(MailClient client, TasksConfig tasksConfig)
        {
            var mailbox = client.Account;

            Stopwatch watch = null;

            if (_tasksConfig.CollectStatistics)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            var failed = false;

            var taskLogger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net,
                string.Format("Mbox_{0} Task_{1}", mailbox.MailBoxId, Task.CurrentId));

            taskLogger.Info(
                "ProcessMailbox(Tenant = {0}, MailboxId = {1} Address = '{2}') Is {3}",
                mailbox.TenantId, mailbox.MailBoxId,
                mailbox.EMail, mailbox.Active ? "Active" : "Inactive");

            try
            {
                client.Log = taskLogger;

                client.GetMessage += ClientOnGetMessage;

                client.Aggregate(tasksConfig, tasksConfig.MaxMessagesPerSession);
            }
            catch (OperationCanceledException)
            {
                taskLogger.Info(
                    "[CANCEL] ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);

                NotifySignalrIfNeed(mailbox, taskLogger);
            }
            catch (Exception ex)
            {
                taskLogger.Error(
                    "ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail,
                    ex is ImapProtocolException || ex is Pop3ProtocolException ? ex.Message : ex.ToString());

                failed = true;
            }
            finally
            {
                CloseMailClient(client, mailbox, _log);

                if (_tasksConfig.CollectStatistics && watch != null)
                {
                    watch.Stop();

                    LogStat(PROCESS_MAILBOX, mailbox, watch.Elapsed, failed);
                }
            }

            var manager = new MailBoxManager(taskLogger);

            var state = GetMailboxState(manager, mailbox, taskLogger);

            switch (state)
            {
                case MailboxState.NoChanges:
                    taskLogger.Info("MailBox with id={0} not changed.", mailbox.MailBoxId);
                    break;
                case MailboxState.Disabled:
                    taskLogger.Info("MailBox with id={0} is deactivated.", mailbox.MailBoxId);
                    break;
                case MailboxState.Deleted:
                    taskLogger.Info("MailBox with id={0} is removed.", mailbox.MailBoxId);

                    try
                    {
                        taskLogger.Info("RemoveMailBox(id={0}) >> Try clear new data from removed mailbox", mailbox.MailBoxId);
                        manager.RemoveMailBox(mailbox);
                    }
                    catch (Exception exRem)
                    {
                        taskLogger.Info(
                            "[REMOVE] ProcessMailbox->RemoveMailBox(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                            mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exRem.Message);
                    }
                    break;
                case MailboxState.DateChanged:
                    taskLogger.Info("MailBox with id={0}: beginDate was changed.", mailbox.MailBoxId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            taskLogger.Info("Mailbox '{0}' has been processed.", mailbox.EMail);
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

            Stopwatch watch = null;

            if (_tasksConfig.CollectStatistics)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            var failed = false;

            var mailbox = mailClientMessageEventArgs.Mailbox;

            try
            {
                var mimeMessage = mailClientMessageEventArgs.Message;
                var uid = mailClientMessageEventArgs.MessageUid;
                var folder = mailClientMessageEventArgs.Folder;
                var unread = mailClientMessageEventArgs.Unread;
                var fromEmail = mimeMessage.From.Mailboxes.FirstOrDefault();
                log = mailClientMessageEventArgs.Logger;

                log.Debug("ClientOnGetMessage MailboxId = {0}, Address = '{1}'",
                    mailbox.MailBoxId, mailbox.EMail);

                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                SecurityContext.AuthenticateMe(new Guid(mailbox.UserId));

                var manager = new MailBoxManager(log);

                var md5 =
                    string.Format("{0}|{1}|{2}|{3}",
                        mimeMessage.From.Mailboxes.Any() ? mimeMessage.From.Mailboxes.First().Address : "",
                        mimeMessage.Subject, mimeMessage.Date.UtcDateTime, mimeMessage.MessageId).GetMd5();

                var uidl = mailbox.Imap ? string.Format("{0}-{1}", uid, folder.FolderId) : uid;

                var fromThisMailBox = fromEmail != null &&
                                      fromEmail.Address.ToLowerInvariant()
                                          .Equals(mailbox.EMail.Address.ToLowerInvariant());

                var toThisMailBox =
                    mimeMessage.To.Mailboxes.Select(addr => addr.Address.ToLowerInvariant())
                        .Contains(mailbox.EMail.Address.ToLowerInvariant());

                log.Info(
                    @"Message: Subject: '{1}' Date: {2} Unread: {5} FolderId: {3} ('{4}') MimeId: '{0}' Uidl: '{6}' Md5: '{7}' To->From: {8} From->To: {9}",
                    mimeMessage.MessageId, mimeMessage.Subject, mimeMessage.Date, folder.FolderId, folder.Name, unread,
                    uidl, md5, fromThisMailBox, toThisMailBox);

                List<int> tagsIds = null;

                if (folder.Tags.Any())
                {
                    log.Debug("GetOrCreateTags()");

                    tagsIds = manager.GetOrCreateTags(mailbox.TenantId, mailbox.UserId, folder.Tags);
                }

                log.Debug("SearchExistingMessagesAndUpdateIfNeeded()");

                var found = manager.SearchExistingMessagesAndUpdateIfNeeded(mailbox, folder.FolderId, uidl, md5,
                    mimeMessage.MessageId, fromThisMailBox, toThisMailBox, tagsIds);

                var needSave = !found;
                if (!needSave)
                    return;

                log.Debug("DetectChainId()");

                var chainId = manager.DetectChainId(mailbox, mimeMessage.MessageId, mimeMessage.InReplyTo,
                    mimeMessage.Subject);

                var streamId = MailUtil.CreateStreamId();

                log.Debug("Convert MimeMessage->MailMessage");

                var message = ConvertToMailMessage(mimeMessage, folder, unread, chainId, streamId, log);

                log.Debug("TryStoreMailData()");

                if (!TryStoreMailData(message, mailbox, log))
                {
                    failed = true;
                    return;
                }

                log.Debug("MailSave()");

                if (TrySaveMail(manager, mailbox, message, folder, uidl, md5, log))
                {
                    log.Debug("DoOptionalOperations->START");

                    DoOptionalOperations(message, mimeMessage, mailbox, tagsIds, log);
                }
                else
                {
                    failed = true;

                    if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
                    {
                        log.Info("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
                    }
                    else
                    {
                        throw new Exception("Can't delete mail folder with data");
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error("[ClientOnGetMessage] Exception:\r\n{0}\r\n", ex.ToString());

                failed = true;
            }
            finally
            {
                if (_tasksConfig.CollectStatistics && watch != null)
                {
                    watch.Stop();

                    LogStat(PROCESS_MESSAGE, mailbox, watch.Elapsed, failed);
                }
            }
        }

        private static MailMessage ConvertToMailMessage(MimeMessage mimeMessage, MailFolder folder, bool unread, string chainId, string streamId, ILogger log)
        {
            MailMessage message;

            try
            {
                message = mimeMessage.CreateMailMessage(folder.FolderId, unread, chainId, streamId, log);
            }
            catch (Exception ex)
            {
                log.Error("Convert MimeMessage->MailMessage: Exception: {0}", ex.ToString());

                log.Debug("Creating fake message with original MimeMessage in attachments");

                message = mimeMessage.CreateCorruptedMesage(folder.FolderId, unread, chainId, streamId);
            }

            return message;
        }

        private static bool TrySaveMail(MailBoxManager manager, MailBox mailbox, MailMessage message, MailFolder folder,  string uidl, string md5, ILogger log)
        {
            try
            {
                var folderRestoreId = folder.FolderId == MailFolder.Ids.spam ? MailFolder.Ids.inbox : folder.FolderId;

                var attempt = 1;

                while (attempt < 3)
                {
                    try
                    {
                        message.Id = manager.MailSave(mailbox, message, 0, folder.FolderId, folderRestoreId,
                            uidl, md5, true);

                        break;
                    }
                    catch (MySqlException exSql)
                    {
                        if (!exSql.Message.StartsWith("Deadlock found"))
                            throw;

                        if (attempt > 2)
                            throw;

                        log.Warn("[DEADLOCK] MailSave() try again (attempt {0}/2)", attempt);

                        attempt++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("TrySaveMail Exception:\r\n{0}\r\n", ex.ToString());
            }

            return false;
        }

        enum MailboxState
        {
            NoChanges,
            Disabled,
            Deleted,
            DateChanged
        }

        private MailboxState GetMailboxState(MailBoxManager manager, MailBox mailbox, ILogger log)
        {
            try
            {
                bool isMailboxRemoved;
                bool isMailboxDeactivated;
                DateTime beginDate;

                log.Debug("GetMailBoxState()");

                manager.GetMailBoxState(mailbox.MailBoxId, out isMailboxRemoved, out isMailboxDeactivated, out beginDate);

                if (mailbox.BeginDate != beginDate)
                {
                    mailbox.BeginDateChanged = true;
                    mailbox.BeginDate = beginDate;

                    return MailboxState.DateChanged;
                }

                if (isMailboxRemoved)
                    return MailboxState.Deleted;

                if (isMailboxDeactivated)
                    return MailboxState.Disabled;
            }
            catch (Exception exGetMbInfo)
            {
                log.Info("GetMailBoxState(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exGetMbInfo.Message);
            }

            return MailboxState.NoChanges;
        }

        private readonly ConcurrentDictionary<string, bool> _userCrmAvailabeDictionary = new ConcurrentDictionary<string, bool>();
        private readonly object _locker = new object();

        private bool IsCrmAvailable(MailBox mailbox, ILogger log)
        {
            bool crmAvailable;

            lock (_locker)
            {
                if (_userCrmAvailabeDictionary.TryGetValue(mailbox.UserId, out crmAvailable))
                    return crmAvailable;

                crmAvailable = mailbox.IsCrmAvailable(_tasksConfig.DefaultApiSchema, log);
                _userCrmAvailabeDictionary.GetOrAdd(mailbox.UserId, crmAvailable);
            }

            return crmAvailable;
        }

        private void SetMessageTags(MailBoxManager manager, MailMessage message, MailBox mailbox, List<int> tagIds,
            ILogger log)
        {
            var messageId = (int) message.Id;

            if (tagIds == null)
                tagIds = new List<int>();

            if (IsCrmAvailable(mailbox, log))
            {
                var crmTagIds = manager.GetCrmTags(message.FromEmail, mailbox.TenantId, mailbox.UserId);

                if (crmTagIds.Any())
                {
                    tagIds.AddRange(crmTagIds);
                }
            }

            if (!tagIds.Any())
                return;

            foreach (var tagId in tagIds)
            {
                try
                {
                    manager.SetMessagesTag(mailbox.TenantId, mailbox.UserId, tagId, new[] {messageId});
                }
                catch (Exception e)
                {
                    log.Error(
                        "SetMessagesTag(tenant={0}, userId='{1}', messageId={2}, tagid = {3}) Exception:\r\n{4}\r\n",
                        mailbox.TenantId, mailbox.UserId, messageId, e.ToString(),
                        tagIds != null ? string.Join(",", tagIds) : "null");
                }
            }
        }

        private void DoOptionalOperations(MailMessage message, MimeMessage mimeMessage, MailBox mailbox, List<int> tagIds,
            ILogger log)
        {
            var manager = new MailBoxManager(log);

            log.Debug("DoOptionalOperations->SetMessageTags()");

            SetMessageTags(manager, message, mailbox, tagIds, log);

            log.Debug("DoOptionalOperations->AddRelationshipEventForLinkedAccounts()");

            manager.AddRelationshipEventForLinkedAccounts(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            log.Debug("DoOptionalOperations->SaveEmailInData()");

            manager.SaveEmailInData(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            log.Debug("DoOptionalOperations->SendAutoreply()");

            manager.SendAutoreply(mailbox, message, _tasksConfig.DefaultApiSchema, log);

            log.Debug("DoOptionalOperations->UploadIcsToCalendar()");

            manager.UploadIcsToCalendar(mailbox, message.CalendarId, message.CalendarUid, message.CalendarEventIcs,
                message.CalendarEventCharset, message.CalendarEventMimeType, mailbox.EMail.Address,
                _tasksConfig.DefaultApiSchema, log);

            if (_tasksConfig.SaveOriginalMessage)
            {
                log.Debug("DoOptionalOperations->StoreMailEml()");
                StoreMailEml(mailbox.TenantId, mailbox.UserId, message.StreamId, mimeMessage, log);
            }

            if (!_tasksConfig.EnableSignalr)
            {
                log.Debug("DoOptionalOperations->END");
                return;
            }

            var now = DateTime.UtcNow;

            if (mailbox.LastSignalrNotify.HasValue &&
                !((now - mailbox.LastSignalrNotify.Value).TotalSeconds > SIGNALR_WAIT_SECONDS))
            {
                mailbox.LastSignalrNotifySkipped = true;
                return;
            }

            log.Debug("DoOptionalOperations->NotifySignalrIfNeed()");

            NotifySignalrIfNeed(mailbox, log);

            mailbox.LastSignalrNotify = now;
            mailbox.LastSignalrNotifySkipped = false;

            log.Debug("DoOptionalOperations->END");
        }

        public string StoreMailEml(int tenant, string user, string streamId, MimeMessage message, ILogger log)
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

                    var res = storage.Save(savePath, stream, MailStoragePathCombiner.EML_FILE_NAME).ToString();

                    log.Debug("StoreMailEml() tenant='{0}', user_id='{1}', save_eml_path='{2}' Result: {3}", tenant, user, savePath, res);

                    return res;
                }
            }
            catch (Exception ex)
            {
                log.Error("StoreMailEml Exception: {0}", ex.ToString());
            }

            return string.Empty;
        }

        private static bool TryStoreMailData(MailMessage message, MailBox mailbox, ILogger log)
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
                    manager.StoreAttachments(mailbox, message.Attachments, message.StreamId);

                    log.Debug("MailMessage.ReplaceEmbeddedImages()");
                    message.ReplaceEmbeddedImages(log);
                }

                log.Debug("StoreMailBody()");
                manager.StoreMailBody(mailbox, message);
            }
            catch (Exception ex)
            {
                log.Error("TryStoreMailData(Account:{0}): Exception:\r\n{1}\r\n", mailbox.EMail, ex.ToString());

                //Trying to delete all attachments and mailbody
                if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
                {
                    log.Info("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
                }

                return false;
            }

            return true;
        }

        private void FreeTask(TaskData taskData, ICollection<TaskData> tasks)
        {
            try
            {
                _log.Debug("End Task {0} with status = '{1}'.", taskData.Task.Id, taskData.Task.Status);

                if (!tasks.Remove(taskData))
                    _log.Error("Task not exists in tasks array.");

                var mailbox = taskData.Mailbox;

                ReleaseMailbox(mailbox);

                taskData.Task.Dispose();
            }
            catch (Exception ex)
            {
                _log.Error("FreeTask(id:'{0}', email:'{1}'): Exception:\r\n{2}\r\n", taskData.Mailbox.MailBoxId, taskData.Mailbox.EMail, ex.ToString());
            }
        }

        private void ReleaseMailbox(MailBox mailbox)
        {
            if (mailbox == null)
                return;

            if (mailbox.LastSignalrNotifySkipped)
                NotifySignalrIfNeed(mailbox, _log);

            _queueManager.ReleaseMailbox(mailbox);
        }

        private void LogStat(string method, MailBox mailBox, TimeSpan duration, bool failed)
        {
            if(!_tasksConfig.CollectStatistics)
                return;

            ThreadContext.Properties["duration"] = duration.TotalMilliseconds;
            ThreadContext.Properties["mailboxId"] = mailBox.MailBoxId;
            ThreadContext.Properties["address"] = mailBox.EMail.ToString();
            ThreadContext.Properties["status"] = failed ? S_FAIL : S_OK;
            _logStat.Debug(method);
        }

        #endregion
    }
}