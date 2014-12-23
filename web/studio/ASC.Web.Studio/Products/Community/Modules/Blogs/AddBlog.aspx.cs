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

using AjaxPro;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Blogs.Core.Security;
using ASC.Core;
using ASC.Web.Community.Blogs.Views;
using ASC.Web.Community.Product;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("BlogsPage")]
    public partial class AddBlog : BasePage
    {
        protected string _text = "";

        protected override void PageLoad()
        {
            if (!CommunitySecurity.CheckPermissions(Constants.Action_AddPost))
                Response.Redirect(Constants.DefaultPageUrl, true);
            
            if (CheckTitle(txtTitle.Text))
            {
                mainContainer.Options.InfoMessageText = "";
            }

            mainContainer.CurrentPageCaption = BlogsResource.NewPost;
            Title = HeaderStringHelper.GetPageTitle(BlogsResource.NewPost);

            InitPreviewTemplate();

            if (IsPostBack)
            {
                var control = FindControl(Request.Params["__EVENTTARGET"]);
                if (lbCancel.Equals(control))
                {
                    Response.Redirect(Constants.DefaultPageUrl);
                }
                else
                {
                    TryPostBlog(GetEngine());
                }
            }

            InitScript();
        }

        private void InitScript()
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor-connector.js"));

            //Page.RegisterInlineScript("ckeditorConnector.onReady(function () {BlogsManager.blogsEditor = jq('#ckEditor').ckeditor({ toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + @"'}).editor;});");
            Page.RegisterInlineScript("ckeditorConnector.onReady(function () {" +
                                      "BlogsManager.blogsEditor = CKEDITOR.replace('ckEditor', { toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + "'});" +
                                      "BlogsManager.blogsEditor.on('change',  function() {if (this.getData() == '') {jq('#btnPreview').addClass('disable');} else {jq('#btnPreview').removeClass('disable');}});"+
                                       "});");
        }

        private static bool CheckTitle(string title)
        {
            return !string.IsNullOrEmpty(title.Trim());
        }

        private static bool IsExistsTagName(Post post, string tagName)
        {
            return post.TagList.Any(tag => tag.Content == tagName);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse GetSuggest(string text, string varName, int limit)
        {
            var resp = new AjaxResponse();

            var startSymbols = text;
            var ind = startSymbols.LastIndexOf(",");
            if (ind != -1)
                startSymbols = startSymbols.Substring(ind + 1);

            startSymbols = startSymbols.Trim();

            var engine = GetEngine();

            var tags = new List<string>();

            if (!string.IsNullOrEmpty(startSymbols))
            {
                tags = engine.GetTags(startSymbols, limit);
            }

            var resNames = new StringBuilder();
            var resHelps = new StringBuilder();

            foreach (var tag in tags)
            {
                resNames.Append(tag);
                resNames.Append("$");
                resHelps.Append(tag);
                resHelps.Append("$");
            }
            resp.rs1 = resNames.ToString().TrimEnd('$');
            resp.rs2 = resHelps.ToString().TrimEnd('$');
            resp.rs3 = text;
            resp.rs4 = varName;

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string[] GetPreview(string title, string html)
        {
            var result = new string[2];

            result[0] = HttpUtility.HtmlEncode(title);
            result[1] = HtmlUtility.GetFull(html);

            return result;
        }

        private Post AddNewBlog(BlogsEngine engine)
        {
            var authorId = SecurityContext.CurrentAccount.ID;

            if (CommunitySecurity.CheckPermissions(
                new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(authorId)),
                Constants.Action_AddPost))
            {
                var newPost = new Post
                    {
                        Content = (Request["blog_text"] ?? "")
                    };

                var dateNow = ASC.Core.Tenants.TenantUtil.DateTimeNow();
                
                newPost.Datetime = dateNow;
                newPost.Title = GetLimitedText(txtTitle.Text);
                newPost.UserID = authorId;

                newPost.TagList = new List<Tag>();
                foreach (var tagName in txtTags.Text.Split(','))
                {
                    if (tagName == string.Empty || IsExistsTagName(newPost, tagName))
                        continue;

                    var tag = new Tag
                        {
                            Content = GetLimitedText(tagName.Trim())
                        };
                    newPost.TagList.Add(tag);
                }
                engine.SavePost(newPost, true, Request.Form["notify_comments"] == "on");

                CommonControlsConfigurer.FCKEditingComplete("blogs", newPost.ID.ToString(), newPost.Content, false);

                return newPost;
            }

            Response.Redirect("addblog.aspx");
            return null;
        }

        private void TryPostBlog(BlogsEngine engine)
        {
            if (CheckTitle(txtTitle.Text))
            {
                var post = AddNewBlog(engine);

                if (post != null)
                    Response.Redirect("viewblog.aspx?blogid=" + post.ID.ToString());
                else
                    Response.Redirect(Constants.DefaultPageUrl);
            }
            else
            {
                mainContainer.Options.InfoMessageText = BlogsResource.BlogTitleEmptyMessage;
                mainContainer.Options.InfoType = InfoType.Alert;
            }
        }

        private void InitPreviewTemplate()
        {
            var post = new Post
                {
                    Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    Title = string.Empty,
                    Content = string.Empty,
                    UserID = SecurityContext.CurrentAccount.ID
                };

            var control = (ViewBlogView)LoadControl("~/products/community/modules/blogs/views/viewblogview.ascx");
            control.IsPreview = true;
            control.post = post;

            PlaceHolderPreview.Controls.Add(new Literal { Text = "<div class='headerPanel' style='margin-top:25px;'>" + BlogsResource.PreviewButton + "</div>" });
            PlaceHolderPreview.Controls.Add(control);
            PlaceHolderPreview.Controls.Add(new Literal { Text = "<div style='margin-top:20px;'><a class='button blue big' href='javascript:void(0);' onclick='BlogsManager.HidePreview(); return false;'>" + BlogsResource.HideButton + "</a></div>" });
        }

        #region Events

        protected void lbCancel_Click(object sender, EventArgs e)
        {
            CommonControlsConfigurer.FCKEditingCancel("blogs");
            Response.Redirect(Constants.DefaultPageUrl);
        }

        #endregion
    }
}