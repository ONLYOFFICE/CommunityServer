/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Net;
using ASC.Mail.Aggregator.CollectionService.Configuration;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.CollectionService
{
    public sealed class CollectorService: ServiceBase
    {
        public const string asc_mail_collection_service_name = "ASC Mail Collection Service";
        private readonly MailBoxManager _manager;
        private readonly Collector _collector;
        private readonly ILogger _log;

        public CollectorService(IEnumerable<string> work_on_this_users_only = null)
        {
            ServiceName = asc_mail_collection_service_name;
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
                _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "CollectorService");

                _log.Info("Connecting to db...");

                _manager = new MailBoxManager(25, _log);

                var auth_error_warning_timeout =
                    ConfigurationManager.AppSettings["mail.AuthErrorSendWarningAlertTimeout"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.AuthErrorSendWarningAlertTimeout"]))
                        : TimeSpan.FromHours(2);

                _manager.AuthErrorWarningTimeout = auth_error_warning_timeout;

                _log.Info("Auth login error warning timeout is {0}.", auth_error_warning_timeout.ToString());

                var auth_error_disable_mailbox_timeout =
                    ConfigurationManager.AppSettings["mail.AuthErrorDisableMailboxTimeout"] != null
                        ? TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.AuthErrorDisableMailboxTimeout"]))
                        : TimeSpan.FromDays(3);

                _log.Info("Auth login error disable mailbox timeout is {0}.", auth_error_disable_mailbox_timeout.ToString());

                _manager.AuthErrorDisableTimeout = auth_error_disable_mailbox_timeout;

                _log.Info("Creating collector service...");

                var handlers_log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "MessageHandlers");
                var queue_settings = MailQueueSettings.FromConfig;
                if (work_on_this_users_only != null)
                    queue_settings.WorkOnUsersOnly = work_on_this_users_only.ToList();
                else
                {
                    var user_to_work_on = ConfigurationManager.AppSettings["mail.OneUserMode"];
                    if (!string.IsNullOrEmpty(user_to_work_on))
                        queue_settings.WorkOnUsersOnly.Add(user_to_work_on);
                }

                _manager.TenantOverdueDays = queue_settings.OverdueDays;

                var handlers = MessageHandlersSettings.FromConfig(handlers_log, "mail");

                var enable_activity_log = ConfigurationManager.AppSettings["mail.EnableActivityLog"] == null || Convert.ToBoolean(ConfigurationManager.AppSettings["mail.EnableActivityLog"]);

                _manager.EnableActivityLog = enable_activity_log;

                _log.Info("Db aggregator activity log is {0}.", enable_activity_log ? "enabled" : "disabled");

                _collector = new Collector(_manager, queue_settings, handlers);

                _log.Info("Service is ready.");

                AggregatorLogger.Instance.Initialize(_manager, GetServiceIp());
                _log.Info("Aggregator logger initialized.");
            }
            catch (Exception ex)
            {
                _log.Fatal("CollectorService error under constuct: {0}", ex.ToString());
            }
        }

        private string GetServiceIp()
        {
            var dns_host_name = Dns.GetHostName();
            IPHostEntry ip_entry = Dns.GetHostEntry(dns_host_name);
            IPAddress first_or_default = ip_entry.AddressList.FirstOrDefault(x => x.AddressFamily != AddressFamily.InterNetworkV6);
            if (first_or_default != null)
                return first_or_default.ToString();

            return "Unspecified address for hostname: " + dns_host_name;
        }

        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            _log.Info("Service Dispose(disposing = {0}).", disposing);
            base.Dispose(disposing);
        }

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            _log.Info("Service start.");
            _collector.Start();
            base.OnStart(args);
        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            _log.Info("Service stop.");
            _collector.Stop();
            _log.Flush();
            base.OnStop();
        }

        /// <summary>
        /// OnPause: Put your pause code here
        /// - Pause working threads, etc.
        /// </summary>
        protected override void OnPause()
        {
            _log.Info("Service pause.");
            _log.Flush();
            base.OnPause();
        }

        /// <summary>
        /// OnContinue(): Put your continue code here
        /// - Un-pause working threads, etc.
        /// </summary>
        protected override void OnContinue()
        {
            _log.Info("Service continue.");
            _log.Flush();
            base.OnContinue();
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
            _collector.Stop();
            _log.Flush();
            base.OnShutdown();
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 & 256</param>
        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            _log.Info("Service call custom command({0}).", command);
            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="power_status">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus power_status)
        {
            _log.Info("Service power event detected = {0}.", power_status.ToString());
            _log.Flush();
            if (power_status == PowerBroadcastStatus.Suspend)
            {
                _collector.Stop();
            }
            return base.OnPowerEvent(power_status);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event
        ///   from a Terminal Server session.
        ///   Useful if you need to determine
        ///   when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="change_description">The Session Change
        /// Event that occured.</param>
        protected override void OnSessionChange(
                  SessionChangeDescription change_description)
        {
            _log.Info("Service session change event detected = {0}.", change_description.ToString());
            _log.Flush();
            base.OnSessionChange(change_description);
        }

        /// <summary>
        /// Start service in console mode.
        /// </summary>
        public void StartDaemon()
        {
            _log.Info("Service Start console daemon().");
            _collector.Start();
        }
    }
}