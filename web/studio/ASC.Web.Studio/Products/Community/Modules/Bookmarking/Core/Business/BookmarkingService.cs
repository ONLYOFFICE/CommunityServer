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


using ASC.Bookmarking.Business.Subscriptions;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Common.Util;
using ASC.Bookmarking.Dao;
using ASC.Bookmarking.Pojo;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Notify;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.ElasticSearch;
using ASC.Web.Community.Search;
using Tag = ASC.Bookmarking.Pojo.Tag;

namespace ASC.Bookmarking.Business
{
	public class BookmarkingService : BookmarkingSessionObject<BookmarkingService>, IBookmarkingService
	{
		#region Fields

	    private BookmarkingHibernateDao dao;

	    public BookmarkingHibernateDao Dao
	    {
            get { return dao ?? (dao = BookmarkingHibernateDao.GetCurrentInstanse()); }
            set { dao = value; }
	    }

		private IList<Tag> tagsList;
		
        public BookmarkingService()
        {
            
        }

        public BookmarkingService(BookmarkingHibernateDao dao)
        {
            this.dao = dao;
            BookmarkingService.UpdateCurrentInstanse(this);
        }

        #endregion

		#region Get Bookmarks
		
        public UserInfo GetCurrentUser()
		{
			return Dao.GetCurrentUser();
		}

		public IList<Bookmark> GetAllBookmarks(int firstResult, int maxResults)
		{
			return Dao.GetAllBookmarks(firstResult, maxResults);
		}

		public IList<Bookmark> GetAllBookmarks()
		{
			return Dao.GetAllBookmarks();
		}

		public Bookmark GetBookmarkByUrl(string url)
		{
			return Dao.GetBookmarkByUrl(url);
		}

		public Bookmark GetBookmarkByID(long id)
		{
			return Dao.GetBookmarkByID(id);
		}
		public long GetBookmarksCountCreatedByUser(Guid userID)
		{
			return Dao.GetBookmarksCountCreatedByUser(userID);
		}

		public UserBookmark GetCurrentUserBookmark(Bookmark b)
		{
			return Dao.GetCurrentUserBookmark(b);
		}

		public IList<Bookmark> GetBookmarksCreatedByUser(Guid userID, int firstResult, int maxResults)
		{
			return Dao.GetBookmarksCreatedByUser(userID, firstResult, maxResults);
		}

		public IList<Bookmark> GetMostPopularBookmarksCreatedByUser(Guid userID, int firstResult, int maxResults)
		{
			var bookmarks = Dao.GetBookmarksCreatedByUser(userID, 0, int.MaxValue);
			var ids = (from b in bookmarks
					   select b.ID).ToList<long>();
			return Dao.GetMostPopularBookmarks(ids, firstResult, maxResults);
		}

		public IList<UserBookmark> GetUserBookmarks(Bookmark b)
		{
			return Dao.GetUserBookmarks(b);
		}

		public Bookmark GetBookmarkWithUserBookmarks(string url)
		{
			return Dao.GetBookmarkWithUserBookmarks(url);
		}
		
        #endregion		

		#region Get Tags
		
        public IList<Tag> GetAllTags(string startSymbols, int limit)
		{
			return Dao.GetAllTags(startSymbols, limit);
		}

		public IList<Tag> GetAllTags()
		{
			tagsList = Dao.GetAllTags(null, Int32.MaxValue);
			return tagsList;
		}
		
        #endregion

		#region Add Bookmark
		
        public Bookmark AddBookmark(Bookmark b, IList<Tag> tags)
		{
			Dao.UpdateBookmark(b, tags);
			SendRecentBookmarkUpdates(b);
			SubscribeOnBookmarkComments(b);
			return b;
		}

	    #endregion

		#region Update Bookmark
		
        public Bookmark UpdateBookmark(UserBookmark userBookmark, IList<Tag> tags)
		{
			Dao.UpdateBookmark(userBookmark, tags);
			var b = GetBookmarkByID(userBookmark.BookmarkID);
			SubscribeOnBookmarkComments(b);
            FactoryIndexer<BookmarksUserWrapper>.IndexAsync(BookmarksUserWrapper.Create(userBookmark, b));
			return b;
		}

		public Bookmark UpdateBookmark(Bookmark bookmark, IList<Tag> tags)
		{
			Dao.UpdateBookmark(bookmark, tags);
			var b = GetBookmarkByID(bookmark.ID);
			return b;
		}

