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

        public ApiRequest WithFile(string name, Stream data)
        {
            return WithFile(name, null, data);
        }

        public ApiRequest WithFile(string name, string contentType, Stream data)
        {
            Files.Add(new RequestFile {Name = name, ContentType = contentType, Data = data});
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
