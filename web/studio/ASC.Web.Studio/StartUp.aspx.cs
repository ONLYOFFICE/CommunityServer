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
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Core;
using ASC.Web.Studio.Utility;

using AjaxPro;

namespace ASC.Web.Studio
{
    [AjaxNamespace("AjaxPro.StartUp")]
    public partial class StartUp : MainPage
    {
        protected override bool MayNotAuth
        {
            get
            {
                var s = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
                return !s.Completed;
            }
        }

        protected string TenantName;

        protected override bool CheckWizardCompleted { get { return false; } }

        protected override bool RedirectToStartup { get { return false; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            TenantName = CoreContext.TenantManager.GetCurrentTenant().Name;
            Page.Title = TenantName;

            if (Request.QueryString["stop"] == "true")
            {
                WarmUp.Instance.Terminate();
            }

            if (Request.QueryString["restart"] == "true")
            {
                WarmUp.Instance.Restart();
            }

            if (!CoreContext.Configuration.Standalone || WarmUp.Instance.CheckCompleted())
                Response.Redirect(CommonLinkUtility.GetDefault(), true);

            if (!SecurityContext.IsAuthenticated)
            {
                var owner = CoreContext.TenantManager.GetCurrentTenant();
                var cookie = SecurityContext.AuthenticateMe(owner.OwnerId);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
            }

            Master.DisabledSidePanel = true;

            //top panel
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;

            var loader = (LoaderPage) LoadControl(LoaderPage.Location);
            loader.Static = true;
            loaderHolder.Controls.Add(loader);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyle("~/usercontrols/common/startup/css/startup.less");
            Page.RegisterBodyScripts("~/usercontrols/common/startup/js/startup.js");
            Page.RegisterInlineScript(string.Format("ProgressStartUpManager.init({0});", WarmUp.Instance.GetSerializedProgress()));
        }

        [AjaxMethod]
        public string GetStartUpProgress()
        {
            return WarmUp.Instance.GetSerializedProgress();
        }
    }
}