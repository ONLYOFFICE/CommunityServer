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

            var file = (FileEntry)data;

            bool targetCond;
            if (feed.Target != null)
            {
                if (!string.IsNullOrEmpty(file.SharedToMeBy) && file.SharedToMeBy == userId.ToString()) return false;

                var owner = new Guid((string)feed.Target);
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
                   new FileSecurity(new DaoFactory()).CanRead(file, userId);
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
                return db.ExecuteList(q1).ConvertAll(r => Convert.ToInt32(r[0]))
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
                    .Where(f => f.RootFolderType != FolderType.TRASH && f.RootFolderType != FolderType.BUNCH)
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

        private static File ToFile(object[] r)
        {
            return new File
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
                    RootFolderId = DocumentsDbHelper.ParseRootFolderId(r[13]),
                    SharedToMeOn = r[14] != null ? Convert.ToDateTime(r[14]) : (DateTime?)null,
                    SharedToMeBy = r[15] != null ? Convert.ToString(r[15]) : null,
                    // here stored subject of the file share 
                    CreateByString = r[16] != null ? Convert.ToString(r[16]) : null
                };
        }

        private Feed ToFeed(File file)
        {
            var rootFolder = new FolderDao(Tenant, DbId).GetFolder(file.FolderID);

            if (file.SharedToMeOn.HasValue)
            {
                var feed = new Feed(new Guid(file.SharedToMeBy), file.SharedToMeOn.Value, true)
                    {
                        Item = sharedFileItem,
                        ItemId = string.Format("{0}_{1}", file.ID, file.CreateByString),
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
                        Target = file.CreateByString,
                        GroupId = GetGroupId(sharedFileItem, new Guid(file.SharedToMeBy), file.FolderID.ToString())
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