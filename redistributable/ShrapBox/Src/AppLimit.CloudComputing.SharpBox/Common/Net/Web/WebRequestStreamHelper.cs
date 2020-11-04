using System.IO;
using System.Net;


namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal class WebRequestStreamHelper
    {
        public static Stream GetRequestStream(WebRequest request)
        {
            return request.GetRequestStream();
        }

        public static WebResponse GetResponse(WebRequest request)
        {
            return request.GetResponse();
        }

        public static Stream GetResponseStream(WebResponse response)
        {
            return response.GetResponseStream();
        }
    }
}