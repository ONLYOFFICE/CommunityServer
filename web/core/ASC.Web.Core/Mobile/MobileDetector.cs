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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ASC.Core;
using log4net;

namespace ASC.Web.Core.Mobile
{
    public class MobileDetector : IHttpModule
    {
        #region IHttpModule

        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        private static readonly Regex UrlCheckRegex;
        private static readonly Regex UserAgentMobileRegex;
        private static readonly Regex UserAgentRedirectRegex;

        static MobileDetector()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.urlregex"]))
            {
                UrlCheckRegex = new Regex(ConfigurationManager.AppSettings["mobile.urlregex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.regex"]))
            {
                UserAgentMobileRegex = new Regex(ConfigurationManager.AppSettings["mobile.regex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mobile.redirect-regex"]))
            {
                UserAgentRedirectRegex = new Regex(ConfigurationManager.AppSettings["mobile.redirect-regex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
        }

        private static void BeginRequest(object sender, EventArgs e)
        {
            //Detect mobile support on begin request
            var context = ((HttpApplication)sender).Context;

            try
            {
                var mobileAddress = ConfigurationManager.AppSettings["mobile.redirect-url"];
                if (string.IsNullOrEmpty(mobileAddress)
                    || UrlCheckRegex != null && !UrlCheckRegex.IsMatch(context.Request.GetUrlRewriter().ToString())
                    || !IsRequestMatchesMobile(true)
                    || CookiesManager.IsMobileBlocked())
                    return;

                //TODO: check user status to display desktop or mobile version
                if (mobileAddress.StartsWith("~"))
                {
                    //Resolve to current
                    mobileAddress = VirtualPathUtility.ToAbsolute(mobileAddress);
                }

                const string tenantReplacePattern = "%tenant%";
                if (mobileAddress.Contains(tenantReplacePattern))
                {
                    var tennant = CoreContext.TenantManager.GetCurrentTenant();
                    mobileAddress = mobileAddress.Replace(tenantReplacePattern, tennant.TenantAlias);
                }

                var redirectUri = Uri.IsWellFormedUriString(mobileAddress, UriKind.Absolute)
                                      ? new Uri(mobileAddress)
                                      : new Uri(context.Request.GetUrlRewriter(), mobileAddress);

                if (redirectUri.Equals(context.Request.GetUrlRewriter()))
                    return;

                var builder = new UriBuilder(redirectUri);
                var abspath = context.Request.GetUrlRewriter().AbsolutePath;
                if (!string.IsNullOrEmpty(abspath) && abspath.EndsWith("default.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    abspath = abspath.Substring(0, abspath.Length - "default.aspx".Length);
                }
                builder.Path += abspath;
                builder.Query += context.Request.GetUrlRewriter().Query.TrimStart('?');
                redirectUri = builder.Uri;
                LogManager.GetLogger("ASC.Mobile.Redirect").DebugFormat("Redirecting url:'{1}' to mobile. UA={0}", context.Request.UserAgent, context.Request.GetUrlRewriter());
                context.Response.Redirect(redirectUri.ToString(), true);
            }
            catch (ThreadAbortException)
            {
                //Don't do nothing
            }
            catch (Exception ex)
            {
                //If error happens it's not so bad as you may think. We won't redirect user to mobile version.
                LogManager.GetLogger("ASC.Mobile.Redirect").Error("failed to redirect user to mobile", ex);
            }
        }

        public static bool IsMobile
        {
            get { return IsRequestMatchesMobile(); }
        }

        public static bool IsRequestMatchesMobile(bool forMobileVersion = false)
        {
            var userAgent = HttpContext.Current.Request.UserAgent;
            //Check user agent
            var regex = forMobileVersion
                            ? UserAgentRedirectRegex
                            : UserAgentMobileRegex;
            return !string.IsNullOrEmpty(userAgent) && regex != null && regex.IsMatch(userAgent);
        }
    }
}