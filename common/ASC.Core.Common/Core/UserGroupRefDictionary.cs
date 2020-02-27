/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
