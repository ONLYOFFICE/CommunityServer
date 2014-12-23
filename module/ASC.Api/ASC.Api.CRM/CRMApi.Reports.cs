/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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