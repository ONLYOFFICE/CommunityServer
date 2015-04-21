using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web.Dav
{
    internal class DavService : HttpService
    {
        #region Special WebDAV Verb Calls

        private const String  xmlPROPFIND =    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>" +
                                                "<d:propfind xmlns:d=\"DAV:\">" +
                                                    "<d:prop>"+                                    
                                                        "<d:ishidden/>"+
                                                        "<d:getlastmodified/>"+
                                                        "<d:getcontentlength/>"+
                                                        "<d:getcontenttype/>"+
                                                        "<d:resourcetype/>"+
                                                        "<d:getetag/>"+
                                                        "<d:lockdiscovery/>"+
                                                    "</d:prop>"+
                                                "</d:propfind>";

        public WebRequest CreateWebRequestPROPFIND(String url, ICredentials credentials)
        {
            // build a standard webrequest
            WebRequest request = CreateWebRequest(url, WebRequestMethodsEx.WebDAV.PropFind, credentials, false, null);

            // build data stream 
            MemoryStream contentStream = StreamHelper.ToStream(xmlPROPFIND);

            // write the content into request Stream
            using (Stream requestStream = GetRequestStream(request, contentStream.Length))
            {
                // copy the data
                StreamHelper.CopyStreamData(this, contentStream, requestStream, null, null);                
            }

            // return the stream
            return request;
        }

        public WebRequest CreateWebRequestPUT(String url, ICredentials credentials, Boolean bAllowStreamBuffering)
        {
            // create the request
            HttpWebRequest request = CreateWebRequest(url, WebRequestMethodsEx.Http.Put, credentials, bAllowStreamBuffering, null, null) as HttpWebRequest;

            // set the content type
            request.ContentType = "application/octet-stream";

            // go ahead
            return request;
        }
        
        public HttpStatusCode PerformMoveWebRequest(String url, String urlNewTarget, ICredentials credentials)
        {
            // build the service
            DavService svc = new DavService();

            // set the error code 
            int code;
            WebException e;

            // perform the request
            svc.PerformSimpleWebCall(url, WebRequestMethodsEx.WebDAV.Move, credentials, urlNewTarget, out code, out e);

            // return the code
            return (HttpStatusCode)code;
        }

        public HttpStatusCode PerformCopyWebRequest(String url, String urlNewTarget, ICredentials credentials)
        {
            // build the service
            DavService svc = new DavService();

            // set the error code 
            int code;
            WebException e;

            // perform the request
            svc.PerformSimpleWebCall(url, WebRequestMethodsEx.WebDAV.Copy, credentials, urlNewTarget, out code, out e);

            // return the code
            return (HttpStatusCode)code;
        }

        #endregion
        
        #region Overrides

        public override WebRequest CreateWebRequest(string url, String method, ICredentials credentials, Boolean bAllowStreamBuffering, object context)
        {
            // quote the string in the right way
            String quotedurl = HttpUtilityEx.GenerateEncodedUriString(new Uri(url));

            // create base request
            WebRequest request = base.CreateWebRequest(quotedurl, method, credentials, bAllowStreamBuffering, context, DavPreparation) as HttpWebRequest;
        
            // go ahead 
            return request;
        }

        private void DavPreparation(WebRequest request, Object context)
        {
            // cast to http webrequest
            HttpWebRequest hrequest = request as HttpWebRequest;

            // Set default headers            
            hrequest.Headers["Pragma"] = "no-cache";
            hrequest.Headers["Cache-Control"] = "no-cache";
            hrequest.ContentType = "text/xml";           
            hrequest.Accept = "*/*";

#if !WINDOWS_PHONE
            // for all meta data calls we allow stream buffering in webdav            
            if (hrequest.Method != WebRequestMethods.Http.Put)
                hrequest.AllowWriteStreamBuffering = true;
#endif

            // set translate mode
            hrequest.Headers["Translate"] = "f";
                       
            // Retrieve only the requested folder
            if (hrequest.Method == WebRequestMethodsEx.WebDAV.PropFind)
                hrequest.Headers["Depth"] = "1";

            // set the target in case of move operation
            else if (hrequest.Method == WebRequestMethodsEx.WebDAV.Move)
            {
                String targetUri = context as String;
                if (targetUri == null)
                    throw new Exception("Not a valid URL");

                targetUri = HttpUtilityEx.GenerateEncodedUriString(new Uri(targetUri));
                hrequest.Headers["Destination"] = targetUri;
            }
            else if (hrequest.Method == WebRequestMethodsEx.WebDAV.Copy)
            {
                String targetUri = context as String;
                if (targetUri == null)
                    throw new Exception("Not a valid URL");

                targetUri = HttpUtilityEx.GenerateEncodedUriString(new Uri(targetUri));
                hrequest.Headers["Destination"] = targetUri;
                hrequest.Headers["Depth"] = "infinity";
                hrequest.Headers["Overwrite"] = "T";
            }
        }

        #endregion
    }
}
