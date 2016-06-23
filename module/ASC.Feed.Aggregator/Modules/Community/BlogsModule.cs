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
using ASC.Blogs.Core.Domain;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Utility;
using System.Linq;

namespace ASC.Feed.Aggregator.Modules.Community
{
    internal class BlogsModule : FeedModule
    {
        private const string item = "blog";


        protected override string Table
        {
            get { return "blogs_posts"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "created_when"; }
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
            get { return Constants.BlogsModule; }
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
            var q1 = new SqlQuery("blogs_posts")
                .Select("tenant")
                .Where(Exp.Gt("created_when", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("blogs_comments")
                .Select("tenant")
                .Where(Exp.Gt("created_when", fromTime))
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
            var query = new SqlQuery("blogs_posts p")
                .Select(BlogColumns().Select(p => "p." + p).ToArray())
                .LeftOuterJoin("blogs_comments c",
                               Exp.EqColumns("c.post_id", "p.id") &
                               Exp.Eq("c.inactive", 0) &
                               Exp.Eq("c.tenant", filter.Tenant)
                )
                .Select(BlogCommentColumns().Select(c => "c." + c).ToArray())
                .Where("p.tenant", filter.Tenant)
                .Where(Exp.Between("p.created_when", filter.Time.From, filter.Time.To) |
                       Exp.Between("c.created_when", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var comments = db.ExecuteList(query).ConvertAll(ToComment);
                var groupedBlogs = comments.GroupBy(c => c.Post.ID);

                return groupedBlogs
                    .Select(b => new Tuple<Post, IEnumerable<Comment>>(b.First().Post, b))
                    .Select(ToFeed);
            }
        }


        private static IEnumerable<string> BlogColumns()
        {
            return new[]
                {
                    "id",
                    "title",
                    "content",
                    "created_by",
                    "created_when" // 4
                };
        }

        private static IEnumerable<string> BlogCommentColumns()
        {
            return new[]
                {
                    "id", // 5
                    "content",
                    "created_by",
                    "created_when" // 8
                };
        }

        private static Comment ToComment(object[] r)
        {
            var comment = new Comment
                {
                    Post = new Post
                        {
                            ID = new Guid(Convert.ToString(r[0])),
                            Title = Convert.ToString(r[1]),
                            Content = Convert.ToString(r[2]),
                            UserID = new Guid(Convert.ToString(r[3])),
                            Datetime = Convert.ToDateTime(r[4])
                        }
                };

            if (r[5] != null)
            {
                comment.ID = new Guid(Convert.ToString(r[5]));
                comment.Content = Convert.ToString(r[6]);
                comment.UserID = new Guid(Convert.ToString(r[7]));
                comment.Datetime = Convert.ToDateTime(r[8]);
            }
            return comment;
        }

        private Tuple<Feed, object> ToFeed(Tuple<Post, IEnumerable<Comment>> p)
        {
            var post = p.Item1;

            var itemUrl = "/products/community/modules/blogs/viewblog.aspx?blogid=" + post.ID;
            var commentApiUrl = "/api/2.0/community/blog/" + post.ID + "/comment.json";

            var comments = p.Item2.Where(c => c.ID != Guid.Empty).OrderBy(c => c.Datetime).ToList();
            var feedDate = comments.Any() ? comments.First().Datetime : post.Datetime;
            var feedAuthor = comments.Any() ? comments.Last().UserID : post.UserID;

            var feed = new Feed(post.UserID, post.Datetime)
                {
                    Item = item,
                    ItemId = post.ID.ToString(),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    ModifiedBy = feedAuthor,
                    ModifiedDate = feedDate,
                    Product = Product,
                    Module = Name,
                    Action = comments.Any() ? FeedAction.Commented : FeedAction.Created,
                    Title = post.Title,
                    Description = HtmlSanitizer.Sanitize(post.Content),
                    HasPreview = post.Content.Contains("class=\"asccut\""),
                    CanComment = true,
                    CommentApiUrl = CommonLinkUtility.ToAbsolute(commentApiUrl),
                    Comments = comments.Select(ToFeedComment),
                    GroupId = string.Format("{0}_{1}", item, post.ID)
                };
            feed.Keywords = string.Format("{0} {1} {2}",
                                          post.Title,
                                          Helper.GetText(post.Content),
                                          string.Join(" ", feed.Comments.Select(x => x.Description)));

            return new Tuple<Feed, object>(feed, post);
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