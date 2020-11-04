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
using System.Security;
using System.Security.Authentication;
using System.Web;
using System.Web.UI;
using ASC.ActiveDirectory;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin.Profile;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using Resources;
using SecurityContext = ASC.Core.SecurityContext;
using SsoSettingsV2 = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SsoSettingsV2;


namespace ASC.Web.Studio.UserControls.Common
{
    public partial class Authorize : UserControl
    {
        protected string LoginMessage;
        private string _errorMessage;
        private readonly ICache cache = AscCache.Memory;
        protected bool ShowRecaptcha;
        protected string Culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        protected bool EnableLdap
        {
            get
            {
                try
                {
                    if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                        (CoreContext.Configuration.Standalone &&
                         !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
                    {
                        return false;
                    }

                    var enabled = LdapSettings.Load().EnableLdapAuthentication;

                    return enabled;
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.Web").Error("[LDAP] EnableLdap failed", ex);
                    return false;
                }
            }
        }

        private SsoSettingsV2 ssoSettingsV2 = null;

        private SsoSettingsV2 SsoSettings
        {
            get
            {
                return ssoSettingsV2 ?? (ssoSettingsV2 = SsoSettingsV2.Load());
            }
        }

        protected bool EnableSso
        {
            get
            {
                try
                {
                    if (!SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()) ||
                        !CoreContext.Configuration.Standalone ||
                        !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso)
                    {
                        return false;
                    }

                    if (Request.DesktopApp()
                        && PrivacyRoomSettings.Available
                        && PrivacyRoomSettings.Enabled)
                    {
                        return false;
                    }

                    return SsoSettings.EnableSso;
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.Web").Error("[SSO] EnableSso failed", ex);
                    return false;
                }
            }
        }

        protected string SsoLabel
        {
            get { return EnableSso ? SsoSettings.SpLoginLabel : SsoSettingsV2.SSO_SP_LOGIN_LABEL; }
        }

        protected string SsoUrl
        {
            get { return IsSaml ? SetupInfo.SsoSamlLoginUrl : "jwtlogin.ashx?auth=true"; }
        }

        protected bool IsSaml
        {
            get { return true; } //Todo: Change after jwt fixes
        }

        protected bool EnableSession
        {
            get { return TenantCookieSettings.Load().IsDefault(); }
        }

        protected bool RecaptchaEnable
        {
            get { return !string.IsNullOrEmpty(SetupInfo.RecaptchaPublicKey) && !string.IsNullOrEmpty(SetupInfo.RecaptchaPrivateKey); }
        }

        protected bool ThirdpartyEnable;

        protected string ErrorMessage
        {
            get
            {
                return string.IsNullOrEmpty(_errorMessage)
                           ? string.Empty
                           : "<div class='errorBox'>" + _errorMessage.HtmlEncode() + "</div>";
            }
            set { _errorMessage = value; }
        }

        public bool IsLoginInvalid { get; set; }
        public bool IsPasswordInvalid { get; set; }

        protected string Login;
        protected string PasswordHash;
        protected string HashId;

        protected string ConfirmedEmail;

        public bool IsLogout { get; set; }

