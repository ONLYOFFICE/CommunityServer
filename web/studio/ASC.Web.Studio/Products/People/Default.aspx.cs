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
using System.Configuration;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.People.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
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

        protected bool DisplayPayments { get; private set; }

        protected bool DisplayPaymentsFirst { get; private set; }

        protected string HelpLink { get; set; }

        public AllowedActions Actions;

        protected void Page_Load(object sender, EventArgs e)
        {
            var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            IsAdmin = userInfo.IsAdmin() || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, userInfo.ID);
            Actions = new AllowedActions(userInfo);

            var quota = TenantExtra.GetTenantQuota();
            IsFreeTariff = quota.Free && !quota.Open;

            DisplayPayments = TenantExtra.EnableTarrifSettings && (!CoreContext.Configuration.Standalone || quota.ActiveUsers != LicenseReader.MaxUserCount);

            if (DisplayPayments)
            {
                int notifyCount;
                int.TryParse(ConfigurationManagerExtension.AppSettings["web.tariff-notify.user"] ?? "5", out notifyCount);
                DisplayPaymentsFirst = notifyCount > 0 && quota.ActiveUsers - TenantStatisticsProvider.GetUsersCount() < notifyCount;
            }

            var controlEmailChange = (UserEmailChange) LoadControl(UserEmailChange.Location);
            controlEmailChange.UserInfo = userInfo;
            controlEmailChange.RegisterStylesAndScripts = true;
            userEmailChange.Controls.Add(controlEmailChange);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
            userConfirmationDelete.Controls.Add(LoadControl(ConfirmationDeleteUser.Location));

            if (Actions.AllowEdit)
            {
                userPwdChange.Controls.Add(LoadControl(PwdTool.Location));
            }
            Title = HeaderStringHelper.GetPageTitle(PeopleResource.ProductName);

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}