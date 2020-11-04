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
using ASC.Web.Community.News.Code;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using Event = ASC.Web.Community.News.Code.Feed;
using EventComment = ASC.Web.Community.News.Code.FeedComment;

namespace ASC.Feed.Aggregator.Modules.Community
{
    internal class EventsModule : FeedModule
    {
        protected override string Table
        {
            get { return "events_feed"; }
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
            get { return Constants.EventsModule; }
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
            var q1 = new SqlQuery("events_feed")
                .Select("tenant")
                .Where(Exp.Gt("date", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("events_comment")
                .Select("tenant")
                .Where(Exp.Gt("date", fromTime))
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
            var query = new SqlQuery("events_feed e")
                .Select(EventColumns().Select(e => "e." + e).ToArray())
                .LeftOuterJoin("events_comment c",
                               Exp.EqColumns("c.feed", "e.id") &
                               Exp.Eq("c.tenant", filter.Tenant)
                )
                .Select(EventCommentColumns().Select(c => "c." + c).ToArray())
                .Where("e.tenant", filter.Tenant)
                .Where(Exp.Between("e.date", filter.Time.From, filter.Time.To) |
                       Exp.Between("c.date", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var comments = db.ExecuteList(query).ConvertAll(ToComment);
                var groupedEvents = comments.GroupBy(c => c.Feed.Id);

                return groupedEvents
                    .Select(e => new Tuple<Event, IEnumerable<EventComment>>(e.First().Feed, e))
                    .Select(ToFeed);
            }
        }


        private static IEnumerable<string> EventColumns()
        {
            return new[]
                {
                    "id",
                    "feedtype",
                    "caption",
                    "text",
                    "date",
                    "creator" // 5
                };
        }

        private static IEnumerable<string> EventCommentColumns()
        {
            return new[]
                {
                    "id", // 6
                    "comment",
                    "parent",
                    "date",
                    "creator" // 10
                };
        }

        private static EventComment ToComment(object[] r)
        {
            var comment = new EventComment
                {
                    Feed = new Event
                        {
                            Id = Convert.ToInt64(r[0]),
                            FeedType = (FeedType)Convert.ToInt32(r[1]),
                            Caption = Convert.ToString(r[2]),
                            Text = Convert.ToString(r[3]),
                            Date = Convert.ToDateTime(r[4]),
                            Creator = Convert.ToString(r[5])
                        }
                };

            if (r[6] != null)
            {
                comment.Id = Convert.ToInt64(r[6]);
                comment.Comment = Convert.ToString(r[7]);
                comment.ParentId = Convert.ToInt64(r[8]);
                comment.Date = Convert.ToDateTime(r[9]);
                comment.Creator = Convert.ToString(r[10]);
            }
            return comment;
        }

        private Tuple<Feed, object> ToFeed(Tuple<Event, IEnumerable<EventComment>> e)
        {
            var evt = e.Item1;
            var item = e.Item1.FeedType.ToString().ToLowerInvariant();

            var itemUrl = "/Products/Community/Modules/News/Default.aspx?docid=" + evt.Id;
            var commentApiUrl = "/api/2.0/community/event/" + evt.Id + "/comment.json";

            var comments = e.Item2.Where(c => c.Id != 0).OrderBy(c => c.Date).ToList();
            var feedDate = comments.Any() ? comments.First().Date : evt.Date;
            var feedAuthor = comments.Any() ? comments.Last().Creator : evt.Creator;

            var feed = new Feed(new Guid(evt.Creator), evt.Date)
                {
                    Item = item,
                    ItemId = evt.Id.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    ModifiedBy = new Guid(feedAuthor),
                    ModifiedDate = feedDate,
                    Product = Product,
                    Module = Name,
                    Action = comments.Any() ? FeedAction.Commented : FeedAction.Created,
                    Title = evt.Caption,
                    Description = HtmlUtility.GetFull(evt.Text),
                    HasPreview = false,
                    CanComment = true,
                    CommentApiUrl = CommonLinkUtility.ToAbsolute(commentApiUrl),
                    Comments = comments.Select(ToFeedComment),
                    GroupId = string.Format("{0}_{1}", item, evt.Id)
                };
            feed.Keywords = string.Format("{0} {1} {2}",
                                          evt.Caption,
                                          Helper.GetText(evt.Text),
                                          string.Join(" ", feed.Comments.Select(x => x.Description)));

            return new Tuple<Feed, object>(feed, evt);
        }

        private static FeedComment ToFeedComment(EventComment comment)
        {
            return new FeedComment(new Guid(comment.Creator))
                {
                    Id = comment.Id.ToString(CultureInfo.InvariantCulture),
                    Description = HtmlUtility.GetFull(comment.Comment),
                    Date = comment.Date
                };
        }
    }
}