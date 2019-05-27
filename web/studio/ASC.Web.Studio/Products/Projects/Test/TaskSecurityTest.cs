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