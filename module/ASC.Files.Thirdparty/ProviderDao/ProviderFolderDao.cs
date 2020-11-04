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


using System.Threading;
using ASC.Files.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderFolderDao : ProviderDaoBase, IFolderDao
    {
        public void Dispose()
        {
        }

        public Folder GetFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                var result = folderDao.GetFolder(selector.ConvertId(folderId));

                if (result != null && !Default.IsMatch(folderId))
                {
                    SetSharedProperty(new[] {result});
                }

                return result;
            }
        }

        public Folder GetFolder(string title, object parentId)
        {
            var selector = GetSelector(parentId);
            return selector.GetFolderDao(parentId).GetFolder(title, selector.ConvertId(parentId));
        }

        public Folder GetRootFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.GetRootFolder(selector.ConvertId(folderId));
            }
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            var selector = GetSelector(fileId);
            using (var folderDao = selector.GetFolderDao(fileId))
            {
                return folderDao.GetRootFolderByFile(selector.ConvertId(fileId));
            }
        }

        public List<Folder> GetFolders(object parentId)
        {
            var selector = GetSelector(parentId);
            using (var folderDao = selector.GetFolderDao(parentId))
            {
                return folderDao
                        .GetFolders(selector.ConvertId(parentId))
                        .Where(r => r != null)
                        .ToList();
            }
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, bool subjectGroup, Guid subjectID, string searchText, bool withSubfolders = false)
        {
            var selector = GetSelector(parentId);
            using (var folderDao = selector.GetFolderDao(parentId))
            {
                var result = folderDao.GetFolders(selector.ConvertId(parentId), orderBy, filterType, subjectGroup, subjectID, searchText, withSubfolders)
                        .Where(r => r != null).ToList();

                if (!result.Any()) return new List<Folder>();

                if (!Default.IsMatch(parentId))
                {
                    SetSharedProperty(result);
                }

                return result;
            }
        }

        public List<Folder> GetFolders(object[] folderIds, FilterType filterType = FilterType.None, bool subjectGroup = false, Guid? subjectID = null, string searchText = "", bool searchSubfolders = false, bool checkShare = true)
        {
            var result = Enumerable.Empty<Folder>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = folderIds.Where(selectorLocal.IsMatch).ToList();

                if (!matchedIds.Any()) continue;

                result = result.Concat(matchedIds.GroupBy(selectorLocal.GetIdCode)
                                                .SelectMany(matchedId =>
                                                {
                                                    using (var folderDao = selectorLocal.GetFolderDao(matchedId.FirstOrDefault()))
                                                    {
                                                        return folderDao
                                                            .GetFolders(matchedId.Select(selectorLocal.ConvertId).ToArray(),
                                                                filterType, subjectGroup, subjectID, searchText, searchSubfolders, checkShare);
                                                    }
                                                })
                                                .Where(r => r != null));
            }

            return result.Distinct().ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.GetParentFolders(selector.ConvertId(folderId));
            }
        }

        public object SaveFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");

            if (folder.ID != null)
            {
                var folderId = folder.ID;
                var selector = GetSelector(folderId);
                folder.ID = selector.ConvertId(folderId);
                using (var folderDao = selector.GetFolderDao(folderId))
                {
                    var newFolderId = folderDao.SaveFolder(folder);
                    folder.ID = folderId;
                    return newFolderId;
                }
            }
            if (folder.ParentFolderID != null)
            {
                var folderId = folder.ParentFolderID;
                var selector = GetSelector(folderId);
                folder.ParentFolderID = selector.ConvertId(folderId);
                using (var folderDao = selector.GetFolderDao(folderId))
                {
                    var newFolderId = folderDao.SaveFolder(folder);
                    folder.ParentFolderID = folderId;
                    return newFolderId;
                }

            }
            throw new ArgumentException("No folder id or parent folder id to determine provider");
        }

        public void DeleteFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                folderDao.DeleteFolder(selector.ConvertId(folderId));
            }
        }

        public object MoveFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var selector = GetSelector(folderId);
            if (IsCrossDao(folderId, toFolderId))
            {
                var newFolder = PerformCrossDaoFolderCopy(folderId, toFolderId, true, cancellationToken);
                return newFolder != null ? newFolder.ID : null;
            }
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.MoveFolder(selector.ConvertId(folderId), selector.ConvertId(toFolderId), null);
            }
        }

        public Folder CopyFolder(object folderId, object toFolderId, CancellationToken? cancellationToken)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return IsCrossDao(folderId, toFolderId)
                    ? PerformCrossDaoFolderCopy(folderId, toFolderId, false, cancellationToken)
                    : folderDao.CopyFolder(selector.ConvertId(folderId), selector.ConvertId(toFolderId), null);
            }
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            if (!folderIds.Any()) return new Dictionary<object, string>();

            var selector = GetSelector(to);
            var matchedIds = folderIds.Where(selector.IsMatch).ToArray();

            if (!matchedIds.Any()) return new Dictionary<object, string>();

            using (var folderDao = selector.GetFolderDao(matchedIds.FirstOrDefault()))
            {
                return folderDao.CanMoveOrCopy(matchedIds, to);
            }
        }

        public object RenameFolder(Folder folder, string newTitle)
        {
            var folderId = folder.ID;
            var selector = GetSelector(folderId);
            folder.ID = selector.ConvertId(folderId);
            folder.ParentFolderID = selector.ConvertId(folder.ParentFolderID);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.RenameFolder(folder, newTitle);
            }
        }

        public int GetItemsCount(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.GetItemsCount(selector.ConvertId(folderId));
            }
        }

        public bool IsEmpty(object folderId)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                return folderDao.IsEmpty(selector.ConvertId(folderId));
            }
        }

        public bool UseTrashForRemove(Folder folder)
        {
            var selector = GetSelector(folder.ID);
            using (var folderDao = selector.GetFolderDao(folder.ID))
            {
                return folderDao.UseTrashForRemove(folder);
            }
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            var selector = GetSelector(folderId);
            bool useRecursive;

            using (var folderDao = selector.GetFolderDao(folderId))
            {
                useRecursive = folderDao.UseRecursiveOperation(folderId, null);
            }
            if (toRootFolderId != null)
            {
                var toFolderSelector = GetSelector(toRootFolderId);

                using (var folderDao = toFolderSelector.GetFolderDao(toRootFolderId))
                {
                    useRecursive = useRecursive && folderDao.UseRecursiveOperation(folderId, toFolderSelector.ConvertId(toRootFolderId));
                }
            }
            return useRecursive;
        }

        public bool CanCalculateSubitems(object entryId)
        {
            var selector = GetSelector(entryId);
            using (var folderDao = selector.GetFolderDao(entryId))
            {
                return folderDao.CanCalculateSubitems(entryId);
            }
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var selector = GetSelector(folderId);
            using (var folderDao = selector.GetFolderDao(folderId))
            {
                var storageMaxUploadSize = folderDao.GetMaxUploadSize(selector.ConvertId(folderId), chunkedUpload);

                if (storageMaxUploadSize == -1 || storageMaxUploadSize == long.MaxValue)
                    storageMaxUploadSize = 1024L*1024L*1024L;

                return storageMaxUploadSize;
            }
        }

        #region Only for TMFolderDao

        public void ReassignFolders(object[] folderIds, Guid newOwnerId)
        {
            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var matchedIds = folderIds.Where(selectorLocal.IsMatch);

                if (!matchedIds.Any()) continue;

                foreach (var matchedId in matchedIds.GroupBy(selectorLocal.GetIdCode))
                {
                    using (var folderDao = selectorLocal.GetFolderDao(matchedId.FirstOrDefault()))
                    {
                        folderDao.ReassignFolders(matchedId.Select(selectorLocal.ConvertId).ToArray(), newOwnerId);
                    }
                }
            }
        }

        public IEnumerable<Folder> Search(string text, bool bunch)
        {
            using (var folderDao = TryGetFolderDao())
            {
                return folderDao.Search(text, bunch);
            }
        }

        public object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderID(module, bunch, data, createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists)
        {
            using (var folderDao = TryGetFolderDao())
            {
                return folderDao.GetFolderIDs(module, bunch, data, createIfNotExists);
            }
        }

        public object GetFolderIDCommon(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDCommon(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDProjects(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDProjects(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDPhotos(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDProjects(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public string GetBunchObjectID(object folderID)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetBunchObjectID(folderID);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public Dictionary<string, string> GetBunchObjectIDs(List<object> folderIDs)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetBunchObjectIDs(folderIDs);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDUser(bool createIfNotExists, Guid? userId = null)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDUser(createIfNotExists, userId);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDShare(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDRecent(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDRecent(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDFavorites(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDFavorites(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDTemplates(bool createIfNotExists)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDTemplates(createIfNotExists);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDPrivacy(bool createIfNotExists, Guid? userId = null)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDPrivacy(createIfNotExists, userId);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        public object GetFolderIDTrash(bool createIfNotExists, Guid? userId = null)
        {
            foreach (var selector in GetSelectors())
            {
                using (var folderDao = selector.GetFolderDao(null))
                {
                    var folderId = folderDao.GetFolderIDTrash(createIfNotExists, userId);
                    if (folderId != null) return folderId;
                }
            }
            return null;
        }

        #endregion
    }
}