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
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Projects;
using ASC.Web.Projects.Classes;
using Autofac;

namespace ASC.Projects.Engine
{
    public class ProjectSecurityCommon
    {
        public bool Can(Guid userId)
        {
            return !IsVisitor(userId) && IsProjectsEnabled(userId);
        }

        public bool Can()
        {
            return Can(CurrentUserId);
        }

        public Guid CurrentUserId { get; private set; }

        public bool CurrentUserAdministrator { get; private set; }

        private bool CurrentUserIsVisitor { get; set; }

        internal bool CurrentUserIsOutsider { get; set; }

        public bool CurrentUserIsProjectsEnabled { get; private set; }

        public bool CurrentUserIsCRMEnabled { get; private set; }

        public bool IsPrivateDisabled { get; private set; }

        public IDaoFactory DaoFactory { get; set; }

        public EngineFactory EngineFactory { get; set; }

        public ProjectSecurityCommon()
        {
            CurrentUserId = SecurityContext.CurrentAccount.ID;
            CurrentUserAdministrator = CoreContext.UserManager.IsUserInGroup(CurrentUserId, Constants.GroupAdmin.ID) ||
                   WebItemSecurity.IsProductAdministrator(WebItemManager.ProjectsProductID, CurrentUserId);
            CurrentUserIsVisitor = CoreContext.UserManager.GetUsers(CurrentUserId).IsVisitor();
            CurrentUserIsOutsider = IsOutsider(CurrentUserId);
            IsPrivateDisabled = TenantAccessSettings.Load().Anyone;
            CurrentUserIsProjectsEnabled = IsModuleEnabled(WebItemManager.ProjectsProductID, CurrentUserId);
            CurrentUserIsCRMEnabled = IsModuleEnabled(WebItemManager.CRMProductID, CurrentUserId);
        }

        public bool IsAdministrator(Guid userId)
        {
            if (userId == CurrentUserId) return CurrentUserAdministrator;

            return CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID) ||
                   WebItemSecurity.IsProductAdministrator(WebItemManager.ProjectsProductID, userId);
        }

        public bool IsProjectsEnabled()
        {
            return IsProjectsEnabled(CurrentUserId);
        }

        public bool IsProjectsEnabled(Guid userID)
        {
            if (userID == CurrentUserId) return CurrentUserIsProjectsEnabled;

            return IsModuleEnabled(WebItemManager.ProjectsProductID, userID);
        }

        public bool IsCrmEnabled()
        {
            return IsCrmEnabled(CurrentUserId);
        }

        public bool IsCrmEnabled(Guid userID)
        {
            if (userID == CurrentUserId) return CurrentUserIsCRMEnabled;

            return IsModuleEnabled(WebItemManager.CRMProductID, userID);
        }

        private bool IsModuleEnabled(Guid module, Guid userId)
        {
            var projects = WebItemManager.Instance[module];

            if (projects != null)
            {
                return !projects.IsDisabled(userId);
            }

            return false;
        }

        public bool IsVisitor(Guid userId)
        {
            if (userId == CurrentUserId) return CurrentUserIsVisitor;

            return CoreContext.UserManager.GetUsers(userId).IsVisitor();
        }

        public bool IsOutsider(Guid userId)
        {
            return CoreContext.UserManager.GetUsers(userId).IsOutsider();
        }

        public bool IsProjectManager(Project project)
        {
            return IsProjectManager(project, CurrentUserId);
        }

        public bool IsProjectManager(Project project, Guid userId)
        {
            return (IsAdministrator(userId) || (project != null && project.Responsible == userId)) &&
                   !CurrentUserIsVisitor;
        }

        public bool IsProjectCreator(Project project)
        {
            return IsProjectCreator(project, CurrentUserId);
        }

        public bool IsProjectCreator(Project project, Guid userId)
        {
            return (project != null && project.CreateBy == userId);
        }

        public bool IsInTeam(Project project)
        {
            return IsInTeam(project, CurrentUserId);
        }

