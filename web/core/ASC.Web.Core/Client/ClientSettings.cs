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

namespace ASC.Web.Core.Client
{
    public static class ClientSettings
    {
        private static bool? bundlesEnabled;
        private static bool? storeEnabled;
        private static bool? gzipEnabled;
        private static readonly string resetCacheKey = ConfigurationManagerExtension.AppSettings["web.client.cache.resetkey"] ?? DateTime.UtcNow.ToString("yyyyMMddhhmmss");


        public static bool BundlingEnabled
        {
            get { return bundlesEnabled ?? (bool)(bundlesEnabled = bool.Parse(ConfigurationManagerExtension.AppSettings["web.client.bundling"] ?? "false")); }
        }

        public static bool StoreBundles
        {
            get { return storeEnabled ?? (bool)(storeEnabled = bool.Parse(ConfigurationManagerExtension.AppSettings["web.client.store"] ?? "false")); }
        }

        public static bool GZipEnabled
        {
            get { return gzipEnabled ?? (bool)(gzipEnabled = bool.Parse(ConfigurationManagerExtension.AppSettings["web.client.store.gzip"] ?? "true")); }
        }

        public static string ResetCacheKey
        {
            get { return resetCacheKey; }
        }
    }
}