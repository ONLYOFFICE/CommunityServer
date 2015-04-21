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
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.GarbageEraser;
using log4net.Config;

namespace ASC.Mail.StorageCleaner
{
    class StorageCleanerService : ServiceBase
    {
        #region - Declaration -

        private readonly ILogger _log;
        private Timer _intervalTimer;
        readonly ManualResetEvent _resetEvent;
        readonly TimeSpan _tsInterval;

        #endregion

        #region - Constructor -

        public StorageCleanerService()
        {
            CanStop = true;
            AutoLog = true;

            XmlConfigurator.Configure();

            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "StorageCleaner");
            _resetEvent = new ManualResetEvent(false);
            _tsInterval = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["timer.wait-minutes"]));

            _log.Info("Service will clear mail storage every {0} minutes\r\n", _tsInterval.TotalMinutes);
        }

        #endregion

        #region - Start / Stop -

        public void StartConsole()
        {
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
            _resetEvent.WaitOne();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Start service\r\n");
                _intervalTimer = new Timer(IntervalTimer_Elapsed, _resetEvent, 0, Timeout.Infinite);
            }
            catch (Exception)
            {
                OnStop();
            }
        }

        protected override void OnStop()
        {
            _resetEvent.Set();

            _log.Info("Stop service\r\n");

            if (_intervalTimer == null) return;

            _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _intervalTimer.Dispose();
            _intervalTimer = null;
        }

        private void IntervalTimer_Elapsed(object state)
        {
            var resetEvent = (ManualResetEvent)state;
            _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            var eraser = new MailGarbageEraser(_log);
            eraser.ClearMailGarbage(resetEvent);
            _intervalTimer.Change(_tsInterval, _tsInterval);
        }

        #endregion
    }
}
