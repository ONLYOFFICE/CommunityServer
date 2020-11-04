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
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using PathProvider = ASC.Web.Projects.Classes.PathProvider;
using Report = ASC.Web.Projects.Classes.Report;

namespace ASC.Web.Projects
{
    public partial class Reports : BasePage
    {
        public List<Report> ListReports { get; private set; }
        public List<ReportTemplate> ListTemplates { get; set; }
        public int ReportsCount { get; set; }

        protected override bool CheckSecurity { get { return !Participant.IsVisitor; } }

        protected override void PageLoad()
        {
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath, "reports.js");

            ReportsCount = EngineFactory.ReportEngine.Get().Count();

            var tmplId = Request["tmplId"];
            if (!string.IsNullOrEmpty(tmplId))
            {
                _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportTemplateView.ascx")));
            }
            else
            {
                if (!string.IsNullOrEmpty(Request["reportType"]))
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportView.ascx")));
                }
                else if (ReportsCount > 0)
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportFile.ascx")));
                }
                else
                {
                    Response.Redirect("Reports.aspx?reportType=0");
                }
            }

            ListTemplates = EngineFactory.ReportEngine.GetTemplates(Participant.ID);
            SetReportList();
        }


        #region private Methods

        private void SetReportList()
        {
            var typeCount = Enum.GetValues(typeof(ReportType)).Length - 1;
            ListReports = new List<Report>();
            for (var i = 0; i < typeCount; i++)
            {
                var report = Report.CreateNewReport((ReportType)i, new TaskFilter());
                ListReports.Add(report);
            }
        }


        #endregion
    }

    public static class TemplateParamInitialiser
    {
        public static string InitHoursCombobox()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 24; i++)
            {
                var value = string.Format(i < 10 ? "0{0}:00" : "{0}:00", i);

                sb.AppendFormat(
                    i == 12
                        ? "<option value='{0}' selected='selected'>{1}</option>"
                        : "<option value='{0}' >{1}</option>", i, value);
            }

            return sb.ToString();
        }
        public static string InitDaysOfWeek()
        {
            var sb = new StringBuilder();
            var format = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            //in cron expression week day 1-7 (not 0-6)
            var firstday = (int)format.FirstDayOfWeek;
            for (var i = firstday; i < firstday + 7; i++)
            {
                sb.AppendFormat("<option value='{0}'>{1}</option>", i % 7 + 1, format.GetDayName((DayOfWeek)(i % 7)));
            }
            return sb.ToString();
        }
        public static string InitDaysOfMonth()
        {
            var sb = new StringBuilder();
            for (var i = 1; i < 32; i++)
            {
                sb.AppendFormat("<option value='{0}'>{0}</option>", i);
            }
            return sb.ToString();
        }
    }
}
