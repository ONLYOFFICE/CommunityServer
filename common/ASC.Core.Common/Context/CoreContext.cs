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


using System.Configuration;

using ASC.Common.Data;
using ASC.Core.Billing;
using ASC.Core.Caching;
using ASC.Core.Data;


namespace ASC.Core
{
    public static class CoreContext
    {
        static CoreContext()
        {
            ConfigureCoreContextByDefault();
        }


        public static CoreConfiguration Configuration { get; private set; }

        public static TenantManager TenantManager { get; private set; }

        public static UserManager UserManager { get; private set; }

        public static AuthManager Authentication { get; private set; }

        public static AuthorizationManager AuthorizationManager { get; private set; }

        public static PaymentManager PaymentManager { get; private set; }

        public static SubscriptionManager SubscriptionManager { get; private set; }

        private static bool QuotaCacheEnabled
        {
            get
            {
                if (ConfigurationManagerExtension.AppSettings["core.enable-quota-cache"] == null)
                    return true;

                bool enabled;

                return !bool.TryParse(ConfigurationManagerExtension.AppSettings["core.enable-quota-cache"], out enabled) || enabled;
            }
        }

        private static void ConfigureCoreContextByDefault()
        {
            var cs = DbRegistry.GetConnectionString("core");
            if (cs == null)
            {
                throw new ConfigurationErrorsException("Can not configure CoreContext: connection string with name core not found.");
            }

            var tenantService = new CachedTenantService(new DbTenantService(cs));
            var userService = new CachedUserService(new DbUserService(cs));
            var azService = new CachedAzService(new DbAzService(cs));
            var quotaService = QuotaCacheEnabled ? (IQuotaService) new CachedQuotaService(new DbQuotaService(cs)) : new DbQuotaService(cs);
            var subService = new CachedSubscriptionService(new DbSubscriptionService(cs));
            var tariffService = new TariffService(cs, quotaService, tenantService);

            Configuration = new CoreConfiguration(tenantService);
            TenantManager = new TenantManager(tenantService, quotaService, tariffService);
            PaymentManager = new PaymentManager(tariffService);
            UserManager = new UserManager(userService);
            Authentication = new AuthManager(userService);
            AuthorizationManager = new AuthorizationManager(azService);
            SubscriptionManager = new SubscriptionManager(subService);
        }
    }
}