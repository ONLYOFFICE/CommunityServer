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
