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
using System.Configuration;

namespace ASC.Mail.Data.Contracts
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

                var maxTasksAtOnce = ConfigurationManagerExtension.AppSettings["cleaner.max-tasks-at-once"] != null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["cleaner.max-tasks-at-once"])
                    : defaultConfig.MaxTasksAtOnce;

                var maxFilesToRemoveAtOnce = ConfigurationManagerExtension.AppSettings["cleaner.files-remove-limit-at-once"] !=
                                             null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["cleaner.files-remove-limit-at-once"])
                    : defaultConfig.MaxFilesToRemoveAtOnce;

                var tenantCacheDays = ConfigurationManagerExtension.AppSettings["cleaner.tenant-cache-days"] != null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["cleaner.tenant-cache-days"])
                    : defaultConfig.TenantCacheDays;

                var tenantOverdueDays = ConfigurationManagerExtension.AppSettings["cleaner.tenant-overdue-days"] != null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["cleaner.tenant-overdue-days"])
                    : defaultConfig.TenantOverdueDays;

                var garbageOverdueDays = ConfigurationManagerExtension.AppSettings["cleaner.mailbox-garbage-overdue-days"] !=
                                         null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["cleaner.mailbox-garbage-overdue-days"])
                    : defaultConfig.GarbageOverdueDays;

                var httpContextScheme = ConfigurationManagerExtension.AppSettings["mail.default-api-scheme"] != null
                    ? (ConfigurationManagerExtension.AppSettings["mail.default-api-scheme"] == Uri.UriSchemeHttps
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
