/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Security.Authentication;
using System.Web;
using System.Web.UI;

using ASC.Common.Caching;
using ASC.Core;
using ASC.FederatedLogin.Profile;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

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
        protected bool DontShowMainPage;

        protected string HelpLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginMessage = Auth.GetAuthMessage(Request["am"]);

            var theme = ModeThemeSettings.GetModeThemesSettings().ModeThemeName;

            if (theme == ModeTheme.dark)
            {
                Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/dark-authorizedocs.less");
            }
            else
            {
                Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/authorizedocs.less");
            }
            Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/slick.less")
                .RegisterBodyScripts("~/js/third-party/lodash.min.js", "~/js/third-party/masonry.pkgd.min.js", "~/UserControls/Common/AuthorizeDocs/js/reviews.js", "~/UserControls/Common/AuthorizeDocs/js/review_builder_script.js", "~/UserControls/Common/AuthorizeDocs/js/authorizedocs.js", "~/UserControls/Common/Authorize/js/authorize.js", "~/js/third-party/slick.min.js");

            if (CoreContext.Configuration.CustomMode)
                if (theme == ModeTheme.dark)
                {
                    Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/dark-custom-mode.less");
                }
                else
                {
                    Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/custom-mode.less");
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

            Page.Title = CoreContext.Configuration.CustomMode ? CustomModeResource.TitlePageNewCustomMode : PersonalResource.AuthDocsTitlePage;
            Page.MetaDescription = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaDescriptionCustomMode.HtmlEncode() : PersonalResource.AuthDocsMetaDescription.HtmlEncode();
            Page.MetaKeywords = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaKeywordsCustomMode : PersonalResource.AuthDocsMetaKeywords;

            HelpLink = GetHelpLink();

            PersonalFooterHolder.Controls.Add(LoadControl(CoreContext.Configuration.CustomMode
                                                              ? PersonalFooter.PersonalFooter.LocationCustomMode
                                                              : PersonalFooter.PersonalFooter.Location));

            if (ThirdpartyEnable)
            {
                var loginWithThirdParty = (LoginWithThirdParty)LoadControl(LoginWithThirdParty.Location);
                loginWithThirdParty.RenderDisabled = true;
                HolderLoginWithThirdParty.Controls.Add(loginWithThirdParty);
                var loginWithThirdPartySocial = (LoginWithThirdParty)LoadControl(LoginWithThirdParty.Location);
                loginWithThirdPartySocial.RenderDisabled = true;
                LoginSocialNetworks.Controls.Add(loginWithThirdPartySocial);
            }
            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));

            if (IsPostBack)
            {
                var requestIp = MessageSettings.GetFullIPAddress(Request);
                var bruteForceLoginManager = new BruteForceLoginManager(cache, Login, requestIp);
                var bruteForceSuccessAttempt = false;

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
                        bruteForceSuccessAttempt = bruteForceLoginManager.Increment(out ShowRecaptcha);

                        if (!bruteForceSuccessAttempt)
                        {
                            if (!RecaptchaEnable)
                            {
                                throw new Authorize.BruteForceCredentialException();
                            }
                            else
                            {
                                var recaptchaResponse = Request["g-recaptcha-response"];
                                if (String.IsNullOrEmpty(recaptchaResponse)
                                    || !Authorize.ValidateRecaptcha(recaptchaResponse, requestIp))
                                {
                                    throw new Authorize.RecaptchaException();
                                }
                            }
                        }
                    }

                    var session = string.IsNullOrEmpty(Request["remember"]);
                    CookiesManager.AuthenticateMeAndSetCookies(Login, passwordHash, MessageAction.LoginSuccess, session);

                }
                catch (InvalidCredentialException ex)
                {
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
                        DontShowMainPage = true;
                    }

                    var loginName = string.IsNullOrWhiteSpace(Login) ? AuditResource.EmailNotSpecified : Login;

                    MessageService.Send(HttpContext.Current.Request, loginName, messageAction);

                    Auth.ProcessLogout();

                    return;
                }
                catch (System.Security.SecurityException)
                {
                    LoginMessage = Resource.ErrorDisabledProfile;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailDisabledProfile);
                    Auth.ProcessLogout();
                    return;
                }
                catch (Exception ex)
                {
                    LoginMessage = ex.Message;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFail);
                    Auth.ProcessLogout();
                    return;
                }

                if (bruteForceSuccessAttempt)
                {
                    bruteForceLoginManager.Decrement();
                }

                Response.Redirect(Context.GetRefererURL());
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