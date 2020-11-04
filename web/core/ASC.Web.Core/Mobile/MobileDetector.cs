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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Caching;

namespace ASC.Web.Core.Mobile
{
    public class MobileDetector
    {
        private static readonly Regex uaMobileRegex;

        private static readonly ICache cache = AscCache.Memory;


        public static bool IsMobile
        {
            get { return IsRequestMatchesMobile(); }
        }


        static MobileDetector()
        {
            if (!string.IsNullOrEmpty(ConfigurationManagerExtension.AppSettings["mobile.regex"]))
            {
                uaMobileRegex = new Regex(ConfigurationManagerExtension.AppSettings["mobile.regex"], RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            }
        }


        public static bool IsRequestMatchesMobile()
        {
            bool? result = false;
            var ua = HttpContext.Current.Request.UserAgent;
            var regex = uaMobileRegex;
            if (!string.IsNullOrEmpty(ua) && regex != null)
            {
                var key = "mobileDetector/" + ua.GetHashCode();

                bool fromCache;

                if (bool.TryParse(cache.Get<string>(key), out fromCache))
                {
                    result = fromCache;
                }
                else
                {
                    cache.Insert(key, (result = regex.IsMatch(ua)).ToString(), TimeSpan.FromMinutes(10));
                }
            }
            return result.GetValueOrDefault();
        }
    }
}