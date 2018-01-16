using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Core;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Dsl;

namespace ASC.Web.Projects.Test
{
    public class DataGenerator
    {
        private List<Guid> Users { get; set; }
        private Fixture Fixture { get; set; }

        public DataGenerator()
        {
            WebItemManager.Instance.LoadItems();
            CoreContext.TenantManager.SetCurrentTenant(0);
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            SecurityContext.AuthenticateMe(tenant.OwnerId);
            Users = CoreContext.UserManager.GetUsers().Select(r=> r.ID).ToList();
            Fixture = new Fixture();
        }

        private IPostprocessComposer<Project> GetProjectBuilder(Guid responsibleID)
        {
            return Fixture.Build<Project>()
                .Without(p => p.ID)
                .With(p => p.Private, false)
                .With(p=> p.CreateBy, responsibleID)
                .With(p=> p.CreateOn, DateTime.UtcNow)
                .With(p => p.Responsible, responsibleID)
                .With(p => p.Status, ProjectStatus.Open);
        }

        private IPostprocessComposer<Milestone> GetMilestoneBuilder(Project project)
        {
            return Fixture.Build<Milestone>()
                .Without(t => t.ID)
                .With(t => t.Project, project)
                .With(t => t.CreateBy, project.Responsible)
                .With(t => t.CreateOn, DateTime.UtcNow)
                .With(t => t.Status, MilestoneStatus.Open);
        }

        private IPostprocessComposer<Message> GetMessageBuilder(Project project)
        {
            return Fixture.Build<Message>()
                .Without(t => t.ID)
                .With(t => t.Project, project)
                .With(t => t.CreateOn, DateTime.UtcNow)
                .With(t => t.Status, MessageStatus.Open);
        }

        private IPostprocessComposer<Task> GetTaskBuilder(Project project)
        {
            return Fixture.Build<Task>()
                .Without(t => t.ID)
                .With(t => t.Project, project)
                .With(t => t.Status, TaskStatus.Open)
                .With(t => t.CreateOn, DateTime.UtcNow)
                .With(t => t.Links, new List<TaskLink>())
                .With(t => t.SubTasks, new List<Subtask>())
                .With(p => p.Responsibles, new List<Guid>());
        }

        private IPostprocessComposer<Subtask> GetSubTaskBuilder(Task task)
        {
            return Fixture.Build<Subtask>()
                .Without(t => t.ID)
                .With(t => t.Task, task.ID)
                .With(t => t.ParentTask, task)
                .With(t => t.Status, TaskStatus.Open)
                .With(t => t.CreateOn, DateTime.UtcNow);
        }

        private IPostprocessComposer<TimeSpend> GetTimeTrackingBuilder(Task task)
        {
            return Fixture.Build<TimeSpend>()
                .Without(t => t.ID)
                .With(t => t.Task, task)
                .With(t => t.Person, SecurityContext.CurrentAccount.ID)
                .With(t => t.CreateOn, DateTime.UtcNow)
                .With(t => t.PaymentStatus, PaymentStatus.NotChargeable);
        }

        public Project GenerateProject(Guid responsibleID)
        {
            return GetProjectBuilder(responsibleID).Create();
        }

        public Task GenerateTask(Project project)
        {
            return GetTaskBuilder(project).Create();
        }

        public Subtask GenerateSubtask(Task task)
        {
            return GetSubTaskBuilder(task).Create();
        }

        public Milestone GenerateMilestone(Project project)
        {
            return GetMilestoneBuilder(project).Create();
        }

        public Message GenerateMessage(Project project)
        {
            return GetMessageBuilder(project).Create();
        }

        public TimeSpend GenerateTimeTracking(Task task)
        {
            return GetTimeTrackingBuilder(task).Create();
        }
    }
}