/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Mobile;
using ASC.Web.Projects.Classes;
using ASC.Core.Users;

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ProjectNavigatePanel : BaseUserControl
    {
        public string CurrentPage { get { return Page.Master.CurrentPage; } }

        public bool IsSubcribed { get { return Page.IsSubcribed; } }

        public bool InConcreteProjectModule { get; set; }

        protected Project Project { get { return Page.Project; } }

        protected string ProjectLeaderName { get; set; }

        protected bool CanEditProject { get; set; }

        protected bool CanDeleteProject { get; set; }

        protected bool IsInTeam { get; set; }

        protected string UpLink
        {
            get
            {
                var stringBuilder = new StringBuilder(CurrentPage + ".aspx");

                if (CheckUpLink(HttpContext.Current.Request.UrlReferrer))
                    stringBuilder.AppendFormat("?prjID={0}", Project.ID);

                return stringBuilder.ToString();
            }
        }

        public bool ShowGanttChartFlag
        {
            get
            {
                return !InConcreteProjectModule && !MobileDetector.IsMobile && ProjectSecurity.CanReadGantt(Project) && Project.Status == ProjectStatus.Open;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _hintPopup.Options.IsPopup = true;
            _projectDescriptionPopup.Options.IsPopup = true;

            CanEditProject = ProjectSecurity.CanEdit(Project);
            CanDeleteProject = ProjectSecurity.CanDelete(Project);
            ProjectLeaderName = Global.EngineFactory.GetParticipantEngine().GetByID(Project.Responsible).UserInfo.DisplayUserName();
            IsInTeam = Global.EngineFactory.GetProjectEngine().IsInTeam(Project.ID, Page.Participant.ID);
            InConcreteProjectModule = RequestContext.IsInConcreteProjectModule;
        }

        private static bool CheckUpLink(Uri uri)
        {
            if (uri == null) return true;

            var urlReferrer = uri.ToString().ToLower();

            return
                urlReferrer.IndexOf("add", StringComparison.OrdinalIgnoreCase) > 0 ||
                urlReferrer.IndexOf("edit", StringComparison.OrdinalIgnoreCase) > 0 ||
                (urlReferrer.IndexOf("messages", StringComparison.OrdinalIgnoreCase) > 0 ||
                 urlReferrer.IndexOf("tasks", StringComparison.OrdinalIgnoreCase) > 0) &&
                urlReferrer.IndexOf("prjid", StringComparison.OrdinalIgnoreCase) > 0;
        }
    }
}