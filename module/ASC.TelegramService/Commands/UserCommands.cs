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
using System.Runtime.Caching;
using System.Threading.Tasks;

using ASC.Core.Common.Notify.Telegram;
using ASC.TelegramService.Core;

namespace ASC.TelegramService.Commands
{
    public class UserCommands : CommandContext
    {
        [Command("start")]
        public async Task StartCommand(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            var user = MemoryCache.Default.Get(token);

            if (user != null)
            {
                MemoryCache.Default.Remove(token);
                MemoryCache.Default.Remove((string)user);
                var split = ((string)user).Split(':');

                var guid = Guid.Parse(split[0]);
                var tenant = int.Parse(split[1]);

                if (tenant == TenantId)
                {
                    CachedTelegramDao.Instance.RegisterUser(guid, tenant, Context.User.Id);
                    await ReplyAsync("Ok!");
                    return;
                }
            }

            await ReplyAsync("Error");
        }
    }
}