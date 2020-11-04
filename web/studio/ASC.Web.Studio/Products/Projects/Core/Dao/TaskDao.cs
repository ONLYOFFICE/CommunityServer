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
using System.Data;
using System.Globalization;
using System.Linq;

using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Data.DAO
{
    internal class CachedTaskDao : TaskDao
    {
        private readonly HttpRequestDictionary<Task> taskCache = new HttpRequestDictionary<Task>("task");

        public CachedTaskDao(int tenantID) : base(tenantID)
        {
        }

        public override void Delete(Task task)
        {
            ResetCache(task.ID);
            base.Delete(task);
        }

        public override Task GetById(int id)
        {
            return taskCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private Task GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Task Update(Task task)
        {
            if (task != null)
            {
                ResetCache(task.ID);
            }
            return base.Update(task);
        }

        private void ResetCache(int taskId)
        {
            taskCache.Reset(taskId.ToString(CultureInfo.InvariantCulture));
        }
    }


    class TaskDao : BaseDao, ITaskDao
    {
        public static readonly string[] TaskColumns = new[] { "id", "title", "description", "status", "create_by", "create_on", "last_modified_by", "last_modified_on", "priority", "milestone_id", "sort_order", "deadline", "start_date", "progress", "responsibles", "status_id" };
        private readonly Converter<object[], Task> converter;

        private ISubtaskDao SubtaskDao { get { return DaoFactory.SubtaskDao; } }

        public IDaoFactory DaoFactory { get; set; }

        public TaskDao(int tenantID) : base(tenantID)
        {
            converter = ToTask;
        }


        #region ITaskDao

        public List<Task> GetAll()
        {
            return Db.ExecuteList(CreateQuery()).ConvertAll(converter);
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
            if (status.HasValue)
            {
                if (status.Value == TaskStatus.Open)
                    query.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                else
                    query.Where("t.status", TaskStatus.Closed);
            }

            if (!participant.Equals(Guid.Empty))
            {
                var existSubtask = new SqlQuery(SubtasksTable + " pst").Select("pst.task_id").Where(Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("t.id", "pst.task_id") & Exp.Eq("pst.status", TaskStatus.Open));
                var existResponsible = new SqlQuery(TasksResponsibleTable + " ptr1").Select("ptr1.task_id").Where(Exp.EqColumns("t.tenant_id", "ptr1.tenant_id") & Exp.EqColumns("t.id", "ptr1.task_id"));

                existSubtask.Where(Exp.Eq("pst.responsible_id", participant.ToString()));
                existResponsible.Where(Exp.Eq("ptr1.responsible_id", participant.ToString()));

                query.Where(Exp.Exists(existSubtask) | Exp.Exists(existResponsible));
            }

            return Db.ExecuteList(query).ConvertAll(converter);
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

            return Db.ExecuteList(query).ConvertAll(ToTaskMilestone);
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


            var result = Db.ExecuteList(queryCount).ToDictionary(r => Convert.ToInt32(r[1]), r => Convert.ToInt32(r[0]));
            var tasksOpen = result.Where(row => row.Key != (int)TaskStatus.Closed).Sum(row => row.Value);
            //that's right. open its not closed.
            int tasksClosed;
            result.TryGetValue((int)TaskStatus.Closed, out tasksClosed);
            return new TaskFilterCountOperationResult { TasksOpen = tasksOpen, TasksClosed = tasksClosed };
        }

        public IEnumerable<TaskFilterCountOperationResult> GetByFilterCountForStatistic(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var result = new List<TaskFilterCountOperationResult>();
            var query = new SqlQuery(TasksTable + " t")
                .Select("t.status", "r.responsible_id")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .InnerJoin(TasksResponsibleTable + " r", Exp.EqColumns("r.task_id", "t.id") & Exp.EqColumns("r.tenant_id", "t.tenant_id"))
                .Where("t.tenant_id", Tenant);

            if (filter.HasUserId)
            {
                query.Where(Exp.In("r.responsible_id", filter.GetUserIds()));
            }
            else
            {
                query.Where(!Exp.Eq("r.responsible_id", Guid.Empty.ToString()));
            }

            filter.UserId = Guid.Empty;

            query = CreateQueryFilterCount(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery()
                .SelectCount()
                .Select("t1.responsible_id")
                .Select("t1.status")
                .From(query, "t1")
                .GroupBy("responsible_id", "status");

            if (filter.ParticipantId.HasValue && !filter.ParticipantId.Value.Equals(Guid.Empty))
            {
                queryCount.Where("t1.responsible_id", filter.ParticipantId.Value);
            }

            var fromDb = Db.ExecuteList(queryCount)
                .Select(r => new
                {
                    Id = Guid.Parse((string)r[1]),
                    Count = Convert.ToInt32(r[0]),
                    Status = Convert.ToInt32(r[2])
                }).GroupBy(r => r.Id);

            foreach (var r in fromDb)
            {
                var tasksOpen = r.Where(row => row.Status != (int)TaskStatus.Closed).Sum(row => row.Count);
                var tasksClosed = r.Where(row => row.Status == (int)TaskStatus.Closed).Sum(row => row.Count);
                result.Add(new TaskFilterCountOperationResult { UserId = r.Key, TasksOpen = tasksOpen, TasksClosed = tasksClosed });
            }

            return result;
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(TasksTable + " t")
                .Select("t.create_by", "t.project_id")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Where("t.tenant_id", Tenant)
                .Where(Exp.Between("t.create_on", filter.GetFromDate(), filter.GetToDate()));

            if (filter.HasUserId)
            {
                query.Where(Exp.In("t.create_by", filter.GetUserIds()));
                filter.UserId = Guid.Empty;
                filter.DepartmentId = Guid.Empty;
            }

            query = CreateQueryFilterCount(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery()
                .SelectCount()
                .Select("t1.create_by", "t1.project_id")
                .From(query, "t1")
                .GroupBy("create_by", "project_id");

            return Db.ExecuteList(queryCount).ConvertAll(b => new Tuple<Guid, int, int>(Guid.Parse((string)b[1]), Convert.ToInt32(b[2]), Convert.ToInt32(b[0])));
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

            return Db.ExecuteList(q).ConvertAll(converter);
        }

        public List<Task> GetMilestoneTasks(int milestoneId)
        {
            var query = CreateQuery()
                .Where("t.milestone_id", milestoneId)
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", false)
                .OrderBy("t.create_on", false);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Task> GetById(ICollection<int> ids)
        {
            var query = CreateQuery().Where(Exp.In("t.id", ids.ToArray()));
            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public virtual Task GetById(int id)
        {
            var query = CreateQuery().Where("t.id", id);
            return Db.ExecuteList(query).ConvertAll(converter).SingleOrDefault();
        }

        public bool IsExists(int id)
        {
            var count = Db.ExecuteScalar<long>(Query(TasksTable).SelectCount().Where("id", id));
            return 0 < count;
        }

        public List<object[]> GetTasksForReminder(DateTime deadline)
        {
            var deadlineDate = deadline.Date;
            var q = new SqlQuery(TasksTable + " t")
                .Select("t.tenant_id", "t.id", "t.deadline")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Where(Exp.Between("t.deadline", deadlineDate.AddDays(-1), deadlineDate.AddDays(1)))
                .Where(!Exp.Eq("t.status", TaskStatus.Closed))
                .Where("p.status", ProjectStatus.Open);

            return Db.ExecuteList(q)
                .ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToDateTime(r[2]) });
        }

        public Task Create(Task task)
        {
            using (var tr = Db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var insert = Insert(TasksTable, false)
                    .InColumnValue("id", task.ID)
                    .InColumnValue("project_id", task.Project != null ? task.Project.ID : 0)
                    .InColumnValue("title", task.Title)
                    .InColumnValue("create_by", task.CreateBy.ToString())
                    .InColumnValue("create_on", TenantUtil.DateTimeToUtc(task.CreateOn))
                    .InColumnValue("description", task.Description)
                    .InColumnValue("priority", task.Priority)
                    .InColumnValue("status", task.Status)
                    .InColumnValue("milestone_id", task.Milestone)
                    .InColumnValue("sort_order", task.SortOrder)
                    .InColumnValue("deadline", task.Deadline)
                    .InColumnValue("start_date", task.StartDate)
                    .InColumnValue("progress", task.Progress)
                    .Identity(1, 0, true);

                task.ID = Db.ExecuteScalar<int>(insert);

                if (task.Responsibles.Any())
                {
                    insert = new SqlInsert(TasksResponsibleTable).InColumns("tenant_id", "task_ID", "responsible_id");

                    foreach (var responsible in task.Responsibles.Distinct())
                    {
                        insert.Values(Tenant, task.ID, responsible);
                    }

                    Db.ExecuteNonQuery(insert);
                }

                tr.Commit();

                return task;
            }
        }

        public virtual Task Update(Task task)
        {
            using (var tr = Db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var update = Update(TasksTable)
                    .Set("project_id", task.Project != null ? task.Project.ID : 0)
                    .Set("title", task.Title)
                    .Set("last_modified_by", task.LastModifiedBy.ToString())
                    .Set("last_modified_on", TenantUtil.DateTimeToUtc(task.LastModifiedOn))
                    .Set("description", task.Description)
                    .Set("priority", task.Priority)
                    .Set("status", task.Status)
                    .Set("milestone_id", task.Milestone)
                    .Set("sort_order", task.SortOrder)
                    .Set("deadline", task.Deadline)
                    .Set("status_changed", TenantUtil.DateTimeToUtc(task.StatusChangedOn))
                    .Set("start_date", task.StartDate)
                    .Set("progress", task.Progress)
                    .Set("status_id", task.CustomTaskStatus)
                    .Where("id", task.ID);

                Db.ExecuteNonQuery(update);

                Db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where("task_id", task.ID));

                if (task.Responsibles.Any())
                {
                    var insert = new SqlInsert(TasksResponsibleTable).InColumns("tenant_id", "task_ID", "responsible_id");

                    foreach (var responsible in task.Responsibles.Distinct())
                    {
                        insert.Values(Tenant, task.ID, responsible);
                    }

                    Db.ExecuteNonQuery(insert);
                }

                tr.Commit();

                return task;
            }
        }

        public virtual void Delete(Task task)
        {
            using (var tx = Db.BeginTransaction())
            {
                var id = task.ID;
                task.Links.ForEach(RemoveLink);
                task.SubTasks.ForEach(subTask => SubtaskDao.Delete(subTask.ID));
                Db.ExecuteNonQuery(Delete(CommentsTable).Where("target_uniq_id", task.UniqID));
                Db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where("task_id", id));
                Db.ExecuteNonQuery(Delete(TasksTable).Where("id", id));

                tx.Commit();
            }
        }

        #region Recurrence

        public List<object[]> GetRecurrence(DateTime date)
        {
            var q = new SqlQuery("projects_tasks")
                .Select("tenant_id", "task_id")
                .InnerJoin("projects_tasks_recurrence as ptr", Exp.Eq("ptr.task_id", "t.id"))
                .Where(Exp.Ge("ptr.start_date", date))
                .Where(Exp.Le("ptr.end_date", date))
                .Where(Exp.Eq("t.status", TaskStatus.Open));

            return Db.ExecuteList(q).ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]) });
        }

        public void SaveRecurrence(Task task, string cron, DateTime startDate, DateTime endDate)
        {
            Insert("projects_tasks_recurrence")
                .InColumnValue("task_id", task.ID)
                .InColumnValue("cron", cron)
                .InColumnValue("title", task.Title)
                .InColumnValue("startDate", startDate)
                .InColumnValue("endDate", endDate);
        }

        public void DeleteReccurence(int taskId)
        {
            Db.ExecuteNonQuery(Delete("projects_tasks_recurrence").Where("task_id", taskId));
        }

        #endregion

        #region Link

        public void AddLink(TaskLink link)
        {
            var query = Insert(TasksLinksTable)
                .InColumnValue("task_id", link.DependenceTaskId)
                .InColumnValue("parent_id", link.ParentTaskId)
                .InColumnValue("link_type", link.LinkType);

            Db.ExecuteNonQuery(query);
        }

        public void RemoveLink(TaskLink link)
        {
            var query = Delete(TasksLinksTable)
                .Where((Exp.Eq("task_id", link.DependenceTaskId) & Exp.Eq("parent_id", link.ParentTaskId)) |
                        (Exp.Eq("task_id", link.ParentTaskId) & Exp.Eq("parent_id", link.DependenceTaskId)));

            Db.ExecuteNonQuery(query);
        }

        public bool IsExistLink(TaskLink link)
        {
            var query = Query(TasksLinksTable)
                .SelectCount()
                .Where((Exp.Eq("task_id", link.DependenceTaskId) & Exp.Eq("parent_id", link.ParentTaskId)) |
                        (Exp.Eq("task_id", link.ParentTaskId) & Exp.Eq("parent_id", link.DependenceTaskId)));

            return Db.ExecuteScalar<long>(query) > 0;
        }

        public IEnumerable<TaskLink> GetLinks(int taskID)
        {
            var query = Query(TasksLinksTable)
                .Select("task_id", "parent_id", "link_type")
                .Where(Exp.Eq("task_id", taskID) | Exp.Eq("parent_id", taskID));

            return Db.ExecuteList(query).ConvertAll(ToTaskLink);
        }

        public IEnumerable<TaskLink> GetLinks(List<Task> tasks)
        {
            var query = Query(TasksLinksTable)
                .Select("task_id", "parent_id", "link_type")
                .Where(Exp.Or(Exp.In("task_id", tasks.Select(r => r.ID).ToList()),
                                Exp.In("parent_id", tasks.Select(r => r.ID).ToList())));

            return Db.ExecuteList(query).ConvertAll(ToTaskLink);
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
                .Select("t.status_id")
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
                if (ProjectsCommonSettings.Load().HideEntitiesInPausedProjects)
                {
                    query.Where(!Exp.Eq("p.status", ProjectStatus.Paused));
                }

                if (filter.MyProjects)
                {
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("t.tenant_id", "ppp.tenant"));
                    query.Where("ppp.participant_id", CurrentUserID);
                }
            }

            if (filter.TagId != 0)
            {
                if (filter.TagId == -1)
                {
                    query.LeftOuterJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "t.project_id"));
                    query.Where("ptag.tag_id", null);
                }
                else
                {
                    query.InnerJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "t.project_id"));
                    query.Where("ptag.tag_id", filter.TagId);
                }
            }

            if (filter.Substatus.HasValue)
            {
                var substatus = filter.Substatus.Value;
                if (substatus > -1)
                {
                    query.InnerJoin(StatusTable + " pstat", Exp.EqColumns("pstat.tenant_id", "t.tenant_id") & Exp.Eq("pstat.id", filter.Substatus.Value));
                    query.Where(Exp.Eq("t.status_id", filter.Substatus.Value) | (Exp.Eq("t.status_id", null) & Exp.EqColumns("t.status", "pstat.statusType") & Exp.Eq("pstat.isDefault", true)));
                }
                else
                {
                    query.Where("t.status", -substatus).Where("t.status_id", null);

                }
            }
            else if (filter.TaskStatuses.Count != 0)
            {
                var status = filter.TaskStatuses.First();
                query.Where("t.status", status);
            }

            if (!filter.UserId.Equals(Guid.Empty))
            {
                query.Where("t.create_by", filter.UserId);
            }

            if (!filter.DepartmentId.Equals(Guid.Empty) || (filter.ParticipantId.HasValue && !filter.ParticipantId.Value.Equals(Guid.Empty)))
            {
                var existSubtask = new SqlQuery(SubtasksTable + " pst").Select("pst.task_id").Where(Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("t.id", "pst.task_id") & Exp.Eq("pst.status", TaskStatus.Open));
                var existResponsible = new SqlQuery(TasksResponsibleTable + " ptr1").Select("ptr1.task_id").Where(Exp.EqColumns("t.tenant_id", "ptr1.tenant_id") & Exp.EqColumns("t.id", "ptr1.task_id"));

                if (!filter.DepartmentId.Equals(Guid.Empty))
                {
                    existSubtask.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "pst.responsible_id") & Exp.Eq("cug.tenant", Tenant));
                    existResponsible.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "ptr1.responsible_id") & Exp.Eq("cug.tenant", Tenant));
                    existSubtask.Where("cug.groupid", filter.DepartmentId);
                    existResponsible.Where("cug.groupid", filter.DepartmentId);
                }

                if (filter.ParticipantId.HasValue && !filter.ParticipantId.Value.Equals(Guid.Empty))
                {
                    existSubtask.Where(Exp.Eq("pst.responsible_id", filter.ParticipantId.Value.ToString()));
                    existResponsible.Where(Exp.Eq("ptr1.responsible_id", filter.ParticipantId.Value.ToString()));
                }

                query.Where(Exp.Exists(existSubtask) | Exp.Exists(existResponsible));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                List<int> taskIds;
                if (FactoryIndexer<TasksWrapper>.TrySelectIds(s => s.MatchAll(filter.SearchText), out taskIds))
                {
                    IReadOnlyCollection<SubtasksWrapper> subtaskIds;
                    if (FactoryIndexer<SubtasksWrapper>.TrySelect(s => s.MatchAll(filter.SearchText), out subtaskIds))
                    {
                        taskIds.AddRange(subtaskIds.Select(r => r.Task).ToList());
                    }

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
            var minDateTime = DateTime.MinValue;
            var maxDateTime = DateTime.MaxValue;
            var minDateTimeString = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");

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

            if (filter.ParticipantId.HasValue && filter.ParticipantId.Value.Equals(Guid.Empty))
            {
                var notExists = new SqlQuery(TasksResponsibleTable + " ptr").Select("ptr.responsible_id").Where(Exp.EqColumns("t.id", "ptr.task_id") & Exp.Eq("ptr.tenant_id", Tenant));
                query.Where(!Exp.Exists(notExists));
            }

            var hasFromDate = !filter.FromDate.Equals(minDateTime) && !filter.FromDate.Equals(maxDateTime);
            var hasToDate = !filter.ToDate.Equals(minDateTime) && !filter.ToDate.Equals(maxDateTime);

            if (hasFromDate && hasToDate)
            {
                var existsMilestone = new SqlQuery(MilestonesTable + " m").Select("m.id").Where(Exp.EqColumns("m.id", "t.milestone_id") & Exp.EqColumns("m.tenant_id", "t.tenant_id") & Exp.Between("m.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate), TenantUtil.DateTimeFromUtc(filter.ToDate)));
                var expExists = Exp.Exists(existsMilestone) & Exp.Eq("t.deadline", minDateTimeString);
                query.Where(Exp.Between("t.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate), TenantUtil.DateTimeFromUtc(filter.ToDate)) | expExists);
            }
            else if (hasFromDate)
            {
                var existsMilestone = new SqlQuery(MilestonesTable + " m")
                    .Select("m.id")
                    .Where(Exp.EqColumns("m.id", "t.milestone_id"))
                    .Where(Exp.EqColumns("m.tenant_id", "t.tenant_id"))
                    .Where(Exp.Ge("m.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate)))
                    .Where(Exp.Eq("t.deadline", minDateTimeString));

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
                    .Where(!Exp.Eq("m.deadline", minDateTimeString))
                    .Where("t.deadline", minDateTimeString);

                query.Where(!Exp.Eq("t.deadline", minDateTimeString)
                            & Exp.Le("t.deadline", TenantUtil.DateTimeFromUtc(filter.ToDate))
                            | Exp.Exists(existsMilestone));
            }

            CheckSecurity(query, filter, isAdmin, checkAccess);

            return query;
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            query = CreateQueryFilterBase(query, filter);

            if (filter.ParticipantId.HasValue && filter.ParticipantId.Value.Equals(Guid.Empty))
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
            var deadline = Convert.ToDateTime(r[11 + offset]);
            var startDate = Convert.ToDateTime(r[12 + offset]);
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
                Deadline = !deadline.Equals(DateTime.MinValue) ? DateTime.SpecifyKind(deadline, DateTimeKind.Local) : default(DateTime),
                StartDate = !startDate.Equals(DateTime.MinValue) ? DateTime.SpecifyKind(startDate, DateTimeKind.Local) : default(DateTime),
                Progress = Convert.ToInt32(r[13 + offset]),
                Responsibles = !string.IsNullOrEmpty((string)r[14 + offset]) ? new List<Guid>(((string)r[14 + offset]).Split(',').Select(ToGuid)) : new List<Guid>(),
                SubTasks = new List<Subtask>(),
                CustomTaskStatus = r[15 + offset] != null ? (int?)Convert.ToInt32(r[15 + offset]) : null
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
                    Title = (string)r[0 + offset] ?? "",
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

        public List<Task> GetTasks(Exp where)
        {
            return Db.ExecuteList(CreateQuery().Where(where)).ConvertAll(converter);
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
