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
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using ASC.Data.Backup.Storage;

namespace ASC.Data.Backup.Service
{
    public class BackupCleanerService
    {
        private readonly object cleanerLock = new object();
        private readonly ILog log = LogManager.GetLogger("ASC.Backup.Cleaner");
        private Timer cleanTimer;
        private bool isStarted;

        public TimeSpan Period { get; set; }

        public BackupCleanerService()
        {
            Period = TimeSpan.FromMinutes(15);
        }

        public void Start()
        {
            if (!isStarted && Period > TimeSpan.Zero)
            {
                log.Info("starting backup cleaner service...");
                cleanTimer = new Timer(_ => DeleteExpiredBackups(), null, TimeSpan.Zero, Period);
                log.Info("backup cleaner service started");
                isStarted = true;
            }
        }

        public void Stop()
        {
            if (isStarted)
            {
                log.Info("stopping backup cleaner service...");
                if (cleanTimer != null)
                {
                    cleanTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    cleanTimer.Dispose();
                    cleanTimer = null;
                }
                log.Info("backup cleaner service stopped");
                isStarted = false;
            }
        }

        private void DeleteExpiredBackups()
        {
            if (Monitor.TryEnter(cleanerLock))
            {
                try
                {
                    log.Debug("started to clean expired backups");
                    
                    var backupRepository = BackupStorageFactory.GetBackupRepository();
                    
                    var backupsToRemove = backupRepository.GetExpiredBackupRecords();
                    log.DebugFormat("found {0} backups which are expired", backupsToRemove.Count);

                    if (!isStarted) return;
                    foreach (var scheduledBackups in backupRepository.GetScheduledBackupRecords().GroupBy(r => r.TenantId))
                    {
                        if (!isStarted) return;
                        var schedule = backupRepository.GetBackupSchedule(scheduledBackups.Key);
                        if (schedule != null)
                        {
                            var scheduledBackupsToRemove = scheduledBackups.OrderByDescending(r => r.CreatedOn).Skip(schedule.NumberOfBackupsStored).ToList();
                            if (scheduledBackupsToRemove.Any())
                            {
                                log.DebugFormat("only last {0} scheduled backup records are to keep for tenant {1} so {2} records must be removed", schedule.NumberOfBackupsStored, schedule.TenantId, scheduledBackupsToRemove.Count);
                                backupsToRemove.AddRange(scheduledBackupsToRemove);
                            }
                        }
                        else
                        {
                            backupsToRemove.AddRange(scheduledBackups);
                        }
                    }

                    foreach (var backupRecord in backupsToRemove)
                    {
                        if (!isStarted) return;
                        try
                        {
                            var backupStorage = BackupStorageFactory.GetBackupStorage(backupRecord);
                            if (backupStorage == null) continue;

                            backupStorage.Delete(backupRecord.StoragePath);

                            backupRepository.DeleteBackupRecord(backupRecord.Id);
                        }
                        catch (Exception error)
                        {
                            log.Warn("can't remove backup record: {0}", error);
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error("error while cleaning expired backup records: {0}", error);
                }
                finally
                {
                    Monitor.Exit(cleanerLock);
                }
            }
        }
    }
}
