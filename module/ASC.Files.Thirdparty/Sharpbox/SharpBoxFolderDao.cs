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
using System.Net;
using System.Security;
using System.Threading;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;

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
            try
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

        public object MoveFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var entry = GetFolderById(folderId);
            var folder = GetFolderById(toFolderId);

            var oldFolderId = MakeId(entry);

            if (!SharpBoxProviderInfo.Storage.MoveFileSystemEntry(entry, folder))
                throw new Exception("Error while moving");

            var newFolderId = MakeId(entry);

            UpdatePathInDB(oldFolderId, newFolderId);

            return newFolderId;
        }

        public Folder CopyFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
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