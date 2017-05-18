using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace ASC.Web.Core.Client.Bundling
{
    class ClientScriptRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ClientScriptHandler();
        }


        class ClientScriptHandler : IHttpHandler
        {
            public bool IsReusable
            {
                get { return true; }
            }

            public void ProcessRequest(HttpContext context)
            {
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
                    var content = Encoding.UTF8.GetBytes(clientScriptReference.GetContent(context.Request.Url.AbsolutePath));
                    using (var inputStream = new MemoryStream())
                    {

                        if (ClientSettings.GZipEnabled)
                        {
                            using (var zip = new GZipStream(inputStream, CompressionMode.Compress, true))
                            {
                                zip.Write(content, 0, content.Length);
                                zip.Flush();
                            }
                            context.Response.Headers["Content-Encoding"] = "gzip";
                        }
                        else
                        {
                            inputStream.Write(content, 0, content.Length);
                        }
                        inputStream.Position = 0;
                        inputStream.CopyTo(context.Response.OutputStream);
                    }
                }
            }
        }
    }
}
