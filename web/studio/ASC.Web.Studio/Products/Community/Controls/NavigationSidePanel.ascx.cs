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
using System.Web;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Community.Product;
using ASC.Core;
using ASC.Web.Studio.Core;
using System.Text;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using Newtonsoft.Json.Linq;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.InviteLink;

namespace ASC.Web.Community.Controls
{
    public partial class NavigationSidePanel : System.Web.UI.UserControl
    {
        public static String Location = VirtualPathUtility.ToAbsolute("~/Products/Community/Controls/NavigationSidePanel.ascx");

        protected string CurrentPage { get; set; }

        protected bool IsInBlogs { get; set; }
        protected bool IsInEvents { get; set; }
        protected bool IsInSettings { get; set; }
        protected bool IsInBookmarks { get; set; }
        protected bool IsInWiki { get; set; }

        protected bool IsBlogsAvailable { get; set; }
        protected bool IsEventsAvailable { get; set; }
        protected bool IsForumsAvailable { get; set; }
        protected bool IsBookmarksAvailable { get; set; }
        protected bool IsWikiAvailable { get; set; }
        protected bool IsBirthdaysAvailable { get; set; }

        protected bool IsAdmin { get; set; }
        protected bool IsVisitor { get; set; }
        protected bool IsFullAdministrator { get; set; }

        protected int TopicID
        {
            get
            {
                int result;
                return int.TryParse(Request["t"], out result) ? result : 0;
            }
        }

        protected int ForumID
        {
            get
            {
                int result;
                return int.TryParse(Request["f"], out result) ? result : 0;
            }
        }

        protected bool InAParticularTopic { get; set; }

        protected bool MakeCreateNewTopic { get; set; }

        protected bool ForumsHasThreadCategories { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            IsAdmin = WebItemSecurity.IsProductAdministrator(CommunityProduct.ID, SecurityContext.CurrentAccount.ID);
            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();
            IsFullAdministrator = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();

            InitCurrentPage();
            InitPermission();
            InitModulesState();

            var help = (HelpCenter) LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void InitPermission()
        {           
            foreach (var module in WebItemManager.Instance.GetSubItems(CommunityProduct.ID))
            {
                switch (module.GetSysName())
                {
                    case "community-blogs":
                        IsBlogsAvailable = true;
                        break;
                    case "community-news":
                        IsEventsAvailable = true;
                        break;
                    case "community-forum":
                        InitForumsData();
                        break;
                    case "community-bookmarking":
                        IsBookmarksAvailable = true;
                        break;
                    case "community-wiki":
                        IsWikiAvailable = true;
                        break;
                    case "community-birthdays":
                        IsBirthdaysAvailable = true;
                        break;
                }
            }
        }

        private void InitCurrentPage()
        {
            var currentPath = HttpContext.Current.Request.Path;
            if (currentPath.IndexOf("Modules/Blogs", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "blogs";
                if (currentPath.IndexOf("AllBlogs.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "allblogs";
                }
            }
            else if (currentPath.IndexOf("Modules/News", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "events";
                if (currentPath.IndexOf("EditPoll.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                }
                else if (currentPath.IndexOf("EditNews.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                }
                else
                {
                    var type = Request["type"];
                    if (!string.IsNullOrEmpty(type))
                    {
                        switch (type)
                        {
                            case "News":
                                CurrentPage = "news";
                                break;
                            case "Order":
                                CurrentPage = "order";
                                break;
                            case "Advert":
                                CurrentPage = "advert";
                                break;
                            case "Poll":
                                CurrentPage = "poll";
                                break;
                        }
                    }
                }
            }
            else if (currentPath.IndexOf("Modules/Forum", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "forum";
                if (currentPath.IndexOf("ManagementCenter.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "forumeditor";
                }
                if (currentPath.IndexOf("Posts.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    InAParticularTopic = true;
                    MakeCreateNewTopic = true;
                }
            }
            else if (currentPath.IndexOf("Modules/Bookmarking", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "bookmarking";              
                if (currentPath.IndexOf("FavouriteBookmarks.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "bookmarkingfavourite";
                }
            }
            else if (currentPath.IndexOf("Modules/Wiki", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "wiki";
                if (currentPath.IndexOf("ListCategories.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "wikicategories";
                }
                if (currentPath.IndexOf("ListPages.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "wikiindex";
                    var type = Request["n"];
                    if (type != null)
                    {
                        CurrentPage = "wikinew";
                    }
                    type = Request["f"];
                    if (type != null)
                    {
                        CurrentPage = "wikirecently";
                    }
                }
                if (currentPath.IndexOf("ListFiles.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    CurrentPage = "wikifiles";
                }
                var page = Request["page"];
                if (!string.IsNullOrEmpty(page))
                {
                    if(page.StartsWith("Category:"))
                        CurrentPage = "wikicategories";

                    if (page == UserControls.Wiki.Resources.WikiUCResource.HelpPageCaption)
                        CurrentPage = "wikihelp";
                }
            }
            else if (currentPath.IndexOf("Modules/Birthdays", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "birthdays";
            }
            else if (currentPath.IndexOf("Help.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "help";
            }
            else
            {
                CurrentPage = "blogs";
            }
        }

        private void InitModulesState()
        {
            IsInBlogs = CurrentPage == "allblogs";

            IsInEvents = CurrentPage == "news" ||
                CurrentPage == "order" ||
                CurrentPage == "advert" ||
                CurrentPage == "poll";

            IsInSettings = CurrentPage == "forumeditor";

            IsInBookmarks = CurrentPage == "bookmarkingfavourite";

            IsInWiki = CurrentPage == "wikicategories" ||
                CurrentPage == "wikiindex" ||
                CurrentPage == "wikinew" ||
                CurrentPage == "wikirecently" ||
                CurrentPage == "wikifiles" ||
                CurrentPage == "wikihelp";
        }

        private void InitForumsData()
        {
            IsForumsAvailable = true;

            var apiServer = new Api.ApiServer();
            var apiResponse = apiServer.GetApiResponse(String.Format("{0}community/forum/count.json", SetupInfo.WebApiBaseUrl), "GET");
            var obj = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse)));
            var count = 0;
            if (Int32.TryParse(obj["response"].ToString(), out count))
                ForumsHasThreadCategories = count > 0;

            if (InAParticularTopic && TopicID > 0)
            {
                apiServer = new Api.ApiServer();
                apiResponse = apiServer.GetApiResponse(String.Format("{0}community/forum/topic/{1}.json", SetupInfo.WebApiBaseUrl, TopicID), "GET");
                obj = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse)));
                if(obj["response"]!=null)
                {
                    obj = JObject.Parse(obj["response"].ToString());
                    var status = 0;
                    if (Int32.TryParse(obj["status"].ToString(), out status))
                        MakeCreateNewTopic = status != 1 && status != 3;
                }
            }
        }

        protected string GetDefaultSettingsPageUrl()
        {
            var defaultUrl = VirtualPathUtility.ToAbsolute("~/Management.aspx") + "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights +"#community";
            if (IsForumsAvailable)
            {
                defaultUrl = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/ManagementCenter.aspx");
            }
            return defaultUrl;
        }
    }
}
