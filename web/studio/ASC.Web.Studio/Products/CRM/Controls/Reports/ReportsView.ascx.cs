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
using System.Text;
using System.Web;
using ASC.CRM.Core;
using ASC.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Controls.Reports
{
    public partial class ReportsView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Reports/ReportsView.ascx"); }
        }

        protected bool ViewFiles;

        protected string ReportHeader;

        protected string ReportDescription;

        protected Dictionary<ReportTimePeriod, string> ReportTimePeriodArray;

        protected ReportType CurrentReportType;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            var urlParam = UrlParameters.ReportType;

            if (string.IsNullOrEmpty(urlParam))
            {
                ViewFiles = true;
                ExecViewFiles();
            }
            else
            {
                int reportType;

                Int32.TryParse(UrlParameters.ReportType, out reportType);

                if (Enum.IsDefined(typeof (ReportType), reportType))
                    CurrentReportType = (ReportType) reportType;

                if (CurrentReportType == ReportType.WorkloadByVoip && CoreContext.Configuration.CustomMode)
                {
                    Response.Redirect(PathProvider.StartURL());
                }

                InitReportInfo();
            }

            RegisterScript();
        }

        private void ExecViewFiles()
        {
            var filesViewControl = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            filesViewControl.EntityType = "report";
            filesViewControl.ModuleName = "crm";
            filesViewControl.CanAddFile = false;
            filesViewControl.EmptyScreenVisible = false;
            _phFilesView.Controls.Add(filesViewControl);
        }

        private void InitReportInfo()
        {
            switch (CurrentReportType)
            {
                case ReportType.SalesByManagers:
                    ReportHeader = CRMReportResource.SalesByManagersReport;
                    ReportDescription = string.Format(CRMReportResource.SalesByManagersReportDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                        };
                    break;
                case ReportType.SalesForecast:
                    ReportHeader = CRMReportResource.SalesForecastReport;
                    ReportDescription = string.Format(CRMReportResource.SalesForecastReportDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.NextWeek, CRMReportResource.NextWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.NextMonth, CRMReportResource.NextMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.NextQuarter, CRMReportResource.NextQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.NextYear, CRMReportResource.NextYear},
                        };
                    break;
                case ReportType.SalesFunnel:
                    ReportHeader = CRMReportResource.SalesFunnelReport;
                    ReportDescription = string.Format(CRMReportResource.SalesFunnelReportDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                        };
                    break;
                case ReportType.WorkloadByContacts:
                    ReportHeader = CRMReportResource.WorkloadByContactsReport;
                    ReportDescription = string.Format(CRMReportResource.WorkloadByContactsDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                            {ReportTimePeriod.DuringAllTime, CRMReportResource.DuringAllTime}
                        };
                    break;
                case ReportType.WorkloadByTasks:
                    ReportHeader = CRMReportResource.WorkloadByTasksReport;
                    ReportDescription = string.Format(CRMReportResource.WorkloadByTasksDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                            {ReportTimePeriod.DuringAllTime, CRMReportResource.DuringAllTime}
                        };
                    break;
                case ReportType.WorkloadByDeals:
                    ReportHeader = CRMReportResource.WorkloadByDealsReport;
                    ReportDescription = string.Format(CRMReportResource.WorkloadByDealsDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                            {ReportTimePeriod.DuringAllTime, CRMReportResource.DuringAllTime}
                        };
                    break;
                case ReportType.WorkloadByInvoices:
                    ReportHeader = CRMReportResource.WorkloadByInvoicesReport;
                    ReportDescription = string.Format(CRMReportResource.WorkloadByInvoicesDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                            {ReportTimePeriod.DuringAllTime, CRMReportResource.DuringAllTime}
                        };
                    break;
                case ReportType.WorkloadByVoip:
                    ReportHeader = CRMReportResource.WorkloadByVoipReport;
                    ReportDescription = string.Format(CRMReportResource.WorkloadByVoipDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear},
                            {ReportTimePeriod.DuringAllTime, CRMReportResource.DuringAllTime}
                        };
                    break;
                case ReportType.SummaryForThePeriod:
                    ReportHeader = CRMReportResource.SummaryForThePeriodReport;
                    ReportDescription = string.Format(CRMReportResource.SummaryForThePeriodReportDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.Yesterday, CRMReportResource.Yesterday},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.PreviousWeek, CRMReportResource.PreviousWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.PreviousMonth, CRMReportResource.PreviousMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.PreviousQuarter, CRMReportResource.PreviousQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear}
                        };
                    break;
                case ReportType.SummaryAtThisMoment:
                    ReportHeader = CRMReportResource.SummaryAtThisMomentReport;
                    ReportDescription = string.Format(CRMReportResource.SummaryAtThisMomentDescription, "<br/>", "<b>", "</b>");
                    ReportTimePeriodArray = new Dictionary<ReportTimePeriod, string>
                        {
                            {ReportTimePeriod.Today, CRMReportResource.Today},
                            {ReportTimePeriod.CurrentWeek, CRMReportResource.CurrentWeek},
                            {ReportTimePeriod.CurrentMonth, CRMReportResource.CurrentMonth},
                            {ReportTimePeriod.CurrentQuarter, CRMReportResource.CurrentQuarter},
                            {ReportTimePeriod.CurrentYear, CRMReportResource.CurrentYear},
                            {ReportTimePeriod.PreviousYear, CRMReportResource.PreviousYear}
                        };
                    break;
            }
        }

        private void RegisterScript()
        {
            Page.RegisterClientScript(new Masters.ClientScripts.ReportsViewData());

            RegisterClientScriptHelper.DataReportsView(Page);

            Page.RegisterInlineScript(string.Format("ASC.CRM.Reports.init({0}, {1});",
                                                    (int) CurrentReportType,
                                                    ViewFiles.ToString().ToLowerInvariant()));

            RegisterClientScriptHelper.DataUserSelectorListView(Page, "_Reports", null);
        }

        #endregion

    }
}