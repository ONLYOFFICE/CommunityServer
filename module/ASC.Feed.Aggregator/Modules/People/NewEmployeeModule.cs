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
using ASC.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Feed.Aggregator.Modules.People
{
    internal class NewEmployeeModule : FeedModule
    {

        public override string Name
        {
            get { return Constants.NewEmployeeModule; }
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
            get { return "core_user"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "last_modified"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant"; }
        }

        protected override string DbId
        {
            get { return Constants.PeopleDbId; }
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery(Table)
                .Select(TenantColumn)
                .Where(Exp.Gt(LastUpdatedColumn, fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = DbManager.FromHttpContext(DbId))
            {
                return db.ExecuteList(q1)
                         .ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var list = (from u in CoreContext.UserManager.GetUsers(EmployeeStatus.Active, EmployeeType.User)
                    where u.CreateDate >= filter.Time.From && u.CreateDate <= filter.Time.To
                        orderby u.DisplayUserName()
                    select u)
                .ToList();
            return list.Select(c => new Tuple<Feed, object>(ToFeed(c), c));
        }


        private Feed ToFeed(UserInfo user)
        {
            var item = Constants.NewEmployeeModule;

            return new Feed(user.ID, user.CreateDate)
            {
                Item = item,
                ItemId = user.ID.ToString(),
                Title = Web.Studio.PublicResources.FeedResource.NewEmployee,
                ItemUrl = CommonLinkUtility.ToAbsolute("~/Products/People/Default.aspx"),
                Product = Product,
                Module = Name,
                IsAllDayEvent = true,
                Action = FeedAction.AllDayEventCreated,
                GroupId = GetGroupId(item, user.ID),
                HasPreview = false,
                CanComment = false
            };
        }
    }
}
