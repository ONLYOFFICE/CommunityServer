/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
                int.TryParse(ConfigurationManager.AppSettings["web.tariff-notify.user"] ?? "5", out notifyCount);
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