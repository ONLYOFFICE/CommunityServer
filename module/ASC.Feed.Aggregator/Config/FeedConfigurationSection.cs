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
using System.Configuration;

namespace ASC.Feed.Aggregator.Config
{
    public class FeedConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("serverRoot", DefaultValue = "http://*/")]
        public string ServerRoot
        {
            get { return (string)this["serverRoot"]; }
            set { base["serverRoot"] = value; }
        }

        [ConfigurationProperty("aggregatePeriod", DefaultValue = "0:5:0")]
        public TimeSpan AggregatePeriod
        {
            get { return (TimeSpan)this["aggregatePeriod"]; }
        }

        [ConfigurationProperty("aggregateInterval", DefaultValue = "14.0:0:0")]
        public TimeSpan AggregateInterval
        {
            get { return (TimeSpan)this["aggregateInterval"]; }
        }

        [ConfigurationProperty("removePeriod", DefaultValue = "1.0:0:0")]
        public TimeSpan RemovePeriod
        {
            get { return (TimeSpan)this["removePeriod"]; }
        }


        public static FeedConfigurationSection GetFeedSection()
        {
            return (FeedConfigurationSection)ConfigurationManagerExtension.GetSection("feed");
        }
    }
}