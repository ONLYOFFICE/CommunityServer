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
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Web.Community.Blogs
{
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

            Utility.RegisterTypeForAjax(typeof(Subscriber));

            var engine = GetEngine();
            ShowPost(engine);

            Title = HeaderStringHelper.GetPageTitle(mainContainer.CurrentPageCaption ?? BlogsResource.AddonName);
        }

        private void ShowPost(BlogsEngine engine)
        {
            //EditBlogPresenter presenter = new EditBlogPresenter(ctrlViewBlogView, DaoFactory.GetBlogDao());
            //ctrlViewBlogView.AttachPresenter(presenter);

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
                        CommentBody = HtmlUtility.GetFull(comment.Content),
                        UserFullName = DisplayUserSettings.GetFullUserName(comment.UserID),
                        UserProfileLink = CommonLinkUtility.GetUserProfile(comment.UserID),
                        UserAvatarPath = UserPhotoManager.GetBigPhotoURL(comment.UserID),
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
            commentList.FckDomainName = commentList.ObjectID = postToUpdate != null ? postToUpdate.ID.ToString() : "";

            commentList.BehaviorID = "commentsObj";
            commentList.ModuleName = "blogs";
            commentList.FckDomainName = "blogs_comments";

            commentList.TotalCount = totalCount;
        }

        #endregion

    }
}