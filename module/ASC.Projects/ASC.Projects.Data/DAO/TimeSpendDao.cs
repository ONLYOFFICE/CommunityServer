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
using System.Linq;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class TimeSpendDao : BaseDao, ITimeSpendDao
    {
        private readonly string[] columns = new[] { "id", "note", "date", "hours", "relative_task_id", "person_id", "project_id", "create_on", "create_by", 
            "payment_status", "status_changed" };

        public TimeSpendDao(string dbId, int tenantID) : base(dbId, tenantID) { }

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

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToTimeSpend);
            }
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

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(queryCount);
            }
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

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<float>(queryCount);
            }
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
                query.InnerJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "t.project_id"));
                query.Where("ptag.tag_id", filter.TagId);
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

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.FromDate.Equals(DateTime.MaxValue) &&
                !filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Between("t.date", filter.FromDate, filter.ToDate));
            }

            if (filter.PaymentStatuses.Any())
            {
                query.Where(Exp.In("payment_status", filter.PaymentStatuses));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query.Where(Exp.Like("t.note", filter.SearchText, SqlLike.AnyWhere));
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
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where("t.project_id", projectId).OrderBy("date", false)).ConvertAll(ToTimeSpend);
            }
        }

        public List<TimeSpend> GetByTask(int taskId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where("t.relative_task_id", taskId).OrderBy("date", false)).ConvertAll(ToTimeSpend);
            }
        }

        public TimeSpend GetById(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(CreateQuery().Where("t.id", id))
                                .ConvertAll(ToTimeSpend)
                                .SingleOrDefault();
            }
        }

        public TimeSpend Save(TimeSpend timeSpend)
        {
            using (var db = new DbManager(DatabaseId))
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

                timeSpend.ID = db.ExecuteScalar<int>(insert);

                return timeSpend;
            }
        }

        public void Delete(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(Delete(TimeTrackingTable).Where("id", id));
            }
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
