/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;
using Box.V2.Models;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.Box
{
    internal class BoxFileDao : BoxDaoBase, IFileDao
    {
        public BoxFileDao(BoxDaoSelector.BoxInfo providerInfo, BoxDaoSelector boxDaoSelector)
            : base(providerInfo, boxDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            var boxFileId = MakeBoxId(fileId);
            BoxProviderInfo.CacheReset(boxFileId, true);

            var boxFile = GetBoxFile(fileId);
            var parentPath = GetParentFolderId(boxFile);
            if (parentPath != null) BoxProviderInfo.CacheReset(parentPath);
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetBoxFile(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ToFile(GetBoxItems(parentId, false)
                              .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)) as BoxFile);
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            if (fileIds == null || fileIds.Length == 0) return new List<File>();
            return fileIds.Select(GetBoxFile).Select(ToFile).ToList();
        }

        public List<File> GetFilesForShare(object[] fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
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
            return GetBoxItems(parentId, false).Select(entry => (object)MakeId(entry.Id)).ToList();
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            if (filterType == FilterType.FoldersOnly) return new List<File>();

            //Get only files
            var files = GetBoxItems(parentId, false).Select(item => ToFile(item as BoxFile));

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
            var boxFileId = MakeBoxId(file.ID);
            BoxProviderInfo.CacheReset(boxFileId, true);

            var boxFile = GetBoxFile(file.ID);
            if (boxFile == null) throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);
            if (boxFile is ErrorFile) throw new Exception(((ErrorFile)boxFile).Error);

            var fileStream = BoxProviderInfo.Storage.DownloadStream(boxFile, (int)offset);

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

            BoxFile newBoxFile = null;

            if (file.ID != null)
            {
                var fileId = MakeBoxId(file.ID);
                newBoxFile = BoxProviderInfo.Storage.SaveStream(fileId, fileStream, file.Title);

                //https://github.com/box/box-windows-sdk-v2/issues/496
                if (!newBoxFile.Name.Equals(file.Title))
                {
                    var folderId = GetParentFolderId(GetBoxFile(fileId));
                    file.Title = GetAvailableTitle(file.Title, folderId, IsExist);
                    newBoxFile = BoxProviderInfo.Storage.RenameFile(fileId, file.Title);
                }
            }
            else if (file.FolderID != null)
            {
                var folderId = MakeBoxId(file.FolderID);
                file.Title = GetAvailableTitle(file.Title, folderId, IsExist);
                newBoxFile = BoxProviderInfo.Storage.CreateFile(fileStream, file.Title, folderId);
            }

            BoxProviderInfo.CacheReset(newBoxFile);
            var parentId = GetParentFolderId(newBoxFile);
            if (parentId != null) BoxProviderInfo.CacheReset(parentId);

            return ToFile(newBoxFile);
        }

        public void DeleteFile(object fileId)
        {
            var boxFile = GetBoxFile(fileId);
            if (boxFile == null) return;
            var id = MakeId(boxFile.Id);

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

            if (!(boxFile is ErrorFile))
                BoxProviderInfo.Storage.DeleteItem(boxFile);

            BoxProviderInfo.CacheReset(boxFile.Id, true);
            var parentFolderId = GetParentFolderId(boxFile);
            if (parentFolderId != null) BoxProviderInfo.CacheReset(parentFolderId);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetBoxItems(folderId, false)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var boxFile = GetBoxFile(fileId);
            if (boxFile is ErrorFile) throw new Exception(((ErrorFile)boxFile).Error);

            var toBoxFolder = GetBoxFolder(toFolderId);
            if (toBoxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toBoxFolder).Error);

            var fromFolderId = GetParentFolderId(boxFile);

            var newTitle = GetAvailableTitle(boxFile.Name, toBoxFolder.Id, IsExist);
            boxFile = BoxProviderInfo.Storage.MoveFile(boxFile.Id, newTitle, toBoxFolder.Id);

            BoxProviderInfo.CacheReset(boxFile.Id, true);
            BoxProviderInfo.CacheReset(fromFolderId);
            BoxProviderInfo.CacheReset(toBoxFolder.Id);

            return MakeId(boxFile.Id);
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var boxFile = GetBoxFile(fileId);
            if (boxFile is ErrorFile) throw new Exception(((ErrorFile)boxFile).Error);

            var toBoxFolder = GetBoxFolder(toFolderId);
            if (toBoxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toBoxFolder).Error);

            var newTitle = GetAvailableTitle(boxFile.Name, toBoxFolder.Id, IsExist);
            var newBoxFile = BoxProviderInfo.Storage.CopyFile(boxFile.Id, newTitle, toBoxFolder.Id);

            BoxProviderInfo.CacheReset(newBoxFile);
            BoxProviderInfo.CacheReset(toBoxFolder.Id);

            return ToFile(newBoxFile);
        }

        public object FileRename(File file, string newTitle)
        {
            var boxFile = GetBoxFile(file.ID);
            newTitle = GetAvailableTitle(newTitle, GetParentFolderId(boxFile), IsExist);

            boxFile = BoxProviderInfo.Storage.RenameFile(boxFile.Id, newTitle);

            BoxProviderInfo.CacheReset(boxFile);
            var parentId = GetParentFolderId(boxFile);
            if (parentId != null) BoxProviderInfo.CacheReset(parentId);

            return MakeId(boxFile.Id);
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

            uploadSession.Items["TempPath"] = Path.GetTempFileName();

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

            var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
            using (var fs = new FileStream(tempPath, FileMode.Append))
            {
                stream.CopyTo(fs);
            }

            uploadSession.BytesUploaded += chunkLength;

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"),
                                               FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
                {
                    uploadSession.File = SaveFile(uploadSession.File, fs);
                }
            }
            else
            {
                uploadSession.File = RestoreIds(uploadSession.File);
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