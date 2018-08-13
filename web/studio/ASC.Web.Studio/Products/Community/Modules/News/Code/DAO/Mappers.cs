/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Core.Tenants;

#endregion

namespace ASC.Web.Community.News.Code.DAO
{
	static class Mappers
	{
		public static Feed ToFeed(object[] row)
		{
		    return new Feed
		               {
		                   Id = Convert.ToInt64(row[0]),
		                   FeedType = (FeedType) Convert.ToInt32(row[1]),
                           Caption = Convert.ToString(row[2]),
		                   Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])),
		                   Creator = Convert.ToString(row[4]),
		                   Readed = Convert.ToBoolean(row[5])
		               };
		}

        public static Feed ToFeed2(object[] row)
        {
            return new Feed
            {
                Id = Convert.ToInt64(row[0]),
                FeedType = (FeedType)Convert.ToInt32(row[1]),
                Caption = Convert.ToString(row[2]),
                Text = Convert.ToString(row[3]),
                Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[4])),
                Creator = Convert.ToString(row[5]),
                Readed = Convert.ToBoolean(row[6])
            };
        }

		public static Feed ToFeedFinded(object[] row)
		{
		    return new Feed
		               {
		                   Id = Convert.ToInt64(row[0]),
		                   Caption = Convert.ToString(row[1]),
		                   Text = Convert.ToString(row[2]),
		                   Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3]))
		               };
		}

		public static Feed ToNewsOrPoll(object[] row)
		{
			var feedType = (FeedType)Convert.ToInt32(row[1]);
			var feed = feedType == FeedType.Poll ? new FeedPoll() : (Feed)new FeedNews();

			feed.Id = Convert.ToInt64(row[0]);
			feed.FeedType = feedType;
			feed.Caption = Convert.ToString(row[2]);
			feed.Text = Convert.ToString(row[3]);
			feed.Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[4]));
			feed.Creator = Convert.ToString(row[5]);
			feed.Readed = Convert.ToBoolean(row[6]);
			
            return feed;
		}

		public static FeedComment ToFeedComment(object[] row)
		{
		    return new FeedComment(Convert.ToInt64(row[1]))
		               {
		                   Id = Convert.ToInt64(row[0]),
		                   Comment = Convert.ToString(row[2]),
		                   ParentId = Convert.ToInt64(row[3]),
		                   Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[4])),
		                   Creator = Convert.ToString(row[5]),
		                   Inactive = Convert.ToBoolean(row[6]),
		               };
		}

        public static FeedComment ToFeedComment(object[] row, bool withFeed)
        {
            if (!withFeed) return ToFeedComment(row);

            var feed = new Feed
                           {
                               Id = Convert.ToInt64(row[6]),
                               FeedType = (FeedType) Convert.ToInt32(row[7]),
                               Caption = Convert.ToString(row[8]),
                               Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[9])),
                               Creator = Convert.ToString(row[10]),
                               Readed = Convert.ToBoolean(row[11])
                           };

            return new FeedComment(Convert.ToInt64(row[6]), feed)
            {
                Id = Convert.ToInt64(row[0]),
                Comment = Convert.ToString(row[1]),
                ParentId = Convert.ToInt64(row[2]),
                Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])),
                Creator = Convert.ToString(row[4]),
                Inactive = Convert.ToBoolean(row[5]),
            };
        }
	}
}
