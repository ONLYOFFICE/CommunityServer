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
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Community
{
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
        /// Returns the list of all posts in blogs on the portal with the post title, date of creation and update, post text and author ID and display name
        ///</summary>
        ///<short>All posts</short>
        ///<returns>List of all posts</returns>
        ///<category>Blogs</category>
        [Read("blog")]
        public IEnumerable<BlogPostWrapperSummary> GetPosts()
        {
            return BlogEngine.SelectPosts(GetQuery()).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Creates a blog post with the specified title, content, tags and subscription to comments defined in the request body
        ///</summary>
        ///<short>Create post</short>
        ///<param name="title">New post title</param>
        ///<param name="content">New post text</param>
        ///<param name="tags">Tag list separated with ','</param>
        ///<param name="subscribeComments">Subscribe to post comments</param>
        ///<returns>Newly created post</returns>
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
        ///Updates the specified post changing the post title, content or/and tags specified
        ///</summary>
        ///<short>Update post</short>
        ///<param name="postid">post ID</param>
        ///<param name="title">new title</param>
        ///<param name="content">new post text</param>
        ///<param name="tags">tag list separated with ','</param>
        ///<returns>Updated post</returns>
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
        ///Deletes the selected post from blogs
        ///</summary>
        ///<short>Delete post</short>
        ///<param name="postid">post ID to delete</param>
        ///<returns>Nothing</returns>
        ///<exception cref="ItemNotFoundException"></exception>
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
        ///Returns the list of all blog posts for the current user with the post title, date of creation and update, post text and author ID and display name
        ///</summary>
        ///<category>Blogs</category>
        ///<short>My posts</short>
        ///<returns>My post list</returns>
        [Read("blog/@self")]
        public IEnumerable<BlogPostWrapperSummary> GetMyPosts()
        {
            return BlogEngine.SelectPosts(GetQuery().SetUser(SecurityContext.CurrentAccount.ID)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts matching the search query with the post title, date of creation and update, post text and author
        ///</summary>
        ///<category>Blogs</category>
        ///<short>
        ///Search posts
        ///</short>
        /// <param name="query">search query</param>
        ///<returns>Found post list</returns>
        [Read("blog/@search/{query}")]
        public IEnumerable<BlogPostWrapperSummary> GetSearchPosts(string query)
        {
            return BlogEngine.SelectPosts(GetQuery().SetSearch(query)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts of the specified user with the post title, date of creation and update and post text
        ///</summary>
        ///<short>User posts</short>
        ///<category>Blogs</category>
        ///<param name="username" remark="You can retrieve username through 'people' api">Username</param>
        ///<returns>List of user posts</returns>
        [Read("blog/user/{username}")]
        public IEnumerable<BlogPostWrapperSummary> GetUserPosts(string username)
        {
            var userid = CoreContext.UserManager.GetUserByUserName(username).ID;
            return BlogEngine.SelectPosts(GetQuery().SetUser(userid)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of blog posts containing the specified tag with the post title, date of creation and update, post text and author
        ///</summary>
        ///<short>By tag</short>
        ///<category>Blogs</category>
        ///<param name="tag">tag name</param>
        ///<returns>List of posts tagged with tag name</returns>
        [Read("blog/tag/{tag}")]
        public IEnumerable<BlogPostWrapperSummary> GetPostsByTag(string tag)
        {
            return BlogEngine.SelectPosts(GetQuery().SetTag(tag)).Select(x => new BlogPostWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all tags used with blog posts with the post title, date of creation and update, post text, author and all the tags used
        ///</summary>
        /// <category>Blogs</category>
        /// <short>Tags</short>
        ///<returns>List of tags</returns>
        [Read("blog/tag")]
        public IEnumerable<BlogTagWrapper> GetTags()
        {
            var result = BlogEngine.GetTopTagsList((int)_context.Count).Select(x => new BlogTagWrapper(x));
            _context.SetDataPaginated();
            return result.ToSmartList();
        }



        ///<summary>
        /// Returns the detailed information for the blog post with the ID specified in the request
        ///</summary>
        ///<short>Specific post</short>
        ///<category>Blogs</category>
        ///<param name="postid">post ID</param>
        ///<returns>post</returns>
        [Read("blog/{postid}")]
        public BlogPostWrapperFull GetPost(Guid postid)
        {
            return new BlogPostWrapperFull(BlogEngine.GetPostById(postid).NotFoundIfNull());
        }

        ///<summary>
        /// Returns the list of all the comments for the blog post with the ID specified in the request
        ///</summary>
        ///<category>Blogs</category>
        /// <short>Get comments</short>
        ///<param name="postid">post ID (GUID)</param>
        ///<returns>list of post comments</returns>
        [Read("blog/{postid}/comment")]
        public IEnumerable<BlogPostCommentWrapper> GetComments(Guid postid)
        {
            return BlogEngine.GetPostComments(postid).Where(x => !x.Inactive).Select(x => new BlogPostCommentWrapper(x)).ToSmartList();
        }

        ///<summary>
        /// Adds a comment to the specified post with the comment text specified. The parent comment ID can be also specified if needed.
        ///</summary>
        /// <short>Add comment</short>
        /// <category>Blogs</category>
        ///<param name="postid">post ID</param>
        ///<param name="content">comment text</param>
        ///<param name="parentId">parent comment ID</param>
        ///<returns>List of post comments</returns>
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
        /// Send parentId=00000000-0000-0000-0000-000000000000 or don't send it at all if you want your comment to be on the root level
        /// </remarks>
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

        private PostsQuery GetQuery()
        {
            var query = new PostsQuery().SetOffset((int)_context.StartIndex).SetCount((int)_context.Count);
            _context.SetDataPaginated();
            return query;
        }

    }
}
