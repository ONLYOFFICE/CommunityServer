/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Linq;

using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using Autofac;

namespace ASC.Web.Projects.Core.Engine
{
    public class ProjectsReassign
    {
        private ProjectEngine ProjectEngine { get; set; }
        private MilestoneEngine MilestoneEngine { get; set; }
        private TaskEngine TaskEngine { get; set; }
        private SubtaskEngine SubtaskEngine { get; set; }
        private List<Project> FromUserProjects { get; set; }
        private List<Project> ToUserProjects { get; set; }

        public ProjectsReassign()
        {
            FromUserProjects = new List<Project>();
        }

        public void Reassign(Guid fromUserId, Guid toUserId)
        {
            using (var scope = DIHelper.Resolve(true))
            {
                var factory = scope.Resolve<EngineFactory>();
                ProjectEngine = factory.ProjectEngine;
                MilestoneEngine = factory.MilestoneEngine;
                TaskEngine = factory.TaskEngine;
                SubtaskEngine = factory.SubtaskEngine;

                FromUserProjects = ProjectEngine.GetByParticipant(fromUserId).ToList();
                ToUserProjects = ProjectEngine.GetByParticipant(toUserId).ToList();

                ReplaceTeam(fromUserId, toUserId);
                ReassignProjectManager(fromUserId, toUserId);
                ReassignMilestones(fromUserId, toUserId);
                ReassignTasks(fromUserId, toUserId);
                ReassignSubtasks(fromUserId, toUserId);
            }
        }

        private void ReplaceTeam(Guid fromUserId, Guid toUserId)
        {
            foreach (var project in FromUserProjects)
            {
                var teamSecurity = ProjectEngine.GetTeamSecurity(project, fromUserId);

                if (!ToUserProjects.Exists(r => r.ID == project.ID))
                {
                    ProjectEngine.AddToTeam(project, toUserId, false);
                    ProjectEngine.SetTeamSecurity(project, toUserId, teamSecurity);
                }

                ProjectEngine.RemoveFromTeam(project, fromUserId, false);
            }
        }

        private void ReassignProjectManager(Guid fromUserId, Guid toUserId)
        {
            var filter = new TaskFilter { UserId = fromUserId, ProjectStatuses = new List<ProjectStatus> { ProjectStatus.Open, ProjectStatus.Paused } };
            var projects = ProjectEngine.GetByFilter(filter);

            foreach (var project in projects)
            {
                project.Responsible = toUserId;
                ProjectEngine.SaveOrUpdate(project, false);
            }
        }

        private void ReassignMilestones(Guid fromUserId, Guid toUserId)
        {
            var filter = new TaskFilter { UserId = fromUserId, MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open} };
            var milestones = MilestoneEngine.GetByFilter(filter);

            foreach (var milestone in milestones)
            {
                AddToTeam(milestone.Project, toUserId);
                milestone.Responsible = toUserId;
                MilestoneEngine.SaveOrUpdate(milestone, false);
            }
        }

        private void ReassignTasks(Guid fromUserId, Guid toUserId)
        {
            var tasks = TaskEngine.GetByResponsible(fromUserId, TaskStatus.Open);

            foreach (var task in tasks.Where(r=> r.Responsibles.Any()))
            {
                AddToTeam(task.Project, toUserId);
                task.Responsibles = task.Responsibles.Where(r=> r != fromUserId).ToList();
                task.Responsibles.Add(toUserId);
                TaskEngine.SaveOrUpdate(task, null, false);
            }
        }

        private void ReassignSubtasks(Guid fromUserId, Guid toUserId)
        {
            var tasks = SubtaskEngine.GetByResponsible(fromUserId, TaskStatus.Open);

            foreach (var task in tasks)
            {
                AddToTeam(task.Project, toUserId);
                foreach (var subtask in task.SubTasks)
                {
                    subtask.Responsible = toUserId;
                    SubtaskEngine.SaveOrUpdate(subtask, task);
                }
            }
        }

        private void AddToTeam(Project project, Guid userId)
        {
            if (!FromUserProjects.Exists(r => r.ID == project.ID) && !ToUserProjects.Exists(r => r.ID == project.ID))
            {
                ProjectEngine.AddToTeam(project, userId, false);
                ToUserProjects.Add(project);
            }
        }
    }
}