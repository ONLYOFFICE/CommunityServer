using System;
using System.IO;
using System.Net;
using System.Threading;

#if WINDOWS_PHONE
using System.Windows.Threading;
using System.Windows.Controls;
#endif

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal class WebRequestStreamHelper
    {        
        private static Stream GetRequestStreamInternal(WebRequest request)
        {
            // build the envent
			using (var revent = new ManualResetEvent(false))
			{
				// start the async call
				IAsyncResult result = request.BeginGetRequestStream(CallStreamCallback, revent);

				// wait for event
				revent.WaitOne();

				// return data stream
				return request.EndGetRequestStream(result);
			}
        }        
        
        private static WebResponse GetResponseInternal(WebRequest request)
        {            
            // build the envent
			using (var revent = new ManualResetEvent(false))
			{
				// start the async call
				IAsyncResult result = request.BeginGetResponse(new AsyncCallback(CallStreamCallback), revent);

				// wait for event
				revent.WaitOne();

				// get the response
				WebResponse response = request.EndGetResponse(result);

				// go ahead
				return response;
			}
        }

        private static void CallStreamCallback(IAsyncResult asynchronousResult)
        {
            var revent = asynchronousResult.AsyncState as ManualResetEvent;
			if (revent != null)
				revent.Set();
        }

        public static Stream GetRequestStream(WebRequest request)
        {
#if WINDOWS_PHONE            
            return WebRequestStreamHelper.GetRequestStreamInternal(request);
#else
            return request.GetRequestStream();
#endif
        }

        public static WebResponse GetResponse(WebRequest request)
        {
#if WINDOWS_PHONE            
            return WebRequestStreamHelper.GetResponseInternal(request);            
#else
            return request.GetResponse();
#endif
        }

        public static Stream GetResponseStream(WebResponse response)
        {
             return response.GetResponseStream();
        }
    }
}
