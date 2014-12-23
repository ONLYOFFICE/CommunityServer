/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core.Domain;
using System.Globalization;

using ASC.Core.Users;
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

        protected List<Project> UserProjects { get; set; }

        protected IEnumerable<Task> OpenUserTasks { get; set; }

        protected IEnumerable<Task> ClosedUserTasks { get; set; }

        protected string DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator; }
        }

        public static int TaskID
        {
            get
            {
                int id;
                if (Int32.TryParse(UrlParameters.EntityID, out id))
                {
                    return id;
                }
                return -1;
            }
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

            UserProjects = Global.EngineFactory.GetProjectEngine().GetOpenProjectsWithTasks(participantId);

            if (UserProjects.Any() && (Project == null || !UserProjects.Contains(Project)))
                Project = UserProjects[0];

            var tasks = Global.EngineFactory.GetTaskEngine().GetByProject(Project.ID, null, Participant.IsVisitor ? participantId : Guid.Empty);

            OpenUserTasks = tasks.Where(r => r.Status == TaskStatus.Open).OrderBy(r => r.Title);
            ClosedUserTasks = tasks.Where(r => r.Status == TaskStatus.Closed).OrderBy(r => r.Title);

            Users = Global.EngineFactory.GetProjectEngine().GetTeam(Project.ID).OrderBy(r => DisplayUserSettings.GetFullUserName(r.UserInfo)).Where(r => r.UserInfo.IsVisitor() != true).ToList();

            if (!string.IsNullOrEmpty(Request.QueryString["taskId"]))
            {
                Target = int.Parse(Request.QueryString["taskId"]);
            }
        }

        private void GetTargetTaskId()
        {
            Target = -1;
            int id;
            if (Int32.TryParse(UrlParameters.EntityID, out id))
            {
                Target = id;
            }

            if (Target > 0)
            {
                var t = Global.EngineFactory.GetTaskEngine().GetByID(Target);
                if (t == null || t.Status == TaskStatus.Closed) Target = -1;
            }
        }

        #endregion
    }
}