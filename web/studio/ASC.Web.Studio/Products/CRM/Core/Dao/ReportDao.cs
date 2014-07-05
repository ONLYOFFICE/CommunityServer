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

#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.CRM.Core.Entities;
using ASC.Web.Studio.Utility;
using System.Xml.XPath;

#endregion

namespace ASC.CRM.Core.Dao
{

    public enum ReportViewType
    {
        Html,
        Print,
        EMail,
        Xml,
        Csv
    }

    public enum ReportType
    {
        SalesByStage,
        SalesByContact,
        SalesByMonth,
        SalesByManager,
        SalesForecastByClient,
        SalesForecastByMonth,
        SalesForecastByManager
    }

    public class ReportDao : AbstractDao
    {

        public ReportDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {

        }

        public List<Object[]> BuildContactPopulateReport(
                                                   DateTime fromDate,
                                                   DateTime toDate,
                                                   bool? isCompany,
                                                   String tagName,
                                                   int contactType
                                              )
        {
            var sqlQuery = Query("crm_contact")
                           .Select("date(create_on) as cur_date")
                           .Select("count(*) as total")
                           .GroupBy("cur_date");

            if (contactType != 0)
                sqlQuery.Where(Exp.Eq("status_id", contactType));

            using (var db = GetDb())
            {
                if (!String.IsNullOrEmpty(tagName))
                {

                    var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                 .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)EntityType.Contact)));

                    var findedContacts = db.ExecuteList(new SqlQuery("tag_id")
                                         .Select("entity_id")
                                        .Where(Exp.Eq("tag_id", tagID) &
                                               Exp.Eq("entity_type", (int)EntityType.Contact)
                                              ));

