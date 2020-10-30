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


using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffStandaloneController")]
    public partial class TariffStandalone : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffStandalone.ascx"; }
        }

        protected int UsersCount;
        protected Tariff CurrentTariff;
        protected TenantQuota CurrentQuota;
        protected AdditionalWhiteLabelSettings Settings;
        protected int TenantCount;

        protected bool RequestLicenseAccept
        {
            get { return !TariffSettings.LicenseAccept && Settings.LicenseAgreementsEnabled; }
        }

        protected bool PeopleModuleAvailable
        {
            get
            {
                var peopleProduct = WebItemManager.Instance[WebItemManager.PeopleProductID];
                return peopleProduct != null && !peopleProduct.IsDisabled();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page
                .RegisterBodyScripts(
                    "~/js/uploader/jquery.fileupload.js",
                    "~/UserControls/Management/TariffSettings/js/tariffstandalone.js")
                .RegisterStyle("~/UserControls/Management/TariffSettings/css/tariff.less",
                    "~/UserControls/Management/TariffSettings/css/tariffstandalone.less");

            UsersCount = TenantStatisticsProvider.GetUsersCount();
            CurrentTariff = TenantExtra.GetCurrentTariff();
            CurrentQuota = TenantExtra.GetTenantQuota();
            TenantCount = CoreContext.TenantManager.GetTenants().Count();

            Settings = AdditionalWhiteLabelSettings.Instance;
            Settings.LicenseAgreementsUrl = CommonLinkUtility.GetRegionalUrl(Settings.LicenseAgreementsUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            Settings.FeedbackAndSupportUrl = CommonLinkUtility.GetRegionalUrl(Settings.FeedbackAndSupportUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        protected string TariffDescription()
        {
            if (TenantExtra.UpdatedWithoutLicense)
            {
                return String.Format(UserControlsCommonResource.TariffUpdateWithoutLicense.HtmlEncode(),
                                     "<span class='tariff-marked'>",
                                     "</span>",
                                     "<br />");
            }

            if (CurrentQuota.Trial)
            {
                if (CurrentTariff.State == TariffState.Trial)
                {
                    return "<b>" + Resource.TariffTrial + "</b> "
                           + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                                  ? string.Format(Resource.TariffExpiredDateStandaloneV11, CurrentTariff.DueDate.Date.ToLongDateString())
                                  : string.Empty);
                }
                return String.Format(Resource.TariffTrialOverdue.HtmlEncode(),
                                     "<span class='tarifff-marked'>",
                                     "</span>",
                                     "<br />", string.Empty, string.Empty);
            }

            if (TenantExtra.EnterprisePaid
                && CurrentTariff.DueDate.Date >= DateTime.Today)
            {
                return "<b>" + (CoreContext.Configuration.CustomMode ? CustomModeResource.TariffPaidStandaloneCustomMode.HtmlEncode() : UserControlsCommonResource.TariffPaidStandalone.HtmlEncode()) + "</b> "
                       + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                              ? string.Format(Resource.TariffExpiredDateStandaloneV11, CurrentTariff.DueDate.Date.ToLongDateString())
                              : string.Empty);
            }

            if (CurrentTariff.LicenseDate == DateTime.MaxValue)
            {
                return String.Format(
                CoreContext.Configuration.CustomMode
                    ? CustomModeResource.TariffNotPaidStandaloneCustomMode.HtmlEncode()
                    : CurrentQuota.Update
                        ? UserControlsCommonResource.TariffNotPaidStandaloneSupport.HtmlEncode()
                        : UserControlsCommonResource.TariffNotPaidStandalone2.HtmlEncode(),
                                 "<span class='tariff-marked'>",
                                 "</span>");
            }

            return String.Format(
                CoreContext.Configuration.CustomMode
                    ? CustomModeResource.TariffOverdueStandaloneCustomMode.HtmlEncode()
                    : CurrentQuota.Update
                        ? UserControlsCommonResource.TariffOverdueStandaloneSupport.HtmlEncode()
                        : UserControlsCommonResource.TariffOverdueStandalone2.HtmlEncode(),
                                 "<span class='tariff-marked'>",
                                 "</span>",
                                 CurrentTariff.LicenseDate.Date.ToLongDateString());
        }

        [AjaxMethod]
        public object ActivateLicenseKey()
        {
            if (!CoreContext.Configuration.Standalone) throw new NotSupportedException();

            TariffSettings.LicenseAccept = true;
            MessageService.Send(HttpContext.Current.Request, MessageAction.LicenseKeyUploaded);

            try
            {
                LicenseReader.RefreshLicense();

                return new { Status = 1 };
            }
            catch (BillingNotFoundException)
            {
                return new { Status = 0, Message = UserControlsCommonResource.LicenseKeyNotFound };
            }
            catch (BillingNotConfiguredException)
            {
                return new { Status = 0, Message = UserControlsCommonResource.LicenseKeyNotCorrect };
            }
            catch (BillingException)
            {
                return new { Status = 0, Message = UserControlsCommonResource.LicenseException };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Message = ex.Message };
            }
        }
    }
}