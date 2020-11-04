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
using System.Diagnostics;

namespace ASC.Files.Core
{
    [Flags]
    public enum TagType
    {
        New = 1,
        Favorite = 2,
        System = 4,
        Locked = 8,
        Recent = 16,
        Template = 32,
    }

    [Serializable]
    [DebuggerDisplay("{TagName} ({Id}) entry {EntryType} ({EntryId})")]
    public class Tag
    {
        public string TagName { get; set; }

        public TagType TagType { get; set; }

        public Guid Owner { get; set; }

        public object EntryId { get; set; }

        public FileEntryType EntryType { get; set; }

        public int Id { get; set; }

        public int Count { get; set; }


        public Tag()
        {
        }

        public Tag(string name, TagType type, Guid owner)
            : this(name, type, owner, null, 0)
        {
        }

        public Tag(string name, TagType type, Guid owner, FileEntry entry, int count)
        {
            TagName = name;
            TagType = type;
            Owner = owner;
            Count = count;
            if (entry != null)
            {
                EntryId = entry.ID;
                EntryType = entry.FileEntryType;
            }
        }


        public static Tag New(Guid owner, FileEntry entry)
        {
            return New(owner, entry, 1);
        }

        public static Tag New(Guid owner, FileEntry entry, int count)
        {
            return new Tag("new", TagType.New, owner, entry, count);
        }

        public static Tag Recent(Guid owner, FileEntry entry)
        {
            return new Tag("recent", TagType.Recent, owner, entry, 0);
        }

        public static Tag Favorite(Guid owner, FileEntry entry)
        {
            return new Tag("favorite", TagType.Favorite, owner, entry, 0);
        }

        public static Tag Template(Guid owner, FileEntry entry)
        {
            return new Tag("template", TagType.Template, owner, entry, 0);
        }

        public override bool Equals(object obj)
        {
            var f = obj as Tag;
            return f != null && f.Id == Id && f.EntryType == EntryType && Equals(f.EntryId, EntryId);
        }

        public override int GetHashCode()
        {
            return (Id + EntryType + EntryId.ToString()).GetHashCode();
        }
    }
}