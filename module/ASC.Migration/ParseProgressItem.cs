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
using ASC.Core;
using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;

namespace ASC.Migration
{
    public class ParseProgressItem : IProgressItem
    {
        private readonly int _tenantId;
        public object Id { get; set; }
        public object Status { get { return migration.GetProgressStatus(); } set { } }
        public object Error { get; set; }
        public double Percentage { get { return migration.GetProgress(); } set { } }
        public bool IsCompleted { get; set; }
        private IMigration migration;
        private string path;
        public MigrationApiInfo migrationInfo;
        public CancellationToken cancellationToken;

        public ParseProgressItem(IMigration migration, string path, int tenantId, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.migration = migration;
            this.path = path;
            _tenantId = tenantId;
            Id = QueueWorker.GetProgressItemId(tenantId, typeof(ParseProgressItem));
            Error = null;
            Percentage = 0;
            IsCompleted = false;
        }

        public void RunJob()
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(_tenantId);
                migration.Init(path, cancellationToken);
                migrationInfo = migration.Parse();
                IsCompleted = true;
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
