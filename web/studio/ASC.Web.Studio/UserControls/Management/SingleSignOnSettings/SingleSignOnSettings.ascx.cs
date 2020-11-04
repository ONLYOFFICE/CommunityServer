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
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management.SingleSignOnSettings
{
    //TODO: Remove this or re-write like in Control Panel? 

    [ManagementControl(ManagementType.PortalSecurity, ManagementType.SingleSignOnSettings, Location, SortOrder = 100)]
    [AjaxNamespace("SsoSettingsController")]
    public partial class SingleSignOnSettings : UserControl
    {
        protected SsoSettingsV2 Settings { get; private set; }

        protected const string Location = "~/UserControls/Management/SingleSignOnSettings/SingleSignOnSettings.ascx";
        
        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Standalone || !SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(typeof(SingleSignOnSettings), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/SingleSignOnSettings/js/singlesignonsettings.js");

            Settings = SsoSettingsV2.Load();

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}