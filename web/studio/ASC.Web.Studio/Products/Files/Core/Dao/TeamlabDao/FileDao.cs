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
using System.Linq.Expressions;
using System.Threading.Tasks;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Search;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;

namespace ASC.Files.Core.Data
{
    internal class FileDao : AbstractDao, IFileDao
    {
        private static readonly object syncRoot = new object();

        public FileDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        public void InvalidateCache(object fileId)
        {
        }

        public File GetFile(object fileId)
        {
            List<object[]> fromDb;
            var query = GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("current_version", true));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(query);
            }

            return fromDb
                .ConvertAll(ToFile)
                .SingleOrDefault();
        }

        public File GetFile(object fileId, int fileVersion)
        {
            List<object[]> fromDb;
            var query = GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("version", fileVersion));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(query);
            }

            return
                fromDb
                .ConvertAll(ToFile)
                .SingleOrDefault();
        }

        public File GetFile(object parentId, String title)
        {
            if (String.IsNullOrEmpty(title)) throw new ArgumentNullException(title);

            List<object[]> fromDb;
            var query = GetFileQuery(Exp.Eq("title", title) & Exp.Eq("current_version", true) & Exp.Eq("folder_id", parentId))
                                 .OrderBy("create_on", true)
                                 .SetMaxResults(1);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(query);
            }

            return
                fromDb
                .ConvertAll(ToFile)
                .FirstOrDefault();
        }

        public File GetFileStable(object fileId, int fileVersion = -1)
        {
            List<object[]> fromDb;

            var query = GetFileQuery(Exp.Eq("id", fileId)
                                     & Exp.Eq("forcesave", 0))
                .OrderBy("version", false)
                .SetMaxResults(1);

            if (fileVersion >= 0)
            {
                query.Where(Exp.Le("version", fileVersion));
            }

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(query);
            }

            return fromDb
                .ConvertAll(ToFile)
                .SingleOrDefault();
        }

        public List<File> GetFileHistory(object fileId)
        {
            List<object[]> fromDb;
            var query = GetFileQuery(Exp.Eq("id", fileId)).OrderBy("version", false);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(query);
            }

            return fromDb.ConvertAll(ToFile);
        }

        public List<File> GetFiles(IEnumerable<object> fileIds)
        {
            if (fileIds == null || !fileIds.Any()) return new List<File>();

            List<object[]> fromDb;
            var query = GetFileQuery(Exp.In("id", fileIds) & Exp.Eq("current_version", true));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(query);
            }

            return fromDb.ConvertAll(ToFile);
        }

        public List<File> GetFilesFiltered(IEnumerable<object> fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, string extension)
        {
            if (fileIds == null || !fileIds.Any() || filterType == FilterType.FoldersOnly) return new List<File>();

            var q = GetFileQuery(Exp.In("id", fileIds) & Exp.Eq("current_version", true), false);

            var searchByText = !string.IsNullOrEmpty(searchText);
            var searchByExtension = (filterType == FilterType.ByExtension || filterType == FilterType.ByExtensionIncludeFolders) && !string.IsNullOrEmpty(extension);

            if (searchByText || searchByExtension)
            {
                List<int> searchIds;
                var func = GetFuncForSearch(null, null, filterType, subjectGroup, subjectID, searchText, searchInContent, extension, false);

                if (FactoryIndexer<FilesWrapper>.TrySelectIds(s => func(s).In(r => r.Id, fileIds.ToArray()), out searchIds))
                {
                    q.Where(Exp.In("id", searchIds));
                }
                else
                {
                    if (searchByText)
                    {
                        q.Where(BuildSearch("title", searchText));
                    }
                    if (searchByExtension)
                    {
                        q.Where(BuildSearch("title", extension, SqlLike.EndWith));
                    }
                }
            }

            if (subjectID != Guid.Empty)
            {
                if (subjectGroup)
                {
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                }
                else
                {
                    q.Where("create_by", subjectID.ToString());
                }
            }

            switch (filterType)
            {
                case FilterType.DocumentsOnly:
                case FilterType.ImagesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.MediaOnly:
                    q.Where("category", (int)filterType);
                    break;
            }

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(q);
            }

            return fromDb.ConvertAll(ToFile);
        }

        public List<object> GetFiles(object parentId)
        {
            var query =
                Query("files_file")
                    .Select("id")
                    .Where(Exp.Eq("folder_id", parentId) & Exp.Eq("current_version", true));

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(query);
            }

            return fromDb.ConvertAll(r => r[0]);
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, string extension, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFileQuery(Exp.Eq("current_version", true) & Exp.Eq("folder_id", parentId));

            if (withSubfolders)
            {
                q = GetFileQuery(Exp.Eq("current_version", true) & Exp.Eq("fft.parent_id", parentId))
                    .InnerJoin("files_folder_tree fft", Exp.EqColumns("fft.folder_id", "f.folder_id"));
            }

            var searchByText = !string.IsNullOrEmpty(searchText);
            var searchByExtension = (filterType == FilterType.ByExtension || filterType == FilterType.ByExtensionIncludeFolders) && !string.IsNullOrEmpty(extension);

            if (searchByText || searchByExtension)
            {
                List<int> searchIds;

                var func = GetFuncForSearch(parentId, orderBy, filterType, subjectGroup, subjectID, searchText, searchInContent, extension, withSubfolders);

                Expression<Func<Selector<FilesWrapper>, Selector<FilesWrapper>>> expression = s => func(s);

                if (FactoryIndexer<FilesWrapper>.TrySelectIds(expression, out searchIds))
                {
                    q.Where(Exp.In("id", searchIds));
                }
                else
                {
                    if (searchByText)
                    {
                        q.Where(BuildSearch("title", searchText));
                    }
                    if (searchByExtension)
                    {
                        q.Where(BuildSearch("title", extension, SqlLike.EndWith));
                    }
                }
            }

            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    q.OrderBy("create_by", orderBy.IsAsc);
                    break;
                case SortedByType.Size:
                    q.OrderBy("content_length", orderBy.IsAsc);
                    break;
                case SortedByType.AZ:
                    q.OrderBy("title", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTime:
                    q.OrderBy("modified_on", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTimeCreation:
                    q.OrderBy("create_on", orderBy.IsAsc);
                    break;
                default:
                    q.OrderBy("title", true);
                    break;
            }

            if (subjectID != Guid.Empty)
            {
                if (subjectGroup)
                {
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                }
                else
                {
                    q.Where("create_by", subjectID.ToString());
                }
            }

            switch (filterType)
            {
                case FilterType.DocumentsOnly:
                case FilterType.ImagesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.MediaOnly:
                    q.Where("category", (int)filterType);
                    break;
            }

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(q);
            }

            return fromDb.ConvertAll(ToFile);
        }

        public Stream GetFileStream(File file, long offset)
        {
            return Global.GetStore().GetReadStream(string.Empty, GetUniqFilePath(file), offset);
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            var path = GetUniqFilePath(file);
            var headers = new List<string>
            {
                string.Concat("Content-Disposition:", ContentDispositionUtil.GetHeaderValue(file.Title, withoutBase: true))
            };

            if (!SecurityContext.IsAuthenticated)
            {
                headers.Add(ASC.Data.Storage.SecureHelper.GenerateSecureKeyHeader(path));
            }

            return Global.GetStore().GetPreSignedUri(string.Empty, path, expires, headers);
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            return Global.GetStore().IsSupportedPreSignedUri;
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public async Task<Stream> GetFileStreamAsync(File file)
        {
            return await Global.GetStore().GetReadStreamAsync(string.Empty, GetUniqFilePath(file), 0);
        }

        public File SaveFile(File file, Stream fileStream)
        {
            return SaveFile(file, fileStream, true);
        }

        public File SaveFile(File file, Stream fileStream, bool checkQuota = true)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (checkQuota && SetupInfo.MaxChunkedUploadSize < file.ContentLength)
            {
                throw FileSizeComment.GetFileSizeException(SetupInfo.MaxChunkedUploadSize);
            }

            if (CoreContext.Configuration.Personal && SetupInfo.IsVisibleSettings("PersonalMaxSpace"))
            {
                if (CoreContext.Configuration.PersonalMaxSpace - Global.GetUserUsedSpace(file.GetFileQuotaOwner()) < file.ContentLength)
                {
                    throw FileSizeComment.GetPersonalFreeSpaceException(CoreContext.Configuration.PersonalMaxSpace);
                }
            }

            var isNew = false;
            List<object> parentFoldersIds;
            lock (syncRoot)
            {
                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                using (var tx = dbManager.BeginTransaction())
                {
                    if (file.ID == null)
                    {
                        file.ID = dbManager.ExecuteScalar<int>(new SqlQuery("files_file").SelectMax("id")) + 1;
                        file.Version = 1;
                        file.VersionGroup = 1;
                        isNew = true;
                    }

                    file.Title = Global.ReplaceInvalidCharsAndTruncate(file.Title);
                    //make lowerCase
                    file.Title = FileUtility.ReplaceFileExtension(file.Title, FileUtility.GetFileExtension(file.Title));

                    file.ModifiedBy = SecurityContext.CurrentAccount.ID;
                    file.ModifiedOn = TenantUtil.DateTimeNow();
                    if (file.CreateBy == default(Guid)) file.CreateBy = SecurityContext.CurrentAccount.ID;
                    if (file.CreateOn == default(DateTime)) file.CreateOn = TenantUtil.DateTimeNow();

                    dbManager.ExecuteNonQuery(
                        Update("files_file")
                            .Set("current_version", false)
                            .Where("id", file.ID)
                            .Where("current_version", true));

                    var sql = Insert("files_file")
                                .InColumnValue("id", file.ID)
                                .InColumnValue("version", file.Version)
                                .InColumnValue("version_group", file.VersionGroup)
                                .InColumnValue("current_version", true)
                                .InColumnValue("folder_id", file.FolderID)
                                .InColumnValue("title", file.Title)
                                .InColumnValue("content_length", file.ContentLength)
                                .InColumnValue("category", (int)file.FilterType)
                                .InColumnValue("create_by", file.CreateBy.ToString())
                                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(file.CreateOn))
                                .InColumnValue("modified_by", file.ModifiedBy.ToString())
                                .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .InColumnValue("converted_type", file.ConvertedType)
                                .InColumnValue("comment", file.Comment)
                                .InColumnValue("encrypted", file.Encrypted)
                                .InColumnValue("forcesave", (int)file.Forcesave)
                                .InColumnValue("thumb", file.ThumbnailStatus);
                    dbManager.ExecuteNonQuery(sql);

                    file.PureTitle = file.Title;

                    parentFoldersIds = dbManager.ExecuteList(
                        new SqlQuery("files_folder_tree")
                            .Select("parent_id")
                            .Where(Exp.Eq("folder_id", file.FolderID))
                            .OrderBy("level", false)
                        ).ConvertAll(row => row[0]);

                    if (parentFoldersIds.Count > 0)
                    {
                        dbManager.ExecuteNonQuery(
                            Update("files_folder")
                                .Set("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .Set("modified_by", file.ModifiedBy.ToString())
                                .Where(Exp.In("id", parentFoldersIds)));
                    }

                    if (isNew)
                    {
                        RecalculateFilesCount(dbManager, file.FolderID);
                    }

                    tx.Commit();
                }
            }

            if (fileStream != null)
            {
                try
                {
                    SaveFileStream(file, fileStream);
                }
                catch (Exception saveException)
                {
                    try
                    {
                        if (isNew)
                        {
                            var stored = Global.GetStore().IsDirectory(GetUniqFileDirectory(file.ID));
                            DeleteFile(file.ID, stored, file.GetFileQuotaOwner());
                        }
                        else if (!IsExistOnStorage(file))
                        {
                            DeleteVersion(file);
                        }
                    }
                    catch (Exception deleteException)
                    {
                        throw new Exception(saveException.Message, deleteException);
                    }
                    throw;
                }
            }

            FactoryIndexer<FilesWrapper>.IndexAsync(FilesWrapper.GetFilesWrapper(file, parentFoldersIds));

            return GetFile(file.ID);
        }

        public File ReplaceFileVersion(File file, Stream fileStream)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (file.ID == null) throw new ArgumentException("No file id or folder id toFolderId determine provider");

            if (SetupInfo.MaxChunkedUploadSize < file.ContentLength)
            {
                throw FileSizeComment.GetFileSizeException(SetupInfo.MaxChunkedUploadSize);
            }

            if (CoreContext.Configuration.Personal && SetupInfo.IsVisibleSettings("PersonalMaxSpace"))
            {
                if (CoreContext.Configuration.PersonalMaxSpace - Global.GetUserUsedSpace(file.GetFileQuotaOwner()) < file.ContentLength)
                {
                    throw FileSizeComment.GetPersonalFreeSpaceException(CoreContext.Configuration.PersonalMaxSpace);
                }
            }
            List<object> parentFoldersIds;
            lock (syncRoot)
            {
                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                using (var tx = dbManager.BeginTransaction())
                {

                    file.Title = Global.ReplaceInvalidCharsAndTruncate(file.Title);
                    //make lowerCase
                    file.Title = FileUtility.ReplaceFileExtension(file.Title, FileUtility.GetFileExtension(file.Title));

                    file.ModifiedBy = SecurityContext.CurrentAccount.ID;
                    file.ModifiedOn = TenantUtil.DateTimeNow();
                    if (file.CreateBy == default(Guid)) file.CreateBy = SecurityContext.CurrentAccount.ID;
                    if (file.CreateOn == default(DateTime)) file.CreateOn = TenantUtil.DateTimeNow();

                    var sql = Update("files_file")
                        .Set("version", file.Version)
                        .Set("version_group", file.VersionGroup)
                        .Set("folder_id", file.FolderID)
                        .Set("title", file.Title)
                        .Set("content_length", file.ContentLength)
                        .Set("category", (int)file.FilterType)
                        .Set("create_by", file.CreateBy.ToString())
                        .Set("create_on", TenantUtil.DateTimeToUtc(file.CreateOn))
                        .Set("modified_by", file.ModifiedBy.ToString())
                        .Set("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                        .Set("converted_type", file.ConvertedType)
                        .Set("comment", file.Comment)
                        .Set("encrypted", file.Encrypted)
                        .Set("forcesave", (int)file.Forcesave)
                        .Set("thumb", file.ThumbnailStatus)
                        .Where("id", file.ID)
                        .Where("version", file.Version);
                    dbManager.ExecuteNonQuery(sql);

                    file.PureTitle = file.Title;

                    parentFoldersIds = dbManager.ExecuteList(
                        new SqlQuery("files_folder_tree")
                            .Select("parent_id")
                            .Where(Exp.Eq("folder_id", file.FolderID))
                            .OrderBy("level", false)
                        ).ConvertAll(row => row[0]);

                    if (parentFoldersIds.Count > 0)
                    {
                        dbManager.ExecuteNonQuery(
                            Update("files_folder")
                                .Set("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .Set("modified_by", file.ModifiedBy.ToString())
                                .Where(Exp.In("id", parentFoldersIds)));
                    }

                    tx.Commit();
                }
            }

            if (fileStream != null)
            {
                try
                {
                    DeleteVersionStream(file);
                    SaveFileStream(file, fileStream);
                }
                catch
                {
                    if (!IsExistOnStorage(file))
                    {
                        DeleteVersion(file);
                    }
                    throw;
                }
            }

            FactoryIndexer<FilesWrapper>.IndexAsync(FilesWrapper.GetFilesWrapper(file, parentFoldersIds));

            return GetFile(file.ID);
        }

        private void DeleteVersion(File file)
        {
            if (file == null
                || file.ID == null
                || file.Version <= 1) return;

            var deleteQuery =
                Delete("files_file")
                    .Where("id", file.ID)
                    .Where("version", file.Version);

            var updateQuery =
                    Update("files_file")
                        .Set("current_version", true)
                        .Where("id", file.ID)
                        .Where("version", file.Version - 1);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(deleteQuery);
                dbManager.ExecuteNonQuery(updateQuery);
            }
        }

        private static void DeleteVersionStream(File file)
        {
            Global.GetStore().DeleteDirectory(GetUniqFileVersionPath(file.ID, file.Version));
        }

        private static void SaveFileStream(File file, Stream stream)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(file.FolderID);
                file.RootFolderCreator = folder.RootFolderCreator;
                file.RootFolderType = folder.RootFolderType;

                Global.GetStore().Save(string.Empty, GetUniqFilePath(file), file.GetFileQuotaOwner(), stream, file.Title);
            }
        }

        public void DeleteFile(object fileId)
        {
            DeleteFile(fileId, Guid.Empty);
        }
        public void DeleteFile(object fileId, Guid ownerId)
        {
            DeleteFile(fileId, true, ownerId);
        }

        private void DeleteFile(object fileId, bool deleteFolder, Guid ownerId)
        {
            if (fileId == null) return;
            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            using (var tx = dbManager.BeginTransaction())
            {
                var fromFolders = dbManager
                    .ExecuteList(Query("files_file").Select("folder_id").Where("id", fileId).GroupBy("id"))
                    .ConvertAll(r => r[0]);

                dbManager.ExecuteNonQuery(Delete("files_file").Where("id", fileId));
                dbManager.ExecuteNonQuery(Delete("files_tag_link").Where("entry_id", fileId.ToString()).Where("entry_type", (int)FileEntryType.File));
                var tagsToRemove = dbManager.ExecuteList(
             Query("files_tag tbl_ft ")
                 .Select("tbl_ft.id")
                 .LeftOuterJoin("files_tag_link tbl_ftl", Exp.EqColumns("tbl_ft.tenant_id", "tbl_ftl.tenant_id") &
                                                          Exp.EqColumns("tbl_ft.id", "tbl_ftl.tag_id"))
                 .Where("tbl_ftl.tag_id  is null"))
             .ConvertAll(r => Convert.ToInt32(r[0]));

                dbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.In("id", tagsToRemove)));

                dbManager.ExecuteNonQuery(Delete("files_security").Where("entry_id", fileId.ToString()).Where("entry_type", (int)FileEntryType.File));

                fromFolders.ForEach(folderId => RecalculateFilesCount(dbManager, folderId));

                tx.Commit();
            }

            if (deleteFolder)
                DeleteFolder(fileId, ownerId);

            FactoryIndexer<FilesWrapper>.DeleteAsync(new FilesWrapper { Id = (int)fileId });
        }

        public bool IsExist(String title, object folderId)
        {
            var query =
                Query("files_file")
                    .SelectCount()
                    .Where("title", title)
                    .Where("folder_id", folderId)
                    .Where("current_version", true);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                var fileCount = dbManager.ExecuteScalar<int>(query);

                return fileCount != 0;
            }
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            if (fileId == null) return null;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            using (var tx = dbManager.BeginTransaction())
            {
                var fromFolders = dbManager
                    .ExecuteList(Query("files_file").Select("folder_id").Where("id", fileId).GroupBy("id"))
                    .ConvertAll(r => r[0]);

                var sql = Update("files_file")
                    .Set("folder_id", toFolderId)
                    .Where("id", fileId);

                if (Global.FolderTrash.Equals(toFolderId))
                {
                    sql
                        .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                        .Set("modified_on", DateTime.UtcNow);
                }

                dbManager.ExecuteNonQuery(sql);

                fromFolders.ForEach(folderId => RecalculateFilesCount(dbManager, folderId));
                RecalculateFilesCount(dbManager, toFolderId);

                tx.Commit();
            }

            var parentFoldersIds = new List<object>();

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                parentFoldersIds = dbManager.ExecuteList(
                new SqlQuery("files_folder_tree")
                    .Select("parent_id")
                    .Where(Exp.Eq("folder_id", toFolderId))
                    .OrderBy("level", false)
                ).ConvertAll(row => row[0]);
            }

            FactoryIndexer<FilesWrapper>.Update(
                new FilesWrapper()
                {
                    Id = (int)fileId,
                    Folders = parentFoldersIds.Select(r => new FilesFoldersWrapper() { FolderId = r.ToString() }).ToList(),
                },
                UpdateAction.Replace,
                w => w.Folders);

            return fileId;
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var file = GetFile(fileId);
            if (file != null)
            {
                var copy = new File
                {
                    FileStatus = file.FileStatus,
                    FolderID = toFolderId,
                    Title = file.Title,
                    ConvertedType = file.ConvertedType,
                    Comment = FilesCommonResource.CommentCopy,
                    Encrypted = file.Encrypted,
                };

                using (var stream = GetFileStream(file))
                {
                    copy.ContentLength = stream.CanSeek ? stream.Length : file.ContentLength;
                    copy = SaveFile(copy, stream);
                }

                if (file.ThumbnailStatus == Thumbnail.Created)
                {
                    using (var thumbnail = GetThumbnail(file))
                    {
                        SaveThumbnail(copy, thumbnail);
                    }
                    copy.ThumbnailStatus = Thumbnail.Created;
                }

                return copy;
            }
            return null;
        }

        public object FileRename(File file, string newTitle)
        {
            newTitle = Global.ReplaceInvalidCharsAndTruncate(newTitle);

            var updateQuery =
                Update("files_file")
                    .Set("title", newTitle)
                    .Set("modified_on", DateTime.UtcNow)
                    .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                    .Where("id", file.ID)
                    .Where("current_version", true);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(updateQuery);
            }

            return file.ID;
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            comment = comment ?? string.Empty;
            comment = comment.Substring(0, Math.Min(comment.Length, 255));

            var updateQuery =
                Update("files_file")
                    .Set("comment", comment)
                    .Where("id", fileId)
                    .Where("version", fileVersion);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(updateQuery);
            }

            return comment;
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
            var updateQuery =
                Update("files_file")
                    .Set("version_group = version_group + 1")
                    .Where("id", fileId)
                    .Where(Exp.Gt("version", fileVersion));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(updateQuery);
            }
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
            var selectQuery =
                        Query("files_file")
                            .Select("version_group")
                            .Where("id", fileId)
                            .Where("version", fileVersion)
                            .GroupBy("id");

            var updateQuery =
                    Update("files_file")
                        .Set("version_group = version_group - 1")
                        .Where("id", fileId)
                        .Where(Exp.Gt("version", fileVersion));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            using (var tx = dbManager.BeginTransaction())
            {
                var versionGroup = dbManager.ExecuteScalar<int>(selectQuery);

                updateQuery.Where(Exp.Gt("version_group", versionGroup));

                dbManager.ExecuteNonQuery(updateQuery);

                tx.Commit();
            }
        }

        public bool UseTrashForRemove(File file)
        {
            return file.RootFolderType != FolderType.TRASH && file.RootFolderType != FolderType.Privacy;
        }

        public static String GetUniqFileDirectory(object fileIdObject)
        {
            if (fileIdObject == null) throw new ArgumentNullException("fileIdObject");
            var fileIdInt = Convert.ToInt32(Convert.ToString(fileIdObject));
            return string.Format("folder_{0}/file_{1}", (fileIdInt / 1000 + 1) * 1000, fileIdInt);
        }

        public static String GetUniqFilePath(File file)
        {
            return file != null
                       ? GetUniqFilePath(file, "content" + FileUtility.GetFileExtension(file.PureTitle))
                       : null;
        }

        public static String GetUniqFilePath(File file, string fileTitle)
        {
            return file != null
                       ? string.Format("{0}/{1}", GetUniqFileVersionPath(file.ID, file.Version), fileTitle)
                       : null;
        }

        public static String GetUniqFileVersionPath(object fileIdObject, int version)
        {
            return fileIdObject != null
                       ? string.Format("{0}/v{1}", GetUniqFileDirectory(fileIdObject), version)
                       : null;
        }

        private void RecalculateFilesCount(IDbManager db, object folderId)
        {
            db.ExecuteNonQuery(GetRecalculateFilesCountUpdate(folderId));
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            return ChunkedUploadSessionHolder.CreateUploadSession(file, contentLength);
        }

        public File UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                using (var streamToSave = ChunkedUploadSessionHolder.UploadSingleChunk(uploadSession, stream, chunkLength))
                {
                    if (streamToSave != Stream.Null)
                    {
                        uploadSession.File = SaveFile(GetFileForCommit(uploadSession), streamToSave, uploadSession.CheckQuota);
                    }
                }

                return uploadSession.File;
            }

            ChunkedUploadSessionHolder.UploadChunk(uploadSession, stream, chunkLength);

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal || uploadSession.LastChunk)
            {
                uploadSession.BytesTotal = uploadSession.BytesUploaded;
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
            return uploadSession.File;
        }

        public async Task UploadChunkAsync(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength)
        {
            await ChunkedUploadSessionHolder.UploadChunkAsync(uploadSession, chunkStream, chunkLength);
        }

        public File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            ChunkedUploadSessionHolder.FinalizeUploadSession(uploadSession);

            var file = GetFileForCommit(uploadSession);
            SaveFile(file, null, uploadSession.CheckQuota);
            ChunkedUploadSessionHolder.Move(uploadSession, GetUniqFilePath(file), file.GetFileQuotaOwner());

            return file;
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            ChunkedUploadSessionHolder.AbortUploadSession(uploadSession);
        }

        private File GetFileForCommit(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.File.ID != null)
            {
                var file = GetFile(uploadSession.File.ID);

                if (!uploadSession.KeepVersion)
                {
                    file.Version++;
                }

                file.ContentLength = uploadSession.BytesTotal;
                file.ConvertedType = null;
                file.Comment = FilesCommonResource.CommentUpload;
                file.Encrypted = uploadSession.Encrypted;
                file.ThumbnailStatus = Thumbnail.Waiting;

                return file;
            }

            return new File
            {
                FolderID = uploadSession.File.FolderID,
                Title = uploadSession.File.Title,
                ContentLength = uploadSession.BytesTotal,
                Comment = FilesCommonResource.CommentUpload,
                Encrypted = uploadSession.Encrypted,
            };
        }

        #endregion

        #region Only in TMFileDao

        public void ReassignFiles(IEnumerable<object> fileIds, Guid newOwnerId)
        {
            var updateQuery =
                Update("files_file")
                    .Set("create_by", newOwnerId.ToString())
                    .Where(Exp.In("id", fileIds))
                    .Where("current_version", true);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(updateQuery);
            }
        }

        public List<File> GetFiles(IEnumerable<object> parentIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, string extension)
        {
            if (parentIds == null || !parentIds.Any() || filterType == FilterType.FoldersOnly) return new List<File>();

            var q = GetFileQuery(Exp.Eq("current_version", true) & Exp.In("fft.parent_id", parentIds))
                        .InnerJoin("files_folder_tree fft", Exp.EqColumns("fft.folder_id", "f.folder_id"));

            var searchByText = !string.IsNullOrEmpty(searchText);
            var searchByExtension = (filterType == FilterType.ByExtension || filterType == FilterType.ByExtensionIncludeFolders) && !string.IsNullOrEmpty(extension);

            if (searchByText || searchByExtension)
            {
                List<int> searchIds;

                var func = GetFuncForSearch(null, null, filterType, subjectGroup, subjectID, searchText, searchInContent, extension, false);

                if (FactoryIndexer<FilesWrapper>.TrySelectIds(s => func(s), out searchIds))
                {
                    q.Where(Exp.In("id", searchIds));
                }
                else
                {
                    if (searchByText)
                    {
                        q.Where(BuildSearch("title", searchText));
                    }
                    if (searchByExtension)
                    {
                        q.Where(BuildSearch("title", extension, SqlLike.EndWith));
                    }
                }
            }

            if (subjectID != Guid.Empty)
            {
                if (subjectGroup)
                {
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                }
                else
                {
                    q.Where("create_by", subjectID.ToString());
                }
            }

            switch (filterType)
            {
                case FilterType.DocumentsOnly:
                case FilterType.ImagesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.MediaOnly:
                    q.Where("category", (int)filterType);
                    break;
            }

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(q);
            }

            return fromDb.ConvertAll(ToFile);
        }

        public IEnumerable<File> Search(String searchText, bool bunch)
        {
            List<int> ids;
            if (FactoryIndexer<FilesWrapper>.TrySelectIds(s => s.MatchAll(searchText), out ids))
            {
                var query = GetFileQuery(Exp.In("id", ids) & Exp.Eq("current_version", true));

                List<object[]> fromDb;

                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                {
                    fromDb = dbManager.ExecuteList(query);
                }

                return fromDb.ConvertAll(ToFile)
                    .Where(
                        f =>
                        bunch
                            ? f.RootFolderType == FolderType.BUNCH
                            : f.RootFolderType == FolderType.USER || f.RootFolderType == FolderType.COMMON)
                    .ToList();
            }
            else
            {
                var q = GetFileQuery(Exp.Eq("current_version", true) & BuildSearch("title", searchText));

                List<object[]> fromDb;

                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                {
                    fromDb = dbManager.ExecuteList(q);
                }

                return fromDb
                    .ConvertAll(ToFile)
                    .Where(f =>
                           bunch
                                ? f.RootFolderType == FolderType.BUNCH
                                : f.RootFolderType == FolderType.USER || f.RootFolderType == FolderType.COMMON)
                    .ToList();
            }
        }


        private static void DeleteFolder(object fileId, Guid ownerId)
        {
            Global.GetStore().DeleteDirectory(ownerId, GetUniqFileDirectory(fileId));
        }

        public bool IsExistOnStorage(File file)
        {
            return Global.GetStore().IsFile(GetUniqFilePath(file));
        }

        public async Task<bool> IsExistOnStorageAsync(File file)
        {
            return await Global.GetStore().IsFileAsync(string.Empty, GetUniqFilePath(file));
        }

        private const string DiffTitle = "diff.zip";
        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(changes)) throw new ArgumentNullException("changes");
            if (differenceStream == null) throw new ArgumentNullException("differenceStream");

            changes = changes.Trim();

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(
                Update("files_file")
                    .Set("changes", changes)
                    .Where("id", file.ID)
                    .Where("version", file.Version));
            }

            Global.GetStore().Save(string.Empty, GetUniqFilePath(file, DiffTitle), differenceStream, DiffTitle);
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion = 0)
        {
            var query = Query("files_file")
                .Select("id")
                .Select("version")
                .Select("version_group")
                .Select("modified_on")
                .Select("modified_by")
                .Select("changes")
                .Select("create_on")
                .Where(Exp.Eq("id", fileId))
                .Where(Exp.Eq("forcesave", 0))
                .OrderBy("version", true);

            if (fileVersion > 0)
            {
                query.Where(Exp.Eq("version", fileVersion));
            }

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(query);
            }

            return fromDb.ConvertAll(r =>
            {
                var item = new EditHistory
                {
                    ID = Convert.ToInt32(r[0]),
                    Version = Convert.ToInt32(r[1]),
                    VersionGroup = Convert.ToInt32(r[2]),
                    ModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3])),
                    ModifiedBy = new EditHistoryAuthor { Id = (string)r[4] },
                    ChangesString = (string)r[5],
                };

                item.Key = DocumentServiceHelper.GetDocKey(item.ID, item.Version, TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6])));
                return item;
            });
        }

        public Stream GetDifferenceStream(File file)
        {
            return Global.GetStore().GetReadStream(string.Empty, GetUniqFilePath(file, DiffTitle));
        }

        public bool ContainChanges(object fileId, int fileVersion)
        {
            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                return dbManager.ExecuteScalar<int>(
                Query("files_file")
                    .SelectCount()
                    .Where(Exp.Eq("id", fileId))
                    .Where(Exp.Eq("version", fileVersion))
                    .Where(!Exp.Eq("changes", null))) > 0;
            }
        }


        private const string ThumbnailTitle = "thumb";

        public void SaveThumbnail(File file, Stream thumbnail)
        {
            if (file == null) throw new ArgumentNullException("file");

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(
                Update("files_file")
                    .Set("thumb", thumbnail != null ? Thumbnail.Created : file.ThumbnailStatus)
                    .Where("id", file.ID)
                    .Where("version", file.Version));
            }

            if (thumbnail == null) return;

            var thumnailName = ThumbnailTitle + "." + Global.ThumbnailExtension;
            Global.GetStore().Save(string.Empty, GetUniqFilePath(file, thumnailName), thumbnail, thumnailName);
        }

        public Stream GetThumbnail(File file)
        {
            var thumnailName = ThumbnailTitle + "." + Global.ThumbnailExtension;
            var path = GetUniqFilePath(file, thumnailName);
            var storage = Global.GetStore();
            if (!storage.IsFile(string.Empty, path)) throw new FileNotFoundException();
            return storage.GetReadStream(string.Empty, path);
        }

        public EntryProperties GetProperties(object fileId)
        {
            var query = Query("files_properties")
                .Select("data")
                .Where("entry_id", fileId);

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager
                .ExecuteList(Query("files_properties")
                .Select("data")
                .Where(Exp.Eq("entry_id", fileId)));
            }

            return fromDb
                .ConvertAll(r =>
                {
                    return EntryProperties.Parse((string)r[0]);
                })
                .SingleOrDefault();
        }

        public void SaveProperties(object fileId, EntryProperties entryProperties)
        {
            string data;
            if (entryProperties == null
                || string.IsNullOrEmpty(data = EntryProperties.Serialize(entryProperties)))
            {
                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                {
                    dbManager.ExecuteNonQuery(
                    Delete("files_properties")
                    .Where("entry_id", fileId));
                }

                return;
            }

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(Insert("files_properties")
                .InColumnValue("entry_id", fileId)
                .InColumnValue("data", data));
            }
        }

        #endregion

        private File ToFile(object[] r)
        {
            var result = new File
            {
                ID = Convert.ToInt32(r[0]),
                Title = (String)r[1],
                FolderID = Convert.ToInt32(r[2]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3])),
                CreateBy = new Guid((string)r[4]),
                Version = Convert.ToInt32(r[5]),
                VersionGroup = Convert.ToInt32(r[6]),
                ContentLength = Convert.ToInt64(r[7]),
                ModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[8])),
                ModifiedBy = new Guid((string)r[9]),
                RootFolderType = ParseRootFolderType(r[10]),
                RootFolderCreator = ParseRootFolderCreator(r[10]),
                RootFolderId = ParseRootFolderId(r[10]),
                Shared = Convert.ToBoolean(r[11]),
                ConvertedType = (string)r[12],
                Comment = (string)r[13],
                Encrypted = Convert.ToBoolean(r[14]),
                Forcesave = ParseForcesaveType(r[15]),
                ThumbnailStatus = (Thumbnail)Enum.Parse(typeof(Thumbnail), r[16].ToString()),
                IsFillFormDraft = Convert.ToBoolean(r[17])
            };

            SetEntryDenyProperties(result, (string)r[18]);

            return result;
        }

        private static ForcesaveType ParseForcesaveType(object v)
        {
            return v != null
                       ? (ForcesaveType)Enum.Parse(typeof(ForcesaveType), v.ToString().Substring(0, 1))
                       : default(ForcesaveType);
        }

        private static IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private Func<Selector<FilesWrapper>, Selector<FilesWrapper>> GetFuncForSearch(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, string extension, bool withSubfolders = false)
        {
            return s =>
           {
               Selector<FilesWrapper> result = null;

               if (!string.IsNullOrEmpty(searchText))
               {
                   result = searchInContent ? s.MatchAll(searchText) : s.Match(r => r.Title, searchText);
               }

               if ((filterType == FilterType.ByExtension || filterType == FilterType.ByExtensionIncludeFolders) && !string.IsNullOrEmpty(extension))
               {
                   if (result != null)
                       result.Match(r => r.Title, "*" + extension);
                   else
                       result = s.Match(r => r.Title, "*" + extension);
               }

               if (parentId != null)
               {
                   if (withSubfolders)
                   {
                       result.In(a => a.Folders.Select(r => r.FolderId), new[] { parentId.ToString() });
                   }
                   else
                   {
                       result.InAll(a => a.Folders.Select(r => r.FolderId), new[] { parentId.ToString() });
                   }
               }

               if (orderBy != null)
               {
                   switch (orderBy.SortedBy)
                   {
                       case SortedByType.Author:
                           result.Sort(r => r.CreateBy, orderBy.IsAsc);
                           break;
                       case SortedByType.Size:
                           result.Sort(r => r.ContentLength, orderBy.IsAsc);
                           break;
                       //case SortedByType.AZ:
                       //    result.Sort(r => r.Title, orderBy.IsAsc);
                       //    break;
                       case SortedByType.DateAndTime:
                           result.Sort(r => r.LastModifiedOn, orderBy.IsAsc);
                           break;
                       case SortedByType.DateAndTimeCreation:
                           result.Sort(r => r.CreateOn, orderBy.IsAsc);
                           break;
                   }
               }

               if (subjectID != Guid.Empty)
               {
                   if (subjectGroup)
                   {
                       var users = CoreContext.UserManager.GetUsersByGroup(subjectID).Select(u => u.ID.ToString()).ToArray();
                       result.In(r => r.CreateBy, users);
                   }
                   else
                   {
                       result.Where(r => r.CreateBy, subjectID);
                   }
               }

               switch (filterType)
               {
                   case FilterType.DocumentsOnly:
                   case FilterType.ImagesOnly:
                   case FilterType.PresentationsOnly:
                   case FilterType.SpreadsheetsOnly:
                   case FilterType.ArchiveOnly:
                   case FilterType.MediaOnly:
                       result.Where(r => r.Category, (int)filterType);
                       break;
               }

               return result;
           };
        }
    }
}