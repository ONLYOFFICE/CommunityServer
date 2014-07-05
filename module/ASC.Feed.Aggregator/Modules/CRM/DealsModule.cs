/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

namespace ASC.Feed.Aggregator.Modules.CRM
{
    internal class DealsModule : FeedModule
    {
        private const string item = "deal";


        protected override string Table
        {
            get { return "crm_deal"; }
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
            get { return Constants.DealsModule; }
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
            return base.VisibleFor(feed, data, userId) && CRMSecurity.CanAccessTo((Deal)data);
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query = new SqlQuery("crm_deal d")
                .Select(DealColumns().Select(d => "d." + d).ToArray())
                .Where("d.tenant_id", filter.Tenant)
                .Where(Exp.Between("d.create_on", filter.Time.From, filter.Time.To))
                .LeftOuterJoin("crm_contact c",
                               Exp.EqColumns("c.id", "d.contact_id")
                               & Exp.Eq("c.tenant_id", filter.Tenant)
                )
                .Select(ContactColumns().Select(c => "c." + c).ToArray());

            using (var db = new DbManager(Constants.CrmDbId))
            {
                var deals = db.ExecuteList(query).ConvertAll(ToDeal);
                return deals.Select(d => new Tuple<Feed, object>(ToFeed(d), d));
            }
        }


        private static IEnumerable<string> DealColumns()
        {
            return new[]
                {
                    "id",
                    "title",
                    "description",
                    "responsible_id",
                    "create_by",
                    "create_on",
                    "last_modifed_by",
                    "last_modifed_on",
                    "contact_id" // 8
                };
        }

        private static IEnumerable<string> ContactColumns()
        {
            return new[]
                {
                    "is_company", // 9
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
                    "last_modifed_on" // 20
                };
        }

        private static Deal ToDeal(object[] r)
        {
            var deal = new Deal
                {
                    ID = Convert.ToInt32(r[0]),
                    Title = Convert.ToString(r[1]),
                    Description = Convert.ToString(r[2]),
                    ResponsibleID = new Guid(Convert.ToString(r[3])),
                    CreateBy = new Guid(Convert.ToString(r[4])),
                    CreateOn = Convert.ToDateTime(r[5]),
                    LastModifedBy = new Guid(Convert.ToString(r[6])),
                    LastModifedOn = Convert.ToDateTime(r[7])
                };

            var contactId = Convert.ToInt32(r[8]);
            if (!string.IsNullOrEmpty(Convert.ToString(r[9])))
            {
                var isCompany = Convert.ToBoolean(r[9]);
                if (contactId > 0 && isCompany)
                {
                    deal.Contact = new Company
                        {
                            ID = Convert.ToInt32(r[10]),
                            About = Convert.ToString(r[11]),
                            CompanyName = Convert.ToString(r[14]),
                            CreateBy = new Guid(Convert.ToString(r[17])),
                            CreateOn = Convert.ToDateTime(r[18]),
                            LastModifedBy = new Guid(Convert.ToString(r[19])),
                            LastModifedOn = Convert.ToDateTime(r[20])
                        };
                }
                else
                {
                    deal.Contact = new Person
                        {
                            ID = Convert.ToInt32(r[10]),
                            About = Convert.ToString(r[11]),
                            FirstName = Convert.ToString(r[12]),
                            LastName = Convert.ToString(r[13]),
                            CompanyID = Convert.ToInt32(r[15]),
                            JobTitle = Convert.ToString(r[16]),
                            CreateBy = new Guid(Convert.ToString(r[17])),
                            CreateOn = Convert.ToDateTime(r[18]),
                            LastModifedBy = new Guid(Convert.ToString(r[19])),
                            LastModifedOn = Convert.ToDateTime(r[20])
                        };
                }
            }

            return deal;
        }

        private Feed ToFeed(Deal deal)
        {
            var itemId = "/products/crm/deals.aspx?id=" + deal.ID + "#profile";
            return new Feed(deal.CreateBy, deal.CreateOn)
                {
                    Item = item,
                    ItemId = deal.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemId),
                    Product = Product,
                    Module = Name,
                    Title = deal.Title,
                    Description = Helper.GetHtmlDescription(HttpUtility.HtmlEncode(deal.Description)),
                    AdditionalInfo = deal.Contact.GetTitle(),
                    Keywords = string.Format("{0} {1}", deal.Title, deal.Description),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, deal.CreateBy)
                };
        }
    }
}