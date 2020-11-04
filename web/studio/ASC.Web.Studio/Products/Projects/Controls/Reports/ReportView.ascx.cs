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
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using Report = ASC.Web.Projects.Classes.Report;

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportView : BaseUserControl
    {
        public Report Report { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            reportTemplateContainer.Options.IsPopup = true;
            InitReport();
            Page.Title = HeaderStringHelper.GetPageTitle(string.Format(ReportResource.ReportPageTitle, Report.ReportInfo.Title)); 
        }

        private void InitReport()
        {
            var filter = TaskFilter.FromUri(HttpContext.Current.Request.GetUrlRewriter());
            var reportType = Request["reportType"];
            if (string.IsNullOrEmpty(reportType)) return;

            Report = Report.CreateNewReport((ReportType) int.Parse(reportType), filter);

            var filters = (ReportFilters)LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportFilters.ascx"));
            filters.Report = Report;
            _filter.Controls.Add(filters);
        }
    }
}