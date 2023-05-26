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
using System.Threading.Tasks;

using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;

using DriveFile = Google.Apis.Drive.v3.Data.File;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveFileDao : GoogleDriveDaoBase, IFileDao
    {
        public GoogleDriveFileDao(GoogleDriveDaoSelector.GoogleDriveInfo providerInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
            : base(providerInfo, googleDriveDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            var driveId = MakeDriveId(fileId);
            GoogleDriveProviderInfo.CacheReset(driveId, true);

            var driveFile = GetDriveEntry(fileId);
            var parentDriveId = GetParentDriveId(driveFile);
            if (parentDriveId != null) GoogleDriveProviderInfo.CacheReset(parentDriveId);
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetDriveEntry(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ToFile(GetDriveEntries(parentId, false)
                              .FirstOrDefault(file => file.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public File GetFileStable(object fileId, int fileVersion)
        {
            return ToFile(GetDriveEntry(fileId));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(IEnumerable<object> fileIds)
        {
            if (fileIds == null || !fileIds.Any()) return new List<File>();
            return fileIds.Select(GetDriveEntry).Select(ToFile).ToList();
        }

        public List<File> GetFilesFiltered(IEnumerable<object> fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
        {
            if (fileIds == null || !fileIds.Any() || filterType == FilterType.FoldersOnly) return new List<File>();

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
                    {
                        searchText = searchText.Trim().ToLower();
                        files = files.Where(x => FileUtility.GetFileExtension(x.Title).Equals(searchText));
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                files = files.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1);

            return files.ToList();
        }

        public List<object> GetFiles(object parentId)
        {
            return GetDriveEntries(parentId, false).Select(entry => (object)MakeId(entry.Id)).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = GetDriveEntries(parentId, false).Select(ToFile);

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
                    {
                        searchText = searchText.Trim().ToLower();
                        files = files.Where(x => FileUtility.GetFileExtension(x.Title).Equals(searchText));
                    }
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
            var driveId = MakeDriveId(file.ID);
            GoogleDriveProviderInfo.CacheReset(driveId, true);
            var driveFile = GetDriveEntry(file.ID);
            if (driveFile == null) throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            long length;
            var fileStream = GoogleDriveProviderInfo.Storage.DownloadStream(driveFile, (int)offset, out length);

            if (offset == 0)
            {
                file.ContentLength = length;
            }

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

            DriveFile newDriveFile = null;

            if (file.ID != null)
            {
                newDriveFile = GoogleDriveProviderInfo.Storage.SaveStream(MakeDriveId(file.ID), fileStream, file.Title);
            }
            else if (file.FolderID != null)
            {
                newDriveFile = GoogleDriveProviderInfo.Storage.InsertEntry(fileStream, file.Title, MakeDriveId(file.FolderID));
            }

            GoogleDriveProviderInfo.CacheReset(newDriveFile);
            var parentDriveId = GetParentDriveId(newDriveFile);
            if (parentDriveId != null) GoogleDriveProviderInfo.CacheReset(parentDriveId, false);

            return ToFile(newDriveFile);
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream);
        }
        public void DeleteFile(object fileId, Guid ownerId)
        {
            DeleteFile(fileId);
        }
        public void DeleteFile(object fileId)
        {
            var driveFile = GetDriveEntry(fileId);
            if (driveFile == null) return;
            var id = MakeId(driveFile.Id);

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var hashIDs = db.ExecuteList(Query("files_thirdparty_id_mapping")
                                                 .Select("hash_id")
                                                 .Where(Exp.Like("id", id, SqlLike.StartWith)))
                                .ConvertAll(x => x[0]);

                db.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                db.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                db.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                var tagsToRemove = db.ExecuteList(
                            Query("files_tag tbl_ft ")
                                .Select("tbl_ft.id")
                                .LeftOuterJoin("files_tag_link tbl_ftl", Exp.EqColumns("tbl_ft.tenant_id", "tbl_ftl.tenant_id") &
                                                                         Exp.EqColumns("tbl_ft.id", "tbl_ftl.tag_id"))
                                .Where("tbl_ftl.tag_id is null"))
                            .ConvertAll(r => Convert.ToInt32(r[0]));

                db.ExecuteNonQuery(Delete("files_tag").Where(Exp.In("id", tagsToRemove)));


                tx.Commit();
            }

            if (!(driveFile is ErrorDriveEntry))
                GoogleDriveProviderInfo.Storage.DeleteEntry(driveFile.Id);

            GoogleDriveProviderInfo.CacheReset(driveFile.Id);
            var parentDriveId = GetParentDriveId(driveFile);
            if (parentDriveId != null) GoogleDriveProviderInfo.CacheReset(parentDriveId, false);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetDriveEntries(folderId, false)
                .Any(file => file.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var driveFile = GetDriveEntry(fileId);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            var toDriveFolder = GetDriveEntry(toFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var fromFolderDriveId = GetParentDriveId(driveFile);

            driveFile = GoogleDriveProviderInfo.Storage.InsertEntryIntoFolder(driveFile, toDriveFolder.Id);
            if (fromFolderDriveId != null)
            {
                GoogleDriveProviderInfo.Storage.RemoveEntryFromFolder(driveFile, fromFolderDriveId);
            }

            GoogleDriveProviderInfo.CacheReset(driveFile.Id);
            GoogleDriveProviderInfo.CacheReset(fromFolderDriveId, false);
            GoogleDriveProviderInfo.CacheReset(toDriveFolder.Id, false);

            return MakeId(driveFile.Id);
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var driveFile = GetDriveEntry(fileId);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            var toDriveFolder = GetDriveEntry(toFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var newDriveFile = GoogleDriveProviderInfo.Storage.CopyEntry(toDriveFolder.Id, driveFile.Id);

            GoogleDriveProviderInfo.CacheReset(newDriveFile);
            GoogleDriveProviderInfo.CacheReset(toDriveFolder.Id, false);

            return ToFile(newDriveFile);
        }

        public object FileRename(File file, string newTitle)
        {
            var driveFile = GetDriveEntry(file.ID);
            driveFile.Name = newTitle;

            driveFile = GoogleDriveProviderInfo.Storage.RenameEntry(driveFile.Id, driveFile.Name);

            GoogleDriveProviderInfo.CacheReset(driveFile);
            var parentDriveId = GetParentDriveId(driveFile);
            if (parentDriveId != null) GoogleDriveProviderInfo.CacheReset(parentDriveId, false);

            return MakeId(driveFile.Id);
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
            if (SetupInfo.ChunkUploadSize > contentLength && contentLength != -1)
                return new ChunkedUploadSession(RestoreIds(file), contentLength) { UseChunks = false };

            var uploadSession = new ChunkedUploadSession(file, contentLength);

            DriveFile driveFile;
            if (file.ID != null)
            {
                driveFile = GetDriveEntry(file.ID);
            }
            else
            {
                var folder = GetDriveEntry(file.FolderID);
                driveFile = GoogleDriveProviderInfo.Storage.FileConstructor(file.Title, null, folder.Id);
            }

            var googleDriveSession = GoogleDriveProviderInfo.Storage.CreateResumableSession(driveFile, contentLength);
            if (googleDriveSession != null)
            {
                uploadSession.Items["GoogleDriveSession"] = googleDriveSession;
            }
            else
            {
                uploadSession.Items["TempPath"] = TempPath.GetTempFileName();
            }

            uploadSession.File = RestoreIds(uploadSession.File);
            return uploadSession;
        }

        public File UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                if (uploadSession.BytesTotal == 0)
                    uploadSession.BytesTotal = chunkLength;

                uploadSession.File = SaveFile(uploadSession.File, stream);
                uploadSession.BytesUploaded = chunkLength;
                return uploadSession.File;
            }

            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");
                GoogleDriveProviderInfo.Storage.Transfer(googleDriveSession, stream, chunkLength, uploadSession.LastChunk);
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

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal || uploadSession.LastChunk)
            {
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
            else
            {
                uploadSession.File = RestoreIds(uploadSession.File);
            }
            return uploadSession.File;
        }

        public File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");

                GoogleDriveProviderInfo.CacheReset(googleDriveSession.FileId);
                var parentDriveId = googleDriveSession.FolderId;
                if (parentDriveId != null) GoogleDriveProviderInfo.CacheReset(parentDriveId, false);

                return ToFile(GetDriveEntry(googleDriveSession.FileId));
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");

                if (googleDriveSession.Status != ResumableUploadSessionStatus.Completed)
                {
                    googleDriveSession.Status = ResumableUploadSessionStatus.Aborted;
                }
            }
            else if (uploadSession.Items.ContainsKey("TempPath"))
            {
                var path = uploadSession.GetItemOrDefault<string>("TempPath");
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        #endregion


        #region Only in TMFileDao

        public void ReassignFiles(IEnumerable<object> fileIds, Guid newOwnerId)
        {
        }

        public List<File> GetFiles(IEnumerable<object> parentIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
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

        public void SaveThumbnail(File file, Stream thumbnail)
        {
            //Do nothing
        }

        public Stream GetThumbnail(File file)
        {
            return null;
        }

        public Task<Stream> GetFileStreamAsync(File file)
        {
            return Task.FromResult(GetFileStream(file));
        }

        public Task<bool> IsExistOnStorageAsync(File file)
        {
            return Task.FromResult(IsExistOnStorage(file));
        }

        public EntryProperties GetProperties(object fileId)
        {
            return null;
        }

        public void SaveProperties(object fileId, EntryProperties entryProperties)
        {
        }

        public Task UploadChunkAsync(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}