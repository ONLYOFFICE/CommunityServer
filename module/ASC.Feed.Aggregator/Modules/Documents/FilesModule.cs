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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Feed.Data;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.Core.Files;

namespace ASC.Feed.Aggregator.Modules.Documents
{
    internal class FilesModule : FeedModule
    {
        private const string fileItem = "file";
        private const string sharedFileItem = "sharedFile";


        protected override string Table
        {
            get { return "files_file"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "modified_on"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant_id"; }
        }

        protected override string DbId
        {
            get { return Constants.FilesDbId; }
        }


        public override string Name
        {
            get { return Constants.FilesModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.DocumentsProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.DocumentsProductID; }
        }


        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            if (!WebItemSecurity.IsAvailableForUser(ProductID, userId)) return false;

            var tuple = (Tuple<File, SmallShareRecord>)data;
            var file = tuple.Item1;
            var shareRecord = tuple.Item2;

            bool targetCond;
            if (feed.Target != null)
            {
                if (shareRecord != null && shareRecord.ShareBy == userId) return false;

                var owner = (Guid)feed.Target;
                var groupUsers = CoreContext.UserManager.GetUsersByGroup(owner).Select(x => x.ID).ToList();
                if (!groupUsers.Any())
                {
                    groupUsers.Add(owner);
                }
                targetCond = groupUsers.Contains(userId);
            }
            else
            {
                targetCond = true;
            }

            return targetCond && new FileSecurity(new DaoFactory()).CanRead(file, userId);
        }

