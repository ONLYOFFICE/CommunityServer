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
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
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
            return ProviderInfo.ToFile(ProviderInfo.GetFolderFiles(parentId).FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public File GetFileStable(object fileId, int fileVersion)
        {
            return ProviderInfo.ToFile(ProviderInfo.GetFileById(fileId));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            return fileIds.Select(fileId => ProviderInfo.ToFile(ProviderInfo.GetFileById(fileId))).ToList();
        }

        public List<File> GetFilesFiltered(object[] fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
        {
            if (fileIds == null || fileIds.Length == 0 || filterType == FilterType.FoldersOnly) return new List<File>();

            var files = GetFiles(fileIds).AsEnumerable();

            //Filter
            if (subjectID != Guid.Empty)
            {
                files = files.Where(x => subjectGroup
                                             ? CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID)
                                             : x.CreateBy == subjectID);
            }

            switch (filterType)
            {
                case FilterType.FoldersOnly:
                    return new List<File>();
                case FilterType.DocumentsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Document);
                    break;
                case FilterType.PresentationsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Presentation);
                    break;
                case FilterType.SpreadsheetsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Spreadsheet);
                    break;
                case FilterType.ImagesOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Image);
                    break;
                case FilterType.ArchiveOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Archive);
                    break;
                case FilterType.MediaOnly:
                    files = files.Where(x =>
                        {
                            FileType fileType;
                            return (fileType = FileUtility.GetFileTypeByFileName(x.Title)) == FileType.Audio || fileType == FileType.Video;
                        });
                    break;
                case FilterType.ByExtension:
                    if (!string.IsNullOrEmpty(searchText))
                        files = files.Where(x => FileUtility.GetFileExtension(x.Title).Contains(searchText));
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                files = files.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1);

            return files.ToList();
        }

        public List<object> GetFiles(object parentId)
        {
            return ProviderInfo.GetFolderFiles(parentId).Select(r => ProviderInfo.ToFile(r).ID).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = ProviderInfo.GetFolderFiles(parentId).Select(r => ProviderInfo.ToFile(r));

            //Filter
            if (subjectID != Guid.Empty)
            {
                files = files.Where(x => subjectGroup
                                             ? CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID)
                                             : x.CreateBy == subjectID);
            }

            switch (filterType)
            {
                case FilterType.FoldersOnly:
                    return new List<File>();
                case FilterType.DocumentsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Document).ToList();
                    break;
                case FilterType.PresentationsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Presentation).ToList();
                    break;
                case FilterType.SpreadsheetsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Spreadsheet).ToList();
                    break;
                case FilterType.ImagesOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Image).ToList();
                    break;
                case FilterType.ArchiveOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Archive).ToList();
                    break;
                case FilterType.MediaOnly:
                    files = files.Where(x =>
                    {
                        FileType fileType;
                        return (fileType = FileUtility.GetFileTypeByFileName(x.Title)) == FileType.Audio || fileType == FileType.Video;
                    });
                    break;
                case FilterType.ByExtension:
                    if (!string.IsNullOrEmpty(searchText))
                        files = files.Where(x => FileUtility.GetFileExtension(x.Title).Contains(searchText));
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                files = files.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1).ToList();

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.CreateBy) : files.OrderByDescending(x => x.CreateBy);
                    break;
                case SortedByType.AZ:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.Title) : files.OrderByDescending(x => x.Title);
                    break;
                case SortedByType.DateAndTime:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.ModifiedOn) : files.OrderByDescending(x => x.ModifiedOn);
                    break;
                case SortedByType.DateAndTimeCreation:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.CreateOn) : files.OrderByDescending(x => x.CreateOn);
                    break;
                default:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.Title) : files.OrderByDescending(x => x.Title);
                    break;
            }

            return files.ToList();
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

            var fileStream = ProviderInfo.GetFileStream(fileToDownload.ServerRelativeUrl, (int) offset);

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
                var sharePointFile = ProviderInfo.CreateFile((string)file.ID, fileStream);

                var resultFile = ProviderInfo.ToFile(sharePointFile);
                if (!sharePointFile.Name.Equals(file.Title))
                {
                    var folder = ProviderInfo.GetFolderById(file.FolderID);
                    file.Title = GetAvailableTitle(file.Title, folder, IsExist);

                    var id = ProviderInfo.RenameFile(SharePointDaoSelector.ConvertId(resultFile.ID).ToString(), file.Title);
                    return GetFile(SharePointDaoSelector.ConvertId(id));
                }
                return resultFile;
            }

            if (file.FolderID != null)
            {
                var folder = ProviderInfo.GetFolderById(file.FolderID);
                file.Title = GetAvailableTitle(file.Title, folder, IsExist);
                return ProviderInfo.ToFile(ProviderInfo.CreateFile(folder.ServerRelativeUrl + "/" + file.Title, fileStream));
            }

            return null;
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream);
        }

        public void DeleteFile(object fileId)
        {
            ProviderInfo.DeleteFile((string)fileId);
        }

        public bool IsExist(string title, object folderId)
        {
            return ProviderInfo.GetFolderFiles(folderId)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsExist(string title, Microsoft.SharePoint.Client.Folder folder)
        {
            return ProviderInfo.GetFolderFiles(folder.ServerRelativeUrl)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
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

        public object FileRename(File file, string newTitle)
        {
            var newFileId = ProviderInfo.RenameFile((string) file.ID, newTitle);
            UpdatePathInDB(ProviderInfo.MakeId((string) file.ID), (string) newFileId);
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

        public void ReassignFiles(object[] fileIds, Guid newOwnerId)
        {
        }

        public List<File> GetFiles(object[] parentIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
        {
            return new List<File>();
        }

        public IEnumerable<File> Search(string text, bool bunch)
        {
            return null;
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

        public Stream GetDifferenceStream(File file)
        {
            return null;
        }

        public bool ContainChanges(object fileId, int fileVersion)
        {
            return false;
        }

        #endregion
    }
}
