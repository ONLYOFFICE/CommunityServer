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
using ASC.Web.Core;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Projects.Engine
{
    public class EngineFactory
    {
        public static readonly Guid ProductId = WebItemManager.ProjectsProductID;

        public TypedParameter DisableNotifications { get; set; }

        public ILifetimeScope Container { get; set; }

        public EngineFactory(bool disableNotifications)
        {
            DisableNotifications = DIHelper.GetParameter(disableNotifications);
        }

        private FileEngine fileEngine;
        public FileEngine FileEngine { get { return fileEngine ?? (fileEngine = Container.Resolve<FileEngine>()); } }

        private ProjectEngine projectEngine;
        public ProjectEngine ProjectEngine { get { return projectEngine ?? (projectEngine = Container.Resolve<ProjectEngine>(DisableNotifications)); } }

        private MilestoneEngine milestoneEngine;
        public MilestoneEngine MilestoneEngine { get { return milestoneEngine ?? (milestoneEngine = Container.Resolve<MilestoneEngine>(DisableNotifications)); } }

        private CommentEngine commentEngine;
        public CommentEngine CommentEngine { get { return commentEngine ?? (commentEngine = Container.Resolve<CommentEngine>(DisableNotifications)); } }

        private SearchEngine searchEngine;
        public SearchEngine SearchEngine { get { return searchEngine ?? (searchEngine = Container.Resolve<SearchEngine>()); } }

        private TaskEngine taskEngine;
        public TaskEngine TaskEngine { get { return taskEngine ?? (taskEngine = Container.Resolve<TaskEngine>(DisableNotifications)); } }

        private SubtaskEngine subtaskEngine;
        public SubtaskEngine SubtaskEngine { get { return subtaskEngine ?? (subtaskEngine = Container.Resolve<SubtaskEngine>(DisableNotifications)); } }

        private MessageEngine messageEngine;
        public MessageEngine MessageEngine { get { return messageEngine ?? (messageEngine = Container.Resolve<MessageEngine>(DisableNotifications)); } }

        private TimeTrackingEngine timeTrackingEngine;
        public TimeTrackingEngine TimeTrackingEngine { get { return timeTrackingEngine ?? (timeTrackingEngine =Container.Resolve<TimeTrackingEngine>()); } }

        private ParticipantEngine participantEngine;
        public ParticipantEngine ParticipantEngine { get { return participantEngine ?? (participantEngine = Container.Resolve<ParticipantEngine>()); } }

        private TagEngine tagEngine;
        public TagEngine TagEngine { get { return tagEngine ?? (tagEngine = Container.Resolve<TagEngine>()); } }

        private ReportEngine reportEngine;
        public ReportEngine ReportEngine { get { return reportEngine ?? (reportEngine = Container.Resolve<ReportEngine>()); } }

        private TemplateEngine templateEngine;
        public TemplateEngine TemplateEngine { get { return templateEngine ?? (templateEngine = Container.Resolve<TemplateEngine>()); } }

        private StatusEngine statusEngine;
        public StatusEngine StatusEngine { get { return statusEngine ?? (statusEngine = Container.Resolve<StatusEngine>()); } }
    }
}