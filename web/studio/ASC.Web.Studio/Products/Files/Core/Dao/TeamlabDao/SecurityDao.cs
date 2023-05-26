/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core.Security;

namespace ASC.Files.Core.Data
{
    internal class SecurityDao : AbstractDao, ISecurityDao
    {
        public SecurityDao(int tenant, string key)
            : base(tenant, key)
        {
        }

        public void DeleteShareRecords(IEnumerable<FileShareRecord> records)
        {
            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            using (var tx = dbManager.BeginTransaction())
            {
                foreach (var record in records)
                {
                    var d1 = new SqlDelete("files_security")
                        .Where("tenant_id", record.Tenant)
                        .Where(Exp.Eq("entry_id", MappingID(record.EntryId).ToString()))
                        .Where("entry_type", (int)record.EntryType)
                        .Where("subject", record.Subject.ToString());

                    dbManager.ExecuteNonQuery(d1);
                }

                tx.Commit();
            }
        }

        public bool IsShared(object entryId, FileEntryType type)
        {
            var q = Query("files_security s")
                .SelectCount()
                .Where(Exp.Eq("s.entry_id", MappingID(entryId).ToString()))
                .Where("s.entry_type", (int)type)
                .Where(!Exp.Eq("s.subject_type", SubjectType.Restriction));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                return dbManager.ExecuteScalar<int>(q) > 0;
            }
        }

        public void SetShare(FileShareRecord r)
        {
            if (r.Share == FileShare.None)
            {
                var entryId = (MappingID(r.EntryId) ?? "").ToString();
                if (string.IsNullOrEmpty(entryId)) return;

                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                using (var tx = dbManager.BeginTransaction())
                {
                    var files = new List<string>();

                    if (r.EntryType == FileEntryType.Folder)
                    {
                        var folders = new List<string>();
                        int intEntryId;
                        if (int.TryParse(entryId, out intEntryId))
                        {
                            var foldersInt = dbManager.ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", entryId))
                                                      .ConvertAll(o => o[0]);
                            folders.AddRange(foldersInt.Select(folderInt => folderInt.ToString()));
                            files.AddRange(dbManager.ExecuteList(Query("files_file").Select("id").Where(Exp.In("folder_id", foldersInt))).
                                                     ConvertAll(o => o[0].ToString()));
                        }
                        else
                        {
                            folders.Add(entryId);
                        }

                        var d1 = new SqlDelete("files_security")
                            .Where("tenant_id", r.Tenant)
                            .Where(Exp.In("entry_id", folders))
                            .Where("entry_type", (int)FileEntryType.Folder)
                            .Where("subject", r.Subject.ToString());

                        dbManager.ExecuteNonQuery(d1);
                    }
                    else
                    {
                        files.Add(entryId);
                    }

                    if (0 < files.Count)
                    {
                        var d2 = new SqlDelete("files_security")
                            .Where("tenant_id", r.Tenant)
                            .Where(Exp.In("entry_id", files))
                            .Where("entry_type", (int)FileEntryType.File)
                            .Where("subject", r.Subject.ToString());

                        dbManager.ExecuteNonQuery(d2);
                    }

                    tx.Commit();
                }
            }
            else
            {
                var i = new SqlInsert("files_security", true)
                    .InColumnValue("tenant_id", r.Tenant)
                    .InColumnValue("entry_id", MappingID(r.EntryId, true).ToString())
                    .InColumnValue("entry_type", (int)r.EntryType)
                    .InColumnValue("subject", r.Subject.ToString())
                    .InColumnValue("subject_type", (int)r.SubjectType)
                    .InColumnValue("owner", r.Owner.ToString())
                    .InColumnValue("security", (int)r.Share)
                    .InColumnValue("timestamp", DateTime.UtcNow)
                    .InColumnValue("options", FileShareOptions.Serialize(r.Options));

                using (var dbManager = new DbManager(FileConstant.DatabaseId))
                {
                    dbManager.ExecuteNonQuery(i);
                }
            }
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<Guid> subjects)
        {
            var q = GetQuery(Exp.In("subject", subjects.Select(s => s.ToString()).ToList()));

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(q);
            }

            var result = fromDb.ConvertAll(ToFileShareRecord);

            ClearExpired(result);

            return result;
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(IEnumerable<FileEntry> entries)
        {
            if (entries == null) return new List<FileShareRecord>();

            var files = new List<string>();
            var folders = new List<string>();

            foreach (var entry in entries)
            {
                SelectFilesAndFoldersForShare(entry, files, folders, null);
            }

            return GetPureShareRecordsDb(files, folders);
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(FileEntry entry)
        {
            if (entry == null) return new List<FileShareRecord>();

            var files = new List<string>();
            var folders = new List<string>();

            SelectFilesAndFoldersForShare(entry, files, folders, null);

            return GetPureShareRecordsDb(files, folders);
        }

        private IEnumerable<FileShareRecord> GetPureShareRecordsDb(List<string> files, List<string> folders)
        {
            var q = GetQuery(Exp.In("s.entry_id", folders) & Exp.Eq("s.entry_type", (int)FileEntryType.Folder));

            if (files.Any())
                q.Union(GetQuery(Exp.In("s.entry_id", files) & Exp.Eq("s.entry_type", (int)FileEntryType.File)));

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(q);
            }

            var result = fromDb.ConvertAll(ToFileShareRecord);

            ClearExpired(result);

            return result;
        }

