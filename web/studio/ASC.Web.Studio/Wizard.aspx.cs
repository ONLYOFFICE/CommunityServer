/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            get { return CoreContext.Configuration.Standalone; }
            set { }
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
            if (!SecurityContext.IsAuthenticated && CoreContext.Configuration.Standalone)
            {
                try
                {
                    SecurityContext.AuthenticateMe(UserManagerWrapper.AdminID.ToString(), "admin");
                }
                catch (System.Security.Authentication.InvalidCredentialException)
                {
                }
                catch (System.Security.SecurityException)
                {
                }
            }

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            Page.RegisterBodyScripts(ResolveUrl("~/js/third-party/head.load.min.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/js/asc/core/asc.listscript.js"));

            Title = Resource.WizardPageTitle;

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableVideo = true;

            content.Controls.Add(LoadControl(StepContainer.Location));
        }
    }
}