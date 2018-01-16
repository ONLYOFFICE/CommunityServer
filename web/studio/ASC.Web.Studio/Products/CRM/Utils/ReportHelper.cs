/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using log4net;

namespace ASC.Web.CRM.Classes
{
    public class ReportHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ReportHelper));

        private const string TmpFileName = "tmp.xlsx";

        private static Timer _timer;

        private static readonly ICache Cache = AscCache.Default;

        private static readonly object Locker = new object();

        private static readonly IDictionary<string, ReportTaskState> Queue = new Dictionary<string, ReportTaskState>();

        private static string GetCacheKey()
        {
            return String.Format("{0}_{1}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID);
        }

        private static void ParseCacheKey(string key, out int tenantId, out Guid userId)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            tenantId = 0;
            userId = Guid.Empty;

            var parts = key.Split('_');

            if (parts.Length < 2)
                throw new ArgumentException("key");

            int.TryParse(parts[0], out tenantId);

            Guid.TryParse(parts[1], out userId);
        }

        private static ReportTaskState GetCacheValue()
        {
            return Cache.Get<ReportTaskState>(GetCacheKey());
        }

        private static void SetCacheValue(ReportTaskState value)
        {
            Cache.Insert(value.Id, value, TimeSpan.FromMinutes(5));
        }

        private static void ClearCacheValue(string key)
        {
            Cache.Remove(key);
        }

        private static void InsertItem(ReportTaskState value)
        {
            lock (Locker)
            {
                if (Queue.ContainsKey(value.Id))
                    return;

                Queue.Add(value.Id, value);

                RunTimer(0);
            }
        }

        private static void RunTimer(int dueTime)
        {
            if (_timer == null)
                _timer = new Timer(TimerCallback, null, dueTime, Timeout.Infinite);
            else
                _timer.Change(dueTime, Timeout.Infinite);
        }

        private static void TimerCallback(object obj)
        {
            lock (Locker)
            {
                var keys = Queue.Keys.ToList();

                foreach (var key in keys)
                {
                    try
                    {
                        var value = DocumentService.DocbuilderRequest(FilesLinkUtility.DocServiceDocbuilderUrl,
                                                                      Queue[key].Response.Key, null, true,
                                                                      FileUtility.SignatureSecret);

                        if(value == null)
                            throw new Exception(CRMReportResource.ErrorNullDocbuilderResponse);

                        Queue[key].Response = value;
                        SetCacheValue(Queue[key]);

                        if (value.End)
                        {
                            SaveReportFile(Queue[key]);
                            Queue.Remove(key);
                            continue;
                        }

                        if (value.Error == 0) continue;

                        Queue[key].IsCompleted = true;
                        Queue[key].Percentage = 100;
                        Queue[key].Status = ReportTaskStatus.Failed;
                        Queue[key].ErrorText = value.GetErrorText();
                        SetCacheValue(Queue[key]);
                        Queue.Remove(key);
                    }
                    catch (Exception ex)
                    {
                        Queue[key].IsCompleted = true;
                        Queue[key].Percentage = 100;
                        Queue[key].Status = ReportTaskStatus.Failed;
                        Queue[key].ErrorText = ex.Message;
                        SetCacheValue(Queue[key]);
                        Queue.Remove(key);
                    }
                }

                if (Queue.Any())
                    RunTimer(1000);
            }
        }

        private static string GetFileName(ReportType reportType)
        {
            string reportName;

            switch (reportType)
            {
                case ReportType.SalesByManagers:
                    reportName = CRMReportResource.SalesByManagersReport;
                    break;
                case ReportType.SalesForecast:
                    reportName = CRMReportResource.SalesForecastReport;
                    break;
                case ReportType.SalesFunnel:
                    reportName = CRMReportResource.SalesFunnelReport;
                    break;
                case ReportType.WorkloadByContacts:
                    reportName = CRMReportResource.WorkloadByContactsReport;
                    break;
                case ReportType.WorkloadByTasks:
                    reportName = CRMReportResource.WorkloadByTasksReport;
                    break;
                case ReportType.WorkloadByDeals:
                    reportName = CRMReportResource.WorkloadByDealsReport;
                    break;
                case ReportType.WorkloadByInvoices:
                    reportName = CRMReportResource.WorkloadByInvoicesReport;
                    break;
                case ReportType.WorkloadByVoip:
                    reportName = CRMReportResource.WorkloadByVoipReport;
                    break;
                case ReportType.SummaryForThePeriod:
                    reportName = CRMReportResource.SummaryForThePeriodReport;
                    break;
                case ReportType.SummaryAtThisMoment:
                    reportName = CRMReportResource.SummaryAtThisMomentReport;
                    break;
                default:
                    reportName = string.Empty;
                    break;
            }

            return string.Format("{0} ({1} {2}).xlsx",
                                 reportName,
                                 DateTime.Now.ToShortDateString(),
                                 DateTime.Now.ToShortTimeString());
        }

        public static bool CheckReportData(ReportType reportType, ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportDao = Global.DaoFactory.GetReportDao();

            switch (reportType)
            {
                case ReportType.SalesByManagers:
                    return reportDao.CheckSalesByManagersReportData(timePeriod, managers);
                case ReportType.SalesForecast:
                    return reportDao.CheckSalesForecastReportData(timePeriod, managers);
                case ReportType.SalesFunnel:
                    return reportDao.CheckSalesFunnelReportData(timePeriod, managers);
                case ReportType.WorkloadByContacts:
                    return reportDao.CheckWorkloadByContactsReportData(timePeriod, managers);
                case ReportType.WorkloadByTasks:
                    return reportDao.CheckWorkloadByTasksReportData(timePeriod, managers);
                case ReportType.WorkloadByDeals:
                    return reportDao.CheckWorkloadByDealsReportData(timePeriod, managers);
                case ReportType.WorkloadByInvoices:
                    return reportDao.CheckWorkloadByInvoicesReportData(timePeriod, managers);
                case ReportType.WorkloadByVoip:
                    return reportDao.CheckWorkloadByViopReportData(timePeriod, managers);
                case ReportType.SummaryForThePeriod:
                    return reportDao.CheckSummaryForThePeriodReportData(timePeriod, managers);
                case ReportType.SummaryAtThisMoment:
                    return reportDao.CheckSummaryAtThisMomentReportData(timePeriod, managers);
                default:
                    return false;
            }
        }

        public static List<string> GetMissingRates(ReportType reportType)
        {
            if (reportType == ReportType.WorkloadByTasks || reportType == ReportType.WorkloadByInvoices ||
                reportType == ReportType.WorkloadByContacts || reportType == ReportType.WorkloadByVoip) return null;

            return Global.DaoFactory.GetReportDao().GetMissingRates(Global.TenantSettings.DefaultCurrency.Abbreviation);
        }

        private static object GetReportData(ReportType reportType, ReportTimePeriod timePeriod, Guid[] managers)
        {
            var reportDao = Global.DaoFactory.GetReportDao();
            var defaultCurrency = Global.TenantSettings.DefaultCurrency.Abbreviation;

            switch (reportType)
            {
                case ReportType.SalesByManagers:
                    return reportDao.GetSalesByManagersReportData(timePeriod, managers, defaultCurrency);
                case ReportType.SalesForecast:
                    return reportDao.GetSalesForecastReportData(timePeriod, managers, defaultCurrency);
                case ReportType.SalesFunnel:
                    return reportDao.GetSalesFunnelReportData(timePeriod, managers, defaultCurrency);
                case ReportType.WorkloadByContacts:
                    return reportDao.GetWorkloadByContactsReportData(timePeriod, managers);
                case ReportType.WorkloadByTasks:
                    return reportDao.GetWorkloadByTasksReportData(timePeriod, managers);
                case ReportType.WorkloadByDeals:
                    return reportDao.GetWorkloadByDealsReportData(timePeriod, managers, defaultCurrency);
                case ReportType.WorkloadByInvoices:
                    return reportDao.GetWorkloadByInvoicesReportData(timePeriod, managers);
                case ReportType.WorkloadByVoip:
                    return reportDao.GetWorkloadByViopReportData(timePeriod, managers);
                case ReportType.SummaryForThePeriod:
                    return reportDao.GetSummaryForThePeriodReportData(timePeriod, managers, defaultCurrency);
                case ReportType.SummaryAtThisMoment:
                    return reportDao.GetSummaryAtThisMomentReportData(timePeriod, managers, defaultCurrency);
                default:
                    return null;
            }
        }

        private static string GetReportScript(object data, ReportType type)
        {
            var script =
                FileHelper.ReadTextFromEmbeddedResource(string.Format("ASC.Web.CRM.ReportTemplates.{0}.docbuilder", type));

            if (string.IsNullOrEmpty(script))
                throw new Exception(CRMReportResource.BuildErrorEmptyDocbuilderTemplate);

            return script.Replace("${outputFilePath}", TmpFileName)
                         .Replace("${reportData}", JsonConvert.SerializeObject(data));
        }

        public static ReportTaskState GetCurrentState()
        {
            var state = GetCacheValue();

            if (state != null && (state.IsCompleted || !string.IsNullOrEmpty(state.ErrorText)))
                ClearCacheValue(state.Id);

            return state;
        }

        public static void Terminate()
        {
            lock (Locker)
            {
                var key = GetCacheKey();

                if (Queue.ContainsKey(key))
                    Queue.Remove(key);

                ClearCacheValue(key);
            }
        }

        private static void SaveReportFile(ReportTaskState state)
        {
            int tenantId;
            Guid userId;

            ParseCacheKey(state.Id, out tenantId, out userId);

            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            SecurityContext.AuthenticateMe(userId);

            var daoFactory = Global.DaoFactory;
            var url = state.Response.GetFileUrl(TmpFileName);
            var data = new WebClient().DownloadData(url);

            using (var stream = new MemoryStream(data))
            {
                var document = new ASC.Files.Core.File
                    {
                        Title = state.FileName,
                        FolderID = daoFactory.GetFileDao().GetRoot(),
                        ContentLength = stream.Length
                    };

                var file = daoFactory.GetFileDao().SaveFile(document, stream);

                daoFactory.GetReportDao().SaveFile((int) file.ID, (int) state.ReportType);

                state.Percentage = 100;
                state.IsCompleted = true;
                state.Status = ReportTaskStatus.Done;
                state.FileId = (int) file.ID;

                SetCacheValue(state);
            }
        }

        private static void GenareteReport(ReportTaskState state, ReportType reportType, ReportTimePeriod timePeriod,
                                           Guid[] managers)
        {
            state.Status = ReportTaskStatus.Started;
            state.Percentage = 10;
            SetCacheValue(state);

            var reportData = GetReportData(reportType, timePeriod, managers);

            if (reportData != null)
            {
                state.Percentage = 50;
            }
            else
            {
                state.Percentage = 100;
                state.IsCompleted = true;
                state.ErrorText = CRMReportResource.ErrorNullReportData;
                state.Status = ReportTaskStatus.Failed;
            }

            SetCacheValue(state);

            if (state.Status == ReportTaskStatus.Failed) return;

            var script = GetReportScript(reportData, reportType);

            if (!string.IsNullOrEmpty(script))
            {
                state.Percentage = 60;
            }
            else
            {
                state.Percentage = 100;
                state.IsCompleted = true;
                state.ErrorText = CRMReportResource.ErrorNullReportScript;
                state.Status = ReportTaskStatus.Failed;
            }

            SetCacheValue(state);

            if (state.Status == ReportTaskStatus.Failed) return;

            state.Response = DocumentService.DocbuilderRequest(FilesLinkUtility.DocServiceDocbuilderUrl, null,
                                                               script, true, FileUtility.SignatureSecret);

            if (state.Response != null && state.Response.Error == 0)
            {
                state.Percentage = 80;
            }
            else
            {
                state.Percentage = 100;
                state.IsCompleted = true;
                state.ErrorText = state.Response == null
                                      ? CRMReportResource.ErrorNullDocbuilderResponse
                                      : state.Response.GetErrorText();
                state.Status = ReportTaskStatus.Failed;
            }

            SetCacheValue(state);

            if (state.Status == ReportTaskStatus.Failed) return;

            InsertItem(state);
        }

        private static void StartGenareteReport(object parameter)
        {
            try
            {
                var obj = (ReportTaskParameters) parameter;

                CoreContext.TenantManager.SetCurrentTenant(obj.TenantId);
                SecurityContext.AuthenticateMe(obj.CurrentUser);

                var user = CoreContext.UserManager.GetUsers(obj.CurrentUser);

                if (user != null && !string.IsNullOrEmpty(user.CultureName))
                {
                    var culture = user.GetCulture();
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }

                GenareteReport(obj.TaskState, obj.ReportType, obj.TimePeriod, obj.Managers);
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                var state = GetCacheValue();

                state.Percentage = 100;
                state.IsCompleted = true;
                state.Status = ReportTaskStatus.Failed;
                state.ErrorText = ex.Message;

                SetCacheValue(state);
            }
        }

        public static ReportTaskState RunGenareteReport(ReportType reportType, ReportTimePeriod timePeriod,
                                                        Guid[] managers)
        {
            var state = new ReportTaskState
                {
                    Id = GetCacheKey(),
                    Status = ReportTaskStatus.Queued,
                    ReportType = reportType,
                    Percentage = 0,
                    IsCompleted = false,
                    ErrorText = null,
                    Response = null,
                    FileName = GetFileName(reportType),
                    FileId = 0
                };

            SetCacheValue(state);

            var th = new Thread(StartGenareteReport);

            th.Start(new ReportTaskParameters
                {
                    TaskState = state,
                    TenantId = TenantProvider.CurrentTenantID,
                    CurrentUser = SecurityContext.CurrentAccount.ID,
                    ReportType = reportType,
                    TimePeriod = timePeriod,
                    Managers = managers
                });

            return state;
        }

        private class ReportTaskParameters
        {
            public ReportTaskState TaskState { get; set; }
            public int TenantId { get; set; }
            public Guid CurrentUser { get; set; }
            public ReportType ReportType { get; set; }
            public ReportTimePeriod TimePeriod { get; set; }
            public Guid[] Managers { get; set; }
        }
    }
}