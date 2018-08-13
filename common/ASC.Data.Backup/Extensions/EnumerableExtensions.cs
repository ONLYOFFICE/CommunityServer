/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Linq;

namespace ASC.Data.Backup.Extensions
{
    public class TreeNode<TEntry>
    {
        public TEntry Entry { get; set; }
        public TreeNode<TEntry> Parent { get; set; }
        public List<TreeNode<TEntry>> Children { get; private set; }

        public TreeNode()
        {
            Children = new List<TreeNode<TEntry>>();
        }

        public TreeNode(TEntry entry)
            :this()
        {
            Entry = entry;
            Parent = null;
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<TreeNode<TEntry>> ToTree<TEntry, TKey>(this IEnumerable<TEntry> elements,
                                                                         Func<TEntry, TKey> keySelector,
                                                                         Func<TEntry, TKey> parentKeySelector)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            if (parentKeySelector == null)
                throw new ArgumentNullException("parentKeySelector");

            var dic = elements.ToDictionary(keySelector, x => new TreeNode<TEntry>(x));
            foreach (var keyValue in dic)
            {
                var parentKey = parentKeySelector(keyValue.Value.Entry);
                TreeNode<TEntry> parent;
                if (parentKey != null && dic.TryGetValue(parentKeySelector(keyValue.Value.Entry), out parent))
                {
                    parent.Children.Add(keyValue.Value);
                    keyValue.Value.Parent = parent;
                }
            }

            return dic.Values.Where(x => x.Parent == null);
        }

        public static IEnumerable<IEnumerable<TEntry>> MakeParts<TEntry>(this IEnumerable<TEntry> collection, int partLength)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (partLength <= 0)
                throw new ArgumentOutOfRangeException("partLength", partLength, "Length must be positive integer");

            var part = new List<TEntry>(partLength);

            foreach (var entry in collection)
            {
                part.Add(entry);

                if (part.Count == partLength)
                {
                    yield return part.AsEnumerable();
                    part = new List<TEntry>(partLength);
                }
            }

            if (part.Count > 0)
            {
                yield return part.AsEnumerable();
            }
        }
    }
}
