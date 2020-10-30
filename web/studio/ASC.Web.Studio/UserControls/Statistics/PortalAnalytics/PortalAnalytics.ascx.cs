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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Studio.UserControls.Statistics
{
    [ManagementControl(ManagementType.Statistic, Location, SortOrder = 100)]
    public partial class PortalAnalytics : UserControl
    {
        public const string Location = "~/UserControls/Statistics/PortalAnalytics/PortalAnalytics.ascx";

        protected bool Enabled;

        protected bool SwitchedOn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.CustomMode)
                return;

            Page
                .RegisterStyle("~/UserControls/Statistics/PortalAnalytics/css/portalanalytics.less")
                .RegisterBodyScripts("~/UserControls/Statistics/PortalAnalytics/js/portalanalytics.js");

            if (TenantExtra.Opensource)
            {
                Enabled = true;
                SwitchedOn = WizardSettings.Load().Analytics;
            }
            else if (TenantExtra.Saas && SetupInfo.CustomScripts.Length != 0)
            {
                Enabled = true;
                SwitchedOn = TenantAnalyticsSettings.Load().Analytics;
            }
        }
    }
}