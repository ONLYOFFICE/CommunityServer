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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Forum;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Feed.Aggregator.Modules.Community
{
    internal class ForumPostsModule : FeedModule
    {
        private const string item = "forumPost";


        protected override string Table
        {
            get { return "forum_post"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "create_date"; }
        }

        protected override string TenantColumn
        {
            get { return "tenantId"; }
        }

        protected override string DbId
        {
            get { return Constants.CommunityDbId; }
        }


        public override string Name
        {
            get { return Constants.ForumsModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.CommunityProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.CommunityProductID; }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query = new SqlQuery("forum_post p")
                .Select(PostColumns().Select(p => "p." + p).ToArray())
                .InnerJoin("forum_topic t", Exp.EqColumns("p.topic_id", "t.id"))
                .Where("p.tenantId", filter.Tenant)
                .Where(!Exp.EqColumns("p.create_date", "t.create_date"))
                .Where(Exp.Between("p.create_date", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var posts = db.ExecuteList(query).ConvertAll(ToPost);
                return posts.Select(c => new Tuple<Feed, object>(ToFeed(c), c));
            }
        }

        private static IEnumerable<string> PostColumns()
        {
            return new[]
                {
                    "id", // 0
                    "topic_id",
                    "poster_id",
                    "create_date",
                    "subject",
                    "text" // 5
                };
        }


        private static Post ToPost(object[] r)
        {
            return new Post
                {
                    ID = Convert.ToInt32(r[0]),
                    TopicID = Convert.ToInt32(r[1]),
                    PosterID = new Guid(Convert.ToString(r[2])),
                    CreateDate = Convert.ToDateTime(r[3]),
                    Subject = Convert.ToString(r[4]),
                    Text = Convert.ToString(r[5])
                };
        }

        private Feed ToFeed(Post post)
        {
            var itemUrl = "/Products/Community/Modules/Forum/Posts.aspx?t=" + post.TopicID + "&post=" + post.ID;
            return new Feed(post.PosterID, post.CreateDate)
                {
                    Item = item,
                    ItemId = post.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = post.Subject,
                    Description = HtmlUtility.GetFull(post.Text),
                    Keywords = string.Format("{0} {1}", post.Subject, post.Text),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = string.Format("{0}_{1}", item, post.ID)
                };
        }
    }
}