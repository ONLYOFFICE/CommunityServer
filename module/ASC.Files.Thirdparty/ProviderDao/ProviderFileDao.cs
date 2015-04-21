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
using System.IO;
using System.Linq;
using ASC.Files.Core;
using File = ASC.Files.Core.File;

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

        public File GetFile(object fileId)
        {
            var selector = GetSelector(fileId);
            var result = selector.GetFileDao(fileId).GetFile(selector.ConvertId(fileId));

            if (result != null && !Default.IsMatch(fileId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public File GetFile(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            var result = selector.GetFileDao(fileId).GetFile(selector.ConvertId(fileId), fileVersion);

            if (result != null && !Default.IsMatch(fileId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public File GetFile(object parentId, string title)
        {
            var selector = GetSelector(parentId);
            var result = selector.GetFileDao(parentId).GetFile(selector.ConvertId(parentId), title);

            if (result != null && !Default.IsMatch(parentId))
                SetSharedByMeProperty(new[] { result });

            return result;
        }

        public List<File> GetFileHistory(object fileId)
        {
            var selector = GetSelector(fileId);
            return selector.GetFileDao(fileId).GetFileHistory(selector.ConvertId(fileId));
        }

        public List<File> GetFiles(object[] fileIds)
        {
            var result = Enumerable.Empty<File>();

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

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        /// <summary>
        /// Get stream of file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="offset"></param>
        /// <returns>Stream</returns>
        public Stream GetFileStream(File file, long offset)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var stream = selector.GetFileDao(fileId).GetFileStream(file, offset);
            file.ID = fileId; //Restore id

            return stream;
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var isSupported = selector.GetFileDao(fileId).IsSupportedPreSignedUri(file);
            file.ID = fileId; //Restore id

            return isSupported;
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var streamUri = selector.GetFileDao(fileId).GetPreSignedUri(file, expires);
            file.ID = fileId; //Restore id

            return streamUri;
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (file == null) throw new ArgumentNullException("file");

            var fileId = file.ID;
            var folderId = file.FolderID;

            IDaoSelector selector;
            File fileSaved = null;
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

        public File CopyFile(object fileId, object toFolderId)
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

        public bool UseTrashForRemove(File file)
        {
            var selector = GetSelector(file.ID);
            return selector.GetFileDao(file.ID).UseTrashForRemove(file);
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
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

        private IFileDao GetFileDao(File file)
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

        private File ConvertId(File file)
        {
            file.ID = ConvertId(file.ID);
            file.FolderID = ConvertId(file.FolderID);
            return file;
        }

        #endregion

        #region Only in TMFileDao

        public IEnumerable<File> Search(string text, FolderType folderType)
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

        public bool IsExistOnStorage(File file)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var exist = selector.GetFileDao(fileId).IsExistOnStorage(file);
            file.ID = fileId; //Restore
            return exist;
        }

        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            selector.GetFileDao(fileId).SaveEditHistory(file, changes, differenceStream);
            file.ID = fileId; //Restore
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            return selector.GetFileDao(fileId).GetEditHistory(selector.ConvertId(fileId), fileVersion);
        }

        public string GetDifferenceUrl(File file)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            var url = selector.GetFileDao(fileId).GetDifferenceUrl(file);
            file.ID = fileId; //Restore
            return url;
        }

        #endregion
    }
}