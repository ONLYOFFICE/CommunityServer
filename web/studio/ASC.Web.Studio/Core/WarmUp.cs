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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using System.Web.Script.Serialization;
using ASC.Common.Caching;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Web.Core.Client;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;

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
        private int TenantId { get; set; }
        private List<string> Urls { get; set; }
        private readonly object locker = new object();
        private readonly CancellationTokenSource tokenSource;
        private readonly ParallelOptions parallelOptions;
        private readonly ICacheNotify cacheNotify = AscCache.Notify;
        private readonly Dictionary<string, StartupProgress> startupProgresses;
        private StartupProgress progress;
        private bool successInitialized = false;

        internal bool Started { get; private set; }

        private static readonly string instanseId = Process.GetCurrentProcess().Id.ToString();
        private static readonly WarmUp instance = new WarmUp();
        public static WarmUp Instance { get { return instance; } }

        private WarmUp()
        {
            try
            {
                int timeout;

                if (!int.TryParse(ConfigurationManager.AppSettings["web.warmup.timeout"], out timeout))
                {
                    timeout = 15;
                }

                tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(timeout));
                parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = tokenSource.Token
                };

                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;

                int instanceCount;
                if (!int.TryParse(ConfigurationManager.AppSettings["web.warmup.count"], out instanceCount))
                {
                    instanceCount = 1;
                }

                startupProgresses = new Dictionary<string, StartupProgress>(instanceCount);

                Urls = GetUrlsForRequests();

                progress = new StartupProgress(Urls.Count);

                Subscribe();

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
            return startupProgresses.All(pair => pair.Value.Completed);
        }

        public string GetSerializedProgress()
        {
            var combinedProgress = (StartupProgress)progress.Clone();

            var averageProgress = startupProgresses.Average(pair => pair.Value.Progress);
            combinedProgress.Progress = Convert.ToInt32(averageProgress);

            return new JavaScriptSerializer().Serialize(combinedProgress);
        }

        private void Subscribe()
        {
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
                                Publish();
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
        }

        private static List<string> GetUrlsForRequests()
        {
            var result = new List<string>();
            var warmUpType = (WarmUpType)Enum.Parse(typeof(WarmUpType), ConfigurationManager.AppSettings["web.warmup.type"] ?? "none", true);

            if (warmUpType >= WarmUpType.Basic)
            {
                result.AddRange(new List<string>
                                  {
                                      "~/wizard.aspx",
                                      "~/auth.aspx",
                                      "~/confirm.aspx",
                                      "~/default.aspx",
                                      "~/feed.aspx",
                                      "~/my.aspx",
                                      "~/preparationportal.aspx",
                                      "~/search.aspx",
                                      "~/servererror.aspx",
                                      "~/startscriptsstyles.aspx",
                                      "~/tariffs.aspx",
                                      "~/products/files/default.aspx",
                                      "~/products/crm/cases.aspx",
                                      "~/products/crm/deals.aspx",
                                      "~/products/crm/default.aspx",
                                      "~/products/crm/help.aspx",
                                      "~/products/crm/invoices.aspx",
                                      "~/products/crm/mailviewer.aspx",
                                      "~/products/crm/sender.aspx",
                                      "~/products/crm/settings.aspx",
                                      "~/products/crm/tasks.aspx",
                                      "~/products/projects/contacts.aspx",
                                      "~/products/projects/default.aspx",
                                      "~/products/projects/ganttchart.aspx",
                                      "~/products/projects/GeneratedReport.aspx",
                                      "~/products/projects/help.aspx",
                                      "~/products/projects/import.aspx",
                                      "~/products/projects/messages.aspx",
                                      "~/products/projects/milestones.aspx",
                                      "~/products/projects/projects.aspx",
                                      //"~/products/projects/projectteam.aspx",
                                      "~/products/projects/projecttemplates.aspx",
                                      "~/products/projects/reports.aspx",
                                      "~/products/projects/tasks.aspx",
                                      "~/products/projects/timer.aspx",
                                      "~/products/projects/timetracking.aspx",
                                      "~/products/projects/tmdocs.aspx",
                                      "~/products/people/default.aspx",
                                      "~/products/people/help.aspx",
                                      "~/products/people/profile.aspx",
                                      "~/products/people/profileaction.aspx",
                                      "~/addons/mail/default.aspx",
                                      "~/addons/calendar/default.aspx"
                                  });
            }

            if (warmUpType == WarmUpType.Full)
            {
                result.AddRange(new List<string>
                                  {
                                      "~/management.aspx?type=1",
                                      "~/management.aspx?type=2",
                                      "~/management.aspx?type=3",
                                      "~/management.aspx?type=4",
                                      "~/management.aspx?type=5",
                                      "~/management.aspx?type=6",
                                      "~/management.aspx?type=7",
                                      "~/management.aspx?type=10",
                                      "~/management.aspx?type=11",
                                      "~/management.aspx?type=15",
                                      "~/products/community/default.aspx",
                                      "~/products/community/help.aspx",
                                      "~/products/community/modules/birthdays/default.aspx",
                                      "~/products/community/modules/blogs/addblog.aspx",
                                      "~/products/community/modules/blogs/default.aspx",
                                      "~/products/community/modules/blogs/editblog.aspx",
                                      "~/products/community/modules/blogs/viewblog.aspx",
                                      "~/products/community/modules/bookmarking/default.aspx",
                                      "~/products/community/modules/bookmarking/createbookmark.aspx",
                                      "~/products/community/modules/bookmarking/bookmarkinfo.aspx",
                                      "~/products/community/modules/bookmarking/favouritebookmarks.aspx",
                                      "~/products/community/modules/bookmarking/userbookmarks.aspx",
                                      "~/products/community/modules/forum/default.aspx",
                                      "~/products/community/modules/forum/edittopic.aspx",
                                      "~/products/community/modules/forum/managementcenter.aspx",
                                      "~/products/community/modules/forum/newforum.aspx",
                                      "~/products/community/modules/forum/newpost.aspx",
                                      "~/products/community/modules/forum/posts.aspx",
                                      "~/products/community/modules/forum/search.aspx",
                                      "~/products/community/modules/forum/topics.aspx",
                                      "~/products/community/modules/forum/usertopics.aspx",
                                      "~/products/community/modules/news/default.aspx",
                                      "~/products/community/modules/news/editnews.aspx",
                                      "~/products/community/modules/news/editpoll.aspx",
                                      "~/products/community/modules/news/news.aspx",
                                      //"~/products/community/modules/wiki/default.aspx",
                                      "~/products/community/modules/wiki/diff.aspx",
                                      "~/products/community/modules/wiki/listcategories.aspx",
                                      "~/products/community/modules/wiki/listfiles.aspx",
                                      "~/products/community/modules/wiki/listpages.aspx",
                                      //"~/products/community/modules/wiki/pagehistorylist.aspx",
                                  });
            }

            return result.Select(GetFullAbsolutePath).ToList();
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
            if(!successInitialized) return;

            if (!Started)
            {
                Started = true;

                Task.Run(() =>
                {
                    try
                    {
                        Parallel.ForEach(Urls, parallelOptions, MakeRequest);
                    }
                    catch (OperationCanceledException)
                    {
                        Terminate();
                        LogManager.GetLogger("ASC.Web").Error("Warmup canceled");
                    }
                    
                }, tokenSource.Token);
            }
        }

        internal void Restart()
        {
            Started = false;
            WarmUpSettings.SetCompleted(false);
            progress = new StartupProgress(Urls.Count);
            Publish();
            Start();
        }

        internal void Terminate()
        {
            cacheNotify.Publish(GetNotifyKey(), CacheNotifyAction.Remove);
        }

        private void MakeRequest(string requestUrl)
        {
            try
            {
                if(tokenSource.IsCancellationRequested) return;

                CoreContext.TenantManager.SetCurrentTenant(TenantId);
                if (WorkContext.IsMono)
                    ServicePointManager.ServerCertificateValidationCallback = (s, c, h, p) => true;

                var currentUserId = SecurityContext.CurrentAccount.ID;
                if (!SecurityContext.IsAuthenticated)
                    currentUserId = CoreContext.TenantManager.GetCurrentTenant().OwnerId;

                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.4; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36");
                    webClient.Headers.Add("Authorization", SecurityContext.AuthenticateMe(currentUserId));
                    webClient.DownloadData(requestUrl);
                }
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("ASC.Web").Error(string.Format("Page is not avaliable by url {0}", requestUrl), exception);
            }
            finally
            {
                lock (locker)
                {
                    progress.Increment();
                    Publish();
                }
            }
        }

        private void Publish()
        {
            cacheNotify.Publish(GetNotifyKey(), CacheNotifyAction.InsertOrUpdate);
        }

        private KeyValuePair<string, StartupProgress> GetNotifyKey()
        {
            return new KeyValuePair<string, StartupProgress>(instanseId, progress);
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

        public void Increment()
        {
            if (!Completed)
            {
                Progress += 1;
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