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

namespace ASC.Mail.Aggregator.Common
{
    public class TasksConfig
    {
        public enum AggregateModeType
        {
            All,
            External,
            Internal
        };

        public TimeSpan CheckTimerInterval { get; set; }
        public TimeSpan ActiveInterval { get; set; }
        public List<string> WorkOnUsersOnly { get; set; }
        public AggregateModeType AggregateMode { get; set; }
        public bool EnableSignalr { get; set; }
        public int ChunkOfPop3Uidl { get; set; }
        public int MaxMessagesPerSession { get; set; }
        public int MaxTasksAtOnce { get; set; }
        public TimeSpan OverdueAccountDelay { get; set; }
        public TimeSpan QuotaEndedDelay { get; set; }
        public TimeSpan TenantCachingPeriod { get; set; }
        public TimeSpan QueueLifetime { get; set; }
        public double InactiveMailboxesRatio { get; set; }
        public TimeSpan AuthErrorWarningTimeout { get; set; }
        public TimeSpan AuthErrorDisableMailboxTimeout { get; set; }
        public int TenantOverdueDays { get; set; }
        public long TenantMinQuotaBalance { get; set; }
        public bool SaveOriginalMessage { get; set; }
        public Dictionary<string, int> ImapFlags { get; set; }
        public string[] SkipImapFlags { get; set; }
        public string[] PopUnorderedDomains { get; set; }
        public Dictionary<string, Dictionary<string, MailBox.MailboxInfo>> SpecialDomainFolders { get; set; }
        public Dictionary<string, int> DefaultFolders { get; set; }
        public string DefaultApiSchema { get; set; }
        public TimeSpan TaskLifetime { get; set; }
        public bool SslCertificateErrorsPermit { get; set; }
        public int TcpTimeout { get; set; }
        public string ProtocolLogPath { get; set; }
        public bool CollectStatistics { get; set; }

        public static readonly TasksConfig Default = new TasksConfig
        {
            CheckTimerInterval = TimeSpan.FromSeconds(10),
            ActiveInterval = TimeSpan.FromSeconds(90),
            WorkOnUsersOnly = new List<string>(),
            AggregateMode = AggregateModeType.All,
            EnableSignalr = false,
            ChunkOfPop3Uidl = 100,
            MaxMessagesPerSession = 10,
            MaxTasksAtOnce = 10,
            OverdueAccountDelay = TimeSpan.FromSeconds(600),
            QuotaEndedDelay = TimeSpan.FromSeconds(600),
            TenantCachingPeriod = TimeSpan.FromSeconds(86400), // 1 day
            QueueLifetime = TimeSpan.FromSeconds(30),
            InactiveMailboxesRatio = 25,
            AuthErrorWarningTimeout = TimeSpan.FromHours(1),
            AuthErrorDisableMailboxTimeout = TimeSpan.FromDays(3),
            TenantOverdueDays = 10,
            TenantMinQuotaBalance = 26214400,
            SaveOriginalMessage = false,
            ImapFlags = new Dictionary<string, int>(),
            SkipImapFlags = new string[] {},
            PopUnorderedDomains = new string[] {},
            SpecialDomainFolders = new Dictionary<string, Dictionary<string, MailBox.MailboxInfo>>(),
            DefaultFolders = new Dictionary<string, int>(),
            DefaultApiSchema = Uri.UriSchemeHttp,
            TaskLifetime = TimeSpan.FromSeconds(300),
            SslCertificateErrorsPermit = false,
            TcpTimeout = 30000,
            ProtocolLogPath = "",
            CollectStatistics = true
        };

