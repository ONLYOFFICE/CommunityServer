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
using System.Collections.Generic;
using ASC.AuditTrail.Model;
using ASC.MessagingSystem.Contracts;
using System.Linq;

namespace ASC.AuditTrail.Data
{
    public class AuditActionsMapper
    {
        private static readonly Dictionary<MessageAction, MessageMaps> actions = new Dictionary
            <MessageAction, MessageMaps>
            {
                #region login
                
                {MessageAction.LoginSuccess, new MessageMaps {ActionText = AuditReportResource.LoginSuccess}},
                {MessageAction.LoginSuccessThirdParty, new MessageMaps {ActionText = AuditReportResource.LoginSuccessThirdParty}},
                {MessageAction.LoginFail, new MessageMaps {ActionText = AuditReportResource.LoginFail}},
                {MessageAction.LoginFailInvalidCombination, new MessageMaps {ActionText = AuditReportResource.LoginFailInvalidCombination}},
                {MessageAction.LoginFailThirdPartyNotFound, new MessageMaps {ActionText = AuditReportResource.LoginFailThirdPartyNotFound}},
                {MessageAction.LoginFailDisabledProfile, new MessageMaps {ActionText = AuditReportResource.LoginFailDisabledProfile}},
                {MessageAction.Logout, new MessageMaps {ActionText = AuditReportResource.Logout}},

                #endregion

                #region projects
                
                {
                    MessageAction.ProjectCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ProjectCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectCreatedFromTemplate, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ProjectCreatedFromTemplate,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProjectUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProjectUpdatedStatus,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ProjectDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectFollowed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.FollowActionType,
                            ActionText = AuditReportResource.ProjectFollowed,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUnfollowed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnfollowActionType,
                            ActionText = AuditReportResource.ProjectUnfollowed,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectExcludedMember, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProjectExcludedMember,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUpdatedMembers, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProjectUpdatedMembers,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUpdatedMemberRights, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.ProjectUpdatedMemberRights,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.MilestoneCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.MilestoneCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.MilestonesModule
                        }
                },
                {
                    MessageAction.MilestoneUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.MilestoneUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.MilestonesModule
                        }
                },
                {
                    MessageAction.MilestoneUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.MilestoneUpdatedStatus,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.MilestonesModule
                        }
                },
                {
                    MessageAction.MilestoneDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.MilestoneDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.MilestonesModule
                        }
                },
                {
                    MessageAction.TaskCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.TaskCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskCreatedFromDiscussion, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.TaskCreatedFromDiscussion,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskUpdatedStatus,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskMovedToMilestone, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskMovedToMilestone,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.TaskDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskUpdatedFollowing, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.FollowActionType,
                            ActionText = AuditReportResource.TaskUpdatedFollowing,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskCommentCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.TaskCommentCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskCommentUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskCommentUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskCommentDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.TaskCommentDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TasksLinked, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.TasksLinked,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TasksUnlinked, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.TasksUnlinked,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.TaskAttachedFiles,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskDetachedFile, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DetachActionType,
                            ActionText = AuditReportResource.TaskDetachedFile,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskUnlinkedMilestone, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskUnlinkedMilestone,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.SubtaskCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.SubtaskCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.SubtaskUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.SubtaskUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.SubtaskUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.SubtaskUpdatedStatus,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.SubtaskDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.SubtaskDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TasksModule
                        }
                },
                {
                    MessageAction.TaskTimeCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.TaskTimeCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TimeTrackingModule
                        }
                },
                {
                    MessageAction.TaskTimeUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskTimeUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TimeTrackingModule
                        }
                },
                {
                    MessageAction.TaskTimesUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TaskTimesUpdatedStatus,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TimeTrackingModule
                        }
                },
                {
                    MessageAction.TaskTimesDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.TaskTimesDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TimeTrackingModule
                        }
                },
                {
                    MessageAction.DiscussionCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.DiscussionCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DiscussionUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.DiscussionDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionUpdatedFollowing, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.FollowActionType,
                            ActionText = AuditReportResource.DiscussionUpdatedFollowing,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionCommentCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.DiscussionCommentCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionCommentUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DiscussionCommentUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionCommentDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.DiscussionCommentDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.DiscussionAttachedFiles,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.DiscussionDetachedFile, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DetachActionType,
                            ActionText = AuditReportResource.DiscussionDetachedFile,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.DiscussionsModule
                        }
                },
                {
                    MessageAction.ProjectTemplateCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ProjectTemplateCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TemplatesModule
                        }
                },
                {
                    MessageAction.ProjectTemplateUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProjectTemplateUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TemplatesModule
                        }
                },
                {
                    MessageAction.ProjectTemplateDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ProjectTemplateDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.TemplatesModule
                        }
                },
                {
                    MessageAction.ReportTemplateCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ReportTemplateCreated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ReportsModule
                        }
                },
                {
                    MessageAction.ReportTemplateUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ReportTemplateUpdated,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ReportsModule
                        }
                },
                {
                    MessageAction.ReportTemplateDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ReportTemplateDeleted,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ReportsModule
                        }
                },
                {
                    MessageAction.ProjectLinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.ProjectLinkedCompany,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUnlinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.ProjectUnlinkedCompany,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectLinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.ProjectLinkedPerson,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectUnlinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.ProjectUnlinkedPerson,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectLinkedContacts, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.ProjectLinkedContacts,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },
                {
                    MessageAction.ProjectsImportedFromBasecamp, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.ProjectsImportedFromBasecamp,
                            Product = AuditReportResource.ProjectsProduct,
                            Module = AuditReportResource.ProjectsModule
                        }
                },

                #endregion

                #region crm

                #region companies

                {
                    MessageAction.CompanyCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CompanyCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompanyUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompanyUpdatedStatus,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUpdatedPersonsStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompanyUpdatedPersonsStatus,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUpdatedPrincipalInfo, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompanyUpdatedPrincipalInfo,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUpdatedPhoto, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompanyUpdatedPhoto,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyLinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.CompanyLinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUnlinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.CompanyUnlinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompaniesMerged, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompaniesMerged,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CompanyCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyCreatedPersonsTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CompanyCreatedPersonsTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CompanyDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyCreatedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CompanyCreatedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyDeletedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CompanyDeletedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyLinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.CompanyLinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUnlinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.CompanyUnlinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyUploadedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.CompanyUploadedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.CompanyAttachedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },
                {
                    MessageAction.CompanyDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CompanyDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CompaniesModule
                        }
                },

                #endregion

                #region persons
                
                {
                    MessageAction.PersonCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonsCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonsCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonUpdatedStatus,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUpdatedCompanyStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonUpdatedCompanyStatus,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUpdatedPrincipalInfo, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonUpdatedPrincipalInfo,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUpdatedPhoto, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonUpdatedPhoto,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonLinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.PersonLinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUnlinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.PersonUnlinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonsMerged, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonsMerged,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonCreatedCompanyTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonCreatedCompanyTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.PersonDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonCreatedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonCreatedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonDeletedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.PersonDeletedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonUploadedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.PersonUploadedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.PersonAttachedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },
                {
                    MessageAction.PersonDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.PersonDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.PersonsModule
                        }
                },

                #endregion

                #region contacts
                
                {
                    MessageAction.ContactLinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.ContactLinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsModule
                        }
                },
                {
                    MessageAction.ContactUnlinkedProject, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.ContactUnlinkedProject,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsModule
                        }
                },
                {
                    MessageAction.ContactsDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ContactsDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsModule
                        }
                },

                #endregion

                #region tasks

                {
                    MessageAction.CrmTaskCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CrmTaskCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.CrmTaskUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.CrmTaskOpened, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskOpened,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.CrmTaskClosed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskClosed,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.CrmTaskDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CrmTaskDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.ContactsCreatedCrmTasks, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ContactsCreatedCrmTasks,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },

                #endregion

                #region opportunities

                {
                    MessageAction.OpportunityCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunityCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunityUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityUpdatedStage, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunityUpdatedStage,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunityCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunityDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityCreatedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunityCreatedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityDeletedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunityDeletedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityLinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.OpportunityLinkedCompany,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityUnlinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.OpportunityUnlinkedCompany,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityLinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.OpportunityLinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityUnlinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.OpportunityUnlinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityUploadedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.OpportunityUploadedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.OpportunityAttachedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityOpenedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.OpportunityOpenedAccess,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityRestrictedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.OpportunityRestrictedAccess,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunityDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunityDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.OpportunitiesDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunitiesDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },

                #endregion

                #region cases

                {
                    MessageAction.CaseCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CaseCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CaseUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseOpened, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CaseOpened,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseClosed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CaseClosed,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CaseCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CaseDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseCreatedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CaseCreatedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseDeletedHistory, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CaseDeletedHistory,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseLinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.CaseLinkedCompany,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseUnlinkedCompany, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.CaseUnlinkedCompany,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseLinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.CaseLinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseUnlinkedPerson, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.CaseUnlinkedPerson,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseUploadedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.CaseUploadedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseAttachedFiles, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.AttachActionType,
                            ActionText = AuditReportResource.CaseAttachedFiles,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseOpenedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.CaseOpenedAccess,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseRestrictedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.CaseRestrictedAccess,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CaseDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CaseDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },
                {
                    MessageAction.CasesDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CasesDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },

                #endregion

                #region import

                {
                    MessageAction.ContactsImportedFromCSV, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.ContactsImportedFromCSV,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsModule
                        }
                },
                {
                    MessageAction.CrmTasksImportedFromCSV, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.CrmTasksImportedFromCSV,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.OpportunitiesImportedFromCSV, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.OpportunitiesImportedFromCSV,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.CasesImportedFromCSV, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.CasesImportedFromCSV,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },

                #endregion

                #region export

                {
                    MessageAction.ContactsExportedToCsv, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ExportActionType,
                            ActionText = AuditReportResource.ContactsExportedToCsv,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsModule
                        }
                },
                {
                    MessageAction.CrmTasksExportedToCsv, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ExportActionType,
                            ActionText = AuditReportResource.CrmTasksExportedToCsv,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmTasksModule
                        }
                },
                {
                    MessageAction.OpportunitiesExportedToCsv, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ExportActionType,
                            ActionText = AuditReportResource.OpportunitiesExportedToCsv,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OpportunitiesModule
                        }
                },
                {
                    MessageAction.CasesExportedToCsv, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ExportActionType,
                            ActionText = AuditReportResource.CasesExportedToCsv,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CasesModule
                        }
                },

                #endregion

                #region common settings

                {
                    MessageAction.CrmSmtpSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmSmtpSettingsUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmTestMailSent, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.CrmTestMailSent,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmDefaultCurrencyUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmDefaultCurrencyUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmAllDataExported, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ExportActionType,
                            ActionText = AuditReportResource.CrmAllDataExported,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTemperatureLevelSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelSettingsUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTagSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsTagSettingsUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CommonCrmSettingsModule
                        }
                },

                #endregion

                #region contact settings

                {
                    MessageAction.ContactsTemperatureLevelCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTemperatureLevelUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTemperatureLevelUpdatedColor, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelUpdatedColor,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTemperatureLevelsUpdatedOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelsUpdatedOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsSettingsModule
                        }
                },
                {
                    MessageAction.ContactsTemperatureLevelDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ContactsTemperatureLevelDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactsSettingsModule
                        }
                },
                {
                    MessageAction.ContactTypeCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ContactTypeCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactTypesModule
                        }
                },
                {
                    MessageAction.ContactTypeUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactTypeUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactTypesModule
                        }
                },
                {
                    MessageAction.ContactTypesUpdatedOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactTypesUpdatedOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactTypesModule
                        }
                },
                {
                    MessageAction.ContactTypeDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ContactTypeDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.ContactTypesModule
                        }
                },

                #endregion

                #region user fields settings

                {
                    MessageAction.ContactsCreatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ContactsCreatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsUpdatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsUpdatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsUpdatedUserFieldsOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ContactsUpdatedUserFieldsOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsDeletedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ContactsDeletedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.PersonsCreatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.PersonsCreatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.PersonsUpdatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonsUpdatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.PersonsUpdatedUserFieldsOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PersonsUpdatedUserFieldsOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.PersonsDeletedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.PersonsDeletedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CompaniesCreatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CompaniesCreatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CompaniesUpdatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompaniesUpdatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CompaniesUpdatedUserFieldsOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CompaniesUpdatedUserFieldsOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CompaniesDeletedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CompaniesDeletedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesCreatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunitiesCreatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesUpdatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunitiesUpdatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesUpdatedUserFieldsOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunitiesUpdatedUserFieldsOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesDeletedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunitiesDeletedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesCreatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CasesCreatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesUpdatedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CasesUpdatedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesUpdatedUserFieldsOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CasesUpdatedUserFieldsOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesDeletedUserField, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CasesDeletedUserField,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },

                #endregion

                #region history categories settings

                {
                    MessageAction.HistoryCategoryCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.HistoryCategoryCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.HistoryCategoryUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.HistoryCategoryUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.HistoryCategoryUpdatedIcon, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.HistoryCategoryUpdatedIcon,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.HistoryCategoriesUpdatedOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.HistoryCategoriesUpdatedOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.HistoryCategoryDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.HistoryCategoryDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },

                #endregion

                #region task actegories settings

                {
                    MessageAction.CrmTaskCategoryCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CrmTaskCategoryCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmTaskCategoryUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskCategoryUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmTaskCategoryUpdatedIcon, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskCategoryUpdatedIcon,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmTaskCategoriesUpdatedOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.CrmTaskCategoriesUpdatedOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmTaskCategoryDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CrmTaskCategoryDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },

                #endregion

                #region opportunity stages settings

                {
                    MessageAction.OpportunityStageCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunityStageCreated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunityStageUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunityStageUpdated,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunityStageUpdatedColor, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunityStageUpdatedColor,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunityStagesUpdatedOrder, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OpportunityStagesUpdatedOrder,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunityStageDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunityStageDeleted,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },

                #endregion

                #region tags settings

                {
                    MessageAction.ContactsCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ContactsCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.ContactsDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ContactsDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.OpportunitiesCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.OpportunitiesDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.OpportunitiesDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesCreatedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.CasesCreatedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CasesDeletedTag, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.CasesDeletedTag,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },

                #endregion

                {
                    MessageAction.WebsiteContactFormUpdatedKey, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.WebsiteContactFormUpdatedKey,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.OtherCrmSettingsModule
                        }
                },
                {
                    MessageAction.CrmEntityDetachedFile, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DetachActionType,
                            ActionText = AuditReportResource.CrmEntityDetachedFile,
                            Product = AuditReportResource.CrmProduct,
                            Module = AuditReportResource.CrmFilesModule
                        }
                },

                #endregion

                #region people
                
                {
                    MessageAction.UserCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.UserCreated,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.GuestCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.GuestCreated,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserUpdated,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserUpdatedAvatar, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserUpdatedAvatar,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserSentActivationInstructions, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.UserSentActivationInstructions,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserSentPasswordInstructions, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.UserSentPasswordInstructions,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserSentEmailInstructions, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.UserSentEmailInstructions,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UserDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.UserDeleted,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UsersUpdatedType, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UsersUpdatedType,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UsersUpdatedStatus, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UsersUpdatedStatus,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UsersSentActivationInstructions, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.UsersSentActivationInstructions,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.UsersDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.UsersDeleted,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },
                {
                    MessageAction.GroupCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.GroupCreated,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.GroupsModule
                        }
                },
                {
                    MessageAction.GroupUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.GroupUpdated,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.GroupsModule
                        }
                },
                {
                    MessageAction.GroupDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.GroupDeleted,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.GroupsModule
                        }
                },                
                {
                    MessageAction.UserUpdatedPassword, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserUpdatedPassword,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.UserLinkedSocialAccount, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.LinkActionType,
                            ActionText = AuditReportResource.UserLinkedSocialAccount,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },               
                {
                    MessageAction.UserUnlinkedSocialAccount, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UnlinkActionType,
                            ActionText = AuditReportResource.UserUnlinkedSocialAccount,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.UserUpdatedLanguage, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserUpdatedLanguage,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.UserActivatedEmail, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserActivatedEmail,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.UserUpdatedAvatarThumbnails, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.UserUpdatedAvatarThumbnails,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },                
                {
                    MessageAction.UserSentDeleteInstructions, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.SendActionType,
                            ActionText = AuditReportResource.UserSentDeleteInstructions,
                            Product = AuditReportResource.PeopleProduct,
                            Module = AuditReportResource.UsersModule
                        }
                },

                #endregion

                #region documents
                
                {
                    MessageAction.FolderCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.FolderCreated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderRenamed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FolderRenamed,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderUpdatedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.FolderUpdatedAccess,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FileCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.FileCreated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileRenamed, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FileRenamed,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FileUpdated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileCreatedVersion, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.FileCreatedVersion,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileUpdatedToVersion, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.FileUpdatedToVersion,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileDeletedVersion, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FileDeletedVersion,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileUpdatedRevisionComment, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FileUpdatedRevisionComment,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileLocked, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FileLocked,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileUnlocked, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.FileUnlocked,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileUpdatedAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateAccessActionType,
                            ActionText = AuditReportResource.FileUpdatedAccess,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileDownloaded, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DownloadActionType,
                            ActionText = AuditReportResource.FileDownloaded,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileDownloadedAs, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DownloadActionType,
                            ActionText = AuditReportResource.FileDownloadedAs,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.DocumentsDownloaded, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DownloadActionType,
                            ActionText = AuditReportResource.DocumentsDownloaded,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsModule
                        }
                },
                {
                    MessageAction.DocumentsCopied, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CopyActionType,
                            ActionText = AuditReportResource.DocumentsCopied,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsModule
                        }
                },
                {
                    MessageAction.DocumentsMoved, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DocumentsMoved,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsModule
                        }
                },
                {
                    MessageAction.DocumentsDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.DocumentsDeleted,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsModule
                        }
                },
                {
                    MessageAction.TrashCleaned, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.TrashCleaned,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsModule
                        }
                },
                {
                    MessageAction.FileMovedToTrash, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FileMovedToTrash,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FileDeleted,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FolderMovedToTrash, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FolderMovedToTrash,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FolderDeleted,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FileCopied, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CopyActionType,
                            ActionText = AuditReportResource.FileCopied,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileCopiedWithOverwriting, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CopyActionType,
                            ActionText = AuditReportResource.FileCopiedWithOverwriting,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileMoved, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FileMoved,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileMovedWithOverwriting, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FileMovedWithOverwriting,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FolderCopied, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CopyActionType,
                            ActionText = AuditReportResource.FolderCopied,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderCopiedWithOverwriting, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CopyActionType,
                            ActionText = AuditReportResource.FolderCopiedWithOverwriting,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderMoved, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FolderMoved,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FolderMovedWithOverwriting, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.MoveActionType,
                            ActionText = AuditReportResource.FolderMovedWithOverwriting,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FoldersModule
                        }
                },
                {
                    MessageAction.FilesImportedFromBoxNet, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FilesImportedFromBoxNet,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FilesImportedFromGoogleDocs, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FilesImportedFromGoogleDocs,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FilesImportedFromZoho, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.FilesImportedFromZoho,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.FileImported, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.ImportActionType,
                            ActionText = AuditReportResource.FileImported,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.DocumentsThirdPartySettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DocumentsThirdPartySettingsUpdated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },
                {
                    MessageAction.DocumentsOverwritingSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DocumentsOverwritingSettingsUpdated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },
                {
                    MessageAction.DocumentsUploadingFormatsSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DocumentsUploadingFormatsSettingsUpdated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },
                {
                    MessageAction.FileUploaded, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UploadActionType,
                            ActionText = AuditReportResource.FileUploaded,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.FilesModule
                        }
                },
                {
                    MessageAction.ThirdPartyCreated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.ThirdPartyCreated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },
                {
                    MessageAction.ThirdPartyUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ThirdPartyUpdated,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },
                {
                    MessageAction.ThirdPartyDeleted, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.DeleteActionType,
                            ActionText = AuditReportResource.ThirdPartyDeleted,
                            Product = AuditReportResource.DocumentsProduct,
                            Module = AuditReportResource.DocumentsSettingsModule
                        }
                },

                #endregion

                #region settings

                {
                    MessageAction.LanguageSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.LanguageSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.TimeZoneSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TimeZoneSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.DnsSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DnsSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.TrustedMailDomainSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TrustedMailDomainSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.PasswordStrengthSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.PasswordStrengthSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.TwoFactorAuthenticationSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.TwoFactorAuthenticationSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.AdministratorMessageSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.AdministratorMessageSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.DefaultStartPageSettingsUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.DefaultStartPageSettingsUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.GeneralModule
                        }
                },
                {
                    MessageAction.ProductsListUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.ProductsListUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.ProductsModule
                        }
                },
                {
                    MessageAction.OwnerUpdated, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.OwnerUpdated,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.ProductsModule
                        }
                },
                {
                    MessageAction.AdministratorUpdatedProductAccess, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.UpdateActionType,
                            ActionText = AuditReportResource.AdministratorUpdatedProductAccess,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.ProductsModule
                        }
                },
                {
                    MessageAction.AdministratorAdded, new MessageMaps
                        {
                            ActionTypeText = AuditReportResource.CreateActionType,
                            ActionText = AuditReportResource.AdministratorAdded,
                            Product = AuditReportResource.SettingsProduct,
                            Module = AuditReportResource.ProductsModule
                        }
                },

                #endregion
            };

        public static string GetActionText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            if (!actions.ContainsKey(action))
                throw new ArgumentException(string.Format("There is no action text for \"{0}\" type of event", action));

            var text = actions[(MessageAction)evt.Action].ActionText;

            return evt.Ids == null || !evt.Ids.Any()
                       ? text
                       : string.Format(text, evt.Ids.Select(GetLimitedText).ToArray());
        }

        public static string GetActionText(LoginEvent evt)
        {
            var action = (MessageAction)evt.Action;
            if (!actions.ContainsKey(action))
                throw new ArgumentException(string.Format("There is no action text for \"{0}\" type of event", action));

            var text = actions[(MessageAction)evt.Action].ActionText;

            return evt.Ids == null || !evt.Ids.Any()
                       ? text
                       : string.Format(text, evt.Ids.Select(GetLimitedText).ToArray());
        }

        public static string GetActionTypeText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].ActionTypeText;
        }

        public static string GetProductText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].Product;
        }

        public static string GetModuleText(AuditEvent evt)
        {
            var action = (MessageAction)evt.Action;
            return !actions.ContainsKey(action)
                       ? string.Empty
                       : actions[(MessageAction)evt.Action].Module;
        }

        private static string GetLimitedText(string text)
        {
            if (text == null) throw new ArgumentException("text");
            return text.Length < 50 ? text : string.Format("[{0}...]", text.Substring(0, 47));
        }

        private class MessageMaps
        {
            public string ActionTypeText { get; set; }
            public string ActionText { get; set; }
            public string Product { get; set; }
            public string Module { get; set; }
        }
    }
}