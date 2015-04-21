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
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using ASC.Core;
using AjaxPro;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.UserControls.Management.CoBranding.Resources;
using ASC.Web.Core.Users;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ASC.Web.Core.CoBranding;


namespace ASC.Web.UserControls.CoBranding
{
    [AjaxNamespace("AjaxPro.CoBranding")]
    public partial class CoBranding : UserControl
    {
        public const string Location = "~/UserControls/Management/CoBranding/CoBranding.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/cobranding/js/cobranding.js"));

            RegisterScript();
        }

        private bool? isRetina;
        protected bool IsRetina
        {
            get
            {
                if (isRetina.HasValue) return isRetina.Value;
                isRetina = TenantLogoManager.IsRetina(Request);
                return isRetina.Value;
            }
            set
            {
                isRetina = value;
            }
        }

        private void RegisterScript()
        {
            var coBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);
            Page.RegisterInlineScript(@"jq('input.fileuploadinput').attr('accept', 'image/png,image/jpeg,image/gif');");
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveCoBrandingSettings(bool isRetina, List<int> logoTypeList, List<string> logoPathList)
        {
            try
            {
                if (logoTypeList == null || logoPathList == null || logoTypeList.Count != logoPathList.Count)
                    throw new Exception("Incorrect data");

                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);

                for (var i = 0; i < logoTypeList.Count; i++)
                {
                    var currentLogoType = (CoBrandingLogoTypeEnum)logoTypeList[i];
                    if (!String.IsNullOrEmpty(logoPathList[i]))
                    {
                        var fileName = Path.GetFileName(logoPathList[i]);
                        var data = UserPhotoManager.GetTempPhotoData(fileName);

                        var fileExt = fileName.Split('.').Last();
                        _tenantCoBrandingSettings.SetLogo(currentLogoType, fileExt, data);

                        try
                        {
                            UserPhotoManager.RemoveTempPhoto(fileName);
                        }
                        catch (Exception ex)
                        {
                            log4net.LogManager.GetLogger("ASC").Error(ex);
                        }
                    }
                }

                SettingsManager.Instance.SaveSettings(_tenantCoBrandingSettings, TenantProvider.CurrentTenantID);

                return new {
                    Status = 1,
                    Message = CoBrandingResource.SuccessfullySaveCoBrandingSettingsMessage,
                    LogoPath = JsonConvert.SerializeObject(
                    new Dictionary<int, string>() {
                            {(int)CoBrandingLogoTypeEnum.Light, _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Light, !isRetina)},
                            {(int)CoBrandingLogoTypeEnum.LightSmall, _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.LightSmall, !isRetina)},
                            {(int)CoBrandingLogoTypeEnum.Dark, _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Dark, !isRetina)},
                            {(int)CoBrandingLogoTypeEnum.Favicon, _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.Favicon, !isRetina)},
                            {(int)CoBrandingLogoTypeEnum.DocsEditor, _tenantCoBrandingSettings.GetAbsoluteLogoPath(CoBrandingLogoTypeEnum.DocsEditor, !isRetina)}
                    })
                };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object RestoreCoBrandingOptions(bool isRetina)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);
                _tenantCoBrandingSettings.RestoreDefault();
                SettingsManager.Instance.SaveSettings(_tenantCoBrandingSettings, TenantProvider.CurrentTenantID);


                /*** simple scheme restore logo***/
                var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                _tenantInfoSettings.RestoreDefaultLogo();
                SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);
                /***/

                return new
                {
                    Status = 1,
                    Message = CoBrandingResource.SuccessfullySaveCoBrandingSettingsMessage,
                    LogoPath = JsonConvert.SerializeObject(
                    new Dictionary<int,string>() {
                       {(int)CoBrandingLogoTypeEnum.Light, TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Light, !isRetina)},
                       {(int)CoBrandingLogoTypeEnum.LightSmall, TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.LightSmall, !isRetina)},
                       {(int)CoBrandingLogoTypeEnum.Dark, TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Dark, !isRetina)},
                       {(int)CoBrandingLogoTypeEnum.Favicon, TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Favicon, !isRetina)},
                       {(int)CoBrandingLogoTypeEnum.DocsEditor, TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.DocsEditor, !isRetina)}
                    })
                };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }


    }
}