        public static TasksConfig FromConfig
        {
            get
            {
                var config = Default;

                if (ConfigurationManager.AppSettings["mail.check-work-timer-seconds"] != null)
                {
                    config.CheckTimerInterval = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.check-work-timer-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.activity-timeout-seconds"] != null)
                {
                    config.ActiveInterval = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.activity-timeout-seconds"]));
                }

                Guid userId;
                if (ConfigurationManager.AppSettings["mail.one-user-mode"] != null &&
                    Guid.TryParse(ConfigurationManager.AppSettings["mail.one-user-mode"], out userId))
                {
                    config.WorkOnUsersOnly.Add(ConfigurationManager.AppSettings["mail.one-user-mode"]);
                }

                if (ConfigurationManager.AppSettings["mail.aggregate-mode"] != null)
                {
                    AggregateModeType aggregateMode;

                    switch (ConfigurationManager.AppSettings["mail.aggregate-mode"])
                    {
                        case "external":
                            aggregateMode = AggregateModeType.External;
                            break;
                        case "internal":
                            aggregateMode = AggregateModeType.Internal;
                            break;
                        default:
                            aggregateMode = AggregateModeType.All;
                            break;
                    }

                    config.AggregateMode = aggregateMode;
                }

                if (ConfigurationManager.AppSettings["web.enable-signalr"] != null)
                {
                    config.EnableSignalr = Convert.ToBoolean(ConfigurationManager.AppSettings["web.enable-signalr"]);
                }

                if (ConfigurationManager.AppSettings["mail.check-pop3-uidl-chunk"] != null)
                {
                    config.ChunkOfPop3Uidl =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.check-pop3-uidl-chunk"]);

                    if (config.ChunkOfPop3Uidl < 1 || config.ChunkOfPop3Uidl > 100)
                        config.ChunkOfPop3Uidl = 100;
                }

                if (ConfigurationManager.AppSettings["mail.max-messages-per-mailbox"] != null)
                {
                    config.MaxMessagesPerSession =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.max-messages-per-mailbox"]);

                    if (config.MaxMessagesPerSession < 1)
                        config.MaxMessagesPerSession = -1;
                }

                if (ConfigurationManager.AppSettings["mail.max-tasks-count"] != null)
                {
                    config.MaxTasksAtOnce = Convert.ToInt32(ConfigurationManager.AppSettings["mail.max-tasks-count"]);

                    if (config.MaxTasksAtOnce < 1)
                        config.MaxTasksAtOnce = 1;
                }

                if (ConfigurationManager.AppSettings["mail.overdue-account-delay-seconds"] != null)
                {
                    config.OverdueAccountDelay = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.overdue-account-delay-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.quota-ended-delay-seconds"] != null)
                {
                    config.QuotaEndedDelay = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.quota-ended-delay-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.tenant-cache-lifetime-seconds"] != null)
                {
                    config.TenantCachingPeriod = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.tenant-cache-lifetime-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.queue-lifetime-seconds"] != null)
                {
                    config.QueueLifetime =
                        TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.queue-lifetime-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.inactive-mailboxes-ratio"] != null)
                {
                    config.InactiveMailboxesRatio =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.inactive-mailboxes-ratio"]);

                    if (config.InactiveMailboxesRatio < 0)
                        config.InactiveMailboxesRatio = 0;
                    if (config.InactiveMailboxesRatio > 100)
                        config.InactiveMailboxesRatio = 100;
                }

                if (ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"] != null)
                {
                    config.AuthErrorWarningTimeout = TimeSpan.FromMinutes(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"]));
                }

                if (ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"] != null)
                {
                    config.AuthErrorDisableMailboxTimeout = TimeSpan.FromMinutes(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"]));
                }

                if (ConfigurationManager.AppSettings["mail.tenant-overdue-days"] != null)
                {
                    config.TenantOverdueDays =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.tenant-overdue-days"]);
                }

                if (ConfigurationManager.AppSettings["mail.tenant-min-quota-balance"] != null)
                {
                    config.TenantMinQuotaBalance =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.tenant-min-quota-balance"]);
                }

                if (ConfigurationManager.AppSettings["mail.save-original-message"] != null)
                {
                    config.SaveOriginalMessage = Convert.ToBoolean(
                        ConfigurationManager.AppSettings["mail.save-original-message"]);
                }

                if (ConfigurationManager.AppSettings["mail.default-api-scheme"] != null)
                {
                    config.DefaultApiSchema = ConfigurationManager.AppSettings["mail.default-api-scheme"] ==
                                              Uri.UriSchemeHttps
                        ? Uri.UriSchemeHttps
                        : Uri.UriSchemeHttp;
                }

                if (ConfigurationManager.AppSettings["mail.task-process-lifetime-seconds"] != null)
                {
                    config.TaskLifetime =
                        TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManager.AppSettings["mail.task-process-lifetime-seconds"]));
                }

                if (ConfigurationManager.AppSettings["mail.certificate-permit"] != null)
                {
                    config.SslCertificateErrorsPermit =
                        Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);
                }

                if (ConfigurationManager.AppSettings["mail.tcp-timeout"] != null)
                {
                    config.TcpTimeout =
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.tcp-timeout"]);
                }

                if (ConfigurationManager.AppSettings["mail.protocol-log-path"] != null)
                {
                    config.ProtocolLogPath = ConfigurationManager.AppSettings["mail.protocol-log-path"] ?? "";
                }

                if (ConfigurationManager.AppSettings["mail.collect-statistics"] != null)
                {
                    config.CollectStatistics = Convert.ToBoolean(ConfigurationManager.AppSettings["mail.collect-statistics"] ?? "true");
                }

                return config;
            }
        }
    }
}
