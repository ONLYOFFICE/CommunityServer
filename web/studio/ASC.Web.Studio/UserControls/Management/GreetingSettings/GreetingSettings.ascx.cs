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
using ASC.MessagingSystem;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("GreetingSettingsController")]
    public partial class GreetingSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/GreetingSettings/GreetingSettings.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/UserControls/Management/GreetingSettings/js/greetingsettings.js"))
                .RegisterStyle("~/UserControls/Management/GreetingSettings/css/greetingsettings.less");

            if (GreetingLogoSettings.AvailableControl)
            {
                logoSettingsContainer.Controls.Add(LoadControl(GreetingLogoSettings.Location));
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveGreetingSettings(string header)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
                currentTenant.Name = header;
                CoreContext.TenantManager.SaveTenant(currentTenant);

                MessageService.Send(HttpContext.Current.Request, MessageAction.GreetingSettingsUpdated);

                return new {Status = 1, Message = Resource.SuccessfullySaveGreetingSettingsMessage};
            }
            catch (Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object RestoreGreetingSettings()
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var _tenantInfoSettings = TenantInfoSettings.Load();
                _tenantInfoSettings.RestoreDefaultTenantName();
                //_tenantInfoSettings.Save();

                return new
                    {
                        Status = 1,
                        Message = Resource.SuccessfullySaveGreetingSettingsMessage,
                        CompanyName = CoreContext.TenantManager.GetCurrentTenant().Name
                    };
            }
            catch (Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}