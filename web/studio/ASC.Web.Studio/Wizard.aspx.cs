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


using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.FirstTime;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.IO;
using System.Web;

namespace ASC.Web.Studio
{
    public partial class Wizard : MainPage
    {
        protected override bool MayNotAuth
        {
            get { return true; }
        }

        protected override bool MayNotPaid
        {
            get { return true; }
        }

        protected override bool CheckWizardCompleted
        {
            get { return false; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            var wizardSettings = WizardSettings.Load();
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

            Page.RegisterBodyScripts("~/js/third-party/head.load.js",
                "~/js/asc/core/asc.listscript.js");

            Title = Resource.WizardPageTitle;

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableGift = true;

            content.Controls.Add(LoadControl(StepContainer.Location));
        }
    }
}