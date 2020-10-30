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


using ASC.Web.Core;
using ASC.Web.Projects.Core;
using Autofac;
using NUnit.Framework;

namespace ASC.Web.Projects.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASC.Core;
    using ASC.Projects.Core.Domain;
    using ASC.Projects.Engine;

    [TestFixture]
    public class BaseTest
    {
        protected ILifetimeScope Scope { get; set; }
        protected TagEngine TagEngine { get; set; }
        protected TaskEngine TaskEngine { get; set; }
        protected SubtaskEngine SubtaskEngine { get; set; }
        protected MilestoneEngine MilestoneEngine { get; set; }
        protected MessageEngine MessageEngine { get; set; }
        protected TimeTrackingEngine TimeTrackingEngine { get; set; }
        protected ProjectEngine ProjectEngine { get; set; }
        protected ParticipantEngine ParticipantEngine { get; set; }
        protected DataGenerator DataGenerator { get; set; }

        private Project Project { get; set; }

        public static Guid UserInTeam = new Guid("809a74fd-2f52-4284-a267-f512fb60ce9e");
        public static Guid ProjectManager = new Guid("8a911318-129e-48b0-8414-04f49b308a9c");
        public static Guid UserNotInTeam = new Guid("ce90d596-9c6c-490f-b538-e8d3ccc12a72");
        public static Guid Guest = new Guid("f4381b0a-694a-4ab7-bfdb-6a6d25df65f8");
        public static Guid Admin = new Guid("f412903b-9601-42e9-91f2-1bd31f6da9e3");
        public static Guid Owner = new Guid("23f101b0-bc41-11e8-b696-9cb6d0fc71d8");

        [OneTimeSetUp]
        public void Init()
        {
            WebItemManager.Instance.LoadItems();

            CoreContext.TenantManager.SetCurrentTenant(CoreContext.TenantManager.GetTenants().First());
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            SecurityContext.AuthenticateMe(tenant.OwnerId);

            Scope = DIHelper.Resolve(true);

            var engineFactory = Scope.Resolve<EngineFactory>();
            Scope.Resolve<ProjectSecurity>();
            ProjectEngine = engineFactory.ProjectEngine;
            ParticipantEngine = engineFactory.ParticipantEngine;
            TaskEngine = engineFactory.TaskEngine;
            SubtaskEngine = engineFactory.SubtaskEngine;
            MilestoneEngine = engineFactory.MilestoneEngine;
            MessageEngine = engineFactory.MessageEngine;
            TimeTrackingEngine = engineFactory.TimeTrackingEngine;
            TagEngine = engineFactory.TagEngine;
            DataGenerator = new DataGenerator();

            var team = new List<Guid>(2) { ProjectManager, UserInTeam, Guest };

            Project = SaveOrUpdate(GenerateProject(ProjectManager));
            AddTeamToProject(Project, team);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Delete(Project);

            if (Scope != null)
            {
                Scope.Dispose();
            }
        }

        protected Project GenerateProject(Guid userId)
        {
            return DataGenerator.GenerateProject(userId);
        }

        protected Project SaveOrUpdate(Project project)
        {
            return ProjectEngine.SaveOrUpdate(project, false);
        }

        protected Project Get(Project project)
        {
            return ProjectEngine.GetByID(project.ID);
        }

        protected void AddTeamToProject(Project project, List<Guid> team)
        {
            foreach (var user in team.Select(r => ParticipantEngine.GetByID(r)))
            {
                ProjectEngine.AddToTeam(project, user, false);
            }
        }

        protected List<Guid> GetTeam(int projectId)
        {
            return ProjectEngine.GetTeam(projectId).Select(r => r.ID).ToList();
        }

        protected void Delete(Project project)
        {
            ProjectEngine.Delete(project.ID);
        }


        protected Task GenerateTask()
        {
            return DataGenerator.GenerateTask(Project);
        }

        protected Task SaveOrUpdate(Task task)
        {
            return TaskEngine.SaveOrUpdate(task, new List<int>(), false);
        }

        protected Task Get(Task task)
        {
            return TaskEngine.GetByID(task.ID);
        }

        protected void Delete(Task task)
        {
            TaskEngine.Delete(task);
        }

        protected Milestone GenerateMilestone()
        {
            return DataGenerator.GenerateMilestone(Project);
        }

        protected Milestone SaveOrUpdate(Milestone milestone)
        {
            return MilestoneEngine.SaveOrUpdate(milestone);
        }

        protected Milestone Get(Milestone milestone)
        {
            return MilestoneEngine.GetByID(milestone.ID);
        }

        protected void Delete(Milestone milestone)
        {
            MilestoneEngine.Delete(milestone);
        }


        protected Message GenerateMessage()
        {
            return DataGenerator.GenerateMessage(Project);
        }

        protected Message SaveOrUpdate(Message message)
        {
            return MessageEngine.SaveOrUpdate(message, false, new List<Guid>());
        }

        protected Message Get(Message message)
        {
            return MessageEngine.GetByID(message.ID);
        }

        protected void Delete(Message message)
        {
            MessageEngine.Delete(message);
        }

        protected TimeSpend GenerateTimeTracking()
        {
            var task = SaveOrUpdate(GenerateTask());
            return DataGenerator.GenerateTimeTracking(task);
        }

        protected TimeSpend SaveOrUpdate(TimeSpend timeSpend)
        {
            return TimeTrackingEngine.SaveOrUpdate(timeSpend);
        }

        protected TimeSpend Get(TimeSpend timeSpend)
        {
            return TimeTrackingEngine.GetByID(timeSpend.ID);
        }

        protected void Delete(TimeSpend timeSpend)
        {
            TimeTrackingEngine.Delete(timeSpend);
        }

        protected void ChangeProjectPrivate(bool @private)
        {
            SecurityContext.AuthenticateMe(Owner);
            Project.Private = @private;
            SaveOrUpdate(Project);
        }

        protected void ChangeProjectStatus(ProjectStatus status)
        {
            SecurityContext.AuthenticateMe(Owner);
            Project.Status = status;
            SaveOrUpdate(Project);
        }

        protected void RestrictAccess(Guid userID, ProjectTeamSecurity projectTeamSecurity, bool visible)
        {
            SecurityContext.AuthenticateMe(Owner);
            ProjectEngine.SetTeamSecurity(Project, ParticipantEngine.GetByID(userID), projectTeamSecurity, visible);
        }
    }
}