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
