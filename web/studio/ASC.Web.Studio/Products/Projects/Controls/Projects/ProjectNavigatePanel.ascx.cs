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

        protected bool IsOutsider { get { return ProjectSecurity.IsOutsider(Page.Participant.ID); } }

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