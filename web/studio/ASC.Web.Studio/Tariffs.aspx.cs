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
using System.Web;
using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    public partial class Tariffs : MainPage
    {
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

            if (!TenantExtra.EnableTarrifSettings ||
                (TariffSettings.HidePricingPage &&
                 !CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID)))
                Response.Redirect("~/", true);

            if (TenantExtra.EnableControlPanel)
                Response.Redirect(TenantExtra.GetTariffPageLink(), true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.DisabledSidePanel = true;

            if (TenantStatisticsProvider.IsNotPaid())
            {
                Master.TopStudioPanel.DisableProductNavigation = true;
                Master.TopStudioPanel.DisableSettings = true;
                Master.TopStudioPanel.DisableSearch = true;
                Master.TopStudioPanel.DisableGift = true;
            }

            Title = HeaderStringHelper.GetPageTitle(Resources.Resource.Tariffs);

            if (Request.DesktopApp())
            {
                Master.DisabledTopStudioPanel = true;
                pageContainer.Controls.Add(LoadControl(TariffDesktop.Location));
            }
            else if (CoreContext.Configuration.Standalone)
            {
                pageContainer.Controls.Add(LoadControl(TariffStandalone.Location));
            }
            else
            {
                if (CoreContext.Configuration.CustomMode)
                {
                    pageContainer.Controls.Add(LoadControl(TariffCustom.Location));
                }
                else
                {
                    pageContainer.Controls.Add(LoadControl(TariffUsage.Location));
                }

                var payments = CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID).ToList();
                if (payments.Any()
                    && !TenantExtra.GetTenantQuota().Trial
                    && CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                {
                    var tariffHistory = (TariffHistory)LoadControl(TariffHistory.Location);
                    tariffHistory.Payments = payments;
                    pageContainer.Controls.Add(tariffHistory);
                }
            }
        }
    }
}