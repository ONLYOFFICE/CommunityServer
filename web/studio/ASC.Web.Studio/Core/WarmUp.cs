/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Optimization;
using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;

namespace ASC.Web.Studio.Core
{
    public class WarmUp
    {
        private static readonly WorkerQueue<RequestItem> Requests = new WorkerQueue<RequestItem>(4, TimeSpan.FromSeconds(10), 10, true);

        public static StartupProgress Progress;

        public static void InitRequests()
        {
            var listUrls = new List<RequestItem>
                {
                    new RequestItem("~/auth.aspx"),
                    new RequestItem("~/confirm.aspx"),
                    new RequestItem("~/default.aspx"),
                    new RequestItem("~/feed.aspx"),
                    new RequestItem("~/management.aspx?type=1"),
                    new RequestItem("~/management.aspx?type=2"),
                    new RequestItem("~/management.aspx?type=3"),
                    new RequestItem("~/management.aspx?type=4"),
                    new RequestItem("~/management.aspx?type=5"),
                    new RequestItem("~/management.aspx?type=6"),
                    new RequestItem("~/management.aspx?type=7"),
                    new RequestItem("~/management.aspx?type=10"),
                    new RequestItem("~/management.aspx?type=11"),
                    new RequestItem("~/management.aspx?type=15"),
                    new RequestItem("~/my.aspx"),
                    new RequestItem("~/preparationportal.aspx"),
                    new RequestItem("~/search.aspx"),
                    new RequestItem("~/servererror.aspx"),
                    new RequestItem("~/startscriptsstyles.aspx"),
                    new RequestItem("~/tariffs.aspx"),
                    new RequestItem("~/products/files/default.aspx"),
                    new RequestItem("~/products/files/doceditor.aspx"),
                    new RequestItem("~/products/crm/cases.aspx"),
                    new RequestItem("~/products/crm/deals.aspx"),
                    new RequestItem("~/products/crm/default.aspx"),
                    new RequestItem("~/products/crm/help.aspx"),
                    new RequestItem("~/products/crm/invoices.aspx"),
                    new RequestItem("~/products/crm/mailviewer.aspx"),
                    new RequestItem("~/products/crm/sender.aspx"),
                    new RequestItem("~/products/crm/settings.aspx"),
                    new RequestItem("~/products/crm/tasks.aspx"),
                    new RequestItem("~/products/projects/contacts.aspx"),
                    new RequestItem("~/products/projects/default.aspx"),
                    new RequestItem("~/products/projects/ganttchart.aspx"),
                    new RequestItem("~/products/projects/GeneratedReport.aspx"),
                    new RequestItem("~/products/projects/help.aspx"),
                    new RequestItem("~/products/projects/import.aspx"),
                    new RequestItem("~/products/projects/messages.aspx"),
                    new RequestItem("~/products/projects/milestones.aspx"),
                    new RequestItem("~/products/projects/projects.aspx"),
                    //new RequestItem("~/products/projects/projectteam.aspx"),
                    new RequestItem("~/products/projects/projecttemplates.aspx"),
                    new RequestItem("~/products/projects/reports.aspx"),
                    new RequestItem("~/products/projects/tasks.aspx"),
                    new RequestItem("~/products/projects/timer.aspx"),
                    new RequestItem("~/products/projects/timetracking.aspx"),
                    new RequestItem("~/products/projects/tmdocs.aspx"),
                    new RequestItem("~/products/people/default.aspx"),
                    new RequestItem("~/products/people/help.aspx"),
                    new RequestItem("~/products/people/profile.aspx"),
                    new RequestItem("~/products/people/profileaction.aspx"),
                    new RequestItem("~/addons/mail/default.aspx"),
                    new RequestItem("~/products/community/default.aspx"),
                    new RequestItem("~/products/community/help.aspx"),
                    new RequestItem("~/products/community/modules/birthdays/default.aspx"),
                    new RequestItem("~/products/community/modules/blogs/addblog.aspx"),
                    new RequestItem("~/products/community/modules/blogs/default.aspx"),
                    new RequestItem("~/products/community/modules/blogs/editblog.aspx"),
                    new RequestItem("~/products/community/modules/blogs/viewblog.aspx"),
                    new RequestItem("~/products/community/modules/bookmarking/default.aspx"),
                    new RequestItem("~/products/community/modules/bookmarking/createbookmark.aspx"),
                    new RequestItem("~/products/community/modules/bookmarking/bookmarkinfo.aspx"),
                    new RequestItem("~/products/community/modules/bookmarking/favouritebookmarks.aspx"),
                    new RequestItem("~/products/community/modules/bookmarking/userbookmarks.aspx"),
                    new RequestItem("~/products/community/modules/forum/default.aspx"),
                    new RequestItem("~/products/community/modules/forum/edittopic.aspx"),
                    new RequestItem("~/products/community/modules/forum/managementcenter.aspx"),
                    new RequestItem("~/products/community/modules/forum/newforum.aspx"),
                    new RequestItem("~/products/community/modules/forum/newpost.aspx"),
                    new RequestItem("~/products/community/modules/forum/posts.aspx"),
                    new RequestItem("~/products/community/modules/forum/search.aspx"),
                    new RequestItem("~/products/community/modules/forum/topics.aspx"),
                    new RequestItem("~/products/community/modules/forum/usertopics.aspx"),
                    new RequestItem("~/products/community/modules/news/default.aspx"),
                    new RequestItem("~/products/community/modules/news/editnews.aspx"),
                    new RequestItem("~/products/community/modules/news/editpoll.aspx"),
                    new RequestItem("~/products/community/modules/news/news.aspx"),
                    //new RequestItem("~/products/community/modules/wiki/default.aspx"),
                    new RequestItem("~/products/community/modules/wiki/diff.aspx"),
                    new RequestItem("~/products/community/modules/wiki/listcategories.aspx"),
                    new RequestItem("~/products/community/modules/wiki/listfiles.aspx"),
                    new RequestItem("~/products/community/modules/wiki/listpages.aspx"),
                    //new RequestItem("~/products/community/modules/wiki/pagehistorylist.aspx"),
                    new RequestItem("~/addons/calendar/default.aspx")
                };
            Progress = new StartupProgress { Total = listUrls.Count };
            Requests.Stop();
            Requests.Terminate();
            Requests.AddRange(listUrls);
            Requests.Start(LoaderPortalPages);
        }

