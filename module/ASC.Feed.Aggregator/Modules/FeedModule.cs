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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Feed.Data;
using ASC.Web.Core;

namespace ASC.Feed.Aggregator.Modules
{
    internal abstract class FeedModule : IFeedModule
    {
        public abstract string Name { get; }
        public abstract string Product { get; }
        public abstract Guid ProductID { get; }

        protected abstract string Table { get; }
        protected abstract string LastUpdatedColumn { get; }
        protected abstract string TenantColumn { get; }
        protected abstract string DbId { get; }

        protected int Tenant
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        protected string GetGroupId(string item, Guid author, string rootId = null, int action = -1)
        {
            const int interval = 2;

            var now = DateTime.UtcNow;
            var hours = now.Hour;
            var groupIdHours = hours - (hours % interval);

            if (rootId == null)
            {
                // groupId = {item}_{author}_{date}
                return string.Format("{0}_{1}_{2}",
                                     item,
                                     author,
                                     now.ToString("yyyy.MM.dd.") + groupIdHours);
            }
            if (action == -1)
            {
                // groupId = {item}_{author}_{date}_{rootId}_{action}
                return string.Format("{0}_{1}_{2}_{3}",
                                     item,
                                     author,
                                     now.ToString("yyyy.MM.dd.") + groupIdHours,
                                     rootId);
            }

            // groupId = {item}_{author}_{date}_{rootId}_{action}
            return string.Format("{0}_{1}_{2}_{3}_{4}",
                                 item,
                                 author,
                                 now.ToString("yyyy.MM.dd.") + groupIdHours,
                                 rootId,
                                 action);
        }


        public virtual IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q = new SqlQuery(Table)
                .Select(TenantColumn)
                .Where(Exp.Gt(LastUpdatedColumn, fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = new DbManager(DbId))
            {
                return db
                    .ExecuteList(q)
                    .ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public abstract IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter);

        public virtual bool VisibleFor(Feed feed, object data, Guid userId)
        {
            return WebItemSecurity.IsAvailableForUser(ProductID, userId);
        }

        public virtual void VisibleFor(List<Tuple<FeedRow, object>> feed, Guid userId)
        {
            if (!WebItemSecurity.IsAvailableForUser(ProductID, userId)) return;

            foreach (var tuple in feed)
            {
                if (VisibleFor(tuple.Item1.Feed, tuple.Item2, userId))
                {
                    tuple.Item1.Users.Add(userId);
                }
            }
        }


        protected static Guid ToGuid(object guid)
        {
            try
            {
                var str = guid as string;
                return !string.IsNullOrEmpty(str) ? new Guid(str) : Guid.Empty;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }

        }
    }
}