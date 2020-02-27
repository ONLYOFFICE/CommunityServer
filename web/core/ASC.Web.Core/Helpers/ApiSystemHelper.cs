/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Core;
using ASC.Core.Tenants;
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

        private static string Skey { get; set; }

        static ApiSystemHelper()
        {
            ApiSystemUrl = ConfigurationManager.AppSettings["web.api-system"];
            ApiCacheUrl = ConfigurationManager.AppSettings["web.api-cache"];
            Skey = ConfigurationManager.AppSettings["core.machinekey"];
        }


        public static string CreateAuthToken(string pkey)
        {
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(Skey)))
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