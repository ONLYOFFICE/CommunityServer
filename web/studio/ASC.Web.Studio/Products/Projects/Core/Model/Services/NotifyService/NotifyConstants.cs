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


#region Usings

using ASC.Notify.Model;

#endregion

namespace ASC.Projects.Core.Services.NotifyService
{
    public static class NotifyConstants
    {
        public static INotifyAction Event_NewCommentForMessage = new NotifyAction("NewCommentForMessage", "new comment for message");
        public static INotifyAction Event_EditedCommentForMessage = new NotifyAction("EditedCommentForMessage", "Edited comment for message");

        public static INotifyAction Event_NewCommentForTask = new NotifyAction("NewCommentForTask", "new comment for task");
        public static INotifyAction Event_EditedCommentForTask = new NotifyAction("EditedCommentForTask", "Edited comment for task");

        public static INotifyAction Event_NewFileForDiscussion = new NotifyAction("NewFileForDiscussion", "new file for discussion");
        public static INotifyAction Event_NewFileForTask = new NotifyAction("NewFileForTask", "new file for task");

        public static INotifyAction Event_TaskEdited = new NotifyAction("TaskEdited", "task edited");
        public static INotifyAction Event_TaskClosed = new NotifyAction("TaskClosed", "task closed");
        public static INotifyAction Event_TaskCreated = new NotifyAction("TaskCreated", "task created");
        public static INotifyAction Event_TaskDeleted = new NotifyAction("TaskDeleted", "task deleted");
        public static INotifyAction Event_TaskResumed = new NotifyAction("TaskResumed", "task resumed");
        public static INotifyAction Event_TaskMovedToMilestone = new NotifyAction("TaskMovedToMilestone", "task moved to milestone");
        public static INotifyAction Event_TaskMovedFromMilestone = new NotifyAction("TaskMovedFromMilestone", "task moved from milestone");
        public static INotifyAction Event_MilestoneDeadline = new NotifyAction("MilestoneDeadline", "milestone deadline");
        public static INotifyAction Event_ResponsibleForProject = new NotifyAction("ResponsibleForProject", "responsible for project");
        public static INotifyAction Event_ResponsibleForTask = new NotifyAction("ResponsibleForTask", "responsible for task");
        public static INotifyAction Event_RemoveResponsibleForTask = new NotifyAction("RemoveResponsibleForTask", "remove responsible for task");
        public static INotifyAction Event_ReminderAboutTask = new NotifyAction("ReminderAboutTask", "reminder about task");
        public static INotifyAction Event_ReminderAboutTaskDeadline = new NotifyAction("ReminderAboutTaskDeadline", "reminder about task deadline");
        public static INotifyAction Event_InviteToProject = new NotifyAction("InviteToProject", "invite to project");
        public static INotifyAction Event_RemoveFromProject = new NotifyAction("RemoveFromProject", "remove from project");

        public static INotifyAction Event_MilestoneCreated = new NotifyAction("MilestoneCreated", "milestone created");
        public static INotifyAction Event_MilestoneEdited = new NotifyAction("MilestoneEdited", "milestone edited");
        public static INotifyAction Event_MilestoneClosed = new NotifyAction("MilestoneClosed", "milestone closed");
        public static INotifyAction Event_MilestoneDeleted = new NotifyAction("MilestoneDeleted", "milestone deleted");
        public static INotifyAction Event_MilestoneResumed = new NotifyAction("MilestoneResumed", "milestone resumed");
        public static INotifyAction Event_ResponsibleForMilestone = new NotifyAction("ResponsibleForMilestone", "responsible for milestone");

        public static INotifyAction Event_MessageCreated = new NotifyAction("NewMessage", "message created");
        public static INotifyAction Event_MessageEdited = new NotifyAction("EditMessage", "message edited");
        public static INotifyAction Event_MessageDeleted = new NotifyAction("MessageDeleted", "message deleted");

        public static INotifyAction Event_ImportData = new NotifyAction("ImportData", "import data");

        public static INotifyAction Event_ResponsibleForSubTask = new NotifyAction("ResponsibleForSubTask", "responsible for subtask");
        public static INotifyAction Event_SubTaskCreated = new NotifyAction("SubTaskCreated", "subtask created");
        public static INotifyAction Event_SubTaskEdited = new NotifyAction("SubTaskEdited", "subtask edited");
        public static INotifyAction Event_SubTaskResumed = new NotifyAction("SubTaskResumed", "subtask resumed");
        public static INotifyAction Event_SubTaskClosed = new NotifyAction("SubTaskClosed", "subtask closed");
        public static INotifyAction Event_SubTaskDeleted = new NotifyAction("SubTaskDeleted", "subtask deleted");

        public static INotifyAction Event_ProjectDeleted = new NotifyAction("ProjectDeleted", "project deleted");

        public static string Tag_ProjectTitle = "ProjectTitle";
        public static string Tag_ProjectID = "ProjectID";
        public static string Tag_AdditionalData = "AdditionalData";
        public static string Tag_EntityID = "EntityID";
        public static string Tag_EntityTitle = "EntityTitle";
        public static string Tag_SubEntityTitle = "SubEntityTitle";
        public static string Tag_EventType = "EventType";
        public static string Tag_Responsible = "Responsible";
        public static string Tag_Priority = "Priority";
    }
}
