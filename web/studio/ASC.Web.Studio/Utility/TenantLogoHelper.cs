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


using ASC.Core.Common.Settings;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Web.Studio.Utility
{
    public class TenantLogoHelper
    {
        public static string GetLogo(WhiteLabelLogoTypeEnum type, bool general = true, bool isDefIfNoWhiteLabel = false)
        {
            var imgUrl = "";
            if (TenantLogoManager.WhiteLabelEnabled)
            {
                var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
                return _tenantWhiteLabelSettings.GetAbsoluteLogoPath(type, general);
            }
            else
            {
                if (isDefIfNoWhiteLabel)
                {
                    imgUrl = TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(type, general);
                }
                else
                {
                    if (type == WhiteLabelLogoTypeEnum.Dark)
                    {
                        /*** simple scheme ***/
                        var _tenantInfoSettings = TenantInfoSettings.Load();
                        imgUrl = _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
                        /***/
                    }
                    else
                    {
                        imgUrl = TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(type, general);
                    }
                }
            }

            return imgUrl;

        }
    }
}
