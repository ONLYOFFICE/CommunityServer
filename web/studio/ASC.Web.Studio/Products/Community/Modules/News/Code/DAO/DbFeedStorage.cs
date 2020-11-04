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

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Core.Tenants;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.ElasticSearch;
using ASC.Web.Community.Search;

namespace ASC.Web.Community.News.Code.DAO
{
    internal class DbFeedStorage : IFeedStorage
    {
        private readonly DbManager dbManager = new DbManager(FeedStorageFactory.Id);

        private readonly int tenant;

        private const int minSearchLength = 3;


        public DbFeedStorage(int tenant)
        {
            this.tenant = tenant;
        }

        public List<FeedType> GetUsedFeedTypes()
        {
            return dbManager
                .ExecuteList(Query("events_feed").Select("FeedType").GroupBy(1))
                .ConvertAll(row => (FeedType)Convert.ToInt32(row[0]))
                .OrderBy(f => f)
                .ToList();
        }

        public List<Feed> GetFeeds(FeedType feedType, Guid userId, int count, int offset)
        {
            return SearchFeeds(null, feedType, userId, count, offset);
        }

        public long GetFeedsCount(FeedType feedType, Guid userId)
        {
            return SearchFeedsCount(null, feedType, userId);
        }

        public List<Feed> SearchFeeds(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length < minSearchLength)
            {
                return new List<Feed>();
            }

            return dbManager
                .ExecuteList(Query("events_feed").Select("Id", "Caption", "Text", "Date")
                                                 .Where(GetWhere(s, FeedType.All, Guid.Empty))
                                                 .OrderBy("Id", false))
                .ConvertAll(Mappers.ToFeedFinded);
        }

        public long SearchFeedsCount(string s, FeedType feedType, Guid userId)
        {
            if (s != null && s.Length < minSearchLength)
            {
                return 0;
            }

            return dbManager.ExecuteScalar<long>(Query("events_feed").SelectCount().Where(GetWhere(s, feedType, userId)));
        }

        public List<Feed> SearchFeeds(string s, FeedType feedType, Guid userId, int count, int offset)
        {
            if (s != null && s.Length < minSearchLength)
            {
                return new List<Feed>();
            }

            var select = Query("events_feed")
                .Select("Id", "FeedType", "Caption", "Date", "Creator")
                .Select(GetFeedReadedQuery())
                .Where(GetWhere(s, feedType, userId))
                .OrderBy("Id", false)
                .SetFirstResult(offset)
                .SetMaxResults(count);

            return dbManager
                .ExecuteList(select)
                .ConvertAll(Mappers.ToFeed);
        }

        private static Exp GetWhere(string s, FeedType feedType, Guid userId)
        {
            var where = Exp.Empty;

            if (!string.IsNullOrEmpty(s))
            {
                List<int> news;
                if (FactoryIndexer<NewsWrapper>.TrySelectIds(r => r.MatchAll(s), out news))
                {
                    where = where & Exp.In("Id", news);
                }
                else
                {
                    where = where & (Exp.Like("lower(Caption)", s.ToLower()) | Exp.Like("lower(Text)", s.ToLower()));
                }
            }
            if (feedType == FeedType.AllNews)
            {
                where = where & !Exp.Eq("FeedType", FeedType.Poll);
            }
            else if (feedType != FeedType.All)
            {
                where = where & Exp.Eq("FeedType", feedType);
            }
            if (userId != Guid.Empty)
            {
                where = where & Exp.Eq("Creator", userId.ToString());
            }
            return where;
        }

        private SqlQuery GetFeedReadedQuery()
        {
            return Query("events_reader")
                .SelectCount()
                .Where(Exp.EqColumns("Feed", "Id") & Exp.Eq("Reader", SecurityContext.CurrentAccount.ID.ToString()));
        }

        public Feed GetFeed(long id)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                var list = dbManager
                    .ExecuteList(
                        Query("events_feed").Select("Id", "FeedType", "Caption", "Text", "Date", "Creator").Select(
                            GetFeedReadedQuery()).Where("Id", id))
                    .ConvertAll(Mappers.ToNewsOrPoll);

                var feed = 0 < list.Count ? list[0] : null;
                if (feed == null || feed is FeedNews) return feed;

