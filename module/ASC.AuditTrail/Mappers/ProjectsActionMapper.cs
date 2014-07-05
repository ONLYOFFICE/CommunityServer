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

using System.Collections.Generic;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    internal class ProjectsActionsMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    #region projects

                    {
                        MessageAction.ProjectCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ProjectCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectCreatedFromTemplate, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ProjectCreatedFromTemplate",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ProjectUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ProjectUpdatedStatus",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectFollowed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "FollowActionType",
                                ActionTextResourceName = "ProjectFollowed",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUnfollowed, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnfollowActionType",
                                ActionTextResourceName = "ProjectUnfollowed",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ProjectDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },

                    #endregion
                    
                    #region team 

                    {
                        MessageAction.ProjectDeletedMember, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ProjectDeletedMember",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUpdatedTeam, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ProjectUpdatedTeam",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUpdatedMemberRights, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "ProjectUpdatedMemberRights",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },

                    #endregion

                    #region contacts

                    {
                        MessageAction.ProjectLinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "ProjectLinkedCompany",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUnlinkedCompany, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "ProjectUnlinkedCompany",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectLinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "ProjectLinkedPerson",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectUnlinkedPerson, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "ProjectUnlinkedPerson",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },
                    {
                        MessageAction.ProjectLinkedContacts, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "ProjectLinkedContacts",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },

                    #endregion

                    #region milestones

                    {
                        MessageAction.MilestoneCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "MilestoneCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "MilestonesModule"
                            }
                    },
                    {
                        MessageAction.MilestoneUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "MilestoneUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "MilestonesModule"
                            }
                    },
                    {
                        MessageAction.MilestoneUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "MilestoneUpdatedStatus",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "MilestonesModule"
                            }
                    },
                    {
                        MessageAction.MilestoneDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "MilestoneDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "MilestonesModule"
                            }
                    },

                    #endregion

                    #region tasks

                    {
                        MessageAction.TaskCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "TaskCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskCreatedFromDiscussion, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "TaskCreatedFromDiscussion",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskUpdatedStatus",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskMovedToMilestone, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskMovedToMilestone",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskUnlinkedMilestone, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskUnlinkedMilestone",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskUpdatedFollowing, new MessageMaps
                            {
                                ActionTypeTextResourceName = "FollowActionType",
                                ActionTextResourceName = "TaskUpdatedFollowing",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "TaskAttachedFiles",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "TaskDetachedFile",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TasksLinked, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "TasksLinked",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TasksUnlinked, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "TasksUnlinked",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "TaskDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskCommentCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "TaskCommentCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskCommentUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskCommentUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.TaskCommentDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "TaskCommentDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },

                    #endregion

                    #region subtasks

                    {
                        MessageAction.SubtaskCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "SubtaskCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.SubtaskUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "SubtaskUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.SubtaskUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "SubtaskUpdatedStatus",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },
                    {
                        MessageAction.SubtaskDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "SubtaskDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TasksModule"
                            }
                    },

                    #endregion

                    #region discussions

                    {
                        MessageAction.DiscussionCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "DiscussionCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DiscussionUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionUpdatedFollowing, new MessageMaps
                            {
                                ActionTypeTextResourceName = "FollowActionType",
                                ActionTextResourceName = "DiscussionUpdatedFollowing",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionAttachedFiles, new MessageMaps
                            {
                                ActionTypeTextResourceName = "AttachActionType",
                                ActionTextResourceName = "DiscussionAttachedFiles",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionDetachedFile, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DetachActionType",
                                ActionTextResourceName = "DiscussionDetachedFile",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "DiscussionDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionCommentCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "DiscussionCommentCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionCommentUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DiscussionCommentUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },
                    {
                        MessageAction.DiscussionCommentDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "DiscussionCommentDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "DiscussionsModule"
                            }
                    },

                    #endregion

                    #region time tracking

                    {
                        MessageAction.TaskTimeCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "TaskTimeCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TimeTrackingModule"
                            }
                    },
                    {
                        MessageAction.TaskTimeUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskTimeUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TimeTrackingModule"
                            }
                    },
                    {
                        MessageAction.TaskTimesUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TaskTimesUpdatedStatus",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TimeTrackingModule"
                            }
                    },
                    {
                        MessageAction.TaskTimesDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "TaskTimesDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "TimeTrackingModule"
                            }
                    },

                    #endregion

                    #region reports

                    {
                        MessageAction.ReportTemplateCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ReportTemplateCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ReportsModule"
                            }
                    },
                    {
                        MessageAction.ReportTemplateUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ReportTemplateUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ReportsModule"
                            }
                    },
                    {
                        MessageAction.ReportTemplateDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ReportTemplateDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ReportsModule"
                            }
                    },

                    #endregion
 
                    #region settings
                    
                    {
                        MessageAction.ProjectTemplateCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ProjectTemplateCreated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ProjectTemplateUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ProjectTemplateUpdated",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ProjectTemplateDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ProjectTemplateDeleted",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsSettingsModule"
                            }
                    },
                    {
                        MessageAction.ProjectsImportedFromBasecamp, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "ProjectsImportedFromBasecamp",
                                ProductResourceName = "ProjectsProduct",
                                ModuleResourceName = "ProjectsModule"
                            }
                    },

                    #endregion
                };
        }
    }
}