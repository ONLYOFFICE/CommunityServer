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


using System;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.ActiveDirectory;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin.Profile;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.SingleSignOn.Common;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class Authorize : UserControl
    {
        protected string LoginMessage;
        private string _errorMessage;
        private readonly ICache cache = AscCache.Memory;

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

                    var enabled = SsoSettingsV2.Load().EnableSso;

                    return enabled;
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
            get
            {
                return EnableSso ? SsoSettingsV2.Load().SpLoginLabel : SsoSettingsV2.SSO_SP_LOGIN_LABEL;
            }
        }

        protected bool IsSaml
        {
            get { return true; } //Todo: Change after jwt fixes
        }

        protected bool EnableSession
        {
            get { return TenantCookieSettings.Load().IsDefault(); }
        }

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
        protected string Password;
        protected string HashId;

        protected string ConfirmedEmail;

        public bool IsLogout { get; set; }

        public static string Location
        {
            get { return "~/UserControls/Common/Authorize/Authorize.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/usercontrols/common/authorize/css/authorize.less")
                .RegisterBodyScripts("~/usercontrols/common/authorize/js/authorize.js");

            Login = "";
            Password = "";
            HashId = "";

            //Account link control
            bool withAccountLink = false;

            if (SetupInfo.ThirdPartyAuthEnabled && AccountLinkControl.IsNotEmpty)
            {
                var accountLink = (AccountLinkControl) LoadControl(AccountLinkControl.Location);
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
            }

            Page.Title = HeaderStringHelper.GetPageTitle(Resource.Authorization);

            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));

            var msg = Request["m"];
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

                var refererURL = (string) Session["refererURL"];
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
        }

        private bool AuthProcess(LoginProfile thirdPartyProfile, bool withAccountLink)
        {
            var authMethod = AuthMethod.Login;
            var smsLoginUrl = string.Empty;
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

                if (!string.IsNullOrEmpty(Request["pwd"]))
                {
                    Password = Request["pwd"];
                }
                else if (string.IsNullOrEmpty(HashId))
                {
                    IsPasswordInvalid = true;
                    throw new InvalidCredentialException("password");
                }

                if (string.IsNullOrEmpty(HashId))
                {
                    int counter;

                    int.TryParse(cache.Get<String>("loginsec/" + Login), out counter);

                    if (++counter%5 == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                    cache.Insert("loginsec/" + Login, counter.ToString(), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
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
                    smsLoginUrl = Studio.Confirm.SmsConfirmUrl(userInfo);
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
            catch (InvalidCredentialException)
            {
                Auth.ProcessLogout();

                ErrorMessage = authMethod == AuthMethod.ThirdParty ? Resource.LoginWithAccountNotFound : Resource.InvalidUsernameOrPassword;

                var loginName = !string.IsNullOrWhiteSpace(Login)
                                    ? Login
                                    : authMethod == AuthMethod.ThirdParty && !string.IsNullOrWhiteSpace(HashId)
                                          ? HashId
                                          : AuditResource.EmailNotSpecified;

                var messageAction = authMethod == AuthMethod.ThirdParty
                                        ? MessageAction.LoginFailSocialAccountNotFound
                                        : MessageAction.LoginFailInvalidCombination;

                MessageService.Send(HttpContext.Current.Request, loginName, messageAction);

                if (authMethod == AuthMethod.ThirdParty && thirdPartyProfile != null) Response.Redirect("~/auth.aspx?m=" + HttpUtility.UrlEncode(_errorMessage), true);
                return false;
            }
            catch (System.Security.SecurityException)
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

            if (!string.IsNullOrEmpty(smsLoginUrl))
            {
                Response.Redirect(smsLoginUrl, true);
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
                if (ldapUserManager.TryGetAndSyncLdapUserInfo(Login, Password, out userInfo))
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
            return CoreContext.UserManager.GetUsers(TenantProvider.CurrentTenantID, Login, Hasher.Base64Hash(Password, HashAlg.SHA256));
        }

        private void ProcessConfirmedEmailCondition()
        {
            if (IsPostBack) return;

            var confirmedEmail = (Request.QueryString["confirmed-email"] ?? "").Trim();

            if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

            Login = confirmedEmail;
            LoginMessage = String.Format("<div class=\"confirmBox\">{0} {1}</div>", Resource.MessageEmailConfirmed, Resource.MessageAuthorize);
        }
    }
}