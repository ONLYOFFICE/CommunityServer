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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text;
using System.Xml.Linq;

namespace ASC.Api.Client
{
    public class ApiClient
    {
        public string Host { get; private set; }

        public string ApiRoot { get; private set; }

        public UriScheme UriScheme { get; set; }

        public ApiClient(string host)
            : this(host, ApiClientConfiguration.GetSection())
        {

        }

        public ApiClient(string host, ApiClientConfiguration config)
            : this(host, config.ApiRoot, config.UriScheme)
        {
            
        }

        public ApiClient(string host, string apiRoot, UriScheme uriScheme)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Empty host.", "host");
            }
            if (apiRoot == null)
            {
                throw new ArgumentNullException("apiRoot");
            }
            Host = host;
            ApiRoot = apiRoot.TrimStart('~').Trim('/');
            UriScheme = uriScheme;
        }

        public string Authorize(string email, string password)
        {
            ApiResponse response = GetResponse(
                new ApiRequest("authentication")
                    .WithMethod(HttpMethod.Post)
                    .WithResponseType(ResponseType.Xml)
                    .WithParameter("userName", email)
                    .WithParameter("password", password));

            var xml = XElement.Load(new StringReader(response.Response));

            var tokenElement = xml.Element("token");
            var token = tokenElement != null ? tokenElement.Value : null;
            
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthenticationException("No authorization token returned from server.");
            }

            return token;
        }

        public ApiResponse GetResponse(ApiRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpWebRequest webRequest = CreateWebRequest(request);
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException exception)
            {
                if (exception.Response == null)
                {
                    throw;
                }

                webResponse = (HttpWebResponse)exception.Response;
                if (new ContentType(webResponse.ContentType).MediaType == "text/html") // generic http error
                {
                    throw new HttpErrorException((int)webResponse.StatusCode, webResponse.StatusDescription);
                }
            }

            using (webResponse)
            using (var responseStream = webResponse.GetResponseStream())
            {
                var mediaType = new ContentType(webResponse.ContentType).MediaType;
                switch (mediaType)
                {
                    case "application/json":
                        return ResponseParser.ParseJsonResponse(responseStream);
                    case "text/xml":
                        return ResponseParser.ParseXmlResponse(responseStream);
                    default:
                        throw new UnsupportedMediaTypeException(mediaType);
                }
            }
        }

        private HttpWebRequest CreateWebRequest(ApiRequest request)
        {
            var uri = new UriBuilder(UriScheme.ToString().ToLower(), Host) {Path = CreatePath(request)};

            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Delete)
            {
                uri.Query = CreateQuery(request);
            }
            
            var httpRequest = (HttpWebRequest)WebRequest.Create(uri.ToString());
            httpRequest.Method = request.Method.ToString().ToUpper();
            httpRequest.AllowAutoRedirect = true;

            if (!string.IsNullOrWhiteSpace(request.AuthToken))
            {
                httpRequest.Headers["Authorization"] = request.AuthToken;
            }
            httpRequest.Headers.Add(request.Headers);

            if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Delete)
            {
                if (request.Files.Any() || request.RequestType == RequestType.Multipart)
                {
                    WriteMultipart(httpRequest, request);
                }
                else
                {
                    WriteUrlencoded(httpRequest, request);
                }
            }

            return httpRequest;
        }

        private string CreatePath(ApiRequest request)
        {
            return ApiRoot + "/" + request.Url + "." + request.ResponseType.ToString().ToLower();
        }

        private string CreateQuery(ApiRequest request)
        {
            var sb = new StringBuilder();
            foreach (var parameter in EnumerateParameters(request))
            {
                sb.AppendFormat("{0}={1}&", parameter.Name, parameter.Value);
            }

            if (sb.Length > 0) 
                sb.Remove(sb.Length - 1, 1);
            
            return sb.ToString();
        }

        private void WriteMultipart(HttpWebRequest httpRequest, ApiRequest request)
        {
            string boundary = DateTime.UtcNow.Ticks.ToString("x");

            httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            using (var requestStream = httpRequest.GetRequestStream())
            {
                foreach (var parameter in EnumerateParameters(request))
                {
                    requestStream.WriteString("\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\";\r\n\r\n{2}", boundary, parameter.Name, parameter.Value);
                }

                foreach (var file in request.Files)
                {
                    requestStream.WriteString("\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\nContent-Transfer-Encoding: binary\r\n\r\n",
                                              boundary,
                                              Path.GetFileNameWithoutExtension(file.Name),
                                              Path.GetFileName(file.Name),
                                              !string.IsNullOrEmpty(file.ContentType) ? file.ContentType : "application/octet-stream");

                    if (file.Data.CanSeek)
                    {
                        file.Data.Seek(0, SeekOrigin.Begin);
                    }
                    file.Data.CopyTo(requestStream);
                    if (file.CloseStream)
                    {
                        file.Data.Close();
                    }
                }

                requestStream.WriteString("\r\n--{0}--\r\n", boundary);
            }
        }

        private void WriteUrlencoded(HttpWebRequest httpRequest, ApiRequest request)
        {
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            using (var requestStream = httpRequest.GetRequestStream())
            using (var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write(CreateQuery(request));
            }
        }

        private static IEnumerable<RequestParameter> EnumerateParameters(ApiRequest request)
        {
            foreach (var parameter in request.Parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Name) && parameter.Value != null))
            {
                var enumerable = parameter.Value as IEnumerable;
                if (enumerable != null && !(parameter.Value is string))
                {
                    foreach (var value in enumerable)
                    {
                        yield return new RequestParameter {Name = parameter.Name + "[]", Value = value};
                    }
                }
                else if (parameter.Value is DateTime)
                {
                    yield return new RequestParameter {Name = parameter.Name, Value = ((DateTime)parameter.Value).ToString("o")};
                }
                else
                {
                    yield return parameter;
                }
            }
        }
    }
}
