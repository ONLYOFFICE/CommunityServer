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
using System.Threading.Tasks;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Common.DependencyInjection;
using ASC.Common.Threading;
using ASC.Notify;
using ASC.Notify.Model;

using Autofac;

namespace ASC.ActiveDirectory.Base
{
    public static class LdapNotifyHelper
    {
        private static readonly Dictionary<int, Tuple<INotifyClient, LdapNotifySource>> clients;
        private static readonly DistributedTaskQueue ldapTasks;

        private static IContainer Builder { get; set; }
        private static INotifySource studioNotify;
        private static INotifyClient notifyClient;

        public static INotifyClient StudioNotifyClient
        {
            get
            {
                if (studioNotify == null)
                {
                    studioNotify = Builder.Resolve<INotifySource>();
                }

                if (notifyClient == null)
                {
                    notifyClient = WorkContext.NotifyContext.NotifyService.RegisterClient(studioNotify);
                }

                return notifyClient;
            }
        }

        static LdapNotifyHelper()
        {
            var container = AutofacConfigLoader.Load("ldap");
            if (container != null)
            {
                Builder = container.Build();
            }

            clients = new Dictionary<int, Tuple<INotifyClient, LdapNotifySource>>();
            ldapTasks = new DistributedTaskQueue("ldapAutoSyncOperations");
        }

        public static void RegisterAll()
        {
            var task = new Task(() =>
            {
                var tenants = CoreContext.TenantManager.GetTenants();
                foreach (var t in tenants)
                {
                    var tId = t.TenantId;

                    var cronSettings = LdapCronSettings.LoadForTenant(tId);

                    if (string.IsNullOrEmpty(cronSettings.Cron))
                        continue;

                    if (LdapSettings.LoadForTenant(tId).EnableLdapAuthentication)
                    {
                        RegisterAutoSync(t, cronSettings.Cron);
                    }
                    else
                    {
                        cronSettings.Cron = null;
                        cronSettings.Save();
                    }
                }
            }, TaskCreationOptions.LongRunning);

            task.Start();
        }

        public static void RegisterAutoSync(Tenant tenant, string cron)
        {
            if (!clients.ContainsKey(tenant.TenantId))
            {
                var source = new LdapNotifySource(tenant);
                var client = WorkContext.NotifyContext.NotifyService.RegisterClient(source);
                client.RegisterSendMethod(source.AutoSync, cron);
                clients.Add(tenant.TenantId, new Tuple<INotifyClient, LdapNotifySource>(client, source));
            }
        }

        public static void UnregisterAutoSync(Tenant tenant)
        {
            if (clients.ContainsKey(tenant.TenantId))
            {
                var client = clients[tenant.TenantId];
                client.Item1.UnregisterSendMethod(client.Item2.AutoSync);
                clients.Remove(tenant.TenantId);
            }
        }

        public static void AutoSync(Tenant tenant)
        {
            var ldapSettings = LdapSettings.LoadForTenant(tenant.TenantId);

            if (!ldapSettings.EnableLdapAuthentication)
            {
                var cronSettings = LdapCronSettings.LoadForTenant(tenant.TenantId);
                cronSettings.Cron = "";
                cronSettings.SaveForTenant(tenant.TenantId);
                UnregisterAutoSync(tenant);
                return;
            }

            var op = new LdapSaveSyncOperation(ldapSettings, tenant, LdapOperationType.Sync);
            ldapTasks.QueueTask(op.RunJob, op.GetDistributedTask());
        }
    }
}
