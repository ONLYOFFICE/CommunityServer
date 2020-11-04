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
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.CRM.Core;
using ASC.Web.Studio.Utility;
using Autofac;

namespace ASC.Feed.Aggregator.Modules.CRM
{
    internal class CasesModule : FeedModule
    {
        private const string item = "cases";


        protected override string Table
        {
            get { return "crm_case"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "create_on"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant_id"; }
        }

        protected override string DbId
        {
            get { return Constants.CrmDbId; }
        }


        public override string Name
        {
            get { return Constants.CasesModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.CRMProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.CRMProductID; }
        }

        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            return base.VisibleFor(feed, data, userId) && CRMSecurity.CanAccessTo((Cases)data);
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query = new SqlQuery("crm_case c")
                .Select(CasesColumns().Select(c => "c." + c).ToArray())
                .LeftOuterJoin("crm_entity_contact ec",
                               Exp.EqColumns("ec.entity_id", "c.id")
                               & Exp.Eq("ec.entity_type", 7)
                )
                .Select("group_concat(distinct cast(ec.contact_id as char))")
                .Where("c.tenant_id", filter.Tenant)
                .Where(Exp.Between("c.create_on", filter.Time.From, filter.Time.To))
                .GroupBy("c.id");

            using (var db = new DbManager(DbId))
            using (var scope = DIHelper.Resolve())
            {
                var dao = scope.Resolve<DaoFactory>().ContactDao;
                var cases = db.ExecuteList(query).ConvertAll(ToCases);
                return cases.Select(c => new Tuple<Feed, object>(ToFeed(c, dao), c)).ToList();
            }
        }


        private static IEnumerable<string> CasesColumns()
        {
            return new[]
                {
                    "id",
                    "title",
                    "is_closed",
                    "create_by",
                    "create_on",
                    "last_modifed_by",
                    "last_modifed_on" //6
                };
        }

        private static Cases ToCases(object[] r)
        {
            var cases = new Cases
                {
                    ID = Convert.ToInt32(r[0]),
                    Title = Convert.ToString(r[1]),
                    IsClosed = Convert.ToBoolean(r[2]),
                    CreateBy = new Guid(Convert.ToString(r[3])),
                    CreateOn = Convert.ToDateTime(r[4]),
                    LastModifedBy = new Guid(Convert.ToString(r[5])),
                    LastModifedOn = Convert.ToDateTime(r[6])
                };

            var members = Convert.ToString(r[7]);
            if (!string.IsNullOrEmpty(members))
            {
                cases.Members = new HashSet<int>(
                    members.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                           .Select(x => Convert.ToInt32(x))
                    );
            }

            return cases;
        }

        private Feed ToFeed(Cases cases, ContactDao dao)
        {
            var contactsString = Helper.GetContactsString(cases.Members, dao);

            var itemUrl = "/Products/CRM/Cases.aspx?id=" + cases.ID + "#profile";
            return new Feed(cases.CreateBy, cases.CreateOn)
            {
                Item = item,
                ItemId = cases.ID.ToString(CultureInfo.InvariantCulture),
                ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                Product = Product,
                Module = Name,
                Title = cases.Title,
                AdditionalInfo = contactsString,
                Keywords = cases.Title,
                HasPreview = false,
                CanComment = false,
                GroupId = GetGroupId(item, cases.CreateBy)
            };
        }
    }
}