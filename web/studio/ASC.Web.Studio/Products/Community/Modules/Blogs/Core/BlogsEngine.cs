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
using System.Linq;
using System.Web;

using ASC.Blogs.Core.Data;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Security;
using ASC.Blogs.Core.Service;
using ASC.Common.Caching;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.ElasticSearch;
using ASC.Web.Community.Search;

namespace ASC.Blogs.Core
{
    public class BlogsEngine
    {
        private const string DbRegistryKey = "community";

        #region fabric

        public static BlogsEngine GetEngine(int tenant)
        {
            if (HttpContext.Current != null)
            {
                var bs = DisposableHttpContext.Current["blogs"] as BlogsStorage;
                if (bs == null)
                {
                    bs = GetStorage(tenant);
                    DisposableHttpContext.Current["blogs"] = bs;
                }
                return new BlogsEngine(tenant, bs);
            }
            return new BlogsEngine(tenant, GetStorage(tenant));
        }

        public static BlogsStorage GetStorage(int tenant)
        {
            return new BlogsStorage(DbRegistryKey, tenant);
        }

        #endregion

        private readonly BlogsStorage _storage;
        private static INotifyClient _notifyClient;
        private static BlogNotifySource _notifySource;

        public BlogsEngine(int tenant, BlogsStorage storage)
        {
            _storage = storage;
            InitNotify();
        }

        #region database

        #region posts

