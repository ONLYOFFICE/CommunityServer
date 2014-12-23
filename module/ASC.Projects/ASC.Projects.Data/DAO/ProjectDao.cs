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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    internal class CachedProjectDao : ProjectDao
    {
        private readonly HttpRequestDictionary<Project> projectCache = new HttpRequestDictionary<Project>("project");

        public CachedProjectDao(string dbId, int tenantId)
            : base(dbId, tenantId)
        {
        }

        public override void Delete(int projectId)
        {
            ResetCache(projectId);
            base.Delete(projectId);
        }

        public override void RemoveFromTeam(int projectId, Guid participantId)
        {
            ResetCache(projectId);
            base.RemoveFromTeam(projectId, participantId);
        }

        public override Project Save(Project project)
        {
            if (project != null)
            {
                ResetCache(project.ID);
            }
            return base.Save(project);
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

        private static readonly HttpRequestDictionary<TeamCacheItem> TeamCache = new HttpRequestDictionary<TeamCacheItem>("ProjectDao-TeamCacheItem");
        private static readonly HttpRequestDictionary<List<Guid>> FollowCache = new HttpRequestDictionary<List<Guid>>("ProjectDao-FollowCache");



        public ProjectDao(string dbId, int tenantId)
            : base(dbId, tenantId)
        {
        }


        public List<Project> GetAll(ProjectStatus? status, int max)
        {
            var query = Query(ProjectsTable)
                .Select(ProjectColumns)
                .SetMaxResults(max)
                .OrderBy("title", true);
            if (status != null) query.Where("status", status);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProject).ToList();
            }
        }

        public List<Project> GetLast(ProjectStatus? status, int offset, int max)
        {
            var query = Query(ProjectsTable)
                .SetFirstResult(offset)
                .Select(ProjectColumns)
                .SetMaxResults(max)
                .OrderBy("create_on", false);
            if (status != null) query.Where("status", status);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProject).ToList();
            }
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
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProject).ToList();
            }
        }

        public DateTime GetMaxLastModified()
        {
            var query = Query(ProjectsTable).SelectMax("last_modified_on");

            using (var db = new DbManager(DatabaseId))
            {
                return TenantUtil.DateTimeFromUtc(db.ExecuteScalar<DateTime>(query));
            }
        }

        public void UpdateLastModified(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    Update(ProjectsTable)
                        .Set("last_modified_on", DateTime.UtcNow)
                        .Set("last_modified_by", CurrentUserID)
                        .Where("id", projectId));
            }
        }

        public List<Project> GetByParticipiant(Guid participantId, ProjectStatus status)
        {
            var query = Query(ProjectsTable)
                .Select(ProjectColumns)
                .InnerJoin(ParticipantTable, Exp.EqColumns("id", "project_id") & Exp.Eq("removed", false) & Exp.EqColumns("tenant_id", "tenant"))
                .Where("status", status)
                .Where("participant_id", participantId.ToString())
                .OrderBy("title", true);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProject).ToList();
            }
        }

        public List<Project> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .Select(new SqlQuery(MilestonesTable + " m").SelectCount().Where(Exp.EqColumns("m.tenant_id", "p.tenant_id") & Exp.EqColumns("m.project_id", "p.id")).Where(Exp.Eq("m.status", MilestoneStatus.Open)))
                .Select(new SqlQuery(TasksTable + " t").SelectCount().Where(Exp.EqColumns("t.tenant_id", "p.tenant_id") & Exp.EqColumns("t.project_id", "p.id")).Where(!Exp.Eq("t.status", TaskStatus.Closed)))
                .Select(new SqlQuery(ParticipantTable + " pp").SelectCount().Where(Exp.EqColumns("pp.tenant", "p.tenant_id") & Exp.EqColumns("pp.project_id", "p.id") & Exp.Eq("pp.removed", false)))
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

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProjectFull).ToList();
            }
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                           .Select("p.id")
                           .Where("p.tenant_id", Tenant);

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = new SqlQuery().SelectCount().From(query, "t1");

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(queryCount);
            }
        }

        private SqlQuery CreateQueryFilter(SqlQuery query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.TagId != 0)
            {
                query.InnerJoin(ProjectTagTable + " ptag", Exp.EqColumns("ptag.project_id", "p.id"));
                query.Where("ptag.tag_id", filter.TagId);
            }

            if (filter.HasUserId || (filter.ParticipantId.HasValue && filter.ParticipantId != Guid.Empty))
            {
                var existParticipant = new SqlQuery(ParticipantTable + " ppp").Select("ppp.participant_id").Where(Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.Eq("ppp.tenant", Tenant));

                if (filter.DepartmentId != Guid.Empty)
                {
                    existParticipant.InnerJoin("core_usergroup cug", Exp.Eq("cug.removed", false) & Exp.EqColumns("cug.userid", "ppp.participant_id") & Exp.EqColumns("cug.tenant", "ppp.tenant"));
                    existParticipant.Where("cug.groupid", filter.DepartmentId);
                }

                if (filter.ParticipantId.HasValue && filter.ParticipantId != Guid.Empty)
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

            if (filter.ProjectStatuses.Count != 0)
            {
                query.Where(Exp.Eq("p.status", filter.ProjectStatuses.First()));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query.Where(Exp.Like("p.title", filter.SearchText, SqlLike.AnyWhere));
            }

            query.GroupBy("p.id");

            if (checkAccess)
            {
                query.Where(Exp.Eq("p.private", false));
            }
            else if (!isAdmin)
            {
                var isInTeam = new SqlQuery(ParticipantTable).Select("security").Where(Exp.EqColumns("p.id", "project_id") & Exp.Eq("removed", false) & Exp.Eq("participant_id", CurrentUserID));
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

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProject).ToList();
            }
        }

        public bool IsFollow(int projectId, Guid participantId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var users = FollowCache[projectId.ToString(CultureInfo.InvariantCulture)];
                if (users == null)
                {
                    var q = new SqlQuery(FollowingProjectTable).Select("participant_id").Where("project_id", projectId);
                    users = db.ExecuteList(q).ConvertAll(r => new Guid((string) r[0]));
                    FollowCache.Add(projectId.ToString(CultureInfo.InvariantCulture), users);
                }

                return users.Contains(participantId);
            }
        }

        public virtual Project GetById(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(Query(ProjectsTable).Select(ProjectColumns).Where("id", projectId))
                    .ConvertAll(ToProject)
                    .SingleOrDefault();
            }
        }

        public Project GetFullProjectById(int projectId)
        {
            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .Select(new SqlQuery("projects_milestones m").SelectCount().Where(Exp.EqColumns("m.project_id", "p.id")).Where(Exp.Eq("m.status", MilestoneStatus.Open)))
                .Select(new SqlQuery("projects_tasks t").SelectCount().Where(Exp.EqColumns("t.project_id", "p.id")).Where(!Exp.Eq("t.status", TaskStatus.Closed)))
                .Select(new SqlQuery("projects_project_participant pp").SelectCount().Where(Exp.EqColumns("pp.project_id", "p.id") & Exp.Eq("pp.removed", false)))
                .Where("p.id", projectId)
                .Where("p.tenant_id", Tenant); 

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query).ConvertAll(ToProjectFull).SingleOrDefault();
            }
        }
        public List<Project> GetById(ICollection projectIDs)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(Query(ProjectsTable).Select(ProjectColumns).Where(Exp.In("id", projectIDs)))
                    .ConvertAll(ToProject)
                    .ToList();
            }
        }

        public bool IsExists(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var count = db.ExecuteScalar<int>(Query(ProjectsTable).SelectCount().Where("id", projectId));
                return 0 < count;
            }
        }

        public List<Project> GetByContactID(int contactId)
        {
            IEnumerable<int> projectIds;
            using (var crmDb = new DbManager("crm"))
            {
                projectIds = crmDb
                    .ExecuteList(Query("crm_projects").Select("project_id").Where("contact_id", contactId))
                    .ConvertAll(r => Convert.ToInt32(r[0]));
            }

            var query = new SqlQuery(ProjectsTable + " p")
                .Select(ProjectColumns.Select(c => "p." + c).ToArray())
                .Select(new SqlQuery(MilestonesTable + " m").SelectCount().Where(Exp.EqColumns("m.project_id", "p.id")).Where(Exp.Eq("m.status", MilestoneStatus.Open)))
                .Select(new SqlQuery(TasksTable + " t").SelectCount().Where(Exp.EqColumns("t.project_id", "p.id")).Where(!Exp.Eq("t.status", TaskStatus.Closed)))
                .Select(new SqlQuery(ParticipantTable + " pp").SelectCount().Where(Exp.EqColumns("pp.project_id", "p.id") & Exp.Eq("pp.removed", false)))
                .Where(Exp.In("p.id", projectIds.ToList()))
                .Where("p.tenant_id", Tenant);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(query)
                    .Select(r =>
                                {
                                    var prj = ToProject(r);
                                    prj.TaskCount = Convert.ToInt32(r[11]);
                                    prj.MilestoneCount = Convert.ToInt32(r[10]);
                                    prj.ParticipantCount = Convert.ToInt32(r[12]);
                                    return prj;
                                }
                    )
                    .ToList();
            }
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
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(Query(ProjectsTable).SelectCount());
            }
        }

        public List<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus, bool isAdmin)
        {
            var query = new SqlQuery(TasksTable + " t")

                .Select("t.project_id").SelectCount()
                .Where(Exp.In("t.project_id", projectId))
                .Where("t.tenant_id", Tenant)
                .GroupBy("t.project_id");

            if (taskStatus != null)
            {
                if (taskStatus == TaskStatus.Open)
                    query.Where(!Exp.Eq("t.status", TaskStatus.Closed));
                else
                    query.Where("t.status", TaskStatus.Closed);
            }

            if (!isAdmin)
            {
                query.InnerJoin(ProjectsTable + " p", Exp.EqColumns("t.project_id", "p.id") & Exp.EqColumns("t.tenant_id", "p.tenant_id"))
                    .LeftOuterJoin(TasksResponsibleTable + " ptr", Exp.EqColumns("t.tenant_id", "ptr.tenant_id") & Exp.EqColumns("t.id", "ptr.task_id") & Exp.Eq("ptr.responsible_id", CurrentUserID))
                    .LeftOuterJoin(ParticipantTable + " ppp", Exp.EqColumns("p.id", "ppp.project_id") & Exp.Eq("ppp.removed", false) & Exp.Eq("ppp.participant_id", CurrentUserID))
                    .Where(Exp.Eq("p.private", false) | !Exp.Eq("ptr.responsible_id", null) | (Exp.Eq("p.private", true) & !Exp.Eq("ppp.security", null) & !Exp.Eq("ppp.security & " + (int)ProjectTeamSecurity.Tasks, (int)ProjectTeamSecurity.Tasks)));
            }
            using (var db = new DbManager(DatabaseId))
            {
                var result = db.ExecuteList(query);

                return projectId.ConvertAll(
                    pid =>
                        {
                            var res = result.Find(r => Convert.ToInt32(r[0]) == pid);
                            return res == null ? 0 : Convert.ToInt32(res[1]);
                        }
                    );
            }
        }

        public int GetMessageCount(int projectId)
        {
            var query = Query(MessagesTable)
                .SelectCount()
                .Where("project_id", projectId);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(query);
            }
        }

        public int GetTotalTimeCount(int projectId)
        {
            var query = Query(TimeTrackingTable)
                .SelectCount()
                .Where("project_id", projectId);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(query);
            }
        }

        public int GetMilestoneCount(int projectId, params MilestoneStatus[] statuses)
        {
            var query = Query(MilestonesTable)
                .SelectCount()
                .Where("project_id", projectId);
            if (statuses != null && 0 < statuses.Length)
            {
                query.Where(Exp.In("status", statuses));
            }

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(query);
            }
        }

        public virtual Project Save(Project project)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tx = db.BeginTransaction())
                {
                    var insert = Insert(ProjectsTable)
                        .InColumns(ProjectColumns)
                        .Values(
                            project.ID,
                            project.Title,
                            project.Description,
                            project.Status,
                            project.Responsible.ToString(),
                            project.Private,
                            project.CreateBy.ToString(),
                            TenantUtil.DateTimeToUtc(project.CreateOn),
                            project.LastModifiedBy.ToString(),
                            TenantUtil.DateTimeToUtc(project.LastModifiedOn))
                        .InColumnValue("status_changed", project.StatusChangedOn)
                        .Identity(1, 0, true);
                    project.ID = db.ExecuteScalar<int>(insert);

                    //remove not used tags
                    db.ExecuteNonQuery(Delete(TagsTable).Where(!Exp.In("id", new SqlQuery("projects_project_tag").Select("tag_id"))));

                    tx.Commit();
                }
            }

            return project;
        }

        public virtual void Delete(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(new SqlDelete(ParticipantTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(new SqlDelete(FollowingProjectTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(new SqlDelete(ProjectTagTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(Delete(TagsTable).Where(!Exp.In("id", new SqlQuery("projects_project_tag").Select("tag_id"))));
                    db.ExecuteNonQuery(Delete(TimeTrackingTable).Where("project_id", projectId));

                    var messages = db.ExecuteList(Query(MessagesTable)
                                .Select("concat('Message_', cast(id as char))")
                                .Where("project_id", projectId)).ConvertAll(r => (string) r[0]);

                    var milestones = db.ExecuteList(Query(MilestonesTable)
                                                        .Select("concat('Milestone_', cast(id as char))")
                                                        .Where("project_id", projectId)).ConvertAll(r => (string) r[0]);

                    var tasks = db.ExecuteList(Query(TasksTable)
                                                   .Select("id")
                                                   .Where("project_id", projectId)).ConvertAll(r => Convert.ToInt32(r[0]));

                    db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", messages)));
                    db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", milestones)));
                    db.ExecuteNonQuery(Delete(CommentsTable).Where(Exp.In("target_uniq_id", new object[]{ tasks.Select(r=> "Task_" + r) })));

                    db.ExecuteNonQuery(Delete(MessagesTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(Delete(MilestonesTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(Delete(TasksOrderTable).Where("project_id", projectId));
                    db.ExecuteNonQuery(Delete(TasksResponsibleTable).Where(Exp.In("task_id", tasks)));
                    db.ExecuteNonQuery(Delete(SubtasksTable).Where(Exp.In("task_id", tasks)));
                    db.ExecuteNonQuery(Delete(TasksTable).Where("project_id", projectId));

                    db.ExecuteNonQuery(Delete(ProjectsTable).Where("id", projectId));

                    tx.Commit();
                }
            }
        }


        public virtual void AddToTeam(int projectId, Guid participantId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    new SqlInsert(ParticipantTable, true)
                        .InColumnValue("project_id", projectId)
                        .InColumnValue("participant_id", participantId.ToString())
                        .InColumnValue("created", DateTime.UtcNow)
                        .InColumnValue("updated", DateTime.UtcNow)
                        .InColumnValue("removed", false)
                        .InColumnValue("tenant", Tenant));
            }

            UpdateLastModified(projectId);

            lock (TeamCache)
            {
                var key = string.Format("{0}|{1}", projectId, participantId);
                var item = TeamCache.Get(key, () => new TeamCacheItem(true, ProjectTeamSecurity.None));
                if (item != null) item.InTeam = true;
            }
        }

        public virtual void RemoveFromTeam(int projectId, Guid participantId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(ParticipantTable)
                        .Set("removed", true)
                        .Set("updated", DateTime.UtcNow)
                        .Where("tenant", Tenant)
                        .Where("project_id", projectId)
                        .Where("participant_id", participantId.ToString()));
            }

            UpdateLastModified(projectId);

            lock (TeamCache)
            {
                var key = string.Format("{0}|{1}", projectId, participantId);
                var item = TeamCache.Get(key, () => new TeamCacheItem(true, ProjectTeamSecurity.None));
                if (item != null) item.InTeam = false;
            }
        }

        public bool IsInTeam(int projectId, Guid participantId)
        {
            return GetTeamItemFromCacheOrLoad(projectId, participantId).InTeam;
        }

        public List<Participant> GetTeam(int projectId)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(
                    new SqlQuery(ParticipantTable)
                        .Select("participant_id, security")
                        .Where("tenant", Tenant)
                        .Where("project_id", projectId)
                        .Where("removed", false))
                    .ConvertAll(r =>new Participant(new Guid((string) r[0]), (ProjectTeamSecurity) Convert.ToInt32(r[1])));
            }
        }

        public List<Participant> GetTeam(List<int> projectIds)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(
                    new SqlQuery(ParticipantTable)
                        .Select("distinct participant_id, security, project_id")
                        .Where("tenant", Tenant)
                        .Where(Exp.In("project_id", projectIds))
                        .Where("removed", false))
                    .ConvertAll( r =>
                        new Participant(new Guid((string) r[0]), (ProjectTeamSecurity) Convert.ToInt32(r[1]))
                            {
                                ProjectID = Convert.ToInt32(r[2])
                            });
            }
        }

        public List<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = new SqlQuery(ProjectsTable + " p")
                    .Select(ProjectColumns.Select(x => "p." + x).ToArray())
                    .Select("pp.participant_id", "pp.removed", "pp.created", "pp.updated")
                    .LeftOuterJoin("projects_project_participant pp", Exp.EqColumns("pp.project_id", "p.id"))
                    .Where(Exp.Between("pp.created", from, to) | Exp.Between("pp.updated", from, to));

                return db.ExecuteList(query).Select(ToParticipantFull).ToList();
            }
        }

        public DateTime GetTeamMaxLastModified()
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = new SqlQuery(ParticipantTable).SelectMax("updated").Where("tenant", Tenant);

                return TenantUtil.DateTimeFromUtc(db.ExecuteScalar<DateTime>(query));
            }
        }

        public void SetTeamSecurity(int projectId, Guid participantId, ProjectTeamSecurity teamSecurity)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(ParticipantTable)
                        .Set("updated", DateTime.UtcNow)
                        .Set("security", (int) teamSecurity)
                        .Where("tenant", Tenant)
                        .Where("project_id", projectId)
                        .Where("participant_id", participantId.ToString()));
            }

            lock (TeamCache)
            {
                var key = string.Format("{0}|{1}", projectId, participantId);
                var item = TeamCache.Get(key);
                if (item != null) TeamCache[key].Security = teamSecurity;
            }
        }

        public ProjectTeamSecurity GetTeamSecurity(int projectId, Guid participantId)
        {
            return GetTeamItemFromCacheOrLoad(projectId, participantId).Security;
        }

        private TeamCacheItem GetTeamItemFromCacheOrLoad(int projectId, Guid participantId)
        {
            var key = string.Format("{0}|{1}", projectId, participantId);

            lock (TeamCache)
            {
                var item = TeamCache.Get(key);
                if (item != null) return item;

                item = TeamCache.Get(string.Format("{0}|{1}", 0, participantId));
                if (item != null) return new TeamCacheItem(false, ProjectTeamSecurity.None);
            }

            List<object[]> projectList;
            
            using (var db = new DbManager(DatabaseId))
            {
                projectList = db.ExecuteList(
                    new SqlQuery(ParticipantTable)
                        .Select("project_id", "security")
                        .Where("tenant", Tenant)
                        .Where("participant_id", participantId.ToString())
                        .Where("removed", false));
            }
           
            lock (TeamCache)
            {
                var teamCacheItem = new TeamCacheItem(true, ProjectTeamSecurity.None);
                TeamCache.Add(string.Format("{0}|{1}", 0, participantId), teamCacheItem);

                foreach (var prj in projectList)
                {
                    teamCacheItem = new TeamCacheItem(true, (ProjectTeamSecurity)Convert.ToInt32(prj[1]));
                    key = string.Format("{0}|{1}", prj[0], participantId);
                    TeamCache.Add(key, teamCacheItem);
                }

                var currentProject = projectList.Find(r => Convert.ToInt32(r[0]) == projectId);
                teamCacheItem = new TeamCacheItem(currentProject != null, currentProject != null ? (ProjectTeamSecurity)Convert.ToInt32(currentProject[1]) : ProjectTeamSecurity.None);
                key = string.Format("{0}|{1}", projectId, participantId);
                TeamCache.Add(key, teamCacheItem);
                return teamCacheItem;
            }
        }


        public void SetTaskOrder(int projectID, string order)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Insert(TasksOrderTable)
                  .InColumnValue("project_id", projectID)
                  .InColumnValue("task_order", order);

                db.ExecuteNonQuery(query);
            }
        }

        public string GetTaskOrder(int projectID)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Query(TasksOrderTable)
                  .Select("task_order")
                  .Where("project_id", projectID);

                return db.ExecuteList(query, r=> Convert.ToString(r[0])).FirstOrDefault();
            }
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
            project.MilestoneCount = Convert.ToInt32(r[10]);
            project.ParticipantCount = Convert.ToInt32(r[12]);

            return project;
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


        internal List<Project> GetProjects(Exp where)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(Query(ProjectsTable + " p").Select(ProjectColumns).Where(where)).ConvertAll(ToProject);
            }
        }
    }
}
