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