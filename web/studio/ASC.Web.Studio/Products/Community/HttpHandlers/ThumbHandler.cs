using ASC.Core;
using ASC.Web.UserControls.Bookmarking.Util;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ASC.Web.Community.HttpHandlers
{
    public class ThumbHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated || !ThumbnailHelper.HasService 
                || !context.Request.QueryString.AllKeys.Contains("url")) {

                ProccessFail(context);
                return;
            }

            var url = context.Request.QueryString["url"];
            url = url.Replace("&amp;", "&");
            url = System.Net.WebUtility.UrlEncode(url);

            try
            {
                using (var wc = new WebClient())
                {
                    var bytes = wc.DownloadData(string.Format(ThumbnailHelper.ServiceUrl, url));
                    context.Response.ContentType = wc.ResponseHeaders["Content-Type"] ?? "image/png";
                    context.Response.BinaryWrite(bytes);
                    context.Response.Flush();
                }
            }
            catch
            {
                ProccessFail(context);
            }
        }

        private void ProccessFail(HttpContext context)
        {
            context.Response.Redirect("~/products/community/modules/bookmarking/app_Themes/default/images/noimageavailable.jpg");
        }
    }
}
