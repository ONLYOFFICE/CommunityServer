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


#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.S3;
using ASC.VoipService;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.CRM.Core.Entities;
using ASC.Web.Files.Api;


#endregion

namespace ASC.CRM.Core.Dao
{
    public class ReportDao : AbstractDao
    {
        const string TimeFormat = "[h]:mm:ss;@";
        const string ShortDateFormat = "M/d/yyyy";

        private DaoFactory DaoFactory {get; set; }

        #region Constructor

        public ReportDao(int tenantID, DaoFactory daoFactory)
            : base(tenantID)
        {
            this.DaoFactory = daoFactory;
        }

        #endregion


        #region Common Methods

        private static void GetTimePeriod(ReportTimePeriod timePeriod, out DateTime fromDate, out DateTime toDate)
        {
            var now = TenantUtil.DateTimeNow().Date;

            var diff = (int)now.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            var quarter = (now.Month + 2) / 3;

            var year = now.Year;

            switch (timePeriod)
            {
                case ReportTimePeriod.Today:
                    fromDate = now;
                    toDate = now.AddDays(1).AddSeconds(-1);
                    break;
                case ReportTimePeriod.Yesterday:
                    fromDate = now.AddDays(-1);
                    toDate = now.AddSeconds(-1);
                    break;
                case ReportTimePeriod.Tomorrow:
                    fromDate = now.AddDays(1);
                    toDate = now.AddDays(2).AddSeconds(-1);
                    break;
                case ReportTimePeriod.CurrentWeek:
                    fromDate = now.AddDays(-1 * diff);
                    toDate = now.AddDays(-1 * diff + 7).AddSeconds(-1);
                    break;
                case ReportTimePeriod.PreviousWeek:
                    fromDate = now.AddDays(-1 * diff - 7);
                    toDate = now.AddDays(-1 * diff).AddSeconds(-1);
                    break;
                case ReportTimePeriod.NextWeek:
                    fromDate = now.AddDays(-1 * diff + 7);
                    toDate = now.AddDays(-1 * diff + 14).AddSeconds(-1);
                    break;
                case ReportTimePeriod.CurrentMonth:
                    fromDate = new DateTime(now.Year, now.Month, 1);
                    toDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1);
                    break;
                case ReportTimePeriod.PreviousMonth:
                    toDate = new DateTime(now.Year, now.Month, 1).AddSeconds(-1);
                    fromDate = new DateTime(toDate.Year, toDate.Month, 1);
                    break;
                case ReportTimePeriod.NextMonth:
                    fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                    toDate = new DateTime(now.Year, now.Month, 1).AddMonths(2).AddSeconds(-1);
                    break;
                case ReportTimePeriod.CurrentQuarter:
                    fromDate = new DateTime(now.Year, quarter * 3 - 2, 1);
                    toDate = new DateTime(now.Year, fromDate.Month, 1).AddMonths(3).AddSeconds(-1);
                    break;
                case ReportTimePeriod.PreviousQuarter:
                    quarter--;
                    if (quarter == 0)
                    {
                        year--;
                        quarter = 4;
                    }
                    fromDate = new DateTime(year, quarter * 3 - 2, 1);
                    toDate = new DateTime(year, fromDate.Month, 1).AddMonths(3).AddSeconds(-1);
                    break;
                case ReportTimePeriod.NextQuarter:
                    quarter++;
                    if (quarter == 5)
                    {
                        year++;
                        quarter = 1;
                    }
                    fromDate = new DateTime(year, quarter * 3 - 2, 1);
                    toDate = new DateTime(year, fromDate.Month, 1).AddMonths(3).AddSeconds(-1);
                    break;
                case ReportTimePeriod.CurrentYear:
                    fromDate = new DateTime(now.Year, 1, 1);
                    toDate = new DateTime(now.Year, 1, 1).AddYears(1).AddSeconds(-1);
                    break;
                case ReportTimePeriod.PreviousYear:
                    toDate = new DateTime(now.Year, 1, 1).AddSeconds(-1);
                    fromDate = new DateTime(toDate.Year, 1, 1);
                    break;
                case ReportTimePeriod.NextYear:
                    fromDate = new DateTime(now.Year, 1, 1).AddYears(1);
                    toDate = new DateTime(now.Year, 1, 1).AddYears(2).AddSeconds(-1);
                    break;
                case ReportTimePeriod.DuringAllTime:
                    fromDate = DateTime.MinValue;
                    toDate = DateTime.MaxValue;
                    break;
                default:
                    fromDate = DateTime.MinValue;
                    toDate = DateTime.MinValue;
                    break;
            }
        }

