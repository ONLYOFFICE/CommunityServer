/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
            var feedAuthor = comments.Any() ? comments.Last().UserID : bookmark.UserCreatorID;

            var feed = new Feed(bookmark.UserCreatorID, bookmark.Date)
                {
                    Item = item,
                    ItemId = bookmark.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    ModifiedBy = feedAuthor,
                    ModifiedDate = feedDate,
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