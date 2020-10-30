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
using ASC.Mail.Enums;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Folder")]
    public class MailFolderData
    {
        [DataMember(IsRequired = true, Name = "id")]
        public FolderType Id { get; set; }

        [DataMember(IsRequired = true, Name = "unread")]
        public int UnreadCount { get; set; }

        [DataMember(IsRequired = true, Name = "unread_messages")]
        public int UnreadMessagesCount { get; set; }

        [DataMember(IsRequired = true, Name = "total_count")]
        public int TotalCount { get; set; }

        [DataMember(IsRequired = true, Name = "total_messages_count")]
        public int TotalMessgesCount { get; set; }

        [DataMember(IsRequired = true, Name = "time_modified")]
        public DateTime TimeModified { get; set; }
    }
}
