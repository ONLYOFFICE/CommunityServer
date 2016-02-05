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
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Projects.Masters
{
    public partial class BasicTemplate : MasterPage
    {
        #region Properties

        private string currentPage;

        public string CurrentPage
        {
            get
            {
                if (string.IsNullOrEmpty(currentPage))
                {
                    var absolutePathWithoutQuery = Request.Url.AbsolutePath.Substring(0, Request.Url.AbsolutePath.IndexOf(".aspx", StringComparison.Ordinal));
                    currentPage = absolutePathWithoutQuery.Substring(absolutePathWithoutQuery.LastIndexOf('/') + 1);
                }
                return currentPage;
            }
        }

        public bool DisabledSidePanel
        {
            get { return Master.DisabledSidePanel; }
            set { Master.DisabledSidePanel = value; }
        }

        public bool DisabledPrjNavPanel { get; set; }

        public bool DisabledEmptyScreens { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            WriteClientScripts();

            Page.EnableViewState = false;
        }

        #endregion

        #region Methods

        protected void InitControls()
        {
            var requestContext = ((BasePage)Page).RequestContext;
            if (!Master.DisabledSidePanel)
            {
                projectsNavigationPanel.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Common/NavigationSidePanel.ascx")));
            }

            if (!DisabledPrjNavPanel && requestContext.IsInConcreteProject)
            {
                _projectNavigatePanel.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Projects/ProjectNavigatePanel.ascx")));
            }

            _commonPopupHolder.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Common/CommonPopupContainer.ascx")));

            if (!(DisabledEmptyScreens))
                InitEmptyScreens();
        }

        private void InitEmptyScreens()
        {
            var requestContext = ((BasePage)Page).RequestContext;
            emptyScreenPlaceHolders.Controls.Add(RenderEmptyScreenForFilter(MessageResource.FilterNoDiscussions, MessageResource.DescrEmptyListMilFilter, "discEmptyScreenForFilter"));
            emptyScreenPlaceHolders.Controls.Add(RenderEmptyScreenForFilter(TaskResource.NoTasks, TaskResource.DescrEmptyListTaskFilter, "tasksEmptyScreenForFilter"));
            emptyScreenPlaceHolders.Controls.Add(RenderEmptyScreenForFilter(MilestoneResource.FilterNoMilestones, MilestoneResource.DescrEmptyListMilFilter, "mileEmptyScreenForFilter"));
            emptyScreenPlaceHolders.Controls.Add(RenderEmptyScreenForFilter(ProjectsCommonResource.Filter_NoProjects, ProjectResource.DescrEmptyListProjFilter, "prjEmptyScreenForFilter"));
            emptyScreenPlaceHolders.Controls.Add(RenderEmptyScreenForFilter(TimeTrackingResource.NoTimersFilter, TimeTrackingResource.DescrEmptyListTimersFilter, "timeEmptyScreenForFilter"));

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_tasks.png", ProductEntryPoint.ID),
                    Header = TaskResource.NoTasksCreated,
                    Describe = String.Format(TaskResource.TasksHelpTheManage, TaskResource.DescrEmptyListTaskFilter),
                    ID = "emptyListTask",
                    ButtonHTML = requestContext.CanCreateTask(true) ? String.Format("<span class='link dotline addFirstElement'>{0}</span>", TaskResource.AddFirstTask) : string.Empty
                });

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_discussions.png", ProductEntryPoint.ID),
                    Header = MessageResource.DiscussionNotFound_Header,
                    Describe = MessageResource.DiscussionNotFound_Describe,
                    ID = "emptyListDiscussion",
                    ButtonHTML = requestContext.CanCreateDiscussion(true) ?
                                     (requestContext.IsInConcreteProject
                                          ? String.Format("<a href='messages.aspx?prjID={0}&action=add' class='link dotline addFirstElement'>{1}</a>", requestContext.GetCurrentProjectId(), MessageResource.StartFirstDiscussion)
                                          : String.Format("<a href='messages.aspx?action=add' class='link dotline addFirstElement'>{0}</a>", MessageResource.StartFirstDiscussion))
                                     : string.Empty
                });

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_milestones.png", ProductEntryPoint.ID),
                    Header = MilestoneResource.MilestoneNotFound_Header,
                    Describe = String.Format(MilestoneResource.MilestonesMarkMajorTimestamps),
                    ID = "emptyListMilestone",
                    ButtonHTML = requestContext.CanCreateMilestone(true) ? String.Format("<a class='link dotline addFirstElement'>{0}</a>", MilestoneResource.PlanFirstMilestone) : string.Empty
                });

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    Header = ProjectResource.EmptyListProjHeader,
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("projects_logo.png", ProductEntryPoint.ID),
                    Describe = ProjectSecurity.CanCreateProject() ? ProjectResource.EmptyListProjDescribe : string.Empty,
                    ID = "emptyListProjects",
                    ButtonHTML = ProjectSecurity.CanCreateProject() ? string.Format("<a href='projects.aspx?action=add' class='projectsEmpty link dotline addFirstElement'>{0}<a>", ProjectResource.CreateFirstProject) : string.Empty
                });

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_time_tracking.png", ProductEntryPoint.ID),
                    Header = TimeTrackingResource.NoTtimers,
                    Describe = String.Format(TimeTrackingResource.NoTimersNote),
                    ID = "emptyListTimers",
                    ButtonHTML = String.Format("<span class='link dotline addFirstElement {1}'>{0}</span>", TimeTrackingResource.StartTimer, requestContext.CanCreateTime(true) ? string.Empty : "display-none")
                });

            emptyScreenPlaceHolders.Controls.Add(new EmptyScreenControl
                {
                    Header = ProjectTemplatesResource.EmptyListTemplateHeader,
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("project-templates_logo.png", ProductEntryPoint.ID),
                    Describe = ProjectTemplatesResource.EmptyListTemplateDescr,
                    ID = "emptyListTemplates",
                    ButtonHTML = string.Format("<a href='projectTemplates.aspx?action=add' class='projectsEmpty link dotline addFirstElement'>{0}<a>", ProjectTemplatesResource.EmptyListTemplateButton)
                });
        }

        protected void WriteClientScripts()
        {
            WriteProjectResources();

            if (Page is GanttChart)
            {
                Page.RegisterBodyScriptsControl("~/products/projects/masters/GanttBodyScripts.ascx");
                return;
            }

            Page.RegisterStyleControl("~/products/projects/masters/Styles.ascx");
            Page.RegisterBodyScriptsControl("~/products/projects/masters/CommonBodyScripts.ascx");
        }

        public void RegisterCRMResources()
        {
            Page.RegisterStyle("~/products/crm/app_themes/default/css/common.less");
            Page.RegisterStyle("~/products/crm/app_themes/default/css/contacts.less");

            Page.RegisterBodyScripts("~/js/third-party/jquery/jquery.watermarkinput.js");
            Page.RegisterBodyScripts("~/products/crm/js/contacts.js");
            Page.RegisterBodyScripts("~/products/crm/js/common.js");
        }

        public void WriteProjectResources()
        {
            var requestContext = ((BasePage)Page).RequestContext;
            Page.RegisterClientLocalizationScript(typeof(ClientScripts.ClientLocalizationResources));
            Page.RegisterClientLocalizationScript(typeof(ClientScripts.ClientTemplateResources));

            Page.RegisterClientScript(typeof(ClientScripts.ClientUserResources));
            Page.RegisterClientScript(typeof(ClientScripts.ClientCurrentUserResources));

            if (requestContext.IsInConcreteProject)
            {
                Page.RegisterClientScript(typeof(ClientScripts.ClientProjectResources));
            }
        }

        private static EmptyScreenControl RenderEmptyScreenForFilter(string headerText, string description, string id = "emptyScreenForFilter")
        {
            return new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_filter.png"),
                    Header = headerText,
                    Describe = description,
                    ID = id,
                    ButtonHTML = String.Format("<a class='clearFilterButton link dotline'>{0}</a>", ProjectsFilterResource.ClearFilter)
                };
        }

        #endregion
    }
}