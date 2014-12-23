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
            _cacheCodition = (T) => true;
            _baseKey = baseKey;
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