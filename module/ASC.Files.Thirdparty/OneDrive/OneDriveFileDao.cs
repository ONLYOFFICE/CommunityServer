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
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;
using Microsoft.OneDrive.Sdk;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.OneDrive
{
    internal class OneDriveFileDao : OneDriveDaoBase, IFileDao
    {
        public OneDriveFileDao(OneDriveDaoSelector.OneDriveInfo providerInfo, OneDriveDaoSelector onedriveDaoSelector)
            : base(providerInfo, onedriveDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            var onedriveFileId = MakeOneDriveId(fileId);
            OneDriveProviderInfo.CacheReset(onedriveFileId);

            var onedriveFile = GetOneDriveItem(fileId);
            var parentId = GetParentFolderId(onedriveFile);
            if (parentId != null) OneDriveProviderInfo.CacheReset(parentId);
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetOneDriveItem(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ToFile(GetOneDriveItems(parentId, false)
                              .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase) && item.File != null));
        }

        public File GetFileStable(object fileId, int fileVersion)
        {
            return ToFile(GetOneDriveItem(fileId));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            if (fileIds == null || fileIds.Length == 0) return new List<File>();
            return fileIds.Select(GetOneDriveItem).Select(ToFile).ToList();
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
            return GetOneDriveItems(parentId, false).Select(entry => (object)MakeId(entry.Id)).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = GetOneDriveItems(parentId, false).Select(ToFile);

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
            var onedriveFileId = MakeOneDriveId(file.ID);
            OneDriveProviderInfo.CacheReset(onedriveFileId);

            var onedriveFile = GetOneDriveItem(file.ID);
            if (onedriveFile == null) throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);
            if (onedriveFile is ErrorItem) throw new Exception(((ErrorItem)onedriveFile).Error);

            var fileStream = OneDriveProviderInfo.Storage.DownloadStream(onedriveFile, (int)offset);

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
            if (file == null) throw new ArgumentNullException("file");
            if (fileStream == null) throw new ArgumentNullException("fileStream");

            Item newOneDriveFile = null;

            if (file.ID != null)
            {
                newOneDriveFile = OneDriveProviderInfo.Storage.SaveStream(MakeOneDriveId(file.ID), fileStream);
                if (!newOneDriveFile.Name.Equals(file.Title))
                {
                    file.Title = GetAvailableTitle(file.Title, GetParentFolderId(newOneDriveFile), IsExist);
                    newOneDriveFile = OneDriveProviderInfo.Storage.RenameItem(newOneDriveFile.Id, file.Title);
                }
            }
            else if (file.FolderID != null)
            {
                var folderId = MakeOneDriveId(file.FolderID);
                var folder = GetOneDriveItem(folderId);
                file.Title = GetAvailableTitle(file.Title, folderId, IsExist);
                newOneDriveFile = OneDriveProviderInfo.Storage.CreateFile(fileStream, file.Title, MakeOneDrivePath(folder));
            }

            if (newOneDriveFile != null) OneDriveProviderInfo.CacheReset(newOneDriveFile.Id);
            var parentId = GetParentFolderId(newOneDriveFile);
            if (parentId != null) OneDriveProviderInfo.CacheReset(parentId);

            return ToFile(newOneDriveFile);
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream);
        }

        public void DeleteFile(object fileId)
        {
            var onedriveFile = GetOneDriveItem(fileId);
            if (onedriveFile == null) return;
            var id = MakeId(onedriveFile.Id);

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var hashIDs = db.ExecuteList(Query("files_thirdparty_id_mapping")
                                                 .Select("hash_id")
                                                 .Where(Exp.Like("id", id, SqlLike.StartWith)))
                                .ConvertAll(x => x[0]);

                db.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                db.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                db.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                db.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                tx.Commit();
            }

            if (!(onedriveFile is ErrorItem))
                OneDriveProviderInfo.Storage.DeleteItem(onedriveFile);

            OneDriveProviderInfo.CacheReset(onedriveFile.Id);
            var parentFolderId = GetParentFolderId(onedriveFile);
            if (parentFolderId != null) OneDriveProviderInfo.CacheReset(parentFolderId);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetOneDriveItems(folderId, false)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var onedriveFile = GetOneDriveItem(fileId);
            if (onedriveFile is ErrorItem) throw new Exception(((ErrorItem)onedriveFile).Error);

            var toOneDriveFolder = GetOneDriveItem(toFolderId);
            if (toOneDriveFolder is ErrorItem) throw new Exception(((ErrorItem)toOneDriveFolder).Error);

            var fromFolderId = GetParentFolderId(onedriveFile);

            var newTitle = GetAvailableTitle(onedriveFile.Name, toOneDriveFolder.Id, IsExist);
            onedriveFile = OneDriveProviderInfo.Storage.MoveItem(onedriveFile.Id, newTitle, toOneDriveFolder.Id);

            OneDriveProviderInfo.CacheReset(onedriveFile.Id);
            OneDriveProviderInfo.CacheReset(fromFolderId);
            OneDriveProviderInfo.CacheReset(toOneDriveFolder.Id);

            return MakeId(onedriveFile.Id);
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var onedriveFile = GetOneDriveItem(fileId);
            if (onedriveFile is ErrorItem) throw new Exception(((ErrorItem)onedriveFile).Error);

            var toOneDriveFolder = GetOneDriveItem(toFolderId);
            if (toOneDriveFolder is ErrorItem) throw new Exception(((ErrorItem)toOneDriveFolder).Error);

            var newTitle = GetAvailableTitle(onedriveFile.Name, toOneDriveFolder.Id, IsExist);
            var newOneDriveFile = OneDriveProviderInfo.Storage.CopyItem(onedriveFile.Id, newTitle, toOneDriveFolder.Id);

            OneDriveProviderInfo.CacheReset(newOneDriveFile.Id);
            OneDriveProviderInfo.CacheReset(toOneDriveFolder.Id);

            return ToFile(newOneDriveFile);
        }

        public object FileRename(File file, string newTitle)
        {
            var onedriveFile = GetOneDriveItem(file.ID);
            newTitle = GetAvailableTitle(newTitle, GetParentFolderId(onedriveFile), IsExist);

            onedriveFile = OneDriveProviderInfo.Storage.RenameItem(onedriveFile.Id, newTitle);

            OneDriveProviderInfo.CacheReset(onedriveFile.Id);
            var parentId = GetParentFolderId(onedriveFile);
            if (parentId != null) OneDriveProviderInfo.CacheReset(parentId);

            return MakeId(onedriveFile.Id);
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

        #region chunking

        private File RestoreIds(File file)
        {
            if (file == null) return null;

            if (file.ID != null)
                file.ID = MakeId(file.ID.ToString());

            if (file.FolderID != null)
                file.FolderID = MakeId(file.FolderID.ToString());

            return file;
        }

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(RestoreIds(file), contentLength) { UseChunks = false };

            var uploadSession = new ChunkedUploadSession(file, contentLength);

            Item onedriveFile;
            if (file.ID != null)
            {
                onedriveFile = GetOneDriveItem(file.ID);
            }
            else
            {
                var folder = GetOneDriveItem(file.FolderID);
                onedriveFile = new Item { Name = file.Title, ParentReference = new ItemReference { Id = folder.Id } };
            }

            var onedriveSession = OneDriveProviderInfo.Storage.CreateResumableSession(onedriveFile, contentLength);
            if (onedriveSession != null)
            {
                uploadSession.Items["OneDriveSession"] = onedriveSession;
            }
            else
            {
                uploadSession.Items["TempPath"] = Path.GetTempFileName();
            }

            uploadSession.File = RestoreIds(uploadSession.File);
            return uploadSession;
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                if (uploadSession.BytesTotal == 0)
                    uploadSession.BytesTotal = chunkLength;

                uploadSession.File = SaveFile(uploadSession.File, stream);
                uploadSession.BytesUploaded = chunkLength;
                return;
            }

            if (uploadSession.Items.ContainsKey("OneDriveSession"))
            {
                var oneDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("OneDriveSession");
                OneDriveProviderInfo.Storage.Transfer(oneDriveSession, stream, chunkLength);
            }
            else
            {
                var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
                using (var fs = new FileStream(tempPath, FileMode.Append))
                {
                    stream.CopyTo(fs);
                }
            }

            uploadSession.BytesUploaded += chunkLength;

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
            else
            {
                uploadSession.File = RestoreIds(uploadSession.File);
            }
        }

        private File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("OneDriveSession"))
            {
                var oneDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("OneDriveSession");

                OneDriveProviderInfo.CacheReset(oneDriveSession.FileId);
                var parentDriveId = oneDriveSession.FolderId;
                if (parentDriveId != null) OneDriveProviderInfo.CacheReset(parentDriveId);

                return ToFile(GetOneDriveItem(oneDriveSession.FileId));
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("OneDriveSession"))
            {
                var oneDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("OneDriveSession");

                if (oneDriveSession.Status != ResumableUploadSessionStatus.Completed)
                {
                    OneDriveProviderInfo.Storage.CancelTransfer(oneDriveSession);

                    oneDriveSession.Status = ResumableUploadSessionStatus.Aborted;
                }
            }
            else if (uploadSession.Items.ContainsKey("TempPath"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("TempPath"));
            }
        }

        #endregion


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