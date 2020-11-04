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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Core;
using ASC.Core.Common;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Web.Studio.Utility
{
    public enum ManagementType
    {
        General = 0,
        Customization = 1,
        ProductsAndInstruments = 2,
        PortalSecurity = 3,
        AccessRights = 4,
        Backup = 5,
        LoginHistory = 6,
        AuditTrail = 7,
        LdapSettings = 8,
        ThirdPartyAuthorization = 9,
        SmtpSettings = 10,
        Statistic = 11,
        Monitoring = 12,
        SingleSignOnSettings = 13,
        Migration = 14,
        DeletionPortal = 15,
        HelpCenter = 16,
        DocService = 17,
        FullTextSearch = 18,
        WhiteLabel = 19,
        MailService = 20,
        Storage = 21,
        PrivacyRoom = 22
    }

    //  emp-invite - confirm ivite by email
    //  portal-suspend - confirm portal suspending - Tenant.SetStatus(TenantStatus.Suspended)
    //  portal-continue - confirm portal continuation  - Tenant.SetStatus(TenantStatus.Active)
    //  portal-remove - confirm portal deletation - Tenant.SetStatus(TenantStatus.RemovePending)
    //  DnsChange - change Portal Address and/or Custom domain name
    public enum ConfirmType
    {
        EmpInvite,
        LinkInvite,
        PortalSuspend,
        PortalContinue,
        PortalRemove,
        DnsChange,
        PortalOwnerChange,
        Activation,
        EmailChange,
        EmailActivation,
        PasswordChange,
        ProfileRemove,
        PhoneActivation,
        PhoneAuth,
        Auth,
        TfaActivation,
        TfaAuth
    }

    public static class CommonLinkUtility
    {
        private static readonly Regex RegFilePathTrim = new Regex("/[^/]*\\.aspx", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public const string ParamName_ProductSysName = "product";
        public const string ParamName_UserUserName = "user";
        public const string ParamName_UserUserID = "uid";

        public static void Initialize(string serverUri)
        {
            BaseCommonLinkUtility.Initialize(serverUri);
        }

        public static string VirtualRoot
        {
            get { return BaseCommonLinkUtility.VirtualRoot; }
        }

        public static string ServerRootPath
        {
            get { return BaseCommonLinkUtility.ServerRootPath; }
        }

        public static string GetFullAbsolutePath(string virtualPath)
        {
            return BaseCommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        public static string ToAbsolute(string virtualPath)
        {
            return BaseCommonLinkUtility.ToAbsolute(virtualPath);
        }

        public static string Logout
        {
            get { return ToAbsolute("~/Auth.aspx") + "?t=logout"; }
        }

        public static string GetDefault()
        {
            return VirtualRoot;
        }

        public static string GetMyStaff()
        {
            return CoreContext.Configuration.Personal ? ToAbsolute("~/My.aspx") : ToAbsolute("~/Products/People/Profile.aspx");
        }

        public static string GetEmployees()
        {
            return GetEmployees(EmployeeStatus.Active);
        }

        public static string GetEmployees(EmployeeStatus empStatus)
        {
            return ToAbsolute("~/Products/People/") +
                   (empStatus == EmployeeStatus.Terminated ? "#type=disabled" : string.Empty);
        }

        public static string GetDepartment(Guid depId)
        {
            return depId != Guid.Empty ? ToAbsolute("~/Products/People/#group=") + depId.ToString() : GetEmployees();
        }

        #region user profile link

        public static string GetUserProfile()
        {
            return GetUserProfile(null);
        }

        public static string GetUserProfile(Guid userID)
        {
            if (!CoreContext.UserManager.UserExists(userID))
                return GetEmployees();

            return GetUserProfile(userID.ToString());
        }

        public static string GetUserProfile(string user)
        {
            return GetUserProfile(user, true);
        }

        public static string GetUserProfile(string user, bool absolute)
        {
            var queryParams = "";

            if (!String.IsNullOrEmpty(user))
            {
                var guid = Guid.Empty;
                if (!String.IsNullOrEmpty(user) && 32 <= user.Length && user[8] == '-')
                {
                    try
                    {
                        guid = new Guid(user);
                    }
                    catch
                    {
                    }
                }

                queryParams = guid != Guid.Empty ? GetUserParamsPair(guid) : ParamName_UserUserName + "=" + HttpUtility.UrlEncode(user);
            }

            var url = absolute ? ToAbsolute("~/Products/People/") : "/Products/People/";
            url += "Profile.aspx?";
            url += queryParams;

            return url;
        }

        #endregion

        public static Guid GetProductID()
        {
            var productID = Guid.Empty;

            if (HttpContext.Current != null)
            {
                IProduct product;
                IModule module;
                GetLocationByRequest(out product, out module);
                if (product != null) productID = product.ID;
                }

            return productID;
        }

        public static Guid GetAddonID() {
            var addonID = Guid.Empty;

            if (HttpContext.Current != null)
            {
                var addonName = GetAddonNameFromUrl(HttpContext.Current.Request.Url.AbsoluteUri);

                switch (addonName)
                {
                    case "mail":
                        addonID = WebItemManager.MailProductID;
                        break;
                    case "talk":
                        addonID = WebItemManager.TalkProductID;
                        break;
                    case "calendar":
                        addonID = WebItemManager.CalendarProductID;
                        break;
                    default:
                        break;
                }
            }

            return addonID;
        }

        public static void GetLocationByRequest(out IProduct currentProduct, out IModule currentModule)
        {
            GetLocationByRequest(out currentProduct, out currentModule, HttpContext.Current);
        }

        public static void GetLocationByRequest(out IProduct currentProduct, out IModule currentModule, HttpContext context)
        {
            var currentURL = string.Empty;
            if (context != null && context.Request != null)
            {
                currentURL = HttpContext.Current.Request.GetUrlRewriter().AbsoluteUri;

                // http://[hostname]/[virtualpath]/[AjaxPro.Utility.HandlerPath]/[assembly],[classname].ashx
                if (currentURL.Contains("/" + AjaxPro.Utility.HandlerPath + "/") && HttpContext.Current.Request.UrlReferrer != null)
                {
                    currentURL = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                }
            }

            GetLocationByUrl(currentURL, out currentProduct, out currentModule);
        }

        public static IWebItem GetWebItemByUrl(string currentURL)
        {
            if (!String.IsNullOrEmpty(currentURL))
            {

                var itemName = GetWebItemNameFromUrl(currentURL);
                if (!string.IsNullOrEmpty(itemName))
                {
                    foreach (var item in WebItemManager.Instance.GetItemsAll())
                    {
                        var _itemName = GetWebItemNameFromUrl(item.StartURL);
                        if (String.Compare(itemName, _itemName, StringComparison.InvariantCultureIgnoreCase) == 0)
                            return item;
                    }
                }
                else
                {
                    var urlParams = HttpUtility.ParseQueryString(new Uri(currentURL).Query);
                    var productByName = GetProductBySysName(urlParams[ParamName_ProductSysName]);
                    var pid = productByName == null ? Guid.Empty : productByName.ID;

                    if (pid == Guid.Empty && !String.IsNullOrEmpty(urlParams["pid"]))
                    {
                        try
                        {
                            pid = new Guid(urlParams["pid"]);
                        }
                        catch
                        {
                            pid = Guid.Empty;
                        }
                    }

                    if (pid != Guid.Empty)
                        return WebItemManager.Instance[pid];
                }
            }

            return null;
        }

        public static void GetLocationByUrl(string currentURL, out IProduct currentProduct, out IModule currentModule)
        {
            currentProduct = null;
            currentModule = null;

            if (String.IsNullOrEmpty(currentURL)) return;

            var urlParams = HttpUtility.ParseQueryString(new Uri(currentURL).Query);
            var productByName = GetProductBySysName(urlParams[ParamName_ProductSysName]);
            var pid = productByName == null ? Guid.Empty : productByName.ID;

            if (pid == Guid.Empty && !String.IsNullOrEmpty(urlParams["pid"]))
            {
                try
                {
                    pid = new Guid(urlParams["pid"]);
                }
                catch
                {
                    pid = Guid.Empty;
                }
            }

            var productName = GetProductNameFromUrl(currentURL);
            var moduleName = GetModuleNameFromUrl(currentURL);

            if (!string.IsNullOrEmpty(productName) || !string.IsNullOrEmpty(moduleName))
            {
                foreach (var product in WebItemManager.Instance.GetItemsAll<IProduct>())
                {
                    var _productName = GetProductNameFromUrl(product.StartURL);
                    if (!string.IsNullOrEmpty(_productName))
                    {
                        if (String.Compare(productName, _productName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            currentProduct = product;

                            if (!String.IsNullOrEmpty(moduleName))
                            {
                                foreach (var module in WebItemManager.Instance.GetSubItems(product.ID).OfType<IModule>())
                                {
                                    var _moduleName = GetModuleNameFromUrl(module.StartURL);
                                    if (!string.IsNullOrEmpty(_moduleName))
                                    {
                                        if (String.Compare(moduleName, _moduleName, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        {
                                            currentModule = module;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var module in WebItemManager.Instance.GetSubItems(product.ID).OfType<IModule>())
                                {
                                    if (!module.StartURL.Equals(product.StartURL) && currentURL.Contains(RegFilePathTrim.Replace(module.StartURL, string.Empty)))
                                    {
                                        currentModule = module;
                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            if (pid != Guid.Empty)
                currentProduct = WebItemManager.Instance[pid] as IProduct;
        }

        private static string GetWebItemNameFromUrl(string url)
        {
            var name = GetModuleNameFromUrl(url);
            if (String.IsNullOrEmpty(name))
            {
                name = GetProductNameFromUrl(url);
                if (String.IsNullOrEmpty(name))
                {
                    return GetAddonNameFromUrl(url);
                }
            }

            return name;
        }

        private static string GetProductNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/Products/", StringComparison.InvariantCultureIgnoreCase);
                if (0 <= pos)
                {
                    url = url.Substring(pos + 10).ToLower();
                    pos = url.IndexOf('/');
                    return 0 < pos ? url.Substring(0, pos) : url;
                }
            }
            catch
            {
            }
            return null;
        }

        private static string GetAddonNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/addons/", StringComparison.InvariantCultureIgnoreCase);
                if (0 <= pos)
                {
                    url = url.Substring(pos + 8).ToLower();
                    pos = url.IndexOf('/');
                    return 0 < pos ? url.Substring(0, pos) : url;
                }
            }
            catch
            {
            }
            return null;
        }

        private static string GetModuleNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/Modules/", StringComparison.InvariantCultureIgnoreCase);
                if (0 <= pos)
                {
                    url = url.Substring(pos + 9).ToLower();
                    pos = url.IndexOf('/');
                    return 0 < pos ? url.Substring(0, pos) : url;
                }
            }
            catch
            {
            }
            return null;
        }

        private static IProduct GetProductBySysName(string sysName)
        {
            IProduct result = null;

            if (!String.IsNullOrEmpty(sysName))
                foreach (var product in WebItemManager.Instance.GetItemsAll<IProduct>())
                {
                    if (String.CompareOrdinal(sysName, WebItemExtension.GetSysName(product as IWebItem)) == 0)
                    {
                        result = product;
                        break;
                    }
                }

            return result;
        }

        public static string GetUserParamsPair(Guid userID)
        {
            return
                CoreContext.UserManager.UserExists(userID)
                    ? GetUserParamsPair(CoreContext.UserManager.GetUsers(userID))
                    : "";
        }

        public static string GetUserParamsPair(UserInfo user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
                return "";

            return String.Format("{0}={1}", ParamName_UserUserName, HttpUtility.UrlEncode(user.UserName.ToLowerInvariant()));
        }

        #region Help Centr

        public static string GetHelpLink(bool inCurrentCulture = true)
        {
            if (!AdditionalWhiteLabelSettings.Instance.HelpCenterEnabled)
                return String.Empty;

            var url = AdditionalWhiteLabelSettings.DefaultHelpCenterUrl;

            if (String.IsNullOrEmpty(url))
                return String.Empty;

            return GetRegionalUrl(url, inCurrentCulture ? CultureInfo.CurrentCulture.TwoLetterISOLanguageName : null);
        }

        public static string GetRegionalUrl(string url, string lang)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            //-replace language
            var regex = new Regex("{.*?}");
            var matches = regex.Matches(url);

            if (String.IsNullOrEmpty(lang))
            {
                url = matches.Cast<Match>().Aggregate(url, (current, match) => current.Replace(match.Value, String.Empty));
            }
            else
            {
                foreach (Match match in matches)
                {
                    var values = match.Value.TrimStart('{').TrimEnd('}').Split('|');
                    url = url.Replace(match.Value, values.Contains(lang) ? lang : String.Empty);
                }
            }
            //-

            //--remove redundant slashes
            var uri = new Uri(url);

            if (uri.Scheme == "mailto")
                return uri.OriginalString;

            var baseUri = new UriBuilder(uri.Scheme, uri.Host, uri.Port).Uri;
            baseUri = uri.Segments.Aggregate(baseUri, (current, segment) => new Uri(current, segment));
            //--
            //todo: lost query string!!!


            return baseUri.ToString().TrimEnd('/');
        }

        #endregion

        #region management links

        public static string GetAdministration(ManagementType managementType)
        {
            if (managementType == ManagementType.General)
                return ToAbsolute("~/Management.aspx") + string.Empty;

            return ToAbsolute("~/Management.aspx") + "?" + "type=" + ((int)managementType).ToString();
        }

        #endregion

        #region confirm links

        public static string GetConfirmationUrl(string email, ConfirmType confirmType, object postfix = null, Guid userId = default(Guid))
        {
            return GetFullAbsolutePath(GetConfirmationUrlRelative(CoreContext.TenantManager.GetCurrentTenant().TenantId, email, confirmType, postfix, userId));
        }

        public static string GetConfirmationUrlRelative(int tenantId, string email, ConfirmType confirmType, object postfix = null, Guid userId = default(Guid))
        {
            var validationKey = EmailValidationKeyProvider.GetEmailKey(tenantId, email + confirmType + (postfix ?? ""));

            var link = string.Format("confirm.aspx?type={0}&key={1}", confirmType, validationKey);

            if (!string.IsNullOrEmpty(email))
            {
                link += "&email=" + HttpUtility.UrlEncode(email);
            }

            if (userId != default(Guid))
            {
                link += "&uid=" + userId;
            }

            return link;
        }

        #endregion

    }
}
