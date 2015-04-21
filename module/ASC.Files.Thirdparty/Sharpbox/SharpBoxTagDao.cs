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
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal class SharpBoxTagDao : SharpBoxDaoBase, ITagDao
    {
        public SharpBoxTagDao(SharpBoxDaoSelector.SharpBoxInfo providerInfo, SharpBoxDaoSelector sharpBoxDaoSelector)
            : base(providerInfo, sharpBoxDaoSelector)
        {
        }

        #region ITagDao Members

        public IEnumerable<Tag> GetTags(TagType tagType, params FileEntry[] fileEntries)
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
            var folderId = SharpBoxDaoSelector.ConvertId(parentFolder.ID);

            var fakeFolderId = parentFolder.ID.ToString();

            var entryIDs = DbManager.ExecuteList(Query("files_thirdparty_id_mapping")
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
                       Exp.Eq("ft.flag", (int) TagType.New) &
                       Exp.In("ftl.entry_id", entryIDs));

            if (subject != Guid.Empty)
                sqlQuery.Where(Exp.Eq("ft.owner", subject));

            var tags = DbManager.ExecuteList(sqlQuery).ConvertAll(r => new Tag
                {
                    TagName = Convert.ToString(r[0]),
                    TagType = (TagType) r[1],
                    Owner = new Guid(r[2].ToString()),
                    EntryId = MappingID(r[3]),
                    EntryType = (FileEntryType) r[4],
                    Count = Convert.ToInt32(r[5]),
                    Id = Convert.ToInt32(r[6])
                });

            if (deepSearch) return tags;

            var folderFileIds = new[] {fakeFolderId}
                .Concat(GetFolderSubfolders(folderId).Select(x => MakeId(x)))
                .Concat(GetFolderFiles(folderId).Select(x => MakeId(x)));

            return tags.Where(tag => folderFileIds.Contains(tag.EntryId.ToString()));
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, params FileEntry[] fileEntries)
        {
            return null;
        }

        public IEnumerable<Tag> SaveTags(params Tag[] tag)
        {
            return null;
        }

        public void UpdateNewTags(params Tag[] tag)
        {
        }

        public void RemoveTags(params Tag[] tag)
        {
        }

        public void RemoveTags(params int[] tagIds)
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