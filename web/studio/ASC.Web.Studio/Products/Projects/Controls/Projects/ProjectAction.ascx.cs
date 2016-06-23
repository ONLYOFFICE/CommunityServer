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
using System.Web;
using System.Linq;

using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Core;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ProjectAction : BaseUserControl
    {
        protected Project Project { get { return Page.Project; } }
        protected string ProjectTags { get; set; }
        protected string ProjectManagerName { get; set; }
        protected string UrlProject { get; set; }
        protected int ActiveTasksCount { get; set; }
        protected int ActiveMilestonesCount { get; set; }
        protected bool IsEditingProjectAvailable { get; set; }
        protected string PageTitle { get; set; }
        protected string ActiveTasksUrl { get; set; }
        protected string ActiveMilestonesUrl { get; set; }
        protected string ProjectActionButtonTitle { get; set; }
        protected int TemplatesCount { get; set; }
        protected bool RenderProjectPrivacyCheckboxValue { get; set; }
        protected bool HideChooseTeam { get; set; }
        protected string ProjectManagerId { get; set; }
        protected string ProjectStatusTitle { get; set; }
        protected string ProjectStatusList { get; set; }

        protected bool IsProjectCreatedFromCrm
        {
            get { return Request.Params["opportunityID"] != null || Request.Params["contactID"] != null; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath, "projectaction.js");

            _hintPopupDeleteProject.Options.IsPopup = true;
            _hintPopupActiveTasks.Options.IsPopup = true;
            _hintPopupActiveMilestones.Options.IsPopup = true;

            TemplatesCount = Page.EngineFactory.TemplateEngine.GetCount();
            HideChooseTeam = CoreContext.UserManager.GetUsers().All(r => r.ID == SecurityContext.CurrentAccount.ID);

            if (Project != null)
            {
                ProjectManagerName = CoreContext.UserManager.GetUsers(Project.Responsible).DisplayUserName();
                UrlProject = "tasks.aspx?prjID=" + Project.ID;
                ActiveTasksCount = Page.EngineFactory.TaskEngine.GetByProject(Project.ID, TaskStatus.Open, Guid.Empty).Count();
                ActiveMilestonesCount = Page.EngineFactory.MilestoneEngine.GetByProject(Project.ID).Count(m => m.Status == MilestoneStatus.Open);
                IsEditingProjectAvailable = true;
                PageTitle = ProjectResource.EditProject;
                ActiveTasksUrl = string.Format("tasks.aspx?prjID={0}#status=open", Project.ID);
                ActiveMilestonesUrl = string.Format("milestones.aspx?prjID={0}#status=open", Project.ID);
                ProjectActionButtonTitle = ProjectResource.SaveProject;
                RenderProjectPrivacyCheckboxValue = Project.Private;
                ProjectManagerId = Project.Responsible.ToString();

                projectTitle.Text = Project.Title;
                projectDescription.Text = Project.Description;

                var tags = Page.EngineFactory.TagEngine.GetProjectTags(Project.ID).Select(r => r.Value.HtmlEncode()).ToArray();
                ProjectTags = string.Join(", ", tags);

                ProjectStatusTitle = Project.Status.ToString();
                ProjectStatusList = string.Join(";",
                    string.Join(",", (int)ProjectStatus.Open, ProjectStatus.Open),
                    string.Join(",", (int)ProjectStatus.Paused, ProjectStatus.Paused),
                    string.Join(",", (int)ProjectStatus.Closed, ProjectStatus.Closed));

                Page.Title = HeaderStringHelper.GetPageTitle(Project.Title);
            }
            else
            {
                if(TemplatesCount > 0)
                {
                    ControlPlaceHolder.Controls.Add(LoadControl("../Common/AddMilestoneContainer.ascx"));
                }

                projectTitle.Attributes.Add("deftext", ProjectTemplatesResource.DefaultProjTitle);
                
                _hintPopupDeleteProject.Options.IsPopup = true;
                _hintPopupActiveTasks.Options.IsPopup = true;
                _hintPopupActiveMilestones.Options.IsPopup = true;

                PageTitle = ProjectResource.CreateNewProject;
                ProjectActionButtonTitle = ProjectResource.AddNewProject;
                RenderProjectPrivacyCheckboxValue = true;
                ProjectManagerName = ProjectResource.AddProjectManager;

                var users = CoreContext.UserManager.GetUsers().Where(r => ProjectSecurity.IsProjectsEnabled(r.ID)).ToList();
                if (users.Count == 1)
                {
                    var manager = users.First();
                    ProjectManagerId = manager.ID.ToString();
                    ProjectManagerName = manager.DisplayUserName();
                }
                
                Page.Title = HeaderStringHelper.GetPageTitle(PageTitle);

                Page.Master.RegisterCRMResources();
            }

        }
    }
}