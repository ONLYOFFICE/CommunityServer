using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http
{
    internal class HttpService : WebRequestService
    {
        protected override System.Net.WebRequest CreateBasicWebRequest(Uri uri, bool bAllowStreamBuffering)
        {
            // build the http Webrequest
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;            
            request.AllowAutoRedirect = false;

#if !WINDOWS_PHONE && !MONODROID
            request.AllowWriteStreamBuffering = bAllowStreamBuffering;
            request.Timeout = Timeout.Infinite;
            request.ReadWriteTimeout = Timeout.Infinite;
#endif

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