	    #endregion

		#region Remove Bookmark from Favourite
		
        public static string DeletedBookmarkUrl { get; set; }

		public Bookmark RemoveBookmarkFromFavourite(long bookmarkID, Guid? userID = null)
		{
			var b = Dao.GetBookmarkByID(bookmarkID);
			var comments = GetBookmarkComments(b);
			var result = Dao.RemoveBookmarkFromFavourite(bookmarkID, userID);
			if (b != null)
			{
				DeletedBookmarkUrl = b.URL;
				UnSubscribe(b.ID.ToString(CultureInfo.InvariantCulture), BookmarkingBusinessConstants.NotifyActionNewComment);
			}
			if (result == null)
			{
				foreach (var comment in comments)
				{
					CommonControlsConfigurer.FCKUploadsRemoveForItem("bookmarking_comments", comment.ID.ToString());
				}

                AscCache.Default.Remove("communityScreen" + TenantProvider.CurrentTenantID);
			}
			return result;
		}
		#endregion

		#region Sorting
		
        public IList<Bookmark> GetFavouriteBookmarksSortedByRaiting(int firstResult, int maxResults)
		{
			return Dao.GetFavouriteBookmarksSortedByRaiting(firstResult, maxResults);
		}
		public IList<Bookmark> GetFavouriteBookmarksSortedByDate(int firstResult, int maxResults)
		{
			return Dao.GetFavouriteBookmarksSortedByDate(firstResult, maxResults);
		}

		public IList<Bookmark> GetMostRecentBookmarks(int firstResult, int maxResults)
		{
			return Dao.GetMostRecentBookmarks(firstResult, maxResults);
		}

		public IList<Bookmark> GetMostRecentBookmarksWithRaiting(int firstResult, int maxResults)
		{
			return Dao.GetMostRecentBookmarksWithRaiting(firstResult, maxResults);
		}

		public IList<Bookmark> GetTopOfTheDay(int firstResult, int maxResults)
		{
			var d = GetDateTimeToUtcWithShift(-1);
			return Dao.GetBookmarksSortedByRaiting(d, firstResult, maxResults);
		}

		public IList<Bookmark> GetTopOfTheWeek(int firstResult, int maxResults)
		{
			var d = GetDateTimeToUtcWithShift(-7);
			return Dao.GetBookmarksSortedByRaiting(d, firstResult, maxResults);
		}

		public IList<Bookmark> GetTopOfTheMonth(int firstResult, int maxResults)
		{
			var d = GetDateTimeToUtcWithShift(-31);
			return Dao.GetBookmarksSortedByRaiting(d, firstResult, maxResults);
		}

		public IList<Bookmark> GetTopOfTheYear(int firstResult, int maxResults)
		{
			var d = GetDateTimeToUtcWithShift(-365);
			return Dao.GetBookmarksSortedByRaiting(d, firstResult, maxResults);
		}

		private static DateTime GetDateTimeToUtcWithShift(int days)
		{
			var now = DateTime.UtcNow;
			return now.Date.AddDays(days);
		}
		
		#endregion

		#region Tags
		private static IList<Tag> SortTagsCloud(List<Tag> tags)
		{
			tags.Sort((t1, t2) => string.CompareOrdinal(t1.Name, t2.Name));
			return tags;
		}


		public IList<Bookmark> GetMostPopularBookmarksByTag(Tag t)
		{
			return Dao.GetMostPopularBookmarksByTag(t, BookmarkingBusinessConstants.MostPopularBookmarksByTagLimit);
		}

		public IList<Bookmark> GetMostPopularBookmarksByTag(IList<Tag> tags)
		{
			return Dao.SearchMostPopularBookmarksByTag(tags, 0, Int32.MaxValue);
		}

		public IList<Tag> GetBookmarkTags(Bookmark b)
		{
			return Dao.GetBookmarkTags(b);
		}

		public IList<Tag> GetUserBookmarkTags(UserBookmark ub)
		{
			return Dao.GetUserBookmarkTags(ub);
		}
		#endregion

		#region Comments

		public Comment GetCommentById(Guid commentID)
		{
			return Dao.GetCommentById(commentID);
		}

		public void AddComment(Comment comment)
		{
			var c = Dao.AddComment(comment);

			if (c == null)
			{
				return;
			}
			var b = GetCommentBookmark(c);

			SendCommentUpdates(c, b);
			SubscribeOnBookmarkComments(b, true);
		}

