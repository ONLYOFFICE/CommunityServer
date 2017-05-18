/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using System.Web.Script.Serialization;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core.Client;
using ASC.Web.Studio.Utility;
using log4net;
using Timer = System.Timers.Timer;

namespace ASC.Web.Studio.Core
{
    enum WarmUpType
    {
        None,
        Basic,
        Full
    }

    public class WarmUpController
    {
        private int TenantId { get; set; }
        private List<string> Pages { get; set; }
        private string WarmUpUrl { get; set; }
        private readonly CancellationTokenSource tokenSource;
        private readonly int timeout;
        private readonly ICacheNotify cacheNotify = AscCache.Notify;
        private readonly Dictionary<string, StartupProgress> startupProgresses;
        private readonly ILog logger;
        private StartupProgress progress;
        private readonly bool successInitialized = false;
        private readonly PerformanceCounter cpuCounter;
        internal bool Started { get; private set; }
        private Timer Timer { get; set; }

        private static readonly string InstanseId = Process.GetCurrentProcess().Id.ToString();
        private static readonly WarmUpController instance = new WarmUpController();
        private static int instanceCount = 1;
        public static WarmUpController Instance { get { return instance; } }

        private WarmUpController()
        {
            try
            {
                logger = LogManager.GetLogger("ASC");

                if (!int.TryParse(ConfigurationManager.AppSettings["web.warmup.timeout"], out timeout))
                {
                    timeout = 15;
                }

                tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(timeout));
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;

                if (!int.TryParse(ConfigurationManager.AppSettings["web.warmup.count"], out instanceCount))
                {
                    instanceCount = 1;
                }

                bool enabled;
                if (!bool.TryParse(ConfigurationManager.AppSettings["web.warmup.enabled"] ?? "false", out enabled))
                {
                    enabled = false;
                }
                try
                {
                    cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                }
                catch (Exception e)
                {
                    logger.Error("new PerformanceCounter error", e);
                }

                startupProgresses = new Dictionary<string, StartupProgress>(instanceCount);

                Pages = GetPagesForWarmup();
                WarmUpUrl = GetFullAbsolutePath("~/Warmup.aspx");
                progress = new StartupProgress(Pages.Count);

                if (CoreContext.Configuration.Standalone)
                {
                    Timer = new Timer();
                    Timer.Elapsed += Timer_Elapsed;
                    Timer.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
                }

                cacheNotify.Subscribe<KeyValuePair<string, StartupProgress>>(
                    (kv, action) =>
                    {
                        switch (action)
                        {
                            case CacheNotifyAction.Remove:
                                tokenSource.Cancel();

                                progress.Complete();

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
                successInitialized = enabled;
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web").Error("Warmup error", e);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var page in Pages.Take(7))
            {
                try
                {
                    MakeRequest(GetFullAbsolutePath(page));
                }
                catch (Exception ex)
                {
                    logger.Error("Timer Warmup Page:" + page, ex);
                }
            }
        }

        public bool CheckCompleted()
        {
            if (!successInitialized)
            {
                return true;
            }

            return startupProgresses.All(pair => pair.Value.Completed);
        }

        public string GetSerializedProgress()
        {
            var combinedProgress = (StartupProgress)progress.Clone();

            var averageProgress = startupProgresses.Average(pair => pair.Value.Progress);
            combinedProgress.Progress = Convert.ToInt32(averageProgress);

            return new JavaScriptSerializer().Serialize(combinedProgress);
        }

        public void Execute()
        {
            if (progress.Completed) return;

            foreach (var page in Pages)
            {
                logger.DebugFormat("Warmup Page:{0}", page);
                try
                {
                    CheckCPU(page);
                    HttpContext.Current.Server.Execute(page);
                }
                catch (ThreadAbortException e)
                {
                    Terminate();
                    logger.Error("ThreadAbortException Warmup Page:" + page, e);
                }
                catch (Exception e)
                {
                    logger.Error("Warmup Page:" + page, e);
                }
                finally
                {
                    progress.Increment();
                    Publish();
                }
            }
        }

        private static List<string> GetPagesForWarmup()
        {
            return new List<string>
            {
                "Products/Files/default.aspx",
                "Products/Projects/Tasks.aspx",
                "Products/CRM/Default.aspx",
                "Addons/mail/default.aspx",
                "Addons/calendar/default.aspx",
                "Products/People/Default.aspx",
                "Default.aspx",

                "Products/Projects/GanttChart.aspx",
                "Products/Projects/Default.aspx",
                "Products/Projects/Tasks.aspx",
                "Products/Projects/Messages.aspx",
                "Products/Projects/Milestones.aspx",
                "Products/Projects/Projects.aspx",
                "Products/Projects/Contacts.aspx",
                "Products/Projects/Reports.aspx?reportType=0",
                "Products/Projects/GeneratedReport.aspx",
                "Products/Projects/Help.aspx",
                "Products/Projects/Timetracking.aspx",
                "Products/Projects/TMDocs.aspx",
                "Products/CRM/Cases.aspx",
                "Products/CRM/Deals.aspx",
                "Products/CRM/Help.aspx",
                "Products/CRM/Invoices.aspx",
                "Products/CRM/Settings.aspx",
                "Products/CRM/Tasks.aspx",
                "Products/People/Help.aspx",
                "Products/People/Profile.aspx",
                "Products/People/ProfileAction.aspx",
                "Feed.aspx",
                "My.aspx",
                "Preparationportal.aspx",
                "Search.aspx",
                "ServerError.aspx",
                "StartScriptsStyles.aspx",
                //"Tariffs.aspx",
                "Management.aspx?type=1",
                "Management.aspx?type=2",
                "Management.aspx?type=3",
                "Management.aspx?type=4",
                "Management.aspx?type=5",
                "Management.aspx?type=6",
                "Management.aspx?type=7",
                "Management.aspx?type=10",
                "Management.aspx?type=11",
                "Management.aspx?type=15"
            }.ToList();
        }

        private static string GetFullAbsolutePath(string virtualPath)
        {
            string result;
            const string warmupParam = "warmup=true";

            if (CoreContext.Configuration.Standalone)
            {
                var domain = ConfigurationManager.AppSettings["web.warmup.domain"] ?? "localhost";
                result = "http://" + domain + "/" + virtualPath.TrimStart('~', '/');
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
            if (!successInitialized)
            {
                Terminate();
                return;
            }

            if (!Started)
            {
                Started = true;
                try
                {
                    progress = new StartupProgress(Pages.Count);
                    Task.Run(() =>
                    {
                        MakeRequest(WarmUpUrl);
                        if (Timer != null)
                        {
                            Timer.Start();
                        }
                    }, tokenSource.Token);
                }
                catch (Exception ex)
                {
                    logger.Error("Error", ex);
                    Terminate();
                }
            }
        }

        internal void Restart()
        {
            Started = false;
            WarmUpSettings.SetCompleted(false);
            progress = new StartupProgress(Pages.Count);

            Publish();
            Start();
        }

        internal void Terminate()
        {
            cacheNotify.Publish(GetNotifyKey(), CacheNotifyAction.Remove);
            if (Timer != null)
            {
                Timer.Stop();
            }
        }

        private void MakeRequest(string url)
        {
            try
            {
                if (WorkContext.IsMono)
                    ServicePointManager.ServerCertificateValidationCallback = (s, c, h, p) => true;

                CoreContext.TenantManager.SetCurrentTenant(TenantId);
                var auth = SecurityContext.AuthenticateMe(CoreContext.TenantManager.GetCurrentTenant().OwnerId);

                using (var webClient = new MyWebClient(timeout))
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.4; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36");
                    webClient.Headers.Add("Authorization", auth);
                    webClient.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error", ex);
                Terminate();
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

        private void CheckCPU(string page)
        {
            if (cpuCounter == null) return;

            var tries = 5;
            while (tries-- > 0)
            {
                var counter = cpuCounter.NextValue();
                logger.DebugFormat("Page: {0}, CPU:{1}%", page, counter);
                if (counter > 80)
                {
                    Thread.Sleep(10000);
                }
                else
                {
                    return;
                }
            }
        }

        private class MyWebClient : WebClient
        {
            private readonly int timeout;

            public MyWebClient(int timeout)
            {
                this.timeout = timeout * 60 * 1000;
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                w.Timeout = timeout;
                return w;
            }
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
        public int Percentage { get { return ClientSettings.BundlingEnabled ? 50 : 100; } }

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
            var logger = LogManager.GetLogger("ASC");

            if (!Completed)
            {
                Progress += incrementProgress;
            }
            logger.DebugFormat("Progress: {0}, Total:{1}", Progress, Total);
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
    public sealed class WarmUpSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("5C699566-34B1-4714-AB52-0E82410CE4E5"); }
        }

        [DataMember]
        public bool Completed { get; set; }

        internal static bool GetCompleted()
        {
            return SettingsManager.Instance.LoadSettings<WarmUpSettings>(TenantProvider.CurrentTenantID).Completed;
        }

        internal static void SetCompleted(bool newValue)
        {
            var settings = SettingsManager.Instance.LoadSettings<WarmUpSettings>(TenantProvider.CurrentTenantID);
            settings.Completed = newValue;
            SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
        }

        public ISettings GetDefault()
        {
            return new WarmUpSettings();
        }
    }
}