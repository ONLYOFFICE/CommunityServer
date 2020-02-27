/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Web;
using ASC.Common.Logging;

namespace ASC.Core.Common
{
    public static class BaseCommonLinkUtility
    {
        private const string LOCALHOST = "localhost";

        private static UriBuilder _serverRoot;
        private static string _vpath;

        static BaseCommonLinkUtility()
        {
            try
            {
                var uriBuilder = new UriBuilder(Uri.UriSchemeHttp, LOCALHOST);
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var u = HttpContext.Current.Request.GetUrlRewriter();
                    uriBuilder = new UriBuilder(u.Scheme, LOCALHOST, u.Port);
                }
                _serverRoot = uriBuilder;
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
        }

        public static void Initialize(string serverUri)
        {
            if (string.IsNullOrEmpty(serverUri))
            {
                throw new ArgumentNullException("serverUri");
            }

            var uri = new Uri(serverUri.Replace('*', 'x').Replace('+', 'x'));
            _serverRoot = new UriBuilder(uri.Scheme, LOCALHOST, uri.Port);
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
                var result = new UriBuilder(_serverRoot.Uri);

                // first, take from current request
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var u = HttpContext.Current.Request.GetUrlRewriter();
                    result = new UriBuilder(u.Scheme, u.Host, u.Port);

                    if (CoreContext.Configuration.Standalone && !result.Uri.IsLoopback)
                    {
                        // save for stanalone
                        _serverRoot.Host = result.Host;
                    }
                }

                if (result.Uri.IsLoopback)
                {
                    // take values from db if localhost or no http context thread
                    var tenant = CoreContext.TenantManager.GetCurrentTenant();
                    result.Host = tenant.TenantDomain;

#if DEBUG
                    // for Visual Studio debug
                    if (tenant.TenantAlias == LOCALHOST)
                    {
                        result.Host = LOCALHOST;
                    }
#endif

                    if (!string.IsNullOrEmpty(tenant.MappedDomain))
                    {
                        var mapped = tenant.MappedDomain.ToLowerInvariant();
                        if (!mapped.Contains(Uri.SchemeDelimiter))
                        {
                            mapped = Uri.UriSchemeHttp + Uri.SchemeDelimiter + mapped;
                        }
                        result = new UriBuilder(mapped);
                    }
                }

                return result.Uri.ToString().TrimEnd('/');
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
    }
}
