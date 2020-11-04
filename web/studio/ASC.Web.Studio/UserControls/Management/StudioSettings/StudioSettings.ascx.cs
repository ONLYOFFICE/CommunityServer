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
using System.Web.UI;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Customization, Location)]
    public partial class StudioSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/StudioSettings/StudioSettings.ascx";

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            //timezone & language
            _timelngHolder.Controls.Add(LoadControl(TimeAndLanguage.Location));

            if (SetupInfo.IsVisibleSettings<PromoCode>() &&
                TenantExtra.GetCurrentTariff().State == ASC.Core.Billing.TariffState.Trial &&
                string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().PartnerId))
            {
                promoCodeSettings.Controls.Add(LoadControl(PromoCode.Location));
            }

            //dns
            if (SetupInfo.IsVisibleSettings<DnsSettings.DnsSettings>())
                _dnsSettings.Controls.Add(LoadControl(DnsSettings.DnsSettings.Location));

            //Portal version
            if (SetupInfo.IsVisibleSettings<VersionSettings.VersionSettings>() && 1 < CoreContext.TenantManager.GetTenantVersions().Count())
                _portalVersionSettings.Controls.Add(LoadControl(VersionSettings.VersionSettings.Location));

            //greeting settings
            _greetingSettings.Controls.Add(LoadControl(GreetingSettings.Location));

            //portal rename control
            _portalRename.Controls.Add(LoadControl(PortalRename.Location));

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}