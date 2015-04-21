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
using System.Data;
using System.Globalization;
using System.Linq;

using ASC.Collections;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.FullTextIndex.Service;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    internal class CachedTaskDao : TaskDao
    {
        private readonly HttpRequestDictionary<Task> taskCache = new HttpRequestDictionary<Task>("task");

        public CachedTaskDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        public override Task GetById(int id)
        {
            return taskCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private Task GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Task Save(Task task)
        {
            if (task != null)
            {
                ResetCache(task.ID);
            }
            return base.Save(task);
        }

        private void ResetCache(int taskId)
        {
            taskCache.Reset(taskId.ToString(CultureInfo.InvariantCulture));
        }
    }


    class TaskDao : BaseDao, ITaskDao
    {
        public static readonly string[] TaskColumns = new[] { "id", "title", "description", "status", "create_by", "create_on", "last_modified_by", "last_modified_on", "priority", "milestone_id", "sort_order", "deadline", "start_date", "progress", "responsibles" };

        public TaskDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }


        #region ITaskDao

        public List<Task> GetAll()
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery()).ConvertAll(ToTask);
            }
        }

        public List<Task> GetByProject(int projectId, TaskStatus? status, Guid participant)
        {
            var query = CreateQuery()
                .LeftOuterJoin(MilestonesTable + " m", Exp.EqColumns("m.id", "t.milestone_id") & Exp.EqColumns("m.tenant_id", "t.tenant_id"))
                .Where("t.project_id", projectId)
                .OrderBy("t.sort_order", false)
                .OrderBy("m.status", true)
                .OrderBy("m.deadLine", true)
                .OrderBy("m.id", true)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", true)
                .OrderBy("t.create_on", true);
            if (status != null)
            {
                if (status == TaskStatus.Open)
                    query.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                else
                    query.Where("t.status", TaskStatus.Closed);
            }

            if (participant != Guid.Empty)
            {
                var existSubtask = new SqlQuery(SubtasksTable + " pst").Select("pst.task_id").Where(Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("t.id", "pst.task_id") & Exp.Eq("pst.status", TaskStatus.Open));
                var existResponsible = new SqlQuery(TasksResponsibleTable + " ptr1").Select("ptr1.task_id").Where(Exp.EqColumns("t.tenant_id", "ptr1.tenant_id") & Exp.EqColumns("t.id", "ptr1.task_id"));

                existSubtask.Where(Exp.Eq("pst.responsible_id", participant.ToString()));
                existResponsible.Where(Exp.Eq("ptr1.responsible_id", participant.ToString()));

                query.Where(Exp.Exists(existSubtask) | Exp.Exists(existResponsible));
            }

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToTask);
            }
        }

        public List<Task> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = CreateQuery()
                .LeftOuterJoin(MilestonesTable + " m", Exp.EqColumns("m.id", "t.milestone_id") & Exp.EqColumns("t.tenant_id", "m.tenant_id"))
                .Select("m.title", "m.deadline");
            
            if (filter.Max > 0 && filter.Max < 150000)
            {
                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max);
            }

            //query.OrderBy("(case t.status when 2 then 1 else 0 end)", true);

            if (filter.TaskStatuses.Count == 1 && filter.TaskStatuses.Contains(TaskStatus.Closed))
            {
                filter.SortBy = "status_changed";
                filter.SortOrder = false;
            }

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortColumns = filter.SortColumns["Task"];
                sortColumns.Remove(filter.SortBy);

                query.OrderBy(GetSortFilter(filter.SortBy, filter.SortOrder), filter.SortOrder);

                foreach (var sort in sortColumns.Keys)
                {
                    query.OrderBy(GetSortFilter(sort, sortColumns[sort]), sortColumns[sort]);
                }
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToTaskMilestone);
            }
        }

        public TaskFilterCountOperationResult GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(TasksTable + " t").Select("t.id", "t.status")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Where("t.tenant_id", Tenant);

            query = CreateQueryFilterCount(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery()
                .SelectCount("t1.id").Select("t1.status")
                .From(query, "t1")
                .GroupBy("status");
            using (var db = new DbManager(DatabaseId))
            {
                var result = db.ExecuteList(queryCount).ToDictionary(r => Convert.ToInt32(r[1]), r => Convert.ToInt32(r[0]));
                int tasksOpen = result.Where(row => row.Key != (int) TaskStatus.Closed).Sum(row => row.Value);
                    //that's right. open its not closed.
                int tasksClosed;
                result.TryGetValue((int) TaskStatus.Closed, out tasksClosed);
                return new TaskFilterCountOperationResult {TasksOpen = tasksOpen, TasksClosed = tasksClosed};
            }
        }

        public List<Task> GetByResponsible(Guid responsibleId, IEnumerable<TaskStatus> statuses)
        {
            var q = CreateQuery()
                .LeftOuterJoin(SubtasksTable + " pst", Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("t.id", "pst.task_id"))
                .Where((Exp.Eq("pst.responsible_id", responsibleId) & !Exp.Eq("Coalesce(pst.status,-1)", TaskStatus.Closed)) | Exp.Eq("ptr.responsible_id", responsibleId))
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", true)
                .OrderBy("t.create_on", true);

            if (statuses != null && statuses.Any())
            {
                var status = statuses.First();
                if (status == TaskStatus.Open)
                {
                    q.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                }
                else
                {
                    q.Where("t.status", TaskStatus.Closed);
                }
            }

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(q).ConvertAll(ToTask);
            }
        }

        public List<Task> GetMilestoneTasks(int milestoneId)
        {
            var query = CreateQuery()
                .Where("t.milestone_id", milestoneId)
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", false)
                .OrderBy("t.create_on", false);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToTask);
            }
        }

        public List<Task> GetById(ICollection<int> ids)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = CreateQuery().Where(Exp.In("t.id", ids.ToArray()));
                return db.ExecuteList(query).ConvertAll(ToTask);
            }
        }

        public virtual Task GetById(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = CreateQuery().Where("t.id", id);
                return db.ExecuteList(query).ConvertAll(ToTask).SingleOrDefault();
            }
        }

        public bool IsExists(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var count = db.ExecuteScalar<long>(Query(TasksTable).SelectCount().Where("id", id));
                return 0 < count;
            }
        }

        public List<object[]> GetTasksForReminder(DateTime deadline)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var q = new SqlQuery(TasksTable)
                    .Select("tenant_id", "id", "deadline")
                    .Where(Exp.Between("deadline", deadline.Date.AddDays(-1), deadline.Date.AddDays(1)))
                    .Where(!Exp.Eq("status", TaskStatus.Closed));

                return db.ExecuteList(q)
                    .ConvertAll(r => new object[] {Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToDateTime(r[2])});
            }
        }

        public virtual Task Save(Task task)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tr = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    task.Responsibles.RemoveAll(r => r.Equals(Guid.Empty));

                    if (task.Deadline.Kind != DateTimeKind.Local && task.Deadline != DateTime.MinValue)
                        task.Deadline = TenantUtil.DateTimeFromUtc(task.Deadline);

                    if (task.StartDate.Kind != DateTimeKind.Local && task.StartDate != DateTime.MinValue)
                        task.StartDate = TenantUtil.DateTimeFromUtc(task.StartDate);

                    var insert = Insert(TasksTable)
                        .InColumnValue("id", task.ID)
                        .InColumnValue("project_id", task.Project != null ? task.Project.ID : 0)
                        .InColumnValue("title", task.Title)
                        .InColumnValue("create_by", task.CreateBy.ToString())
                        .InColumnValue("create_on", TenantUtil.DateTimeToUtc(task.CreateOn))
                        .InColumnValue("last_modified_by", task.LastModifiedBy.ToString())
                        .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(task.LastModifiedOn))
                        .InColumnValue("description", task.Description)
                        .InColumnValue("priority", task.Priority)
                        .InColumnValue("status", task.Status)
                        .InColumnValue("milestone_id", task.Milestone)
                        .InColumnValue("sort_order", task.SortOrder)
                        .InColumnValue("deadline", task.Deadline)
                        .InColumnValue("status_changed", task.StatusChangedOn)
                        .InColumnValue("start_date", task.StartDate)
                        .InColumnValue("progress", task.Progress)
                        .Identity(1, 0, true);

                    task.ID = db.ExecuteScalar<int>(insert);

                    db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where("task_id", task.ID));

                    if (task.Responsibles.Any())
                    {
                        insert = new SqlInsert(TasksResponsibleTable).InColumns("tenant_id", "task_ID", "responsible_id");

                        foreach (var responsible in task.Responsibles.Distinct())
                        {
                            insert.Values(Tenant, task.ID, responsible);
                        }

                        db.ExecuteNonQuery(insert);
                    }

                    tr.Commit();
                }
                return task;
            }
        }

        public virtual void Delete(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(Delete(CommentsTable).Where("target_uniq_id", ProjectEntity.BuildUniqId<Task>(id)));
                    db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where("task_id", id));
                    db.ExecuteNonQuery(Delete(TasksTable).Where("id", id));
                    db.ExecuteNonQuery(Delete(SubtasksTable).Where("task_id", id));

                    tx.Commit();
                }
            }
        }

        #region Recurrence

        public List<object[]> GetRecurrence(DateTime date)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var q = new SqlQuery("projects_tasks")
                    .Select("tenant_id", "task_id")
                    .InnerJoin("projects_tasks_recurrence as ptr", Exp.Eq("ptr.task_id", "t.id"))
                    .Where(Exp.Ge("ptr.start_date", date))
                    .Where(Exp.Le("ptr.end_date", date))
                    .Where(Exp.Eq("t.status", TaskStatus.Open));

                return db.ExecuteList(q).ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]) });
            }
        }

        public void SaveRecurrence(Task task, string cron, DateTime startDate, DateTime endDate)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tr = db.Connection.BeginTransaction())
                {
                    Insert("projects_tasks_recurrence")
                        .InColumnValue("task_id", task.ID)
                        .InColumnValue("cron", cron)
                        .InColumnValue("title", task.Title)
                        .InColumnValue("startDate", startDate)
                        .InColumnValue("endDate", endDate);

                    tr.Commit();
                }
            }
        }

        public void DeleteReccurence(int taskId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(Delete("projects_tasks_recurrence").Where("task_id", taskId));
                    tx.Commit();
                }
            }
        }

        #endregion

        #region Link

        public void AddLink(TaskLink link)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Insert(TasksLinksTable)
                    .InColumnValue("task_id", link.DependenceTaskId)
                    .InColumnValue("parent_id", link.ParentTaskId)
                    .InColumnValue("link_type", link.LinkType);

                db.ExecuteNonQuery(query);
            }
        }

        public void RemoveLink(TaskLink link)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Delete(TasksLinksTable)
                    .Where((Exp.Eq("task_id", link.DependenceTaskId) & Exp.Eq("parent_id", link.ParentTaskId)) |
                           (Exp.Eq("task_id", link.ParentTaskId) & Exp.Eq("parent_id", link.DependenceTaskId)));

                db.ExecuteNonQuery(query);
            }
        }

        public bool IsExistLink(TaskLink link)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Query(TasksLinksTable)
                    .SelectCount()
                    .Where((Exp.Eq("task_id", link.DependenceTaskId) & Exp.Eq("parent_id", link.ParentTaskId)) |
                           (Exp.Eq("task_id", link.ParentTaskId) & Exp.Eq("parent_id", link.DependenceTaskId)));

                return db.ExecuteScalar<long>(query) > 0;
            }
        }

        public IEnumerable<TaskLink> GetLinks(int taskID)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Query(TasksLinksTable)
                    .Select("task_id", "parent_id", "link_type")
                    .Where(Exp.Eq("task_id", taskID) | Exp.Eq("parent_id", taskID));

                return db.ExecuteList(query).ConvertAll(ToTaskLink);
            }
        }

        public IEnumerable<TaskLink> GetLinks(List<Task> tasks)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Query(TasksLinksTable)
                    .Select("task_id", "parent_id", "link_type")
                    .Where(Exp.Or(Exp.In("task_id", tasks.Select(r => r.ID).ToList()),
                                  Exp.In("parent_id", tasks.Select(r => r.ID).ToList())));

                return db.ExecuteList(query).ConvertAll(ToTaskLink);
            }
        }

        #endregion

        #endregion

        #region Private

        private SqlQuery CreateQuery()
        {
            return new SqlQuery(TasksTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .LeftOuterJoin(TasksResponsibleTable + " ptr", Exp.EqColumns("t.tenant_id", "ptr.tenant_id") & Exp.EqColumns("t.id", "ptr.task_id"))
                .Select(ProjectDao.ProjectColumns.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on")
                .Select("t.description", "t.priority", "t.status", "t.milestone_id", "t.sort_order", "t.deadline", "t.start_date", "t.progress")
                .Select("group_concat(distinct ptr.responsible_id) task_resp")
                .Where("t.tenant_id", Tenant)
                .GroupBy("t.id");
        }

        private SqlQuery CreateQueryFilterBase(SqlQuery query, TaskFilter filter)
        {
            if (filter.ProjectIds.Count != 0)
            {
                query.Where(Exp.In("t.project_id", filter.ProjectIds));
            }
            else
            {
                if (filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("t.tenant_id", "ppp.tenant"));
                    query.Where("ppp.participant_id", CurrentUserID);
                }
            }

            if (filter.TagId != 0)
            {
                query.InnerJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "t.project_id"));
                query.Where("ptag.tag_id", filter.TagId);
            }

            if (filter.TaskStatuses.Count != 0)
            {
                var status = filter.TaskStatuses.First();
                if (status == TaskStatus.Open)
                    query.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                else
                    query.Where("t.status", TaskStatus.Closed);
            }

            if(!filter.UserId.Equals(Guid.Empty))
            {
                query.Where("t.create_by", filter.UserId);
            }

            if (filter.DepartmentId != Guid.Empty || (filter.ParticipantId.HasValue && filter.ParticipantId != Guid.Empty))
            {
                var existSubtask = new SqlQuery(SubtasksTable + " pst").Select("pst.task_id").Where(Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("t.id", "pst.task_id") & Exp.Eq("pst.status", TaskStatus.Open));
                var existResponsible = new SqlQuery(TasksResponsibleTable + " ptr1").Select("ptr1.task_id").Where(Exp.EqColumns("t.tenant_id", "ptr1.tenant_id") & Exp.EqColumns("t.id", "ptr1.task_id"));

                if (filter.DepartmentId != Guid.Empty)
                {
                    existSubtask.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "pst.responsible_id") & Exp.Eq("cug.tenant", Tenant));
                    existResponsible.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "ptr1.responsible_id") & Exp.Eq("cug.tenant", Tenant));
                    existSubtask.Where("cug.groupid", filter.DepartmentId);
                    existResponsible.Where("cug.groupid", filter.DepartmentId);
                }

                if (filter.ParticipantId.HasValue && filter.ParticipantId != Guid.Empty)
                {
                    existSubtask.Where(Exp.Eq("pst.responsible_id", filter.ParticipantId.ToString()));
                    existResponsible.Where(Exp.Eq("ptr1.responsible_id", filter.ParticipantId.ToString()));
                }

                query.Where(Exp.Exists(existSubtask) | Exp.Exists(existResponsible));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                if (FullTextSearch.SupportModule(FullTextSearch.ProjectsTasksModule, FullTextSearch.ProjectsCommentsModule))
                {
                    var taskIds = FullTextSearch.Search(
                     FullTextSearch.ProjectsTasksModule.Match(filter.SearchText),
                     FullTextSearch.ProjectsCommentsModule.Match(filter.SearchText, "content")
                     .Select("target_uniq_id").Match("Task_*", "target_uniq_id"));

                    query.Where(Exp.In("t.id", taskIds));
                }
                else
                {
                    query.Where(Exp.Like("t.title", filter.SearchText, SqlLike.AnyWhere));
                }
            }

            return query;
        }

        private SqlQuery CreateQueryFilterCount(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            query = CreateQueryFilterBase(query, filter);

            if (filter.Milestone.HasValue)
            {
                query.Where("t.milestone_id", filter.Milestone);
            }
            else if (filter.MyMilestones)
            {
                if (!filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("t.tenant_id", "ppp.tenant"));
                    query.Where("ppp.participant_id", CurrentUserID);
                }

                var existsMilestone = new SqlQuery(MilestonesTable + " m").Select("m.id").Where(Exp.EqColumns("t.milestone_id", "m.id") & Exp.EqColumns("m.tenant_id", "t.tenant_id"));
                query.Where(Exp.Exists(existsMilestone));
            }

            if (filter.ParticipantId.HasValue && filter.ParticipantId == Guid.Empty)
            {
                var notExists = new SqlQuery(TasksResponsibleTable + " ptr").Select("ptr.responsible_id").Where(Exp.EqColumns("t.id", "ptr.task_id") & Exp.Eq("ptr.tenant_id", Tenant));
                query.Where(!Exp.Exists(notExists));
            }

            var hasFromDate = !filter.FromDate.Equals(DateTime.MinValue) && !filter.FromDate.Equals(DateTime.MaxValue);
            var hasToDate = !filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue);

            if (hasFromDate && hasToDate)
            {
                var existsMilestone = new SqlQuery(MilestonesTable + " m").Select("m.id").Where(Exp.EqColumns("m.id", "t.milestone_id") & Exp.EqColumns("m.tenant_id", "t.tenant_id") & Exp.Between("m.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate), TenantUtil.DateTimeFromUtc(filter.ToDate)));
                var expExists = Exp.Exists(existsMilestone) & Exp.Eq("t.deadline", DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"));
                query.Where(Exp.Between("t.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate), TenantUtil.DateTimeFromUtc(filter.ToDate)) | expExists);
            }
            else if (hasFromDate)
            {
                var existsMilestone = new SqlQuery(MilestonesTable + " m")
                    .Select("m.id")
                    .Where(Exp.EqColumns("m.id", "t.milestone_id"))
                    .Where(Exp.EqColumns("m.tenant_id", "t.tenant_id"))
                    .Where(Exp.Ge("m.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate)))
                    .Where(Exp.Eq("t.deadline", DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")));

                query.Where(Exp.Ge("t.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate))
                            | Exp.Exists(existsMilestone));
            }
            else if (hasToDate)
            {
                var existsMilestone = new SqlQuery(MilestonesTable + " m")
                    .Select("m.id")
                    .Where(Exp.EqColumns("m.id", "t.milestone_id"))
                    .Where(Exp.EqColumns("m.tenant_id", "t.tenant_id"))
                    .Where(Exp.Le("m.deadline", TenantUtil.DateTimeFromUtc(filter.ToDate)))
                    .Where(!Exp.Eq("m.deadline", DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")))
                    .Where("t.deadline", DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"));

                query.Where(!Exp.Eq("t.deadline", DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"))
                            & Exp.Le("t.deadline", TenantUtil.DateTimeFromUtc(filter.ToDate))
                            | Exp.Exists(existsMilestone));
            }

            CheckSecurity(query, filter, isAdmin, checkAccess);

            return query;
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            query = CreateQueryFilterBase(query, filter);

            if (filter.ParticipantId.HasValue && filter.ParticipantId == Guid.Empty)
            {
                query.Where(Exp.Eq("ptr.task_id", null));
            }

            if (filter.Milestone.HasValue)
            {
                query.Where("t.milestone_id", filter.Milestone);
            }
            else if (filter.MyMilestones)
            {
                if (!filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("t.tenant_id", "ppp.tenant"));
                    query.Where("ppp.participant_id", CurrentUserID);
                }

                query.Where(Exp.Gt("m.id", 0));
            }

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.FromDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Ge(GetSortFilter("deadline", true), TenantUtil.DateTimeFromUtc(filter.FromDate)));
            }

            if (!filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Le(GetSortFilter("deadline", true), TenantUtil.DateTimeFromUtc(filter.ToDate)));
            }

            CheckSecurity(query, filter, isAdmin, checkAccess);

            return query;
        }

        private static Task ToTask(object[] r)
        {
            var offset = ProjectDao.ProjectColumns.Length;
            var task = new Task
            {
                Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                ID = Convert.ToInt32(r[0 + offset]),
                Title = (string)r[1 + offset],
                CreateBy = ToGuid(r[2 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3 + offset])),
                LastModifiedBy = ToGuid(r[4 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5 + offset])),
                Description = (string)r[6 + offset],
                Priority = (TaskPriority)Convert.ToInt32(r[7 + offset]),
                Status = (TaskStatus)Convert.ToInt32(r[8 + offset]),
                Milestone = r[9 + offset] == null ? 0 : Convert.ToInt32(r[9 + offset]),
                SortOrder = Convert.ToInt32(r[10 + offset]),
                Deadline = r[11 + offset] != null ? DateTime.SpecifyKind(Convert.ToDateTime(r[11 + offset]), DateTimeKind.Local) : default(DateTime),
                StartDate = !Convert.ToDateTime(r[12 + offset]).Equals(DateTime.MinValue) ? DateTime.SpecifyKind(Convert.ToDateTime(r[12 + offset]), DateTimeKind.Local) : default(DateTime),
                Progress = Convert.ToInt32(r[13 + offset]),
                Responsibles = !string.IsNullOrEmpty((string)r[14 + offset]) ? new List<Guid>(((string)r[14 + offset]).Split(',').Select(ToGuid)) : new List<Guid>(),
                SubTasks = new List<Subtask>()
            };

            return task;
        }

        private static Task ToTaskMilestone(object[] r)
        {
            var task = ToTask(r);
            var offset = ProjectDao.ProjectColumns.Length + TaskColumns.Length;

            if (task.Milestone > 0)
                task.MilestoneDesc = new Milestone
                                         {
                                             ID = task.Milestone,
                                             Title = (string) r[0 + offset] ?? "",
                                             DeadLine = r[1 + offset] != null ? DateTime.SpecifyKind(Convert.ToDateTime(r[1 + offset]), DateTimeKind.Local) : default(DateTime),
                                         };

            return task;
        }

        private static TaskLink ToTaskLink(IList<object> r)
        {
            return new TaskLink
                       {
                           DependenceTaskId = Convert.ToInt32(r[0]),
                           ParentTaskId = Convert.ToInt32(r[1]),
                           LinkType = (TaskLinkType)Convert.ToInt32(r[2])
                       };
        }

        internal List<Task> GetTasks(Exp where)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where(where)).ConvertAll(ToTask);
            }
        }

        private static string GetSortFilter(string sortBy, bool sortOrder)
        {
            if (sortBy != "deadline") return "t." + sortBy;

            var sortDate = sortOrder ? DateTime.MaxValue.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
            return string.Format("COALESCE(COALESCE(NULLIF(t.deadline,'{0}'),m.deadline), '{1}')",
                                 DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"), sortDate);

        }

        private void CheckSecurity(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (checkAccess)
            {
                query.Where(Exp.Eq("p.private", false));
                return;
            }

            if (isAdmin) return;

            if (!filter.MyProjects && !filter.MyMilestones)
            {
                query.LeftOuterJoin(ParticipantTable + " ppp", Exp.Eq("ppp.participant_id", CurrentUserID) & Exp.EqColumns("ppp.project_id", "t.project_id") & Exp.EqColumns("ppp.tenant", "t.tenant_id"));
            }
            var isInTeam = !Exp.Eq("ppp.security", null) & Exp.Eq("ppp.removed", false);
            var canReadTasks = !Exp.Eq("security & " + (int)ProjectTeamSecurity.Tasks, (int)ProjectTeamSecurity.Tasks);
            var canReadMilestones = Exp.Eq("t.milestone_id", 0) | !Exp.Eq("security & " + (int)ProjectTeamSecurity.Milestone, (int)ProjectTeamSecurity.Milestone);
            var exists = new SqlQuery("projects_tasks_responsible ptr1").Select("ptr1.responsible_id").Where(Exp.EqColumns("t.id", "ptr1.task_id") & Exp.EqColumns("ptr1.tenant_id", "t.tenant_id") & Exp.Eq("ptr1.responsible_id", CurrentUserID));
            var responsible = Exp.Exists(exists);

            query.Where(Exp.Eq("p.private", false) | isInTeam & (responsible | canReadTasks & canReadMilestones));
        }

        #endregion
    }
}
