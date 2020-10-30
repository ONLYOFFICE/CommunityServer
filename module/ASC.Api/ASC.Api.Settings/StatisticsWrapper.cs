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
using System.Runtime.Serialization;

namespace ASC.Api.Settings
{
    [DataContract(Name = "statistics", Namespace = "")]
    public class UsageSpaceStatItemWrapper
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Icon { get; set; }

        [DataMember]
        public bool Disabled { get; set; }

        [DataMember]
        public string Size { get; set; }

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
        [DataMember]
        public string DisplayDate { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public int Hosts { get; set; }

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