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

        private IStatusDao statusDao;
        public IStatusDao StatusDao { get { return statusDao ?? (statusDao = Container.Resolve<IStatusDao>(Tenant)); } }

    }
}
