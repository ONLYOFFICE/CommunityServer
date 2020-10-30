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
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Sample.Classes;
using ASC.Web.Sample.Configuration;
using ASC.Web.Sample.Controls;

namespace ASC.Web.Sample.Masters
{
    public partial class BasicTemplate : MasterPage, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SampleDao.CheckTable();

            InitControls();

            Page.EnableViewState = false;

            Master
                .AddClientScript(((Product) WebItemManager.Instance[ProductEntryPoint.Id]).ClientScriptLocalization);
        }

        protected void InitControls()
        {
            CreateButtonContent.Controls.Add(LoadControl(ButtonsSidePanel.Location));
            SideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript())
                .RegisterInlineScript(string.Format("ASC.Sample.PageScript.init('{0}');", SecurityContext.CurrentAccount.ID));
        }


        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("sample", "sample")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "script.js")
                       .AddSource(ResolveUrl,
                                  "~/js/uploader/ajaxupload.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("sample", "sample")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "style.less");
        }
    }
}