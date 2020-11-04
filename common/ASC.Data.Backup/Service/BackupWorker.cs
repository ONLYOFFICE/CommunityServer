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
using System.Threading;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Data.Backup.Storage;
using ASC.Data.Backup.Tasks;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Backup.Utils;

namespace ASC.Data.Backup.Service
{
    internal static class BackupWorker
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        private static ProgressQueue tasks;
        private static ProgressQueue schedulerTasks;
        internal static string TempFolder;
        private static string currentRegion;
        private static Dictionary<string, string> configPaths;
        private static int limit;
        private static string upgradesPath;

        public static void Start(BackupConfigurationSection config)
        {
            TempFolder = PathHelper.ToRootedPath(config.TempFolder);
            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }

            limit = config.Limit;
            upgradesPath = config.UpgradesPath;
            currentRegion = config.WebConfigs.CurrentRegion;
            configPaths = config.WebConfigs.Cast<WebConfigElement>().ToDictionary(el => el.Region, el => PathHelper.ToRootedConfigPath(el.Path));
            configPaths[currentRegion] = PathHelper.ToRootedConfigPath(config.WebConfigs.CurrentPath);

            var invalidConfigPath = configPaths.Values.FirstOrDefault(path => !File.Exists(path));
            if (invalidConfigPath != null)
            {
                Log.WarnFormat("Configuration file {0} not found", invalidConfigPath);
            }

