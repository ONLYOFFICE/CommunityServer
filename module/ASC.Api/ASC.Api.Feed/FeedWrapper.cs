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


using System.Collections.Generic;

using ASC.Feed.Data;
using ASC.Specific;

namespace ASC.Api.Feed
{
    public struct FeedWrapper
    {
        public FeedWrapper(FeedResultItem item)
            : this()
        {
            Feed = item.Json;
            Module = item.Module;
            GroupId = item.GroupId;
            IsToday = item.IsToday;
            IsYesterday = item.IsYesterday;
            IsTomorrow = item.IsTomorrow;
            CreatedDate = (ApiDateTime)item.CreatedDate;
            ModifiedDate = (ApiDateTime)item.ModifiedDate;
            AggregatedDate = (ApiDateTime)item.AggregatedDate;
        }

        public string Feed { get; private set; }

        public string Module { get; private set; }

        public bool IsToday { get; private set; }

        public bool IsYesterday { get; private set; }

        public bool IsTomorrow { get; private set; }

        public ApiDateTime CreatedDate { get; private set; }

        public ApiDateTime ModifiedDate { get; private set; }

        public ApiDateTime AggregatedDate { get; private set; }

        public ApiDateTime TimeReaded { get; private set; }

        public string GroupId { get; set; }

        public IEnumerable<FeedWrapper> GroupedFeeds { get; set; }
    }
}