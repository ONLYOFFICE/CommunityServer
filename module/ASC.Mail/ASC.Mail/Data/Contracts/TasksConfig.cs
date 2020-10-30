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
using System.Collections.Generic;
using System.Configuration;

namespace ASC.Mail.Data.Contracts
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
        public Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>> SpecialDomainFolders { get; set; }
        public Dictionary<string, int> DefaultFolders { get; set; }
        public string DefaultApiSchema { get; set; }
        public TimeSpan TaskLifetime { get; set; }
        public bool SslCertificateErrorsPermit { get; set; }
        public int TcpTimeout { get; set; }
        public string ProtocolLogPath { get; set; }
        public bool CollectStatistics { get; set; }

        public bool UseDump { get; set; }

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
            SkipImapFlags = new string[] { },
            SpecialDomainFolders = new Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>>(),
            DefaultFolders = new Dictionary<string, int>(),
            DefaultApiSchema = Uri.UriSchemeHttp,
            TaskLifetime = TimeSpan.FromSeconds(300),
            SslCertificateErrorsPermit = false,
            TcpTimeout = 30000,
            ProtocolLogPath = "",
            CollectStatistics = true,
            UseDump = false
        };

        public static TasksConfig FromConfig
        {
            get
            {
                var config = Default;

                if (ConfigurationManagerExtension.AppSettings["mail.check-work-timer-seconds"] != null)
                {
                    config.CheckTimerInterval = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.check-work-timer-seconds"]));
                }

                if (ConfigurationManagerExtension.AppSettings["mail.activity-timeout-seconds"] != null)
                {
                    config.ActiveInterval = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.activity-timeout-seconds"]));
                }

                Guid userId;
                if (ConfigurationManagerExtension.AppSettings["mail.one-user-mode"] != null &&
                    Guid.TryParse(ConfigurationManagerExtension.AppSettings["mail.one-user-mode"], out userId))
                {
                    config.WorkOnUsersOnly.Add(ConfigurationManagerExtension.AppSettings["mail.one-user-mode"]);
                }

                if (ConfigurationManagerExtension.AppSettings["mail.aggregate-mode"] != null)
                {
                    AggregateModeType aggregateMode;

                    switch (ConfigurationManagerExtension.AppSettings["mail.aggregate-mode"])
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

                if (ConfigurationManagerExtension.AppSettings["web.enable-signalr"] != null)
                {
                    config.EnableSignalr = Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["web.enable-signalr"]);
                }

                if (ConfigurationManagerExtension.AppSettings["mail.check-pop3-uidl-chunk"] != null)
                {
                    config.ChunkOfPop3Uidl =
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.check-pop3-uidl-chunk"]);

                    if (config.ChunkOfPop3Uidl < 1 || config.ChunkOfPop3Uidl > 100)
                        config.ChunkOfPop3Uidl = 100;
                }

                if (ConfigurationManagerExtension.AppSettings["mail.max-messages-per-mailbox"] != null)
                {
                    config.MaxMessagesPerSession =
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.max-messages-per-mailbox"]);

                    if (config.MaxMessagesPerSession < 1)
                        config.MaxMessagesPerSession = -1;
                }

                if (ConfigurationManagerExtension.AppSettings["mail.max-tasks-count"] != null)
                {
                    config.MaxTasksAtOnce = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.max-tasks-count"]);

                    if (config.MaxTasksAtOnce < 1)
                        config.MaxTasksAtOnce = 1;
                }

                if (ConfigurationManagerExtension.AppSettings["mail.overdue-account-delay-seconds"] != null)
                {
                    config.OverdueAccountDelay = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.overdue-account-delay-seconds"]));
                }

                if (ConfigurationManagerExtension.AppSettings["mail.quota-ended-delay-seconds"] != null)
                {
                    config.QuotaEndedDelay = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.quota-ended-delay-seconds"]));
                }

                if (ConfigurationManagerExtension.AppSettings["mail.tenant-cache-lifetime-seconds"] != null)
                {
                    config.TenantCachingPeriod = TimeSpan.FromSeconds(
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.tenant-cache-lifetime-seconds"]));
                }

                if (ConfigurationManagerExtension.AppSettings["mail.queue-lifetime-seconds"] != null)
                {
                    config.QueueLifetime =
                        TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.queue-lifetime-seconds"]));
                }

                if (ConfigurationManagerExtension.AppSettings["mail.inactive-mailboxes-ratio"] != null)
                {
                    config.InactiveMailboxesRatio =
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.inactive-mailboxes-ratio"]);

                    if (config.InactiveMailboxesRatio < 0)
                        config.InactiveMailboxesRatio = 0;
                    if (config.InactiveMailboxesRatio > 100)
                        config.InactiveMailboxesRatio = 100;
                }

                config.AuthErrorWarningTimeout = Defines.AuthErrorWarningTimeout;
                config.AuthErrorDisableMailboxTimeout = Defines.AuthErrorDisableTimeout;

                if (ConfigurationManagerExtension.AppSettings["mail.tenant-overdue-days"] != null)
                {
                    config.TenantOverdueDays =
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.tenant-overdue-days"]);
                }

                if (ConfigurationManagerExtension.AppSettings["mail.tenant-min-quota-balance"] != null)
                {
                    config.TenantMinQuotaBalance =
                        Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.tenant-min-quota-balance"]);
                }

                config.SaveOriginalMessage = Defines.SaveOriginalMessage;

                config.DefaultApiSchema = Defines.DefaultApiSchema;

                if (ConfigurationManagerExtension.AppSettings["mail.task-process-lifetime-seconds"] != null)
                {
                    config.TaskLifetime =
                        TimeSpan.FromSeconds(
                            Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.task-process-lifetime-seconds"]));
                }

                config.SslCertificateErrorsPermit = Defines.SslCertificatesErrorPermit;

                config.TcpTimeout = Default.TcpTimeout;

                if (ConfigurationManagerExtension.AppSettings["mail.protocol-log-path"] != null)
                {
                    config.ProtocolLogPath = ConfigurationManagerExtension.AppSettings["mail.protocol-log-path"] ?? "";
                }

                if (ConfigurationManagerExtension.AppSettings["mail.collect-statistics"] != null)
                {
                    config.CollectStatistics = Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.collect-statistics"] ?? "true");
                }

                if (ConfigurationManagerExtension.AppSettings["mail.use-damp"] != null)
                {
                    config.UseDump = Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.use-damp"] ?? "false");
                }

                return config;
            }
        }
    }
}
