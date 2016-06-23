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


using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace ASC.Data.Storage.DiscStorage
{
    class DiscDataHandler : IRouteHandler, IHttpHandler
    {
        private readonly string physPath;


        public bool IsReusable
        {
            get { return true; }
        }


        public DiscDataHandler(string physPath)
        {
            this.physPath = physPath;
        }


        public static void RegisterVirtualPath(string virtPath, string physPath)
        {
            var pos = virtPath.IndexOf('{');
            if (0 <= pos)
            {
                virtPath = virtPath.Substring(0, pos);
            }

            pos = physPath.IndexOf('{');
            if (0 <= pos)
            {
                physPath = physPath.Substring(0, pos);
            }

            virtPath = virtPath.TrimStart('/');
            if (virtPath != string.Empty)
            {
                var url = virtPath + "{*pathInfo}";
                var exists = false;
                using (var readLock = RouteTable.Routes.GetReadLock())
                {
                    exists = RouteTable.Routes.OfType<Route>().Any(r => r.Url == url);
                }
                if (!exists)
                {
                    using (var writeLock = RouteTable.Routes.GetWriteLock())
                    {
                        RouteTable.Routes.Add(new Route(url, new DiscDataHandler(physPath)));
                    }
                }
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var vpath = requestContext.HttpContext.Request.CurrentExecutionFilePath;
            var path = Path.Combine(physPath, requestContext.RouteData.Values["pathInfo"].ToString().Replace('/', Path.DirectorySeparatorChar));
            return new DiscDataHandler(path);
        }

        public void ProcessRequest(HttpContext context)
        {
            if (File.Exists(physPath))
            {
                var lastwrite = File.GetLastWriteTime(physPath);
                var etag = '"' + lastwrite.Ticks.ToString("X8", CultureInfo.InvariantCulture) + '"';

                var notmodified = context.Request.Headers["If-None-Match"] == etag ||
                    context.Request.Headers["If-Modified-Since"] == lastwrite.ToString("R");

                if (notmodified)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
                else
                {
                    context.Response.WriteFile(physPath);
                    context.Response.ContentType = MimeMapping.GetMimeMapping(physPath);
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
}
