/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
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
                    "~/usercontrols/management/tariffsettings/js/tariffstandalone.js")
                .RegisterStyle("~/usercontrols/management/tariffsettings/css/tariff.less",
                    "~/usercontrols/management/tariffsettings/css/tariffstandalone.less");

            UsersCount = TenantStatisticsProvider.GetUsersCount();
            CurrentTariff = TenantExtra.GetCurrentTariff();
            CurrentQuota = TenantExtra.GetTenantQuota();
            TenantCount = CoreContext.TenantManager.GetTenants().Count();

            Settings = AdditionalWhiteLabelSettings.Instance;
            Settings.LicenseAgreementsUrl = CommonLinkUtility.GetRegionalUrl(Settings.LicenseAgreementsUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        protected string TariffDescription()
        {
            if (CurrentQuota.Trial)
            {
                if (CurrentTariff.State == TariffState.Trial)
                {
                    return "<b>" + Resource.TariffTrial + "</b> "
                           + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                                  ? string.Format(Resource.TariffExpiredDateStandalone, CurrentTariff.DueDate.Date.ToLongDateString())
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
                return "<b>" + UserControlsCommonResource.TariffPaidStandalone.HtmlEncode() + "</b> "
                       + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                              ? string.Format(Resource.TariffExpiredDateStandalone, CurrentTariff.DueDate.Date.ToLongDateString())
                              : string.Empty);
            }

            return String.Format(UserControlsCommonResource.TariffOverdueStandalone.HtmlEncode(),
                                 "<span class='tariff-marked'>",
                                 "</span>",
                                 "<br />");
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