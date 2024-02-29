/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Data.Backup.Service;
using ASC.Data.Backup.Utils;

namespace ASC.Data.Backup.Storage
{
    internal static class BackupStorageFactory
    {
        public static IBackupStorage GetBackupStorage(BackupRecord record)
        {
            try
            {
                return GetBackupStorage(record.StorageType, record.TenantId, record.StorageParams);
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Backup.Service").Error("can't get backup storage for record " + record.Id, error);
                return null;
            }
        }

        public static IBackupStorage GetBackupStorage(BackupStorageType type, int tenantId, Dictionary<string, string> storageParams)
        {
            var config = BackupConfigurationSection.GetSection();
            var webConfigPath = PathHelper.ToRootedConfigPath(config.WebConfigs.CurrentPath);
            switch (type)
            {
                case BackupStorageType.Documents:
                case BackupStorageType.ThridpartyDocuments:
                    return new DocumentsBackupStorage(tenantId, webConfigPath);
                case BackupStorageType.DataStore:
                    return new ConsumerBackupStorage(tenantId, webConfigPath);
                case BackupStorageType.Local:
                    return new LocalBackupStorage();
                case BackupStorageType.ThirdPartyConsumer:
                    if (storageParams == null) return null;
                    CoreContext.TenantManager.SetCurrentTenant(tenantId);
                    return new ConsumerBackupStorage(storageParams);
                default:
                    throw new InvalidOperationException("Unknown storage type.");
            }
        }

        public static void CheckBackupStorage(StartBackupRequest backupRequest)
        {
            if (backupRequest == null)
            {
                throw new ArgumentNullException("backupRequest");
            }

            if (backupRequest.StorageType != BackupStorageType.Documents &&
                backupRequest.StorageType != BackupStorageType.ThridpartyDocuments)
            {
                return;
            }

            CoreContext.TenantManager.SetCurrentTenant(backupRequest.TenantId);

            using (var folderDao = Web.Files.Classes.Global.DaoFactory.GetFolderDao())
            {
                var folder = folderDao.GetFolder(backupRequest.StorageBasePath);

                if (folder == null)
                {
                    throw new System.IO.DirectoryNotFoundException("Folder not found.");
                }

                if (folder.RootFolderType != Files.Core.FolderType.COMMON)
                {
                    throw new NotSupportedException("Root folder type is not supported.");
                }
            }
        }

        public static IBackupRepository GetBackupRepository()
        {
            return new BackupRepository();
        }
    }
}
