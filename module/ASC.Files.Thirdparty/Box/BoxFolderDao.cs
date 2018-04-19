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
using Box.V2.Models;

namespace ASC.Files.Thirdparty.Box
{
    internal class BoxFolderDao : BoxDaoBase, IFolderDao
    {
        public BoxFolderDao(BoxDaoSelector.BoxInfo boxInfo, BoxDaoSelector boxDaoSelector)
            : base(boxInfo, boxDaoSelector)
        {
        }

        public void Dispose()
        {
            BoxProviderInfo.Dispose();
        }

        public Folder GetFolder(object folderId)
        {
            return ToFolder(GetBoxFolder(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            return ToFolder(GetBoxItems(parentId, true)
                                .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)) as BoxFolder);
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return GetRootFolder(fileId);
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetBoxItems(parentId, true).Select(item => ToFolder(item as BoxFolder)).ToList();
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
                var boxFolder = GetBoxFolder(folderId);

                if (boxFolder is ErrorFolder)
                {
                    folderId = null;
                }
                else
                {
                    path.Add(ToFolder(boxFolder));
                    folderId = GetParentFolderId(boxFolder);
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
                var boxFolderId = MakeBoxId(folder.ParentFolderID);

                folder.Title = GetAvailableTitle(folder.Title, boxFolderId, IsExist);

                var boxFolder = BoxProviderInfo.Storage.CreateFolder(folder.Title, boxFolderId);

                BoxProviderInfo.CacheReset(boxFolder);
                var parentFolderId = GetParentFolderId(boxFolder);
                if (parentFolderId != null) BoxProviderInfo.CacheReset(parentFolderId);

                return MakeId(boxFolder);
            }
            return null;
        }

        public bool IsExist(string title, string folderId)
        {
            return GetBoxItems(folderId, true)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public void DeleteFolder(object folderId)
        {
            var boxFolder = GetBoxFolder(folderId);
            var id = MakeId(boxFolder);

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

            if (!(boxFolder is ErrorFolder))
                BoxProviderInfo.Storage.DeleteItem(boxFolder);

            BoxProviderInfo.CacheReset(boxFolder.Id, true);
            var parentFolderId = GetParentFolderId(boxFolder);
            if (parentFolderId != null) BoxProviderInfo.CacheReset(parentFolderId);
        }

        public object MoveFolder(object folderId, object toFolderId)
        {
            var boxFolder = GetBoxFolder(folderId);
            if (boxFolder is ErrorFolder) throw new Exception(((ErrorFolder)boxFolder).Error);

            var toBoxFolder = GetBoxFolder(toFolderId);
            if (toBoxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toBoxFolder).Error);

            var fromFolderId = GetParentFolderId(boxFolder);

            boxFolder = BoxProviderInfo.Storage.MoveFolder(boxFolder.Id, toBoxFolder.Id);

            BoxProviderInfo.CacheReset(boxFolder.Id, false);
            BoxProviderInfo.CacheReset(fromFolderId);
            BoxProviderInfo.CacheReset(toBoxFolder.Id);

            return MakeId(boxFolder.Id);
        }

        public Folder CopyFolder(object folderId, object toFolderId)
        {
            var boxFolder = GetBoxFolder(folderId);
            if (boxFolder is ErrorFolder) throw new Exception(((ErrorFolder)boxFolder).Error);

            var toBoxFolder = GetBoxFolder(toFolderId);
            if (toBoxFolder is ErrorFolder) throw new Exception(((ErrorFolder)toBoxFolder).Error);

            var newBoxFolder = BoxProviderInfo.Storage.CopyFolder(boxFolder, toBoxFolder.Id);

            BoxProviderInfo.CacheReset(newBoxFolder);
            BoxProviderInfo.CacheReset(newBoxFolder.Id, false);
            BoxProviderInfo.CacheReset(toBoxFolder.Id);

            return ToFolder(newBoxFolder);
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var boxFolder = GetBoxFolder(folder.ID);
            var parentFolderId = GetParentFolderId(boxFolder);

            if (IsRoot(boxFolder))
            {
                //It's root folder
                BoxDaoSelector.RenameProvider(BoxProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                newTitle = GetAvailableTitle(newTitle, parentFolderId, IsExist);

                //rename folder
                boxFolder = BoxProviderInfo.Storage.RenameFolder(boxFolder.Id, newTitle);
            }

            BoxProviderInfo.CacheReset(boxFolder);
            if (parentFolderId != null) BoxProviderInfo.CacheReset(parentFolderId);

            return MakeId(boxFolder.Id);
        }

        public int GetItemsCount(object folderId)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty(object folderId)
        {
            var boxFolderId = MakeBoxId(folderId);
            //note: without cache
            return BoxProviderInfo.Storage.GetItems(boxFolderId, 1).Count == 0;
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
                    ? BoxProviderInfo.Storage.MaxChunkedUploadFileSize
                    : BoxProviderInfo.Storage.MaxUploadFileSize;

            if (storageMaxUploadSize == -1)
                storageMaxUploadSize = long.MaxValue;

            return chunkedUpload ? storageMaxUploadSize : Math.Min(storageMaxUploadSize, SetupInfo.AvailableFileSize);
        }

        #region Only for TMFolderDao

        public void ReassignFolders(object[] folderIds, Guid newOwnerId)
        {
        }

        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            return null;
        }

        public IEnumerable<Folder> Search(string text, FolderType folderType1, FolderType folderType2)
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