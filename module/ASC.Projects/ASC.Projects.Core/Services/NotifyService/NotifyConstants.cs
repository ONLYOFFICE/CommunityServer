/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Usings

using ASC.Notify.Model;
using ASC.Notify.Patterns;

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
