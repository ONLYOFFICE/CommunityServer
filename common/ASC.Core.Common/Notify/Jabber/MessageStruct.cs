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


using Newtonsoft.Json;
using System;

namespace ASC.Core.Common.Notify.Jabber
{
    public class MessageClass : IComparable<MessageClass>
    {
        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("u")]
        public string UserName { get; set; }

        [JsonProperty("t")]
        public string Text { get; set; }

        [JsonProperty("d")]
        public DateTime DateTime { get; set; }

        public int CompareTo(MessageClass other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}