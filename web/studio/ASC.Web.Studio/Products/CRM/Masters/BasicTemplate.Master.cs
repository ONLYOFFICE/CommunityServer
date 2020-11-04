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
using ASC.Web.Core.Client.Bundling;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Masters.ClientScripts;

namespace ASC.Web.CRM
{
    public partial class BasicTemplate : MasterPage, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            Page.EnableViewState = false;

            Master
                .AddClientScript(
                    new CRMSettingsResources(),
                    new ClientCustomResources(),
                    new CommonData(),
                    ((Product)WebItemManager.Instance[WebItemManager.CRMProductID]).ClientScriptLocalization,
                    ((Product)WebItemManager.Instance[WebItemManager.ProjectsProductID]).ClientScriptLocalization);
        }

        protected void InitControls()
        {
            CreateButton.Controls.Add(LoadControl(ButtonsSidePanel.Location));
            SideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript())
                .RegisterInlineScript("ASC.CRM.NavSidePanel.init();");
        }

        #region Methods

        public string CurrentPageCaption
        {
            get { return HeaderContainer.CurrentPageCaption; }
            set { HeaderContainer.CurrentPageCaption = value; }
        }

        public String CommonContainerHeader
        {
            set { HeaderContainer.Options.HeaderBreadCrumbCaption = value; }
        }

        #endregion

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("crm", "crm")
                    .AddSource(ResolveUrl, new ClientTemplateResources())
                    .AddSource(PathProvider.GetFileStaticRelativePath,
                        "common.js",
                        "navsidepanel.js",
                        "fileuploader.js",
                        "tasks.js",
                        "contacts.js",
                        "cases.js",
                        "deals.js",
                        "invoices.js",
                        "socialmedia.js",
                        "sender.js",
                        "reports.js")
                    .AddSource(ResolveUrl,
                        "~/js/uploader/ajaxupload.js",
                        "~/js/third-party/autosize.js",
                        "~/js/asc/plugins/progressdialog.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("crm", "crm")
                    .AddSource(PathProvider.GetFileStaticRelativePath,
                        "common.less",
                        "tasks.less",
                        "cases.less",
                        "contacts.less",
                        "deals.less",
                        "invoices.less",
                        "fg.css",
                        "socialmedia.less",
                        "settings.less",
                        "voip.common.less",
                        "voip.quick.less",
                        "voip.numbers.less",
                        "voip.calls.less",
                        "reports.less");
        }
    }
}