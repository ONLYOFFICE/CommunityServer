/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Files.Api;

namespace ASC.Projects.Engine
{
    public class ProjectEngine
    {
        private readonly EngineFactory factory;
        private readonly IProjectDao projectDao;
        private readonly IParticipantDao participantDao;
        private readonly Func<Project, bool> canReadDelegate;

        public ProjectEngine(IDaoFactory daoFactory, EngineFactory factory)
        {
            this.factory = factory;
            projectDao = daoFactory.GetProjectDao();
            participantDao = daoFactory.GetParticipantDao();
            canReadDelegate = CanRead;
        }

        #region Get Projects

        public IEnumerable<Project> GetAll()
        {
            return projectDao.GetAll(null, 0).Where(canReadDelegate);
        }

        public IEnumerable<Project> GetAll(ProjectStatus status, int max)
        {
            return projectDao.GetAll(status, max)
                .Where(canReadDelegate);
        }

        public IEnumerable<Project> GetLast(ProjectStatus status, int max)
        {
            var offset = 0;
            var lastProjects = new List<Project>();

            do
            {
                var projects = projectDao.GetLast(status, offset, max);

                if (!projects.Any())
                    return lastProjects;

                lastProjects.AddRange(projects.Where(canReadDelegate));
                offset = offset + max;
            } while (lastProjects.Count < max);

            return lastProjects.Count == max ? lastProjects : lastProjects.GetRange(0, max);
        }

        public IEnumerable<Project> GetOpenProjectsWithTasks(Guid participantId)
        {
            return projectDao.GetOpenProjectsWithTasks(participantId).Where(canReadDelegate);
        }

        public IEnumerable<Project> GetByParticipant(Guid participant)
        {
            return projectDao.GetByParticipiant(participant, ProjectStatus.Open).Where(canReadDelegate);
        }

        public IEnumerable<Project> GetByFilter(TaskFilter filter)
        {
            return projectDao.GetByFilter(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return projectDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public IEnumerable<Project> GetFollowing(Guid participant)
        {
            return projectDao.GetFollowing(participant).Where(canReadDelegate);
        }

        public Project GetByID(int projectID)
        {
            return GetByID(projectID, true);
        }

        public Project GetByID(int projectID, bool checkSecurity)
        {
            var project = projectDao.GetById(projectID);

            if (!checkSecurity)
                return project;

            return CanRead(project) ? project : null;
        }

        public Project GetFullProjectByID(int projectID)
        {
            var project = projectDao.GetById(projectID);

            if (!CanRead(project)) return null;

            var filter = new TaskFilter
            {
                ProjectIds = new List<int> { projectID },
                MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open },
                TaskStatuses = new List<TaskStatus> { TaskStatus.Open }
            };
            project.MilestoneCount = factory.MilestoneEngine.GetByFilterCount(filter);
            project.TaskCount = factory.TaskEngine.GetByFilterCount(filter);
            project.DiscussionCount = factory.MessageEngine.GetByFilterCount(filter);

            using (var folderDao = FilesIntegration.GetFolderDao())
            {
                var folderId = factory.FileEngine.GetRoot(projectID);
                project.DocumentsCount = folderDao.GetItemsCount(folderId);
            }

            var time = factory.TimeTrackingEngine.GetByProject(projectID).Sum(r => r.Hours);
            var hours = (int)time;
            var minutes = (int)(Math.Round((time - hours) * 60));
            var result = hours + ":" + minutes.ToString("D2");
            
            project.TimeTrackingTotal = !result.Equals("0:00", StringComparison.InvariantCulture) ? result : "";
            project.ParticipantCount = GetTeam(projectID).Count();


            return project;
        }

        public IEnumerable<Project> GetByID(ICollection projectIDs)
        {
            return projectDao.GetById(projectIDs).Where(canReadDelegate);
        }

        public bool IsExists(int projectID)
        {
            return projectDao.IsExists(projectID);
        }

        private static bool CanRead(Project project)
        {
            return ProjectSecurity.CanRead(project);
        }

        public DateTime GetMaxLastModified()
        {
            return projectDao.GetMaxLastModified();
        }

        #endregion

        #region Order

        public void SetTaskOrder(Project project, string order)
        {
            projectDao.SetTaskOrder(project.ID, order);
        }

        public string GetTaskOrder(Project project)
        {
            return projectDao.GetTaskOrder(project.ID);
        }

        #endregion

        #region Get Counts

        public virtual int CountOpen()
        {
            return Count(ProjectStatus.Open);
        }

        public int Count(ProjectStatus? status = null)
        {
            var filter = new TaskFilter();

            if (status.HasValue)
            {
                filter.ProjectStatuses = new List<ProjectStatus> {ProjectStatus.Open};
            }

            return GetByFilterCount(filter);
        }

        public int GetTaskCount(int projectId, TaskStatus? taskStatus)
        {
            return GetTaskCount(new List<int> { projectId }, taskStatus).First();
        }

        public IEnumerable<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus)
        {
            return projectDao.GetTaskCount(projectId, taskStatus, ProjectSecurity.CurrentUserAdministrator);
        }

        public int GetMilestoneCount(int projectId, params MilestoneStatus[] milestoneStatus)
        {
            return projectDao.GetMilestoneCount(projectId, milestoneStatus);
        }

        public int GetMessageCount(int projectId)
        {
            return projectDao.GetMessageCount(projectId);
        }

        #endregion

        #region Save, Delete

        public Project SaveOrUpdate(Project project, bool notifyManager)
        {
            return SaveOrUpdate(project, notifyManager, false);
        }

