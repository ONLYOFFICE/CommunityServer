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
using System.Threading;

namespace ASC.Files.Core
{
    public interface IFolderDao : IDisposable
    {
        /// <summary>
        ///     Get folder by id.
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <returns>folder</returns>
        Folder GetFolder(object folderId);

        /// <summary>
        ///     Returns the folder with the given name and id of the root
        /// </summary>
        /// <param name="title"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Folder GetFolder(String title, object parentId);

        /// <summary>
        ///    Gets the root folder
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <returns>root folder</returns>
        Folder GetRootFolder(object folderId);

        /// <summary>
        ///    Gets the root folder
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns>root folder</returns>
        Folder GetRootFolderByFile(object fileId);

        /// <summary>
        ///     Get a list of folders in current folder.
        /// </summary>
        /// <param name="parentId"></param>
        List<Folder> GetFolders(object parentId);

        /// <summary>
        /// Get a list of folders.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="orderBy"></param>
        /// <param name="filterType"></param>
        /// <param name="subjectGroup"></param>
        /// <param name="subjectID"></param>
        /// <param name="searchText"></param>
        /// <param name="withSubfolders"></param>
        /// <returns></returns>
        List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool withSubfolders = false);

        /// <summary>
        /// Gets the folder (s) by ID (s)
        /// </summary>
        /// <param name="folderIds"></param>
        /// <param name="filterType"></param>
        /// <param name="subjectGroup"></param>
        /// <param name="subjectID"></param>
        /// <param name="searchText"></param>
        /// <param name="searchSubfolders"></param>
        /// <param name="checkShare"></param>
        /// <returns></returns>
        List<Folder> GetFolders(object[] folderIds, FilterType filterType = FilterType.None, bool subjectGroup = false, Guid? subjectID = null, string searchText = "", bool searchSubfolders = false, bool checkShare = true);

        /// <summary>
        ///     Get folder, contains folder with id
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <returns></returns>
        List<Folder> GetParentFolders(object folderId);

        /// <summary>
        ///     save or update folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        object SaveFolder(Folder folder);

        /// <summary>
        ///     delete folder
        /// </summary>
        /// <param name="folderId">folder id</param>
        void DeleteFolder(object folderId);

        /// <summary>
        ///  move folder
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <param name="toFolderId">destination folder id</param>
        /// <param name="cancellationToken"></param>
        object MoveFolder(object folderId, object toFolderId, CancellationToken? cancellationToken);

        /// <summary>
        ///     copy folder
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="toFolderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns> 
        /// </returns>
        Folder CopyFolder(object folderId, object toFolderId, CancellationToken? cancellationToken);

        /// <summary>
        /// Validate the transfer operation directory to another directory.
        /// </summary>
        /// <param name="folderIds"></param>
        /// <param name="to"></param>
        /// <returns>
        /// Returns pair of file ID, file name, in which the same name.
        /// </returns>
        IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to);

        /// <summary>
        ///     Rename folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="newTitle">new name</param>
        object RenameFolder(Folder folder, string newTitle);

        /// <summary>
        ///    Gets the number of files and folders to the container in your
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <returns></returns>
        int GetItemsCount(object folderId);

        /// <summary>
        ///    Check folder on emptiness
        /// </summary>
        /// <param name="folderId">folder id</param>
        /// <returns></returns>
        bool IsEmpty(object folderId);

        /// <summary>
        /// Check the need to use the trash before removing
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool UseTrashForRemove(Folder folder);

        /// <summary>
        /// Check the need to use recursion for operations
        /// </summary>
        /// <param name="folderId"> </param>
        /// <param name="toRootFolderId"> </param>
        /// <returns></returns>
        bool UseRecursiveOperation(object folderId, object toRootFolderId);

        /// <summary>
        /// Check the possibility to calculate the number of subitems
        /// </summary>
        /// <param name="entryId"> </param>
        /// <returns></returns>
        bool CanCalculateSubitems(object entryId);

        /// <summary>
        /// Returns maximum size of file which can be uploaded to specific folder
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <param name="chunkedUpload">Determines whenever supposed upload will be chunked (true) or not (false)</param>
        /// <returns>Maximum size of file which can be uploaded to folder</returns>
        long GetMaxUploadSize(object folderId, bool chunkedUpload = false);

        #region Only for TMFolderDao

        /// <summary>
        /// Set created by
        /// </summary>
        /// <param name="folderIds"></param>
        /// <param name="newOwnerId"></param>
        void ReassignFolders(object[] folderIds, Guid newOwnerId);

        /// <summary>
        /// Search the list of folders containing text in title
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bunch"></param>
        /// <returns></returns>
        IEnumerable<Folder> Search(string text, bool bunch = false);

        /// <summary>
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="module"></param>
        /// <param name="bunch"></param>
        /// <param name="data"></param>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderID(string module, string bunch, string data, bool createIfNotExists);

        IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists);

        /// <summary>
        ///  Returns id folder "Shared Documents"
        /// Only in TMFolderDao
        /// </summary>
        /// <returns></returns>
        object GetFolderIDCommon(bool createIfNotExists);

        /// <summary>
        ///  Returns id folder "My Documents"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        object GetFolderIDUser(bool createIfNotExists, Guid? userId = null);

        /// <summary>
        /// Returns id folder "Shared with me"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDShare(bool createIfNotExists);

        /// <summary>
        /// Returns id folder "Recent"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDRecent(bool createIfNotExists);

        /// <summary>

        /// <summary>
        /// Returns id folder "Favorites"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDFavorites(bool createIfNotExists);

        /// <summary>
        /// Returns id folder "Templates"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDTemplates(bool createIfNotExists);

        /// <summary>
        /// Returns id folder "Privacy"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDPrivacy(bool createIfNotExists, Guid? userId = null);

        /// <summary>
        /// Returns id folder "Trash"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        object GetFolderIDTrash(bool createIfNotExists, Guid? userId = null);

        /// <summary>
        /// Returns id folder "Projects"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDProjects(bool createIfNotExists);


        /// <summary>
        /// Return id of related object
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="folderID"></param>
        /// <returns></returns>
        String GetBunchObjectID(object folderID);

        /// <summary>
        /// Return ids of related objects
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="folderIDs"></param>
        /// <returns></returns>
        Dictionary<string, string> GetBunchObjectIDs(List<object> folderIDs);

        #endregion
    }
}