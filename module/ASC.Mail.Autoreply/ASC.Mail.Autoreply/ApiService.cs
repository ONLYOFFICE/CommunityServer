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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using ASC.Common.Threading.Workers;
using ASC.Core;
using log4net;

namespace ASC.Mail.Autoreply
{
    internal class ApiService
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ApiService));
        private readonly WorkerQueue<ApiRequest> _messageQueue = new WorkerQueue<ApiRequest>(4, TimeSpan.FromMinutes(5), 5, false);
        private readonly string _adressTemplate;

        public ApiService(bool https)
        {

            _adressTemplate = (https ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + "{{0}}{0}/{{1}}";
            var baseDomain = CoreContext.Configuration.BaseDomain.TrimEnd('/');
            if (baseDomain == "localhost")
            {
                baseDomain = string.Empty;
            }
            if (!string.IsNullOrEmpty(baseDomain) && baseDomain[0] != '.' && baseDomain[0] != '/')
            {
                baseDomain = '.' + baseDomain;
            }
            _adressTemplate = string.Format(_adressTemplate, baseDomain);
        }

        public void Start()
        {
            if (!_messageQueue.IsStarted)
            {
                _messageQueue.Start(SendMessage);
            }
        }

        public void Stop()
        {
            if (_messageQueue.IsStarted)
            {
                _messageQueue.Stop();
            }
        }

        public void EnqueueRequest(ApiRequest request)
        {
            _log.DebugFormat("queue request \"{0}\"", request);
            _messageQueue.Add(request);
        }

        private void SendMessage(ApiRequest requestInfo)
        {
            try
            {
                _log.DebugFormat("begin send request {0}", requestInfo);

                CoreContext.TenantManager.SetCurrentTenant(requestInfo.Tenant);
                var authKey = SecurityContext.AuthenticateMe(requestInfo.User.ID);

                if (string.IsNullOrEmpty(authKey))
                {
                    _log.ErrorFormat("can't obtain authorization cookie for user {0}", requestInfo.User.ID);
                    return;
                }

                var uri = BuildUri(requestInfo);
                _log.Debug(uri);

                _log.DebugFormat("builded uri for request {0}", uri);

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = requestInfo.Method; 
                request.AllowAutoRedirect = true;
                request.Headers["Authorization"] = authKey;

                using (var requestStream = request.GetRequestStream())
                {
                    if (requestInfo.FilesToPost != null && requestInfo.FilesToPost.Any())
                    {
                        WriteMultipartRequest(request, requestStream, requestInfo);
                    }
                    else
                    {
                        WriteFormEncoded(request, requestStream, requestInfo, authKey);
                    }
                }

                try
                {
                    using (var response = request.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var readStream = new StreamReader(responseStream))
                            {
                                _log.DebugFormat("response from server: {0}", readStream.ReadToEnd());
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    _log.Error("error while getting the response", error);
                }
                
                _log.DebugFormat("end send request {0}", requestInfo);
            }
            catch (Exception x)
            {
                _log.Error("error while sending request", x);
                throw;
            }
        }

        private Uri BuildUri(ApiRequest requestInfo)
        {
            return new Uri(string.Format(_adressTemplate, requestInfo.Tenant.TenantAlias, requestInfo.Url.TrimStart('/')));
        }

        private void WriteMultipartRequest(WebRequest request, Stream requestStream, ApiRequest requestInfo)
        {
            var boundaryId = DateTime.Now.Ticks.ToString("x");
            var boundary = "\r\n--" + boundaryId + "\r\n";
            var formdataTemplate = "\r\n--" + boundaryId + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            if (requestInfo.Parameters != null && requestInfo.Parameters.Any())
            {
                foreach (var parameter in requestInfo.Parameters.Where(x => x.Value != null))
                {
                    var formItem = string.Empty;
                    if (parameter.Value is IEnumerable<object>)
                    {
                        foreach (var value in ((IEnumerable<object>)parameter.Value))
                        {
                            formItem = string.Format(formdataTemplate, parameter.Name, value);
                        }
                    }
                    else if (parameter.Value is DateTime)
                    {
                        formItem = string.Format(formdataTemplate, parameter.Name, ((DateTime)parameter.Value).ToString("s"));
                    }
                    else
                    {
                        formItem = string.Format(formdataTemplate, parameter.Name, parameter.Value);
                    }

                    if (!string.IsNullOrEmpty(formItem))
                    {
                        _log.DebugFormat("writing form item boundary:{0}", formItem);
                        WriteUtf8String(requestStream, formItem);
                    }
                }
            }

            WriteUtf8String(requestStream, boundary);

            var files = requestInfo.FilesToPost.ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                WriteFile(requestStream, files[i]);
                
                if (i < files.Length - 1)
                    WriteUtf8String(requestStream, boundary);
                else
                    WriteUtf8String(requestStream, "\r\n--" + boundaryId + "--\r\n");
            }

            request.ContentType = new ContentType("multipart/form-data") { Boundary = boundaryId }.ToString();
        }

        private static void WriteFile(Stream requestStream, RequestFileInfo fileInfo)
        {
            var header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\nContent-Transfer-Encoding: binary\r\n\r\n",
                                       Path.GetFileNameWithoutExtension(fileInfo.Name),
                                       Path.GetFileName(fileInfo.Name),
                                       fileInfo.ContentType ?? "application/octet-stream");

            WriteUtf8String(requestStream, header);
            requestStream.Write(fileInfo.Body, 0, fileInfo.Body.Length);
        }

        private void WriteFormEncoded(WebRequest request, Stream requestStream, ApiRequest requestInfo, string authKey)
        {
            var stringBuilder = new StringBuilder();
            if (requestInfo.Parameters != null && requestInfo.Parameters.Any())
            {
                foreach (var parameter in requestInfo.Parameters.Where(x => x.Value != null))
                {
                    if (parameter.Value is IEnumerable<object>)
                    {
                        foreach (var value in ((IEnumerable<object>)parameter.Value))
                        {
                            stringBuilder.AppendFormat("{0}[]={1}&", HttpUtility.UrlEncode(parameter.Name), HttpUtility.UrlEncode(value.ToString()));
                        }
                    }
                    else if (parameter.Value is DateTime)
                    {
                        stringBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(parameter.Name), HttpUtility.UrlEncode(((DateTime)parameter.Value).ToString("s")));
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(parameter.Name), HttpUtility.UrlEncode(parameter.Value.ToString()));
                    }
                }
            }

            stringBuilder.AppendFormat("asc_auth_key={0}", authKey);

            _log.DebugFormat("writing form data {0}", stringBuilder);

            WriteUtf8String(requestStream, stringBuilder.ToString());
            request.ContentType = "application/x-www-form-urlencoded";
        }

        private static void WriteUtf8String(Stream stream, string str)
        {
            byte[] headerbytes = Encoding.UTF8.GetBytes(str);
            stream.Write(headerbytes, 0, headerbytes.Length);
        }
    }
}