        public static string Location
        {
            get { return "~/UserControls/Common/Authorize/Authorize.ascx"; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (EnableSso && SsoSettings.HideAuthPage && HttpContext.Current.Request["skipssoredirect"] != "true")
                Response.Redirect(SsoUrl);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/UserControls/Common/Authorize/css/authorize.less")
                .RegisterBodyScripts("~/UserControls/Common/Authorize/js/authorize.js");

            if (RecaptchaEnable)
            {
                Page
                    .RegisterBodyScripts("~/usercontrols/common/authorize/js/recaptchacontroller.js");
            }

            ThirdpartyEnable = SetupInfo.ThirdPartyAuthEnabled && AccountLinkControl.IsNotEmpty;
            if (Request.DesktopApp()
                && PrivacyRoomSettings.Available
                && PrivacyRoomSettings.Enabled)
            {
                ThirdpartyEnable = false;
                Page
                    .RegisterBodyScripts("~/UserControls/Common/Authorize/js/desktop.js");
            }

            Login = "";
            PasswordHash = "";
            HashId = "";

            //Account link control
            bool withAccountLink = false;

            if (ThirdpartyEnable)
            {
                var accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
                accountLink.Visible = true;
                accountLink.ClientCallback = "authCallback";
                accountLink.SettingsView = false;
                signInPlaceholder.Controls.Add(accountLink);

                withAccountLink = true;
            }

            //top panel
            var master = Page.Master as BaseTemplate;
            if (master != null)
            {
                master.TopStudioPanel.DisableProductNavigation = true;
                master.TopStudioPanel.DisableSearch = true;
                master.TopStudioPanel.DisableGift = true;
            }

            Page.Title = HeaderStringHelper.GetPageTitle(Resource.Authorization);

            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));

            var msg = Auth.GetAuthMessage(Request["am"]);
            var urlError = Request.QueryString["error"];

            if (!string.IsNullOrEmpty(msg))
            {
                ErrorMessage = msg;
            }
            else if (urlError == "ipsecurity")
            {
                ErrorMessage = Resource.LoginFailIPSecurityMsg;
            }

            var thirdPartyProfile = Request.Url.GetProfile();
            if ((IsPostBack || thirdPartyProfile != null) && !SecurityContext.IsAuthenticated)
            {
                if (!AuthProcess(thirdPartyProfile, withAccountLink)) return;

                CookiesManager.ClearCookies(CookiesType.SocketIO);
                var refererURL = (string)Session["refererURL"];
                if (string.IsNullOrEmpty(refererURL))
                {
                    Response.Redirect(CommonLinkUtility.GetDefault(), true);
                }
                else
                {
                    Session["refererURL"] = null;
                    Response.Redirect(refererURL, true);
                }
            }
            ProcessConfirmedEmailCondition();
            ProcessConfirmedEmailLdap();
        }

