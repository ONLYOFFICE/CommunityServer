/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Collections.Generic;

using ASC.AuditTrail.Types;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public class ProjectsActionsMapper : IProductActionMapper
    {
        public List<IModuleActionMapper> Mappers { get; }
        public ProductType Product { get; }
        public ProjectsActionsMapper()
        {
            Product = ProductType.Projects;

            Mappers = new List<IModuleActionMapper>()
            {
                new ProjectsActionMapper(),
                new MilestonesActionMapper(),
                new TasksActionMapper(),
                new DiscussionsActionMapper(),
                new TimeTrackingActionMapper(),
                new ReportsActionMapper(),
                new ProjectsSettingsActionMapper()
            };
        }
    }

    public class ProjectsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ProjectsActionMapper()
        {
            Module = ModuleType.Projects;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.Project, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create, new[] { MessageAction.ProjectCreated, MessageAction.ProjectCreatedFromTemplate } },
                        { ActionType.Update, new[] { MessageAction.ProjectUpdated, MessageAction.ProjectUpdatedStatus, MessageAction.ProjectUpdatedTeam } },
                        { ActionType.Delete, new[] { MessageAction.ProjectDeleted, MessageAction.ProjectDeletedMember } },
                        { ActionType.Link, new[] { MessageAction.ProjectLinkedCompany, MessageAction.ProjectLinkedPerson, MessageAction.ProjectLinkedContacts } },
                        { ActionType.Unlink, new[] { MessageAction.ProjectUnlinkedCompany, MessageAction.ProjectUnlinkedPerson } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {

                        { ActionType.Follow, MessageAction.ProjectFollowed },
                        { ActionType.Unfollow, MessageAction.ProjectUnfollowed },
                        { ActionType.UpdateAccess, MessageAction.ProjectUpdatedMemberRights },
                        { ActionType.Import, MessageAction.ProjectsImportedFromBasecamp }
                    }
                }
            };
        }
    }

    public class MilestonesActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public MilestonesActionMapper()
        {
            Module = ModuleType.Milestones;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.Milestone, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[] { MessageAction.MilestoneUpdated, MessageAction.MilestoneUpdatedStatus } }
                    }, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.MilestoneCreated },
                        { ActionType.Delete, MessageAction.MilestoneDeleted }
                    }
                }
            };
        }
    }

    public class TasksActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public TasksActionMapper()
        {
            Module = ModuleType.Tasks;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.Task, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create, new[] { MessageAction.TaskCreated, MessageAction.TaskCreatedFromDiscussion  } },
                        { ActionType.Update, new[] { MessageAction.TaskUpdated, MessageAction.TaskUpdatedStatus, MessageAction.TaskMovedToMilestone, MessageAction.TaskUnlinkedMilestone } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Delete, MessageAction.TaskDeleted },
                        { ActionType.Follow, MessageAction.TaskUpdatedFollowing },
                        { ActionType.Attach, MessageAction.TaskAttachedFiles },
                        { ActionType.Detach, MessageAction.TaskDetachedFile },
                        { ActionType.Link, MessageAction.TasksLinked },
                        { ActionType.Unlink, MessageAction.TasksUnlinked },
                    }
                },
                {
                    EntryType.Comment, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.TaskCommentCreated },
                        { ActionType.Update, MessageAction.TaskCommentUpdated },
                        { ActionType.Delete, MessageAction.TaskCommentDeleted }
                    }
                },
                {
                    EntryType.SubTask, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[] { MessageAction.SubtaskUpdated, MessageAction.SubtaskUpdatedStatus, MessageAction.SubtaskMoved }  }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.SubtaskCreated },
                        { ActionType.Delete,MessageAction.SubtaskDeleted  }
                    }
                },
            };
        }
    }

    public class DiscussionsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }

        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public DiscussionsActionMapper()
        {
            Module = ModuleType.Discussions;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.Message, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.DiscussionCreated },
                        { ActionType.Update, MessageAction.DiscussionUpdated },
                        { ActionType.Delete, MessageAction.DiscussionDeleted },
                        { ActionType.Follow, MessageAction.DiscussionUpdatedFollowing },
                        { ActionType.Attach, MessageAction.DiscussionAttachedFiles },
                        { ActionType.Detach, MessageAction.DiscussionDetachedFile }
                    }
                },
                {
                    EntryType.Comment, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.DiscussionCommentCreated },
                        { ActionType.Update, MessageAction.DiscussionCommentUpdated },
                        { ActionType.Delete, MessageAction.DiscussionCommentDeleted }
                    }
                },
            };
        }
    }

    public class TimeTrackingActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public TimeTrackingActionMapper()
        {
            Module = ModuleType.TimeTracking;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.TimeSpend, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Update, new[] { MessageAction.TaskTimeUpdated, MessageAction.TaskTimesUpdatedStatus } }
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.TaskTimeCreated },
                        { ActionType.Delete, MessageAction.TaskTimesDeleted },
                    }
                }
            };
        }
    }

    public class ReportsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ReportsActionMapper()
        {
            Module = ModuleType.Reports;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.TimeSpend, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.ReportTemplateCreated },
                        { ActionType.Update, MessageAction.ReportTemplateUpdated },
                        { ActionType.Delete, MessageAction.ReportTemplateDeleted },
                    }
                }
            };
        }
    }

    public class ProjectsSettingsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ProjectsSettingsActionMapper()
        {
            Module = ModuleType.ProjectsSettings;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    EntryType.Template, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.ProjectTemplateCreated },
                        { ActionType.Update, MessageAction.ProjectTemplateUpdated },
                        { ActionType.Delete, MessageAction.ProjectTemplateDeleted },
                    }
                }
            };
        }
    }
}