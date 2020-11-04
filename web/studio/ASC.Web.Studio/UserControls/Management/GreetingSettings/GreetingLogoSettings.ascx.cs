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
using System.IO;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.MessagingSystem;
using ASC.Web.Core.Users;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("GreetingLogoSettingsController")]
    public partial class GreetingLogoSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/GreetingSettings/GreetingLogoSettings.ascx";

        public static bool AvailableControl
        {
            get
            {
                return !TenantLogoManager.WhiteLabelEnabled;
            }
        }

        protected TenantInfoSettings _tenantInfoSettings;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AvailableControl)
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                "~/UserControls/Management/GreetingSettings/js/greetinglogosettings.js");

            _tenantInfoSettings = TenantInfoSettings.Load();

            RegisterScript();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveGreetingLogoSettings(string logoVP)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                _tenantInfoSettings = TenantInfoSettings.Load();

                if (!String.IsNullOrEmpty(logoVP))
                {
                    var fileName = Path.GetFileName(logoVP);
                    var data = UserPhotoManager.GetTempPhotoData(fileName);
                    _tenantInfoSettings.SetCompanyLogo(fileName, data);

                    try
                    {
                        UserPhotoManager.RemoveTempPhoto(fileName);
                    }
                    catch
                    {
                    }
                }

                _tenantInfoSettings.Save();

                MessageService.Send(HttpContext.Current.Request, MessageAction.GreetingSettingsUpdated);

                return new { Status = 1, Message = Resource.SuccessfullySaveGreetingSettingsMessage };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object RestoreGreetingLogoSettings()
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
                var tenantId = TenantProvider.CurrentTenantID;

                _tenantInfoSettings = TenantInfoSettings.Load();
                _tenantInfoSettings.RestoreDefault();
                _tenantInfoSettings.Save();

                if (TenantLogoManager.WhiteLabelEnabled)
                {

                    var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                    _tenantWhiteLabelSettings.RestoreDefault(WhiteLabelLogoTypeEnum.Dark);
                    _tenantWhiteLabelSettings.Save(tenantId);
                }

                return new
                {
                    Status = 1,
                    Message = Resource.SuccessfullySaveGreetingSettingsMessage,
                    LogoPath = _tenantInfoSettings.GetAbsoluteCompanyLogoPath()
                };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"jq('input.fileuploadinput').attr('accept', 'image/png,image/jpeg,image/gif');");
        }
    }
}