        public virtual Project SaveOrUpdate(Project project, bool notifyManager, bool isImport)
        {
            if (project == null) throw new ArgumentNullException("project");

            // check guest responsible
            if (ProjectSecurity.IsVisitor(project.Responsible))
            {
                ProjectSecurity.CreateGuestSecurityException();
            }

            project.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            project.LastModifiedOn = TenantUtil.DateTimeNow();

            if (project.ID == 0)
            {
                if (project.CreateBy == default(Guid)) project.CreateBy = SecurityContext.CurrentAccount.ID;
                if (project.CreateOn == default(DateTime)) project.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateProject();

                projectDao.Save(project);
            }
            else
            {
                var oldProject = projectDao.GetById(new[]{project.ID}).FirstOrDefault();
                ProjectSecurity.DemandEdit(oldProject);

                projectDao.Save(project);

                if (!project.Private) ResetTeamSecurity(oldProject);
            }

            if (notifyManager && !factory.DisableNotifications && !project.Responsible.Equals(SecurityContext.CurrentAccount.ID))
                NotifyClient.Instance.SendAboutResponsibleByProject(project.Responsible, project);

            return project;
        }

        public Project ChangeStatus(Project project, ProjectStatus status)
        {
            if (project == null) throw new ArgumentNullException("project");
            ProjectSecurity.DemandEdit(project);

            project.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            project.LastModifiedOn = TenantUtil.DateTimeNow();
            project.StatusChangedOn = DateTime.Now;
            project.Status = status;

            projectDao.Save(project);

            return project;
        }

        public virtual void Delete(int projectId)
        {
            var project = GetByID(projectId);
            if (project == null) return;

            ProjectSecurity.DemandEdit(project);

            factory.FileEngine.RemoveRoot(projectId);

            projectDao.Delete(projectId);

            NotifyClient.Instance.SendAboutProjectDeleting(new HashSet<Guid> { project.Responsible }, project);
        }

        #endregion

        #region Contacts

        public IEnumerable<Project> GetProjectsByContactID(int contactId)
        {
            return projectDao.GetByContactID(contactId).Where(canReadDelegate);
        }

        public void AddProjectContact(int projectId, int contactId)
        {
            projectDao.AddProjectContact(projectId, contactId);
        }

        public void DeleteProjectContact(int projectId, int contactId)
        {
            projectDao.DeleteProjectContact(projectId, contactId);
        }

        #endregion

        #region Team

        public IEnumerable<Participant> GetTeam(int projectId)
        {
            var project = GetByID(projectId);
            return projectDao.GetTeam(project).Where(r => !r.UserInfo.Equals(ASC.Core.Users.Constants.LostUser));
        }

        public IEnumerable<Participant> GetTeam(List<int> projectIds)
        {
            var projects = GetByID(projectIds);
            return projectDao.GetTeam(projects).Where(r => !r.UserInfo.Equals(ASC.Core.Users.Constants.LostUser));
        }

        public bool IsInTeam(int project, Guid participant)
        {
            return projectDao.IsInTeam(project, participant);
        }

        public bool IsFollow(int project, Guid participant)
        {
            return projectDao.IsFollow(project, participant);
        }

        public void AddToTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.AddToTeam(project.ID, participant.ID);

            if (!factory.DisableNotifications && sendNotification && !project.Responsible.Equals(participant.ID) && participant.ID != SecurityContext.CurrentAccount.ID)
                NotifyClient.Instance.SendInvaiteToProjectTeam(participant.ID, project);
        }

        public void RemoveFromTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.RemoveFromTeam(project.ID, participant.ID);

            if (!factory.DisableNotifications && sendNotification)
                NotifyClient.Instance.SendRemovingFromProjectTeam(participant.ID, project);
        }

        public void UpdateTeam(Project project, IEnumerable<Guid> participants, bool notify)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participants == null) throw new ArgumentNullException("participants");

            ProjectSecurity.DemandEditTeam(project);

            var newTeam = participants.Select(p => new Participant(p));
            var oldTeam = GetTeam(project.ID);

            var removeFromTeam = oldTeam.Where(p => !newTeam.Contains(p)).Where(p => p.ID != project.Responsible);
            var inviteToTeam = new List<Participant>();

            foreach (var participant in newTeam.Where(participant => !oldTeam.Contains(participant)))
            {
                participantDao.RemoveFromFollowingProjects(project.ID, participant.ID);
                inviteToTeam.Add(participant);
            }

            foreach (var participant in inviteToTeam)
            {
                AddToTeam(project, participant, notify);
            }

            foreach (var participant in removeFromTeam)
            {
                RemoveFromTeam(project, participant, notify);
            }
        }

        public void SetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity, bool visible)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);

            var security = projectDao.GetTeamSecurity(project.ID, participant.ID);
            if (visible)
            {
                if (security != ProjectTeamSecurity.None) security ^= teamSecurity;
            }
            else
            {
                security |= teamSecurity;
            }
            projectDao.SetTeamSecurity(project.ID, participant.ID, security);
        }

        public void ResetTeamSecurity(Project project)
        {
            if (project == null) throw new ArgumentNullException("project");

            ProjectSecurity.DemandEditTeam(project);

            var participant = GetTeam(project.ID);

            foreach (var part in participant)
            {
                projectDao.SetTeamSecurity(project.ID, part.ID, ProjectTeamSecurity.None);
            }

        }

        public bool GetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            var security = projectDao.GetTeamSecurity(project.ID, participant.ID);
            return (security & teamSecurity) != teamSecurity;
        }

        public IEnumerable<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to)
        {
            return projectDao.GetTeamUpdates(from, to).Where(x => CanRead(x.Project));
        }

        public DateTime GetTeamMaxLastModified()
        {
            return projectDao.GetTeamMaxLastModified();
        }

        #endregion
    }
}
