/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using log4net;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Talk.HttpHandlers
{
    public class HttpPollHandler : IHttpAsyncHandler
    {
        private static readonly Uri boshUri;


        static HttpPollHandler()
        {
            var uri = WebConfigurationManager.AppSettings["BoshPath"] ?? "http://localhost:5280/http-poll/";
            boshUri = new Uri(VirtualPathUtility.AppendTrailingSlash(uri));
        }


        public bool IsReusable
        {
            get { return true; }
        }


        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return new AsynchOperation(cb, context, extraData).Start();
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotSupportedException();
        }


        private class AsynchOperation : IAsyncResult
        {
            private AsyncCallback cb;
            private HttpContext context;


            public WaitHandle AsyncWaitHandle
            {
                get { return null; }
            }

            public object AsyncState
            {
                get;
                private set;
            }

            public bool IsCompleted
            {
                get;
                private set;
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }


            public AsynchOperation(AsyncCallback callback, HttpContext context, object state)
            {
                cb = callback;
                this.context = context;
                AsyncState = state;
                IsCompleted = false;
            }

            public IAsyncResult Start()
            {
                ThreadPool.QueueUserWorkItem(AsyncWork, null);
                return this;
            }

            private void AsyncWork(object _)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(boshUri);
                    request.Method = context.Request.HttpMethod;

                    // copy headers & body
                    request.UserAgent = context.Request.UserAgent;
                    request.Accept = string.Join(",", context.Request.AcceptTypes);
                    if (!string.IsNullOrEmpty(context.Request.Headers["Accept-Encoding"]))
                    {
                        request.Headers["Accept-Encoding"] = context.Request.Headers["Accept-Encoding"];
                    }
                    request.ContentType = context.Request.ContentType;
                    request.ContentLength = context.Request.ContentLength;

                    using (var stream = request.GetRequestStream())
                    {
                        CopyStream(context.Request.InputStream, stream);
                    }

                    request.BeginGetResponse(EndGetResponse, Tuple.Create(context, request));
                }
                catch (Exception err)
                {
                    if (err is IOException || err.InnerException is IOException)
                    {
                        // ignore
                    }
                    else
                    {
                        LogManager.GetLogger("ASC.Web.BOSH").Error(err);
                    }
                }
            }

            private void EndGetResponse(IAsyncResult ar)
            {
                var data = (Tuple<HttpContext, HttpWebRequest>)ar.AsyncState;
                var context = data.Item1;
                var request = data.Item2;

                try
                {
                    using (var response = request.EndGetResponse(ar))
                    {
                        context.Response.ContentType = response.ContentType;

                        // copy headers & body
                        foreach (string h in response.Headers)
                        {
                            context.Response.AppendHeader(h, response.Headers[h]);
                        }
                        using (var stream = response.GetResponseStream())
                        {
                            CopyStream(stream, context.Response.OutputStream);
                        }
                        context.Response.Flush();
                    }
                }
                catch (Exception err)
                {
                    if (err is IOException || err.InnerException is IOException || err is HttpException)
                    {
                        // ignore
                    }
                    else
                    {
                        LogManager.GetLogger("ASC.Web.BOSH").Error(err);
                    }
                }
                finally
                {
                    IsCompleted = true;
                    cb(this);
                }
            }

            private void CopyStream(Stream from, Stream to)
            {
                var buffer = new byte[1024];
                while (true)
                {
                    var read = from.Read(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    to.Write(buffer, 0, read);
                }
            }
        }
    }
}
