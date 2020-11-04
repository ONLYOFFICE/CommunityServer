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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Core.Search;
using Newtonsoft.Json.Linq;

namespace ASC.Projects.Data.DAO
{
    internal class CachedProjectDao : ProjectDao
    {
        private readonly HttpRequestDictionary<Project> projectCache = new HttpRequestDictionary<Project>("project");

        public CachedProjectDao(int tenantId)
            : base(tenantId)
        {
        }

        public override void Delete(int projectId, out List<int> messages, out List<int> tasks)
        {
            ResetCache(projectId);
            base.Delete(projectId, out messages, out tasks);
        }

        public override void RemoveFromTeam(int projectId, Guid participantId)
        {
            ResetCache(projectId);
            base.RemoveFromTeam(projectId, participantId);
        }

        public override Project Update(Project project)
        {
            if (project != null)
            {
                ResetCache(project.ID);
            }
            return base.Update(project);
        }

        public override Project GetById(int projectId)
        {
            return projectCache.Get(projectId.ToString(CultureInfo.InvariantCulture), () => GetBaseById(projectId));
        }

        private Project GetBaseById(int projectId)
        {
            return base.GetById(projectId);
        }

        public override void AddToTeam(int projectId, Guid participantId)
        {
            ResetCache(projectId);
            base.AddToTeam(projectId, participantId);
        }

        private void ResetCache(int projectId)
        {
            projectCache.Reset(projectId.ToString(CultureInfo.InvariantCulture));
        }

    }

    class ProjectDao : BaseDao, IProjectDao
    {
        public static readonly string[] ProjectColumns = new[] { "id", "title", "description", "status", "responsible_id", "private", "create_by", "create_on", "last_modified_by", "last_modified_on" };

        private readonly HttpRequestDictionary<TeamCacheItem> teamCache = new HttpRequestDictionary<TeamCacheItem>("ProjectDao-TeamCacheItem");
        private readonly HttpRequestDictionary<List<Guid>> followCache = new HttpRequestDictionary<List<Guid>>("ProjectDao-FollowCache");
        private readonly Converter<object[], Project> converter;


        public ProjectDao(int tenantId) : base(tenantId)
        {
            converter = ToProject;
        }


