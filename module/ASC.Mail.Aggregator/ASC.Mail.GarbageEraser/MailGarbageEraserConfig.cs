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

namespace ASC.Mail.GarbageEraser
{
    public class MailGarbageEraserConfig
    {
        public string HttpContextScheme { get; private set; }
        public int GarbageOverdueDays { get; private set; }
        public int MaxTasksAtOnce { get; private set; }
        public int MaxFilesToRemoveAtOnce { get; private set; }
        public int TenantCacheDays { get; private set; }
        public int TenantOverdueDays { get; private set; }

        public MailGarbageEraserConfig(int maxTasksAtOnce, int maxFilesToRemoveAtOnce, int tenantCacheDays,
            int tenantOverdueDays, int garbageOverdueDays, string httpContextScheme)
        {
            HttpContextScheme = httpContextScheme;
            GarbageOverdueDays = garbageOverdueDays;
            MaxTasksAtOnce = maxTasksAtOnce;
            MaxFilesToRemoveAtOnce = maxFilesToRemoveAtOnce;
            TenantCacheDays = tenantCacheDays;
            TenantOverdueDays = tenantOverdueDays;
        }

        public static MailGarbageEraserConfig Default()
        {
            return new MailGarbageEraserConfig(1, 100, 1, 30, 30, "http");
        }

        public static MailGarbageEraserConfig FromConfig()
        {
            try
            {
                var defaultConfig = Default();

                var maxTasksAtOnce = ConfigurationManager.AppSettings["cleaner.max-tasks-at-once"] != null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.max-tasks-at-once"])
                    : defaultConfig.MaxTasksAtOnce;

                var maxFilesToRemoveAtOnce = ConfigurationManager.AppSettings["cleaner.files-remove-limit-at-once"] !=
                                             null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.files-remove-limit-at-once"])
                    : defaultConfig.MaxFilesToRemoveAtOnce;

                var tenantCacheDays = ConfigurationManager.AppSettings["cleaner.tenant-cache-days"] != null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.tenant-cache-days"])
                    : defaultConfig.TenantCacheDays;

                var tenantOverdueDays = ConfigurationManager.AppSettings["cleaner.tenant-overdue-days"] != null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.tenant-overdue-days"])
                    : defaultConfig.TenantOverdueDays;

                var garbageOverdueDays = ConfigurationManager.AppSettings["cleaner.mailbox-garbage-overdue-days"] !=
                                         null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["cleaner.mailbox-garbage-overdue-days"])
                    : defaultConfig.GarbageOverdueDays;

                var httpContextScheme = ConfigurationManager.AppSettings["mail.default-api-scheme"] != null
                    ? (ConfigurationManager.AppSettings["mail.default-api-scheme"] == Uri.UriSchemeHttps
                        ? Uri.UriSchemeHttps
                        : Uri.UriSchemeHttp)
                    : defaultConfig.HttpContextScheme;

                return new MailGarbageEraserConfig(maxTasksAtOnce, maxFilesToRemoveAtOnce, tenantCacheDays,
                    tenantOverdueDays, garbageOverdueDays, httpContextScheme);
            }
            catch (Exception)
            {
                return Default();
            }
        }
    }
}
