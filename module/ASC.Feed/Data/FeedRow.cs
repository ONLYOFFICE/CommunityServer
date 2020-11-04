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
using Newtonsoft.Json;

namespace ASC.Feed.Data
{
    public class FeedRow
    {
        public Aggregator.Feed Feed { get; private set; }

        public string Id { get { return Feed.Id; } }

        public bool ClearRightsBeforeInsert { get { return Feed.Variate; } }

        public int Tenant { get; set; }

        public string ProductId { get; set; }

        public string ModuleId { get { return Feed.Module; } }

        public Guid AuthorId { get { return Feed.AuthorId; } }

        public Guid ModifiedById { get { return Feed.ModifiedBy; } }

        public DateTime CreatedDate { get { return Feed.CreatedDate; } }

        public DateTime ModifiedDate { get { return Feed.ModifiedDate; } }

        public string GroupId { get { return Feed.GroupId; } }

        public string Json {
            get
            {
                return JsonConvert.SerializeObject(Feed, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });
            }
        }

        public string Keywords { get { return Feed.Keywords; } }

        public DateTime AggregatedDate { get; set; }

        public IList<Guid> Users { get; set; }


        public FeedRow(Aggregator.Feed feed)
        {
            Users = new List<Guid>();
            Feed = feed;
        }
    }
}