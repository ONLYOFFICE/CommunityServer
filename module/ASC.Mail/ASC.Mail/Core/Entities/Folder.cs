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
    public class Folder
    {
        public int Tenant { get; set; }

        public string UserId { get; set; }

        public FolderType FolderType { get; set; }

        public DateTime TimeModified { get; set; }

        public int TotalCount { get; set; }

        public int UnreadCount { get; set; }

        public int TotalChainCount { get; set; }

        public int UnreadChainCount { get; set; }
    }
}
