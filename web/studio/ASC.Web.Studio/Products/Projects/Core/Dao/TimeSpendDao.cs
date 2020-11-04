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
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects;

namespace ASC.Projects.Data.DAO
{
    class TimeSpendDao : BaseDao, ITimeSpendDao
    {
        private readonly string[] columns = { "id", "note", "date", "hours", "relative_task_id", "person_id", "project_id", "create_on", "create_by", 
            "payment_status", "status_changed" };
        private readonly Converter<object[], TimeSpend> converter;

        public TimeSpendDao(int tenantID) : base(tenantID)
        {
            converter = ToTimeSpend;
        }

        public List<TimeSpend> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = CreateQuery();

            if (filter.Max != 0 && !filter.Max.Equals(int.MaxValue))
            {
                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max * 2);
            }

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortColumns = filter.SortColumns["TimeSpend"];
                sortColumns.Remove(filter.SortBy);

                query.OrderBy("t." + filter.SortBy, filter.SortOrder);

                foreach (var sort in sortColumns.Keys)
                {
                    query.OrderBy("t." + sort, sortColumns[sort]);
                }
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(TimeTrackingTable + " t")
                          .InnerJoin(TasksTable + " pt",
                                Exp.EqColumns("t.tenant_id", "pt.tenant_id") &
                                Exp.EqColumns("t.relative_task_id", "pt.id"))
                          .Select("t.id")
                          .Where("t.tenant_id", Tenant);


            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectCount().From(query, "t1");