                    sqlQuery.Where(Exp.In("id", findedContacts));

                }

                if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                    sqlQuery.Where(Exp.Between("cur_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate.AddDays(1).AddHours(-1))));
                else if (fromDate != DateTime.MinValue)
                    sqlQuery.Where(Exp.Ge("cur_date", TenantUtil.DateTimeToUtc(fromDate)));
                else if (toDate != DateTime.MinValue)
                    sqlQuery.Where(Exp.Le("cur_date", TenantUtil.DateTimeToUtc(toDate)));

                if (isCompany.HasValue)
                    sqlQuery.Where("is_company", isCompany);

                var sqlResult = db.ExecuteList(sqlQuery);

                return sqlResult;
            }
        }

        public List<Object[]> BuildSalesForecastByMonthReport(Guid responsibleID, DateTime fromDate, DateTime toDate)
        {

            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
                        .Select("MONTH(tbl_deal.expected_close_date) as acdMonth",
                                "tbl_deal.bid_currency",
                                @"sum(CASE tbl_deal.bid_type
                                           WHEN 0 THEN
                                             (tbl_deal.bid_value)
                                           ELSE
                                             (tbl_deal.bid_value * tbl_deal.per_period_value * tbl_deal.deal_milestone_probability)
                                           END) AS bid_value",
                                "count(tbl_deal.id) as count")
                        .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                        .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
                               Exp.Between("tbl_deal.expected_close_date", fromDate, toDate) &
                               !Exp.Eq("tbl_deal.bid_value", 0) &
                               Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.Open))
                        .GroupBy("acdMonth", "tbl_deal.bid_currency");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0) return new List<object[]>();

                sqlResult.ForEach(row => row[0] = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)row[0]));

                sqlResult = sqlResult.GroupBy(row =>
                {
                    var bidCurrency = Convert.ToString(row[1]);

                    if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation && CurrencyProvider.IsConvertable(bidCurrency))
                        row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                    return row[0];

                }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

                var totalDeals = sqlResult.Sum(row => Convert.ToInt32(row[2]));

                foreach (var item in sqlResult)
                    item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;

                return sqlResult;
            }
        }

        public List<Object[]> BuildSalesForecastByManagerReport(DateTime fromDate, DateTime toDate)
        {

            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
                         .Select("tbl_deal.responsible_id",
                                 "tbl_deal.bid_currency",
                                 @"sum(CASE tbl_deal.bid_type
                                           WHEN 0 THEN
                                             (tbl_deal.bid_value)
                                           ELSE
                                             (tbl_deal.bid_value * tbl_deal.per_period_value * tbl_deal.deal_milestone_probability)
                                           END) AS bid_value",
                                 "count(tbl_deal.id) as count")
                         .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                         .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
                                Exp.Between("tbl_deal.expected_close_date", fromDate, toDate) &
                               !Exp.Eq("tbl_deal.bid_value", 0) &
                                Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.Open))
                         .GroupBy("tbl_deal.responsible_id", "tbl_deal.bid_currency");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0) return new List<object[]>();

                sqlResult = sqlResult.GroupBy(row =>
                {
                    var bidCurrency = Convert.ToString(row[1]);

                    if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation && CurrencyProvider.IsConvertable(bidCurrency))
                        row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                    return row[0];

                }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

                var totalDeals = sqlResult.Sum(row => Convert.ToInt32(row[2]));

                foreach (var item in sqlResult)
                    item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;

                return sqlResult;
            }
        }

        public List<Object[]> BuildSalesForecastByClientReport(DateTime fromDate, DateTime toDate)
        {
            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
                        .Select("tbl_deal.contact_id",
                                "tbl_deal.bid_currency",
                                @"sum(CASE tbl_deal.bid_type
                                           WHEN 0 THEN
                                             (tbl_deal.bid_value)
                                           ELSE
                                             (tbl_deal.bid_value * tbl_deal.per_period_value * tbl_deal.deal_milestone_probability)
                                           END) AS bid_value",
                                "count(tbl_deal.id) as count")
                        .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                        .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
                               Exp.Between("tbl_deal.expected_close_date", fromDate, toDate) &
                              !Exp.Eq("tbl_deal.bid_value", 0) &
                              !Exp.Eq("tbl_deal.contact_id", 0) &
                               Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.Open))
                        .GroupBy("tbl_deal.contact_id", "tbl_deal.bid_currency");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0) return sqlResult;

                sqlResult.ForEach(row => row[0] = Global.DaoFactory.GetContactDao().GetByID((int)row[0]).GetTitle());

                sqlResult = sqlResult.GroupBy(row =>
                {
                    var bidCurrency = Convert.ToString(row[1]);

                    if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation && CurrencyProvider.IsConvertable(bidCurrency))
                        row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                    return row[0];

                }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

                var totalDeals = sqlResult.Sum(row => Convert.ToInt32(row[2]));

                foreach (var item in sqlResult)
                    item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;

                return sqlResult;
            }
        }

        public List<Object[]> BuildWorkLoadReport(Guid responsibleID)
        {

            var sqlSubQuery =
                 Query("crm_task")
                .Select("responsible_id,category_id, is_closed, count(*) as total")
                .GroupBy(0, 1, 2)
                .OrderBy(0, true)
                .OrderBy(1, true);

            if (responsibleID != Guid.Empty)
                sqlSubQuery.Where(Exp.Eq("tbl_task.responsible_id", responsibleID));

            var sqlQuery = new SqlQuery()
                .Select("tbl_stat.*", "tbl_task_category.title")
                .From(sqlSubQuery, "tbl_stat")
                .LeftOuterJoin("crm_list_item tbl_task_category", Exp.EqColumns("tbl_stat.category_id", "tbl_task_category.id"));

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);
            }
            throw new NotImplementedException();

        }

        public XDocument BuildCasesReportTemp()
        {
            var sqlQuery = Query("crm_case");



            throw new NotImplementedException();
        }

       
        //public ReportWrapper BuildWorkLoadReport(Guid responsibleID)
        //{
        //    var sqlSubQuery =
        //         Query("crm_task")
        //        .Select("responsible_id,category_id, is_closed, count(*) as total")
        //        .GroupBy(0, 1, 2)
        //        .OrderBy(0, true)
        //        .OrderBy(1, true);

        //    if (responsibleID != Guid.Empty)
        //        sqlSubQuery.Where(Exp.Eq("tbl_task.responsible_id", responsibleID));

        //    var sqlQuery = new SqlQuery()
        //        .Select("tbl_stat.*", "tbl_task_category.title")
        //        .From(sqlSubQuery, "tbl_stat")
        //        .LeftOuterJoin("crm_list_item tbl_task_category", Exp.EqColumns("tbl_stat.category_id", "tbl_task_category.id"));

        //    var sqlResult = DbManager.ExecuteList(sqlQuery);

        //    var report = new ReportWrapper
        //    {
        //        ReportTitle = CRMReportResource.Report_WorkLoad_Title,
        //        ReportDescription = CRMReportResource.Report_WorkLoad_Description
        //    };


        //    //var responsibleStr = String.Empty;

        //    //if (responsibleID != Guid.Empty)
        //    //    responsibleStr = ASC.Core.CoreContext.UserManager.GetUsers(responsibleID).DisplayUserName();

        //    //var xDocument = new XDocument(
        //    //  new XElement("report",
        //    //   new XElement("metadata",
        //    //        new XElement("filters", new XElement("filter", new XAttribute("name", "responsible"), responsibleStr, new XAttribute("title", CRMCommonResource.Responsible))),
        //    //        new XElement("description", CRMReportResource.Report_WorkLoad_Description, new XAttribute("title", CRMReportResource.Report_WorkLoad_Title))
        //    //     ),
        //    //    new XElement("content",
        //    //    new XElement("columns",
        //    //        new XElement("column", new XAttribute("name", "user"), ""),
        //    //        new XElement("column", new XAttribute("name", "category"), CRMDealResource.Stage),
        //    //        new XElement("column", new XAttribute("name", "total"), CRMDealResource.DealAmount)
        //    //                ),
        //    //    new XElement("rows",
        //    //    sqlResult.ConvertAll(row =>
        //    //                               new XElement("row",
        //    //                                           new XElement("userID", row[0].ToString()),
        //    //                                           new XElement("userDisplayName", ASC.Core.CoreContext.UserManager.GetUsers(new Guid(row[0].ToString()))),
        //    //                                           new XElement("percent", row[4]))

        //    //        ).ToArray()))
        //    // ));

        //    throw new NotImplementedException();

        //}

        public List<object[]> BuildTasksReport(
            Guid responsibleID,
            DateTime fromDate,
            DateTime toDate,
            bool? isClosed,
            bool showWithoutResponsible)
        {

            var sqlQuery = Query("crm_task tbl_task");

            if (responsibleID != Guid.Empty)
                sqlQuery.Where(Exp.Eq("tbl_task.responsible_id", responsibleID));

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Between("tbl_task.deadline", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate.AddDays(1).AddHours(-1))));
            else if (fromDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Ge("tbl_task.deadline", TenantUtil.DateTimeToUtc(fromDate)));
            else if (toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Le("tbl_task.deadline", TenantUtil.DateTimeToUtc(toDate)));

            if (!showWithoutResponsible)
                sqlQuery.Where(!Exp.Eq("tbl_task.responsible_id", Guid.Empty));

            if (isClosed.HasValue)
                sqlQuery.Where(Exp.Le("tbl_task.is_closed", isClosed.Value));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery);
            }
        }

        public XDocument BuildSalesByStageReport(Guid responsibleID, DateTime fromDate, DateTime toDate)
        {

            var sqlQuery = new SqlQuery("crm_deal_milestone tbl_list")
                          .LeftOuterJoin("crm_deal tbl_deal", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                          .Select("tbl_list.title",
                                  "bid_currency",
                                 @"sum(CASE tbl_deal.bid_type
                                   WHEN 0 THEN
                                     (tbl_deal.bid_value)
                                   ELSE
                                     (tbl_deal.bid_value * tbl_deal.per_period_value)
                                   END) AS bid_value",
                                  "count(tbl_deal.id) as count",
                                  "tbl_list.color")
                          .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID))
                          .GroupBy("tbl_list.id", "tbl_deal.bid_currency")
                          .OrderBy("tbl_list.sort_order", true);

            if (responsibleID != Guid.Empty)
                sqlQuery.Where(Exp.Eq("tbl_deal.responsible_id", responsibleID));

            if (fromDate > DateTime.MinValue && toDate > DateTime.MinValue)
                sqlQuery.Where(Exp.Ge("tbl_deal.expected_close_date", fromDate) & Exp.Le("tbl_deal.expected_close_date", toDate));

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0)
                    return new XDocument();

                var result = sqlResult.GroupBy(row =>
                                                    {
                                                        var bidCurrency = Convert.ToString(row[1]);

                                                        if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation)
                                                            row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                                                        return row[0];

                                                    }).Select(group => new[]
                                                                       {
                                                                          group.Key,
                                                                          group.Sum(p => Convert.ToDecimal(p[2])),
                                                                          group.Sum(p => Convert.ToDecimal(p[3])),
                                                                          group.First()[4],
                                                                          0
                                                                       }).ToList();

                var totalDeals = result.Sum(row => Convert.ToInt32(row[2]));

                if (totalDeals == 0)
                    return new XDocument();

                for (int index = 0; index < result.Count; index++)
                {

                    result[index][4] = Convert.ToInt32(result[index][2]) * 100 / totalDeals;
                    result[index][2] = result.Skip(index).Sum(row => Convert.ToInt32(row[2]));

                }

                String fromDateStr = String.Empty;

                if (fromDate != DateTime.MinValue)
                    fromDateStr = fromDate.ToShortDateString();

                String toDateStr = String.Empty;

                if (toDate != DateTime.MinValue)
                    toDateStr = toDate.ToShortDateString();

                String responsibleStr = String.Empty;

                if (responsibleID != Guid.Empty)
                    responsibleStr = ASC.Core.CoreContext.UserManager.GetUsers(ASC.Core.SecurityContext.CurrentAccount.ID).DisplayUserName();

                var xDocument = new XDocument(
                new XElement("report",
                   new XElement("metadata",
                        new XElement("filters",
                                                new XElement("filter", new XAttribute("name", "fromDate"), fromDateStr, new XAttribute("title", CRMCommonResource.From)),
                                                new XElement("filter", new XAttribute("name", "toDate"), toDateStr, new XAttribute("title", CRMCommonResource.To)),
                                                new XElement("filter", new XAttribute("name", "responsible"), responsibleStr, new XAttribute("title", CRMCommonResource.Responsible))
                            ),
                        new XElement("totalDeals", totalDeals),
                        new XElement("description", CRMReportResource.Report_SalesByStage_Description, new XAttribute("title", CRMCommonResource.Description))
                     ),
                    new XElement("content",
                    new XElement("columns",
                        new XElement("column", new XAttribute("name", "color"), ""),
                        new XElement("column", new XAttribute("name", "title"), CRMDealResource.DealMilestone),
                        new XElement("column", new XAttribute("name", "amount"), CRMDealResource.DealAmount),
                        new XElement("column", new XAttribute("name", "count"), CRMDealResource.DealCount),
                        new XElement("column", new XAttribute("name", "percent"), CRMCommonResource.Part)),
                    new XElement("rows",
                    result.ConvertAll(row =>
                                               new XElement("row",
                                                           new XElement("title", row[0]),
                                                           new XElement("amount", row[1]),
                                                           new XElement("count", row[2]),
                                                           new XElement("color", row[3]),
                                                           new XElement("percent", row[4]))

                        ).ToArray()))
                 )
               );

                InsertCommonMetadata(xDocument);

                return xDocument;
            }
        }

        public XDocument BuildSalesByContactReport(DateTime fromDate, DateTime toDate)
        {
            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
                .Select("tbl_deal.contact_id",
                        "tbl_deal.bid_currency",
                        @"sum(CASE tbl_deal.bid_type
                           WHEN 0 THEN
                             (tbl_deal.bid_value)
                           ELSE
                             (tbl_deal.bid_value * tbl_deal.per_period_value)
                           END) AS bid_value",
                        "count(tbl_deal.id) as count")
                .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
                       Exp.Between("tbl_deal.actual_close_date", fromDate, toDate) &
                       !Exp.Eq("tbl_deal.bid_value", 0) &
                       !Exp.Eq("tbl_deal.contact_id", 0) &
                       Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.ClosedAndWon))
                .GroupBy("tbl_deal.contact_id", "tbl_deal.bid_currency");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0)
                    return new XDocument();

                sqlResult.ForEach(row => row[0] = Global.DaoFactory.GetContactDao().GetByID((int)row[0]).GetTitle());

                var xDocument = BuildSalesReport(sqlResult, Guid.Empty, fromDate, toDate);

                xDocument.Root.XPathSelectElement("content/columns/column[@name='title']").Value = CRMContactResource.Contact;
                xDocument.Root.XPathSelectElement("metadata/description").Value = CRMReportResource.Report_SalesByContact_Description;

                return xDocument;
            }
        }

        public XDocument BuildSalesByManagerReport(DateTime fromDate, DateTime toDate)
        {
            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
       .Select("tbl_deal.responsible_id",
               "tbl_deal.bid_currency",
               @"sum(CASE tbl_deal.bid_type
                           WHEN 0 THEN
                             (tbl_deal.bid_value)
                           ELSE
                             (tbl_deal.bid_value * tbl_deal.per_period_value)
                           END) AS bid_value",
               "count(tbl_deal.id) as count")
       .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
       .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
              Exp.Between("tbl_deal.actual_close_date", fromDate, toDate) &
              !Exp.Eq("tbl_deal.bid_value", 0) &
              Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.ClosedAndWon))
       .GroupBy("tbl_deal.responsible_id", "tbl_deal.bid_currency");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0)
                    return new XDocument();

                sqlResult.ForEach(row => row[0] = ASC.Core.CoreContext.UserManager.GetUsers(new Guid(row[0].ToString())).DisplayUserName());

                var xDocument = BuildSalesReport(sqlResult, Guid.Empty, fromDate, toDate);

                xDocument.Root.XPathSelectElement("content/columns/column[@name='title']").Value = CRMDealResource.ResponsibleDeal;
                xDocument.Root.XPathSelectElement("metadata/description").Value = "";

                return xDocument;
            }
        }

        public XDocument BuildSalesByMonthReport(Guid responsibleID, DateTime fromDate, DateTime toDate)
        {

            var sqlQuery = new SqlQuery("crm_deal tbl_deal")
                        .Select("MONTH(tbl_deal.actual_close_date) as acdMonth",
                                "tbl_deal.bid_currency",
                                @"sum(CASE tbl_deal.bid_type
                                           WHEN 0 THEN
                                             (tbl_deal.bid_value)
                                           ELSE
                                             (tbl_deal.bid_value * tbl_deal.per_period_value)
                                           END) AS bid_value",
                                "count(tbl_deal.id) as count")
                        .LeftOuterJoin("crm_deal_milestone tbl_list", Exp.EqColumns("tbl_deal.deal_milestone_id", "tbl_list.id"))
                        .Where(Exp.Eq("tbl_list.tenant_id", TenantID) & Exp.Eq("tbl_deal.tenant_id", TenantID) &
                               Exp.Between("tbl_deal.actual_close_date", fromDate, toDate) &
                               !Exp.Eq("tbl_deal.bid_value", 0) &
                               Exp.Eq("tbl_list.status", (int)DealMilestoneStatus.ClosedAndWon))
                        .GroupBy("acdMonth", "tbl_deal.bid_currency");

            if (responsibleID != Guid.Empty)
                sqlQuery.Where(Exp.Eq("tbl_deal.responsible_id", responsibleID));

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                if (sqlResult.Count == 0)
                    return new XDocument();

                var result = sqlResult.GroupBy(row =>
                {
                    var bidCurrency = Convert.ToString(row[1]);

                    if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation)
                        row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                    return row[0];

                }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

                var totalDeals = result.Sum(row => Convert.ToInt32(row[2]));

                if (totalDeals == 0)
                    return new XDocument();

                foreach (var item in result)
                    item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;

            }
            throw new NotImplementedException();

        }


        private void BuildSalesReport(ref Report report,
                                     IEnumerable<object[]> reportData, Guid responsibleID,
                                     DateTime fromDate,
                                     DateTime toDate)
        {

            report.Lables = new List<String>
                                {
                                    CRMDealResource.DealAmount,
                                    CRMDealResource.DealCount,
                                    CRMCommonResource.Part
                                };

            var fromDateStr = String.Empty;

            if (fromDate != DateTime.MinValue)
                fromDateStr = fromDate.ToShortDateString();


            var toDateStr = String.Empty;

            if (toDate != DateTime.MinValue)
                toDateStr = toDate.ToShortDateString();

            var responsibleStr = String.Empty;

            if (responsibleID != Guid.Empty)
                responsibleStr = ASC.Core.CoreContext.UserManager.GetUsers(ASC.Core.SecurityContext.CurrentAccount.ID).DisplayUserName();


            var result = reportData.GroupBy(row =>
            {
                var bidCurrency = Convert.ToString(row[1]);

                if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation)
                    row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                return row[0];

            }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

            var totalDeals = result.Sum(row => Convert.ToInt32(row[2]));

            foreach (var item in result)
                item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;
                       

            report.Data = result.ConvertAll(row => new
                                                       {
                                                           title = row[0],
                                                           amount = row[1],
                                                           count = row[2],
                                                           percent = row[3]
                                                       });

        }


        private XDocument BuildSalesReport(IEnumerable<object[]> reportData, Guid responsibleID,
                                          DateTime fromDate,
                                          DateTime toDate)
        {


            var fromDateStr = String.Empty;

            if (fromDate != DateTime.MinValue)
                fromDateStr = fromDate.ToShortDateString();


            var toDateStr = String.Empty;

            if (toDate != DateTime.MinValue)
                toDateStr = toDate.ToShortDateString();

            var responsibleStr = String.Empty;

            if (responsibleID != Guid.Empty)
                responsibleStr = ASC.Core.CoreContext.UserManager.GetUsers(ASC.Core.SecurityContext.CurrentAccount.ID).DisplayUserName();


            var result = reportData.GroupBy(row =>
            {
                var bidCurrency = Convert.ToString(row[1]);

                if (bidCurrency != Global.TenantSettings.DefaultCurrency.Abbreviation)
                    row[2] = CurrencyProvider.MoneyConvertToDefaultCurrency(Convert.ToDecimal(row[2]), bidCurrency);

                return row[0];

            }).Select(group => new[]
                                   {
                                      group.Key,
                                      group.Sum(p => Convert.ToDecimal(p[2])),
                                      group.Sum(p => Convert.ToDecimal(p[3])),
                                      0
                                   }).ToList();

            var totalDeals = result.Sum(row => Convert.ToInt32(row[2]));

            if (totalDeals == 0)
                return new XDocument();

            foreach (var item in result)
                item[3] = Convert.ToInt32(item[2]) * 100 / totalDeals;

            var xDocument = new XDocument(
    new XDeclaration("1.0", "utf-8", "true"),
    new XElement("report",
                 new XElement("metadata",
                              new XElement("filters",
                                           new XElement("filter", new XAttribute("name", "fromDate"),
                                                        fromDateStr,
                                                        new XAttribute("title", CRMCommonResource.From)),
                                           new XElement("filter", new XAttribute("name", "toDate"),
                                                        toDateStr,
                                                        new XAttribute("title", CRMCommonResource.To)),
                                           new XElement("filter", new XAttribute("name", "responsible"), responsibleStr, new XAttribute("title", CRMCommonResource.Responsible))

                                  ),
                              new XElement("totalDeals", totalDeals),
                              new XElement("description", "",
                                           new XAttribute("title", CRMCommonResource.Description))
                     ),
                 new XElement("content",
                              new XElement("columns",
                                           new XElement("column", new XAttribute("name", "title"), ""),
                                           new XElement("column", new XAttribute("name", "amount"),
                                                        CRMDealResource.DealAmount),
                                           new XElement("column", new XAttribute("name", "count"),
                                                        CRMDealResource.DealCount),
                                           new XElement("column", new XAttribute("name", "percent"),
                                                        CRMCommonResource.Part)),
                 new XElement("rows",
                              result.ConvertAll(row =>
                                                new XElement("row",
                                                             new XElement("title", row[0]),
                                                             new XElement("amount", row[1]),
                                                             new XElement("count", row[2]),
                                                             new XElement("percent", row[3])
                                                    )

                                  ).ToArray()))));

            InsertCommonMetadata(xDocument);


            return xDocument;
        }

        private String GetXSLTMarking(ReportType reportType, ReportViewType reportView)
        {
            var folderPath = HttpContext.Current.Server.MapPath(@"~\products\crm\reports\");

            var filePath = String.Empty;

            switch (reportType)
            {
                case ReportType.SalesByStage:
                    filePath = String.Format("{0}/{1}.{2}.xsl", folderPath, reportType.ToString().ToLower(),
                                         reportView.ToString());
                    break;
                default:
                    filePath = String.Format("{0}/{1}.{2}.xsl", folderPath, "sales",
                                             reportView.ToString());
                    break;
            }


            return File.ReadAllText(filePath);
        }

        private XsltArgumentList GetXsltArgumentList(ReportType reportType, ReportViewType reportView)
        {
            var xsltArgumentList = new XsltArgumentList();

            switch (reportView)
            {
                case ReportViewType.Html:
                    xsltArgumentList.AddParam("decimalFormat", "", "### ###.00");
                    xsltArgumentList.AddParam("csvExportUrl", "", String.Format("reports.aspx?reportType={0}&View={1}",
                        reportType.ToString(),
                        "csv").ToLower());

                    break;
                default:
                    break;
            }

            return xsltArgumentList;

        }

        public String Transform(XDocument reportData, ReportType reportType, ReportViewType reportView)
        {

            var xsltTransformResult = new XDocument();

            using (var writer = xsltTransformResult.CreateWriter())
            {
                var xslt = new XslCompiledTransform();

                xslt.Load(XmlReader.Create(new StringReader(GetXSLTMarking(reportType, reportView))), null, new ReportXmlUrlResolver());

                xslt.Transform(reportData.CreateReader(), GetXsltArgumentList(reportType, reportView), writer);
            }

            return xsltTransformResult.ToString();
        }

        private void InsertCommonMetadata(XDocument xDocument)
        {

            xDocument.Declaration = new XDeclaration("1.0", "utf-8", "true");

            if (xDocument.Root == null) return;
            if (xDocument.Root.Element("metadata") == null) return;

            xDocument.Root.Element("metadata").Add(

                new XElement("defaultCurrency",
                             new XElement("abbreviation", Global.TenantSettings.DefaultCurrency.Abbreviation),
                             new XElement("symbol", Global.TenantSettings.DefaultCurrency.Symbol)),
                new XElement("resources", new XElement("total", CRMCommonResource.Total)),
                new XElement("images",
                             new XElement("image",
                                          new XAttribute("src", WebImageSupplier.GetAbsoluteWebPath("csv_16.png")),
                                          new XAttribute("name", "exportToCSV"),
                                          new XAttribute("title", CRMReportResource.ExportToCSV)
                                 ),
                             new XElement("image",
                                          new XAttribute("src", WebImageSupplier.GetAbsoluteWebPath("printer.gif")),
                                          new XAttribute("name", "printReport"),
                                          new XAttribute("title", CRMReportResource.PrintReport)
                                 ),
                             new XElement("image",
                                          new XAttribute("src", WebImageSupplier.GetAbsoluteWebPath("edit_small.png")),
                                          new XAttribute("name", "changeReport"),
                                          new XAttribute("title", CRMReportResource.ChangeFilterData)
                                 )
                    ));
        }
    }

    class ReportXmlUrlResolver : XmlUrlResolver
    {

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (baseUri != null)
                return base.ResolveUri(baseUri, relativeUri);


            return new Uri(String.Concat(CommonLinkUtility.GetFullAbsolutePath("~/products/crm/reports/"), relativeUri).ToLower());

        }
    }


}