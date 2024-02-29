/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

namespace ASC.Web.Studio.Utility
{
    public static class RefererURL
    {
        private const string refererurl = "refererurl";
        public static string GetRefererURL(this HttpContext context)
        {
            var relativePath = context.Request.QueryString[refererurl];
            var refURL = HttpUtility.UrlDecode(Uri.IsWellFormedUriString(relativePath, UriKind.Absolute) ? null: relativePath);
            return string.IsNullOrEmpty(refURL)
                ? CommonLinkUtility.GetDefault()
                : refURL;
        }

        public static string AppendRefererURL(this HttpRequest request, string redirectPage)
        {
            var relativePath = request.GetUrlRewriter().PathAndQuery;
            return string.Format("{0}{1}{2}={3}",
                                    redirectPage,
                                    redirectPage != null && redirectPage.Contains("?") ? "&" : "?",
                                    refererurl,
                                    HttpUtility.UrlEncode(relativePath)
                                );
        }
    }
}