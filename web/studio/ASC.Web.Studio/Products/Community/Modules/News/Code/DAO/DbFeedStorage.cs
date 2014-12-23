/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

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
                if (FullTextSearch.SupportModule(FullTextSearch.NewsModule))
                {
                    var ids = FullTextSearch
                        .Search(s, FullTextSearch.NewsModule)
                        .GetIdentifiers()
                        .Select(int.Parse)
                        .ToList();
                    where = where & Exp.In("Id", ids);
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
                                         "~/products/community/modules/news/?docid=" + feed.Id)),
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
                                         "~/products/community/modules/news/?docid=" + feed.Id)),
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

        public void RemoveFeed(long id)
        {
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
                    new TagValue(NewsConst.TagURL, CommonLinkUtility.GetFullAbsolutePath("~/products/community/modules/news/?docid=" + feed.Id)),
                    new TagValue("CommentURL", CommonLinkUtility.GetFullAbsolutePath("~/products/community/modules/news/?docid=" + feed.Id + "#" + comment.Id.ToString(CultureInfo.InvariantCulture))),
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