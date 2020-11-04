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
                createButtonPanel.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Common/ButtonSidePanel.ascx")));
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
                        "~/js/third-party/slick.min.js",
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