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
using System.Web;

namespace ASC.Web.Core.Utility
{
    public static class UrlSwitcher
    {
        public static string SelectCurrentUriScheme(string uri)
        {
            return HttpContext.Current != null ? SelectUriScheme(uri, HttpContext.Current.Request.GetUrlRewriter().Scheme) : uri;
        }

        public static string SelectUriScheme(string uri, string scheme)
        {
            return Uri.IsWellFormedUriString(uri,UriKind.Absolute) ? SelectUriScheme(new Uri(uri, UriKind.Absolute),scheme).ToString() : uri;
        }

        public static Uri SelectCurrentUriScheme(Uri uri)
        {
            if (HttpContext.Current!=null)
            {
                return SelectUriScheme(uri, HttpContext.Current.Request.GetUrlRewriter().Scheme);
            }
            return uri;
        }

        public static Uri SelectUriScheme(Uri uri, string scheme)
        {
            if (!string.IsNullOrEmpty(scheme) && !scheme.Equals(uri.Scheme,StringComparison.OrdinalIgnoreCase))
            {
                //Switch
                var builder = new UriBuilder(uri) { Scheme = scheme.ToLowerInvariant(), Port = scheme.Equals("https",StringComparison.OrdinalIgnoreCase)?443:80};//Set proper port!
                return builder.Uri;
            }
            return uri;
        }

        public static Uri ToCurrentScheme(this Uri uri)
        {
            return SelectCurrentUriScheme(uri);
        }

        public static Uri ToScheme(this Uri uri, string scheme)
        {
            return SelectUriScheme(uri,scheme);
        }
    }
}