        private List<int> SearchPostsInternal(string searchText)
        {
            List<int> blogs;
            if (FactoryIndexer<BlogsWrapper>.TrySelectIds(r => r.MatchAll(searchText), out blogs))
            {
                return blogs;
            }

            var keyWords = new List<string>(searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            keyWords.RemoveAll(kw => kw.Length < 3);

            var wordResults = new Dictionary<string, int>(keyWords.Count);
            var counts = new Dictionary<int, int>();
            var wordsCount = 0;
            foreach (var word in keyWords.Where(word => !wordResults.ContainsKey(word)))
            {
                wordsCount++;
                var wordResult = _storage.GetPostDao().SearchPostsByWord(word);
                wordResults.Add(word, wordResult.Count);

                wordResult.ForEach(
                    pid =>
                        {
                            if (counts.ContainsKey(pid))
                                counts[pid] = counts[pid] + 1;
                            else
                                counts.Add(pid, 1);
                        });
            }

            return (from kw in counts where kw.Value == wordsCount select kw.Key).ToList();
        }

        public int SearchPostsCount(string searchText)
        {
            if (String.IsNullOrEmpty(searchText))
                throw new ArgumentNullException("searchText");

            var ids = SearchPostsInternal(searchText);
            var forSelectPosts = _storage.GetPostDao().GetPosts(ids, false, false);

            return forSelectPosts.Count;
        }

        public List<Post> SearchPosts(string searchText, PagingQuery paging)
        {
            if (String.IsNullOrEmpty(searchText)) throw new ArgumentNullException("searchText");

            var ids = SearchPostsInternal(searchText);

            var forSelectPosts = _storage.GetPostDao().GetPosts(ids, false, false);
            forSelectPosts.Sort((p1, p2) => DateTime.Compare(p2.Datetime, p1.Datetime));

            var offset = paging.Offset.GetValueOrDefault(0);
            var count = paging.Count.GetValueOrDefault(forSelectPosts.Count);
            if (offset > forSelectPosts.Count) return new List<Post>();
            if (count > forSelectPosts.Count - offset) count = forSelectPosts.Count - offset;

            var result = _storage.GetPostDao().GetPosts(forSelectPosts.GetRange(offset, count).ConvertAll(p => p.ID), true, true);

            result.Sort((p1, p2) => DateTime.Compare(p2.Datetime, p1.Datetime));

            return result;
        }

        public int GetPostsCount(PostsQuery query)
        {
            if (query.SearchText != null)
                return SearchPostsCount(query.SearchText);
            return
                _storage.GetPostDao()
                        .GetCount(
                            null,
                            null,
                            query.UserId,
                            query.Tag);
        }

        public List<Post> SelectPosts(PostsQuery query)
        {
            if (query.SearchText != null)
                return SearchPosts(query.SearchText, new PagingQuery(query));
            
            return
                _storage.GetPostDao()
                        .Select(
                            null,
                            null,
                            query.UserId,
                            query.Tag,
                            query.WithContent,
                            false,
                            query.Offset,
                            query.Count,
                            query.WithTags,
                            false);
        }

        public List<Post> SelectPostsInfo(List<Guid> ids)
        {
            return _storage.GetPostDao().GetPosts(ids, false, false);
        }

        public Post GetPostById(Guid postId)
        {
            return _storage.GetPostDao().Select(postId, null, null, true, true, false).FirstOrDefault();
        }

        public List<Tuple<Post, int>> GetPostsCommentsCount(List<Post> posts)
        {
            var result = new List<Tuple<Post, int>>();
            if (posts.Count == 0) return result;

            var postIds = posts.ConvertAll(p => p.ID);
            var counts = _storage.GetPostDao().GetCommentsCount(postIds);

            result.AddRange(counts.Select((t, i) => Tuple.Create(posts[i], t)));

            return result;
        }

        public void SavePostReview(Post post, Guid userID)
        {
            _storage.GetPostDao()
                    .SavePostReview(userID, post.ID, ASC.Core.Tenants.TenantUtil.DateTimeNow());
        }

        public void SavePost(Post post, bool isNew, bool notifyComments)
        {
            CommunitySecurity.DemandPermissions(
                new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(post.UserID)),
                isNew ? Constants.Action_AddPost : Constants.Action_EditRemovePost);

            _storage.GetPostDao().SavePost(post);
            FactoryIndexer<BlogsWrapper>.IndexAsync(post);

            if (isNew)
            {
                var initiatorInterceptor = new InitiatorInterceptor(new DirectRecipient(post.UserID.ToString(), ""));
                try
                {
                    NotifyClient.BeginSingleRecipientEvent("asc_blog");
                    NotifyClient.AddInterceptor(initiatorInterceptor);

                    var tags = new List<ITagValue>
                        {
                            new TagValue(Constants.TagPostSubject, post.Title),
                            new TagValue(Constants.TagPostPreview,
                                         post.GetPreviewText(500)),
                            new TagValue(Constants.TagUserName,
                                         DisplayUserSettings.GetFullUserName(post.UserID)),
                            new TagValue(Constants.TagUserURL,
                                         CommonLinkUtility.GetFullAbsolutePath(
                                             CommonLinkUtility.GetUserProfile(post.UserID))),
                            new TagValue(Constants.TagDate,
                                         string.Format("{0:d} {0:t}", post.Datetime)),
                            new TagValue(Constants.TagURL,
                                         CommonLinkUtility.GetFullAbsolutePath(
                                             Constants.ViewBlogPageUrl +
                                             "?blogid=" + post.ID.ToString())),
                            GetReplyToTag(Guid.Empty, post)
                        };

                    NotifyClient.SendNoticeAsync(
                        Constants.NewPost,
                        null,
                        null,
                        tags.ToArray());


                    NotifyClient.SendNoticeAsync(
                        Constants.NewPostByAuthor,
                        post.UserID.ToString(),
                        null,
                        tags.ToArray());

                    NotifyClient.EndSingleRecipientEvent("asc_blog");
                }
                finally
                {
                    NotifyClient.RemoveInterceptor(initiatorInterceptor.Name);
                }
            }
            if (!notifyComments) return;
            
            var subscriptionProvider = NotifySource.GetSubscriptionProvider();

            subscriptionProvider.Subscribe(
                Constants.NewComment,
                post.ID.ToString(),
                NotifySource.GetRecipientsProvider().
                             GetRecipient(post.UserID.ToString())
                );
        }

        public void DeletePost(Post post)
        {
            CommunitySecurity.CheckPermissions(post, Constants.Action_EditRemovePost);
            _storage.GetPostDao().DeletePost(post.ID);
            NotifySource.GetSubscriptionProvider().UnSubscribe(
                Constants.NewComment,
                post.ID.ToString()
                );
            FactoryIndexer<BlogsWrapper>.DeleteAsync(post);
            AscCache.Default.Remove("communityScreen" + TenantProvider.CurrentTenantID);
        }

        #endregion

        #region misc

        public List<TagStat> GetTopTagsList(int count)
        {
            return _storage.GetPostDao().GetTagStat(count);
        }

        public List<string> GetTags(string like, int limit)
        {
            return _storage.GetPostDao().GetTags(like, limit);
        }

        #endregion

        #region comments

        public List<Comment> GetPostComments(Guid postId)
        {
            return _storage.GetPostDao().GetComments(postId);
        }

        public Comment GetCommentById(Guid commentId)
        {
            return _storage.GetPostDao().GetCommentById(commentId);
        }

