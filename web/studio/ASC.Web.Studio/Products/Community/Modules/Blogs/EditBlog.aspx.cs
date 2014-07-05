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
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/asc/core/decoder.js"));

            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor.js"));

            Page.RegisterInlineScript("CKEDITOR.replace('ckEditor', { toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + @"'});");
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
            post.Content = (Request["mobiletext"] ?? "");

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