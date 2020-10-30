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
using System.Linq;
using System.Security.Cryptography;
using System.Web.Configuration;

using ASC.Notify.Messages;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Notify.Telegram;

namespace ASC.Core.Common.Notify
{
    public class TelegramHelper
    {
        private TelegramServiceClient Client
        {
            get
            {
                return new TelegramServiceClient();
            }
        }

        private static TelegramHelper _instance;
        public static TelegramHelper Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new TelegramHelper();
                return _instance;
            }
        }

        public enum RegStatus
        {
            NotRegistered,
            Registered,
            AwaitingConfirmation
        }

        public string RegisterUser(Guid userId, int tenantId)
        {
            var token = GenerateToken(userId);

            using (var client = Client)
            {
                client.RegisterUser(userId.ToString(), tenantId, token);
            }

            return GetLink(token);
        }

        public void SendMessage(NotifyMessage msg)
        {
            using (var client = Client)
            {
                client.SendMessage(msg);
            }
        }

        public bool CheckConnection(int tenantId, string token, int tokenLifespan, string proxy)
        {
            using (var client = Client)
            {
                return client.CheckConnection(tenantId, token, tokenLifespan, proxy);
            }
        }

        public RegStatus UserIsConnected(Guid userId, int tenantId)
        {
            if (CachedTelegramDao.Instance.GetUser(userId, tenantId) != null) return RegStatus.Registered;

            return IsAwaitingRegistration(userId, tenantId) ? RegStatus.AwaitingConfirmation : RegStatus.NotRegistered;
        }

        public string CurrentRegistrationLink(Guid userId, int tenantId)
        {
            var token = GetCurrentToken(userId, tenantId);
            if (token == null) return "";

            return GetLink(token);
        }

        public void Disconnect(Guid userId, int tenantId)
        {
            CachedTelegramDao.Instance.Delete(userId, tenantId);
        }

        private bool IsAwaitingRegistration(Guid userId, int tenantId)
        {
            return GetCurrentToken(userId, tenantId) == null ? false : true;
        }

        private string GetCurrentToken(Guid userId, int tenantId)
        {
            using (var client = Client)
            {
                return client.RegistrationToken(userId.ToString(), tenantId);
            }
        }

        private string GenerateToken(Guid userId)
        {
            var id = userId.ToByteArray();
            var d = BitConverter.GetBytes(DateTime.Now.Ticks);

            var buf = id.Concat(d).ToArray();

            using (var sha = new SHA256CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha.ComputeHash(buf))
                    .Replace('+', '-').Replace('/', '_').Replace("=", ""); // make base64 url safe
            }
        }

        private string GetLink(string token)
        {
            var tgProvider = (ITelegramLoginProvider)ConsumerFactory.GetByName("Telegram");
            var botname = tgProvider == null ? default(string) : tgProvider.TelegramBotName;
            if (string.IsNullOrEmpty(botname)) return null;

            return string.Format("t.me/{0}?start={1}", botname, token);
        }
    }
}
