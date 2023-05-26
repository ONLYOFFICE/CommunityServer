/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.LdapSettings, Location)]
    [AjaxNamespace("LdapSettingsController")]
    public partial class LdapSettings : UserControl
    {
        protected ActiveDirectory.Base.Settings.LdapSettings Settings;
        protected string LdapMapping;

        public const string Location = "~/UserControls/Management/LdapSettings/LdapSettings.ascx";

        protected bool isAvailable
        {
            get
            {
                return CoreContext.Configuration.Standalone || TenantExtra.GetTenantQuota().Ldap;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(typeof(LdapSettings), Page);
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle(
                "~/UserControls/Management/LdapSettings/css/Default/dark-ldapsettings.less");
            }
            else
            {
                Page.RegisterStyle(
                "~/UserControls/Management/LdapSettings/css/Default/ldapsettings.less");
            }
            Page.RegisterStyle("~/UserControls/Management/LdapSettings/css/Default/jqCron.css");

            Page.RegisterBodyScripts(
                "~/js/third-party/moment.min.js", "~/js/third-party/moment-timezone.min.js",
                "~/js/third-party/jquery/jquery.cron.js",
                "~/UserControls/Management/LdapSettings/js/ldapsettings.js"
                );
            ldapSettingsConfirmContainerId.Options.IsPopup = true;
            ldapSettingsLimitPanelId.Options.IsPopup = true;
            ldapSettingsCronPanel.Options.IsPopup = true;
            ldapSettingsCronTurnOffContainer.Options.IsPopup = true;
            ldapSettingsTurnOffContainer.Options.IsPopup = true;
            ldapSettingsCertificateValidationContainer.Options.IsPopup = true;

            Settings = ActiveDirectory.Base.Settings.LdapSettings.Load();

            LdapMapping = JsonConvert.SerializeObject(Settings.LdapMapping, Formatting.Indented).ToString();
        }
    }
}