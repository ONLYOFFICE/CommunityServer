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
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Web.CRM.Controls.Common;

namespace ASC.Web.CRM
{
    public partial class BasicTemplate : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            Page.EnableViewState = false;

            Page.RegisterClientScript(typeof(Masters.ClientScripts.CRMSettingsResources));
            Page.RegisterClientScript(typeof(Masters.ClientScripts.ClientCustomResources));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientLocalizationResources));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientTemplateResources));

            Page.RegisterClientScript(typeof(Masters.ClientScripts.CommonData));
        }

        protected void RegisterScriptForTaskAction()
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.TaskActionViewData));
            Page.RegisterInlineScript("ASC.CRM.NavSidePanel.init();");
        }

        protected void InitControls()
        {
            SideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Page.RegisterStyle(PathProvider.GetFileStaticRelativePath,
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
                "voip.calls.less");

            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath, 
                                         "common.js",
                                         "navsidepanel.js",
                                         "fileUploader.js",
                                         "tasks.js",
                                         "contacts.js",
                                         "cases.js",
                                         "deals.js",
                                         "invoices.js",
                                         "socialmedia.js",
                                         "sender.js"
                                     );

            Page.RegisterBodyScripts(ResolveUrl,
                                         "~/js/uploader/ajaxupload.js",
                                         "~/js/third-party/jquery/jquery.autosize.js");


            RegisterScriptForTaskAction();
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
    }
}