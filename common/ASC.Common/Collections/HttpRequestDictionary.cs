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
using System.Diagnostics;
using System.Web;

namespace ASC.Collections
{
    public sealed class HttpRequestDictionary<T> : CachedDictionaryBase<T>
    {
        private class CachedItem
        {
            internal T Value { get; set; }

            internal CachedItem(T value)
            {
                Value = value;
            }
        }

        public HttpRequestDictionary(string baseKey)
        {
            condition = (T) => true;
            this.baseKey = baseKey;
        }

        protected override void InsertRootKey(string rootKey)
        {
            //We can't expire in HtppContext in such way
        }

        public override void Reset(string rootKey, string key)
        {
            if (HttpContext.Current != null)
            {
                var builtkey = BuildKey(key, rootKey);
                HttpContext.Current.Items[builtkey] = null;
            }
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            if (HttpContext.Current != null)
            {
                var builtkey = BuildKey(key, rootkey);
                HttpContext.Current.Items[builtkey] = new CachedItem(newValue);
            }
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return HttpContext.Current != null ? HttpContext.Current.Items[fullKey] : null;
        }

        protected override bool FitsCondition(object cached)
        {
            return cached is CachedItem;
        }
        protected override T ReturnCached(object objectCache)
        {
            return ((CachedItem)objectCache).Value;
        }

        protected override void OnHit(string fullKey)
        {
            Debug.Print("{0} http cache hit:{1}", HttpContext.Current.Request.Url, fullKey);
        }

        protected override void OnMiss(string fullKey)
        {
            Uri uri = null;
            if (HttpContext.Current != null)
            {
                uri = HttpContext.Current.Request.Url;
            }
            Debug.Print("{0} http cache miss:{1}", uri == null ? "no-context" : uri.AbsolutePath, fullKey);
        }

    }
}