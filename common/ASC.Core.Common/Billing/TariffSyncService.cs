/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Common.Module;
using ASC.Core.Data;
using ASC.Core.Tenants;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace ASC.Core.Billing
{
    class TariffSyncService : ITariffSyncService, IServiceController
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(TariffSyncService));
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
                var tenant = CoreContext.TenantManager.GetTenants().OrderByDescending(t => t.Version).FirstOrDefault();
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
