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
using ASC.Mail.Enums;

namespace ASC.Mail.Data.Contracts
{
    public class MailFolder : IEquatable<MailFolder>
    {
        public static bool IsIdOk(FolderType folderType)
        {
            return folderType >= FolderType.Inbox && folderType <= FolderType.Templates;
        }

        public FolderType Folder { get; private set; }
        public string Name { get; private set; }
        public string[] Tags { get; private set; }

        public MailFolder(FolderType folder, string name, string[] tags = null)
        {
            if (!IsIdOk(folder))
                throw new ArgumentException(@"Incorrect folder id", "folder");

            Folder = folder;
            Name = name;
            Tags = tags ?? new string[] {};
        }

        public bool Equals(MailFolder other)
        {
            if (other == null) return false;

            return Folder == other.Folder
                   && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
                   && Tags == other.Tags;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as ContactInfo);
        }

        public override int GetHashCode()
        {
            return Folder.GetHashCode() ^ Name.GetHashCode() ^ Tags.GetHashCode();
        }
    }
}
