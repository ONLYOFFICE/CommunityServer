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
using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace ASC.Api.Client
{
    public class ApiRequest
    {
        public string Url { get; private set; }
        
        public string AuthToken { get; private set; }

        public HttpMethod Method { get; set; }

        public RequestType RequestType { get; set; }

        public ResponseType ResponseType { get; set; }

        public RequestParameterCollection Parameters { get; private set; }

        public RequestFileCollection Files { get; private set; }

        public NameValueCollection Headers { get; private set; }

        public ApiRequest(string url)
            : this(url, null)
        {
            
        }

        public ApiRequest(string url, string authToken)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (string.IsNullOrEmpty(authToken) && HttpContext.Current != null)
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies.Get("asc_auth_key");
                if (authCookie != null)
                    authToken = authCookie.Value;
            }

            url = url.Trim('/');

            int queryIndex = url.IndexOf('?');
            if (queryIndex > 0)
                url = url.Remove(queryIndex);

            if (url.EndsWith(".xml"))
            {
                url = url.Remove(url.Length - 4);
                ResponseType = ResponseType.Xml;
            }
            else if (url.EndsWith(".json"))
            {
                url = url.Remove(url.Length - 5);
                ResponseType = ResponseType.Json;
            }

            Url = url;
            AuthToken = authToken;
            Parameters = new RequestParameterCollection();
            Files = new RequestFileCollection();
            Headers = new NameValueCollection();
        }

        public ApiRequest WithMethod(HttpMethod method)
        {
            Method = method;
            return this;
        }

        public ApiRequest WithRequestType(RequestType requestType)
        {
            RequestType = requestType;
            return this;
        }

        public ApiRequest WithResponseType(ResponseType responseType)
        {
            ResponseType = responseType;
            return this;
        }

        public ApiRequest WithParameter(string name, object value)
        {
            Parameters.Add(new RequestParameter {Name = name, Value = value});
            return this;
        }

        public ApiRequest WithFile(string name, Stream data, bool closeStream = false)
        {
            return WithFile(name, null, data);
        }

        public ApiRequest WithFile(string name, string contentType, Stream data, bool closeStream = false)
        {
            Files.Add(new RequestFile {Name = name, ContentType = contentType, Data = data, CloseStream = closeStream});
            return this;
        }

        public ApiRequest WithHeader(string name, string value)
        {
            Headers.Add(name, value);
            return this;
        }

        public override string ToString()
        {
            return Method.ToString().ToUpper() + " " + Url + "." + ResponseType.ToString().ToLower();
        }
    }
}
