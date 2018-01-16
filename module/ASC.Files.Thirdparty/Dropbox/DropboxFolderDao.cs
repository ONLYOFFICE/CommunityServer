/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Web.Studio.Core;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal class DropboxFolderDao : DropboxDaoBase, IFolderDao
    {
        public DropboxFolderDao(DropboxDaoSelector.DropboxInfo dropboxInfo, DropboxDaoSelector dropboxDaoSelector)
            : base(dropboxInfo, dropboxDaoSelector)
        {
        }

        public void Dispose()
        {
            DropboxProviderInfo.Dispose();
        }

        public Folder GetFolder(object folderId)
        {
            return ToFolder(GetDropboxFolder(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            var metadata = GetDropboxItems(parentId, true)
                .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));

            return metadata == null
                       ? null
                       : ToFolder(metadata.AsFolder);
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return GetRootFolder(fileId);
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetDropboxItems(parentId, true).Select(item => ToFolder(item.AsFolder)).ToList();
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText, bool withSubfolders = false)
        {
            if (filterType == FilterType.FilesOnly || filterType == FilterType.ByExtension) return new List<Folder>();

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

        public List<Folder> GetFolders(object[] folderIds, string searchText = "", bool searchSubfolders = false, bool checkShare = true)
        {
            return folderIds.Select(GetFolder).ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var path = new List<Folder>();

            while (folderId != null)
            {
                var dropboxFolder = GetDropboxFolder(folderId);

                if (dropboxFolder is ErrorFolder)
                {
                    folderId = null;
                }
                else
                {
                    path.Add(ToFolder(dropboxFolder));
                    folderId = GetParentFolderPath(dropboxFolder);
                }
            }

            path.Reverse();
            return path;
        }

        public object SaveFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");
            if (folder.ID != null)
            {
                return RenameFolder(folder, folder.Title);
            }

            if (folder.ParentFolderID != null)
            {
                var dropboxFolderPath = MakeDropboxPath(folder.ParentFolderID);

                folder.Title = GetAvailableTitle(folder.Title, dropboxFolderPath, IsExist);

                var dropboxFolder = DropboxProviderInfo.Storage.CreateFolder(folder.Title, dropboxFolderPath);

                DropboxProviderInfo.CacheReset(dropboxFolder);
                var parentFolderPath = GetParentFolderPath(dropboxFolder);
                if (parentFolderPath != null) DropboxProviderInfo.CacheReset(parentFolderPath);

                return MakeId(dropboxFolder);
            }
            return null;
        }

        public bool IsExist(string title, string folderId)
        {
            return GetDropboxItems(folderId, true)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public void DeleteFolder(object folderId)
        {
            var dropboxFolder = GetDropboxFolder(folderId);
            var id = MakeId(dropboxFolder);

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

            if (!(dropboxFolder is ErrorFolder))
                DropboxProviderInfo.Storage.DeleteItem(dropboxFolder);

            DropboxProviderInfo.CacheReset(MakeDropboxPath(dropboxFolder), true);
            var parentFolderPath = GetParentFolderPath(dropboxFolder);
            if (parentFolderPath != null) DropboxProviderInfo.CacheReset(parentFolderPath);
        }

        public object MoveFolder(object folderId, object toFolderId)
        {
            var dropboxFolder = GetDropboxFolder(folderId);
            if (dropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)dropboxFolder).Error);

            var toDropboxFolder = GetDropboxFolder(toFolderId);
            if (toDropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toDropboxFolder).Error);

            var fromFolderPath = GetParentFolderPath(dropboxFolder);

            dropboxFolder = DropboxProviderInfo.Storage.MoveFolder(MakeDropboxPath(dropboxFolder), MakeDropboxPath(toDropboxFolder), dropboxFolder.Name);

            DropboxProviderInfo.CacheReset(MakeDropboxPath(dropboxFolder), false);
            DropboxProviderInfo.CacheReset(fromFolderPath);
            DropboxProviderInfo.CacheReset(MakeDropboxPath(toDropboxFolder));

            return MakeId(dropboxFolder);
        }

        public Folder CopyFolder(object folderId, object toFolderId)
        {
            var dropboxFolder = GetDropboxFolder(folderId);
            if (dropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)dropboxFolder).Error);

            var toDropboxFolder = GetDropboxFolder(toFolderId);
            if (toDropboxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toDropboxFolder).Error);

            var newDropboxFolder = DropboxProviderInfo.Storage.CopyFolder(MakeDropboxPath(dropboxFolder), MakeDropboxPath(toDropboxFolder), dropboxFolder.Name);

            DropboxProviderInfo.CacheReset(newDropboxFolder);
            DropboxProviderInfo.CacheReset(MakeDropboxPath(newDropboxFolder), false);
            DropboxProviderInfo.CacheReset(MakeDropboxPath(toDropboxFolder));

            return ToFolder(newDropboxFolder);
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var dropboxFolder = GetDropboxFolder(folder.ID);
            var parentFolderPath = GetParentFolderPath(dropboxFolder);

            if (IsRoot(dropboxFolder))
            {
                //It's root folder
                DropboxDaoSelector.RenameProvider(DropboxProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                newTitle = GetAvailableTitle(newTitle, parentFolderPath, IsExist);

                //rename folder
                dropboxFolder = DropboxProviderInfo.Storage.MoveFolder(MakeDropboxPath(dropboxFolder), parentFolderPath, newTitle);
            }

            DropboxProviderInfo.CacheReset(dropboxFolder);
            if (parentFolderPath != null) DropboxProviderInfo.CacheReset(parentFolderPath);

            return MakeId(dropboxFolder);
        }

        public int GetItemsCount(object folderId)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty(object folderId)
        {
            var dropboxFolderPath = MakeDropboxPath(folderId);
            //note: without cache
            return DropboxProviderInfo.Storage.GetItems(dropboxFolderPath).Count == 0;
        }

        public bool UseTrashForRemove(Folder folder)
        {
            return false;
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            return false;
        }

        public bool CanCalculateSubitems(object entryId)
        {
            return false;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var storageMaxUploadSize =
                chunkedUpload
                    ? DropboxProviderInfo.Storage.MaxChunkedUploadFileSize
                    : DropboxProviderInfo.Storage.MaxUploadFileSize;

            if (storageMaxUploadSize == -1)
                storageMaxUploadSize = long.MaxValue;

            return chunkedUpload ? storageMaxUploadSize : Math.Min(storageMaxUploadSize, SetupInfo.AvailableFileSize);
        }

        #region Only for TMFolderDao

        public IEnumerable<Folder> Search(string text, params FolderType[] folderTypes)
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

        public object GetFolderIDUser(bool createIfNotExists, Guid? userId)
        {
            return null;
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDTrash(bool createIfNotExists, Guid? userId)
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

        public Dictionary<string, string> GetBunchObjectIDs(List<object> folderIDs)
        {
            return null;
        }

        #endregion
    }
}