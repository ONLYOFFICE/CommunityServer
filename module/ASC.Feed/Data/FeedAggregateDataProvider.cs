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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using Newtonsoft.Json;

namespace ASC.Feed.Data
{
    public class FeedAggregateDataProvider
    {
        public static DateTime GetLastTimeAggregate(string key)
        {
            var q = new SqlQuery("feed_last")
                .Select("last_date")
                .Where("last_key", key);

            using (var db = new DbManager(Constants.FeedDbId))
            {
                var value = db.ExecuteScalar<DateTime>(q);
                return value != default(DateTime) ? value.AddSeconds(1) : value;
            }
        }

        public static void SaveFeeds(IEnumerable<FeedRow> feeds, string key, DateTime value)
        {
            using (var db = new DbManager(Constants.FeedDbId))
            {
                db.ExecuteNonQuery(new SqlInsert("feed_last", true).InColumnValue("last_key", key).InColumnValue("last_date", value));
            }

            const int feedsPortionSize = 1000;
            var aggregatedDate = DateTime.UtcNow;

            var feedsPortion = new List<FeedRow>();
            foreach (var feed in feeds)
            {
                feedsPortion.Add(feed);
                if (feedsPortion.Sum(f => f.Users.Count) <= feedsPortionSize) continue;

                SaveFeedsPortion(feedsPortion, aggregatedDate);
                feedsPortion.Clear();
            }
            if (feedsPortion.Any())
            {
                SaveFeedsPortion(feedsPortion, aggregatedDate);
            }
        }

        private static void SaveFeedsPortion(IEnumerable<FeedRow> feeds, DateTime aggregatedDate)
        {
            using (var db = new DbManager(Constants.FeedDbId))
            using (var tx = db.BeginTransaction())
            {
                var i = new SqlInsert("feed_aggregate", true)
                    .InColumns("id", "tenant", "product", "module", "author", "modified_by", "group_id", "created_date",
                        "modified_date", "json", "keywords", "aggregated_date");
                var i2 = new SqlInsert("feed_users", true).InColumns("feed_id", "user_id");

                foreach (var f in feeds)
                {
                    if (0 >= f.Users.Count) continue;

                   i.Values(f.Id, f.Tenant, f.ProductId, f.ModuleId, f.AuthorId, f.ModifiedById, f.GroupId, f.CreatedDate, f.ModifiedDate, f.Json, f.Keywords, aggregatedDate);



                    if (f.ClearRightsBeforeInsert)
                    {
                        db.ExecuteNonQuery(
                            new SqlDelete("feed_users")
                                .Where("feed_id", f.Id)
                            );
                    }

                    foreach (var u in f.Users)
                    {
                        i2.Values(f.Id, u.ToString());
                    }
                }

                db.ExecuteNonQuery(i);
                db.ExecuteNonQuery(i2);

                tx.Commit();
            }
        }

        public static void RemoveFeedAggregate(DateTime fromTime)
        {
            using (var db = new DbManager(Constants.FeedDbId))
            using (var command = db.Connection.CreateCommand())
            using (var tx = db.Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                command.Transaction = tx;
                command.CommandTimeout = 60 * 60; // a hour
                var dialect = DbRegistry.GetSqlDialect(Constants.FeedDbId);
                if (dialect.SupportMultiTableUpdate)
                {
                    command.ExecuteNonQuery("delete from feed_aggregate, feed_users using feed_aggregate, feed_users where id = feed_id and aggregated_date < @date", new { date = fromTime });
                }
                else
                {
                    command.ExecuteNonQuery(new SqlDelete("feed_users").Where(Exp.In("feed_id", new SqlQuery("feed_aggregate").Select("id").Where(Exp.Lt("aggregated_date", fromTime)))), dialect);
                    command.ExecuteNonQuery(new SqlDelete("feed_aggregate").Where(Exp.Lt("aggregated_date", fromTime)), dialect);
                }
                tx.Commit();
            }
        }

