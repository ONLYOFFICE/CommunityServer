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
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.AuthorizeDocs;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.UserControls.Management.SingleSignOnSettings;
using Resources;

namespace ASC.Web.Studio
{
    public partial class Auth : MainPage
    {
        protected string LogoPath {
            get { return String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.Dark, (!TenantLogoManager.IsRetina(Request)).ToString().ToLower()); }
        }

        protected bool withHelpBlock { get; set; }

        protected override bool MayNotAuth { get { return true; } }

        protected override bool MayNotPaid { get { return true; } }

        protected override bool MayPhoneNotActivate { get { return true; } }

        protected string TenantName;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!SecurityContext.IsAuthenticated)
            {
                if (CoreContext.Configuration.Personal)
                {
                    CheckSocialMedia();

                    SetLanguage();
                }

                var token = Request["asc_auth_key"];
                if (SecurityContext.AuthenticateMe(token))
                {
                    CookiesManager.SetCookies(CookiesType.AuthKey, token);

                    var refererURL = Request["refererURL"];
                    if (string.IsNullOrEmpty(refererURL)) refererURL = "~/Auth.aspx";

                    Response.Redirect(refererURL, true);
                }

                return;
            }

            if (IsLogout)
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var loginName = user.DisplayUserName(false);
                ProcessLogout();
                MessageService.Send(HttpContext.Current.Request, loginName, MessageAction.Logout);

                if (!string.IsNullOrEmpty(user.SsoNameId))
                {
                    var settings = SsoSettingsV2.Load();

                    if (settings.EnableSso && !string.IsNullOrEmpty(settings.IdpSettings.SloUrl))
                    {
                        var logoutSsoUserData = Signature.Create(new LogoutSsoUserData
                        {
                            NameId = user.SsoNameId,
                            SessionId = user.SsoSessionId
                        });

                        HttpContext.Current.Response.Redirect(SetupInfo.SsoSamlLogoutUrl + "?data=" + HttpUtility.UrlEncode(logoutSsoUserData), true);
                    }
                }

                Response.Redirect("~/Auth.aspx", true);
            }
            else
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            TenantName = CoreContext.TenantManager.GetCurrentTenant().Name;
            Page.Title = TenantName;

            Master.DisabledSidePanel = true;
            if (Request.DesktopApp())
            {
                Master.DisabledTopStudioPanel = true;
            }

            withHelpBlock = false;
            if (CoreContext.Configuration.Personal)
            {
                Master.DisabledLayoutMedia = true;
                Master.TopStudioPanel.TopLogo = TenantLogoManager.IsRetina(Request)
                                                        ? WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth-@2x.png") 
                                                        : WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth.png");
                AutorizeDocuments.Controls.Add(CoreContext.Configuration.CustomMode
                                                   ? LoadControl(AuthorizeDocs.LocationCustomMode)
                                                   : LoadControl(AuthorizeDocs.Location));
            }
            else
            {
                var authControl = (Authorize)LoadControl(Authorize.Location);
                authControl.IsLogout = IsLogout;
                AuthorizeHolder.Controls.Add(authControl);

                CommunitationsHolder.Controls.Add(LoadControl(AuthCommunications.Location));
                withHelpBlock = true;
            }
        }

        public static void ProcessLogout()
        {
            //logout
            CookiesManager.ClearCookies(CookiesType.AuthKey);
            CookiesManager.ClearCookies(CookiesType.SocketIO);
            SecurityContext.Logout();
        }

        private bool IsLogout
        {
            get
            {
                var logoutParam = Request["t"];
                if (String.IsNullOrEmpty(logoutParam))
                    return false;

                return logoutParam.ToLower() == "logout";
            }
        }

        private void CheckSocialMedia()
        {
            var social = Request["from"];
            if (string.IsNullOrEmpty(social)) return;

            social = social.ToLower();
            if (string.Equals(social, "openid", StringComparison.InvariantCultureIgnoreCase))
            {
                social = "google";
            }

            var loginUrl = string.Empty;

            if (string.Equals(social, "facebook", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(social, "google", StringComparison.InvariantCultureIgnoreCase))
            {
                var returnUrl = new Uri(Request.GetUrlRewriter(), "Auth.aspx");
                loginUrl = "~/login.ashx?auth=" + social
                           + "&mode=Redirect&returnurl=" + HttpUtility.UrlEncode(returnUrl.ToString());
            }

            if (!string.IsNullOrEmpty(loginUrl))
            {
                Response.Redirect(loginUrl, true);
            }
        }


        public static string GetAuthMessage(string messageKey)
        {
            MessageKey authMessage;
            if (!Enum.TryParse(messageKey, out authMessage)) return null;
            return GetAuthMessage(authMessage);
        }
        
        public static string GetAuthMessage(MessageKey messageKey)
        {
            switch (messageKey)
            {
                case MessageKey.Error:
                    return Resource.ErrorBadRequest;
                case MessageKey.ErrorUserNotFound:
                    return Resource.ErrorUserNotFound;
                case MessageKey.ErrorExpiredActivationLink:
                    return Resource.ErrorExpiredActivationLink;
                case MessageKey.ErrorInvalidActivationLink:
                    return Resource.ErrorInvalidActivationLink;
                case MessageKey.ErrorConfirmURLError:
                    return Resource.ErrorConfirmURLError;
                case MessageKey.ErrorNotCorrectEmail:
                    return Resource.ErrorNotCorrectEmail;
                case MessageKey.LoginWithBruteForce:
                    return Resource.LoginWithBruteForce;
                case MessageKey.RecaptchaInvalid:
                    return Resource.RecaptchaInvalid;
                case MessageKey.LoginWithAccountNotFound:
                    return Resource.LoginWithAccountNotFound;
                case MessageKey.InvalidUsernameOrPassword:
                    return Resource.InvalidUsernameOrPassword;
                case MessageKey.SsoSettingsDisabled:
                    return Resource.SsoSettingsDisabled;
                case MessageKey.ErrorNotAllowedOption:
                    return Resource.ErrorNotAllowedOption;
                case MessageKey.SsoSettingsEmptyToken:
                    return Resource.SsoSettingsEmptyToken;
                case MessageKey.SsoSettingsNotValidToken:
                    return Resource.SsoSettingsNotValidToken;
                case MessageKey.SsoSettingsCantCreateUser:
                    return Resource.SsoSettingsCantCreateUser;
                case MessageKey.SsoSettingsUserTerminated:
                    return Resource.SsoSettingsUserTerminated;
                case MessageKey.SsoError:
                    return Resource.SsoError;
                case MessageKey.SsoAuthFailed:
                    return Resource.SsoAuthFailed;
                case MessageKey.SsoAttributesNotFound:
                    return Resource.SsoAttributesNotFound;
                default: return null;
            }
        }

        public enum MessageKey
        {
            None,
            Error,
            ErrorUserNotFound,
            ErrorExpiredActivationLink,
            ErrorInvalidActivationLink,
            ErrorConfirmURLError,
            ErrorNotCorrectEmail,
            LoginWithBruteForce,
            RecaptchaInvalid,
            LoginWithAccountNotFound,
            InvalidUsernameOrPassword,
            SsoSettingsDisabled,
            ErrorNotAllowedOption,
            SsoSettingsEmptyToken,
            SsoSettingsNotValidToken,
            SsoSettingsCantCreateUser,
            SsoSettingsUserTerminated,
            SsoError,
            SsoAuthFailed,
            SsoAttributesNotFound,
        }
    }
}