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


using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class TariffLimitExceed : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffLimitExceed.ascx"; }
        }

        protected bool IsFreeTariff;
        protected MailWhiteLabelSettings MailWhiteLabelSettings;
        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/TariffSettings/js/tarifflimitexceed.js")
                .RegisterStyle("~/UserControls/Management/TariffSettings/css/tarifflimitexceed.less");

            tariffLimitExceedUsersDialog.Options.IsPopup = true;
            tariffLimitExceedStorageDialog.Options.IsPopup = true;
            tariffLimitExceedFileSizeDialog.Options.IsPopup = true;
            personalLimitExceedStorageDialog.Options.IsPopup = true;

            var quota = TenantExtra.GetTenantQuota();
            IsFreeTariff = (quota.Free || quota.NonProfit || quota.Trial) && !quota.Open;
            MailWhiteLabelSettings = MailWhiteLabelSettings.Instance;

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}