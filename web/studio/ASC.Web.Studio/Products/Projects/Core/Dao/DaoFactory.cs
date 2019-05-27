/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Projects.Core.DataInterfaces;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Projects.Data
{
    public class DaoFactory : IDaoFactory
    {
        public ILifetimeScope Container { get; set; }
        private TypedParameter Tenant { get; set; }

        public DaoFactory(int tenantId)
        {
            Tenant = DIHelper.GetParameter(tenantId);
        }

        private IProjectDao projectDao;
        public IProjectDao ProjectDao { get { return projectDao ?? (projectDao = Container.Resolve<IProjectDao>(Tenant)); } }

        private IParticipantDao participantDao;
        public IParticipantDao ParticipantDao
        {
            get
            {
                return participantDao ?? (participantDao = Container.Resolve<IParticipantDao>(Tenant));
            }
        }

        private IMilestoneDao milestoneDao;
        public IMilestoneDao MilestoneDao { get { return milestoneDao ??(milestoneDao = Container.Resolve<IMilestoneDao>(Tenant)); } }

        private ITaskDao taskDao;
        public ITaskDao TaskDao { get { return taskDao ?? (taskDao = Container.Resolve<ITaskDao>(Tenant)); } }

        private ISubtaskDao subtaskDao;
        public ISubtaskDao SubtaskDao { get { return subtaskDao ?? (subtaskDao = Container.Resolve<ISubtaskDao>(Tenant)); } }

        private IMessageDao messageDao;
        public IMessageDao MessageDao { get { return messageDao ?? (messageDao = Container.Resolve<IMessageDao>(Tenant)); } }

        private ICommentDao commentDao;
        public ICommentDao CommentDao { get { return commentDao ??  (commentDao = Container.Resolve<ICommentDao>(Tenant)); } }

        private ITemplateDao templateDao;
        public ITemplateDao TemplateDao { get { return templateDao ?? (templateDao = Container.Resolve<ITemplateDao>(Tenant)); } }

        private ITimeSpendDao timeSpendDao;
        public ITimeSpendDao TimeSpendDao { get { return timeSpendDao ?? (timeSpendDao = Container.Resolve<ITimeSpendDao>(Tenant)); } }

        private IReportDao reportDao;
        public IReportDao ReportDao { get { return reportDao ?? (reportDao = Container.Resolve<IReportDao>(Tenant)); } }

        private ISearchDao searchDao;
        public ISearchDao SearchDao { get { return searchDao ?? (searchDao = Container.Resolve<ISearchDao>(Tenant)); } }

        private ITagDao tagDao;
        public ITagDao TagDao { get { return tagDao ?? (tagDao = Container.Resolve<ITagDao>(Tenant)); } }

    }
}
