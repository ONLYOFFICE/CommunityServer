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
using System.Globalization;
using System.Web;
using ASC.Bookmarking.Pojo;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Utility;
using System.Linq;

namespace ASC.Feed.Aggregator.Modules.Community
{
    internal class BookmarksModule : FeedModule
    {
        private const string item = "bookmark";


        protected override string Table
        {
            get { return "bookmarking_bookmark"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "date"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant"; }
        }

        protected override string DbId
        {
            get { return Constants.CommunityDbId; }
        }


        public override string Name
        {
            get { return Constants.BookmarksModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.CommunityProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.CommunityProductID; }
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery("bookmarking_bookmark")
                .Select("tenant")
                .Where(Exp.Gt("date", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("bookmarking_comment")
                .Select("tenant")
                .Where(Exp.Gt("datetime", fromTime))
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
            var q = new SqlQuery("bookmarking_bookmark b")
                .Select("b.id", "b.url", "b.name", "b.description", "b.usercreatorid", "b.date")
                .LeftOuterJoin("bookmarking_comment c",
                               Exp.EqColumns("b.id", "c.bookmarkId") &
                               Exp.Eq("c.inactive", 0) &
                               Exp.Eq("c.tenant", filter.Tenant)
                )
                .Select("c.id", "c.content", "c.userId", "c.datetime")
                .Where("b.tenant", filter.Tenant)
                .Where(Exp.Between("b.date", filter.Time.From, filter.Time.To) |
                       Exp.Between("c.datetime", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var comments = db.ExecuteList(q).ConvertAll(ToComment);
                var groupedBookmarks = comments.GroupBy(c => c.Bookmark.ID);

                return groupedBookmarks
                    .Select(b => new Tuple<Bookmark, IEnumerable<Comment>>(b.First().Bookmark, b))
                    .Select(ToFeed);
            }
        }


        private static Comment ToComment(object[] r)
        {
            var comment = new Comment
                {
                    Bookmark = new Bookmark
                        {
                            ID = Convert.ToInt64(r[0]),
                            URL = Convert.ToString(r[1]),
                            Name = Convert.ToString(r[2]),
                            Description = Convert.ToString(r[3]),
                            UserCreatorID = new Guid(Convert.ToString(r[4])),
                            Date = Convert.ToDateTime(Convert.ToString(r[5]))
                        }
                };

            if (r[6] != null)
            {
                comment.ID = new Guid(Convert.ToString(r[6]));
                comment.Content = Convert.ToString(r[7]);
                comment.UserID = new Guid(Convert.ToString(r[8]));
                comment.Datetime = Convert.ToDateTime(Convert.ToString(r[9]));
            }
            return comment;
        }

        private Tuple<Feed, object> ToFeed(Tuple<Bookmark, IEnumerable<Comment>> b)
        {
            var bookmark = b.Item1;

            var itemUrl = "/products/community/modules/bookmarking/bookmarkinfo.aspx?url=" + HttpUtility.UrlEncode(bookmark.URL);
            var commentApiUrl = "/api/2.0/community/bookmark/" + bookmark.ID + "/comment.json";

            var comments = b.Item2.Where(c => c.ID != Guid.Empty).OrderBy(c => c.Datetime).ToList();
            var feedDate = comments.Any() ? comments.First().Datetime : bookmark.Date;
            var feedAutohor = comments.Any() ? comments.Last().UserID : bookmark.UserCreatorID;

            var feed = new Feed(bookmark.UserCreatorID, feedDate)
                {
                    Item = item,
                    ItemId = bookmark.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    LastModifiedBy = feedAutohor,
                    Product = Product,
                    Module = Name,
                    Action = comments.Any() ? FeedAction.Commented : FeedAction.Created,
                    Title = bookmark.Name,
                    Description = bookmark.Description,
                    HasPreview = false,
                    CanComment = true,
                    CommentApiUrl = CommonLinkUtility.ToAbsolute(commentApiUrl),
                    Comments = comments.Select(ToFeedComment),
                    GroupId = string.Format("{0}_{1}", item, bookmark.ID)
                };
            feed.Keywords = string.Format("{0} {1} {2} {3}",
                                          bookmark.Name,
                                          bookmark.URL,
                                          bookmark.Description,
                                          string.Join(" ", feed.Comments.Select(x => x.Description)));

            return new Tuple<Feed, object>(feed, bookmark);
        }

        private static FeedComment ToFeedComment(Comment comment)
        {
            return new FeedComment(comment.UserID)
                {
                    Id = comment.ID.ToString(),
                    Description = HtmlSanitizer.Sanitize(comment.Content),
                    Date = comment.Datetime
                };
        }
    }
}