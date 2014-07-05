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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using log4net;

namespace ASC.Web.Studio.Utility
{
    public enum ManagementType
    {
        General = 0,
        ProductsAndInstruments = 1,
        AccessRights = 2,
        Customization = 3,
        Backup = 4,
        Statistic = 5,
        ThirdPartyAuthorization = 6,
        HelpCenter = 7,
        LdapSettings = 8,
        SingleSignOnSettings = 9,
        Monitoring = 10,
        LoginHistory = 11,
        AuditTrail = 12
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
        Auth
    }

    public static class CommonLinkUtility
    {
        private static readonly Regex RegFilePathTrim = new Regex("/[^/]*\\.aspx", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static Uri _serverRoot;
        private static string _vpath;
        private static string _hostname;

        public const string ParamName_ProductSysName = "product";
        public const string ParamName_UserUserName = "user";
        public const string ParamName_UserUserID = "uid";

        static CommonLinkUtility()
        {
            try
            {
                var uriBuilder = new UriBuilder(Uri.UriSchemeHttp, "localhost");
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var u = HttpContext.Current.Request.GetUrlRewriter();
                    uriBuilder = new UriBuilder(u.Scheme, "localhost", u.Port);
                }
                _serverRoot = uriBuilder.Uri;

                try
                {
                    _hostname = Dns.GetHostName();
                }
                catch
                {
                }
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error.StackTrace);
            }
        }

        public static void Initialize(string serverUri)
        {
            if (string.IsNullOrEmpty(serverUri)) throw new ArgumentNullException("serverUri");

            var uri = new Uri(serverUri.Replace('*', 'x').Replace('+', 'x'));
            _serverRoot = new UriBuilder(uri.Scheme, _serverRoot.Host, uri.Port).Uri;
            _vpath = "/" + uri.AbsolutePath.Trim('/');
        }

        public static string VirtualRoot
        {
            get { return ToAbsolute("~"); }
        }

        public static string ServerRootPath
        {
            get
            {
                /*
                 * NOTE: fixed bug with warning on SSL certificate when coming from Email to teamlab. 
                 * Valid only for users that have custom domain set. For that users we should use a http scheme
                 * Like https://mydomain.com that maps to <alias>.teamlab.com
                */
                var basedomain = WebConfigurationManager.AppSettings["core.base-domain"];
                var http = !string.IsNullOrEmpty(basedomain) && !CoreContext.TenantManager.GetCurrentTenant().TenantDomain.EndsWith("." + basedomain, StringComparison.OrdinalIgnoreCase);

                var u = HttpContext.Current != null ? HttpContext.Current.Request.GetUrlRewriter() : _serverRoot;
                var uriBuilder = new UriBuilder(http ? Uri.UriSchemeHttp : u.Scheme, u.Host, http && u.IsDefaultPort ? 80 : u.Port);

                if (uriBuilder.Uri.IsLoopback || CoreContext.Configuration.Standalone)
                {
                    var tenant = CoreContext.TenantManager.GetCurrentTenant();
                    if (!string.IsNullOrEmpty(tenant.MappedDomain))
                    {
                        uriBuilder = new UriBuilder(Uri.UriSchemeHttp + Uri.SchemeDelimiter + tenant.TenantDomain); // use TenantDomain, not MappedDomain
                    }
                    else
                    {
                        uriBuilder.Host = tenant.TenantDomain;
                    }
                }
                if (uriBuilder.Uri.IsLoopback && !string.IsNullOrEmpty(_hostname))
                {
                    uriBuilder.Host = _hostname;
                }

                return uriBuilder.Uri.ToString().TrimEnd('/');
            }
        }

        public static string GetFullAbsolutePath(string virtualPath)
        {
            if (String.IsNullOrEmpty(virtualPath))
                return ServerRootPath;

            if (virtualPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                return virtualPath;

            if (string.IsNullOrEmpty(virtualPath) || virtualPath.StartsWith("/"))
            {
                return ServerRootPath + virtualPath;
            }
            return ServerRootPath + VirtualRoot.TrimEnd('/') + "/" + virtualPath.TrimStart('~', '/');
        }

        public static string ToAbsolute(string virtualPath)
        {
            if (_vpath == null)
            {
                return VirtualPathUtility.ToAbsolute(virtualPath);
            }

            if (string.IsNullOrEmpty(virtualPath) || virtualPath.StartsWith("/"))
            {
                return virtualPath;
            }
            return (_vpath != "/" ? _vpath : string.Empty) + "/" + virtualPath.TrimStart('~', '/');
        }

        public static string Logout
        {
            get { return ToAbsolute("~/auth.aspx") + "?t=logout"; }
        }

        public static string GetDefault()
        {
            return VirtualRoot;
        }

        public static string GetMyStaff()
        {
            return ToAbsolute("~/products/people/profile.aspx");
        }

        public static string GetEmployees()
        {
            return GetEmployees(EmployeeStatus.Active);
        }

        public static string GetEmployees(EmployeeStatus empStatus)
        {
            return ToAbsolute("~/products/people/") +
                   (empStatus == EmployeeStatus.Terminated ? "#type=disabled" : string.Empty);
        }

        public static string GetDepartment(Guid depId)
        {
            return depId != Guid.Empty ? ToAbsolute("~/products/people/#group=") + depId.ToString() : GetEmployees();
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

            var url = absolute ? ToAbsolute("~/products/people/") : "/products/people/";
            url += "profile.aspx?";
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

            if (productID == Guid.Empty)
            {
                var pid = CallContext.GetData("asc.web.product_id");
                if (pid != null) productID = (Guid)pid;
            }

            return productID;
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

            }

            return name;
        }

        private static string GetProductNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/products/", StringComparison.InvariantCultureIgnoreCase);
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

        private static string GetModuleNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/modules/", StringComparison.InvariantCultureIgnoreCase);
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
            var url = WebConfigurationManager.AppSettings["web.help-center"] ?? string.Empty;
            if (url.Contains("{"))
            {
                var parts = url.Split('{');
                url = parts[0];
                if (inCurrentCulture && parts[1].Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                {
                    url += CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "/";
                }
            }
            return url;
        }

        #endregion

        #region management links

        public static string GetAdministration(ManagementType managementType)
        {
            if (managementType == ManagementType.General)
                return ToAbsolute("~/management.aspx") + string.Empty;

            return ToAbsolute("~/management.aspx") + "?" + "type=" + ((int)managementType).ToString();
        }

        #endregion

        #region confirm links

        public static string GetConfirmationUrl(string email, ConfirmType confirmType, object postfix = null, Guid userId = default(Guid))
        {
            return GetFullAbsolutePath(GetConfirmationUrlRelative(email, confirmType, postfix, userId));
        }

        public static string GetConfirmationUrlRelative(string email, ConfirmType confirmType, object postfix = null, Guid userId = default(Guid))
        {
            var validationKey = EmailValidationKeyProvider.GetEmailKey(email + confirmType + (postfix ?? ""));

            var link = string.Format("confirm.aspx?type={0}&key={1}", confirmType, validationKey);

            if (!string.IsNullOrEmpty(email))
            {
                link += "&email=" + HttpUtility.UrlEncode(email);
            }

            if (userId != default (Guid))
            {
                link += "&uid=" + userId;
            }

            if (postfix != null)
            {
                link += "&p=1";
            }

            return link;
        }

        #endregion
    }
}