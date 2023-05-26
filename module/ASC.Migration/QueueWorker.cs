/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Threading.Progress;
using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;
using ASC.Web.Studio.Utility;

namespace ASC.Migration
{
    public class QueueWorker
    {
        private static readonly ProgressQueue Queue = new ProgressQueue(1, TimeSpan.FromMinutes(5), true);
        public static CancellationTokenSource CancelTokenSource { get; set; }

        public static string GetProgressItemId(int tenantId, Type progressItemType)
        {
            return string.Format("{0}_{1}", tenantId, progressItemType.Name);
        }

        public static IProgressItem GetProgressItemStatus(int tenantId, Type progressItemType)
        {
            var id = GetProgressItemId(tenantId, progressItemType);
            return Queue.GetStatus(id);
        }

        public static void Terminate()
        {
            var tenantId = TenantProvider.CurrentTenantID;
            var item = GetProgressItemStatus(tenantId, typeof(ParseProgressItem)) ?? GetProgressItemStatus(tenantId, typeof(MigrateProgressItem));

            if (item != null)
                Queue.Remove(item);
        }

        public static ParseProgressItem StartParsing(IMigration migration, string path, CancellationToken cancellationToken)
        {
            lock (Queue.SynchRoot)
            {
                var tenantId = TenantProvider.CurrentTenantID;
                var task = GetProgressItemStatus(tenantId, typeof(ParseProgressItem)) as ParseProgressItem;
                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }
                if (task == null)
                {
                    task = new ParseProgressItem(migration, path, tenantId, cancellationToken);
                    Queue.Add(task);
                }
                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }

        public static MigrateProgressItem StartMigration(IMigration migration, MigrationApiInfo info)
        {
            lock (Queue.SynchRoot)
            {
                var tenantId = TenantProvider.CurrentTenantID;
                var task = GetProgressItemStatus(tenantId, typeof(MigrateProgressItem)) as MigrateProgressItem;
                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }
                if (task == null)
                {
                    task = new MigrateProgressItem(migration, info, tenantId);
                    Queue.Add(task);
                }
                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }
    }
}