        public bool IsInTeam(Project project, Guid userId, bool includeAdmin = true)
        {
            var isAdmin = includeAdmin && IsAdministrator(userId);
            return isAdmin || (project != null && DaoFactory.ProjectDao.IsInTeam(project.ID, userId));
        }

        public bool IsFollow(Project project, Guid userID)
        {
            var isAdmin = IsAdministrator(userID);
            var isPrivate = project != null && (!project.Private || isAdmin);

            return isPrivate && DaoFactory.ProjectDao.IsFollow(project.ID, userID);
        }

        public bool GetTeamSecurity(Project project, ProjectTeamSecurity security)
        {
            return GetTeamSecurity(project, CurrentUserId, security);
        }

        public bool GetTeamSecurity(Project project, Guid userId, ProjectTeamSecurity security)
        {
            if (IsProjectManager(project, userId) || project == null || !project.Private) return true;
            var dao = DaoFactory.ProjectDao;
            var s = dao.GetTeamSecurity(project.ID, userId);
            return (s & security) != security && dao.IsInTeam(project.ID, userId);
        }

        public bool GetTeamSecurityForParticipants(Project project, Guid userId, ProjectTeamSecurity security)
        {
            if (IsProjectManager(project, userId) || !project.Private) return true;
            var s = DaoFactory.ProjectDao.GetTeamSecurity(project.ID, userId);
            return (s & security) != security;
        }
    }

    public abstract class ProjectSecurityTemplate<T> where T : DomainObject<int>
    {
        public ProjectSecurityCommon Common { get; set; }

        public virtual bool CanCreateEntities(Project project)
        {
            return project != null && project.Status == ProjectStatus.Open && Common.Can();
        }

        public virtual bool CanReadEntities(Project project, Guid userId)
        {
            return Common.IsProjectsEnabled(userId);
        }

        public bool CanReadEntities(Project project)
        {
            return CanReadEntities(project, Common.CurrentUserId);
        }

        public virtual bool CanReadEntity(T entity, Guid userId)
        {
            return entity != null && Common.Can(userId);
        }

        public bool CanReadEntity(T entity)
        {
            return CanReadEntity(entity, Common.CurrentUserId);
        }

        public virtual bool CanUpdateEntity(T entity)
        {
            return Common.Can() && entity != null;
        }

        public virtual bool CanDeleteEntity(T entity)
        {
            return Common.Can() && entity != null;
        }

        public virtual bool CanCreateComment(T entity)
        {
            return false;
        }

        public virtual bool CanEditFiles(T entity)
        {
            return false;
        }

        public virtual bool CanEditComment(T entity, Comment comment)
        {
            return false;
        }

        public virtual bool CanGoToFeed(T entity, Guid userId)
        {
            return false;
        }
    }

    public sealed class ProjectSecurityProject : ProjectSecurityTemplate<Project>
    {
        public override bool CanCreateEntities(Project project)
        {
            return Common.Can() && (Common.CurrentUserAdministrator || ProjectsCommonSettings.Load().EverebodyCanCreate);
        }

        public override bool CanReadEntity(Project project, Guid userId)
        {
            if (!Common.IsProjectsEnabled(userId)) return false;
            if (project == null) return false;
            if (project.Private && Common.IsPrivateDisabled) return false;
            return !project.Private || Common.IsProjectCreator(project, userId) || Common.IsInTeam(project, userId);
        }

        public override bool CanUpdateEntity(Project project)
        {
            return base.CanUpdateEntity(project) && Common.IsProjectManager(project) || Common.IsProjectCreator(project);
        }

        public override bool CanDeleteEntity(Project project)
        {
            return base.CanDeleteEntity(project) && Common.CurrentUserAdministrator || Common.IsProjectCreator(project);
        }

        public override bool CanGoToFeed(Project project, Guid userId)
        {
            if (project == null || !Common.IsProjectsEnabled(userId))
            {
                return false;
            }
            return WebItemSecurity.IsProductAdministrator(EngineFactory.ProductId, userId)
                   || Common.IsInTeam(project, userId, false)
                   || Common.IsFollow(project, userId)
                   || Common.IsProjectCreator(project, userId);
        }

        public bool CanEditTeam(Project project)
        {
            return Common.Can() && (Common.IsProjectManager(project) || Common.IsProjectCreator(project));
        }

        public bool CanReadFiles(Project project)
        {
            return CanReadFiles(project, SecurityContext.CurrentAccount.ID);
        }

        public bool CanReadFiles(Project project, Guid userId)
        {
            return Common.IsProjectsEnabled(userId) && Common.GetTeamSecurity(project, userId, ProjectTeamSecurity.Files);
        }

        public override bool CanEditComment(Project project, Comment comment)
        {
            if (!Common.IsProjectsEnabled()) return false;
            if (project == null || comment == null) return false;
            return comment.CreateBy == Common.CurrentUserId || Common.IsProjectManager(project);
        }

        public bool CanReadContacts(Project project)
        {
            return Common.IsCrmEnabled() && Common.IsProjectsEnabled() &&
                   Common.GetTeamSecurity(project, ProjectTeamSecurity.Contacts);
        }

        public bool CanLinkContact(Project project)
        {
            return Common.IsProjectsEnabled() && CanUpdateEntity(project);
        }
    }

    public sealed class ProjectSecurityTask : ProjectSecurityTemplate<Task>
    {
        public ProjectSecurityProject ProjectSecurityProject { get; set; }

        public ProjectSecurityMilestone ProjectSecurityMilestone { get; set; }

        public override bool CanCreateEntities(Project project)
        {
            if (!base.CanCreateEntities(project)) return false;
            if (Common.IsProjectManager(project)) return true;
            return Common.IsInTeam(project) && CanReadEntities(project);
        }

        public override bool CanReadEntities(Project project, Guid userId)
        {
            return base.CanReadEntities(project, userId) && Common.GetTeamSecurity(project, userId, ProjectTeamSecurity.Tasks);
        }

        public override bool CanReadEntity(Task task, Guid userId)
        {
            if (task == null || !ProjectSecurityProject.CanReadEntity(task.Project, userId)) return false;

            if (task.Responsibles.Contains(userId)) return true;

            if (!CanReadEntities(task.Project, userId)) return false;

            if (task.Milestone != 0 && !ProjectSecurityMilestone.CanReadEntities(task.Project, userId))
            {
                var m = Common.DaoFactory.MilestoneDao.GetById(task.Milestone);
                if (!ProjectSecurityMilestone.CanReadEntity(m, userId)) return false;
            }

            return true;
        }

        public override bool CanUpdateEntity(Task task)
        {
            if (!base.CanUpdateEntity(task)) return false;
            if (task.Project.Status == ProjectStatus.Closed) return false;
            if (Common.IsProjectManager(task.Project)) return true;

            return Common.IsInTeam(task.Project) &&
                   (task.CreateBy == Common.CurrentUserId ||
                    !task.Responsibles.Any() ||
                    task.Responsibles.Contains(Common.CurrentUserId) ||
                    task.SubTasks.Select(r => r.Responsible).Contains(Common.CurrentUserId));
        }

        public override bool CanDeleteEntity(Task task)
        {
            if (!base.CanDeleteEntity(task)) return false;
            if (Common.IsProjectManager(task.Project)) return true;

            return Common.IsInTeam(task.Project) && task.CreateBy == Common.CurrentUserId;
        }

        public override bool CanCreateComment(Task entity)
        {
            return CanReadEntity(entity) &&
                Common.IsProjectsEnabled() &&
                SecurityContext.IsAuthenticated &&
                !Common.CurrentUserIsOutsider;
        }

        public bool CanEdit(Task task, Subtask subtask)
        {
            if (subtask == null || !Common.Can()) return false;
            if (CanUpdateEntity(task)) return true;

            return Common.IsInTeam(task.Project) &&
                   (subtask.CreateBy == Common.CurrentUserId ||
                    subtask.Responsible == Common.CurrentUserId);
        }

        public override bool CanGoToFeed(Task task, Guid userId)
        {
            if (task == null || !Common.IsProjectsEnabled(userId)) return false;
            if (task.CreateBy == userId) return true;
            if (!Common.IsInTeam(task.Project, userId, false) && !Common.IsFollow(task.Project, userId)) return false;
            if (task.Responsibles.Contains(userId)) return true;
            if (task.Milestone != 0 && !ProjectSecurityMilestone.CanReadEntities(task.Project, userId))
            {
                var milestone = Common.DaoFactory.MilestoneDao.GetById(task.Milestone);
                if (milestone.Responsible == userId)
                {
                    return true;
                }
            }
            return Common.GetTeamSecurityForParticipants(task.Project, userId, ProjectTeamSecurity.Tasks);
        }

        public override bool CanEditFiles(Task entity)
        {
            if (!Common.IsProjectsEnabled()) return false;
            if(entity.Project.Status == ProjectStatus.Closed) return false;
            if (Common.IsProjectManager(entity.Project)) return true;

            return CanUpdateEntity(entity);
        }

        public override bool CanEditComment(Task entity, Comment comment)
        {
            if (entity == null) return false;

            return ProjectSecurityProject.CanEditComment(entity.Project, comment);
        }

        public bool CanCreateSubtask(Task task)
        {
            if (task== null || !Common.Can()) return false;
            if (Common.IsProjectManager(task.Project)) return true;

            return Common.IsInTeam(task.Project) &&
                   ((task.CreateBy == Common.CurrentUserId) ||
                    !task.Responsibles.Any() ||
                    task.Responsibles.Contains(Common.CurrentUserId));
        }

        public bool CanCreateTimeSpend(Task task)
        {
            if (task == null || !Common.Can()) return false;
            if (task.Project.Status != ProjectStatus.Open) return false;
            if (Common.IsInTeam(task.Project)) return true;

            return task.Responsibles.Contains(Common.CurrentUserId) ||
                   task.SubTasks.Select(r => r.Responsible).Contains(Common.CurrentUserId);
        }
    }

    public sealed class ProjectSecurityMilestone : ProjectSecurityTemplate<Milestone>
    {
        public ProjectSecurityProject ProjectSecurityProject { get; set; }

        public override bool CanCreateEntities(Project project)
        {
            return base.CanCreateEntities(project) && Common.IsProjectManager(project);
        }

        public override bool CanReadEntities(Project project, Guid userId)
        {
            return base.CanReadEntities(project, userId) && Common.GetTeamSecurity(project, userId, ProjectTeamSecurity.Milestone);
        }

        public override bool CanReadEntity(Milestone entity, Guid userId)
        {
            if (entity == null || !ProjectSecurityProject.CanReadEntity(entity.Project, userId)) return false;
            if (entity.Responsible == userId) return true;

            return CanReadEntities(entity.Project, userId);
        }

        public override bool CanUpdateEntity(Milestone milestone)
        {
            if (!base.CanUpdateEntity(milestone)) return false;
            if (milestone.Project.Status == ProjectStatus.Closed) return false;
            if (Common.IsProjectManager(milestone.Project)) return true;
            if (!CanReadEntity(milestone)) return false;

            return Common.IsInTeam(milestone.Project) &&
                   (milestone.CreateBy == Common.CurrentUserId ||
                    milestone.Responsible == Common.CurrentUserId);
        }

        public override bool CanDeleteEntity(Milestone milestone)
        {
            if (!base.CanDeleteEntity(milestone)) return false;
            if (Common.IsProjectManager(milestone.Project)) return true;

            return Common.IsInTeam(milestone.Project) && milestone.CreateBy == Common.CurrentUserId;
        }

        public override bool CanGoToFeed(Milestone milestone, Guid userId)
        {
            if (milestone == null || !Common.IsProjectsEnabled(userId)) return false;
            if (!Common.IsInTeam(milestone.Project, userId, false) && !Common.IsFollow(milestone.Project, userId)) return false;
            return milestone.Responsible == userId || Common.GetTeamSecurityForParticipants(milestone.Project, userId, ProjectTeamSecurity.Milestone);
        }
    }

    public sealed class ProjectSecurityMessage : ProjectSecurityTemplate<Message>
    {
        public ProjectSecurityProject ProjectSecurityProject { get; set; }

        public override bool CanCreateEntities(Project project)
        {
            if (!base.CanCreateEntities(project)) return false;
            if (Common.IsProjectManager(project)) return true;
            return Common.IsInTeam(project) && CanReadEntities(project);
        }

        public override bool CanReadEntities(Project project, Guid userId)
        {
            return base.CanReadEntities(project, userId) && Common.GetTeamSecurity(project, userId, ProjectTeamSecurity.Messages);
        }

        public override bool CanReadEntity(Message entity, Guid userId)
        {
            if (entity == null || !ProjectSecurityProject.CanReadEntity(entity.Project, userId)) return false;

            return CanReadEntities(entity.Project, userId);
        }

        public override bool CanUpdateEntity(Message message)
        {
            if (!base.CanUpdateEntity(message)) return false;
            if (Common.IsProjectManager(message.Project)) return true;
            if (!CanReadEntity(message)) return false;

            return Common.IsInTeam(message.Project) && message.CreateBy == Common.CurrentUserId;
        }

        public override bool CanDeleteEntity(Message message)
        {
            return CanUpdateEntity(message);
        }

        public override bool CanCreateComment(Message message)
        {
            return CanReadEntity(message) &&
                (message == null || message.Status == MessageStatus.Open) &&
                Common.IsProjectsEnabled() &&
                SecurityContext.IsAuthenticated &&
                !Common.CurrentUserIsOutsider;
        }

        public override bool CanGoToFeed(Message message, Guid userId)
        {
            if (message == null || !Common.IsProjectsEnabled(userId)) return false;
            if (message.CreateBy == userId) return true;
            if (!Common.IsInTeam(message.Project, userId, false) && !Common.IsFollow(message.Project, userId)) return false;

            var isSubscriber = Common.EngineFactory.MessageEngine.GetSubscribers(message).Any(r => new Guid(r.ID).Equals(userId));
            return isSubscriber && Common.GetTeamSecurityForParticipants(message.Project, userId, ProjectTeamSecurity.Messages);
        }

        public override bool CanEditComment(Message message, Comment comment)
        {
            return message.Status == MessageStatus.Open && ProjectSecurityProject.CanEditComment(message.Project, comment);
        }

        public override bool CanEditFiles(Message entity)
        {
            if (!Common.IsProjectsEnabled()) return false;
            if (entity.Status == MessageStatus.Archived || entity.Project.Status == ProjectStatus.Closed) return false;
            if (Common.IsProjectManager(entity.Project)) return true;

            return Common.IsInTeam(entity.Project);
        }
    }

    public sealed class ProjectSecurityTimeTracking : ProjectSecurityTemplate<TimeSpend>
    {
        public ProjectSecurityTask ProjectSecurityTask { get; set; }

        public override bool CanCreateEntities(Project project)
        {
            if (!base.CanCreateEntities(project)) return false;
            if (project.Status == ProjectStatus.Closed) return false;
            return Common.IsInTeam(project);
        }

        public override bool CanReadEntities(Project project, Guid userId)
        {
            return ProjectSecurityTask.CanReadEntities(project, userId);
        }

        public override bool CanReadEntity(TimeSpend entity, Guid userId)
        {
            return ProjectSecurityTask.CanReadEntity(entity.Task, userId);
        }

        public override bool CanUpdateEntity(TimeSpend timeSpend)
        {
            if (!base.CanUpdateEntity(timeSpend)) return false;
            if (Common.IsProjectManager(timeSpend.Task.Project)) return true;
            if (timeSpend.PaymentStatus == PaymentStatus.Billed) return false;

            return timeSpend.Person == Common.CurrentUserId || timeSpend.CreateBy == Common.CurrentUserId;
        }

        public override bool CanDeleteEntity(TimeSpend timeSpend)
        {
            if (!base.CanDeleteEntity(timeSpend)) return false;
            if (Common.IsProjectManager(timeSpend.Task.Project)) return true;
            if (timeSpend.PaymentStatus == PaymentStatus.Billed) return false;

            return Common.IsInTeam(timeSpend.Task.Project) &&
                   (timeSpend.CreateBy == Common.CurrentUserId || timeSpend.Person == Common.CurrentUserId);
        }

        public bool CanEditPaymentStatus(TimeSpend timeSpend)
        {
            return timeSpend != null && Common.Can() && Common.IsProjectManager(timeSpend.Task.Project);
        }
    }

    public class ProjectSecurity
    {
        public ILifetimeScope Scope { get; set; }

        public bool CanCreate<T>(Project project) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanCreateEntities(project);
        }

        public bool CanEdit<T>(T entity) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanUpdateEntity(entity);
        }

        public bool CanRead<T>(T entity) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanReadEntity(entity);
        }

        public bool CanRead<T>(T entity, Guid userId) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanReadEntity(entity, userId);
        }

        public bool CanRead<T>(Project project) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanReadEntities(project);
        }

        public bool CanDelete<T>(T entity) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanDeleteEntity(entity);
        }

        public bool CanEditComment(ProjectEntity entity, Comment comment)
        {
            var task = entity as Task;
            if (task != null)
            {
                return Scope.Resolve<ProjectSecurityTask>().CanEditComment(task, comment);
            }

            var message = entity as Message;
            return Scope.Resolve<ProjectSecurityMessage>().CanEditComment(message, comment);
        }

        public bool CanEditComment(Project entity, Comment comment)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanEditComment(entity, comment);
        }

        public bool CanCreateComment(ProjectEntity entity)
        {
            var task = entity as Task;
            if (task != null)
            {
                return Scope.Resolve<ProjectSecurityTask>().CanCreateComment(task);
            }

            var message = entity as Message;
            return Scope.Resolve<ProjectSecurityMessage>().CanCreateComment(message);
        }

        public bool CanCreateComment(Project project)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanCreateComment(project);
        }

        public bool CanEdit(Task task, Subtask subtask)
        {
            return Scope.Resolve<ProjectSecurityTask>().CanEdit(task, subtask);
        }

        public bool CanReadFiles(Project project, Guid userId)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanReadFiles(project, userId);
        }

        public bool CanReadFiles(Project project)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanReadFiles(project);
        }

        public bool CanEditFiles<T>(T entity) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanEditFiles(entity);
        }

        public bool CanEditTeam(Project project)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanEditTeam(project);
        }

        public bool CanLinkContact(Project project)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanLinkContact(project);
        }

        public bool CanReadContacts(Project project)
        {
            return Scope.Resolve<ProjectSecurityProject>().CanReadContacts(project);
        }

        public bool CanEditPaymentStatus(TimeSpend timeSpend)
        {
            return Scope.Resolve<ProjectSecurityTimeTracking>().CanEditPaymentStatus(timeSpend);
        }

        public bool CanCreateSubtask(Task task)
        {
            return Scope.Resolve<ProjectSecurityTask>().CanCreateSubtask(task);
        }

        public bool CanCreateTimeSpend(Task task)
        {
            return Scope.Resolve<ProjectSecurityTask>().CanCreateTimeSpend(task);
        }

        public bool CanEditTemplate(Template template)
        {
            return CurrentUserAdministrator || template.CreateBy.Equals(SecurityContext.CurrentAccount.ID);
        }

        public bool CanGoToFeed<T>(T entity, Guid userId) where T : DomainObject<int>
        {
            return Scope.Resolve<ProjectSecurityTemplate<T>>().CanGoToFeed(entity, userId);
        }

        public bool CanGoToFeed(ParticipantFull participant, Guid userId)
        {
            var common = Scope.Resolve<ProjectSecurityCommon>();
            if (participant == null || !IsProjectsEnabled(userId)) return false;
            return common.IsInTeam(participant.Project, userId, false) || common.IsFollow(participant.Project, userId);
        }

        public bool IsInTeam(Project project, Guid userId, bool includeAdmin = true)
        {
            return Scope.Resolve<ProjectSecurityCommon>().IsInTeam(project, userId, includeAdmin);
        }

        public void DemandCreate<T>(Project project) where T : DomainObject<int>
        {
            if (!CanCreate<T>(project)) throw CreateSecurityException();
        }

        public void DemandEdit<T>(T entity) where T : DomainObject<int>
        {
            if (!CanEdit(entity)) throw CreateSecurityException();
        }

        public void DemandEdit(Task task, Subtask subtask)
        {
            if (!CanEdit(task, subtask)) throw CreateSecurityException();
        }

        public void DemandDelete<T>(T entity) where T : DomainObject<int>
        {
            if (!CanDelete(entity)) throw CreateSecurityException();
        }

        public void DemandEditComment(ProjectEntity entity, Comment comment)
        {
            if (!CanEditComment(entity, comment)) throw CreateSecurityException();
        }

        public void DemandEditComment(Project entity, Comment comment)
        {
            if (!CanEditComment(entity, comment)) throw CreateSecurityException();
        }

        public void DemandCreateComment(Project project)
        {
            if (!CanCreateComment(project)) throw CreateSecurityException();
        }

        public void DemandCreateComment(ProjectEntity entity)
        {
            if (!CanCreateComment(entity)) throw CreateSecurityException();
        }

        public void DemandLinkContact(Project project)
        {
            if (!Scope.Resolve<ProjectSecurityProject>().CanLinkContact(project))
            {
                throw CreateSecurityException();
            }
        }

        public void DemandEditTeam(Project project)
        {
            if (!CanEditTeam(project)) throw CreateSecurityException();
        }

        public void DemandReadFiles(Project project)
        {
            if (!CanReadFiles(project)) throw CreateSecurityException();
        }

        public static void DemandAuthentication()
        {
            if (!SecurityContext.CurrentAccount.IsAuthenticated)
            {
                throw CreateSecurityException();
            }
        }

        public bool CurrentUserAdministrator
        {
            get
            {
                return Scope.Resolve<ProjectSecurityCommon>().CurrentUserAdministrator;
            }
        }

        public bool IsPrivateDisabled
        {
            get
            {
                return Scope.Resolve<ProjectSecurityCommon>().IsPrivateDisabled;
            }
        }

        public bool IsVisitor()
        {
            return IsVisitor(SecurityContext.CurrentAccount.ID);
        }

        public bool IsVisitor(Guid userId)
        {
            return Scope.Resolve<ProjectSecurityCommon>().IsVisitor(userId);
        }

        public bool IsAdministrator()
        {
            return IsAdministrator(SecurityContext.CurrentAccount.ID);
        }

        public bool IsAdministrator(Guid userId)
        {
            return Scope.Resolve<ProjectSecurityCommon>().IsAdministrator(userId);
        }

        public bool IsProjectsEnabled()
        {
            return IsProjectsEnabled(SecurityContext.CurrentAccount.ID);
        }

        public bool IsProjectsEnabled(Guid userId)
        {
            return Scope.Resolve<ProjectSecurityCommon>().IsProjectsEnabled(userId);
        }


        public void GetProjectSecurityInfo(Project project)
        {
            var projectSecurity = Scope.Resolve<ProjectSecurityProject>();
            var milestoneSecurity = Scope.Resolve<ProjectSecurityMilestone>();
            var messageSecurity = Scope.Resolve<ProjectSecurityMessage>();
            var taskSecurity = Scope.Resolve<ProjectSecurityTask>();
            var timeSpendSecurity = Scope.Resolve<ProjectSecurityTimeTracking>();

            project.Security = GetProjectSecurityInfoWithSecurity(project, projectSecurity, milestoneSecurity, messageSecurity, taskSecurity, timeSpendSecurity);
        }

        public void GetProjectSecurityInfo(IEnumerable<Project> projects)
        {
            var projectSecurity = Scope.Resolve<ProjectSecurityProject>();
            var milestoneSecurity = Scope.Resolve<ProjectSecurityMilestone>();
            var messageSecurity = Scope.Resolve<ProjectSecurityMessage>();
            var taskSecurity = Scope.Resolve<ProjectSecurityTask>();
            var timeSpendSecurity = Scope.Resolve<ProjectSecurityTimeTracking>();

            foreach (var project in projects)
            {
                project.Security = GetProjectSecurityInfoWithSecurity(project, projectSecurity, milestoneSecurity, messageSecurity, taskSecurity, timeSpendSecurity);
            }
        }

        public void GetTaskSecurityInfo(Task task)
        {
            var projectSecurity = Scope.Resolve<ProjectSecurityProject>();
            var taskSecurity = Scope.Resolve<ProjectSecurityTask>();

            task.Security = GetTaskSecurityInfoWithSecurity(task, projectSecurity, taskSecurity);
        }

        public void GetTaskSecurityInfo(IEnumerable<Task> tasks)
        {
            var projectSecurity = Scope.Resolve<ProjectSecurityProject>();
            var taskSecurity = Scope.Resolve<ProjectSecurityTask>();

            foreach (var task in tasks)
            {
                task.Security = GetTaskSecurityInfoWithSecurity(task, projectSecurity, taskSecurity);
            }
        }

        private ProjectSecurityInfo GetProjectSecurityInfoWithSecurity(Project project, ProjectSecurityProject projectSecurity, ProjectSecurityMilestone milestoneSecurity, ProjectSecurityMessage messageSecurity, ProjectSecurityTask taskSecurity, ProjectSecurityTimeTracking timeSpendSecurity)
        {
            return new ProjectSecurityInfo
            {
                CanCreateMilestone = milestoneSecurity.CanCreateEntities(project),
                CanCreateMessage = messageSecurity.CanCreateEntities(project),
                CanCreateTask = taskSecurity.CanCreateEntities(project),
                CanCreateTimeSpend = timeSpendSecurity.CanCreateEntities(project),

                CanEditTeam = projectSecurity.CanEditTeam(project),
                CanReadFiles = projectSecurity.CanReadFiles(project),
                CanReadMilestones = milestoneSecurity.CanReadEntities(project),
                CanReadMessages = messageSecurity.CanReadEntities(project),
                CanReadTasks = taskSecurity.CanReadEntities(project),
                IsInTeam = milestoneSecurity.Common.IsInTeam(project, SecurityContext.CurrentAccount.ID, false),
                CanLinkContact = projectSecurity.CanLinkContact(project),
                CanReadContacts = projectSecurity.CanReadContacts(project),

                CanEdit = projectSecurity.CanUpdateEntity(project),
                CanDelete = projectSecurity.CanDeleteEntity(project),
            };
        }

        private TaskSecurityInfo GetTaskSecurityInfoWithSecurity(Task task, ProjectSecurityProject projectSecurity, ProjectSecurityTask taskSecurity)
        {
            return new TaskSecurityInfo
            {
                CanEdit = taskSecurity.CanUpdateEntity(task),
                CanCreateSubtask = taskSecurity.CanCreateSubtask(task),
                CanCreateTimeSpend = taskSecurity.CanCreateTimeSpend(task),
                CanDelete = taskSecurity.CanDeleteEntity(task),
                CanReadFiles = projectSecurity.CanReadFiles(task.Project)
            };
        }

        public static Exception CreateSecurityException()
        {
            throw new System.Security.SecurityException("Access denied.");
        }

        public static Exception CreateGuestSecurityException()
        {
            throw new System.Security.SecurityException("A guest cannot be appointed as responsible.");
        }


    }
}