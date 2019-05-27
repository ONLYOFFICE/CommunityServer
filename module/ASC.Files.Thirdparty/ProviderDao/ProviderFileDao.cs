/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Files.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            using (var fileDao = selector.GetFileDao(fileId))
            {
                fileDao.InvalidateCache(selector.ConvertId(fileId));
            }
        }

        public File GetFile(object fileId)
        {
            var selector = GetSelector(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var result = fileDao.GetFile(selector.ConvertId(fileId));

                if (result != null && !Default.IsMatch(fileId))
                    SetSharedProperty(new[] {result});

                return result;
            }
        }

        public File GetFile(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var result = fileDao.GetFile(selector.ConvertId(fileId), fileVersion);

                if (result != null && !Default.IsMatch(fileId))
                    SetSharedProperty(new[] {result});

                return result;
            }
        }

        public File GetFile(object parentId, string title)
        {
            var selector = GetSelector(parentId);
            using (var fileDao = selector.GetFileDao(parentId))
            {
                var result = fileDao.GetFile(selector.ConvertId(parentId), title);

                if (result != null && !Default.IsMatch(parentId))
                    SetSharedProperty(new[] {result});

                return result;
            }
        }

        public List<File> GetFileHistory(object fileId)
        {
            var selector = GetSelector(fileId);
            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.GetFileHistory(selector.ConvertId(fileId));
            }
        }

        public List<File> GetFiles(object[] fileIds)
        {
            var result = Enumerable.Empty<File>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = fileIds.Where(selectorLocal.IsMatch);

                if (!matchedIds.Any()) continue;

                result = result.Concat(matchedIds.GroupBy(selectorLocal.GetIdCode)
                                                .SelectMany(matchedId =>
                                                {
                                                    using (var fileDao = selectorLocal.GetFileDao(matchedId.FirstOrDefault()))
                                                    {
                                                        return fileDao.GetFiles(matchedId.Select(selectorLocal.ConvertId).ToArray());
                                                    }
                                                }
                    )
                                                .Where(r => r != null));
            }

            return result.ToList();
        }

        public List<File> GetFilesForShare(object[] fileIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
        {
            var result = Enumerable.Empty<File>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = fileIds.Where(selectorLocal.IsMatch);

                if (!matchedIds.Any()) continue;

                result = result.Concat(matchedIds.GroupBy(selectorLocal.GetIdCode)
                                        .SelectMany(matchedId =>
                                        {
                                            using (var fileDao = selectorLocal.GetFileDao(matchedId.FirstOrDefault()))
                                            {
                                                return fileDao.GetFilesForShare(matchedId.Select(selectorLocal.ConvertId).ToArray(),
                                                    filterType, subjectGroup, subjectID, searchText, searchInContent);
                                            }
                                        })
                                        .Where(r => r != null));
            }

            return result.ToList();
        }

        public List<object> GetFiles(object parentId)
        {
            var selector = GetSelector(parentId);
            using (var fileDao = selector.GetFileDao(parentId))
            {
                return fileDao.GetFiles(selector.ConvertId(parentId)).Where(r => r != null).ToList();
            }
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent, bool withSubfolders = false)
        {
            var selector = GetSelector(parentId);

            using (var fileDao = selector.GetFileDao(parentId))
            {
                var result = fileDao.GetFiles(selector.ConvertId(parentId), orderBy, filterType, subjectGroup, subjectID, searchText, searchInContent, withSubfolders)
                        .Where(r => r != null).ToList();

                if (!result.Any()) return new List<File>();

                if (!Default.IsMatch(parentId))
                {
                    SetSharedProperty(result);
                }

                return result;
            }
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

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var stream = fileDao.GetFileStream(file, offset);
                file.ID = fileId; //Restore id
                return stream;
            }
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var isSupported = fileDao.IsSupportedPreSignedUri(file);
                file.ID = fileId; //Restore id
                return isSupported;
            }
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            if (file == null) throw new ArgumentNullException("file");
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var streamUri = fileDao.GetPreSignedUri(file, expires);
                file.ID = fileId; //Restore id
                return streamUri;
            }
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
                using (var fileDao = selector.GetFileDao(fileId))
                {
                    fileSaved = fileDao.SaveFile(file, fileStream);
                }
            }
            else if (folderId != null)
            {
                selector = GetSelector(folderId);
                file.FolderID = selector.ConvertId(folderId);
                using (var fileDao = selector.GetFileDao(folderId))
                {
                    fileSaved = fileDao.SaveFile(file, fileStream);
                }
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
            using (var fileDao = selector.GetFileDao(fileId))
            {
                fileDao.DeleteFile(selector.ConvertId(fileId));
            }
        }

        public bool IsExist(string title, object folderId)
        {
            var selector = GetSelector(folderId);

            using (var fileDao = selector.GetFileDao(folderId))
            {
                return fileDao.IsExist(title, selector.ConvertId(folderId));
            }
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var selector = GetSelector(fileId);
            if (IsCrossDao(fileId, toFolderId))
            {
                var movedFile = PerformCrossDaoFileCopy(fileId, toFolderId, true);
                return movedFile.ID;
            }

            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.MoveFile(selector.ConvertId(fileId), selector.ConvertId(toFolderId));
            }
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var selector = GetSelector(fileId);
            if (IsCrossDao(fileId, toFolderId))
            {
                return PerformCrossDaoFileCopy(fileId, toFolderId, false);
            }

            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.CopyFile(selector.ConvertId(fileId), selector.ConvertId(toFolderId));
            }
        }

        public object FileRename(File file, string newTitle)
        {
            var selector = GetSelector(file.ID);
            using (var fileDao = selector.GetFileDao(file.ID))
            {
                return fileDao.FileRename(ConvertId(file), newTitle);
            }
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            var selector = GetSelector(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.UpdateComment(selector.ConvertId(fileId), fileVersion, comment);
            }
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                fileDao.CompleteVersion(selector.ConvertId(fileId), fileVersion);
            }
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            using (var fileDao = selector.GetFileDao(fileId))
            {
                fileDao.ContinueVersion(selector.ConvertId(fileId), fileVersion);
            }
        }

        public bool UseTrashForRemove(File file)
        {
            var selector = GetSelector(file.ID);
            using (var fileDao = selector.GetFileDao(file.ID))
            {
                return fileDao.UseTrashForRemove(file);
            }
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            using (var fileDao = GetFileDao(file))
            {
                return fileDao.CreateUploadSession(ConvertId(file), contentLength);
            }
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream chunkStream, long chunkLength)
        {
            using (var fileDao = GetFileDao(uploadSession.File))
            {
                uploadSession.File = ConvertId(uploadSession.File);
                fileDao.UploadChunk(uploadSession, chunkStream, chunkLength);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            using (var fileDao = GetFileDao(uploadSession.File))
            {
                uploadSession.File = ConvertId(uploadSession.File);
                fileDao.AbortUploadSession(uploadSession);
            }
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

        public void ReassignFiles(object[] fileIds, Guid newOwnerId)
        {
            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = fileIds.Where(selectorLocal.IsMatch);

                if (!matchedIds.Any()) continue;

                foreach (var matchedId in matchedIds.GroupBy(selectorLocal.GetIdCode))
                {
                    using (var fileDao = selectorLocal.GetFileDao(matchedId.FirstOrDefault()))
                    {
                        fileDao.ReassignFiles(matchedId.Select(selectorLocal.ConvertId).ToArray(), newOwnerId);
                    }
                }
            }
        }

        public List<File> GetFiles(object[] parentIds, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool searchInContent)
        {
            var result = Enumerable.Empty<File>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = parentIds.Where(selectorLocal.IsMatch);

                if (!matchedIds.Any()) continue;

                result = result.Concat(matchedIds.GroupBy(selectorLocal.GetIdCode)
                                                .SelectMany(matchedId =>
                                                {
                                                    using (var fileDao = selectorLocal.GetFileDao(matchedId.FirstOrDefault()))
                                                    {
                                                        return fileDao.GetFiles(matchedId.Select(selectorLocal.ConvertId).ToArray(),
                                                            filterType, subjectGroup, subjectID, searchText, searchInContent);
                                                    }
                                                }));
            }

            return result.Distinct().ToList();
        }

        public IEnumerable<File> Search(string text, bool bunch)
        {
            using (var fileDao = TryGetFileDao())
            {
                return fileDao.Search(text, bunch);
            }
        }

        public bool IsExistOnStorage(File file)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);
            using (var fileDao = selector.GetFileDao(fileId))
            {
                var exist = fileDao.IsExistOnStorage(file);
                file.ID = fileId; //Restore
                return exist;
            }
        }

        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                fileDao.SaveEditHistory(file, changes, differenceStream);
                file.ID = fileId; //Restore
            }
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.GetEditHistory(selector.ConvertId(fileId), fileVersion);
            }
        }

        public Stream GetDifferenceStream(File file)
        {
            var fileId = file.ID;
            var selector = GetSelector(fileId);
            file.ID = selector.ConvertId(fileId);

            using (var fileDao = selector.GetFileDao(fileId))
            {
                var stream = fileDao.GetDifferenceStream(file);
                file.ID = fileId; //Restore
                return stream;
            }
        }

        public bool ContainChanges(object fileId, int fileVersion)
        {
            var selector = GetSelector(fileId);
            using (var fileDao = selector.GetFileDao(fileId))
            {
                return fileDao.ContainChanges(selector.ConvertId(fileId), fileVersion);
            }
        }

        #endregion
    }
}