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
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderTagDao : ProviderDaoBase, ITagDao
    {
        public IEnumerable<Tag> GetTags(Guid subject, TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(subject, tagType, fileEntries);
            }
        }

        public IEnumerable<Tag> GetTags(TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(tagType, fileEntries);
            }
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch)
        {
            return GetSelector(parentFolder.ID).GetTagDao(parentFolder.ID).GetNewTags(subject, parentFolder, deepSearch);
        }

        #region Only for Teamlab Documents

        public IEnumerable<Tag> GetNewTags(Guid subject, IEnumerable<FileEntry> fileEntries)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetNewTags(subject, fileEntries);
            }
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, FileEntry fileEntry)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetNewTags(subject, fileEntry);
            }
        }

        public IEnumerable<Tag> GetTags(Guid owner, TagType tagType)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(owner, tagType);
            }
        }

        public IEnumerable<Tag> GetTags(string name, TagType tagType)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(name, tagType);
            }
        }

        public IEnumerable<Tag> GetTags(string[] names, TagType tagType)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(names, tagType);
            }
        }


        public IEnumerable<Tag> SaveTags(IEnumerable<Tag> tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.SaveTags(tag);
            }
        }

        public IEnumerable<Tag> SaveTags(Tag tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.SaveTags(tag);
            }
        }

        public void UpdateNewTags(IEnumerable<Tag> tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                tagDao.UpdateNewTags(tag);
            }
        }

        public void UpdateNewTags(Tag tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                tagDao.UpdateNewTags(tag);
            }
        }

        public void RemoveTags(IEnumerable<Tag> tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                tagDao.RemoveTags(tag);
            }
        }

        public void RemoveTags(Tag tag)
        {
            using (var tagDao = TryGetTagDao())
            {
                tagDao.RemoveTags(tag);
            }
        }

        public IEnumerable<Tag> GetTags(object entryID, FileEntryType entryType, TagType tagType)
        {
            using (var tagDao = TryGetTagDao())
            {
                return tagDao.GetTags(entryID, entryType, tagType);
            }
        }

        #endregion

        public void Dispose()
        {
        }
    }
}