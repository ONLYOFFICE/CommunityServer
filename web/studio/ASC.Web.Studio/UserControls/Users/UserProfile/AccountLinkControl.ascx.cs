/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
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
                    !string.IsNullOrEmpty(KeyStorage.Get("googleConsumerKey"))
                    || !string.IsNullOrEmpty(KeyStorage.Get("facebookAppID"))
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

            if (!string.IsNullOrEmpty(KeyStorage.Get("googleClientId")) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "google" || fromOnly == "openid"))
                AddProvider(ProviderConstants.Google, linkedAccounts);

            if (!string.IsNullOrEmpty(KeyStorage.Get("facebookAppID")) && (string.IsNullOrEmpty(fromOnly) || fromOnly == "facebook"))
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
                          + (SettingsView || InviteView || !MobileDetector.IsMobile || provider == ProviderConstants.Google
                                 ? ("&mode=popup&callback=" + ClientCallback)
                                 : ("&mode=Redirect&returnurl=" + HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(), "auth.aspx").ToString())))
                });
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse LinkAccount(string serializedProfile)
        {
            //Link it
            var profile = new LoginProfile(serializedProfile);
            GetLinker().AddLink(SecurityContext.CurrentAccount.ID.ToString(), profile);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserLinkedSocialAccount, GetMeaningfulProviderName(profile.Provider));
            
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

        public AccountLinker GetLinker()
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