/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Feed;
using ASC.Feed.Data;
using ASC.Specific;
using System.Linq;

namespace ASC.Api.Feed
{
    public class FeedApi : IApiEntryPoint
    {
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
            var feedReadedProvider = new FeedReadedDataProvider();
            var lastTimeReaded = feedReadedProvider.GetTimeReaded();

            return FeedAggregateDataProvider.GetNewFeedsCount(lastTimeReaded);
        }
    }
}