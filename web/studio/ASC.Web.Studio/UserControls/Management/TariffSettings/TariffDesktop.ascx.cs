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
using ASC.Core;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class TariffDesktop : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffDesktop.ascx"; }
        }

        protected string TenantName;

        protected string LogoPath
        {
            get { return String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.Dark, (!TenantLogoManager.IsRetina(Request)).ToString().ToLower()); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page
                .RegisterBodyScripts("~/UserControls/Management/TariffSettings/js/tariffdesktop.js")
                .RegisterStyle("~/UserControls/Management/TariffSettings/css/tariffdesktop.less");

            TenantName = CoreContext.TenantManager.GetCurrentTenant().Name;
        }
    }
}