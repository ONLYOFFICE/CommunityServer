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
using ASC.Core.Tenants;
using ASC.Projects.Core.Domain.Reports;
using Report = ASC.Web.Projects.Classes.Report;

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportFilters : BaseUserControl
    {
        public Report Report { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Report.ReportType == ReportType.UsersActivity || Report.ReportType == ReportType.TimeSpend)
            {
                DateTime fromDateTime;
                DateTime toDateTime;

                if (!TryParseFilter("ffrom", out fromDateTime) || !TryParseFilter("fto", out toDateTime))
                {
                    fromDateTime = TenantUtil.DateTimeNow();
                    toDateTime = fromDateTime.AddDays(7);
                }

                fromDate.Text = fromDateTime.ToString(DateTimeExtension.DateFormatPattern);
                toDate.Text = toDateTime.ToString(DateTimeExtension.DateFormatPattern);
            }
        }

        private bool TryParseFilter(string requestParam, out DateTime result)
        {
            result = new DateTime();

            var param = Request.QueryString[requestParam];

            if (string.IsNullOrEmpty(param)) return false;

            try
            {
                result = new DateTime(Convert.ToInt32(param.Substring(0, 4)), Convert.ToInt32(param.Substring(4, 2)), Convert.ToInt32(param.Substring(6, 2)));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}