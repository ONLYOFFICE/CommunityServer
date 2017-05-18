/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.FederatedLogin;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal class DropboxStorage
    {
        private OAuth20Token _token;

        private DropboxClient _dropboxClient;

        public bool IsOpened { get; private set; }

        public long MaxUploadFileSize { get; internal set; }

        public long MaxChunkedUploadFileSize { get; internal set; }

        public DropboxStorage()
        {
            MaxUploadFileSize = MaxChunkedUploadFileSize = 20L*1024L*1024L*1024L;
        }

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
            return (long) _dropboxClient.Users.GetSpaceUsageAsync().Result.Used;
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

        public Stream DownloadStream(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("file");

            return _dropboxClient.Files.DownloadAsync(filePath).Result.GetContentAsStreamAsync().Result;
        }

        public FolderMetadata CreateFolder(string title, string parentPath)
        {
            var path = MakeDropboxPath(parentPath, title);
            return _dropboxClient.Files.CreateFolderAsync(path).Result;
        }

        public FileMetadata CreateFile(Stream fileStream, string title, string parentPath)
        {
            var path = MakeDropboxPath(parentPath, title);
            return _dropboxClient.Files.UploadAsync(path, WriteMode.Add.Instance, true, body: fileStream).Result;
        }

        public void DeleteItem(Metadata dropboxItem)
        {
            _dropboxClient.Files.DeleteAsync(dropboxItem.PathDisplay).Wait();
        }

        public FolderMetadata MoveFolder(string dropboxFolderPath, string dropboxFolderPathTo, string folderName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, folderName);
            return _dropboxClient.Files.MoveAsync(dropboxFolderPath, pathTo).Result.AsFolder;
        }

        public FileMetadata MoveFile(string dropboxFilePath, string dropboxFolderPathTo, string fileName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, fileName);
            return _dropboxClient.Files.MoveAsync(dropboxFilePath, pathTo).Result.AsFile;
        }

        public FolderMetadata CopyFolder(string dropboxFolderPath, string dropboxFolderPathTo, string folderName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, folderName);
            return _dropboxClient.Files.CopyAsync(dropboxFolderPath, pathTo).Result.AsFolder;
        }

        public FileMetadata CopyFile(string dropboxFilePath, string dropboxFolderPathTo, string fileName)
        {
            var pathTo = MakeDropboxPath(dropboxFolderPathTo, fileName);
            return _dropboxClient.Files.CopyAsync(dropboxFilePath, pathTo).Result.AsFile;
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
            _dropboxClient.Files.UploadSessionAppendV2Async(new UploadSessionCursor(dropboxSession, (ulong) offset), body: stream).Wait();
        }

        public Metadata FinishResumableSession(string dropboxSession, string dropboxFolderPath, string fileName, long offset)
        {
            var dropboxFilePath = MakeDropboxPath(dropboxFolderPath, fileName);
            return FinishResumableSession(dropboxSession, dropboxFilePath, offset);
        }

        public Metadata FinishResumableSession(string dropboxSession, string dropboxFilePath, long offset)
        {
            return _dropboxClient.Files.UploadSessionFinishAsync(
                new UploadSessionCursor(dropboxSession, (ulong) offset),
                new CommitInfo(dropboxFilePath, WriteMode.Overwrite.Instance),
                new MemoryStream()).Result;
        }
    }
}