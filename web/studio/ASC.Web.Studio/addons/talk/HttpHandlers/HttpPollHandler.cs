/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Net;
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
            var request = (HttpWebRequest)WebRequest.Create(boshUri);

            request.Method = context.Request.HttpMethod;

            // headers
            request.UserAgent = context.Request.UserAgent;
            request.Accept = string.Join(",", context.Request.AcceptTypes);
            if (!string.IsNullOrEmpty(context.Request.Headers["Accept-Encoding"]))
            {
                request.Headers["Accept-Encoding"] = context.Request.Headers["Accept-Encoding"];
            }
            request.ContentType = context.Request.ContentType;
            request.ContentLength = context.Request.ContentLength;
            // body
            using (var stream = request.GetRequestStream())
            {
                CopyStream(context.Request.InputStream, stream);
            }

            return request.BeginGetResponse(cb, new object[] { context, request });
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            try
            {
                var context = (HttpContext)((object[])result.AsyncState)[0];
                var request = (HttpWebRequest)((object[])result.AsyncState)[1];
                using (var response = request.EndGetResponse(result))
                {
                    context.Response.ContentType = response.ContentType;

                    // headers
                    foreach (string h in response.Headers)
                    {
                        context.Response.AppendHeader(h, response.Headers[h]);
                    }
                    // body
                    using (var stream = response.GetResponseStream())
                    {
                        CopyStream(stream, context.Response.OutputStream);
                    }

                    response.Close();
                    context.Response.Flush();
                }
            }
            catch (Exception err)
            {
                if (err is IOException || err.InnerException is IOException)
                {
                    // ignore
                }
                else
                {
                    throw;
                }
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotSupportedException();
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
