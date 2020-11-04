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
using System.Globalization;
using System.IO;
using System.Linq;

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Projects.Engine;
using ASC.Web.Core.Helpers;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Projects.Core;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;

using Autofac;

using Newtonsoft.Json;


namespace ASC.Web.Projects.Classes
{
    public class Report
    {
        private static string GetDocbuilderMasterTemplate { get; set; }

        static Report()
        {
            GetDocbuilderMasterTemplate = GetDocbuilderTemplate("master", -1);
        }

        private ExtendedReportType ExtendedReportType { get; set; }

        public TaskFilter Filter { get; set; }

        public ReportType ReportType
        {
            get { return ExtendedReportType.ReportType; }
        }

        public ReportInfo ReportInfo
        {
            get { return ExtendedReportType.ReportInfo; }
        }

        public string FileName
        {
            get
            {
                var date = TenantUtil.DateTimeNow();
                return string.Format("{0} ({1} {2}).xlsx",
                               ExtendedReportType.ReportFileName.Replace(' ', '_'),
                               date.ToShortDateString(),
                               date.ToShortTimeString());
            }
        }

        private Report(ExtendedReportType reportType, TaskFilter filter)
        {
            ExtendedReportType = reportType;
            Filter = filter;
        }

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

        public static bool TryCreateReport(string query, out ReportState state)
        {
            var p = ReportFilterSerializer.GetParameterFromUri(query, "reportType");
            if (string.IsNullOrEmpty(p))
                throw new Exception(ReportResource.ErrorParse);

            ReportType reportType;
            if (!Enum.TryParse(p, out reportType))
                throw new Exception(ReportResource.ErrorParse);

            var filter = ReportFilterSerializer.FromUri(query);

            var template = new ReportTemplate(reportType) { Id = -1, Filter = filter, CreateBy = SecurityContext.CurrentAccount.ID };

            return TryCreateReportFromTemplate(template, template.SaveDocbuilderReport, null, out state);
        }

