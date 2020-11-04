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
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.Storage;

namespace ASC.Api.Impl
{
    internal class ApiKeyValueInMemoryStorage : IApiKeyValueStorage
    {
        private static readonly Hashtable Cache = Hashtable.Synchronized(new Hashtable());

        #region IApiKeyValueStorage Members

        public object Get(IApiEntryPoint entrypoint, string key)
        {
            return Cache[GetKey(entrypoint, key)];
        }

        public void Set(IApiEntryPoint entrypoint, string key, object @object)
        {
            Cache[GetKey(entrypoint, key)] = @object;
        }

        public bool Exists(IApiEntryPoint entrypoint, string key)
        {
            return Cache.ContainsKey(GetKey(entrypoint, key));
        }

        public void Remove(IApiEntryPoint entrypoint, string key)
        {
            Cache.Remove(GetKey(entrypoint, key));
        }

        public void Clear(IApiEntryPoint entrypoint)
        {
            IEnumerable<string> toRemove = Cache.Keys.Cast<string>().Where(x => x.StartsWith(entrypoint.GetType() + ":"));
            foreach (string remove in toRemove)
            {
                Cache.Remove(remove);
            }
        }

        #endregion

        private static string GetKey(IApiEntryPoint point, string key)
        {
            return point.GetType() + ":" + key;
        }
    }
}