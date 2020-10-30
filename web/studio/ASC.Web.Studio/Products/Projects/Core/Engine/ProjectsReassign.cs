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