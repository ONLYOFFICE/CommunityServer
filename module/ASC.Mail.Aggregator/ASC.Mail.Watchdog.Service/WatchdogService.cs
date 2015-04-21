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
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator;
using System.Configuration;
using log4net.Config;

namespace ASC.Mail.Watchdog.Service
{
    partial class Watchdog : ServiceBase
    {
        #region - Declaration -

        private readonly ILogger _log;
        private readonly MailBoxManager _manager;

        private Timer _intervalTimer;
        readonly TimeSpan _tsInterval;
        readonly TimeSpan _tsTasksTimeoutInterval;

        private readonly ManualResetEvent _mreStop;

        #endregion

        #region - Constructor -

        public Watchdog()
        {
            InitializeComponent();
            CanStop = true;
            AutoLog = true;

            XmlConfigurator.Configure();

            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "Watchdog");
            _mreStop = new ManualResetEvent(false);
            _manager = new MailBoxManager(_log);
            _tsInterval =
                TimeSpan.FromMinutes(
                    Convert.ToInt32(ConfigurationManager.AppSettings["mailwatchdog.timer-interval-in-minutes"]));
            _tsTasksTimeoutInterval =
                TimeSpan.FromMinutes(
                    Convert.ToInt32(ConfigurationManager.AppSettings["mailwatchdog.tasks-timeout-in-minutes"]));

            _log.Info("\r\nConfiguration:\r\n" +
                      "\t- check locked mailboxes in every {0} minutes;\r\n" +
                      "\t- locked mailboxes timeout {1} minutes;\r\n",
                      _tsInterval.TotalMinutes,
                      _tsTasksTimeoutInterval.TotalMinutes);
        }

        #endregion

        #region - Start / Stop -

        public void StartConsole()
        {
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
            _mreStop.WaitOne();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Start service\r\n");
                _intervalTimer = new Timer(IntervalTimer_Elapsed, _mreStop, 0, Timeout.Infinite);
            }
            catch (Exception)
            {
                OnStop();
            }
        }

        protected override void OnStop()
        {
            //Service will be stoped in future 30 seconds!
            // signal to tell the worker process to stop
            _mreStop.Set();

            _log.Info("Stop service\r\n");

            if (_intervalTimer == null) return;

            _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _intervalTimer.Dispose();
            _intervalTimer = null;
        }

        #endregion

        #region - OnTimer -

        private void IntervalTimer_Elapsed(object state)
        {
            try
            {
                _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);

                _log.Info("ReleaseLockedMailboxes(timeout is {0} minutes)", _tsTasksTimeoutInterval.TotalMinutes);

                var freeMailboxIds = _manager.ReleaseLockedMailboxes((int) _tsTasksTimeoutInterval.TotalMinutes);

                if (freeMailboxIds.Any())
                {
                    _log.Info("Released next locked mailbox's ids: {0}", string.Join(",", freeMailboxIds));
                }
                else
                {
                    _log.Info("Nothing to do!");
                }

            }
            catch (Exception ex)
            {
                _log.Error("IntervalTimer_Elapsed() Exception:\r\n{0}", ex.ToString());
            }
            finally
            {
                _log.Info("Waiting for {0} minutes for next check...", _tsInterval.TotalMinutes);
                _intervalTimer.Change(_tsInterval, _tsInterval);
            }
        }

        #endregion
    }
}
