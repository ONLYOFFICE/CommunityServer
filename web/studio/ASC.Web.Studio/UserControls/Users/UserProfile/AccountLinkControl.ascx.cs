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
                                 : ("&mode=Redirect&returnurl=" 
                                    + HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(),
                                        "Auth.aspx"
                                        + (Request.DesktopApp() ? "?desktop=true" : "")
                                        ).ToString())
                                 ))
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