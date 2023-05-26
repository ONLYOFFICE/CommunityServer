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
using System.Runtime.Serialization;

namespace ASC.Api.Settings
{
    [DataContract(Name = "statistics", Namespace = "")]
    public class UsageSpaceStatItemWrapper
    {
        ///<example>Item name</example>
        [DataMember]
        public string Name { get; set; }

        ///<example>Item icon path</example>
        [DataMember]
        public string Icon { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool Disabled { get; set; }

        ///<example>0 Byte</example>
        [DataMember]
        public string Size { get; set; }

        ///<example>Item url</example>
        [DataMember]
        public string Url { get; set; }

        public static UsageSpaceStatItemWrapper GetSample()
        {
            return new UsageSpaceStatItemWrapper
            {
                Name = "Item name",
                Icon = "Item icon path",
                Disabled = false,
                Size = "0 Byte",
                Url = "Item url"
            };
        }
    }

    [DataContract(Name = "statistics", Namespace = "")]
    public class ChartPointWrapper
    {
        ///<example>12/22/2020</example>
        [DataMember]
        public string DisplayDate { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        [DataMember]
        public DateTime Date { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int Hosts { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int Hits { get; set; }

        public static ChartPointWrapper GetSample()
        {
            return new ChartPointWrapper
            {
                DisplayDate = DateTime.Now.ToShortDateString(),
                Date = DateTime.Now,
                Hosts = 0,
                Hits = 0
            };
        }
    }
}