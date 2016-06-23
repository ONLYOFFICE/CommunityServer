/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;

namespace ASC.Files.Core
{
    [Flags]
    public enum TagType
    {
        New = 1,
        //Favorite = 2,
        System = 4,
        Locked = 8,
    }

    [Serializable]
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
                EntryType = entry is File ? FileEntryType.File : FileEntryType.Folder;
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