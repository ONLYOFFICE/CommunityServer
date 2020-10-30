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
using ASC.Core.Users;
using ASC.Web.People.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using AjaxPro;

namespace ASC.Web.People.UserControls
{
    [AjaxNamespace("ImportUsersController")]
    public partial class ImportUsers : UserControl
    {
        public static string Location
        {
            get { return PeopleProduct.ProductPath + "UserControls/ImportUsers/ImportUsers.ascx"; }
        }

        public enum Operation
        {
            Success = 1,
            Error = 0
        }
        protected int PeopleLimit { get; set; }

        protected bool FreeTariff { get; set; }

        protected string HelpLink { get; set; }

        protected bool EnableInviteLink = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser))
            {
                Response.Redirect(CommonLinkUtility.GetDefault());
            }

            var tariff = (ASC.Web.Studio.UserControls.Management.TariffLimitExceed)LoadControl(Studio.UserControls.Management.TariffLimitExceed.Location);
            Tariff.Controls.Add(tariff);
            var quota = TenantExtra.GetTenantQuota();

            PeopleLimit = Math.Min(quota.ActiveUsers - TenantStatisticsProvider.GetUsersCount(), 0);
            FreeTariff = (quota.Free || quota.NonProfit || quota.Trial) && !quota.Open;
            HelpLink = CommonLinkUtility.GetHelpLink();

            icon.Options.IsPopup = true;
            icon.Options.PopupContainerCssClass = "okcss popupContainerClass";
            icon.Options.OnCancelButtonClick = "ASC.People.Import.hideInfoWindow('okcss');";

            limitPanel.Options.IsPopup = true;
            limitPanel.Options.OnCancelButtonClick = "ASC.People.Import.hideImportUserLimitPanel();";

            Utility.RegisterTypeForAjax(GetType());

            RegisterScript();

        }

        private void RegisterScript()
        {
            Page.RegisterStyle(PeopleProduct.ProductPath + "UserControls/ImportUsers/css/import.less")
                .RegisterBodyScripts("~/js/uploader/ajaxupload.js", "~/js/third-party/xregexp.js",
                PeopleProduct.ProductPath + "UserControls/ImportUsers/js/importusers.js");
        }
    }
}


