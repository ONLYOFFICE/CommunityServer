using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using ASC.Common.Caching;
using ASC.Common.Threading.Workers;
using ASC.Web.Core.Client;
using log4net;

namespace ASC.Web.Studio.Core.HelpCenter
{
    [Serializable]
    [DataContract(Name = "BaseHelpCenterData", Namespace = "")]
    public abstract class BaseHelpCenterData
    {
        [DataMember(Name = "ResetCacheKey")]
        public string ResetCacheKey { get; set; }

        public abstract void Init(string html, string helpLinkBlock, string url);
    }

    class HelpDownloader
    {
        private static readonly WorkerQueue<HelpCenterRequest> Tasks;
        private static readonly object LockObj;
        private static readonly bool DownloadEnabled;
        private static bool stopRequesting;
        private static readonly ILog Log = LogManager.GetLogger("ASC.Web.HelpCenter");

        static HelpDownloader()
        {
            Tasks = new WorkerQueue<HelpCenterRequest>(1, TimeSpan.FromSeconds(60), 1, true);
            LockObj = new object();
            if (
                !bool.TryParse(WebConfigurationManager.AppSettings["web.help-center.download"] ?? "false",
                    out DownloadEnabled))
            {
                DownloadEnabled = false;
            }
        }

        public static void Make(HelpCenterRequest request, Action<HelpCenterRequest, string> starter)
        {
            if (!DownloadEnabled) return;

            lock (LockObj)
            {
                if (Tasks.GetItems().Any(r => r.Url == request.Url)) return;

                Tasks.Add(request);

                if (!Tasks.IsStarted)
                {
                    Tasks.Start(r =>
                    {
                        var html = SendRequest(r.Url);
                        starter(r, html);
                    });
                }
            }
        }

        private static string SendRequest(string url)
        {
            if (stopRequesting) return string.Empty;
            try
            {
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);

                httpWebRequest.AllowAutoRedirect = false;
                httpWebRequest.Timeout = 15000;
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers["Accept-Language"] = "en"; // get correct en lang

                var countTry = 0;
                const int maxTry = 3;
                while (countTry < maxTry)
                {
                    try
                    {
                        countTry++;
                        using (var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                        using (var stream = httpWebResponse.GetResponseStream())
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet))
                            )
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.Timeout)
                        {
                            throw;
                        }
                    }
                }

                stopRequesting = true;
                throw new WebException("Timeout " + maxTry, WebExceptionStatus.Timeout);
            }
            catch (Exception e)
            {
                Log.Error(string.Format("HelpCenter is not avaliable by url {0}", url), e);
            }
            return string.Empty;
        }
    }

    public class BaseHelpCenterStorage<T> : IDisposable where T : BaseHelpCenterData, new()
    {
        private static Dictionary<string, T> data = new Dictionary<string, T>();
        const string BaseStoragePath = "/App_Data/static/helpcenter/";

        private string FilePath { get; set; }
        private string CacheKey { get; set; }

        private readonly ICache cache = AscCache.Memory;
        private readonly TimeSpan expirationTimeout = TimeSpan.FromDays(1);
        private HttpClient httpClient;

        public BaseHelpCenterStorage(string basePath, string fileName, string cacheKey)
        {
            FilePath = Path.GetFullPath(Path.Combine(basePath + BaseStoragePath, fileName.TrimStart('/')));
            CacheKey = cacheKey;

            var httpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false
            };
            httpClient = new HttpClient(httpHandler) {Timeout = TimeSpan.FromMinutes(1)};
        }

        public T GetData(string baseUrl, string page, string helpLinkBlock)
        {
            var url = baseUrl + page;
            var helpCenterData = GetFromCacheOrFile(url);
            if (helpCenterData != null) return helpCenterData;

            var request = new HelpCenterRequest
            {
                Url = url,
                BaseUrl = baseUrl,
                HelpLinkBlock = helpLinkBlock
            };

            HelpDownloader.Make(request, (r, html) => InitAndCacheData(r, html, ClientSettings.ResetCacheKey));

            return null;
        }

        public async Task<T> GetDataAsync(string baseUrl, string page, string helpLinkBlock, string resetCacheKey)
        {
            var url = baseUrl + page;
            var helpCenterData = GetFromCacheOrFile(url);

            if (helpCenterData == null)
            {
                var request = new HelpCenterRequest
                {
                    Url = url,
                    BaseUrl = baseUrl,
                    HelpLinkBlock = helpLinkBlock
                };
                var html = await httpClient.GetStringAsync(url);
                InitAndCacheData(request, html, resetCacheKey);
            }

            return helpCenterData;
        }

        private T GetFromCacheOrFile(string url)
        {
            try
            {
                data = FromCache();
                if (data == null)
                {
                    data = FromFile();
                    if (data != null)
                    {
                        ToCache(data);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error GetVideoGuide", e);
            }

            if (data == null)
            {
                data = new Dictionary<string, T>();
            }

            T helpCenterData = null;
            if (data.ContainsKey(url))
            {
                helpCenterData = data[url];
            }
            if (helpCenterData != null &&
                string.CompareOrdinal(helpCenterData.ResetCacheKey, ClientSettings.ResetCacheKey) != 0)
            {
                return null;
            }
            return helpCenterData;
        }

        private void InitAndCacheData(HelpCenterRequest request, string html, string resetCacheKey)
        {
            var helpCenterData = new T {ResetCacheKey = resetCacheKey };
            helpCenterData.Init(html, request.HelpLinkBlock, request.BaseUrl);

            lock (data)
            {
                data[request.Url] = helpCenterData;

                ToFile(data);
                ToCache(data);
            }
        }

        private Dictionary<string, T> FromFile()
        {
            if (File.Exists(FilePath))
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    return FromStream(stream);
                }
            }

            return null;
        }

        private void ToFile(Dictionary<string, T> obj)
        {
            try
            {
                using (var filesStream = File.Open(FilePath, FileMode.Create))
                using (var stream = ToStream(obj))
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                    var buffer = new byte[4096];
                    int readed;
                    while ((readed = stream.Read(buffer, 0, 4096)) != 0)
                    {
                        filesStream.Write(buffer, 0, readed);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error UpdateVideoGuide", e);
            }
        }

        private static MemoryStream ToStream(Dictionary<string, T> obj)
        {
            var stream = new MemoryStream();
            using (var gzip = new GZipStream(stream, CompressionMode.Compress, true))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(gzip, obj);
            }
            return stream;
        }

        private static Dictionary<string, T> FromStream(Stream stream)
        {
            var formatter = new BinaryFormatter();
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                return (Dictionary<string, T>) formatter.Deserialize(gzip);
            }
        }

        private void ToCache(Dictionary<string, T> obj)
        {
            cache.Insert(CacheKey, obj, DateTime.UtcNow.Add(expirationTimeout));
        }

        private Dictionary<string, T> FromCache()
        {
            return cache.Get<Dictionary<string, T>>(CacheKey);
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
        }
    }

    public class HelpCenterRequest
    {
        public string Url { get; set; }
        public string HelpLinkBlock { get; set; }
        public string BaseUrl { get; set; }
    }
}