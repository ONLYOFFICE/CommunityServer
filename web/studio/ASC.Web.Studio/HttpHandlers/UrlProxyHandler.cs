/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;

namespace ASC.Web.Studio.HttpHandlers
{
    public class UrlProxyHandler : IHttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            try
            {
                if (!SecurityContext.IsAuthenticated)
                    throw new HttpException(403, "Access denied.");

                var url = context.Request.QueryString["url"];
                if (string.IsNullOrEmpty(url))
                    throw new HttpException(400, "Bad request.");

                var uri = Encoding.UTF8.GetString(Convert.FromBase64String(url));

                /* Create an HttpWebRequest to send the URI on and process results. */
                var proxyRequest = (HttpWebRequest) WebRequest.Create(uri);

                /* Set the appropriate values to the request methods. */
                proxyRequest.Method = request.HttpMethod;
                proxyRequest.ServicePoint.Expect100Continue = false;
                proxyRequest.Referer = request.Headers["referer"];

                /* Set the body of ProxyRequest for POST requests to the proxy. */
                if (request.InputStream.Length > 0)
                {
                    /* 
                     * Since we are using the same request method as the original request, and that is 
                     * a POST, the values to send on in the new request must be grabbed from the 
                     * original POSTed request.
                     */
                    var bytes = new byte[request.InputStream.Length];

                    request.InputStream.Read(bytes, 0, (int) request.InputStream.Length);

                    proxyRequest.ContentLength = bytes.Length;

                    var contentType = request.ContentType;

                    proxyRequest.ContentType = string.IsNullOrEmpty(contentType)
                        ? "application/x-www-form-urlencoded"
                        : contentType;

                    using (var outputStream = proxyRequest.GetRequestStream())
                    {
                        outputStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    /*
                     * When the original request is a GET, things are much easier, as we need only to 
                     * pass the URI we collected earlier which will still have any parameters 
                     * associated with the request attached to it.
                     */
                    proxyRequest.Method = "GET";
                }

                var serverResponse = proxyRequest.GetResponse();

                /* Set up the response to the client if there is one to set up. */
                response.ContentType = serverResponse.ContentType;
                using (var byteStream = serverResponse.GetResponseStream())
                {
                    if (byteStream == null)
                        throw new Exception("Empty serverResponse.GetResponseStream()");

                    /* What is the response type? */
                    if (serverResponse.ContentType.Contains("text") ||
                        serverResponse.ContentType.Contains("json") ||
                        serverResponse.ContentType.Contains("xml"))
                    {
                        /* These "text" types are easy to handle. */
                        using (var reader = new StreamReader(byteStream))
                        {
                            var responseString = reader.ReadToEnd();

                            /* 
                             * Tell the client not to cache the response since it 
                             * could easily be dynamic, and we do not want to mess
                             * that up!
                             */
                            response.CacheControl = "no-cache";
                            response.Write(responseString);
                        }
                    }
                    else
                    {
                        byte[] binaryOutputs;

                        /* 
                         * Handle binary responses (image, layer file, other binary 
                         * files) differently than text.
                         */
                        using (var binReader = new BinaryReader(byteStream))
                        {
                            binaryOutputs = binReader.ReadBytes((int) serverResponse.ContentLength);
                            binReader.Close();
                        }

                        /* 
                         * Tell the client not to cache the response since it could 
                         * easily be dynamic, and we do not want to mess that up!
                         */
                        //response.CacheControl = "no-cache";

                        // Send the binary response to the client.
                        // TODO: if large images/files are sent, we could modify this to send back in chunks instead.
                        response.OutputStream.Write(binaryOutputs, 0, binaryOutputs.Length);
                    }
                    serverResponse.Close();
                }

            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    var r = we.Response as HttpWebResponse;
                    if (r != null)
                    {
                        response.StatusCode = (int)r.StatusCode;
                        response.Write(we.Message);
                    }
                    else
                    {
                        response.StatusCode = 500;
                    }
                }
                else
                {
                    response.StatusCode = 500;
                }

                response.StatusDescription = we.Status.ToString();
                response.Write(we.Response);
                response.End();
            }
            catch (HttpException he)
            {
                response.StatusCode = he.GetHttpCode();
                response.Write(he.Message);
                response.End();
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Url: {0} {1}", context.Request.Url, e);
                response.StatusCode = 500;
                response.Write(e.Message);
                response.End();
            }
        }
    }
}