        private bool AuthProcess(LoginProfile thirdPartyProfile, bool withAccountLink)
        {
            var authMethod = AuthMethod.Login;
            var tfaLoginUrl = string.Empty;
            var loginCounter = 0;
            ShowRecaptcha = false;
            try
            {
                if (thirdPartyProfile != null)
                {
                    if (string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
                    {
                        HashId = thirdPartyProfile.HashId;
                        Login = thirdPartyProfile.EMail;
                    }
                    else
                    {
                        // ignore cancellation
                        if (thirdPartyProfile.AuthorizationError != "Canceled at provider")
                        {
                            ErrorMessage = thirdPartyProfile.AuthorizationError;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) && Request["__EVENTTARGET"] == "signInLogin" && withAccountLink)
                    {
                        HashId = ASC.Common.Utils.Signature.Read<string>(Request["__EVENTARGUMENT"]);
                    }
                }

                if (!string.IsNullOrEmpty(Request["login"]))
                {
                    Login = Request["login"].Trim();
                }
                else if (string.IsNullOrEmpty(HashId))
                {
                    IsLoginInvalid = true;
                    throw new InvalidCredentialException("login");
                }

                if (!string.IsNullOrEmpty(Request["passwordHash"]))
                {
                    PasswordHash = Request["passwordHash"];
                }
                else if (string.IsNullOrEmpty(HashId))
                {
                    IsPasswordInvalid = true;
                    throw new InvalidCredentialException("password");
                }

                if (string.IsNullOrEmpty(HashId) && !SetupInfo.IsSecretEmail(Login))
                {
                    int.TryParse(cache.Get<String>("loginsec/" + Login), out loginCounter);

                    loginCounter++;

                    if (!RecaptchaEnable)
                    {
                        if (loginCounter > SetupInfo.LoginThreshold)
                        {
                            throw new BruteForceCredentialException();
                        }
                    }
                    else
                    {
                        if (loginCounter > SetupInfo.LoginThreshold - 1)
                        {
                            ShowRecaptcha = true;
                        }
                        if (loginCounter > SetupInfo.LoginThreshold)
                        {
                            var ip = Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress;

                            var recaptchaResponse = Request["g-recaptcha-response"];
                            if (String.IsNullOrEmpty(recaptchaResponse)
                                || !ValidateRecaptcha(recaptchaResponse, ip))
                            {
                                throw new RecaptchaException();
                            }
                        }
                    }

                    cache.Insert("loginsec/" + Login, loginCounter.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                }

                var userInfo = GetUser(out authMethod);
                if (!CoreContext.UserManager.UserExists(userInfo.ID) || userInfo.Status != EmployeeStatus.Active)
                {
                    IsLoginInvalid = true;
                    IsPasswordInvalid = true;
                    throw new InvalidCredentialException();
                }

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var settings = IPRestrictionsSettings.Load();
                if (settings.Enable && userInfo.ID != tenant.OwnerId && !IPSecurity.IPSecurity.Verify(tenant))
                {
                    throw new IPSecurityException();
                }

                if (StudioSmsNotificationSettings.IsVisibleSettings
                    && StudioSmsNotificationSettings.Enable)
                {
                    tfaLoginUrl = Studio.Confirm.SmsConfirmUrl(userInfo);
                }
                else if (TfaAppAuthSettings.IsVisibleSettings
                         && TfaAppAuthSettings.Enable)
                {
                    tfaLoginUrl = Studio.Confirm.TfaConfirmUrl(userInfo);
                }
                else
                {
                    var session = EnableSession && string.IsNullOrEmpty(Request["remember"]);

                    var cookiesKey = SecurityContext.AuthenticateMe(userInfo.ID);
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey, session);
                    MessageService.Send(HttpContext.Current.Request,
                                        authMethod == AuthMethod.ThirdParty
                                            ? MessageAction.LoginSuccessViaSocialAccount
                                            : MessageAction.LoginSuccess
                        );
                }
            }
            catch (InvalidCredentialException ex)
            {
                Auth.ProcessLogout();

                Auth.MessageKey messageKey;
                MessageAction messageAction;

                if (ex is BruteForceCredentialException)
                {
                    messageKey = Auth.MessageKey.LoginWithBruteForce;
                    messageAction = MessageAction.LoginFailBruteForce;
                }
                else if (ex is RecaptchaException)
                {
                    messageKey = Auth.MessageKey.RecaptchaInvalid;
                    messageAction = MessageAction.LoginFailRecaptcha;
                }
                else if (authMethod == AuthMethod.ThirdParty)
                {
                    messageKey = Auth.MessageKey.LoginWithAccountNotFound;
                    messageAction = MessageAction.LoginFailSocialAccountNotFound;
                }
                else
                {
                    messageKey = Auth.MessageKey.InvalidUsernameOrPassword;
                    messageAction = MessageAction.LoginFailInvalidCombination;
                }

                var loginName = !string.IsNullOrWhiteSpace(Login)
                                    ? Login
                                    : authMethod == AuthMethod.ThirdParty && !string.IsNullOrWhiteSpace(HashId)
                                          ? HashId
                                          : AuditResource.EmailNotSpecified;

                MessageService.Send(HttpContext.Current.Request, loginName, messageAction);

                if (authMethod == AuthMethod.ThirdParty && thirdPartyProfile != null)
                {
                    Response.Redirect("~/Auth.aspx?am=" + (int)messageKey + (Request.DesktopApp() ? "&desktop=true" : ""), true);
                }
                else
                {
                    ErrorMessage = Auth.GetAuthMessage(messageKey);
                }

                return false;
            }
            catch (SecurityException)
            {
                Auth.ProcessLogout();
                ErrorMessage = Resource.ErrorDisabledProfile;
                MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailDisabledProfile);
                return false;
            }
            catch (IPSecurityException)
            {
                Auth.ProcessLogout();
                ErrorMessage = Resource.ErrorIpSecurity;
                MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailIpSecurity);
                return false;
            }
            catch (Exception ex)
            {
                Auth.ProcessLogout();
                ErrorMessage = ex.Message;
                MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFail);
                return false;
            }

            if (loginCounter > 0)
            {
                cache.Insert("loginsec/" + Login, (--loginCounter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
            }

            if (!string.IsNullOrEmpty(tfaLoginUrl))
            {
                if (Request.DesktopApp())
                    tfaLoginUrl += "&desktop=true";

                Response.Redirect(tfaLoginUrl, true);
            }

            return true;
        }

        private enum AuthMethod
        {
            Login,
            ThirdParty,
            Ldap
        }

        private UserInfo GetUser(out AuthMethod method)
        {
            if (EnableLdap)
            {
                var localization = new LdapLocalization(Resource.ResourceManager);
                var ldapUserManager = new LdapUserManager(localization);

                UserInfo userInfo;
                //todo: think about password
                if (ldapUserManager.TryGetAndSyncLdapUserInfo(Login, PasswordHash, out userInfo))
                {
                    method = AuthMethod.Ldap;
                    return userInfo;
                }
            }

            Guid userId;
            if (LoginWithThirdParty.TryGetUserByHash(HashId, out userId))
            {
                method = AuthMethod.ThirdParty;
                return CoreContext.UserManager.GetUsers(userId);
            }

            method = AuthMethod.Login;
            return CoreContext.UserManager.GetUsersByPasswordHash(TenantProvider.CurrentTenantID, Login, PasswordHash);
        }

        private void ProcessConfirmedEmailCondition()
        {
            if (IsPostBack) return;

            var confirmedEmail = (Request.QueryString["confirmed-email"] ?? "").Trim();

            if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

            Login = confirmedEmail;
            LoginMessage = String.Format("<div class=\"confirmBox\">{0} {1}</div>", Resource.MessageEmailConfirmed, Resource.MessageAuthorize);
        }

        private void ProcessConfirmedEmailLdap()
        {
            if (IsPostBack) return;

            var login = (Request.QueryString["ldap-login"] ?? "").Trim();

            if (String.IsNullOrEmpty(login)) return;

            Login = login;
            LoginMessage = String.Format("<div class=\"confirmBox\">{0}<br>{1} {2}</div>",
                                         Resource.MessageEmailConfirmed,
                                         Resource.MessageAuthorizeLdap,
                                         LdapCurrentDomain.Load().CurrentDomain);
        }


        public class BruteForceCredentialException : InvalidCredentialException
        {
            public BruteForceCredentialException()
            {
            }

            public BruteForceCredentialException(string message)
                : base(message)
            {
            }
        }

        public class RecaptchaException : InvalidCredentialException
        {
            public RecaptchaException()
            {
            }

            public RecaptchaException(string message)
                : base(message)
            {
            }
        }

        public static bool ValidateRecaptcha(string response, string ip)
        {
            try
            {
                var data = string.Format("secret={0}&remoteip={1}&response={2}", SetupInfo.RecaptchaPrivateKey, ip, response);

                var webRequest = (HttpWebRequest)WebRequest.Create(SetupInfo.RecaptchaVerifyUrl);
                webRequest.Method = WebRequestMethods.Http.Post;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;
                using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                {
                    writer.Write(data);
                }

                using (var webResponse = webRequest.GetResponse())
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    var resp = reader.ReadToEnd();
                    var resObj = JObject.Parse(resp);

                    if (resObj["success"] != null && resObj.Value<bool>("success"))
                    {
                        return true;
                    }
                    if (resObj["error-codes"] != null && resObj["error-codes"].HasValues)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}