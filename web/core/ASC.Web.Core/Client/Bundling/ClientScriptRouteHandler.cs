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
