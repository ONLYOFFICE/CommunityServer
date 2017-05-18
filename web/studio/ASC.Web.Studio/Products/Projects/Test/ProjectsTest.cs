using System;

#if DEBUG
namespace ASC.Web.Projects.Test
{
    using ASC.Core;
    using ASC.Projects.Core.Domain;
    using ASC.Projects.Engine;
    using Core;
    using Studio.Utility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProjectsTest
    {
        public static Guid OwnerId { get; set; }
        public static ProjectEngine ProjectEngine { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            WebItemManager.Instance.LoadItems();
            CoreContext.TenantManager.SetCurrentTenant(0);
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            SecurityContext.AuthenticateMe(tenant.OwnerId);
            OwnerId = tenant.OwnerId;
            ProjectEngine = new EngineFactory("test", TenantProvider.CurrentTenantID).ProjectEngine;
        }

        [TestMethod]
        public Project CreateProject()
        {
            var newProject = new Project {Title = "Test", Responsible = OwnerId };

            ProjectEngine.SaveOrUpdate(newProject, false);

            Assert.AreNotEqual(newProject.ID, 0);

            return newProject;
        }

        [TestMethod]
        public void GetProject()
        {
            var newProject = CreateProject();

            var result = ProjectEngine.GetByID(newProject.ID);

            Assert.AreEqual(newProject.ID, result.ID);
        }

        [TestMethod]
        public void UpdateProject()
        {
            var project = CreateProject();

            project.Title = "NewTitle";
            project.Private = true;

            ProjectEngine.SaveOrUpdate(project, false);

            var updatedProject = ProjectEngine.GetByID(project.ID);

            Assert.AreEqual(updatedProject.Title, "NewTitle");
            Assert.AreEqual(updatedProject.Private, true);
        }

        [TestMethod]
        public void DeleteProject()
        {
            var project = CreateProject();

            ProjectEngine.Delete(project.ID);

            var deletedProject = ProjectEngine.GetByID(project.ID);

            Assert.IsNull(deletedProject);
        }
    }
}
#endif