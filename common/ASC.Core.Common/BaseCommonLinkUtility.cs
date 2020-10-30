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
