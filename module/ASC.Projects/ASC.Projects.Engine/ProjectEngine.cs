/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        public ProjectEngine(IDaoFactory daoFactory, EngineFactory factory)
        {
            this.factory = factory;
            projectDao = daoFactory.GetProjectDao();
            participantDao = daoFactory.GetParticipantDao();
        }

        #region Get Projects

        public virtual List<Project> GetAll()
        {
            return projectDao.GetAll(null, 0)
                .Where(CanRead)
                .ToList();
        }

        public virtual List<Project> GetAll(ProjectStatus status, int max)
        {
            return projectDao.GetAll(status, max)
                .Where(CanRead)
                .ToList();
        }

        public virtual List<Project> GetLast(ProjectStatus status, int max)
        {
            var offset = 0;
            var lastProjects = new List<Project>();
            var projects = projectDao.GetLast(status, offset, max)
                .Where(CanRead)
                .ToList();

            lastProjects.AddRange(projects);

            while (lastProjects.Count < max)
            {
                offset = offset + max;
                projects = projectDao.GetLast(status, offset, max);

                if (projects.Count == 0)
                    return lastProjects;
                projects = projects
                    .Where(CanRead)
                    .ToList();

                lastProjects.AddRange(projects);
            }

            return lastProjects.Count == max ? lastProjects : lastProjects.GetRange(0, max);
        }

        public virtual List<Project> GetOpenProjectsWithTasks(Guid participantId)
        {
            return projectDao.GetOpenProjectsWithTasks(participantId)
                .Where(CanRead)
                .ToList();
        }

        public virtual List<Project> GetByParticipant(Guid participant)
        {
            return projectDao.GetByParticipiant(participant, ProjectStatus.Open)
                .Where(CanRead)
                .ToList();
        }

        public virtual List<Project> GetByFilter(TaskFilter filter)
        {
            return projectDao.GetByFilter(filter, ProjectSecurity.CurrentUserAdministrator);
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return projectDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator);
        }

        public virtual List<Project> GetFollowing(Guid participant)
        {
            return projectDao.GetFollowing(participant)
                .Where(CanRead)
                .ToList();
        }

        public virtual Project GetByID(int projectID)
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
            project.MilestoneCount = factory.GetMilestoneEngine().GetByFilterCount(filter);
            project.TaskCount = factory.GetTaskEngine().GetByFilterCount(filter);
            project.DiscussionCount = factory.GetMessageEngine().GetByFilterCount(filter);

            using (var folderDao = FilesIntegration.GetFolderDao())
            {
                var folderId = factory.GetFileEngine().GetRoot(projectID);
                project.DocumentsCount = folderDao.GetItemsCount(folderId, true);
            }

            var time = factory.GetTimeTrackingEngine().GetByProject(projectID).Sum(r => r.Hours);
            var hours = (int)time;
            var minutes = (int)(Math.Round((time - hours) * 60));
            var result = hours + ":" + minutes.ToString("D2");
            
            project.TimeTrackingTotal = !result.Equals("0:00", StringComparison.InvariantCulture) ? result : "";
            project.ParticipantCount = GetTeam(projectID).Count;


            return project;
        }

        public virtual List<Project> GetByID(ICollection projectIDs)
        {
            return projectDao.GetById(projectIDs)
                .Where(CanRead)
                .ToList();
        }

        public virtual bool IsExists(int projectID)
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

        public virtual int Count()
        {
            return projectDao.Count();
        }

        public virtual int GetTaskCount(int projectId, TaskStatus? taskStatus)
        {
            return GetTaskCount(new List<int> { projectId }, taskStatus)[0];
        }

        public virtual List<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus)
        {
            return projectDao.GetTaskCount(projectId, taskStatus, ProjectSecurity.CurrentUserAdministrator);
        }

        public virtual int GetMilestoneCount(int projectId, params MilestoneStatus[] milestoneStatus)
        {
            return projectDao.GetMilestoneCount(projectId, milestoneStatus);
        }

        public virtual int GetMessageCount(int projectId)
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

            factory.GetFileEngine().RemoveRoot(projectId);

            projectDao.Delete(projectId);

            NotifyClient.Instance.SendAboutProjectDeleting(new HashSet<Guid> { project.Responsible }, project);
        }

        #endregion

        #region Contacts

        public List<Project> GetProjectsByContactID(int contactId)
        {
            return projectDao.GetByContactID(contactId).Where(CanRead).ToList();
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

        public virtual List<Participant> GetTeam(int project)
        {
            return projectDao.GetTeam(project).Where(r => !r.UserInfo.Equals(ASC.Core.Users.Constants.LostUser)).ToList();
        }

        public virtual List<Participant> GetTeam(List<int> projects)
        {
            return projectDao.GetTeam(projects).Where(r => !r.UserInfo.Equals(ASC.Core.Users.Constants.LostUser)).ToList();
        }

        public virtual bool IsInTeam(int project, Guid participant)
        {
            return projectDao.IsInTeam(project, participant);
        }

        public virtual bool IsFollow(int project, Guid participant)
        {
            return projectDao.IsFollow(project, participant);
        }

        public virtual void AddToTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.AddToTeam(project.ID, participant.ID);

            if (!factory.DisableNotifications && sendNotification && !project.Responsible.Equals(participant.ID) && participant.ID != SecurityContext.CurrentAccount.ID)
                NotifyClient.Instance.SendInvaiteToProjectTeam(participant.ID, project);
        }

        public virtual void RemoveFromTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.RemoveFromTeam(project.ID, participant.ID);

            if (!factory.DisableNotifications && sendNotification)
                NotifyClient.Instance.SendRemovingFromProjectTeam(participant.ID, project);
        }

        public virtual void UpdateTeam(Project project, IEnumerable<Guid> participants, bool notify)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participants == null) throw new ArgumentNullException("participants");

            ProjectSecurity.DemandEditTeam(project);

            var newTeam = participants.Select(p => new Participant(p)).ToList();
            var oldTeam = GetTeam(project.ID);

            var removeFromTeam = oldTeam.Where(p => !newTeam.Contains(p)).Where(p => p.ID != project.Responsible).ToList();
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

        public virtual void SetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity, bool visible)
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

        public virtual void ResetTeamSecurity(Project project)
        {
            if (project == null) throw new ArgumentNullException("project");

            ProjectSecurity.DemandEditTeam(project);

            var participant = GetTeam(project.ID);

            foreach (var part in participant)
            {
                projectDao.SetTeamSecurity(project.ID, part.ID, ProjectTeamSecurity.None);
            }

        }

        public virtual bool GetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            var security = projectDao.GetTeamSecurity(project.ID, participant.ID);
            return (security & teamSecurity) != teamSecurity;
        }

        public virtual List<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to)
        {
            return projectDao.GetTeamUpdates(from, to).Where(x => CanRead(x.Project)).ToList();
        }

        public DateTime GetTeamMaxLastModified()
        {
            return projectDao.GetTeamMaxLastModified();
        }

        #endregion
    }
}
