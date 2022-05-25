/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Feed.Data;
using ASC.Web.Studio.Utility;

namespace ASC.Feed.Aggregator.Modules.People
{
    internal class BirthdaysModule : FeedModule
    {

        public override string Name
        {
            get { return Constants.BirthdaysModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.PeopleProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.PeopleProductID; }
        }

        protected override string Table
        {
            get { return null; }
        }

        protected override string LastUpdatedColumn
        {
            get { return null; }
        }

        protected override string TenantColumn
        {
            get { return null; }
        }

        protected override string DbId
        {
            get { return Constants.PeopleDbId; }
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var lastTimeAggregate = FeedAggregateDataProvider.GetLastTimeAggregate(GetType().Name);
            var now = DateTime.UtcNow;

            if (lastTimeAggregate.Date == now.Date)
            {
                return new List<int>();
            }

            var q = new SqlQuery("tenants_tenants t")
                .Select("t.id")
                .Distinct()
                .InnerJoin("core_user u", Exp.EqColumns("u.tenant", "t.id"))
                .Where(Exp.Eq("t.status", TenantStatus.Active))
                .Where(Exp.Eq("u.status", EmployeeStatus.Active))
                .Where(!Exp.Eq("u.bithdate", null))
                .Where(Exp.Eq("month(u.bithdate)", now.Month))
                .Where(Exp.Eq("day(u.bithdate)", now.Day));

            using (var db = DbManager.FromHttpContext(DbId))
            {
                return db.ExecuteList(q)
                         .ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var now = DateTime.UtcNow;
            var list = (from u in CoreContext.UserManager.GetUsers(EmployeeStatus.Active, EmployeeType.User)
                        where u.BirthDate.HasValue && u.BirthDate.Value.Month.Equals(now.Month) && u.BirthDate.Value.Day.Equals(now.Day)
                        orderby u.DisplayUserName()
                        select u).ToList();
            return list.Select(c => new Tuple<Feed, object>(ToFeed(c, now), c));
        }


        private Feed ToFeed(UserInfo user, DateTime now)
        {
            var item = Constants.BirthdaysModule;

            return new Feed(user.ID, now.Date)
            {
                Item = item,
                ItemId = user.ID.ToString(),
                Title = Web.Studio.PublicResources.FeedResource.Birthday,
                ItemUrl = CommonLinkUtility.ToAbsolute("~/Products/People/Birthdays.aspx"),
                Product = Product,
                Action = FeedAction.AllDayEventCreated,
                Module = Name,
                IsAllDayEvent = true,
                GroupId = GetGroupId(item, user.ID),
                HasPreview = false,
                CanComment = false
            };
        }
    }
}
