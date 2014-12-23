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
