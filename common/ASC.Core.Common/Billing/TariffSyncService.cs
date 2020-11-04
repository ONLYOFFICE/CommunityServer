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
using System.Configuration;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Core.Data;
using ASC.Core.Tenants;

namespace ASC.Core.Billing
{
    public class TariffSyncService : ITariffSyncService, IServiceController
    {
        private readonly static ILog log = LogManager.GetLogger("ASC");
        private readonly TariffSyncServiceSection config;
        private readonly IDictionary<int, IEnumerable<TenantQuota>> quotaServices = new Dictionary<int, IEnumerable<TenantQuota>>();
        private Timer timer;


        public TariffSyncService()
        {
            config = TariffSyncServiceSection.GetSection();
        }


        // server part of service
        public IEnumerable<TenantQuota> GetTariffs(int version, string key)
        {
            lock (quotaServices)
            {
                if (!quotaServices.ContainsKey(version))
                {
                    var cs = ConfigurationManager.ConnectionStrings[config.ConnectionStringName + version] ??
                             ConfigurationManager.ConnectionStrings[config.ConnectionStringName];
                    quotaServices[version] = new DbQuotaService(cs).GetTenantQuotas();
                }
                return quotaServices[version];
            }
        }


        // client part of service
        public string ServiceName
        {
            get { return "Tariffs synchronizer"; }
        }

        public void Start()
        {
            if (timer == null)
            {
                timer = new Timer(Sync, null, TimeSpan.Zero, config.Period);
            }
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }
        }

        private void Sync(object _)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetTenants(false).OrderByDescending(t => t.Version).FirstOrDefault();
                if (tenant != null)
                {
                    using (var wcfClient = new TariffSyncClient())
                    {
                        var quotaService = new DbQuotaService(ConfigurationManager.ConnectionStrings[config.ConnectionStringName]);

                        var oldtariffs = quotaService.GetTenantQuotas().ToDictionary(t => t.Id);
                        // save new
                        foreach (var tariff in wcfClient.GetTariffs(tenant.Version, CoreContext.Configuration.GetKey(tenant.TenantId)))
                        {
                            quotaService.SaveTenantQuota(tariff);
                            oldtariffs.Remove(tariff.Id);
                        }

                        // remove old
                        foreach (var tariff in oldtariffs.Values)
                        {
                            tariff.Visible = false;
                            quotaService.SaveTenantQuota(tariff);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                log.Error(error);
            }
        }
    }
}
