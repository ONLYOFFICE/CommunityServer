/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using NotifySourceBase = ASC.Core.Notify.NotifySource;

namespace ASC.Projects.Core.Services.NotifyService
{
    public class NotifySource : NotifySourceBase
    {
        public static NotifySource Instance
        {
            get;
            private set;
        }


        static NotifySource()
        {
            Instance = new NotifySource();
        }

        private NotifySource()
            : base(new Guid("{6045B68C-2C2E-42db-9E53-C272E814C4AD}"))
        {
        }


        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                NotifyConstants.Event_NewCommentForMessage,
                NotifyConstants.Event_NewCommentForTask,
                NotifyConstants.Event_NewFileForDiscussion,
                NotifyConstants.Event_NewFileForTask,
                NotifyConstants.Event_ResponsibleForProject,
                NotifyConstants.Event_ResponsibleForTask,
                NotifyConstants.Event_RemoveResponsibleForTask,
                NotifyConstants.Event_ReminderAboutTask,
                NotifyConstants.Event_ReminderAboutTaskDeadline,
                NotifyConstants.Event_TaskEdited,
                NotifyConstants.Event_TaskClosed,
                NotifyConstants.Event_TaskCreated,
                NotifyConstants.Event_TaskResumed,
                NotifyConstants.Event_TaskDeleted,
                NotifyConstants.Event_TaskMovedToMilestone,
                NotifyConstants.Event_TaskMovedFromMilestone,
                NotifyConstants.Event_InviteToProject,
                NotifyConstants.Event_RemoveFromProject,
                NotifyConstants.Event_MilestoneDeadline,
                NotifyConstants.Event_MilestoneEdited,
                NotifyConstants.Event_MilestoneClosed,
                NotifyConstants.Event_MilestoneCreated,
                NotifyConstants.Event_MilestoneDeleted,
                NotifyConstants.Event_MilestoneResumed,
                NotifyConstants.Event_ResponsibleForMilestone,
                NotifyConstants.Event_MessageCreated,
                NotifyConstants.Event_MessageEdited,
                NotifyConstants.Event_MessageDeleted,
                NotifyConstants.Event_ImportData,
                NotifyConstants.Event_ResponsibleForSubTask,
                NotifyConstants.Event_SubTaskDeleted,
                NotifyConstants.Event_SubTaskCreated,
                NotifyConstants.Event_SubTaskEdited,
                NotifyConstants.Event_SubTaskClosed,
                NotifyConstants.Event_SubTaskResumed,
                NotifyConstants.Event_ProjectDeleted,
                NotifyConstants.Event_EditedCommentForMessage,
                NotifyConstants.Event_EditedCommentForTask);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(Resources.ProjectsPatternResource.patterns) { GetPatternMethod = ChoosePattern };
        }

        private IPattern ChoosePattern(INotifyAction action, string senderName, Notify.Engine.NotifyRequest request)
        {
            if (action == NotifyConstants.Event_NewCommentForMessage)
            {
                var tag = request.Arguments.Find(tv => tv.Tag == "EventType");
                if (tag != null)
                {
                    return PatternProvider.GetPattern(new NotifyAction(Convert.ToString(tag.Value)), senderName);
                }
            }
            return null;
        }
    }
}
