/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace ASC.Files.Core
{
    /// <summary>
    ///    Interface encapsulates access toFolderId files
    /// </summary>
    public interface IFileDao : IDisposable
    {
        /// <summary>
        ///     Clear the application cache for the specific file
        /// </summary>
        void InvalidateCache(object fileId);

        /// <summary>
        ///     Receive file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns></returns>
        File GetFile(object fileId);

        /// <summary>
        ///     Receive file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="fileVersion">file version</param>
        /// <returns></returns>
        File GetFile(object fileId, int fileVersion);

        /// <summary>
        ///     Receive file
        /// </summary>
        /// <param name="parentId">folder id</param>
        /// <param name="title">file name</param>
        /// <returns>
        ///   file
        /// </returns>
        File GetFile(object parentId, String title);

        /// <summary>
        ///  Returns all versions of the file
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        List<File> GetFileHistory(object fileId);

        /// <summary>
        ///     Gets the file (s) by ID (s)
        /// </summary>
        /// <param name="fileIds">id file</param>
        /// <returns></returns>
        List<File> GetFiles(object[] fileIds);

        /// <summary>
        /// Get stream of file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Stream</returns>
        Stream GetFileStream(File file);

        /// <summary>
        /// Get stream of file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="offset"></param>
        /// <returns>Stream</returns>
        Stream GetFileStream(File file, long offset);

        /// <summary>
        /// Get presigned uri
        /// </summary>
        /// <param name="file"></param>
        /// <param name="expires"></param>
        /// <returns>Stream uri</returns>
        Uri GetPreSignedUri(File file, TimeSpan expires);

        /// <summary>
        ///  Check is supported PreSignedUri
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Stream uri</returns>
        bool IsSupportedPreSignedUri(File file);

        /// <summary>
        ///  Saves / updates the version of the file
        ///  and save stream of file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileStream"> </param>
        /// <returns></returns>
        /// <remarks>
        /// Updates the file if:
        /// - The file comes with the given id
        /// - The file with that name in the folder / container exists
        ///
        /// Save in all other cases
        /// </remarks>
        File SaveFile(File file, Stream fileStream);

        /// <summary>
        ///   Deletes a file including all previous versions
        /// </summary>
        /// <param name="fileId">file id</param>
        void DeleteFile(object fileId);

        /// <summary>
        ///     Checks whether or not file
        /// </summary>
        /// <param name="title">file name</param>
        /// <param name="folderId">folder id</param>
        /// <returns>Returns true if the file exists, otherwise false</returns>
        bool IsExist(String title, object folderId);

        /// <summary>
        ///   Moves a file or set of files in a folder
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="toFolderId">The ID of the destination folder</param>
        object MoveFile(object fileId, object toFolderId);

        /// <summary>
        ///  Copy the files in a folder
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="toFolderId">The ID of the destination folder</param>
        File CopyFile(object fileId, object toFolderId);

        /// <summary>
        ///   Rename file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="newTitle">new name</param>
        object FileRename(object fileId, String newTitle);

        /// <summary>
        ///   Update comment file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="fileVersion">file version</param>
        /// <param name="comment">new comment</param>
        string UpdateComment(object fileId, int fileVersion, String comment);

        /// <summary>
        ///   Complete file version
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="fileVersion">file version</param>
        void CompleteVersion(object fileId, int fileVersion);

        /// <summary>
        ///   Continue file version
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="fileVersion">file version</param>
        void ContinueVersion(object fileId, int fileVersion);

        /// <summary>
        /// Check the need to use the trash before removing
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool UseTrashForRemove(File file);

        #region chunking

        ChunkedUploadSession CreateUploadSession(File file, long contentLength);

        void UploadChunk(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength);

        void AbortUploadSession(ChunkedUploadSession uploadSession);

        #endregion

        #region Only in TMFileDao

        /// <summary>
        /// Search the list of files containing text
        /// Only in TMFileDao
        /// </summary>
        /// <param name="text">search text</param>
        /// <param name="folderType">type of parent folder</param>
        /// <returns>list of files</returns>
        IEnumerable<File> Search(String text, FolderType folderType);

        /// <summary>
        /// Delete streama of file
        /// Only in TMFileDao
        /// </summary>
        /// <param name="fileId"></param>
        void DeleteFileStream(object fileId);

        /// <summary>
        /// Delete parent folder on storage
        /// Only in TMFileDao
        /// </summary>
        /// <param name="fileId"></param>
        void DeleteFolder(object fileId);

        /// <summary>
        ///   Checks whether file exists on storage
        /// </summary>
        /// <param name="file">file</param>
        /// <returns></returns>
        bool IsExistOnStorage(File file);

        #endregion
    }
}