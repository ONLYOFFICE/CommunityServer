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


using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Forums;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Forum;
using System;
using ASC.Web.Community.Forum;
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Community
{
    //TODO: Add html decoding to some fields!!! 
    public partial class CommunityApi
    {
        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private const int DefaultItemsPerPage = 100;

        private int CurrentPage
        {
            get { return (int)(_context.StartIndex / Count); }
        }

        private int Count
        {
            get { return (int)(_context.Count == 0 ? DefaultItemsPerPage : _context.Count); }
        }

        ///<summary>
        ///Returns the list of all forums created on the portal with the topic/thread titles, date of creation and update, post text and author ID and display name
        ///</summary>
        ///<short>
        ///Forum list
        ///</short>
        ///<returns>List of forums</returns>
        ///<category>Forums</category>
        [Read("forum")]
        public ForumWrapper GetForums()
        {
            List<ThreadCategory> categories;
            List<Thread> threads;
            Forum.ForumDataProvider.GetThreadCategories(TenantId, false, out categories, out threads);
            return new ForumWrapper(categories, threads);
        }

        ///<summary>
        ///Returns the number of all forums created on the portal
        ///</summary>
        ///<short>
        ///Forums count
        ///</short>
        ///<returns>Number of forums</returns>
        ///<visible>false</visible>
        ///<category>Forums</category>
        [Read("forum/count")]
        public int GetForumsCount()
        {
            return Forum.ForumDataProvider.GetThreadCategoriesCount(TenantId);
        }

        ///<summary>
        ///Returns the list of all thread topics in the forums on the portal with the thread title, date of creation and update, post text and author id and display name
        ///</summary>
        ///<short>
        ///Thread topics
        ///</short>
        ///<param name="threadid">Thread ID</param>
        ///<returns>List of topics in thread</returns>
        ///<category>Forums</category>
        [Read("forum/{threadid}")]
        public ForumThreadWrapperFull GetThreadTopics(int threadid)
        {
            var topicsIds = ForumDataProvider.GetTopicIDs(TenantId, threadid).Skip((int)_context.StartIndex);
            if (_context.Count > 0)
            {
                topicsIds = topicsIds.Take((int)_context.Count);
            }
            _context.SetDataPaginated();
            return new ForumThreadWrapperFull(ForumDataProvider.GetThreadByID(TenantId, threadid).NotFoundIfNull(), ForumDataProvider.GetTopicsByIDs(TenantId, topicsIds.ToList(), true));
        }


        ///<summary>
        ///Returns the list of all recently updated topics in the forums on the portal with the topic title, date of creation and update, post text and author
        ///</summary>
        ///<short>
        ///Last updated topics
        ///</short>
        ///<returns></returns>
        ///<category>Forums</category>
        [Read("forum/topic/recent")]
        public IEnumerable<ForumTopicWrapper> GetLastTopics()
        {
            var result = ForumDataProvider.GetLastUpdateTopics(TenantId, (int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return result.Select(x => new ForumTopicWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all posts in a selected thread in the forums on the portal with the thread title, date of creation and update, post text and author ID and display name
        ///</summary>
        ///<short>
        ///Posts
        ///</short>
        ///<param name="topicid">Topic ID</param>
        ///<returns>List of posts in topic</returns>
        ///<category>Forums</category>
        [Read("forum/topic/{topicid}")]
        public ForumTopicWrapperFull GetTopicPosts(int topicid)
        {
            //TODO: Deal with polls
            var postIds = ForumDataProvider.GetPostIDs(TenantId, topicid).Skip((int)_context.StartIndex);
            if (_context.Count > 0)
            {
                postIds = postIds.Take((int)_context.Count);
            }
            _context.SetDataPaginated();
            return new ForumTopicWrapperFull(ForumDataProvider.GetTopicByID(TenantId, topicid).NotFoundIfNull(),
                                             ForumDataProvider.GetPostsByIDs(TenantId, postIds.ToList()));
        }

        ///<summary>
        /// Add thread to category
        ///</summary>
        ///<short>
        /// Add thread to category
        ///</short>
        /// <param name="categoryId">Category ID (-1 for new category)</param>
        /// <param name="categoryName">Category name</param>
        /// <param name="threadName">Thread name</param>
        /// <param name="threadDescription">Thread description</param>
        ///<returns>Added thread</returns>
        ///<category>Forums</category>
        [Create("forum")]
        public ForumThreadWrapper AddThreadToCategory(int categoryId, string categoryName, string threadName, string threadDescription)
        {
            categoryName = categoryName.Trim();
            threadName = threadName.Trim();
            threadDescription = threadDescription.Trim();
            
            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                throw new Exception("Error access denied");
            }

            if (String.IsNullOrEmpty(threadName))
            {
                throw new Exception("Error empty thread name");
            }

            var thread = new Thread
            {
                Title = threadName,
                Description = threadDescription,
                SortOrder = 100,
                CategoryID = categoryId
            };

            if (thread.CategoryID == -1)
            {
                if (String.IsNullOrEmpty(categoryName))
                {
                    throw new Exception("Error empty category name");
                }
                thread.CategoryID = ForumDataProvider.CreateThreadCategory(TenantId, categoryName, String.Empty, 100);
            }

            var threadId = ForumDataProvider.CreateThread(TenantId, thread.CategoryID, thread.Title, thread.Description, thread.SortOrder);
            thread = ForumDataProvider.GetThreadByID(TenantId, threadId);

            return new ForumThreadWrapper(thread);
        }

        ///<summary>
        /// Adds a new topic to an existing thread with a subject, content and topic type specified
        ///</summary>
        ///<short>
        /// Add topic to thread
        ///</short>
        /// <param name="subject">Topic subject</param>
        /// <param name="threadid">ID of thread to add to</param>
        /// <param name="content">Topic text</param>
        /// <param name="topicType">Type of topic</param>
        ///<returns>Added topic</returns>
        ///<category>Forums</category>
        [Create("forum/{threadid}")]
        public ForumTopicWrapperFull AddTopic(int threadid, string subject, string content, TopicType topicType)
        {
            var id = ForumDataProvider.CreateTopic(TenantId, threadid, subject, topicType);
            ForumDataProvider.CreatePost(TenantId, id, 0, subject, content, true,
                                         PostTextFormatter.BBCode);
            return GetTopicPosts(id);
        }

        ///<summary>
        /// Updates a topic in an existing thread changing the thread subject, making it sticky or closing it
        ///</summary>
        ///<short>
        /// Update topic in thread
        ///</short>
        /// <param name="topicid">ID of topic to update</param>
        /// <param name="subject">Subject</param>
        /// <param name="sticky">Is sticky</param>
        /// <param name="closed">Close topic</param>
        ///<returns>Updated topic</returns>
        ///<category>Forums</category>
        [Update("forum/topic/{topicid}")]
        public ForumTopicWrapperFull UpdateTopic(int topicid, string subject, bool sticky, bool closed)
        {
            ForumDataProvider.UpdateTopic(TenantId, topicid, subject, sticky, closed);
            return GetTopicPosts(topicid);
        }
        ///<summary>
        /// Adds a post to an existing topic with a post subject and content specified in the request
        ///</summary>
        ///<short>
        /// Add post to topic
        ///</short>
        ///<param name="topicid">Topic ID</param>
        ///<param name="parentPostId">Parent post ID</param>
        ///<param name="subject">Post subject (required)</param>
        ///<param name="content">Post text</param>
        ///<returns>New post</returns>
        ///<category>Forums</category>
        [Create("forum/topic/{topicid}")]
        public ForumTopicPostWrapper AddTopicPosts(int topicid, int parentPostId, string subject, string content)
        {
            var id = ForumDataProvider.CreatePost(TenantId, topicid, parentPostId, subject, content, true,
                                         PostTextFormatter.BBCode);
            return new ForumTopicPostWrapper(ForumDataProvider.GetPostByID(TenantId, id));
        }

        ///<summary>
        /// Updates a post in an existing topic changing the post subject or/and content
        ///</summary>
        ///<short>
        /// Update post in topic
        ///</short>
        ///<param name="topicid">Topic ID</param>
        ///<param name="postid">ID of post to update</param>
        ///<param name="subject">Post subject (required)</param>
        ///<param name="content">Post text</param>
        ///<returns>Updated post</returns>    
        ///<category>Forums</category>
        [Update("forum/topic/{topicid}/{postid}")]
        public ForumTopicPostWrapper UpdateTopicPosts(int topicid, int postid, string subject, string content)
        {
            ForumDataProvider.UpdatePost(TenantId, postid, subject, content, PostTextFormatter.BBCode);
            return new ForumTopicPostWrapper(ForumDataProvider.GetPostByID(TenantId, postid));
        }

        ///<summary>
        ///Returns a list of topics matching the search query with the topic title, date of creation and update, post text and author
        ///</summary>
        ///<short>
        ///Search
        ///</short>
        ///<param name="query">Search query</param>
        ///<returns>list of topics</returns>
        ///<category>Forums</category>
        [Read("forum/@search/{query}")]
        public IEnumerable<ForumTopicWrapper> SearchTopics(string query)
        {
            int count;
            var topics = ForumDataProvider.SearchTopicsByText(TenantId, query, 0, -1, out count);
            return topics.Select(x => new ForumTopicWrapper(x)).ToSmartList();
        }



        ///<summary>
        /// Deletes a selected post
        ///</summary>
        ///<short>
        /// Delete post
        ///</short>
        ///<param name="postid">Post ID</param>
        ///<returns></returns>
        ///<category>Forums</category>
        [Delete("forum/post/{postid}")]
        public ForumTopicPostWrapper DeletePost(int postid)
        {
            var post = ForumDataProvider.GetPostByID(TenantId, postid);

            if (post == null || !ForumManager.Settings.ForumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, post))
            {
                throw new SecurityException(ForumResource.ErrorAccessDenied);
            }

            var result = RemoveDataHelper.RemovePost(post);

            if(result != DeletePostResult.Successfully)
                throw new Exception("DeletePostResult: " + result);
            
            return new ForumTopicPostWrapper(post);
        }

        ///<summary>
        /// Deletes a selected topic
        ///</summary>
        ///<short>
        /// Delete topic
        ///</short>
        ///<param name="topicid">Topic ID</param>
        ///<returns></returns>
        ///<category>Forums</category>
        [Delete("forum/topic/{topicid}")]
        public ForumTopicWrapper DeleteTopic(int topicid)
        {
            var topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, topicid);

            if (topic == null || !ForumManager.Settings.ForumManager.ValidateAccessSecurityAction(ForumAction.TopicDelete, topic))
            {
                throw new SecurityException(ForumResource.ErrorAccessDenied);
            }

            RemoveDataHelper.RemoveTopic(topic);

            return new ForumTopicWrapper(topic);
        }

        ///<summary>
        /// Deletes a selected thread
        ///</summary>
        ///<short>
        /// Delete thread
        ///</short>
        ///<param name="threadid">Thread ID</param>
        ///<returns></returns>
        ///<category>Forums</category>
        [Delete("forum/thread/{threadid}")]
        public ForumThreadWrapper DeleteThread(int threadid)
        {
            var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, threadid);

            if (thread == null || !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                throw new SecurityException(ForumResource.ErrorAccessDenied);
            }

            RemoveDataHelper.RemoveThread(thread);

            return new ForumThreadWrapper(thread);
        }

        ///<summary>
        /// Deletes a selected thread category
        ///</summary>
        ///<short>
        /// Delete category
        ///</short>
        ///<param name="categoryid">Category ID</param>
        ///<returns></returns>
        ///<category>Forums</category>
        [Delete("forum/category/{categoryid}")]
        public ForumCategoryWrapper DeleteThreadCategory(int categoryid)
        {
            List<Thread> threads;

            var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, categoryid, out threads);

            if (category == null || !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                throw new SecurityException(ForumResource.ErrorAccessDenied);
            }

            RemoveDataHelper.RemoveThreadCategory(category);

            return new ForumCategoryWrapper(category, threads);
        }
    }
}