                var poll = (FeedPoll)feed;
                dbManager.ExecuteList(Query("events_poll").Select("PollType", "StartDate", "EndDate").Where("Id", id))
                         .ForEach(row =>
                                      {
                                          poll.PollType = (FeedPollType)Convert.ToInt32(row[0]);
                                          poll.StartDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[1]));
                                          poll.EndDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2]));
                                      });

                dbManager.ExecuteList(
                    Query("events_pollvariant v").Select("v.Id", "v.Name", "a.User")
                                                 .LeftOuterJoin("events_pollanswer a", Exp.EqColumns("v.Id", "a.Variant"))
                                                 .Where("v.Poll", id))
                         .ForEach(row =>
                                      {
                                          var variantId = Convert.ToInt64(row[0]);
                                          if (!poll.Variants.Exists(v => v.ID == variantId))
                                          {
                                              poll.Variants.Add(new FeedPollVariant
                                                  {
                                                      ID = variantId,
                                                      Name = Convert.ToString(row[1])
                                                  });
                                          }
                                          if (row[2] != null)
                                          {
                                              poll.Answers.Add(new FeedPollAnswer(variantId, Convert.ToString(row[2])));
                                          }
                                      });
                tx.Commit();

                return poll;
            }
        }

        public List<Feed> GetFeedByDate(DateTime from, DateTime to, Guid userId)
        {
            var query = new SqlQuery("events_feed")
                .Select("Id", "FeedType", "Caption", "Date", "Creator", "true")
                .Where(Exp.Between("Date", from, to) & Exp.Eq("Tenant", tenant))
                .OrderBy("Date", true);
            return dbManager.ExecuteList(query).Select(Mappers.ToFeed).ToList();
        }

        public List<FeedComment> GetCommentsByDate(DateTime from, DateTime to)
        {
            var query = new SqlQuery("events_comment")
                .SelectAll()
                .Where(Exp.Between("Date", from, to) & Exp.Eq("Tenant", tenant))
                .OrderBy("Date", true);
            return dbManager.ExecuteList(query).Select(Mappers.ToFeedComment).ToList();
        }

        public Feed SaveFeed(Feed feed, bool isEdit, FeedType type)
        {
            if (feed == null) throw new ArgumentNullException("feed");

            using (var tx = dbManager.BeginTransaction())
            {
                feed.Id = dbManager.ExecuteScalar<long>(
                    Insert("events_feed").InColumns("Id", "FeedType", "Caption", "Text", "Date", "Creator")
                                         .Values(feed.Id, feed.FeedType, feed.Caption, feed.Text, TenantUtil.DateTimeToUtc(feed.Date),
                                                 feed.Creator)
                                         .Identity(1, 0L, true)
                    );

                var poll = feed as FeedPoll;
                if (poll != null)
                {
                    dbManager.ExecuteNonQuery(Insert("events_poll").InColumns("Id", "PollType", "StartDate", "EndDate")
                                                                   .Values(poll.Id, poll.PollType,
                                                                           TenantUtil.DateTimeToUtc(poll.StartDate),
                                                                           TenantUtil.DateTimeToUtc(poll.EndDate)));

                    dbManager.ExecuteNonQuery(Delete("events_pollvariant").Where("Poll", poll.Id));
                    foreach (var variant in poll.Variants)
                    {
                        variant.ID = dbManager.ExecuteScalar<long>(
                            Insert("events_pollvariant").InColumns("Id", "Name", "Poll")
                                                        .Values(variant.ID, variant.Name, poll.Id)
                                                        .Identity(1, 0L, true));
                    }
                }
                tx.Commit();
            }


            NotifyFeed(feed, isEdit, type);

            FactoryIndexer<NewsWrapper>.IndexAsync(feed);
            return feed;
        }

        private static void NotifyFeed(Feed feed, bool isEdit, FeedType type)
        {
            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(feed.Creator, ""));
            try
            {
                NewsNotifyClient.NotifyClient.AddInterceptor(initatorInterceptor);
                var replyToTag = GetReplyToTag(feed, null);
                if (type == FeedType.Poll && feed is FeedPoll)
                {
                    NewsNotifyClient.NotifyClient.SendNoticeAsync(
                        NewsConst.NewFeed, null, null,
                        new TagValue(NewsConst.TagFEED_TYPE, "poll"),
                        new TagValue(NewsConst.TagAnswers, ((FeedPoll)feed).Variants.ConvertAll(v => v.Name)),
                        new TagValue(NewsConst.TagCaption, feed.Caption),
                        new TagValue(NewsConst.TagText, HtmlUtility.GetFull(feed.Text)),
                        new TagValue(NewsConst.TagDate, feed.Date.ToShortString()),
                        new TagValue(NewsConst.TagURL,
                                     CommonLinkUtility.GetFullAbsolutePath(
                                         "~/Products/Community/Modules/News/Default.aspx?docid=" + feed.Id)),
                        new TagValue(NewsConst.TagUserName,
                                     DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                        new TagValue(NewsConst.TagUserUrl,
                                     CommonLinkUtility.GetFullAbsolutePath(
                                         CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID))),
                        replyToTag
                        );
                }
                else
                {
                    NewsNotifyClient.NotifyClient.SendNoticeAsync(
                        NewsConst.NewFeed, null, null,
                        new TagValue(NewsConst.TagFEED_TYPE, "feed"),
                        new TagValue(NewsConst.TagCaption, feed.Caption),
                        new TagValue(NewsConst.TagText,
                                     HtmlUtility.GetFull(feed.Text)),
                        new TagValue(NewsConst.TagDate, feed.Date.ToShortString()),
                        new TagValue(NewsConst.TagURL,
                                     CommonLinkUtility.GetFullAbsolutePath(
                                         "~/Products/Community/Modules/News/Default.aspx?docid=" + feed.Id)),
                        new TagValue(NewsConst.TagUserName,
                                     DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                        new TagValue(NewsConst.TagUserUrl,
                                     CommonLinkUtility.GetFullAbsolutePath(
                                         CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID))),
                        replyToTag
                        );
                }

                // subscribe to new comments
                var subsciber = NewsNotifySource.Instance.GetSubscriptionProvider();
                var me = (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
                if (me != null && !subsciber.IsUnsubscribe(me, NewsConst.NewComment, feed.Id.ToString(CultureInfo.InvariantCulture)))
                {
                    subsciber.Subscribe(NewsConst.NewComment, feed.Id.ToString(CultureInfo.InvariantCulture), me);
                }
            }
            finally
            {
                NewsNotifyClient.NotifyClient.RemoveInterceptor(initatorInterceptor.Name);

            }
        }

        public void ReadFeed(long feedId, string reader)
        {
            dbManager.ExecuteNonQuery(Insert("events_reader").InColumns("Feed", "Reader").Values(feedId, reader));
        }

        public void PollVote(string userId, ICollection<long> variantIds)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                foreach (var variantId in variantIds)
                {
                    if (variantId == 0) continue;
                    dbManager.ExecuteNonQuery(Insert("events_pollanswer").InColumns("Variant", "User").Values(variantId, userId));
                }
                tx.Commit();
            }
        }

        public void RemoveFeed(Feed feed)
        {
            if (feed == null) return;

            var id = feed.Id;

            using (var tx = dbManager.BeginTransaction())
            {
                dbManager.ExecuteNonQuery(Delete("events_pollanswer").Where(Exp.In("Variant", Query("events_pollvariant").Select("Id").Where(Exp.Eq("Poll", id)))));
                dbManager.ExecuteNonQuery(Delete("events_pollvariant").Where("Poll", id));
                dbManager.ExecuteNonQuery(Delete("events_poll").Where("Id", id));
                dbManager.ExecuteNonQuery(Delete("events_comment").Where("Feed", id));
                dbManager.ExecuteNonQuery(Delete("events_reader").Where("Feed", id));
                dbManager.ExecuteNonQuery(Delete("events_feed").Where("Id", id));
                tx.Commit();
            }

            AscCache.Default.Remove("communityScreen" + TenantProvider.CurrentTenantID);
            FactoryIndexer<NewsWrapper>.DeleteAsync(feed);
        }

        public List<FeedComment> GetFeedComments(long feedId)
        {
            return dbManager
                .ExecuteList(GetFeedCommentQuery(Exp.Eq("Feed", feedId)))
                .ConvertAll(Mappers.ToFeedComment);
        }

        public FeedComment GetFeedComment(long commentId)
        {
            var list = dbManager
                .ExecuteList(GetFeedCommentQuery(Exp.Eq("Id", commentId)))
                .ConvertAll(Mappers.ToFeedComment);
            return 0 < list.Count ? list[0] : null;
        }

        private SqlQuery GetFeedCommentQuery(Exp where)
        {
            return Query("events_comment").Select("Id", "Feed", "Comment", "Parent", "Date", "Creator", "Inactive").Where(where);
        }

        public FeedComment SaveFeedComment(Feed feed, FeedComment comment)
        {
            SaveFeedComment(comment);
            if (feed != null)
            {
                NotifyNewComment(comment, feed);
            }
            return comment;
        }

        public void RemoveFeedComment(FeedComment comment)
        {
            SaveFeedComment(comment);
        }

        public void UpdateFeedComment(FeedComment comment)
        {
            SaveFeedComment(comment);
        }

        private void SaveFeedComment(FeedComment comment)
        {
            if (comment == null) throw new ArgumentNullException("comment");

            comment.Id = dbManager.ExecuteScalar<long>(
                Insert("events_comment").InColumns("Id", "Feed", "Comment", "Parent", "Date", "Creator", "Inactive")
                                        .Values(comment.Id, comment.FeedId, comment.Comment, comment.ParentId, TenantUtil.DateTimeToUtc(comment.Date), comment.Creator, comment.Inactive)
                                        .Identity(1, 0L, true)
                );
        }

        private void NotifyNewComment(FeedComment comment, Feed feed)
        {
            var feedType = feed.FeedType == FeedType.Poll ? "poll" : "feed";

            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(comment.Creator, ""));
            try
            {
                NewsNotifyClient.NotifyClient.AddInterceptor(initatorInterceptor);
                NewsNotifyClient.NotifyClient.SendNoticeAsync(
                    NewsConst.NewComment, feed.Id.ToString(CultureInfo.InvariantCulture),
                    null,
                    new TagValue(NewsConst.TagFEED_TYPE, feedType),
                    //new TagValue(NewsConst.TagAnswers, feed.Variants.ConvertAll<string>(v => v.Name)),
                    new TagValue(NewsConst.TagCaption, feed.Caption),
                    new TagValue("CommentBody", HtmlUtility.GetFull(comment.Comment)),
                    new TagValue(NewsConst.TagDate, comment.Date.ToShortString()),
                    new TagValue(NewsConst.TagURL, CommonLinkUtility.GetFullAbsolutePath("~/Products/Community/Modules/News/Default.aspx?docid=" + feed.Id)),
                    new TagValue("CommentURL", CommonLinkUtility.GetFullAbsolutePath("~/Products/Community/Modules/News/Default.aspx?docid=" + feed.Id + "#container_" + comment.Id.ToString(CultureInfo.InvariantCulture))),
                    new TagValue(NewsConst.TagUserName, DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                    new TagValue(NewsConst.TagUserUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID))),
                    GetReplyToTag(feed, comment)
                    );
            }
            finally
            {
                NewsNotifyClient.NotifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }

        private static TagValue GetReplyToTag(Feed feed, FeedComment feedComment)
        {
            return ReplyToTagProvider.Comment("event", feed.Id.ToString(CultureInfo.InvariantCulture), feedComment != null ? feedComment.Id.ToString(CultureInfo.InvariantCulture) : null);
        }

        public void RemoveFeedComment(long commentId)
        {
            dbManager.ExecuteNonQuery(Delete("events_comment").Where("Id", commentId));
        }

        public void Dispose()
        {
            dbManager.Dispose();
        }


        private SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), tenant);
        }

        private SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), tenant);
        }

        private SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(GetTenantColumnName(table)).Values(tenant);
        }

        private static string GetTenantColumnName(string table)
        {
            const string tenantColumnName = "Tenant";
            if (!table.Contains(" ")) return tenantColumnName;
            return table.Substring(table.IndexOf(" ")).Trim() + "." + tenantColumnName;
        }
    }
}