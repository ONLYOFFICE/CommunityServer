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

using ASC.Common.Module;
using ASC.Core.Common.Configuration;
using ASC.Notify.Messages;

namespace ASC.Core.Common.Notify
{
    public class TelegramServiceClient : BaseWcfClient<ITelegramService>, ITelegramService, IDisposable
    {
        public void SendMessage(NotifyMessage m)
        {
            Channel.SendMessage(m);
        }

        public void RegisterUser(string userId, int tenantId, string token)
        {
            Channel.RegisterUser(userId, tenantId, token);
        }

        public bool CheckConnection(int tenantId, string token, int tokenLifespan, string proxy)
        {
            return Channel.CheckConnection(tenantId, token, tokenLifespan, proxy);
        }

        public string RegistrationToken(string userId, int tenantId)
        {
            return Channel.RegistrationToken(userId, tenantId);
        }
    }
}
