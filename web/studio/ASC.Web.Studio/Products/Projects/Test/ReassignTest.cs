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
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Projects.Core;
using ASC.Web.Projects.Core.Engine;
using Autofac;
using NUnit.Framework;

namespace ASC.Web.Projects.Test
{
    [TestFixture]
    [TestOf(typeof(ReassignTest))]
    public class ReassignTest
    {
        protected ILifetimeScope Scope { get; set; }
        private TaskEngine TaskEngine { get; set; }
        private SubtaskEngine SubtaskEngine { get; set; }
        private MilestoneEngine MilestoneEngine { get; set; }
        private MessageEngine MessageEngine { get; set; }
        private TimeTrackingEngine TimeTrackingEngine { get; set; }
        private ProjectEngine ProjectEngine { get; set; }
        private ParticipantEngine ParticipantEngine { get; set; }
        private DataGenerator DataGenerator { get; set; }
        private ProjectsReassign ProjectsReassign { get; set; }
        private Project Project { get; set; }

        private static readonly Guid From = new Guid("0d5ed025-a78c-48b6-8ec9-29b225e85e23");
        private static readonly Guid To = new Guid("e4308b59-90bd-4f6c-807e-d1ee7716fe2d");
        private static readonly Guid Admin = new Guid("93580c54-1132-4d6b-bf2d-da0bfaaa1a28");

        [OneTimeSetUp]
        public void Init()
        {
            WebItemManager.Instance.LoadItems();

            CoreContext.TenantManager.SetCurrentTenant(0);
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            SecurityContext.AuthenticateMe(tenant.OwnerId);

            Scope = DIHelper.Resolve(true);

            var engineFactory = Scope.Resolve<EngineFactory>();
            ProjectEngine = engineFactory.ProjectEngine;
            ParticipantEngine = engineFactory.ParticipantEngine;
            TaskEngine = engineFactory.TaskEngine;
            SubtaskEngine = engineFactory.SubtaskEngine;
            MilestoneEngine = engineFactory.MilestoneEngine;
            MessageEngine = engineFactory.MessageEngine;
            TimeTrackingEngine = engineFactory.TimeTrackingEngine;
            DataGenerator = new DataGenerator();
            ProjectsReassign = new ProjectsReassign();
        }

        [SetUp]
        public void InitTest()
        {
            Project = ProjectEngine.SaveOrUpdate(DataGenerator.GenerateProject(Admin), false);
            ProjectEngine.AddToTeam(Project, From, false);
        }

        [TearDown]
        public void CleanupTest()
        {
            ProjectEngine.Delete(Project.ID);
        }

        [Test]
        public void ReassignTeam()
        {
            Project.Private = true;
            Project = ProjectEngine.SaveOrUpdate(Project, false);
            ProjectEngine.SetTeamSecurity(Project, From,
                ProjectTeamSecurity.Contacts |
                ProjectTeamSecurity.Files |
                ProjectTeamSecurity.Messages |
                ProjectTeamSecurity.Milestone |
                ProjectTeamSecurity.Tasks);

            ProjectsReassign.Reassign(From, To);

            var team = ProjectEngine.GetTeam(Project.ID).Select(r => r.ID).ToList();
            Assert.That(team, Has.No.Member(From));
            Assert.That(team, Has.Member(To));

            var security = ProjectEngine.GetTeamSecurity(Project, To);
            Assert.That(security & ProjectTeamSecurity.Tasks, Is.EqualTo(ProjectTeamSecurity.Tasks));
            Assert.That(security & ProjectTeamSecurity.Contacts, Is.EqualTo(ProjectTeamSecurity.Contacts));
            Assert.That(security & ProjectTeamSecurity.Files, Is.EqualTo(ProjectTeamSecurity.Files));
            Assert.That(security & ProjectTeamSecurity.Messages, Is.EqualTo(ProjectTeamSecurity.Messages));
            Assert.That(security & ProjectTeamSecurity.Milestone, Is.EqualTo(ProjectTeamSecurity.Milestone));
        }

        [Test]
        public void ReassignProjectManager()
        {
            Project.Responsible = From;
            ProjectEngine.SaveOrUpdate(Project, false);

            ProjectsReassign.Reassign(From, To);

            Project = ProjectEngine.GetByID(Project.ID);
            Assert.That(Project.Responsible, Is.EqualTo(To));
        }

        [Test]
        public void ReassignMilestone()
        {
            var milestone = DataGenerator.GenerateMilestone(Project);
            milestone.Responsible = From;
            milestone = MilestoneEngine.SaveOrUpdate(milestone);

            ProjectsReassign.Reassign(From, To);

            milestone = MilestoneEngine.GetByID(milestone.ID);
            Assert.That(milestone.Responsible, Is.EqualTo(To));
        }

        [Test]
        public void ReassignTask()
        {
            var task = DataGenerator.GenerateTask(Project);
            task.Responsibles = new List<Guid> {From};
            task = TaskEngine.SaveOrUpdate(task, new List<int>(), false);

            ProjectsReassign.Reassign(From, To);

            task = TaskEngine.GetByID(task.ID);
            Assert.That(task.Responsibles, Has.No.Member(From));
            Assert.That(task.Responsibles, Has.Member(To));
        }

        [Test]
        public void ReassignSubtasks()
        {
            var task = DataGenerator.GenerateTask(Project);
            task.Responsibles = new List<Guid> {Admin};
            task = TaskEngine.SaveOrUpdate(task, new List<int>(), false);

            var subtask1 = DataGenerator.GenerateSubtask(task);
            subtask1.Responsible = From;
            subtask1 = SubtaskEngine.SaveOrUpdate(subtask1, task);

            var subtask2 = DataGenerator.GenerateSubtask(task);
            subtask2.Responsible = From;
            subtask2 = SubtaskEngine.SaveOrUpdate(subtask2, task);

            var subtask3 = DataGenerator.GenerateSubtask(task);
            subtask3.Responsible = From;
            subtask3 = SubtaskEngine.SaveOrUpdate(subtask3, task);
            SubtaskEngine.ChangeStatus(task, subtask3, TaskStatus.Closed);

            ProjectsReassign.Reassign(From, To);

            subtask1 = SubtaskEngine.GetById(subtask1.ID);
            Assert.That(subtask1.Responsible, Is.EqualTo(To));

            subtask2 = SubtaskEngine.GetById(subtask2.ID);
            Assert.That(subtask2.Responsible, Is.EqualTo(To));

            subtask3 = SubtaskEngine.GetById(subtask3.ID);
            Assert.That(subtask3.Responsible, Is.EqualTo(From));
        }
    }
}