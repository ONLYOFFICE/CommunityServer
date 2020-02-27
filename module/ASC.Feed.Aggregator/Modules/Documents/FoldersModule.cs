/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    internal class FoldersModule : FeedModule
    {
        private const string folderItem = "folder";
        private const string sharedFolderItem = "sharedFolder";


        protected override string Table
        {
            get { return "files_folder"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "create_on"; }
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
            get { return Constants.FoldersModule; }
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

            var tuple = (Tuple<Folder, SmallShareRecord>)data;
            var folder = tuple.Item1;
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

            return targetCond &&
                   new FileSecurity(new DaoFactory()).CanRead(folder, userId);
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery("files_folder")
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
            var q1 = new SqlQuery("files_folder f")
                .Select(FolderColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("parent_id"))
                .Select("null, null, null")
                .Where(
                    Exp.Eq("f.tenant_id", filter.Tenant) &
                    Exp.Eq("f.folder_type", 0) &
                    Exp.Between("f.create_on", filter.Time.From, filter.Time.To)
                );

            var q2 = new SqlQuery("files_folder f")
                .LeftOuterJoin("files_security s",
                               Exp.EqColumns("s.entry_id", "f.id") &
                               Exp.Eq("s.tenant_id", filter.Tenant) &
                               Exp.Eq("s.entry_type", (int)FileEntryType.Folder)
                )
                .Select(FolderColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("parent_id"))
                .Select("s.timestamp, s.owner, s.subject")
                .Where(
                    Exp.Eq("f.tenant_id", filter.Tenant) &
                    Exp.Eq("f.folder_type", 0) &
                    Exp.Lt("s.security", 3) &
                    Exp.Between("s.timestamp", filter.Time.From, filter.Time.To)
                );

            List<Tuple<Folder, SmallShareRecord>> folders;
            using (var db = new DbManager(DbId))
            {
                folders = db.ExecuteList(q1.UnionAll(q2))
                    .ConvertAll(ToFolder)
                    .Where(f => f.Item1.RootFolderType != FolderType.TRASH && f.Item1.RootFolderType != FolderType.BUNCH)
                    .ToList();
            }

            var parentFolderIDs = folders.Select(r => r.Item1.ParentFolderID).ToArray();
            var parentFolders = new FolderDao(Tenant, DbId).GetFolders(parentFolderIDs, checkShare: false);

            return folders.Select(f => new Tuple<Feed, object>(ToFeed(f, parentFolders.FirstOrDefault(r => r.ID.Equals(f.Item1.ParentFolderID))), f));
        }


        private static IEnumerable<string> FolderColumns()
        {
            return new[]
                {
                    "id",
                    "parent_id",
                    "title",
                    "create_by",
                    "create_on",
                    "modified_by",
                    "modified_on",
                    "foldersCount",
                    "filesCount" // 8
                };
        }

        private static Tuple<Folder, SmallShareRecord> ToFolder(object[] r)
        {
            var folder = new Folder
                {
                    ID = Convert.ToInt32(r[0]),
                    ParentFolderID = Convert.ToInt32(r[1]),
                    Title = Convert.ToString(r[2]),
                    CreateBy = new Guid(Convert.ToString(r[3])),
                    CreateOn = Convert.ToDateTime(r[4]),
                    ModifiedBy = new Guid(Convert.ToString(r[5])),
                    ModifiedOn = Convert.ToDateTime(r[6]),
                    TotalSubFolders = Convert.ToInt32(r[7]),
                    TotalFiles = Convert.ToInt32(r[8]),
                    RootFolderType = DocumentsDbHelper.ParseRootFolderType(r[9]),
                    RootFolderCreator = DocumentsDbHelper.ParseRootFolderCreator(r[9]),
                    RootFolderId = DocumentsDbHelper.ParseRootFolderId(r[9])
                };

            SmallShareRecord shareRecord = null;
            if (r[10] != null)
            {
                shareRecord = new SmallShareRecord
                    {
                        ShareOn = Convert.ToDateTime(r[10]),
                        ShareBy = new Guid(Convert.ToString(r[11])),
                        ShareTo = new Guid(Convert.ToString(r[12]))
                    };
            }

            return new Tuple<Folder, SmallShareRecord>(folder, shareRecord);
        }

        private Feed ToFeed(Tuple<Folder, SmallShareRecord> tuple, Folder rootFolder)
        {
            var folder = tuple.Item1;
            var shareRecord = tuple.Item2;

            if (shareRecord != null)
            {
                var feed = new Feed(shareRecord.ShareBy, shareRecord.ShareOn, true)
                    {
                        Item = sharedFolderItem,
                        ItemId = string.Format("{0}_{1}", folder.ID, shareRecord.ShareTo),
                        ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ID, false),
                        Product = Product,
                        Module = Name,
                        Title = folder.Title,
                        ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                        ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ParentFolderID, false) : string.Empty,
                        Keywords = string.Format("{0}", folder.Title),
                        HasPreview = false,
                        CanComment = false,
                        Target = shareRecord.ShareTo,
                        GroupId = GetGroupId(sharedFolderItem, shareRecord.ShareBy, folder.ParentFolderID.ToString())
                    };

                return feed;
            }

            return new Feed(folder.CreateBy, folder.CreateOn)
                {
                    Item = folderItem,
                    ItemId = folder.ID.ToString(),
                    ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ID, false),
                    Product = Product,
                    Module = Name,
                    Title = folder.Title,
                    ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                    ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ParentFolderID, false) : string.Empty,
                    Keywords = string.Format("{0}", folder.Title),
                    HasPreview = false,
                    CanComment = false,
                    Target = null,
                    GroupId = GetGroupId(folderItem, folder.CreateBy, folder.ParentFolderID.ToString())
                };
        }
    }
}