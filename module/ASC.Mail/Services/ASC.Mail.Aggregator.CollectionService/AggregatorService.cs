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
using ASC.Mail.Clients;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using MailKit.Security;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using ILog = ASC.Common.Logging.ILog;
using LogManager = ASC.Common.Logging.LogManager;

namespace ASC.Mail.Aggregator.CollectionService
{
    public sealed class AggregatorService : ServiceBase
    {
        public const string ASC_MAIL_COLLECTION_SERVICE_NAME = "ASC Mail Collection Service";

        private const string S_FAIL = "error";
        private const string S_OK = "success";

        private const string PROCESS_MESSAGE = "process message";
        private const string PROCESS_MAILBOX = "process mailbox";
        private const string CONNECT_MAILBOX = "connect mailbox";

        private readonly ILog _log;
        private readonly ILog _logStat;
        private readonly CancellationTokenSource _cancelTokenSource;
        private ManualResetEvent _resetEvent;
        private Timer _workTimer;
        private readonly TasksConfig _tasksConfig;
        private readonly QueueManager _queueManager;
        readonly TaskFactory _taskFactory;
        private readonly TimeSpan _tsTaskStateCheckInterval;
        private bool _isFirstTime = true;
        private static SignalrWorker _signalrWorker;
        private const int SIGNALR_WAIT_SECONDS = 30;
        private readonly TimeSpan _taskSecondsLifetime;

        public ConcurrentDictionary<string, List<MailSieveFilterData>> Filters { get; set; }

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
                _log = LogManager.GetLogger("ASC.Mail.MainThread");

                _logStat = LogManager.GetLogger("ASC.Mail.Stat");

                _tasksConfig = TasksConfig.FromConfig;

                var mailSettings = new MailQueueItemSettings();

                _tasksConfig.DefaultFolders = mailSettings.DefaultFolders;

                _tasksConfig.ImapFlags = mailSettings.ImapFlags;

                _tasksConfig.SkipImapFlags = mailSettings.SkipImapFlags;

                _tasksConfig.SpecialDomainFolders = mailSettings.SpecialDomainFolders;

                if (options.OnlyUsers != null)
                    _tasksConfig.WorkOnUsersOnly.AddRange(options.OnlyUsers.ToList());

                if (options.NoMessagesLimit)
                    _tasksConfig.MaxMessagesPerSession = -1;

                _taskSecondsLifetime =
                    TimeSpan.FromSeconds(ConfigurationManagerExtension.AppSettings["mail.task-process-lifetime-seconds"] != null
                        ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.task-process-lifetime-seconds"])
                        : 300);

                _queueManager = new QueueManager(_tasksConfig, _log);

                _cancelTokenSource = new CancellationTokenSource();

                _taskFactory = new TaskFactory();

                _tsTaskStateCheckInterval = ConfigurationManagerExtension.AppSettings["mail.task-check-state-seconds"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.task-check-state-seconds"]))
                        : TimeSpan.FromSeconds(30);

                if (_tasksConfig.EnableSignalr)
                    _signalrWorker = new SignalrWorker();

                _workTimer = new Timer(workTimer_Elapsed, _cancelTokenSource.Token, Timeout.Infinite, Timeout.Infinite);

                Filters = new ConcurrentDictionary<string, List<MailSieveFilterData>>();

                _log.Info("Service is ready.");
            }
            catch (Exception ex)
            {
                _log.FatalFormat("CollectorService error under construct: {0}", ex.ToString());

                throw ex;
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

                if (_queueManager != null)
                    _queueManager.Dispose();

                if (_signalrWorker != null)
                    _signalrWorker.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Stop service Error: {0}\r\n", ex.ToString());
            }
            finally
            {
                _log.Info("Stop service\r\n");

                base.OnStop();

                if (_resetEvent != null)
                    _resetEvent.Set();
            }
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
            _log.InfoFormat("Service power event detected = {0}.", powerStatus.ToString());

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
            _resetEvent = new ManualResetEvent(false);
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
                        _log.InfoFormat("Found {0} tasks to release", _queueManager.ProcessingCount);

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
                        _log.InfoFormat("Task.WaitAny timeout. Tasks count = {0}\r\nTasks:\r\n{1}", tasks.Count,
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
                        _log.InfoFormat("Need free next tasks = {0}: ({1})", tasks2Free.Count,
                                  string.Join(",",
                                              tasks2Free.Select(t => t.Task.Id.ToString(CultureInfo.InvariantCulture))));

                        tasks2Free.ForEach(task => FreeTask(task, tasks));
                    }

