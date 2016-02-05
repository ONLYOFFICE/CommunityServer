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
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Data;
using ASC.Web.Core;

namespace ASC.Projects.Engine
{
    public class EngineFactory
    {
        public static readonly Guid ProductId = WebItemManager.ProjectsProductID;

        private readonly IDaoFactory daoFactory;

        public bool DisableNotifications { get; set; }

        public EngineFactory(string dbId, int tenantID)
        {
            daoFactory = new DaoFactory(dbId, tenantID);
        }

        private FileEngine fileEngine;
        public FileEngine FileEngine
        {
            get { return fileEngine ?? (fileEngine = new FileEngine()); }
        }

        private ProjectEngine projectEngine;
        public ProjectEngine ProjectEngine
        {
            get { return projectEngine ?? (projectEngine = new CachedProjectEngine(daoFactory, this)); }
        }

        private MilestoneEngine milestoneEngine;
        public MilestoneEngine MilestoneEngine
        {
            get { return milestoneEngine ?? (milestoneEngine = new MilestoneEngine(daoFactory, this)); }
        }

        private CommentEngine commentEngine;
        public CommentEngine CommentEngine
        {
            get { return commentEngine ?? (commentEngine = new CommentEngine(daoFactory, this)); }
        }

        private SearchEngine searchEngine;
        public SearchEngine SearchEngine
        {
            get { return searchEngine ?? (searchEngine = new SearchEngine(daoFactory, this)); }
        }

        private TaskEngine taskEngine;
        public TaskEngine TaskEngine
        {
            get { return taskEngine ?? (taskEngine = new TaskEngine(daoFactory, this)); }
        }

        private SubtaskEngine subtaskEngine;
        public SubtaskEngine SubtaskEngine
        {
            get { return subtaskEngine ?? (subtaskEngine = new SubtaskEngine(daoFactory, this)); }
        }

        private MessageEngine messageEngine;
        public MessageEngine MessageEngine
        {
            get { return messageEngine ?? (messageEngine = new MessageEngine(daoFactory, this)); }
        }

        private TimeTrackingEngine timeTrackingEngine;
        public TimeTrackingEngine TimeTrackingEngine
        {
            get { return timeTrackingEngine ?? (timeTrackingEngine = new TimeTrackingEngine(daoFactory)); }
        }

        private ParticipantEngine participantEngine;
        public ParticipantEngine ParticipantEngine
        {
            get { return participantEngine ?? (participantEngine = new ParticipantEngine(daoFactory)); }
        }

        private TagEngine tagEngine;
        public TagEngine TagEngine
        {
            get { return tagEngine ?? (tagEngine = new TagEngine(daoFactory)); }
        }

        private ReportEngine reportEngine;
        public ReportEngine ReportEngine
        {
            get { return reportEngine ?? (reportEngine = new ReportEngine(daoFactory, this)); }
        }

        private TemplateEngine templateEngine;
        public TemplateEngine TemplateEngine
        {
            get { return templateEngine ?? (templateEngine = new TemplateEngine(daoFactory)); }
        }
    }
}
