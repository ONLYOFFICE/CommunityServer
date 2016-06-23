/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region Usings

using System;
using System.Collections.Generic;
using ASC.Bookmarking.Pojo;
using ASC.Core.Users;
using ASC.Notify.Model;

#endregion

namespace ASC.Bookmarking.Business
{
	public interface IBookmarkingService
	{
		IList<Bookmark> GetAllBookmarks(int firstResult, int maxResults);
		IList<Bookmark> GetAllBookmarks();

		Bookmark AddBookmark(Bookmark b, IList<Tag> tags);

		Bookmark UpdateBookmark(UserBookmark userBookmark, IList<Tag> tags);
		Bookmark UpdateBookmark(Bookmark bookmark, IList<Tag> tags);
		
		UserInfo GetCurrentUser();		
		
		IList<Tag> GetAllTags(string startSymbols, int limit);

		IList<Tag> GetAllTags();

		Bookmark GetBookmarkByUrl(string url);

		Bookmark GetBookmarkByID(long id);


		IList<UserBookmark> GetUserBookmarks(Bookmark b);

		UserBookmark GetCurrentUserBookmark(Bookmark b);

		Bookmark GetBookmarkWithUserBookmarks(string url);



		Bookmark RemoveBookmarkFromFavourite(long bookmarkID, Guid? userID = null);

		IList<Bookmark> GetFavouriteBookmarksSortedByRaiting(int firstResult, int maxResults);
		IList<Bookmark> GetFavouriteBookmarksSortedByDate(int firstResult, int maxResults);


		IList<Bookmark> GetMostRecentBookmarks(int firstResult, int maxResults);
		IList<Bookmark> GetMostRecentBookmarksWithRaiting(int firstResult, int maxResults);
		IList<Bookmark> GetTopOfTheDay(int firstResult, int maxResults);
		IList<Bookmark> GetTopOfTheWeek(int firstResult, int maxResults);
		IList<Bookmark> GetTopOfTheMonth(int firstResult, int maxResults);
		IList<Bookmark> GetTopOfTheYear(int firstResult, int maxResults);

		#region Tags		

		IList<Bookmark> GetMostPopularBookmarksByTag(Tag t);
		IList<Bookmark> GetMostPopularBookmarksByTag(IList<Tag> tags);
		IList<Tag> GetBookmarkTags(Bookmark b);
		IList<Tag> GetUserBookmarkTags(UserBookmark b);
		
        #endregion

		#region Comments
		
        Comment GetCommentById(Guid commentID);

		void AddComment(Comment comment);

		void UpdateComment(Guid commentID, string text);

		void RemoveComment(Guid commentID);

		long GetCommentsCount(long bookmarkID);

		IList<Comment> GetBookmarkComments(Bookmark b);

		IList<Comment> GetChildComments(Comment c);
		
        #endregion

		void Subscribe(string objectID, INotifyAction notifyAction);

		bool IsSubscribed(string objectID, INotifyAction notifyAction);

		void UnSubscribe(string objectID, INotifyAction notifyAction, Guid? userID = null);

		#region Search
		
        IList<Bookmark> SearchBookmarks(IList<string> searchStringList, int firstResult, int maxResults);

		IList<Bookmark> SearchAllBookmarks(IList<string> searchStringList);

		IList<Bookmark> SearchBookmarksSortedByRaiting(IList<string> searchStringList, int firstResult, int maxResults);

		IList<Bookmark> SearchBookmarksByTag(string searchString, int firstResult, int maxResults);

		IList<Bookmark> SearchMostPopularBookmarksByTag(string tagName, int firstResult, int maxResults);
		
        #endregion

		IList<Bookmark> GetBookmarksCreatedByUser(Guid userID, int firstResult, int maxResults);

		IList<Bookmark> GetMostPopularBookmarksCreatedByUser(Guid userID, int firstResult, int maxResults);

		long GetBookmarksCountCreatedByUser(Guid userID);


		IList<Bookmark> GetFullBookmarksInfo(IList<long> bookmarkIds);

		IList<Bookmark> GetFullBookmarksInfo(IList<Bookmark> bookmarks);

		void SetBookmarkTags(Bookmark b);
	}
}