            return Db.ExecuteScalar<int>(queryCount);
        }

        public float GetByFilterTotal(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(TimeTrackingTable + " t")
                        .InnerJoin(TasksTable + " pt",
                                Exp.EqColumns("t.tenant_id", "pt.tenant_id") &
                                Exp.EqColumns("t.relative_task_id", "pt.id"))
                          .Select("t.hours")
                          .Where("t.tenant_id", Tenant);

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectSum("hours").From(query, "t1");

            return Db.ExecuteScalar<float>(queryCount);
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.MyProjects || filter.MyMilestones)
            {
                query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("t.project_id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("t.tenant_id", "ppp.tenant"));
                query.Where("ppp.participant_id", CurrentUserID);
            }

            if (filter.ProjectIds.Count != 0)
            {
                query.Where(Exp.In("t.project_id", filter.ProjectIds));
            }
            else
            {
                if (ProjectsCommonSettings.Load().HideEntitiesInPausedProjects)
                {
                    if (!checkAccess && isAdmin)
                    {
                        query.InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id"));
                    }
                    query.Where(!Exp.Eq("p.status", ProjectStatus.Paused));
                }
            }

            if (filter.Milestone.HasValue || filter.MyMilestones)
            {
                query.InnerJoin(MilestonesTable + " pm", Exp.EqColumns("pm.tenant_id", "t.tenant_id") & Exp.EqColumns("pm.project_id", "t.project_id"));
                query.Where(Exp.EqColumns("pt.milestone_id", "pm.id"));

                if (filter.Milestone.HasValue)
                {
                    query.Where("pm.id", filter.Milestone);
                }
                else if (filter.MyMilestones)
                {
                    query.Where(Exp.Gt("pm.id", 0));
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


            if (filter.UserId != Guid.Empty)
            {
                query.Where("t.person_id", filter.UserId);
            }


            if (filter.DepartmentId != Guid.Empty)
            {
                query.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "t.person_id") & Exp.EqColumns("cug.tenant", "t.tenant_id"));
                query.Where("cug.groupid", filter.DepartmentId);
            }

            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;

            if (!filter.FromDate.Equals(minDate) && !filter.FromDate.Equals(maxDate) &&
                !filter.ToDate.Equals(minDate) && !filter.ToDate.Equals(maxDate))
            {
                query.Where(Exp.Between("t.date", filter.FromDate, filter.ToDate));
            }

            if (filter.PaymentStatuses.Any())
            {
                query.Where(Exp.In("payment_status", filter.PaymentStatuses));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query.Where(Exp.Like("t.note", filter.SearchText, SqlLike.AnyWhere) | Exp.Like("pt.title", filter.SearchText, SqlLike.AnyWhere));
            }

            if (checkAccess)
            {
                query.InnerJoin(ProjectsTable + " p", Exp.EqColumns("p.tenant_id", "t.tenant_id") & Exp.EqColumns("p.id", "t.project_id"));
                query.Where(Exp.Eq("p.private", false));
            }
            else if (!isAdmin)
            {
                query.InnerJoin(ProjectsTable + " p", Exp.EqColumns("p.tenant_id", "t.tenant_id") & Exp.EqColumns("p.id", "t.project_id"));

                if (!(filter.MyProjects || filter.MyMilestones))
                {
                    query.LeftOuterJoin(ParticipantTable + " ppp", Exp.Eq("ppp.participant_id", CurrentUserID) & Exp.EqColumns("ppp.project_id", "t.project_id") & Exp.EqColumns("ppp.tenant", "t.tenant_id"));
                }

                var isInTeam = !Exp.Eq("ppp.security", null) & Exp.Eq("ppp.removed", false);
                var canReadTasks = !Exp.Eq("security & " + (int)ProjectTeamSecurity.Tasks, (int)ProjectTeamSecurity.Tasks);
                var canReadMilestones = Exp.Eq("pt.milestone_id", 0) | !Exp.Eq("security & " + (int)ProjectTeamSecurity.Milestone, (int)ProjectTeamSecurity.Milestone);
                var responsible = Exp.Exists(new SqlQuery("projects_tasks_responsible ptr")
                        .Select("ptr.responsible_id")
                        .Where(Exp.EqColumns("pt.id", "ptr.task_id") &
                        Exp.EqColumns("ptr.tenant_id", "pt.tenant_id") &
                        Exp.Eq("ptr.responsible_id", CurrentUserID)));

                query.Where(Exp.Eq("p.private", false) | isInTeam & (responsible | canReadTasks & canReadMilestones));
            }

            query.GroupBy("t.id");

            return query;
        }

        public List<TimeSpend> GetByProject(int projectId)
        {
            return Db.ExecuteList(CreateQuery().Where("t.project_id", projectId).OrderBy("date", false)).ConvertAll(converter);
        }

        public List<TimeSpend> GetByTask(int taskId)
        {
            return Db.ExecuteList(CreateQuery().Where("t.relative_task_id", taskId).OrderBy("date", false)).ConvertAll(converter);
        }

        public TimeSpend GetById(int id)
        {
            return Db.ExecuteList(CreateQuery().Where("t.id", id))
                            .ConvertAll(converter)
                            .SingleOrDefault();
        }

        public TimeSpend Save(TimeSpend timeSpend)
        {
            timeSpend.Date = TenantUtil.DateTimeToUtc(timeSpend.Date);
            timeSpend.StatusChangedOn = TenantUtil.DateTimeToUtc(timeSpend.StatusChangedOn);

            var insert = Insert(TimeTrackingTable)
                .InColumnValue("id", timeSpend.ID)
                .InColumnValue("note", timeSpend.Note)
                .InColumnValue("date", timeSpend.Date)
                .InColumnValue("hours", timeSpend.Hours)
                .InColumnValue("relative_task_id", timeSpend.Task.ID)
                .InColumnValue("person_id", timeSpend.Person.ToString())
                .InColumnValue("project_id", timeSpend.Task.Project.ID)
                .InColumnValue("create_on", timeSpend.CreateOn)
                .InColumnValue("create_by", CurrentUserID)
                .InColumnValue("payment_status", timeSpend.PaymentStatus)
                .InColumnValue("status_changed", timeSpend.StatusChangedOn)
                .Identity(1, 0, true);

            timeSpend.ID = Db.ExecuteScalar<int>(insert);

            return timeSpend;
        }

        public void Delete(int id)
        {
            Db.ExecuteNonQuery(Delete(TimeTrackingTable).Where("id", id));
        }

        private SqlQuery CreateQuery()
        {
            return new SqlQuery(TimeTrackingTable + " t")
                .InnerJoin(TasksTable + " pt", Exp.EqColumns("t.tenant_id", "pt.tenant_id") & Exp.EqColumns("t.relative_task_id", "pt.id"))
                .Select(columns.Select(c => "t." + c).ToArray())
                .Where("t.tenant_id", Tenant);
        }

        private static TimeSpend ToTimeSpend(IList<object> r)
        {
            return new TimeSpend
                       {
                           ID = Convert.ToInt32(r[0]),
                           Note = (string) r[1],
                           Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[2])),
                           Hours = Convert.ToSingle(r[3]),
                           Task = new Task {ID = Convert.ToInt32(r[4])},
                           Person = ToGuid(r[5]),
                           CreateOn = r[7] != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[7])) : default(DateTime),
                           CreateBy = r[8] != null ? ToGuid(r[8]) : ToGuid(r[5]),
                           PaymentStatus = (PaymentStatus)r[9],
                           StatusChangedOn = r[10] != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[10])) : default(DateTime)
                       };
        }
    }
}
