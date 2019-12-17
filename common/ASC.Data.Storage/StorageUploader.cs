/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage
{
    public class StorageUploader
    {
        private static readonly TaskScheduler Scheduler;
        private static readonly CancellationTokenSource TokenSource;

        private static readonly ICache Cache;
        private static readonly object Locker;

        static StorageUploader()
        {
            Scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
            TokenSource = new CancellationTokenSource();
            Cache = AscCache.Memory;
            Locker = new object();
        }

        public static void Start(int tenantId, StorageSettings newStorageSettings)
        {
            if (TokenSource.Token.IsCancellationRequested) return;

            MigrateOperation migrateOperation;

            lock (Locker)
            {
                migrateOperation = Cache.Get<MigrateOperation>(GetCacheKey(tenantId));
                if (migrateOperation != null) return;

                migrateOperation = new MigrateOperation(tenantId, newStorageSettings);
                Cache.Insert(GetCacheKey(tenantId), migrateOperation, DateTime.MaxValue);
            }

            var task = new Task(migrateOperation.RunJob, TokenSource.Token, TaskCreationOptions.LongRunning);

            task.ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    lock (Locker)
                    {
                        Cache.Remove(GetCacheKey(tenantId));
                    }
                });

            task.Start(Scheduler);
        }

        public static MigrateOperation GetProgress(int tenantId)
        {
            lock (Locker)
            {
                return Cache.Get<MigrateOperation>(GetCacheKey(tenantId));
            }
        }

        public static void Stop()
        {
            TokenSource.Cancel();
        }

        private static string GetCacheKey(int tenantId)
        {
            return typeof(MigrateOperation).FullName + tenantId;
        }
    }

    [DataContract]
    public class MigrateOperation : ProgressBase
    {
        private static readonly ILog Log;
        private static readonly string ConfigPath;
        private static readonly IEnumerable<string> Modules;
        private readonly StorageSettings settings;
        private readonly int tenantId;

        static MigrateOperation()
        {
            Log = LogManager.GetLogger("ASC");
            ConfigPath = "";
            Modules = StorageFactory.GetModuleList(ConfigPath, true);
        }

        public MigrateOperation(int tenantId, StorageSettings settings)
        {
            this.tenantId = tenantId;
            this.settings = settings;
            StepCount = Modules.Count();
        }

        protected override void DoJob()
        {
            try
            {
                Log.DebugFormat("Tenant: {0}", tenantId);
                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                SecurityContext.AuthenticateMe(tenant.OwnerId);

                foreach (var module in Modules)
                {
                    var oldStore = StorageFactory.GetStorage(ConfigPath, tenantId.ToString(), module);
                    var store = StorageFactory.GetStorageFromConsumer(ConfigPath, tenantId.ToString(), module, settings.DataStoreConsumer);
                    var domains = StorageFactory.GetDomainList(ConfigPath, module).ToList();

                    var crossModuleTransferUtility = new CrossModuleTransferUtility(oldStore, store);

                    string[] files;
                    foreach (var domain in domains)
                    {
                        Status = module + domain;
                        Log.DebugFormat("Domain: {0}", domain);
                        files = oldStore.ListFilesRelative(domain, "\\", "*.*", true);

                        foreach (var file in files)
                        {
                            Log.DebugFormat("File: {0}", file);
                            crossModuleTransferUtility.CopyFile(domain, file, domain, file);
                        }
                    }

                    Log.DebugFormat("Module:{0},Domain:", module);

                    files = oldStore.ListFilesRelative(string.Empty, "\\", "*.*", true)
                        .Where(path => domains.All(domain => !path.Contains(domain + "/")))
                        .ToArray();

                    foreach (var file in files)
                    {
                        Log.DebugFormat("File: {0}", file);
                        crossModuleTransferUtility.CopyFile("", file, "", file);
                    }

                    StepDone();
                    Log.DebugFormat("Percentage:{0}", Percentage);
                }

                settings.Save();
                Log.Debug("StaticUploader Save");
                tenant.SetStatus(TenantStatus.Active);
                CoreContext.TenantManager.SaveTenant(tenant);
                Log.Debug("StaticUploader SetStatus");
            }
            catch (Exception e)
            {
                Error = e;
                Log.Error(e);
            }
        }
    }
}
