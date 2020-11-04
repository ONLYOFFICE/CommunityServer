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
using System.Linq;
using System.Threading;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Studio.Core;

namespace ASC.Files.Thirdparty.OneDrive
{
    internal class OneDriveFolderDao : OneDriveDaoBase, IFolderDao
    {
        public OneDriveFolderDao(OneDriveDaoSelector.OneDriveInfo onedriveInfo, OneDriveDaoSelector onedriveDaoSelector)
            : base(onedriveInfo, onedriveDaoSelector)
        {
        }

        public Folder GetFolder(object folderId)
        {
            return ToFolder(GetOneDriveItem(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            return ToFolder(GetOneDriveItems(parentId, true)
                                .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase) && item.Folder != null));
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return GetRootFolder(fileId);
        }

        public List<Folder> GetFolders(object parentId)
        {
            return GetOneDriveItems(parentId, true).Select(ToFolder).ToList();
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool withSubfolders = false)
        {
            if (filterType == FilterType.FilesOnly || filterType == FilterType.ByExtension
                || filterType == FilterType.DocumentsOnly || filterType == FilterType.ImagesOnly
                || filterType == FilterType.PresentationsOnly || filterType == FilterType.SpreadsheetsOnly
                || filterType == FilterType.ArchiveOnly || filterType == FilterType.MediaOnly)
                return new List<Folder>();

            var folders = GetFolders(parentId).AsEnumerable(); //TODO:!!!
            //Filter
            if (subjectID != Guid.Empty)
            {
                folders = folders.Where(x => subjectGroup
                                                 ? CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID)
                                                 : x.CreateBy == subjectID);
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
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.ModifiedOn) : folders.OrderByDescending(x => x.ModifiedOn);
                    break;
                case SortedByType.DateAndTimeCreation:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.CreateOn) : folders.OrderByDescending(x => x.CreateOn);
                    break;
                default:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.Title) : folders.OrderByDescending(x => x.Title);
                    break;
            }

            return folders.ToList();
        }

        public List<Folder> GetFolders(object[] folderIds, FilterType filterType = FilterType.None, bool subjectGroup = false, Guid? subjectID = null, string searchText = "", bool searchSubfolders = false, bool checkShare = true)
        {
            if (filterType == FilterType.FilesOnly || filterType == FilterType.ByExtension
                || filterType == FilterType.DocumentsOnly || filterType == FilterType.ImagesOnly
                || filterType == FilterType.PresentationsOnly || filterType == FilterType.SpreadsheetsOnly
                || filterType == FilterType.ArchiveOnly || filterType == FilterType.MediaOnly)
                return new List<Folder>();

            var folders = folderIds.Select(GetFolder);

            if (subjectID.HasValue && subjectID != Guid.Empty)
            {
                folders = folders.Where(x => subjectGroup
                                                 ? CoreContext.UserManager.IsUserInGroup(x.CreateBy, subjectID.Value)
                                                 : x.CreateBy == subjectID);
            }

            if (!string.IsNullOrEmpty(searchText))
                folders = folders.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1);

            return folders.ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var path = new List<Folder>();

            while (folderId != null)
            {
                var onedriveFolder = GetOneDriveItem(folderId);

                if (onedriveFolder is ErrorItem)
                {
                    folderId = null;
                }
                else
                {
                    path.Add(ToFolder(onedriveFolder));
                    folderId = GetParentFolderId(onedriveFolder);
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
                var onedriveFolderId = MakeOneDriveId(folder.ParentFolderID);

                folder.Title = GetAvailableTitle(folder.Title, onedriveFolderId, IsExist);

                var onedriveFolder = OneDriveProviderInfo.Storage.CreateFolder(folder.Title, onedriveFolderId);

                OneDriveProviderInfo.CacheReset(onedriveFolder.Id);
                var parentFolderId = GetParentFolderId(onedriveFolder);
                if (parentFolderId != null) OneDriveProviderInfo.CacheReset(parentFolderId);

                return MakeId(onedriveFolder);
            }
            return null;
        }

        public bool IsExist(string title, string folderId)
        {
            return GetOneDriveItems(folderId, true)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public void DeleteFolder(object folderId)
        {
            var onedriveFolder = GetOneDriveItem(folderId);
            var id = MakeId(onedriveFolder);

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

            if (!(onedriveFolder is ErrorItem))
                OneDriveProviderInfo.Storage.DeleteItem(onedriveFolder);

            OneDriveProviderInfo.CacheReset(onedriveFolder.Id);
            var parentFolderId = GetParentFolderId(onedriveFolder);
            if (parentFolderId != null) OneDriveProviderInfo.CacheReset(parentFolderId);
        }

        public object MoveFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var onedriveFolder = GetOneDriveItem(folderId);
            if (onedriveFolder is ErrorItem) throw new Exception(((ErrorItem)onedriveFolder).Error);

            var toOneDriveFolder = GetOneDriveItem(toFolderId);
            if (toOneDriveFolder is ErrorItem) throw new Exception(((ErrorItem)toOneDriveFolder).Error);

            var fromFolderId = GetParentFolderId(onedriveFolder);

            var newTitle = GetAvailableTitle(onedriveFolder.Name, toOneDriveFolder.Id, IsExist);
            onedriveFolder = OneDriveProviderInfo.Storage.MoveItem(onedriveFolder.Id, newTitle, toOneDriveFolder.Id);

            OneDriveProviderInfo.CacheReset(onedriveFolder.Id);
            OneDriveProviderInfo.CacheReset(fromFolderId);
            OneDriveProviderInfo.CacheReset(toOneDriveFolder.Id);

            return MakeId(onedriveFolder.Id);
        }

        public Folder CopyFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var onedriveFolder = GetOneDriveItem(folderId);
            if (onedriveFolder is ErrorItem) throw new Exception(((ErrorItem)onedriveFolder).Error);

            var toOneDriveFolder = GetOneDriveItem(toFolderId);
            if (toOneDriveFolder is ErrorItem) throw new Exception(((ErrorItem)toOneDriveFolder).Error);

            var newTitle = GetAvailableTitle(onedriveFolder.Name, toOneDriveFolder.Id, IsExist);
            var newOneDriveFolder = OneDriveProviderInfo.Storage.CopyItem(onedriveFolder.Id, newTitle, toOneDriveFolder.Id);

            OneDriveProviderInfo.CacheReset(newOneDriveFolder.Id);
            OneDriveProviderInfo.CacheReset(toOneDriveFolder.Id);

            return ToFolder(newOneDriveFolder);
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var onedriveFolder = GetOneDriveItem(folder.ID);
            var parentFolderId = GetParentFolderId(onedriveFolder);

            if (IsRoot(onedriveFolder))
            {
                //It's root folder
                OneDriveDaoSelector.RenameProvider(OneDriveProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                newTitle = GetAvailableTitle(newTitle, parentFolderId, IsExist);

                //rename folder
                onedriveFolder = OneDriveProviderInfo.Storage.RenameItem(onedriveFolder.Id, newTitle);
            }

            OneDriveProviderInfo.CacheReset(onedriveFolder.Id);
            if (parentFolderId != null) OneDriveProviderInfo.CacheReset(parentFolderId);

            return MakeId(onedriveFolder.Id);
        }

        public int GetItemsCount(object folderId)
        {
            var onedriveFolder = GetOneDriveItem(folderId);
            return (onedriveFolder == null
                    || onedriveFolder.Folder == null
                    || !onedriveFolder.Folder.ChildCount.HasValue)
                       ? 0
                       : onedriveFolder.Folder.ChildCount.Value;
        }

        public bool IsEmpty(object folderId)
        {
            var onedriveFolder = GetOneDriveItem(folderId);
            return onedriveFolder == null
                   || onedriveFolder.Folder == null
                   || onedriveFolder.Folder.ChildCount == 0;
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
            return true;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var storageMaxUploadSize = OneDriveProviderInfo.Storage.MaxChunkedUploadFileSize;

            return chunkedUpload ? storageMaxUploadSize : Math.Min(storageMaxUploadSize, SetupInfo.AvailableFileSize);
        }

        #region Only for TMFolderDao

        public void ReassignFolders(object[] folderIds, Guid newOwnerId)
        {
        }

        public IEnumerable<Folder> Search(string text, bool bunch)
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

        public object GetFolderIDRecent(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDFavorites(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDTemplates(bool createIfNotExists)
        {
            return null;
        }

        public object GetFolderIDPrivacy(bool createIfNotExists, Guid? userId)
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