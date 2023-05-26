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

using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;

namespace ASC.Migration
{
    public class MigrateProgressItem : IProgressItem
    {
        private readonly int _tenantId;
        public object Id { get; set; }
        public object Status { get { return migration.GetProgressStatus(); } set { } }
        public object Error { get; set; }
        public double Percentage { get { return migration.GetProgress(); } set { } }
        public bool IsCompleted { get; set; }
        private IMigration migration;
        private MigrationApiInfo migrationInfo;

        public MigrateProgressItem(IMigration migration, MigrationApiInfo migrationInfo, int tenantId)
        {
            this.migration = migration;
            this.migrationInfo = migrationInfo;
            _tenantId = tenantId;
            Id = QueueWorker.GetProgressItemId(tenantId, typeof(MigrateProgressItem));
            Error = null;
            Percentage = 0;
            IsCompleted = false;
        }

        public void RunJob()
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(_tenantId);
                migration.Migrate(migrationInfo);
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
