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
using System.Collections.Generic;
using System.Linq;

using ASC.Common.Caching;
using ASC.Web.Core.Utility;

namespace ASC.Web.Core
{
    public class BruteForceLoginManager
    {
        private LoginSettings settings;
        private ICache cache;
        private string login;
        private string requestIp;

        public BruteForceLoginManager(ICache cache, string login, string requestIp)
        {
            settings = LoginSettings.Load();
            this.cache = cache;
            this.login = login;
            this.requestIp = requestIp;
        }

        private string GetBlockCacheKey()
        {
            return "loginblock/" + login + requestIp;
        }

        private string GetHistoryCacheKey()
        {
            return "loginsec/" + login + requestIp;
        }

        public bool Increment(out bool showRecaptcha)
        {
            showRecaptcha = true;

            var blockCacheKey = GetBlockCacheKey();

            if (cache.Get<string>(blockCacheKey) != null)
            {
                return false;
            }

            var historyCacheKey = GetHistoryCacheKey();

            var history = cache.Get<List<DateTime>>(historyCacheKey) ?? new List<DateTime>();

            var now = DateTime.UtcNow;

            var checkTime = now.Subtract(TimeSpan.FromSeconds(settings.CheckPeriod));

            history = history.Where(item => item > checkTime).ToList();

            history.Add(now);

            showRecaptcha = history.Count > settings.AttemptCount - 1;

            if (history.Count > settings.AttemptCount)
            {
                cache.Insert(blockCacheKey, "block", now.Add(TimeSpan.FromSeconds(settings.BlockTime)));
                cache.Remove(historyCacheKey);
                return false;
            }

            cache.Insert(historyCacheKey, history, now.Add(TimeSpan.FromSeconds(settings.CheckPeriod)));
            return true;
        }

        public void Decrement()
        {
            var historyCacheKey = GetHistoryCacheKey();

            var history = cache.Get<List<DateTime>>(historyCacheKey) ?? new List<DateTime>();

            if (history.Count > 0)
            {
                history.RemoveAt(history.Count - 1);
            }

            cache.Insert(historyCacheKey, history, DateTime.UtcNow.Add(TimeSpan.FromSeconds(settings.CheckPeriod)));
        }
    }
}
