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
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Feed;
using ASC.Feed.Data;
using ASC.Specific;

namespace ASC.Api.Feed
{
    /// <summary>
    /// Feed API.
    /// </summary>
    /// <name>feed</name>
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

        ///<summary>
        ///Opens feeds for reading.
        ///</summary>
        ///<short>
        ///Read feeds
        ///</short>
        /// <path>api/2.0/feed/read</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update("/read")]
        public void Read()
        {
            new FeedReadedDataProvider().SetTimeReaded(SecurityContext.CurrentAccount.ID);
            newFeedsCountCache.Remove(GetNewFeedsCountKey());
        }

        ///<summary>
        ///Returns a list of feeds that are filtered by the parameters specified in the request.
        ///</summary>
        ///<short>
        ///Get feeds
        ///</short>
        /// <param type="System.String, System" name="product">Product which feeds you want to read</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="from">Time from which the feeds should be displayed</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="to">Time until which the feeds should be displayed</param>
        /// <param type="System.Nullable{System.Guid}, System" name="author">Author whose feeds you want to read</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="onlyNew">Displays only fresh feeds</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="timeReaded">Time when the feeds were read</param>
        /// <returns>List of filtered feeds</returns>
        /// <path>api/2.0/feed/filter</path>
        /// <httpMethod>GET</httpMethod>
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
            return new { feeds, readedDate };
        }

        ///<summary>
        ///Returns a number of fresh feeds.
        ///</summary>
        ///<short>
        ///Count fresh feeds
        ///</short>
        ///<returns>Number of fresh feeds</returns>
        /// <path>api/2.0/feed/newfeedscount</path>
        /// <httpMethod>GET</httpMethod>
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