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
using System.Web.UI;
using System.Web;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using AjaxPro;
using ASC.Web.Core.Utility.Settings;
using ASC.Core;
using ASC.Web.Core.Users;
using System.IO;
using ASC.MessagingSystem;
using Resources;
using ASC.Web.Core.WhiteLabel;

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
            Page.RegisterBodyScripts("~/js/uploader/ajaxupload.js");
            Page.RegisterBodyScripts("~/usercontrols/management/greetingsettings/js/greetinglogosettings.js");

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            RegisterScript();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveGreetingLogoSettings(string logoVP)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

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

                SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);

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

                _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                _tenantInfoSettings.RestoreDefault();
                SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);

                if (TenantLogoManager.WhiteLabelEnabled)
                {
                    var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
                    _tenantWhiteLabelSettings.RestoreDefault(WhiteLabelLogoTypeEnum.Dark);
                    _tenantWhiteLabelSettings.Save();
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