        private static void LoaderPortalPages(RequestItem requestItem)
        {
            CoreContext.TenantManager.SetCurrentTenant(requestItem.TenantId);

            var authCookie = SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID);
            var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantDomain;

            try
            {
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(requestItem.Url);
                httpWebRequest.Headers.Add("Authorization", authCookie);
                httpWebRequest.Method = "GET";
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(new Cookie("asc_auth_key", authCookie, "/", tenant));
                httpWebRequest.Timeout = 120000;

                httpWebRequest.GetResponseAsync().ContinueWith(r=> Progress.Increment());
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("ASC.Web")
                    .Error(string.Format("Page is not avaliable by url {0}", requestItem.Url), exception);
            }
            finally
            {
                
            }
        }

        public static void Terminate()
        {
            Requests.Terminate();
        }
    }

    public class RequestItem
    {
        public string Url { get; set; }
        public int TenantId { get; set; }

        public RequestItem(string url)
        {
            Url = CommonLinkUtility.GetFullAbsolutePath(url);
            TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }
    }

    [DataContract]
    public class StartupProgress
    {
        [DataMember]
        public bool IsCompleted { get { return Progress == Total; } }

        public int Progress { get; private set; }

        [DataMember]
        public int ProgressPercent { get { return (Progress * 75) / Total; } }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public List<string> Bundles { get; set; }

        public int Total { get; set; }

        public void Increment()
        {
            Progress += 1;

            if (IsCompleted)
            {
                Link = VirtualPathUtility.ToAbsolute("~/default.aspx");
                Bundles = BundleTable.Bundles.Select(bundle => VirtualPathUtility.ToAbsolute(bundle.Path)).ToList();

                WarmUp.Terminate();
                log4net.LogManager.GetLogger("ASC").Debug("Complete");
            }
        }
    }

    [Serializable]
    [DataContract]
    public class StartupSettings : ISettings
    {
        [DataMember(Name = "Start")]
        public bool Start { get; set; }

        public Guid ID
        {
            get { return new Guid("{098F4CB6-A7EF-4D79-9A0E-73F5846F6B2F}"); }
        }


        public ISettings GetDefault()
        {
            return new StartupSettings();
        }
    }
}