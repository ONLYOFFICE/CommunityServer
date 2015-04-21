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
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;

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
            return ProviderInfo.ToFolder(ProviderInfo.GetFolderFolders(parentId).FirstOrDefault(x => x.Name.Contains(title)));
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

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            var folders = GetFolders(parentId).AsEnumerable(); 
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
            var folder = ProviderInfo.GetFolderById(folderId);
            if (folder != null)
            {
                do
                {
                    path.Add(ProviderInfo.ToFolder(folder));
                } while (folder != ProviderInfo.RootFolder && !(folder is SharePointFolderErrorEntry) && (folder = ProviderInfo.GetParentFolder(folder.ServerRelativeUrl)) != null);
            }
            path.Reverse();
            return path;
        }

        public object SaveFolder(Folder folder)
        {
            if (folder.ID != null)
            {
                //Create with id
                var savedfolder = ProviderInfo.CreateFolder((string)folder.ID);
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
            return ProviderInfo.GetFolderFolders(folder.ServerRelativeUrl).FirstOrDefault(x => x.Name.Contains(title)) != null;
        }

        public void DeleteFolder(object folderId)
        {
            var folder = ProviderInfo.GetFolderById(folderId);

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    var hashIDs = dbManager.ExecuteList(Query("files_thirdparty_id_mapping").Select("hash_id").Where(Exp.Like("id", folder.ServerRelativeUrl, SqlLike.StartWith))).ConvertAll(x => x[0]);

                    dbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                    dbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                    dbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                    dbManager.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                    tx.Commit();
                }
            }
            ProviderInfo.DeleteFolder((string)folderId);
        }

        public object MoveFolder(object folderId, object toRootFolderId)
        {
            var newFolderId = ProviderInfo.MoveFolder(folderId, toRootFolderId);
            UpdatePathInDB(ProviderInfo.MakeId((string)folderId), (string)newFolderId);
            return newFolderId;
        }

        public Folder CopyFolder(object folderId, object toRootFolderId)
        {
            return ProviderInfo.ToFolder(ProviderInfo.CopyFolder(folderId, toRootFolderId));
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            return new Dictionary<object, string>();
        }

        public object RenameFolder(object folderId, string newTitle)
        {
            var oldId = ProviderInfo.MakeId((string)folderId);
            var newFolderId = oldId;
            if (ProviderInfo.SpRootFolderId.Equals(folderId))
            {
                //It's root folder
                SharePointDaoSelector.RenameProvider(ProviderInfo, newTitle);
                //rename provider customer title
            }
            else
            {
                newFolderId = (string)ProviderInfo.RenameFolder(folderId, newTitle);
            }
            UpdatePathInDB(oldId, newFolderId);
            return newFolderId;
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            //Get only files
            var files = ProviderInfo.GetFolderFiles(parentId).Select(r => ProviderInfo.ToFile(r));
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
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Document).ToList();
                    break;
                case FilterType.PresentationsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Presentation).ToList();
                    break;
                case FilterType.SpreadsheetsOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Spreadsheet).ToList();
                    break;
                case FilterType.ImagesOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Image).ToList();
                    break;
                case FilterType.ArchiveOnly:
                    files = files.Where(x => FileUtility.GetFileTypeByFileName(x.Title) == FileType.Archive).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(searchText))
                files = files.Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1).ToList();

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
            return ProviderInfo.GetFolderFiles(parentId).Select(r=> ProviderInfo.ToFile(r).ID).ToList();
        }

        public int GetItemsCount(object folderId, bool withSubfoldes)
        {
            return ProviderInfo.GetFolderById(folderId).ItemCount;
        }

        public bool UseTrashForRemove(Folder folder)
        {
            return false;
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            return false;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload = false)
        {
            return 2L * 1024L * 1024L * 1024L;
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
