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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Community.Forum;
using ASC.Web.Community.Product;
using ASC.Web.Studio.Utility;
using ASC.ElasticSearch;
using ASC.Web.Community.Search;

namespace ASC.Forum
{
    public enum DeletePostResult
    {
        Successfully,

        ReferencesBlock,

        Error
    }

    [Flags]
    public enum SearchTopicPlace
    {
        TopicName = 1,

        PostText = 2,

        TagName = 4,

        All = TopicName | PostText | TagName
    }

    internal class ThreadVisitInfo
    {
        public DateTime RecentVisitDate { get; set; }
        public int ThreadID { get; set; }
        public Dictionary<int, int> TopicViewRecentPostIDs { get; set; }

        public ThreadVisitInfo()
        {
            TopicViewRecentPostIDs = new Dictionary<int, int>();
            RecentVisitDate = DateTime.MinValue;
        }

        public static ThreadVisitInfo GetThreadVisitInfo(int threadID)
        {
            if (HttpContext.Current != null)
            {
                var key = UserKeys.StringSessionKey + SecurityContext.CurrentAccount.ID.ToString();
                var hash = HttpContext.Current.Session[key] as Hashtable;
                if (hash == null)
                {
                    hash = Hashtable.Synchronized(new Hashtable());
                    foreach (var f in ForumDataProvider.InitFirstVisit())
                    {
                        hash[UserKeys.StringThreadKey + f.ThreadID.ToString(CultureInfo.InvariantCulture)] = f;
                    }
                    HttpContext.Current.Session.Add(key, hash);
                }

                var threadKey = UserKeys.StringThreadKey + threadID.ToString(CultureInfo.InvariantCulture);
                var tvi = new ThreadVisitInfo { ThreadID = threadID };
                if (hash.Contains(threadKey))
                {
                    tvi = (ThreadVisitInfo)hash[threadKey];
                }
                else
                {
                    hash[threadKey] = tvi;
                    HttpContext.Current.Session[key] = hash;
                }

                return tvi;
            }

            return null;
        }

        public static DateTime GetMinVisitThreadDate()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return DateTime.MinValue;

            Guid userID = SecurityContext.CurrentAccount.ID;
            if (context.Session[UserKeys.StringSessionKey + userID.ToString()] == null)
                return DateTime.MinValue;

            DateTime result = DateTime.MinValue;
            var hash = (Hashtable)context.Session[UserKeys.StringSessionKey + userID.ToString()];

            foreach (var tvi in hash.Values)
            {
                if (tvi == null || !(tvi is ThreadVisitInfo)) continue;
                
                var t = (tvi as ThreadVisitInfo);
                if (result == DateTime.MinValue)
                    result = t.RecentVisitDate;

                if (t.RecentVisitDate < result)
                    result = t.RecentVisitDate;
            }

