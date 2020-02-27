/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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