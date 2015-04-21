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

        public FileEngine GetFileEngine()
        {
            return new FileEngine();
        }

        public ProjectEngine GetProjectEngine()
        {
            return new CachedProjectEngine(daoFactory, this);
        }

        public MilestoneEngine GetMilestoneEngine()
        {
            return new MilestoneEngine(daoFactory, this);
        }

        public CommentEngine GetCommentEngine()
        {
            return new CommentEngine(daoFactory);
        }

        public SearchEngine GetSearchEngine()
        {
            return new SearchEngine(daoFactory);
        }

        public TaskEngine GetTaskEngine()
        {
            return new TaskEngine(daoFactory, this);
        }

        public SubtaskEngine GetSubtaskEngine()
        {
            return new SubtaskEngine(daoFactory, this);
        }

        public MessageEngine GetMessageEngine()
        {
            return new MessageEngine(daoFactory, this);
        }

        public TimeTrackingEngine GetTimeTrackingEngine()
        {
            return new TimeTrackingEngine(daoFactory);
        }

        public ParticipantEngine GetParticipantEngine()
        {
            return new ParticipantEngine(daoFactory);
        }

        public TagEngine GetTagEngine()
        {
            return new TagEngine(daoFactory);
        }

        public ReportEngine GetReportEngine()
        {
            return new ReportEngine(daoFactory);
        }

        public TemplateEngine GetTemplateEngine()
        {
            return new TemplateEngine(daoFactory);
        }
    }
}