        public void SaveComment(Comment comment, Post post)
        {
            CommunitySecurity.DemandPermissions(post, Constants.Action_AddComment);
            SaveComment(comment);

            var initiatorInterceptor = new InitiatorInterceptor(new DirectRecipient(comment.UserID.ToString(), ""));
            try
            {
                NotifyClient.BeginSingleRecipientEvent("asc_blog_c");
                NotifyClient.AddInterceptor(initiatorInterceptor);

                var tags = new List<ITagValue>
                    {
                        new TagValue(Constants.TagPostSubject, post.Title),
                        new TagValue(Constants.TagPostPreview, post.GetPreviewText(500)),
                        new TagValue(Constants.TagUserName, DisplayUserSettings.GetFullUserName(comment.UserID)),
                        new TagValue(Constants.TagUserURL, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(comment.UserID))),
                        new TagValue(Constants.TagDate, string.Format("{0:d} {0:t}", comment.Datetime)),
                        new TagValue(Constants.TagCommentBody, comment.Content),

                        new TagValue(Constants.TagURL, CommonLinkUtility.GetFullAbsolutePath(Constants.ViewBlogPageUrl + "?blogid=" + post.ID.ToString())),
                        new TagValue(Constants.TagCommentURL, CommonLinkUtility.GetFullAbsolutePath(Constants.ViewBlogPageUrl + "?blogid=" + post.ID.ToString() + "#container_" + comment.ID.ToString())),
                        GetReplyToTag(comment.ID, post)
                    };

                NotifyClient.SendNoticeAsync(
                    Constants.NewComment,
                    post.ID.ToString(),
                    null,
                    tags.ToArray());

                NotifyClient.EndSingleRecipientEvent("asc_blog_c");
            }
            finally
            {
                NotifyClient.RemoveInterceptor(initiatorInterceptor.Name);
            }

            var subscriptionProvider = NotifySource.GetSubscriptionProvider();

            if (!subscriptionProvider.IsUnsubscribe((IDirectRecipient)NotifySource.GetRecipientsProvider().
                                                                                    GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), Constants.NewComment, post.ID.ToString()))
            {
                subscriptionProvider.Subscribe(
                    Constants.NewComment,
                    post.ID.ToString(),
                    NotifySource.GetRecipientsProvider().
                                 GetRecipient(SecurityContext.CurrentAccount.ID.ToString())
                    );
            }
        }

        private static TagValue GetReplyToTag(Guid commentId, Post post)
        {
            return ReplyToTagProvider.Comment("blog", post.ID.ToString(), commentId.ToString());
        }

        private void SaveComment(Comment comment)
        {
            CommunitySecurity.DemandPermissions(comment, Constants.Action_EditRemoveComment);

            if (String.IsNullOrEmpty(comment.Content))
                throw new ArgumentException("comment");
            if (comment.PostId == Guid.Empty)
                throw new ArgumentException("comment");

            _storage.GetPostDao().SaveComment(comment);
        }

        #endregion


        #endregion

        #region notify

        private void InitNotify()
        {
            if (_notifySource == null)
                _notifySource = new BlogNotifySource();
            if (_notifyClient == null)
                _notifyClient = WorkContext.NotifyContext.NotifyService.RegisterClient(_notifySource);
        }

        public BlogNotifySource NotifySource
        {
            get { return _notifySource; }
        }

        public INotifyClient NotifyClient
        {
            get { return _notifyClient; }
        }

        #endregion

        public void UpdateComment(Comment comment, Post post)
        {
            SaveComment(comment);
        }

        public void RemoveComment(Comment comment, Post post)
        {
            SaveComment(comment);
        }
    }


    public class PostsQuery : PagingQuery<PostsQuery>
    {
        internal bool WithTags = true;
        internal bool WithContent = true;
        internal string Tag;
        internal Guid? UserId;
        internal string SearchText;

        public PostsQuery NoTags()
        {
            WithTags = false;
            return this;
        }

        public PostsQuery NoContent()
        {
            WithContent = false;
            return this;
        }

        public PostsQuery SetTag(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                throw new ArgumentException("tag");

            Tag = tag;
            return this;
        }

        public PostsQuery SetUser(Guid userId)
        {
            UserId = userId;
            return this;
        }

        public PostsQuery SetSearch(string searchText)
        {
            SearchText = searchText;
            return this;
        }
    }

    public class PagingQuery : PagingQuery<PagingQuery>
    {
        public PagingQuery()
        {
        }

        public PagingQuery(PostsQuery q)
        {
            Count = q.Count;
            Offset = q.Offset;
        }
    }

    public class PagingQuery<T> where T : class
    {
        internal int? Count;
        internal int? Offset;

        public T SetCount(int count)
        {
            Count = count;
            return this as T;
        }

        public T SetOffset(int offset)
        {
            Offset = offset;
            return this as T;
        }
    }
}