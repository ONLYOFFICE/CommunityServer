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
using System.Globalization;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Files.Core.Data
{
    internal class TagDao : AbstractDao, ITagDao
    {
        private static readonly object syncRoot = new object();

        public TagDao(int tenant, string dbid)
            : base(tenant, dbid)
        {
        }

        public IEnumerable<Tag> GetTags(Guid subject, TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            var filesId = fileEntries.Where(e => e.FileEntryType == FileEntryType.File).Select(e => MappingID(e.ID)).ToList();
            var foldersId = fileEntries.Where(e => e.FileEntryType == FileEntryType.Folder).Select(e => MappingID(e.ID)).ToList();

            var q = Query("files_tag t")
                .InnerJoin("files_tag_link l",
                           Exp.EqColumns("l.tenant_id", "t.tenant_id") &
                           Exp.EqColumns("l.tag_id", "t.id"))
                .Select("t.name", "t.flag", "t.owner", "entry_id", "entry_type", "tag_count", "t.id")
                .Where("l.tenant_id", TenantID)
                .Where((Exp.Eq("l.entry_type", (int)FileEntryType.File) & Exp.In("l.entry_id", filesId))
                       | (Exp.Eq("l.entry_type", (int)FileEntryType.Folder) & Exp.In("l.entry_id", foldersId)))
                .Where("t.flag", (int)tagType);

            if (subject != Guid.Empty)
                q.Where(Exp.Eq("l.create_by", subject));

            return SelectTagByQuery(q);
        }

        public IEnumerable<Tag> GetTags(TagType tagType, IEnumerable<FileEntry> fileEntries)
        {
            return GetTags(Guid.Empty, tagType, fileEntries);
        }

        public IEnumerable<Tag> GetTags(object entryID, FileEntryType entryType, TagType tagType)
        {
            var q = Query("files_tag t")
                .InnerJoin("files_tag_link l",
                           Exp.EqColumns("l.tenant_id", "t.tenant_id") &
                           Exp.EqColumns("l.tag_id", "t.id"))
                .Select("t.name", "t.flag", "t.owner", "entry_id", "entry_type", "tag_count", "t.id")
                .Where("l.tenant_id", TenantID)
                .Where("l.entry_type", (int)entryType)
                .Where(Exp.Eq("l.entry_id", MappingID(entryID).ToString()))
                .Where("t.flag", (int)tagType);

            return SelectTagByQuery(q);
        }

        public IEnumerable<Tag> GetTags(string[] names, TagType tagType)
        {
            if (names == null) throw new ArgumentNullException("names");

            var q = Query("files_tag t")
                .InnerJoin("files_tag_link l",
                           Exp.EqColumns("l.tenant_id", "t.tenant_id") &
                           Exp.EqColumns("l.tag_id", "t.id"))
                .Select("t.name", "t.flag", "t.owner", "entry_id", "entry_type", "tag_count", "t.id")
                .Where("l.tenant_id", TenantID)
                .Where("t.owner", Guid.Empty.ToString())
                .Where(Exp.In("t.name", names))
                .Where("t.flag", (int)tagType);

            return SelectTagByQuery(q);
        }

        private IEnumerable<Tag> SelectTagByQuery(SqlQuery q)
        {
            return dbManager.ExecuteList(q).ConvertAll(ToTag);
        }

        public IEnumerable<Tag> GetTags(string name, TagType tagType)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            return GetTags(new[] { name }, tagType);
        }

        public IEnumerable<Tag> GetTags(Guid owner, TagType tagType)
        {
            var q = Query("files_tag t")
                .InnerJoin("files_tag_link l",
                           Exp.EqColumns("l.tenant_id", "t.tenant_id") &
                           Exp.EqColumns("l.tag_id", "t.id"))
                .Select("t.name", "t.flag", "t.owner", "entry_id", "entry_type", "tag_count", "t.id")
                .Where("l.tenant_id", TenantID)
                .Where("t.flag", (int)tagType)
                .OrderBy("l.create_on", false);

            if (owner != Guid.Empty)
                q.Where("t.owner", owner.ToString());

            return SelectTagByQuery(q);
        }

        public IEnumerable<Tag> SaveTags(IEnumerable<Tag> tags)
        {
            var result = new List<Tag>();

            if (tags == null) return result;

            tags = tags.Where(x => x != null && !x.EntryId.Equals(null) && !x.EntryId.Equals(0)).ToArray();

            if (!tags.Any()) return result;

            lock (syncRoot)
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    DeleteTagsBeforeSave();

                    var createOn = TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow());
                    var cacheTagId = new Dictionary<string, int>();

                    result.AddRange(tags.Select(t => SaveTag(t, cacheTagId, createOn)));

                    tx.Commit();
                }
            }

            return result;
        }

        public IEnumerable<Tag> SaveTags(Tag tag)
        {
            var result = new List<Tag>();

            if (tag == null) return result;

            if (tag.EntryId.Equals(null) || tag.EntryId.Equals(0)) return result;

            lock (syncRoot)
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    DeleteTagsBeforeSave();

                    var createOn = TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow());
                    var cacheTagId = new Dictionary<string, int>();

                    result.Add(SaveTag(tag, cacheTagId, createOn));

                    tx.Commit();
                }
            }

            return result;
        }

        private void DeleteTagsBeforeSave()
        {
            var mustBeDeleted = dbManager.ExecuteList(Query("files_tag ft")
                                              .Select("ftl.tag_id",
                                                      "ftl.entry_id",
                                                      "ftl.entry_type")
                                              .Distinct()
                                              .InnerJoin("files_tag_link ftl",
                                                         Exp.EqColumns("ft.tenant_id", "ftl.tenant_id") &
                                                         Exp.EqColumns("ft.id", "ftl.tag_id"))
                                              .Where((Exp.Eq("ft.flag", (int)TagType.New) | Exp.Eq("ft.flag", (int)TagType.Recent))
                                                     & Exp.Le("ftl.create_on", TenantUtil.DateTimeNow().AddMonths(-1))));

            foreach (var row in mustBeDeleted)
            {
                dbManager.ExecuteNonQuery(Delete("files_tag_link")
                                              .Where(Exp.Eq("tag_id", row[0]) &
                                                     Exp.Eq("entry_id", row[1]) &
                                                     Exp.Eq("entry_type", row[2])));
            }

            var tagsToRemove = dbManager.ExecuteList(
                Query("files_tag")
                    .Select("id")
                    .Where(Exp.EqColumns("0",
                        Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))))
                .ConvertAll(r => Convert.ToInt32(r[0]));

            dbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.In("id", tagsToRemove)));
        }

        private Tag SaveTag(Tag t, Dictionary<String, int> cacheTagId, DateTime createOn)
        {
            int id;

            var cacheTagIdKey = String.Join("/", new[] { TenantID.ToString(), t.Owner.ToString(), t.TagName, ((int)t.TagType).ToString(CultureInfo.InvariantCulture) });

            if (!cacheTagId.TryGetValue(cacheTagIdKey, out id))
            {
                id = dbManager.ExecuteScalar<int>(Query("files_tag")
                                                      .Select("id")
                                                      .Where("owner", t.Owner.ToString())
                                                      .Where("name", t.TagName)
                                                      .Where("flag", (int)t.TagType));

                if (id == 0)
                {
                    var i1 = Insert("files_tag")
                        .InColumnValue("id", 0)
                        .InColumnValue("name", t.TagName)
                        .InColumnValue("owner", t.Owner.ToString())
                        .InColumnValue("flag", (int)t.TagType)
                        .Identity(1, 0, true);
                    id = dbManager.ExecuteScalar<int>(i1);
                }

                cacheTagId.Add(cacheTagIdKey, id);
            }

            t.Id = id;

            var i2 = Insert("files_tag_link")
                .InColumnValue("tag_id", id)
                .InColumnValue("entry_id", MappingID(t.EntryId, true))
                .InColumnValue("entry_type", (int)t.EntryType)
                .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                .InColumnValue("create_on", createOn)
                .InColumnValue("tag_count", t.Count);

            dbManager.ExecuteNonQuery(i2);

            return t;
        }

        public void UpdateNewTags(IEnumerable<Tag> tags)
        {
            if (tags == null || !tags.Any()) return;

            lock (syncRoot)
            {
                using (var tx = dbManager.BeginTransaction(true))
                {
                    var createOn = TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow());

                    foreach (var tag in tags)
                    {
                        UpdateNewTagsInDb(tag, createOn);
                    }
                    tx.Commit();
                }
            }
        }

        public void UpdateNewTags(Tag tag)
        {
            if (tag == null) return;

            lock (syncRoot)
            {
                var createOn = TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow());

                UpdateNewTagsInDb(tag, createOn);
            }
        }

        private void UpdateNewTagsInDb(Tag tag, DateTime createOn)
        {
            if (tag == null) return;

            dbManager.ExecuteNonQuery(
                Update("files_tag_link")
                    .Set("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                    .Set("create_on", createOn)
                    .Set("tag_count", tag.Count)
                    .Where("tag_id", tag.Id)
                    .Where("entry_type", (int)tag.EntryType)
                    .Where(Exp.Eq("entry_id", MappingID(tag.EntryId).ToString()))
                );
        }

        public void RemoveTags(IEnumerable<Tag> tags)
        {
            if (tags == null || !tags.Any()) return;

            lock (syncRoot)
            {
                using (var tx = dbManager.BeginTransaction(true))
                {
                    foreach (var t in tags)
                    {
                        RemoveTagInDb(t);
                    }
                    tx.Commit();
                }
            }
        }

        public void RemoveTags(Tag tag)
        {
            if (tag == null) return;

            lock (syncRoot)
            {
                using (var tx = dbManager.BeginTransaction(true))
                {
                    RemoveTagInDb(tag);

                    tx.Commit();
                }
            }
        }

        private void RemoveTagInDb(Tag tag)
        {
            if (tag == null) return;

            var id = dbManager.ExecuteScalar<int>(Query("files_tag")
                .Select("id")
                .Where("name", tag.TagName)
                .Where("owner", tag.Owner.ToString())
                .Where("flag", (int) tag.TagType));

            if (id != 0)
            {
                var d = Delete("files_tag_link")
                    .Where("tag_id", id)
                    .Where(Exp.Eq("entry_id", MappingID(tag.EntryId).ToString()))
                    .Where("entry_type", (int) tag.EntryType);
                dbManager.ExecuteNonQuery(d);

                var count = dbManager.ExecuteScalar<int>(Query("files_tag_link").SelectCount().Where("tag_id", id));
                if (count == 0)
                {
                    d = Delete("files_tag").Where("id", id);
                    dbManager.ExecuteNonQuery(d);
                }
            }
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, IEnumerable<FileEntry> fileEntries)
        {
            return GetNewTags(subject, () =>
            {
                while (fileEntries.Any())
                {
                    var insertQuery = new SqlInsert("files_tag_temporary", true)
                        .InColumns(new[] { GetTenantColumnName("files_tag_temporary"), "entry_id", "entry_type" });

                    foreach (var fileEntrie in fileEntries.Take(100).Where(fileEntrie => fileEntrie != null))
                    {
                        insertQuery.Values(new[]
                        {
                            TenantID,
                            MappingID(fileEntrie.ID),
                            (fileEntrie.FileEntryType == FileEntryType.File) ? (int)FileEntryType.File : (int)FileEntryType.Folder
                        });
                    }

                    dbManager.ExecuteNonQuery(insertQuery);

                    fileEntries = fileEntries.Skip(100).ToArray();
                }
            });
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, FileEntry fileEntry)
        {
            return GetNewTags(subject, () =>
            {
                var insertQuery = new SqlInsert("files_tag_temporary", true)
                    .InColumns(new[] {GetTenantColumnName("files_tag_temporary"), "entry_id", "entry_type"});

                insertQuery.Values(new[]
                {
                    TenantID,
                    MappingID(fileEntry.ID),
                    (int)fileEntry.FileEntryType
                });

                dbManager.ExecuteNonQuery(insertQuery);
            });
        }

        private IEnumerable<Tag> GetNewTags(Guid subject, Action insert)
        {
            List<Tag> result;

            using (var tx = dbManager.BeginTransaction())
            {
                var sqlQuery = Query("files_tag ft")
                    .Select("ft.name",
                            "ft.flag",
                            "ft.owner",
                            "ftl.entry_id",
                            "ftl.entry_type",
                            "ftl.tag_count",
                            "ft.id")
                    .Distinct()
                    .InnerJoin("files_tag_link ftl",
                                Exp.EqColumns("ft.tenant_id", "ftl.tenant_id") &
                                Exp.EqColumns("ft.id", "ftl.tag_id"))
                    .Where(Exp.Eq("ft.flag", (int)TagType.New));

                if (subject != Guid.Empty)
                    sqlQuery.Where(Exp.Eq("ft.owner", subject));

                const string sqlQueryStr =
                    @"
                                CREATE  TEMPORARY TABLE IF NOT EXISTS `files_tag_temporary` (
                                `tenant_id` INT(10) NOT NULL,
	                            `entry_id` VARCHAR(255) NOT NULL,
	                            `entry_type` INT(10) NOT NULL,
	                            PRIMARY KEY (`tenant_id`, `entry_id`, `entry_type`)
                            );";

                dbManager.ExecuteNonQuery(sqlQueryStr);

                insert();

                var resultSql = sqlQuery
                    .InnerJoin("files_tag_temporary ftt", Exp.EqColumns("ftl.tenant_id", "ftt.tenant_id") &
                                                            Exp.EqColumns("ftl.entry_id", "ftt.entry_id") &
                                                            Exp.EqColumns("ftl.entry_type", "ftt.entry_type"));

                result = dbManager.ExecuteList(resultSql).ConvertAll(ToTag).Where(x => x.EntryId != null).ToList();

                dbManager.ExecuteNonQuery(Delete("files_tag_temporary"));

                tx.Commit();
            }

            return result;
        }

        public IEnumerable<Tag> GetNewTags(Guid subject, Folder parentFolder, bool deepSearch)
        {
            if (parentFolder == null || parentFolder.ID == null)
                throw new ArgumentException("folderId");

            var result = new List<Tag>();

            var monitorFolderIds = new[] { parentFolder.ID }.AsEnumerable();

            var getBaseSqlQuery = new Func<SqlQuery>(() =>
                                                            {
                                                                var fnResult =
                                                                    Query("files_tag ft")
                                                                        .Select("ft.name",
                                                                                "ft.flag",
                                                                                "ft.owner",
                                                                                "ftl.entry_id",
                                                                                "ftl.entry_type",
                                                                                "ftl.tag_count",
                                                                                "ft.id")
                                                                        .Distinct()
                                                                        .InnerJoin("files_tag_link ftl",
                                                                                Exp.EqColumns("ft.tenant_id", "ftl.tenant_id") &
                                                                                Exp.EqColumns("ft.id", "ftl.tag_id"))
                                                                        .Where(Exp.Eq("ft.flag", (int)TagType.New));

                                                                if (subject != Guid.Empty)
                                                                    fnResult.Where(Exp.Eq("ft.owner", subject));

                                                                return fnResult;
                                                            });

            var tempTags = Enumerable.Empty<Tag>();

            if (parentFolder.FolderType == FolderType.SHARE)
            {
                var shareQuery =
                    new Func<SqlQuery>(() => getBaseSqlQuery().Where(Exp.Exists(
                        new SqlQuery("files_security fs")
                        .Select("fs.entry_id")
                        .Where(
                            Exp.EqColumns("fs.tenant_id", "ftl.tenant_id") &
                            Exp.EqColumns("fs.entry_id", "ftl.entry_id") &
                            Exp.EqColumns("fs.entry_type", "ftl.entry_type")))));

                var tmpShareFileTags = dbManager.ExecuteList(
                    shareQuery().InnerJoin("files_file f",
                                            Exp.EqColumns("f.tenant_id", "ftl.tenant_id") &
                                            !Exp.Eq("f.create_by", subject) &
                                            Exp.EqColumns("f.id", "ftl.entry_id") &
                                            Exp.Eq("ftl.entry_type", (int)FileEntryType.File))
                                .Select(GetRootFolderType("folder_id")))
                                                .Where(r => ParseRootFolderType(r[7]) == FolderType.USER).ToList()
                                                .ConvertAll(ToTag);
                tempTags = tempTags.Concat(tmpShareFileTags);


                var tmpShareFolderTags = dbManager.ExecuteList(
                    shareQuery().InnerJoin("files_folder f",
                                            Exp.EqColumns("f.tenant_id", "ftl.tenant_id") &
                                            !Exp.Eq("f.create_by", subject) &
                                            Exp.EqColumns("f.id", "ftl.entry_id") &
                                            Exp.Eq("ftl.entry_type", (int)FileEntryType.Folder))
                                .Select(GetRootFolderType("parent_id")))
                                                    .Where(r => ParseRootFolderType(r[7]) == FolderType.USER).ToList()
                                                    .ConvertAll(ToTag);
                tempTags = tempTags.Concat(tmpShareFolderTags);

                var tmpShareSboxTags = dbManager.ExecuteList(
                    shareQuery()
                        .InnerJoin("files_thirdparty_id_mapping m",
                                    Exp.EqColumns("m.tenant_id", "ftl.tenant_id") &
                                    Exp.EqColumns("m.hash_id", "ftl.entry_id"))
                        .InnerJoin("files_thirdparty_account ac",
                                    Exp.EqColumns("ac.tenant_id", "m.tenant_id") &
                                    Exp.Sql("m.id Like concat('sbox-', ac.id, '%') or m.id Like concat('box-', ac.id, '%') or m.id Like concat('dropbox-', ac.id, '%') or m.id Like concat('spoint-', ac.id, '%') or m.id Like concat('drive-', ac.id, '%') or m.id Like concat('onedrive-', ac.id, '%')") &
                                    !Exp.Eq("ac.user_id", subject) &
                                    Exp.Eq("ac.folder_type", FolderType.USER)
                        )
                    ).ConvertAll(ToTag);

                tempTags = tempTags.Concat(tmpShareSboxTags);
            }
            else if (parentFolder.FolderType == FolderType.Privacy)
            {
                var shareQuery =
                    new Func<SqlQuery>(() => getBaseSqlQuery().Where(Exp.Exists(
                        new SqlQuery("files_security fs")
                        .Select("fs.entry_id")
                        .Where(
                            Exp.EqColumns("fs.tenant_id", "ftl.tenant_id") &
                            Exp.EqColumns("fs.entry_id", "ftl.entry_id") &
                            Exp.EqColumns("fs.entry_type", "ftl.entry_type")))));

                var tmpShareFileTags = dbManager.ExecuteList(
                    shareQuery().InnerJoin("files_file f",
                                            Exp.EqColumns("f.tenant_id", "ftl.tenant_id") &
                                            !Exp.Eq("f.create_by", subject) &
                                            Exp.EqColumns("f.id", "ftl.entry_id") &
                                            Exp.Eq("ftl.entry_type", (int)FileEntryType.File))
                                .Select(GetRootFolderType("folder_id")))
                                                .Where(r => ParseRootFolderType(r[7]) == FolderType.Privacy).ToList()
                                                .ConvertAll(ToTag);
                tempTags = tempTags.Concat(tmpShareFileTags);


                var tmpShareFolderTags = dbManager.ExecuteList(
                    shareQuery().InnerJoin("files_folder f",
                                            Exp.EqColumns("f.tenant_id", "ftl.tenant_id") &
                                            !Exp.Eq("f.create_by", subject) &
                                            Exp.EqColumns("f.id", "ftl.entry_id") &
                                            Exp.Eq("ftl.entry_type", (int)FileEntryType.Folder))
                                .Select(GetRootFolderType("parent_id")))
                                                    .Where(r => ParseRootFolderType(r[7]) == FolderType.Privacy).ToList()
                                                    .ConvertAll(ToTag);
                tempTags = tempTags.Concat(tmpShareFolderTags);
            }
            else if (parentFolder.FolderType == FolderType.Projects)
                tempTags = tempTags.Concat(
                    dbManager.ExecuteList(getBaseSqlQuery()
                                                .InnerJoin("files_bunch_objects fbo",
                                                            Exp.EqColumns("fbo.tenant_id", "ftl.tenant_id") &
                                                            Exp.EqColumns("fbo.left_node", "ftl.entry_id") &
                                                            Exp.Eq("ftl.entry_type", (int)FileEntryType.Folder))
                                                .Where(Exp.Eq("fbo.tenant_id", TenantID) & Exp.Like("fbo.right_node", "projects/project/", SqlLike.StartWith)))
                                .ConvertAll(ToTag));

            if (tempTags.Any())
            {
                if (!deepSearch) return tempTags;

                monitorFolderIds = monitorFolderIds.Concat(tempTags.Where(x => x.EntryType == FileEntryType.Folder).Select(x => x.EntryId));
                result.AddRange(tempTags);
            }

            var subFoldersSqlQuery = new SqlQuery("files_folder_tree")
                .Select("folder_id")
                .Where(Exp.In("parent_id", monitorFolderIds.ToArray()));

            if (!deepSearch)
                subFoldersSqlQuery.Where(Exp.Eq("level", 1));

            monitorFolderIds = monitorFolderIds.Concat(dbManager.ExecuteList(subFoldersSqlQuery).ConvertAll(x => x[0]));

            var newTagsForFolders = dbManager.ExecuteList(getBaseSqlQuery()
                                                                .Where(Exp.In("ftl.entry_id", monitorFolderIds.ToArray()) &
                                                                        Exp.Eq("ftl.entry_type", (int)FileEntryType.Folder)))
                                                .ConvertAll(ToTag);

            result.AddRange(newTagsForFolders);

            var newTagsForFiles = dbManager.ExecuteList(getBaseSqlQuery()
                                                            .InnerJoin("files_file ff",
                                                                        Exp.EqColumns("ftl.tenant_id", "ff.tenant_id") &
                                                                        Exp.EqColumns("ftl.entry_id", "ff.id"))
                                                            .Where(Exp.In("ff.folder_id", (deepSearch
                                                                                                ? monitorFolderIds.ToArray()
                                                                                                : new[] { parentFolder.ID })) &
                                                                    Exp.Eq("ftl.entry_type", (int)FileEntryType.File)))
                                            .ConvertAll(ToTag);

            result.AddRange(newTagsForFiles);

            if (parentFolder.FolderType == FolderType.USER || parentFolder.FolderType == FolderType.COMMON)
            {
                var folderType = parentFolder.FolderType;

                var querySelect = new SqlQuery("files_thirdparty_account")
                    .Select("id")
                    .Where("tenant_id", TenantID)
                    .Where("folder_type", (int)folderType);

                if (folderType == FolderType.USER)
                    querySelect = querySelect.Where(Exp.Eq("user_id", subject.ToString()));

                var folderIds = dbManager.ExecuteList(querySelect);
                var thirdpartyFolderIds = folderIds.ConvertAll(r => "sbox-" + r[0])
                                                    .Concat(folderIds.ConvertAll(r => "box-" + r[0]))
                                                    .Concat(folderIds.ConvertAll(r => "dropbox-" + r[0]))
                                                    .Concat(folderIds.ConvertAll(r => "spoint-" + r[0]))
                                                    .Concat(folderIds.ConvertAll(r => "drive-" + r[0]))
                                                    .Concat(folderIds.ConvertAll(r => "onedrive-" + r[0]));

                var newTagsForSBox = dbManager.ExecuteList(getBaseSqlQuery()
                                                                .InnerJoin("files_thirdparty_id_mapping mp",
                                                                            Exp.EqColumns("mp.tenant_id", "ftl.tenant_id") &
                                                                            Exp.EqColumns("ftl.entry_id", "mp.hash_id"))
                                                                .Where(Exp.In("mp.id", thirdpartyFolderIds.ToList()) &
                                                                        Exp.Eq("ft.owner", subject) &
                                                                        Exp.Eq("ftl.entry_type", (int)FileEntryType.Folder)))
                                                .ConvertAll(ToTag);

                result.AddRange(newTagsForSBox);
            }

            return result;
        }

        private Tag ToTag(object[] r)
        {
            var result = new Tag((string)r[0], (TagType)Convert.ToInt32(r[1]), new Guid((string)r[2]), null, Convert.ToInt32(r[5]))
                {
                    EntryId = MappingID(r[3]),
                    EntryType = (FileEntryType)Convert.ToInt32(r[4]),
                    Id = Convert.ToInt32(r[6]),
                };

            return result;
        }
    }
}