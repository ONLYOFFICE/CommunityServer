/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management.IpSecurity
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 200)]
    public partial class IpSecurity : UserControl
    {
        public const string Location = "~/UserControls/Management/IpSecurity/IpSecurity.ascx";

        protected bool Enabled
        {
            get { return IPSecurity.IPSecurity.IpSecurityEnabled; }
        }

        protected IPRestrictionsSettings RestrictionsSettings = IPRestrictionsSettings.Load();

        protected bool TenantAccessAnyone;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Enabled) return;

            Page.RegisterBodyScripts("~/UserControls/Management/IpSecurity/js/ipsecurity.js")
                .RegisterStyle("~/UserControls/Management/IpSecurity/css/ipsecurity.less");

            var managementPage = Page as Studio.Management;
            TenantAccessAnyone = managementPage != null ?
                                     managementPage.TenantAccess.Anyone :
                                     TenantAccessSettings.Load().Anyone;
        }
    }
}