		public void UpdateComment(Guid commentID, string text)
		{
			Dao.UpdateComment(commentID, text);
		}

		public void RemoveComment(Guid commentID)
		{
			Dao.RemoveComment(commentID);
		}

		public long GetCommentsCount(long bookmarkID)
		{
			return Dao.GetCommentsCount(bookmarkID);
		}

		public IList<Comment> GetBookmarkComments(Bookmark b)
		{
			return Dao.GetBookmarkComments(b);
		}

		private Bookmark GetCommentBookmark(Comment c)
		{
			return Dao.GetCommentBookmark(c);
		}

		public IList<Comment> GetChildComments(Comment c)
		{
			return Dao.GetChildComments(c);
		}

		#endregion

		#region Notifications
		private static void SendCommentUpdates(Comment c, Bookmark b)
		{
			if (c == null || b == null)
			{
				return;
			}
			var url = ModifyBookmarkUrl(b);
			var tags = new[]{
					new TagValue(BookmarkSubscriptionConstants.BookmarkTitle, b.Name),
					new TagValue(BookmarkSubscriptionConstants.BookmarkUrl, url),
					new TagValue(BookmarkSubscriptionConstants.UserURL,
													 BookmarkingBusinessUtil.RenderProfileLink(c.UserID)),
					new TagValue(BookmarkSubscriptionConstants.Date, c.Datetime.ToShortString()),
					new TagValue(BookmarkSubscriptionConstants.CommentBody, c.Content),GetReplyToTag(b,c)
							};

			var objectID = b.ID.ToString(CultureInfo.InvariantCulture);

			var action = BookmarkingBusinessConstants.NotifyActionNewComment;

			SendBookmarkNoticeAsync(action, objectID, tags);
		}

		private void SendRecentBookmarkUpdates(Bookmark b)
		{
			var url = ModifyBookmarkUrl(b);
		    var tags = new[]
		                   {
		                       new TagValue(BookmarkSubscriptionConstants.BookmarkTitle, b.Name),
		                       new TagValue(BookmarkSubscriptionConstants.BookmarkUrl, url),
		                       new TagValue(BookmarkSubscriptionConstants.UserURL,
		                                    string.Format("\"{0}\":\"{1}\"",
		                                                  CommonLinkUtility.GetFullAbsolutePath(
		                                                      CommonLinkUtility.GetUserProfile(b.UserCreatorID)),
		                                                  DisplayUserSettings.GetFullUserName(b.UserCreatorID))),
		                       //TODO: Rewrite patterns
		                       new TagValue(BookmarkSubscriptionConstants.Date, b.Date.ToShortString()),
		                       new TagValue(BookmarkSubscriptionConstants.BookmarkDescription, b.Description),
		                       GetReplyToTag(b, null)
		                   };

			const string objectID = BookmarkingBusinessConstants.SubscriptionRecentBookmarkID;

			var action = BookmarkingBusinessConstants.NotifyActionNewBookmark;

			SendBookmarkNoticeAsync(action, objectID, tags);
		}

        private static TagValue GetReplyToTag(Bookmark bookmark, Comment comment)
        {
            return ReplyToTagProvider.Comment("bookmark", bookmark.ID.ToString(CultureInfo.InvariantCulture), comment != null ? comment.ID.ToString() : null);
        }

		internal static string ModifyBookmarkUrl(Bookmark b)
		{
			var bookmarkUrl = HttpUtility.UrlEncode(HttpUtility.HtmlDecode(b.URL));
			var bookmarkingAbsolutePath = CommonLinkUtility.GetFullAbsolutePath(BookmarkingBusinessConstants.BookmarkingBasePath + "/BookmarkInfo.aspx");
			
			var url = string.Format("{0}?Url={1}", bookmarkingAbsolutePath, bookmarkUrl);
			return url;
		}

		private static void SendBookmarkNoticeAsync(INotifyAction action, string objectID, TagValue[] tags)
		{
			var initatorInterceptor = new InitiatorInterceptor(GetCurrentRecipient());
			var notifyClient = BookmarkingNotifyClient.NotifyClient;
			try
			{
				notifyClient.AddInterceptor(initatorInterceptor);
				notifyClient.SendNoticeAsync(action, objectID, null, tags);
			}
			finally
			{
				notifyClient.RemoveInterceptor(initatorInterceptor.Name);
			}
		}

