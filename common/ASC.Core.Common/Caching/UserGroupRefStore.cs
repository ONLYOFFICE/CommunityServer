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
