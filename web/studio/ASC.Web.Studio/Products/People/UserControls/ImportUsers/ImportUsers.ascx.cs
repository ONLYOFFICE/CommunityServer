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


