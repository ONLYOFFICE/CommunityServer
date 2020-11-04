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
using Contact = ASC.CRM.Core.Entities.Contact;
using System.Linq;
using ASC.Web.CRM.Core.Enums;

namespace ASC.Feed.Aggregator.Modules.CRM
{
    internal class ContactsModule : FeedModule
    {
        protected override string Table
        {
            get { return "crm_contact"; }
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
            get { return Constants.ContactsModule; }
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
            return base.VisibleFor(feed, data, userId) && CRMSecurity.CanAccessTo((Contact)data);
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query = new SqlQuery("crm_contact")
                .Select(ContactColumns().ToArray())
                .Where("tenant_id", filter.Tenant)
                .Where(Exp.Between("create_on", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var contacts = db.ExecuteList(query).ConvertAll(ToContact);
                return contacts.Select(c => new Tuple<Feed, object>(ToFeed(c), c));
            }
        }


        private static IEnumerable<string> ContactColumns()
        {
            return new[]
                {
                    "is_company",
                    "id",
                    "notes",
                    "first_name",
                    "last_name",
                    "company_name",
                    "company_id",
                    "display_name",
                    "is_shared",
                    "create_by",
                    "create_on",
                    "last_modifed_by",
                    "last_modifed_on" // 12
                };
        }

        private static Contact ToContact(object[] r)
        {
            Contact contact;
            var isCompany = Convert.ToBoolean(r[0]);

            if (isCompany)
            {
                contact = new Company
                    {
                        ID = Convert.ToInt32(r[1]),
                        About = Convert.ToString(r[2]),
                        CompanyName = Convert.ToString(r[5]),
                        ShareType = r[8] != null ? (ShareType)Convert.ToInt32(r[8]) : ShareType.None,
                        CreateBy = new Guid(Convert.ToString(r[9])),
                        CreateOn = Convert.ToDateTime(r[10]),
                        LastModifedBy = new Guid(Convert.ToString(r[11])),
                        LastModifedOn = Convert.ToDateTime(r[12])
                    };
            }
            else
            {
                contact = new Person
                    {
                        ID = Convert.ToInt32(r[1]),
                        About = Convert.ToString(r[2]),
                        FirstName = Convert.ToString(r[3]),
                        LastName = Convert.ToString(r[4]),
                        CompanyID = Convert.ToInt32(r[6]),
                        JobTitle = Convert.ToString(r[7]),
                        ShareType =r[8] != null ? (ShareType)Convert.ToInt32(r[8]) : ShareType.None,
                        CreateBy = new Guid(Convert.ToString(r[9])),
                        CreateOn = Convert.ToDateTime(r[10]),
                        LastModifedBy = new Guid(Convert.ToString(r[11])),
                        LastModifedOn = Convert.ToDateTime(r[12])
                    };
            }

            return contact;
        }

        private Feed ToFeed(Contact contact)
        {
            var item = string.Empty;
            if (contact is Person)
            {
                item = "person";
            }
            else if (contact is Company)
            {
                item = "company";
            }

            var itemUrl = "/Products/CRM/Default.aspx?id=" + contact.ID + "#profile";
            return new Feed(contact.CreateBy, contact.CreateOn)
                {
                    Item = item,
                    ItemId = contact.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = contact.GetTitle(),
                    Description = Helper.GetHtmlDescription(HttpUtility.HtmlEncode(contact.About)),
                    Keywords = string.Format("{0}", contact.GetTitle()),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, contact.CreateBy)
                };
        }
    }
}