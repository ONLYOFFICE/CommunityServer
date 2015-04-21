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
using System.Web;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.FirstTime;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using Resources;

namespace ASC.Web.Studio
{
    public partial class Wizard : MainPage
    {
        protected override bool MayNotAuth
        {
            get { return true; }
            set { }
        }
        protected override bool CheckWizardCompleted
        {
            get { return false; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            if (wizardSettings.Completed)
                Response.Redirect(CommonLinkUtility.GetDefault());

            if (CoreContext.Configuration.Personal)
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    var owner = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
                    SecurityContext.AuthenticateMe(owner.ID);
                }
                catch (System.Security.Authentication.InvalidCredentialException)
                {
                }
                catch (System.Security.SecurityException)
                {
                }
            }

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            Page.RegisterBodyScripts(ResolveUrl("~/js/third-party/head.load.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/js/asc/core/asc.listscript.js"));

            Title = Resource.WizardPageTitle;

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;

            content.Controls.Add(LoadControl(StepContainer.Location));
        }
    }
}