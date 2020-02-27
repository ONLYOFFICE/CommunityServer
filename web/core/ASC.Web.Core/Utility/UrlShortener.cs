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


using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ASC.FederatedLogin.LoginProviders;
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
                    else if (WebConfigurationManager.AppSettings.AllKeys.Contains("web.url-shortener"))
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
        private readonly string sKey;

        public OnlyoShortener()
        {
            url = WebConfigurationManager.AppSettings["web.url-shortener"];
            internalUrl = WebConfigurationManager.AppSettings["web.url-shortener.internal"];
            sKey = WebConfigurationManager.AppSettings["core.machinekey"];

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
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(sKey)))
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
