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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Settings;
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
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

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
                        applicationStarted = true;
                        Startup.Configure();
                    }
                }
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);

            BlockNotFoundPortal(tenant);
            BlockRemovedOrSuspendedPortal(tenant);
            BlockTransferingOrRestoringPortal(tenant);
            BlockNotPaidPortal(tenant);
            TenantWhiteLabelSettings.Apply(tenant.TenantId);

            Authenticate();
            BlockIPSecurityPortal(tenant);
            ResolveUserCulture();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            CallContext.FreeNamedDataSlot(TenantManager.CURRENT_TENANT);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            Authenticate();
            ResolveUserCulture();
        }

        protected void Session_End(object sender, EventArgs e)
        {
            CommonControlsConfigurer.FCKClearTempStore(Session);
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
                        var whiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
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
                    }
                }

                var accessSettings = SettingsManager.Instance.LoadSettings<TenantAccessSettings>(tenant.TenantId);
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
                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                    Response.End();
                }
                else
                {
                    var redirectUrl = string.Format("{0}?url={1}", SetupInfo.NoTenantRedirectURL, Request.GetUrlRewriter().Host);
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
            
            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", "confirm.aspx" };
            if (tenant.Status == TenantStatus.Suspended && passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            if (string.IsNullOrEmpty(SetupInfo.NoTenantRedirectURL))
            {
                Response.StatusCode = (int) HttpStatusCode.NotFound;
                Response.End();
            }
            else
            {
                var redirectUrl = string.Format("{0}?url={1}", SetupInfo.NoTenantRedirectURL, Request.GetUrlRewriter().Host);
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

            var passthroughtRequestEndings = new[] { ".js", ".css", ".less", backupHandler, "PreparationPortal.aspx", "portal/getrestoreprogress.json",  };
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            ResponseRedirect("~/PreparationPortal.aspx?type=" + (tenant.Status == TenantStatus.Transfering ? "0" : "1"), HttpStatusCode.ServiceUnavailable);
        }

        private void BlockNotPaidPortal(Tenant tenant)
        {
            if (tenant == null) return;

            var passthroughtRequestEndings = new[] {".htm", ".ashx", ".png", ".ico", ".less", ".css", ".js"};
            if (passthroughtRequestEndings.Any(path => Request.Url.AbsolutePath.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            if (!TenantExtra.EnableTarrifSettings && TenantExtra.GetCurrentTariff().State >= TariffState.NotPaid)
            {
                if (string.IsNullOrEmpty(AdditionalWhiteLabelSettings.Instance.BuyUrl)
                    || AdditionalWhiteLabelSettings.Instance.BuyUrl == AdditionalWhiteLabelSettings.DefaultBuyUrl)
                {
                    Response.StatusCode = (int) HttpStatusCode.PaymentRequired;
                    Response.End();
                }
                else if (!Request.Url.AbsolutePath.EndsWith(CommonLinkUtility.ToAbsolute(PaymentRequired.Location)))
                {
                    ResponseRedirect(PaymentRequired.Location, HttpStatusCode.PaymentRequired);
                }
                return;
            }

            if (CoreContext.Configuration.Standalone)
            {
                var licenseDay = TenantExtra.GetCurrentTariff().LicenseDate.Date;
                if (licenseDay < DateTime.Today && licenseDay < TenantExtra.VersionReleaseDate)
                {
                    Response.StatusCode = (int) HttpStatusCode.PaymentRequired;
                    Response.End();
                }
            }
        }

        private void BlockIPSecurityPortal(Tenant tenant)
        {
            if (tenant == null) return;

            var settings = SettingsManager.Instance.LoadSettings<IPRestrictionsSettings>(tenant.TenantId);
            if (settings.Enable && SecurityContext.IsAuthenticated && !IPSecurity.IPSecurity.Verify(tenant))
            {
                Auth.ProcessLogout();

                ResponseRedirect("~/auth.aspx?error=ipsecurity", HttpStatusCode.Forbidden);
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