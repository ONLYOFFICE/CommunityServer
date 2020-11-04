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
using System.Globalization;
using System.Security.Authentication;
using System.Web;
using System.Web.UI;

using ASC.Common.Caching;
using ASC.Core;
using ASC.FederatedLogin.Profile;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

using Resources;

namespace ASC.Web.Studio.UserControls.Common.AuthorizeDocs
{
    public partial class AuthorizeDocs : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/AuthorizeDocs/AuthorizeDocs.ascx"; }
        }

        public static string LocationCustomMode
        {
            get { return "~/UserControls/Common/AuthorizeDocs/AuthorizeDocsCustomMode.ascx"; }
        }

        protected bool RecaptchaEnable
        {
            get { return !string.IsNullOrEmpty(SetupInfo.RecaptchaPublicKey) && !string.IsNullOrEmpty(SetupInfo.RecaptchaPrivateKey); }
        }
        protected bool ShowRecaptcha;

        protected bool ThirdpartyEnable;

        private readonly ICache cache = AscCache.Memory;
        protected string Login;

        protected string LoginMessage;
        protected int LoginMessageType = 0;

        protected string HelpLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginMessage = Auth.GetAuthMessage(Request["am"]);

            Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/authorizedocs.less", "~/UserControls/Common/AuthorizeDocs/css/slick.less")
                .RegisterBodyScripts("~/UserControls/Common/AuthorizeDocs/js/authorizedocs.js", "~/UserControls/Common/Authorize/js/authorize.js", "~/js/third-party/slick.min.js");

            if (CoreContext.Configuration.CustomMode)
                Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/custom-mode.less");

            ThirdpartyEnable = SetupInfo.ThirdPartyAuthEnabled && AccountLinkControl.IsNotEmpty;
            if (Request.DesktopApp()
                && PrivacyRoomSettings.Available
                && PrivacyRoomSettings.Enabled)
            {
                ThirdpartyEnable = false;
                Page
                    .RegisterBodyScripts("~/UserControls/Common/Authorize/js/desktop.js");
            }

            Page.Title = CoreContext.Configuration.CustomMode ? CustomModeResource.TitlePageNewCustomMode : Resource.AuthDocsTitlePage;
            Page.MetaDescription = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaDescriptionCustomMode.HtmlEncode() : Resource.AuthDocsMetaDescription.HtmlEncode();
            Page.MetaKeywords = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaKeywordsCustomMode : Resource.AuthDocsMetaKeywords;

            HelpLink = GetHelpLink();

            PersonalFooterHolder.Controls.Add(LoadControl(CoreContext.Configuration.CustomMode
                                                              ? PersonalFooter.PersonalFooter.LocationCustomMode
                                                              : PersonalFooter.PersonalFooter.Location));

            if (ThirdpartyEnable)
            {
                HolderLoginWithThirdParty.Controls.Add(LoadControl(LoginWithThirdParty.Location));
                LoginSocialNetworks.Controls.Add(LoadControl(LoginWithThirdParty.Location));
            }
            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));

            if (IsPostBack)
            {
                var loginCounter = 0;
                ShowRecaptcha = false;
                try
                {
                    Login = Request["login"].Trim();
                    var passwordHash = Request["passwordHash"];

                    if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(passwordHash))
                    {
                        if (AccountLinkControl.IsNotEmpty
                            && (Request.Url.GetProfile() != null
                                || Request["__EVENTTARGET"] == "thirdPartyLogin"))
                        {
                            return;
                        }
                        throw new InvalidCredentialException(Resource.InvalidUsernameOrPassword);
                    }

                    if (!SetupInfo.IsSecretEmail(Login))
                    {
                        int.TryParse(cache.Get<string>("loginsec/" + Login), out loginCounter);

                        loginCounter++;

                        if (!RecaptchaEnable)
                        {
                            if (loginCounter > SetupInfo.LoginThreshold)
                            {
                                throw new Authorize.BruteForceCredentialException();
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
                                    || !Authorize.ValidateRecaptcha(recaptchaResponse, ip))
                                {
                                    throw new Authorize.RecaptchaException();
                                }
                            }
                        }

                        cache.Insert("loginsec/" + Login, loginCounter.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                    }

                    var session = string.IsNullOrEmpty(Request["remember"]);

                    var cookiesKey = SecurityContext.AuthenticateMe(Login, passwordHash);
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey, session);
                    MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccess);

                    cache.Insert("loginsec/" + Login, (--loginCounter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                }
                catch (InvalidCredentialException ex)
                {
                    Auth.ProcessLogout();
                    MessageAction messageAction;

                    if (ex is Authorize.BruteForceCredentialException)
                    {
                        LoginMessage = Resource.LoginWithBruteForce;
                        messageAction = MessageAction.LoginFailBruteForce;
                    }
                    else if (ex is Authorize.RecaptchaException)
                    {
                        LoginMessage = Resource.RecaptchaInvalid;
                        messageAction = MessageAction.LoginFailRecaptcha;
                    }
                    else
                    {
                        LoginMessage = Resource.InvalidUsernameOrPassword;
                        messageAction = MessageAction.LoginFailInvalidCombination;
                    }

                    var loginName = string.IsNullOrWhiteSpace(Login) ? AuditResource.EmailNotSpecified : Login;

                    MessageService.Send(HttpContext.Current.Request, loginName, messageAction);

                    return;
                }
                catch (System.Security.SecurityException)
                {
                    Auth.ProcessLogout();
                    LoginMessage = Resource.ErrorDisabledProfile;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailDisabledProfile);
                    return;
                }
                catch (Exception ex)
                {
                    Auth.ProcessLogout();
                    LoginMessage = ex.Message;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFail);
                    return;
                }

                if (loginCounter > 0)
                {
                    cache.Insert("loginsec/" + Login, (--loginCounter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                }

                var refererURL = (string)Session["refererURL"];

                if (string.IsNullOrEmpty(refererURL))
                {
                    Response.Redirect(CommonLinkUtility.GetDefault());
                }
                else
                {
                    Session["refererURL"] = null;
                    Response.Redirect(refererURL);
                }
            }
            else
            {
                var confirmedEmail = (Request.QueryString["confirmed-email"] ?? "").Trim();

                if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

                Login = confirmedEmail;
                LoginMessage = Resource.MessageEmailConfirmed + " " + Resource.MessageAuthorize;
                LoginMessageType = 1;
            }
        }

        private static string GetHelpLink()
        {
            var baseHelpLink = CommonLinkUtility.GetHelpLink();

            if (string.IsNullOrEmpty(baseHelpLink))
                baseHelpLink = ConfigurationManagerExtension.AppSettings["web.faq-url"] ?? string.Empty;

            return baseHelpLink.TrimEnd('/');
        }
    }
}