                    var difference = _tasksConfig.MaxTasksAtOnce - tasks.Count;

                    if (difference <= 0) continue;

                    var newTasks = CreateTasks(difference, cancelToken);

                    tasks.AddRange(newTasks);

                    _log.InfoFormat("Total tasks count = {0} ({1}).", tasks.Count,
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

                _log.ErrorFormat("Timer->workTimer_Elapsed. Exception:\r\n{0}\r\n", ex.ToString());

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

            _log.DebugFormat("Setup _workTimer to {0} seconds", _tasksConfig.CheckTimerInterval.TotalSeconds);
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

        private void NotifySignalrIfNeed(MailBoxData mailbox, ILog log)
        {
            if (!_tasksConfig.EnableSignalr)
            {
                log.Debug("Skip NotifySignalrIfNeed: EnableSignalr == false");

                return;
            }

            var now = DateTime.UtcNow;

            try
            {
                if (mailbox.LastSignalrNotify.HasValue &&
                    !((now - mailbox.LastSignalrNotify.Value).TotalSeconds > SIGNALR_WAIT_SECONDS))
                {
                    mailbox.LastSignalrNotifySkipped = true;

                    log.InfoFormat(
                        "Skip NotifySignalrIfNeed: last notification has occurend less then {0} seconds ago",
                        SIGNALR_WAIT_SECONDS);

                    return;
                }

                if (_signalrWorker == null)
                    throw new NullReferenceException("_signalrWorker");

                _signalrWorker.AddMailbox(mailbox);

                log.InfoFormat("NotifySignalrIfNeed(UserId = {0} TenantId = {1}) has been succeeded",
                    mailbox.UserId, mailbox.TenantId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("NotifySignalrIfNeed(UserId = {0} TenantId = {1}) Exception: {2}", mailbox.UserId,
                    mailbox.TenantId, ex.ToString());
            }

            mailbox.LastSignalrNotify = now;
            mailbox.LastSignalrNotifySkipped = false;
        }

        private List<TaskData> CreateTasks(int needCount, CancellationToken cancelToken)
        {
            _log.InfoFormat("CreateTasks(need {0} tasks).", needCount);

            var mailboxes = _queueManager.GetLockedMailboxes(needCount);

            var tasks = new List<TaskData>();

            foreach (var mailbox in mailboxes)
            {
                var timeoutCancel = new CancellationTokenSource(_taskSecondsLifetime);

                var commonCancelToken =
                    CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutCancel.Token).Token;

                var taskLogger = LogManager.GetLogger(string.Format("Mbox_{0}", mailbox.MailBoxId));

                var client = CreateMailClient(mailbox, taskLogger, commonCancelToken);

                if (client == null)
                {
                    taskLogger.InfoFormat("ReleaseMailbox(Tenant = {0} MailboxId = {1}, Address = '{2}')",
                               mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);
                    ReleaseMailbox(mailbox);
                    continue;
                }

                var task = _taskFactory.StartNew(() => ProcessMailbox(client, _tasksConfig),
                    commonCancelToken);

                tasks.Add(new TaskData(mailbox, task));
            }

            if (tasks.Any())
                _log.InfoFormat("Created {0} tasks.", tasks.Count);
            else
                _log.Info("No more mailboxes for processing.");

            return tasks;
        }

        private MailClient CreateMailClient(MailBoxData mailbox, ILog log, CancellationToken cancelToken)
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

            try
            {
                client = new MailClient(mailbox, cancelToken, _tasksConfig.TcpTimeout,
                   mailbox.IsTeamlab || _tasksConfig.SslCertificateErrorsPermit, _tasksConfig.ProtocolLogPath, log, true);

                log.DebugFormat("MailClient.LoginImapPop(Tenant = {0}, MailboxId = {1} Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);

                if (!mailbox.Imap)
                {
                    client.FuncGetPop3NewMessagesIDs =
                        uidls => MessageEngine.GetPop3NewMessagesIDs(mailbox, uidls, _tasksConfig.ChunkOfPop3Uidl);
                }

                client.Authenticated += ClientOnAuthenticated;

                client.LoginImapPop();
            }
            catch (System.TimeoutException exTimeout)
            {
                log.WarnFormat(
                    "[TIMEOUT] CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exTimeout.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (OperationCanceledException)
            {
                log.InfoFormat(
                    "[CANCEL] CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);
                stopClient = true;
            }
            catch (AuthenticationException authEx)
            {
                log.ErrorFormat(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, authEx.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (WebException webEx)
            {
                log.ErrorFormat(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, webEx.ToString());

                connectError = true;
                stopClient = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "CreateTasks->client.LoginImapPop(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail,
                    ex is ImapProtocolException || ex is Pop3ProtocolException ? ex.Message : ex.ToString());

                stopClient = true;
            }
            finally
            {
                if (connectError)
                {
                    SetMailboxAuthError(mailbox, log);
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

        private static void SetMailboxAuthError(MailBoxData mailbox, ILog log)
        {
            try
            {
                if (mailbox.AuthErrorDate.HasValue)
                    return;

                mailbox.AuthErrorDate = DateTime.UtcNow;

                var engine = new EngineFactory(mailbox.TenantId);
                engine.MailboxEngine.SetMaiboxAuthError(mailbox.MailBoxId, mailbox.AuthErrorDate.Value);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "CreateTasks->SetMailboxAuthError(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, ex.Message);
            }
        }

        private void CloseMailClient(MailClient client, MailBoxData mailbox, ILog log)
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
                log.ErrorFormat(
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

            var taskLogger = LogManager.GetLogger(string.Format("ASC.Mail Mbox_{0} Task_{1}", mailbox.MailBoxId, Task.CurrentId));

            taskLogger.InfoFormat(
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
                taskLogger.InfoFormat(
                    "[CANCEL] ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail);

                NotifySignalrIfNeed(mailbox, taskLogger);
            }
            catch (Exception ex)
            {
                taskLogger.ErrorFormat(
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

            var state = GetMailboxState(mailbox, taskLogger);

            switch (state)
            {
                case MailboxState.NoChanges:
                    taskLogger.InfoFormat("MailBox with id={0} not changed.", mailbox.MailBoxId);
                    break;
                case MailboxState.Disabled:
                    taskLogger.InfoFormat("MailBox with id={0} is deactivated.", mailbox.MailBoxId);
                    break;
                case MailboxState.Deleted:
                    taskLogger.InfoFormat("MailBox with id={0} is removed.", mailbox.MailBoxId);

                    try
                    {
                        taskLogger.InfoFormat("RemoveMailBox(id={0}) >> Try clear new data from removed mailbox", mailbox.MailBoxId);

                        var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);
                        engine.MailboxEngine.RemoveMailBox(mailbox);
                    }
                    catch (Exception exRem)
                    {
                        taskLogger.InfoFormat(
                            "[REMOVE] ProcessMailbox->RemoveMailBox(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                            mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exRem.Message);
                    }
                    break;
                case MailboxState.DateChanged:
                    taskLogger.InfoFormat("MailBox with id={0}: beginDate was changed.", mailbox.MailBoxId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            taskLogger.InfoFormat("Mailbox '{0}' has been processed.", mailbox.EMail);
        }

        private static void ClientOnAuthenticated(object sender, MailClientEventArgs mailClientEventArgs)
        {
            if (!mailClientEventArgs.Mailbox.AuthErrorDate.HasValue)
                return;

            mailClientEventArgs.Mailbox.AuthErrorDate = null;

            var engine = new EngineFactory(mailClientEventArgs.Mailbox.TenantId);
            engine.MailboxEngine.SetMaiboxAuthError(mailClientEventArgs.Mailbox.MailBoxId, mailClientEventArgs.Mailbox.AuthErrorDate);
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
                log = mailClientMessageEventArgs.Logger;

                var uidl = mailbox.Imap ? string.Format("{0}-{1}", uid, (int)folder.Folder) : uid;

                log.InfoFormat("Found message (UIDL: '{0}', MailboxId = {1}, Address = '{2}')",
                    uidl, mailbox.MailBoxId, mailbox.EMail);

                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                SecurityContext.AuthenticateMe(new Guid(mailbox.UserId));

                var message = MessageEngine.Save(mailbox, mimeMessage, uidl, folder, null, unread, log);

                if (message == null || message.Id <= 0)
                {
                    return;
                }

                log.InfoFormat("Message saved (id: {0}, From: '{1}', Subject: '{2}', Unread: {3})", 
                    message.Id, message.From, message.Subject, message.IsNew);

                log.Info("DoOptionalOperations->START");

                DoOptionalOperations(message, mimeMessage, mailbox, folder, log);

                log.Info("DoOptionalOperations->END");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("[ClientOnGetMessage] Exception:\r\n{0}\r\n", ex.ToString());

                failed = true;

                throw ex;
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

        enum MailboxState
        {
            NoChanges,
            Disabled,
            Deleted,
            DateChanged
        }

        private static MailboxState GetMailboxState(MailBoxData mailbox, ILog log)
        {
            try
            {
                log.Debug("GetMailBoxState()");

                var engine = new EngineFactory(-1);
                var status = engine.MailboxEngine.GetMailboxStatus(new СoncreteUserMailboxExp(mailbox.MailBoxId, mailbox.TenantId, mailbox.UserId, null));

                if (mailbox.BeginDate != status.BeginDate)
                {
                    mailbox.BeginDateChanged = true;
                    mailbox.BeginDate = status.BeginDate;

                    return MailboxState.DateChanged;
                }

                if (status.IsRemoved)
                    return MailboxState.Deleted;

                if (!status.Enabled)
                    return MailboxState.Disabled;
            }
            catch (Exception exGetMbInfo)
            {
                log.InfoFormat("GetMailBoxState(Tenant = {0}, MailboxId = {1}, Address = '{2}') Exception: {3}",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, exGetMbInfo.Message);
            }

            return MailboxState.NoChanges;
        }

        private readonly ConcurrentDictionary<string, bool> _userCrmAvailabeDictionary = new ConcurrentDictionary<string, bool>();
        private readonly object _locker = new object();

        private bool IsCrmAvailable(MailBoxData mailbox, ILog log)
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

        private List<MailSieveFilterData> GetFilters(EngineFactory factory, ILog log)
        {
            var user = factory.UserId;

            if (string.IsNullOrEmpty(user))
                return new List<MailSieveFilterData>();

            try
            {
                if (Filters.ContainsKey(user)) return Filters[user];

                var filters = factory.FilterEngine.GetList();

                Filters.TryAdd(user, filters);

                return filters;
            }
            catch (Exception ex)
            {
                log.Error("GetFilters failed", ex);
            }

            return new List<MailSieveFilterData>();
        }

        private void DoOptionalOperations(MailMessageData message, MimeMessage mimeMessage, MailBoxData mailbox, MailFolder folder, ILog log)
        {
            try
            {
                var factory = new EngineFactory(mailbox.TenantId, mailbox.UserId, log);

                var tagIds = new List<int>();

                if (folder.Tags.Any())
                {
                    log.Debug("DoOptionalOperations->GetOrCreateTags()");

                    tagIds = factory.TagEngine.GetOrCreateTags(mailbox.TenantId, mailbox.UserId, folder.Tags);
                }

                log.Debug("DoOptionalOperations->IsCrmAvailable()");

                if (IsCrmAvailable(mailbox, log))
                {
                    log.Debug("DoOptionalOperations->GetCrmTags()");

                    var crmTagIds = factory.TagEngine.GetCrmTags(message.FromEmail);

                    if (crmTagIds.Any())
                    {
                        if (tagIds == null)
                            tagIds = new List<int>();

                        tagIds.AddRange(crmTagIds.Select(t => t.Id));
                    }
                }

                if (tagIds.Any())
                {
                    if (message.TagIds == null || !message.TagIds.Any())
                        message.TagIds = tagIds;
                    else
                        message.TagIds.AddRange(tagIds);

                    message.TagIds = message.TagIds.Distinct().ToList();
                }

                log.Debug("DoOptionalOperations->AddMessageToIndex()");

                var wrapper = message.ToMailWrapper(mailbox.TenantId, new Guid(mailbox.UserId));

                factory.IndexEngine.Add(wrapper);

                foreach (var tagId in tagIds)
                {
                    try
                    {
                        log.DebugFormat("DoOptionalOperations->SetMessagesTag(tagId: {0})", tagId);

                        factory.TagEngine.SetMessagesTag(new List<int> { message.Id }, tagId);
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat(
                            "SetMessagesTag(tenant={0}, userId='{1}', messageId={2}, tagid = {3}) Exception:\r\n{4}\r\n",
                            mailbox.TenantId, mailbox.UserId, message.Id, e.ToString(),
                            tagIds != null ? string.Join(",", tagIds) : "null");
                    }
                }

                log.Debug("DoOptionalOperations->AddRelationshipEventForLinkedAccounts()");

                factory.CrmLinkEngine.AddRelationshipEventForLinkedAccounts(mailbox, message, _tasksConfig.DefaultApiSchema);

                log.Debug("DoOptionalOperations->SaveEmailInData()");

                factory.EmailInEngine.SaveEmailInData(mailbox, message, _tasksConfig.DefaultApiSchema);

                log.Debug("DoOptionalOperations->SendAutoreply()");

                factory.AutoreplyEngine.SendAutoreply(mailbox, message, _tasksConfig.DefaultApiSchema, log);

                log.Debug("DoOptionalOperations->UploadIcsToCalendar()");

                if (folder.Folder != Enums.FolderType.Spam)
                {
                    factory
                        .CalendarEngine
                        .UploadIcsToCalendar(mailbox, message.CalendarId, message.CalendarUid, message.CalendarEventIcs,
                            message.CalendarEventCharset, message.CalendarEventMimeType, mailbox.EMail.Address,
                            _tasksConfig.DefaultApiSchema);
                }

                if (_tasksConfig.SaveOriginalMessage)
                {
                    log.Debug("DoOptionalOperations->StoreMailEml()");
                    StoreMailEml(mailbox.TenantId, mailbox.UserId, message.StreamId, mimeMessage, log);
                }

                log.Debug("DoOptionalOperations->ApplyFilters()");

                var filters = GetFilters(factory, log);

                factory.FilterEngine.ApplyFilters(message, mailbox, folder, filters);

                log.Debug("DoOptionalOperations->NotifySignalrIfNeed()");

                NotifySignalrIfNeed(mailbox, log);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DoOptionalOperations() Exception:\r\n{0}\r\n", ex.ToString());
            }
        }

        public string StoreMailEml(int tenant, string user, string streamId, MimeMessage message, ILog log)
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

                    log.InfoFormat("StoreMailEml() tenant='{0}', user_id='{1}', save_eml_path='{2}' Result: {3}", tenant, user, savePath, res);

                    return res;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("StoreMailEml Exception: {0}", ex.ToString());
            }

            return string.Empty;
        }

        private void FreeTask(TaskData taskData, ICollection<TaskData> tasks)
        {
            try
            {
                _log.DebugFormat("End Task {0} with status = '{1}'.", taskData.Task.Id, taskData.Task.Status);

                if (!tasks.Remove(taskData))
                    _log.Error("Task not exists in tasks array.");

                var mailbox = taskData.Mailbox;

                ReleaseMailbox(mailbox);

                taskData.Task.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("FreeTask(id:'{0}', email:'{1}'): Exception:\r\n{2}\r\n", taskData.Mailbox.MailBoxId, taskData.Mailbox.EMail, ex.ToString());
            }
        }

        private void ReleaseMailbox(MailBoxData mailbox)
        {
            if (mailbox == null)
                return;

            if (mailbox.LastSignalrNotifySkipped)
                NotifySignalrIfNeed(mailbox, _log);

            _queueManager.ReleaseMailbox(mailbox);

            if (!Filters.ContainsKey(mailbox.UserId))
                return;

            List<MailSieveFilterData> filters;
            if (!Filters.TryRemove(mailbox.UserId, out filters))
            {
                _log.Error("Try forget Filters for user failed");
            }
        }

        private void LogStat(string method, MailBoxData mailBoxData, TimeSpan duration, bool failed)
        {
            if (!_tasksConfig.CollectStatistics)
                return;

            _logStat.DebugWithProps(method,
                new KeyValuePair<string, object>("duration", duration.TotalMilliseconds),
                new KeyValuePair<string, object>("mailboxId", mailBoxData.MailBoxId),
                new KeyValuePair<string, object>("address", mailBoxData.EMail.ToString()),
                new KeyValuePair<string, object>("status", failed ? S_FAIL : S_OK));
        }

        #endregion
    }
}