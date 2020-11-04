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
    internal class ForumTopicsModule : FeedModule
    {
        protected override string Table
        {
            get { return "forum_topic"; }
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
                .Select(TopicColumns().Select(t => "t." + t).ToArray())
                .InnerJoin("forum_thread th", Exp.EqColumns("t.thread_id", "th.id"))
                .Select(ThreadColumns().Select(th => "th." + th).ToArray())
                .Where("t.tenantid", filter.Tenant)
                .Where(Exp.EqColumns("p.create_date", "t.create_date"))
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

        private static IEnumerable<string> TopicColumns()
        {
            return new[]
                {
                    "id", // 6
                    "title",
                    "type",
                    "recent_post_id",
                    "create_date",
                    "poster_id",
                    "thread_id" // 12
                };
        }

        private static IEnumerable<string> ThreadColumns()
        {
            return new[]
                {
                    "id", //13
                    "title" //14
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
                    Text = Convert.ToString(r[5]),
                    Topic = new Topic
                        {
                            ID = Convert.ToInt32(r[6]),
                            Title = Convert.ToString(r[7]),
                            Type = (TopicType)Convert.ToInt32(r[8]),
                            RecentPostID = Convert.ToInt32(r[9]),
                            CreateDate = Convert.ToDateTime(r[10]),
                            PosterID = new Guid(Convert.ToString(r[11])),
                            ThreadID = Convert.ToInt32(r[12]),
                            ThreadTitle = Convert.ToString(r[14])
                        }
                };
        }

        private Feed ToFeed(Post post)
        {
            var item = string.Empty;
            if (post.Topic.Type == TopicType.Informational)
            {
                item = "forumTopic";
            }
            else if (post.Topic.Type == TopicType.Poll)
            {
                item = "forumPoll";
            }

            var itemUrl = "/Products/Community/Modules/Forum/Posts.aspx?t=" + post.Topic.ID + "&post=" + post.ID;
            var threadUrl = "/Products/Community/Modules/Forum/Topics.aspx?f=" + post.Topic.ThreadID;
            return new Feed(post.Topic.PosterID, post.Topic.CreateDate)
                {
                    Item = item,
                    ItemId = post.Topic.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = post.Topic.Title,
                    Description = HtmlUtility.GetFull(post.Text),
                    ExtraLocation = post.Topic.ThreadTitle,
                    ExtraLocationUrl = CommonLinkUtility.ToAbsolute(threadUrl),
                    Keywords = string.Format("{0} {1}", post.Topic.Title, post.Text),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, post.Topic.PosterID, post.Topic.ThreadID.ToString(CultureInfo.InvariantCulture))
                };
        }
    }
}