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

namespace ASC.Core.Common.Notify.Push
{
    [DataContract]
    public class PushNotification
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string ShortMessage { get; set; }

        [DataMember]
        public int? Badge { get; set; } 

        [DataMember]
        public PushModule Module { get; set; }

        [DataMember]
        public PushAction Action { get; set; }

        [DataMember]
        public PushItem Item { get; set; }

        [DataMember]
        public PushItem ParentItem { get; set; }

        [DataMember]
        public DateTime QueuedOn { get; set; }
        
        public static PushNotification ApiNotification(string message, int? badge)
        {
            return new PushNotification {Message = message, Badge = badge};
        }
    }

    [DataContract]
    public class PushItem
    {
        [DataMember]
        public PushItemType Type { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Description { get; set; }

        public PushItem()
        {
            
        }

        public PushItem(PushItemType type, string id, string description)
        {
            Type = type;
            ID = id;
            Description = description;
        }
    }
}