		private static IRecipient GetCurrentRecipient()
		{
			return GetRecipient(SecurityContext.CurrentAccount.ID);
		}

		private static IRecipient GetRecipient(Guid userID)
		{
		    var user = CoreContext.UserManager.GetUsers(userID);
            return new DirectRecipient(user.ID.ToString(), user.ToString());
		}

		#endregion

		#region Subscriptions
		public void Subscribe(string objectID, INotifyAction notifyAction)
		{
			var provider = BookmarkingNotifySource.Instance.GetSubscriptionProvider();
			provider.Subscribe(notifyAction, objectID, GetCurrentRecipient());
		}

		public bool IsSubscribed(string objectID, INotifyAction notifyAction)
		{
			var provider = BookmarkingNotifySource.Instance.GetSubscriptionProvider();
			var isSubscribed = provider.IsSubscribed(notifyAction, GetCurrentRecipient(), objectID);
			return isSubscribed;
		}

		public void UnSubscribe(string objectID, INotifyAction notifyAction, Guid? userID = null)
		{
			var provider = BookmarkingNotifySource.Instance.GetSubscriptionProvider();
            provider.UnSubscribe(notifyAction, objectID, GetRecipient(userID ?? SecurityContext.CurrentAccount.ID));
		}

		private void SubscribeOnBookmarkComments(Bookmark b)
		{
			SubscribeOnBookmarkComments(b, false);
		}

		private void SubscribeOnBookmarkComments(Bookmark b, bool checkIfUnsubscribed)
		{
			if (b == null)
			{
				return;
			}
			var id = b.ID.ToString();
			var provider = BookmarkingNotifySource.Instance.GetSubscriptionProvider();
			var subsribe = true;
			if (checkIfUnsubscribed)
			{
				if (provider.IsUnsubscribe(GetCurrentRecipient() as DirectRecipient, BookmarkingBusinessConstants.NotifyActionNewComment, id))
				{
					subsribe = false;
				}
			}
			if (subsribe)
			{
				provider.Subscribe(BookmarkingBusinessConstants.NotifyActionNewComment, id, GetCurrentRecipient());
			}
		}
		#endregion

		#region Search

		public IList<Bookmark> SearchBookmarks(IList<string> searchStringList, int firstResult, int maxResults)
		{
			return Dao.SearchBookmarks(searchStringList, firstResult, maxResults);
		}

		public IList<Bookmark> SearchAllBookmarks(IList<string> searchStringList)
		{
			return Dao.SearchBookmarks(searchStringList, 0, Int32.MaxValue, true);
		}

		public IList<Bookmark> SearchBookmarksSortedByRaiting(IList<string> searchStringList, int firstResult, int maxResults)
		{
			var bookmarks = Dao.SearchBookmarks(searchStringList, 0, int.MaxValue);
			var ids = (from b in bookmarks
					   select b.ID).ToList<long>();
			return Dao.GetMostPopularBookmarks(ids, firstResult, maxResults);
		}

		public IList<Bookmark> SearchBookmarksByTag(string searchString, int firstResult, int maxResults)
		{
			return Dao.SearchBookmarksByTag(searchString, firstResult, maxResults);
		}

		public IList<Bookmark> SearchMostPopularBookmarksByTag(string tagName, int firstResult, int maxResults)
		{
			return Dao.SearchMostPopularBookmarksByTag(tagName, firstResult, maxResults);
		}

		#endregion

		#region IBookmarkingService Members


		public IList<Bookmark> GetFullBookmarksInfo(IList<long> bookmarkIds)
		{
			return Dao.GetFullBookmarksInfo(bookmarkIds);
		}

		public IList<Bookmark> GetFullBookmarksInfo(IList<Bookmark> bookmarks)
		{
			if (bookmarks == null || bookmarks.Count == 0)
			{
				return new List<Bookmark>();
			}
			var ids = (from b in bookmarks
					   select b.ID).Distinct<long>().ToList<long>();
			return GetFullBookmarksInfo(ids);
		}

		#endregion

		#region IBookmarkingService Members


		public void SetBookmarkTags(Bookmark b)
		{
			Dao.SetBookmarkTags(b);
		}

		#endregion

        public IList<Bookmark> GetBookmarks(DateTime from, DateTime to)
        {
            return Dao.GetBookmarksUpdates(from, to);
        }

        public IList<Comment> GetComments(DateTime from, DateTime to)
        {
            return Dao.GetCommentsUpdates(from, to);
        }
	}
}