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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.SharePoint
{
    internal class SharePointTagDao: SharePointDaoBase, ITagDao
    {
        public SharePointTagDao(SharePointProviderInfo sharePointInfo, SharePointDaoSelector sharePointDaoSelector)
            : base(sharePointInfo, sharePointDaoSelector)
        {
        }

        public void Dispose()
        {
            ProviderInfo.Dispose();
        }

        public IEnumerable<Tag> GetTags(TagType tagType, params FileEntry[] fileEntries)
        {
            return new List<Tag>();
        }

        public IEnumerable<Tag> GetTags(Guid owner, TagType tagType)
        {
            return new List<Tag>();
        }

        public IEnumerable<Tag> GetTags(string name, TagType tagType)
        {
            return new List<Tag>();
        }

        public IEnumerable<Tag> GetTags(string[] names, TagType tagType)
        {
            return new List<Tag>();
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch)
        {
            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                var folderId = SharePointDaoSelector.ConvertId(parentFolder.ID);

                var fakeFolderId = parentFolder.ID.ToString();

                var entryIDs = dbManager.ExecuteList(Query("files_thirdparty_id_mapping")
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

                var tags = dbManager.ExecuteList(sqlQuery).ConvertAll(r => new Tag
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
                    .Concat(ProviderInfo.GetFolderFolders(folderId).Select(x => ProviderInfo.MakeId(x.ServerRelativeUrl)))
                    .Concat(ProviderInfo.GetFolderFiles(folderId).Select(x => ProviderInfo.MakeId(x.ServerRelativeUrl)));

                return tags.Where(tag => folderFileIds.Contains(tag.EntryId.ToString()));
            }
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, params FileEntry[] fileEntries)
        {
            return new List<Tag>();
        }

        public IEnumerable<Tag> SaveTags(params Tag[] tag)
        {
            return new List<Tag>();
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
            return new List<Tag>();
        }
    }
}
