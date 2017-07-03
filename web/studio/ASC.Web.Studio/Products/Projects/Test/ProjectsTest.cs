namespace ASC.Web.Projects.Test
{
    using System;
    using System.Collections.Generic;
    using ASC.Core;
    using NUnit.Framework;

    [TestFixture]
    public class ProjectsTest : BaseTest
    {
        [Test]
        public void Project()
        {
            var newProject = GenerateProject(SecurityContext.CurrentAccount.ID);

            SaveOrUpdate(newProject);

            Assert.AreNotEqual(newProject.ID, 0);

            var result = Get(newProject);

            Assert.AreEqual(newProject.ID, result.ID);

            newProject.Title = "NewTitle";
            newProject.Private = true;

            SaveOrUpdate(newProject);

            var updatedProject = Get(newProject);

            Assert.AreEqual(updatedProject.Title, newProject.Title);
            Assert.AreEqual(updatedProject.Private, newProject.Private);

            var team = new List<Guid>(4)
            {
                Owner,
                Admin,
                UserInTeam,
                Guest
            };

            AddTeamToProject(newProject, team);

            var getTeam = GetTeam(newProject.ID);
            CollectionAssert.AreEquivalent(team, getTeam);

            Delete(newProject);
            var deletedProject = Get(newProject);

            Assert.IsNull(deletedProject);
        }
    }
}