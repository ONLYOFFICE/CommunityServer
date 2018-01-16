/*
 *
 * (c) Copyright Ascensio System Limited 2010-2017
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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffNotifyController")]
    public partial class TariffNotify : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffNotify.ascx"; }
        }

        protected Tuple<string, string> Notify = null;
        protected bool CanClose = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SecurityContext.IsAuthenticated
                && TenantExtra.EnableTarrifSettings
                && !TariffSettings.HideNotify
                && !CoreContext.Configuration.Personal
                && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                Page.RegisterStyle("~/usercontrols/management/tariffsettings/css/tariffnotify.less");

                Notify = GetTariffNotify();

                if (CanClose)
                {
                    Page.RegisterBodyScripts("~/usercontrols/management/tariffsettings/js/tariffnotify.js");
                    AjaxPro.Utility.RegisterTypeForAjax(GetType());
                }
            }
        }

        private Tuple<string, string> GetTariffNotify()
        {
            var hidePricingPage = !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin() && TariffSettings.HidePricingPage;

            var tariff = TenantExtra.GetCurrentTariff();

            var count = tariff.DueDate.Date.Subtract(DateTime.Today).Days;
            if (tariff.State == TariffState.Trial)
            {
                if (!hidePricingPage && count <= 5)
                {
                    var text = String.Format(CoreContext.Configuration.Standalone ? Resource.TariffLinkStandalone : Resource.TrialPeriodInfoText,
                                             "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");

                    if (count <= 0)
                        return new Tuple<string, string>(Resource.TrialPeriodExpired, text);

                    var end = GetNumeralResourceByCount(count, Resource.Day, Resource.DaysOne, Resource.DaysTwo);
                    return new Tuple<string, string>(string.Format(Resource.TrialPeriod, count, end), text);
                }

                if (CoreContext.Configuration.Standalone)
                {
                    return new Tuple<string, string>(Resource.TrialPeriodInfoTextLicense, string.Empty);
                }
            }

            if (!hidePricingPage && tariff.State == TariffState.Paid)
            {
                if (CoreContext.Configuration.Standalone)
                {
                    CanClose = true;
                    var text = String.Format(Resource.TariffLinkStandaloneLife,
                                             "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");
                    if (count <= 0)
                        return new Tuple<string, string>(Resource.PaidPeriodExpiredStandaloneLife, text);

                    if (count < 10)
                    {
                        var end = GetNumeralResourceByCount(count, Resource.Day, Resource.DaysOne, Resource.DaysTwo);
                        return new Tuple<string, string>(string.Format(Resource.PaidPeriodStandaloneLife, count, end), text);
                    }
                }
                else
                {
                    var quota = TenantExtra.GetTenantQuota();
                    long notifySize;
                    long.TryParse(ConfigurationManager.AppSettings["web.tariff-notify.storage"] ?? "314572800", out notifySize); //300 MB
                    if (notifySize > 0 && quota.MaxTotalSize - TenantStatisticsProvider.GetUsedSize() < notifySize)
                    {
                        var head = string.Format(Resource.TariffExceedLimit, FileSizeComment.FilesSizeToString(quota.MaxTotalSize));
                        var text = String.Format(Resource.TariffExceedLimitInfoText, "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");
                        return new Tuple<string, string>(head, text);
                    }
                }
            }

            if (!hidePricingPage && tariff.State == TariffState.Delay)
            {
                var text = String.Format(Resource.TariffPaymentDelayText,
                                         "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>",
                                         tariff.DelayDueDate.Date.ToLongDateString());
                return new Tuple<string, string>(Resource.TariffPaymentDelay, text);
            }

            return null;
        }

        public static string GetNumeralResourceByCount(int count, string resource, string resourceOne, string resourceTwo)
        {
            var num = count%100;
            if (num >= 11 && num <= 19)
            {
                return resourceTwo;
            }

            var i = count%10;
            switch (i)
            {
                case (1):
                    return resource;
                case (2):
                case (3):
                case (4):
                    return resourceOne;
                default:
                    return resourceTwo;
            }
        }

        [AjaxMethod]
        public void HideNotify()
        {
            TariffSettings.HideNotify = true;
        }
    }
}