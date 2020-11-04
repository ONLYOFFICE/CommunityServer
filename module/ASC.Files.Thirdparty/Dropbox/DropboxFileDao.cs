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
using Dropbox.Api.Files;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal class DropboxFileDao : DropboxDaoBase, IFileDao
    {
        public DropboxFileDao(DropboxDaoSelector.DropboxInfo dropboxInfo, DropboxDaoSelector dropboxDaoSelector)
            : base(dropboxInfo, dropboxDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            var dropboxFilePath = MakeDropboxPath(fileId);
            DropboxProviderInfo.CacheReset(dropboxFilePath, true);

            var dropboxFile = GetDropboxFile(fileId);
            var parentPath = GetParentFolderPath(dropboxFile);
            if (parentPath != null) DropboxProviderInfo.CacheReset(parentPath);
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetDropboxFile(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            var metadata = GetDropboxItems(parentId, false)
                .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
            return metadata == null
                       ? null
                       : ToFile(metadata.AsFile);
        }

        public File GetFileStable(object fileId, int fileVersion)
        {
            return ToFile(GetDropboxFile(fileId));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            if (fileIds == null || fileIds.Length == 0) return new List<File>();
            return fileIds.Select(GetDropboxFile).Select(ToFile).ToList();
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
            return GetDropboxItems(parentId, false).Select(entry => (object)MakeId(entry)).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = GetDropboxItems(parentId, false).Select(item => ToFile(item.AsFile));

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
            var dropboxFilePath = MakeDropboxPath(file.ID);
            DropboxProviderInfo.CacheReset(dropboxFilePath, true);

            var dropboxFile = GetDropboxFile(file.ID);
            if (dropboxFile == null) throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);
            if (dropboxFile is ErrorFile) throw new Exception(((ErrorFile)dropboxFile).Error);

            var fileStream = DropboxProviderInfo.Storage.DownloadStream(MakeDropboxPath(dropboxFile), (int)offset);

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

            FileMetadata newDropboxFile = null;

            if (file.ID != null)
            {
                var filePath = MakeDropboxPath(file.ID);
                newDropboxFile = DropboxProviderInfo.Storage.SaveStream(filePath, fileStream);
                if (!newDropboxFile.Name.Equals(file.Title))
                {
                    var parentFolderPath = GetParentFolderPath(newDropboxFile);
                    file.Title = GetAvailableTitle(file.Title, parentFolderPath, IsExist);
                    newDropboxFile = DropboxProviderInfo.Storage.MoveFile(filePath, parentFolderPath, file.Title);
                }
            }
            else if (file.FolderID != null)
            {
                var folderPath = MakeDropboxPath(file.FolderID);
                file.Title = GetAvailableTitle(file.Title, folderPath, IsExist);
                newDropboxFile = DropboxProviderInfo.Storage.CreateFile(fileStream, file.Title, folderPath);
            }

            DropboxProviderInfo.CacheReset(newDropboxFile);
            var parentPath = GetParentFolderPath(newDropboxFile);
            if (parentPath != null) DropboxProviderInfo.CacheReset(parentPath);

            return ToFile(newDropboxFile);
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream);
        }

        public void DeleteFile(object fileId)
        {
            var dropboxFile = GetDropboxFile(fileId);
            if (dropboxFile == null) return;
            var id = MakeId(dropboxFile);

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

            if (!(dropboxFile is ErrorFile))
                DropboxProviderInfo.Storage.DeleteItem(dropboxFile);

            DropboxProviderInfo.CacheReset(MakeDropboxPath(dropboxFile), true);
            var parentFolderPath = GetParentFolderPath(dropboxFile);
            if (parentFolderPath != null) DropboxProviderInfo.CacheReset(parentFolderPath);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetDropboxItems(folderId, false)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var dropboxFile = GetDropboxFile(fileId);
            if (dropboxFile is ErrorFile) throw new Exception(((ErrorFile)dropboxFile).Error);

            var toDropboxFolder = GetDropboxFolder(toFolderId);
            if (toDropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toDropboxFolder).Error);

            var fromFolderPath = GetParentFolderPath(dropboxFile);

            dropboxFile = DropboxProviderInfo.Storage.MoveFile(MakeDropboxPath(dropboxFile), MakeDropboxPath(toDropboxFolder), dropboxFile.Name);

            DropboxProviderInfo.CacheReset(MakeDropboxPath(dropboxFile), true);
            DropboxProviderInfo.CacheReset(fromFolderPath);
            DropboxProviderInfo.CacheReset(MakeDropboxPath(toDropboxFolder));

            return MakeId(dropboxFile);
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var dropboxFile = GetDropboxFile(fileId);
            if (dropboxFile is ErrorFile) throw new Exception(((ErrorFile)dropboxFile).Error);

            var toDropboxFolder = GetDropboxFolder(toFolderId);
            if (toDropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toDropboxFolder).Error);

            var newDropboxFile = DropboxProviderInfo.Storage.CopyFile(MakeDropboxPath(dropboxFile), MakeDropboxPath(toDropboxFolder), dropboxFile.Name);

            DropboxProviderInfo.CacheReset(newDropboxFile);
            DropboxProviderInfo.CacheReset(MakeDropboxPath(toDropboxFolder));

            return ToFile(newDropboxFile);
        }

        public object FileRename(File file, string newTitle)
        {
            var dropboxFile = GetDropboxFile(file.ID);
            var parentFolderPath = GetParentFolderPath(dropboxFile);
            newTitle = GetAvailableTitle(newTitle, parentFolderPath, IsExist);

            dropboxFile = DropboxProviderInfo.Storage.MoveFile(MakeDropboxPath(dropboxFile), parentFolderPath, newTitle);

            DropboxProviderInfo.CacheReset(dropboxFile);
            var parentPath = GetParentFolderPath(dropboxFile);
            if (parentPath != null) DropboxProviderInfo.CacheReset(parentPath);

            return MakeId(dropboxFile);
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

            var dropboxSession = DropboxProviderInfo.Storage.CreateResumableSession();
            if (dropboxSession != null)
            {
                uploadSession.Items["DropboxSession"] = dropboxSession;
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

            if (uploadSession.Items.ContainsKey("DropboxSession"))
            {
                var dropboxSession = uploadSession.GetItemOrDefault<string>("DropboxSession");
                DropboxProviderInfo.Storage.Transfer(dropboxSession, uploadSession.BytesUploaded, stream);
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

        public File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("DropboxSession"))
            {
                var dropboxSession = uploadSession.GetItemOrDefault<string>("DropboxSession");

                Metadata dropboxFile;
                var file = uploadSession.File;
                if (file.ID != null)
                {
                    var dropboxFilePath = MakeDropboxPath(file.ID);
                    dropboxFile = DropboxProviderInfo.Storage.FinishResumableSession(dropboxSession, dropboxFilePath, uploadSession.BytesUploaded);
                }
                else
                {
                    var folderPath = MakeDropboxPath(file.FolderID);
                    var title = GetAvailableTitle(file.Title, folderPath, IsExist);
                    dropboxFile = DropboxProviderInfo.Storage.FinishResumableSession(dropboxSession, folderPath, title, uploadSession.BytesUploaded);
                }

                DropboxProviderInfo.CacheReset(MakeDropboxPath(dropboxFile));
                DropboxProviderInfo.CacheReset(GetParentFolderPath(dropboxFile), false);

                return ToFile(dropboxFile.AsFile);
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"),
                                           FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("TempPath"))
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