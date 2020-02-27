/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Globalization;
using System.Security.Authentication;
using System.Web;
using System.Web.Configuration;
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

        private readonly ICache cache = AscCache.Memory;
        protected string Login;

        protected string LoginMessage;
        protected int LoginMessageType = 0;

        protected string HelpLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginMessage = Request["m"];

            Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/authorizedocs.less", "~/UserControls/Common/AuthorizeDocs/css/slick.less")
                .RegisterBodyScripts("~/UserControls/Common/AuthorizeDocs/js/authorizedocs.js", "~/UserControls/Common/Authorize/js/authorize.js", "~/js/third-party/slick.min.js");

            if (CoreContext.Configuration.CustomMode)
                Page.RegisterStyle("~/UserControls/Common/AuthorizeDocs/css/custom-mode.less");

            Page.Title = CoreContext.Configuration.CustomMode ? CustomModeResource.TitlePageNewCustomMode : Resource.AuthDocsTitlePage;
            Page.MetaDescription = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaDescriptionCustomMode.HtmlEncode() : Resource.AuthDocsMetaDescription.HtmlEncode();
            Page.MetaKeywords = CoreContext.Configuration.CustomMode ? CustomModeResource.AuthDocsMetaKeywordsCustomMode : Resource.AuthDocsMetaKeywords;

            HelpLink = GetHelpLink();

            PersonalFooterHolder.Controls.Add(LoadControl(CoreContext.Configuration.CustomMode
                                                              ? PersonalFooter.PersonalFooter.LocationCustomMode
                                                              : PersonalFooter.PersonalFooter.Location));

            if (AccountLinkControl.IsNotEmpty)
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
                    var password = Request["pwd"];

                    if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(password))
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

                    var cookiesKey = SecurityContext.AuthenticateMe(Login, password);
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
                baseHelpLink = WebConfigurationManager.AppSettings["web.faq-url"] ?? string.Empty;

            return baseHelpLink.TrimEnd('/');
        }
    }
}