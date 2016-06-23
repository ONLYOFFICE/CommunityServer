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
using AppLimit.CloudComputing.SharpBox;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal class SharpBoxFolderDao : SharpBoxDaoBase, IFolderDao
    {
        public SharpBoxFolderDao(SharpBoxDaoSelector.SharpBoxInfo sharpBoxInfo, SharpBoxDaoSelector sharpBoxDaoSelector)
            : base(sharpBoxInfo, sharpBoxDaoSelector)
        {
        }

        public Folder GetFolder(object folderId)
        {
            return ToFolder(GetFolderById(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            var parentFolder = SharpBoxProviderInfo.Storage.GetFolder(MakePath(parentId));
            return ToFolder(parentFolder.OfType<ICloudDirectoryEntry>().FirstOrDefault(x => x.Name.Equals(title, StringComparison.OrdinalIgnoreCase)));
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(RootFolder());
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return ToFolder(RootFolder());
        }

        public List<Folder> GetFolders(object parentId)
        {
            var parentFolder = SharpBoxProviderInfo.Storage.GetFolder(MakePath(parentId));
            return parentFolder.OfType<ICloudDirectoryEntry>().Select(ToFolder).ToList();
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

        public List<Folder> GetFolders(object[] folderIds, string searchText = "", bool searchSubfolders = false)
        {
            return folderIds.Select(GetFolder).ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var path = new List<Folder>();
            var folder = GetFolderById(folderId);
            if (folder != null)
            {
                do
                {
                    path.Add(ToFolder(folder));
                } while ((folder = folder.Parent) != null);
            }
            path.Reverse();
            return path;
        }

        public object SaveFolder(Folder folder)
        {
            if (folder.ID != null)
            {
                //Create with id
                var savedfolder = SharpBoxProviderInfo.Storage.CreateFolder(MakePath(folder.ID));
                return MakeId(savedfolder);
            }
            if (folder.ParentFolderID != null)
            {
                var parentFolder = GetFolderById(folder.ParentFolderID);

                folder.Title = GetAvailableTitle(folder.Title, parentFolder, IsExist);

                var newFolder = SharpBoxProviderInfo.Storage.CreateFolder(folder.Title, parentFolder);
                return MakeId(newFolder);
            }
            return null;
        }

        public void DeleteFolder(object folderId)
        {
            var folder = GetFolderById(folderId);
            var id = MakeId(folder);

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

            if (!(folder is ErrorEntry))
                SharpBoxProviderInfo.Storage.DeleteFileSystemEntry(folder);
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

        public object MoveFolder(object folderId, object toFolderId)
        {
            var oldIdValue = MakeId(GetFolderById(folderId));

            SharpBoxProviderInfo.Storage.MoveFileSystemEntry(MakePath(folderId), MakePath(toFolderId));

            var newIdValue = MakeId(GetFolderById(folderId));

            UpdatePathInDB(oldIdValue, newIdValue);

            return newIdValue;
        }

        public Folder CopyFolder(object folderId, object toFolderId)
        {
            var folder = GetFolderById(folderId);
            if (!SharpBoxProviderInfo.Storage.CopyFileSystemEntry(MakePath(folderId), MakePath(toFolderId)))
                throw new Exception("Error while copying");
            return ToFolder(GetFolderById(toFolderId).OfType<ICloudDirectoryEntry>().FirstOrDefault(x => x.Name == folder.Name));
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var entry = GetFolderById(folder.ID);

            var oldId = MakeId(entry);
            var newId = oldId;

            if ("/".Equals(MakePath(folder.ID)))
            {
                //It's root folder
                SharpBoxDaoSelector.RenameProvider(SharpBoxProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                var parentFolder = GetFolderById(folder.ParentFolderID);
                newTitle = GetAvailableTitle(newTitle, parentFolder, IsExist);

                //rename folder
                if (SharpBoxProviderInfo.Storage.RenameFileSystemEntry(entry, newTitle))
                {
                    //Folder data must be already updated by provider
                    //We can't search google folders by title because root can have multiple folders with the same name
                    //var newFolder = SharpBoxProviderInfo.Storage.GetFileSystemObject(newTitle, folder.Parent);
                    newId = MakeId(entry);
                }
            }

            UpdatePathInDB(oldId, newId);

            return newId;
        }

        public int GetItemsCount(object folderId)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty(object folderId)
        {
            return GetFolderById(folderId).Count == 0;
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
                    ? SharpBoxProviderInfo.Storage.CurrentConfiguration.Limits.MaxChunkedUploadFileSize
                    : SharpBoxProviderInfo.Storage.CurrentConfiguration.Limits.MaxUploadFileSize;

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