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
using ASC.FederatedLogin;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal class DropboxStorage
    {
        private OAuth20Token _token;

        private DropboxClient _dropboxClient;

        public bool IsOpened { get; private set; }

        public long MaxChunkedUploadFileSize = 20L*1024L*1024L*1024L;

        public void Open(OAuth20Token token)
        {
            if (IsOpened)
                return;

            _token = token;

            _dropboxClient = new DropboxClient(_token.AccessToken);

            IsOpened = true;
        }

        public void Close()
        {
            _dropboxClient.Dispose();

            IsOpened = false;
        }



        public string MakeDropboxPath(string parentPath, string name)
        {
            return (parentPath ?? "") + "/" + (name ?? "");
        }

        public long GetUsedSpace()
        {
            return (long)_dropboxClient.Users.GetSpaceUsageAsync().Result.Used;
        }

        public FolderMetadata GetFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || folderPath == "/")
            {
                return new FolderMetadata(string.Empty, "/");
            }
            try
            {
                return _dropboxClient.Files.GetMetadataAsync(folderPath).Result.AsFolder;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ApiException<GetMetadataError>
                    && ex.InnerException.Message.StartsWith("path/not_found/"))
                {
                    return null;
                }
                throw;
            }
        }

        public FileMetadata GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath == "/")
            {
                return null;
            }
            try
            {
                return _dropboxClient.Files.GetMetadataAsync(filePath).Result.AsFile;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ApiException<GetMetadataError>
                    && ex.InnerException.Message.StartsWith("path/not_found/"))
                {
                    return null;
                }
                throw;
            }
        }

        public List<Metadata> GetItems(string folderPath)
        {
            return new List<Metadata>(_dropboxClient.Files.ListFolderAsync(folderPath).Result.Entries);
        }

        public Stream DownloadStream(string filePath, int offset = 0)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("file");

            using (var response = _dropboxClient.Files.DownloadAsync(filePath).Result)
            {
                var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
                using (var str = response.GetContentAsStreamAsync().Result)
                {
                    if (str != null)
                    {
                        str.CopyTo(tempBuffer);
                        tempBuffer.Flush();
                        tempBuffer.Seek(offset, SeekOrigin.Begin);
                    }
                }
                return tempBuffer;
            }
        }

        public FolderMetadata CreateFolder(string title, string parentPath)
        {
            var path = MakeDropboxPath(parentPath, title);
            var result = _dropboxClient.Files.CreateFolderV2Async(path, true).Result;
            return result.Metadata;
        }

        public FileMetadata CreateFile(Stream fileStream, string title, string parentPath)
        {
            var path = MakeDropboxPath(parentPath, title);
            return _dropboxClient.Files.UploadAsync(path, WriteMode.Add.Instance, true, body: fileStream).Result;
        }

        public void DeleteItem(Metadata dropboxItem)
        {
            _dropboxClient.Files.DeleteV2Async(dropboxItem.PathDisplay).Wait();
        }

        public FolderMetadata MoveFolder(string dropboxFolderPath, string dropboxFolderPathTo, string folderName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, folderName);
            var result = _dropboxClient.Files.MoveV2Async(dropboxFolderPath, pathTo, autorename: true).Result;
            return (FolderMetadata)result.Metadata;
        }

        public FileMetadata MoveFile(string dropboxFilePath, string dropboxFolderPathTo, string fileName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, fileName);
            var result = _dropboxClient.Files.MoveV2Async(dropboxFilePath, pathTo, autorename: true).Result;
            return (FileMetadata)result.Metadata;
        }

        public FolderMetadata CopyFolder(string dropboxFolderPath, string dropboxFolderPathTo, string folderName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, folderName);
            var result = _dropboxClient.Files.CopyV2Async(dropboxFolderPath, pathTo, autorename: true).Result;
            return (FolderMetadata)result.Metadata;
        }

        public FileMetadata CopyFile(string dropboxFilePath, string dropboxFolderPathTo, string fileName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, fileName);
            var result = _dropboxClient.Files.CopyV2Async(dropboxFilePath, pathTo, autorename: true).Result;
            return (FileMetadata)result.Metadata;
        }

        public FileMetadata SaveStream(string filePath, Stream fileStream)
        {
            return _dropboxClient.Files.UploadAsync(filePath, WriteMode.Overwrite.Instance, body: fileStream).Result.AsFile;
        }

        public string CreateResumableSession()
        {
            return _dropboxClient.Files.UploadSessionStartAsync(body: new MemoryStream()).Result.SessionId;
        }

        public void Transfer(string dropboxSession, long offset, Stream stream)
        {
            _dropboxClient.Files.UploadSessionAppendV2Async(new UploadSessionCursor(dropboxSession, (ulong)offset), body: stream).Wait();
        }

        public Metadata FinishResumableSession(string dropboxSession, string dropboxFolderPath, string fileName, long offset)
        {
            var dropboxFilePath = MakeDropboxPath(dropboxFolderPath, fileName);
            return FinishResumableSession(dropboxSession, dropboxFilePath, offset);
        }

        public Metadata FinishResumableSession(string dropboxSession, string dropboxFilePath, long offset)
        {
            return _dropboxClient.Files.UploadSessionFinishAsync(
                new UploadSessionCursor(dropboxSession, (ulong)offset),
                new CommitInfo(dropboxFilePath, WriteMode.Overwrite.Instance),
                new MemoryStream()).Result;
        }
    }
}