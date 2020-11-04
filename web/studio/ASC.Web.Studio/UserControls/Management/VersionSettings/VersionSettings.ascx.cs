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
using System.Linq;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management.VersionSettings
{
    [AjaxNamespace("VersionSettingsController")]
    public partial class VersionSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/VersionSettings/VersionSettings.ascx";

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            HelpLink = CommonLinkUtility.GetHelpLink();

            Page.RegisterStyle("~/UserControls/Management/VersionSettings/css/versionsettings.less")
                .RegisterBodyScripts("~/UserControls/Management/VersionSettings/js/script.js");
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SwitchVersion(string version)
        {
            try
            {
                var tenantVersion = int.Parse(version);

                if (CoreContext.TenantManager.GetTenantVersions().All(x => x.Id != tenantVersion))
                    throw new ArgumentException(Resource.SettingsBadPortalVersion);

                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                try
                {
                    CoreContext.TenantManager.SetTenantVersion(tenant, tenantVersion);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(Resource.SettingsAlreadyCurrentPortalVersion, e);
                }
                return new { Status = 1 };
            }
            catch (Exception e)
            {
                return new { Status = 0, e.Message };
            }
        }

        protected string GetLocalizedName(string name)
        {
            try
            {
                var localizedName = Resource.ResourceManager.GetString(("version_" + name.Replace(".", "")).ToLowerInvariant());
                if (string.IsNullOrEmpty(localizedName))
                {
                    localizedName = name;
                }
                return localizedName;
            }
            catch (Exception)
            {
                return name;
            }
        }
    }
}