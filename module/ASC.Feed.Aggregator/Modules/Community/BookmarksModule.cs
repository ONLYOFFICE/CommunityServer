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
using System.Web;
using ASC.Bookmarking.Pojo;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

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

            var itemUrl = "/Products/Community/Modules/Bookmarking/BookmarkInfo.aspx?url=" + HttpUtility.UrlEncode(bookmark.URL);
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
                    Description = HtmlUtility.GetFull(comment.Content),
                    Date = comment.Datetime
                };
        }
    }
}