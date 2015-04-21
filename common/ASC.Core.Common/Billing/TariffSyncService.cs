/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
