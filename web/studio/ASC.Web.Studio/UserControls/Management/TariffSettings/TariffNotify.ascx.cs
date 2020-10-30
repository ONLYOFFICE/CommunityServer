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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
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
            if (CoreContext.Configuration.Personal && SetupInfo.IsVisibleSettings("PersonalMaxSpace"))
            {
                Notify = GetPersonalTariffNotify();
                return;
            }
            
            if (SecurityContext.IsAuthenticated
                && TenantExtra.EnableTarrifSettings
                && !TariffSettings.HideNotify
                && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                Page.RegisterStyle("~/UserControls/Management/TariffSettings/css/tariffnotify.less");

                Notify = GetTariffNotify();

                if (CanClose)
                {
                    Page.RegisterBodyScripts("~/UserControls/Management/TariffSettings/js/tariffnotify.js");
                    AjaxPro.Utility.RegisterTypeForAjax(GetType());
                }
            }
        }

        private Tuple<string, string> GetPersonalTariffNotify()
        {
            var maxTotalSize = CoreContext.Configuration.PersonalMaxSpace;

            var webItem = WebItemManager.Instance[WebItemManager.DocumentsProductID];
            var spaceUsageManager = webItem.Context.SpaceUsageStatManager as IUserSpaceUsage;
            
            if (spaceUsageManager == null) return null;
            
            var usedSize = spaceUsageManager.GetUserSpaceUsage(SecurityContext.CurrentAccount.ID);

            long notifySize;
            long.TryParse(ConfigurationManagerExtension.AppSettings["web.tariff-notify.storage"] ?? "104857600", out notifySize); //100 MB

            if (notifySize > 0 && maxTotalSize - usedSize < notifySize)
            {
                var head = string.Format(Resource.PersonalTariffExceedLimit, FileSizeComment.FilesSizeToString(maxTotalSize));

                string text;

                if (CoreContext.Configuration.CustomMode)
                {
                    text = string.Format(Resource.PersonalTariffExceedLimitInfoText, string.Empty, string.Empty, "</br>");
                }
                else
                {
                    var settings = MailWhiteLabelSettings.Instance;
                    var supportLink = string.Format("<a target=\"_blank\" href=\"{0}\">", settings.SupportUrl);
                    text = string.Format(Resource.PersonalTariffExceedLimitInfoText, supportLink, "</a>", "</br>");
                }

                return new Tuple<string, string>(head, text);
            }

            return null;
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
                    long.TryParse(ConfigurationManagerExtension.AppSettings["web.tariff-notify.storage"] ?? "314572800", out notifySize); //300 MB
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