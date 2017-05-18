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
        /// <param name="subjectID"></param>
        /// <param name="searchText"></param>
        /// <param name="withSubfolders"></param>
        /// <returns></returns>
        List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText, bool withSubfolders = false);

        /// <summary>
        /// Gets the folder (s) by ID (s)
        /// </summary>
        /// <param name="folderIds"></param>
        /// <param name="searchText"></param>
        /// <param name="searchSubfolders"></param>
        /// <returns></returns>
        List<Folder> GetFolders(object[] folderIds, string searchText = "", bool searchSubfolders = false);

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
        object MoveFolder(object folderId, object toFolderId);

        /// <summary>
        ///     copy folder
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="toFolderId"></param>
        /// <returns> 
        /// </returns>
        Folder CopyFolder(object folderId, object toFolderId);

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
        /// Search the list of folders containing text in title
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="text"></param>
        /// <param name="folderTypes"></param>
        /// <returns></returns>
        IEnumerable<Folder> Search(string text, params FolderType[] folderTypes);

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
        /// <returns></returns>
        object GetFolderIDUser(bool createIfNotExists);

        /// <summary>
        /// Returns id folder "Shared with me"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDShare(bool createIfNotExists);

        /// <summary>
        /// Returns id folder "Trash"
        /// Only in TMFolderDao
        /// </summary>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        object GetFolderIDTrash(bool createIfNotExists);

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


        #endregion
    }
}