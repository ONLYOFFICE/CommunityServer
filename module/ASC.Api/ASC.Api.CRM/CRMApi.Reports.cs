/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Core.Tenants;
using ASC.Specific;
using ASC.Web.CRM.Resources;

namespace ASC.Api.CRM
{
    public class ReportFilter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public partial class CRMApi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="isCompany"></param>
        /// <param name="tagName"></param>
        /// <param name="contactType"></param>
        /// <returns></returns>
        /// <visible>false</visible>
        [Read(@"report/contactpopulate")]
        public ReportWrapper BuildContactPopulateReport(
            ApiDateTime fromDate,
            ApiDateTime toDate,
            bool? isCompany,
            string tagName,
            int contactType)
        {
            var reportData = DaoFactory.GetReportDao().BuildContactPopulateReport(fromDate, toDate, isCompany, tagName, contactType);
            var report = new ReportWrapper
                {
                    ReportTitle = CRMReportResource.Report_ContactPopulate_Title,
                    ReportDescription = CRMReportResource.Report_ContactPopulate_Description,
                    Data = reportData.ConvertAll(row => new
                        {
                            Date = (ApiDateTime)TenantUtil.DateTimeFromUtc(DateTime.Parse(Convert.ToString(row[0]))),
                            Count = row[1]
                        }),
                    Lables = new List<string>
                        {
                            CRMCommonResource.Date,
                            CRMCommonResource.Count
                        }
                };

            return report;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responsibleID"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        /// <visible>false</visible>
        [Read(@"report/salesforecastbymonth")]
        public ReportWrapper BuildSalesForecastByMonthReport(Guid responsibleID, DateTime fromDate, DateTime toDate)
        {
            var reportData = DaoFactory.GetReportDao().
                                        BuildSalesForecastByMonthReport(
                                            responsibleID,
                                            fromDate,
                                            toDate
                );

            var report = new ReportWrapper
                {
                    ReportTitle = CRMReportResource.Report_SalesForecastByMonth_Title,
                    ReportDescription = CRMReportResource.Report_SalesForecastByMonth_Description,
                    Data = reportData.ConvertAll(row => new
                        {
                            month = row[0],
                            amount = row[1],
                            count = row[2],
                            percent = row[3]
                        }),
                    Lables = new List<string>
                        {
                            CRMCommonResource.Date,
                            CRMCommonResource.Count
                        }
                };

            return report;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        /// <visible>false</visible>
        [Read(@"report/salesforecastbyclient")]
        public ReportWrapper BuildSalesForecastByClientReport(DateTime fromDate, DateTime toDate)
        {
            var reportData = DaoFactory.GetReportDao().BuildSalesForecastByClientReport(fromDate, toDate);
            var report = new ReportWrapper
                {
                    ReportTitle = CRMReportResource.Report_SalesForecastByMonth_Title,
                    ReportDescription = CRMReportResource.Report_SalesForecastByMonth_Description,
                    Data = reportData.ConvertAll(row => new
                        {
                            client = row[0],
                            amount = row[1],
                            count = row[2],
                            percent = row[3]
                        }),
                    Lables = new List<string>
                        {
                            CRMCommonResource.Date,
                            CRMCommonResource.Count
                        }
                };

            return report;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        /// <visible>false</visible>
        [Read(@"report/salesforecastbymanager")]
        public ReportWrapper BuildSalesForecastByManagerReport(DateTime fromDate, DateTime toDate)
        {
            var reportData = DaoFactory.GetReportDao().BuildSalesForecastByManagerReport(fromDate, toDate);
            var report = new ReportWrapper
                {
                    ReportTitle = CRMReportResource.Report_SalesForecastByMonth_Title,
                    ReportDescription = CRMReportResource.Report_SalesForecastByMonth_Description,
                    Data = reportData.ConvertAll(row => new
                        {
                            manager = row[0],
                            amount = row[1],
                            count = row[2],
                            percent = row[3]
                        }),
                    Lables = new List<string>
                        {
                            CRMCommonResource.Date,
                            CRMCommonResource.Count
                        }
                };

            return report;
        }
    }
}