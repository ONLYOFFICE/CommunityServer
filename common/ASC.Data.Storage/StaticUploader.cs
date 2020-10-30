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
using System.IO;
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
    public class StaticUploader
    {
        private static readonly TaskScheduler Scheduler;
        private static readonly CancellationTokenSource TokenSource;

        private static readonly ICache Cache;
        private static readonly object Locker;

        static StaticUploader()
        {
            Scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
            Cache = AscCache.Memory;
            Locker = new object();
            TokenSource = new CancellationTokenSource();
        }

        public static string UploadFile(string relativePath, string mappedPath, Action<string> onComplete = null)
        {
            if (TokenSource.Token.IsCancellationRequested) return null;
            if (!CanUpload()) return null;
            if (!File.Exists(mappedPath)) return null;

            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            UploadOperation uploadOperation;
            var key = GetCacheKey(tenantId.ToString(), relativePath);

            lock (Locker)
            {
                uploadOperation = Cache.Get<UploadOperation>(key);
                if (uploadOperation != null)
                {
                    return !string.IsNullOrEmpty(uploadOperation.Result) ? uploadOperation.Result : string.Empty;
                }

                uploadOperation = new UploadOperation(tenantId, relativePath, mappedPath);
                Cache.Insert(key, uploadOperation, DateTime.MaxValue);
            }

            uploadOperation.DoJob();
            if (onComplete != null)
            {
                onComplete(uploadOperation.Result);
            }

            return uploadOperation.Result;
        }

        public static Task<string> UploadFileAsync(string relativePath, string mappedPath, Action<string> onComplete = null)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var task = new Task<string>(() =>
            {
                CoreContext.TenantManager.SetCurrentTenant(tenantId);
                return UploadFile(relativePath, mappedPath, onComplete);
            }, TaskCreationOptions.LongRunning);

            task.ConfigureAwait(false);

            task.Start(Scheduler);

            return task;
        }

        public static async void UploadDir(string relativePath, string mappedPath)
        {
            if (!CanUpload()) return;
            if (!Directory.Exists(mappedPath)) return;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var key = typeof(UploadOperationProgress).FullName + tenant.TenantId;
            UploadOperationProgress uploadOperation;

            lock (Locker)
            {
                uploadOperation = Cache.Get<UploadOperationProgress>(key);
                if (uploadOperation != null) return;

                uploadOperation = new UploadOperationProgress(relativePath, mappedPath);
                Cache.Insert(key, uploadOperation, DateTime.MaxValue);
            }


            tenant.SetStatus(TenantStatus.Migrating);
            CoreContext.TenantManager.SaveTenant(tenant);

            await uploadOperation.RunJobAsync();

            tenant.SetStatus(Core.Tenants.TenantStatus.Active);
            CoreContext.TenantManager.SaveTenant(tenant);
        }

        public static bool CanUpload()
        {
            var current = CdnStorageSettings.Load().DataStoreConsumer;
            if (current == null || !current.IsSet || (string.IsNullOrEmpty(current["cnamessl"]) && string.IsNullOrEmpty(current["cname"])))
            {
                return false;
            }

            return true;
        }

        public static void Stop()
        {
            TokenSource.Cancel();
        }

        public static UploadOperationProgress GetProgress(int tenantId)
        {
            lock (Locker)
            {
                var key = typeof(UploadOperationProgress).FullName + tenantId;
                return Cache.Get<UploadOperationProgress>(key);
            }
        }

        private static string GetCacheKey(string tenantId, string path)
        {
            return typeof(UploadOperation).FullName + tenantId + path;
        }
    }

    public class UploadOperation
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        private readonly int tenantId;
        private readonly string path;
        private readonly string mappedPath;
        public string Result { get; private set; }

        public UploadOperation(int tenantId, string path, string mappedPath)
        {
            this.tenantId = tenantId;
            this.path = path.TrimStart('/');
            this.mappedPath = mappedPath;
            Result = string.Empty;
        }

        public string DoJob()
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                SecurityContext.AuthenticateMe(tenant.OwnerId);

                var dataStore = CdnStorageSettings.Load().DataStore;

                if (File.Exists(mappedPath))
                {
                    if (!dataStore.IsFile(path))
                    {
                        using (var stream = File.OpenRead(mappedPath))
                        {
                            dataStore.Save(path, stream);
                        }
                    }

                    Result = dataStore.GetInternalUri("", path, TimeSpan.Zero, null).AbsoluteUri.ToLower();
                    LogManager.GetLogger("ASC").DebugFormat("UploadFile {0}", Result);
                    return Result;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }
    }

    [DataContract]
    public class UploadOperationProgress : ProgressBase
    {
        private readonly string relativePath;
        private readonly string mappedPath;
        private readonly IEnumerable<string> directoryFiles;

        public UploadOperationProgress(string relativePath, string mappedPath)
        {
            this.relativePath = relativePath;
            this.mappedPath = mappedPath;

            var extensions = ".png|.jpeg|.jpg|.gif|.ico|.swf|.mp3|.ogg|.eot|.svg|.ttf|.woff|.woff2|.css|.less|.js";
            var extensionsArray = extensions.Split('|');

            directoryFiles = Directory.GetFiles(mappedPath, "*", SearchOption.AllDirectories)
                .Where(r => extensionsArray.Contains(Path.GetExtension(r)))
                .ToList();

            StepCount = directoryFiles.Count();
        }

        protected override async Task DoJobAsync()
        {
            var tasks = new List<Task>();
            foreach (var file in directoryFiles)
            {
                var filePath = file.Substring(mappedPath.TrimEnd('/').Length);
                tasks.Add(StaticUploader.UploadFileAsync(Path.Combine(relativePath, filePath), file, (res) => StepDone()));
            }

            await Task.WhenAll(tasks);
        }

        protected override void DoJob()
        {
            foreach (var file in directoryFiles)
            {
                var filePath = file.Substring(mappedPath.TrimEnd('/').Length);
                StaticUploader.UploadFileAsync(Path.Combine(relativePath, filePath), file, (res) => StepDone()).Wait();
            }
        }
    }
}
