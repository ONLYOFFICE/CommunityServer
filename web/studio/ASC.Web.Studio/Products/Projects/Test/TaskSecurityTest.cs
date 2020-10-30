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
using System.Security;
using ASC.Projects.Core.Domain;

namespace ASC.Web.Projects.Test
{
    using ASC.Core;
    using NUnit.Framework;

    [TestFixture]
    [TestOf(typeof(TaskSecurityTest))]
    public class TaskSecurityTest : BaseTest
    {
        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Create Task" })]
        public void Create(Guid userID)
        {
            var task = CreateNewTask(userID);

            Assert.AreNotEqual(task.ID, 0);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestGuest", new object[] { "Create Task" })]
        public void CreateByGuestAndUserNotInTeam(Guid userID)
        {
            Assert.That(() => CreateNewTask(userID), Throws.TypeOf<SecurityException>());
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Create With Responsible Task" })]
        public void CreateWithResponsible(Guid userID)
        {
            var task = CreateNewTask(Owner, userID);

            Assert.AreNotEqual(task.ID, 0);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestGuest", new object[] { "Create With Responsible Task" })]
        public void CreateWithResponsibleByGuestAndUserNotInTeam(Guid userID)
        {
            Assert.That(() => CreateNewTask(Owner, userID), Throws.TypeOf<SecurityException>());
        }

        protected Task CreateNewTask(Guid userID, Guid? responsibleID = null)
        {
            SecurityContext.AuthenticateMe(userID);

            var newTask = GenerateTask();

            if (responsibleID.HasValue)
            {
                newTask.Responsibles.Add(responsibleID.Value);
            }

            SaveOrUpdate(newTask);

            return newTask;
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Read Task" })]
        [TestCaseGuest(TestName = "Read Task Guest")]
        public void Read(Guid userID)
        {
            var newTask = CreateNewTask(Owner);

            SecurityContext.AuthenticateMe(userID);

            var task = Get(newTask);

            Assert.AreEqual(newTask.ID, task.ID);
        }

        [TestCaseUserNotInTeam(TestName = "Read Task User Not In Team")]
        public virtual void ReadForUserNotInTeam(Guid userId)
        {
            Read(userId);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Read Task With Current Responsible" })]
        public void ReadTaskWithCurrentResponsible(Guid userID)
        {
            var newTask = CreateNewTask(Owner, userID);

            SecurityContext.AuthenticateMe(userID);

            var task = Get(newTask);

            Assert.AreEqual(newTask.ID, task.ID);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Update Task without responsible" })]
        public void UpdateWithoutResponsible(Guid userID)
        {
            var newTask = CreateNewTask(Owner);

            SecurityContext.AuthenticateMe(userID);

            newTask.Title = "Test";

            var task = SaveOrUpdate(newTask);

            Assert.AreEqual(newTask.ID, task.ID);
            Assert.AreEqual(newTask.Title, task.Title);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestGuest", new object[] { "Update By Guest and User not In Team" })]
        public void UpdateByGuest(Guid userId)
        {
            Assert.That(() => UpdateWithoutResponsible(userId), Throws.TypeOf<SecurityException>());
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Update In Closed Project" })]
        public void UpdateInClosedProject(Guid userID)
        {
            Assert.That(() =>
            {
                try
                {
                    ChangeProjectStatus(ProjectStatus.Closed);

                    UpdateWithoutResponsible(Guest);
                }
                finally
                {
                    ChangeProjectStatus(ProjectStatus.Open);
                }
            }, Throws.TypeOf<SecurityException>());
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Update Task By Created By" })]
        public void UpdateByCreatedBy(Guid userID)
        {
            var newTask = CreateNewTask(userID);

            newTask.Title = "Test";

            var task = SaveOrUpdate(newTask);

            Assert.AreEqual(newTask.ID, task.ID);
            Assert.AreEqual(newTask.Title, task.Title);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestCases", new object[] { "Update Task By Responsible" })]
        public void UpdateByResponsible(Guid userID)
        {
            var newTask = CreateNewTask(Owner, userID);

            SecurityContext.AuthenticateMe(userID);

            newTask.Title = "Test";

            var task = SaveOrUpdate(newTask);

            Assert.AreEqual(newTask.ID, task.ID);
            Assert.AreEqual(newTask.Title, task.Title);
        }

        [Test(Description = "Update Task With Another Responsible")]
        public void UpdateWithAnotherResponsible()
        {
            Assert.That(() =>
            {
                var newTask = CreateNewTask(Owner, ProjectManager);

                SecurityContext.AuthenticateMe(UserInTeam);

                newTask.Title = "Test";

                SaveOrUpdate(newTask);
            }, Throws.TypeOf<SecurityException>());
        }

        [TestCaseAdmin(TestName = "Delete Task by Admin")]
        [TestCaseOwner(TestName = "Delete Task by Admin")]
        [TestCaseProjectManager(TestName = "Delete Task by Admin")]
        public void DeleteByAdmin(Guid userID)
        {
            Assert.IsNull(DeleteTask(userID));
        }

        public Task DeleteTask(Guid userID)
        {
            var newTask = CreateNewTask(Owner);

            SecurityContext.AuthenticateMe(userID);

            Delete(newTask);

            return Get(newTask);
        }

        [TestCaseSource(typeof(CustomTestCaseData), "TestGuest", new object[] { "Delete" })]
        public void DeleteWithException(Guid userID)
        {
            Assert.That(() => DeleteTask(userID), Throws.TypeOf<SecurityException>());
        }
    }

    [TestFixture]
    [TestOf(typeof(TaskSecurityTestPrivateProject))]
    public class TaskSecurityTestPrivateProject : TaskSecurityTest
    {
        [SetUp]
        public void SetProjectPrivate()
        {
            ChangeProjectPrivate(true);
        }

        [TestCaseUserNotInTeam(TestName = "Read Task User Not In Team")]
        public override void ReadForUserNotInTeam(Guid userId)
        {
            var newTask = CreateNewTask(Owner);

            SecurityContext.AuthenticateMe(userId);

            var task = Get(newTask);

            Assert.IsNull(task);
        }

        [Test(Description = "Read With Tasks Restricted")]
        public void ReadWithTasksRestricted()
        {
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Tasks, false);
            ReadForUserNotInTeam(UserInTeam);
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Tasks, true);
        }

        [Test(Description = "Read With Tasks Restricted WithResponsible")]
        public void ReadWithTasksRestrictedWithResponsible()
        {
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Tasks, false);
            ReadTaskWithCurrentResponsible(UserInTeam);
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Tasks, true);
        }

        [Test(Description = "Read With Tasks Restricted")]
        public void ReadWithMilestonesRestricted()
        {
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Milestone, false);

            var newTask = CreateNewTask(Owner);

            var milestone = SaveOrUpdate(GenerateMilestone());

            newTask.Milestone = milestone.ID;

            SaveOrUpdate(newTask);

            SecurityContext.AuthenticateMe(UserInTeam);

            var task = Get(newTask);

            Assert.IsNull(task);
            RestrictAccess(UserInTeam, ProjectTeamSecurity.Milestone, true);
        }

        [TearDown]
        public void SetProjectPublic()
        {
            ChangeProjectPrivate(false);
        }
    }
}