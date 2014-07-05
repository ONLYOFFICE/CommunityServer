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
