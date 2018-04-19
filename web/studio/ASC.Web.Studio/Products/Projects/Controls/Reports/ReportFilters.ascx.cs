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
using ASC.Core.Tenants;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Classes;

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