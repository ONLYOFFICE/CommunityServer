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
