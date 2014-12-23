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
using System.Globalization;
using System.ServiceProcess;
using System.Threading;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator;
using System.Configuration;

namespace ASC.Mail.Watchdog.Service
{
    partial class Watchdog : ServiceBase
    {
        #region - Declaration -

        private readonly ManualResetEvent _mreStop;
        private readonly ILogger _log;
        private readonly MailBoxManager _manger;
        Thread _thread;
        private readonly int _waitTimeInMinutes;
        #endregion

        #region - Constructor -
        public Watchdog()
        {
            InitializeComponent();
            CanStop = true;
            AutoLog = true;

            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "Watchdog");
            _mreStop = new ManualResetEvent(false);
            _manger = new MailBoxManager(25);
            _waitTimeInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["old_tasks_timeout_in_minutes"]);
        }
        #endregion

        #region - Start / Stop -
        public void StartConsole()
        {
            OnStart(null);
            Console.CancelKeyPress += delegate {
                    _mreStop.Set();
                };
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //Start service
                _mreStop.Reset();

                ThreadStart pts = MainThread;

                // Setup thread.
                _thread = new Thread(pts);
                // Start it.
                _thread.Start();
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
            _log.Info("Administrator stopped the service");
            // give it a little time to finish any pending work
            if (_thread == null) return;
            _thread.Join(new TimeSpan(0, 0, 30));
            _thread.Abort();
        }
        #endregion

        #region - Threads -
        void MainThread()
        {
            try
            {
                _log.Info("Start service");

                _log.Debug("The service will kill old aggregator's tasks in every {0} minutes", _waitTimeInMinutes);

                var wait_time = TimeSpan.FromMinutes(_waitTimeInMinutes);

                while (true)
                {
                    try
                    {
                        var killed_mailbox_ids = _manger.KillOldTasks(_waitTimeInMinutes);

                        if (killed_mailbox_ids.Count > 0)
                        {
                            var mail_boxes = "";

                            killed_mailbox_ids.ForEach(i => mail_boxes += i.ToString(CultureInfo.InvariantCulture) + "|");

                            mail_boxes = mail_boxes.Remove(mail_boxes.Length - 1, 1);

                            _log.Info("Killed next old task's ids: {0}", mail_boxes);
                        }
                        else
                        {
                            _log.Info("Nothing to do!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal("Unable to kill tasks. Exception:\r\n{0}", ex.ToString());
                    }

                    _log.Debug("Waiting for {0} minutes for next check...", _waitTimeInMinutes);

                    if (_mreStop.WaitOne(wait_time))
                    {
                        break;
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                OnStop();
            }
        }
        #endregion
    }
}