        public List<Project> GetAll(ProjectStatus? status, int max)
        {
            var query = Query(ProjectsTable)
                .Select(ProjectColumns)
                .SetMaxResults(max)
                .OrderBy("title", true);
            if (status != null) query.Where("status", status);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Project> GetLast(ProjectStatus? status, int offset, int max)
        {
            var query = Query(ProjectsTable)
                .SetFirstResult(offset)
                .Select(ProjectColumns)
                .SetMaxResults(max)
                .OrderBy("create_on", false);
            if (status != null) query.Where("status", status);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Project> GetOpenProjectsWithTasks(Guid participantId)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .InnerJoin(TasksTable + " t", Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id"))
                .Where("p.tenant_id", Tenant)
                .Where("p.status", ProjectStatus.Open)
                .OrderBy("p.title", true)
                .GroupBy("p.id");

            if (!participantId.Equals(Guid.Empty))
            {
                query.InnerJoin(ParticipantTable + " ppp", Exp.EqColumns("ppp.tenant", "p.tenant_id") & Exp.EqColumns("ppp.project_id", "p.id") & Exp.Eq("ppp.removed", false))
                    .Where("ppp.participant_id", participantId);

            }

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public DateTime GetMaxLastModified()
        {
            var query = Query(ProjectsTable).SelectMax("last_modified_on");

            return TenantUtil.DateTimeFromUtc(Db.ExecuteScalar<DateTime>(query));
        }

        public void UpdateLastModified(int projectId)
        {
            Db.ExecuteNonQuery(
                Update(ProjectsTable)
                    .Set("last_modified_on", DateTime.UtcNow)
                    .Set("last_modified_by", CurrentUserID)
                    .Where("id", projectId));
        }

        public List<Project> GetByParticipiant(Guid participantId, ProjectStatus status)
        {
            var query = Query(ProjectsTable)
                .Select(ProjectColumns)
                .InnerJoin(ParticipantTable, Exp.EqColumns("id", "project_id") & Exp.Eq("removed", false) & Exp.EqColumns("tenant_id", "tenant"))
                .Where("status", status)
                .Where("participant_id", participantId.ToString())
                .OrderBy("title", true);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public List<Project> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var teamQuery = new SqlQuery(ParticipantTable + " pp")
                .SelectCount()
                .InnerJoin("core_user cup", Exp.EqColumns("cup.tenant", "pp.tenant") & Exp.EqColumns("cup.id", "pp.participant_id"))
                .Where(Exp.EqColumns("pp.tenant", "p.tenant_id"))
                .Where(Exp.EqColumns("pp.project_id", "p.id"))
                .Where("pp.removed", false)
                .Where("cup.status", EmployeeStatus.Active);

            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .Select(new SqlQuery(MilestonesTable + " m").SelectCount().Where(Exp.EqColumns("m.tenant_id", "p.tenant_id") & Exp.EqColumns("m.project_id", "p.id")).Where(Exp.Eq("m.status", MilestoneStatus.Open)))
                .Select(new SqlQuery(TasksTable + " t").SelectCount().Where(Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id")).Where(!Exp.Eq("t.status", TaskStatus.Closed)))
                .Select(new SqlQuery(TasksTable + " t").SelectCount().Where(Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id")))
                .Select(teamQuery)
                .Select("p.private")
                .Where("p.tenant_id", Tenant);

            if (filter.Max > 0 && filter.Max < 150000)
            {
                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max * 2);
            }

            query.OrderBy("(case p.status when 2 then 1 when 1 then 2 else 0 end)", true);

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortColumns = filter.SortColumns["Project"];
                sortColumns.Remove(filter.SortBy);

                query.OrderBy("p." + filter.SortBy, filter.SortOrder);

                foreach (var sort in sortColumns.Keys)
                {
                    query.OrderBy("p." + sort, sortColumns[sort]);
                }
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            return Db.ExecuteList(query).ConvertAll(ToProjectFull);
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                           .Select("p.id")
                           .Where("p.tenant_id", Tenant);

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectCount().From(query, "t1");

            return Db.ExecuteScalar<int>(queryCount);
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.TagId != 0)
            {
                if (filter.TagId == -1)
                {
                    query.LeftOuterJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "p.id"));
                    query.Where("ptag.tag_id", null);
                }
                else
                {
                    query.InnerJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "p.id"));
                    query.Where("ptag.tag_id", filter.TagId);
                }
            }

            if (filter.HasUserId || (filter.ParticipantId.HasValue && !filter.ParticipantId.Equals(Guid.Empty)))
            {
                var existParticipant = new SqlQuery(ParticipantTable + " ppp").Select("ppp.participant_id").Where(Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.Eq("ppp.tenant", Tenant));

                if (filter.DepartmentId != Guid.Empty)
                {
                    existParticipant.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "ppp.participant_id") & Exp.EqColumns("cug.tenant", "ppp.tenant"));
                    existParticipant.Where("cug.groupid", filter.DepartmentId);
                }

                if (filter.ParticipantId.HasValue && !filter.ParticipantId.Equals(Guid.Empty))
                {
                    existParticipant.Where(Exp.Eq("ppp.participant_id", filter.ParticipantId.ToString()));
                }

                query.Where(Exp.Exists(existParticipant));
            }

            if (filter.UserId != Guid.Empty)
            {
                query.Where("responsible_id", filter.UserId);
            }

            if (filter.Follow)
            {
                query.InnerJoin(FollowingProjectTable + " pfpp", Exp.EqColumns("p.id", "pfpp.project_id"));
                query.Where(Exp.Eq("pfpp.participant_id", CurrentUserID));
            }

            if (filter.ProjectStatuses.Any())
            {
                query.Where(Exp.In("p.status", filter.ProjectStatuses));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                List<int> projIds;
                if (FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(filter.SearchText), out projIds))
                {
                    query.Where(Exp.In("p.id", projIds));
                }
                else
                {
                    query.Where(Exp.Like("p.title", filter.SearchText, SqlLike.AnyWhere));
                }
            }

