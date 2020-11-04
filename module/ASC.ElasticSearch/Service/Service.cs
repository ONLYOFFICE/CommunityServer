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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Core;
using ASC.ElasticSearch.Core;
using Autofac;

namespace ASC.ElasticSearch.Service
{
    public class Service : IService
    {
        public bool Support(string table)
        {
            return FactoryIndexer.Builder.Resolve<IEnumerable<Wrapper>>().Any(r => r.IndexName == table);
        }

        public void ReIndex(List<string> toReIndex, int tenant)
        {
            var allItems = FactoryIndexer.Builder.Resolve<IEnumerable<Wrapper>>().ToList();
            var tasks = new List<Task>(toReIndex.Count);

            foreach (var item in toReIndex)
            {
                var index = allItems.FirstOrDefault(r => r.IndexName == item);
                if (index == null) continue;

                var generic = typeof(BaseIndexer<>);
                var instance = (IIndexer)Activator.CreateInstance(generic.MakeGenericType(index.GetType()), index);
                tasks.Add(instance.ReIndex());
            }

            Task.WhenAll(tasks).ContinueWith(r =>
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                SearchSettings.Load().ClearCache();
            });
        }

        public State GetState()
        {
            return new State
            {
                Indexing = Launcher.Indexing,
                LastIndexed = Launcher.LastIndexed
            };
        }
    }
}
