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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Storage;
using System;
using System.Linq;
using System.Threading;

namespace ASC.Data.Backup.Service
{
    internal class BackupSchedulerService
    {
        private readonly object schedulerLock = new object();
        private readonly ILog log = LogFactory.Create("ASC.Backup.Scheduler");
        private Timer schedulerTimer;
        private bool isStarted;

        public TimeSpan Period { get; set; }

        public BackupSchedulerService()
        {
            Period = TimeSpan.FromMinutes(15);
        }

        public void Start()
        {
            if (!isStarted && Period > TimeSpan.Zero)
            {
                log.Info("staring backup scheduler service...");
                schedulerTimer = new Timer(_ => ScheduleBackupTasks(), null, TimeSpan.Zero, Period);
                log.Info("backup scheduler service service started");
                isStarted = true;
            }
        }

        public void Stop()
        {
            if (isStarted)
            {
                log.Info("stoping backup scheduler service...");
                if (schedulerTimer != null)
                {
                    schedulerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    schedulerTimer.Dispose();
                    schedulerTimer = null;
                }
                log.Info("backup scheduler service stoped");
                isStarted = false;
            }
        }

        private void ScheduleBackupTasks()
        {
            if (Monitor.TryEnter(schedulerLock))
            {
                try
                {
                    log.Debug("started to schedule backups");
                    var backupRepostory = BackupStorageFactory.GetBackupRepository();
                    var backupsToSchedule = backupRepostory.GetBackupSchedules().Where(schedule => schedule.IsToBeProcessed()).ToList();
                    log.Debug("{0} backups are to schedule", backupsToSchedule.Count);
                    foreach (var schedule in backupsToSchedule)
                    {
                        if (!isStarted)
                        {
                            return;
                        }
                        try
                        {
                            var tariff = CoreContext.PaymentManager.GetTariff(schedule.TenantId);
                            if (tariff.State < TariffState.Delay)
                            {
                                schedule.LastBackupTime = DateTime.UtcNow;
                                backupRepostory.SaveBackupSchedule(schedule);
                                log.Debug("Start scheduled backup: {0}, {1}, {2}, {3}", schedule.TenantId, schedule.BackupMail, schedule.StorageType, schedule.StorageBasePath);
                                BackupWorker.StartScheduledBackup(schedule.TenantId, schedule.BackupMail, schedule.StorageType, schedule.StorageBasePath);
                            }
                            else
                            {
                                log.Debug("Skip portal {0} not paid", schedule.TenantId);
                            }
                        }
                        catch (Exception error)
                        {
                            log.Error("error while scheduling backups: {0}", error);
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error("error while scheduling backups: {0}", error);
                }
                finally
                {
                    Monitor.Exit(schedulerLock);
                }
            }
        }
    }
}
