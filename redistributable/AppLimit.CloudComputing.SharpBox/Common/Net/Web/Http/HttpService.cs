using System;
using System.Net;
using System.Threading;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http
{
    internal class HttpService : WebRequestService
    {
        protected override WebRequest CreateBasicWebRequest(Uri uri, bool bAllowStreamBuffering)
        {
            // build the http Webrequest
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.AllowAutoRedirect = false;

            request.AllowWriteStreamBuffering = bAllowStreamBuffering;
            request.Timeout = Timeout.Infinite;
            request.ReadWriteTimeout = Timeout.Infinite;

            // go ahead
            return request;
        }

        protected override int GetWebResponseStatus(WebResponse response)
        {
            if (!(response is HttpWebResponse))
                throw new InvalidOperationException("This is not a HTTP based web response");

            return (int)((response as HttpWebResponse).StatusCode);
        }
    }
}