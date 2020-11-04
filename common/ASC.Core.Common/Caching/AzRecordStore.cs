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


using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Core.Caching
{
    class AzRecordStore : IEnumerable<AzRecord>
    {
        private readonly Dictionary<string, List<AzRecord>> byObjectId = new Dictionary<string, List<AzRecord>>();


        public AzRecordStore(IEnumerable<AzRecord> aces)
        {
            foreach (var a in aces)
            {
                Add(a);
            }
        }


        public IEnumerable<AzRecord> Get(string objectId)
        {
            List<AzRecord> aces;
            byObjectId.TryGetValue(objectId ?? string.Empty, out aces);
            return aces ?? new List<AzRecord>();
        }

        public void Add(AzRecord r)
        {
            if (r == null) return;

            var id = r.ObjectId ?? string.Empty;
            if (!byObjectId.ContainsKey(id))
            {
                byObjectId[id] = new List<AzRecord>();
            }
            byObjectId[id].RemoveAll(a => a.SubjectId == r.SubjectId && a.ActionId == r.ActionId); // remove escape, see DbAzService
            byObjectId[id].Add(r);
        }

        public void Remove(AzRecord r)
        {
            if (r == null) return;

            var id = r.ObjectId ?? string.Empty;
            if (byObjectId.ContainsKey(id))
            {
                byObjectId[id].RemoveAll(a => a.SubjectId == r.SubjectId && a.ActionId == r.ActionId && a.Reaction == r.Reaction);
            }
        }

        public IEnumerator<AzRecord> GetEnumerator()
        {
            return byObjectId.Values.SelectMany(v => v).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}