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


using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ASC.Web.Core.Helpers
{
    public class ApiSystemHelper
    {
        public static string ApiSystemUrl { get; private set; }

        public static string ApiCacheUrl { get; private set; }

        private static byte[] Skey { get; set; }

        static ApiSystemHelper()
        {
            ApiSystemUrl = ConfigurationManagerExtension.AppSettings["web.api-system"];
            ApiCacheUrl = ConfigurationManagerExtension.AppSettings["web.api-cache"];
            Skey = MachinePseudoKeys.GetMachineConstant();
        }


        public static string CreateAuthToken(string pkey)
        {
            using (var hasher = new HMACSHA1(Skey))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var hash = HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
                return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
            }
        }

        #region system

        public static void ValidatePortalName(string domain)
        {
            try
            {
                var data = string.Format("portalName={0}", HttpUtility.UrlEncode(domain));
                SendToApi(ApiSystemUrl, "portal/validateportalname", WebRequestMethods.Http.Post, data);
            }
            catch (WebException exception)
            {
                if (exception.Status != WebExceptionStatus.ProtocolError || exception.Response == null) return;

                var response = exception.Response;
                try
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var result = reader.ReadToEnd();

                        var resObj = JObject.Parse(result);
                        if (resObj["error"] != null)
                        {
                            if (resObj["error"].ToString() == "portalNameExist")
                            {
                                var varians = resObj.Value<JArray>("variants").Select(jv => jv.Value<String>());
                                throw new TenantAlreadyExistsException("Address busy.", varians);
                            }

                            throw new Exception(resObj["error"].ToString());
                        }
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
        }

        #endregion

        #region cache

        public static void AddTenantToCache(string domain)
        {
            var data = string.Format("portalName={0}", HttpUtility.UrlEncode(domain));
            SendToApi(ApiCacheUrl, "portal/add", WebRequestMethods.Http.Post, data);
        }

        public static void RemoveTenantFromCache(string domain)
        {
            SendToApi(ApiCacheUrl, "portal/remove?portalname=" + HttpUtility.UrlEncode(domain), "DELETE");
        }

        public static IEnumerable<string> FindTenantsInCache(string domain)
        {
            var result = SendToApi(ApiCacheUrl, "portal/find?portalname=" + HttpUtility.UrlEncode(domain), WebRequestMethods.Http.Get);
            var resObj = JObject.Parse(result);

            var variants = resObj.Value<JArray>("variants");
            return variants == null
                       ? null
                       : variants.Select(jv => jv.Value<String>()).ToList();
        }

        #endregion

        private static string SendToApi(string absoluteApiUrl, string apiPath, string httpMethod, string data = null)
        {
            Uri uri;
            if (!Uri.TryCreate(absoluteApiUrl, UriKind.Absolute, out uri))
            {
                var appUrl = CommonLinkUtility.GetFullAbsolutePath("/");
                absoluteApiUrl = string.Format("{0}/{1}", appUrl.TrimEnd('/'), absoluteApiUrl.TrimStart('/')).TrimEnd('/');
            }

            var url = String.Format("{0}/{1}", absoluteApiUrl, apiPath);

            var webRequest = (HttpWebRequest) WebRequest.Create(url);
            webRequest.Method = httpMethod;
            webRequest.Accept = "application/json";
            webRequest.Headers.Add(HttpRequestHeader.Authorization, CreateAuthToken(SecurityContext.CurrentAccount.ID.ToString()));
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = 0;

            if (data != null)
            {
                webRequest.ContentLength = data.Length;

                using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                {
                    writer.Write(data);
                }
            }

            using (var response = webRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}