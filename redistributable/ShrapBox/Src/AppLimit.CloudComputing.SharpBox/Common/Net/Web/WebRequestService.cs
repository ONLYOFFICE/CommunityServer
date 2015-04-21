using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using AppLimit.CloudComputing.SharpBox.Common.IO;

#if WINDOWS_PHONE
using AppLimit.CloudComputing.oAuth.WP7.SilverLightHelper;
#endif

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal abstract class WebRequestService
    {
        private const string watchTag = "WebRequestServiceRequestWatch";

        #region StopWatch Helper

        private static void StopStopWatch(WebResponse response, Stream ret, WebException exception)
        {
#if !WINDOWS_PHONE
            // 2. stop the watch from tls
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(watchTag);
            Stopwatch watch = Thread.GetData(slot) as Stopwatch;
#else
            Stopwatch watch = null;
#endif

            if (watch != null)
            {
                // stop watch
                watch.Stop();

                // notify
                WebRequestManager.Instance.NotifyWebRequestExecuted(response, watch.Elapsed, ret, exception);
            }
        }

        #endregion

        #region Base Web Methods

        /// <summary>
        /// This method generates a new webrequest and instruments the stopwatch
        /// counter
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="credentials"></param>
        /// <param name="bAllowStreamBuffering"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual WebRequest CreateWebRequest(String url, String method, ICredentials credentials, Boolean bAllowStreamBuffering, Object context)
        {
            return CreateWebRequest(url, method, credentials, bAllowStreamBuffering, context, null);
        }

        /// <summary>
        /// The callback 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        public delegate void CreateWebRequestPreparationCallback(WebRequest request, Object context);      

        /// <summary>
        /// Internal web request generator
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="credentials"></param>
        /// <param name="bAllowStreamBuffering"></param>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected WebRequest CreateWebRequest(String url, String method, ICredentials credentials, Boolean bAllowStreamBuffering, Object context, CreateWebRequestPreparationCallback callback)
        {
#if !WINDOWS_PHONE
            // 1. create and start the watch
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif
            
            // 2. build uri
            Uri uriNew = new Uri(url);

            // 2. create request 
            WebRequest request = CreateBasicWebRequest(uriNew, bAllowStreamBuffering);
            request.Method = method.ToString().ToUpper();                        
            
            // 3. call back
            if (callback != null)
                callback(request, context);

#if !WINDOWS_PHONE
            // 4. set the proxy class if needed
            if (WebRequestManager.Instance.GetProxySettings() != null)
                request.Proxy = WebRequestManager.Instance.GetProxySettings();

            // 4.1. check if we got a null proxy
            if (request.Proxy != null && request.Proxy is WebRequestManagerNullProxy)
                request.Proxy = null;
#endif
            // 5. set the credentials if needed
            if (credentials != null)
            {
                request.Credentials = credentials;

#if !WINDOWS_PHONE                
                try
                {
                    request.PreAuthenticate = true;
                }
                catch (NotSupportedException)
                { }
#endif
            }

            // 6. notify
            WebRequestManager.Instance.NotifyWebRequestPrepared(request);

#if !WINDOWS_PHONE
            // 7. add watch object to tls
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(watchTag);
            Thread.SetData(slot, watch);
#endif

            // 8. return the request
            return request;
        }

        /// <summary>
        /// Please implement this method for the right WebRequest type
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="bAllowStreamBuffering"></param>
        /// <returns></returns>
        protected abstract WebRequest CreateBasicWebRequest(Uri uri, Boolean bAllowStreamBuffering);        

        /// <summary>
        /// This method returns a webrequest data stream and should be used in 
        /// a using clause
        /// </summary>
        /// <param name="request"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual WebRequestStream GetRequestStream(WebRequest request, long length)
        {
#if !WINDOWS_PHONE
            // set the conten length
            request.ContentLength = length;
#endif
            
            // get the network stream
            Stream nwStream = WebRequestStreamHelper.GetRequestStream(request);

            // return the request streaM;
            WebRequestStream ws =  new WebRequestStream(nwStream, request, this);

            // add pre dispose opp
            ws.PushPreDisposeOperation(DisposeWebRequestStreams, request, ws);

            // go ahead
            return ws;
        }

        /// <summary>
        /// This method contains the routine which has to be executed when a request 
        /// stream will be disposed and will be called autoamtically
        /// </summary>
        private void DisposeWebRequestStreams(params object[] arg)
        {
            // get the params
            WebRequest request = arg[0] as WebRequest;
            WebRequestStream stream = arg[1] as WebRequestStream;

            // check if we have a multipart upload
            WebRequestMultipartFormDataSupport md = new WebRequestMultipartFormDataSupport();
            if (md.IsRequestMultiPartUpload(request))
                md.FinalizeNetworkFileDataStream(stream);

#if WINDOWS_PHONE 
            // Close Stream Bugfix for WP7
            stream.Close();
#endif
        }

        /// <summary>
        /// This method returns a webresponse or throws an exception. In the case of an 
        /// exception the stop watch is stop here
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual WebResponse GetWebResponse(WebRequest request)
        {            
            // execute the webrequest
            try
            {
#if !WINDOWS_PHONE
                // check the length
                if (request.ContentLength == -1)
                {
                    request.ContentLength = 0;
                    request.ContentType = "";
                }
#endif

                // Notify
                WebRequestManager.Instance.NotifyWebRequestExecuting(request);

                // get the response
                return WebRequestStreamHelper.GetResponse(request);                
            }
            catch (WebException e)
            {
                // stop the watch
                StopStopWatch(null, null, e);

                // rethrow the exception
                throw e;                
            }            
        }

        /// <summary>
        /// This method returns an response stream 
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Stream GetResponseStream(WebResponse response)
        {
            Stream responseStream = null;

            try
            {
                // get the network stream
                Stream nwStream = WebRequestStreamHelper.GetResponseStream(response);

                // get the response stream            
                responseStream = new WebResponseStream(nwStream, response, this);
            }
            catch (WebException)
            {
                return null;
            }
            finally
            {
                StopStopWatch(response, responseStream, null);
            }

            // return the stream
            return responseStream;
        }

        /// <summary>
        /// This method executes alle dispose code for webresponse streams
        /// </summary>
        /// <param name="response"></param>
        /// <param name="stream"></param>
        public virtual void DisposeWebResponseStreams(WebResponse response, WebResponseStream stream)
        {            
        }

        /// <summary>
        /// Please override this method to ensure that the system can evaluate the 
        /// protocol specific error code
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected abstract int GetWebResponseStatus(WebResponse response);        

        #endregion

        #region Comfort Functions

        /// <summary>
        /// Performs a five webrequest and returns the result as an in memory stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>         
        public MemoryStream PerformWebRequest(WebRequest request, Object context)
        {
            int code;
            WebException e;
            return PerformWebRequest(request, context, out code, out e);
        }

        /// <summary>
        /// Performs a five webrequest and returns the result as an in memory stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ProtocolSpecificStatusCode"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public MemoryStream PerformWebRequest(WebRequest request, Object context, out int ProtocolSpecificStatusCode, out WebException errorInfo)
        {
            return PerformWebRequest(request, context, out ProtocolSpecificStatusCode, out errorInfo, null);
        }

        /// <summary>
        /// Performs a five webrequest and returns the result as an in memory stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ProtocolSpecificStatusCode"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public MemoryStream PerformWebRequest(WebRequest request, Object context, out int ProtocolSpecificStatusCode, out WebException errorInfo, Func<int,bool> httpCodeCondition)
        {
            WebHeaderCollection headers;
            return PerformWebRequest(request, context, null, out ProtocolSpecificStatusCode, out errorInfo, out headers,httpCodeCondition);
        }

        /// <summary>
        /// Performs a five webrequest and returns the result as an in memory stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="ProtocolSpecificStatusCode"></param>
        /// <param name="errorInfo"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public MemoryStream PerformWebRequest(WebRequest request, Object context, Stream content, out int ProtocolSpecificStatusCode, out WebException errorInfo, out WebHeaderCollection headers)
        {
            return PerformWebRequest(request, context, content, out ProtocolSpecificStatusCode, out errorInfo, out headers, null);
        }

        /// <summary>
        /// Performs a five webrequest and returns the result as an in memory stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="ProtocolSpecificStatusCode"></param>
        /// <param name="errorInfo"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public MemoryStream PerformWebRequest(WebRequest request, Object context, Stream content, out int ProtocolSpecificStatusCode, out WebException errorInfo, out WebHeaderCollection headers, Func<int, bool> httpCodeCondition)
        {
            // no error
            errorInfo = null;
            headers = null;

            // start
            try
            {
                // add content if needed
                if (content != null)
                {
                    using (Stream requestStream = GetRequestStream(request, content.Length))
                    {
                        StreamHelper.CopyStreamData(this, content, requestStream, null, null);
                    }
                }

                // create the memstream
                MemoryStream memStream = new MemoryStream();

                // get the response
                using (WebResponse response = GetWebResponse(request))
                {
                    
                    // set the error code
                    ProtocolSpecificStatusCode = GetWebResponseStatus(response);

                    // handle move
                    if (ProtocolSpecificStatusCode == (int)HttpStatusCode.Moved)
                    {
                        // set the headers
                        headers = response.Headers;                                                
                    }
                    else if (httpCodeCondition!=null && httpCodeCondition(ProtocolSpecificStatusCode))
                    {
                        //Just return. Do nothing
                    }
                    else
                    {
                        // read the data 
                        using (Stream responseStream = GetResponseStream(response))
                        {
                            // copy the data into memory                        
                            StreamHelper.CopyStreamData(this, responseStream, memStream, null, null);

                            // reset the memory stream
                            memStream.Position = 0;

                            // close the source stream
                            responseStream.Close();
                        }                        
                    }

                    // close the response
                    response.Close();
                }                

                // return the byte stream
                return memStream;
            }
            catch (WebException e)
            {
                if (e.Response == null)
                    ProtocolSpecificStatusCode = (int)HttpStatusCode.BadRequest;
                else
                    ProtocolSpecificStatusCode = GetWebResponseStatus(e.Response);

                errorInfo = e;
                return null;
            }
        }        
              
        /// <summary>
        /// Forms a webrequest and performs them. The result will generated as memory stream
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="credentials"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public MemoryStream PerformSimpleWebCall(String url, String method, ICredentials credentials, Object context)
        {
            int code;
            WebException e;
            return PerformSimpleWebCall(url, method, credentials, context, out code, out e);    
        }

        /// <summary>
        /// Forms a webrequest and performs them. The result will generated as memory stream 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="credentials"></param>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public MemoryStream PerformSimpleWebCall(String url, String method, ICredentials credentials, Object context, out int code, out WebException errorInfo)
        {
            return PerformSimpleWebCall(url, method, credentials, null, context, out code, out errorInfo);
        }
        
        /// <summary>
        /// Forms a webrequest and performs them. The result will generated as memory stream 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="credentials"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public MemoryStream PerformSimpleWebCall(String url, String method, ICredentials credentials, Stream content, Object context, out int code, out WebException errorInfo)        
        {
            // create a web request
            WebRequest request = CreateWebRequest(url, method, credentials, false, context);
            if (request == null)
                throw new Exception("Could not generate WebRequest for " + method + ":" + url);

            WebHeaderCollection headers;
            MemoryStream s = PerformWebRequest(request, context, content, out code, out errorInfo, out headers);

            if (code == (int)HttpStatusCode.Moved && request is HttpWebRequest)
            {
                // get new location
                String newLoc = headers["Location"];

                // do it again
                return PerformSimpleWebCall(newLoc, method, credentials, content, context, out code, out errorInfo);
            }

            // go ahead
            return s;
        }
        
        #endregion

        #region Multi Part Form Data Support

        /// <summary>
        /// This method implements a standard multipart/form-data upload and can be overriden 
        /// e.g. for WebDav upload
        /// </summary>
        /// <param name="url"></param>        
        /// <param name="credentials"></param>
        /// <returns></returns>
        public virtual WebRequest CreateWebRequestMultiPartUpload(String url, ICredentials credentials)
        {
            // 1. build a valid webrequest
            WebRequest request = CreateWebRequest(url, WebRequestMethodsEx.Http.Post, credentials, false, null);

            // 2. set the request paramter
            WebRequestMultipartFormDataSupport mp = new WebRequestMultipartFormDataSupport();
            mp.PrepareWebRequest(request);
            
            // 3. go ahead
            return request;
        }

        public virtual WebRequestStream GetRequestStreamMultiPartUpload(WebRequest request, String fileName, long fileSize)
        {
            // generate mp support
            WebRequestMultipartFormDataSupport mp = new WebRequestMultipartFormDataSupport();

            // set the right size
            fileSize = fileSize + mp.GetHeaderFooterSize(fileName);

            // set the streaming buffering
#if !WINDOWS_PHONE
            if (fileSize > 0)
            {                
                // set the maximum content length size
                request.ContentLength = fileSize;
            }
#endif
            // get the stream
            WebRequestStream stream = GetRequestStream(request, fileSize);

            // prepare the stream            
            mp.PrepareRequestStream(stream, fileName);

            // go ahead
            return stream;
        }

        #endregion                                     
    }
}
