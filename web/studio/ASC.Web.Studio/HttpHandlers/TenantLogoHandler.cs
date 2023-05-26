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


#region Usings

using System;
using System.Web;

using ASC.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.WhiteLabel;

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

            var whiteLabelType = (WhiteLabelLogoTypeEnum)Convert.ToInt32(type);
            var general = Convert.ToBoolean(context.Request["general"] ?? "true");
            var defaultIfNoWhiteLabel = Convert.ToBoolean(context.Request["defifnoco"] ?? "false");
            var dependsOnInterfaceTheme = Convert.ToBoolean(context.Request["dependsontheme"] ?? "false");

            if (dependsOnInterfaceTheme)
            {
                whiteLabelType = GetRigthLogo(whiteLabelType);
            }

            var imgUrl = GetLogo(whiteLabelType, general, defaultIfNoWhiteLabel);

            context.Response.ContentType = "image";
            context.Response.Redirect(imgUrl);
        }

        private static string GetLogo(WhiteLabelLogoTypeEnum type, bool general = true, bool defaultIfNoWhiteLabel = false)
        {
            if (TenantLogoManager.WhiteLabelEnabled)
            {
                return TenantWhiteLabelSettings.Load().GetAbsoluteLogoPath(type, general);
            }

            if (defaultIfNoWhiteLabel) 
            {
                return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(type, general);
            }

            if ((type == WhiteLabelLogoTypeEnum.Dark || type == WhiteLabelLogoTypeEnum.Light) && !TenantLogoManager.IsVisibleWhiteLabelSettings)
            {
                /*** simple scheme ***/
                return TenantInfoSettings.Load().GetAbsoluteCompanyLogoPath(type == WhiteLabelLogoTypeEnum.Dark);
                /***/
            }

            return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(type, general);
        }

        private static WhiteLabelLogoTypeEnum GetRigthLogo(WhiteLabelLogoTypeEnum type)
        {
            if (!SecurityContext.IsAuthenticated) return type;

            var theme = ModeThemeSettings.GetModeThemesSettings().ModeThemeName;

            if (theme == ModeTheme.light)
            {
                if (type == WhiteLabelLogoTypeEnum.Light)
                    return WhiteLabelLogoTypeEnum.Dark;

                if (type == WhiteLabelLogoTypeEnum.AboutLight)
                    return WhiteLabelLogoTypeEnum.AboutDark;
            }
            else
            {
                if (type == WhiteLabelLogoTypeEnum.Dark)
                    return WhiteLabelLogoTypeEnum.Light;

                if (type == WhiteLabelLogoTypeEnum.AboutDark)
                    return WhiteLabelLogoTypeEnum.AboutLight;
            }

            return type;
        }
    }
}
