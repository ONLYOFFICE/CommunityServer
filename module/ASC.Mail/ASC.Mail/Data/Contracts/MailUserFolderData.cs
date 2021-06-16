/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "UserFolder")]
    public class MailUserFolderData
    {
        [DataMember(IsRequired = true, Name = "id")]
        public uint Id { get; set; }

        [DataMember(IsRequired = true, Name = "parent")]
        public uint ParentId { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "unread_count")]
        public int UnreadCount { get; set; }

        [DataMember(IsRequired = true, Name = "total_count")]
        public int TotalCount { get; set; }

        [DataMember(IsRequired = true, Name = "unread_chain_count")]
        public int UnreadChainCount { get; set; }

        [DataMember(IsRequired = true, Name = "total_chain_count")]
        public int TotalChainCount { get; set; }

        [DataMember(IsRequired = true, Name = "folder_count")]
        public int FolderCount { get; set; }
    }
}
