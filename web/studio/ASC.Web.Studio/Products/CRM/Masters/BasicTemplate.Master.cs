/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
            SideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript())
                .RegisterInlineScript("ASC.CRM.NavSidePanel.init();");
        }

        #region Methods

        public string CurrentPageCaption
        {
            get { return _commonContainer.CurrentPageCaption; }
            set { _commonContainer.CurrentPageCaption = value; }
        }

        public String CommonContainerHeader
        {
            set { _commonContainer.Options.HeaderBreadCrumbCaption = value; }
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