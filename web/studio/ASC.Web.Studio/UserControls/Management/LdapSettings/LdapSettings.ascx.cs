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

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.LdapSettings, Location)]
    [AjaxNamespace("LdapSettingsController")]
    public partial class LdapSettings : UserControl
    {
        protected ActiveDirectory.Base.Settings.LdapSettings Settings;

        public const string Location = "~/UserControls/Management/LdapSettings/LdapSettings.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Standalone || !SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(typeof(LdapSettings), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/LdapSettings/js/ldapsettings.js");
            ldapSettingsConfirmContainerId.Options.IsPopup = true;
            ldapSettingsLimitPanelId.Options.IsPopup = true;
            Settings = ActiveDirectory.Base.Settings.LdapSettings.Load();
        }
    }
}