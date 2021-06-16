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


using ASC.Mail.Enums;

namespace ASC.Mail.Core.Entities
{
    public class Chain
    {
        public string Id { get; set; }
        public int MailboxId { get; set; }
        public int Tenant { get; set; }
        public string User { get; set; }
        public FolderType Folder { get; set; }
        public int Length { get; set; }
        public bool Unread { get; set; }
        public bool HasAttachments { get; set; }
        public bool Importance { get; set; }
        public string Tags { get; set; }
    }
}
