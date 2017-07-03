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
using System.Web;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.SingleSignOn.Common;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Import;
using ASC.Web.Studio.UserControls;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.AuthorizeDocs;
using ASC.Web.Studio.Utility;

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

        protected override bool RedirectToStartup { get { return false; } }

        protected string TenantName;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!SecurityContext.IsAuthenticated)
            {
                if (CoreContext.Configuration.Personal)
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        var admin = CoreContext.UserManager.GetUserByUserName("administrator");
                        var cookie = SecurityContext.AuthenticateMe(admin.ID);
                        CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
                        Response.Redirect(CommonLinkUtility.GetDefault(), true);
                    }

                    if (Request["campaign"] == "personal")
                    {
                        Session["campaign"] = "personal";
                    }
                    CheckSocialMedia();

                    SetLanguage(abTesting: true);
                }

                var token = Request["asc_auth_key"];
                if (SecurityContext.AuthenticateMe(token))
                {
                    CookiesManager.SetCookies(CookiesType.AuthKey, token);

                    var refererURL = Request["refererURL"];
                    if (string.IsNullOrEmpty(refererURL)) refererURL = "~/auth.aspx";

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
                    var settings = SettingsManager.Instance.LoadSettings<SsoSettingsV2>(TenantProvider.CurrentTenantID);

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

                Response.Redirect("~/auth.aspx", true);
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
            withHelpBlock = false;
            if (CoreContext.Configuration.Personal)
            {
                Master.TopStudioPanel.TopLogo = WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth.png");
                AutorizeDocuments.Controls.Add(LoadControl(AuthorizeDocs.Location));
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
                var returnUrl = new Uri(Request.GetUrlRewriter(), "auth.aspx");
                loginUrl = "~/login.ashx?auth=" + social
                           + "&mode=Redirect&returnurl=" + HttpUtility.UrlEncode(returnUrl.ToString());
            }

            if (!string.IsNullOrEmpty(loginUrl))
            {
                Response.Redirect(loginUrl, true);
            }
        }
    }
}