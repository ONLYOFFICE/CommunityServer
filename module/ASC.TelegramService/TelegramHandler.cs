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
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Runtime.Caching;

using ASC.Common.Logging;
using ASC.Core.Common.Notify.Telegram;
using ASC.Notify.Messages;
using ASC.TelegramService.Core;

using Telegram.Bot;
using Telegram.Bot.Args;

namespace ASC.TelegramService
{
    public class TelegramHandler
    {
        private readonly Dictionary<int, TenantTgClient> clients;
        private readonly CommandModule command;
        private readonly ILog log;

        public TelegramHandler(CommandModule command, ILog log)
        {
            this.command = command;
            this.log = log;
            clients = new Dictionary<int, TenantTgClient>();
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        public async void SendMessage(NotifyMessage msg)
        {
            if (string.IsNullOrEmpty(msg.To)) return;
            if (!clients.ContainsKey(msg.Tenant)) return;

            var client = clients[msg.Tenant].Client;

            try
            {
                var tgUser = CachedTelegramDao.Instance.GetUser(Guid.Parse(msg.To), msg.Tenant);

                if (tgUser == null)
                {
                    log.DebugFormat("Couldn't find telegramId for user '{0}'", msg.To);
                    return;
                }

                var chat = await client.GetChatAsync(tgUser.TelegramId);
                await client.SendTextMessageAsync(chat, msg.Content, Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch (Exception e)
            {
                log.DebugFormat("Couldn't send message for user '{0}' got an '{1}'", msg.To, e.Message);
            }
        }

        public void DisableClient(int tenantId)
        {
            if (!clients.ContainsKey(tenantId)) return;

            var client = clients[tenantId];
            client.Client.StopReceiving();

            clients.Remove(tenantId);
        }

        public bool CreateOrUpdateClientForTenant(int tenantId, string token, int tokenLifespan, string proxy, bool force = false)
        {
            if (clients.ContainsKey(tenantId))
            {
                var client = clients[tenantId];
                client.TokenLifeSpan = tokenLifespan;

                if (token != client.Token || proxy != client.Proxy)
                {
                    var newClient = InitClient(token, proxy);

                    try
                    {
                        if (!newClient.TestApiAsync().GetAwaiter().GetResult()) return false;
                    }
                    catch (Exception e)
                    {
                        log.DebugFormat("Couldn't test api connection: {0}", e);
                        return false;
                    }


                    client.Client.StopReceiving();

                    BindClient(newClient, tenantId);

                    client.Client = newClient;
                    client.Token = token;
                    client.Proxy = proxy;
                }
            }
            else
            {
                var client = InitClient(token, proxy);

                if (!force)
                {
                    try
                    {
                        if (!client.TestApiAsync().GetAwaiter().GetResult()) return false;
                    }
                    catch (Exception e)
                    {
                        log.DebugFormat("Couldn't test api connection: {0}", e);
                        return false;
                    }
                }

                BindClient(client, tenantId);

                clients.Add(tenantId, new TenantTgClient()
                {
                    Token = token,
                    Client = client,
                    Proxy = proxy,
                    TenantId = tenantId,
                    TokenLifeSpan = tokenLifespan
                });
            }

            return true;
        }

        public void RegisterUser(string userId, int tenantId, string token)
        {
            if (!clients.ContainsKey(tenantId)) return;

            var userKey = UserKey(userId, tenantId);
            var dateExpires = DateTimeOffset.Now.AddMinutes(clients[tenantId].TokenLifeSpan);
            MemoryCache.Default.Set(token, userKey, dateExpires);
            MemoryCache.Default.Set(string.Format(userKey), token, dateExpires);
        }

        public string CurrentRegistrationToken(string userId, int tenantId)
        {
            return (string)MemoryCache.Default.Get(UserKey(userId, tenantId));
        }

        private async void OnMessage(object sender, MessageEventArgs e, TelegramBotClient client, int tenantId)
        {
            if (string.IsNullOrEmpty(e.Message.Text) || e.Message.Text[0] != '/') return;
            await command.HandleCommand(e.Message, client, tenantId);
        }

        private TelegramBotClient InitClient(string token, string proxy)
        {
            return string.IsNullOrEmpty(proxy) ? new TelegramBotClient(token) : new TelegramBotClient(token, new WebProxy(proxy));
        }

        private void BindClient(TelegramBotClient client, int tenantId)
        {
            client.OnMessage += (sender, e) => { OnMessage(sender, e, client, tenantId); };
            client.StartReceiving();
        }

        private string UserKey(string userId, int tenantId)
        {
            return string.Format("{0}:{1}", userId, tenantId);
        }
    }
}
