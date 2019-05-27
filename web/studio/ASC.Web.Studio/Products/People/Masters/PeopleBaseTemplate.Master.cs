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
using ASC.Web.Core.Client.Bundling;
using ASC.Web.People.Masters.ClientScripts;
using ASC.Web.People.UserControls;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.People.Masters
{
    public partial class PeopleBaseTemplate : MasterPage, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();

            _sidepanelHolder.Controls.Add(LoadControl(SideNavigationPanel.Location));

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
                        "~/products/people/js/peoplemanager.js",
                        "~/products/people/js/filterHandler.js",
                        "~/products/people/js/navigatorHandler.js",
                        "~/products/people/js/peopleController.js",
                        "~/products/people/js/peopleCore.js",
                        "~/products/people/js/departmentmanagement.js",
                        "~/products/people/js/peopleActions.js",
                        "~/products/people/js/reassigns.js",
                        "~/products/people/js/sideNavigationPanel.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("people", "people")
                    .AddSource(ResolveUrl,
                        "~/products/people/app_themes/default/css/people.master.less");
        }
    }
}