            query.GroupBy("p.id");

            if (checkAccess)
            {
                query.Where(Exp.Eq("p.private", false));
            }
            else if (!isAdmin)
            {
                var isInTeam = new SqlQuery(ParticipantTable).Select("security").Where(Exp.EqColumns("p.id", "project_id") & Exp.Eq("removed", false) & Exp.Eq("participant_id", CurrentUserID) & Exp.EqColumns("tenant", "p.tenant_id"));
                query.Where(Exp.Eq("p.private", false) | Exp.Eq("p.responsible_id", CurrentUserID) | (Exp.Eq("p.private", true) & Exp.Exists(isInTeam)));
            }

            return query;
        }

        public List<Project> GetFollowing(Guid participantId)
        {
            var query = Query(ProjectsTable)
                .Select(ProjectColumns)
                .InnerJoin(FollowingProjectTable, Exp.EqColumns("id", "project_id"))
                .Where("participant_id", participantId.ToString())
                .Where("status", ProjectStatus.Open)
                .OrderBy("create_on", true);

            return Db.ExecuteList(query).ConvertAll(converter);
        }

        public bool IsFollow(int projectId, Guid participantId)
        {
            var users = followCache[projectId.ToString(CultureInfo.InvariantCulture)];
            if (users == null)
            {
                var q = new SqlQuery(FollowingProjectTable).Select("participant_id").Where("project_id", projectId);
                users = Db.ExecuteList(q).ConvertAll(r => new Guid((string)r[0]));
                followCache.Add(projectId.ToString(CultureInfo.InvariantCulture), users);
            }

            return users.Contains(participantId);
        }

        public virtual Project GetById(int projectId)
        {
            return Db.ExecuteList(Query(ProjectsTable).Select(ProjectColumns).Where("id", projectId))
                .ConvertAll(converter)
                .SingleOrDefault();
        }

        public List<Project> GetById(ICollection projectIDs)
        {
            return Db.ExecuteList(Query(ProjectsTable).Select(ProjectColumns).Where(Exp.In("id", projectIDs)))
                .ConvertAll(converter);
        }

        public bool IsExists(int projectId)
        {
            var count = Db.ExecuteScalar<int>(Query(ProjectsTable).SelectCount().Where("id", projectId));
            return 0 < count;
        }

        public List<Project> GetByContactID(int contactId)
        {
            var projectIds = Db
                .ExecuteList(Query("crm_projects").Select("project_id").Where("contact_id", contactId))
                .ConvertAll(r => Convert.ToInt32(r[0]));

            if (!projectIds.Any()) return new List<Project>(0);

            var milestoneCountQuery =
                new SqlQuery(MilestonesTable + " m").SelectCount()
                    .Where(Exp.EqColumns("m.project_id", "p.id"))
                    .Where(Exp.Eq("m.status", MilestoneStatus.Open))
                    .Where(Exp.EqColumns("m.tenant_id", "p.tenant_id"));

            var taskCountQuery =
                new SqlQuery(TasksTable + " t").SelectCount()
                    .Where(Exp.EqColumns("t.project_id", "p.id"))
                    .Where(!Exp.Eq("t.status", TaskStatus.Closed))
                    .Where(Exp.EqColumns("t.tenant_id", "p.tenant_id"));
            var participantCountQuery =
                new SqlQuery(ParticipantTable + " pp").SelectCount()
                    .Where(Exp.EqColumns("pp.project_id", "p.id") & Exp.Eq("pp.removed", false))
                    .Where(Exp.EqColumns("pp.tenant", "p.tenant_id"));

            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .Select(milestoneCountQuery)
                .Select(taskCountQuery)
                .Select(participantCountQuery)
                .Where(Exp.In("p.id", projectIds.ToList()))
                .Where("p.tenant_id", Tenant);

            return Db.ExecuteList(query)
                .Select(r =>
                            {
                                var prj = ToProject(r);
                                prj.TaskCount = Convert.ToInt32(r[11]);
                                prj.MilestoneCount = Convert.ToInt32(r[10]);
                                prj.ParticipantCount = Convert.ToInt32(r[12]);
                                return prj;
                            }
                ).ToList();
        }

        public void AddProjectContact(int projectID, int contactID)
        {
            using (var crmDb = new DbManager("crm"))
            {
                crmDb.ExecuteNonQuery(Insert("crm_projects").InColumnValue("project_id", projectID).InColumnValue("contact_id", contactID));
            }
        }

        public void DeleteProjectContact(int projectID, int contactID)
        {
            using (var crmDb = new DbManager("crm"))
            {
                crmDb.ExecuteNonQuery(Delete("crm_projects").Where("project_id", projectID).Where("contact_id", contactID));
            }
        }

        public int Count()
        {
            return Db.ExecuteScalar<int>(Query(ProjectsTable).SelectCount());
        }

        public List<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus, bool isAdmin)
        {
            var query = new SqlQuery(TasksTable + " t")

                .Select("t.project_id").SelectCount()
                .Where(Exp.In("t.project_id", projectId))
                .Where("t.tenant_id", Tenant)
                .GroupBy("t.project_id");

            if (taskStatus.HasValue)
            {
                if (taskStatus.Value == TaskStatus.Open)
                    query.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                else
                    query.Where("t.status", TaskStatus.Closed);
            }

            if (!isAdmin)
            {
                query.InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                    .LeftOuterJoin(TasksResponsibleTable + " ptr", Exp.EqColumns("t.tenant_id", "ptr.tenant_id") & Exp.EqColumns("t.id", "ptr.task_id") & Exp.Eq("ptr.responsible_id", CurrentUserID))
                    .LeftOuterJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.EqColumns("p.tenant_id", "ppp.tenant") & Exp.Eq("ppp.removed", false) & Exp.Eq("ppp.participant_id", CurrentUserID))
                    .Where(Exp.Eq("p.private", false) | !Exp.Eq("ptr.responsible_id", null) | (Exp.Eq("p.private", true) & !Exp.Eq("ppp.security", null) & !Exp.Eq("ppp.security & " + (int)ProjectTeamSecurity.Tasks, (int)ProjectTeamSecurity.Tasks)));
            }
            var result = Db.ExecuteList(query);

            return projectId.ConvertAll(
                pid =>
                    {
                        var res = result.Find(r => Convert.ToInt32(r[0]) == pid);
                        return res == null ? 0 : Convert.ToInt32(res[1]);
                    }
                );
        }

        public int GetMessageCount(int projectId)
        {
            var query = Query(MessagesTable)
                .SelectCount()
                .Where("project_id", projectId);

            return Db.ExecuteScalar<int>(query);
        }

        public int GetTotalTimeCount(int projectId)
        {
            var query = Query(TimeTrackingTable)
                .SelectCount()
                .Where("project_id", projectId);

            return Db.ExecuteScalar<int>(query);
        }

        public int GetMilestoneCount(int projectId, params MilestoneStatus[] statuses)
        {
            var query = Query(MilestonesTable)
                .SelectCount()
                .Where("project_id", projectId);
            if (statuses.Any())
            {
                query.Where(Exp.In("status", statuses));
            }

            return Db.ExecuteScalar<int>(query);
        }

        public Project Create(Project project)
        {
            var insert = Insert(ProjectsTable, false)
                .InColumns("id", "title", "description", "status", "responsible_id", "private", "create_by", "create_on", "last_modified_on")
                .Values(
                    project.ID,
                    project.Title,
                    project.Description,
                    project.Status,
                    project.Responsible.ToString(),
                    project.Private,
                    project.CreateBy.ToString(),
                    TenantUtil.DateTimeToUtc(project.CreateOn),
                    TenantUtil.DateTimeToUtc(project.LastModifiedOn))
                .Identity(1, 0, true);

            project.ID = Db.ExecuteScalar<int>(insert);

            return project;
        }

        public virtual Project Update(Project project)
        {
            var update = Update(ProjectsTable)
                .Set("title", project.Title)
                .Set("description", project.Description)
                .Set("status", project.Status)
                .Set("status_changed", project.StatusChangedOn)
                .Set("responsible_id", project.Responsible.ToString())
                .Set("private", project.Private)
                .Set("last_modified_by", project.LastModifiedBy.ToString())
                .Set("last_modified_on", TenantUtil.DateTimeToUtc(project.LastModifiedOn))
                .Where("id", project.ID);

            Db.ExecuteNonQuery(update);

            return project;
        }

        public virtual void Delete(int projectId, out List<int> messages, out List<int> tasks)
        {
            using (var tx = Db.BeginTransaction())
            {
                messages = Db.ExecuteList(Query(MessagesTable)
                            .Select("id")
                            .Where("project_id", projectId)).ConvertAll(r => Convert.ToInt32(r[0]));

                var milestones = Db.ExecuteList(Query(MilestonesTable)
                                                    .Select("id")
                                                    .Where("project_id", projectId)).ConvertAll(r => Convert.ToInt32(r[0]));

                tasks = Db.ExecuteList(Query(TasksTable)
                                               .Select("id")
                                               .Where("project_id", projectId)).ConvertAll(r => Convert.ToInt32(r[0]));

                if (messages.Any())
                {
                    Db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", messages.Select(r => "Message_" + r).ToList())));
                    Db.ExecuteNonQuery(Delete(MessagesTable).Where("project_id", projectId));
                }
                if (milestones.Any())
                {
                    Db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", milestones.Select(r => "Milestone_" + r).ToList())));
                    Db.ExecuteNonQuery(Delete(MilestonesTable).Where("project_id", projectId));
                }
                if (tasks.Any())
                {
                    Db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", tasks.Select(r => "Task_" + r).ToList())));
                    Db.ExecuteNonQuery(Delete(TasksOrderTable).Where("project_id", projectId));
                    Db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where(Exp.In("task_id", tasks)));
                    Db.ExecuteNonQuery(Delete(SubtasksTable).Where(Exp.In("task_id", tasks)));
                    Db.ExecuteNonQuery(Delete(TasksTable).Where("project_id", projectId));
                }

                Db.ExecuteNonQuery(new SqlDelete(ParticipantTable).Where("project_id", projectId).Where("tenant", Tenant));
                Db.ExecuteNonQuery(new SqlDelete(FollowingProjectTable).Where("project_id", projectId));
                Db.ExecuteNonQuery(new SqlDelete(ProjectTagTable).Where("project_id", projectId));
                Db.ExecuteNonQuery(Delete(TimeTrackingTable).Where("project_id", projectId));
                Db.ExecuteNonQuery(Delete(ProjectsTable).Where("id", projectId));

                tx.Commit();
            }
            Db.ExecuteNonQuery(Delete(TagsTable).Where(!Exp.In("id", new SqlQuery("projects_project_tag").Select("tag_id"))));
        }


        public virtual void AddToTeam(int projectId, Guid participantId)
        {
            Db.ExecuteNonQuery(
                new SqlInsert(ParticipantTable, true)
                    .InColumnValue("project_id", projectId)
                    .InColumnValue("participant_id", participantId.ToString())
                    .InColumnValue("created", DateTime.UtcNow)
                    .InColumnValue("updated", DateTime.UtcNow)
                    .InColumnValue("removed", false)
                    .InColumnValue("tenant", Tenant));

            UpdateLastModified(projectId);

            var key = string.Format("{0}|{1}", projectId, participantId);
            var item = teamCache.Get(key, () => new TeamCacheItem(true, ProjectTeamSecurity.None));
            if (item != null) item.InTeam = true;
        }

        public virtual void RemoveFromTeam(int projectId, Guid participantId)
        {
            Db.ExecuteNonQuery(
                new SqlUpdate(ParticipantTable)
                    .Set("removed", true)
                    .Set("updated", DateTime.UtcNow)
                    .Where("tenant", Tenant)
                    .Where("project_id", projectId)
                    .Where("participant_id", participantId.ToString()));

            UpdateLastModified(projectId);

            var key = string.Format("{0}|{1}", projectId, participantId);
            var item = teamCache.Get(key, () => new TeamCacheItem(true, ProjectTeamSecurity.None));
            if (item != null) item.InTeam = false;
        }

        public bool IsInTeam(int projectId, Guid participantId)
        {
            return GetTeamItemFromCacheOrLoad(projectId, participantId).InTeam;
        }

        public List<Participant> GetTeam(Project project, bool withExcluded = false)
        {
            if (project == null) return new List<Participant>();

            var query = new SqlQuery(ParticipantTable + " pp")
                .InnerJoin(ProjectsTable + " pt",
                    Exp.EqColumns("pp.tenant", "pt.tenant_id") & Exp.EqColumns("pp.project_id", "pt.id"))
                .Select("pp.participant_id, pp.security, pp.project_id")
                .Select(Exp.EqColumns("pp.project_id", "pt.id"))
                .Select("pp.removed")
                .Where("pp.tenant", Tenant)
                .Where("pp.project_id", project.ID);

            if (!withExcluded)
            {
                query.Where("pp.removed", false);
            }

            return Db.ExecuteList(query).ConvertAll(ToParticipant);
        }

        public List<Participant> GetTeam(IEnumerable<Project> projects)
        {
            return Db.ExecuteList(
                new SqlQuery(ParticipantTable + " pp")
                    .InnerJoin(ProjectsTable + " pt", Exp.EqColumns("pp.tenant", "pt.tenant_id") & Exp.EqColumns("pp.project_id", "pt.id"))
                    .Select("distinct pp.participant_id, pp.security, pp.project_id")
                    .Select(Exp.EqColumns("pp.project_id", "pt.id"))
                    .Select("pp.removed")
                    .Where("pp.tenant", Tenant)
                    .Where(Exp.In("pp.project_id", projects.Select(r => r.ID).ToArray()))
                    .Where("pp.removed", false))
                .ConvertAll(ToParticipant);
        }

        public List<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(x => "p." + x).ToArray())
                .Select("pp.participant_id", "pp.removed", "pp.created", "pp.updated")
                .LeftOuterJoin("projects_project_participant pp", Exp.EqColumns("pp.project_id", "p.id"))
                .Where(Exp.Between("pp.created", from, to) | Exp.Between("pp.updated", from, to));

            return Db.ExecuteList(query).ConvertAll(ToParticipantFull);
        }

        public DateTime GetTeamMaxLastModified()
        {
            var query = new SqlQuery(ParticipantTable).SelectMax("updated").Where("tenant", Tenant);

            return TenantUtil.DateTimeFromUtc(Db.ExecuteScalar<DateTime>(query));
        }

        public void SetTeamSecurity(int projectId, Guid participantId, ProjectTeamSecurity teamSecurity)
        {
            Db.ExecuteNonQuery(
                new SqlUpdate(ParticipantTable)
                    .Set("updated", DateTime.UtcNow)
                    .Set("security", (int)teamSecurity)
                    .Where("tenant", Tenant)
                    .Where("project_id", projectId)
                    .Where("participant_id", participantId.ToString()));

            var key = string.Format("{0}|{1}", projectId, participantId);
            var item = teamCache.Get(key);
            if (item != null) teamCache[key].Security = teamSecurity;
        }

        public ProjectTeamSecurity GetTeamSecurity(int projectId, Guid participantId)
        {
            return GetTeamItemFromCacheOrLoad(projectId, participantId).Security;
        }

        private TeamCacheItem GetTeamItemFromCacheOrLoad(int projectId, Guid participantId)
        {
            var key = string.Format("{0}|{1}", projectId, participantId);

            var item = teamCache.Get(key);
            if (item != null) return item;

            item = teamCache.Get(string.Format("{0}|{1}", 0, participantId));
            if (item != null) return new TeamCacheItem(false, ProjectTeamSecurity.None);

            var projectList = Db.ExecuteList(
                new SqlQuery(ParticipantTable)
                    .Select("project_id", "security")
                    .Where("tenant", Tenant)
                    .Where("participant_id", participantId.ToString())
                    .Where("removed", false));

            var teamCacheItem = new TeamCacheItem(true, ProjectTeamSecurity.None);
            teamCache.Add(string.Format("{0}|{1}", 0, participantId), teamCacheItem);

            foreach (var prj in projectList)
            {
                teamCacheItem = new TeamCacheItem(true, (ProjectTeamSecurity)Convert.ToInt32(prj[1]));
                key = string.Format("{0}|{1}", prj[0], participantId);
                teamCache.Add(key, teamCacheItem);
            }

            var currentProject = projectList.Find(r => Convert.ToInt32(r[0]) == projectId);
            teamCacheItem = new TeamCacheItem(currentProject != null,
                                                currentProject != null
                                                    ? (ProjectTeamSecurity)Convert.ToInt32(currentProject[1])
                                                    : ProjectTeamSecurity.None);
            key = string.Format("{0}|{1}", projectId, participantId);
            teamCache.Add(key, teamCacheItem);
            return teamCacheItem;
        }


        public void SetTaskOrder(int projectID, string order)
        {
            using (var tr = Db.BeginTransaction())
            {
                var query = Insert(TasksOrderTable)
                  .InColumnValue("project_id", projectID)
                  .InColumnValue("task_order", order);

                Db.ExecuteNonQuery(query);

                try
                {
                    var orderJson = JObject.Parse(order);
                    var newTaskOrder = orderJson["tasks"].Select(r => r.Value<int>()).ToList();

                    for (var i = 0; i < newTaskOrder.Count; i++)
                    {
                        Db.ExecuteNonQuery(Update(TasksTable)
                            .Where("project_id", projectID)
                            .Where("id", newTaskOrder[i])
                            .Set("sort_order", i));
                    }
                }
                finally
                {
                    tr.Commit();
                }
            }
        }

        public string GetTaskOrder(int projectID)
        {
            var query = Query(TasksOrderTable)
                .Select("task_order")
                .Where("project_id", projectID);

            return Db.ExecuteList(query, r => Convert.ToString(r[0])).FirstOrDefault();
        }

        public static ParticipantFull ToParticipantFull(object[] x)
        {
            int offset = ProjectColumns.Count();
            return new ParticipantFull(new Guid((string)x[0 + offset]))
            {
                Project = ToProject(x),
                Removed = Convert.ToBoolean(x[1 + offset]),
                Created = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(x[2 + offset])),
                Updated = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(x[3 + offset]))
            };
        }

        public static Project ToProject(object[] r)
        {
            return new Project
            {
                ID = Convert.ToInt32(r[0]),
                Title = (string)r[1],
                Description = (string)r[2],
                Status = (ProjectStatus)Convert.ToInt32(r[3]),
                Responsible = ToGuid(r[4]),
                Private = Convert.ToBoolean(r[5]),
                CreateBy = ToGuid(r[6]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[7])),
                LastModifiedBy = ToGuid(r[8]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[9])),
            };
        }

        public static Project ToProjectFull(object[] r)
        {
            var project = ToProject(r);

            project.TaskCount = Convert.ToInt32(r[11]);
            project.TaskCountTotal = Convert.ToInt32(r[12]);
            project.MilestoneCount = Convert.ToInt32(r[10]);
            project.ParticipantCount = Convert.ToInt32(r[13]);

            return project;
        }

        public static Participant ToParticipant(object[] r)
        {
            return new Participant(new Guid((string)r[0]))
            {
                ProjectTeamSecurity = (ProjectTeamSecurity)Convert.ToInt32(r[1]),
                ProjectID = Convert.ToInt32(r[2]),
                IsManager = Convert.ToBoolean(r[3]),
                IsRemovedFromTeam = Convert.ToBoolean(r[4])
            };
        }

        private class TeamCacheItem
        {
            public bool InTeam { get; set; }

            public ProjectTeamSecurity Security { get; set; }

            public TeamCacheItem(bool inteam, ProjectTeamSecurity security)
            {
                InTeam = inteam;
                Security = security;
            }
        }


        public IEnumerable<Project> GetProjects(Exp where)
        {
            return Db.ExecuteList(Query(ProjectsTable + " p").Select(ProjectColumns).Where(where)).ConvertAll(converter);
        }
    }
}
