/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Files.Core;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.SharePoint
{
    internal class SharePointFileDao : SharePointDaoBase, IFileDao
    {
        public SharePointFileDao(SharePointProviderInfo sharePointInfo, SharePointDaoSelector sharePointDaoSelector)
            : base(sharePointInfo, sharePointDaoSelector)
        {
        }

        public void Dispose()
        {
            ProviderInfo.Dispose();
        }

        public void InvalidateCache(object fileId)
        {
            ProviderInfo.InvalidateStorage();
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ProviderInfo.ToFile(ProviderInfo.GetFileById(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ProviderInfo.ToFile(ProviderInfo.GetFolderFiles(parentId).FirstOrDefault(x => x.Name.Contains(title)));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            return fileIds.Select(fileId => ProviderInfo.ToFile(ProviderInfo.GetFileById(fileId))).ToList();
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public Stream GetFileStream(File file, long offset)
        {
            var fileToDownload = ProviderInfo.GetFileById(file.ID);
            if (fileToDownload == null)
                throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);

            var fileStream = ProviderInfo.GetFileStream(fileToDownload.ServerRelativeUrl);

            if (offset > 0)
                fileStream.Seek(offset, SeekOrigin.Begin);

            return fileStream;
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            throw new NotSupportedException();
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            return false;
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");

            if (file.ID != null)
            {
                return ProviderInfo.ToFile(ProviderInfo.CreateFile((string) file.ID, fileStream));
            }

            if (file.FolderID != null)
            {
                var folder = ProviderInfo.GetFolderById(file.FolderID);

                file.Title = GetAvailableTitle(file.Title, folder, IsExist);

                return ProviderInfo.ToFile(ProviderInfo.CreateFile(folder.ServerRelativeUrl + "/" + file.Title, fileStream));
            }

            return null;
        }

        public void DeleteFile(object fileId)
        {
            ProviderInfo.DeleteFile((string)fileId);
        }

        public bool IsExist(string title, object folderId)
        {
            return ProviderInfo.GetFolderFiles(folderId).FirstOrDefault(x => x.Name.Contains(title)) != null;
        }

        public bool IsExist(string title, Microsoft.SharePoint.Client.Folder folder)
        {
            return ProviderInfo.GetFolderFiles(folder.ServerRelativeUrl).FirstOrDefault(x => x.Name.Contains(title)) != null;
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var newFileId = ProviderInfo.MoveFile(fileId, toFolderId);
            UpdatePathInDB(ProviderInfo.MakeId((string)fileId), (string)newFileId);
            return newFileId;
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            return ProviderInfo.ToFile(ProviderInfo.CopyFile(fileId, toFolderId));
        }

        public object FileRename(object fileId, string newTitle)
        {
            var newFileId = ProviderInfo.RenameFile((string)fileId, newTitle);
            UpdatePathInDB(ProviderInfo.MakeId((string)fileId), (string)newFileId);
            return newFileId;
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            return string.Empty;
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
        }

        public bool UseTrashForRemove(File file)
        {
            return false;
        }

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            return new ChunkedUploadSession(FixId(file), contentLength) { UseChunks = false };
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                if (uploadSession.BytesTotal == 0)
                    uploadSession.BytesTotal = chunkLength;

                uploadSession.File = SaveFile(uploadSession.File, chunkStream);
                uploadSession.BytesUploaded = chunkLength;
                return;
            }

            throw new NotImplementedException();
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            //throw new NotImplementedException();
        }

        private File FixId(File file)
        {
            if (file.ID != null)
                file.ID = ProviderInfo.MakeId((string)file.ID);

            if (file.FolderID != null)
                file.FolderID = ProviderInfo.MakeId((string)file.FolderID);

            return file;
        }

        #region Only in TMFileDao

        public IEnumerable<File> Search(string text, FolderType folderType)
        {
            return null;
        }

        public void DeleteFolder(object fileId)
        {
            //Do nothing
        }

        public void DeleteFileStream(object file)
        {
            //Do nothing
        }

        public bool IsExistOnStorage(File file)
        {
            return true;
        }

        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            //Do nothing
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion)
        {
            return null;
        }

        public string GetDifferenceUrl(File file)
        {
            return null;
        }

        #endregion
    }
}
