/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Web.Optimization;

using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Web.Core.Client;
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
        private readonly object obj = new object();
        private readonly int tenantId;

        private readonly List<string> urls;
        private WorkerQueue<string> requests;

        private static WarmUp instance;

        public bool Started { get; private set; }
        public bool Completed { get { return Progress.Completed; } }
        public StartupProgress Progress { get; private set; }
        public static WarmUp Instance { get { return instance ?? (instance = new WarmUp()); } }

        public WarmUp()
        {
            tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            
            urls = GetUrlsForRequests();

            Progress = new StartupProgress(urls.Count());
        }

        private static List<string> GetUrlsForRequests()
        {
            var result = new List<string>();
            var warmUpType = (WarmUpType)Enum.Parse(typeof(WarmUpType), ConfigurationManager.AppSettings["web.warmup.type"] ?? "none", true);

            if (warmUpType >= WarmUpType.Basic)
            {
                result.AddRange(new List<string>
                                  {
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
                                      "~/products/files/doceditor.aspx",
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

            return result.Select(TransformUrl).ToList();
        }

        private static string TransformUrl(string url)
        {
            var result = CommonLinkUtility.GetFullAbsolutePath(url);
            const string warmupParam = "warmup=true";

            return url.Contains("?")
                       ? string.Format("{0}&{1}", result, warmupParam)
                       : string.Format("{0}?{1}", result, warmupParam);
        }

        public void Start()
        {
            if (!Started)
            {
                Started = true;
                requests = new WorkerQueue<string>(4, TimeSpan.Zero, 10, true);
                requests.AddRange(urls);
                requests.Start(LoaderPortalPages);
            }
        }

        public void StartSync()
        {
            if (!Started)
            {
                Started = true;
                urls.ForEach(LoaderPortalPages);
                Progress.Bundles.ForEach(r => MakeRequest(CommonLinkUtility.GetFullAbsolutePath(r)));
            }
        }

        public void Terminate()
        {
            try
            {
                if (requests != null)
                {
                    requests.Terminate();
                    requests = null;
                }
            }
            catch (ThreadAbortException)
            {
            }

        }

        private void LoaderPortalPages(string requestUrl)
        {
            try
            {
                MakeRequest(requestUrl);
            }
            catch (Exception exception)
            {
                lock (obj)
                {
                    Progress.Error.Add(exception.StackTrace);
                }

                LogManager.GetLogger("ASC.Web")
                    .Error(string.Format("Page is not avaliable by url {0}", requestUrl), exception);
            }
            finally
            {
                lock (obj)
                {
                    Progress.Increment();
                }
            }
        }

        private void MakeRequest(string requestUrl)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenantId);
            if (WorkContext.IsMono)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, h, p) => true;

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.4; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36");
                webClient.Headers.Add("Authorization", SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID));
                webClient.DownloadData(requestUrl);
            }
        }
    }

    [DataContract]
    public class StartupProgress
    {
        private int progress;
        private readonly int total;

        [DataMember]
        public int Percentage { get { return ClientSettings.BundlingEnabled ? 50 : 100; } }

        [DataMember]
        public bool Completed { get { return progress == total; } }

        [DataMember]
        public int ProgressPercent { get { return (progress * Percentage) / total; } }

        [DataMember]
        public List<string> Error { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public List<string> Bundles { get; set; }

        public StartupProgress(int total)
        {
            this.total = total;
            Error = new List<string>();
        }

        public void Increment()
        {
            progress += 1;

            if (Completed)
            {
                LogManager.GetLogger("ASC").Debug("Complete");

                Link = VirtualPathUtility.ToAbsolute("~/default.aspx");
                Bundles = BundleTable.Bundles.Select(GetBundlePath).ToList();

                WarmUp.Instance.Terminate();
            }
        }

        private static string GetBundlePath(Bundle bundle)
        {
            return !string.IsNullOrEmpty(bundle.CdnPath)
                       ? bundle.CdnPath
                       : VirtualPathUtility.ToAbsolute(bundle.Path);
        }
    }
}