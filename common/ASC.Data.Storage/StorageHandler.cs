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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Security.Cryptography;

namespace ASC.Data.Storage.DiscStorage
{
    public class StorageHandler : IRouteHandler, IHttpHandler
    {
        private readonly string _path;
        private readonly string _module;
        private readonly string _domain;
        private readonly bool _checkAuth;


        public bool IsReusable
        {
            get { return true; }
        }


        public StorageHandler(string path, string module, string domain, bool checkAuth = true)
        {
            _path = path;
            _module = module;
            _domain = domain;
            _checkAuth = checkAuth;
        }


        public static void RegisterVirtualPath(string module, string domain, bool publicRoute = false)
        {
            var virtPath = PathUtils.ResolveVirtualPath(module, domain);
            virtPath = virtPath.TrimStart('/');

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
                                    ? (IRouteHandler)new PublicStorageHandler(string.Empty, module, domain)
                                    : new StorageHandler(string.Empty, module, domain);

                    RouteTable.Routes.Add(new Route(url, route));

                    url = url.Replace("{0}", "{t1}/{t2}/{t3}");

                    RouteTable.Routes.Add(new Route(url, route));
                }
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var path = requestContext.RouteData.Values["pathInfo"].ToString().Replace('/', Path.DirectorySeparatorChar);
            return new StorageHandler(path, _module, _domain);
        }

        public void ProcessRequest(HttpContext context)
        {
            if (_checkAuth && !SecurityContext.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var storage = StorageFactory.GetStorage(CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(CultureInfo.InvariantCulture), _module);

            var path = _path;
            var header = context.Request[Constants.QUERY_HEADER] ?? "";

            var auth = context.Request[Constants.QUERY_AUTH];
            var storageExpire = storage.GetExpire(_domain);

            if (storageExpire != TimeSpan.Zero && storageExpire != TimeSpan.MinValue && storageExpire != TimeSpan.MaxValue || !string.IsNullOrEmpty(auth))
            {
                var expire = context.Request[Constants.QUERY_EXPIRE];
                if (string.IsNullOrEmpty(expire)) expire = storageExpire.TotalMinutes.ToString(CultureInfo.InvariantCulture);

                var validateResult = EmailValidationKeyProvider.ValidateEmailKey(path + "." + header + "." + expire, auth ?? "", TimeSpan.FromMinutes(Convert.ToDouble(expire)));
                if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }

            if (!storage.IsFile(_domain, path))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var headers = header.Length > 0 ? header.Split('&').Select(HttpUtility.UrlDecode) : new string[] { };

            const int bigSize = 5 * 1024 * 1024;
            if (storage.IsSupportInternalUri && bigSize < storage.GetFileSize(_domain, path))
            {
                var uri = storage.GetInternalUri(_domain, path, TimeSpan.FromMinutes(15), headers);

                context.Response.Cache.SetAllowResponseInBrowserHistory(false);
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                context.Response.Redirect(uri.ToString());
                return;
            }

            string encoding = null;
            if (storage is DiscDataStore && storage.IsFile(_domain, path + ".gz"))
            {
                path += ".gz";
                encoding = "gzip";
            }

            var headersToCopy = new List<string> { "Content-Disposition", "Cache-Control", "Content-Encoding", "Content-Language", "Content-Type", "Expires" };
            foreach (var h in headers)
            {
                var toCopy = headersToCopy.Find(x => h.StartsWith(x));
                if (string.IsNullOrEmpty(toCopy)) continue;
                context.Response.Headers[toCopy] = h.Substring(toCopy.Length + 1);
            }

            context.Response.ContentType = MimeMapping.GetMimeMapping(path);
            if (encoding != null)
                context.Response.Headers["Content-Encoding"] = encoding;

            using (var stream = storage.GetReadStream(_domain, path))
            {
                context.Response.Buffer = false;

                long offset = 0;
                long length = stream.Length;
                if (stream.CanSeek)
                {
                    length = ProcessRangeHeader(context, stream.Length, ref offset);
                    stream.Seek(offset, SeekOrigin.Begin);
                }

                context.Response.AddHeader("Connection", "Keep-Alive");
                context.Response.AddHeader("Content-Length", length.ToString(CultureInfo.InvariantCulture));

                const int bufferSize = 32 * 1024; // 32KB
                var buffer = new byte[bufferSize];
                var toRead = length;

                while (toRead > 0)
                {
                    int read;

                    try
                    {
                        read = stream.Read(buffer, 0, bufferSize);

                        if (!context.Response.IsClientConnected) throw new Exception("StorageHandler: ProcessRequest failed: client disconnected");

                        context.Response.OutputStream.Write(buffer, 0, read);
                        context.Response.Flush();
                        toRead -= read;
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").Error("storage", e);

                        throw;
                    }
                }
            }
        }

        // \web\studio\ASC.Web.Studio\Products\Files\HttpHandlers\FileHandler.ashx.cs ProcessRangeHeader()
        private static long ProcessRangeHeader(HttpContext context, long fullLength, ref long offset)
        {
            if (context == null) throw new ArgumentNullException();
            if (context.Request.Headers["Range"] == null) return fullLength;

            long endOffset = -1;

            var range = context.Request.Headers["Range"].Split(new[] { '=', '-' });
            offset = Convert.ToInt64(range[1]);
            if (range.Count() > 2 && !string.IsNullOrEmpty(range[2]))
            {
                endOffset = Convert.ToInt64(range[2]);
            }
            if (endOffset < 0 || endOffset >= fullLength)
            {
                endOffset = fullLength - 1;
            }

            var length = endOffset - offset + 1;

            if (length <= 0) throw new HttpException("Wrong Range header");

            if (length < fullLength)
            {
                context.Response.StatusCode = (int)HttpStatusCode.PartialContent;
            }
            context.Response.AddHeader("Accept-Ranges", "bytes");
            context.Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", offset, endOffset, fullLength));

            return length;
        }
    }

    public class PublicStorageHandler : IRouteHandler, IHttpHandler
    {
        private readonly string _path;
        private readonly string _module;
        private readonly string _domain;


        public bool IsReusable
        {
            get { return true; }
        }


        public PublicStorageHandler(string path, string module, string domain)
        {
            _path = path;
            _module = module;
            _domain = domain;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var path = requestContext.RouteData.Values["pathInfo"].ToString().Replace('/', Path.DirectorySeparatorChar);
            return new PublicStorageHandler(path, _module, _domain);
        }

        public void ProcessRequest(HttpContext context)
        {
            new StorageHandler(_path, _module, _domain, false).ProcessRequest(context);
        }
    }
}