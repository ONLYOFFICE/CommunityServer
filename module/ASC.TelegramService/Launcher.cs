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


using System.ServiceModel;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.TelegramService.Core;

namespace ASC.TelegramService
{
    public class Launcher : IServiceController
    {
        public static TelegramHandler Handler { get; private set; }

        private ServiceHost _serviceHost;
        private readonly CommandModule command;

        private readonly ILog log = LogManager.GetLogger("ASC.Telegram");

        public Launcher()
        {
            command = new CommandModule(log);
            Handler = new TelegramHandler(command, log);
        }

        public void Start()
        {
            _ = Task.Run(CreateClients);
            _serviceHost = new ServiceHost(typeof(TelegramService));
            _serviceHost.Open();
        }

        public void Stop()
        {
            _serviceHost.Close();
            _serviceHost = null;
        }

        private void CreateClients()
        {
            var tenants = CoreContext.TenantManager.GetTenants();
            foreach (var tenant in tenants)
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var consumer = TelegramLoginProvider.Instance;
                if (consumer.IsEnabled())
                {
                    Handler.CreateOrUpdateClientForTenant(tenant.TenantId, consumer.TelegramBotToken, consumer.TelegramAuthTokenLifespan, consumer.TelegramProxy, true);
                }
            }
        }
    }
}