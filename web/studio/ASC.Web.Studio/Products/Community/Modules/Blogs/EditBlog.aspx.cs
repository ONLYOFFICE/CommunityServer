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


using ASC.Blogs.Core.Resources;
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Web.Community.Blogs.Views;
using ASC.Web.Community.Product;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ASC.Web.Community.Blogs
{
    public partial class EditBlog : BasePage
    {
        protected string _text = "";

        private string BlogId
        {
            get { return Request.QueryString["blogid"]; }
        }

        protected override void PageLoad()
        {
            Utility.RegisterTypeForAjax(typeof(AddBlog));

            if (String.IsNullOrEmpty(BlogId))
                Response.Redirect(Constants.DefaultPageUrl);

            var engine = GetEngine();

            Utility.RegisterTypeForAjax(typeof(EditBlog), Page);

            mainContainer.CurrentPageCaption = BlogsResource.EditPostTitle;
            Title = HeaderStringHelper.GetPageTitle(BlogsResource.EditPostTitle);

            ShowForEdit(engine);

            if (IsPostBack)
            {
                var control = FindControl(Request.Params["__EVENTTARGET"]);
                if (lbCancel.Equals(control))
                {
                    Response.Redirect("viewblog.aspx?blogid=" + Request.Params["blogid"]);
                }
                else
                {
                    if (CheckTitle(txtTitle.Text))
                    {
                        var pageEngine = GetEngine();
                        var post = pageEngine.GetPostById(new Guid(hidBlogID.Value));
                        UpdatePost(post, engine);
                    }
                    else
                    {
                        mainContainer.Options.InfoMessageText = BlogsResource.BlogTitleEmptyMessage;
                        mainContainer.Options.InfoType = InfoType.Alert;
                    }
                }
            }
            InitScript();
        }

        private void InitScript()
        {
            Page.RegisterBodyScripts("~/usercontrols/common/ckeditor/ckeditor-connector.js");

            //Page.RegisterInlineScript("ckeditorConnector.onReady(function () {BlogsManager.blogsEditor = jq('#ckEditor').ckeditor({ toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + @"'}).editor;});");
            Page.RegisterInlineScript("ckeditorConnector.onReady(function () {" +
                          "BlogsManager.blogsEditor = CKEDITOR.replace('ckEditor', { toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + "'});" +
                          "BlogsManager.blogsEditor.on('change',  function() {if (this.getData() == '') {jq('#btnPreview').addClass('disable');} else {jq('#btnPreview').removeClass('disable');}});" +
                           "});");

        }

        private void InitPreviewTemplate(Post post)
        {
            var control = (ViewBlogView)LoadControl("~/Products/Community/Modules/Blogs/Views/ViewBlogView.ascx");
            control.IsPreview = true;
            control.post = post;

            PlaceHolderPreview.Controls.Add(new Literal { Text = "<div class='headerPanel' style='margin-top:20px;'>" + BlogsResource.PreviewButton + "</div>" });
            PlaceHolderPreview.Controls.Add(control);
            PlaceHolderPreview.Controls.Add(new Literal { Text = "<div style='margin-top:25px;'><a class='button blue big' href='javascript:void(0);' onclick='BlogsManager.HidePreview(); return false;'>" + BlogsResource.HideButton + "</a></div>" });
        }


        private void ShowForEdit(BlogsEngine engine)
        {
            if (!IsPostBack)
            {
                var post = engine.GetPostById(new Guid(BlogId));

                InitPreviewTemplate(post);

                if (post != null && CommunitySecurity.CheckPermissions(post, Constants.Action_EditRemovePost))
                {
                    hdnUserID.Value = post.UserID.ToString();

                    if (Request.QueryString["action"] == "delete")
                    {
                        foreach (var comment in engine.GetPostComments(post.ID))
                        {
                            CommonControlsConfigurer.FCKUploadsRemoveForItem("blogs_comments", comment.ID.ToString());
                        }

                        engine.DeletePost(post);
                        CommonControlsConfigurer.FCKUploadsRemoveForItem("blogs", post.ID.ToString());
                        Response.Redirect(Constants.DefaultPageUrl);
                        return;
                    }
                    else
                    {
                        txtTitle.Text = Server.HtmlDecode(post.Title);
                        _text = post.Content;
                        hidBlogID.Value = post.ID.ToString();

                        LoadTags(post.TagList);
                    }
                }
                else
                {
                    Response.Redirect(Constants.DefaultPageUrl);
                    return;
                }
            }
        }

        private void LoadTags(IList<Tag> tags)
        {
            var sb = new StringBuilder();

            var i = 0;
            foreach (var tag in tags)
            {
                if (i != 0)
                    sb.Append(", " + tag.Content);
                else
                    sb.Append(tag.Content);
                i++;
            }

            txtTags.Text = sb.ToString();
        }


        private static bool CheckTitle(string title)
        {
            return !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(title.Trim());
        }

        private static bool IsExistsTagName(Post post, string tagName)
        {
            return post.TagList.Any(tag => tag.Content == tagName);
        }

        protected override string RenderRedirectUpload()
        {
            return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=blogs&iid=" + BlogId);
        }

        public void UpdatePost(Post post, BlogsEngine engine)
        {
            post.Title = GetLimitedText(txtTitle.Text);
            post.Content = (Request["blog_text"] ?? "");

            post.TagList = new List<Tag>();

            foreach (var tagName in txtTags.Text.Split(','))
            {
                if (tagName == string.Empty || IsExistsTagName(post, tagName))
                    continue;

                var tag = new Tag(post)
                    {
                        Content = GetLimitedText(tagName.Trim())
                    };
                post.TagList.Add(tag);
            }

            engine.SavePost(post, false, false);

            CommonControlsConfigurer.FCKEditingComplete("blogs", post.ID.ToString(), post.Content, true);

            Response.Redirect("viewblog.aspx?blogid=" + post.ID.ToString());
        }

        #region Events

        protected void lbCancel_Click(object sender, EventArgs e)
        {
            CommonControlsConfigurer.FCKEditingCancel("blogs", BlogId);
            Response.Redirect(Constants.DefaultPageUrl);
        }

        #endregion
    }
}