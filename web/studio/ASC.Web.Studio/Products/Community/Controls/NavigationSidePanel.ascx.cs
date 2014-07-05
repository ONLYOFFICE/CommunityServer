/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Usings

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

#endregion

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

            InitCurrentPage();
            InitPermission();
            InitModulesState();

            var help = (HelpCenter) LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
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
            var currentPath = HttpContext.Current.Request.Path.ToLower();
            if (currentPath.IndexOf("modules/blogs", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "blogs";
                if (currentPath.IndexOf("allblogs.aspx", StringComparison.Ordinal) > 0)
                {
                    CurrentPage = "allblogs";
                }
            }
            else if (currentPath.IndexOf("modules/news", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "events";
                if (currentPath.IndexOf("editpoll.aspx", StringComparison.Ordinal) > 0)
                {
                }
                else if (currentPath.IndexOf("editnews.aspx", StringComparison.Ordinal) > 0)
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
            else if (currentPath.IndexOf("modules/forum", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "forum";
                if (currentPath.IndexOf("managementcenter.aspx", StringComparison.Ordinal) > 0)
                {
                    CurrentPage = "forumeditor";
                }
                if (currentPath.IndexOf("posts.aspx", StringComparison.Ordinal) > 0)
                {
                    InAParticularTopic = true;
                    MakeCreateNewTopic = true;
                }
            }
            else if (currentPath.IndexOf("modules/bookmarking", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "bookmarking";              
                if (currentPath.IndexOf("favouritebookmarks.aspx", StringComparison.Ordinal) > 0)
                {
                    CurrentPage = "bookmarkingfavourite";
                }
            }
            else if (currentPath.IndexOf("modules/wiki", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "wiki";
                if (currentPath.IndexOf("listcategories.aspx", StringComparison.Ordinal) > 0)
                {
                    CurrentPage = "wikicategories";
                }
                if (currentPath.IndexOf("listpages.aspx", StringComparison.Ordinal) > 0)
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
                if (currentPath.IndexOf("listfiles.aspx", StringComparison.Ordinal) > 0)
                {
                    CurrentPage = "wikifiles";
                }
                var page = Request["page"];
                if (!string.IsNullOrEmpty(page) && page.StartsWith("Category:"))
                {
                    CurrentPage = "wikicategories";
                }
            }
            else if (currentPath.IndexOf("modules/birthdays", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "birthdays";
            }
            else if (currentPath.IndexOf("help.aspx", StringComparison.Ordinal) > 0)
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
            var defaultUrl = VirtualPathUtility.ToAbsolute("~/management.aspx") + "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights +"#community";
            if (IsForumsAvailable)
            {
                defaultUrl = VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/managementcenter.aspx");
            }
            return defaultUrl;
        }
    }
}
