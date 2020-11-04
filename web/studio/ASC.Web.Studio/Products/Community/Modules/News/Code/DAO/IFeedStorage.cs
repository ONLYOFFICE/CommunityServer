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
		
		void RemoveFeed(Feed feed);

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