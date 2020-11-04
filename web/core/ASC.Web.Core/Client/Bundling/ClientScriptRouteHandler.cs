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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using ASC.Common.Logging;

namespace ASC.Web.Core.Client.Bundling
{
    class ClientScriptRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ClientScriptHandler();
        }


        class ClientScriptHandler : HttpTaskAsyncHandler
        {
            public override async Task ProcessRequestAsync(HttpContext context)
            {
                try
                {
                    HttpContext.Current = context;
                    var clientScriptReference = new ClientScriptReference();
                    var version = clientScriptReference.GetContentHash(context.Request.Url.AbsolutePath);
                    if (string.Equals(context.Request.Headers["If-None-Match"], version))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    }
                    else
                    {
                        context.Response.Charset = Encoding.UTF8.WebName;
                        context.Response.ContentType = new ContentType("application/x-javascript") { CharSet = Encoding.UTF8.WebName }.ToString();

                        // cache
                        context.Response.Cache.SetVaryByCustom("*");
                        context.Response.Cache.SetAllowResponseInBrowserHistory(true);
                        context.Response.Cache.SetETag(version);
                        context.Response.Cache.SetCacheability(HttpCacheability.Public);

                        // body
                        var content = Encoding.UTF8.GetBytes(await clientScriptReference.GetContentAsync(context));

                        using (var inputStream = new MemoryStream())
                        {
                            if (ClientSettings.GZipEnabled)
                            {
                                using (var zip = new GZipStream(inputStream, CompressionMode.Compress, true))
                                {
                                    await zip.WriteAsync(content, 0, content.Length);
                                    zip.Flush();
                                }
                                context.Response.Headers["Content-Encoding"] = "gzip";
                            }
                            else
                            {
                                await inputStream.WriteAsync(content, 0, content.Length);
                            }
                            inputStream.Position = 0;
                            inputStream.CopyTo(context.Response.OutputStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("ClientScriptHandler", e);
                }
            }
        }
    }
}
