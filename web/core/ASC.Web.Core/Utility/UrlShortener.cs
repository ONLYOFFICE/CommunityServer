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
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using ASC.FederatedLogin.LoginProviders;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.Utility
{
    public interface IUrlShortener
    {
        string GetShortenLink(string shareLink);
    }

    public static class UrlShortener
    {
        public static bool Enabled { get { return !(Instance is NullShortener); } }

        private static IUrlShortener _instance;
        public static IUrlShortener Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (BitlyLoginProvider.Enabled)
                    {
                        _instance = new BitLyShortener();
                    }
                    else if (ConfigurationManagerExtension.AppSettings.AllKeys.Contains("web.url-shortener"))
                    {
                        _instance = new OnlyoShortener();
                    }
                    else
                    {
                        _instance = new NullShortener();
                    }
                }

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }

    public class BitLyShortener : IUrlShortener
    {
        public string GetShortenLink(string shareLink)
        {
            return BitlyLoginProvider.GetShortenLink(shareLink);
        }
    }

    public class OnlyoShortener : IUrlShortener
    {
        private readonly string url;
        private readonly string internalUrl;
        private readonly byte[] sKey;

        public OnlyoShortener()
        {
            url = ConfigurationManagerExtension.AppSettings["web.url-shortener"];
            internalUrl = ConfigurationManagerExtension.AppSettings["web.url-shortener.internal"];
            sKey = MachinePseudoKeys.GetMachineConstant();

            if (!url.EndsWith("/"))
                url += '/';
        }

        public string GetShortenLink(string shareLink)
        {
            using (var client = new WebClient { Encoding = Encoding.UTF8 })
            {
                client.Headers.Add("Authorization", CreateAuthToken());
                return CommonLinkUtility.GetFullAbsolutePath(url + client.DownloadString(new Uri(internalUrl + "?url=" + HttpUtility.UrlEncode(shareLink))));
            }
        }

        private string CreateAuthToken(string pkey = "urlShortener")
        {
            using (var hasher = new HMACSHA1(sKey))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var hash = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
                return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
            }
        }
    }

    public class NullShortener : IUrlShortener
    {
        public string GetShortenLink(string shareLink)
        {
            return null;
        }
    }
}
