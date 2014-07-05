/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Service.ProgressItems;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Service
{
    internal class BackupService : IBackupService
    {
        private static readonly ILog log = LogFactory.Create();
        private static ProgressQueue tasks;
        private static Timer cleaner;

        public static void Initialize()
        {
            var config = BackupConfigurationSection.GetSection();

            tasks = new ProgressQueue(config.ThreadCount, TimeSpan.FromMinutes(15), false);
            cleaner = new Timer(Clean, config.ExpirePeriod, config.ExpirePeriod, config.ExpirePeriod);

            ThreadPool.QueueUserWorkItem(_ => RestoreTransferingTenants());
        }

        public static void Terminate()
        {
            if (tasks != null)
            {
                tasks.Terminate();
                tasks = null;
            }
            if (cleaner != null)
            {
                cleaner.Change(Timeout.Infinite, Timeout.Infinite);
                cleaner.Dispose();
                cleaner = null;
            }
        }

        public BackupResult CreateBackup(int tenantID, Guid userID)
        {
            return ToResult(FindOrAddItem(tenantID, () => new BackupProgressItem(log, tenantID, userID)));
        }

        public BackupResult GetBackupStatus(string id)
        {
            return ToResult(FindItem<BackupProgressItem>(Convert.ToInt32(id)));
        }

        public BackupResult TransferPortal(TransferRequest request)
        {
            return ToResult(FindOrAddItem(request.TenantId,
                                          () => new TransferProgressItem(log, request.TenantId, request.TargetRegion)
                                              {
                                                  NotifyOnlyOwner = !request.NotifyUsers,
                                                  TransferMail = request.BackupMail
                                              }));
        }

        public BackupResult GetTransferStatus(int tenantID)
        {
            return ToResult(FindItem<TransferProgressItem>(tenantID));
        }

        public BackupResult RestorePortal(int tenantId, string pathToBackupFile)
        {
            if (!File.Exists(pathToBackupFile))
            {
                log.Warn("Invalid restore request: file ({0}) not found.", pathToBackupFile);
                throw new FaultException("Backup file not found.");
            }
            return ToResult(FindOrAddItem(tenantId, () => new RestoreProgressItem(log, tenantId, pathToBackupFile)));
        }

        public BackupResult GetRestoreStatus(int tenantId)
        {
            return ToResult(FindItem<RestoreProgressItem>(tenantId));
        }

        public List<TransferRegion> GetTransferRegions()
        {
            var config = BackupConfigurationSection.GetSection();
            var currentRegion = config.WebConfigs.CurrentRegion;

            return (from WebConfigElement configFileElement in config.WebConfigs
                    let webConfig = FileUtility.OpenConfigurationFile(configFileElement.Path)
                    let baseDomain = webConfig.AppSettings.Settings["core.base-domain"].Value
                    select new TransferRegion
                        {
                            Name = configFileElement.Region,
                            BaseDomain = baseDomain,
                            IsCurrentRegion = configFileElement.Region.Equals(currentRegion, StringComparison.InvariantCultureIgnoreCase)
                        }).ToList();
        }

        private static T FindItem<T>(int tenantId) where T : BackupProgressItemBase
        {
            lock (tasks.SynchRoot)
            {
                return tasks.GetItems().OfType<T>().FirstOrDefault(i => i.TenantId == tenantId);
            }
        }

        private static T FindOrAddItem<T>(int tenantId, Func<T> itemFactory) where T : BackupProgressItemBase
        {
            lock (tasks.SynchRoot)
            {
                var item = FindItem<T>(tenantId);
                if (item != null && item.IsCompleted)
                {
                    tasks.Remove(item);
                    item = null;
                }
                if (item == null)
                {
                    item = itemFactory();
                    tasks.Add(item);
                }
                return item;
            }
        }

        private static BackupResult ToResult(BackupProgressItemBase task)
        {
            if (task == null)
                return null;

            if (task.Error != null)
                throw new FaultException(task.Error.Message);

            var result = new BackupResult
                {
                    Id = task.TenantId.ToString(),
                    Completed = task.IsCompleted,
                    Percent = task.Progress,
                };

            var backupTask = task as BackupProgressItem;
            if (backupTask != null)
            {
                result.Link = backupTask.Link;
                result.ExpireDate = backupTask.ExpirationDate;
            }
            return result;
        }

        private static void Clean(object period)
        {
            try
            {
                lock (tasks.SynchRoot)
                {
                    foreach (var item in tasks.GetItems().Where(i => i.IsCompleted).ToList())
                    {
                        var backupItem = item as BackupProgressItem;
                        if (backupItem == null || backupItem.ExpirationDate < DateTime.UtcNow)
                        {
                            tasks.Remove(item);
                        }
                    }
                }
                var pathToWebConfig = FileUtility.GetRootedPath(BackupConfigurationSection.GetSection().WebConfigs.GetCurrentConfig());
                var store = StorageFactory.GetStorage(pathToWebConfig, "backupfiles", "backup");
                store.DeleteExpired(string.Empty, string.Empty, (TimeSpan)period);
            }
            catch (Exception error)
            {
                log.Error(error);
            }
        }

        private static void RestoreTransferingTenants()
        {
            foreach (var tenant in CoreContext.TenantManager.GetTenants().Where(t => t.Status == TenantStatus.Transfering))
            {
                tenant.SetStatus(TenantStatus.Active);
                CoreContext.TenantManager.SaveTenant(tenant);
                NotifyHelper.SendAboutTransferError(tenant.TenantId, string.Empty, tenant.TenantDomain, true);
            }
        }
    }
}
