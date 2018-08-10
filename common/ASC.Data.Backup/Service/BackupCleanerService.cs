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
using System.Linq;
using System.Threading;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Storage;

namespace ASC.Data.Backup.Service
{
    public class BackupCleanerService
    {
        private readonly object cleanerLock = new object();
        private readonly ILog log = LogFactory.Create("ASC.Backup.Cleaner");
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
                    log.Debug("found {0} backups which are expired", backupsToRemove.Count);

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
                                log.Debug("only last {0} scheduled backup records are to keep for tenant {1} so {2} records must be removed", schedule.NumberOfBackupsStored, schedule.TenantId, scheduledBackupsToRemove.Count);
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
                            var backupStorage = BackupStorageFactory.GetBackupStorage(backupRecord.StorageType, backupRecord.TenantId);
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
