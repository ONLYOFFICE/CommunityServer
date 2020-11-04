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

namespace ASC.Notify.Messages
{
    [Serializable]
    [DataContract]
    public class NotifyMessage
    {
        [DataMember(Order = 1)]
        public int Tenant
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        public string Sender
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        public string From
        {
            get;
            set;
        }

        [DataMember(Order = 4)]
        public string To
        {
            get;
            set;
        }

        [DataMember(Order = 5)]
        public string ReplyTo
        {
            get;
            set;
        }

        [DataMember(Order = 6)]
        public string Subject
        {
            get;
            set;
        }

        [DataMember(Order = 7)]
        public string ContentType
        {
            get;
            set;
        }

        [DataMember(Order = 8)]
        public string Content
        {
            get;
            set;
        }

        [DataMember(Order = 9)]
        public DateTime CreationDate
        {
            get;
            set;
        }

        [DataMember(Order = 10)]
        public int Priority
        {
            get;
            set;
        }

        [DataMember(Order = 11)]
        public NotifyMessageAttachment[] EmbeddedAttachments
        {
            get;
            set;
        }

        [DataMember(Order = 12)]
        public string AutoSubmitted
        {
            get;
            set;
        }
    }

    [Serializable]
    [DataContract]
    public class NotifyMessageAttachment
    {
        [DataMember(Order = 1)]
        public string FileName
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        public string ContentId
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        public byte[] Content
        {
            get;
            set;
        }
    }
}
