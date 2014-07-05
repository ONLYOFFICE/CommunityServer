/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderTagDao : ProviderDaoBase, ITagDao
    {
        public IEnumerable<Tag> GetTags(TagType tagType, params FileEntry[] fileEntries)
        {
            return TryGetTagDao().GetTags(tagType, fileEntries);
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch)
        {
            return GetSelector(parentFolder.ID).GetTagDao(parentFolder.ID).GetNewTags(subject, parentFolder, deepSearch);
        }

        #region Only for Teamlab Documents

        public IEnumerable<Tag> GetNewTags(Guid subject, params FileEntry[] fileEntries)
        {
            return TryGetTagDao().GetNewTags(subject, fileEntries);
        }

        public IEnumerable<Tag> GetTags(Guid owner, TagType tagType)
        {
            return TryGetTagDao().GetTags(owner, tagType);
        }

        public IEnumerable<Tag> GetTags(string name, TagType tagType)
        {
            return TryGetTagDao().GetTags(name, tagType);
        }

        public IEnumerable<Tag> GetTags(string[] names, TagType tagType)
        {
            return TryGetTagDao().GetTags(names, tagType);
        }


        public IEnumerable<Tag> SaveTags(params Tag[] tag)
        {
            return TryGetTagDao().SaveTags(tag);
        }

        public void UpdateNewTags(params Tag[] tag)
        {
            TryGetTagDao().UpdateNewTags(tag);
        }

        public void RemoveTags(params Tag[] tag)
        {
            TryGetTagDao().RemoveTags(tag);
        }

        public void RemoveTags(params int[] tagIds)
        {
            TryGetTagDao().RemoveTags(tagIds);
        }

        public IEnumerable<Tag> GetTags(object entryID, FileEntryType entryType, TagType tagType)
        {
            return TryGetTagDao().GetTags(entryID, entryType, tagType);
        }

        #endregion

        public void Dispose()
        {
        }
    }
}