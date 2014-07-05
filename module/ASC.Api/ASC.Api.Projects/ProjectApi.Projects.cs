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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;
using Comment = ASC.Projects.Core.Domain.Comment;
using Task = ASC.Projects.Core.Domain.Task;
using LocalizedEnumConverter = ASC.Projects.Core.Domain.LocalizedEnumConverter;
using ASC.Core.Users;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region projects

        #region Read

        ///<summary>
        ///Returns the list of all the portal projects with base information about them
        ///</summary>
        ///<short>
        ///Projects
        ///</short>
        /// <category>Projects</category>
        ///<returns>List of projects</returns>
        [Read("")]
        public IEnumerable<ProjectWrapper> GetAllProjects()
        {
            return EngineFactory.GetProjectEngine().GetAll().Select(x => new ProjectWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all projects in which the current user participates
        ///</summary>
        ///<short>
        ///Participated projects
        ///</short>
        /// <category>Projects</category>
        ///<returns>List of projects</returns>
        [Read(@"@self")]
        public IEnumerable<ProjectWrapper> GetMyProjects()
        {
            return EngineFactory
                .GetProjectEngine()
                .GetByParticipant(CurrentUserId)
                .Select(x => new ProjectWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all projects which the current user follows
        ///</summary>
        ///<short>
        ///Followed projects
        ///</short>
        /// <category>Projects</category>
        ///<returns>List of projects</returns>
        [Read(@"@follow")]
        public IEnumerable<ProjectWrapper> GetFollowProjects()
        {
            return EngineFactory
                .GetProjectEngine()
                .GetFollowing(CurrentUserId)
                .Select(x => new ProjectWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all projects with the status specified in the request
        ///</summary>
        ///<short>
        ///Project by status
        ///</short>
        /// <category>Projects</category>
        ///<param name="status">"open" or "closed"</param>
        ///<returns>List of projects</returns>
        [Read("{status:(open|closed)}")]
        public IEnumerable<ProjectWrapper> GetProjects(ProjectStatus status)
        {
            return EngineFactory.GetProjectEngine().GetAll(status, 0).Select(x => new ProjectWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information about the project with ID specified in the request
        ///</summary>
        ///<short>
        ///Project by ID
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>Project</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}")]
        public ProjectWrapperFull GetProject(int id)
        {
            return new ProjectWrapperFull(EngineFactory.GetProjectEngine().GetFullProjectByID(id).NotFoundIfNull(), EngineFactory.GetFileEngine().GetRoot(id));
        }

        ///<summary>
        ///Returns the list of all the portal projects filtered using project title, status or participant ID and 'Followed' status specified in the request
        ///</summary>
        ///<short>
        ///Projects
        ///</short>
        /// <category>Projects</category>
        ///<param name="tag" optional="true">Project tag</param>
        ///<param name="status" optional="true">Project status</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="manager" optional="true">Project manager GUID</param>
        ///<param name="follow" optional="true">My followed project</param>
        ///<param name="lastId">Last project ID</param>
        ///<returns>Projects list</returns>
        [Read(@"filter")]
        public IEnumerable<ProjectWrapperFull> GetProjectsByFilter(int tag, ProjectStatus? status, Guid participant, Guid manager, bool follow, int lastId)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var filter = new TaskFilter
                {
                    ParticipantId = participant,
                    UserId = manager,
                    SortBy = _context.SortBy,
                    SortOrder = !_context.SortDescending,
                    SearchText = _context.FilterValue,
                    TagId = tag,
                    Follow = follow,
                    LastId = lastId,
                    Offset = _context.StartIndex,
                    Max = _context.Count
                };

            if (status != null)
                filter.ProjectStatuses.Add((ProjectStatus)status);

            _context.SetDataFiltered();
            _context.SetDataPaginated();
            _context.SetDataSorted();
            _context.TotalCount = projectEngine.GetByFilterCount(filter);

            var projects = projectEngine.GetByFilter(filter).NotFoundIfNull();
            var projectIds = projects.Select(p => p.ID).ToList();
            var projectRoots = EngineFactory.GetFileEngine().GetRoots(projectIds).ToList();

            return projects.Select((t, i) => new ProjectWrapperFull(t, projectRoots[i])).ToSmartList();
        }

        ///<summary>
        ///Returns the search results for the project containing the words/phrases matching the query specified in the request
        ///</summary>
        ///<short>
        ///Search project
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="query">Search query</param>
        ///<returns>List of results</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}/@search/{query}")]
        public IEnumerable<SearchWrapper> SearchProject(int id, string query)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(id)) throw new ItemNotFoundException();
            return EngineFactory.GetSearchEngine().Search(query, id).Select(x => new SearchWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all projects matching the query specified in the request
        ///</summary>
        ///<short>
        ///Search all projects
        ///</short>
        /// <category>Projects</category>
        ///<param name="query">Search query</param>
        ///<returns>List of results</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"@search/{query}")]
        public IEnumerable<SearchWrapper> SearchProjects(string query)
        {
            return EngineFactory.GetSearchEngine().Search(query, 0).Select(x => new SearchWrapper(x)).ToSmartList();
        }

        #endregion

        #region Create

        ///<summary>
        ///Creates a new project using all the necessary (title, description, responsible ID, etc) and some optional parameters specified in the request
        ///</summary>
        ///<short>
        ///Create project
        ///</short>
        /// <category>Projects</category>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="responsibleId">Responsible ID</param>
        ///<param name="tags">Tags</param>
        ///<param name="private">Is project private</param>
        ///<param name="participants" optional="true">Project participants</param>
        ///<param name="notify" optional="true">Notify project manager</param>
        ///<returns>Newly created project</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create("")]
        public ProjectWrapperFull CreateProject(string title, string description, Guid responsibleId, string tags, bool @private, IEnumerable<Guid> participants, bool? notify)
        {
            if (responsibleId == Guid.Empty) throw new ArgumentException(@"responsible can't be empty", "responsibleId");
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            ProjectSecurity.DemandCreateProject();

            var projectEngine = EngineFactory.GetProjectEngine();
            var participantEngine = EngineFactory.GetParticipantEngine();

            var project = new Project
                {
                    Title = title,
                    Status = ProjectStatus.Open,
                    Responsible = responsibleId,
                    Description = description,
                    Private = @private
                };

            project = projectEngine.SaveOrUpdate(project, true);
            projectEngine.AddToTeam(project, participantEngine.GetByID(responsibleId), notify ?? true);
            EngineFactory.GetTagEngine().SetProjectTags(project.ID, tags);

            var participantsList = participants.ToList();
            foreach (var participant in participantsList)
            {
                projectEngine.AddToTeam(project, participantEngine.GetByID(participant), true);
            }

            MessageService.Send(_context, MessageAction.ProjectCreated, project.Title);

            return new ProjectWrapperFull(project) {ParticipantCount = participantsList.Count()};
        }

        #endregion

        #region Update

        ///<summary>
        ///Updates the existing project information using all the parameters (project ID, title, description, responsible ID, etc) specified in the request
        ///</summary>
        ///<short>
        ///Update project
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="responsibleId">Responsible ID</param>
        ///<param name="tags">Tags</param>
        ///<param name="private">Is project private</param>
        ///<param name="status">Status. One of (Open|Closed)</param>
        ///<param name="notify">Notify project manager</param>
        ///<returns>Updated project</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{id:[0-9]+}")]
        public ProjectWrapperFull UpdateProject(int id, string title, string description, Guid responsibleId, string tags, ProjectStatus? status, bool? @private, bool notify)
        {
            if (responsibleId == Guid.Empty) throw new ArgumentException(@"responsible can't be empty", "responsibleId");
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            var projectEngine = EngineFactory.GetProjectEngine();
            var participantEngine = EngineFactory.GetParticipantEngine();

            var project = projectEngine.GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandEdit(project);

            if (!projectEngine.IsInTeam(project.ID, responsibleId))
            {
                projectEngine.AddToTeam(project, participantEngine.GetByID(responsibleId), false);
            }

            project.Title = Update.IfNotEmptyAndNotEquals(project.Title, title);
            project.StatusChangedOn = DateTime.Now;

            if (status.HasValue)
            {
                project.Status = status.Value;
            }

            project.Responsible = Update.IfNotEmptyAndNotEquals(project.Responsible, responsibleId);
            project.Description = Update.IfNotEmptyAndNotEquals(project.Description, description);

            if (@private.HasValue)
            {
                project.Private = @private.Value;
            }

            project = projectEngine.SaveOrUpdate(project, notify);
            EngineFactory.GetTagEngine().SetProjectTags(project.ID, tags);
            MessageService.Send(_context, MessageAction.ProjectUpdated, project.Title);

            return new ProjectWrapperFull(project, EngineFactory.GetFileEngine().GetRoot(id));
        }

        ///<summary>
        ///Updates the status of the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update project status
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="status">Status. One of (Open|Paused|Closed)</param>
        ///<returns>Updated project</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{id:[0-9]+}/status")]
        public ProjectWrapperFull UpdateProject(int id, ProjectStatus status)
        {
            var projectEngine = EngineFactory.GetProjectEngine();
            var project = projectEngine.GetByID(id).NotFoundIfNull();

            project = projectEngine.ChangeStatus(project, status);
            MessageService.Send(_context, MessageAction.ProjectUpdatedStatus, project.Title, LocalizedEnumConverter.ConvertToString(project.Status));

            return new ProjectWrapperFull(project, EngineFactory.GetFileEngine().GetRoot(id));
        }

        #endregion

        #region Delete

        ///<summary>
        ///Deletes the project with the ID specified in the request from the portal
        ///</summary>
        ///<short>
        ///Delete project
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>Deleted project</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"{id:[0-9]+}")]
        public ProjectWrapperFull DeleteProject(int id)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandEdit(project);

            projectEngine.Delete(id);
            MessageService.Send(_context, MessageAction.ProjectDeleted, project.Title);

            return new ProjectWrapperFull(project, EngineFactory.GetFileEngine().GetRoot(id));
        }

        #endregion

        #region Follow, Tags, Time

        ///<summary>
        ///Subscribe or unsubscribe to notifications about the actions performed with the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Following/Unfollowing project
        ///</short>
        /// <category>Projects</category>
        ///<param name="projectId">Project ID</param>
        /// <returns>Project</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{projectid:[0-9]+}/follow")]
        public ProjectWrapper FollowToProject(int projectId)
        {
            var projectEngine = EngineFactory.GetProjectEngine();
            var project = projectEngine.GetByID(projectId).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            var participantEngine = EngineFactory.GetParticipantEngine();
            if (participantEngine.GetFollowingProjects(CurrentUserId).Contains(projectId))
            {
                participantEngine.RemoveFromFollowingProjects(projectId, CurrentUserId);
                MessageService.Send(_context, MessageAction.ProjectUnfollowed, project.Title);
            }
            else
            {
                participantEngine.AddToFollowingProjects(projectId, CurrentUserId);
                MessageService.Send(_context, MessageAction.ProjectFollowed, project.Title);
            }

            return new ProjectWrapper(project);
        }

        ///<summary>
        ///Updates the tags for the project with the selected project ID with the tags specified in the request
        ///</summary>
        ///<short>
        ///Update project tags
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="tags">Tags</param>
        ///<returns>project</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{id:[0-9]+}/tag")]
        public ProjectWrapperFull UpdateProjectTags(int id, string tags)
        {
            var project = EngineFactory.GetProjectEngine().GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandEdit(project);

            EngineFactory.GetTagEngine().SetProjectTags(id, tags);

            return new ProjectWrapperFull(project, EngineFactory.GetFileEngine().GetRoot(id));
        }

        ///<summary>
        ///Returns the detailed information about the time spent on the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Project time spent
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>List of time spent</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}/time")]
        public IEnumerable<TimeWrapper> GetProjectTime(int id)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(id)) throw new ItemNotFoundException();
            return EngineFactory.GetTimeTrackingEngine().GetByProject(id).Select(x => new TimeWrapper(x)).ToSmartList();
        }

        #endregion

        #region Milestones

        ///<summary>
        ///Creates a new milestone using the parameters (project ID, milestone title, deadline, etc) specified in the request
        ///</summary>
        ///<short>
        ///Add milestone
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="title">Milestone title</param>
        ///<param name="deadline">Milestone deadline</param>
        ///<param name="isKey">Is milestone key or not</param>
        ///<param name="isNotify">Remind me 48 hours before the due date</param>
        ///<param name="description">Milestone description</param>
        ///<param name="responsible">Milestone responsible</param>
        ///<param name="notifyResponsible">Notify responsible</param>
        ///<returns>Created milestone</returns>
        ///<exception cref="ArgumentNullException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"{id:[0-9]+}/milestone")]
        public MilestoneWrapper AddProjectMilestone(int id, string title, ApiDateTime deadline, bool isKey, bool isNotify, string description, Guid responsible, bool notifyResponsible)
        {
            if (title == null) throw new ArgumentNullException("title");
            if (deadline == DateTime.MinValue) throw new ArgumentNullException("deadline");

            var project = EngineFactory.GetProjectEngine().GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandCreateMilestone(project);

            var milestone = new Milestone
                {
                    Description = description ?? "",
                    Project = project,
                    Title = title.Trim(),
                    DeadLine = deadline,
                    IsKey = isKey,
                    Status = MilestoneStatus.Open,
                    IsNotify = isNotify,
                    Responsible = responsible
                };
            milestone = EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone, notifyResponsible);
            MessageService.Send(_context, MessageAction.MilestoneCreated, milestone.Project.Title, milestone.Title);

            return new MilestoneWrapper(milestone);
        }

        ///<summary>
        ///Returns the list of all the milestones within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get milestones by project ID
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>List of milestones</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}/milestone")]
        public IEnumerable<MilestoneWrapper> GetProjectMilestones(int id)
        {
            var project = EngineFactory.GetProjectEngine().GetByID(id).NotFoundIfNull();

            //NOTE: move to engine
            if (!ProjectSecurity.CanReadMilestones(project)) throw ProjectSecurity.CreateSecurityException();

            var milestones = EngineFactory.GetMilestoneEngine().GetByProject(id);

            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all the milestones with the selected status within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get milestones by project ID and milestone status
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<param name="status">Milestone status</param>
        ///<returns>List of milestones</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}/milestone/{status:(open|closed|late|disable)}")]
        public IEnumerable<MilestoneWrapper> GetProjectMilestones(int id, MilestoneStatus status)
        {
            var project = EngineFactory.GetProjectEngine().GetByID(id).NotFoundIfNull();

            if (!ProjectSecurity.CanReadMilestones(project)) throw ProjectSecurity.CreateSecurityException();

            var milestones = EngineFactory.GetMilestoneEngine().GetByStatus(id, status);

            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        #endregion

        #region Team

        ///<summary>
        ///Returns the list of all users participating in the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Project team
        ///</short>
        /// <category>Team</category>
        /// <param name="projectid">Project ID</param>
        ///<returns>List of team members</returns>
        [Read(@"{projectid:[0-9]+}/team")]
        public IEnumerable<ParticipantWrapper> GetProjectTeam(int projectid)
        {
            var projectEngine = EngineFactory.GetProjectEngine();
            if (!projectEngine.IsExists(projectid)) throw new ItemNotFoundException();

            return projectEngine.GetTeam(projectid)
                                .Select(x => new ParticipantWrapper(x))
                                .OrderBy(r => r.DisplayName).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all users participating in the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Project team
        ///</short>
        /// <category>Team</category>
        /// <param name="ids">Project IDs</param>
        ///<returns>List of team members</returns>
        [Create(@"team")]
        public IEnumerable<ParticipantWrapper> GetProjectTeam(List<int> ids)
        {
            return EngineFactory.GetProjectEngine().GetTeam(ids)
                                .Select(x => new ParticipantWrapper(x))
                                .OrderBy(r => r.DisplayName).ToSmartList();
        }

        ///<summary>
        ///Adds the user with the ID specified in the request to the selected project team
        ///</summary>
        ///<short>
        ///Add to team
        ///</short>
        /// <category>Team</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="userId">ID of the user to add</param>
        ///<returns>List of team members</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"{projectid:[0-9]+}/team")]
        public IEnumerable<ParticipantWrapper> AddToProjectTeam(int projectid, Guid userId)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEditTeam(project);

            projectEngine.AddToTeam(project, EngineFactory.GetParticipantEngine().GetByID(userId), true);

            return GetProjectTeam(projectid);
        }

        ///<summary>
        ///Sets the security rights for the user or users with the IDs specified in the request within the selected project
        ///</summary>
        ///<short>
        ///Set team security
        ///</short>
        /// <category>Team</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="userId">ID of the user to set</param>
        ///<param name="security">Security rights</param>
        ///<param name="visible">Visible</param>
        ///<returns>List of team members</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{projectid:[0-9]+}/team/security")]
        public IEnumerable<ParticipantWrapper> SetProjectTeamSecurity(int projectid, Guid userId, ProjectTeamSecurity security, bool visible)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEditTeam(project);

            if (!projectEngine.IsInTeam(projectid, userId))
            {
                throw new ArgumentOutOfRangeException("userId", "Not a project memeber");
            }

            projectEngine.SetTeamSecurity(project, EngineFactory.GetParticipantEngine().GetByID(userId), security, visible);

            var team = GetProjectTeam(projectid);
            var user = team.SingleOrDefault(t => t.Id == userId);
            if (user != null)
            {
                MessageService.Send(_context, MessageAction.ProjectUpdatedMemberRights, project.Title, HttpUtility.HtmlDecode(user.DisplayName));
            }

            return team;
        }

        ///<summary>
        ///Removes the user with the ID specified in the request from the selected project team
        ///</summary>
        ///<short>
        ///Remove from team
        ///</short>
        /// <category>Team</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="userId">ID of the user to add</param>
        ///<returns>List of team members</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"{projectid:[0-9]+}/team")]
        public IEnumerable<ParticipantWrapper> DeleteFromProjectTeam(int projectid, Guid userId)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEditTeam(project);

            var particapant = EngineFactory.GetParticipantEngine().GetByID(userId);
            projectEngine.RemoveFromTeam(project, particapant, true);

            MessageService.Send(_context, MessageAction.ProjectDeletedMember, project.Title, particapant.UserInfo.DisplayUserName(false));

            return GetProjectTeam(projectid);
        }

        ///<summary>
        /// Updates the project team with the users IDs specified in the request
        ///</summary>
        ///<short>
        ///Updates project team
        ///</short>
        /// <category>Team</category>
        ///<param name="projectId">Project ID</param>
        ///<param name="participants">IDs of users to update team</param>
        ///<param name="notify">Notify project team</param>
        ///<returns>String with the number of project participants</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"{projectid:[0-9]+}/team")]
        public IEnumerable<ParticipantWrapper> UpdateProjectTeam(int projectId, IEnumerable<Guid> participants, bool notify)
        {
            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectId).NotFoundIfNull();
            ProjectSecurity.DemandEditTeam(project);

            var participantsList = participants.ToList();
            projectEngine.UpdateTeam(project, participantsList, notify);

            var team = GetProjectTeam(projectId);
            MessageService.Send(_context, MessageAction.ProjectUpdatedTeam, project.Title, team.Select(t => t.DisplayName));

            return team;
        }

        #endregion

        #region Task

        ///<summary>
        ///Returns the list of all the tasks within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Tasks
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<returns></returns>
        ///<exception cref="ItemNotFoundException">List of tasks</exception>
        [Read(@"{projectid:[0-9]+}/task")]
        public IEnumerable<TaskWrapper> GetProjectTasks(int projectid)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(projectid)) throw new ItemNotFoundException();

            return EngineFactory
                .GetTaskEngine().GetByProject(projectid, TaskStatus.Open, Guid.Empty)
                .Select(x => new TaskWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Adds the task to the selected project with the parameters (responsible user ID, task description, deadline time, etc) specified in the request
        ///</summary>
        ///<short>
        ///Add task
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="description">Description</param>
        ///<param name="deadline">Deadline time</param>
        ///<param name="priority">Priority: Low|Normal|High</param>
        ///<param name="title">Title</param>
        ///<param name="milestoneid">Milestone ID</param>
        ///<param name="responsibles">List responsibles</param>
        ///<param name="notify">Notify responsible</param>
        ///<param name="startDate"></param>
        ///<returns>Created task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"{projectid:[0-9]+}/task")]
        public TaskWrapper AddProjectTask(int projectid, string description, ApiDateTime deadline,
                                          TaskPriority priority, string title, int milestoneid,
                                          IEnumerable<Guid> responsibles, bool notify, ApiDateTime startDate)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandCreateTask(project);

            if (!EngineFactory.GetMilestoneEngine().IsExists(milestoneid) && milestoneid > 0)
            {
                throw new ItemNotFoundException("Milestone not found");
            }

            var task = new Task
                {
                    CreateBy = CurrentUserId,
                    CreateOn = Core.Tenants.TenantUtil.DateTimeNow(),
                    Deadline = deadline,
                    Description = description ?? "",
                    Priority = priority,
                    Status = TaskStatus.Open,
                    Title = title,
                    Project = project,
                    Milestone = milestoneid,
                    Responsibles = new HashSet<Guid>(responsibles),
                    StartDate = startDate
                };
            EngineFactory.GetTaskEngine().SaveOrUpdate(task, null, notify);

            var wrapper = GetTask(task.ID);
            MessageService.Send(_context, MessageAction.TaskCreated, project.Title, wrapper.Title);

            return wrapper;
        }

        ///<summary>
        ///Adds the task to the selected project 
        ///</summary>
        ///<short>
        ///Add task
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="messageid">Message ID</param>
        ///<returns>Created task</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"{projectid:[0-9]+}/task/{messageid:[0-9]+}")]
        public TaskWrapper AddProjectTaskByMessage(int projectid, int messageid)
        {
            var projectEngine = EngineFactory.GetProjectEngine();
            var messageEngine = EngineFactory.GetMessageEngine();
            var taskEngine = EngineFactory.GetTaskEngine();
            var commentEngine = EngineFactory.GetCommentEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandCreateTask(project);

            var task = new Task
                {
                    CreateBy = CurrentUserId,
                    CreateOn = Core.Tenants.TenantUtil.DateTimeNow(),
                    Status = TaskStatus.Open,
                    Title = discussion.Title,
                    Project = project
                };

            task = taskEngine.SaveOrUpdate(task, null, true);

            commentEngine.SaveOrUpdate(new Comment
                {
                    ID = Guid.NewGuid(),
                    TargetUniqID = ProjectEntity.BuildUniqId<Task>(task.ID),
                    Content = discussion.Content
                });
            //copy comments
            var comments = commentEngine.GetComments(discussion);
            var newOldComments = new Dictionary<Guid, Guid>();

            var i = 1;
            foreach (var comment in comments)
            {
                var newID = Guid.NewGuid();
                newOldComments.Add(comment.ID, newID);

                comment.ID = newID;
                comment.CreateOn = Core.Tenants.TenantUtil.DateTimeNow().AddSeconds(i);
                comment.TargetUniqID = ProjectEntity.BuildUniqId<Task>(task.ID);

                if (!comment.Parent.Equals(Guid.Empty))
                {
                    comment.Parent = newOldComments[comment.Parent];
                }

                commentEngine.SaveOrUpdate(comment);
                i++;
            }

            //copy files
            var files = messageEngine.GetFiles(discussion);

            foreach (var file in files)
            {
                taskEngine.AttachFile(task, file.ID, false);
            }

            //copy recipients

            foreach (var participiant in messageEngine.GetSubscribers(discussion))
            {
                taskEngine.Subscribe(task, new Guid(participiant.ID));
            }

            MessageService.Send(_context, MessageAction.TaskCreatedFromDiscussion, project.Title, discussion.Title, task.Title);

            return new TaskWrapper(task);
        }

        ///<summary>
        ///Returns the list of all tasks with the selected status in the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Tasks with status
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="status">Task status. Can be one of: notaccept|open|closed|disable|unclassified|notinmilestone</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{projectid:[0-9]+}/task/{status:(notaccept|open|closed|disable|unclassified|notinmilestone)}")]
        public IEnumerable<TaskWrapper> GetProjectTasks(int projectid, TaskStatus status)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(projectid)) throw new ItemNotFoundException();
            return EngineFactory
                .GetTaskEngine().GetByProject(projectid, status, Guid.Empty)
                .Select(x => new TaskWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all tasks in the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///All tasks
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{projectid:[0-9]+}/task/@all")]
        public IEnumerable<TaskWrapper> GetAllProjectTasks(int projectid)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(projectid)) throw new ItemNotFoundException();
            return EngineFactory
                .GetTaskEngine().GetByProject(projectid, null, Guid.Empty)
                .Select(x => new TaskWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all tasks for the current user with the selected status in the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///My tasks
        ///</short>
        /// <category>Tasks</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="status">Task status. Can be one of: notaccept|open|closed|disable|unclassified|notinmilestone</param>
        ///<returns>List of tasks</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{projectid:[0-9]+}/task/@self/{status:(notaccept|open|closed|disable|unclassified|notinmilestone)}")]
        public IEnumerable<TaskWrapper> GetProjectMyTasks(int projectid, TaskStatus status)
        {
            if (!EngineFactory.GetProjectEngine().IsExists(projectid)) throw new ItemNotFoundException();

            return EngineFactory
                .GetTaskEngine().GetByProject(projectid, status, CurrentUserId)
                .Select(x => new TaskWrapper(x)).ToSmartList();
        }

        #endregion

        #region Files

        ///<summary>
        ///Returns the detailed list of all files and folders for the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Project files by project ID
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>Project files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{id:[0-9]+}/files")]
        public FolderContentWrapper GetProjectFiles(int id)
        {
            var project = EngineFactory.GetProjectEngine().GetByID(id).NotFoundIfNull();

            if (ProjectSecurity.CanReadFiles(project))
                return documentsApi.GetFolder(EngineFactory.GetFileEngine().GetRoot(id).ToString(), Guid.Empty, Files.Core.FilterType.None);

            throw new SecurityException("Access to files is denied");
        }

        ///<summary>
        ///Returns the list of all files within the entity (project, milestone, task) with the type and ID specified
        ///</summary>
        ///<short>
        ///Entity files
        ///</short>
        /// <category>Files</category>
        ///<param name="entityType">Entity type</param>
        ///<param name="entityID">Entity ID</param>
        ///<returns>Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{entityID:[0-9]+}/entityfiles")]
        public IEnumerable<FileWrapper> GetEntityFiles(EntityType entityType, int entityID)
        {
            switch (entityType)
            {
                case EntityType.Message:
                    return GetMessageFiles(entityID);

                case EntityType.Task:
                    return GetTaskFiles(entityID);
            }

            return new List<FileWrapper>();
        }

        ///<summary>
        ///Uploads the selected files to the entity (project, milestone, task) with the type and ID specified
        ///</summary>
        ///<short>
        /// Upload file to entity
        ///</short>
        /// <category>Files</category>
        ///<param name="entityType">Entity type </param>
        ///<param name="entityID">Entity ID</param>
        ///<param name="files">File IDs</param>
        ///<returns>Uploaded files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<visible>false</visible>
        [Create(@"{entityID:[0-9]+}/entityfiles")]
        public IEnumerable<FileWrapper> UploadFilesToEntity(EntityType entityType, int entityID, IEnumerable<int> files)
        {
            var fileEngine = EngineFactory.GetFileEngine();
            var filesList = files.ToList();

            switch (entityType)
            {
                case EntityType.Message:
                    UploadFilesToMessage(entityID, filesList);
                    break;

                case EntityType.Task:
                    UploadFilesToTask(entityID, filesList);
                    break;
            }

            var listFiles = filesList.Select(r => fileEngine.GetFile(r, 1).NotFoundIfNull()).ToList();

            return listFiles.Select(x => new FileWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Detaches the selected file from the entity (project, milestone, task) with the type and ID specified
        ///</summary>
        ///<short>
        /// Detach file from entity
        ///</short>
        /// <category>Files</category>
        ///<param name="entityType">Entity type </param>
        ///<param name="entityID">Entity ID</param>
        ///<param name="fileid">File ID</param>
        ///<returns>Detached file</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<visible>false</visible>
        [Delete(@"{entityID:[0-9]+}/entityfiles")]
        public FileWrapper DetachFileFromEntity(EntityType entityType, int entityID, int fileid)
        {
            switch (entityType)
            {
                case EntityType.Message:
                    DetachFileFromMessage(entityID, fileid);
                    break;

                case EntityType.Task:
                    DetachFileFromTask(entityID, fileid);
                    break;
            }

            var file = EngineFactory.GetFileEngine().GetFile(fileid, 1).NotFoundIfNull();
            return new FileWrapper(file);
        }

        ///<summary>
        ///Uploads the selected files to the entity (project, milestone, task) with the type and ID specified
        ///</summary>
        ///<short>
        /// Upload file to entity
        ///</short>
        /// <category>Files</category>
        ///<param name="entityType">Entity type </param>
        ///<param name="entityID">Entity ID</param>
        /// <param name="folderid">ID of the folder to upload to</param>
        /// <param name="file" visible="false">Request enput stream</param>
        /// <param name="contentType" visible="false">Content-type header</param>
        /// <param name="contentDisposition" visible="false">Content disposition header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <param name="createNewIfExist" visible="false">Create new if exist</param>
        /// <param name="storeOriginalFileFlag" visible="false">If true, upload documents in original formats as well</param>
        ///<returns>Uploaded files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<visible>false</visible>
        [Create(@"{entityID:[0-9]+}/entityfiles/upload")]
        public object UploadFilesToEntity(EntityType entityType, int entityID, string folderid, Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files, bool createNewIfExist, bool storeOriginalFileFlag)
        {
            var filesList = files.ToList();
            if (!filesList.Any()) return new object();
            var fileWrappers = documentsApi.UploadFile(folderid, file, contentType, contentDisposition, filesList, createNewIfExist, storeOriginalFileFlag);

            if (fileWrappers == null) return null;

            var fileIDs = new List<int>();

            if (filesList.Count() > 1)
            {
                var wrappers = fileWrappers as IEnumerable<FileWrapper>;
                if (wrappers != null)
                {
                    fileIDs = wrappers.Select(r => (int)r.Id).ToList();
                }
            }
            else
            {
                var fileWrapper = fileWrappers as FileWrapper;
                if (fileWrapper != null)
                    fileIDs = new List<int> {(int)fileWrapper.Id};
            }

            switch (entityType)
            {
                case EntityType.Message:
                    UploadFilesToMessage(entityID, fileIDs);
                    break;

                case EntityType.Task:
                    UploadFilesToTask(entityID, fileIDs);
                    break;
            }

            return fileWrappers;
        }

        #endregion

        #endregion

        #region contacts

        /// <summary>
        ///  Returns the list of all the projects linked with the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <category>Contacts</category>
        /// <short>Get projects for contact</short> 
        /// <returns>
        ///     Projects list
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read("contact/{contactid:[0-9]+}")]
        public IEnumerable<ProjectWrapperFull> GetProjectsByContactID(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            return EngineFactory.GetProjectEngine().GetProjectsByContactID(contactid).Select(x => new ProjectWrapperFull(x)).ToSmartList();
        }

        /// <summary>
        ///  Adds the selected contact to the project with the ID specified in the request
        /// </summary>
        /// <param name="projectid">Project ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <category>Contacts</category>
        /// <short>Add project contact</short> 
        /// <returns>Project</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"{projectid:[0-9]+}/contact")]
        public ProjectWrapperFull AddProjectContact(int projectid, int contactid)
        {
            var contact = CrmDaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null) throw new ArgumentException();

            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetFullProjectByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandLinkContact(project);

            projectEngine.AddProjectContact(projectid, contactid);

            var messageAction = contact is Company ? MessageAction.CompanyLinkedProject : MessageAction.PersonLinkedProject;
            MessageService.Send(_context, messageAction, contact.GetTitle(), project.Title);

            return new ProjectWrapperFull(project);
        }

        /// <summary>
        ///  Deletes the selected contact from the project with the ID specified in the request
        /// </summary>       
        /// <param name="projectid">Project ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <category>Contacts</category>
        /// <short>Delete project contact</short> 
        /// <returns>Project</returns>
        ///<exception cref="ArgumentException"></exception>
        [Delete("{projectid:[0-9]+}/contact")]
        public ProjectWrapperFull DeleteProjectContact(int projectid, int contactid)
        {
            var contact = CrmDaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null) throw new ArgumentException();

            var projectEngine = EngineFactory.GetProjectEngine();

            var project = projectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(project);

            projectEngine.DeleteProjectContact(projectid, contactid);

            var messageAction = contact is Company ? MessageAction.CompanyUnlinkedProject : MessageAction.PersonUnlinkedProject;
            MessageService.Send(_context, messageAction, contact.GetTitle(), project.Title);

            return new ProjectWrapperFull(project);
        }

        #endregion

        #region templates

        ///<summary>
        ///Returns the list of all the templates with base information about them
        ///</summary>
        ///<short>
        ///Templates
        ///</short>
        /// <category>Projects</category>
        ///<returns>List of templates</returns>
        [Read("template")]
        public IEnumerable<ObjectWrapperBase> GetAllTemplates()
        {
            return EngineFactory.GetTemplateEngine().GetAll().Select(x => new ObjectWrapperBase {Id = x.Id, Title = x.Title, Description = x.Description}).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information about the template with ID specified in the request
        ///</summary>
        ///<short>
        ///Template by ID
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Template ID</param>
        ///<returns>Template</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"template/{id:[0-9]+}")]
        public ObjectWrapperBase GetTemplate(int id)
        {
            var template = EngineFactory.GetTemplateEngine().GetByID(id).NotFoundIfNull();
            return new ObjectWrapperBase {Id = template.Id, Title = template.Title, Description = template.Description};
        }

        ///<summary>
        ///Creates a new template 
        ///</summary>
        ///<short>
        ///Create template
        ///</short>
        /// <category>Projects</category>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<returns>Newly created template</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create("template")]
        public ObjectWrapperBase CreateTemplate(string title, string description)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            ProjectSecurity.DemandCreateProject();

            var template = new Template
                {
                    Title = title,
                    Description = description
                };

            template = EngineFactory.GetTemplateEngine().SaveOrUpdate(template).NotFoundIfNull();
            MessageService.Send(_context, MessageAction.ProjectTemplateCreated, template.Title);

            return new ObjectWrapperBase {Id = template.Id, Title = template.Title, Description = template.Description};
        }

        ///<summary>
        ///Updates the existing template information 
        ///</summary>
        ///<short>
        ///Update template
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Template ID</param>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<returns>Updated template</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"template/{id:[0-9]+}")]
        public ObjectWrapperBase UpdateTemplate(int id, string title, string description)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");

            var templateEngine = EngineFactory.GetTemplateEngine();

            var template = templateEngine.GetByID(id).NotFoundIfNull();

            template.Title = Update.IfNotEmptyAndNotEquals(template.Title, title);
            template.Description = Update.IfNotEmptyAndNotEquals(template.Description, description);

            template = templateEngine.SaveOrUpdate(template);
            MessageService.Send(_context, MessageAction.ProjectTemplateUpdated, template.Title);

            return new ObjectWrapperBase {Id = template.Id, Title = template.Title, Description = template.Description};
        }

        ///<summary>
        ///Deletes the template with the ID specified in the request from the portal
        ///</summary>
        ///<short>
        ///Delete template
        ///</short>
        /// <category>Projects</category>
        ///<param name="id">Project ID</param>
        ///<returns>Deleted template</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"template/{id:[0-9]+}")]
        public ObjectWrapperBase DeleteTemplate(int id)
        {
            var templateEngine = EngineFactory.GetTemplateEngine();
            var template = templateEngine.GetByID(id).NotFoundIfNull();

            templateEngine.Delete(id);
            MessageService.Send(_context, MessageAction.ProjectTemplateDeleted, template.Title);

            return new ObjectWrapperBase {Id = template.Id, Title = template.Title, Description = template.Description};
        }

        #endregion

        #region HACK: Hidden api methods

        ///<summary>
        ///  Returns the basic information about the access rights
        ///</summary>
        ///<short>
        ///   Access rights info
        ///</short>
        ///<category>Projects</category>
        ///<returns>Basic information about access rights</returns>
        ///<visible>false</visible>
        [Read("securityinfo")]
        public CommonSecurityInfo GetProjectSecurityInfo()
        {
            return new CommonSecurityInfo();
        }

        ///<visible>false</visible>
        [Read("maxlastmodified")]
        public ApiDateTime GetProjectMaxLastModified()
        {
            var maxModified = EngineFactory.GetProjectEngine().GetMaxLastModified();
            var maxTeamModified = EngineFactory.GetProjectEngine().GetTeamMaxLastModified();
            var result = DateTime.Compare(maxModified, maxTeamModified) > 0 ? maxModified : maxTeamModified;
            return new ApiDateTime(result);
        }

        ///<visible>false</visible>
        [Read(@"{id:[0-9]+}/order")]
        public string GetTaskOrder(int id)
        {
            var projectEngine = EngineFactory.GetProjectEngine();
            var project = projectEngine.GetByID(id).NotFoundIfNull();

            return projectEngine.GetTaskOrder(project);
        }

        ///<visible>false</visible>
        [Update(@"{id:[0-9]+}/order")]
        public void SetTaskOrder(int id, string order)
        {
            if (string.IsNullOrEmpty(order)) throw new ArgumentException(@"order can't be empty", "order");

            var projectEngine = EngineFactory.GetProjectEngine();
            var project = projectEngine.GetByID(id).NotFoundIfNull();

            projectEngine.SetTaskOrder(project, order);
        }

        #endregion
    }
}