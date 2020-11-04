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


using ASC.Core;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using System;
using System.IO;
using System.Net;
using File = ASC.Files.Core.File;
using IoFile = System.IO.File;

namespace ASC.Data.Backup.Storage
{
    internal class DocumentsBackupStorage : IBackupStorage
    {
        private readonly int tenantId;
        private readonly string webConfigPath;

        public DocumentsBackupStorage(int tenantId, string webConfigPath)
        {
            this.tenantId = tenantId;
            this.webConfigPath = webConfigPath;
        }

        public string Upload(string folderId, string localPath, Guid userId)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            if (!userId.Equals(Guid.Empty))
            {
                SecurityContext.AuthenticateMe(userId);
            }
            else
            {
                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                SecurityContext.AuthenticateMe(tenant.OwnerId);
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
                    var file = fileDao.SaveFile(
                        new File
                            {
                                Title = Path.GetFileName(localPath),
                                FolderID = folder.ID,
                                ContentLength = source.Length
                            },
                        source);

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
                    source.StreamCopyTo(destination);
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

        private IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private IFileDao GetFileDao()
        {
            // hack: create storage using webConfigPath and put it into DataStoreCache
            // FileDao will use this storage and will not try to create the new one from service config
            StorageFactory.GetStorage(webConfigPath, tenantId.ToString(), "files");
            return Global.DaoFactory.GetFileDao();
        }
    }
}
