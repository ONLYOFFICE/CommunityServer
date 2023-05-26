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
using System.IO;
using System.Linq;

using ASC.Core.ChunkedUploader;
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.ZipOperators;
using ASC.Web.Studio.Core;

namespace ASC.Data.Backup.Storage
{
    internal class ConsumerBackupStorage : IBackupStorage, IGetterWriteOperator
    {
        private readonly IDataStore _store;
        private readonly bool _isTemporary;
        private readonly CommonChunkedUploadSessionHolder _sessionHolder;

        private string Domain { get => _isTemporary ? "" : "backup"; }

        public ConsumerBackupStorage(IReadOnlyDictionary<string, string> storageParams)
        {
            var settings = new StorageSettings { Module = storageParams["module"], Props = storageParams.Where(r => r.Key != "module").ToDictionary(r => r.Key, r => r.Value) };
            _store = settings.DataStore;
            _sessionHolder = new CommonChunkedUploadSessionHolder(_store, Domain, SetupInfo.ChunkUploadSize);
        }

        public ConsumerBackupStorage(int tenant, string webConfigPath)
        {
            _store = StorageFactory.GetStorage(webConfigPath, tenant.ToString(), Domain, null);
            _isTemporary = true;
            _sessionHolder = new CommonChunkedUploadSessionHolder(_store, Domain, SetupInfo.ChunkUploadSize);
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            using (var stream = System.IO.File.OpenRead(localPath))
            {
                var storagePath = Path.GetFileName(localPath);
                _store.Save(Domain, storagePath, stream, ACL.Private);
                return storagePath;
            }
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            using (var source = _store.GetReadStream(Domain, storagePath))
            using (var destination = System.IO.File.OpenWrite(targetLocalPath))
            {
                source.CopyTo(destination);
            }
        }

        public void Delete(string storagePath)
        {
            if (_store.IsFile(Domain, storagePath))
            {
                _store.Delete(Domain, storagePath);
            }
        }

        public bool IsExists(string storagePath)
        {
            return _store.IsFile(Domain, storagePath);
        }

        public string GetPublicLink(string storagePath)
        {   
            if (_isTemporary)
            {
                return _store.GetPreSignedUri(Domain, storagePath, TimeSpan.FromDays(1), null).ToString();
            }
            else
            {
                return _store.GetInternalUri(Domain, storagePath, TimeSpan.FromDays(1), null).AbsoluteUri;
            }
        }


        public IDataWriteOperator GetWriteOperator(string storageBasePath, string title, Guid userId)
        {
            var session = new CommonChunkedUploadSession(-1)
            {
                TempPath = title,
                UploadId = _store.InitiateChunkedUpload(Domain, title)
            };

            return _store.CreateDataWriteOperator(session, _sessionHolder);
        }
    }
}
