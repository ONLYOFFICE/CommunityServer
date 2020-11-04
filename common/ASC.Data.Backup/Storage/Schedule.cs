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
using System.Threading;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Notify.Cron;
using ASC.Web.Studio.Core.Backup;

namespace ASC.Data.Backup.Storage
{
    public class Schedule
    {
        public int TenantId { get; private set; }
        public string Cron { get; set; }
        public bool BackupMail { get; set; }
        public int NumberOfBackupsStored { get; set; }
        public BackupStorageType StorageType { get; set; }
        public string StorageBasePath { get; set; }
        public DateTime LastBackupTime { get; internal set; }
        public Dictionary<string, string> StorageParams { get; internal set; }

        public Schedule(int tenantId)
        {
            TenantId = tenantId;
        }

        public bool IsToBeProcessed()
        {
            try
            {
                if (BackupHelper.ExceedsMaxAvailableSize(TenantId)) throw new Exception("Backup file exceed " + TenantId);

                var cron = new CronExpression(Cron);
                var tenant = CoreContext.TenantManager.GetTenant(TenantId);
                var tenantTimeZone = tenant.TimeZone;
                var culture = tenant.GetCulture();
                Thread.CurrentThread.CurrentCulture = culture;

                var lastBackupTime = LastBackupTime.Equals(default(DateTime))
                    ? DateTime.UtcNow.Date.AddSeconds(-1)
                    : TenantUtil.DateTimeFromUtc(tenantTimeZone, LastBackupTime);

                var nextBackupTime = cron.GetTimeAfter(lastBackupTime);

                if (!nextBackupTime.HasValue) return false;
                var now = TenantUtil.DateTimeFromUtc(tenantTimeZone, DateTime.UtcNow);
                return nextBackupTime <= now;
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error("Schedule " + TenantId, e);
                return false;
            }
        }
    }
}
