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
using System.Linq;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderFileDao : ProviderDaoBase, IFileDao
    {
        public void Dispose()
        {
        }

        public void InvalidateCache(object fileId)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).InvalidateCache(selector.ConvertId(fileId));
        }

        public Core.File GetFile(object fileId)
        {
            var selector = GetSelector(fileId);
            var result = selector.GetFileDao(fileId).GetFile(selector.ConvertId(fileId));

            if (result != null && !Default.IsMatch(fileId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public Core.File GetFile(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            var result = selector.GetFileDao(fileId).GetFile(selector.ConvertId(fileId), fileVersion);

            if (result != null && !Default.IsMatch(fileId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public Core.File GetFile(object parentId, string title)
        {
            var selector = GetSelector(parentId);
            var result = selector.GetFileDao(parentId).GetFile(selector.ConvertId(parentId), title);

            if (result != null && !Default.IsMatch(parentId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public List<Core.File> GetFileHistory(object fileId)
        {
            var selector = GetSelector(fileId);
            return selector.GetFileDao(fileId).GetFileHistory(selector.ConvertId(fileId));
        }

        public List<Core.File> GetFiles(object[] fileIds)
        {
            var result = Enumerable.Empty<Core.File>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var mathedIds = fileIds.Where(selectorLocal.IsMatch);

                if (!mathedIds.Any()) continue;

                result = result.Concat(mathedIds.GroupBy(selectorLocal.GetIdCode)
                                                .SelectMany(y => selectorLocal.GetFileDao(y.FirstOrDefault())
                                                                              .GetFiles(y.Select(selectorLocal.ConvertId).ToArray())));
            }

            return result.ToList();
        }

        public Stream GetFileStream(Core.File file)
        {
            return GetFileStream(file, 0);
        }

        /// <summary>
        /// Get stream of file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="offset"></param>
        /// <returns>Stream</returns>
        public Stream GetFileStream(Core.File file, long offset)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var stream = selector.GetFileDao(fileId).GetFileStream(file, offset);
            file.ID = fileId; //Restore id

            return stream;
        }

        public bool IsSupportedPreSignedUri(Core.File file)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var isSupported = selector.GetFileDao(fileId).IsSupportedPreSignedUri(file);
            file.ID = fileId; //Restore id

            return isSupported;
        }

        public Uri GetPreSignedUri(Core.File file, TimeSpan expires)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var streamUri = selector.GetFileDao(fileId).GetPreSignedUri(file, expires);
            file.ID = fileId; //Restore id

            return streamUri;
        }

        public Core.File SaveFile(Core.File file, Stream fileStream)
        {
            if (file == null) throw new ArgumentNullException("file");

            var fileId = file.ID;
            var folderId = file.FolderID;

            IDaoSelector selector;
            Core.File fileSaved = null;
            //Convert
            if (fileId != null)
            {
                selector = GetSelector(fileId);
                file.ID = selector.ConvertId(fileId);
                if (folderId != null)
                    file.FolderID = selector.ConvertId(folderId);
                fileSaved = selector.GetFileDao(fileId).SaveFile(file, fileStream);
            }
            else if (folderId != null)
            {
                selector = GetSelector(folderId);
                file.FolderID = selector.ConvertId(folderId);
                fileSaved = selector.GetFileDao(folderId).SaveFile(file, fileStream);
            }

            if (fileSaved != null)
            {
                return fileSaved;
            }
            throw new ArgumentException("No file id or folder id toFolderId determine provider");
        }

        public void DeleteFile(object fileId)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).DeleteFile(selector.ConvertId(fileId));
        }

        public bool IsExist(string title, object folderId)
        {
            var selector = GetSelector(folderId);
            return selector.GetFileDao(folderId).IsExist(title, selector.ConvertId(folderId));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var selector = GetSelector(fileId);
            if (IsCrossDao(fileId, toFolderId))
            {
                var movedFile = PerformCrossDaoFileCopy(fileId, toFolderId, true);
                return movedFile.ID;
            }

            return selector.GetFileDao(fileId).MoveFile(selector.ConvertId(fileId), selector.ConvertId(toFolderId));
        }

        public Core.File CopyFile(object fileId, object toFolderId)
        {
            var selector = GetSelector(fileId);
            if (IsCrossDao(fileId, toFolderId))
            {
                return PerformCrossDaoFileCopy(fileId, toFolderId, false);
            }

            return selector.GetFileDao(fileId).CopyFile(selector.ConvertId(fileId), selector.ConvertId(toFolderId));
        }

        public object FileRename(object fileId, string newTitle)
        {
            var selector = GetSelector(fileId);
            return selector.GetFileDao(fileId).FileRename(selector.ConvertId(fileId), newTitle);
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            var selector = GetSelector(fileId);
            return selector.GetFileDao(fileId).UpdateComment(selector.ConvertId(fileId), fileVersion, comment);
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).CompleteVersion(selector.ConvertId(fileId), fileVersion);
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).ContinueVersion(selector.ConvertId(fileId), fileVersion);
        }

        public bool UseTrashForRemove(Core.File file)
        {
            var selector = GetSelector(file.ID);
            return selector.GetFileDao(file.ID).UseTrashForRemove(file);
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(Core.File file, long contentLength)
        {
            return GetFileDao(file).CreateUploadSession(ConvertId(file), contentLength);
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength)
        {
            var dao = GetFileDao(uploadSession.File);
            uploadSession.File = ConvertId(uploadSession.File);
            dao.UploadChunk(uploadSession, chunkStream, chunkLength);
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            var dao = GetFileDao(uploadSession.File);
            uploadSession.File = ConvertId(uploadSession.File);
            dao.AbortUploadSession(uploadSession);
        }

        private IFileDao GetFileDao(Core.File file)
        {
            if (file.ID != null)
                return GetSelector(file.ID).GetFileDao(file.ID);

            if (file.FolderID != null)
                return GetSelector(file.FolderID).GetFileDao(file.FolderID);

            throw new ArgumentException("Can't create instance of dao for given file.", "file");
        }

        private object ConvertId(object id)
        {
            return id != null ? GetSelector(id).ConvertId(id) : null;
        }

        private Core.File ConvertId(Core.File file)
        {
            file.ID = ConvertId(file.ID);
            file.FolderID = ConvertId(file.FolderID);
            return file;
        }

        #endregion

        #region Only in TMFileDao

        public IEnumerable<Core.File> Search(string text, FolderType folderType)
        {
            return TryGetFileDao().Search(text, folderType);
        }

        public void DeleteFileStream(object fileId)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).DeleteFileStream(selector.ConvertId(fileId));
        }

        public void DeleteFolder(object fileId)
        {
            var selector = GetSelector(fileId);
            selector.GetFileDao(fileId).DeleteFolder(selector.ConvertId(fileId));
        }

        public bool IsExistOnStorage(Core.File file)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var exist = selector.GetFileDao(fileId).IsExistOnStorage(file);
            file.ID = fileId; //Restore
            return exist;
        }

        #endregion
    }
}