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
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.ElasticSearch;
using ASC.Web.Projects;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Data.DAO
{
    class CachedMilestoneDao : MilestoneDao
    {
        private readonly HttpRequestDictionary<Milestone> projectCache = new HttpRequestDictionary<Milestone>("milestone");


        public CachedMilestoneDao(int tenant)
            : base(tenant)
        {
        }

        public override Milestone GetById(int id)
        {
            return projectCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private Milestone GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Milestone Save(Milestone milestone)
        {
            if (milestone != null)
            {
                ResetCache(milestone.ID);
            }
            return base.Save(milestone);
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        private void ResetCache(int milestoneId)
        {
            projectCache.Reset(milestoneId.ToString(CultureInfo.InvariantCulture));
        }
    }

    class MilestoneDao : BaseDao, IMilestoneDao
    {
        private readonly Converter<object[], Milestone> converter;

        public MilestoneDao(int tenant)
            : base(tenant)
        {
            converter = ToMilestone;
        }

        public List<Milestone> GetAll()
        {
            return Db.ExecuteList(CreateQuery()).ConvertAll(converter);
        }

        public List<Milestone> GetByProject(int projectId)
        {
            return Db.ExecuteList(CreateQuery().Where("t.project_id", projectId))
                    .ConvertAll(converter);
        }

        public List<Milestone> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = CreateQuery();

            if (filter.Max > 0 && filter.Max < 150000)
            {
                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max * 2);
            }

            query.OrderBy("t.status", true);

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortColumns = filter.SortColumns["Milestone"];
                sortColumns.Remove(filter.SortBy);

                query.OrderBy("t." + (filter.SortBy == "create_on" ? "id" : filter.SortBy), filter.SortOrder);

                foreach (var sort in sortColumns.Keys)
                {
                    query.OrderBy("t." + (sort == "create_on" ? "id" : sort), sortColumns[sort]);
                }
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(MilestonesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Select("t.id")
                .GroupBy("t.id")
                .Where("t.tenant_id", Tenant);

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectCount().From(query, "t1");

            return Db.ExecuteScalar<int>(queryCount);
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(MilestonesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                .Select("t.create_by", "t.project_id")
                .Where("t.tenant_id", Tenant)
                .Where(Exp.Between("t.create_on", filter.GetFromDate(), filter.GetToDate()));

            if (filter.HasUserId)
            {
                query.Where(Exp.In("t.create_by", filter.GetUserIds()));
                filter.UserId = Guid.Empty;
                filter.DepartmentId = Guid.Empty;
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery()
                .SelectCount()
                .Select("t1.create_by", "t1.project_id")
                .GroupBy("create_by", "project_id")
                .From(query, "t1");

            return Db.ExecuteList(queryCount).ConvertAll(b => new Tuple<Guid, int, int>(Guid.Parse((string)b[1]), Convert.ToInt32(b[2]), Convert.ToInt32(b[0]))); ;
        }

        public List<Milestone> GetByStatus(int projectId, MilestoneStatus milestoneStatus)
        {
            return Db.ExecuteList(CreateQuery().Where("t.project_id", projectId).Where("t.status", milestoneStatus))
                    .ConvertAll(converter);
        }

        public List<Milestone> GetUpcomingMilestones(int offset, int max, params int[] projects)
        {
            var query = CreateQuery()
                .SetFirstResult(offset)
                .Where("p.status", ProjectStatus.Open)
                .Where(Exp.Ge("t.deadline", TenantUtil.DateTimeNow().Date))
                .Where("t.status", MilestoneStatus.Open)
                .SetMaxResults(max)
                .OrderBy("t.deadline", true);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("p.id", projects.Take(0 < max ? max : projects.Length).ToArray()));
            }

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Milestone> GetLateMilestones(int offset, int max)
        {
            var now = TenantUtil.DateTimeNow();
            var yesterday = now.Date.AddDays(-1);
            var query = CreateQuery()
                .SetFirstResult(offset)
                .Where("p.status", ProjectStatus.Open)
                .Where(!Exp.Eq("t.status", MilestoneStatus.Closed))
                .Where(Exp.Le("t.deadline", yesterday))
                .SetMaxResults(max)
                .OrderBy("t.deadline", true);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Milestone> GetByDeadLine(DateTime deadline)
        {
            return Db.ExecuteList(CreateQuery().Where("t.deadline", deadline.Date).OrderBy("t.deadline", true))
                    .ConvertAll(converter);
        }

        public virtual Milestone GetById(int id)
        {
            return Db.ExecuteList(CreateQuery().Where("t.id", id))
                    .ConvertAll(converter)
                    .SingleOrDefault();
        }

        public List<Milestone> GetById(int[] id)
        {
            return Db.ExecuteList(CreateQuery().Where(Exp.In("t.id", id)))
                    .ConvertAll(converter);
        }

        public bool IsExists(int id)
        {
            var count = Db.ExecuteScalar<long>(Query(MilestonesTable).SelectCount().Where("id", id));
            return 0 < count;
        }

        public List<object[]> GetInfoForReminder(DateTime deadline)
        {
            var deadlineDate = deadline.Date;
            var q = new SqlQuery(MilestonesTable + " t")
                .Select("t.tenant_id", "t.id", "t.deadline")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id"))
                .Where(Exp.Between("t.deadline", deadlineDate.AddDays(-1), deadlineDate.AddDays(1)))
                .Where("t.status", MilestoneStatus.Open)
                .Where("p.status", ProjectStatus.Open)
                .Where("t.is_notify", 1);

                return Db.ExecuteList(q)
                    .ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToDateTime(r[2]) });
        }

        public virtual Milestone Save(Milestone milestone)
        {
            if (milestone.DeadLine.Kind != DateTimeKind.Local)
                milestone.DeadLine = TenantUtil.DateTimeFromUtc(milestone.DeadLine);

            var insert = Insert(MilestonesTable)
                .InColumnValue("id", milestone.ID)
                .InColumnValue("project_id", milestone.Project != null ? milestone.Project.ID : 0)
                .InColumnValue("title", milestone.Title)
                .InColumnValue("create_by", milestone.CreateBy.ToString())
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(milestone.CreateOn))
                .InColumnValue("last_modified_by", milestone.LastModifiedBy.ToString())
                .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(milestone.LastModifiedOn))
                .InColumnValue("deadline", milestone.DeadLine)
                .InColumnValue("status", milestone.Status)
                .InColumnValue("is_notify", milestone.IsNotify)
                .InColumnValue("is_key", milestone.IsKey)
                .InColumnValue("description", milestone.Description)
                .InColumnValue("status_changed", milestone.StatusChangedOn)
                .InColumnValue("responsible_id", milestone.Responsible.ToString())
                .Identity(1, 0, true);
            milestone.ID = Db.ExecuteScalar<int>(insert);
            return milestone;
        }

        public virtual void Delete(int id)
        {
            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete(CommentsTable).Where("target_uniq_id", ProjectEntity.BuildUniqId<Milestone>(id)));
                Db.ExecuteNonQuery(Update(TasksTable).Set("milestone_id", 0).Where("milestone_id", id));
                Db.ExecuteNonQuery(Delete(MilestonesTable).Where("id", id));

                tx.Commit();
            }
        }

        public string GetLastModified()
        {
            var query = Query(MilestonesTable).SelectMax("last_modified_on").SelectCount();
            var data = Db.ExecuteList(query).FirstOrDefault();
            if (data == null)
            {
                return "";
            }
            var lastModified = "";
            if (data[0] != null)
            {
                lastModified += TenantUtil.DateTimeFromUtc(Convert.ToDateTime(data[0])).ToString(CultureInfo.InvariantCulture);
            }
            if (data[1] != null)
            {
                lastModified += data[1];
            }

            return lastModified;
        }

        private SqlQuery CreateQuery()
        {
            return new SqlQuery(MilestonesTable + " t")
                .InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id"))
                .Select(ProjectDao.ProjectColumns.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on")
                .Select("t.deadline", "t.status", "t.is_notify", "t.is_key", "t.description", "t.responsible_id")
                .Select("(select sum(case pt1.status when 1 then 1 when 4 then 1 else 0 end) from projects_tasks pt1 where pt1.tenant_id = t.tenant_id and  pt1.milestone_id=t.id)")
                .Select("(select sum(case pt2.status when 2 then 1 else 0 end) from projects_tasks pt2 where pt2.tenant_id = t.tenant_id and  pt2.milestone_id=t.id)")
                .GroupBy("t.id")
                .Where("t.tenant_id", Tenant);
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.MilestoneStatuses.Count != 0)
            {
                query.Where(Exp.In("t.status", filter.MilestoneStatuses));
            }

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
                    query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.EqColumns("ppp.tenant", "t.tenant_id"));
                    query.Where("ppp.participant_id", CurrentUserID);
                }
            }

            if (filter.UserId != Guid.Empty)
            {
                query.Where(Exp.Eq("t.responsible_id", filter.UserId));
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

            if (filter.ParticipantId.HasValue)
            {
                var existSubtask = new SqlQuery(SubtasksTable + " pst").Select("pst.task_id").Where(Exp.EqColumns("t.tenant_id", "pst.tenant_id") & Exp.EqColumns("pt.id", "pst.task_id") & Exp.Eq("pst.status", TaskStatus.Open));
                var existResponsible = new SqlQuery(TasksResponsibleTable + " ptr1").Select("ptr1.task_id").Where(Exp.EqColumns("t.tenant_id", "ptr1.tenant_id") & Exp.EqColumns("pt.id", "ptr1.task_id"));

                existSubtask.Where(Exp.Eq("pst.responsible_id", filter.ParticipantId.ToString()));
                existResponsible.Where(Exp.Eq("ptr1.responsible_id", filter.ParticipantId.ToString()));

                query.LeftOuterJoin(TasksTable + " as pt", Exp.EqColumns("pt.milestone_id", "t.id") & Exp.EqColumns("pt.tenant_id", "t.tenant_id"));
                query.Where(Exp.Exists(existSubtask) | Exp.Exists(existResponsible));
            }

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.FromDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Ge("t.deadline", TenantUtil.DateTimeFromUtc(filter.FromDate)));
            }

            if (!filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue))
            {
                query.Where(Exp.Le("t.deadline", TenantUtil.DateTimeFromUtc(filter.ToDate)));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                List<int> mIds;
                if (FactoryIndexer<MilestonesWrapper>.TrySelectIds(s => s.MatchAll(filter.SearchText), out mIds))
                {
                    query.Where(Exp.In("t.id", mIds));
                }
                else
                {
                    query.Where(Exp.Like("t.title", filter.SearchText, SqlLike.AnyWhere));
                }
            }

            CheckSecurity(query, filter, isAdmin, checkAccess);

            return query;
        }

        private static Milestone ToMilestone(object[] r)
        {
            var offset = ProjectDao.ProjectColumns.Length;
            return new Milestone
            {
                Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                ID = Convert.ToInt32(r[0 + offset]),
                Title = (string)r[1 + offset],
                CreateBy = ToGuid(r[2 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3 + offset])),
                LastModifiedBy = ToGuid(r[4 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5 + offset])),
                DeadLine = DateTime.SpecifyKind(Convert.ToDateTime(r[6 + offset]), DateTimeKind.Local),
                Status = (MilestoneStatus)Convert.ToInt32(r[7 + offset]),
                IsNotify = Convert.ToBoolean(r[8 + offset]),
                IsKey = Convert.ToBoolean(r[9 + offset]),
                Description = (string)r[10 + offset],
                Responsible = ToGuid(r[11 + offset]),
                ActiveTaskCount = Convert.ToInt32(r[12 + ProjectDao.ProjectColumns.Length]),
                ClosedTaskCount = Convert.ToInt32(r[13 + ProjectDao.ProjectColumns.Length])
            };
        }

        public List<Milestone> GetMilestones(Exp where)
        {
            return Db.ExecuteList(CreateQuery().Where(where)).ConvertAll(converter);
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
            var canReadMilestones = !Exp.Eq("security & " + (int)ProjectTeamSecurity.Milestone, (int)ProjectTeamSecurity.Milestone);
            var responsible = Exp.Eq("t.responsible_id", CurrentUserID);

            query.Where(Exp.Eq("p.private", false) | isInTeam & (responsible | canReadMilestones));
        }
    }
}
