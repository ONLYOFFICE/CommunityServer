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


using ASC.Common.Caching;
using ASC.Core;
using log4net;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Core.Mobile
{
    public class MobileDetector : IHttpModule
    {
        private static readonly Regex urlRegex;
        private static readonly Regex uaMobileRegex;
        private static readonly Regex uaRedirectRegex;
        private static readonly DateTime lastMobileDate;
        private static readonly string MobileAddress;

        private static readonly ICache cache = AscCache.Memory;


        public static bool IsMobile
        {
            get { return IsRequestMatchesMobile(); }
        }


        static MobileDetector()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.urlregex"]))
            {
                urlRegex = new Regex(ConfigurationManager.AppSettings["mobile.urlregex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.regex"]))
            {
                uaMobileRegex = new Regex(ConfigurationManager.AppSettings["mobile.regex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.redirect-regex"]))
            {
                uaRedirectRegex = new Regex(ConfigurationManager.AppSettings["mobile.redirect-regex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
            if (!DateTime.TryParse(WebConfigurationManager.AppSettings["mobile.redirect.date"], out lastMobileDate))
            {
                lastMobileDate = DateTime.MaxValue;
            }
            MobileAddress = ConfigurationManager.AppSettings["mobile.redirect-url"];
        }


        public void Init(HttpApplication context)
        {
            if (!string.IsNullOrEmpty(MobileAddress))
            {
                context.BeginRequest += BeginRequest;
            }
        }

        public void Dispose()
        {
        }


        public static bool IsRequestMatchesMobile(bool forMobileVersion = false)
        {
            bool? result = false;
            var ua = HttpContext.Current.Request.UserAgent;
            var regex = forMobileVersion ? uaRedirectRegex : uaMobileRegex;
            if (!string.IsNullOrEmpty(ua) && regex != null)
            {
                var key = "mobileDetetor/" + ua.GetHashCode();

                bool fromCache;

                if (bool.TryParse(cache.Get<string>(key), out fromCache))
                    result = fromCache;

                if (result == null)
                {
                    cache.Insert(key, result = regex.IsMatch(ua), TimeSpan.FromMinutes(10));
                }
            }
            return result.GetValueOrDefault();
        }


        private static void BeginRequest(object sender, EventArgs e)
        {
            try
            {
                if (CookiesManager.IsMobileBlocked())
                {
                    return;
                }

                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                if (tenant != null && tenant.CreatedDateTime > lastMobileDate)
                {
                    CookiesManager.SetCookies(CookiesType.NoMobile, "1", true);
                    return;
                }

                var context = ((HttpApplication)sender).Context;
                var url = context.Request.GetUrlRewriter();

                if (!IsRequestMatchesMobile(true) || (urlRegex != null && !urlRegex.IsMatch(url.ToString())))
                {
                    return;
                }

                var mobileAddress = MobileAddress;
                if (mobileAddress.StartsWith("~"))
                {
                    mobileAddress = VirtualPathUtility.ToAbsolute(mobileAddress);
                }

                var redirectUri = Uri.IsWellFormedUriString(mobileAddress, UriKind.Absolute) ? new Uri(mobileAddress) : new Uri(url, mobileAddress);
                if (redirectUri.Equals(url))
                {
                    return;
                }

                var builder = new UriBuilder(redirectUri);
                var abspath = url.AbsolutePath;
                if (!string.IsNullOrEmpty(abspath) && abspath.EndsWith("default.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    abspath = abspath.Substring(0, abspath.Length - "default.aspx".Length);
                }
                builder.Path += abspath;
                builder.Query += url.Query.TrimStart('?');
                redirectUri = builder.Uri;

                LogManager.GetLogger("ASC.Mobile.Redirect").DebugFormat("Redirecting url:'{1}' to mobile. UA={0}", context.Request.UserAgent, url);
                context.Response.Redirect(redirectUri.ToString(), true);
            }
            catch (ThreadAbortException)
            {
                //Don't do nothing
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Mobile.Redirect").Error("failed to redirect user to mobile", ex);
            }
        }
    }
}