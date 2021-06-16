﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using ASC.Core.Common.Contracts;
using ASC.Core.Common.Notify;
using ASC.Notify.Messages;

namespace ASC.TelegramService
{
    public class TelegramService : ITelegramService, IHealthCheckService
    {
        public void SendMessage(NotifyMessage m)
        {
            Launcher.Handler.SendMessage(m);
        }

        public void RegisterUser(string userId, int tenantId, string token)
        {
            Launcher.Handler.RegisterUser(userId, tenantId, token);
        }

        public bool CheckConnection(int tenantId, string token, int tokenLifespan, string proxy)
        {
            if (string.IsNullOrEmpty(token))
            {
                Launcher.Handler.DisableClient(tenantId);
                return true;
            }
            else
            {
                return Launcher.Handler.CreateOrUpdateClientForTenant(tenantId, token, tokenLifespan, proxy);
            }
        }


        public string RegistrationToken(string userId, int tenantId)
        {
            return Launcher.Handler.CurrentRegistrationToken(userId, tenantId);
        }

        public HealthCheckResponse CheckHealth()
        {
            return HealthCheckResult.ToResponse(new HealthCheckResult
            {
                Message = "Service Telegram is OK! Warning: Method is not implement. Always return the Healthy status",
                Status = HealthStatus.Healthy
            });
        }
    }
}