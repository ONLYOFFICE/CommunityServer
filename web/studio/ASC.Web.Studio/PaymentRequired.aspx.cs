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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Core.Files;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Web;

namespace ASC.Web.Studio
{
    public partial class PaymentRequired : MainPage
    {
        public static string Location
        {
            get { return "~/PaymentRequired.aspx"; }
        }

        protected AdditionalWhiteLabelSettings Settings;

        protected override bool MayNotAuth
        {
            get { return true; }
        }

        protected override bool MayNotPaid
        {
            get { return true; }
            set { }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (CoreContext.Configuration.Personal)
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);

            if (TenantExtra.GetCurrentTariff().State < TariffState.NotPaid)
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableLoginPersonal = true;
            Master.TopStudioPanel.DisableGift = true;

            Title = HeaderStringHelper.GetPageTitle(Resource.PaymentRequired);

            Page.RegisterStyle("~/UserControls/Management/TariffSettings/css/tariff.less");
            Page.RegisterStyle("~/UserControls/Management/TariffSettings/css/tariffstandalone.less");

            Settings = AdditionalWhiteLabelSettings.Instance;
        }
    }
}