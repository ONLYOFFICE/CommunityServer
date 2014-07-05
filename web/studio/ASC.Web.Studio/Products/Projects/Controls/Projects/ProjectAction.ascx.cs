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
using System.Web;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;

using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ProjectAction : BaseUserControl
    {
        protected Project Project { get { return Page.Project; } }

        protected string ProjectTags { get; set; }

        protected string UrlProject { get { return Project == null ? string.Empty : "tasks.aspx?prjID=" + Project.ID; } }

        protected int ActiveTasksCount
        {
            get { return Project == null ? 0 : Global.EngineFactory.GetTaskEngine().GetByProject(Project.ID, TaskStatus.Open, Guid.Empty).Count(); }
        }

        protected int ActiveMilestonesCount
        {
            get { return Project == null ? 0 : Global.EngineFactory.GetMilestoneEngine().GetByProject(Project.ID).Count(m => m.Status == MilestoneStatus.Open); }
        }

        protected bool IsProjectCreatedFromCrm { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("projectaction.js"));

            IsProjectCreatedFromCrm = (Request.Params["opportunityID"] != null || Request.Params["contactID"] != null);

            _hintPopupDeleteProject.Options.IsPopup = true;
            _hintPopupActiveTasks.Options.IsPopup = true;
            _hintPopupActiveMilestones.Options.IsPopup = true;

            var projectManagerSelector = new AdvancedUserSelector
            {
                ID = "projectManagerSelector",
                DefaultGroupText = CustomResourceHelper.GetResource("EmployeeAllDepartments"),
                EmployeeType = EmployeeType.User
            };

            if (Project != null)
            {
                projectTitle.Text = Project.Title;
                projectDescription.Text = Project.Description;
                if (!CoreContext.UserManager.GetUsers(Project.Responsible).IsVisitor())
                    projectManagerSelector.SelectedUserId = Project.Responsible;

                var tags = Global.EngineFactory.GetTagEngine().GetProjectTags(Project.ID).Select(r => r.Value).ToArray();
                ProjectTags = string.Join(", ", tags);

                Page.Title = HeaderStringHelper.GetPageTitle(Project.Title);
            }
            else
            {
                _hintPopupDeleteProject.Options.IsPopup = true;
                _hintPopupActiveTasks.Options.IsPopup = true;
                _hintPopupActiveMilestones.Options.IsPopup = true;

                Page.Title = HeaderStringHelper.GetPageTitle(ProjectResource.CreateNewProject);
                ProjectTags = "";

                Page.Master.RegisterCRMResources();

                LoadProjectTeamSelector();
            }

            projectManagerPlaceHolder.Controls.Add(projectManagerSelector);            
        }

        private void LoadProjectTeamSelector()
        {
            var projectTeamSelector = (Studio.UserControls.Users.UserSelector)LoadControl(Studio.UserControls.Users.UserSelector.Location);
            projectTeamSelector.BehaviorID = "projectTeamSelector";
            projectTeamSelector.DisabledUsers.Add(new Guid());
            projectTeamSelector.Title = ProjectResource.ManagmentTeam;
            projectTeamSelector.SelectedUserListTitle = ProjectResource.Team;

            projectTeamPlaceHolder.Controls.Add(projectTeamSelector);
        }

        protected bool IsEditingProjectAvailable()
        {
            return Project != null;
        }

        protected string RenderProjectPrivacyCheckboxValue()
        {
            return Project != null && Project.Private ? "checked" : "";
        }

        protected string GetPageTitle()
        {
            return Project == null ? ProjectResource.CreateNewProject : ProjectResource.EditProject;
        }

        protected string GetActiveTasksUrl()
        {
            return Project == null ? string.Empty : string.Format("tasks.aspx?prjID={0}#status=open", Project.ID);
        }

        protected string GetActiveMilestonesUrl()
        {
            return Project == null ? string.Empty : string.Format("milestones.aspx?prjID={0}#status=open", Project.ID);
        }

        protected string GetProjectActionButtonTitle()
        {
            return Project == null ? ProjectResource.AddNewProject : ProjectResource.SaveProject;
        }
    }
}