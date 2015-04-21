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
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveFolderDao : GoogleDriveDaoBase, IFolderDao
    {
        public GoogleDriveFolderDao(GoogleDriveDaoSelector.GoogleDriveInfo googleDriveInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
            : base(googleDriveInfo, googleDriveDaoSelector)
        {
        }

        public Folder GetFolder(object folderId)
        {
            return ToFolder(GetDriveEntry(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            return ToFolder(GetDriveEntries(parentId, true)
                                .FirstOrDefault(folder => folder.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return GetRootFolder("");
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetDriveEntries(parentId, true).Select(ToFolder).ToList();
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            var folders = GetFolders(parentId).AsEnumerable(); //TODO:!!!
            //Filter
            switch (filterType)
            {
                case FilterType.ByUser:
                    folders = folders.Where(x => x.CreateBy == subjectID);
                    break;
                case FilterType.ByDepartment:
                    folders = folders.Where(x => CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID));
                    break;
                case FilterType.FoldersOnly:
                case FilterType.None:
                    break;
                default:
                    return new List<Folder>();
            }

            if (!string.IsNullOrEmpty(searchText))
                folders = folders.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1);

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.CreateBy) : folders.OrderByDescending(x => x.CreateBy);
                    break;
                case SortedByType.AZ:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.Title) : folders.OrderByDescending(x => x.Title);
                    break;
                case SortedByType.DateAndTime:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.CreateOn) : folders.OrderByDescending(x => x.CreateOn);
                    break;
                default:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.Title) : folders.OrderByDescending(x => x.Title);
                    break;
            }

            return folders.ToList();
        }

        public List<Folder> GetFolders(object[] folderIds)
        {
            return folderIds.Select(GetFolder).ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var path = new List<Folder>();

            while (folderId != null)
            {
                var driveFolder = GetDriveEntry(folderId);

                if (driveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFolder).Error);

                path.Add(ToFolder(driveFolder));
                folderId = GetParentDriveId(driveFolder);
            }

            path.Reverse();
            return path;
        }

        public object SaveFolder(Folder folder)
        {
            if (folder.ID != null)
            {
                return RenameFolder(folder.ID, folder.Title);
            }

            if (folder.ParentFolderID != null)
            {
                var driveFolderId = MakeDriveId(folder.ParentFolderID);

                var driveFolder = GoogleDriveProviderInfo.Storage.InsertEntry(null, folder.Title, driveFolderId, true);

                CacheInsert(driveFolder);
                var parentDriveId = GetParentDriveId(driveFolder);
                if (parentDriveId != null) CacheReset(parentDriveId, true);

                return MakeId(driveFolder);
            }
            return null;
        }

        public void DeleteFolder(object folderId)
        {
            var driveFolder = GetDriveEntry(folderId);
            var id = MakeId(driveFolder);

            using (var tx = DbManager.BeginTransaction())
            {
                var hashIDs = DbManager.ExecuteList(Query("files_thirdparty_id_mapping")
                                                        .Select("hash_id")
                                                        .Where(Exp.Like("id", id, SqlLike.StartWith)))
                                       .ConvertAll(x => x[0]);

                DbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                DbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                DbManager.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                tx.Commit();
            }

            if (!(driveFolder is ErrorDriveEntry))
                GoogleDriveProviderInfo.Storage.DeleteEntry(driveFolder.Id);

            CacheReset(driveFolder.Id);
            var parentDriveId = GetParentDriveId(driveFolder);
            if (parentDriveId != null) CacheReset(parentDriveId, true);
        }

        public object MoveFolder(object folderId, object toRootFolderId)
        {
            var driveFolder = GetDriveEntry(folderId);
            if (driveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFolder).Error);

            var toDriveFolder = GetDriveEntry(toRootFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var fromFolderDriveId = GetParentDriveId(driveFolder);

            GoogleDriveProviderInfo.Storage.InsertEntryIntoFolder(driveFolder.Id, toDriveFolder.Id);
            if (fromFolderDriveId != null)
            {
                GoogleDriveProviderInfo.Storage.RemoveEntryFromFolder(driveFolder.Id, fromFolderDriveId);
            }

            CacheReset(driveFolder.Id);
            CacheReset(fromFolderDriveId, true);
            CacheReset(toDriveFolder.Id, true);

            return MakeId(driveFolder.Id);
        }

        public Folder CopyFolder(object folderId, object toRootFolderId)
        {
            var driveFolder = GetDriveEntry(folderId);
            if (driveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFolder).Error);

            var toDriveFolder = GetDriveEntry(toRootFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var newDriveFolder = GoogleDriveProviderInfo.Storage.InsertEntry(null, driveFolder.Title, toDriveFolder.Id, true);

            CacheInsert(newDriveFolder);
            CacheReset(toDriveFolder.Id, true);

            return ToFolder(newDriveFolder);
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(object folderId, string newTitle)
        {
            var driveFolder = GetDriveEntry(folderId);

            if (IsRoot(driveFolder))
            {
                //It's root folder
                GoogleDriveDaoSelector.RenameProvider(GoogleDriveProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                //rename folder
                driveFolder.Title = newTitle;
                driveFolder = GoogleDriveProviderInfo.Storage.UpdateEntry(driveFolder);
            }

            CacheInsert(driveFolder);
            var parentDriveId = GetParentDriveId(driveFolder);
            if (parentDriveId != null) CacheReset(parentDriveId, true);

            return MakeId(driveFolder.Id);
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            //Get only files
            var files = GetDriveEntries(parentId, false).Select(ToFile);
            //Filter
            switch (filterType)
            {
                case FilterType.ByUser:
                    files = files.Where(x => x.CreateBy == subjectID);
                    break;
                case FilterType.ByDepartment:
                    files = files.Where(x => CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID));
                    break;
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
                    files = orderBy.IsAsc ? files.OrderBy(x => x.CreateOn) : files.OrderByDescending(x => x.CreateOn);
                    break;
                default:
                    files = orderBy.IsAsc ? files.OrderBy(x => x.Title) : files.OrderByDescending(x => x.Title);
                    break;
            }

            return files.ToList();
        }

        public List<object> GetFiles(object parentId, bool withSubfolders)
        {
            return GetDriveEntries(parentId, false).Select(entry => (object)MakeId(entry.Id)).ToList();
        }

        public int GetItemsCount(object folderId, bool withSubfoldes)
        {
            return GetDriveEntries(folderId).Count;
        }

        public bool UseTrashForRemove(Folder folder)
        {
            return false;
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            return true;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var storageMaxUploadSize =
                chunkedUpload
                    ? GoogleDriveProviderInfo.Storage.MaxChunkedUploadFileSize
                    : GoogleDriveProviderInfo.Storage.MaxUploadFileSize;

            if (storageMaxUploadSize == -1)
                storageMaxUploadSize = long.MaxValue;

            return chunkedUpload ? storageMaxUploadSize : Math.Min(storageMaxUploadSize, SetupInfo.AvailableFileSize);
        }

        #region Only for TMFolderDao

        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            return null;
        }

        public object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            return null;
        }

        public IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists)
        {
            return new List<object>();
        }

        public object GetFolderIDCommon(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDUser(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDTrash(bool createIfNotExists)
        {
            return null;
        }


        public object GetFolderIDPhotos(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDProjects(bool createIfNotExists)
        {
            return null;
        }

        public string GetBunchObjectID(object folderID)
        {
            return null;
        }

        #endregion
    }
}