            tasks = new ProgressQueue(config.Service.WorkerCount, TimeSpan.FromMinutes(15), false);
            schedulerTasks = new ProgressQueue(config.Scheduler.WorkerCount, TimeSpan.FromMinutes(15), false);
        }

        public static void Stop()
        {
            if (tasks != null)
            {
                tasks.Terminate();
                tasks = null;
            }
            if (schedulerTasks != null)
            {
                schedulerTasks.Terminate();
                schedulerTasks = null;
            }
        }

        public static BackupProgress StartBackup(StartBackupRequest request)
        {
            lock (tasks.SynchRoot)
            {
                var item = tasks.GetItems().OfType<BackupProgressItem>().FirstOrDefault(t => t.TenantId == request.TenantId);
                if (item != null && item.IsCompleted)
                {
                    tasks.Remove(item);
                    item = null;
                }
                if (item == null)
                {
                    item = new BackupProgressItem(false, request.TenantId, request.UserId, request.StorageType, request.StorageBasePath) { BackupMail = request.BackupMail, StorageParams = request.StorageParams };
                    tasks.Add(item);
                }
                return ToBackupProgress(item);
            }
        }

        public static void StartScheduledBackup(Schedule schedule)
        {
            lock (schedulerTasks.SynchRoot)
            {
                var item = schedulerTasks.GetItems().OfType<BackupProgressItem>().FirstOrDefault(t => t.TenantId == schedule.TenantId);
                if (item != null && item.IsCompleted)
                {
                    schedulerTasks.Remove(item);
                    item = null;
                }
                if (item == null)
                {
                    item = new BackupProgressItem(true, schedule.TenantId, Guid.Empty, schedule.StorageType, schedule.StorageBasePath) { BackupMail = schedule.BackupMail, StorageParams = schedule.StorageParams };
                    schedulerTasks.Add(item);
                }
            }
        }

        public static BackupProgress GetBackupProgress(int tenantId)
        {
            lock (tasks.SynchRoot)
            {
                return ToBackupProgress(tasks.GetItems().OfType<BackupProgressItem>().FirstOrDefault(t => t.TenantId == tenantId));
            }
        }

        public static void ResetBackupError(int tenantId)
        {
            lock (tasks.SynchRoot)
            {
                var progress = tasks.GetItems().OfType<BackupProgressItem>().FirstOrDefault(t => t.TenantId == tenantId);
                if (progress != null)
                {
                    progress.Error = null;
                }
            }
        }

        public static void ResetRestoreError(int tenantId)
        {
            lock (tasks.SynchRoot)
            {
                var progress = tasks.GetItems().OfType<RestoreProgressItem>().FirstOrDefault(t => t.TenantId == tenantId);
                if (progress != null)
                {
                    progress.Error = null;
                }
            }
        }

        public static BackupProgress StartRestore(StartRestoreRequest request)
        {
            lock (tasks.SynchRoot)
            {
                var item = tasks.GetItems().OfType<RestoreProgressItem>().FirstOrDefault(t => t.TenantId == request.TenantId);
                if (item != null && item.IsCompleted)
                {
                    tasks.Remove(item);
                    item = null;
                }
                if (item == null)
                {
                    item = new RestoreProgressItem(request.TenantId, request.StorageType, request.FilePathOrId, request.NotifyAfterCompletion) { StorageParams = request.StorageParams };
                    tasks.Add(item);
                }
                return ToBackupProgress(item);
            }
        }

        public static BackupProgress GetRestoreProgress(int tenantId)
        {
            lock (tasks.SynchRoot)
            {
                return ToBackupProgress(tasks.GetItems().OfType<RestoreProgressItem>().FirstOrDefault(t => t.TenantId == tenantId));
            }
        }

        public static BackupProgress StartTransfer(int tenantId, string targetRegion, bool transferMail, bool notify)
        {
            lock (tasks.SynchRoot)
            {
                var item = tasks.GetItems().OfType<TransferProgressItem>().FirstOrDefault(t => t.TenantId == tenantId);
                if (item != null && item.IsCompleted)
                {
                    tasks.Remove(item);
                    item = null;
                }
                if (item == null)
                {
                    item = new TransferProgressItem(tenantId, targetRegion, transferMail, notify);
                    tasks.Add(item);
                }
                return ToBackupProgress(item);
            }
        }

        public static BackupProgress GetTransferProgress(int tenantId)
        {
            lock (tasks.SynchRoot)
            {
                return ToBackupProgress(tasks.GetItems().OfType<TransferProgressItem>().FirstOrDefault(t => t.TenantId == tenantId));
            }
        }

        private static BackupProgress ToBackupProgress(IProgressItem progressItem)
        {
            if (progressItem == null)
            {
                return null;
            }

            var progress = new BackupProgress
            {
                IsCompleted = progressItem.IsCompleted,
                Progress = (int)progressItem.Percentage,
                Error = progressItem.Error != null ? ((Exception)progressItem.Error).Message : null
            };

            var backupProgressItem = progressItem as BackupProgressItem;
            if (backupProgressItem != null)
            {
                progress.Link = backupProgressItem.Link;
            }
            else
            {
                var transferProgressItem = progressItem as TransferProgressItem;
                if (transferProgressItem != null)
                {
                    progress.Link = transferProgressItem.Link;
                }
            }

            return progress;
        }

        private class BackupProgressItem : IProgressItem
        {
            private const string ArchiveFormat = "tar.gz";
            private bool IsScheduled { get; set; }
            public int TenantId { get; private set; }
            private Guid UserId { get; set; }
            private BackupStorageType StorageType { get; set; }
            private string StorageBasePath { get; set; }
            public bool BackupMail { get; set; }

            public Dictionary<string, string> StorageParams { get; set; }

            public string Link { get; private set; }

            public object Id { get; set; }
            public object Status { get; set; }
            public object Error { get; set; }
            public double Percentage { get; set; }
            public bool IsCompleted { get; set; }

            public BackupProgressItem(bool isScheduled, int tenantId, Guid userId, BackupStorageType storageType, string storageBasePath)
            {
                Id = Guid.NewGuid();
                IsScheduled = isScheduled;
                TenantId = tenantId;
                UserId = userId;
                StorageType = storageType;
                StorageBasePath = storageBasePath;
            }

            public void RunJob()
            {
                if (ThreadPriority.BelowNormal < Thread.CurrentThread.Priority)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                }

                var dateTime = CoreContext.Configuration.Standalone ? DateTime.Now : DateTime.UtcNow;
                var backupName = string.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}.{2}", CoreContext.TenantManager.GetTenant(TenantId).TenantAlias, dateTime, ArchiveFormat);
                var tempFile = Path.Combine(TempFolder, backupName);
                var storagePath = tempFile;
                try
                {
                    var backupTask = new BackupPortalTask(Log, TenantId, configPaths[currentRegion], tempFile, limit);
                    if (!BackupMail)
                    {
                        backupTask.IgnoreModule(ModuleName.Mail);
                    }
                    backupTask.IgnoreTable("tenants_tariff");
                    backupTask.ProgressChanged += (sender, args) => Percentage = 0.9 * args.Progress;
                    backupTask.RunJob();

                    var backupStorage = BackupStorageFactory.GetBackupStorage(StorageType, TenantId, StorageParams);
                    if (backupStorage != null)
                    {
                        storagePath = backupStorage.Upload(StorageBasePath, tempFile, UserId);
                        Link = backupStorage.GetPublicLink(storagePath);
                    }

                    var repo = BackupStorageFactory.GetBackupRepository();
                    repo.SaveBackupRecord(
                        new BackupRecord
                        {
                            Id = (Guid)Id,
                            TenantId = TenantId,
                            IsScheduled = IsScheduled,
                            FileName = Path.GetFileName(tempFile),
                            StorageType = StorageType,
                            StorageBasePath = StorageBasePath,
                            StoragePath = storagePath,
                            CreatedOn = DateTime.UtcNow,
                            ExpiresOn = StorageType == BackupStorageType.DataStore ? DateTime.UtcNow.AddDays(1) : DateTime.MinValue,
                            StorageParams = StorageParams
                        });

                    Percentage = 100;

                    if (UserId != Guid.Empty && !IsScheduled)
                    {
                        NotifyHelper.SendAboutBackupCompleted(TenantId, UserId, Link);
                    }

                    IsCompleted = true;
                }
                catch (Exception error)
                {
                    Log.ErrorFormat("RunJob - Params: {0}, Error = {1}", new { Id = Id, Tenant = TenantId, File = tempFile, BasePath = StorageBasePath, }, error);
                    Error = error;
                    IsCompleted = true;
                }
                finally
                {
                    try
                    {
                        if (!(storagePath == tempFile && StorageType == BackupStorageType.Local))
                        {
                            File.Delete(tempFile);
                        }
                    }
                    catch (Exception error)
                    {
                        Log.Error("can't delete file: {0}", error);
                    }
                }
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        private class RestoreProgressItem : IProgressItem
        {
            public int TenantId { get; private set; }
            public BackupStorageType StorageType { get; set; }
            public string StoragePath { get; set; }
            public bool Notify { get; set; }

            public Dictionary<string, string> StorageParams { get; set; }

            public object Id { get; set; }
            public object Status { get; set; }
            public object Error { get; set; }
            public double Percentage { get; set; }
            public bool IsCompleted { get; set; }

            public RestoreProgressItem(int tenantId, BackupStorageType storageType, string storagePath, bool notify)
            {
                Id = Guid.NewGuid();
                TenantId = tenantId;
                StorageType = storageType;
                StoragePath = storagePath;
                Notify = notify;
            }

            public void RunJob()
            {
                Tenant tenant = null;
                var tempFile = PathHelper.GetTempFileName(TempFolder);
                try
                {
                    NotifyHelper.SendAboutRestoreStarted(TenantId, Notify);

                    var storage = BackupStorageFactory.GetBackupStorage(StorageType, TenantId, StorageParams);
                    storage.Download(StoragePath, tempFile);

                    Percentage = 10;

                    tenant = CoreContext.TenantManager.GetTenant(TenantId);
                    tenant.SetStatus(TenantStatus.Restoring);
                    CoreContext.TenantManager.SaveTenant(tenant);

                    var columnMapper = new ColumnMapper();
                    columnMapper.SetMapping("tenants_tenants", "alias", tenant.TenantAlias, ((Guid)Id).ToString("N"));
                    columnMapper.Commit();

                    var restoreTask = new RestorePortalTask(Log, TenantId, configPaths[currentRegion], tempFile, columnMapper, upgradesPath);
                    restoreTask.IgnoreTable("tenants_tariff");
                    restoreTask.ProgressChanged += (sender, args) => Percentage = (10d + 0.65 * args.Progress);
                    restoreTask.RunJob();

                    Tenant restoredTenant = null;

                    if (restoreTask.Dump)
                    {
                        if (Notify)
                        {
                            AscCache.OnClearCache();
                            var tenants = CoreContext.TenantManager.GetTenants();
                            foreach (var t in tenants)
                            {
                                NotifyHelper.SendAboutRestoreCompleted(t.TenantId, Notify);
                            }
                        }
                    }
                    else
                    {
                        CoreContext.TenantManager.RemoveTenant(tenant.TenantId);

                        restoredTenant = CoreContext.TenantManager.GetTenant(columnMapper.GetTenantMapping());
                        restoredTenant.SetStatus(TenantStatus.Active);
                        restoredTenant.TenantAlias = tenant.TenantAlias;
                        restoredTenant.PaymentId = string.Empty;
                        if (string.IsNullOrEmpty(restoredTenant.MappedDomain) && !string.IsNullOrEmpty(tenant.MappedDomain))
                        {
                            restoredTenant.MappedDomain = tenant.MappedDomain;
                        }
                        CoreContext.TenantManager.SaveTenant(restoredTenant);

                        // sleep until tenants cache expires
                        Thread.Sleep(TimeSpan.FromMinutes(2));

                        NotifyHelper.SendAboutRestoreCompleted(restoredTenant.TenantId, Notify);
                    }

                    Percentage = 75;

                    File.Delete(tempFile);

                    Percentage = 100;
                }
                catch (Exception error)
                {
                    Log.Error(error);
                    Error = error;

                    if (tenant != null)
                    {
                        tenant.SetStatus(TenantStatus.Active);
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    IsCompleted = true;
                }
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        private class TransferProgressItem : IProgressItem
        {
            public int TenantId { get; private set; }
            public string TargetRegion { get; set; }
            public bool TransferMail { get; set; }
            public bool Notify { get; set; }

            public string Link { get; set; }

            public object Id { get; set; }
            public object Status { get; set; }
            public object Error { get; set; }
            public double Percentage { get; set; }
            public bool IsCompleted { get; set; }

            public TransferProgressItem(int tenantId, string targetRegion, bool transferMail, bool notify)
            {
                Id = Guid.NewGuid();
                TenantId = tenantId;
                TargetRegion = targetRegion;
                TransferMail = transferMail;
                Notify = notify;
            }

            public void RunJob()
            {
                var tempFile = PathHelper.GetTempFileName(TempFolder);
                var alias = CoreContext.TenantManager.GetTenant(TenantId).TenantAlias;
                try
                {
                    NotifyHelper.SendAboutTransferStart(TenantId, TargetRegion, Notify);

                    var transferProgressItem = new TransferPortalTask(Log, TenantId, configPaths[currentRegion], configPaths[TargetRegion], limit) { BackupDirectory = TempFolder };
                    transferProgressItem.ProgressChanged += (sender, args) => Percentage = args.Progress;
                    if (!TransferMail)
                    {
                        transferProgressItem.IgnoreModule(ModuleName.Mail);
                    }
                    transferProgressItem.RunJob();

                    Link = GetLink(alias, false);
                    NotifyHelper.SendAboutTransferComplete(TenantId, TargetRegion, Link, !Notify);
                }
                catch (Exception error)
                {
                    Log.Error(error);
                    Error = error;

                    Link = GetLink(alias, true);
                    NotifyHelper.SendAboutTransferError(TenantId, TargetRegion, Link, !Notify);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    IsCompleted = true;
                }
            }

            private string GetLink(string alias, bool isErrorLink)
            {
                return "http://" + alias + "." + ConfigurationProvider.Open(configPaths[isErrorLink ? currentRegion : TargetRegion]).AppSettings.Settings["core.base-domain"].Value;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}
