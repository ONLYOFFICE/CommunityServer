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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ASC.FederatedLogin.Helpers
{
    public class RequestHelper
    {
        public static String PerformRequest(String uri, String contentType = "", String method = "GET", String body = "", Dictionary<string, string> headers = null, int timeout = 30000)
        {
            if (String.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");

            var request = WebRequest.Create(uri);
            request.Method = method;
            request.Timeout = timeout;
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }


            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }

            var bytes = Encoding.UTF8.GetBytes(body ?? "");
            if (request.Method != "GET" && bytes.Length > 0)
            {
                request.ContentLength = bytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            try
            {
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null) return null;
                    using (var readStream = new StreamReader(stream))
                    {
                        return readStream.ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                request.Abort();
                throw;
            }
        }
    }
}