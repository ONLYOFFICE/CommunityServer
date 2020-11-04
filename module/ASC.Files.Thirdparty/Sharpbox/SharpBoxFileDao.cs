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
using System.Net;
using System.Security;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal class SharpBoxFileDao : SharpBoxDaoBase, IFileDao
    {
        public SharpBoxFileDao(SharpBoxDaoSelector.SharpBoxInfo providerInfo, SharpBoxDaoSelector sharpBoxDaoSelector)
            : base(providerInfo, sharpBoxDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            SharpBoxProviderInfo.InvalidateStorage();
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetFileById(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ToFile(GetFolderFiles(parentId).FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public File GetFileStable(object fileId, int fileVersion)
        {
            return ToFile(GetFileById(fileId));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            return fileIds.Select(fileId => ToFile(GetFileById(fileId))).ToList();
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
            var folder = GetFolderById(parentId).AsEnumerable();

            return folder
                .Where(x => !(x is ICloudDirectoryEntry))
                .Select(x => (object) MakeId(x)).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = GetFolderById(parentId).Where(x => !(x is ICloudDirectoryEntry)).Select(ToFile);

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

        public Stream GetFileStream(File file, long offset)
        {
            var fileToDownload = GetFileById(file.ID);

            if (fileToDownload == null)
                throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);
            if (fileToDownload is ErrorEntry)
                throw new Exception(((ErrorEntry)fileToDownload).Error);

            var fileStream = fileToDownload.GetDataTransferAccessor().GetDownloadStream();

            if (fileStream != null && offset > 0)
            {
                if (!fileStream.CanSeek)
                {
                    var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);

                    fileStream.CopyTo(tempBuffer);
                    tempBuffer.Flush();
                    tempBuffer.Seek(offset, SeekOrigin.Begin);

                    fileStream.Dispose();
                    return tempBuffer;
                }

                fileStream.Seek(offset, SeekOrigin.Begin);
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

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");
            ICloudFileSystemEntry entry = null;
            if (file.ID != null)
            {
                entry = SharpBoxProviderInfo.Storage.GetFile(MakePath(file.ID), null);
            }
            else if (file.FolderID != null)
            {
                var folder = GetFolderById(file.FolderID);
                file.Title = GetAvailableTitle(file.Title, folder, IsExist);
                entry = SharpBoxProviderInfo.Storage.CreateFile(folder, file.Title);
            }

            if (entry == null)
            {
                return null;
            }

            try
            {
                entry.GetDataTransferAccessor().Transfer(fileStream.GetBuffered(), nTransferDirection.nUpload);
            }
            catch (SharpBoxException e)
            {
                var webException = (WebException)e.InnerException;
                if (webException != null)
                {
                    var response = ((HttpWebResponse)webException.Response);
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
                        }
                    }
                    throw;
                }
            }

            if (file.ID != null && !entry.Name.Equals(file.Title))
            {
                file.Title = GetAvailableTitle(file.Title, entry.Parent, IsExist);
                SharpBoxProviderInfo.Storage.RenameFileSystemEntry(entry, file.Title);
            }

            return ToFile(entry);
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream);
        }

        public void DeleteFile(object fileId)
        {
            var file = GetFileById(fileId);
            if (file == null) return;
            var id = MakeId(file);

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

            if (!(file is ErrorEntry))
                SharpBoxProviderInfo.Storage.DeleteFileSystemEntry(file);
        }

        public bool IsExist(string title, object folderId)
        {
            var folder = SharpBoxProviderInfo.Storage.GetFolder(MakePath(folderId));
            return IsExist(title, folder);
        }

        public bool IsExist(string title, ICloudDirectoryEntry folder)
        {
            try
            {
                return SharpBoxProviderInfo.Storage.GetFileSystemObject(title, folder) != null;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var entry = GetFileById(fileId);
            var folder = GetFolderById(toFolderId);

            var oldFileId = MakeId(entry);

            if (!SharpBoxProviderInfo.Storage.MoveFileSystemEntry(entry, folder))
                throw new Exception("Error while moving");

            var newFileId = MakeId(entry);

            UpdatePathInDB(oldFileId, newFileId);

            return newFileId;
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var file = GetFileById(fileId);
            if (!SharpBoxProviderInfo.Storage.CopyFileSystemEntry(MakePath(fileId), MakePath(toFolderId)))
                throw new Exception("Error while copying");
            return ToFile(GetFolderById(toFolderId).FirstOrDefault(x => x.Name == file.Name));
        }

        public object FileRename(File file, string newTitle)
        {
            var entry = GetFileById(file.ID);

            if (entry == null)
                throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);

            var oldFileId = MakeId(entry);
            var newFileId = oldFileId;

            var folder = GetFolderById(file.FolderID);
            newTitle = GetAvailableTitle(newTitle, folder, IsExist);

            if (SharpBoxProviderInfo.Storage.RenameFileSystemEntry(entry, newTitle))
            {
                //File data must be already updated by provider
                //We can't search google files by title because root can have multiple folders with the same name
                //var newFile = SharpBoxProviderInfo.Storage.GetFileSystemObject(newTitle, file.Parent);
                newFileId = MakeId(entry);
            }

            UpdatePathInDB(oldFileId, newFileId);

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

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(MakeId(file), contentLength) { UseChunks = false };

            var uploadSession = new ChunkedUploadSession(file, contentLength);

            var isNewFile = false;

            ICloudFileSystemEntry sharpboxFile;
            if (file.ID != null)
            {
                sharpboxFile = GetFileById(file.ID);
            }
            else
            {
                var folder = GetFolderById(file.FolderID);
                sharpboxFile = SharpBoxProviderInfo.Storage.CreateFile(folder, GetAvailableTitle(file.Title, folder, IsExist));
                isNewFile = true;
            }

            var sharpboxSession = sharpboxFile.GetDataTransferAccessor().CreateResumableSession(contentLength);
            if (sharpboxSession != null)
            {
                uploadSession.Items["SharpboxSession"] = sharpboxSession;
                uploadSession.Items["IsNewFile"] = isNewFile;
            }
            else
            {
                uploadSession.Items["TempPath"] = Path.GetTempFileName();
            }

            uploadSession.File = MakeId(uploadSession.File);
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

            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession =
                    uploadSession.GetItemOrDefault<AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects.ResumableUploadSession>("SharpboxSession");

                var isNewFile = uploadSession.Items.ContainsKey("IsNewFile") && uploadSession.GetItemOrDefault<bool>("IsNewFile");
                var sharpboxFile =
                    isNewFile
                        ? SharpBoxProviderInfo.Storage.CreateFile(GetFolderById(sharpboxSession.ParentId), sharpboxSession.FileName)
                        : GetFileById(sharpboxSession.FileId);

                sharpboxFile.GetDataTransferAccessor().Transfer(sharpboxSession, stream, chunkLength);
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
                uploadSession.File = MakeId(uploadSession.File);
            }
        }

        public File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession =
                    uploadSession.GetItemOrDefault<AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects.ResumableUploadSession>("SharpboxSession");
                return ToFile(GetFileById(sharpboxSession.FileId));
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession =
                    uploadSession.GetItemOrDefault<AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects.ResumableUploadSession>("SharpboxSession");

                var isNewFile = uploadSession.Items.ContainsKey("IsNewFile") && uploadSession.GetItemOrDefault<bool>("IsNewFile");
                var sharpboxFile =
                    isNewFile
                        ? SharpBoxProviderInfo.Storage.CreateFile(GetFolderById(sharpboxSession.ParentId), sharpboxSession.FileName)
                        : GetFileById(sharpboxSession.FileId);
                sharpboxFile.GetDataTransferAccessor().AbortResumableSession(sharpboxSession);
            }
            else if (uploadSession.Items.ContainsKey("TempPath"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("TempPath"));
            }
        }

        private File MakeId(File file)
        {
            if (file.ID != null)
                file.ID = PathPrefix + "-" + file.ID;

            if (file.FolderID != null)
                file.FolderID = PathPrefix + "-" + file.FolderID;

            return file;
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