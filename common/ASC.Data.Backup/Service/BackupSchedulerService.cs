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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Data.Backup.Storage;
using System;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;

namespace ASC.Data.Backup.Service
{
    internal class BackupSchedulerService
    {
        private readonly object schedulerLock = new object();
        private readonly ILog log = LogManager.GetLogger("ASC.Backup.Scheduler");
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
                    log.DebugFormat("started to schedule backups");
                    var backupRepostory = BackupStorageFactory.GetBackupRepository();
                    var backupsToSchedule = backupRepostory.GetBackupSchedules().Where(schedule => schedule.IsToBeProcessed()).ToList();
                    log.DebugFormat("{0} backups are to schedule", backupsToSchedule.Count);
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
                                log.DebugFormat("Start scheduled backup: {0}, {1}, {2}, {3}", schedule.TenantId, schedule.BackupMail, schedule.StorageType, schedule.StorageBasePath);
                                BackupWorker.StartScheduledBackup(schedule);
                            }
                            else
                            {
                                log.DebugFormat("Skip portal {0} not paid", schedule.TenantId);
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
