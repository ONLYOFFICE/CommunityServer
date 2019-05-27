/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
                    Response.Redirect("reports.aspx?reportType=0");
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
