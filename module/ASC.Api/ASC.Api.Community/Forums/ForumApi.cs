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
using ASC.Api.Collections;
using ASC.Api.Forums;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Forum;
using ASC.Web.Community.Forum;
using ASC.Web.Community.Modules.Forum.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Community
{

    //TODO: Add html decoding to some fields!!! 
    ///<name>community</name>
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
        ///Returns a list of all the portal forums with the topic/thread titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>
        ///Get forums
        ///</short>
        ///<returns type="ASC.Api.Forums.ForumWrapper, ASC.Api.Community">List of forums</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum</path>
        ///<httpMethod>GET</httpMethod>
        [Read("forum")]
        public ForumWrapper GetForums()
        {
            List<ThreadCategory> categories;
            List<Thread> threads;
            Forum.ForumDataProvider.GetThreadCategories(TenantId, false, out categories, out threads);
            return new ForumWrapper(categories, threads);
        }

        ///<summary>
        ///Returns a number of all the portal forums.
        ///</summary>
        ///<short>
        ///Count forums
        ///</short>
        ///<returns>Number of forums</returns>
        ///<visible>false</visible>
        ///<category>Forums</category>
        /// <path>api/2.0/community/forum/count</path>
        /// <httpMethod>GET</httpMethod>
        [Read("forum/count")]
        public int GetForumsCount()
        {
            return Forum.ForumDataProvider.GetThreadCategoriesCount(TenantId);
        }

        ///<summary>
        ///Returns a list of all the thread topics with the topic titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>
        ///Get thread topics
        ///</short>
        ///<param type="System.Int32, System" method="url" name="threadid">Thread ID</param>
        ///<returns type="ASC.Api.Forums.ForumThreadWrapperFull, ASC.Api.Community">List of thread topics</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/{threadid}</path>
        ///<httpMethod>GET</httpMethod>
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
        ///Returns a list of all the recently updated topics in the portal forums with the topic titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>
        ///Get recently updated topics
        ///</short>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapper, ASC.Api.Community">List of recently updated topics</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/recent</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("forum/topic/recent")]
        public IEnumerable<ForumTopicWrapper> GetLastTopics()
        {
            var result = ForumDataProvider.GetLastUpdateTopics(TenantId, (int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return result.Select(x => new ForumTopicWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all the posts of the selected forum topic with the dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>
        ///Get topic posts
        ///</short>
        ///<param type="System.Int32, System" method="url" name="topicid">Topic ID</param>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapperFull, ASC.Api.Community">List of topic posts</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/{topicid}</path>
        ///<httpMethod>GET</httpMethod>
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
        /// Adds a thread to the category with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Add a thread to a category
        ///</short>
        /// <param type="System.Int32, System" name="categoryId">Category ID (-1 for a new category)</param>
        /// <param type="System.String, System" name="categoryName">Category name</param>
        /// <param type="System.String, System" name="threadName">Thread name</param>
        /// <param type="System.String, System" name="threadDescription">Thread description</param>
        ///<returns type="ASC.Api.Forums.ForumThreadWrapper, ASC.Api.Community">Added thread</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum</path>
        ///<httpMethod>POST</httpMethod>
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
        /// Adds a new topic to the existing thread with a subject, content and topic type specified in the request.
        ///</summary>
        ///<short>
        /// Add a topic to a thread
        ///</short>
        /// <param type="System.Int32, System" method="url" name="threadid">Thread ID</param>
        /// <param type="System.String, System" name="subject">Topic subject</param>
        /// <param type="System.String, System" name="content">Topic text</param>
        /// <param type="ACS.Forum.TopicType, ACS.Forum" name="topicType">Topic type</param>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapperFull, ASC.Api.Community">Added topic</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/{threadid}</path>
        ///<httpMethod>POST</httpMethod>
        [Create("forum/{threadid}")]
        public ForumTopicWrapperFull AddTopic(int threadid, string subject, string content, TopicType topicType)
        {
            var id = ForumDataProvider.CreateTopic(TenantId, threadid, subject, topicType);
            ForumDataProvider.CreatePost(TenantId, id, 0, subject, content, true,
                                         PostTextFormatter.BBCode);
            return GetTopicPosts(id);
        }

        ///<summary>
        /// Updates a topic with the ID specified in the request, changing a topic subject, making it sticky, or closing it.
        ///</summary>
        ///<short>
        /// Update a topic
        ///</short>
        /// <param type="System.Int32, System" method="url" name="topicid">Topic ID</param>
        /// <param type="System.String, System" name="subject">New subject</param>
        /// <param type="System.Boolean, System" name="sticky">Makes a topic sticky</param>
        /// <param type="System.Boolean, System" name="closed">Closes a topic</param>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapperFull, ASC.Api.Community">Updated topic</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/{topicid}</path>
        ///<httpMethod>PUT</httpMethod>
        [Update("forum/topic/{topicid}")]
        public ForumTopicWrapperFull UpdateTopic(int topicid, string subject, bool sticky, bool closed)
        {
            ForumDataProvider.UpdateTopic(TenantId, topicid, subject, sticky, closed);
            return GetTopicPosts(topicid);
        }
        ///<summary>
        /// Adds a post to the selected topic with a post subject and content specified in the request.
        ///</summary>
        ///<short>
        /// Add a post to a topic
        ///</short>
        ///<param type="System.Int32, System" method="url" name="topicid">Topic ID</param>
        ///<param type="System.Int32, System" name="parentPostId">Parent post ID</param>
        ///<param type="System.String, System" name="subject">Post subject (required)</param>
        ///<param type="System.String, System" name="content">Post text</param>
        ///<returns type="ASC.Api.Forums.ForumTopicPostWrapper, ASC.Api.Community">New post</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/{topicid}</path>
        ///<httpMethod>POST</httpMethod>
        [Create("forum/topic/{topicid}")]
        public ForumTopicPostWrapper AddTopicPosts(int topicid, int parentPostId, string subject, string content)
        {
            var id = ForumDataProvider.CreatePost(TenantId, topicid, parentPostId, subject, content, true,
                                         PostTextFormatter.BBCode);
            return new ForumTopicPostWrapper(ForumDataProvider.GetPostByID(TenantId, id));
        }

        ///<summary>
        /// Updates a post in the selected topic changing the post subject or/and content specified in the request.
        ///</summary>
        ///<short>
        /// Update a topic post
        ///</short>
        ///<param type="System.Int32, System" method="url" name="topicid">Topic ID</param>
        ///<param type="System.Int32, System" method="url" name="postid">Post ID</param>
        ///<param type="System.String, System" name="subject">New post subject (required)</param>
        ///<param type="System.String, System" name="content">New post text</param>
        ///<returns type="ASC.Api.Forums.ForumTopicPostWrapper, ASC.Api.Community">Updated post</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/{topicid}/{postid}</path>
        ///<httpMethod>PUT</httpMethod>
        [Update("forum/topic/{topicid}/{postid}")]
        public ForumTopicPostWrapper UpdateTopicPosts(int topicid, int postid, string subject, string content)
        {
            ForumDataProvider.UpdatePost(TenantId, postid, subject, content, PostTextFormatter.BBCode);
            return new ForumTopicPostWrapper(ForumDataProvider.GetPostByID(TenantId, postid));
        }

        ///<summary>
        ///Returns a list of topics matching the search query specified in the request with the topic titles, dates of creation and update, post texts, and authors.
        ///</summary>
        ///<short>
        ///Search topics
        ///</short>
        ///<param type="System.String, System" method="url" name="query">Search query</param>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapper, ASC.Api.Community">List of topics</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/@search/{query}</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("forum/@search/{query}")]
        public IEnumerable<ForumTopicWrapper> SearchTopics(string query)
        {
            int count;
            var topics = ForumDataProvider.SearchTopicsByText(TenantId, query, 0, -1, out count);
            return topics.Select(x => new ForumTopicWrapper(x)).ToSmartList();
        }



        ///<summary>
        /// Deletes a post with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Delete a post
        ///</short>
        ///<param type="System.Int32, System" method="url" name="postid">Post ID</param>
        ///<returns type="ASC.Api.Forums.ForumTopicPostWrapper, ASC.Api.Community">Post</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/post/{postid}</path>
        ///<httpMethod>DELETE</httpMethod>
        [Delete("forum/post/{postid}")]
        public ForumTopicPostWrapper DeletePost(int postid)
        {
            var post = ForumDataProvider.GetPostByID(TenantId, postid);

            if (post == null || !ForumManager.Settings.ForumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, post))
            {
                throw new SecurityException(ForumResource.ErrorAccessDenied);
            }

            var result = RemoveDataHelper.RemovePost(post);

            if (result != DeletePostResult.Successfully)
                throw new Exception("DeletePostResult: " + result);

            return new ForumTopicPostWrapper(post);
        }

        ///<summary>
        /// Deletes a topic with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Delete a topic
        ///</short>
        ///<param type="System.Int32, System" method="url" name="topicid">Topic ID</param>
        ///<returns type="ASC.Api.Forums.ForumTopicWrapper, ASC.Api.Community">Topic</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/topic/{topicid}</path>
        ///<httpMethod>DELETE</httpMethod>
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
        /// Deletes a thread with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Delete a thread
        ///</short>
        ///<param type="System.Int32, System" method="url" name="threadid">Thread ID</param>
        ///<returns type="ASC.Api.Forums.ForumThreadWrapper, ASC.Api.Community">Thread</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/thread/{threadid}</path>
        ///<httpMethod>DELETE</httpMethod>
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
        /// Deletes a category with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Delete a category
        ///</short>
        ///<param type="System.Int32, System" method="url" name="categoryid">Category ID</param>
        ///<returns type="ASC.Api.Forums.ForumCategoryWrapper, ASC.Api.Community">Category</returns>
        ///<category>Forums</category>
        ///<path>api/2.0/community/forum/category/{categoryid}</path>
        ///<httpMethod>DELETE</httpMethod>
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
