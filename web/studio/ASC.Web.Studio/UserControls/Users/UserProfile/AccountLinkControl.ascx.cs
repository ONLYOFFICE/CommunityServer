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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.FederatedLogin.LoginProviders;
using ASC.FederatedLogin.Profile;
using ASC.Web.Core.Mobile;
using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class AccountLinkControl : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/AccountLinkControl.ascx"; }
        }

        public static List<string> AuthProviders = new List<string>
            {
                ProviderConstants.Google,
                ProviderConstants.Facebook,
                ProviderConstants.Twitter,
                ProviderConstants.LinkedIn,
                ProviderConstants.MailRu,
                ProviderConstants.VK,
                ProviderConstants.Yandex,
                ProviderConstants.GosUslugi
            };

        public static bool IsNotEmpty
        {
            get
            {
                return AuthProviders
                    .Select(ProviderManager.GetLoginProvider)
                    .Any(loginProvider => loginProvider!= null && loginProvider.IsEnabled);
            }
        }

        public bool SettingsView { get; set; }
        public bool InviteView { get; set; }

        protected ICollection<AccountInfo> Infos = new List<AccountInfo>();

        public AccountLinkControl()
        {
            ClientCallback = "loginCallback";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/UserControls/Users/UserProfile/css/accountlink_style.less")
                .RegisterBodyScripts("~/UserControls/Users/UserProfile/js/accountlinker.js");
            InitProviders();

            Page.RegisterInlineScript(String.Format(@" AccountLinkControl_Providers = {0};
                                                       AccountLinkControl_SettingsView = {1};
                                                       AccountLinkControl_InviteView = {2};",
                                    JsonConvert.SerializeObject(Infos),
                                    SettingsView.ToString().ToLower(),
                                    InviteView.ToString().ToLower()), onReady: false);
        }

        public string ClientCallback { get; set; }

        private void InitProviders()
        {
            IEnumerable<LoginProfile> linkedAccounts = new List<LoginProfile>();

            if (SecurityContext.IsAuthenticated)
            {
                linkedAccounts = GetLinker().GetLinkedProfiles(SecurityContext.CurrentAccount.ID.ToString());
            }

            var fromOnly = string.IsNullOrWhiteSpace(HttpContext.Current.Request["fromonly"]) ? string.Empty : HttpContext.Current.Request["fromonly"].ToLower();

            foreach (var provider in AuthProviders.Where(provider => string.IsNullOrEmpty(fromOnly) || fromOnly == provider || (provider == "google" && fromOnly == "openid")))
            {
                if (InviteView && provider.ToLower() == "twitter") continue;

                var loginProvider = ProviderManager.GetLoginProvider(provider);
                if (loginProvider != null && loginProvider.IsEnabled)
                    AddProvider(provider, linkedAccounts);
            }
        }

        private void AddProvider(string provider, IEnumerable<LoginProfile> linkedAccounts)
        {
            Infos.Add(new AccountInfo
                {
                    Linked = linkedAccounts.Any(x => x.Provider == provider),
                    Provider = provider,
                    Url = VirtualPathUtility.ToAbsolute("~/login.ashx")
                          + "?auth=" + provider
                          + (SettingsView || InviteView || (!MobileDetector.IsMobile && !Request.DesktopApp())
                                 ? ("&mode=popup&callback=" + ClientCallback)
                                 : ("&mode=Redirect&returnurl=" + HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(), "auth.aspx").ToString())))
                });
        }

        private static AccountLinker GetLinker()
        {
            return new AccountLinker("webstudio");
        }
    }

    public class AccountInfo
    {
        public string Provider { get; set; }
        public string Url { get; set; }
        public bool Linked { get; set; }
        public string Class { get; set; }
    }
}