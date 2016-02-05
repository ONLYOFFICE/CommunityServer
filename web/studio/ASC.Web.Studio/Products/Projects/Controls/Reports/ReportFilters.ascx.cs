/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportFilters : BaseUserControl
    {
        public Report Report { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Report.ReportType == ReportType.UsersActivity || Report.ReportType == ReportType.TimeSpend)
            {
                fromDate.Text = TenantUtil.DateTimeNow().ToString(DateTimeExtension.DateFormatPattern);
                toDate.Text = TenantUtil.DateTimeNow().AddDays(7).ToString(DateTimeExtension.DateFormatPattern);
            }
        }

        public string InitUsersDdl()
        {
            var projectIds = Report.Filter.ProjectIds;

            if (Report.Filter.TagId != 0 && Report.Filter.ProjectIds.Count == 0)
            {
                projectIds = Page.EngineFactory.TagEngine.GetTagProjects(Report.Filter.TagId).ToList();
            }

            UserInfo[] users;

            if (!Report.Filter.DepartmentId.Equals(Guid.Empty))
            {
                users = CoreContext.UserManager.GetUsersByGroup(Report.Filter.DepartmentId);
            }
            else if (projectIds.Any())
            {
                users = Page.EngineFactory.ProjectEngine.GetTeam(projectIds).Select(r => r.UserInfo).ToArray();
            }
            else
            {
                users = CoreContext.UserManager.GetUsers();
            }

            var sb = new StringBuilder().AppendFormat("<option value='-1' id='ddlUser-1'>{0}</option>", CustomNamingPeople.Substitute<ProjectsCommonResource>("AllUsers").HtmlEncode());

            users.OrderBy(u => u, UserInfoComparer.Default).ToList()
                 .ForEach(u => sb.AppendFormat("<option value='{0}' id='ddlUser{0}' {2}>{1}</option>", u.ID, u.DisplayUserName(), u.ID == Report.Filter.UserId || u.ID == Report.Filter.ParticipantId ? "selected='selected'" : ""));

            return sb.ToString();
        }

        public string InitProjectsDdl()
        {
            var projectEngine = Page.EngineFactory.ProjectEngine;
            var projectIds = new List<int>();

            if (Report.Filter.TagId != 0)
            {
                projectIds = Page.EngineFactory.TagEngine.GetTagProjects(Report.Filter.TagId).ToList();
            }

            var projects = projectIds.Any() ? projectEngine.GetByID(projectIds) : projectEngine.GetAll();

            var mine = projectEngine.GetByParticipant(Page.Participant.ID).ToList();

            if (projectIds.Any())
            {
                mine = mine.Where(r => Report.Filter.ProjectIds.Contains(r.ID)).ToList();
            }

            var active = projects.Where(p => p.Status == ProjectStatus.Open && !mine.Contains(p)).ToList();
            var archive = projects.Where(p => p.Status == ProjectStatus.Closed && !mine.Contains(p)).ToList();

            var sb = new StringBuilder().AppendFormat("<option value='-1' id='ddlProject-1'>{0}</option>", ProjectResource.AllProjects);

            mine.ForEach(p => sb.AppendFormat("<option value='{0}' id='ddlProject{0}' {2}>{1}</option>", p.ID, p.HtmlTitle.HtmlEncode(), p.ID == Report.Filter.ProjectIds.FirstOrDefault() ? "selected='selected'" : ""));
            active.ForEach(p => sb.AppendFormat("<option value='{0}' id='ddlProject{0}' {2}>{1}</option>", p.ID, p.HtmlTitle.HtmlEncode(), p.ID == Report.Filter.ProjectIds.FirstOrDefault() ? "selected='selected'" : ""));
            archive.ForEach(p => sb.AppendFormat("<option value='{0}' id='ddlProject{0}' {2}>{1}</option>", p.ID, p.HtmlTitle.HtmlEncode(), p.ID == Report.Filter.ProjectIds.FirstOrDefault() ? "selected='selected'" : ""));

            return sb.ToString();
        }

        public string InitDepartmentsDdl()
        {
            var sb = new StringBuilder().AppendFormat("<option value='-1' id='ddlDepartment-1'>{0}</option>", CustomNamingPeople.Substitute<ProjectsCommonResource>("AllDepartments").HtmlEncode());

            CoreContext.UserManager
                .GetDepartments()
                .OrderBy(g => g.Name)
                .ToList()
                .ForEach(g => sb.AppendFormat("<option value='{0}' id='ddlDepartment{0}' {2}>{1}</option>", g.ID, g.Name.HtmlEncode(), g.ID == Report.Filter.DepartmentId ? "selected='selected'" : ""));

            return sb.ToString();
        }

        public string InitTagsDdl()
        {
            var sb = new StringBuilder() .AppendFormat("<option value='-1' id='ddlTag'>{0}</option>", ProjectsCommonResource.All);
            
            Page.EngineFactory.TagEngine.GetTags().ToList()
                .ForEach(tag => sb.AppendFormat("<option value='{1}' id='ddlTag{1}' {2}>{0}</option>", tag.Value.HtmlEncode(), tag.Key, tag.Key == Report.Filter.TagId ? "selected='selected'" : "")); 

            return sb.ToString();
        }

        public string InitUpcomingIntervalsDdl(bool withAnyOption)
        {
            var upcomingInterval = (Report.Filter.GetToDate() - Report.Filter.GetFromDate()).Days;

            var sb = new StringBuilder();

            if (withAnyOption)
                sb.AppendFormat("<option value='-1' id='ddlUpcomingInterval-1'>{0}</option>", ReportResource.Any);

            sb.AppendFormat("<option value='7' id='ddlUpcomingInterval7' {1}>1 {0}</option>", ReportResource.Week, 7 == upcomingInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='14' id='ddlUpcomingInterval14' {1}>2 {0}</option>", ReportResource.Weeks, 14 == upcomingInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='21' id='ddlUpcomingInterval21' {1}>3 {0}</option>", ReportResource.Weeks, 21 == upcomingInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='28' id='ddlUpcomingInterval28' {1}>4 {0}</option>", ReportResource.Weeks, 28 == upcomingInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='35' id='ddlUpcomingInterval35' {1}>5 {0}</option>", ReportResource.Weeks2, 35 == upcomingInterval ? "selected='selected'" : "");

            return sb.ToString();
        }

        public string InitTimeIntervalsDdl()
        {
            var timeInterval = (int)Report.Filter.TimeInterval;

            if (Report.Filter.FromDate.Equals(DateTime.MinValue))
            {
                if (timeInterval == 0) timeInterval = 6;
            }
            else
            {
                fromDate.Text = Report.Filter.FromDate.ToString(DateTimeExtension.DateFormatPattern);
                toDate.Text = Report.Filter.ToDate.ToString(DateTimeExtension.DateFormatPattern);
            }

            var sb = new StringBuilder()
                .AppendFormat("<option value='2' id='ddlTimeInterval2' {1}>{0}</option>", ReportResource.Today, 2 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='3' id='ddlTimeInterval3' {1}>{0}</option>", ReportResource.Yesterday, 3 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='5' id='ddlTimeInterval5' {1}>{0}</option>", ReportResource.ThisWeek, 5 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='6' id='ddlTimeInterval6' {1}>{0}</option>", ReportResource.LastWeek, 6 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='8' id='ddlTimeInterval8' {1}>{0}</option>", ReportResource.ThisMonth, 8 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='9' id='ddlTimeInterval9' {1}>{0}</option>", ReportResource.LastMonth, 9 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='11' id='ddlTimeInterval11' {1}>{0}</option>", ReportResource.ThisYear, 11 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='12' id='ddlTimeInterval12' {1}>{0}</option>", ReportResource.LastYear, 12 == timeInterval ? "selected='selected'" : "")
                .AppendFormat("<option value='0' id='ddlTimeInterval0' {1}>{0}</option>", ReportResource.Other, 0 == timeInterval ? "selected='selected'" : "");

            return sb.ToString();
        }

        public string InitTaskStatusesDdl(bool withAnyOption)
        {
            var taskStatus = (int)Report.Filter.TaskStatuses.FirstOrDefault();

            var sb = new StringBuilder();

            if (withAnyOption)
                sb.AppendFormat("<option value='-1' id='TaskStatuses-1'>{0}</option>", TaskResource.All);

            sb.AppendFormat("<option value='7' id='TaskStatuses1' {1}>{0}</option>", ReportResource.ActiveTaskStatus, taskStatus == 1 ? "selected='selected'" : "")
                .AppendFormat("<option value='14' id='TaskStatuses2' {1}>{0}</option>", ReportResource.ClosedTaskStatus, taskStatus == 2 ? "selected='selected'" : "");

            return sb.ToString();
        }

        public string InitPaymentsStatusesDdl(bool withAnyOption)
        {
            var paymentsNotEmpty = Report.Filter.PaymentStatuses.Any();
            var paymentStatus = Report.Filter.PaymentStatuses.FirstOrDefault();

            var sb = new StringBuilder();

            if (withAnyOption)
                sb.AppendFormat("<option value='-1' id='AllPaymentStatuses'>{0}</option>", TaskResource.All);

            sb.AppendFormat("<option value='0' id='NotChargeable' {1}>{0}</option>", ResourceEnumConverter.ConvertToString(PaymentStatus.NotChargeable), paymentsNotEmpty && paymentStatus == PaymentStatus.NotChargeable ? "selected='selected'" : "");
            sb.AppendFormat("<option value='1' id='NotBilled' {1}>{0}</option>", ResourceEnumConverter.ConvertToString(PaymentStatus.NotBilled), paymentsNotEmpty && paymentStatus == PaymentStatus.NotBilled ? "selected='selected'" : "");
            sb.AppendFormat("<option value='2' id='Billed' {1}>{0}</option>", ResourceEnumConverter.ConvertToString(PaymentStatus.Billed), paymentsNotEmpty && paymentStatus == PaymentStatus.Billed ? "selected='selected'" : "");

            return sb.ToString();
        }
    }
}