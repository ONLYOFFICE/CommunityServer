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
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal class DropboxTagDao : DropboxDaoBase, ITagDao
    {
        public DropboxTagDao(DropboxDaoSelector.DropboxInfo dropboxInfo, DropboxDaoSelector dropboxDaoSelector)
            : base(dropboxInfo, dropboxDaoSelector)
        {
        }

        #region ITagDao Members

        public IEnumerable<Tag> GetTags(Guid subject, TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            return null;
        }

        public IEnumerable<Tag> GetTags(TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            return null;
        }

        public IEnumerable<Tag> GetTags(Guid owner, TagType tagType)
        {
            return null;
        }

        public IEnumerable<Tag> GetTags(string name, TagType tagType)
        {
            return null;
        }

        public IEnumerable<Tag> GetTags(string[] names, TagType tagType)
        {
            return null;
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch)
        {
            var folderId = DropboxDaoSelector.ConvertId(parentFolder.ID);
            var fakeFolderId = parentFolder.ID.ToString();

            using (var db = GetDb())
            {
                var entryIDs = db.ExecuteList(Query("files_thirdparty_id_mapping")
                                                  .Select("hash_id")
                                                  .Where(Exp.Like("id", fakeFolderId, SqlLike.StartWith)))
                                 .ConvertAll(x => x[0]);

                if (!entryIDs.Any()) return new List<Tag>();

                var sqlQuery = new SqlQuery("files_tag ft")
                    .Select("ft.name",
                            "ft.flag",
                            "ft.owner",
                            "ftl.entry_id",
                            "ftl.entry_type",
                            "ftl.tag_count",
                            "ft.id")
                    .Distinct()
                    .LeftOuterJoin("files_tag_link ftl",
                                   Exp.EqColumns("ft.tenant_id", "ftl.tenant_id") &
                                   Exp.EqColumns("ft.id", "ftl.tag_id"))
                    .Where(Exp.Eq("ft.tenant_id", TenantID) &
                           Exp.Eq("ftl.tenant_id", TenantID) &
                           Exp.Eq("ft.flag", (int)TagType.New) &
                           Exp.In("ftl.entry_id", entryIDs));

                if (subject != Guid.Empty)
                    sqlQuery.Where(Exp.Eq("ft.owner", subject));

                var tags = db.ExecuteList(sqlQuery).ConvertAll(r => new Tag
                    {
                        TagName = Convert.ToString(r[0]),
                        TagType = (TagType)r[1],
                        Owner = new Guid(r[2].ToString()),
                        EntryId = MappingID(r[3]),
                        EntryType = (FileEntryType)r[4],
                        Count = Convert.ToInt32(r[5]),
                        Id = Convert.ToInt32(r[6])
                    });

                if (deepSearch) return tags;

                var folderFileIds = new[] { fakeFolderId }
                    .Concat(GetChildren(folderId));

                return tags.Where(tag => folderFileIds.Contains(tag.EntryId.ToString()));
            }
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, IEnumerable<FileEntry> fileEntries)
        {
            return null;
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, FileEntry fileEntry)
        {
            return null;
        }

        public IEnumerable<Tag> SaveTags(IEnumerable<Tag> tag)
        {
            return null;
        }

        public IEnumerable<Tag> SaveTags(Tag tag)
        {
            return null;
        }

        public void UpdateNewTags(IEnumerable<Tag> tag)
        {
        }

        public void UpdateNewTags(Tag tag)
        {
        }

        public void RemoveTags(IEnumerable<Tag> tag)
        {
        }

        public void RemoveTags(Tag tag)
        {
        }

        public IEnumerable<Tag> GetTags(object entryID, FileEntryType entryType, TagType tagType)
        {
            return null;
        }

        public void MarkAsNew(Guid subject, FileEntry fileEntry)
        {
        }

        #endregion
    }
}