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

using ASC.Mail.Enums;

namespace ASC.Mail.Core.Entities
{
    public class MailInfo
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public bool Importance { get; set; }
        public DateTime DateSent { get; set; }
        public int Size { get; set; }
        public bool HasAttachments { get; set; }
        public bool IsNew { get; set; }
        public bool IsAnswered { get; set; }
        public bool IsForwarded { get; set; }
        public string LabelsString { get; set; }
        public FolderType FolderRestore { get; set; }
        public FolderType Folder { get; set; }
        public string ChainId { get; set; }
        public DateTime ChainDate { get; set; }
        public int MailboxId { get; set; }
        public string CalendarUid { get; set; }
        public string Stream { get; set; }
        public string Uidl { get; set; }
        public bool IsRemoved { get; set; }
        public string Intoduction { get; set; }
    }
}
