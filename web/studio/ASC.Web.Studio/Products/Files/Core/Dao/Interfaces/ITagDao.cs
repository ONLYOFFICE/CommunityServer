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

namespace ASC.Files.Core
{
    public interface ITagDao : IDisposable
    {
        IEnumerable<Tag> GetTags(Guid subject, TagType tagType, IEnumerable<FileEntry> fileEntries);

        IEnumerable<Tag> GetTags(TagType tagType, IEnumerable<FileEntry> fileEntries);

        IEnumerable<Tag> GetTags(Guid owner, TagType tagType);

        IEnumerable<Tag> GetTags(string name, TagType tagType);

        IEnumerable<Tag> GetTags(string[] names, TagType tagType);

        IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch);

        IEnumerable<Tag> GetNewTags(Guid subject, IEnumerable<FileEntry> fileEntries);

        IEnumerable<Tag> GetNewTags(Guid subject, FileEntry fileEntry);

        IEnumerable<Tag> SaveTags(IEnumerable<Tag> tag);

        IEnumerable<Tag> SaveTags(Tag tag);

        void UpdateNewTags(IEnumerable<Tag> tag);

        void UpdateNewTags(Tag tag);

        void RemoveTags(IEnumerable<Tag> tag);

        void RemoveTags(Tag tag);

        IEnumerable<Tag> GetTags(object entryID, FileEntryType entryType, TagType tagType);
    }
}