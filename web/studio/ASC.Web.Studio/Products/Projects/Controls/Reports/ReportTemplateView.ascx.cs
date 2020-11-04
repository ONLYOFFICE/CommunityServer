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
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using Report = ASC.Web.Projects.Classes.Report;

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportTemplateView : BaseUserControl
    {
        public ReportTemplate Template { get; set; }

        public string TmplParamHour { get; set; }
        public int TmplParamMonth { get; set; }
        public int TmplParamWeek { get; set; }
        public string TmplParamPeriod { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Template = Page.EngineFactory.ReportEngine.GetTemplate(int.Parse(Request["tmplId"]));
            if (Template == null)
            {
                Page.RedirectNotFound("Reports.aspx");
            }
            else
            {
                var filters = (ReportFilters)LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportFilters.ascx"));
                filters.Report = Report.CreateNewReport(Template.ReportType, Template.Filter);
                _filter.Controls.Add(filters);
                InitTmplParam();

                Page.Title = HeaderStringHelper.GetPageTitle(string.Format(ReportResource.ReportPageTitle, HttpUtility.HtmlDecode(Template.Name)));
            }
        }

        private void InitTmplParam()
        {
            var cron = Template.Cron.Split(' ');

            try
            {
                TmplParamWeek = Int32.Parse(cron[5]);
                TmplParamPeriod = "week";
            }
            catch (FormatException)
            {
                try
                {
                    TmplParamMonth = Int32.Parse(cron[3]);
                    TmplParamPeriod = "month";
                }
                catch (FormatException)
                {
                    TmplParamPeriod = "day";
                }
            }

            TmplParamHour = cron[2];

        }
    }
}