/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Core;
using ASC.Core.Caching;
using ASC.Core.Users;
using ASC.FederatedLogin.Profile;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Import;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class Authorize : UserControl
    {
        protected string LoginMessage;
        private string _errorMessage;
        private readonly ICache cache = AscCache.Default;
        protected bool EnableLdap = ActiveDirectoryUserImporter.LdapIsEnable;
        protected bool EnableSso = SsoImporter.SsoIsEnable;
        protected bool IsSaml = SsoImporter.IsSaml;

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
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/authorize/css/authorize.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/authorize/js/authorize.js"));

            Login = "";
            Password = "";
            HashId = "";

            //Account link control
            AccountLinkControl accountLink = null;

            if (SetupInfo.ThirdPartyAuthEnabled && AccountLinkControl.IsNotEmpty)
            {
                accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
                accountLink.Visible = true;
                accountLink.ClientCallback = "authCallback";
                accountLink.SettingsView = false;
                signInPlaceholder.Controls.Add(accountLink);
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
                var tryByHash = false;
                var smsLoginUrl = string.Empty;
                try
                {
                    if (thirdPartyProfile != null)
                    {
                        if (string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
                        {
                            HashId = thirdPartyProfile.HashId;
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
                        if (!string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) && Request["__EVENTTARGET"] == "signInLogin" && accountLink != null)
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
                        throw new InvalidCredentialException("login");
                    }

                    if (!string.IsNullOrEmpty(Request["pwd"]))
                    {
                        Password = Request["pwd"];
                    }
                    else if (string.IsNullOrEmpty(HashId))
                    {
                        throw new InvalidCredentialException("password");
                    }

                    if (string.IsNullOrEmpty(HashId))
                    {
                        // защита от перебора: на 5-ый неправильный ввод делать Sleep
                        var counter = (int)(cache.Get("loginsec/" + Login) ?? 0);
                        if (++counter % 5 == 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }
                        cache.Insert("loginsec/" + Login, counter, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                    }

                    if (!ActiveDirectoryUserImporter.TryLdapAuth(Login, Password))
                    {
                        smsLoginUrl = SmsLoginUrl();
                        if (string.IsNullOrEmpty(smsLoginUrl))
                        {
                            var session = string.IsNullOrEmpty(Request["remember"]);

                            if (string.IsNullOrEmpty(HashId))
                            {
                                var cookiesKey = SecurityContext.AuthenticateMe(Login, Password);
                                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey, session);
                                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccess);
                            }
                            else
                            {
                                Guid userId;
                                tryByHash = LoginWithThirdParty.TryGetUserByHash(HashId, out userId);
                                var cookiesKey = SecurityContext.AuthenticateMe(userId);
                                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey, session);
                                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialAccount);
                            }
                        }
                    }
                }
                catch(InvalidCredentialException)
                {
                    Auth.ProcessLogout();
                    ErrorMessage = tryByHash ? Resource.LoginWithAccountNotFound : Resource.InvalidUsernameOrPassword;

                    var loginName = tryByHash && !string.IsNullOrWhiteSpace(HashId)
                                        ? HashId
                                        : string.IsNullOrWhiteSpace(Login) ? AuditResource.EmailNotSpecified : Login;
                    var messageAction = tryByHash ? MessageAction.LoginFailSocialAccountNotFound : MessageAction.LoginFailInvalidCombination;

                    MessageService.Send(HttpContext.Current.Request, loginName, messageAction);

                    return;
                }
                catch(System.Security.SecurityException)
                {
                    Auth.ProcessLogout();
                    ErrorMessage = Resource.ErrorDisabledProfile;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailDisabledProfile);
                    return;
                }
                catch(IPSecurityException)
                {
                    Auth.ProcessLogout();
                    ErrorMessage = Resource.ErrorIpSecurity;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailIpSecurity);
                    return;
                }
                catch(Exception ex)
                {
                    Auth.ProcessLogout();
                    ErrorMessage = ex.Message;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFail);
                    return;
                }

                if (!string.IsNullOrEmpty(smsLoginUrl))
                {
                    Response.Redirect(smsLoginUrl);
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
            ProcessConfirmedEmailCondition();
        }

        private string SmsLoginUrl()
        {
            if (!StudioSmsNotificationSettings.IsVisibleSettings
                || !StudioSmsNotificationSettings.Enable)
                return string.Empty;

            UserInfo user;

            if (string.IsNullOrEmpty(HashId))
            {
                user = CoreContext.UserManager.GetUsers(TenantProvider.CurrentTenantID, Login, Hasher.Base64Hash(Password, HashAlg.SHA256));
            }
            else
            {
                Guid userId;
                LoginWithThirdParty.TryGetUserByHash(HashId, out userId);
                user = CoreContext.UserManager.GetUsers(userId);
            }

            if (Constants.LostUser.Equals(user))
            {
                throw new InvalidCredentialException();
            }
            return Studio.Confirm.SmsConfirmUrl(user);
        }

        private void ProcessConfirmedEmailCondition()
        {
            if (IsPostBack) return;

            var confirmedEmail = Request.QueryString["confirmed-email"];

            if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

            Login = confirmedEmail;
            LoginMessage = String.Format("<div class=\"confirmBox\">{0} {1}</div>", Resource.MessageEmailConfirmed, Resource.MessageAuthorize);
        }
    }
}