        private static string GetTimePeriodText(ReportTimePeriod timePeriod)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            switch (timePeriod)
            {
                case ReportTimePeriod.Today:
                case ReportTimePeriod.Yesterday:
                case ReportTimePeriod.Tomorrow:
                    return fromDate.ToShortDateString();
                case ReportTimePeriod.CurrentWeek:
                case ReportTimePeriod.PreviousWeek:
                case ReportTimePeriod.NextWeek:
                    return string.Format("{0}-{1}", fromDate.ToShortDateString(), toDate.ToShortDateString());
                case ReportTimePeriod.CurrentMonth:
                case ReportTimePeriod.PreviousMonth:
                case ReportTimePeriod.NextMonth:
                    return fromDate.ToString("Y");
                case ReportTimePeriod.CurrentQuarter:
                case ReportTimePeriod.PreviousQuarter:
                case ReportTimePeriod.NextQuarter:
                    return string.Format("{0}-{1}", fromDate.ToString("Y"), toDate.ToString("Y"));
                case ReportTimePeriod.CurrentYear:
                case ReportTimePeriod.PreviousYear:
                case ReportTimePeriod.NextYear:
                    return fromDate.Year.ToString(CultureInfo.InvariantCulture);
                case ReportTimePeriod.DuringAllTime:
                    return CRMReportResource.DuringAllTime;
                default:
                    return string.Empty;
            }
        }

        public List<string> GetMissingRates(string defaultCurrency)
        {
            var existingRatesQuery = Query("crm_currency_rate r")
                .Select("distinct r.from_currency")
                .Where("r.to_currency", defaultCurrency);

            var missingRatesQuery = Query("crm_deal d")
                .Select("distinct d.bid_currency")
                .Where(!Exp.Eq("d.bid_currency", defaultCurrency))
                .Where(!Exp.In("d.bid_currency", existingRatesQuery));


            return Db.ExecuteList(missingRatesQuery).ConvertAll(row => row[0].ToString());
        }

        #endregion


        #region Report Files

        public List<Files.Core.File> SaveSampleReportFiles()
        {
            var result = new List<Files.Core.File>();

            var storeTemplate = Global.GetStoreTemplate();

            if (storeTemplate == null) return result;

            var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture() ??
                          CoreContext.TenantManager.GetCurrentTenant().GetCulture();

            var path = culture + "/";

            if (!storeTemplate.IsDirectory(path))
            {
                path = "default/";
                if (!storeTemplate.IsDirectory(path)) return result;
            }

            foreach (var filePath in storeTemplate.ListFilesRelative("", path, "*", false).Select(x => path + x))
            {
                using (var stream = storeTemplate.GetReadStream("", filePath))
                {
                    var document = new Files.Core.File
                        {
                            Title = Path.GetFileName(filePath),
                            FolderID = DaoFactory.FileDao.GetRoot(),
                            ContentLength = stream.Length
                        };

                    var file = DaoFactory.FileDao.SaveFile(document, stream);

                    SaveFile((int) file.ID, -1);

                    result.Add(file);
                }
            }

            return result;
        }

        public List<Files.Core.File> GetFiles()
        {
            return GetFiles(SecurityContext.CurrentAccount.ID);
        }

        public List<Files.Core.File> GetFiles(Guid userId)
        {
            var query = Query("crm_report_file")
                .Select("file_id")
                .Where("create_by", userId);

            using (var filedao = FilesIntegration.GetFileDao())
            {
                var fileIds = Db.ExecuteList(query).ConvertAll(row => row[0]).ToArray();
                return fileIds.Length > 0 ? filedao.GetFiles(fileIds) : new List<Files.Core.File>();
            }
        }

        public List<int> GetFileIds(Guid userId)
        {
            var query = Query("crm_report_file")
                .Select("file_id")
                .Where("create_by", userId);

            return Db.ExecuteList(query).ConvertAll(row => Convert.ToInt32(row[0]));
        }

        public Files.Core.File GetFile(int fileid)
        {
            return GetFile(fileid, SecurityContext.CurrentAccount.ID);
        }

        public Files.Core.File GetFile(int fileid, Guid userId)
        {
            var query = Query("crm_report_file")
                .SelectCount()
                .Where("file_id", fileid)
                .Where("create_by", userId);

            using (var filedao = FilesIntegration.GetFileDao())
            {
                return Db.ExecuteScalar<int>(query) > 0 ? filedao.GetFile(fileid) : null;
            }
        }

        public void DeleteFile(int fileid)
        {
            var query = Delete("crm_report_file")
                .Where("file_id", fileid)
                .Where("create_by", SecurityContext.CurrentAccount.ID);

            using (var filedao = FilesIntegration.GetFileDao())
            {
                Db.ExecuteNonQuery(query);
                filedao.DeleteFile(fileid);
            }
        }

        public void DeleteFiles(Guid userId)
        {
            var fileIds = GetFileIds(userId);

            var query = Delete("crm_report_file")
                .Where("create_by", userId);

            using (var filedao = FilesIntegration.GetFileDao())
            {
                Db.ExecuteNonQuery(query);

                foreach (var fileId in fileIds)
                {
                    filedao.DeleteFile(fileId);
                }
            }
        }

        public void SaveFile(int fileId, int reportType)
        {
            Db.ExecuteScalar<int>(
                Insert("crm_report_file")
                .InColumnValue("file_id", fileId)
                .InColumnValue("report_type", reportType)
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                .Identity(1, 0, true));
        }

        #endregion


        #region SalesByManagersReport

        public bool CheckSalesByManagersReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal d")
                .Select("d.id")
                .LeftOuterJoin("crm_deal_milestone m", Exp.EqColumns("m.id", "d.deal_milestone_id") & Exp.EqColumns("m.tenant_id", "d.tenant_id"))
                .Where("m.status", (int)DealMilestoneStatus.ClosedAndWon)
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetSalesByManagersReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildSalesByManagersReport(timePeriod, managers, defaultCurrency);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<SalesByManager> BuildSalesByManagersReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            string dateSelector;
            
            switch (timePeriod)
            {
                case ReportTimePeriod.Today:
                case ReportTimePeriod.Yesterday:
                    dateSelector = "date_add(date(d.actual_close_date), interval extract(hour from d.actual_close_date) hour) as close_date";
                    break;
                case ReportTimePeriod.CurrentWeek:
                case ReportTimePeriod.PreviousWeek:
                case ReportTimePeriod.CurrentMonth:
                case ReportTimePeriod.PreviousMonth:
                    dateSelector = "date(d.actual_close_date) as close_date";
                    break;
                case ReportTimePeriod.CurrentQuarter:
                case ReportTimePeriod.PreviousQuarter:
                case ReportTimePeriod.CurrentYear:
                case ReportTimePeriod.PreviousYear:
                    dateSelector = "date_sub(date(d.actual_close_date), interval (extract(day from d.actual_close_date) - 1) day) as close_date";
                    break;
                default:
                    return null;
            }

            var sqlQuery = Query("crm_deal d")
                .Select("d.responsible_id",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        string.Format(@"sum((case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                        end)) as bid_value", defaultCurrency),
                        dateSelector)
                .LeftOuterJoin("crm_deal_milestone m", Exp.EqColumns("m.id", "d.deal_milestone_id") & Exp.EqColumns("m.tenant_id", "d.tenant_id"))
                .LeftOuterJoin("crm_currency_rate r", Exp.EqColumns("r.tenant_id", "d.tenant_id") & Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "d.tenant_id") & Exp.EqColumns("u.id", "d.responsible_id"))
                .Where("m.status", (int) DealMilestoneStatus.ClosedAndWon)
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("responsible_id", "close_date");


            return Db.ExecuteList(sqlQuery).ConvertAll(ToSalesByManagers);
        }

        private static SalesByManager ToSalesByManagers(object[] row)
        {
            return new SalesByManager
            {
                UserId = string.IsNullOrEmpty(Convert.ToString(row[0])) ? Guid.Empty : new Guid(Convert.ToString(row[0])),
                UserName = Convert.ToString(row[1]),
                Value = Convert.ToDecimal(row[2]),
                Date = Convert.ToDateTime(row[3]) == DateTime.MinValue ? DateTime.MinValue : TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3]))
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<SalesByManager> data)
        {
            switch (timePeriod)
            {
                case ReportTimePeriod.Today:
                case ReportTimePeriod.Yesterday:
                    return GenerateReportDataByHours(timePeriod, data);
                case ReportTimePeriod.CurrentWeek:
                case ReportTimePeriod.PreviousWeek:
                case ReportTimePeriod.CurrentMonth:
                case ReportTimePeriod.PreviousMonth:
                    return GenerateReportDataByDays(timePeriod, data);
                case ReportTimePeriod.CurrentQuarter:
                case ReportTimePeriod.PreviousQuarter:
                case ReportTimePeriod.CurrentYear:
                case ReportTimePeriod.PreviousYear:
                    return GenerateReportByMonths(timePeriod, data);
                default:
                    return null;
            }
        }

        private static object GenerateReportDataByHours(ReportTimePeriod timePeriod, List<SalesByManager> data)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var res = new Dictionary<Guid, Dictionary<DateTime, decimal>>();

            var users = data.Select(x => x.UserId).Distinct().ToList();

            foreach (var userId in users)
            {
                var date = fromDate;

                while (date < toDate)
                {
                    if (res.ContainsKey(userId))
                    {
                        res[userId].Add(date, 0);
                    }
                    else
                    {
                        res.Add(userId, new Dictionary<DateTime, decimal> {{date, 0}});
                    }

                    date = date.AddHours(1);
                }
            }

            foreach (var item in data)
            {
                var itemDate = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, item.Date.Hour, 0, 0);

                if (itemDate < res[item.UserId].First().Key)
                    itemDate = res[item.UserId].First().Key;

                if (itemDate > res[item.UserId].Last().Key)
                    itemDate = res[item.UserId].Last().Key;

                res[item.UserId][itemDate] += item.Value;
            }

            var body = new List<List<object>>();

            foreach (var resItem in res)
            {
                var bodyItem = new List<object>
                    {
                        data.First(x => x.UserId == resItem.Key).UserName
                    };

                bodyItem.AddRange(resItem.Value.Select(x => new { format = "0.00", value = x.Value.ToString(CultureInfo.InvariantCulture) }));

                body.Add(bodyItem);
            }

            var head = new List<object>();

            foreach (var key in res.First().Value.Keys)
            {
                head.Add(new {format = "H:mm", value = key.ToShortTimeString()});
            }

            return new
                {
                    resource = new
                        {
                            manager = CRMReportResource.Manager,
                            summary = CRMReportResource.Sum,
                            total = CRMReportResource.Total,
                            dateRangeLabel = CRMReportResource.TimePeriod + ":",
                            dateRangeValue = GetTimePeriodText(timePeriod),
                            sheetName = CRMReportResource.SalesByManagersReport,
                            header = CRMReportResource.SalesByManagersReport,
                            header1 = CRMReportResource.SalesByHour + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            header2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            chartName1 = CRMReportResource.SalesByHour + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            chartName2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol
                        },
                    thead = head,
                    tbody = body
                };
        }

        private static object GenerateReportDataByDays(ReportTimePeriod timePeriod, List<SalesByManager> data)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var res = new Dictionary<Guid, Dictionary<DateTime, decimal>>();

            var users = data.Select(x => x.UserId).Distinct().ToList();

            foreach (var userId in users)
            {
                var date = fromDate;

                while (date < toDate)
                {
                    if (res.ContainsKey(userId))
                    {
                        res[userId].Add(date, 0);
                    }
                    else
                    {
                        res.Add(userId, new Dictionary<DateTime, decimal> { { date, 0 } });
                    }

                    date = date.AddDays(1);
                }
            }

            foreach (var item in data)
            {
                var itemDate = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day);

                if (itemDate < res[item.UserId].First().Key)
                    itemDate = res[item.UserId].First().Key;

                if (itemDate > res[item.UserId].Last().Key)
                    itemDate = res[item.UserId].Last().Key;

                res[item.UserId][itemDate] += item.Value;
            }

            var body = new List<List<object>>();

            foreach (var resItem in res)
            {
                var bodyItem = new List<object>
                    {
                        data.First(x => x.UserId == resItem.Key).UserName
                    };

                bodyItem.AddRange(resItem.Value.Select(x => new { format = "0.00", value = x.Value.ToString(CultureInfo.InvariantCulture) }));

                body.Add(bodyItem);
            }

            var head = new List<object>();
            var separator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.ToCharArray();
            var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("yyyy", string.Empty).Trim(separator);

            foreach (var key in res.First().Value.Keys)
            {
                head.Add(new { format = pattern, value = key.ToString(ShortDateFormat, CultureInfo.InvariantCulture) });
            }

            return new
            {
                resource = new
                {
                    manager = CRMReportResource.Manager,
                    summary = CRMReportResource.Sum,
                    total = CRMReportResource.Total,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),
                    sheetName = CRMReportResource.SalesByManagersReport,
                    header = CRMReportResource.SalesByManagersReport,
                    header1 = CRMReportResource.SalesByDay + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    header2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName1 = CRMReportResource.SalesByDay + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol
                },
                thead = head,
                tbody = body
            };
        }

        private static object GenerateReportByMonths(ReportTimePeriod timePeriod, List<SalesByManager> data)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var res = new Dictionary<Guid, Dictionary<DateTime, decimal>>();

            var users = data.Select(x => x.UserId).Distinct().ToList();

            foreach (var userId in users)
            {
                var date = fromDate;

                while (date < toDate)
                {
                    if (res.ContainsKey(userId))
                    {
                        res[userId].Add(date, 0);
                    }
                    else
                    {
                        res.Add(userId, new Dictionary<DateTime, decimal> { { date, 0 } });
                    }

                    date = date.AddMonths(1);
                }
            }

            foreach (var item in data)
            {
                var itemDate = new DateTime(item.Date.Year, item.Date.Month, 1);

                if (itemDate < res[item.UserId].First().Key)
                    itemDate = res[item.UserId].First().Key;

                if (itemDate > res[item.UserId].Last().Key)
                    itemDate = res[item.UserId].Last().Key;

                res[item.UserId][itemDate] += item.Value;
            }

            var body = new List<List<object>>();

            foreach (var resItem in res)
            {
                var bodyItem = new List<object>
                    {
                        data.First(x => x.UserId == resItem.Key).UserName
                    };

                bodyItem.AddRange(resItem.Value.Select(x => new { format = "0.00", value = x.Value.ToString(CultureInfo.InvariantCulture) }));

                body.Add(bodyItem);
            }

            var head = new List<object>();

            foreach (var key in res.First().Value.Keys)
            {
                head.Add(new { format = "MMM-yy", value = key.ToString(ShortDateFormat, CultureInfo.InvariantCulture) });
            }

            return new
            {
                resource = new
                {
                    manager = CRMReportResource.Manager,
                    summary = CRMReportResource.Sum,
                    total = CRMReportResource.Total,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),
                    sheetName = CRMReportResource.SalesByManagersReport,
                    header = CRMReportResource.SalesByManagersReport,
                    header1 = CRMReportResource.SalesByMonth + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    header2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName1 = CRMReportResource.SalesByMonth + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName2 = CRMReportResource.TotalSalesByManagers + ", " + Global.TenantSettings.DefaultCurrency.Symbol
                },
                thead = head,
                tbody = body
            };
        }

        #endregion


        #region SalesForecastReport

        public bool CheckSalesForecastReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal d")
                .Select("d.id")
                .LeftOuterJoin("crm_deal_milestone m", Exp.EqColumns("m.tenant_id", "d.tenant_id") & Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.expected_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetSalesForecastReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildSalesForecastReport(timePeriod, managers, defaultCurrency);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<SalesForecast> BuildSalesForecastReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);          

            string dateSelector;

            switch (timePeriod)
            {
                case ReportTimePeriod.CurrentWeek:
                case ReportTimePeriod.NextWeek:
                case ReportTimePeriod.CurrentMonth:
                case ReportTimePeriod.NextMonth:
                    dateSelector = "d.expected_close_date as close_date";
                    break;
                case ReportTimePeriod.CurrentQuarter:
                case ReportTimePeriod.NextQuarter:
                case ReportTimePeriod.CurrentYear:
                case ReportTimePeriod.NextYear:
                    dateSelector = "date_sub(date(d.expected_close_date), interval (extract(day from d.expected_close_date) - 1) day) as close_date";
                    break;
                default:
                    return null;
            }

            var sqlQuery = Query("crm_deal d")
                .Select(string.Format(@"sum(case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                        end) as value", defaultCurrency),
                        string.Format(@"sum(case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate)) * d.deal_milestone_probability / 100
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate)) * d.deal_milestone_probability / 100
                        end) as value_with_probability", defaultCurrency),
                        dateSelector)
                .LeftOuterJoin("crm_deal_milestone m", Exp.EqColumns("m.tenant_id", "d.tenant_id") & Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .LeftOuterJoin("crm_currency_rate r", Exp.EqColumns("r.tenant_id", "d.tenant_id") & Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.expected_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("close_date");

            return Db.ExecuteList(sqlQuery).ConvertAll(ToSalesForecast);
        }

        private static SalesForecast ToSalesForecast(object[] row)
        {
            return new SalesForecast
            {
                Value = Convert.ToDecimal(row[0]),
                ValueWithProbability = Convert.ToDecimal(row[1]),
                Date = Convert.ToDateTime(row[2]) == DateTime.MinValue ? DateTime.MinValue : TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2]))
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<SalesForecast> data)
        {
            switch (timePeriod)
            {
                case ReportTimePeriod.CurrentWeek:
                case ReportTimePeriod.NextWeek:
                case ReportTimePeriod.CurrentMonth:
                case ReportTimePeriod.NextMonth:
                    return GenerateReportDataByDays(timePeriod, data);
                case ReportTimePeriod.CurrentQuarter:
                case ReportTimePeriod.NextQuarter:
                case ReportTimePeriod.CurrentYear:
                case ReportTimePeriod.NextYear:
                    return GenerateReportByMonths(timePeriod, data);
                default:
                    return null;
            }
        }

        private static object GenerateReportDataByDays(ReportTimePeriod timePeriod, List<SalesForecast> data)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var res = new Dictionary<DateTime, Tuple<decimal, decimal>>();

            var date = fromDate;

            while (date < toDate)
            {
                res.Add(date, new Tuple<decimal, decimal>(0, 0));
                date = date.AddDays(1);
            }

            foreach (var item in data)
            {
                var key = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day);

                if (key < res.First().Key)
                    key = res.First().Key;

                if (key > res.Last().Key)
                    key = res.Last().Key;
                
                res[key] = new Tuple<decimal, decimal>(res[key].Item1 + item.ValueWithProbability,
                                                       res[key].Item2 + item.Value);
            }

            var body = new List<List<object>>();
            var separator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.ToCharArray();
            var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("yyyy", string.Empty).Trim(separator);

            foreach (var resItem in res)
            {
                var bodyItem = new List<object>
                    {
                        new {format = pattern, value = resItem.Key.ToString(ShortDateFormat, CultureInfo.InvariantCulture)},
                        new {format = "0.00", value = resItem.Value.Item1.ToString(CultureInfo.InvariantCulture)},
                        new {format = "0.00", value = resItem.Value.Item2.ToString(CultureInfo.InvariantCulture)}
                    };

                body.Add(bodyItem);
            }

            var head = new List<object>
                {
                    CRMReportResource.Day,
                    CRMReportResource.WithRespectToProbability,
                    CRMReportResource.IfAllOpportunitiesWon
                };

            return new
                {
                    resource = new
                        {
                            total = CRMReportResource.Total,
                            dateRangeLabel = CRMReportResource.TimePeriod + ":",
                            dateRangeValue = GetTimePeriodText(timePeriod),
                            sheetName = CRMReportResource.SalesForecastReport,
                            header = CRMReportResource.SalesForecastReport,
                            header1 = CRMReportResource.SalesForecastReport + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            chartName = CRMReportResource.SalesForecastReport + ", " + Global.TenantSettings.DefaultCurrency.Symbol
                        },
                    thead = head,
                    tbody = body
                };
        }

        private static object GenerateReportByMonths(ReportTimePeriod timePeriod, List<SalesForecast> data)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var res = new Dictionary<DateTime, Tuple<decimal, decimal>>();

            var date = fromDate;

            while (date < toDate)
            {
                res.Add(date, new Tuple<decimal, decimal>(0, 0));
                date = date.AddMonths(1);
            }

            foreach (var item in data)
            {
                var key = new DateTime(item.Date.Year, item.Date.Month, 1);

                if (key < res.First().Key)
                    key = res.First().Key;

                if (key > res.Last().Key)
                    key = res.Last().Key;

                res[key] = new Tuple<decimal, decimal>(res[key].Item1 + item.ValueWithProbability,
                                                       res[key].Item2 + item.Value);
            }

            var body = new List<List<object>>();

            foreach (var resItem in res)
            {
                var bodyItem = new List<object>
                    {
                        new {format = "MMM-yy", value = resItem.Key.ToString(ShortDateFormat, CultureInfo.InvariantCulture)},
                        new {format = "0.00", value = resItem.Value.Item1.ToString(CultureInfo.InvariantCulture)},
                        new {format = "0.00", value = resItem.Value.Item2.ToString(CultureInfo.InvariantCulture)}
                    };

                body.Add(bodyItem);
            }

            var head = new List<object>
                {
                    CRMReportResource.Month,
                    CRMReportResource.WithRespectToProbability,
                    CRMReportResource.IfAllOpportunitiesWon
                };

            return new
                {
                    resource = new
                        {
                            total = CRMReportResource.Total,
                            dateRangeLabel = CRMReportResource.TimePeriod + ":",
                            dateRangeValue = GetTimePeriodText(timePeriod),
                            sheetName = CRMReportResource.SalesForecastReport,
                            header = CRMReportResource.SalesForecastReport,
                            header1 = CRMReportResource.SalesForecastReport + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            chartName = CRMReportResource.SalesForecastReport + ", " + Global.TenantSettings.DefaultCurrency.Symbol
                        },
                    thead = head,
                    tbody = body
                };
        }

        #endregion


        #region SalesFunnelReport

        public bool CheckSalesFunnelReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal d")
                .Select("d.id")
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetSalesFunnelReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildSalesFunnelReport(timePeriod, managers, defaultCurrency);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<SalesFunnel> BuildSalesFunnelReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal_milestone m")
                .Select("m.status", "m.title",
                        "count(d.id) as deals_count",
                        string.Format(@"sum(case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                        end) as deals_value", defaultCurrency),
                        "avg(if(m.status = 1, datediff(d.actual_close_date, d.create_on), 0)) as deals_duration")
                .LeftOuterJoin("crm_deal d", Exp.EqColumns("d.tenant_id", "m.tenant_id") &
                                             Exp.EqColumns("d.deal_milestone_id", "m.id") &
                                             (managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty) &
                                             Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "m.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .GroupBy("m.id")
                .OrderBy("m.sort_order", true);


            return Db.ExecuteList(sqlQuery).ConvertAll(ToSalesFunnel);
        }

        private static SalesFunnel ToSalesFunnel(object[] row)
        {
            return new SalesFunnel
            {
                Status = (DealMilestoneStatus)Convert.ToInt32(row[0]),
                Title = Convert.ToString(row[1]),
                Count = Convert.ToInt32(row[2]),
                Value = Convert.ToDecimal(row[3]),
                Duration = Convert.ToInt32(row[4])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<SalesFunnel> data)
        {           
            var totalCount = data.Sum(x => x.Count);

            if (totalCount == 0) return null;

            var totalBudget = data.Sum(x => x.Value);

            var closed = data.Where(x => x.Status == DealMilestoneStatus.ClosedAndWon).ToList();
            
            var reportData = data.Select(item => new List<object>
                {
                    item.Title,
                    item.Status,
                    item.Count,
                    item.Value
                }).ToList();

            return new
                {
                    resource = new
                        {
                            header = CRMReportResource.SalesFunnelReport,
                            sheetName = CRMReportResource.SalesFunnelReport,
                            dateRangeLabel = CRMReportResource.TimePeriod + ":",
                            dateRangeValue = GetTimePeriodText(timePeriod),

                            chartName = CRMReportResource.SalesFunnelByCount,
                            chartName1 = CRMReportResource.SalesFunnelByBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            chartName2 = CRMReportResource.DealsCount,
                            chartName3 = CRMReportResource.DealsBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,

                            totalCountLabel = CRMReportResource.TotalDealsCount,
                            totalCountValue = totalCount,

                            totalBudgetLabel = CRMReportResource.TotalDealsBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            totalBudgetValue = totalBudget,

                            averageBidLabel = CRMReportResource.AverageDealsBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                            averageBidValue = totalBudget/totalCount,

                            averageDurationLabel = CRMReportResource.AverageDealsDuration,
                            averageDurationValue = closed.Sum(x => x.Duration)/closed.Count,

                            header1 = CRMReportResource.ByCount,
                            header2 = CRMReportResource.ByBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,

                            stage = CRMReportResource.Stage,
                            count = CRMReportResource.Count,
                            budget = CRMReportResource.Budget,
                            conversion = CRMReportResource.Conversion,

                            deals = CRMDealResource.Deals,
                            status0 = DealMilestoneStatus.Open.ToLocalizedString(),
                            status1 = DealMilestoneStatus.ClosedAndWon.ToLocalizedString(),
                            status2 = DealMilestoneStatus.ClosedAndLost.ToLocalizedString()
                        },
                    data = reportData
                };
        }

        #endregion


        #region WorkloadByContactsReport

        public bool CheckWorkloadByContactsReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_contact c")
                .Select("c.id")
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
           
        }

        public object GetWorkloadByContactsReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportData = BuildWorkloadByContactsReport(timePeriod, managers);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<WorkloadByContacts> BuildWorkloadByContactsReport(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_contact c")
                .Select("c.create_by",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "i.id",
                        "i.title",
                        "count(c.id) as total",
                        "count(d.id) as `with deals`")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "c.tenant_id") & Exp.EqColumns("i.id", "c.contact_type_id") & Exp.Eq("i.list_type", (int)ListType.ContactType))
                .LeftOuterJoin("crm_deal d", Exp.EqColumns("d.tenant_id", "c.tenant_id") & Exp.EqColumns("d.contact_id", "c.id"))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "c.tenant_id") & Exp.EqColumns("u.id", "c.create_by"))
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("c.create_by", "i.id")
                .OrderBy("i.sort_order, i.title", true);

            return Db.ExecuteList(sqlQuery).ConvertAll(ToWorkloadByContacts);
        }

        private static WorkloadByContacts ToWorkloadByContacts(object[] row)
        {
            return new WorkloadByContacts
            {
                UserId = string.IsNullOrEmpty(Convert.ToString(row[0])) ? Guid.Empty : new Guid(Convert.ToString(row[0])),
                UserName = Convert.ToString(row[1]),
                CategoryId = Convert.ToInt32(row[2]),
                CategoryName = Convert.ToString(row[3]),
                Count = Convert.ToInt32(row[4]),
                WithDeals = Convert.ToInt32(row[5])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<WorkloadByContacts> reportData)
        {
            return new
            {
                resource = new
                {
                    header = CRMReportResource.WorkloadByContactsReport,
                    sheetName = CRMReportResource.WorkloadByContactsReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),

                    header1 = CRMReportResource.NewContacts,
                    header2 = CRMReportResource.NewContactsWithAndWithoutDeals,
                    
                    manager = CRMReportResource.Manager,
                    total = CRMReportResource.Total,

                    noSet = CRMCommonResource.NoSet,
                    withDeals = CRMReportResource.ContactsWithDeals,
                    withouthDeals = CRMReportResource.ContactsWithoutDeals,
                },
                data = reportData
            };
        }

        #endregion


        #region WorkloadByTasksReport

        public bool CheckWorkloadByTasksReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlNewTasksQuery = Query("crm_task t")
                .Select("t.id")
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var sqlClosedTasksQuery = Query("crm_task t")
                .Select("t.id")
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Eq("t.is_closed", 1))
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var sqlOverdueTasksQuery = Query("crm_task t")
                .Select("t.id")
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.deadline", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where(Exp.Or(Exp.Eq("t.is_closed", 0) & Exp.Lt("t.deadline", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), (Exp.Eq("t.is_closed", 1) & Exp.Sql("t.last_modifed_on > t.deadline"))))
                .SetMaxResults(1);

            bool res;

            using (var tx = Db.BeginTransaction())
            {
                res = Db.ExecuteList(sqlNewTasksQuery).Any() ||
                      Db.ExecuteList(sqlClosedTasksQuery).Any() ||
                      Db.ExecuteList(sqlOverdueTasksQuery).Any();

                tx.Commit();
            }

            return res;
        }

        public object GetWorkloadByTasksReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportData = BuildWorkloadByTasksReport(timePeriod, managers);

            if (reportData == null || !reportData.Any()) return null;

            var hasData = reportData.Any(item => item.Value.Count > 0);

            return hasData ? GenerateReportData(timePeriod, reportData) : null;
        }

        private Dictionary<string, List<WorkloadByTasks>> BuildWorkloadByTasksReport(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlNewTasksQuery = Query("crm_task t")
                .Select("i.id",
                        "i.title",
                        "t.responsible_id",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "count(t.id) as count")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "t.tenant_id") & Exp.EqColumns("i.id", "t.category_id") & Exp.Eq("i.list_type", (int)ListType.TaskCategory))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "t.tenant_id") & Exp.EqColumns("u.id", "t.responsible_id"))
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id", "t.responsible_id")
                .OrderBy("i.sort_order", true);

            var sqlClosedTasksQuery = Query("crm_task t")
                .Select("i.id",
                        "i.title",
                        "t.responsible_id",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "count(t.id) as count")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "t.tenant_id") & Exp.EqColumns("i.id", "t.category_id") & Exp.Eq("i.list_type", (int)ListType.TaskCategory))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "t.tenant_id") & Exp.EqColumns("u.id", "t.responsible_id"))
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Eq("t.is_closed", 1))
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id", "t.responsible_id")
                .OrderBy("i.sort_order", true);

            var sqlOverdueTasksQuery = Query("crm_task t")
                .Select("i.id",
                        "i.title",
                        "t.responsible_id",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "count(t.id) as count")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "t.tenant_id") & Exp.EqColumns("i.id", "t.category_id") & Exp.Eq("i.list_type", (int)ListType.TaskCategory))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "t.tenant_id") & Exp.EqColumns("u.id", "t.responsible_id"))
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.deadline", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where(Exp.Or(Exp.Eq("t.is_closed", 0) & Exp.Lt("t.deadline", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), (Exp.Eq("t.is_closed", 1) & Exp.Sql("t.last_modifed_on > t.deadline"))))
                .GroupBy("i.id", "t.responsible_id")
                .OrderBy("i.sort_order", true);

            Dictionary<string, List<WorkloadByTasks>> res;

            using (var tx = Db.BeginTransaction())
            {
                res = new Dictionary<string, List<WorkloadByTasks>>
                    {
                        {"Created", Db.ExecuteList(sqlNewTasksQuery).ConvertAll(ToWorkloadByTasks)},
                        {"Closed", Db.ExecuteList(sqlClosedTasksQuery).ConvertAll(ToWorkloadByTasks)},
                        {"Overdue", Db.ExecuteList(sqlOverdueTasksQuery).ConvertAll(ToWorkloadByTasks)}
                    };

                tx.Commit();
            }

            return res;
        }

        private static WorkloadByTasks ToWorkloadByTasks(object[] row)
        {
            return new WorkloadByTasks
            {
                CategoryId = Convert.ToInt32(row[0]),
                CategoryName = Convert.ToString(row[1]),
                UserId = string.IsNullOrEmpty(Convert.ToString(row[2])) ? Guid.Empty : new Guid(Convert.ToString(row[2])),
                UserName = Convert.ToString(row[3]),
                Count = Convert.ToInt32(row[4])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, Dictionary<string, List<WorkloadByTasks>> reportData)
        {
            return new
            {
                resource = new
                {
                    header = CRMReportResource.WorkloadByTasksReport,
                    sheetName = CRMReportResource.WorkloadByTasksReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),

                    header1 = CRMReportResource.ClosedTasks,
                    header2 = CRMReportResource.NewTasks,
                    header3 = CRMReportResource.OverdueTasks,

                    manager = CRMReportResource.Manager,
                    total = CRMReportResource.Total
                },
                data = reportData
            };
        }

        #endregion


        #region WorkloadByDealsReport

        public bool CheckWorkloadByDealsReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal d")
                .Select("d.id")
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ?
                        Exp.Empty :
                        Exp.Or(Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)),
                                Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetWorkloadByDealsReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildWorkloadByDealsReport(timePeriod, managers, defaultCurrency);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<WorkloadByDeals> BuildWorkloadByDealsReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_deal d")
                .Select("d.responsible_id",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "m.status",
                        "count(d.id) as deals_count",
                        string.Format(@"sum(case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_deal_milestone m", Exp.EqColumns("m.tenant_id", "d.tenant_id") & Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "d.tenant_id") & Exp.EqColumns("u.id", "d.responsible_id"))
                .LeftOuterJoin("crm_currency_rate r", Exp.EqColumns("r.tenant_id", "d.tenant_id") & Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ?
                        Exp.Empty :
                        Exp.Or(Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)),
                                Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))))
                .GroupBy("d.responsible_id", "m.status");


            return Db.ExecuteList(sqlQuery).ConvertAll(ToWorkloadByDeals);
        }

        private static WorkloadByDeals ToWorkloadByDeals(object[] row)
        {
            return new WorkloadByDeals
            {
                UserId = string.IsNullOrEmpty(Convert.ToString(row[0])) ? Guid.Empty : new Guid(Convert.ToString(row[0])),
                UserName = Convert.ToString(row[1]),
                Status = (DealMilestoneStatus)Convert.ToInt32(row[2]),
                Count = Convert.ToInt32(row[3]),
                Value = Convert.ToDecimal(row[4])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<WorkloadByDeals> data)
        {
            var reportData = data.Select(item => new List<object>
                {
                    item.UserId,
                    item.UserName,
                    (int)item.Status,
                    item.Count,
                    item.Value
                }).ToList();

            return new
            {
                resource = new
                {
                    header = CRMReportResource.WorkloadByDealsReport,
                    sheetName = CRMReportResource.WorkloadByDealsReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),

                    chartName = CRMReportResource.DealsCount,
                    chartName1 = CRMReportResource.DealsBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,

                    header1 = CRMReportResource.ByCount,
                    header2 = CRMReportResource.ByBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,

                    manager = CRMReportResource.Manager,
                    total = CRMReportResource.Total,

                    status0 = CRMReportResource.New,
                    status1 = CRMReportResource.Won,
                    status2 = CRMReportResource.Lost
                },
                data = reportData
            };
        }

        #endregion


        #region WorkloadByInvoicesReport

        public bool CheckWorkloadByInvoicesReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sent = !Exp.Eq("i.status", (int)InvoiceStatus.Draft) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.issue_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));
            var paid = Exp.Eq("i.status", (int)InvoiceStatus.Paid) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));
            var rejected = Exp.Eq("i.status", (int)InvoiceStatus.Rejected) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));
            var overdue = (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.due_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))) & Exp.Or(Exp.Eq("i.status", (int)InvoiceStatus.Sent) & Exp.Lt("i.due_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), Exp.Eq("i.status", (int)InvoiceStatus.Paid) & Exp.Sql("i.last_modifed_on > i.due_date"));

            var sqlQuery = Query("crm_invoice i")
                .Select("i.id")
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .Where(Exp.Or(Exp.Or(sent, paid), Exp.Or(rejected, overdue)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetWorkloadByInvoicesReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportData = BuildWorkloadByInvoicesReport(timePeriod, managers);

            if (reportData == null || !reportData.Any()) return null;

            var hasData = reportData.Any(item => item.SentCount > 0 || item.PaidCount > 0 || item.RejectedCount > 0 || item.OverdueCount > 0);

            return hasData ? GenerateReportData(timePeriod, reportData) : null;
        }

        private List<WorkloadByInvoices> BuildWorkloadByInvoicesReport(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sent = Exp.Sum(Exp.If(!Exp.Eq("i.status", (int)InvoiceStatus.Draft) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.issue_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))), 1, 0));
            var paid = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Paid) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))), 1, 0));
            var rejected = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Rejected) & (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))), 1, 0));
            var overdue = Exp.Sum(Exp.If((timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.due_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))) & Exp.Or(Exp.Eq("i.status", (int)InvoiceStatus.Sent) & Exp.Lt("i.due_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), Exp.Eq("i.status", (int)InvoiceStatus.Paid) & Exp.Sql("i.last_modifed_on > i.due_date")), 1, 0));

            var sqlQuery = Query("crm_invoice i")
                .Select("i.create_by", "concat(u.firstname, ' ', u.lastname) as full_name")
                .Select(sent)
                .Select(paid)
                .Select(rejected)
                .Select(overdue)
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "i.tenant_id") & Exp.EqColumns("u.id", "i.create_by"))
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .GroupBy("i.create_by");


            return Db.ExecuteList(sqlQuery).ConvertAll(ToWorkloadByInvoices);
        }

        private static WorkloadByInvoices ToWorkloadByInvoices(object[] row)
        {
            return new WorkloadByInvoices
            {
                UserId = string.IsNullOrEmpty(Convert.ToString(row[0])) ? Guid.Empty : new Guid(Convert.ToString(row[0])),
                UserName = Convert.ToString(row[1]),
                SentCount = Convert.ToInt32(row[2]),
                PaidCount = Convert.ToInt32(row[3]),
                RejectedCount = Convert.ToInt32(row[4]),
                OverdueCount = Convert.ToInt32(row[5])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<WorkloadByInvoices> reportData)
        {
            return new
            {
                resource = new
                {
                    header = CRMReportResource.WorkloadByInvoicesReport,
                    sheetName = CRMReportResource.WorkloadByInvoicesReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),

                    chartName = CRMReportResource.BilledInvoices,
                    chartName1 = CRMInvoiceResource.Invoices,

                    header1 = CRMInvoiceResource.Invoices,

                    manager = CRMReportResource.Manager,
                    total = CRMReportResource.Total,

                    billed = CRMReportResource.Billed,
                    paid = CRMReportResource.Paid,
                    rejected = CRMReportResource.Rejected,
                    overdue = CRMReportResource.Overdue
                },
                data = reportData
            };
        }

        #endregion


        #region GetWorkloadByViopReport

        public bool CheckWorkloadByViopReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_voip_calls c")
                .Select("c.id")
                .Where(Exp.EqColumns("c.parent_call_id", "''"))
                .Where(managers != null && managers.Any() ? Exp.In("c.answered_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ?
                        Exp.Empty :
                        Exp.Between("c.dial_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            return Db.ExecuteList(sqlQuery).Any();
        }

        public object GetWorkloadByViopReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportData = BuildWorkloadByViopReport(timePeriod, managers);

            return reportData == null || !reportData.Any() ? null : GenerateReportData(timePeriod, reportData);
        }

        private List<WorkloadByViop> BuildWorkloadByViopReport(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var sqlQuery = Query("crm_voip_calls c")
                .Select("c.answered_by",
                        "concat(u.firstname, ' ', u.lastname) as full_name",
                        "c.status",
                        "count(c.id) as calls_count",
                        "sum(c.dial_duration) as duration")
                .LeftOuterJoin("core_user u", Exp.EqColumns("u.tenant", "c.tenant_id") & Exp.EqColumns("u.id", "c.answered_by"))
                .Where(Exp.EqColumns("c.parent_call_id", "''"))
                .Where(managers != null && managers.Any() ? Exp.In("c.answered_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ?
                        Exp.Empty :
                        Exp.Between("c.dial_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("c.answered_by", "c.status");


            return Db.ExecuteList(sqlQuery).ConvertAll(ToWorkloadByViop);
        }

        private static WorkloadByViop ToWorkloadByViop(object[] row)
        {
            return new WorkloadByViop
            {
                UserId = string.IsNullOrEmpty(Convert.ToString(row[0])) ? Guid.Empty : new Guid(Convert.ToString(row[0])),
                UserName = Convert.ToString(row[1] ?? string.Empty),
                Status = (VoipCallStatus)Convert.ToInt32(row[2] ?? 0),
                Count = Convert.ToInt32(row[3]),
                Duration = Convert.ToInt32(row[4])
            };
        }

        private static object GenerateReportData(ReportTimePeriod timePeriod, List<WorkloadByViop> data)
        {
            var reportData = data.Select(item => new List<object>
                {
                    item.UserId,
                    item.UserName,
                    (int) item.Status,
                    item.Count,
                    new {format = TimeFormat, value = SecondsToTimeFormat(item.Duration)}
                }).ToList();
            
            return new
            {
                resource = new
                {
                    header = CRMReportResource.WorkloadByVoipReport,
                    sheetName = CRMReportResource.WorkloadByVoipReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),

                    chartName = CRMReportResource.CallsCount,
                    chartName1 = CRMReportResource.CallsDuration,

                    header1 = CRMReportResource.CallsCount,
                    header2 = CRMReportResource.CallsDuration,

                    manager = CRMReportResource.Manager,
                    total = CRMReportResource.Total,

                    incoming = CRMReportResource.Incoming,
                    outcoming = CRMReportResource.Outcoming,

                    timeFormat = TimeFormat
                },
                data = reportData
            };
        }

        private static string SecondsToTimeFormat(int duration)
        {
            var timeSpan = TimeSpan.FromSeconds(duration);

            return string.Format("{0}:{1}:{2}",
                ((timeSpan.TotalHours < 10 ? "0" : "") + (int)timeSpan.TotalHours),
                ((timeSpan.Minutes < 10 ? "0" : "") + timeSpan.Minutes),
                ((timeSpan.Seconds < 10 ? "0" : "") + timeSpan.Seconds));
        }

        #endregion


        #region SummaryForThePeriodReport

        public bool CheckSummaryForThePeriodReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var newDealsSqlQuery = Query("crm_deal d")
                .Select("d.id")
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var closedDealsSqlQuery = Query("crm_deal d")
                .Select("d.id")
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where(!Exp.Eq("m.status", (int)DealMilestoneStatus.Open))
                .SetMaxResults(1);

            var overdueDealsSqlQuery = Query("crm_deal d")
                .Select("d.id")
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(
                    Exp.And(
                        Exp.Between("d.expected_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)),
                        Exp.Or(Exp.Eq("m.status", (int)DealMilestoneStatus.Open) & Exp.Lt("d.expected_close_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())),
                               Exp.Eq("m.status", (int)DealMilestoneStatus.ClosedAndWon) & Exp.Sql("d.actual_close_date > d.expected_close_date"))));

            var invoicesSqlQuery = Query("crm_invoice i")
                .Select("i.id")
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .Where(Exp.Between("i.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var contactsSqlQuery = Query("crm_contact c")
                .Select("c.id")
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var tasksSqlQuery = Query("crm_task t")
                .Select("t.id")
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var voipSqlQuery = Query("crm_voip_calls c")
                .Select("c.id")
                .Where(Exp.EqColumns("c.parent_call_id", "''"))
                .Where(managers != null && managers.Any() ? Exp.In("c.answered_by", managers) : Exp.Empty)
                .Where(Exp.Between("c.dial_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("c.status");

            bool res;

            using (var tx = Db.BeginTransaction())
            {
                res = Db.ExecuteList(newDealsSqlQuery).Any() ||
                      Db.ExecuteList(closedDealsSqlQuery).Any() ||
                      Db.ExecuteList(overdueDealsSqlQuery).Any() ||
                      Db.ExecuteList(invoicesSqlQuery).Any() ||
                      Db.ExecuteList(contactsSqlQuery).Any() ||
                      Db.ExecuteList(tasksSqlQuery).Any() ||
                      Db.ExecuteList(voipSqlQuery).Any();

                tx.Commit();
            }

            return res;
        }

        public object GetSummaryForThePeriodReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildSummaryForThePeriodReport(timePeriod, managers, defaultCurrency);

            if (reportData == null) return null;

            return GenerateSummaryForThePeriodReportData(timePeriod, reportData);
        }

        private object BuildSummaryForThePeriodReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var newDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));

            var wonDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int) DealMilestoneStatus.ClosedAndWon);

            var lostDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("d.actual_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int) DealMilestoneStatus.ClosedAndLost);

            var overdueDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(
                    Exp.And(
                        Exp.Between("d.expected_close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)),
                        Exp.Or(Exp.Eq("m.status", (int)DealMilestoneStatus.Open) & Exp.Lt("d.expected_close_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())),
                               Exp.Eq("m.status", (int) DealMilestoneStatus.ClosedAndWon) & Exp.Sql("d.actual_close_date > d.expected_close_date"))));

            var sent = Exp.Sum(Exp.If(!Exp.Eq("i.status", (int)InvoiceStatus.Draft), 1, 0));
            var paid = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Paid), 1, 0));
            var rejected = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Rejected), 1, 0));
            var overdue = Exp.Sum(Exp.If(Exp.Or(Exp.Eq("i.status", (int)InvoiceStatus.Sent) & Exp.Lt("i.due_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())),
                                                Exp.Eq("i.status", (int) InvoiceStatus.Paid) & Exp.Sql("i.last_modifed_on > i.due_date")), 1, 0));

            var invoicesSqlQuery = Query("crm_invoice i")
                .Select(sent)
                .Select(paid)
                .Select(rejected)
                .Select(overdue)
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .Where(Exp.Between("i.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));


            var contactsSqlQuery = Query("crm_contact c")
                .Select("i.title",
                        "count(c.id)")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "c.tenant_id") &
                                                  Exp.EqColumns("i.id", "c.contact_type_id") &
                                                  Exp.Eq("i.list_type", (int) ListType.ContactType))
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id")
                .OrderBy("i.sort_order, i.title", true);

            var tasksSqlQuery = Query("crm_task t")
                .Select("i.title")
                .Select(Exp.Sum(Exp.If(Exp.Eq("t.is_closed", 1) & Exp.Between("t.last_modifed_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)), 1, 0)))
                .Select(Exp.Sum(Exp.If(Exp.Or(Exp.Eq("t.is_closed", 0) & Exp.Lt("t.deadline", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), Exp.Eq("t.is_closed", 1) & Exp.Sql("t.last_modifed_on > t.deadline")), 1, 0)))
                .Select("count(t.id)")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "t.tenant_id") & Exp.EqColumns("i.id", "t.category_id") & Exp.Eq("i.list_type", (int)ListType.TaskCategory))
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id")
                .OrderBy("i.sort_order, i.title", true);

            var voipSqlQuery = Query("crm_voip_calls c")
                .Select("c.status",
                        "count(c.id) as calls_count",
                        "sum(c.dial_duration) as duration")
                .Where(Exp.EqColumns("c.parent_call_id", "''"))
                .Where(managers != null && managers.Any() ? Exp.In("c.answered_by", managers) : Exp.Empty)
                .Where(Exp.Between("c.dial_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("c.status");

            object res;

            using (var tx = Db.BeginTransaction())
            {
                res = new
                    {
                        DealsInfo = new
                            {
                                Created = Db.ExecuteList(newDealsSqlQuery),
                                Won = Db.ExecuteList(wonDealsSqlQuery),
                                Lost = Db.ExecuteList(lostDealsSqlQuery),
                                Overdue = Db.ExecuteList(overdueDealsSqlQuery),
                            },
                        InvoicesInfo = Db.ExecuteList(invoicesSqlQuery),
                        ContactsInfo = Db.ExecuteList(contactsSqlQuery),
                        TasksInfo = Db.ExecuteList(tasksSqlQuery),
                        VoipInfo = Db.ExecuteList(voipSqlQuery) 
                    };

                tx.Commit();
            }

            return res;
        }

        private static object GenerateSummaryForThePeriodReportData(ReportTimePeriod timePeriod, object reportData)
        {
            return new
            {
                resource = new
                {
                    header = CRMReportResource.SummaryForThePeriodReport,
                    sheetName = CRMReportResource.SummaryForThePeriodReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),
                    chartName = CRMReportResource.DealsByBudget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName1=CRMReportResource.DealsByCount,
                    chartName2=CRMReportResource.ContactsByType,
                    chartName3=CRMReportResource.TasksForThePeriod,
                    chartName4=CRMReportResource.InvoicesForThePeriod,
                    chartName5 = CRMReportResource.CallsForThePeriod,
                    header1 = CRMDealResource.Deals,
                    header2 = CRMContactResource.Contacts,
                    header3 = CRMTaskResource.Tasks,
                    header4 = CRMInvoiceResource.Invoices,
                    header5 = CRMReportResource.Calls,
                    byBudget = CRMReportResource.ByBudget,
                    currency = Global.TenantSettings.DefaultCurrency.Symbol,
                    byCount = CRMReportResource.ByCount,
                    item = CRMReportResource.Item,
                    type = CRMReportResource.Type,
                    won = CRMReportResource.Won,
                    lost = CRMReportResource.Lost,
                    created = CRMReportResource.Created,
                    closed = CRMReportResource.Closed,
                    overdue = CRMReportResource.Overdue,
                    notSpecified = CRMCommonResource.NoSet,
                    total = CRMReportResource.Total,
                    status = CRMReportResource.Status,
                    billed = CRMReportResource.Billed,
                    paid = CRMReportResource.Paid,
                    rejected = CRMReportResource.Rejected,
                    count = CRMReportResource.Count,
                    duration = CRMReportResource.Duration,
                    incoming = CRMReportResource.Incoming,
                    outcoming = CRMReportResource.Outcoming,
                    missed = CRMReportResource.MissedCount,
                    timeFormat = TimeFormat
                },
                data = reportData
            };
        }

        #endregion


        #region SummaryAtThisMomentReport

        public bool CheckSummaryAtThisMomentReportData(ReportTimePeriod timePeriod, Guid[] managers)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var dealsSqlQuery = Query("crm_deal d")
                .Select("d.id")
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .SetMaxResults(1);

            var contactsSqlQuery = Query("crm_contact c")
                .Select("c.id")
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var tasksSqlQuery = Query("crm_task t")
                .Select("t.id")
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            var invoicesSqlQuery = Query("crm_invoice i")
                .Select("i.id")
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .SetMaxResults(1);

            bool res;

            using (var tx = Db.BeginTransaction())
            {
                res = Db.ExecuteList(dealsSqlQuery).Any() ||
                      Db.ExecuteList(contactsSqlQuery).Any() ||
                      Db.ExecuteList(tasksSqlQuery).Any() ||
                      Db.ExecuteList(invoicesSqlQuery).Any();

                tx.Commit();
            }

            return res;
        }

        public object GetSummaryAtThisMomentReportData(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            var reportData = BuildSummaryAtThisMomentReport(timePeriod, managers, defaultCurrency);

            if (reportData == null) return null;

            return GenerateSummaryAtThisMomentReportData(timePeriod, reportData);
        }

        private object BuildSummaryAtThisMomentReport(ReportTimePeriod timePeriod, Guid[] managers, string defaultCurrency)
        {
            DateTime fromDate;
            DateTime toDate;

            GetTimePeriod(timePeriod, out fromDate, out toDate);

            var openDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int)DealMilestoneStatus.Open);

            var overdueDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .Where(Exp.Lt("d.expected_close_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())));

            var nearDealsSqlQuery = Query("crm_deal d")
                .Select("count(d.id) as count",
                        string.Format(@"sum(case d.bid_type
                                        when 0 then
                                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                                        else
                                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_currency_rate r",
                               Exp.EqColumns("r.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .LeftOuterJoin("crm_deal_milestone m",
                               Exp.EqColumns("m.tenant_id", "d.tenant_id") &
                               Exp.EqColumns("m.id", "d.deal_milestone_id"))
                .Where(managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .Where(Exp.Between("d.expected_close_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()), TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow().AddDays(30))));

            var dealsByStageSqlQuery = Query("crm_deal_milestone m")
                .Select("m.title",
                        "count(d.id) as deals_count",
                        string.Format(@"sum(case d.bid_type
                        when 0 then
                            d.bid_value * (if(d.bid_currency = '{0}', 1, r.rate))
                        else
                            d.bid_value * (if(d.per_period_value = 0, 1, d.per_period_value)) * (if(d.bid_currency = '{0}', 1, r.rate))
                        end) as deals_value", defaultCurrency))
                .LeftOuterJoin("crm_deal d", Exp.EqColumns("d.tenant_id", "m.tenant_id") &
                    Exp.EqColumns("d.deal_milestone_id", "m.id") &
                    (managers != null && managers.Any() ? Exp.In("d.responsible_id", managers) : Exp.Empty) &
                    (timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("d.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate))))
                .LeftOuterJoin("crm_currency_rate r", Exp.EqColumns("r.tenant_id", "m.tenant_id") & Exp.EqColumns("r.from_currency", "d.bid_currency"))
                .Where("m.status", (int)DealMilestoneStatus.Open)
                .GroupBy("m.id")
                .OrderBy("m.sort_order, m.title", true);

            var contactsByTypeSqlQuery = Query("crm_contact c")
                .Select("i.title",
                        "count(c.id) as count")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "c.tenant_id") &
                                                  Exp.EqColumns("i.id", "c.contact_type_id") &
                                                  Exp.Eq("i.list_type", (int) ListType.ContactType))
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id")
                .OrderBy("i.sort_order, i.title", true);

            var contactsByStageSqlQuery = Query("crm_contact c")
                .Select("i.title",
                        "count(c.id) as count")
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "c.tenant_id") &
                                                  Exp.EqColumns("i.id", "c.status_id") &
                                                  Exp.Eq("i.list_type", (int)ListType.ContactStatus))
                .Where(managers != null && managers.Any() ? Exp.In("c.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("c.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id")
                .OrderBy("i.sort_order, i.title", true);

            var tasksSqlQuery = Query("crm_task t")
                .Select("i.title")
                .Select(Exp.Sum(Exp.If(Exp.Eq("t.is_closed", 0), 1, 0)))
                .Select(Exp.Sum(Exp.If(Exp.Eq("t.is_closed", 0) & Exp.Lt("t.deadline", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), 1, 0)))
                .LeftOuterJoin("crm_list_item i", Exp.EqColumns("i.tenant_id", "t.tenant_id") & Exp.EqColumns("i.id", "t.category_id") & Exp.Eq("i.list_type", (int)ListType.TaskCategory))
                .Where(managers != null && managers.Any() ? Exp.In("t.responsible_id", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("t.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)))
                .GroupBy("i.id")
                .OrderBy("i.sort_order, i.title", true);

            var sent = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Sent), 1, 0));
            var overdue = Exp.Sum(Exp.If(Exp.Eq("i.status", (int)InvoiceStatus.Sent) & Exp.Lt("i.due_date", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow())), 1, 0));

            var invoicesSqlQuery = Query("crm_invoice i")
                .Select(sent)
                .Select(overdue)
                .Where(managers != null && managers.Any() ? Exp.In("i.create_by", managers) : Exp.Empty)
                .Where(timePeriod == ReportTimePeriod.DuringAllTime ? Exp.Empty : Exp.Between("i.create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));

            object res;

            using (var tx = Db.BeginTransaction())
            {
                res = new
                    {
                        DealsInfo = new
                            {
                                Open = Db.ExecuteList(openDealsSqlQuery),
                                Overdue = Db.ExecuteList(overdueDealsSqlQuery),
                                Near = Db.ExecuteList(nearDealsSqlQuery),
                                ByStage = Db.ExecuteList(dealsByStageSqlQuery)
                            },
                        ContactsInfo = new
                            {
                                ByType = Db.ExecuteList(contactsByTypeSqlQuery),
                                ByStage = Db.ExecuteList(contactsByStageSqlQuery)
                            },
                        TasksInfo = Db.ExecuteList(tasksSqlQuery),
                        InvoicesInfo = Db.ExecuteList(invoicesSqlQuery),
                    };

                tx.Commit();
            }

            return res;
        }

        private static object GenerateSummaryAtThisMomentReportData(ReportTimePeriod timePeriod, object reportData)
        {
            return new
            {
                resource = new
                {
                    header = CRMReportResource.SummaryAtThisMomentReport,
                    sheetName = CRMReportResource.SummaryAtThisMomentReport,
                    dateRangeLabel = CRMReportResource.TimePeriod + ":",
                    dateRangeValue = GetTimePeriodText(timePeriod),
                    chartName = CRMReportResource.DealsByStatus + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName1 = CRMReportResource.DealsByStage + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    chartName2=CRMReportResource.ContactsByType,
                    chartName3 = CRMReportResource.ContactsByStage,
                    chartName4 = CRMReportResource.TasksByStatus,
                    chartName5 = CRMReportResource.InvoicesByStatus,
                    header1 = CRMDealResource.Deals,
                    header2 = CRMContactResource.Contacts,
                    header3 = CRMTaskResource.Tasks,
                    header4 = CRMInvoiceResource.Invoices,
                    budget = CRMReportResource.Budget + ", " + Global.TenantSettings.DefaultCurrency.Symbol,
                    count = CRMReportResource.Count,
                    open = CRMReportResource.Opened,
                    overdue = CRMReportResource.Overdue,
                    near = CRMReportResource.Near,
                    stage = CRMReportResource.Stage,
                    temperature = CRMContactResource.ContactStage,
                    type = CRMReportResource.Type,
                    total = CRMReportResource.Total,
                    billed = CRMReportResource.Billed,
                    notSpecified = CRMCommonResource.NoSet,
                    status = CRMReportResource.Status,
                },
                data = reportData
            };
        }

        #endregion
    }
}