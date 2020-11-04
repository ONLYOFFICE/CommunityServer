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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;
using Resources;
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

        protected bool IsAmi
        {
            get { return !string.IsNullOrEmpty(SetupInfo.AmiMetaUrl); }
        }

        protected bool RequestLicense
        {
            get
            {
                return TenantExtra.EnableTarrifSettings && TenantExtra.Enterprise;
            }
        }

        protected AdditionalWhiteLabelSettings Settings;

        protected bool RequestLicenseAccept
        {
            get { return !TariffSettings.LicenseAccept && Settings.LicenseAgreementsEnabled; }
        }

        protected bool ShowPortalRename { get; set; }

        protected Web.Core.Utility.PasswordSettings PasswordSetting;

        protected string OpensourceLicenseAgreementsUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Settings = AdditionalWhiteLabelSettings.Instance;

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            InitScript();

            var timeAndLanguage = (TimeAndLanguage)LoadControl(TimeAndLanguage.Location);
            timeAndLanguage.WithoutButton = true;
            _dateandtimeHolder.Controls.Add(timeAndLanguage);

            ShowPortalRename = SetupInfo.IsVisibleSettings("PortalRename");

            PasswordSetting = Web.Core.Utility.PasswordSettings.Load();

            OpensourceLicenseAgreementsUrl = string.IsNullOrEmpty(Web.Core.Files.FilesLinkUtility.DocServiceApiUrl)
                ? "http://www.apache.org/licenses/LICENSE-2.0"
                : "https://help.onlyoffice.com/Products/Files/doceditor.aspx?fileid=6762822&doc=ODdtYzFDVGtXNU9Xd3VMWktoQ25ZZTZWbkpqZmZETWNGTnZQM0JKUVFHVT0_IjY3NjI4MjIi0";
        }

        private void InitScript()
        {
            Page.RegisterBodyScripts(
                "~/js/uploader/jquery.fileupload.js",
                "~/js/third-party/xregexp.js",
                "~/UserControls/FirstTime/js/manager.js")
                .RegisterStyle("~/UserControls/FirstTime/css/emailandpassword.less");

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
        public object SaveData(string email, string passwordHash, string lng, string promocode, string amiid, bool analytics, bool subscribeFromSite)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var settings = WizardSettings.Load();
                if (settings.Completed)
                {
                    throw new Exception("Wizard passed.");
                }

                if (IsAmi && IncorrectAmiId(amiid))
                {
                    throw new Exception(Resource.EmailAndPasswordIncorrectAmiId);
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

                if (String.IsNullOrEmpty(passwordHash))
                    throw new Exception(Resource.ErrorPasswordEmpty);

                SecurityContext.SetUserPasswordHash(currentUser.ID, passwordHash);

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
                StudioNotifyService.Instance.SendRegData(currentUser);
                FirstTimeTenantSettings.SendInstallInfo(currentUser);

                if (subscribeFromSite
                    && TenantExtra.Opensource
                    && !CoreContext.Configuration.CustomMode)
                {
                    SubscribeFromSite(currentUser);
                }

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

        private static string _amiId;

        private static bool IncorrectAmiId(string customAmiId)
        {
            customAmiId = (customAmiId ?? "").Trim();
            if (string.IsNullOrEmpty(customAmiId)) return true;

            if (string.IsNullOrEmpty(_amiId))
            {
                var getAmiIdUrl = SetupInfo.AmiMetaUrl + "instance-id";
                var request = (HttpWebRequest)WebRequest.Create(getAmiIdUrl);
                try
                {
                    using (var response = request.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    using (var reader = new StreamReader(responseStream))
                    {
                        _amiId = reader.ReadToEnd();
                    }

                    LogManager.GetLogger("ASC.Web.FirstTime").Debug("Instance id: " + _amiId);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC.Web.FirstTime").Error("Request AMI id", e);
                }
            }

            return string.IsNullOrEmpty(_amiId) || _amiId != customAmiId;
        }

        private static void SubscribeFromSite(UserInfo user)
        {
            try
            {
                var url = (SetupInfo.TeamlabSiteRedirect ?? "").Trim().TrimEnd('/');
                if (string.IsNullOrEmpty(url)) return;

                url += "/post.ashx";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 10000;

                var bodyString = string.Format("type=sendsubscription&email={0}", HttpUtility.UrlEncode(user.Email));
                var bytes = Encoding.UTF8.GetBytes(bodyString);
                request.ContentLength = bytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null) throw new Exception("Response is null");

                    using (var reader = new StreamReader(stream))
                    {
                        LogManager.GetLogger("ASC.Web.FirstTime").Debug("Subscribe response: " + reader.ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.FirstTime").Error("Subscribe request", e);
            }
        }
    }
}