/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.AuthorizeDocs;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core.Import;

namespace ASC.Web.Studio
{
    public partial class Auth : MainPage
    {
        protected string LogoPath = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID).GetAbsoluteCompanyLogoPath();
        protected bool withHelpBlock { get; set; }

        protected override bool MayNotAuth
        {
            get { return true; }
            set { }
        }

        protected override bool MayNotPaid
        {
            get { return true; }
            set { }
        }

        protected override bool MayPhoneNotActivate
        {
            get { return true; }
            set { }
        }
        protected bool? IsAutorizePartner { get; set; }
        protected Partner Partner { get; set; }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!SecurityContext.IsAuthenticated)
            {
                if (CoreContext.Configuration.Personal)
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        var cookie = SecurityContext.AuthenticateMe(UserManagerWrapper.AdminID);
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
                
                return;
            }

            if (IsLogout)
            {
                var loginName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(false);
                ProcessLogout();
                MessageService.Send(HttpContext.Current.Request, loginName, MessageAction.Logout);

                // slo redirect
                if (SsoImporter.SloIsEnable && HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Redirect(SsoImporter.SloEndPoint, true);
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
            Master.DisabledSidePanel = true;
            withHelpBlock = false;
            if (CoreContext.Configuration.Personal)
            {
                Master.TopStudioPanel.TopLogo = WebImageSupplier.GetAbsoluteWebPath("logo/logo_personal_auth.png");
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

            if (CoreContext.Configuration.PartnerHosted)
            {
                IsAutorizePartner = false;
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner != null)
                {
                    IsAutorizePartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                    Partner = partner;
                }
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