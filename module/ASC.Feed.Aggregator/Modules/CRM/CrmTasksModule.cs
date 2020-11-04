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
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Studio.Utility;
using System.Linq;
using ASC.Core.Users;

namespace ASC.Feed.Aggregator.Modules.CRM
{
    internal class CrmTasksModule : FeedModule
    {
        private const string item = "crmTask";


        protected override string Table
        {
            get { return "crm_task"; }
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
            get { return Constants.CrmTasksModule; }
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
            return base.VisibleFor(feed, data, userId) && CRMSecurity.CanGoToFeed((Task)data);
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query =
                new SqlQuery("crm_task t")
                    .Select(TaskColumns().Select(t => "t." + t).ToArray())
                    .Where("t.tenant_id", filter.Tenant)
                    .Where(Exp.Between("t.create_on", filter.Time.From, filter.Time.To))
                    .LeftOuterJoin("crm_contact c",
                                   Exp.EqColumns("c.id", "t.contact_id")
                                   & Exp.Eq("c.tenant_id", filter.Tenant)
                    )
                    .Select(ContactColumns().Select(c => "c." + c).ToArray());

            using (var db = new DbManager(DbId))
            {
                var tasks = db.ExecuteList(query).ConvertAll(ToTask);
                return tasks.Select(t => new Tuple<Feed, object>(ToFeed(t), t));
            }
        }


        private static IEnumerable<string> TaskColumns()
        {
            return new[]
                {
                    "id",
                    "title",
                    "description",
                    "deadline",
                    "responsible_id",
                    "contact_id",
                    "is_closed",
                    "entity_type",
                    "entity_id",
                    "category_id",
                    "create_by",
                    "create_on",
                    "last_modifed_by",
                    "last_modifed_on" // 13
                };
        }

        private static IEnumerable<string> ContactColumns()
        {
            return new[]
                {
                    "is_company", // 14
                    "id",
                    "notes",
                    "first_name",
                    "last_name",
                    "company_name",
                    "company_id",
                    "display_name",
                    "create_by",
                    "create_on",
                    "last_modifed_by",
                    "last_modifed_on" // 25
                };
        }

        private static Task ToTask(object[] r)
        {
            var task = new Task
                {
                    ID = Convert.ToInt32(r[0]),
                    Title = Convert.ToString(r[1]),
                    Description = Convert.ToString(r[2]),
                    DeadLine = Convert.ToDateTime(r[3]),
                    ResponsibleID = new Guid(Convert.ToString(r[4])),
                    ContactID = Convert.ToInt32(r[5]),
                    IsClosed = Convert.ToBoolean(r[6]),
                    EntityType = (EntityType)Convert.ToInt32(r[7]),
                    EntityID = Convert.ToInt32(r[8]),
                    CategoryID = Convert.ToInt32(r[9]),
                    CreateBy = new Guid(Convert.ToString(r[10])),
                    CreateOn = Convert.ToDateTime(r[11]),
                    LastModifedBy = new Guid(Convert.ToString(r[12])),
                    LastModifedOn = Convert.ToDateTime(r[13])
                };

            if (string.IsNullOrEmpty(Convert.ToString(r[14]))) return task;

            var isCompany = Convert.ToBoolean(r[14]);
            if (isCompany)
            {
                task.Contact = new Company
                    {
                        ID = Convert.ToInt32(r[15]),
                        About = Convert.ToString(r[16]),
                        CompanyName = Convert.ToString(r[19]),
                        CreateBy = new Guid(Convert.ToString(r[22])),
                        CreateOn = Convert.ToDateTime(r[23]),
                        LastModifedBy = new Guid(Convert.ToString(r[24])),
                        LastModifedOn = Convert.ToDateTime(r[25])
                    };
            }
            else
            {
                task.Contact = new Person
                    {
                        ID = Convert.ToInt32(r[15]),
                        About = Convert.ToString(r[16]),
                        FirstName = Convert.ToString(r[17]),
                        LastName = Convert.ToString(r[18]),
                        CompanyID = Convert.ToInt32(r[20]),
                        JobTitle = Convert.ToString(r[21]),
                        CreateBy = new Guid(Convert.ToString(r[22])),
                        CreateOn = Convert.ToDateTime(r[23]),
                        LastModifedBy = new Guid(Convert.ToString(r[24])),
                        LastModifedOn = Convert.ToDateTime(r[25])
                    };
            }

            return task;
        }

        private Feed ToFeed(Task task)
        {
            const string itemUrl = "/Products/CRM/Tasks.aspx";
            return new Feed(task.CreateBy, task.CreateOn)
                {
                    Item = item,
                    ItemId = task.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = task.Title,
                    Description = Helper.GetHtmlDescription(HttpUtility.HtmlEncode(task.Description)),
                    AdditionalInfo = Helper.GetUser(task.ResponsibleID).DisplayUserName(),
                    AdditionalInfo2 = task.Contact.GetTitle(),
                    Keywords = string.Format("{0} {1}", task.Title, task.Description),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, task.CreateBy)
                };
        }
    }
}