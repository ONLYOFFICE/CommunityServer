/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Mail.Core.Engine;

namespace ASC.Mail.StorageCleaner
{
    internal class StorageCleanerService : ServiceBase
    {
        #region - Declaration -

        private readonly ILog _log;
        private Timer _intervalTimer;
        private readonly ManualResetEvent _resetEvent;
        private MailGarbageEngine _eraser;
        private readonly TimeSpan _tsInterval;
        private readonly CancellationTokenSource _cancelTokenSource;

        #endregion

        #region - Constructor -

        public StorageCleanerService()
        {
            CanStop = true;

            AutoLog = true;

            _log = LogManager.GetLogger("ASC.Mail.Cleaner");

            _resetEvent = new ManualResetEvent(false);

            _cancelTokenSource = new CancellationTokenSource();

            _tsInterval = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.timer-wait-minutes"]));

            _log.InfoFormat("Service will clear mail storage every {0} minutes\r\n", _tsInterval.TotalMinutes);

            _eraser = new MailGarbageEngine(_log);

            _intervalTimer = new Timer(IntervalTimer_Elapsed, _cancelTokenSource.Token, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region - Start / Stop -

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

                StopTimer();

                if (_intervalTimer != null)
                {
                    _intervalTimer.Dispose();
                    _intervalTimer = null;
                }

                if (_resetEvent != null)
                    _resetEvent.Set();

                if (_eraser != null)
                {
                    _eraser.Dispose();
                    _eraser = null;
                }

            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Stop service Error: {0}\r\n", ex.ToString());
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

            Console.CancelKeyPress += (sender, e) => OnStop();

            _resetEvent.WaitOne();
        }

        private void StartTimer(bool immediately = false)
        {
            if (_intervalTimer == null)
                return;

            if (immediately)
            {
                _log.Debug("Setup _workTimer to start immediately");

                _intervalTimer.Change(0, Timeout.Infinite);
            }
            else
            {
                _log.DebugFormat("Setup _workTimer to {0} minutes", _tsInterval.TotalMinutes);

                _intervalTimer.Change(_tsInterval, _tsInterval);
            }
        }

        private void StopTimer()
        {
            if (_intervalTimer == null)
                return;

            _log.Debug("Setup _workTimer to Timeout.Infinite");

            _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region - Timer_Elapsed -

        private void IntervalTimer_Elapsed(object state)
        {
            _log.Debug("Timer->workTimer_Elapsed");

            var cancelToken = state as CancellationToken? ?? new CancellationToken();

            try
            {
                _eraser.ClearMailGarbage(cancelToken);

                _log.InfoFormat("All mailboxes were processed. Go back to timer. Next start after {0} minutes.\r\n",
                    _tsInterval.TotalMinutes);

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

                    return;
                }

                _log.ErrorFormat("Timer->workTimer_Elapsed. Exception:\r\n{0}\r\n", ex.ToString());
            }

            StartTimer();
        }

        #endregion
    }
}
