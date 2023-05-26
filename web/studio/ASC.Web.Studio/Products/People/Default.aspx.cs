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


using System;
using System.Configuration;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.People.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Web.Studio.UserControls.Management.ImpersonateUser;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

namespace ASC.Web.People
{
    public partial class Default : MainPage
    {
        protected bool IsAdmin { get; private set; }

        protected bool IsFreeTariff { get; private set; }

        protected bool IsStandalone { get; private set; }

        protected bool DisplayPayments { get; private set; }

        protected bool DisplayPaymentsFirstUser { get; private set; }

        protected bool DisplayPaymentsFirstGuest { get; private set; }

        protected string HelpLink { get; set; }

        public AllowedActions Actions;

        protected void Page_Load(object sender, EventArgs e)
        {
            var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var isFullADmin = userInfo.IsAdmin();

            IsAdmin = isFullADmin || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, userInfo.ID);
            Actions = new AllowedActions(userInfo);

            var quota = TenantExtra.GetTenantQuota();
            IsFreeTariff = quota.Free && !quota.Open;
            IsStandalone = CoreContext.Configuration.Standalone;

            DisplayPayments = TenantExtra.EnableTariffSettings && (!CoreContext.Configuration.Standalone || quota.ActiveUsers != Constants.MaxEveryoneCount);

            if (DisplayPayments)
            {
                int notifyCount;
                int.TryParse(ConfigurationManagerExtension.AppSettings["web.tariff-notify.user"] ?? "5", out notifyCount);
                DisplayPaymentsFirstUser = notifyCount > 0 && quota.ActiveUsers - TenantStatisticsProvider.GetUsersCount() < notifyCount;
                DisplayPaymentsFirstGuest = !IsStandalone && notifyCount > 0 && quota.ActiveUsers * Constants.CoefficientOfVisitors - TenantStatisticsProvider.GetVisitorsCount() < notifyCount;
            }

            var controlEmailChange = (UserEmailChange)LoadControl(UserEmailChange.Location);
            controlEmailChange.UserInfo = userInfo;
            controlEmailChange.RegisterStylesAndScripts = true;
            userEmailChange.Controls.Add(controlEmailChange);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
            userConfirmationDelete.Controls.Add(LoadControl(ConfirmationDeleteUser.Location));

            if (ImpersonationSettings.CanImpersonate(userInfo, out _))
            {
                confirmationImpersonateUser.Controls.Add(LoadControl(ImpersonateUserConfirmationPanel.Location));

                Page.RegisterInlineScript("window.canImpersonate = true;");
            }

            if (Actions.AllowEdit)
            {
                userPwdChange.Controls.Add(LoadControl(PwdTool.Location));
            }
            Title = HeaderStringHelper.GetPageTitle(PeopleResource.ProductName);

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}