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
using System.IO;

using ASC.Core;
using ASC.Core.ChunkedUploader;
using ASC.Data.Storage;
using ASC.Data.Storage.ZipOperators;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;

using File = ASC.Files.Core.File;
using IoFile = System.IO.File;

namespace ASC.Data.Backup.Storage
{
    internal class DocumentsBackupStorage : IBackupStorage, IGetterWriteOperator
    {
        private readonly int tenantId;
        private readonly FilesChunkedUploadSessionHolder _sessionHolder;

        public DocumentsBackupStorage(int tenantId, string webConfigPath)
        {
            this.tenantId = tenantId;

            // hack: create storage using webConfigPath and put it into DataStoreCache
            // FileDao will use this storage and will not try to create the new one from service config
            var store = StorageFactory.GetStorage(webConfigPath, tenantId.ToString(), "files");

            _sessionHolder = new FilesChunkedUploadSessionHolder(store, "", SetupInfo.ChunkUploadSize);
        }

        public string Upload(string folderId, string localPath, Guid userId)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            if (!userId.Equals(Guid.Empty))
            {
                SecurityContext.CurrentUser = userId;
            }
            else
            {
                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                SecurityContext.CurrentUser = tenant.OwnerId;
            }

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var folder = folderDao.GetFolder(folderId);
                if (folder == null)
                {
                    throw new FileNotFoundException("Folder not found.");
                }

                using (var source = IoFile.OpenRead(localPath))
                {
                    var newFile = new File
                    {
                        Title = Path.GetFileName(localPath),
                        FolderID = folder.ID,
                        ContentLength = source.Length
                    };
                    File file = null;
                    var buffer = new byte[SetupInfo.ChunkUploadSize];
                    var chunkedUploadSession = fileDao.CreateUploadSession(newFile, source.Length);
                    chunkedUploadSession.CheckQuota = false;

                    var bytesRead = 0;

                    while ((bytesRead = source.Read(buffer, 0, (int)SetupInfo.ChunkUploadSize)) > 0)
                    {
                        using (var theMemStream = new MemoryStream())
                        {
                            theMemStream.Write(buffer, 0, bytesRead);
                            theMemStream.Position = 0;
                            file = fileDao.UploadChunk(chunkedUploadSession, theMemStream, bytesRead);
                        }
                    }

                    return Convert.ToString(file.ID);
                }
            }
        }

        public void Download(string fileId, string targetLocalPath)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                if (file == null)
                {
                    throw new FileNotFoundException("File not found.");
                }
                using (var source = fileDao.GetFileStream(file))
                using (var destination = IoFile.OpenWrite(targetLocalPath))
                {
                    source.CopyTo(destination);
                }
            }
        }

        public void Delete(string fileId)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            using (var fileDao = GetFileDao())
            {
                fileDao.DeleteFile(fileId);
            }
        }

        public bool IsExists(string fileId)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            using (var fileDao = GetFileDao())
            {
                try
                {

                    var file = fileDao.GetFile(fileId);
                    return file != null && file.RootFolderType != FolderType.TRASH;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public string GetPublicLink(string fileId)
        {
            return string.Empty;
        }

        public IDataWriteOperator GetWriteOperator(string storageBasePath, string title, Guid userId)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            if (!userId.Equals(Guid.Empty))
            {
                SecurityContext.CurrentUser = userId;
            }
            else
            {
                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                SecurityContext.CurrentUser = tenant.OwnerId;
            }
            using (var folderDao = GetFolderDao())
            {
                return folderDao.CreateDataWriteOperator(storageBasePath, InitUploadChunk(storageBasePath, title), _sessionHolder);
            }
        }

        private CommonChunkedUploadSession InitUploadChunk(string folderId, string title)
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var folder = folderDao.GetFolder(folderId);
                var newFile = new File
                {
                    Title = title,
                    FolderID = folder.ID
                };
                var chunkedUploadSession = fileDao.CreateUploadSession(newFile, -1);
                chunkedUploadSession.CheckQuota = false;
                return chunkedUploadSession;
            }
        }

        private IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private IFileDao GetFileDao()
        {
            return Global.DaoFactory.GetFileDao();
        }
    }
}
