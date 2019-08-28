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


using System;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Projects.Classes;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Masters.ClientScripts;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Projects.Masters
{
    public partial class BasicTemplate : MasterPage, IStaticBundle
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

        public bool DisabledEmptyScreens { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            WriteClientScripts();

            Page.EnableViewState = false;
        }

        #region Methods

        protected void InitControls()
        {
            if (!Master.DisabledSidePanel)
            {
                projectsNavigationPanel.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Common/NavigationSidePanel.ascx")));
            }

            _projectNavigatePanel.Controls.Add(LoadControl(CommonList.Location));
            _projectNavigatePanel.Controls.Add(LoadControl(LoaderPage.Location));
        }

        public void AddControl(Control control)
        {
            commonHolder.Controls.Add(control);
        }

        protected void WriteClientScripts()
        {
            WriteProjectResources();

            Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript());
        }

        public void RegisterCRMResources()
        {
            Master
                .AddStyles(ResolveUrl, "~/Products/CRM/App_Themes/default/css/common.less",
                    "~/Products/CRM/App_Themes/default/css/contacts.less")
                .AddBodyScripts(ResolveUrl,
                    "~/Products/CRM/js/contacts.js",
                    "~/Products/CRM/js/common.js");
        }

        public void WriteProjectResources()
        {
            var requestContext = ((BasePage)Page).RequestContext;
            Master
                .AddClientScript(
                    ((Product)WebItemManager.Instance[WebItemManager.ProjectsProductID]).ClientScriptLocalization,
                    ((Product)WebItemManager.Instance[WebItemManager.CRMProductID]).ClientScriptLocalization,
                    new CRMDataResources(),
                    new ClientUserResources(),
                    new ClientCurrentUserResources());

            if (requestContext.IsInConcreteProject)
            {
                Master.AddClientScript(new ClientProjectResources());
            }
        }

        #endregion

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("projects", "projects")
                    .AddSource(ResolveUrl, new ClientTemplateResources())
                    .AddSource(ResolveUrl,
                        "~/js/asc/plugins/jquery-projectadvansedselector.js",
                        "~/js/asc/plugins/progressdialog.js",
                        "~/js/third-party/autosize.js",
                        "~/js/uploader/ajaxupload.js")
                    .AddSource(PathProvider.GetFileStaticRelativePath,
                        "jquery-tagsadvansedselector.js",
                        "jq_projects_extensions.js",
                        "helper.js",
                        "common.js",
                        "navsidepanel.js",
                        "taskaction.js",
                        "milestoneaction.js",
                        "projectnavpanel.js",
                        "common_filter_projects.js",
                        "base.js",
                        "subtasks.js",
                        "tasks.js",
                        "taskdescription.js",
                        "projects.js",
                        "projecttemplates.js",
                        "projectteam.js",
                        "milestones.js",
                        "discussions.js",
                        "timetracking.js",
                        "apitimetraking.js",
                        "ganttchart_min.js",
                        "ganttchartpage.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("projects", "projects")
                    .AddSource(PathProvider.GetFileStaticRelativePath,
                        "allprojects.less",
                        "projectaction.css",
                        "milestones.less",
                        "alltasks.less",
                        "discussions.less",
                        "timetracking.less",
                        "projectteam.less",
                        "projecttemplates.css",
                        "import.css",
                        "reports.css",
                        "common.less");
        }
    }
}