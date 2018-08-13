/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
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
            if (!WebItemSecurity.IsAvailableForUser(ProductID.ToString(), userId)) return false;

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

            using (var db = new DbManager(DbId))
            {
                var files = db.ExecuteList(q1.UnionAll(q2)).ConvertAll(ToFile);
                return files
                    .Where(f => f.Item1.RootFolderType != FolderType.TRASH && f.Item1.RootFolderType != FolderType.BUNCH)
                    .Select(f => new Tuple<Feed, object>(ToFeed(f), f));
            }
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

        private Feed ToFeed(Tuple<File, SmallShareRecord> tuple)
        {
            var file = tuple.Item1;
            var shareRecord = tuple.Item2;

            var rootFolder = new FolderDao(Tenant, DbId).GetFolder(file.FolderID);

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
    }
}