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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Core.Common.Notify.Push;

using ASC.Notify;
using ASC.Notify.Engine;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using ASC.Web.Projects.Resources;
using Autofac;

namespace ASC.Projects.Core.Services.NotifyService
{
    public class NotifyClient
    {
        private static NotifyClient instance;
        private readonly INotifyClient client;
        private readonly INotifySource source;
        private static bool registered;
        private static readonly object Locker = new object();

        public static NotifyClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(NotifyClient))
                    {
                        return instance ?? (instance = new NotifyClient(WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource.Instance), NotifySource.Instance));
                    }
                }
                return instance;
            }
        }

        private INotifyClient Client
        {
            get { return client; }
        }


        private NotifyClient(INotifyClient client, INotifySource source)
        {
            this.client = client;
            this.source = source;
        }

        public static void RegisterSecurityInterceptor()
        {
            var securityInterceptor = new SendInterceptorSkeleton(
            "ProjectInterceptorSecurity",
            InterceptorPlace.DirectSend,
            InterceptorLifetime.Global,
            (r, p) =>
            {
                try
                {
                    using (var scope = DIHelper.Resolve())
                    {
                        var projectSecurity = scope.Resolve<ProjectSecurity>();
                        var factory = scope.Resolve<EngineFactory>();
                        var data = r.ObjectID.Split('_');
                        var entityType = data[0];
                        var entityId = Convert.ToInt32(data[1]);

                        var projectId = 0;

                        if (data.Length == 3)
                            projectId = Convert.ToInt32(r.ObjectID.Split('_')[2]);

                        switch (entityType)
                        {
                            case "Task":
                                var task = factory.TaskEngine.GetByID(entityId, false);

                                if (task == null && projectId != 0)
                                {
                                    var project = factory.ProjectEngine.GetByID(projectId, false);
                                    return !projectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                }

                                return !projectSecurity.CanRead(task, new Guid(r.Recipient.ID));
                            case "Message":
                                var discussion = factory.MessageEngine.GetByID(entityId, false);

                                if (discussion == null && projectId != 0)
                                {
                                    var project = factory.ProjectEngine.GetByID(projectId, false);
                                    return !projectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                }

                                return !projectSecurity.CanRead(discussion, new Guid(r.Recipient.ID));
                            case "Milestone":
                                var milestone = factory.MilestoneEngine.GetByID(entityId, false);

                                if (milestone == null && projectId != 0)
                                {
                                    var project = factory.ProjectEngine.GetByID(projectId, false);
                                    return !projectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                }

                                return !projectSecurity.CanRead(milestone, new Guid(r.Recipient.ID));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC").Error("Send", ex);
                }
                return false;
            });

            Instance.Client.AddInterceptor(securityInterceptor);
        }

        public static void RegisterSendMethods()
        {
            if (!registered)
            {
                lock (Locker)
                {
                    if (!registered)
                    {
                        Instance.Client.RegisterSendMethod(NotifyHelper.SendMsgMilestoneDeadline, "0 0 7 ? * *")
                            .RegisterSendMethod(NotifyHelper.SendAutoReports, "0 0 * ? * *")
                            .RegisterSendMethod(NotifyHelper.SendAutoReminderAboutTask, "0 0 * ? * *");
                        registered = true;
                    }
                }
            }
        }

        public static void UnregisterSendMethods()
        {
            if (registered)
            {
                Instance.Client.UnregisterSendMethod(NotifyHelper.SendMsgMilestoneDeadline)
                               .UnregisterSendMethod(NotifyHelper.SendAutoReports)
                               .UnregisterSendMethod(NotifyHelper.SendAutoReminderAboutTask);

                Instance.Client.RemoveInterceptor("ProjectInterceptorSecurity");
            }
        }

        public void SendInvaiteToProjectTeam(Guid userId, Project project)
        {
            var recipient = ToRecipient(userId);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_InviteToProject,
                    project.UniqID,
                    new[] {recipient},
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    ReplyToTagProvider.Message(project.ID),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Project, project.ID.ToString(CultureInfo.InvariantCulture), project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.InvitedTo));
            }
        }

        public void SendRemovingFromProjectTeam(Guid userId, Project project)
        {
            var recipient = ToRecipient(userId);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_RemoveFromProject,
                    project.UniqID,
                    new[] { recipient },
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    ReplyToTagProvider.Message(project.ID));
            }
        }

        public void SendMilestoneDeadline(Guid userID, Milestone milestone)
        {
            var recipient = ToRecipient(userID);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneDeadline,
                    milestone.NotifyId,
                    new[] { recipient },
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_Priority, 1),
                    ReplyToTagProvider.Comment("project.milestone", milestone.ID.ToString(CultureInfo.InvariantCulture)));
            }
        }


        public void SendAboutResponsibleByProject(Guid responsible, Project project)
        {
            var recipient = ToRecipient(responsible);
            var description = !string.IsNullOrEmpty(project.Description) ? HttpUtility.HtmlEncode(project.Description) : "";
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ResponsibleForProject,
                    project.UniqID,
                    new[] {recipient},
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    ReplyToTagProvider.Message(project.ID),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Project, project.ID.ToString(CultureInfo.InvariantCulture), project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Assigned));
            }
        }

        public void SendAboutResponsibleByMilestone(Milestone milestone)
        {
            var description = !string.IsNullOrEmpty(milestone.Description) ? HttpUtility.HtmlEncode(milestone.Description) : "";
            var recipient = ToRecipient(milestone.Responsible);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ResponsibleForMilestone,
                    milestone.NotifyId,
                    new[] {recipient},
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "MilestoneDescription", description } }),
                    ReplyToTagProvider.Comment("project.milestone", milestone.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Milestone, milestone.ID.ToString(CultureInfo.InvariantCulture), milestone.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, milestone.Project.ID.ToString(CultureInfo.InvariantCulture), milestone.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Assigned));
            }
        }

        public void SendAboutResponsibleByTask(IEnumerable<Guid> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ResponsibleForTask,
                    task.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, task.Project.ID.ToString(CultureInfo.InvariantCulture), task.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Assigned));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutRemoveResponsibleByTask(IEnumerable<Guid> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_RemoveResponsibleForTask,
                    task.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutResponsibleBySubTask(Subtask subtask, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                var recipient = ToRecipient(subtask.Responsible);
                if (recipient != null)
                {
                    client.SendNoticeToAsync(
                        NotifyConstants.Event_ResponsibleForSubTask,
                        task.NotifyId,
                        new[] { recipient },
                        true,
                        new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                        new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                        new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                        new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                        new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                        new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                        ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                        new AdditionalSenderTag("push.sender"),
                        new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Subtask, subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title)),
                        new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                        new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                        new TagValue(PushConstants.PushActionTagName, PushAction.Assigned));
                }
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        public void SendReminderAboutTask(IEnumerable<Guid> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            client.SendNoticeToAsync(
                NotifyConstants.Event_ReminderAboutTask,
                task.NotifyId,
                recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                true,
                new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                new TagValue(NotifyConstants.Tag_Priority, 1),
                ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
        }

        public void SendReminderAboutTaskDeadline(IEnumerable<Guid> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            client.SendNoticeToAsync(
                NotifyConstants.Event_ReminderAboutTaskDeadline,
                task.NotifyId,
                recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                true,
                new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                new TagValue(NotifyConstants.Tag_AdditionalData,
                             new Hashtable
                                 {
                                     {"TaskDescription", description},
                                     {"TaskDeadline", task.Deadline.ToString(CultureInfo.InvariantCulture)}
                                 }),
                new TagValue(NotifyConstants.Tag_Priority, 1),
                ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
        }


        public void SendNewComment(List<IRecipient> recipients, ProjectEntity entity, Comment comment, bool isNew)
        {
            INotifyAction action;
            if (entity.GetType() == typeof(Message)) action = isNew ? NotifyConstants.Event_NewCommentForMessage : NotifyConstants.Event_EditedCommentForMessage;
            else if (entity.GetType() == typeof(Task)) action = isNew ? NotifyConstants.Event_NewCommentForTask : NotifyConstants.Event_EditedCommentForTask;
            else return;

            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeToAsync(
                    action,
                    entity.NotifyId,
                    recipients.ToArray(), 
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, entity.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, entity.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, entity.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, entity.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, comment.Content),
                    GetReplyToEntityTag(entity, comment));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendNewFile(List<IRecipient> recipients, ProjectEntity entity, string fileTitle)
        {
            INotifyAction action;
            if (entity.GetType() == typeof(Message)) action = NotifyConstants.Event_NewFileForDiscussion;
            else if (entity.GetType() == typeof(Task)) action = NotifyConstants.Event_NewFileForTask;
            else return;

            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeToAsync(
                    action,
                    entity.NotifyId,
                    recipients.ToArray(), 
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, entity.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, entity.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, entity.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, entity.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, fileTitle));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        public void SendAboutMilestoneClosing(IEnumerable<Guid> recipients, Milestone milestone)
        {
            var description = !string.IsNullOrEmpty(milestone.Description) ? HttpUtility.HtmlEncode(milestone.Description) : "";

            client.BeginSingleRecipientEvent("milestone closed");
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneClosed,
                    milestone.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Milestone, milestone.ID.ToString(CultureInfo.InvariantCulture), milestone.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, milestone.Project.ID.ToString(CultureInfo.InvariantCulture), milestone.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Closed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
                client.EndSingleRecipientEvent("milestone closed");
            }
        }

        public void SendAboutTaskClosing(IEnumerable<IRecipient> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            client.BeginSingleRecipientEvent("task closed");
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_TaskClosed,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, task.Project.ID.ToString(CultureInfo.InvariantCulture), task.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Closed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
                client.EndSingleRecipientEvent("task closed");
            }
        }

        public void SendAboutSubTaskClosing(List<IRecipient> recipients, Task task, Subtask subtask)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_SubTaskClosed,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Subtask, subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Closed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        public void SendAboutTaskDeleting(List<IRecipient> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_TaskDeleted,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, task.Project.ID.ToString(CultureInfo.InvariantCulture), task.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Deleted));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutSubTaskDeleting(List<IRecipient> recipients, Task task, Subtask subtask)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_SubTaskDeleted,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Subtask, subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Deleted));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutMilestoneDeleting(IEnumerable<Guid> recipients, Milestone milestone)
        {
            var description = !string.IsNullOrEmpty(milestone.Description) ? HttpUtility.HtmlEncode(milestone.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneDeleted,
                    milestone.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Milestone, milestone.ID.ToString(CultureInfo.InvariantCulture), milestone.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, milestone.Project.ID.ToString(CultureInfo.InvariantCulture), milestone.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Deleted));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutMessageDeleting(List<IRecipient> recipients, Message message)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MessageDeleted,
                    message.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, message.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, message.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, message.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, message.ID),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Message, message.ID.ToString(CultureInfo.InvariantCulture), message.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, message.Project.ID.ToString(CultureInfo.InvariantCulture), message.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Deleted));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutProjectDeleting(IEnumerable<Guid> recipients, Project project)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ProjectDeleted,
                    project.UniqID,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_EntityTitle, project.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, project.ID),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Project, project.ID.ToString(CultureInfo.InvariantCulture), project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Deleted));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        public void SendAboutMilestoneCreating(IEnumerable<Guid> recipients, Milestone milestone)
        {
            var description = !string.IsNullOrEmpty(milestone.Description) ? HttpUtility.HtmlEncode(milestone.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneCreated,
                    milestone.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "MilestoneDescription", description } }),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Milestone, milestone.ID.ToString(CultureInfo.InvariantCulture), milestone.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, milestone.Project.ID.ToString(CultureInfo.InvariantCulture), milestone.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Created));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutTaskCreating(List<IRecipient> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var resp = "Nobody";

            if (task.Responsibles.Count != 0)
            {
                var recip = task.Responsibles.Distinct().Select(ToRecipient).Where(r => r != null).ToList();
                resp = recip.Select(r => r.Name).Aggregate((a, b) => a + ", " + b);
            }
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_TaskCreated,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_Responsible, resp),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", description } }),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, task.Project.ID.ToString(CultureInfo.InvariantCulture), task.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Created));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutSubTaskCreating(List<IRecipient> recipients, Task task, Subtask subtask)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_SubTaskCreated,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_Responsible,
                                 !subtask.Responsible.Equals(Guid.Empty) ? ToRecipient(subtask.Responsible).Name : PatternResource.subtaskWithoutResponsible),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Subtask, subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Created));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutMilestoneEditing(Milestone milestone)
        {
            var recipient = ToRecipient(milestone.Responsible);

            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneEdited,
                    milestone.NotifyId,
                    new[] { recipient },
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID));
            }
        }

        public void SendAboutTaskEditing(List<IRecipient> recipients, Task task)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_TaskEdited,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutSubTaskEditing(List<IRecipient> recipients, Task task, Subtask subtask)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_SubTaskEdited,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_Responsible,
                                 !subtask.Responsible.Equals(Guid.Empty)
                                     ? ToRecipient(subtask.Responsible).Name
                                     : PatternResource.subtaskWithoutResponsible),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        public void SendAboutMilestoneResumed(IEnumerable<Guid> recipients, Milestone milestone)
        {
            var description = !string.IsNullOrEmpty(milestone.Description) ? HttpUtility.HtmlEncode(milestone.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneResumed,
                    milestone.NotifyId,
                    recipients.Select(ToRecipient).Where(r => r != null).ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Milestone, milestone.ID.ToString(CultureInfo.InvariantCulture), milestone.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, milestone.Project.ID.ToString(CultureInfo.InvariantCulture), milestone.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Resumed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutTaskResumed(List<IRecipient> recipients, Task task)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";     
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeToAsync(
                    NotifyConstants.Event_TaskResumed,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, task.Project.ID.ToString(CultureInfo.InvariantCulture), task.Project.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Resumed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutSubTaskResumed(List<IRecipient> recipients, Task task, Subtask subtask)
        {
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);

            try
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_SubTaskResumed,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, subtask.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)),
                    new AdditionalSenderTag("push.sender"),
                    new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Subtask, subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title)),
                    new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Task, task.ID.ToString(CultureInfo.InvariantCulture), task.Title)),
                    new TagValue(PushConstants.PushModuleTagName, PushModule.Projects),
                    new TagValue(PushConstants.PushActionTagName, PushAction.Resumed));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutTaskRemoved(List<IRecipient> recipients, Task task, Milestone milestone, bool newMilestone)
        {
            var description = !string.IsNullOrEmpty(task.Description) ? HttpUtility.HtmlEncode(task.Description) : "";
            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                client.SendNoticeToAsync(newMilestone ? NotifyConstants.Event_TaskMovedToMilestone : NotifyConstants.Event_TaskMovedFromMilestone,
                    task.NotifyId,
                    recipients.ToArray(),
                    true,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, milestone != null ? milestone.Title : TaskResource.None),
                    new TagValue(NotifyConstants.Tag_AdditionalData, description),
                    ReplyToTagProvider.Comment("project.task", task.ID.ToString(CultureInfo.InvariantCulture)));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }


        private static TagValue GetReplyToEntityTag(ProjectEntity entity, Comment comment)
        {
            string type = string.Empty;
            if (entity is Task)
            {
                type = "project.task";
            }
            if (entity is Message)
            {
                type = "project.message";
            }
            if (entity is Milestone)
            {
                type = "project.milestone";
            }
            if (!string.IsNullOrEmpty(type))
            {
                return ReplyToTagProvider.Comment(type, entity.ID.ToString(CultureInfo.InvariantCulture), comment != null ? comment.ID.ToString() : null);
            }
            return null;
        }

        public void SendAboutMessageAction(List<IRecipient> recipients, Message message, bool isNew, List<Tuple<string, string>> fileListInfoHashtable)
        {
            var tags = new List<ITagValue>
                {
                    new TagValue(NotifyConstants.Tag_ProjectID, message.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, message.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, message.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, message.ID),
                    new TagValue(NotifyConstants.Tag_EventType, isNew ? NotifyConstants.Event_MessageCreated.ID : NotifyConstants.Event_MessageEdited.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable {{"MessagePreview", message.Description}, {"Files", fileListInfoHashtable}}),
                    ReplyToTagProvider.Comment("project.message", message.ID.ToString(CultureInfo.InvariantCulture))
                };

            if (isNew) //don't send push about edited message!
            {
                tags.Add(new AdditionalSenderTag("push.sender"));
                tags.Add(new TagValue(PushConstants.PushItemTagName, new PushItem(PushItemType.Message, message.ID.ToString(CultureInfo.InvariantCulture), message.Title)));
                tags.Add(new TagValue(PushConstants.PushParentItemTagName, new PushItem(PushItemType.Project, message.Project.ID.ToString(CultureInfo.InvariantCulture), message.Project.Title)));
                tags.Add(new TagValue(PushConstants.PushModuleTagName, PushModule.Projects));
                tags.Add(new TagValue(PushConstants.PushActionTagName, PushAction.Created));
            }

            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeToAsync(
                    isNew ? NotifyConstants.Event_MessageCreated : NotifyConstants.Event_MessageEdited,
                    message.NotifyId,
                    recipients.ToArray(),
                    true,
                    tags.ToArray());
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendAboutImportComplite(Guid user)
        {
            var recipient = ToRecipient(user);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ImportData, 
                    null,
                    new[] { recipient },
                    true);
            }
        }

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }
    }
}