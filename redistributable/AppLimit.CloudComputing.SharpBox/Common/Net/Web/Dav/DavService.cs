using System;
using System.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web.Dav
{
    internal class DavService : HttpService
    {
        #region Special WebDAV Verb Calls

        private const string xmlPROPFIND = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>" +
                                                "<d:propfind xmlns:d=\"DAV:\">" +
                                                    "<d:prop>" +
                                                        "<d:ishidden/>" +
                                                        "<d:getlastmodified/>" +
                                                        "<d:getcontentlength/>" +
                                                        "<d:getcontenttype/>" +
                                                        "<d:resourcetype/>" +
                                                        "<d:getetag/>" +
                                                        "<d:lockdiscovery/>" +
                                                    "</d:prop>" +
                                                "</d:propfind>";

        public WebRequest CreateWebRequestPROPFIND(string url, ICredentials credentials)
        {
            // build a standard webrequest
            var request = CreateWebRequest(url, WebRequestMethodsEx.WebDAV.PropFind, credentials, false, null);

            // build data stream 
            var contentStream = StreamHelper.ToStream(xmlPROPFIND);

            // write the content into request Stream
            using (Stream requestStream = GetRequestStream(request, contentStream.Length))
            {
                // copy the data
                StreamHelper.CopyStreamData(this, contentStream, requestStream, null, null);
            }

            // return the stream
            return request;
        }

        public WebRequest CreateWebRequestPUT(string url, ICredentials credentials, bool bAllowStreamBuffering)
        {
            // create the request
            var request = CreateWebRequest(url, WebRequestMethodsEx.Http.Put, credentials, bAllowStreamBuffering, null, null) as HttpWebRequest;

            // set the content type
            request.ContentType = "application/octet-stream";

            // bug 53196
            request.ServicePoint.Expect100Continue = true;
#if NET6_0_OR_GREATER
            request.Headers.Add("Expect", "100-continue");
#endif
            // go ahead
            return request;
        }

        public HttpStatusCode PerformMoveWebRequest(string url, string urlNewTarget, ICredentials credentials)
        {
            // build the service
            var svc = new DavService();

            // set the error code 
            int code;
            WebException e;

            // perform the request
            svc.PerformSimpleWebCall(url, WebRequestMethodsEx.WebDAV.Move, credentials, urlNewTarget, out code, out e);

            // return the code
            return (HttpStatusCode)code;
        }

        public HttpStatusCode PerformCopyWebRequest(string url, string urlNewTarget, ICredentials credentials)
        {
            // build the service
            var svc = new DavService();

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

        public override WebRequest CreateWebRequest(string url, string method, ICredentials credentials, bool bAllowStreamBuffering, object context)
        {
            // quote the string in the right way
            var quotedurl = HttpUtilityEx.GenerateEncodedUriString(new Uri(url));

            // create base request
            WebRequest request = CreateWebRequest(quotedurl, method, credentials, bAllowStreamBuffering, context, DavPreparation) as HttpWebRequest;

            // go ahead 
            return request;
        }

        private static void DavPreparation(WebRequest request, object context)
        {
            // cast to http webrequest
            var hrequest = request as HttpWebRequest;

            // Set default headers            
            hrequest.Headers["Pragma"] = "no-cache";
            hrequest.Headers["Cache-Control"] = "no-cache";
            hrequest.ContentType = "text/xml";
            hrequest.Accept = "*/*";

            // for all meta data calls we allow stream buffering in webdav            
            if (hrequest.Method != WebRequestMethods.Http.Put)
                hrequest.AllowWriteStreamBuffering = true;

            // set translate mode
            hrequest.Headers["Translate"] = "f";

            // Retrieve only the requested folder
            switch (hrequest.Method)
            {
                case WebRequestMethodsEx.WebDAV.PropFind:
                    hrequest.Headers["Depth"] = "1";
                    break;
                case WebRequestMethodsEx.WebDAV.Move:
                    {
                        var targetUri = context as string;
                        if (targetUri == null)
                            throw new Exception("Not a valid URL");

                        targetUri = HttpUtilityEx.GenerateEncodedUriString(new Uri(targetUri));
                        targetUri = targetUri.Replace(WebDavConfiguration.YaUrl + ":443", WebDavConfiguration.YaUrl);
                        hrequest.Headers["Destination"] = targetUri;
                    }
                    break;
                case WebRequestMethodsEx.WebDAV.Copy:
                    {
                        var targetUri = context as string;
                        if (targetUri == null)
                            throw new Exception("Not a valid URL");

                        targetUri = HttpUtilityEx.GenerateEncodedUriString(new Uri(targetUri));
                        targetUri = targetUri.Replace(WebDavConfiguration.YaUrl + ":443", WebDavConfiguration.YaUrl);
                        hrequest.Headers["Destination"] = targetUri;
                        hrequest.Headers["Depth"] = "infinity";
                        hrequest.Headers["Overwrite"] = "T";
                    }
                    break;
            }
        }

#endregion
    }
}