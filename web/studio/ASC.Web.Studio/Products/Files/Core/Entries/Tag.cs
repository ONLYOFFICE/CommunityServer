/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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