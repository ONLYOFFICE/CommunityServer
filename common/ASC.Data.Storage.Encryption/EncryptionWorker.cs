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
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Caching;
using ASC.Common.Threading;

namespace ASC.Data.Storage.Encryption
{
    public class EncryptionWorker
    {
        private static readonly TaskScheduler Scheduler;
        private static readonly CancellationTokenSource TokenSource;
        private static readonly ICache Cache;
        private static readonly object Locker;

        static EncryptionWorker()
        {
            Scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
            TokenSource = new CancellationTokenSource();
            Cache = AscCache.Memory;
            Locker = new object();
        }

        public static void Start(EncryptionSettings encryptionSettings, string serverRootPath)
        {
            if (TokenSource.Token.IsCancellationRequested) return;

            EncryptionOperation encryptionOperation;

            lock (Locker)
            {
                encryptionOperation = Cache.Get<EncryptionOperation>(GetCacheKey());
                if (encryptionOperation != null) return;

                encryptionOperation = new EncryptionOperation(encryptionSettings, serverRootPath);
                Cache.Insert(GetCacheKey(), encryptionOperation, DateTime.MaxValue);
            }

            var task = new Task(encryptionOperation.RunJob, TokenSource.Token, TaskCreationOptions.LongRunning);

            task.ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    lock (Locker)
                    {
                        Cache.Remove(GetCacheKey());
                    }
                });

            task.Start(Scheduler);
        }

        public static EncryptionOperation GetProgress()
        {
            lock (Locker)
            {
                return Cache.Get<EncryptionOperation>(GetCacheKey());
            }
        }

        public static void Stop()
        {
            TokenSource.Cancel();
        }

        private static string GetCacheKey()
        {
            return typeof(EncryptionOperation).FullName;
        }
    }
}
