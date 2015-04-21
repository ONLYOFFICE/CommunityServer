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
