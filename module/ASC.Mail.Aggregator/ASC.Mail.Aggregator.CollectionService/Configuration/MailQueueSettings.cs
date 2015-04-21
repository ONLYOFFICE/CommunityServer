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

namespace ASC.Mail.Aggregator.CollectionService.Configuration
{
    public class MailQueueSettings
    {
        public int MaxMessagesPerSession { get; set; }
        public int ConcurrentThreadCount { get; set; }
        public TimeSpan CheckInterval { get; set; }
        public TimeSpan ActivityTimeout { get; set; }
        public TimeSpan OverdueAccountDelay { get; set; }
        public TimeSpan TenantCachingPeriod { get; set; }
        public int OverdueDays { get; set; }
        public List<string> WorkOnUsersOnly { get; set; }
        public bool OnlyTeamlabTasks { get; set; }
        public bool EnableSignalr { get; set; }
        public int InactiveMailboxesRatio { get; set; }
        public int CheckPop3UidlChunk { get; set; }

        public static readonly MailQueueSettings Default = new MailQueueSettings
                                                               {
                                                                   MaxMessagesPerSession = 200,
                                                                   CheckInterval = TimeSpan.FromSeconds(1),
                                                                   ConcurrentThreadCount = 5,
                                                                   ActivityTimeout = TimeSpan.FromSeconds(90),
                                                                   OverdueAccountDelay = TimeSpan.FromSeconds(600),
                                                                   TenantCachingPeriod = TimeSpan.FromSeconds(86400), // 1 day
                                                                   OverdueDays = 10,
                                                                   WorkOnUsersOnly = new List<string>(),
                                                                   OnlyTeamlabTasks = false,
                                                                   EnableSignalr = false,
                                                                   InactiveMailboxesRatio = 25,
                                                                   CheckPop3UidlChunk = 100
                                                               };

        public static MailQueueSettings FromConfig
        {
            get
            {
                var configured = Default;
                var section = (CollectionServiceConfigurationSection)ConfigurationManager.GetSection(Schema.section_name);
                configured.CheckInterval = TimeSpan.FromSeconds(section.QueueConfiguration.CheckInterval);
                configured.ConcurrentThreadCount = section.QueueConfiguration.Threads;
                configured.MaxMessagesPerSession = section.QueueConfiguration.MaxNewMessages;
                configured.ActivityTimeout = TimeSpan.FromSeconds(section.QueueConfiguration.ActivityTimeout);
                configured.OverdueAccountDelay = TimeSpan.FromSeconds(section.QueueConfiguration.OverdueAccountDelay);
                configured.OverdueDays = section.QueueConfiguration.OverdueDays;
                configured.TenantCachingPeriod = TimeSpan.FromSeconds(section.QueueConfiguration.TenantCachingPeriod);
                configured.OnlyTeamlabTasks = ConfigurationManager.AppSettings["mail.OnlyTeamlabTasks"] != null &&
                                              Convert.ToBoolean(ConfigurationManager.AppSettings["mail.OnlyTeamlabTasks"]);
                configured.EnableSignalr = ConfigurationManager.AppSettings["web.enable-signalr"] != null &&
                                              Convert.ToBoolean(ConfigurationManager.AppSettings["web.enable-signalr"]);
                configured.InactiveMailboxesRatio = section.QueueConfiguration.InactiveRatio;
                configured.CheckPop3UidlChunk =
                    Convert.ToInt32(ConfigurationManager.AppSettings["mail.check-pop3-uidl-chunk"] ?? "100");
                return configured;
            }
        }
    }
}