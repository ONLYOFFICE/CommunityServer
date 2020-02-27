/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
