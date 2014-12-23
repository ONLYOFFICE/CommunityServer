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
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("ViewBlog")]
    public partial class ViewBlog : BasePage
    {
        #region Members

        private string BlogId
        {
            get { return Request.QueryString["blogid"]; }
        }

        protected string BlogTitle { get; set; }
        protected string SubscribeStatus { get; set; }

        #endregion

        #region Methods

        protected override void PageLoad()
        {
            if (String.IsNullOrEmpty(BlogId))
                Response.Redirect(Constants.DefaultPageUrl);

            Utility.RegisterTypeForAjax(typeof(ViewBlog), Page);
            Utility.RegisterTypeForAjax(typeof(Subscriber));

            var engine = GetEngine();
            ShowPost(engine);

            Title = HeaderStringHelper.GetPageTitle(mainContainer.CurrentPageCaption ?? BlogsResource.AddonName);
        }

        private void ShowPost(BlogsEngine engine)
        {
            //EditBlogPresenter presenter = new EditBlogPresenter(ctrlViewBlogView, DaoFactory.GetBlogDao());
            //ctrlViewBlogView.AttachPresenter(presenter);

            ctrlViewBlogView.UpdateCompleted += HandleUpdateCompleted;
            ctrlViewBlogView.UpdateCancelled += HandleUpdateCancelled;

            if (IsPostBack) return;

            Post post;
            try
            {
                post = engine.GetPostById(new Guid(BlogId));
            }
            catch (Exception)
            {
                post = null;
            }

            if (post != null)
            {
                ctrlViewBlogView.post = post;
                var subscriber = new Subscriber();
                var postId = String.IsNullOrEmpty(BlogId) ? new Guid() : new Guid(BlogId);
                var isBlogSubscribe = subscriber.IsCommentsSubscribe(postId);
                var subscribeTopicLink = subscriber.RenderCommentsSubscriptionLink(!isBlogSubscribe, postId);

                SubscribeLinkBlock.Text = subscribeTopicLink;

                BlogTitle = post.Title;

                var loadedComments = engine.GetPostComments(post.ID);

                commentList.Items = BuildCommentsList(post, loadedComments);

                ConfigureComments(commentList, loadedComments.Count, post);
                engine.SavePostReview(post, SecurityContext.CurrentAccount.ID);
            }
            else
            {
                ctrlViewBlogView.Visible = false;
                lblMessage.Visible = true;
                mainContainer.CurrentPageCaption = BlogsResource.AddonName;
                commentList.Visible = false;
                ConfigureComments(commentList, 0, null);
            }
        }

        private static List<CommentInfo> BuildCommentsList(Post post, List<Comment> loaded)
        {
            return BuildCommentsList(post, loaded, Guid.Empty);
        }

        private static List<CommentInfo> BuildCommentsList(Post post, List<Comment> loaded, Guid parentId)
        {
            var result = new List<CommentInfo>();
            foreach (var comment in Comment.SelectChildLevel(parentId, loaded))
            {
                var info = new CommentInfo
                    {
                        CommentID = comment.ID.ToString(),
                        UserID = comment.UserID,
                        TimeStamp = comment.Datetime,
                        TimeStampStr = comment.Datetime.Ago(),
                        IsRead = true,
                        Inactive = comment.Inactive,
                        CommentBody = comment.Content,
                        UserFullName = DisplayUserSettings.GetFullUserName(comment.UserID),
                        UserAvatar = ImageHTMLHelper.GetHTMLUserAvatar(comment.UserID),
                        UserPost = CoreContext.UserManager.GetUsers(comment.UserID).Title,
                        IsEditPermissions = CommunitySecurity.CheckPermissions(comment, Constants.Action_EditRemoveComment),
                        IsResponsePermissions = CommunitySecurity.CheckPermissions(post, Constants.Action_AddComment),
                        CommentList = BuildCommentsList(post, loaded, comment.ID)
                    };

                result.Add(info);
            }
            return result;
        }

        private static void ConfigureComments(CommentsList commentList, int totalCount, Post postToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = CommunitySecurity.CheckPermissions(postToUpdate, Constants.Action_AddComment);
            commentList.CommentsCountTitle = totalCount > 0 ? totalCount.ToString() : "";
            commentList.FckDomainName = commentList.ObjectID = postToUpdate != null ? postToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "ViewBlog.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "ViewBlog.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "ViewBlog.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "ViewBlog.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "ViewBlog.UpdateComment";
            commentList.FckDomainName = "blogs_comments";

            commentList.TotalCount = totalCount;
        }

        //private string GetBlogTypeName(Blog blog)
        //{
        //    if (blog.GroupID == new Guid())
        //    {
        //        return ASC.Blogs.Core.Resources.BlogsResource.InPersonalBlogLabel;
        //    }
        //    else
        //    {
        //        var g = CoreContext.GroupManager.GetGroupInfo(blog.GroupID);
        //        var name = g.ID != ASC.Core.Users.Constants.LostGroupInfo.ID ? g.Name : string.Empty;
        //        return ASC.Blogs.Core.Resources.BlogsResource.InGroupBlogLabel + " \"" + name + "\"";
        //    }
        //}

        #region Ajax functions for comments management

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse AddComment(string parrentCommentID, string blogID, string text, string pid)
        {
            Guid postId;
            Guid? parentId = null;
            try
            {
                postId = new Guid(blogID);

                if (!String.IsNullOrEmpty(parrentCommentID))
                    parentId = new Guid(parrentCommentID);
            }
            catch
            {
                return new AjaxResponse();
            }

            var engine = GetEngine();

            var resp = new AjaxResponse { rs1 = parrentCommentID };

            var post = engine.GetPostById(postId);

            if (post == null)
            {
                return new AjaxResponse { rs10 = "postDeleted", rs11 = Constants.DefaultPageUrl };
            }

            CommunitySecurity.DemandPermissions(post, Constants.Action_AddComment);

            var newComment = new Comment
                {
                    PostId = postId,
                    Content = text,
                    Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    UserID = SecurityContext.CurrentAccount.ID
                };

            if (parentId.HasValue)
                newComment.ParentId = parentId.Value;

            engine.SaveComment(newComment, post);

            //mark post as seen for the current user
            engine.SavePostReview(post, SecurityContext.CurrentAccount.ID);
            resp.rs2 = GetHTMLComment(newComment, false);

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            var resp = new AjaxResponse { rs1 = commentID };

            Guid? id = null;
            try
            {
                if (!String.IsNullOrEmpty(commentID))
                    id = new Guid(commentID);
            }
            catch
            {
                return new AjaxResponse();
            }

            var engine = GetEngine();

            var comment = engine.GetCommentById(id.Value);
            if (comment == null)
                throw new ApplicationException("Comment not found");

            CommunitySecurity.DemandPermissions(comment, Constants.Action_EditRemoveComment);

            comment.Content = text;

            var post = engine.GetPostById(comment.PostId);
            engine.UpdateComment(comment, post);

            resp.rs2 = HtmlUtility.GetFull(text) + CodeHighlighter.GetJavaScriptLiveHighlight(true);

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string RemoveComment(string commentID, string pid)
        {
            Guid? id = null;
            try
            {
                if (!String.IsNullOrEmpty(commentID))
                    id = new Guid(commentID);
            }
            catch
            {
                return commentID;
            }

            var engine = GetEngine();

            var comment = engine.GetCommentById(id.Value);
            if (comment == null)
                throw new ApplicationException("Comment not found");

            CommunitySecurity.DemandPermissions(comment, Constants.Action_EditRemoveComment);

            comment.Inactive = true;

            var post = engine.GetPostById(comment.PostId);
            engine.RemoveComment(comment, post);

            return commentID;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreview(string text, string commentID)
        {
            return GetHTMLComment(text, commentID);
        }

        #endregion

        private static string GetHTMLComment(Comment comment, bool isPreview)
        {
            var info = new CommentInfo
                {
                    CommentID = comment.ID.ToString(),
                    UserID = comment.UserID,
                    TimeStamp = comment.Datetime,
                    TimeStampStr = comment.Datetime.Ago(),
                    IsRead = true,
                    Inactive = comment.Inactive,
                    CommentBody = comment.Content,
                    UserFullName = DisplayUserSettings.GetFullUserName(comment.UserID),
                    UserAvatar = ImageHTMLHelper.GetHTMLUserAvatar(comment.UserID),
                    UserPost = CoreContext.UserManager.GetUsers(comment.UserID).Title
                };

            if (!isPreview)
            {
                info.IsEditPermissions = CommunitySecurity.CheckPermissions(comment, Constants.Action_EditRemoveComment);

                info.IsResponsePermissions = CommunitySecurity.CheckPermissions(comment.Post, Constants.Action_AddComment);
            }
            var defComment = new CommentsList();
            ConfigureComments(defComment, 0, null);

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                defComment,
                info,
                comment.IsRoot(),
                false);
        }

        private static string GetHTMLComment(string text, string commentID)
        {
            var comment = new Comment
                {
                    Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    UserID = SecurityContext.CurrentAccount.ID
                };

            if (!String.IsNullOrEmpty(commentID))
            {
                comment = GetEngine().GetCommentById(new Guid(commentID));
            }
            comment.Content = text;

            return GetHTMLComment(comment, true);
        }

        #endregion

        #region Events

        private void HandleUpdateCancelled(object sender, EventArgs e)
        {
            Response.Redirect(Constants.DefaultPageUrl);
        }

        private void HandleUpdateCompleted(object sender, EventArgs e)
        {
            Response.Redirect("viewblog.aspx?blogid=" + BlogId);
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
        }

        #endregion
    }
}