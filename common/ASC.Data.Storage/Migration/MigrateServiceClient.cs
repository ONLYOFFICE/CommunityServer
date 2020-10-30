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


using ASC.Common.Module;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage.Migration
{
    public class ServiceClient : BaseWcfClient<IService>, IService
    {
        public void Migrate(int tenant, StorageSettings storageSettings)
        {
            Channel.Migrate(tenant, storageSettings);
        }

        public void UploadCdn(int tenantId, string relativePath, string mappedPath, CdnStorageSettings settings = null)
        {
            Channel.UploadCdn(tenantId, relativePath, mappedPath, settings);
        }

        public double GetProgress(int tenant)
        {
            return Channel.GetProgress(tenant);
        }

        public void StopMigrate()
        {
            Channel.StopMigrate();
        }
    }
}
