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

namespace ASC.Web.Community.News.Code.DAO
{
	public interface IFeedStorage : IDisposable
	{
		List<FeedType> GetUsedFeedTypes();

		
		List<Feed> GetFeeds(FeedType feedType, Guid userId, int count, int offset);

		List<Feed> SearchFeeds(string s, FeedType feedType, Guid userId, int count, int offset);

		long GetFeedsCount(FeedType feedType, Guid userId);

		long SearchFeedsCount(string s, FeedType feedType, Guid userId);

		List<Feed> SearchFeeds(string s);

		Feed GetFeed(long id);

	    List<Feed> GetFeedByDate(DateTime from, DateTime to, Guid userId);

	    List<FeedComment> GetCommentsByDate(DateTime from, DateTime to);

		Feed SaveFeed(Feed feed, bool isEdit, FeedType poll);
		
		void RemoveFeed(long id);

		void ReadFeed(long feedId, string reader);

		void PollVote(string userId, ICollection<long> variantIds);

		
		List<FeedComment> GetFeedComments(long feedId);
		
		FeedComment GetFeedComment(long commentId);
		
		void RemoveFeedComment(long commentId);
        FeedComment SaveFeedComment(Feed feed, FeedComment comment);
        void RemoveFeedComment(FeedComment comment);
        void UpdateFeedComment(FeedComment comment);
    }
}