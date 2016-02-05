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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ASC.Mail.Aggregator.CollectionService.Configuration;
using ASC.Mail.Aggregator.CollectionService.Queue;
using ASC.Mail.Aggregator.CollectionService.Workers;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.CollectionService
{
    public sealed class AggregatorService: ServiceBase
    {
        public const string AscMailCollectionServiceName = "ASC Mail Collection Service";
        private readonly ILogger _log;

        private readonly CancellationTokenSource _cancelTokenSource;
        readonly ManualResetEvent _resetEvent;
        readonly TimeSpan _tsInterval;
        private Timer _intervalTimer;

        private readonly TasksConfig _tasksConfig;

        private readonly QueueManager _queueManager;
        readonly TaskFactory _taskFactory;
        private readonly TimeSpan _tsTaskStateCheckInterval;

        private bool _IsFirstTime = true;

        public AggregatorService(IEnumerable<string> workOnThisUsersOnly = null)
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

                var queueSettings = MailQueueSettings.FromConfig;
                
                if (workOnThisUsersOnly != null)
                    queueSettings.WorkOnUsersOnly = workOnThisUsersOnly.ToList();
                else
                {
                    var userToWorkOn = ConfigurationManager.AppSettings["mail.OneUserMode"];
                    if (!string.IsNullOrEmpty(userToWorkOn))
                        queueSettings.WorkOnUsersOnly.Add(userToWorkOn);
                }

                var authErrorWarningTimeout =
                    ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"] != null
                        ? TimeSpan.FromMinutes(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"]))
                        : TimeSpan.FromHours(1);

                _log.Info("Auth login error warning timeout is {0}.", authErrorWarningTimeout.ToString());

                var authErrorDisableMailboxTimeout =
                    ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"] != null
                        ? TimeSpan.FromMinutes(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"]))
                        : TimeSpan.FromDays(3);

                _log.Info("Auth login error disable mailbox timeout is {0}.", authErrorDisableMailboxTimeout.ToString());

                _log.Info("MailWorkerQueue: ConcurrentThreadCount = {0} and CheckInterval = {1} CheckPOP3_UIDL_Chunck = {2}",
                queueSettings.ConcurrentThreadCount, queueSettings.CheckInterval, queueSettings.CheckPop3UidlChunk);

                var configBuilder = new TasksConfig.Builder();

                if (queueSettings.WorkOnUsersOnly != null && queueSettings.WorkOnUsersOnly.Any())
                {
                    var i = 0;
                    var users = string.Empty;
                    queueSettings.WorkOnUsersOnly.ForEach(user => users += string.Format("\r\n\t\t\t\t{0}. \"{1}\"", ++i, user));

                    _log.Info("Aggregator will get tasks for this users only:" + users);
                }

                var queueLifetime = ConfigurationManager.AppSettings["mail.queue-lifetime-seconds"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.queue-lifetime-seconds"]))
                        : TimeSpan.FromSeconds(30);

                var showActiveUpLogs = ConfigurationManager.AppSettings["mail.show-activeup-logs"] != null &&
                                       Convert.ToBoolean(
                                           ConfigurationManager.AppSettings["mail.show-activeup-logs"]);

                var saveOriginalMessage = ConfigurationManager.AppSettings["mail.save-original-message"] != null &&
                                          Convert.ToBoolean(
                                              ConfigurationManager.AppSettings["mail.save-original-message"]);

                _tasksConfig = configBuilder.SetUsersToWorkOn(queueSettings.WorkOnUsersOnly)
                                            .SetAggregateMode(queueSettings.AggregateMode)
                                            .SetActiveInterval(queueSettings.ActivityTimeout)
                                            .SetChunkOfPop3CheckUidLinDb(queueSettings.CheckPop3UidlChunk)
                                            .SetEnableSignalr(queueSettings.EnableSignalr)
                                            .SetMaxMessagesPerSession(queueSettings.MaxMessagesPerSession)
                                            .SetMaxTasksAtOnce(queueSettings.ConcurrentThreadCount)
                                            .SetQueueLifetime(queueLifetime)
                                            .SetTenantCachingPeriod(queueSettings.TenantCachingPeriod)
                                            .SetShowActiveUpLogs(showActiveUpLogs)
                                            .SetInactiveMailboxesRatio(queueSettings.InactiveMailboxesRatio)
                                            .SetAuthErrorWarningTimeout(authErrorWarningTimeout)
                                            .SetAuthErrorDisableMailboxTimeout(authErrorDisableMailboxTimeout)
                                            .SetMinQuotaBalance(queueSettings.MinQuotaBalance)
                                            .SetOverdueAccountDelay(queueSettings.OverdueAccountDelay)
                                            .SetTenantOverdueDays(queueSettings.OverdueDays)
                                            .SetQuotaEndedDelay(queueSettings.QuotaEndedDelay)
                                            .SetSaveOriginalMessageFlag(saveOriginalMessage)
                                            .Build();

                _tsInterval = queueSettings.CheckInterval;

                _queueManager = new QueueManager(_tasksConfig, _log);

                _resetEvent = new ManualResetEvent(false);

                _cancelTokenSource = new CancellationTokenSource();

                _taskFactory = new TaskFactory();

                _tsTaskStateCheckInterval = ConfigurationManager.AppSettings["mail.task-check-state-seconds"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.task-check-state-seconds"]))
                        : TimeSpan.FromSeconds(30);

                if (ConfigurationManager.AppSettings["mail.default-api-scheme"] != null)
                {
                    var defaultApiScheme = ConfigurationManager.AppSettings["mail.default-api-scheme"];

                    ApiHelper.SetupScheme(defaultApiScheme);
                }

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

                _intervalTimer = new Timer(IntervalTimer_Elapsed, _cancelTokenSource.Token, 0, Timeout.Infinite);

                base.OnStart(args);
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

            if (_intervalTimer != null)
            {
                _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _intervalTimer.Dispose();
                _intervalTimer = null;
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
            _log.Info("Service Start console daemon().");
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
            _resetEvent.WaitOne();
        }

        #endregion

        #region - Timer_Elapsed -

        private void IntervalTimer_Elapsed(object state)
        {
            _log.Debug("Timer->IntervalTimer_Elapsed");

            var cancelToken = state as CancellationToken? ?? new CancellationToken();

            try
            {
                if (_IsFirstTime)
                {
                    _queueManager.LoadQueueFromDump();

                    if (_queueManager.ProcessingCount > 0)
                    {
                        _log.Info("Found {0} tasks to release", _queueManager.ProcessingCount);

                        _queueManager.ReleaseAllProcessingMailboxes();

                        _queueManager.SaveQueueToDump();
                    }

                    _IsFirstTime = false;
                }

                if (cancelToken.IsCancellationRequested || _intervalTimer == null)
                {
                    _log.Debug("Timer->IntervalTimer_Elapsed - {0}. Quit.", cancelToken.IsCancellationRequested ? "IsCancellationRequested" : "_intervalTimer == null");
                    return;
                }

                _log.Debug("Setup _intervalTimer to Timeout.Infinite");
                _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);

                var tasks = CreateTasks(_tasksConfig.MaxTasksAtOnce, cancelToken);

                if(tasks.Any())
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

                _log.Error("Timer->IntervalTimer_Elapsed. Exception:\r\n{0}\r\n", ex.ToString());

                if (_queueManager.ProcessingCount != 0)
                {
                    _queueManager.ReleaseAllProcessingMailboxes();
                }
            }

            _queueManager.SaveQueueToDump();

            _queueManager.CancelHandler.Set();

            if (_intervalTimer != null)
            {
                _log.Debug("Setup _intervalTimer to {0} seconds", _tsInterval.TotalSeconds);
                _intervalTimer.Change(_tsInterval, _tsInterval);
            }
        }

        private List<Task<MailBox>> CreateTasks(int needCount, CancellationToken cancelToken)
        {
            _log.Info("CreateTasks(need {0} tasks).", needCount);

            var mailboxes = _queueManager.GetLockedMailboxes(needCount);

            var tasks =
                mailboxes.Select(
                    mailbox =>
                    _taskFactory.StartNew(() => ProcessMailbox(mailbox, _tasksConfig, cancelToken),
                                          cancelToken)).ToList();

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

            try
            {
                var manager = new MailBoxManager(taskLogger)
                {
                    AuthErrorWarningTimeout = tasksConfig.AuthErrorWarningTimeout,
                    AuthErrorDisableTimeout = tasksConfig.AuthErrorDisableMailboxTimeout
                };

                if (mailbox.Imap)
                {
                    using (var worker = new Imap4Worker(manager, mailbox, tasksConfig, cancelToken, tasksConfig.SaveOriginalMessage, taskLogger))
                    {
                        worker.Aggregate();
                    }
                }
                else
                {
                    using (var worker = new Pop3Worker(manager, mailbox, tasksConfig, cancelToken, tasksConfig.SaveOriginalMessage, taskLogger))
                    {
                        worker.Aggregate();
                    }
                }

                taskLogger.Info("Mailbox '{0}' has been processed.", mailbox.EMail);
            }
            catch (OperationCanceledException)
            {
                taskLogger.Info("Task canceled.");
                throw;
            }
            catch (Exception ex)
            {
                taskLogger.Error(
                    "ProcessMailbox(Tenant = {0}, MailboxId = {1}, Address = '{2}')\r\nException: {3}\r\n",
                    mailbox.TenantId, mailbox.MailBoxId, mailbox.EMail, ex.ToString());
            }

            return mailbox;
        }

        private void FreeTask(Task<MailBox> task, ICollection<Task<MailBox>> tasks)
        {
            _log.Debug("End Task {0} with status = '{1}'.", task.Id, task.Status);

            if (!tasks.Remove(task))
                _log.Error("Task not exists in tasks array.");

            ReleaseMailbox(task.Result);

            task.Dispose();
        }

        private void ReleaseMailbox(MailBox mailbox)
        {
            _queueManager.ReleaseMailbox(mailbox);
        }

        #endregion
    }
}