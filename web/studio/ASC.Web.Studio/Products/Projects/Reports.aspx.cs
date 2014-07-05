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
using System.Text;
using System.Threading;
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Classes;
using Global = ASC.Web.Projects.Classes.Global;
using PathProvider = ASC.Web.Projects.Classes.PathProvider;

namespace ASC.Web.Projects
{
    public partial class Reports : BasePage
    {
        public List<Report> ListReports { get; private set; }
        public List<ReportTemplate> ListTemplates { get; set; }
        protected override bool CheckSecurity { get { return !Participant.IsVisitor; } }

        protected override void PageLoad()
        {
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("reports.js"));

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
                else
                {
                    Response.Redirect("reports.aspx?reportType=0");
                }
            }

            ListTemplates = Global.EngineFactory.GetReportEngine().GetTemplates(Participant.ID);
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
