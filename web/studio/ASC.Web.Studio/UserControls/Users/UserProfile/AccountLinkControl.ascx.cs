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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.FederatedLogin.LoginProviders;
using ASC.MessagingSystem;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using ASC.Web.Core.Mobile;
using AjaxPro;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    [AjaxNamespace("AccountLinkControl")]
    public partial class AccountLinkControl : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/AccountLinkControl.ascx"; }
        }

        public static bool IsNotEmpty
        {
            get
            {
                return
                    !string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientId)
                    || !string.IsNullOrEmpty(FacebookLoginProvider.FacebookOAuth20ClientId)
                    || !string.IsNullOrEmpty(KeyStorage.Get("twitterKey"))
                    || !string.IsNullOrEmpty(KeyStorage.Get("linkedInKey"));
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
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/users/userprofile/css/accountlink_style.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/userprofile/js/accountlinker.js"));
            InitProviders();
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

            if (!string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientId) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "google" || fromOnly == "openid"))
                AddProvider(ProviderConstants.Google, linkedAccounts);

            if (!string.IsNullOrEmpty(FacebookLoginProvider.FacebookOAuth20ClientId) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "facebook"))
                AddProvider(ProviderConstants.Facebook, linkedAccounts);

            if (!string.IsNullOrEmpty(KeyStorage.Get("twitterKey")) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "twitter"))
                AddProvider(ProviderConstants.Twitter, linkedAccounts);

            if (!string.IsNullOrEmpty(KeyStorage.Get("linkedInKey")) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "linkedin"))
                AddProvider(ProviderConstants.LinkedIn, linkedAccounts);
        }

        private void AddProvider(string provider, IEnumerable<LoginProfile> linkedAccounts)
        {
            Infos.Add(new AccountInfo
                {
                    Linked = linkedAccounts.Any(x => x.Provider == provider),
                    Provider = provider,
                    Url = VirtualPathUtility.ToAbsolute("~/login.ashx")
                          + "?auth=" + provider
                          + (SettingsView || InviteView || !MobileDetector.IsMobile
                                 ? ("&mode=popup&callback=" + ClientCallback)
                                 : ("&mode=Redirect&returnurl=" + HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(), "auth.aspx").ToString())))
                });
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse LinkAccount(string serializedProfile)
        {
            //Link it
            var profile = new LoginProfile(serializedProfile);

            if (string.IsNullOrEmpty(profile.AuthorizationError))
            {
                GetLinker().AddLink(SecurityContext.CurrentAccount.ID.ToString(), profile);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserLinkedSocialAccount, GetMeaningfulProviderName(profile.Provider));
            }
            else
            {
                // ignore cancellation
                if (profile.AuthorizationError != "Canceled at provider")
                {
                    throw new Exception(profile.AuthorizationError);
                }
            }
            return RenderControlHtml();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UnlinkAccount(string provider)
        {
            //Link it
            GetLinker().RemoveProvider(SecurityContext.CurrentAccount.ID.ToString(), provider);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUnlinkedSocialAccount, GetMeaningfulProviderName(provider));
            
            return RenderControlHtml();
        }

        private AjaxResponse RenderControlHtml()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                var ctrl = (AccountLinkControl) LoadControl(Location);
                ctrl.SettingsView = true;
                ctrl.InitProviders();
                ctrl.RenderControl(writer);
                return new AjaxResponse {rs1 = stringWriter.GetStringBuilder().ToString()};
            }
        }

        private static AccountLinker GetLinker()
        {
            return new AccountLinker("webstudio");
        }

        public IEnumerable<AccountInfo> GetLinkableProviders()
        {
            return Infos.Where(x => !(x.Provider.ToLower() == "twitter" || x.Provider.ToLower() == "linkedin"));
        }

        private static string GetMeaningfulProviderName(string providerName)
        {
            switch (providerName)
            {
                case "google":
                case "openid":
                    return "Google";
                case "facebook":
                    return "Facebook";
                case "twitter":
                    return "Twitter";
                case "linkedin":
                    return "LinkedIn";
                default:
                    return "Unknown Provider";
            }
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