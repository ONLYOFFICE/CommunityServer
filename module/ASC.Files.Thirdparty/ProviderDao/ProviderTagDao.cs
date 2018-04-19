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
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderTagDao : ProviderDaoBase, ITagDao
    {
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