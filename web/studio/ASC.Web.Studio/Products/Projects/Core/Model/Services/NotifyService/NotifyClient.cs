/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Core.Common.Notify.Push;

using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Core.Notify;

namespace ASC.Projects.Core.Services.NotifyService
{
    public class NotifyClient
    {
        private static NotifyClient instance;
        private readonly INotifyClient client;
        private readonly INotifySource source;

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

        public INotifyClient Client
        {
            get { return client; }
        }


        private NotifyClient(INotifyClient client, INotifySource source)
        {
            this.client = client;
            this.source = source;
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
                var recip = task.Responsibles.Distinct().Select(ToRecipient).Where(r => r != null);
                resp = recip.Select(r => r.Name).Aggregate(string.Empty, (a, b) => a + ", " + b);
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
                    new TagValue(NotifyConstants.Tag_SubEntityTitle, milestone.Title),
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

        public void SendAboutMessageAction(List<IRecipient> recipients, Message message, bool isNew, Hashtable fileListInfoHashtable)
        {
            var tags = new List<ITagValue>
                {
                    new TagValue(NotifyConstants.Tag_ProjectID, message.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, message.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, message.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, message.ID),
                    new TagValue(NotifyConstants.Tag_EventType, isNew ? NotifyConstants.Event_MessageCreated.ID : NotifyConstants.Event_MessageEdited.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable {{"MessagePreview", message.Content}, {"Files", fileListInfoHashtable}}),
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