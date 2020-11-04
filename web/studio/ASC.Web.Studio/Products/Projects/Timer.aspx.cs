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
using System.Globalization;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects
{
    public partial class Timer : BasePage
    {
        #region Properties

        public int Target { get; set; }

        protected List<Participant> Users { get; set; }

        protected IEnumerable<Project> UserProjects { get; set; }

        protected IEnumerable<Task> OpenUserTasks { get; set; }

        protected IEnumerable<Task> ClosedUserTasks { get; set; }

        protected string DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator; }
        }

        #endregion


        #region Events

        protected override void PageLoad()
        {
            Master.Master.DisabledTopStudioPanel = true;
            Master.DisabledSidePanel = true;
            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.AutoTimer);

            RenderContentForTimer();
        }

        #endregion

        #region Methods

        private void RenderContentForTimer()
        {
            var participantId = Guid.Empty;

            if (!WebItemSecurity.IsProductAdministrator(EngineFactory.ProductId, SecurityContext.CurrentAccount.ID))
                participantId = Participant.ID;

            UserProjects = EngineFactory.ProjectEngine.GetByFilter(new TaskFilter
            {
                ProjectStatuses = new List<ProjectStatus> { ProjectStatus.Open },
                SortBy = "title",
                SortOrder = true
            }).Where(r => r.TaskCountTotal > 0).ToList();

            if (UserProjects.Any() && (Project == null || !UserProjects.Contains(Project)))
                Project = UserProjects.First();

            var tasks = EngineFactory.TaskEngine.GetByProject(Project.ID, null, Participant.IsVisitor ? participantId : Guid.Empty).Where(r => ProjectSecurity.CanCreateTimeSpend(r)).ToList();

            OpenUserTasks = tasks.Where(r => r.Status == TaskStatus.Open).OrderBy(r => r.Title);
            ClosedUserTasks = tasks.Where(r => r.Status == TaskStatus.Closed).OrderBy(r => r.Title);

            Users = EngineFactory.ProjectEngine.GetProjectTeamExcluded(Project.ID)
                .OrderBy(r => DisplayUserSettings.GetFullUserName(r.UserInfo))
                .Where(r => !r.UserInfo.IsVisitor())
                .Where(r => !r.IsRemovedFromTeam || tasks.Any(t => t.Responsibles.Contains(r.ID)))
                .ToList();

            if (!string.IsNullOrEmpty(Request.QueryString["taskId"]))
            {
                Target = int.Parse(Request.QueryString["taskId"]);
            }
        }

        #endregion
    }
}