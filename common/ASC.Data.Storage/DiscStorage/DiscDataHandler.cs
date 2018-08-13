/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace ASC.Data.Storage.DiscStorage
{
    public class DiscDataHandler : IRouteHandler, IHttpHandler
    {
        private readonly string _physPath;
        private readonly bool _checkAuth;


        public bool IsReusable
        {
            get { return true; }
        }


        public DiscDataHandler(string physPath, bool checkAuth = true)
        {
            _physPath = physPath;
            _checkAuth = checkAuth;
        }

        protected internal static string CombinePath(string physPath, RequestContext requestContext)
        {
            var pathInfo = requestContext.RouteData.Values["pathInfo"].ToString().Replace('/', Path.DirectorySeparatorChar);

            var path = Path.Combine(physPath, pathInfo);

            var tenant = (requestContext.RouteData.Values["0"] ?? "").ToString();
            if (string.IsNullOrEmpty(tenant))
            {
                tenant = Path.Combine((requestContext.RouteData.Values["t1"] ?? "").ToString(),
                                      (requestContext.RouteData.Values["t2"] ?? "").ToString(),
                                      (requestContext.RouteData.Values["t3"] ?? "").ToString());
            }

            //todo: compare with current tenant id

            path = path.Replace("{0}", tenant);
            return path;
        }


        public static void RegisterVirtualPath(string virtPath, string physPath, bool publicRoute = false)
        {
            virtPath = virtPath.TrimStart('/');
            if (virtPath != string.Empty)
            {
                var url = virtPath + "{*pathInfo}";
                bool exists;
                using (var readLock = RouteTable.Routes.GetReadLock())
                {
                    exists = RouteTable.Routes.OfType<Route>().Any(r => r.Url == url);
                }
                if (!exists)
                {
                    using (var writeLock = RouteTable.Routes.GetWriteLock())
                    {
                        var route = publicRoute
                                        ? (IRouteHandler)new PublicDiscDataHandler(physPath)
                                        : new DiscDataHandler(physPath);

                        RouteTable.Routes.Add(new Route(url, route));

                        url = url.Replace("{0}", "{t1}/{t2}/{t3}");

                        RouteTable.Routes.Add(new Route(url, route));
                    }
                }
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var path = CombinePath(_physPath, requestContext);
            return new DiscDataHandler(path);
        }

        public void ProcessRequest(HttpContext context)
        {
            if (_checkAuth && !Core.SecurityContext.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (File.Exists(_physPath))
            {
                var lastwrite = File.GetLastWriteTime(_physPath);
                var etag = '"' + lastwrite.Ticks.ToString("X8", CultureInfo.InvariantCulture) + '"';

                var notmodified = context.Request.Headers["If-None-Match"] == etag ||
                                  context.Request.Headers["If-Modified-Since"] == lastwrite.ToString("R");

                if (notmodified)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
                else
                {
                    if (File.Exists(_physPath + ".gz"))
                    {
                        context.Response.WriteFile(_physPath + ".gz");
                        context.Response.Headers["Content-Encoding"] = "gzip";
                    }
                    else
                    {
                        context.Response.WriteFile(_physPath);
                    }

                    context.Response.ContentType = MimeMapping.GetMimeMapping(_physPath);
                    context.Response.Cache.SetVaryByCustom("*");
                    context.Response.Cache.SetAllowResponseInBrowserHistory(true);
                    context.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(1));
                    context.Response.Cache.SetLastModified(lastwrite);
                    context.Response.Cache.SetETag(etag);
                    context.Response.Cache.SetCacheability(HttpCacheability.Public);
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }

    public class PublicDiscDataHandler : IRouteHandler, IHttpHandler
    {
        private readonly string _physPath;


        public bool IsReusable
        {
            get { return true; }
        }


        public PublicDiscDataHandler(string physPath)
        {
            _physPath = physPath;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var path = DiscDataHandler.CombinePath(_physPath, requestContext);
            return new PublicDiscDataHandler(path);
        }

        public void ProcessRequest(HttpContext context)
        {
            new DiscDataHandler(_physPath, false).ProcessRequest(context);
        }
    }
}