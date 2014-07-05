/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
