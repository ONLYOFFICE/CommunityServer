/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    public class Report
    {
        #region private

        private ExtendedReportType ExtendedReportType { get; set; }

        public TaskFilter Filter { get; set; }

        private Report(ExtendedReportType reportType, TaskFilter filter)
        {
            ExtendedReportType = reportType;
            Filter = filter;
        }

        #endregion

        #region public

        public static Report CreateNewReport(ReportType reportType, TaskFilter filter)
        {
            switch (reportType)
            {
                case ReportType.MilestonesExpired:
                    return new Report(new MilestonesExpiredReport(), filter);

                case ReportType.MilestonesNearest:
                    return new Report(new MilestonesNearestReport(), filter);

                case ReportType.ProjectsList:
                    return new Report(new ProjectsListReport(), filter);

                case ReportType.ProjectsWithoutActiveMilestones:
                    return new Report(new ProjectsWithoutActiveMilestonesReport(), filter);

                case ReportType.ProjectsWithoutActiveTasks:
                    return new Report(new ProjectsWithoutActiveTasksReport(), filter);

                case ReportType.TasksByProjects:
                    return new Report(new TasksByProjectsReport(), filter);

                case ReportType.TasksByUsers:
                    return new Report(new TasksByUsersReport(), filter);

                case ReportType.TasksExpired:
                    return new Report(new TasksExpiredReport(), filter);

                case ReportType.TimeSpend:
                    return new Report(new TimeSpendReport(), filter);

                case ReportType.UsersWithoutActiveTasks:
                    return new Report(new UsersWithoutActiveTasksReport(), filter);

                case ReportType.UsersWorkload:
                    return new Report(new UsersWorkloadReport(), filter);

                case ReportType.UsersActivity:
                    return new Report(new UsersActivityReport(), filter);

                case ReportType.EmptyReport:
                    return new Report(new EmptyReport(), filter);

            }

            throw new Exception("There is not report Type");
        }

        public string BuildReport(ReportViewType viewType, int templateID)
        {
            //prepare filter
            if (templateID != 0 && !Filter.FromDate.Equals(DateTime.MinValue))
            {
                var interval = Filter.ToDate.DayOfYear - Filter.FromDate.DayOfYear;

                switch (ExtendedReportType.GetReportType())
                {
                    case ReportType.TasksByUsers:
                    case ReportType.TasksByProjects:
                        {
                            Filter.FromDate = TenantUtil.DateTimeNow().Date.AddDays(-interval);
                            Filter.ToDate = TenantUtil.DateTimeNow().Date;
                        }
                        break;
                    case ReportType.MilestonesNearest:
                        {
                            Filter.FromDate = TenantUtil.DateTimeNow().Date;
                            Filter.ToDate = TenantUtil.DateTimeNow().Date.AddDays(interval);
                        }
                        break;
                }
            }

            //exec

            var data = ExtendedReportType.BuildReport(Filter).ToList();

            return !data.Any()
                       ? CreateNewReport(ReportType.EmptyReport, Filter).Transform(new List<object[]>(), viewType, templateID)
                       : this.Transform(data.ToList(), viewType, templateID);
        }


        public ReportType ReportType
        {
            get { return ExtendedReportType.GetReportType(); }
        }

        public List<string> GetColumns(ReportViewType viewType, int templateID)
        {
            var parameters = new List<string>();

            if (viewType == ReportViewType.EMail)
            {
                parameters.Add(CommonLinkUtility.GetFullAbsolutePath(string.Format("~/products/projects/reports.aspx?reportType={0}&tmplId={1}", (int)ReportType, templateID)));
                parameters.Add(ReportResource.ChangeSettings);
            }
            else
            {
                parameters = viewType == ReportViewType.Csv ? ExtendedReportType.GetCsvColumnsName().ToList() : ReportInfo.Columns.ToList();

                if (ReportType == ReportType.TimeSpend)
                {
                    parameters.Add(Filter.PaymentStatuses.Count != 0 ? ((int)Filter.PaymentStatuses[0]).ToString(CultureInfo.InvariantCulture) : "-1");
                }
            }

            return parameters;
        }

        public ReportInfo ReportInfo
        {
            get { return ExtendedReportType.GetReportInfo(); }
        }

        public string FileName
        {
            get { return ExtendedReportType.GetReportFileName().Replace(' ', '_'); }
        }

        #endregion
    }


    public abstract class ExtendedReportType
    {
        #region publicMethods

        public abstract ReportType GetReportType();

        public abstract IEnumerable<object[]> BuildReport(TaskFilter filter);

        public abstract string[] GetCsvColumnsName();

        public abstract ReportInfo GetReportInfo();

        public abstract string GetReportFileName();

        #endregion

        #region ProtectedMethods

        protected string VirtualRoot
        {
            get { return CommonLinkUtility.VirtualRoot != "/" ? CommonLinkUtility.VirtualRoot : string.Empty; }
        }

        protected string VirtulaRootPath
        {
            get { return CommonLinkUtility.ServerRootPath + VirtualRoot; }
        }

        protected static IEnumerable<object[]> AddUserInfo(IEnumerable<object[]> rows, int userIdIndex, Guid? groupId = null)
        {
            var result = new List<object[]>();
            foreach (var row in rows)
            {
                if (row[userIdIndex] == null) continue;

                var users = row[userIdIndex].ToString().Split(',').Select(r => string.IsNullOrEmpty(r) ? Guid.Empty : new Guid(r));
                foreach (var userID in users)
                {
                    var list = new List<object>(row);
                    if (userID != Guid.Empty)
                    {
                        var user = CoreContext.UserManager.GetUsers(userID);
                        if (user.ID != Constants.LostUser.ID && user.Status != EmployeeStatus.Terminated && 
                            (!groupId.HasValue || groupId.Value.Equals(Guid.Empty) || CoreContext.UserManager.IsUserInGroup(user.ID, groupId.Value)))
                        {
                            list.Add(user.DisplayUserName(false));
                            list.Add(user.GetUserProfilePageURL());
                            list[userIdIndex] = userID;
                            result.Add(list.ToArray());
                        }
                    }
                    else
                    {
                        list.Add(TaskResource.WithoutResponsible);
                        list.Add(string.Empty);
                        result.Add(list.ToArray());
                    }
                }
            }

            return result;
        }

        protected static IEnumerable<object[]> AddStatusCssClass(IEnumerable<object[]> rows)
        {
            var result = new List<object[]>();
            foreach (var row in rows)
            {
                var list = new List<object>(row);
                if ((int)row[5] != -1)
                {
                    var milestoneStatus = (MilestoneStatus)row[5];
                    var milestoneCssClass = string.Empty;
                    if (milestoneStatus == MilestoneStatus.Open)
                    {
                        DateTime milestoneDeadline;
                        if (DateTime.TryParse((string)row[4], out milestoneDeadline))
                        {
                            if (milestoneDeadline < TenantUtil.DateTimeNow())
                                milestoneCssClass = "red-text";
                        }
                    }

                    list[5] = ResourceEnumConverter.ConvertToString(milestoneStatus);
                    list.Add(milestoneStatus);
                    list.Add(milestoneCssClass);
                }
                else
                {
                    row[5] = null;
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                }

                var taskStatus = (TaskStatus)row[9];
                var taskCssClass = string.Empty;

                if (taskStatus == TaskStatus.Open)
                {
                    DateTime taskDeadline;
                    if (DateTime.TryParse((string)row[10], out taskDeadline))
                    {
                        if (taskDeadline < TenantUtil.DateTimeNow())
                            taskCssClass = "red-text";
                    }
                }

                list[9] = ResourceEnumConverter.ConvertToString(taskStatus);
                list.Add(taskStatus);
                list.Add(taskCssClass);

                result.Add(list.ToArray());
            }
            return result;
        }

        #endregion
    }

    #region Milestones Reports

    class MilestonesReport : ExtendedReportType
    {
        protected string[] MileColumns
        {
            get
            {
                return new[]
                           {
                               ProjectResource.Project,
                               MilestoneResource.Milestone,
                               ReportResource.DeadLine,
                               VirtulaRootPath
                           };
            }
        }

        public override ReportType GetReportType()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            return Global.EngineFactory.GetMilestoneEngine()
                         .GetByFilter(filter)
                         .OrderBy(r => r.Project.Title)
                         .Select(r => new object[]
                                          {
                                              r.Project.ID, r.Project.Title, r.ID, r.Title, r.DeadLine.ToString("d"),
                                              LocalizedEnumConverter.ConvertToString(r.Status)
                                          });
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnProjectjTitle,
                ReportResource.CsvColumnMilestoneTitle,
                ReportResource.CsvColumnMilestoneDeadline};
        }

        public override ReportInfo GetReportInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportLateMilestones_Title", CultureInfo.InvariantCulture);
        }
    }

    class MilestonesExpiredReport : MilestonesReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.MilestonesExpired;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            filter.ToDate = TenantUtil.DateTimeNow();
            filter.MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open };

            return base.BuildReport(filter);
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(String.Format(ReportResource.ReportLateMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                                    ReportResource.ReportLateMilestones_Title,
                                    MileColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportLateMilestones_Title", CultureInfo.InvariantCulture);
        }
    }

    class MilestonesNearestReport : MilestonesReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.MilestonesNearest;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            filter.MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open };

            return base.BuildReport(filter);
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(String.Format(ReportResource.ReportUpcomingMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                                    ReportResource.ReportUpcomingMilestones_Title,
                                    MileColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportUpcomingMilestones_Title", CultureInfo.InvariantCulture);
        }
    }

    #endregion

    #region Project Reports

    class ProjectsListReport : ExtendedReportType
    {
        protected string[] ProjColumns
        {
            get
            {
                return new[]
                           {
                               ProjectsCommonResource.Title,
                               ProjectResource.ProjectLeader,
                               ProjectsCommonResource.Status,
                               GrammaticalResource.MilestoneGenitivePlural,
                               TaskResource.Tasks,
                               ReportResource.Participiants,
                               ReportResource.ClickToSortByThisColumn,
                               CommonLinkUtility.ServerRootPath,
                               VirtualRoot
                           };
            }
        }

        public override ReportType GetReportType()
        {
            return ReportType.ProjectsList;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            filter.SortBy = "title";
            filter.SortOrder = true;

            var result = Global.EngineFactory.GetProjectEngine()
                               .GetByFilter(filter)
                               .Select(r => new object[]
                                                {
                                                    r.ID, r.Title, r.Responsible,
                                                    LocalizedEnumConverter.ConvertToString(r.Status),
                                                    r.MilestoneCount, r.TaskCount, r.ParticipantCount
                                                });

            result = AddUserInfo(result, 2);
            result = result.OrderBy(r => (string)r[1]);

            return result;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnProjectjTitle,
                ProjectResource.ProjectLeader,
                ProjectsCommonResource.Status,
                GrammaticalResource.MilestoneGenitivePlural,
                TaskResource.Tasks,
                ReportResource.Participiants };
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportProjectList_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportProjectList_Title,
                ProjColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportProjectList_Title", CultureInfo.InvariantCulture);
        }
    }

    class ProjectsWithoutActiveMilestonesReport : ProjectsListReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.ProjectsWithoutActiveMilestones;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            return base.BuildReport(filter).Where(r => (int)r[4] == 0);
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportProjectsWithoutActiveMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportProjectsWithoutActiveMilestones_Title,
                ProjColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportProjectsWithoutActiveMilestones_Title", CultureInfo.InvariantCulture);
        }
    }

    class ProjectsWithoutActiveTasksReport : ProjectsListReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.ProjectsWithoutActiveTasks;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            return base.BuildReport(filter).Where(r => (int)r[5] == 0);
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportProjectsWithoutActiveTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportProjectsWithoutActiveTasks_Title,
                ProjColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportProjectsWithoutActiveTasks_Title", CultureInfo.InvariantCulture);
        }
    }

    #endregion

    #region TaskReports

    class TasksReport : ExtendedReportType
    {
        protected string[] TaskColumns
        {
            get
            {
                return new[]
                           {

                               ProjectResource.Project,
                               MilestoneResource.Milestone,
                               TaskResource.Task,
                               TaskResource.TaskResponsible,
                               ProjectsCommonResource.Status,
                               TaskResource.UnsortedTask,
                               ReportResource.DeadLine,
                               ReportResource.NoMilestonesAndTasks,
                               CommonLinkUtility.ServerRootPath,
                               VirtualRoot
                           };
            }
        }

        public override ReportType GetReportType()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            if (!filter.UserId.Equals(Guid.Empty))
            {
                filter.ParticipantId = filter.UserId;
                filter.UserId = Guid.Empty;
            }

            var createdFrom = filter.FromDate;
            var createdTo = filter.ToDate;

            filter.FromDate = DateTime.MinValue;
            filter.ToDate = DateTime.MinValue;

            filter.SortBy = "deadline";
            filter.SortOrder = true;

            var tasks = Global.EngineFactory.GetTaskEngine().GetByFilter(filter).FilterResult.OrderBy(r => r.Project.Title).ToList();

            filter.FromDate = createdFrom;
            filter.ToDate = createdTo;

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MinValue))
                tasks = tasks.Where(r => r.CreateOn.Date >= filter.FromDate && r.CreateOn.Date <= filter.ToDate).ToList();

            if (!filter.NoResponsible)
                tasks = tasks.Where(r => r.Responsibles.Any()).ToList();

            var result = tasks.Select(r => new object[]
                                               {
                                                   r.Project.ID, r.Project.Title,
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.ID : 0,
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.Title : "",
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.DeadLine.ToString("d") : null,
                                                   r.MilestoneDesc != null ? (int) r.MilestoneDesc.Status : -1,
                                                   r.ID, r.Title,
                                                   GetResponsible(r, filter.ParticipantId),
                                                   r.Status,
                                                   !r.Deadline.Equals(DateTime.MinValue) ? r.Deadline.ToString("d") : "", 
                                                   HtmlUtil.GetText(r.Description, 500)
                                               });

            result = AddUserInfo(result, 8, filter.DepartmentId);
            result = AddStatusCssClass(result);

            return result;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnProjectjTitle,
                ReportResource.CsvColumnMilestoneTitle,
                ReportResource.CsvColumnMilestoneDeadline, 
                ReportResource.CsvColumnMilestoneStatus,
                ReportResource.CsvColumnTaskTitle,
                ReportResource.CsvColumnTaskDueDate,
                ReportResource.CsvColumnTaskStatus,
                ReportResource.CsvColumnUserName,
                TaskResource.UnsortedTask};
        }

        public override ReportInfo GetReportInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetReportFileName()
        {
            throw new NotImplementedException();
        }

        private string GetResponsible(Task task, Guid? userID)
        {
            if (GetReportType() == ReportType.TasksByUsers && userID.HasValue && !userID.Equals(Guid.Empty))
                task.Responsibles.RemoveAll(r => !r.Equals(userID));

            return task.Responsibles.Any()
                                ? task.Responsibles.Select(a => a.ToString()).Aggregate((a, b) => a + "," + b)
                                : "";
        }
    }

    class TasksByProjectsReport : TasksReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.TasksByProjects;
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportTaskList_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportTaskList_Title,
                TaskColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportTaskList_Title", CultureInfo.InvariantCulture);
        }
    }

    class TasksByUsersReport : TasksReport
    {
        public override ReportType GetReportType()
        {
            return ReportType.TasksByUsers;
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportUserTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportUserTasks_Title,
                TaskColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportUserTasks_Title", CultureInfo.InvariantCulture);
        }
    }

    class TasksExpiredReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.TasksExpired;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            filter.FromDate = new DateTime(1970, 1, 1);
            filter.ToDate = TenantUtil.DateTimeNow();
            filter.TaskStatuses.Add(TaskStatus.Open);

            var tasks = Global.EngineFactory.GetTaskEngine().GetByFilter(filter).FilterResult.OrderBy(r => r.Project.Title).ToList();

            var result = tasks.Select(r => new object[]
                                               {
                                                   r.Project.ID, r.Project.Title,
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.ID : 0,
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.Title : "",
                                                   r.MilestoneDesc != null ? r.MilestoneDesc.DeadLine.ToString("d"): null,
                                                   r.MilestoneDesc != null ? (int) r.MilestoneDesc.Status : -1,
                                                   r.ID, r.Title,
                                                   r.Responsibles.Any() ? r.Responsibles.Select(a => a.ToString()).Aggregate((a, b) => a + "," + b) : "",
                                                   r.Status,
                                                   !r.Deadline.Equals(DateTime.MinValue) ? r.Deadline.ToString("d") : "", 
                                                   HtmlUtil.GetText(r.Description, 500)
                                               });

            result = result.Where(row => row[10] != null).ToList();

            result = AddUserInfo(result, 8);
            result = AddStatusCssClass(result);

            return result;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnProjectjTitle,
                ReportResource.CsvColumnMilestoneTitle,
                ReportResource.CsvColumnMilestoneDeadline, 
                ReportResource.CsvColumnMilestoneStatus,
                ReportResource.CsvColumnTaskTitle,
                ReportResource.CsvColumnTaskDueDate,
                ReportResource.CsvColumnTaskStatus,
                ReportResource.CsvColumnUserName,
                TaskResource.UnsortedTask};
        }

        public override ReportInfo GetReportInfo()
        {
            var taskExpiredColumns = new[] {
                ProjectResource.Project,
                MilestoneResource.Milestone,
                TaskResource.Task, 
                TaskResource.TaskResponsible,
                ProjectsCommonResource.Status,
                TaskResource.UnsortedTask,
                TaskResource.DeadLine,
                ReportResource.NoMilestonesAndTasks,
                CommonLinkUtility.ServerRootPath,
                VirtualRoot };

            return new ReportInfo(
                String.Format(ReportResource.ReportLateTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportLateTasks_Title,
                taskExpiredColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportLateTasks_Title", CultureInfo.InvariantCulture);
        }
    }

    #endregion


    class UsersWithoutActiveTasksReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.UsersWithoutActiveTasks;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            if (filter.ProjectIds.Count == 0)
            {
                filter.ProjectIds = Global.EngineFactory.GetTagEngine().GetTagProjects(filter.TagId).ToList();
            }

            var result = Global.EngineFactory.GetReportEngine().BuildUsersWithoutActiveTasks(filter);
            result = AddUserInfo(result, 0).ToList();
            result = result.OrderBy(r => CoreContext.UserManager.GetUsers((Guid)r[0]), UserInfoComparer.Default).ToList();

            return result;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnUserName,
                ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                ProjectsCommonResource.Total };
        }

        public override ReportInfo GetReportInfo()
        {
            var userColumns = new[] {
                ReportResource.User,
                "Not Accept",                
                ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                ReportResource.ActiveTasks, 
                ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                ProjectsCommonResource.Total,
                ReportResource.ClickToSortByThisColumn,
                CommonLinkUtility.ServerRootPath };

            return new ReportInfo(
                                    String.Format(ReportResource.ReportEmployeesWithoutActiveTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                                    CustomNamingPeople.Substitute<ReportResource>("ReportEmployeesWithoutActiveTasks_Title").HtmlEncode(),
                                    userColumns);
        }

        public override string GetReportFileName()
        {
            return CustomNamingPeople.Substitute<ReportResource>("ReportEmployeesWithoutActiveTasks_Title").HtmlEncode();
        }
    }

    class UsersWorkloadReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.UsersWorkload;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {

            if (filter.TagId != 0 && filter.ProjectIds.Count == 0)
            {
                filter.ProjectIds = Global.EngineFactory.GetTagEngine().GetTagProjects(filter.TagId).ToList();
            }

            var result = Global.EngineFactory.GetReportEngine().BuildUsersWorkload(filter);

            result = AddUserInfo(result, 0).ToList();
            result = result.OrderBy(r => CoreContext.UserManager.GetUsers((Guid)r[0]), UserInfoComparer.Default).ToList();

            return result;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnUserName,
                ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                ProjectsCommonResource.Total };
        }

        public override ReportInfo GetReportInfo()
        {
            var userColumns = new[] {
                ReportResource.User,
                "Not Accept",                
                ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                ReportResource.ActiveTasks, 
                ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                ProjectsCommonResource.Total,
                ReportResource.ClickToSortByThisColumn,
                CommonLinkUtility.ServerRootPath };

            return new ReportInfo(
                String.Format(ReportResource.ReportEmployment_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportEmployment_Title,
                userColumns);
        }

        public override string GetReportFileName()
        {
            return ReportResource.ReportEmployment_Title;
        }
    }

    class TimeSpendReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.TimeSpend;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            filter.FromDate = filter.GetFromDate(true);
            filter.ToDate = filter.GetToDate(true);

            var taskTime = Global.EngineFactory.GetTimeTrackingEngine().GetByFilter(filter).Select(r => new object[] { r.Person, r.Task.Project.ID, r.Task.Project.Title, r.Task.ID, r.Task.Title, r.Hours, 0, r.PaymentStatus });

            if (filter.ViewType == 0)
            {
                taskTime = taskTime.GroupBy(r => (Guid)r[0], (a, b) =>
                {
                    var enumerable = b as IList<object[]> ?? b.ToList();
                    var data = (object[])enumerable.First().Clone();
                    data[5] = enumerable.Sum(c => (float)c[5]);
                    data[6] = enumerable.Where(r => (PaymentStatus)r[7] == PaymentStatus.Billed).Sum(c => (float)c[5]);
                    return data;
                });
            }

            if (filter.ViewType == 1)
            {
                taskTime = taskTime.Select(r =>
                {
                    if ((PaymentStatus)r[7] == PaymentStatus.Billed) r[6] = r[5];
                    return r;
                });
            }
            taskTime = AddUserInfo(taskTime, 0);
            taskTime = taskTime.OrderBy(r => CoreContext.UserManager.GetUsers((Guid)r[0]), UserInfoComparer.Default);
            return taskTime;
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnUserName,
                ReportResource.CsvColumnTaskTitle,
                ReportResource.CsvColumnProjectjTitle,
                ProjectsCommonResource.SpentTotally,
                ProjectsCommonResource.SpentBilledTotally,
                TimeTrackingResource.ShortHours,
                TimeTrackingResource.ShortMinutes
            };
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportTimeSpendSummary_Description, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportTimeSpendSummary_Title,
                new[]
                    {
                        ReportResource.User, ProjectsCommonResource.SpentTotally,
                        ProjectsCommonResource.SpentBilledTotally, 
                        ReportResource.ClickToSortByThisColumn, CommonLinkUtility.ServerRootPath, VirtualRoot,
                        TimeTrackingResource.ShortHours, TimeTrackingResource.ShortMinutes
                    });
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportTimeSpendSummary_Title", CultureInfo.InvariantCulture);
        }
    }

    class UsersActivityReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.UsersActivity;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            var result = Global.EngineFactory.GetReportEngine().BuildUsersActivityReport(filter);
            result = AddUserInfo(result, 0).ToList();
            return result
                .OrderBy(r => CoreContext.UserManager.GetUsers((Guid)r[0]), UserInfoComparer.Default)
                .ToList();
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { 
                ReportResource.CsvColumnUserName,
                TaskResource.Tasks,
                MilestoneResource.Milestones,
                MessageResource.Messages,
                ProjectsFileResource.Files,
                ProjectsCommonResource.Total };
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo(
                String.Format(ReportResource.ReportUserActivity_Descripton, "<ul>", "</ul>", "<li>", "</li>"),
                ReportResource.ReportUserActivity_Title,
                new[]
                    {
                        ReportResource.User, TaskResource.Tasks, MilestoneResource.Milestones, MessageResource.Messages,
                        ProjectsFileResource.Files, ProjectsCommonResource.Total, ReportResource.ClickToSortByThisColumn,
                        CommonLinkUtility.ServerRootPath
                    });
        }

        public override string GetReportFileName()
        {
            return ReportResource.ResourceManager.GetString("ReportUserActivity_Title", CultureInfo.InvariantCulture);
        }
    }

    class EmptyReport : ExtendedReportType
    {
        public override ReportType GetReportType()
        {
            return ReportType.EmptyReport;
        }

        public override IEnumerable<object[]> BuildReport(TaskFilter filter)
        {
            return new List<object[]>();
        }

        public override string[] GetCsvColumnsName()
        {
            return new[] { ProjectsCommonResource.NoData };
        }

        public override ReportInfo GetReportInfo()
        {
            return new ReportInfo("", "", new[] { ProjectsCommonResource.NoData });
        }

        public override string GetReportFileName()
        {
            return "EmptyReport";
        }
    }
}