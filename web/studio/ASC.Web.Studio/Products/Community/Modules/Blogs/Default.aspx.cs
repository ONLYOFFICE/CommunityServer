/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Blogs.Core.Security;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.Product;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using System.Globalization;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.Core.Users;

namespace ASC.Web.Community.Blogs
{
    public partial class Default : BasePage
    {
        public long blogsCount { get; set; }

        public int BlogsPageSize
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        #region Properties

        private int SelectedPage
        {
            get
            {
                int result;
                Int32.TryParse(Request.QueryString["page"], out result);
                if (result <= 0)
                    result = 1;
                return result;

            }
        }

        public string GroupID
        {
            get { return Request.QueryString["groupID"]; }
        }

        public string UserID
        {
            get { return Request.QueryString["userID"]; }
        }

        public string TagName
        {
            get { return Request.QueryString["tagName"]; }
        }

        public string Search
        {
            get { return Request.QueryString["search"]; }
        }

        public List<Tuple<Post, int>> PostsAndCommentsCount { get; set; }

        #endregion

        #region Methods

        protected override void PageLoad()
        {
            PostsAndCommentsCount = new List<Tuple<Post, int>>();

            BlogsPageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);
            Guid? userId = null;
            if (!String.IsNullOrEmpty(UserID))
            {
                userId = Guid.NewGuid();
                try
                {
                    userId = new Guid(UserID);
                }
                catch
                {
                }
            }

            var postsQuery = new PostsQuery();

            if (userId.HasValue)
            {
                postsQuery.SetUser(userId.Value);
            }
            else if (!String.IsNullOrEmpty(TagName))
            {
                postsQuery.SetTag(TagName);
            }
            else if (!String.IsNullOrEmpty(Search))
            {
                postsQuery.SetSearch(Search);
            }

            if (!IsPostBack)
            {
                var engine = GetEngine();
                FillPosts(postsQuery, engine);
            }

            Title = HeaderStringHelper.GetPageTitle(BlogsResource.AddonName);

            var jsResource = new StringBuilder();
            jsResource.Append(String.Format("ASC.Community.BlogsJSResource = {{}};ASC.Community.BlogsJSResource.ReadMoreLink = \"{0}\";", BlogsResource.ReadMoreLink));
            jsResource.Append("jq('#tableForNavigation select').val(" + BlogsPageSize + ").change(function(evt) {changeBlogsCountOfRows(this.value);}).tlCombobox();");
            Page.RegisterInlineScript(jsResource.ToString(), true);
        }

        protected string QueryString(string excludeParamList)
        {
            var queryString = "&" + Request.QueryString.ToString();

            foreach (var excludeParamName in excludeParamList.Split(','))
            {
                var startPos = queryString.IndexOf("&" + excludeParamName + "=");
                if (startPos != -1)
                {
                    var endPos = queryString.IndexOf("&", startPos + 1);

                    if (endPos == -1)
                    {
                        queryString = queryString.Remove(startPos, queryString.Length - startPos);
                    }
                    else
                    {
                        queryString = queryString.Remove(startPos, endPos - startPos);
                    }
                }
            }
            return queryString.Trim('&');
        }


        private void FillPosts(PostsQuery query, BlogsEngine engine)
        {
            query
                .SetOffset((SelectedPage - 1)*BlogsPageSize)
                .SetCount(BlogsPageSize);

            SetTotalPostsCount(engine.GetPostsCount(query));
            var posts = engine.SelectPosts(query);
            FillSelectedPage(posts, engine);
        }

        private void FillSelectedPage(List<Post> posts, BlogsEngine engine)
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (posts == null || posts.Count == 0)
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("blog_icon.png", ASC.Blogs.Core.Constants.ModuleId),
                        Header = BlogsResource.EmptyScreenBlogCaption,
                        Describe = BlogsResource.EmptyScreenBlogText
                    };

                if (CommunitySecurity.CheckPermissions(new PersonalBlogSecObject(currentUser), ASC.Blogs.Core.Constants.Action_AddPost)
                    && string.IsNullOrEmpty(UserID) && string.IsNullOrEmpty(Search))
                {
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='addblog.aspx'>{0}</a>", BlogsResource.EmptyScreenBlogLink);
                }

                placeContent.Controls.Add(emptyScreenControl);
                return;
            }


            PostsAndCommentsCount = engine.GetPostsCommentsCount(posts);
        }

        #endregion

        private void SetTotalPostsCount(int count)
        {
            var pageNavigator = new PageNavigator
                {
                    PageUrl = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}?{1}",
                        VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/"),
                        QueryString("page")
                        //BlogsPageSize
                        ),
                    //"./" + "?" + QueryString("page"),
                    CurrentPageNumber = SelectedPage,
                    EntryCountOnPage = BlogsPageSize,
                    VisiblePageCount = 5,
                    ParamName = "page",
                    EntryCount = count
                };

            blogsCount = count;
            pageNavigatorHolder.Controls.Add(pageNavigator);
        }
    }
}