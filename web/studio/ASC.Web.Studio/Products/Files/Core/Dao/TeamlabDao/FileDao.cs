/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.S3;
using ASC.FullTextIndex;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Core;
using FileShare = System.IO.FileShare;

namespace ASC.Files.Core.Data
{
    internal class FileDao : AbstractDao, IFileDao
    {
        private static readonly object syncRoot = new object();

        public FileDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {

        }

        private static Exp BuildLike(string[] columns, string[] keywords)
        {
            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike |= Exp.Like(column, keyword, SqlLike.StartWith) | Exp.Like(column, ' ' + keyword);
                }
                like &= keywordLike;
            }
            return like;
        }

        public void InvalidateCache(object fileId)
        {

        }

        public File GetFile(object fileId)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("current_version", true)))
                    .ConvertAll(ToFile)
                    .SingleOrDefault();
            }
        }

        public File GetFile(object fileId, int fileVersion)
        {
            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("version", fileVersion)))
                    .ConvertAll(ToFile)
                    .SingleOrDefault();
            }
        }

        public File GetFile(object parentId, String title)
        {
            if (String.IsNullOrEmpty(title)) throw new ArgumentNullException(title);
            using (var DbManager = GetDbManager())
            {
                var sqlQueryResult = DbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("title", title) & Exp.Eq("current_version", true) & Exp.Eq("folder_id", parentId)))
                    .ConvertAll(ToFile);

                return sqlQueryResult.Count > 0 ? sqlQueryResult[0] : null;
            }
        }

        public List<File> GetFileHistory(object fileId)
        {
            using (var DbManager = GetDbManager())
            {
                var files = DbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId)).OrderBy("version", false))
                    .ConvertAll(ToFile);

                return files;
            }
        }

        public List<File> GetFiles(object[] fileIds)
        {
            if (fileIds == null || fileIds.Length == 0) return new List<File>();

            using (var DbManager = GetDbManager())
            {
                return DbManager
                    .ExecuteList(GetFileQuery(Exp.In("id", fileIds) & Exp.Eq("current_version", true)))
                    .ConvertAll(ToFile);
            }
        }

        public Stream GetFileStream(File file, long offset)
        {
            return Global.GetStore().GetReadStream(string.Empty, GetUniqFilePath(file), (int)offset);
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            return Global.GetStore().GetPreSignedUri(string.Empty, GetUniqFilePath(file), expires,
                                                     new List<String>
                                                         {
                                                             String.Concat("Content-Disposition:", ContentDispositionUtil.GetHeaderValue(file.Title, withoutBase: true))
                                                         });
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            return Global.GetStore() is S3Storage;
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (SetupInfo.MaxChunkedUploadSize < file.ContentLength)
            {
                throw FileSizeComment.FileSizeException;
            }

            var isNew = false;

            lock (syncRoot)
            {
                using (var DbManager = GetDbManager())
                {
                    using (var tx = DbManager.BeginTransaction())
                    {
                        if (file.ID == null)
                        {
                            file.ID = DbManager.ExecuteScalar<int>(new SqlQuery("files_file").SelectMax("id")) + 1;
                            file.Version = 1;
                            file.VersionGroup = 1;
                            isNew = true;
                        }

                        file.Title = Global.ReplaceInvalidCharsAndTruncate(file.Title);

                        file.ModifiedBy = SecurityContext.CurrentAccount.ID;
                        file.ModifiedOn = TenantUtil.DateTimeNow();
                        if (file.CreateBy == default(Guid)) file.CreateBy = SecurityContext.CurrentAccount.ID;
                        if (file.CreateOn == default(DateTime)) file.CreateOn = TenantUtil.DateTimeNow();

                        DbManager.ExecuteNonQuery(
                            Update("files_file")
                                .Set("current_version", false)
                                .Where("id", file.ID)
                                .Where("current_version", true));

                        DbManager.ExecuteNonQuery(
                            Insert("files_file")
                                .InColumnValue("id", file.ID)
                                .InColumnValue("version", file.Version)
                                .InColumnValue("version_group", file.VersionGroup)
                                .InColumnValue("title", file.Title)
                                .InColumnValue("folder_id", file.FolderID)
                                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(file.CreateOn))
                                .InColumnValue("create_by", file.CreateBy.ToString())
                                .InColumnValue("content_length", file.ContentLength)
                                .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .InColumnValue("modified_by", file.ModifiedBy.ToString())
                                .InColumnValue("category", (int)file.FilterType)
                                .InColumnValue("current_version", true)
                                .InColumnValue("converted_type", file.ConvertedType)
                                .InColumnValue("comment", file.Comment));

                        var parentFoldersIds = DbManager.ExecuteList(
                            new SqlQuery("files_folder_tree")
                                .Select("parent_id")
                                .Where(Exp.Eq("folder_id", file.FolderID))
                                .OrderBy("level", false)
                            ).ConvertAll(row => row[0]);

                        if (parentFoldersIds.Count > 0)
                            DbManager.ExecuteNonQuery(
                                Update("files_folder")
                                    .Set("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                    .Set("modified_by", file.ModifiedBy.ToString())
                                    .Where(Exp.In("id", parentFoldersIds))
                                );

                        file.PureTitle = file.Title;

                        tx.Commit();
                    }

                    RecalculateFilesCount(DbManager, file.FolderID);
                }
            }
            if (fileStream != null)
            {
                try
                {
                    SaveFileStream(file, fileStream);
                }
                catch
                {
                    if (isNew)
                    {
                        DeleteFile(file.ID);
                    }
                    else if (!IsExistOnStorage(file))
                    {
                        DeleteVersion(file);
                    }
                    throw;
                }
            }
            return GetFile(file.ID);
        }

        private void DeleteVersion(File file)
        {
            if (file == null
                || file.ID == null
                || file.Version <= 1) return;

            using (var DbManager = GetDbManager())
            {
                DbManager.ExecuteNonQuery(
                    Delete("files_file")
                        .Where("id", file.ID)
                        .Where("version", file.Version));

                DbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("current_version", true)
                        .Where("id", file.ID)
                        .Where("version", file.Version - 1));
            }
        }

        private void SaveFileStream(File file, Stream stream)
        {
            Global.GetStore().Save(string.Empty, GetUniqFilePath(file), stream, file.Title);
        }

        public void DeleteFile(object fileId)
        {
            if (fileId == null) return;
            using (var DbManager = GetDbManager())
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    var fromFolders = DbManager
                        .ExecuteList(Query("files_file").Select("folder_id").Where("id", fileId).GroupBy("id"))
                        .ConvertAll(r => r[0]);

                    DbManager.ExecuteNonQuery(Delete("files_file").Where("id", fileId));
                    DbManager.ExecuteNonQuery(Delete("files_tag_link").Where("entry_id", fileId).Where("entry_type", FileEntryType.File));
                    DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                    DbManager.ExecuteNonQuery(Delete("files_security").Where("entry_id", fileId).Where("entry_type", FileEntryType.File));

                    tx.Commit();

                    fromFolders.ForEach(folderId => RecalculateFilesCount(DbManager, folderId));
                }
            }
        }

        public bool IsExist(String title, object folderId)
        {
            using (var DbManager = GetDbManager())
            {
                var fileCount = DbManager.ExecuteScalar<int>(
                    Query("files_file")
                        .SelectCount()
                        .Where("title", title)
                        .Where("folder_id", folderId));

                return fileCount != 0;
            }
        }

        public object MoveFile(object id, object toFolderId)
        {
            if (id == null) return null;
            using (var DbManager = GetDbManager())
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    var fromFolders = DbManager
                        .ExecuteList(Query("files_file").Select("folder_id").Where("id", id).GroupBy("id"))
                        .ConvertAll(r => r[0]);

                    var sqlUpdate = Update("files_file")
                        .Set("folder_id", toFolderId)
                        .Where("id", id);

                    if (Global.FolderTrash.Equals(toFolderId))
                        sqlUpdate.Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                                 .Set("modified_on", DateTime.UtcNow);

                    DbManager.ExecuteNonQuery(sqlUpdate);

                    tx.Commit();

                    fromFolders.ForEach(folderId => RecalculateFilesCount(DbManager, folderId));
                    RecalculateFilesCount(DbManager, toFolderId);
                }
            }
            return id;
        }

        public File CopyFile(object id, object toFolderId)
        {
            // copy only current version
            var file = GetFiles(new[] { id }).SingleOrDefault();
            if (file != null)
            {
                var copy = new File
                    {
                        ContentLength = file.ContentLength,
                        FileStatus = file.FileStatus,
                        FolderID = toFolderId,
                        Title = file.Title,
                        Version = file.Version,
                        VersionGroup = file.VersionGroup,
                        ConvertedType = file.ConvertedType,
                    };

                copy = SaveFile(copy, null);

                //Copy streams
                using (var stream = GetFileStream(file))
                {
                    SaveFileStream(copy, stream);
                }
                return copy;
            }
            return null;
        }

        public object FileRename(object fileId, String newTitle)
        {
            newTitle = Global.ReplaceInvalidCharsAndTruncate(newTitle);
            using (var DbManager = GetDbManager())
            {
                DbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("title", newTitle)
                        .Set("modified_on", DateTime.UtcNow)
                        .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                        .Where("id", fileId));
            }
            return fileId;
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            using (var DbManager = GetDbManager())
            {
                comment = comment.Substring(0, Math.Min(comment.Length, 255));
                DbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("comment", comment)
                        .Where("id", fileId)
                        .Where("version", fileVersion));
            }
            return comment;
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
            using (var DbManager = GetDbManager())
            {
                DbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("version_group = version_group + 1")
                        .Where("id", fileId)
                        .Where(Exp.Gt("version", fileVersion)));
            }
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
            using (var DbManager = GetDbManager())
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    var versionGroup =
                        DbManager.ExecuteScalar<int>(
                            Query("files_file")
                                .Select("version_group")
                                .Where("id", fileId)
                                .Where("version", fileVersion)
                                .GroupBy("id"));

                    DbManager.ExecuteNonQuery(
                        Update("files_file")
                            .Set("version_group = version_group - 1")
                            .Where("id", fileId)
                            .Where(Exp.Gt("version", fileVersion))
                            .Where(Exp.Gt("version_group", versionGroup)));

                    tx.Commit();
                }
            }
        }

        public bool UseTrashForRemove(File file)
        {
            return file.RootFolderType != FolderType.TRASH;
        }

        public bool IsExist(object fileId)
        {
            using (var DbManager = GetDbManager())
            {
                var count = DbManager.ExecuteScalar<int>(Query("files_file").SelectCount().Where("id", fileId));
                return count != 0;
            }
        }

        public String GetUniqFileDirectory(object fileIdObject)
        {
            if (fileIdObject == null) throw new ArgumentNullException("fileIdObject");
            var fileIdInt = Convert.ToInt32(Convert.ToString(fileIdObject));
            return string.Format("folder_{0}/file_{1}", (fileIdInt / 1000 + 1) * 1000, fileIdInt);
        }

        public String GetUniqFilePath(File file)
        {
            return file != null
                       ? string.Format("{0}/v{1}/content{2}", GetUniqFileDirectory(file.ID), file.Version, FileUtility.GetFileExtension(file.PureTitle))
                       : null;
        }

        private void RecalculateFilesCount(DbManager dbManager, object folderId)
        {
            dbManager.ExecuteNonQuery(GetRecalculateFilesCountUpdate(folderId));
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(file, contentLength) { UseChunks = false };

            var tempPath = Path.GetRandomFileName();
            var uploadId = Global.GetStore().InitiateChunkedUpload(string.Empty, tempPath);

            var uploadSession = new ChunkedUploadSession(file, contentLength);
            uploadSession.Items["TempPath"] = tempPath;
            uploadSession.Items["UploadId"] = uploadId;

            return uploadSession;
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                UploadSingleChunk(uploadSession, stream, chunkLength);
                return;
            }

            var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
            var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
            var chunkNumber = uploadSession.GetItemOrDefault<int>("ChunksUploaded") + 1;

            var eTag = Global.GetStore().UploadChunk(string.Empty, tempPath, uploadId, stream, chunkNumber, chunkLength);

            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag") ?? new List<string>();
            eTags.Add(eTag);

            uploadSession.Items["ChunksUploaded"] = chunkNumber;
            uploadSession.Items["ETag"] = eTags;
            uploadSession.BytesUploaded += chunkLength;

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
        }

        private void UploadSingleChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (uploadSession.BytesTotal == 0)
                uploadSession.BytesTotal = chunkLength;

            if (uploadSession.BytesTotal == chunkLength)
            {
                uploadSession.File = SaveFile(GetFileForCommit(uploadSession), stream);
                uploadSession.BytesUploaded = chunkLength;
            }
            else if (uploadSession.BytesTotal > chunkLength)
            {
                //This is hack fixing strange behaviour of plupload in flash mode.

                if (!uploadSession.Items.ContainsKey("ChunksBuffer"))
                    uploadSession.Items["ChunksBuffer"] = Path.GetTempFileName();

                using (var bufferStream = new FileStream(uploadSession.GetItemOrDefault<string>("ChunksBuffer"), FileMode.Append))
                {
                    stream.StreamCopyTo(bufferStream);
                }

                uploadSession.BytesUploaded += chunkLength;

                if (uploadSession.BytesTotal == uploadSession.BytesUploaded)
                {
                    using (var bufferStream = new FileStream(uploadSession.GetItemOrDefault<string>("ChunksBuffer"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
                    {
                        uploadSession.File = SaveFile(GetFileForCommit(uploadSession), bufferStream);
                    }
                }
            }
        }

        private File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
            var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag")
                                     .Select((x, i) => new KeyValuePair<int, string>(i + 1, x))
                                     .ToDictionary(x => x.Key, x => x.Value);

            Global.GetStore().FinalizeChunkedUpload(string.Empty, tempPath, uploadId, eTags);

            var file = GetFileForCommit(uploadSession);
            SaveFile(file, null);
            Global.GetStore().Move(string.Empty, tempPath, string.Empty, GetUniqFilePath(file));

            return file;
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.UseChunks)
            {
                var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
                var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
                Global.GetStore().AbortChunkedUpload(string.Empty, tempPath, uploadId);
            }
            else if (uploadSession.Items.ContainsKey("ChunksBuffer"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("ChunksBuffer"));
            }
        }

        private File GetFileForCommit(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.File.ID != null)
            {
                var file = GetFile(uploadSession.File.ID);
                file.Version++;
                file.ContentLength = uploadSession.BytesTotal;
                file.Comment = string.Empty;
                return file;
            }

            return new File { FolderID = uploadSession.File.FolderID, Title = uploadSession.File.Title, ContentLength = uploadSession.BytesTotal };
        }

        #endregion

        #region Only in TMFileDao

        public IEnumerable<File> Search(String searchText, FolderType folderType)
        {
            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var indexResult = FullTextSearch.Search(searchText, FullTextSearch.FileModule);
                var ids = indexResult.GetIdentifiers()
                                     .Where(id => !string.IsNullOrEmpty(id) && id[0] != 'd')
                                     .ToArray();

                using (var DbManager = GetDbManager())
                {
                    return DbManager
                        .ExecuteList(GetFileQuery(Exp.In("id", ids) & Exp.Eq("current_version", true)))
                        .ConvertAll(r => ToFile(r))
                        .Where(
                            f =>
                            folderType == FolderType.BUNCH
                                ? f.RootFolderType == FolderType.BUNCH
                                : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                        .ToList();
                }
            }
            else
            {
                var keywords = searchText
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToArray();

                if (keywords.Length == 0) return Enumerable.Empty<File>();

                var q = GetFileQuery(Exp.Eq("f.current_version", true) & BuildLike(new[] { "f.title" }, keywords));
                using (var DbManager = GetDbManager())
                {
                    return DbManager
                        .ExecuteList(q)
                        .ConvertAll(r => ToFile(r))
                        .Where(f =>
                               folderType == FolderType.BUNCH
                                   ? f.RootFolderType == FolderType.BUNCH
                                   : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                        .ToList();
                }
            }
        }

        public void DeleteFileStream(object fileId)
        {
            Global.GetStore().Delete(GetUniqFilePath(GetFile(fileId)));
        }

        public void DeleteFolder(object fileId)
        {
            Global.GetStore().DeleteDirectory(GetUniqFileDirectory(fileId));
        }

        public bool IsExistOnStorage(File file)
        {
            return Global.GetStore().IsFile(GetUniqFilePath(file));
        }

        #endregion
    }
}