        /// <summary>
        /// Get file share records with hierarchy.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public IEnumerable<FileShareRecord> GetShares(IEnumerable<FileEntry> entries)
        {
            if (entries == null) return new List<FileShareRecord>();

            var files = new List<string>();
            var foldersInt = new List<int>();

            foreach (var entry in entries)
            {
                SelectFilesAndFoldersForShare(entry, files, null, foldersInt);
            }

            return SaveFilesAndFoldersForShare(files, foldersInt);
        }

        /// <summary>
        /// Get file share records with hierarchy.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public IEnumerable<FileShareRecord> GetShares(FileEntry entry)
        {
            if (entry == null) return new List<FileShareRecord>();

            var files = new List<string>();
            var foldersInt = new List<int>();

            SelectFilesAndFoldersForShare(entry, files, null, foldersInt);
            return SaveFilesAndFoldersForShare(files, foldersInt);
        }

        private void SelectFilesAndFoldersForShare(FileEntry entry, ICollection<string> files, ICollection<string> folders, ICollection<int> foldersInt)
        {
            object folderId;
            if (entry.FileEntryType == FileEntryType.File)
            {
                var fileId = MappingID(entry.ID);
                folderId = ((File)entry).FolderID;
                if (!files.Contains(fileId.ToString())) files.Add(fileId.ToString());
            }
            else
            {
                folderId = entry.ID;
            }

            int folderIdInt;
            if (foldersInt != null && int.TryParse(folderId.ToString(), out folderIdInt) && !foldersInt.Contains(folderIdInt)) foldersInt.Add(folderIdInt);

            if (folders != null) folders.Add(MappingID(folderId).ToString());
        }

        private IEnumerable<FileShareRecord> SaveFilesAndFoldersForShare(List<string> files, List<int> folders)
        {
            var q = Query("files_security s")
                .Select("s.tenant_id", "cast(t.folder_id as char)", "s.entry_type", "s.subject", "s.subject_type", "s.owner", "s.security", "s.options", "t.level")
                .InnerJoin("files_folder_tree t", Exp.EqColumns("s.entry_id", "cast(t.parent_id as char)"))
                .Where(Exp.In("t.folder_id", folders))
                .Where("s.entry_type", (int)FileEntryType.Folder);

            if (0 < files.Count)
            {
                q.Union(GetQuery(Exp.In("s.entry_id", files) & Exp.Eq("s.entry_type", (int)FileEntryType.File)).Select("-1"));
            }

            List<object[]> fromDb;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                fromDb = dbManager.ExecuteList(q);
            }

            var result = fromDb
                .Select(ToFileShareRecord)
                .OrderBy(r => r.Level)
                .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer())
                .ToList();

            ClearExpired(result);

            return result;
        }

        public void RemoveSubjects(IEnumerable<Guid> subjects)
        {
            var q = Delete("files_security").Where(Exp.Or(Exp.In("subject", subjects), Exp.In("owner", subjects)));

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                dbManager.ExecuteNonQuery(q);
            }
        }

        private SqlQuery GetQuery(Exp where)
        {
            return Query("files_security s")
                .Select("s.tenant_id", "s.entry_id", "s.entry_type", "s.subject", "s.subject_type", "s.owner", "s.security", "s.options")
                .Where(where);
        }

        private void ClearExpired(List<FileShareRecord> records)
        {
            var expired = new List<FileShareRecord>();

            for (var i = 0; i < records.Count; i++)
            {
                var r = records[i];
                if (r.SubjectType == SubjectType.ExternalLink && r.Options != null && r.Options.IsExpired() && r.Options.AutoDelete)
                {
                    expired.Add(r);
                    records.RemoveAt(i);
                }
            }

            if (expired.Any())
            {
                DeleteShareRecords(expired);
            }
        }

        private FileShareRecord ToFileShareRecord(object[] r)
        {
            var result = new FileShareRecord
            {
                Tenant = Convert.ToInt32(r[0]),
                EntryId = MappingID(r[1]),
                EntryType = (FileEntryType)Convert.ToInt32(r[2]),
                Subject = new Guid((string)r[3]),
                SubjectType = (SubjectType)Convert.ToInt32(r[4]),
                Owner = new Guid((string)r[5]),
                Share = (FileShare)Convert.ToInt32(r[6]),
                Options = FileShareOptions.Deserialize((string)r[7]),
                Level = 8 < r.Length ? Convert.ToInt32(r[8]) : 0,
            };


            return result;
        }
    }
}