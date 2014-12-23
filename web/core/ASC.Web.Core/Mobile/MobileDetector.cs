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

using ASC.Core;
using ASC.Core.Caching;
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

        private static readonly ICache cache = AscCache.Default;


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
                result = cache.Get(key) as bool?;
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