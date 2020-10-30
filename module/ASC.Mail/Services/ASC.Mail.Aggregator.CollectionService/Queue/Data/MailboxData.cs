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


using System.Runtime.Serialization;
using LiteDB;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    [DataContract]
    public class MailboxData
    {
        [DataMember(Name = "tenant")]
        public int TenantId { get; set; }

        [DataMember(Name = "user")]
        public string UserId { get; set; }

        [DataMember(Name = "id")]
        public int MailboxId { get; set; }

        [DataMember]
        public string EMail { get; set; }

        [DataMember(Name = "imap")]
        public bool Imap { get; set; }

        [DataMember(Name = "is_teamlab")]
        public bool IsTeamlab { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "messages_count")]
        public int MessagesCount { get; set; }
    }
}
