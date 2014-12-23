/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users;
using System.IO;
using ASC.Web.Core.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    internal class LogoUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    var logo = context.Request.Files[0];
                    var data = new byte[logo.InputStream.Length];

                    var br = new BinaryReader(logo.InputStream);
                    br.Read(data, 0, (int)logo.InputStream.Length);
                    br.Close();

                    var ap = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, 250, 100);

                    result.Success = true;
                    result.Message = ap;
                }
                else
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch(ImageWeightLimitException)
            {
                result.Success = false;
                result.Message = Resource.ErrorImageWeightLimit;
            }
            catch(ImageSizeLimitException)
            {
                result.Success = false;
                result.Message = Resource.ErrorImageSizetLimit;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        #endregion
    }

    [AjaxNamespace("GreetingSettingsController")]
    public partial class GreetingSettingsContent : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/GreetingSettings/GreetingSettingscontent.ascx"; }
        }

        protected TenantInfoSettings _tenantInfoSettings;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/greetingsettings/js/greetingsettingscontent.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/greetingsettings/css/greetingsettings.less"));

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            RegisterScript();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveGreetingSettings(string logoVP, string header)
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

                var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
                currentTenant.Name = header;
                CoreContext.TenantManager.SaveTenant(currentTenant);

                SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);

                MessageService.Send(HttpContext.Current.Request, MessageAction.GreetingSettingsUpdated);

                return new {Status = 1, Message = Resource.SuccessfullySaveGreetingSettingsMessage};
            }
            catch(Exception e)
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

                _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                _tenantInfoSettings.RestoreDefault();
                SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);

                return new
                    {
                        Status = 1,
                        Message = Resource.SuccessfullySaveGreetingSettingsMessage,
                        LogoPath = _tenantInfoSettings.GetAbsoluteCompanyLogoPath(),
                        CompanyName = CoreContext.TenantManager.GetCurrentTenant().Name,
                    };
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"jq('input.fileuploadinput').attr('accept', 'image/png,image/jpeg,image/gif');");
        }
    }
}