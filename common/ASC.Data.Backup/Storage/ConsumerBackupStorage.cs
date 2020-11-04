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
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Backup.Storage
{
    internal class ConsumerBackupStorage : IBackupStorage
    {
        private readonly IDataStore store;
        private const string Domain = "backup";

        public ConsumerBackupStorage(IReadOnlyDictionary<string, string> storageParams)
        {
            var settings = new StorageSettings { Module = storageParams["module"], Props = storageParams.Where(r => r.Key != "module").ToDictionary(r => r.Key, r => r.Value) };
            store = settings.DataStore;
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            using (var stream = File.OpenRead(localPath))
            {
                var storagePath = Path.GetFileName(localPath);
                store.Save(Domain, storagePath, stream, ACL.Private);
                return storagePath;
            }
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            using (var source = store.GetReadStream(Domain, storagePath))
            using (var destination = File.OpenWrite(targetLocalPath))
            {
                source.CopyTo(destination);
            }
        }

        public void Delete(string storagePath)
        {
            if (store.IsFile(Domain, storagePath))
            {
                store.Delete(Domain, storagePath);
            }
        }

        public bool IsExists(string storagePath)
        {
            return store.IsFile(Domain, storagePath);
        }

        public string GetPublicLink(string storagePath)
        {
            return store.GetInternalUri(Domain, storagePath, TimeSpan.FromDays(1), null).AbsoluteUri;
        }
    }
}
