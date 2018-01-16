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


using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core.Notify;
using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Studio.UserControls.FirstTime
{
    [AjaxNamespace("EmailAndPasswordController")]
    public partial class EmailAndPassword : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/FirstTime/EmailAndPassword.ascx"; }
        }

        protected bool IsVisiblePromocode
        {
            get
            {
                return
                    SetupInfo.IsVisibleSettings("Promocode")
                    && !CoreContext.Configuration.Standalone
                    && string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().PartnerId);
            }
        }

        protected bool RequestLicense
        {
            get
            {
                return TenantExtra.EnableTarrifSettings && TenantExtra.Enterprise && !TenantExtra.EnterprisePaid;
            }
        }

        protected AdditionalWhiteLabelSettings Settings;

        protected bool RequestLicenseAccept
        {
            get { return !TariffSettings.LicenseAccept && Settings.LicenseAgreementsEnabled; }
        }

        protected bool ShowPortalRename { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Settings = AdditionalWhiteLabelSettings.Instance;
            //Settings.LicenseAgreementsUrl = CommonLinkUtility.GetRegionalUrl(Settings.LicenseAgreementsUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            InitScript();

            var timeAndLanguage = (TimeAndLanguage)LoadControl(TimeAndLanguage.Location);
            timeAndLanguage.WithoutButton = true;
            _dateandtimeHolder.Controls.Add(timeAndLanguage);

            ShowPortalRename = SetupInfo.IsVisibleSettings("PortalRename");
        }

        private void InitScript()
        {
            Page.RegisterBodyScripts(
                "~/js/uploader/jquery.fileupload.js",
                "~/usercontrols/firsttime/js/manager.js")
                .RegisterStyle("~/usercontrols/firsttime/css/EmailAndPassword.less");

            var script = new StringBuilder();

            script.AppendFormat(@"ASC.Controls.EmailAndPasswordManager.init('{0}','{1}','{2}','{3}','{4}');",
                                Resource.EmailAndPasswordTypeChangeIt.ReplaceSingleQuote(),
                                Resource.EmailAndPasswordOK.ReplaceSingleQuote(),
                                Resource.EmailAndPasswordWrongPassword.ReplaceSingleQuote(),
                                Resource.EmailAndPasswordEmptyPassword.ReplaceSingleQuote(),
                                Resource.EmailAndPasswordIncorrectEmail.ReplaceSingleQuote()
                );

            Page.RegisterInlineScript(script.ToString());
        }

        [AjaxMethod]
        [SecurityPassthrough]
        public object SaveData(string email, string pwd, string lng, string promocode, bool analytics)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var settings = WizardSettings.Load();
                if (settings.Completed)
                {
                    throw new Exception("Wizard passed.");
                }

                if (tenant.OwnerId == Guid.Empty)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(6)); // wait cache interval
                    tenant = CoreContext.TenantManager.GetTenant(tenant.TenantId);
                    if (tenant.OwnerId == Guid.Empty)
                    {
                        LogManager.GetLogger("ASC.Web.FirstTime").Error(tenant.TenantId + ": owner id is empty.");
                    }
                }

                var currentUser = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
                var cookie = SecurityContext.AuthenticateMe(currentUser.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);

                if (!UserManagerWrapper.ValidateEmail(email))
                {
                    throw new Exception(Resource.EmailAndPasswordIncorrectEmail);
                }

                UserManagerWrapper.CheckPasswordPolicy(pwd);
                SecurityContext.SetUserPassword(currentUser.ID, pwd);

                email = email.Trim();
                if (currentUser.Email != email)
                {
                    currentUser.Email = email;
                    currentUser.ActivationStatus = EmployeeActivationStatus.NotActivated;
                }
                CoreContext.UserManager.SaveUserInfo(currentUser);

                if (!string.IsNullOrWhiteSpace(promocode))
                {
                    try
                    {
                        CoreContext.PaymentManager.ActivateKey(promocode);
                    }
                    catch (Exception err)
                    {
                        LogManager.GetLogger("ASC.Web.FirstTime").Error("Incorrect Promo: " + promocode, err);
                        throw new Exception(Resource.EmailAndPasswordIncorrectPromocode);
                    }
                }

                if (RequestLicense)
                {
                    TariffSettings.LicenseAccept = true;
                    MessageService.Send(HttpContext.Current.Request, MessageAction.LicenseKeyUploaded);

                    LicenseReader.RefreshLicense();
                }

                if (TenantExtra.Opensource)
                {
                    settings.Analytics = analytics;
                }
                settings.Completed = true;
                settings.Save();

                TrySetLanguage(tenant, lng);

                StudioNotifyService.Instance.SendCongratulations(currentUser);
                FirstTimeTenantSettings.SendInstallInfo(currentUser);

                return new { Status = 1, Message = Resource.EmailAndPasswordSaved };
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
                LogManager.GetLogger("ASC.Web.FirstTime").Error(ex);
                return new { Status = 0, Message = ex.Message };
            }
        }

        private static void TrySetLanguage(Tenant tenant, string lng)
        {
            if (string.IsNullOrEmpty(lng)) return;

            try
            {
                var culture = CultureInfo.GetCultureInfo(lng);
                tenant.Language = culture.Name;
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web.FirstTime").Error(err);
            }
        }
    }
}