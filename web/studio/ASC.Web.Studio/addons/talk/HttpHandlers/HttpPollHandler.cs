/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;

namespace ASC.Web.Talk.HttpHandlers
{
    public class HttpPollHandler : HttpTaskAsyncHandler
    {
        private static readonly Uri boshUri = new Uri(VirtualPathUtility.AppendTrailingSlash(ConfigurationManagerExtension.AppSettings["BoshPath"] ?? "http://localhost:5280/http-poll/"));

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

        public override async System.Threading.Tasks.Task ProcessRequestAsync(HttpContext context)
        {
            var request = (HttpWebRequest)WebRequest.Create(boshUri);
            request.Method = context.Request.HttpMethod;

            request.UserAgent = context.Request.UserAgent;
            request.Accept = string.Join(",", context.Request.AcceptTypes);
            if (!string.IsNullOrEmpty(context.Request.Headers["Accept-Encoding"]))
            {
                request.Headers["Accept-Encoding"] = context.Request.Headers["Accept-Encoding"];
            }
            request.ContentType = context.Request.ContentType;
            request.ContentLength = context.Request.ContentLength;

            using (var stream = await request.GetRequestStreamAsync())
            using (var writer = new StreamWriter(stream))
            {
                CopyStream(context.Request.InputStream, stream);

                writer.Flush();
                writer.Dispose();
            }

            using (var response = await request.GetResponseAsync())
            {
                context.Response.ContentType = response.ContentType;

                // copy headers & body
                foreach (string h in response.Headers)
                {
                    context.Response.AppendHeader(h, response.Headers[h]);
                }

                using (var respStream = response.GetResponseStream())
                {
                    CopyStream(respStream, context.Response.OutputStream);
                }
            }
        }
    }
}
