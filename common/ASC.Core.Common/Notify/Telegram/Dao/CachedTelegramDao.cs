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


using ASC.Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Core.Common.Notify.Telegram
{
    public class CachedTelegramDao
    {
        private TelegramDao tgDao { get; set; }
        private ICache cache { get; set; }
        private TimeSpan Expiration { get; set; }

        private string PairKeyFormat { get; set; }
        private string SingleKeyFormat { get; set; }

        private static CachedTelegramDao _instance;
        public static CachedTelegramDao Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new CachedTelegramDao("default");
                return _instance;
            }
        }

        private CachedTelegramDao(string dbid)
        {
            tgDao = new TelegramDao(dbid);
            cache = AscCache.Memory;
            Expiration = TimeSpan.FromMinutes(20);

            PairKeyFormat = "tgUser:{0}:{1}";
            SingleKeyFormat = "tgUser:{0}";
        }

        public void Delete(Guid userId, int tenantId)
        {
            cache.Remove(string.Format(PairKeyFormat, userId, tenantId));
            tgDao.Delete(userId, tenantId);
        }

        public void Delete(int telegramId)
        {
            cache.Remove(string.Format(SingleKeyFormat, telegramId));
            tgDao.Delete(telegramId);
        }

        public TelegramUser GetUser(Guid userId, int tenantId)
        {
            var key = string.Format(PairKeyFormat, userId, tenantId);
            
            var user = cache.Get<TelegramUser>(key);
            if (user != null) return user;
            
            user = tgDao.GetUser(userId, tenantId);
            if (user != null) cache.Insert(key, user, Expiration);
            return user;
        }

        public List<TelegramUser> GetUser(int telegramId)
        {
            var key = string.Format(SingleKeyFormat, telegramId);

            var users = cache.Get<List<TelegramUser>>(key);
            if (users != null) return users;

            users = tgDao.GetUser(telegramId);
            if (users.Any()) cache.Insert(key, users, Expiration);
            return users;
        }

        public void RegisterUser(Guid userId, int tenantId, int telegramId)
        {
            tgDao.RegisterUser(userId, tenantId, telegramId);

            var key = string.Format(PairKeyFormat, userId, tenantId);
            cache.Insert(key, new TelegramUser() { PortalUserId = userId, TenantId = tenantId, TelegramId = telegramId }, Expiration);
        }
    }
}
