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

using ASC.Core;
using ASC.Web.Community.Modules.News.Resources;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Studio.UserControls.Common.PollForm;

namespace ASC.Web.Community.News.Code
{
    public class PollVoteHandler : IVoteHandler
    {
        #region IVoteHandler Members

        public bool VoteCallback(string pollID, List<string> selectedVariantIDs, string additionalParams, out string errorMessage)
        {
            errorMessage = string.Empty;
            var userAnswersIDs = new List<long>();
            selectedVariantIDs.ForEach(strId => { if (!string.IsNullOrEmpty(strId)) userAnswersIDs.Add(Convert.ToInt64(strId)); });
            long pollId = Convert.ToInt64(additionalParams);
            var storage = FeedStorageFactory.Create();
            var feed = storage.GetFeed(pollId);

            return VoteForPoll(userAnswersIDs, storage, feed, out errorMessage);
        }

        public static bool VoteForPoll(List<long> userAnswersIDs, IFeedStorage storage, Feed feed, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (feed == null)
            {
                errorMessage = "Item not found";
                return false;
            }

            if (feed.FeedType != FeedType.Poll)
            {
                errorMessage = "Feed is not a poll";
                return false;
            }

            var poll = feed as FeedPoll;
            var currentAccountId = SecurityContext.CurrentAccount.ID.ToString();

            if (poll.IsUserVote(currentAccountId))
            {
                errorMessage = "User already voted";
                return false;
            }

            if (!userAnswersIDs.Any() || (poll.PollType == FeedPollType.SimpleAnswer && userAnswersIDs.Count > 1))
            {
                errorMessage = "Invalid number of answers";
                return false;
            }

            storage.PollVote(currentAccountId, userAnswersIDs);
            return true;
        }

        #endregion
    }
}
