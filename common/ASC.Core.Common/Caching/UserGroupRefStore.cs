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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Core.Caching
{
    class UserGroupRefStore : IDictionary<string, UserGroupRef>
    {
        private readonly IDictionary<string, UserGroupRef> refs;
        private ILookup<Guid, UserGroupRef> index;
        private bool changed;


        public UserGroupRefStore(IDictionary<string, UserGroupRef> refs)
        {
            this.refs = refs;
            changed = true;
        }


        public void Add(string key, UserGroupRef value)
        {
            refs.Add(key, value);
            RebuildIndex();
        }

        public bool ContainsKey(string key)
        {
            return refs.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return refs.Keys; }
        }

        public bool Remove(string key)
        {
            var result = refs.Remove(key);
            RebuildIndex();
            return result;
        }

        public bool TryGetValue(string key, out UserGroupRef value)
        {
            return refs.TryGetValue(key, out value);
        }

        public ICollection<UserGroupRef> Values
        {
            get { return refs.Values; }
        }

        public UserGroupRef this[string key]
        {
            get
            {
                return refs[key];
            }
            set
            {
                refs[key] = value;
                RebuildIndex();
            }
        }

        public void Add(KeyValuePair<string, UserGroupRef> item)
        {
            refs.Add(item);
            RebuildIndex();
        }

        public void Clear()
        {
            refs.Clear();
            RebuildIndex();
        }

        public bool Contains(KeyValuePair<string, UserGroupRef> item)
        {
            return refs.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, UserGroupRef>[] array, int arrayIndex)
        {
            refs.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return refs.Count; }
        }

        public bool IsReadOnly
        {
            get { return refs.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, UserGroupRef> item)
        {
            var result = refs.Remove(item);
            RebuildIndex();
            return result;
        }

        public IEnumerator<KeyValuePair<string, UserGroupRef>> GetEnumerator()
        {
            return refs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return refs.GetEnumerator();
        }

        public IEnumerable<UserGroupRef> GetRefsByUser(Guid userId)
        {
            if (changed)
            {
                index = refs.Values.ToLookup(r => r.UserId);
                changed = false;
            }
            return index[userId];
        }

        private void RebuildIndex()
        {
            changed = true;
        }
    }
}
