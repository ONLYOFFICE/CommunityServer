/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Core;
using ASC.Web.Core.Client;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    public class Global : HttpApplication
    {
        private static readonly object locker = new object();
        private static volatile bool applicationStarted;


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!applicationStarted)
            {
                lock (locker)
                {
                    if (!applicationStarted)
                    {
                        Startup.Configure();
                        applicationStarted = true;
                    }
                }
            }

            SecurityContext.Logout();
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);

            BlockNotFoundPortal(tenant);
            BlockRemovedOrSuspendedPortal(tenant);
            BlockTransferingOrRestoringPortal(tenant);
            BlockMigratingPortal(tenant);
            BlockPortalEncryption(tenant);
            BlockNotPaidPortal(tenant);
            TenantWhiteLabelSettings.Apply(tenant.TenantId);

            Authenticate();
            BlockIPSecurityPortal(tenant);
            ResolveUserCulture();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            CallContext.FreeNamedDataSlot(TenantManager.CURRENT_TENANT);
            SecurityContext.Logout();
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            Authenticate();
            ResolveUserCulture();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (Request.GetUrlRewriter().Scheme == "https")
                Response.Cookies["ASP.NET_SessionId"].Secure = true;

            Response.Cookies["ASP.NET_SessionId"].HttpOnly = true;
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (String.IsNullOrEmpty(custom)) return base.GetVaryByCustomString(context, custom);
            if (custom == "cacheKey") return ClientSettings.ResetCacheKey;

            var result = String.Empty;

            foreach (Match match in Regex.Matches(custom.ToLower(), "(?<key>\\w+)(\\[(?<subkey>\\w+)\\])?;?", RegexOptions.Compiled))
            {
                var key = String.Empty;
                var subkey = String.Empty;

                if (match.Groups["key"].Success)
                    key = match.Groups["key"].Value;

                if (match.Groups["subkey"].Success)
                    subkey = match.Groups["subkey"].Value;

                var value = String.Empty;

                switch (key)
                {
                    case "relativeurl":
                        var appUrl = VirtualPathUtility.ToAbsolute("~/");
                        value = Request.GetUrlRewriter().AbsolutePath.Remove(0, appUrl.Length - 1);
                        break;
                    case "url":
                        value = Request.GetUrlRewriter().AbsoluteUri;
                        break;
                    case "page":
                        value = Request.GetUrlRewriter().AbsolutePath.Substring(Request.Url.AbsolutePath.LastIndexOfAny(new[] { '/', '\\' }) + 1);
                        break;
                    case "module":
                        var module = "common";
                        var matches = Regex.Match(Request.Url.AbsolutePath.ToLower(), "(products|addons)/(\\w+)/?", RegexOptions.Compiled);

                        if (matches.Success && matches.Groups.Count > 2 && matches.Groups[2].Success)
                            module = matches.Groups[2].Value;

                        value = module;

                        break;
                    case "culture":
                        value = Thread.CurrentThread.CurrentUICulture.Name;
                        break;
                    case "theme":
                        value = ColorThemesSettings.GetColorThemesSettings();
                        break;
                    case "whitelabel":
                        var whiteLabelSettings = TenantWhiteLabelSettings.Load();
                        value = whiteLabelSettings.LogoText ?? string.Empty;
                        break;
                }

                if (!(String.Compare((value ?? "").ToLower(), subkey, StringComparison.Ordinal) == 0
                      || String.IsNullOrEmpty(subkey))) continue;

                result += "|" + value;

            }

            if (!string.IsNullOrEmpty(result)) return String.Join("|", ClientSettings.ResetCacheKey, result);

            return base.GetVaryByCustomString(context, custom);
        }

        public static bool Authenticate()
        {
            if (SecurityContext.IsAuthenticated)
            {
                return true;
            }

            var authenticated = false;
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                if (HttpContext.Current != null)
                {
                    string cookie;
                    if (AuthorizationHelper.ProcessBasicAuthorization(HttpContext.Current, out cookie))
                    {
                        CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
                        authenticated = true;
                    }
                }
                if (!authenticated)
                {
                    var cookie = CookiesManager.GetCookies(CookiesType.AuthKey);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        authenticated = SecurityContext.AuthenticateMe(cookie);

                        if (!authenticated)
                        {
                            var comebackAuthCookies = CookiesManager.GetCookies(CookiesType.ComebackAuthKey);

                            if (!string.IsNullOrEmpty(comebackAuthCookies) && SecurityContext.AuthenticateMe(comebackAuthCookies))
                            {
                                CookiesManager.SetCookies(CookiesType.AuthKey, comebackAuthCookies);
                                CookiesManager.ClearCookies(CookiesType.ComebackAuthKey);
                                authenticated = true;
                            }
                            else
                            {
                                Auth.ProcessLogout();
                                return false;
                            }
                        }
                    }
                }

                var accessSettings = TenantAccessSettings.Load();
                if (authenticated && SecurityContext.CurrentAccount.ID == ASC.Core.Users.Constants.OutsideUser.ID && !accessSettings.Anyone)
                {
                    Auth.ProcessLogout();
                    authenticated = false;
                }
            }
            return authenticated;
        }

        private static void ResolveUserCulture()
        {
            CultureInfo culture = null;

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                culture = tenant.GetCulture();
            }

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            if (!string.IsNullOrEmpty(user.CultureName))
            {
                culture = CultureInfo.GetCultureInfo(user.CultureName);
            }

            if (culture != null && !Equals(Thread.CurrentThread.CurrentCulture, culture))
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            if (culture != null && !Equals(Thread.CurrentThread.CurrentUICulture, culture))
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        private void BlockNotFoundPortal(Tenant tenant)
        {
            if (tenant == null)
            {
                if (string.IsNullOrEmpty(SetupInfo.NoTenantRedirectURL))
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    Response.End();
                }
                else
                {
                    var requestUri = Request.GetUrlRewriter();
                    var requestUrl = requestUri.ToString();
                    var redirectUrl = string.Format("{0}?url={1}&ref={2}", SetupInfo.NoTenantRedirectURL, requestUri.Host, HttpUtility.UrlEncode(requestUrl));
                    ResponseRedirect(redirectUrl, HttpStatusCode.NotFound);
                }
            }
        }

        private void BlockRemovedOrSuspendedPortal(Tenant tenant)
        {
            if (tenant.Status != TenantStatus.RemovePending && tenant.Status != TenantStatus.Suspended)
            {
                return;
            }

            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", "confirm.aspx", "capabilities.json" };
            if (tenant.Status == TenantStatus.Suspended && passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            if (string.IsNullOrEmpty(SetupInfo.NoTenantRedirectURL))
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                Response.End();
            }
            else
            {
                var requestUri = Request.GetUrlRewriter();
                var requestUrl = requestUri.ToString();
                var redirectUrl = string.Format("{0}?url={1}&ref={2}", SetupInfo.NoTenantRedirectURL, requestUri.Host, HttpUtility.UrlEncode(requestUrl));
                ResponseRedirect(redirectUrl, HttpStatusCode.NotFound);
            }
        }

        private void BlockTransferingOrRestoringPortal(Tenant tenant)
        {
            if (tenant.Status != TenantStatus.Restoring && tenant.Status != TenantStatus.Transfering)
            {
                return;
            }

            // allow requests to backup handler to get access to the GetRestoreStatus method
            var handlerType = typeof(BackupAjaxHandler);
            var backupHandler = handlerType.FullName + "," + handlerType.Assembly.GetName().Name + ".ashx";

            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", ".ico", ".png", backupHandler, "PreparationPortal.aspx", "TenantLogo.ashx", "portal/getrestoreprogress.json", "capabilities.json" };
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            ResponseRedirect("~/PreparationPortal.aspx?type=" + (tenant.Status == TenantStatus.Transfering ? "0" : "1"), HttpStatusCode.ServiceUnavailable);
        }

        private void BlockMigratingPortal(Tenant tenant)
        {
            if (tenant.Status != TenantStatus.Migrating)
            {
                return;
            }

            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", ".ico", ".png", "MigrationPortal.aspx", "TenantLogo.ashx", "settings/storage/progress.json", "capabilities.json" };
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            ResponseRedirect("~/MigrationPortal.aspx", HttpStatusCode.ServiceUnavailable);
        }

        private void BlockPortalEncryption(Tenant tenant)
        {
            if (tenant.Status != TenantStatus.Encryption)
            {
                return;
            }

            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", ".ico", ".png", "PortalEncryption.aspx", "TenantLogo.ashx", "settings/encryption/progress.json", "capabilities.json" };
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            ResponseRedirect("~/PortalEncryption.aspx", HttpStatusCode.ServiceUnavailable);
        }

        private void BlockNotPaidPortal(Tenant tenant)
        {
            if (tenant == null) return;

            var passthroughtRequestEndings = new[] { ".htm", ".ashx", ".png", ".ico", ".less", ".css", ".js", "capabilities.json" };
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            if (!TenantExtra.EnableTariffSettings && TenantExtra.GetCurrentTariff().State >= TariffState.NotPaid)
            {
                if (string.IsNullOrEmpty(AdditionalWhiteLabelSettings.Instance.BuyUrl)
                    || AdditionalWhiteLabelSettings.Instance.BuyUrl == AdditionalWhiteLabelSettings.DefaultBuyUrl)
                {
                    LogManager.GetLogger("ASC").WarnFormat("Tenant {0} is not paid", tenant.TenantId);
                    Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                    Response.End();
                }
                else if (!Request.Url.AbsolutePath.EndsWith(CommonLinkUtility.ToAbsolute(PaymentRequired.Location)))
                {
                    ResponseRedirect(PaymentRequired.Location, HttpStatusCode.PaymentRequired);
                }
            }
        }

        private void BlockIPSecurityPortal(Tenant tenant)
        {
            if (tenant == null) return;

            var settings = IPRestrictionsSettings.LoadForTenant(tenant.TenantId);
            if (settings.Enable && SecurityContext.IsAuthenticated && !IPSecurity.IPSecurity.Verify(tenant))
            {
                Auth.ProcessLogout();

                ResponseRedirect("~/Auth.aspx?error=ipsecurity", HttpStatusCode.Forbidden);
            }
        }

        private void ResponseRedirect(string url, HttpStatusCode httpStatusCode)
        {
            if (Request.Url.AbsolutePath.StartsWith(SetupInfo.WebApiBaseUrl, StringComparison.InvariantCultureIgnoreCase) ||
                Request.Url.AbsolutePath.EndsWith(".svc", StringComparison.InvariantCultureIgnoreCase) ||
                Request.Url.AbsolutePath.EndsWith(".ashx", StringComparison.InvariantCultureIgnoreCase))
            {
                // we shouldn't redirect
                Response.StatusCode = (int)httpStatusCode;
                Response.End();
            }

            Response.Redirect(url, true);
        }
    }
}