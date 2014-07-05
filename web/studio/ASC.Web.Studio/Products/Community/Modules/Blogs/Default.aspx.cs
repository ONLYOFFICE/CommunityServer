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

        #endregion

        #region Methods

        protected override void PageLoad()
        {
            

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

            Title = HeaderStringHelper.GetPageTitle(mainContainer.CurrentPageCaption ?? BlogsResource.AddonName);

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
            if (posts == null || posts.Count == 0)
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("blog_icon.png", ASC.Blogs.Core.Constants.ModuleId),
                        Header = BlogsResource.EmptyScreenBlogCaption,
                        Describe = BlogsResource.EmptyScreenBlogText
                    };

                if (CommunitySecurity.CheckPermissions(new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID)), ASC.Blogs.Core.Constants.Action_AddPost)
                    && string.IsNullOrEmpty(UserID) && string.IsNullOrEmpty(Search))
                {
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='addblog.aspx'>{0}</a>", BlogsResource.EmptyScreenBlogLink);
                }

                placeContent.Controls.Add(emptyScreenControl);
                return;
            }

            placeContent.Controls.Add(new Literal {Text = "<div>"});

            var post_with_comments = engine.GetPostsCommentsCount(posts);

            if (!String.IsNullOrEmpty(UserID))
            {

                var post = post_with_comments[0].Item1;
                var user = CoreContext.UserManager.GetUsers(post.UserID);

                var st = new StringBuilder();
                st.Append("<div class=\"BlogsHeaderBlock header-with-menu\" style=\"margin-bottom:16px;\">");
                st.Append("<span class=\"header\">" + CoreContext.UserManager.GetUsers(user.ID).DisplayUserName() + "</span>");
                st.Append("</div>");

                placeContent.Controls.Add(new Literal {Text = st.ToString()});
            }

            for (var i = 0; i < post_with_comments.Count; i++)
            {
                var post = post_with_comments[i].Item1;
                var commentCount = post_with_comments[i].Item2;

                var sb = new StringBuilder();
                var user = CoreContext.UserManager.GetUsers(post.UserID);

                sb.Append("<div class=\"container-list\">");
                sb.Append("<div class=\"header-list\">");

                sb.Append("<div class=\"avatar-list\">");
                sb.Append("<a href=\"viewblog.aspx?blogid=" + post.ID.ToString() + "\">" + ImageHTMLHelper.GetHTMLUserAvatar(post.UserID) + "</a>");
                sb.Append("</div><div class=\"describe-list\">");
                sb.Append("<div class=\"title-list\">");
                sb.Append("<a href=\"viewblog.aspx?blogid=" + post.ID.ToString() + "\">" + HttpUtility.HtmlEncode(post.Title) + "</a>");
                sb.Append("</div>");

                sb.Append("<div class=\"info-list\">");
                sb.Append("<span class=\"caption-list\">" + BlogsResource.PostedTitle + ":</span>");
                sb.Append(CoreContext.UserManager.GetUsers(user.ID).RenderCustomProfileLink(CommunityProduct.ID, "name-list", "link"));
                sb.Append("</div>");
                if (String.IsNullOrEmpty(UserID))
                {
                    sb.Append("<div class=\"info-list\">");
                    sb.Append("<a class=\"link gray-text\" href=\"" + VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath) + "?userid=" + post.UserID + "\">" + BlogsResource.AllRecordsOfTheAutor + "</a>");
                    sb.Append("</div>");
                }

                sb.Append("<div class=\"date-list\">");
                sb.AppendFormat("{0}<span class=\"time-list\">{1}</span>", post.Datetime.ToString("d"), post.Datetime.ToString("t"));
                sb.Append("</div></div></div>");

                sb.Append("<div class=\"content-list\">");
               
                sb.Append(HtmlUtility.GetFull(post.Content, false));
                sb.Append("<div id=\"postIndividualLink\" class=\"display-none\">viewblog.aspx?blogid=" + post.ID.ToString() + "</div>");               
                sb.Append("<div class=\"comment-list\">");
                sb.Append("<a href=\"viewblog.aspx?blogid=" + post.ID + "#comments\">" + BlogsResource.CommentsTitle + ": " + commentCount.ToString() + "</a>");
                sb.Append("<a href=\"viewblog.aspx?blogid=" + post.ID + "#addcomment\">" + BlogsResource.CommentsAddButtonTitle + "</a>");
                sb.Append("</div></div></div>");

                placeContent.Controls.Add(new Literal {Text = sb.ToString()});
            }

            placeContent.Controls.Add(new Literal {Text = "</div>"});
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