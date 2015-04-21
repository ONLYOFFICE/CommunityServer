/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            var itemUrl = "/products/community/modules/forum/posts.aspx?t=" + post.TopicID + "&post=" + post.ID;
            return new Feed(post.PosterID, post.CreateDate)
                {
                    Item = item,
                    ItemId = post.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = post.Subject,
                    Description = HtmlSanitizer.Sanitize(post.Text),
                    Keywords = string.Format("{0} {1}", post.Subject, post.Text),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = string.Format("{0}_{1}", item, post.ID)
                };
        }
    }
}