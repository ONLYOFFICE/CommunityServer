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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using System.Web.Script.Serialization;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core
{
    enum WarmUpType
    {
        None,
        Basic,
        Full
    }

    public class WarmUp
    {
        private Tenant Tenant { get; set; }
        private List<string> Urls { get; set; }
        private readonly object locker = new object();
        private readonly CancellationTokenSource tokenSource;
        private readonly ICacheNotify cacheNotify = AscCache.Notify;
        private readonly Dictionary<string, StartupProgress> startupProgresses;
        private readonly ILog logger;
        private StartupProgress progress;
        private readonly bool successInitialized = false;

        internal bool Started { get; private set; }

        private static readonly string InstanseId = Process.GetCurrentProcess().Id.ToString();
        private static readonly WarmUp instance = new WarmUp();
        private static int instanceCount = 1;
        public static WarmUp Instance { get { return instance; } }

        private WarmUp()
        {
            try
            {
                logger = LogManager.GetLogger("ASC");

                int timeout;

                if (!int.TryParse(ConfigurationManagerExtension.AppSettings["web.warmup.timeout"], out timeout))
                {
                    timeout = 15;
                }

                tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(timeout));

                if (CoreContext.Configuration.Standalone)
                {
                    Tenant = CoreContext.TenantManager.GetTenants().FirstOrDefault();
                }
                else
                {
                    Tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                }


                if (!int.TryParse(ConfigurationManagerExtension.AppSettings["web.warmup.count"], out instanceCount))
                {
                    instanceCount = 1;
                }

                startupProgresses = new Dictionary<string, StartupProgress>(instanceCount);

                Urls = GetUrlsForRequests();

                progress = new StartupProgress(Urls.Count);

                cacheNotify.Subscribe<KeyValuePair<string, StartupProgress>>(
                    (kv, action) =>
                    {
                        switch (action)
                        {
                            case CacheNotifyAction.Remove:
                                tokenSource.Cancel();

                                lock (locker)
                                {
                                    progress.Complete();
                                }

                                break;

                            case CacheNotifyAction.InsertOrUpdate:
                                if (!startupProgresses.ContainsKey(kv.Key))
                                {
                                    startupProgresses.Add(kv.Key, kv.Value);
                                }
                                else
                                {
                                    startupProgresses[kv.Key] = kv.Value;
                                }
                                break;
                        }
                    });

                Publish();
                successInitialized = true;
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web").Error("Warmup error", e);
            }
        }

        public bool CheckCompleted()
        {
            if (!successInitialized) return true;
            if (!CoreContext.Configuration.Standalone) return true;
            if (WarmUpSettings.GetCompleted()) return true;

            return startupProgresses.All(pair => pair.Value.Completed);
        }

        public string GetSerializedProgress()
        {
            var combinedProgress = (StartupProgress)progress.Clone();

            var averageProgress = startupProgresses.Average(pair => pair.Value.Progress);
            combinedProgress.Progress = Convert.ToInt32(averageProgress);

            return new JavaScriptSerializer().Serialize(combinedProgress);
        }

        private static List<string> GetUrlsForRequests()
        {
            var items = WebItemManager.Instance
                .GetItemsAll()
                .Where(item => !item.IsSubItem() && item.Visible && !string.IsNullOrEmpty(item.WarmupURL))
                .Select(r => r.WarmupURL)
                .ToList();

            items.Add("~/Warmup.aspx");

            return items.Select(GetFullAbsolutePath).ToList();
        }

        private static string GetFullAbsolutePath(string virtualPath)
        {
            string result;
            const string warmupParam = "warmup=true";

            if (CoreContext.Configuration.Standalone)
            {
                var url = HttpContext.Current.Request.Url;
                var domain = ConfigurationManagerExtension.AppSettings["web.warmup.domain"] ?? url.Host;
                var uriBuilder = new UriBuilder(url.Scheme, domain, url.Port, virtualPath.TrimStart('~', '/'));
                result = uriBuilder.ToString();
            }
            else
            {
                result = CommonLinkUtility.GetFullAbsolutePath(virtualPath);
            }

            return virtualPath.Contains("?")
                           ? string.Format("{0}&{1}", result, warmupParam)
                           : string.Format("{0}?{1}", result, warmupParam);
        }

        internal void Start()
        {
            if (!successInitialized ||
                (ConfigurationManagerExtension.AppSettings["web.warmup.enabled"] ?? "false") == "false" ||
                WarmUpSettings.GetCompleted())
            {
                progress.Complete();
                return;
            }

            if (!Started)
            {
                Started = true;

                var task = new Task(() =>
                {
                    try
                    {
                        MakeRequest();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error", ex);
                        Terminate();
                    }
                    finally
                    {
                        lock (locker)
                        {
                            progress.Complete();
                        }
                        Publish();
                    }

                }, tokenSource.Token, TaskCreationOptions.LongRunning);

                task.ConfigureAwait(false);
                task.Start();
            }
        }

        public void Restart()
        {
            Started = false;
            WarmUpSettings.SetCompleted(false);
            lock (locker)
            {
                progress = new StartupProgress(Urls.Count);
            }

            Publish();
            Start();
        }

        internal void Terminate()
        {
            cacheNotify.Publish(GetNotifyKey(), CacheNotifyAction.Remove);
        }

        private void MakeRequest()
        {
            if (WorkContext.IsMono)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, h, p) => true;

            if (Tenant == null)
            {
                LogManager.GetLogger("ASC").Warn("Warmup: tenant is null");
                return;
            }

            CoreContext.TenantManager.SetCurrentTenant(Tenant);
            var auth = SecurityContext.AuthenticateMe(Tenant.OwnerId);
            var tasksCount = (int)Math.Round((double)Environment.ProcessorCount / instanceCount);
            if (tasksCount == 0) tasksCount = 1;

            for (var j = 0; j < Urls.Count; j++)
            {
                if (tokenSource.IsCancellationRequested) return;

                var tasks = new List<Task>(tasksCount);

                for (var i = 0; i < tasksCount && j < Urls.Count; i++, j++)
                {
                    tasks.Add(MakeRequest(Urls[j], auth));
                }

                j--;

                try
                {
                    Task.WaitAll(tasks.Where(r => r != null).ToArray());
                }
                catch (AggregateException ex)
                {
                    logger.Error("WarmUp error", ex);
                }


                lock (locker)
                {
                    progress.Increment(tasks.Count(r => r != null));
                }

                Publish();
            }
        }

        private async Task MakeRequest(string url, string auth)
        {
            LogManager.GetLogger("ASC").Debug(url);
            var handler = new HttpClientHandler
            {
                PreAuthenticate = true,
                UseProxy = false
            };

            using (var hc = new HttpClient(handler))
            {
                hc.Timeout = TimeSpan.FromMinutes(5);
                hc.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.4; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", auth);
                await hc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);
            }
        }

        private void Publish()
        {
            cacheNotify.Publish(GetNotifyKey(), CacheNotifyAction.InsertOrUpdate);
        }

        private KeyValuePair<string, StartupProgress> GetNotifyKey()
        {
            return new KeyValuePair<string, StartupProgress>(InstanseId, progress);
        }
    }

    [DataContract]
    class StartupProgress : ICloneable
    {
        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public int Total { get; set; }

        [DataMember]
        public int Percentage { get { return 100; } }//{ get { return ClientSettings.BundlingEnabled ? 50 : 100; } }

        [DataMember]
        public bool Completed { get { return Progress >= Total; } }

        [DataMember]
        public int ProgressPercent { get { return Total == 0 ? 0 : (Progress * Percentage) / Total; } }

        [DataMember]
        public List<string> Error { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public List<string> Bundles { get; set; }

        public StartupProgress()
        {

        }

        public StartupProgress(int total)
        {
            Total = total;
            Error = new List<string>();
        }

        public void Increment(int incrementProgress = 1)
        {
            if (!Completed)
            {
                Progress += incrementProgress;
            }

            if (Completed)
            {
                OnComplete();
            }
        }

        private static string GetBundlePath(Bundle bundle)
        {
            return !string.IsNullOrEmpty(bundle.CdnPath)
                       ? bundle.CdnPath
                       : VirtualPathUtility.ToAbsolute(bundle.Path);
        }

        internal void Complete()
        {
            Progress = Total;
            OnComplete();
        }

        private void OnComplete()
        {
            LogManager.GetLogger("ASC").Debug("Complete");

            Link = VirtualPathUtility.ToAbsolute("~/default.aspx");
            Bundles = BundleTable.Bundles.Select(GetBundlePath).Where(r => !r.Contains("/clientscript/")).ToList();
            WarmUpSettings.SetCompleted(true);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [Serializable]
    [DataContract]
    public sealed class WarmUpSettings : BaseSettings<WarmUpSettings>
    {
        public override Guid ID
        {
            get { return new Guid("5C699566-34B1-4714-AB52-0E82410CE4E5"); }
        }

        [DataMember]
        public bool Completed { get; set; }

        internal static bool GetCompleted()
        {
            return LoadForDefaultTenant().Completed;
        }

        internal static void SetCompleted(bool newValue)
        {
            var settings = LoadForDefaultTenant();
            settings.Completed = newValue;
            settings.SaveForDefaultTenant();
        }

        public override ISettings GetDefault()
        {
            return new WarmUpSettings();
        }
    }
}