        public static bool TryCreateReportFromTemplate(ReportTemplate template, Action<ReportState, string> callback, object obj, out ReportState state, bool auto = false)
        {
            var report = CreateNewReport(template.ReportType, template.Filter);
            template.Name = report.FileName;

            var data = report.GetDocbuilderData(template.Id);

            var dataJson = JsonConvert.SerializeObject(data);
            var columnsJson = JsonConvert.SerializeObject(report.ExtendedReportType.ColumnsName);
            var filterJson = JsonConvert.SerializeObject(template.Filter);

            var userCulture = CoreContext.UserManager.GetUsers(template.CreateBy).GetCulture();
            var reportInfoJson = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "Title", report.ReportInfo.Title },
                { "CreatedText", ReportResource.ReportCreated },
                { "CreatedAt", TenantUtil.DateTimeNow().ToString("M/d/yyyy", CultureInfo.InvariantCulture) },
                { "CreatedBy", ProjectsFilterResource.By + " " + CoreContext.UserManager.GetUsers(template.CreateBy).DisplayUserName(false) },
                { "DateFormat", userCulture.DateTimeFormat.ShortDatePattern }
            });

            var tmpFileName = DocbuilderReportsUtility.TmpFileName;

            var script = GetDocbuilderMasterTemplate
                .Replace("${outputFilePath}", tmpFileName)
                .Replace("${reportData}", dataJson)
                .Replace("${reportColumn}", columnsJson)
                .Replace("${reportFilter}", filterJson)
                .Replace("${reportInfo}", reportInfoJson.Replace("\"", "\\\""))
                .Replace("${templateBody}", GetDocbuilderTemplate(report.ReportType.ToString(), report.Filter.ViewType));

            state = new ReportState(report.FileName, tmpFileName, script, (int)template.ReportType, auto ? ReportOrigin.ProjectsAuto : ReportOrigin.Projects, callback, obj);

            if (data.Count == 0)
            {
                state.Exception = ReportResource.ErrorEmptyData;
                state.Status = ReportStatus.Failed;
                return false;
            }

            DocbuilderReportsUtility.Enqueue(state);

            return true;
        }

        private List<object[]> GetDocbuilderData(int templateID)
        {
            PrepareFilter(templateID);
            return ExtendedReportType.BuildDocbuilderReport(Filter).ToList();
        }

        private void PrepareFilter(int templateID)
        {
            if (templateID != 0 && !Filter.FromDate.Equals(DateTime.MinValue))
            {
                var interval = Filter.ToDate.DayOfYear - Filter.FromDate.DayOfYear;

                switch (ExtendedReportType.ReportType)
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
        }

        private static string GetDocbuilderTemplate(string fileName, int v)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(string.Format("ASC.Web.Projects.DocbuilderTemplates.{0}_{1}.docbuilder", fileName, v)) ??
                assembly.GetManifestResourceStream(string.Format("ASC.Web.Projects.DocbuilderTemplates.{0}.docbuilder", fileName)))
            {
                if (stream != null)
                {
                    using (var sr = new StreamReader(stream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            throw new Exception(ReportResource.ErrorCreatingScript);
        }
    }


    #region Reports

    public abstract class ExtendedReportType
    {
        public ReportType ReportType { get; private set; }

        public abstract string[] ColumnsName { get; }

        public abstract ReportInfo ReportInfo { get; }

        public abstract string ReportFileName { get; }

        public abstract IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter);

        protected string VirtualRoot
        {
            get { return CommonLinkUtility.VirtualRoot != "/" ? CommonLinkUtility.VirtualRoot : string.Empty; }
        }

        protected string VirtulaRootPath
        {
            get { return CommonLinkUtility.ServerRootPath + VirtualRoot; }
        }

        protected ExtendedReportType(ReportType reportType)
        {
            ReportType = reportType;
        }
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

        public MilestonesReport(ReportType reportType)
            : base(reportType)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<EngineFactory>().MilestoneEngine
                    .GetByFilter(filter)
                    .OrderBy(r => r.Project.Title)
                    .Select(r => new object[]
                    {
                        r.Project.Title, CoreContext.UserManager.GetUsers(r.Project.Responsible).DisplayUserName(false),
                        r.Title, CoreContext.UserManager.GetUsers(r.Responsible).DisplayUserName(false), r.DeadLine.ToString("d"),
                        (int)((DateTime.Now - r.DeadLine).TotalDays)
                    });
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    MilestoneResource.Milestone,
                    MilestoneResource.Responsible,
                    MilestoneResource.MilestoneDeadline,
                    MilestoneResource.Overdue + ", " + MilestoneResource.Days
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get { throw new NotImplementedException(); }
        }

        public override string ReportFileName
        {
            get
            {
                return ReportResource.ReportLateMilestones_Title;
            }
        }
    }

    class MilestonesExpiredReport : MilestonesReport
    {
        public MilestonesExpiredReport() : base(ReportType.MilestonesExpired)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            filter.ToDate = TenantUtil.DateTimeNow();
            filter.MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open };

            return base.BuildDocbuilderReport(filter);
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(
                        String.Format(ReportResource.ReportLateMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                        ReportResource.ReportLateMilestones_Title,
                        MileColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportLateMilestones_Title; }
        }
    }

    class MilestonesNearestReport : MilestonesReport
    {
        public MilestonesNearestReport() : base(ReportType.MilestonesNearest)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            filter.MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open };

            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<EngineFactory>().MilestoneEngine
                    .GetByFilter(filter)
                    .OrderBy(r => r.Project.Title)
                    .Where(r => ((r.DeadLine - DateTime.Now).TotalDays) > 0)
                    .Select(r => new object[]
                    {
                        r.Project.Title, CoreContext.UserManager.GetUsers(r.Project.Responsible).DisplayUserName(false),
                        r.Title, CoreContext.UserManager.GetUsers(r.Responsible).DisplayUserName(false), r.DeadLine.ToString("d"),
                        (int)((r.DeadLine - DateTime.Now).TotalDays)
                    });
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    MilestoneResource.Milestone,
                    MilestoneResource.Responsible,
                    MilestoneResource.MilestoneDeadline,
                    MilestoneResource.Next + ", " + MilestoneResource.Days
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(String.Format(ReportResource.ReportUpcomingMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                        ReportResource.ReportUpcomingMilestones_Title,
                        MileColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportUpcomingMilestones_Title; }
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
                               GrammaticalResource.TaskGenitivePlural,
                               ReportResource.Participiants,
                               ReportResource.ClickToSortByThisColumn,
                               CommonLinkUtility.ServerRootPath,
                               VirtualRoot
                           };
            }
        }

        public ProjectsListReport() : base(ReportType.ProjectsList)
        {
        }

        public ProjectsListReport(ReportType reportType)
            : base(reportType)
        {
        }


        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            filter.SortBy = "title";
            filter.SortOrder = true;

            using (var scope = DIHelper.Resolve())
            {
                var result = scope.Resolve<EngineFactory>().ProjectEngine
                    .GetByFilter(filter)
                    .Select(r => new object[]
                    {
                        r.Title, CoreContext.UserManager.GetUsers(r.Responsible).DisplayUserName(false),
                        LocalizedEnumConverter.ConvertToString(r.Status),
                        r.MilestoneCount, r.TaskCount, r.ParticipantCount
                    });

                result = result.OrderBy(r => (string)r[1]);

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ProjectsCommonResource.Title,
                    ProjectResource.ProjectLeader,
                    ProjectsCommonResource.Status,
                    GrammaticalResource.MilestoneGenitivePlural,
                    GrammaticalResource.TaskGenitivePlural,
                    ReportResource.Participiants,
                    LocalizedEnumConverter.ConvertToString(ProjectStatus.Open)
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(
                    String.Format(ReportResource.ReportProjectList_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportProjectList_Title,
                    ProjColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportProjectList_Title; }
        }
    }

    class ProjectsWithoutActiveMilestonesReport : ProjectsListReport
    {
        public ProjectsWithoutActiveMilestonesReport()
            : base(ReportType.ProjectsWithoutActiveMilestones)
        {
        }


        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            return base.BuildDocbuilderReport(filter).Where(r => (int)r[3] == 0);
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(String.Format(ReportResource.ReportProjectsWithoutActiveMilestones_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportProjectsWithoutActiveMilestones_Title,
                    ProjColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportProjectsWithoutActiveMilestones_Title; }
        }
    }

    class ProjectsWithoutActiveTasksReport : ProjectsListReport
    {
        public ProjectsWithoutActiveTasksReport() : base(ReportType.ProjectsWithoutActiveTasks)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            return base.BuildDocbuilderReport(filter).Where(r => (int)r[4] == 0);
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(String.Format(ReportResource.ReportProjectsWithoutActiveTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportProjectsWithoutActiveTasks_Title,
                    ProjColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportProjectsWithoutActiveTasks_Title; }
        }
    }

    #endregion

    #region TaskReports

    class TasksReport : ExtendedReportType
    {
        public TasksReport(ReportType reportType) : base(reportType)
        {
        }

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

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
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

            using (var scope = DIHelper.Resolve())
            {
                var tasks = scope.Resolve<EngineFactory>().TaskEngine.GetByFilter(filter)
                        .FilterResult.OrderBy(r => r.Project.Title)
                        .ToList();

                filter.FromDate = createdFrom;
                filter.ToDate = createdTo;

                if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MinValue))
                    tasks =
                        tasks.Where(r => r.CreateOn.Date >= filter.FromDate && r.CreateOn.Date <= filter.ToDate)
                            .ToList();

                if (!filter.NoResponsible)
                    tasks = tasks.Where(r => r.Responsibles.Any()).ToList();

                var projects = tasks.Select(r => r.Project).Distinct();
                var result = new List<object[]>();

                foreach (var proj in projects)
                    result.Add(new object[] {
                        new object[] { proj.Title, LocalizedEnumConverter.ConvertToString(proj.Status),
                            CoreContext.UserManager.GetUsers(proj.Responsible).DisplayUserName(false), proj.CreateOn.ToString("d"), proj.Description },
                        tasks.Where(r => r.Project.ID == proj.ID).Select(r => new object[]
                        {
                            r.Title,
                            r.MilestoneDesc != null ? r.MilestoneDesc.Title : "",
                            string.Join(", ", r.Responsibles.Select(x => CoreContext.UserManager.GetUsers(x).DisplayUserName(false))),
                            r.Deadline.Equals(DateTime.MinValue) ? ( r.MilestoneDesc == null ? "" : r.MilestoneDesc.DeadLine.ToString("d") ) : r.Deadline.ToString("d"),
                            LocalizedEnumConverter.ConvertToString(r.Status)
                        })
                    });

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ProjectsCommonResource.Status,
                    ProjectResource.ProjectLeader,
                    TaskResource.CreatingDate,
                    ProjectsCommonResource.Description,

                    TaskResource.Task,
                    GrammaticalResource.ResponsibleNominativePlural,
                    MilestoneResource.MilestoneDeadline,
                    ProjectsCommonResource.Status,
                    LocalizedEnumConverter.ConvertToString(TaskStatus.Open)
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get { throw new NotImplementedException(); }
        }

        public override string ReportFileName
        {
            get { throw new NotImplementedException(); }
        }
    }

    class TasksByProjectsReport : TasksReport
    {
        public TasksByProjectsReport() : base(ReportType.TasksByProjects)
        {
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(
                    String.Format(ReportResource.ReportTaskList_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportTaskList_Title,
                    TaskColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportTaskList_Title; }
        }
    }

    class TasksByUsersReport : TasksReport
    {
        public TasksByUsersReport() : base(ReportType.TasksByUsers)
        {
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                return new ReportInfo(
                    String.Format(ReportResource.ReportUserTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportUserTasks_Title,
                    TaskColumns);
            }
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
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

            using (var scope = DIHelper.Resolve())
            {
                var tasks = scope.Resolve<EngineFactory>().TaskEngine.GetByFilter(filter)
                        .FilterResult.OrderBy(r => r.Project.Title)
                        .ToList();

                filter.FromDate = createdFrom;
                filter.ToDate = createdTo;

                if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MinValue))
                    tasks =
                        tasks.Where(r => r.CreateOn.Date >= filter.FromDate && r.CreateOn.Date <= filter.ToDate)
                            .ToList();

                var result = new List<object[]>();

                var users = filter.ParticipantId.HasValue ? new List<Guid> { filter.ParticipantId.Value } : tasks.SelectMany(r => r.Responsibles).Distinct();
                foreach (var user in users)
                {
                    var userTasks = tasks.Where(r => r.Responsibles.Contains(user));
                    var userProj = userTasks.Select(r => r.Project);
                    var projData = new List<object[]>();
                    foreach (var pr in userProj)
                    {
                        var prTasks = userTasks.Where(r => r.Project == pr);
                        projData.Add(new object[] { pr.Title, CoreContext.UserManager.GetUsers(pr.Responsible).DisplayUserName(false),
                        prTasks.Select(r => new object[] { r.Title,
                            r.Deadline.Equals(DateTime.MinValue) ? ( r.MilestoneDesc == null ? "" : r.MilestoneDesc.DeadLine.ToString("d") ) : r.Deadline.ToString("d"),
                            LocalizedEnumConverter.ConvertToString(r.Status),
                            LocalizedEnumConverter.ConvertToString(r.Priority), r.StartDate.Equals(DateTime.MinValue) ? "" : r.StartDate.ToString("d") })});
                    }

                    result.Add(new object[] { CoreContext.UserManager.GetUsers(user).DisplayUserName(false), projData });
                }

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ProjectResource.ProjectLeader,
                    TaskResource.Task,
                    MilestoneResource.MilestoneDeadline,
                    ProjectsCommonResource.Status,
                    TaskResource.Priority,
                    TaskResource.TaskStartDate,
                    LocalizedEnumConverter.ConvertToString(TaskStatus.Open)
                };
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportUserTasks_Title; }
        }
    }

    class TasksExpiredReport : ExtendedReportType
    {
        public TasksExpiredReport() : base(ReportType.TasksExpired)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            filter.FromDate = new DateTime(1970, 1, 1);
            filter.ToDate = TenantUtil.DateTimeNow();
            filter.TaskStatuses.Add(TaskStatus.Open);

            using (var scope = DIHelper.Resolve())
            {
                var tasks = scope.Resolve<EngineFactory>().TaskEngine.GetByFilter(filter)
                        .FilterResult.OrderBy(r => r.Project.Title)
                        .ToList();

                var projects = tasks.Select(r => r.Project).Distinct();

                var result = new List<object[]>();

                foreach (var pr in projects)
                    result.Add(new object[] { pr.Title, CoreContext.UserManager.GetUsers(pr.Responsible).DisplayUserName(false),
                        tasks.Where(r => r.Project.ID == pr.ID).Select(r => new object[]
                {
                    r.Title,
                    string.Join(", ", r.Responsibles.Select(x => CoreContext.UserManager.GetUsers(x).DisplayUserName(false))),
                    r.Deadline.Equals(DateTime.MinValue) ? ( r.MilestoneDesc == null ? "" : r.MilestoneDesc.DeadLine.ToString("d") ) : r.Deadline.ToString("d"),
                    (int)((DateTime.Now - (DateTime.MinValue.Equals(r.Deadline) ? r.MilestoneDesc.DeadLine : r.Deadline)).TotalDays)
                })});

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ProjectResource.ProjectLeader,

                    TaskResource.Task,
                    GrammaticalResource.ResponsibleNominativePlural,
                    MilestoneResource.MilestoneDeadline,
                    MilestoneResource.Overdue + ", " + MilestoneResource.Days
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                var taskExpiredColumns = new[]
                {
                    ProjectResource.Project,
                    MilestoneResource.Milestone,
                    TaskResource.Task,
                    TaskResource.TaskResponsible,
                    ProjectsCommonResource.Status,
                    TaskResource.UnsortedTask,
                    TaskResource.DeadLine,
                    ReportResource.NoMilestonesAndTasks,
                    CommonLinkUtility.ServerRootPath,
                    VirtualRoot
                };

                return new ReportInfo(
                    String.Format(ReportResource.ReportLateTasks_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportLateTasks_Title,
                    taskExpiredColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportLateTasks_Title; }
        }
    }

    #endregion


    class UsersWithoutActiveTasksReport : ExtendedReportType
    {
        public UsersWithoutActiveTasksReport() : base(ReportType.UsersWithoutActiveTasks)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<EngineFactory>();
                if (filter.ProjectIds.Count == 0)
                {
                    filter.ProjectIds = factory.TagEngine.GetTagProjects(filter.TagId).ToList();
                }

                var result = scope.Resolve<EngineFactory>().ReportEngine.BuildUsersWithoutActiveTasks(filter);

                result = result.OrderBy(r => (string)r[0]).ToList();

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ReportResource.CsvColumnUserName,
                    ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                    ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                    ProjectsCommonResource.Total
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                var userColumns = new[]
                {
                    ReportResource.User,
                    "Not Accept",
                    ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                    ReportResource.ActiveTasks,
                    ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                    ProjectsCommonResource.Total,
                    ReportResource.ClickToSortByThisColumn,
                    CommonLinkUtility.ServerRootPath
                };

                return new ReportInfo(
                    String.Format(ReportResource.ReportEmployeesWithoutActiveTasks_Description, "<ul>", "</ul>", "<li>",
                        "</li>"),
                    CustomNamingPeople.Substitute<ReportResource>("ReportEmployeesWithoutActiveTasks_Title")
                        .HtmlEncode(),
                    userColumns);
            }
        }

        public override string ReportFileName
        {
            get
            {
                return CustomNamingPeople.Substitute<ReportResource>("ReportEmployeesWithoutActiveTasks_Title").HtmlEncode();
            }
        }
    }

    class UsersWorkloadReport : ExtendedReportType
    {
        public UsersWorkloadReport() : base(ReportType.UsersWorkload)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<EngineFactory>();
                if (filter.TagId != 0 && filter.ProjectIds.Count == 0)
                {
                    filter.ProjectIds = factory.TagEngine.GetTagProjects(filter.TagId).ToList();
                }

                var result = factory.ReportEngine.BuildUsersWorkload(filter);

                result = result.OrderBy(r => (string)r[0]).ToList();

                return result;
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ReportResource.CsvColumnUserName,
                    ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                    ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                    ProjectsCommonResource.Total
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
            {
                var userColumns = new[]
                {
                    ReportResource.User,
                    "Not Accept",
                    ResourceEnumConverter.ConvertToString(TaskStatus.Open),
                    ReportResource.ActiveTasks,
                    ResourceEnumConverter.ConvertToString(TaskStatus.Closed),
                    ProjectsCommonResource.Total,
                    ReportResource.ClickToSortByThisColumn,
                    CommonLinkUtility.ServerRootPath
                };

                return new ReportInfo(
                    String.Format(ReportResource.ReportEmployment_Description, "<ul>", "</ul>", "<li>", "</li>"),
                    ReportResource.ReportEmployment_Title,
                    userColumns);
            }
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportEmployment_Title; }
        }
    }

    class TimeSpendReport : ExtendedReportType
    {
        public TimeSpendReport() : base(ReportType.TimeSpend)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            filter.FromDate = filter.GetFromDate(true);
            filter.ToDate = filter.GetToDate(true);

            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<EngineFactory>();


                IEnumerable<object[]> taskTime = new List<object[]>();
                switch (filter.ViewType)
                {
                    case 0:
                        taskTime = factory.TimeTrackingEngine.GetByFilter(filter)
                            .Select(r =>
                                new object[]
                                {
                                    CoreContext.UserManager.GetUsers(r.Person).DisplayUserName(false),
                                    r.Hours, 0, r.PaymentStatus, r.PaymentStatus == PaymentStatus.NotChargeable ? "+" : ""
                                });

                        taskTime = taskTime.GroupBy(r => (string)r[0], (a, b) =>
                            {
                                var enumerable = b as IList<object[]> ?? b.ToList();
                                var data = (object[])enumerable.First().Clone();
                                data[1] = enumerable.Sum(c => (float)c[1]);
                                data[2] = enumerable.Where(r => (PaymentStatus)r[3] == PaymentStatus.Billed).Sum(c => (float)c[1]);
                                return data;
                            });
                        return taskTime.OrderBy(r => (string)r[0]);

                    case 1:
                        taskTime = factory.TimeTrackingEngine.GetByFilter(filter)
                            .Select(r =>
                                new object[]
                                {
                                    CoreContext.UserManager.GetUsers(r.Person).DisplayUserName(false),
                                    r.Hours, 0, r.PaymentStatus, r.PaymentStatus == PaymentStatus.NotChargeable ? "+" : "",
                                    r.Task.Project, r.Task.Title
                                });

                        taskTime = taskTime.Select(r =>
                        {
                            if ((PaymentStatus)r[3] == PaymentStatus.Billed) r[2] = r[1];
                            return r;
                        });

                        var users = taskTime.GroupBy(x => x[0]).Select(x => new object[] { x.Key, x.ToList()
                            .Select(r => new object[] { r[1], r[2], r[3], r[4], r[5], r[6] })});


                        var result = new List<object[]>();
                        foreach (var user in users) // user = [string, []]
                        {
                            var userTasks = (IEnumerable<object[]>)user[1];

                            var groupedUserTasks = userTasks.GroupBy(x => x[4]).Select(r => new object[] {
                            new object[] { ((Project)r.Key).Title, ProjectsCommonResource.Status + ": " + LocalizedEnumConverter.ConvertToString(((Project)r.Key).Status),
                            ProjectResource.ProjectLeader + ": " + CoreContext.UserManager.GetUsers(((Project)r.Key).Responsible).DisplayUserName(false),
                            TaskResource.CreatingDate + ": " + ((Project)r.Key).CreateOn.ToString("d"),
                            ((Project)r.Key).Description != "" ? ProjectsCommonResource.Description + ": " + ((Project)r.Key).Description : ""
                            }, r.ToList().Select(y => new object[] { y[0], y[1], y[2], y[3], y[5] } )});

                            result.Add(new object[] { user[0], groupedUserTasks });
                        }

                        return result;

                    case 2:
                        taskTime = factory.TimeTrackingEngine.GetByFilter(filter)
                            .Select(r =>
                                new object[]
                                {
                                    CoreContext.UserManager.GetUsers(r.Person).DisplayUserName(false),
                                    r.Hours, 0, r.PaymentStatus, r.PaymentStatus == PaymentStatus.NotChargeable ? "+" : "",
                                    r.Task.Project.Title
                                });

                        return taskTime.GroupBy(x => (string)x[5]).Select(x => new object[] {x.Key, x.GroupBy(r => (string)r[0], (a, b) =>
                            {
                                var enumerable = b as IList<object[]> ?? b.ToList();
                                var data = (object[])enumerable.First().Clone();
                                data[1] = enumerable.Sum(c => (float)c[1]);
                                data[2] = enumerable.Where(r => (PaymentStatus)r[3] == PaymentStatus.Billed).Sum(c => (float)c[1]);
                                return data;
                            })
                        }).OrderBy(x => (string)x[0]);

                    default:
                        throw new Exception(ProjectsCommonResource.NoData);
                }
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ReportResource.CsvColumnUserName,
                    TaskResource.Task,
                    ProjectsCommonResource.Total,
                    ProjectsCommonResource.SpentTotally,
                    ReportResource.Billed,
                    TimeTrackingResource.PaymentStatusNotChargeable,
                    TimeTrackingResource.ShortHours,
                    TimeTrackingResource.ShortMinutes
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
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
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportTimeSpendSummary_Title; }
        }
    }

    class UsersActivityReport : ExtendedReportType
    {
        public UsersActivityReport() : base(ReportType.UsersActivity)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            using (var scope = DIHelper.Resolve())
            {
                var result = scope.Resolve<EngineFactory>().ReportEngine.BuildUsersActivity(filter);
                return result
                    .OrderBy(r => (string)r[1])
                    .ToList();
            }
        }

        public override string[] ColumnsName
        {
            get
            {
                return new[]
                {
                    ReportResource.CsvColumnUserName,
                    TaskResource.Tasks,
                    MilestoneResource.Milestones,
                    MessageResource.Messages,
                    ProjectsCommonResource.Total
                };
            }
        }

        public override ReportInfo ReportInfo
        {
            get
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
        }

        public override string ReportFileName
        {
            get { return ReportResource.ReportUserActivity_Title; }
        }
    }

    class EmptyReport : ExtendedReportType
    {
        public EmptyReport() : base(ReportType.EmptyReport)
        {
        }

        public override IEnumerable<object[]> BuildDocbuilderReport(TaskFilter filter)
        {
            return new List<object[]>();
        }

        public override string[] ColumnsName
        {
            get { return new[] { ProjectsCommonResource.NoData }; }
        }

        public override ReportInfo ReportInfo
        {
            get { return new ReportInfo("", "", new[] { ProjectsCommonResource.NoData }); }
        }

        public override string ReportFileName
        {
            get { return "EmptyReport"; }
        }
    }

    #endregion
}