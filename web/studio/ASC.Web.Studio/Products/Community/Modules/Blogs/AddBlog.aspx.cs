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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

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
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");

            //Page.RegisterInlineScript("ckeditorConnector.load(function () {BlogsManager.blogsEditor = jq('#ckEditor').ckeditor({ toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + @"'}).editor;});");
            Page.RegisterInlineScript("ckeditorConnector.load(function () {" +
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

            Response.Redirect("AddBlog.aspx");
            return null;
        }

        private void TryPostBlog(BlogsEngine engine)
        {
            if (CheckTitle(txtTitle.Text))
            {
                var post = AddNewBlog(engine);

                if (post != null)
                    Response.Redirect("ViewBlog.aspx?blogid=" + post.ID.ToString());
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

            var control = (ViewBlogView)LoadControl("~/Products/Community/Modules/Blogs/Views/ViewBlogView.ascx");
            control.IsPreview = true;
            control.post = post;

            PlaceHolderPreview.Controls.Add(control);
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