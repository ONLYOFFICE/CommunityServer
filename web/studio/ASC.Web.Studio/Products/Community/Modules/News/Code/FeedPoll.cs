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


#region Usings

using System;
using System.Collections.Generic;
using ASC.Core.Tenants;
using ASC.Web.Community.Product;

#endregion

namespace ASC.Web.Community.News.Code
{
	[Serializable]
	public class FeedPoll : Feed
	{
		public FeedPollType PollType { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public List<FeedPollVariant> Variants { get; private set;}

		internal List<FeedPollAnswer> Answers;


		public FeedPoll()
		{
			FeedType = FeedType.Poll;
			PollType = FeedPollType.SimpleAnswer;
            StartDate = TenantUtil.DateTimeNow();
			EndDate = StartDate.AddYears(100);
			Variants = new List<FeedPollVariant>();
			Answers = new List<FeedPollAnswer>();
		}

		public int GetVariantVoteCount(long variantId)
		{
			return Answers.FindAll(a => a.VariantId == variantId).Count;
		}

		public bool IsUserVote(string userId)
		{
		    if (CommunitySecurity.IsOutsider()) return true;

            return Answers.Exists(a => a.UserId == userId);
		}
	}
}
