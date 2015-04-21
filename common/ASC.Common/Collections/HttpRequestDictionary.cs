/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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