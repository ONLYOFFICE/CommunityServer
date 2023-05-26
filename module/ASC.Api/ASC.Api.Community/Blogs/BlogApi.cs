/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Security;

using ASC.Api.Attributes;
using ASC.Api.Blogs;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Utils;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Community
{
    ///<name>community</name>
    public partial class CommunityApi
    {
        private BlogsEngine _blogEngine;
        protected internal BlogsEngine BlogEngine
        {
            get
            {
                if (_blogEngine == null)
                {
                    _blogEngine = BlogsEngine.GetEngine(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                }
                return _blogEngine;
            }
        }


        ///<summary>
        /// Returns a list of all the posts from the portal blogs with the post titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>Get posts</short>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperSummary, ASC.Api.Community">List of all posts</returns>
        ///<category>Blogs</category>
        ///<path>api/2.0/community/blog</path>
        ///<collection>list</collection>
        ///<httpMethod>GET</httpMethod>
        [Read("blog")]
        public IEnumerable<BlogPostWrapperSummary> GetPosts()
        {
            return BlogEngine.SelectPosts(GetQuery()).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Creates a blog post with the specified title, content, tags and subscription to comments defined in the request body.
        ///</summary>
        ///<short>Create a post</short>
        ///<param type="System.String, System" name="title">Post title</param>
        ///<param type="System.String, System" name="content">Post text</param>
        ///<param type="System.String, System" name="tags">List of tags separated with comma</param>
        ///<param type="System.Boolean, System" name="subscribeComments">Subscribes to the post comments or not</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperFull, ASC.Api.Community">Newly created post</returns>
        ///<example>
        ///<![CDATA[Sending data in application/json:
        ///
        ///{
        ///     title:"My post",
        ///     content:"Post content",
        ///     tags:"Me,Post,News",
        ///     subscribeComments: "true"
        ///}
        ///
        ///Sending data in application/x-www-form-urlencoded
        ///title="My post"&content="Post content"&tags="Me,Post,News"&subscribeComments="true"]]>
        ///</example>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<category>Blogs</category>
        /// <path>api/2.0/community/blog</path>
        /// <httpMethod>POST</httpMethod>
        [Create("blog")]
        public BlogPostWrapperFull CreatePost(string title, string content, string tags, bool subscribeComments)
        {
            var newPost = new Post
            {
                Content = content,
                Datetime = Core.Tenants.TenantUtil.DateTimeNow(),
                Title = title,/*TODO: maybe we should trim this */
                UserID = SecurityContext.CurrentAccount.ID,
                TagList = !string.IsNullOrEmpty(tags) ? tags.Split(',').Distinct().Select(x => new Tag() { Content = x }).ToList() : new List<Tag>()
            };

            BlogEngine.SavePost(newPost, true, subscribeComments);
            return new BlogPostWrapperFull(newPost);
        }

        ///<summary>
        ///Updates the selected post changing the post title, content or/and tags specified in the request.
        ///</summary>
        ///<short>Update a post</short>
        ///<param type="System.Guid, System" method="url" name="postid">Post ID</param>
        ///<param type="System.String, System" name="title">New title</param>
        ///<param type="System.String, System" name="content">New post text</param>
        ///<param type="System.String, System" name="tags">New list of tags separated with comma</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperFull, ASC.Api.Community">Updated post</returns>
        ///<example>
        ///<![CDATA[
        ///Sending data in application/json:
        ///{
        ///     title:"My post",
        ///     content:"Post content",
        ///     tags:"Me,Post,News"
        ///}
        ///
        ///Sending data in application/x-www-form-urlencoded
        ///title="My post"&content="Post content"&tags="Me,Post,News"
        ///]]>
        ///
        ///</example>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<category>Blogs</category>
        ///<path>api/2.0/community/blog/{postid}</path>
        ///<httpMethod>PUT</httpMethod>
        [Update("blog/{postid}")]
        public BlogPostWrapperFull UpdatePost(Guid postid, string title, string content, string tags)
        {
            var post = BlogEngine.GetPostById(postid).NotFoundIfNull();
            post.Content = content;
            post.Title = title;
            if (tags != null)
                post.TagList = tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().Select(x => new Tag() { Content = x }).ToList();
            BlogEngine.SavePost(post, false, false);
            return new BlogPostWrapperFull(post);
        }

        ///<summary>
        ///Deletes a post with the ID specified in the request from blogs.
        ///</summary>
        ///<short>Delete a post</short>
        ///<param type="System.Guid, System" method="url" name="postid">Post ID</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperFull, ASC.Api.Community">Deleted post</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<category>Blogs</category>
        ///<path>api/2.0/community/blog/{postid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("blog/{postid}")]
        public BlogPostWrapperFull DeletePost(Guid postid)
        {
            var post = BlogEngine.GetPostById(postid).NotFoundIfNull();
            if (!CommunitySecurity.CheckPermissions(post, ASC.Blogs.Core.Constants.Action_EditRemovePost))
                throw new SecurityException("Access denied.");
            BlogEngine.DeletePost(post);
            return null;
        }

        ///<summary>
        ///Returns a list of all the blog posts for the current user with the post titles, dates of creation and update, post texts, and author.
        ///</summary>
        ///<category>Blogs</category>
        ///<short>Get my posts</short>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperSummary, ASC.Api.Community">List of my posts</returns>
        ///<colletion>list</colletion>
        ///<path>api/2.0/community/blog/@self</path>
        ///<httpMethod>GET</httpMethod>
        [Read("blog/@self")]
        public IEnumerable<BlogPostWrapperSummary> GetMyPosts()
        {
            return BlogEngine.SelectPosts(GetQuery().SetUser(SecurityContext.CurrentAccount.ID)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts matching the search query specified in the request with the post titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<category>Blogs</category>
        ///<short>
        ///Search posts
        ///</short>
        /// <param type="System.String, System" method="url" name="query">Search query</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperSummary, ASC.Api.Community">List of found posts</returns>
        ///<path>api/2.0/community/blog/@search/{query}</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("blog/@search/{query}")]
        public IEnumerable<BlogPostWrapperSummary> GetSearchPosts(string query)
        {
            return BlogEngine.SelectPosts(GetQuery().SetSearch(query)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts for the specified user with the post titles, dates of creation and update and post texts.
        ///</summary>
        ///<short>Get user posts</short>
        ///<category>Blogs</category>
        ///<param type="System.String, System" method="url" name="username" remark="You can retrieve the user name through the 'people' API">User name</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperSummary, ASC.Api.Community">List of user posts</returns>
        ///<path>api/2.0/community/blog/user/{username}</path>
        ///<httpMethod>GET</httpMethod>
        /// <colletion>list</colletion>
        [Read("blog/user/{username}")]
        public IEnumerable<BlogPostWrapperSummary> GetUserPosts(string username)
        {
            var userid = CoreContext.UserManager.GetUserByUserName(username).ID;
            return BlogEngine.SelectPosts(GetQuery().SetUser(userid)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts containing the tag specified in the request with the post titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>Get posts by tag</short>
        ///<category>Blogs</category>
        ///<param type="System.String, System" method="url" name="tag">Tag name</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperSummary, ASC.Api.Community">List of posts with the specified tag name</returns>
        ///<collection>list</collection>
        ///<path>api/2.0/community/blog/tag/{tag}</path>
        ///<httpMethod>GET</httpMethod>
        [Read("blog/tag/{tag}")]
        public IEnumerable<BlogPostWrapperSummary> GetPostsByTag(string tag)
        {
            return BlogEngine.SelectPosts(GetQuery().SetTag(tag)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all the tags used in the blog posts with a number specifying the tag usage.
        ///</summary>
        /// <category>Blogs</category>
        /// <short>Get tags</short>
        ///<returns type="ASC.Api.Blogs.BlogTagWrapper, ASC.Api.Community">List of tags</returns>
        ///<path>api/2.0/community/blog/tag</path>
        ///<httpMethod>GET</httpMethod>
        ///<colletion>list</colletion>
        [Read("blog/tag")]
        public IEnumerable<BlogTagWrapper> GetTags()
        {
            var result = BlogEngine.GetTopTagsList((int)_context.Count).Select(x => new BlogTagWrapper(x));
            _context.SetDataPaginated();
            return result.ToSmartList();
        }



        ///<summary>
        /// Returns the detailed information on the blog post with the ID specified in the request.
        ///</summary>
        ///<short>Get a post</short>
        ///<category>Blogs</category>
        ///<param type="System.Guid, System" method="url" name="postid">Post ID</param>
        ///<returns type="ASC.Api.Blogs.BlogPostWrapperFull, ASC.Api.Community">Post information</returns>
        ///<path>api/2.0/community/blog/{postid}</path>
        ///<httpMethod>GET</httpMethod>
        [Read("blog/{postid}")]
        public BlogPostWrapperFull GetPost(Guid postid)
        {
            return new BlogPostWrapperFull(BlogEngine.GetPostById(postid).NotFoundIfNull());
        }

        ///<summary>
        /// Returns a list of all the comments on the blog post with the ID specified in the request.
        ///</summary>
        ///<category>Blogs</category>
        /// <short>Get post comments</short>
        ///<param type="System.Guid, System" method="url" name="postid">Post ID (GUID)</param>
        ///<returns type="ASC.Api.Blogs.BlogPostCommentWrapper, ASC.Api.Community">List of post comments</returns>
        ///<path>api/2.0/community/blog/{postid}/comment</path>
        ///<httpMethod>GET</httpMethod>
        /// <colletion>list</colletion>
        [Read("blog/{postid}/comment")]
        public IEnumerable<BlogPostCommentWrapper> GetComments(Guid postid)
        {
            return BlogEngine.GetPostComments(postid).Where(x => !x.Inactive).Select(x => new BlogPostCommentWrapper(x)).ToSmartList();
        }

        ///<summary>
        /// Adds a comment to the post with the ID specified in the request. The parent comment ID can be also specified if needed.
        ///</summary>
        /// <short>Add a post comment</short>
        /// <category>Blogs</category>
        ///<param type="System.Guid, System" method="url" name="postid">Post ID</param>
        ///<param type="System.String, System" name="content">Comment text</param>
        ///<param type="System.Guid, System" name="parentId">Parent comment ID</param>
        ///<returns type="ASC.Api.Blogs.BlogPostCommentWrapper, ASC.Api.Community">List of post comments</returns>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     content:"My comment",
        ///     parentId:"9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// }
        /// 
        /// Sending data in application/x-www-form-urlencoded
        /// content="My comment"&parentId="9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// ]]>
        /// </example>
        /// <remarks>
        /// Send parentId=00000000-0000-0000-0000-000000000000 or don't send it at all if you want your comment to be on the root level.
        /// </remarks>
        /// <path>api/2.0/community/blog/{postid}/comment</path>
        /// <httpMethod>POST</httpMethod>
        [Create("blog/{postid}/comment")]
        public BlogPostCommentWrapper AddComment(Guid postid, string content, Guid parentId)
        {
            var post = BlogEngine.GetPostById(postid).NotFoundIfNull();

            var newComment = new Comment
            {
                PostId = postid,
                Content = content,
                Datetime = DateTime.UtcNow,
                UserID = SecurityContext.CurrentAccount.ID
            };

            if (parentId != Guid.Empty)
                newComment.ParentId = parentId;
            BlogEngine.SaveComment(newComment, post);
            return new BlogPostCommentWrapper(newComment);
        }


        /// <summary>
        /// Returns a comment preview with the content specified in the request.
        /// </summary>
        /// <short>Get a comment preview</short>
        /// <section>Comments</section>
        /// <param type="System.String, System" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="htmltext">Comment text in the HTML format</param>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <category>Blogs</category>
        /// <path>api/2.0/community/blog/comment/preview</path>
        /// <httpMethod>POST</httpMethod>
        [Create("blog/comment/preview")]
        public CommentInfo GetBlogCommentPreview(string commentid, string htmltext)
        {
            var comment = new Comment
            {
                Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                UserID = SecurityContext.CurrentAccount.ID
            };

            if (!String.IsNullOrEmpty(commentid))
            {
                comment = BlogEngine.GetCommentById(new Guid(commentid));
            }
            comment.Content = htmltext;

            return GetCommentInfo(comment, true);
        }


        /// <summary>
        /// Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <section>Comments</section>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <returns>Comment ID</returns>
        /// <category>Blogs</category>
        /// <path>api/2.0/community/blog/comment/{commentid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("blog/comment/{commentid}")]
        public string RemoveBlogComment(string commentid)
        {
            Guid? id = null;
            try
            {
                if (!String.IsNullOrEmpty(commentid))
                    id = new Guid(commentid);
            }
            catch
            {
                return commentid;
            }

            var comment = BlogEngine.GetCommentById(id.Value);
            if (comment == null)
                throw new ApplicationException("Comment not found");

            CommunitySecurity.DemandPermissions(comment, ASC.Blogs.Core.Constants.Action_EditRemoveComment);

            comment.Inactive = true;

            var post = BlogEngine.GetPostById(comment.PostId);
            BlogEngine.RemoveComment(comment, post);

            return commentid;
        }

        /// <summary>
        /// Adds a blog comment with the comment text specified in the request. The parent comment ID can be also specified if needed.
        /// </summary>
        /// <short>Add a blog comment</short>
        /// <section>Comments</section>
        /// <param type="System.String, System" name="parentcommentid">Parent comment ID</param>
        /// <param type="System.String, System" name="entityid">Entity ID where a comment will be added</param>
        /// <param type="System.String, System" name="content">Comment text</param>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <category>Blogs</category>
        /// <path>api/2.0/community/blog/comment</path>
        /// <httpMethod>POST</httpMethod>
        [Create("blog/comment")]
        public CommentInfo AddBlogComment(string parentcommentid, string entityid, string content)
        {
            Guid postId;
            Guid? parentId = null;

            postId = new Guid(entityid);
            if (!String.IsNullOrEmpty(parentcommentid))
                parentId = new Guid(parentcommentid);


            var post = BlogEngine.GetPostById(postId);

            if (post == null)
            {
                throw new Exception("postDeleted");
            }

            CommunitySecurity.DemandPermissions(post, ASC.Blogs.Core.Constants.Action_AddComment);

            var newComment = new Comment
            {
                PostId = postId,
                Content = content,
                Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                UserID = SecurityContext.CurrentAccount.ID
            };

            if (parentId.HasValue)
                newComment.ParentId = parentId.Value;

            BlogEngine.SaveComment(newComment, post);

            //mark post as seen for the current user
            BlogEngine.SavePostReview(post, SecurityContext.CurrentAccount.ID);

            return GetCommentInfo(newComment, false);
        }

        /// <summary>
        /// Updates a blog comment specified in the request changing its content.
        /// </summary>
        /// <short>Update a blog comment</short>
        /// <section>Comments</section>
        /// <category>Blogs</category>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="content">New comment text</param>
        /// <returns>Updated blog comment</returns>
        /// <path>api/2.0/community/blog/comment/{commentid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("blog/comment/{commentid}")]
        public string UpdateBlogComment(string commentid, string content)
        {
            Guid? id = null;
            if (!String.IsNullOrEmpty(commentid))
                id = new Guid(commentid);

            var comment = BlogEngine.GetCommentById(id.Value);
            if (comment == null)
                throw new Exception("Comment not found");

            CommunitySecurity.DemandPermissions(comment, ASC.Blogs.Core.Constants.Action_EditRemoveComment);

            comment.Content = content;

            var post = BlogEngine.GetPostById(comment.PostId);
            BlogEngine.UpdateComment(comment, post);

            return HtmlUtility.GetFull(content);
        }



        private static CommentInfo GetCommentInfo(Comment comment, bool isPreview)
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
                UserPost = CoreContext.UserManager.GetUsers(comment.UserID).Title
            };

            if (!isPreview)
            {
                info.IsEditPermissions = CommunitySecurity.CheckPermissions(comment, ASC.Blogs.Core.Constants.Action_EditRemoveComment);

                info.IsResponsePermissions = CommunitySecurity.CheckPermissions(comment.Post, ASC.Blogs.Core.Constants.Action_AddComment);
            }

            return info;
        }

        private PostsQuery GetQuery()
        {
            var query = new PostsQuery().SetOffset((int)_context.StartIndex).SetCount((int)_context.Count);
            _context.SetDataPaginated();
            return query;
        }

    }
}
