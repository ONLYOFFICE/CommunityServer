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
using System.Net;

using ASC.FederatedLogin;

using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Models;

using BoxSDK = Box.V2;

namespace ASC.Files.Thirdparty.Box
{
    internal class BoxStorage
    {
        private OAuth20Token _token;

        private BoxClient _boxClient;

        private readonly List<string> _boxFields = new List<string> { "created_at", "modified_at", "name", "parent", "size" };

        public bool IsOpened { get; private set; }

        public long MaxChunkedUploadFileSize = 250L * 1024L * 1024L;

        public void Open(OAuth20Token token)
        {
            if (IsOpened)
                return;

            _token = token;

            var config = new BoxConfig(_token.ClientID, _token.ClientSecret, new Uri(_token.RedirectUri));
            var session = new OAuthSession(_token.AccessToken, _token.RefreshToken, (int)_token.ExpiresIn, "bearer");
            _boxClient = new BoxClient(config, session);

            IsOpened = true;
        }

        public void Close()
        {
            IsOpened = false;
        }


        public string GetRootFolderId()
        {
            var root = GetFolder("0");

            return root.Id;
        }

        public BoxFolder GetFolder(string folderId)
        {
            try
            {
                return _boxClient.FoldersManager.GetInformationAsync(folderId, _boxFields).Result;
            }
            catch (Exception ex)
            {
                var boxException = ex.InnerException as BoxSDK.Exceptions.BoxException;
                if (boxException != null && boxException.Error.Status == ((int)HttpStatusCode.NotFound).ToString())
                {
                    return null;
                }
                throw;
            }
        }

        public BoxFile GetFile(string fileId)
        {
            try
            {
                return _boxClient.FilesManager.GetInformationAsync(fileId, _boxFields).Result;
            }
            catch (Exception ex)
            {
                var boxException = ex.InnerException as BoxSDK.Exceptions.BoxException;
                if (boxException != null && boxException.Error.Status == ((int)HttpStatusCode.NotFound).ToString())
                {
                    return null;
                }
                throw;
            }
        }

        public List<BoxItem> GetItems(string folderId, int limit = 500)
        {
            return _boxClient.FoldersManager.GetFolderItemsAsync(folderId, limit, 0, _boxFields).Result.Entries;
        }

        public Stream DownloadStream(BoxFile file, int offset = 0)
        {
            if (file == null) throw new ArgumentNullException("file");

            if (offset > 0 && file.Size.HasValue)
            {
                return _boxClient.FilesManager.DownloadAsync(file.Id, startOffsetInBytes: offset, endOffsetInBytes: (int)file.Size - 1).Result;
            }

            var str = _boxClient.FilesManager.DownloadAsync(file.Id).Result;
            if (offset == 0)
            {
                return str;
            }

            var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
            if (str != null)
            {
                str.CopyTo(tempBuffer);
                tempBuffer.Flush();
                tempBuffer.Seek(offset, SeekOrigin.Begin);

                str.Dispose();
            }

            return tempBuffer;
        }

        public BoxFolder CreateFolder(string title, string parentId)
        {
            var boxFolderRequest = new BoxFolderRequest
            {
                Name = title,
                Parent = new BoxRequestEntity
                {
                    Id = parentId
                }
            };
            return _boxClient.FoldersManager.CreateAsync(boxFolderRequest, _boxFields).Result;
        }

        public BoxFile CreateFile(Stream fileStream, string title, string parentId)
        {
            var boxFileRequest = new BoxFileRequest
            {
                Name = title,
                Parent = new BoxRequestEntity
                {
                    Id = parentId
                }
            };
            return _boxClient.FilesManager.UploadAsync(boxFileRequest, fileStream, _boxFields, setStreamPositionToZero: false).Result;
        }

        public void DeleteItem(BoxItem boxItem)
        {
            if (boxItem is BoxFolder)
            {
                _boxClient.FoldersManager.DeleteAsync(boxItem.Id, true).Wait();
            }
            else
            {
                _boxClient.FilesManager.DeleteAsync(boxItem.Id).Wait();
            }
        }

        public BoxFolder MoveFolder(string boxFolderId, string newFolderName, string toFolderId)
        {
            var boxFolderRequest = new BoxFolderRequest
            {
                Id = boxFolderId,
                Name = newFolderName,
                Parent = new BoxRequestEntity
                {
                    Id = toFolderId
                }
            };
            return _boxClient.FoldersManager.UpdateInformationAsync(boxFolderRequest, _boxFields).Result;
        }

        public BoxFile MoveFile(string boxFileId, string newFileName, string toFolderId)
        {
            var boxFileRequest = new BoxFileRequest
            {
                Id = boxFileId,
                Name = newFileName,
                Parent = new BoxRequestEntity
                {
                    Id = toFolderId
                }
            };
            return _boxClient.FilesManager.UpdateInformationAsync(boxFileRequest, null, _boxFields).Result;
        }

        public BoxFolder CopyFolder(string boxFolderId, string newFolderName, string toFolderId)
        {
            var boxFolderRequest = new BoxFolderRequest
            {
                Id = boxFolderId,
                Name = newFolderName,
                Parent = new BoxRequestEntity
                {
                    Id = toFolderId
                }
            };
            return _boxClient.FoldersManager.CopyAsync(boxFolderRequest, _boxFields).Result;
        }

        public BoxFile CopyFile(string boxFileId, string newFileName, string toFolderId)
        {
            var boxFileRequest = new BoxFileRequest
            {
                Id = boxFileId,
                Name = newFileName,
                Parent = new BoxRequestEntity
                {
                    Id = toFolderId
                }
            };
            return _boxClient.FilesManager.CopyAsync(boxFileRequest, _boxFields).Result;
        }

        public BoxFolder RenameFolder(string boxFolderId, string newName)
        {
            var boxFolderRequest = new BoxFolderRequest { Id = boxFolderId, Name = newName };
            return _boxClient.FoldersManager.UpdateInformationAsync(boxFolderRequest, _boxFields).Result;
        }

        public BoxFile RenameFile(string boxFileId, string newName)
        {
            var boxFileRequest = new BoxFileRequest { Id = boxFileId, Name = newName };
            return _boxClient.FilesManager.UpdateInformationAsync(boxFileRequest, null, _boxFields).Result;
        }

        public BoxFile SaveStream(string fileId, Stream fileStream)
        {
            return _boxClient.FilesManager.UploadNewVersionAsync(null, fileId, fileStream, fields: _boxFields, setStreamPositionToZero: false).Result;
        }

        public long GetMaxUploadSize()
        {
            var boxUser = _boxClient.UsersManager.GetCurrentUserInformationAsync(new List<string>() { "max_upload_size" }).Result;
            var max = boxUser.MaxUploadSize.HasValue ? boxUser.MaxUploadSize.Value : MaxChunkedUploadFileSize;

            //todo: without chunked uploader:
            return Math.Min(max, MaxChunkedUploadFileSize);
        }
    }
}