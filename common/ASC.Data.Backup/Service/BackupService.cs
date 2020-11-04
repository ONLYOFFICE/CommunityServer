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
using System.IO;
using System.Linq;
using System.ServiceModel;
using ASC.Common.Logging;
using ASC.Core.Common.Contracts;
using ASC.Data.Backup.Storage;
using ASC.Data.Backup.Utils;

namespace ASC.Data.Backup.Service
{
    internal class BackupService : IBackupService
    {
        private readonly ILog log = LogManager.GetLogger("ASC.Backup.Service");

        public BackupProgress StartBackup(StartBackupRequest request)
        {
            var progress = BackupWorker.StartBackup(request);
            if (!string.IsNullOrEmpty(progress.Error))
            {
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public BackupProgress GetBackupProgress(int tenantId)
        {
            var progress = BackupWorker.GetBackupProgress(tenantId);
            if (progress != null && !string.IsNullOrEmpty(progress.Error))
            {
                BackupWorker.ResetBackupError(tenantId);
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public void DeleteBackup(Guid id)
        {
            var backupRepository = BackupStorageFactory.GetBackupRepository();
            var backupRecord = backupRepository.GetBackupRecord(id);
            backupRepository.DeleteBackupRecord(backupRecord.Id);

            var storage = BackupStorageFactory.GetBackupStorage(backupRecord);
            if (storage == null) return;
            storage.Delete(backupRecord.StoragePath);
        }

        public void DeleteAllBackups(int tenantId)
        {
            var backupRepository = BackupStorageFactory.GetBackupRepository();
            foreach (var backupRecord in backupRepository.GetBackupRecordsByTenantId(tenantId))
            {
                try
                {
                    backupRepository.DeleteBackupRecord(backupRecord.Id);
                    var storage = BackupStorageFactory.GetBackupStorage(backupRecord);
                    if (storage == null) continue;
                    storage.Delete(backupRecord.StoragePath);
                }
                catch (Exception error)
                {
                    log.Warn("error while removing backup record: {0}", error);
                }
            }
        }

        public List<BackupHistoryRecord> GetBackupHistory(int tenantId)
        {
            var backupHistory = new List<BackupHistoryRecord>();
            var backupRepository = BackupStorageFactory.GetBackupRepository();
            foreach (var record in backupRepository.GetBackupRecordsByTenantId(tenantId))
            {
                var storage = BackupStorageFactory.GetBackupStorage(record);
                if (storage == null) continue;
                if (storage.IsExists(record.StoragePath))
                {
                    backupHistory.Add(new BackupHistoryRecord
                        {
                            Id = record.Id,
                            FileName = record.FileName,
                            StorageType = record.StorageType,
                            CreatedOn = record.CreatedOn,
                            ExpiresOn = record.ExpiresOn
                        });
                }
                else
                {
                    backupRepository.DeleteBackupRecord(record.Id);
                }
            }
            return backupHistory;
        }

        public BackupProgress StartTransfer(StartTransferRequest request)
        {
            var progress = BackupWorker.StartTransfer(request.TenantId, request.TargetRegion, request.BackupMail, request.NotifyUsers);
            if (!string.IsNullOrEmpty(progress.Error))
            {
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public BackupProgress GetTransferProgress(int tenantID)
        {
            var progress = BackupWorker.GetTransferProgress(tenantID);
            if (!string.IsNullOrEmpty(progress.Error))
            {
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public BackupProgress StartRestore(StartRestoreRequest request)
        {
            if (request.StorageType == BackupStorageType.Local)
            {
                if (string.IsNullOrEmpty(request.FilePathOrId) || !File.Exists(request.FilePathOrId))
                {
                    throw new FileNotFoundException();
                }
            }

            if (!request.BackupId.Equals(Guid.Empty))
            {
                var backupRepository = BackupStorageFactory.GetBackupRepository();
                var backupRecord = backupRepository.GetBackupRecord(request.BackupId);
                if (backupRecord == null)
                {
                    throw new FileNotFoundException();
                }

                request.FilePathOrId = backupRecord.StoragePath;
                request.StorageType = backupRecord.StorageType;
                request.StorageParams = backupRecord.StorageParams;
            }

            var progress = BackupWorker.StartRestore(request);
            if (!string.IsNullOrEmpty(progress.Error))
            {
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public BackupProgress GetRestoreProgress(int tenantId)
        {
            var progress = BackupWorker.GetRestoreProgress(tenantId);
            if (progress != null && !string.IsNullOrEmpty(progress.Error))
            {
                BackupWorker.ResetRestoreError(tenantId);
                throw new FaultException(progress.Error);
            }
            return progress;
        }

        public string GetTmpFolder()
        {
            return BackupWorker.TempFolder;
        }

        public List<TransferRegion> GetTransferRegions()
        {
            var webConfigs = BackupConfigurationSection.GetSection().WebConfigs;
            return webConfigs
                .Cast<WebConfigElement>()
                .Select(configElement =>
                    {
                        var config = ConfigurationProvider.Open(PathHelper.ToRootedConfigPath(configElement.Path));
                        var baseDomain = config.AppSettings.Settings["core.base-domain"].Value;
                        return new TransferRegion
                            {
                                Name = configElement.Region,
                                BaseDomain = baseDomain,
                                IsCurrentRegion = configElement.Region.Equals(webConfigs.CurrentRegion, StringComparison.InvariantCultureIgnoreCase)
                            };
                    })
                .ToList();
        }

        public void CreateSchedule(CreateScheduleRequest request)
        {
            BackupStorageFactory.GetBackupRepository().SaveBackupSchedule(
                new Schedule(request.TenantId)
                    {
                        Cron = request.Cron,
                        BackupMail = request.BackupMail,
                        NumberOfBackupsStored = request.NumberOfBackupsStored,
                        StorageType = request.StorageType,
                        StorageBasePath = request.StorageBasePath,
                        StorageParams = request.StorageParams
                    });
        }

        public void DeleteSchedule(int tenantId)
        {
            BackupStorageFactory.GetBackupRepository().DeleteBackupSchedule(tenantId);
        }

        public ScheduleResponse GetSchedule(int tenantId)
        {
            var schedule = BackupStorageFactory.GetBackupRepository().GetBackupSchedule(tenantId);
            return schedule != null
                       ? new ScheduleResponse
                           {
                               StorageType = schedule.StorageType,
                               StorageBasePath = schedule.StorageBasePath,
                               BackupMail = schedule.BackupMail,
                               NumberOfBackupsStored = schedule.NumberOfBackupsStored,
                               Cron = schedule.Cron,
                               LastBackupTime = schedule.LastBackupTime,
                               StorageParams = schedule.StorageParams
                           }
                       : null;
        }
    }
}
