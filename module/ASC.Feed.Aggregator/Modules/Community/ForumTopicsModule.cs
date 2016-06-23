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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Forum;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Utility;
using System.Linq;

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

            var itemUrl = "/products/community/modules/forum/posts.aspx?t=" + post.Topic.ID + "&post=" + post.ID;
            var threadUrl = "/products/community/modules/forum/topics.aspx?f=" + post.Topic.ThreadID;
            return new Feed(post.Topic.PosterID, post.Topic.CreateDate)
                {
                    Item = item,
                    ItemId = post.Topic.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = post.Topic.Title,
                    Description = HtmlSanitizer.Sanitize(post.Text),
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