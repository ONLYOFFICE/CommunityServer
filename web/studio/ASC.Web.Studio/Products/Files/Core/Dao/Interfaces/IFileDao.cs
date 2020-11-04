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
        ///     Receive last file without forcesave
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="fileVersion"></param>
        /// <returns></returns>
        File GetFileStable(object fileId, int fileVersion = -1);

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
        ///     Gets the file (s) by ID (s) for share
        /// </summary>
        /// <param name="fileIds">id file</param>
        /// <param name="filterType"></param>
        /// <param name="subjectGroup"></param>
        /// <param name="subjectID"></param>
        /// <param name="searchText"></param>
        /// <param name="searchInContent"></param>
        /// <returns></returns>
        List<File> GetFilesFiltered(object[] fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        List<object> GetFiles(object parentId);

        /// <summary>
        ///     Get files in folder
        /// </summary>
        /// <param name="parentId">folder id</param>
        /// <param name="orderBy"></param>
        /// <param name="filterType">filterType type</param>
        /// <param name="subjectGroup"></param>
        /// <param name="subjectID"></param>
        /// <param name="searchText"> </param>
        /// <param name="searchInContent"></param>
        /// <param name="withSubfolders"> </param>
        /// <returns>list of files</returns>
        /// <remarks>
        ///    Return only the latest versions of files of a folder
        /// </remarks>
        List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false);

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
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        File ReplaceFileVersion(File file, Stream fileStream);

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
        /// <param name="file"></param>
        /// <param name="newTitle">new name</param>
        object FileRename(File file, string newTitle);

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
        /// Set created by
        /// </summary>
        /// <param name="fileIds"></param>
        /// <param name="newOwnerId"></param>
        void ReassignFiles(object[] fileIds, Guid newOwnerId);

        /// <summary>
        /// Search files in SharedWithMe & Projects
        /// </summary>
        /// <param name="parentIds"></param>
        /// <param name="filterType"></param>
        /// <param name="subjectGroup"></param>
        /// <param name="subjectID"></param>
        /// <param name="searchText"></param>
        /// <param name="searchInContent"></param>
        /// <returns></returns>
        List<File> GetFiles(object[] parentIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent);

        /// <summary>
        /// Search the list of files containing text
        /// Only in TMFileDao
        /// </summary>
        /// <param name="text">search text</param>
        /// <param name="bunch"></param>
        /// <returns>list of files</returns>
        IEnumerable<File> Search(String text, bool bunch = false);

        /// <summary>
        ///   Checks whether file exists on storage
        /// </summary>
        /// <param name="file">file</param>
        /// <returns></returns>
        bool IsExistOnStorage(File file);

        void SaveEditHistory(File file, string changes, Stream differenceStream);

        List<EditHistory> GetEditHistory(object fileId, int fileVersion = 0);

        Stream GetDifferenceStream(File file);

        bool ContainChanges(object fileId, int fileVersion);

        #endregion
    }
}