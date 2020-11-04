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
using ASC.Web.Core.Client.Bundling;
using ASC.Web.People.Masters.ClientScripts;
using ASC.Web.People.UserControls;
using ASC.Web.Studio.UserControls.Users;

namespace ASC.Web.People.Masters
{
    public partial class PeopleBaseTemplate : MasterPage, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();

            CreateButtonContent.Controls.Add(LoadControl(SideButtonsPanel.Location));

            SidePanel.Controls.Add(LoadControl(SideNavigationPanel.Location));

            //UserMaker.AddOnlyOne(Page, ControlHolder);
            //ControlHolder.Controls.Add(new ImportUsersWebControl());
            ControlHolder.Controls.Add(LoadControl(ResendInvitesControl.Location));

            Master
                .AddClientScript(
                    new ClientSettingsResources(),
                    new ClientCustomResources(),
                    new ClientLocalizationResources());
        }

        private void InitScripts()
        {
            Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript())
                .RegisterInlineScript(
                    "jQuery(document.body).children('form').bind('submit', function() { return false; });");
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("people", "people")
                    .AddSource(ResolveUrl, new ClientTemplateResources())
                    .AddSource(ResolveUrl,
                        "~/Products/People/js/peoplemanager.js",
                        "~/Products/People/js/filterhandler.js",
                        "~/Products/People/js/navigatorhandler.js",
                        "~/Products/People/js/peoplecontroller.js",
                        "~/Products/People/js/peoplecore.js",
                        "~/Products/People/js/departmentmanagement.js",
                        "~/Products/People/js/peopleactions.js",
                        "~/Products/People/js/reassigns.js",
                        "~/Products/People/js/sidenavigationpanel.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("people", "people")
                    .AddSource(ResolveUrl,
                        "~/Products/People/App_Themes/default/css/people.master.less");
        }
    }
}