            return result;
        }
    }

    public class ForumDataProvider
    {
        internal static IDbManager DbManager
        {
            get
            {
                return Common.Data.DbManager.FromHttpContext(ASC.Web.Community.Forum.ForumManager.DbId);
            }
        }

        public static void SetThreadVisit(int tenantID, int threadID)
        {
            DbManager.ExecuteNonQuery(new SqlInsert("forum_lastvisit", true)
                .InColumns("tenantid", "user_id", "thread_id", "last_visit")
                .Values(tenantID, SecurityContext.CurrentAccount.ID.ToString(), threadID, DateTime.UtcNow));
        }

        public static void SetTopicVisit(Topic topic)
        {
            if (topic.RecentPostID > 0)
            {
                ThreadVisitInfo tvi = ThreadVisitInfo.GetThreadVisitInfo(topic.ThreadID);
                if (tvi != null)
                    tvi.TopicViewRecentPostIDs[topic.ID] = topic.RecentPostID;
            }

            SetThreadVisit(topic.TenantID, topic.ThreadID);

            DbManager.Connection.CreateCommand(@"update forum_topic set view_count = view_count+1 where id = @topicID and TenantID = @tid")
                .AddParameter("topicID", topic.ID)
                .AddParameter("tid", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .ExecuteNonQuery();
        }

        internal static IEnumerable<ThreadVisitInfo> InitFirstVisit()
        {
            return DbManager.ExecuteList(new SqlQuery("forum_lastvisit")
                .Select("thread_id", "last_visit")
                .Where("user_id", SecurityContext.CurrentAccount.ID.ToString())
                .Where("tenantid", CoreContext.TenantManager.GetCurrentTenant().TenantId))
                .ConvertAll(r => new ThreadVisitInfo { ThreadID = Convert.ToInt32(r[0]), RecentVisitDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[1])), });
        }

        #region Thread & Categories

        private static void ParseThreadCategories(IEnumerable<object[]> data, bool checkNew, out List<ThreadCategory> categories, out List<Thread> threads)
        {
            categories = new List<ThreadCategory>();
            threads = new List<Thread>();

            foreach (var row in data)
            {
                var cid = Convert.ToInt32(row[0]);
                if (categories.Find(c => c.ID == cid) == null)
                {
                    categories.Add(new ThreadCategory
                                       {
                                           ID = cid,
                                           TenantID = Convert.ToInt32(row[1]),
                                           Title = Convert.ToString(row[2]),
                                           Description = (row[3] != null ? Convert.ToString(row[3]) : ""),
                                           SortOrder = Convert.ToInt32(row[4]),
                                           CreateDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[5])),
                                           PosterID = new Guid(Convert.ToString(row[6]))
                                       });
                }

                if (row[7] != null)
                {
                    threads.Add(new Thread
                                    {
                                        CategoryID = cid,
                                        ID = Convert.ToInt32(row[7]),
                                        TenantID = Convert.ToInt32(row[1]),
                                        Title = Convert.ToString(row[8]),
                                        Description = (row[9] != null ? Convert.ToString(row[9]) : ""),
                                        SortOrder = Convert.ToInt32(row[10]),
                                        TopicCount = Convert.ToInt32(row[11]),
                                        PostCount = Convert.ToInt32(row[12]),
                                        RecentPostID = Convert.ToInt32(row[13]),
                                        RecentTopicID = Convert.ToInt32(row[14]),
                                        IsApproved = Convert.ToBoolean(row[15]),
                                        RecentTopicTitle = (row[16] != null ? Convert.ToString(row[16]) : ""),
                                        RecentPostCreateDate = (row[17]) != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[17])) : DateTime.MinValue,
                                        RecentPosterID = (row[18]) != null ? new Guid(Convert.ToString(row[18])) : Guid.Empty,
                                        RecentTopicPostCount = (row[19]) != null ? Convert.ToInt32(row[19]) : 0
                                    });
                }
            }

            if (checkNew)
            {
                if (threads.Count > 0)
                {
                    var minVisitDate = ThreadVisitInfo.GetMinVisitThreadDate();
                    List<object[]> threadTopics;

                    if (minVisitDate == DateTime.MinValue)
                        threadTopics = DbManager.ExecuteList(new SqlQuery("forum_topic t1").Select("t1.thread_id", "t1.id", "t1.recent_post_id", "t2.create_date")
                                                                .LeftOuterJoin("forum_post t2", Exp.EqColumns("t1.recent_post_id", "t2.id") & Exp.Eq("t2.TenantID", threads[0].TenantID))
                                                                .Where(Exp.Eq("t1.TenantID", threads[0].TenantID)
                                                                       & Exp.In("t1.thread_id", threads.ConvertAll(t => t.ID)))
                                                                 );
                    else
                        threadTopics = DbManager.ExecuteList(new SqlQuery("forum_topic t1").Select("t1.thread_id", "t1.id", "t1.recent_post_id", "t2.create_date")
                                                                .InnerJoin("forum_post t2", Exp.EqColumns("t1.recent_post_id", "t2.id") & Exp.Ge("t2.create_date", minVisitDate))
                                                                .Where(Exp.Eq("t1.TenantID", threads[0].TenantID)
                                                                       & Exp.Eq("t2.TenantID", threads[0].TenantID)
                                                                       & Exp.In("t1.thread_id", threads.ConvertAll(t => t.ID))
                                                                 ));


                    foreach (var row in threadTopics)
                    {
                        var tid = Convert.ToInt32(row[0]);

                        var thr = threads.Find(t => t.ID == tid);
                        if (thr != null)
                        {
                            thr.TopicLastUpdates.Add(new Thread.TopicLastUpdate
                                                         {
                                                             TopicID = Convert.ToInt32(row[1]),
                                                             RecentPostID = Convert.ToInt32(row[2]),
                                                             RecentPostCreateDate = (row[3]) != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])) : DateTime.MinValue
                                                         });
                        }
                    }
                }
            }
        }

        public static ThreadCategory GetCategoryByID(int tenantID, int categoryID, out List<Thread> threads)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_category t1")
                                                 .Select("t1.id", "t1.TenantID", "t1.title", "t1.description", "t1.sort_order", "t1.create_date", "t1.poster_id",
                                                         "t2.id", "t2.title, t2.description", "t2.sort_order", "t2.topic_count", "t2.post_count", "t2.recent_post_id", "t2.recent_topic_id", "t2.is_approved",
                                                         "t3.title", "t2.recent_post_date", "t2.recent_poster_id", "t3.post_count")
                                                 .LeftOuterJoin("forum_thread t2", Exp.EqColumns("t1.id", "t2.category_id") & Exp.Eq("t2.TenantID", tenantID))
                                                 .LeftOuterJoin("forum_topic t3", Exp.EqColumns("t2.recent_topic_id", "t3.id") & Exp.Eq("t3.TenantID", tenantID))
                                                 .Where(Exp.Eq("t1.TenantID", tenantID) & Exp.Eq("t1.id", categoryID))
                                                 .OrderBy("t1.sort_order", true).OrderBy("t2.sort_order", true));

            List<ThreadCategory> categories;
            ParseThreadCategories(data, false, out categories, out threads);

            return categories.Count > 0 ? categories[0] : null;
        }

        public static void GetThreadCategories(int tenantID, out List<ThreadCategory> categories, out List<Thread> threads)
        {
            GetThreadCategories(tenantID, false, out categories, out threads);
        }

        public static void GetThreadCategories(int tenantID, bool checkNew, out List<ThreadCategory> categories, out List<Thread> threads)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_category t1").Select("t1.id", "t1.TenantID", "t1.title", "t1.description", "t1.sort_order", "t1.create_date", "t1.poster_id",
                                                                                      "t2.id", "t2.title, t2.description", "t2.sort_order", "t2.topic_count", "t2.post_count", "t2.recent_post_id", "t2.recent_topic_id", "t2.is_approved",
                                                                                      "t3.title", "t2.recent_post_date", "t2.recent_poster_id", "t3.post_count")
                                                 .LeftOuterJoin("forum_thread t2", Exp.EqColumns("t1.id", "t2.category_id") & Exp.Eq("t2.TenantID", tenantID))
                                                 .LeftOuterJoin("forum_topic t3", Exp.EqColumns("t2.recent_topic_id", "t3.id") & Exp.Eq("t3.TenantID", tenantID))
                                                 .Where(Exp.Eq("t1.TenantID", tenantID))
                                                 .OrderBy("t1.sort_order", true).OrderBy("t2.sort_order", true));

            ParseThreadCategories(data, checkNew, out categories, out threads);
        }

        public static Thread GetThreadByID(int tenantID, int threadID)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_thread t2").Select(
                                        "t2.id", "t2.category_id", "t2.title, t2.description", "t2.sort_order", "t2.topic_count", "t2.post_count", "t2.recent_post_id", "t2.recent_topic_id", "t2.is_approved",
                                        "t3.title", "t2.recent_post_date", "t2.recent_poster_id", "t3.post_count")
                                        .LeftOuterJoin("forum_topic t3", Exp.EqColumns("t2.recent_topic_id", "t3.id") & Exp.Eq("t3.TenantID", tenantID))
                                        .Where(Exp.Eq("t2.TenantID", tenantID) & Exp.Eq("t2.id", threadID)));

            if (data.Count == 0)
                return null;

            var row = data[0];
            return new Thread
                       {

                           ID = threadID,
                           CategoryID = Convert.ToInt32(row[1]),
                           TenantID = tenantID,
                           Title = Convert.ToString(row[2]),
                           Description = (row[3] != null ? Convert.ToString(row[3]) : ""),
                           SortOrder = Convert.ToInt32(row[4]),
                           TopicCount = Convert.ToInt32(row[5]),
                           PostCount = Convert.ToInt32(row[6]),
                           RecentPostID = Convert.ToInt32(row[7]),
                           RecentTopicID = Convert.ToInt32(row[8]),
                           IsApproved = Convert.ToBoolean(row[9]),
                           RecentTopicTitle = (row[10] != null ? Convert.ToString(row[10]) : ""),
                           RecentPostCreateDate = (row[11]) != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[11])) : DateTime.MinValue,
                           RecentPosterID = (row[12]) != null ? new Guid(Convert.ToString(row[12])) : Guid.Empty,
                           RecentTopicPostCount = (row[13] != null ? Convert.ToInt32(row[13]) : 0)
                       };
        }

        public static int CreateThreadCategory(int tenantID, string title, string description, int sortOrder)
        {
            return DbManager.ExecuteScalar<int>(new SqlInsert("forum_category")
                                                     .InColumnValue("id", 0)
                                                     .InColumnValue("TenantID", tenantID)
                                                     .InColumnValue("title", title)
                                                     .InColumnValue("description", description)
                                                     .InColumnValue("create_date", DateTime.UtcNow)
                                                     .InColumnValue("poster_id", SecurityContext.CurrentAccount.ID)
                                                     .InColumnValue("sort_order", sortOrder)
                                                     .Identity(0, 0, true));
        }

        public static void UpdateThreadCategory(int tenantID, int threadCategoryID, string title, string description, int sortOrder)
        {
            DbManager.ExecuteNonQuery(new SqlUpdate("forum_category").Set("title", title)
                                        .Set("description", description)
                                        .Set("sort_order", sortOrder)
                                        .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", threadCategoryID)));
        }

        public static int CreateThread(int tenantID, int categoryID, string title, string description, int sortOrder)
        {
            return DbManager.ExecuteScalar<int>(new SqlInsert("forum_thread")
                                                     .InColumnValue("id", 0)
                                                     .InColumnValue("TenantID", tenantID)
                                                     .InColumnValue("category_id", categoryID)
                                                     .InColumnValue("title", title)
                                                     .InColumnValue("description", description)
                                                     .InColumnValue("is_approved", 1)
                                                     .InColumnValue("sort_order", sortOrder)
                                                     .Identity(0, 0, true));
        }

        public static void UpdateThread(int tenantID, int threadID, int categoryID, string title, string description, int sortOrder)
        {
            DbManager.ExecuteNonQuery(new SqlUpdate("forum_thread").Set("title", title)
                                        .Set("category_id", categoryID)
                                        .Set("description", description)
                                        .Set("sort_order", sortOrder)
                                        .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", threadID)));
        }

        public static void RemoveThreadCategory(int tenantID, int threadCategoryID, out List<int> removedPostIDs)
        {
            removedPostIDs = new List<int>();
            List<int> threadIDs;
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                threadIDs = DbManager.Connection.CreateCommand("select id from forum_thread where TenantID = @tid and category_id = @cid")
                    .AddParameter("tid", tenantID).AddParameter("cid", threadCategoryID)
                    .ExecuteList().ConvertAll(row => Convert.ToInt32(row[0]));

                foreach (var threadID in threadIDs)
                {
                    List<int> topicIDs;
                    List<int> postIds;
                    ExeRemoveThreadCommand(tenantID, threadID, out topicIDs, out postIds);
                    removedPostIDs.AddRange(postIds);
                }

                DbManager.Connection.CreateCommand("delete from forum_category where id = @cid and TenantID = @tid")
                    .AddParameter("tid", tenantID).AddParameter("cid", threadCategoryID).ExecuteNonQuery();

                tr.Commit();
            }
            threadIDs.ForEach(SubscriberPresenter.UnsubscribeAllOnThread);

            AscCache.Default.Remove("communityScreen" + TenantProvider.CurrentTenantID);
        }

        public static void RemoveThread(int tenantID, int threadID, out List<int> removedPostIDs)
        {
            List<int> topicIDs;
            var thread = GetThreadByID(tenantID, threadID);

            using (var tr = DbManager.Connection.BeginTransaction())
            {
                ExeRemoveThreadCommand(tenantID, threadID, out topicIDs, out removedPostIDs);
                tr.Commit();
            }
            SubscriberPresenter.UnsubscribeAllOnThread(threadID);
            topicIDs.ForEach(SubscriberPresenter.UnsubscribeAllOnTopic);

            if(GetThreadCountInCategory(tenantID, thread.CategoryID) == 0)
                RemoveThreadCategory(tenantID, thread.CategoryID, out removedPostIDs);

        }

        private static void ExeRemoveThreadCommand(int tenantID, int threadID, out List<int> removedTopicIDs, out List<int> removedPostIDs)
        {
            removedPostIDs = new List<int>();
            DbManager.Connection.CreateCommand(@"delete from forum_attachment
                                                where TenantID = @tid and 
                                                      post_id in (select id from forum_post
                                                                  where TenantID = @tid and topic_id in (select id from forum_topic where TenantID = @tid and thread_id = @threadID))")
                                                 .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();


            removedPostIDs = DbManager.Connection.CreateCommand(@"select id from forum_post
                                    where TenantID = @tid and topic_id in (select id from forum_topic where TenantID = @tid and thread_id = @threadID)")
                                     .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteList()
                                     .ConvertAll(obj => Convert.ToInt32(obj[0]));


            DbManager.Connection.CreateCommand(@"delete from forum_post
                                    where TenantID = @tid and topic_id in (select id from forum_topic where TenantID = @tid and thread_id = @threadID)")
                                     .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();

            DbManager.Connection.CreateCommand(@"delete from forum_topic_tag
                                         where topic_id in (select id from forum_topic where TenantID = @tid and thread_id = @threadID)")
                                                 .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();

            DbManager.Connection.CreateCommand(@"delete from forum_tag
                                         where TenantID = @tid and id not in (select tag_id from forum_topic_tag)")
                                                 .AddParameter("tid", tenantID).ExecuteNonQuery();

            var q = new SqlQuery("forum_question")
                .Select("id")
                .Where("tenantid", tenantID)
                .Where(Exp.In("topic_id", new SqlQuery("forum_topic").Select("id").Where("TenantID", tenantID).Where("thread_id", threadID)));
            var questionIds = DbManager.ExecuteList(q).ConvertAll(r => Convert.ToInt32(r[0]));

            DbManager.ExecuteNonQuery(new SqlDelete("forum_question").Where(Exp.In("id", questionIds)));
            DbManager.ExecuteNonQuery(new SqlDelete("forum_variant").Where(Exp.In("question_id", questionIds)));
            DbManager.ExecuteNonQuery(new SqlDelete("forum_answer_variant").Where(Exp.In("answer_id", new SqlQuery("forum_answer").Select("id").Where(Exp.In("question_id", questionIds)))));
            DbManager.ExecuteNonQuery(new SqlDelete("forum_answer").Where(Exp.In("question_id", questionIds)));

            removedTopicIDs = DbManager.Connection.CreateCommand(@"select id from forum_topic where TenantID = @tid and thread_id = @threadID")
                                    .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteList()
                                     .ConvertAll(obj => Convert.ToInt32(obj[0]));

            DbManager.Connection.CreateCommand(@"delete from forum_topic where TenantID = @tid and thread_id = @threadID")
                                    .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();

            DbManager.Connection.CreateCommand(@"delete from forum_lastvisit where tenantid = @tid and thread_id = @threadID")
                                    .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();

            DbManager.Connection.CreateCommand(@"delete from forum_thread where id=@threadID and TenantID = @tid")
                    .AddParameter("tid", tenantID).AddParameter("threadID", threadID).ExecuteNonQuery();
        }

        public static int GetThreadCategoriesCount(int tenantID)
        {
            return DbManager.ExecuteScalar<int>(new SqlQuery("forum_category").SelectCount().Where("TenantID", tenantID));
        }

        public static int GetThreadCountInCategory(int tenantID, int categoryID)
        {
            return DbManager.ExecuteScalar<int>(new SqlQuery("forum_thread").SelectCount().Where("TenantID", tenantID).Where("category_id", categoryID));
        }

        #endregion

        #region Poll

        public static bool IsUserVote(int tenantID, int pollID, Guid userID)
        {
            if (CommunitySecurity.IsOutsider()) return true;

            var id = DbManager.ExecuteScalar<int>(new SqlQuery("forum_answer").Select("id")
                                            .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("user_id", userID) & Exp.Eq("question_id", pollID)));

            return id > 0;
        }

        public static int CreatePoll(int tenantID, int topicID, QuestionType type, string name, List<string> answerVariants)
        {
            int pollID;
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                pollID = DbManager.ExecuteScalar<int>(new SqlInsert("forum_question")
                                                  .InColumnValue("id", 0)
                                                  .InColumnValue("TenantID", tenantID)
                                                  .InColumnValue("topic_id", topicID)
                                                  .InColumnValue("create_date", DateTime.UtcNow)
                                                  .InColumnValue("name", name)
                                                  .InColumnValue("type", (int)type)
                                                  .Identity(0, 0, true));


                int i = 0;
                foreach (var variant in answerVariants)
                {
                    DbManager.ExecuteNonQuery(new SqlInsert("forum_variant")
                                                .InColumnValue("name", variant)
                                                .InColumnValue("question_id", pollID)
                                                .InColumnValue("sort_order", i));
                    i++;
                }

                DbManager.ExecuteNonQuery(new SqlUpdate("forum_topic").Set("question_id", pollID)
                                            .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", topicID)));

                tr.Commit();
            }

            return pollID;
        }

        public static void PollVote(int tenantID, int pollID, List<int> variantIDs)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                var answerID = DbManager.ExecuteScalar<int>(new SqlInsert("forum_answer")
                                                  .InColumnValue("id", 0)
                                                  .InColumnValue("TenantID", tenantID)
                                                  .InColumnValue("user_id", SecurityContext.CurrentAccount.ID)
                                                  .InColumnValue("create_date", DateTime.UtcNow)
                                                  .InColumnValue("question_id", pollID)
                                                  .Identity(0, 0, true));

                foreach (var vid in variantIDs)
                {
                    DbManager.ExecuteNonQuery(new SqlInsert("forum_answer_variant").InColumns("answer_id", "variant_id")
                                                            .Values(answerID, vid));
                }

                tr.Commit();
            }
        }

        public static Question GetPollByID(int tenantID, int pollID)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_question t1").Select("t1.id", "t1.topic_id", "t1.type", "t1.name", "t1.create_date",
                                                                                      "t2.id", "t2.name", "t2.sort_order", "sum(case when t4.variant_id is null then 0 else 1 end)")
                                                 .LeftOuterJoin("forum_variant t2", Exp.EqColumns("t1.id", "t2.question_id"))
                                                 .LeftOuterJoin("forum_answer t3", Exp.EqColumns("t3.question_id", "t1.id") & Exp.Eq("t3.TenantID", tenantID))
                                                 .LeftOuterJoin("forum_answer_variant t4", Exp.EqColumns("t4.answer_id", "t3.id") & Exp.EqColumns("t4.variant_id", "t2.id"))
                                                 .Where(Exp.Eq("t1.id", pollID) & Exp.Eq("t1.TenantID", tenantID))
                                                 .GroupBy("t1.id", "t1.topic_id", "t1.type", "t1.name", "t1.create_date", "t2.id", "t2.name", "t2.sort_order", "(case when t4.variant_id is null then t2.id else t4.variant_id end)")
                                                 .OrderBy("t2.sort_order", true)
                );

            Question q = null;
            foreach (var row in data)
            {
                if (q == null)
                {
                    q = new Question
                            {
                                ID = Convert.ToInt32(row[0]),
                                TopicID = Convert.ToInt32(row[1]),
                                Type = (QuestionType) Convert.ToInt32(row[2]),
                                Name = Convert.ToString(row[3]),
                                CreateDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[4])),
                                TenantID = tenantID

                            };
                }

                if (row[5] != null)
                {
                    q.AnswerVariants.Add(new AnswerVariant
                                             {
                                                 ID = Convert.ToInt32(row[5]),
                                                 Name = Convert.ToString(row[6]),
                                                 SortOrder = Convert.ToInt32(row[7]),
                                                 QuestionID = q.ID,
                                                 AnswerCount = Convert.ToInt32(row[8])
                                             });
                }
            }

            return q;
        }

        public static void UpdatePoll(int tenantID, int pollID, QuestionType type, string name, List<AnswerVariant> answerVariants)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlUpdate("forum_question")
                    .Set("type", (int)type).Set("name", name).Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", pollID)));

                var vids = DbManager.ExecuteList(new SqlQuery("forum_question t1").Select("t2.id")
                                        .InnerJoin("forum_variant t2", Exp.EqColumns("t1.id", "t2.question_id"))
                                        .Where(Exp.Eq("t1.id", pollID) & Exp.Eq("t1.TenantID", tenantID)))
                                        .ConvertAll(row => Convert.ToInt32(row[0]));

                vids.RemoveAll(vid => answerVariants.Find(av => av.ID == vid) != null);

                DbManager.ExecuteNonQuery(new SqlDelete("forum_variant").Where(Exp.Eq("question_id", pollID) & Exp.In("id", vids)));

                foreach (var av in answerVariants)
                {
                    if (av.ID == 0)
                        DbManager.ExecuteNonQuery(new SqlInsert("forum_variant")
                                                .InColumnValue("name", av.Name)
                                                .InColumnValue("question_id", pollID)
                                                .InColumnValue("sort_order", av.SortOrder));

                    else
                        DbManager.ExecuteNonQuery(new SqlUpdate("forum_variant")
                                                        .Set("name", av.Name).Set("sort_order", av.SortOrder)
                                                        .Where("id", av.ID));
                }

                tr.Commit();
            }
        }

        #endregion

        #region Topics

        public static List<Topic> SearchTopicsByText(int tenantID, string text, int curPageNumber, int topicOnPageCount, out int topicCount)
        {
            List<int> topicIDs, tIDs, pIDs = null;
            if (FactoryIndexer<TopicWrapper>.TrySelectIds(r => r.MatchAll(text), out tIDs) || FactoryIndexer<PostWrapper>.TrySelectIds(r => r.MatchAll(text), out pIDs))
            {
                topicIDs = tIDs;
                if (pIDs != null)
                {
                    topicIDs.AddRange(pIDs);
                }
            }
            else
            {
                var searchStringList = ConvertStringToArray(text);
                if (searchStringList == null || searchStringList.Count == 0)
                {
                    topicCount = 0;
                    return new List<Topic>();
                }

                var wordsCount = searchStringList.Count;
                var query = new StringBuilder();

                query.Append(@"
select id from
(
	select id, GROUP_CONCAT(word) as words, TenantID from (
");

                int j = 0;
                for (; j < wordsCount; j++)
                {
                    query.AppendFormat(@"
		select id, @searchWord{0} as word, TenantID
		from forum_topic as ft
		where tenantid = @tenant and (lower(ft.title) like @searchWord{0} or lower(ft.title) like @searchWordWithSpace{0} or lower(ft.title) like @searchWordWithNbsp{0})
		
		union all

		select topic_id as id, @searchWord{0} as word, TenantID
		from forum_post as fp
		where tenantid = @tenant and (lower(fp.text) like @searchWord{0} or lower(fp.text) like @searchWordWithSpace{0} or lower(fp.text) like @searchWordWithNbsp{0})

		union all

		select ftt.topic_id as id, @searchWord{0} as word, ftag.TenantID as TenantID
		from forum_tag as ftag

		inner join forum_topic_tag as ftt
		on ftag.id = ftt.tag_id

		where tenantid = @tenant and (lower(ftag.name) like @searchWord{0} or lower(ftag.name) like @searchWordWithSpace{0} or lower(ftag.name) like @searchWordWithNbsp{0})", j);


                    if (j < wordsCount - 1)
                    {
                        query.Append(@"
		union all
");
                    }
                }

                query.Append(@"
	) as topics
	group by id, TenantID

) as topicSearch
where ");


                j = 0;
                for (; j < wordsCount; j++)
                {
                    query.AppendFormat(" words like @searchWordWithPercent{0} ", j);
                    if (j < wordsCount - 1)
                    {
                        query.Append(" and ");
                    }
                }

                var topicIdsQuery = DbManager.Connection.CreateCommand(query.ToString()).AddParameter("Tenant", tenantID);

                j = 0;
                foreach (var t in searchStringList)
                {
                    topicIdsQuery.AddParameter(string.Format("searchWord{0}", j), string.Format("{0}%", t.ToLower()));
                    topicIdsQuery.AddParameter(string.Format("searchWordWithPercent{0}", j), string.Format("%{0}%", t.ToLower()));
                    topicIdsQuery.AddParameter(string.Format("searchWordWithSpace{0}", j), string.Format("% {0}%", t.ToLower()));
                    topicIdsQuery.AddParameter(string.Format("searchWordWithNbsp{0}", j), string.Format("%nbsp;{0}%", t.ToLower()));

                    j++;
                }
                topicIDs = topicIdsQuery.ExecuteList().ConvertAll(o => Convert.ToInt32(o[0]));
            }

            topicCount = topicIDs.Count;

            var necessaryTopicIDs = new List<int>();
            if (topicOnPageCount == -1)
                necessaryTopicIDs = topicIDs;
            else
            {
                for (int i = (curPageNumber - 1) * topicOnPageCount; i < curPageNumber * topicOnPageCount && i < topicIDs.Count; i++)
                    necessaryTopicIDs.Add(topicIDs[i]);
            }

            return GetTopicsByIDs(tenantID, necessaryTopicIDs, true);
        }

        private static IList<string> ConvertStringToArray(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            var searchString = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            const char separator = ' ';
            var searchStringList = new List<string>(searchString.Split(separator));

            for (int k = 0; k < searchStringList.Count; k++)
            {
                if (searchStringList[k].Length < 3)
                {
                    searchStringList.RemoveAt(k);
                    k--;
                }
            }

            return searchStringList;
        }

        public static List<Topic> SearchTopicsByTag(int tenantID, int tagID, int curPageNumber, int topicOnPageCount, out int topicCount)
        {
            var topicIDs = DbManager.Connection.CreateCommand(@"select id from forum_topic t1
                                                            join forum_topic_tag t2
                                                            on t1.id = t2.topic_id and t2.tag_id = @tagID and t1.TenantID = @tid   
                                                            order by t1.sticky desc, t1.recent_post_id desc")
                              .AddParameter("tid", tenantID)
                              .AddParameter("tagID", tagID)
                              .ExecuteList().ConvertAll(o => Convert.ToInt32(o[0]));

            topicCount = topicIDs.Count;

            var necessaryTopicIDs = new List<int>();
            for (int i = (curPageNumber - 1) * topicOnPageCount; i < curPageNumber * topicOnPageCount && i < topicIDs.Count; i++)
                necessaryTopicIDs.Add(topicIDs[i]);

            return GetTopicsByIDs(tenantID, necessaryTopicIDs, true);
        }

        public static List<Topic> SearchTopicsByUser(int tenantID, Guid userID, int curPageNumber, int topicOnPageCount, out int topicCount)
        {
            var topicIDs = DbManager.Connection.CreateCommand(@"select distinct(topic_id) id from forum_post t2
                                                                    join forum_topic t1
                                                                    on t1.id = t2.topic_id and  t2.poster_id=@userID
                                                                    where t1.TenantID = @tid and t2.TenantID = @tid
                                                                    order by t1.recent_post_id desc, t1.sticky desc")
                             .AddParameter("tid", tenantID)
                             .AddParameter("userID", userID)
                             .ExecuteList().ConvertAll(o => Convert.ToInt32(o[0]));

            topicCount = topicIDs.Count;

            var necessaryTopicIDs = new List<int>();
            for (var i = (curPageNumber - 1) * topicOnPageCount; i < curPageNumber * topicOnPageCount && i < topicIDs.Count; i++)
                necessaryTopicIDs.Add(topicIDs[i]);

            return GetTopicsByIDs(tenantID, necessaryTopicIDs, true);
        }

        public static List<Topic> GetLastUpdateTopics(int tenantID, int maxTopicCount)
        {
            return GetLastUpdateTopics(tenantID, 0, maxTopicCount);
        }

        public static List<Topic> GetLastUpdateTopics(int tenantID, int from, int maxTopicCount)
        {

            var ids = DbManager.ExecuteList(new SqlQuery("forum_topic").Select("id")
                                                    .Where("TenantID", tenantID)
                                                    .OrderBy("recent_post_id", false).SetFirstResult(from).SetMaxResults(maxTopicCount))
                                                    .ConvertAll(row => Convert.ToInt32(row[0]));

            var topics = GetTopicsByIDs(tenantID, ids, false);
            topics.Sort((t1, t2) => Comparer<int>.Default.Compare(t2.RecentPostID, t1.RecentPostID));
            return topics;

        }

        public static List<int> GetTopicIDs(int tenantID, int threadID)
        {
            return DbManager.Connection.CreateCommand(@"select id from forum_topic
                                                 where thread_id = @threadID and TenantID = @tid
                                                 order by sticky desc, recent_post_id desc")
                                .AddParameter("tid", tenantID)
                                .AddParameter("threadID", threadID)
                                .ExecuteList().ConvertAll(o => Convert.ToInt32(o[0]));
        }

        public static List<Topic> GetTopics(int tenantID, Guid userID, int threadID, int curPageNumber, int topicOnPageCount, out int topicCountInThread)
        {
            var topicIDs = GetTopicIDs(tenantID, threadID);
            topicCountInThread = topicIDs.Count;

            var necessaryTopicIDs = new List<int>();
            for (int i = (curPageNumber - 1) * topicOnPageCount; i < curPageNumber * topicOnPageCount && i < topicIDs.Count; i++)
                necessaryTopicIDs.Add(topicIDs[i]);

            return GetTopicsByIDs(tenantID, necessaryTopicIDs, true);

        }

        public static Topic GetTopicByID(int tenantID, int topicID)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_topic t1").Select("t1.id", "t1.thread_id", "t1.title", "t1.create_date", "t1.view_count",
                                                "t1.post_count", "t1.recent_post_id", "t1.is_approved", "t1.poster_id", "t1.sticky",
                                                "t1.closed", "t1.type", "t1.question_id", "t2.text", "t2.poster_id", "t2.create_date", "t2.formatter", "t3.title")
                                                .LeftOuterJoin("forum_post t2", Exp.EqColumns("t2.id", "t1.recent_post_id") & Exp.Eq("t2.TenantID", tenantID))
                                                .LeftOuterJoin("forum_thread t3", Exp.EqColumns("t1.thread_id", "t3.id") & Exp.Eq("t3.TenantID", tenantID))
                                                .Where(Exp.Eq("t1.id", topicID) & Exp.Eq("t1.TenantID", tenantID)));

            if (data.Count == 0)
                return null;

            return GetTagsForTopics(tenantID, ParseTopic(data[0], tenantID));
        }

        public static List<Topic> GetTopicsByIDs(int tenantID, List<int> necessaryTopicIDs, bool withTags)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_topic t1").Select("t1.id", "t1.thread_id", "t1.title", "t1.create_date", "t1.view_count",
                                                                                   "t1.post_count", "t1.recent_post_id", "t1.is_approved", "t1.poster_id", "t1.sticky",
                                                                                   "t1.closed", "t1.type", "t1.question_id", "t2.text", "t2.poster_id", "t2.create_date", "t2.formatter", "t3.title")
                                                 .LeftOuterJoin("forum_post t2", Exp.EqColumns("t2.id", "t1.recent_post_id") & Exp.Eq("t2.TenantID", tenantID))
                                                 .LeftOuterJoin("forum_thread t3", Exp.EqColumns("t1.thread_id", "t3.id") & Exp.Eq("t3.TenantID", tenantID))
                                                 .Where("t1.tenantid", tenantID)
                                                 .Where(Exp.In("t1.id", necessaryTopicIDs))
                                                 .OrderBy("t1.sticky", false).OrderBy("t1.recent_post_id", false));

            var topics = data.Select(row => ParseTopic(row, tenantID)).ToList();

            return withTags ? GetTagsForTopics(tenantID, topics) : topics;
        }

        public static List<Tag> GetAllTags(int tenantID)
        {
            var data = DbManager.Connection.CreateCommand(@"select t1.id, t1.name, t1.is_approved
                                                from forum_tag t1 
                                                where t1.TenantID= @tid
                                                order by name")
                                                .AddParameter("tid", tenantID).ExecuteList();

            return data.Select(row => new Tag
                                          {
                                              TenantID = tenantID,
                                              ID = Convert.ToInt32(row[0]),
                                              Name = Convert.ToString(row[1]),
                                              IsApproved = Convert.ToBoolean(row[2]),
                                          }).ToList();
        }

        public static List<RankTag> GetTagCloud(int tenantID, int limitTagCount)
        {
            var data = DbManager.Connection.CreateCommand(@"select t1.id, t1.name, t1.is_approved, sum(t3.post_count) rank from forum_tag t1                                                
                                                inner join forum_topic_tag t2 on t1.id = t2.tag_id
                                                inner join forum_topic t3 on t3.id = t2.topic_id
                                                where t1.TenantID = @tid and t3.TenantID = @tid 
                                                group by t1.id, t1.name, t1.is_approved
                                                order by rank desc limit " + limitTagCount.ToString())
                                                .AddParameter("tid", tenantID).ExecuteList();

            var tags = new List<RankTag>();
            foreach (var row in data)
            {
                tags.Add(new RankTag
                             {
                                 TenantID = tenantID,
                                 ID = Convert.ToInt32(row[0]),
                                 Name = Convert.ToString(row[1]),
                                 IsApproved = Convert.ToBoolean(row[2]),
                                 Rank = Convert.ToInt32(row[3])
                             });
            }

            tags.Sort((t1, t2) => String.Compare(t1.Name, t2.Name));
            return tags;
        }

        private static Topic GetTagsForTopics(int tenantID, Topic topic)
        {
            var topics = new List<Topic> {topic};
            return GetTagsForTopics(tenantID, topics)[0];
        }



        private static List<Topic> GetTagsForTopics(int tenantID, List<Topic> topics)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_tag t1").Select("t2.topic_id", "t1.id", "t1.name", "t1.is_approved")
                                               .InnerJoin("forum_topic_tag t2", Exp.EqColumns("t1.id", "t2.tag_id"))
                                               .Where(Exp.In("t2.topic_id", topics.ConvertAll(t => t.ID)) & Exp.Eq("t1.TenantID", tenantID)));

            new List<Tag>();
            foreach (var row in data)
            {
                var topicID = Convert.ToInt32(row[0]);
                var topic = topics.Find(t => t.ID == topicID);
                topic.Tags.Add(new Tag
                                   {
                                       TenantID = tenantID,
                                       ID = Convert.ToInt32(row[1]),
                                       Name = Convert.ToString(row[2]),
                                       IsApproved = Convert.ToBoolean(row[3])
                                   });
            }

            return topics;
        }

        private static Topic ParseTopic(object[] row, int tenantID)
        {
            return new Topic
                       {
                           ID = Convert.ToInt32(row[0]),
                           TenantID = tenantID,
                           ThreadID = Convert.ToInt32(row[1]),
                           Title = Convert.ToString(row[2]),
                           CreateDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])),
                           ViewCount = Convert.ToInt32(row[4]),
                           PostCount = Convert.ToInt32(row[5]),
                           RecentPostID = (row[6] != null ? Convert.ToInt32(row[6]) : 0),
                           IsApproved = Convert.ToBoolean(row[7]),
                           PosterID = new Guid(Convert.ToString(row[8])),
                           Sticky = Convert.ToBoolean(row[9]),
                           Closed = Convert.ToBoolean(row[10]),
                           Type = (TopicType) Convert.ToInt32(row[11]),
                           QuestionID = Convert.ToInt32(row[12]),
                           RecentPostText = (row[13] != null ? Convert.ToString(row[13]) : ""),
                           RecentPostAuthorID = (row[14] != null ? new Guid(Convert.ToString(row[14])) : Guid.Empty),
                           RecentPostCreateDate = (row[15] != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[15])) : DateTime.MinValue),
                           RecentPostFormatter = (row[16] != null ? (PostTextFormatter) Convert.ToInt32(row[16]) : PostTextFormatter.FCKEditor),
                           ThreadTitle = Convert.ToString(row[17])
                       };
        }

        public static void ApproveTopic(int tenantID, int topicID)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                DbManager.Connection.CreateCommand("update forum_post set is_approved = 1 where topic_id = @topicID and TenantID = @tid")
                    .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteNonQuery();

                DbManager.Connection.CreateCommand("update forum_topic set is_approved = 1 where id = @topicID and TenantID = @tid")
                    .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteNonQuery();

                var threadID = DbManager.Connection.CreateCommand("select thread_id from forum_topic where id = @topicID and TenantID = @tid")
                   .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteScalar<int>();

                DbManager.Connection.CreateCommand(@"update forum_thread
                                                      set is_approved = (select coalesce(min(is_approved),1) from forum_topic where thread_id = @threadID and forum_topic.TenantID = @tid)
                                                    where id = @threadID and TenantID = @tid")
                                                .AddParameter("threadID", threadID).AddParameter("tid", tenantID).ExecuteNonQuery();

                tr.Commit();
            }
        }

        public static void UpdateTopic(int tenantID, int topicID, string title, bool sticky, bool closed)
        {
            DbManager.Connection.CreateCommand(@"update forum_topic 
                                                      set title = @title,
                                                          sticky = @sticky,
                                                          closed = @closed
                                                where id = @topicID and TenantID = @tid")
                                .AddParameter("title", title)
                                .AddParameter("sticky", sticky ? 1 : 0)
                                .AddParameter("closed", closed ? 1 : 0)
                                .AddParameter("topicID", topicID)
                                .AddParameter("tid", tenantID).ExecuteNonQuery();
        }

        public static int CreateTopic(int tenantID, int threadID, string title, TopicType type)
        {
            int topicID;
            using (var tr = DbManager.Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                topicID = DbManager.ExecuteScalar<int>(new SqlInsert("forum_topic")
                                                    .InColumnValue("id", 0)
                                                    .InColumnValue("TenantID", tenantID)
                                                    .InColumnValue("thread_id", threadID)
                                                    .InColumnValue("title", title)
                                                    .InColumnValue("type", (int)type)
                                                    .InColumnValue("create_date", DateTime.UtcNow)
                                                    .InColumnValue("is_approved", 1)
                                                    .InColumnValue("poster_id", SecurityContext.CurrentAccount.ID)
                                                    .InColumnValue("sticky", 0)
                                                    .InColumnValue("closed", 0)
                                                    .Identity(0, 0, true));

                DbManager.Connection.CreateCommand(@"update forum_thread 
                                                        set topic_count = (select count(id) from forum_topic where TenantID= @tid and thread_id = @threadID),
                                                        recent_topic_id = @topicID
                                                      where id = @threadID and TenantID = @tid")
                                     .AddParameter("tid", tenantID)
                                     .AddParameter("topicID", topicID)
                                     .AddParameter("threadID", threadID)
                                     .ExecuteNonQuery();

                tr.Commit();
            }

            return topicID;
        }

        public static List<string> RemoveTopic(int tenantID, int topicID, out List<int> removedPostIDs)
        {
            removedPostIDs = new List<int>();

            List<string> attachmantOffsetPhysicalPaths;
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                attachmantOffsetPhysicalPaths = DbManager.ExecuteList(new SqlQuery("forum_attachment t1").Select("t1.path")
                                        .InnerJoin("forum_post t2", Exp.EqColumns("t1.post_id", "t2.id") & Exp.Eq("t2.topic_id", topicID)
                                                                   & Exp.Eq("t1.TenantID", tenantID) & Exp.Eq("t2.TenantID", tenantID))
                                        ).ConvertAll(row => Convert.ToString(row[0]));


                DbManager.Connection.CreateCommand(@"delete from forum_attachment
                                                where TenantID = @tid and 
                                                      post_id in (select id from forum_post
                                                                  where TenantID = @tid and topic_id = @topicID)")
                                                     .AddParameter("tid", tenantID).AddParameter("topicID", topicID).ExecuteNonQuery();

                removedPostIDs = DbManager.Connection.CreateCommand(@"select id from forum_post
                                    where TenantID = @tid and topic_id = @topicID")
                                         .AddParameter("tid", tenantID).AddParameter("topicID", topicID).ExecuteList()
                                         .ConvertAll(obj => Convert.ToInt32(obj[0]));


                DbManager.Connection.CreateCommand(@"delete from forum_post
                                    where TenantID = @tid and topic_id = @topicID")
                                         .AddParameter("tid", tenantID).AddParameter("topicID", topicID).ExecuteNonQuery();

                DbManager.Connection.CreateCommand(@"delete from forum_topic_tag
                                         where topic_id = @topicID")
                                                     .AddParameter("tid", tenantID).AddParameter("topicID", topicID).ExecuteNonQuery();

                DbManager.Connection.CreateCommand(@"delete from forum_tag
                                         where TenantID = @tid and id not in (select tag_id from forum_topic_tag)")
                                                     .AddParameter("tid", tenantID).ExecuteNonQuery();

                var q = new SqlQuery("forum_question").Select("id").Where("tenantid", tenantID).Where("topic_id", topicID);
                var questionIds = DbManager.ExecuteList(q).ConvertAll(r => Convert.ToInt32(r[0]));

                DbManager.ExecuteNonQuery(new SqlDelete("forum_question").Where(Exp.In("id", questionIds)));
                DbManager.ExecuteNonQuery(new SqlDelete("forum_variant").Where(Exp.In("question_id", questionIds)));
                DbManager.ExecuteNonQuery(new SqlDelete("forum_answer_variant").Where(Exp.In("answer_id", new SqlQuery("forum_answer a").Select("a.id").Where(Exp.Eq("a.tenantid", tenantID) & Exp.In("a.question_id", questionIds)))));
                DbManager.ExecuteNonQuery(new SqlDelete("forum_answer").Where(Exp.In("question_id", questionIds)));

                var threadID = DbManager.ExecuteScalar<int>(new SqlQuery("forum_topic").Select("thread_id")
                                            .Where(Exp.Eq("id", topicID) & Exp.Eq("TenantID", tenantID)));


                DbManager.Connection.CreateCommand(@"delete from forum_topic where TenantID = @tid and id = @topicID")
                                        .AddParameter("tid", tenantID).AddParameter("topicID", topicID).ExecuteNonQuery();


                var threadData = DbManager.Connection.CreateCommand(@"select sum(post_count), coalesce(min(is_approved),1), coalesce(max(recent_post_id),0), count(id)
                                                    from  forum_topic
                                                    where thread_id = @threadID and TenantID = @tid")
                                                  .AddParameter("threadID", threadID).AddParameter("tid", tenantID).ExecuteList();

                var postCount = Convert.ToInt32(threadData[0][0]);
                var recentPostID = (threadData[0][2] != null ? Convert.ToInt32(threadData[0][2]) : 0);
                var topicCount = Convert.ToInt32(threadData[0][3]);
                var isApproved = (topicCount == 0 || Convert.ToBoolean(threadData[0][1]));

                int recentTopicID = 0;
                DateTime recentPostDate = DateTime.MinValue;
                Guid recentPosterID = Guid.Empty;

                if (recentPostID != 0)
                {
                    var threadPostData = DbManager.Connection.CreateCommand(@"select topic_id, create_date, poster_id
                                                    from  forum_post
                                                    where id = @pid and TenantID = @tid")
                                                     .AddParameter("pid", recentPostID).AddParameter("tid", tenantID).ExecuteList();


                    recentTopicID = Convert.ToInt32(threadPostData[0][0]);
                    recentPostDate = Convert.ToDateTime(threadPostData[0][1]);
                    recentPosterID = threadPostData[0][2] != null ? new Guid(Convert.ToString(threadPostData[0][2])) : Guid.Empty;

                }

                DbManager.Connection.CreateCommand(@"update forum_thread
                                                        set post_count = @postCount,
                                                            topic_count = @topicCount,
                                                            is_approved = @isApproved,
                                                            recent_post_id = @recentPostID,
                                                            recent_topic_id = @recentTopicID,
                                                            recent_post_date =  @recentPostDate,
                                                            recent_topic_id = @recentTopicID,
                                                            recent_poster_id = @recentPosterID
                                                     where id = @threadID and TenantID = @tid")

                                     .AddParameter("threadID", threadID)
                                     .AddParameter("postCount", postCount)
                                     .AddParameter("topicCount", topicCount)
                                     .AddParameter("isApproved", isApproved)
                                     .AddParameter("recentPostID", recentPostID)
                                     .AddParameter("recentPostDate", recentPostDate)
                                     .AddParameter("recentTopicID", recentTopicID)
                                     .AddParameter("recentPosterID", recentPosterID)
                                     .AddParameter("tid", tenantID).ExecuteNonQuery();


                tr.Commit();
            }

            SubscriberPresenter.UnsubscribeAllOnTopic(topicID);

            return attachmantOffsetPhysicalPaths;
        }

        #endregion

        #region Posts

        public static int GetUserPostCount(int tenantID, Guid userID)
        {
            return DbManager.ExecuteScalar<int>(new SqlQuery("forum_post")
                                                    .SelectCount("id")
                                                    .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("poster_id", userID)));
        }

        public static List<Post> GetRecentTopicPosts(int tenantID, int topicID, int postCount, int replyPostID)
        {
            List<int> necessaryPostIDs;

            if (replyPostID != -1 && replyPostID != 0)
            {
                necessaryPostIDs = DbManager.ExecuteList(new SqlQuery("forum_post").Select("id")
                                                        .Where(Exp.Eq("topic_id", topicID) & Exp.Eq("TenantID", tenantID) & Exp.Le("id", replyPostID))
                                                        .OrderBy("id", false)
                                                        .SetMaxResults(postCount))
                                                        .ConvertAll(o => Convert.ToInt32(o[0]));
            }
            else
            {
                necessaryPostIDs = DbManager.ExecuteList(new SqlQuery("forum_post").Select("id")
                                                        .Where(Exp.Eq("topic_id", topicID) & Exp.Eq("TenantID", tenantID))
                                                        .OrderBy("id", false)
                                                        .SetMaxResults(postCount))
                                                        .ConvertAll(o => Convert.ToInt32(o[0]));
            }

            var data = DbManager.ExecuteList(new SqlQuery("forum_post t1").Select("t1.id", "t1.topic_id", "t1.create_date", "t1.poster_id",
                                               "t1.subject", "t1.text", "t1.edit_date", "t1.edit_count", "t1.is_approved", "t1.parent_post_id", "t1.formatter", "t1.editor_id",
                                               "t2.id", "t2.name", "t2.size", "t2.download_count", "t2.content_type", "t2.mime_content_type", "t2.create_date", "t2.path")
                                               .LeftOuterJoin("forum_attachment t2", Exp.EqColumns("t1.id", "t2.post_id") & Exp.Eq("t2.tenantid", tenantID))
                                               .Where(Exp.In("t1.id", necessaryPostIDs))
                                               .Where("t1.tenantid", tenantID)
                                               .OrderBy("t1.create_date", true));

            return ParsePost(data, tenantID);
        }



        public static List<int> GetPostIDs(int tenantID, int topicID)
        {
            return DbManager.Connection.CreateCommand(@"select id from forum_post
                                                 where topic_id = @topicID and TenantID = @tid
                                                 order by create_date asc")
                                .AddParameter("tid", tenantID)
                                .AddParameter("topicID", topicID)
                                .ExecuteList().ConvertAll(o => Convert.ToInt32(o[0]));
        }

        public static List<Post> GetPosts(int tenantID, int topicID, int curPageNumber, int postOnPageCount, out int postCountInTopic)
        {
            var postIDs = GetPostIDs(tenantID, topicID);
            postCountInTopic = postIDs.Count;

            var necessaryPostIDs = new List<int>();
            for (int i = (curPageNumber - 1) * postOnPageCount; i < curPageNumber * postOnPageCount && i < postIDs.Count; i++)
                necessaryPostIDs.Add(postIDs[i]);

            return GetPostsByIDs(tenantID, necessaryPostIDs);
        }

        public static List<Post> GetPostsByIDs(int tenantID, List<int> necessaryPostIDs)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_post t1").Select("t1.id", "t1.topic_id", "t1.create_date", "t1.poster_id",
                                                                                  "t1.subject", "t1.text", "t1.edit_date", "t1.edit_count", "t1.is_approved", "t1.parent_post_id", "t1.formatter", "t1.editor_id",
                                                                                  "t2.id", "t2.name", "t2.size", "t2.download_count", "t2.content_type", "t2.mime_content_type", "t2.create_date", "t2.path")
                                                 .LeftOuterJoin("forum_attachment t2", Exp.EqColumns("t1.id", "t2.post_id") & Exp.Eq("t2.tenantid", tenantID))
                                                 .Where(Exp.In("t1.id", necessaryPostIDs))
                                                 .Where("t1.tenantid", tenantID)
                                                 .OrderBy("t1.create_date", true));

            return ParsePost(data, tenantID);
        }

        public static Post GetPostByID(int tenantID, int postID)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_post t1").Select("t1.id", "t1.topic_id", "t1.create_date", "t1.poster_id",
                                                "t1.subject", "t1.text", "t1.edit_date", "t1.edit_count", "t1.is_approved", "t1.parent_post_id", "t1.formatter", "t1.editor_id",
                                                "t2.id", "t2.name", "t2.size", "t2.download_count", "t2.content_type", "t2.mime_content_type", "t2.create_date", "t2.path")
                                                .LeftOuterJoin("forum_attachment t2", Exp.EqColumns("t1.id", "t2.post_id") & Exp.Eq("t2.tenantid", tenantID))
                                                .Where(Exp.Eq("t1.id", postID) & Exp.Eq("t1.TenantID", tenantID)));

            var posts = ParsePost(data, tenantID);
            if (posts.Count == 0)
                return null;

            return posts[0];
        }

        private static List<Post> ParsePost(IEnumerable<object[]> data, int tenantID)
        {

            var posts = new List<Post>();
            foreach (var row in data)
            {
                var post = posts.Find(p => p.ID.Equals(Convert.ToInt32(row[0])));
                if (post == null)
                {
                    post = new Post
                               {
                                   TenantID = tenantID,
                                   ID = Convert.ToInt32(row[0]),
                                   TopicID = Convert.ToInt32(row[1]),
                                   CreateDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2])),
                                   PosterID = new Guid(Convert.ToString(row[3])),
                                   Subject = Convert.ToString(row[4]),
                                   Text = Convert.ToString(row[5]),
                                   EditDate = (row[6] != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[6])) : DateTime.MinValue),
                                   EditCount = Convert.ToInt32(row[7]),
                                   IsApproved = Convert.ToBoolean(row[8]),
                                   ParentPostID = (row[9] != null ? Convert.ToInt32(row[9]) : 0),
                                   Formatter = (PostTextFormatter) Convert.ToInt32(row[10]),
                                   EditorID = (row[11] != null ? new Guid(Convert.ToString(row[11])) : Guid.Empty)
                               };
                    posts.Add(post);
                }

                if (row[12] != null)
                {
                    post.Attachments.Add(new Attachment
                                             {
                                                 TenantID = tenantID,
                                                 ID = Convert.ToInt32(row[12]),
                                                 Name = Convert.ToString(row[13]),
                                                 Size = Convert.ToInt32(row[14]),
                                                 DownloadCount = Convert.ToInt32(row[15]),
                                                 ContentType = (AttachmentContentType) Convert.ToInt32(row[16]),
                                                 MIMEContentType = Convert.ToString(row[17]),
                                                 CreateDate = (row[18] != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[18])) : DateTime.MinValue),
                                                 PostID = post.ID,
                                                 OffsetPhysicalPath = Convert.ToString(row[19])
                                             });
                }

            }
            return posts;
        }

        public static int CreateAttachment(int tenantID, int postID, string name, string path, int size, AttachmentContentType contentType, string mimeContentType)
        {

            return DbManager.ExecuteScalar<int>(new SqlInsert("forum_attachment")
                                                    .InColumnValue("id", 0)
                                                    .InColumnValue("TenantID", tenantID)
                                                    .InColumnValue("post_id", postID)
                                                    .InColumnValue("name", name)
                                                    .InColumnValue("size", size)
                                                    .InColumnValue("download_count", 0)
                                                    .InColumnValue("content_type", (int)contentType)
                                                    .InColumnValue("mime_content_type", mimeContentType)
                                                    .InColumnValue("create_date", DateTime.UtcNow)
                                                    .InColumnValue("path", path)
                                                    .Identity(0, 0, true));
        }

        public static void RemoveAttachment(int tenantID, int attachmentID)
        {
            DbManager.Connection.CreateCommand("delete from forum_attachment where id = @id and TenantID = @tid")
                .AddParameter("id", attachmentID).AddParameter("tid", tenantID).ExecuteNonQuery();
        }

        public static void ApprovePost(int tenantID, int postID)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                DbManager.Connection.CreateCommand("update forum_post set is_approved = 1 where id = @postID and TenantID = @tid")
                    .AddParameter("postID", postID).AddParameter("tid", tenantID).ExecuteNonQuery();


                var data = DbManager.Connection.CreateCommand(@"select id, thread_id from forum_topic                                                     
                                                    where TenantID = @tid 
                                                    and id = (select topic_id from forum_post where forum_post.id = @postID and TenantID = @tid)")
                                    .AddParameter("postID", postID).AddParameter("tid", tenantID).ExecuteList();

                if (data.Count == 1)
                {
                    var topicID = Convert.ToInt32(data[0][0]);
                    var threadID = Convert.ToInt32(data[0][1]);

                    DbManager.Connection.CreateCommand(@"update forum_topic
                                                      set is_approved = (select coalesce(min(is_approved),1) from forum_post where topic_id = @topicID and forum_post.TenantID = @tid)
                                                    where id = @topicID and TenantID = @tid")
                                                    .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteNonQuery();

                    DbManager.Connection.CreateCommand(@"update forum_thread
                                                      set is_approved = (select coalesce(min(is_approved),1) from forum_topic where thread_id = @threadID and forum_topic.TenantID = @tid)
                                                    where id = @threadID and TenantID = @tid")
                                                    .AddParameter("threadID", threadID).AddParameter("tid", tenantID).ExecuteNonQuery();
                }

                tr.Commit();
            }
        }

        public static DeletePostResult RemovePost(int tenantID, int postID)
        {
            var result = DeletePostResult.Error;

            using (var tr = DbManager.BeginTransaction())
            {
                var parents = DbManager.Connection.CreateCommand("select id from forum_post where parent_post_id = @pid and TenantID = @tid")
                    .AddParameter("pid", postID).AddParameter("tid", tenantID).ExecuteList();

                if (parents.Count > 0)
                    return DeletePostResult.ReferencesBlock;

                var data = DbManager.Connection.CreateCommand(@"select ft.id, ft.thread_id, ft.post_count
from forum_topic ft

inner join forum_post fp
on ft.id = fp.topic_id and ft.TenantID = fp.TenantID

where ft.TenantID = @tid and fp.id = @postID")
                                  .AddParameter("postID", postID).AddParameter("tid", tenantID).ExecuteList();

                var topicID = Convert.ToInt32(data[0][0]);
                var threadID = Convert.ToInt32(data[0][1]);
                var postsCount = Convert.ToInt32(data[0][2]);

                //deny of deleting the last post in a topic
                if (postsCount == 1)
                {
                    tr.Rollback();
                    return result;
                }

                DbManager.Connection.CreateCommand("delete from forum_post where id = @pid and TenantID = @tid")
                    .AddParameter("pid", postID).AddParameter("tid", tenantID).ExecuteNonQuery();

                DbManager.Connection.CreateCommand("delete from forum_attachment where post_id = @pid and TenantID = @tid")
                    .AddParameter("pid", postID).AddParameter("tid", tenantID).ExecuteNonQuery();

                UpdateTopicThread(tenantID, topicID, threadID);

                tr.Commit();
                result = DeletePostResult.Successfully;

            }

            return result;
        }

        private static void UpdateTopicThread(int tenantID, int topicID, int threadID)
        {
            DbManager.Connection.CreateCommand(@"update forum_topic 
                                                        set post_count = (select count(id) from forum_post where topic_id = @topicID and TenantID = @tid),
                                                            is_approved = (select coalesce(min(is_approved),1) from forum_post where topic_id = @topicID and TenantID = @tid),
                                                            recent_post_id = (select coalesce(max(id),0) from forum_post where topic_id = @topicID and TenantID = @tid)
                                                     where id = @topicID and TenantID = @tid")
                                 .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteNonQuery();


            var threadData = DbManager.Connection.CreateCommand(@"select sum(post_count), coalesce(min(is_approved),1), coalesce(max(recent_post_id),0)
                                                    from  forum_topic
                                                    where thread_id = @threadID and TenantID = @tid")
                                              .AddParameter("threadID", threadID).AddParameter("tid", tenantID).ExecuteList();

            var postCount = Convert.ToInt32(threadData[0][0]);
            var isApproved = Convert.ToBoolean(threadData[0][1]);
            var recentPostID = (threadData[0][2] != null ? Convert.ToInt32(threadData[0][2]) : 0);


            int recentTopicID = 0;
            DateTime recentPostDate = DateTime.MinValue;
            Guid recentPosterID = Guid.Empty;

            if (recentPostID != 0)
            {
                var threadPostData = DbManager.Connection.CreateCommand(@"select topic_id, create_date, poster_id
                                                    from  forum_post
                                                    where id = @pid and TenantID = @tid")
                                                 .AddParameter("pid", recentPostID).AddParameter("tid", tenantID).ExecuteList();


                recentTopicID = Convert.ToInt32(threadPostData[0][0]);
                recentPostDate = Convert.ToDateTime(threadPostData[0][1]);
                recentPosterID = threadPostData[0][2] != null ? new Guid(Convert.ToString(threadPostData[0][2])) : Guid.Empty;

            }


            DbManager.Connection.CreateCommand(@"update forum_thread
                                                        set post_count = @postCount,
                                                            is_approved = @isApproved,
                                                            recent_post_id = @recentPostID,
                                                            recent_topic_id = @recentTopicID,
                                                            recent_post_date =  @recentPostDate,
                                                            recent_topic_id = @recentTopicID,
                                                            recent_poster_id = @recentPosterID
                                                     where id = @threadID and TenantID = @tid")

                                 .AddParameter("threadID", threadID)
                                 .AddParameter("postCount", postCount)
                                 .AddParameter("isApproved", isApproved)
                                 .AddParameter("recentPostID", recentPostID)
                                 .AddParameter("recentPostDate", recentPostDate)
                                 .AddParameter("recentTopicID", recentTopicID)
                                 .AddParameter("recentPosterID", recentPosterID)
                                 .AddParameter("tid", tenantID).ExecuteNonQuery();
        }

        public static void UpdatePost(int tenantID, int postID, string subject, string text, PostTextFormatter formatter)
        {
            DbManager.Connection.CreateCommand(@"update forum_post 
                                                      set text = @text,
                                                          subject = @subject,
                                                          formatter = @formatter,
                                                          edit_count = edit_count+1,
                                                          edit_date = @date,
                                                          editor_id = @euid
                                                where id = @postID and TenantID = @tid")
                                .AddParameter("text", text)
                                .AddParameter("subject", subject)
                                .AddParameter("formatter", (int)formatter)
                                .AddParameter("date", DateTime.UtcNow)
                                .AddParameter("euid", SecurityContext.CurrentAccount.ID)
                                .AddParameter("postID", postID)
                                .AddParameter("tid", tenantID).ExecuteNonQuery();
        }

        public static int CreatePost(int tenantID, int topicID, int parentPostID, string subject, string text, bool isApprove, PostTextFormatter formatter)
        {
            int postID;
            using (var tr = DbManager.Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {

                postID = DbManager.ExecuteScalar<int>(new SqlInsert("forum_post")
                                                    .InColumnValue("id", 0)
                                                    .InColumnValue("TenantID", tenantID)
                                                    .InColumnValue("topic_id", topicID)
                                                    .InColumnValue("create_date", DateTime.UtcNow)
                                                    .InColumnValue("subject", subject)
                                                    .InColumnValue("text", text)
                                                    .InColumnValue("poster_id", SecurityContext.CurrentAccount.ID)
                                                    .InColumnValue("is_approved", isApprove ? 1 : 0)
                                                    .InColumnValue("parent_post_id", parentPostID)
                                                    .InColumnValue("formatter", (int)formatter)

                                                    .Identity(0, 0, true));

                var threadID = DbManager.Connection.CreateCommand("select thread_id from forum_topic where id=@topicID and TenantID = @tid")
                    .AddParameter("topicID", topicID).AddParameter("tid", tenantID).ExecuteScalar<int>();



                UpdateTopicThread(tenantID, topicID, threadID);

                tr.Commit();
            }

            return postID;
        }

        #endregion

        #region Tags

        public static List<Tag> SearchTags(int tenantID, string text)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_tag").Select("id", "name", "is_approved")
                                  .Where(Exp.Eq("TenantID", tenantID) & Exp.Like("name", text, SqlLike.StartWith)));

            return data.Select(row => new Tag
                                          {
                                              TenantID = tenantID, ID = Convert.ToInt32(row[0]),
                                              Name = Convert.ToString(row[1]),
                                              IsApproved = Convert.ToBoolean(row[2])
                                          }).ToList();

        }

        public static List<Tag> GetTagByIDs(int tenantID, List<int> ids)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_tag").Select("id", "name", "is_approved")
                                  .Where(Exp.Eq("TenantID", tenantID) & Exp.In("id", ids)));

            return data.Select(row => new Tag
                                          {
                                              TenantID = tenantID,
                                              ID = Convert.ToInt32(row[0]),
                                              Name = Convert.ToString(row[1]),
                                              IsApproved = Convert.ToBoolean(row[2])
                                          }).ToList();
        }

        public static Tag GetTagByID(int tenantID, int tagID)
        {
            var data = DbManager.ExecuteList(new SqlQuery("forum_tag").Select("id", "name", "is_approved")
                                  .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", tagID)));

            if (data.Count > 0)
            {
                var row = data[0];
                return new Tag
                           {
                               TenantID = tenantID,
                               ID = Convert.ToInt32(row[0]),
                               Name = Convert.ToString(row[1]),
                               IsApproved = Convert.ToBoolean(row[2])
                           };
            }

            return null;

        }

        public static int CreateTag(int tenantID, int topicID, string name, bool isApproved)
        {
            if (string.IsNullOrEmpty(name)) return 0;

            int tagID;
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                tagID = DbManager.ExecuteScalar<int>(new SqlQuery("forum_tag")
                    .Select("id")
                    .Where("TenantID", tenantID)
                    .Where("lower(name)", name.ToLower())
                    .SetMaxResults(1));
                if (tagID == 0)
                {
                    tagID = DbManager.ExecuteScalar<int>(new SqlInsert("forum_tag")
                                                        .InColumnValue("id", 0)
                                                        .InColumnValue("name", name)
                                                        .InColumnValue("TenantID", tenantID)
                                                        .InColumnValue("is_approved", isApproved ? 1 : 0)
                                                        .Identity(0, 0, true));
                }
                AttachTagToTopic(tenantID, tagID, topicID);

                tr.Commit();
            }
            return tagID;
        }

        public static void AttachTagToTopic(int tenantID, int tagID, int topicID)
        {
            DbManager.ExecuteNonQuery(new SqlInsert("forum_topic_tag", true).InColumnValue("topic_id", topicID)
                                                                    .InColumnValue("tag_id", tagID));
        }

        public static void RemoveTagFromTopic(int tenantID, int tagID, int topicID)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlDelete("forum_topic_tag").Where(
                                                Exp.Eq("topic_id", topicID) & Exp.Eq("tag_id", tagID)));


                DbManager.Connection.CreateCommand(@"delete from forum_tag
                                         where TenantID = @tid and id not in (select tag_id from forum_topic_tag)")
                                                     .AddParameter("tid", tenantID).ExecuteNonQuery();

                tr.Commit();
            }
        }

        public static void RemoveTag(int tenantID, int tagID)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlDelete("forum_tag").Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", tagID)));
                DbManager.ExecuteNonQuery(new SqlDelete("forum_topic_tag").Where("tag_id", tagID));

                tr.Commit();
            }
        }

        public static void UpdateTag(int tenantID, int tagID, string name, bool isApproved)
        {
            DbManager.ExecuteNonQuery(new SqlUpdate("forum_tag").Set("name", name)
                                                              .Set("is_approved", isApproved ? 1 : 0)
                                                              .Where(Exp.Eq("TenantID", tenantID) & Exp.Eq("id", tagID)));
        }

        public static List<Topic> GetTopicsByAllTags(int tenantID)
        {
            var topicIDs = DbManager.ExecuteList(new SqlQuery("forum_tag t1").Select("distinct(t2.topic_id)")
                                  .InnerJoin("forum_topic_tag t2", Exp.EqColumns("t1.id", "t2.topic_id") & Exp.Eq("t1.TenantID", tenantID)))
                                  .ConvertAll(r => Convert.ToInt32(r[0]));

            return GetTopicsByIDs(tenantID, topicIDs, true);
        }

        #endregion

        #region News

        public static IEnumerable<Post> GetPosts(DateTime from, DateTime to, int tenantId)
        {
            var query = new SqlQuery("forum_post p")
                .Select("p.id", "p.create_date", "p.subject", "p.text", "p.LastModified", "p.poster_id")
                .LeftOuterJoin("forum_topic t", Exp.EqColumns("p.topic_id", "t.id"))
                .Where(Exp.Between("p.create_date", from, to) & !Exp.EqColumns("p.poster_id", "t.poster_id") & Exp.Eq("p.TenantID", tenantId))
                .OrderBy("p.create_date", false);
            return DbManager.ExecuteList(query).Select(x => new Post
                                                                {
                                                                    ID = Convert.ToInt32(x[0]),
                                                                    CreateDate = Convert.ToDateTime(x[1]),
                                                                    Subject = Convert.ToString(x[2]),
                                                                    Text = Convert.ToString(x[3]),
                                                                    EditDate = Convert.ToDateTime(x[4]),
                                                                    PosterID = new Guid(Convert.ToString(x[5]))
                                                                });
        }

        public static IEnumerable<Topic> GetTopics(DateTime from, DateTime to, int tenantId)
        {
            var query = new SqlQuery("forum_topic t")
                .Select("t.Id", "t.title", "t.create_date", "t.poster_id", "t.LastModified", "th.title", "t.type")
                .LeftOuterJoin("forum_thread th", Exp.EqColumns("t.thread_id", "th.id"))
                .Where(Exp.Between("t.create_date", from, to) & Exp.Eq("t.TenantId", tenantId))
                .OrderBy("t.create_date", false);
            return DbManager.ExecuteList(query).Select(x => new Topic
                                                                {
                                                                    ID = Convert.ToInt32(x[0]),
                                                                    Title = Convert.ToString(x[1]),
                                                                    CreateDate = Convert.ToDateTime(x[2]),
                                                                    PosterID = new Guid(Convert.ToString(x[3])),
                                                                    RecentPostCreateDate = Convert.ToDateTime(x[4]),
                                                                    ThreadTitle = Convert.ToString(x[5]),
                                                                    Type =
                                                                        Convert.ToInt32(x[6]) == 0
                                                                            ? TopicType.Informational
                                                                            : TopicType.Poll
                                                                });
        }

        #endregion
    }

    public class RemoveDataHelper
    {
        public static void RemoveThreadCategory(ThreadCategory category)
        {
            List<int> removedPostIDs;

            ForumDataProvider.RemoveThreadCategory(TenantProvider.CurrentTenantID, category.ID, out removedPostIDs);

            ForumManager.Instance.RemoveAttachments(category);

            removedPostIDs.ForEach(
                idPost =>
                CommonControlsConfigurer.FCKUploadsRemoveForItem(ForumManager.Settings.FileStoreModuleID,
                                                                 idPost.ToString(CultureInfo.InvariantCulture)));
        }

        public static void RemoveThread(Thread thread)
        {
            List<int> removedPostIDs;

            ForumDataProvider.RemoveThread(TenantProvider.CurrentTenantID, thread.ID, out removedPostIDs);

            ForumManager.Instance.RemoveAttachments(thread);

            removedPostIDs.ForEach(
                idPost =>
                CommonControlsConfigurer.FCKUploadsRemoveForItem(ForumManager.Settings.FileStoreModuleID,
                                                                 idPost.ToString(CultureInfo.InvariantCulture)));
        }

        public static void RemoveTopic(Topic topic)
        {
            List<int> removedPostIDs;

            var attachmantOffsetPhysicalPaths = ForumDataProvider.RemoveTopic(TenantProvider.CurrentTenantID, topic.ID,
                                                                              out removedPostIDs);

            foreach (var ace in Module.Constants.Aces)
            {
                CoreContext.AuthorizationManager.RemoveAce(new AzRecord(SecurityContext.CurrentAccount.ID, ace,
                                                                        Common.Security.Authorizing.AceType.Allow, topic));
            }

            FactoryIndexer<TopicWrapper>.DeleteAsync(topic);

            ForumManager.Settings.ForumManager.RemoveAttachments(attachmantOffsetPhysicalPaths.ToArray());

            removedPostIDs.ForEach(
                idPost =>
                CommonControlsConfigurer.FCKUploadsRemoveForItem(ForumManager.Settings.ForumManager.Settings.FileStoreModuleID,
                                                                 idPost.ToString(CultureInfo.InvariantCulture)));
        }

        public static DeletePostResult RemovePost(Post post)
        {
            var result = ForumDataProvider.RemovePost(TenantProvider.CurrentTenantID, post.ID);

            if (result == DeletePostResult.Successfully)
            {
                ForumManager.Settings.ForumManager.RemoveAttachments(post);

                FactoryIndexer<PostWrapper>.DeleteAsync(post);

                CommonControlsConfigurer.FCKUploadsRemoveForItem(ForumManager.Settings.ForumManager.Settings.FileStoreModuleID,
                                                                 post.ID.ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }
    }
}