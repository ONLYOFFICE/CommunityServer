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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Community.News.Code;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Utility;
using Event = ASC.Web.Community.News.Code.Feed;
using EventComment = ASC.Web.Community.News.Code.FeedComment;
using System.Linq;

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

            var itemUrl = "/products/community/modules/news/?docid=" + evt.Id;
            var commentApiUrl = "/api/2.0/community/event/" + evt.Id + "/comment.json";

            var comments = e.Item2.Where(c => c.Id != 0).OrderBy(c => c.Date).ToList();
            var feedDate = comments.Any() ? comments.First().Date : evt.Date;
            var feedAutohor = comments.Any() ? comments.Last().Creator : evt.Creator;

            var feed = new Feed(new Guid(evt.Creator), feedDate)
                {
                    Item = item,
                    ItemId = evt.Id.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    LastModifiedBy = new Guid(feedAutohor),
                    Product = Product,
                    Module = Name,
                    Action = comments.Any() ? FeedAction.Commented : FeedAction.Created,
                    Title = evt.Caption,
                    Description = HtmlSanitizer.Sanitize(evt.Text),
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
                    Description = HtmlSanitizer.Sanitize(comment.Comment),
                    Date = comment.Date
                };
        }
    }
}