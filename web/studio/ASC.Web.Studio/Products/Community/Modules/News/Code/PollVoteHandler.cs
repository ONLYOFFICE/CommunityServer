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


using System;
using System.Collections.Generic;
using ASC.Core;
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

			return VoteForPoll(userAnswersIDs, storage, pollId, out errorMessage);
		}

	    public static bool VoteForPoll(List<long> userAnswersIDs, IFeedStorage storage, long pollId, out string errorMessage)
	    {
	        errorMessage = string.Empty;
	        storage.PollVote(SecurityContext.CurrentAccount.ID.ToString(), userAnswersIDs);
	        var pollFeed = storage.GetFeed(pollId);
	        if (pollFeed == null)
	        {
	            errorMessage = Resources.NewsResource.ErrorAccessDenied;
	            return false;
	        }
	        return true;
	    }

	    #endregion
	}
}