        public override void VisibleFor(List<Tuple<FeedRow, object>> feed, Guid userId)
        {
            if (!WebItemSecurity.IsAvailableForUser(ProductID, userId)) return;

            var fileSecurity = new FileSecurity(new DaoFactory());

            var feed1 = feed.Select(r =>
            {
                var tuple = (Tuple<File, SmallShareRecord>)r.Item2;
                return new Tuple<FeedRow, File, SmallShareRecord>(r.Item1, tuple.Item1, tuple.Item2);
            })
            .ToList();

            var files = feed1.Where(r => r.Item1.Feed.Target == null).Select(r => r.Item2).ToList();

            foreach (var f in feed1.Where(r => r.Item1.Feed.Target != null && !(r.Item3 != null && r.Item3.ShareBy == userId)))
            {
                var file = f.Item2;
                if (IsTarget(f.Item1.Feed.Target, userId) && !files.Any(r => r.UniqID.Equals(file.UniqID)))
                {
                    files.Add(file);
                }
            }

            var canRead = fileSecurity.CanRead(files, userId).Where(r => r.Item2).ToList();

            foreach (var f in feed1)
            {
                if (IsTarget(f.Item1.Feed.Target, userId) && canRead.Any(r => r.Item1.ID.Equals(f.Item2.ID)))
                {
                    f.Item1.Users.Add(userId);
                }
            }
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery("files_file")
                .Select("tenant_id")
                .Where(Exp.Gt("modified_on", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("files_security")
                .Select("tenant_id")
                .Where(Exp.Gt("timestamp", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = new DbManager(DbId))
            {
                return db.ExecuteList(q1)
                         .ConvertAll(r => Convert.ToInt32(r[0]))
                         .Union(db.ExecuteList(q2).ConvertAll(r => Convert.ToInt32(r[0])));
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var q1 = new SqlQuery("files_file f")
                .Select(FileColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("folder_id"))
                .Select("null, null, null")
                .Where(
                    Exp.Eq("f.tenant_id", filter.Tenant)
                    & Exp.Eq("f.current_version", 1)
                    & Exp.Between("f.modified_on", filter.Time.From, filter.Time.To));

            var q2 = new SqlQuery("files_file f")
                .Select(FileColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("folder_id"))
                .LeftOuterJoin("files_security s",
                               Exp.EqColumns("s.entry_id", "f.id") &
                               Exp.Eq("s.tenant_id", filter.Tenant) &
                               Exp.Eq("s.entry_type", (int)FileEntryType.File)
                )
                .Select("s.timestamp, s.owner, s.subject")
                .Where(Exp.Eq("f.tenant_id", filter.Tenant) &
                       Exp.Eq("f.current_version", 1) &
                       Exp.Lt("s.security", 3) &
                       Exp.Between("s.timestamp", filter.Time.From, filter.Time.To));

            List<Tuple<File, SmallShareRecord>> files;
            using (var db = new DbManager(DbId))
            {
                files = db.ExecuteList(q1.UnionAll(q2))
                    .ConvertAll(ToFile)
                    .Where(f => f.Item1.RootFolderType != FolderType.TRASH && f.Item1.RootFolderType != FolderType.BUNCH)
                    .ToList();
            }

            var folderIDs = files.Select(r => r.Item1.FolderID).ToArray();
            var folders = new FolderDao(Tenant, DbId).GetFolders(folderIDs, checkShare: false);

            return files.Select(f => new Tuple<Feed, object>(ToFeed(f, folders.FirstOrDefault(r=> r.ID.Equals(f.Item1.FolderID))), f));
        }


        private static IEnumerable<string> FileColumns()
        {
            return new[]
                {
                    "id",
                    "version",
                    "version_group",
                    "folder_id",
                    "title",
                    "content_length",
                    "file_status",
                    "create_by",
                    "create_on",
                    "modified_by",
                    "modified_on",
                    "converted_type",
                    "comment" // 12
                };
        }

        private static Tuple<File, SmallShareRecord> ToFile(object[] r)
        {
            var file = new File
                {
                    ID = r[0],
                    Version = Convert.ToInt32(r[1]),
                    VersionGroup = Convert.ToInt32(r[2]),
                    FolderID = Convert.ToInt32(r[3]),
                    Title = Convert.ToString(r[4]),
                    ContentLength = Convert.ToInt64(r[5]),
                    FileStatus = (FileStatus)Convert.ToInt32(r[6]),
                    CreateBy = new Guid(Convert.ToString(r[7])),
                    CreateOn = Convert.ToDateTime(r[8]),
                    ModifiedBy = new Guid(Convert.ToString(r[9])),
                    ModifiedOn = Convert.ToDateTime(r[10]),
                    ConvertedType = Convert.ToString(r[11]),
                    Comment = Convert.ToString(r[12]),
                    RootFolderType = DocumentsDbHelper.ParseRootFolderType(r[13]),
                    RootFolderCreator = DocumentsDbHelper.ParseRootFolderCreator(r[13]),
                    RootFolderId = DocumentsDbHelper.ParseRootFolderId(r[13])
                };

            SmallShareRecord shareRecord = null;
            if (r[14] != null && r[15] != null && r[16] != null)
            {
                shareRecord = new SmallShareRecord
                    {
                        ShareOn = Convert.ToDateTime(r[14]),
                        ShareBy = new Guid(Convert.ToString(r[15])),
                        ShareTo = new Guid(Convert.ToString(r[16]))
                    };
            }

            return new Tuple<File, SmallShareRecord>(file, shareRecord);
        }

        private Feed ToFeed(Tuple<File, SmallShareRecord> tuple, Folder rootFolder)
        {
            var file = tuple.Item1;
            var shareRecord = tuple.Item2;

            if (shareRecord != null)
            {
                var feed = new Feed(shareRecord.ShareBy, shareRecord.ShareOn, true)
                    {
                        Item = sharedFileItem,
                        ItemId = string.Format("{0}_{1}", file.ID, shareRecord.ShareTo),
                        ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(file.ID, true),
                        Product = Product,
                        Module = Name,
                        Title = file.Title,
                        ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                        ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(file.FolderID, false) : string.Empty,
                        AdditionalInfo = file.ContentLengthString,
                        Keywords = string.Format("{0}", file.Title),
                        HasPreview = false,
                        CanComment = false,
                        Target = shareRecord.ShareTo,
                        GroupId = GetGroupId(sharedFileItem, shareRecord.ShareBy, file.FolderID.ToString())
                    };

                return feed;
            }

            var updated = file.Version != 1;
            return new Feed(file.ModifiedBy, file.ModifiedOn, true)
                {
                    Item = fileItem,
                    ItemId = string.Format("{0}_{1}", file.ID, file.Version > 1 ? 1 : 0),
                    ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(file.ID, true),
                    Product = Product,
                    Module = Name,
                    Action = updated ? FeedAction.Updated : FeedAction.Created,
                    Title = file.Title,
                    ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                    ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(file.FolderID, false) : string.Empty,
                    AdditionalInfo = file.ContentLengthString,
                    Keywords = string.Format("{0}", file.Title),
                    HasPreview = false,
                    CanComment = false,
                    Target = null,
                    GroupId = GetGroupId(fileItem, file.ModifiedBy, file.FolderID.ToString(), updated ? 1 : 0)
                };
        }

        private bool IsTarget(object target, Guid userId)
        {
            if (target == null) return true;
            var owner = (Guid)target;
            var groupUsers = CoreContext.UserManager.GetUsersByGroup(owner).Select(x => x.ID).ToList();
            if (!groupUsers.Any())
            {
                groupUsers.Add(owner);
            }

            return groupUsers.Contains(userId);
        }
    }
}