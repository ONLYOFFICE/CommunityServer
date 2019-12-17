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
