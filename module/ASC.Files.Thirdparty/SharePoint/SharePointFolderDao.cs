/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.SharePoint
{
    internal class SharePointFolderDao : SharePointDaoBase, IFolderDao
    {
        public SharePointFolderDao(SharePointProviderInfo sharePointInfo, SharePointDaoSelector sharePointDaoSelector)
            : base(sharePointInfo, sharePointDaoSelector)
        {
        }

        public void Dispose()
        {
            ProviderInfo.Dispose();
        }

        public Folder GetFolder(object folderId)
        {
            return ProviderInfo.ToFolder(ProviderInfo.GetFolderById(folderId));
        }

        public Folder GetFolder(string title, object parentId)
        {
            return
                ProviderInfo.ToFolder(
                    ProviderInfo.GetFolderFolders(parentId)
                        .FirstOrDefault(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Folder GetRootFolder(object folderId)
        {
            return ProviderInfo.ToFolder(ProviderInfo.RootFolder);
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            return ProviderInfo.ToFolder(ProviderInfo.RootFolder);
        }

        public List<Folder> GetFolders(object parentId)
        {
            return ProviderInfo.GetFolderFolders(parentId).Select(r => ProviderInfo.ToFolder(r)).ToList();
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool withSubfolders = false)
        {
            if (filterType == FilterType.FilesOnly || filterType == FilterType.ByExtension
                || filterType == FilterType.DocumentsOnly || filterType == FilterType.ImagesOnly
                || filterType == FilterType.PresentationsOnly || filterType == FilterType.SpreadsheetsOnly
                || filterType == FilterType.ArchiveOnly || filterType == FilterType.MediaOnly)
                return new List<Folder>();

            var folders = GetFolders(parentId).AsEnumerable();

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
                    folders = orderBy.IsAsc
                        ? folders.OrderBy(x => x.CreateBy)
                        : folders.OrderByDescending(x => x.CreateBy);
                    break;
                case SortedByType.AZ:
                    folders = orderBy.IsAsc ? folders.OrderBy(x => x.Title) : folders.OrderByDescending(x => x.Title);
                    break;
                case SortedByType.DateAndTime:
                    folders = orderBy.IsAsc
                        ? folders.OrderBy(x => x.ModifiedOn)
                        : folders.OrderByDescending(x => x.ModifiedOn);
                    break;
                case SortedByType.DateAndTimeCreation:
                    folders = orderBy.IsAsc
                        ? folders.OrderBy(x => x.CreateOn)
                        : folders.OrderByDescending(x => x.CreateOn);
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
            var folder = ProviderInfo.GetFolderById(folderId);
            if (folder != null)
            {
                do
                {
                    path.Add(ProviderInfo.ToFolder(folder));
                } while (folder != ProviderInfo.RootFolder && !(folder is SharePointFolderErrorEntry) &&
                         (folder = ProviderInfo.GetParentFolder(folder.ServerRelativeUrl)) != null);
            }
            path.Reverse();
            return path;
        }

        public object SaveFolder(Folder folder)
        {
            if (folder.ID != null)
            {
                //Create with id
                var savedfolder = ProviderInfo.CreateFolder((string) folder.ID);
                return ProviderInfo.ToFolder(savedfolder).ID;
            }

            if (folder.ParentFolderID != null)
            {
                var parentFolder = ProviderInfo.GetFolderById(folder.ParentFolderID);

                folder.Title = GetAvailableTitle(folder.Title, parentFolder, IsExist);

                var newFolder = ProviderInfo.CreateFolder(parentFolder.ServerRelativeUrl + "/" + folder.Title);
                return ProviderInfo.ToFolder(newFolder).ID;
            }

            return null;
        }

        public bool IsExist(string title, Microsoft.SharePoint.Client.Folder folder)
        {
            return ProviderInfo.GetFolderFolders(folder.ServerRelativeUrl)
                .Any(item => item.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public void DeleteFolder(object folderId)
        {
            var folder = ProviderInfo.GetFolderById(folderId);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    var hashIDs =
                        dbManager.ExecuteList(
                            Query("files_thirdparty_id_mapping")
                                .Select("hash_id")
                                .Where(Exp.Like("id", folder.ServerRelativeUrl, SqlLike.StartWith)))
                            .ConvertAll(x => x[0]);

                    dbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                    dbManager.ExecuteNonQuery(
                        Delete("files_tag")
                            .Where(Exp.EqColumns("0",
                                Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                    dbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                    dbManager.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                    tx.Commit();
                }
            }
            ProviderInfo.DeleteFolder((string) folderId);
        }

        public object MoveFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var newFolderId = ProviderInfo.MoveFolder(folderId, toFolderId);
            UpdatePathInDB(ProviderInfo.MakeId((string) folderId), (string) newFolderId);
            return newFolderId;
        }

        public Folder CopyFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            return ProviderInfo.ToFolder(ProviderInfo.CopyFolder(folderId, toFolderId));
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var oldId = ProviderInfo.MakeId((string) folder.ID);
            var newFolderId = oldId;
            if (ProviderInfo.SpRootFolderId.Equals(folder.ID))
            {
                //It's root folder
                SharePointDaoSelector.RenameProvider(ProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                newFolderId = (string) ProviderInfo.RenameFolder(folder.ID, newTitle);
            }
            UpdatePathInDB(oldId, newFolderId);
            return newFolderId;
        }

        public int GetItemsCount(object folderId)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty(object folderId)
        {
            return ProviderInfo.GetFolderById(folderId).ItemCount == 0;
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

        public long GetMaxUploadSize(object folderId, bool chunkedUpload = false)
        {
            return 2L*1024L*1024L*1024L;
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

        public IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data,
            bool createIfNotExists)
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