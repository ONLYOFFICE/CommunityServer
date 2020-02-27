/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Feed;
using ASC.Feed.Data;
using ASC.Specific;
using System;
using System.Linq;

namespace ASC.Api.Feed
{
    public class FeedApi : IApiEntryPoint
    {
        private const string newFeedsCountCacheKey = "newfeedscount";
        private readonly ICache newFeedsCountCache = AscCache.Memory;
        
        private static string GetNewFeedsCountKey()
        {
            return newFeedsCountCacheKey + SecurityContext.CurrentAccount.ID;
        }

        private readonly ApiContext context;

        public string Name
        {
            get { return "feed"; }
        }

        public FeedApi(ApiContext context)
        {
            this.context = context;
        }

        [Update("/read")]
        public void Read()
        {
            new FeedReadedDataProvider().SetTimeReaded(SecurityContext.CurrentAccount.ID);
            newFeedsCountCache.Remove(GetNewFeedsCountKey());
        }

        [Read("/filter")]
        public object GetFeed(
            string product,
            ApiDateTime from,
            ApiDateTime to,
            Guid? author,
            bool? onlyNew,
            ApiDateTime timeReaded)
        {
            var filter = new FeedApiFilter
                {
                    Product = product,
                    Offset = (int)context.StartIndex,
                    Max = (int)context.Count - 1,
                    Author = author ?? Guid.Empty,
                    SearchKeys = context.FilterValues,
                    OnlyNew = onlyNew.HasValue && onlyNew.Value
                };

            if (from != null && to != null)
            {
                var f = TenantUtil.DateTimeFromUtc(from.UtcTime);
                filter.From = new DateTime(f.Year, f.Month, f.Day, 0, 0, 0);

                var t = TenantUtil.DateTimeFromUtc(to.UtcTime);
                filter.To = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59);
            }
            else
            {
                filter.From = from != null ? from.UtcTime : DateTime.MinValue;
                filter.To = to != null ? to.UtcTime : DateTime.MaxValue;
            }

            var feedReadedProvider = new FeedReadedDataProvider();
            var lastTimeReaded = feedReadedProvider.GetTimeReaded();

            var readedDate = timeReaded != null ? (ApiDateTime)timeReaded.UtcTime : (ApiDateTime)lastTimeReaded;

            if (filter.OnlyNew)
            {
                filter.From = (ApiDateTime)lastTimeReaded;
                filter.Max = 100;
            }
            else if (timeReaded == null)
            {
                feedReadedProvider.SetTimeReaded();
                newFeedsCountCache.Remove(GetNewFeedsCountKey());
            }

            var feeds = FeedAggregateDataProvider
                .GetFeeds(filter)
                .GroupBy(n => n.GroupId,
                         n => new FeedWrapper(n),
                         (n, group) =>
                             {
                                 var firstFeed = group.First();
                                 firstFeed.GroupedFeeds = group.Skip(1);
                                 return firstFeed;
                             })
                .ToList();

            context.SetDataPaginated();
            return new {feeds, readedDate};
        }

        [Read("/newfeedscount")]
        public object GetFreshNewsCount()
        {
            var cacheKey = GetNewFeedsCountKey();
            var resultfromCache = newFeedsCountCache.Get<String>(cacheKey);
            int result;
            if (!int.TryParse(resultfromCache, out result))
            {
                var lastTimeReaded = new FeedReadedDataProvider().GetTimeReaded();
                result = FeedAggregateDataProvider.GetNewFeedsCount(lastTimeReaded);
                newFeedsCountCache.Insert(cacheKey, result.ToString(), DateTime.UtcNow.AddMinutes(3));
            }
            return result;
        }
    }
}