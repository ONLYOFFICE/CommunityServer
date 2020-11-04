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
using System.IO;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Storage
{
    internal class DataStoreBackupStorage : IBackupStorage
    {
        private readonly string webConfigPath;
        private readonly int _tenant;

        public DataStoreBackupStorage(int tenant, string webConfigPath)
        {
            this.webConfigPath = webConfigPath;
            _tenant = tenant;
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            using (var stream = File.OpenRead(localPath))
            {
                var storagePath = Path.GetFileName(localPath);
                GetDataStore().Save("", storagePath, stream);
                return storagePath;
            }
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            using (var source = GetDataStore().GetReadStream("", storagePath))
            using (var destination = File.OpenWrite(targetLocalPath))
            {
                source.StreamCopyTo(destination);
            }
        }

        public void Delete(string storagePath)
        {
            var dataStore = GetDataStore();
            if (dataStore.IsFile("", storagePath))
            {
                dataStore.Delete("", storagePath);   
            }
        }

        public bool IsExists(string storagePath)
        {
            return GetDataStore().IsFile("", storagePath);
        }

        public string GetPublicLink(string storagePath)
        {
            return GetDataStore().GetPreSignedUri("", storagePath, TimeSpan.FromDays(1), null).ToString();
        }

        protected virtual IDataStore GetDataStore()
        {
            return StorageFactory.GetStorage(webConfigPath, _tenant.ToString(), "backup", null);
        }
    }
}
