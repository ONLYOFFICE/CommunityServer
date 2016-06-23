/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Resources;

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
            Master.DisabledPrjNavPanel = true;
            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.AutoTimer);

            if (!RequestContext.CanCreateTime()) return;

            RenderContentForTimer();
        }

        #endregion

        #region Methods

        private void RenderContentForTimer()
        {
            var participantId = Guid.Empty;

            if (!Participant.IsAdmin)
                participantId = Participant.ID;

            UserProjects = EngineFactory.ProjectEngine.GetOpenProjectsWithTasks(participantId);

            if (UserProjects.Any() && (Project == null || !UserProjects.Contains(Project)))
                Project = UserProjects.First();

            var tasks = EngineFactory.TaskEngine.GetByProject(Project.ID, null, Participant.IsVisitor ? participantId : Guid.Empty);

            OpenUserTasks = tasks.Where(r => r.Status == TaskStatus.Open).OrderBy(r => r.Title);
            ClosedUserTasks = tasks.Where(r => r.Status == TaskStatus.Closed).OrderBy(r => r.Title);

            Users = EngineFactory.ProjectEngine.GetTeam(Project.ID).OrderBy(r => DisplayUserSettings.GetFullUserName(r.UserInfo)).Where(r => r.UserInfo.IsVisitor() != true).ToList();

            if (!string.IsNullOrEmpty(Request.QueryString["taskId"]))
            {
                Target = int.Parse(Request.QueryString["taskId"]);
            }
        }

        #endregion
    }
}