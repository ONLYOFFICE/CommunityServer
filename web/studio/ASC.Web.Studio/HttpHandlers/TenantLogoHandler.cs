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


#region Usings

using ASC.Web.Core.CoBranding;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Web;

#endregion

namespace ASC.Web.Studio.HttpHandlers
{
    public class TenantLogoHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var type = context.Request["logotype"];
            if (string.IsNullOrEmpty(type)) return;

            var imgUrl = "";

            var coBrandingType = (CoBrandingLogoTypeEnum)Convert.ToInt32(type);
            var general = Convert.ToBoolean(context.Request["general"] ?? "true");
            var isDefIfNoCoBranding = Convert.ToBoolean(context.Request["defifnoco"] ?? "false");


            if (TenantLogoManager.CoBrandingEnabled)
            {
                var _tenantCoBrandingSettings = SettingsManager.Instance.LoadSettings<TenantCoBrandingSettings>(TenantProvider.CurrentTenantID);
                imgUrl = _tenantCoBrandingSettings.GetAbsoluteLogoPath(coBrandingType, general);

                if (coBrandingType == CoBrandingLogoTypeEnum.Dark)
                {
                    var defaultDarkLogoPath = TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum.Dark, general);

                    if (String.Equals(imgUrl, defaultDarkLogoPath, StringComparison.OrdinalIgnoreCase))
                    {
                        /*** simple scheme ***/
                        var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                        imgUrl = _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
                        /***/
                    }
                }
            }
            else
            {
                if (isDefIfNoCoBranding)
                {
                    imgUrl = TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(coBrandingType, general);
                }
                else
                {
                    if (coBrandingType == CoBrandingLogoTypeEnum.Dark)
                    {
                        /*** simple scheme ***/
                        var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                        imgUrl = _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
                        /***/
                    }
                    else
                    {
                        imgUrl = TenantCoBrandingSettings.GetAbsoluteDefaultLogoPath(coBrandingType, general);
                    }
                }
            }

            context.Response.ContentType = "image";
            context.Response.Redirect(imgUrl);
        }
    }
}
