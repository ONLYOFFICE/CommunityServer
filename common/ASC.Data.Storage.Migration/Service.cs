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


using ASC.Common.Logging;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage.Migration
{
    class Service : IService
    {
        public void Migrate(int tenantId, StorageSettings newStorageSettings)
        {
            StorageUploader.Start(tenantId, newStorageSettings);
        }

        public void UploadCdn(int tenantId, string relativePath, string mappedPath, CdnStorageSettings cdnStorageSettings = null)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);

            if (cdnStorageSettings != null)
            {
                cdnStorageSettings.Save();
            }

            StaticUploader.UploadDir(relativePath, mappedPath);
            LogManager.GetLogger("ASC").DebugFormat("UploadDir {0}", mappedPath);
        }

        public double GetProgress(int tenantId)
        {
            var progress = (ProgressBase)StorageUploader.GetProgress(tenantId) ?? StaticUploader.GetProgress(tenantId);

            return progress != null ? progress.Percentage : -1;
        }

        public void StopMigrate()
        {
            StorageUploader.Stop();
        }
    }
}