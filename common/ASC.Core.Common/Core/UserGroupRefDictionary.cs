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

namespace ASC.Core
{
    public class UserGroupRefDictionary : IDictionary<string, UserGroupRef>
    {
        private readonly IDictionary<string, UserGroupRef> d = new Dictionary<string, UserGroupRef>();
        private IDictionary<Guid, IEnumerable<UserGroupRef>> byUsers;
        private IDictionary<Guid, IEnumerable<UserGroupRef>> byGroups;


        public int Count
        {
            get { return d.Count; }
        }

        public bool IsReadOnly
        {
            get { return d.IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return d.Keys; }
        }

        public ICollection<UserGroupRef> Values
        {
            get { return d.Values; }
        }

        public UserGroupRef this[string key]
        {
            get { return d[key]; }
            set
            {
                d[key] = value;
                BuildIndexes();
            }
        }


        public UserGroupRefDictionary(IDictionary<string, UserGroupRef> dic)
        {
            foreach (var p in dic)
            {
                d.Add(p);
            }
            BuildIndexes();
        }


        public void Add(string key, UserGroupRef value)
        {
            d.Add(key, value);
            BuildIndexes();
        }

        public void Add(KeyValuePair<string, UserGroupRef> item)
        {
            d.Add(item);
            BuildIndexes();
        }

        public bool Remove(string key)
        {
            var result = d.Remove(key);
            BuildIndexes();
            return result;
        }

        public bool Remove(KeyValuePair<string, UserGroupRef> item)
        {
            var result = d.Remove(item);
            BuildIndexes();
            return result;
        }

        public void Clear()
        {
            d.Clear();
            BuildIndexes();
        }


        public bool TryGetValue(string key, out UserGroupRef value)
        {
            return d.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return d.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<string, UserGroupRef> item)
        {
            return d.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, UserGroupRef>[] array, int arrayIndex)
        {
            d.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, UserGroupRef>> GetEnumerator()
        {
            return d.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)d).GetEnumerator();
        }


        public IEnumerable<UserGroupRef> GetByUser(Guid userId)
        {
            return byUsers.ContainsKey(userId) ? byUsers[userId].ToList() : new List<UserGroupRef>();
        }

        public IEnumerable<UserGroupRef> GetByGroups(Guid groupId)
        {
            return byGroups.ContainsKey(groupId) ? byGroups[groupId].ToList() : new List<UserGroupRef>();
        }


        private void BuildIndexes()
        {
            byUsers = d.Values.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.AsEnumerable());
            byGroups = d.Values.GroupBy(r => r.GroupId).ToDictionary(g => g.Key, g => g.AsEnumerable());
        }
    }
}