        public static List<FeedResultItem> GetFeeds(FeedApiFilter filter)
        {
            var filterOffset = filter.Offset;
            var filterLimit = filter.Max > 0 && filter.Max < 1000 ? filter.Max : 1000;

            var feeds = new Dictionary<string, List<FeedResultItem>>();

            var tryCount = 0;
            List<FeedResultItem> feedsIteration;
            do
            {
                feedsIteration = GetFeedsInternal(filter);
                foreach (var feed in feedsIteration)
                {
                    if (feeds.ContainsKey(feed.GroupId))
                    {
                        feeds[feed.GroupId].Add(feed);
                    }
                    else
                    {
                        feeds[feed.GroupId] = new List<FeedResultItem> { feed };
                    }
                }
                filter.Offset += feedsIteration.Count;
            } while (feeds.Count < filterLimit
                     && feedsIteration.Count == filterLimit
                     && tryCount++ < 5);

            filter.Offset = filterOffset;
            return feeds.Take(filterLimit).SelectMany(group => group.Value).ToList();
        }

        private static List<FeedResultItem> GetFeedsInternal(FeedApiFilter filter)
        {
            var query = new SqlQuery("feed_aggregate a")
                .InnerJoin("feed_users u", Exp.EqColumns("a.id", "u.feed_id"))
                .Select("a.json, a.module, a.author, a.modified_by, a.group_id, a.created_date, a.modified_date, a.aggregated_date")
                .Where("a.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where(
                    !Exp.Eq("a.modified_by", SecurityContext.CurrentAccount.ID) &
                    Exp.Eq("u.user_id", SecurityContext.CurrentAccount.ID)
                )
                .OrderBy("a.modified_date", false)
                .SetFirstResult(filter.Offset)
                .SetMaxResults(filter.Max);

            if (filter.OnlyNew)
            {
                query.Where(Exp.Ge("a.aggregated_date", filter.From));
            }
            else
            {
                if (1 < filter.From.Year)
                {
                    query.Where(Exp.Ge("a.modified_date", filter.From));
                }
                if (filter.To.Year < 9999)
                {
                    query.Where(Exp.Le("a.modified_date", filter.To));
                }
            }

            if (!string.IsNullOrEmpty(filter.Product))
            {
                query.Where("a.product", filter.Product);
            }
            if (filter.Author != Guid.Empty)
            {
                query.Where("a.modified_by", filter.Author);
            }
            if (filter.SearchKeys != null && filter.SearchKeys.Length > 0)
            {
                var exp = filter.SearchKeys
                                .Where(s => !string.IsNullOrEmpty(s))
                                .Select(s => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_"))
                                .Aggregate(Exp.False, (cur, s) => cur | Exp.Like("a.keywords", s, SqlLike.AnyWhere));
                query.Where(exp);
            }

            using (var db = new DbManager(Constants.FeedDbId))
            {
                var news = db
                    .ExecuteList(query)
                    .ConvertAll(r => new FeedResultItem(
                                         Convert.ToString(r[0]),
                                         Convert.ToString(r[1]),
                                         new Guid(Convert.ToString(r[2])),
                                         new Guid(Convert.ToString(r[3])),
                                         Convert.ToString(r[4]),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6])),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[7]))));
                return news;
            }
        }

        public static int GetNewFeedsCount(DateTime lastReadedTime)
        {
            var q = new SqlQuery("feed_aggregate a")
                .Select("id")
                .Where("a.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where(!Exp.Eq("a.modified_by", SecurityContext.CurrentAccount.ID))
                .InnerJoin("feed_users u", Exp.EqColumns("a.id", "u.feed_id"))
                .Where("u.user_id", SecurityContext.CurrentAccount.ID)
                .SetMaxResults(1001);
                
            if (1 < lastReadedTime.Year)
            {
                q.Where(Exp.Ge("a.aggregated_date", lastReadedTime));
            }

            using (var db = new DbManager(Constants.FeedDbId))
            {
                return db.ExecuteList(q).Count();
            }
        }

        public IEnumerable<int> GetTenants(TimeInterval interval)
        {
            using (var db = new DbManager(Constants.FeedDbId))
            {
                var q = new SqlQuery("feed_aggregate")
                    .Select("tenant")
                    .Where(Exp.Between("aggregated_date", interval.From, interval.To))
                    .GroupBy(1);
                return db.ExecuteList(q).ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public static FeedResultItem GetFeedItem(string id)
        {
            var query = new SqlQuery("feed_aggregate a")
                .Select("a.json, a.module, a.author, a.modified_by, a.group_id, a.created_date, a.modified_date, a.aggregated_date")
                .Where("a.id", id);

            using (var db = new DbManager(Constants.FeedDbId))
            {
                var news = db
                    .ExecuteList(query)
                    .ConvertAll(r => new FeedResultItem(
                                         Convert.ToString(r[0]),
                                         Convert.ToString(r[1]),
                                         new Guid(Convert.ToString(r[2])),
                                         new Guid(Convert.ToString(r[3])),
                                         Convert.ToString(r[4]),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6])),
                                         TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[7]))));

                return news.FirstOrDefault();
            }
        }

        public static void RemoveFeedItem(string id)
        {
            using (var db = new DbManager(Constants.FeedDbId))
            using (var command = db.Connection.CreateCommand())
            using (var tx = db.Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                command.Transaction = tx;
                command.CommandTimeout = 60 * 60; // a hour

                var dialect = DbRegistry.GetSqlDialect(Constants.FeedDbId);

                command.ExecuteNonQuery(new SqlDelete("feed_users").Where("feed_id", id), dialect);
                command.ExecuteNonQuery(new SqlDelete("feed_aggregate").Where("id", id), dialect);

                tx.Commit();
            }
        }
    }


    public class FeedResultItem
    {
        public FeedResultItem(
            string json, 
            string module, 
            Guid authorId, 
            Guid modifiedById,
            string groupId, 
            DateTime createdDate,
            DateTime modifiedDate, 
            DateTime aggregatedDate)
        {
            var now = TenantUtil.DateTimeFromUtc(DateTime.UtcNow);

            Json = json;
            Module = module;

            AuthorId = authorId;
            ModifiedById = modifiedById;
            
            GroupId = groupId;

            if (now.Year == createdDate.Year && now.Date == createdDate.Date)
            {
                IsToday = true;
            }
            else if (now.Year == createdDate.Year && now.Date == createdDate.Date.AddDays(1))
            {
                IsYesterday = true;
            }

            CreatedDate = createdDate;
            ModifiedDate = modifiedDate;
            AggregatedDate = aggregatedDate;
        }

        public string Json { get; private set; }

        public string Module { get; private set; }

        public Guid AuthorId { get; private set; }

        public Guid ModifiedById { get; private set; }

        public string GroupId { get; private set; }

        public bool IsToday { get; private set; }

        public bool IsYesterday { get; private set; }

        public DateTime CreatedDate { get; private set; }

        public DateTime ModifiedDate { get; private set; }

        public DateTime AggregatedDate { get; private set; }

        public FeedMin ToFeedMin()
        {
            var feedMin = JsonConvert.DeserializeObject<FeedMin>(Json);
            feedMin.Author = new FeedMinUser { UserInfo = CoreContext.UserManager.GetUsers(feedMin.AuthorId) };
            feedMin.CreatedDate = CreatedDate;

            if (feedMin.Comments == null) return feedMin;

            foreach (var comment in feedMin.Comments)
            {
                comment.Author = new FeedMinUser { UserInfo = CoreContext.UserManager.GetUsers(comment.AuthorId) };
            }
            return feedMin;
        }
    }
}