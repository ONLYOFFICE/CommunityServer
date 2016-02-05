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
                var version = ClientScriptReference.GetContentHash(context.Request.Url.AbsolutePath);
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
                    context.Response.Write(CopyrigthTransform.CopyrigthText);
                    context.Response.Write(ClientScriptReference.GetContent(context.Request.Url.AbsolutePath));
                }
            }
        }
    }
}
