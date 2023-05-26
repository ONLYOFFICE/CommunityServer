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
using System.Collections.Generic;
using System.IO;
using System.Threading;

using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core
{
    public abstract class AbstractMigration<TMigrationInfo, TUser, TContacts, TCalendar, TFiles, TMail> : IMigration
        where TMigrationInfo : IMigrationInfo
    {
        private MigrationLogger logger;
        protected TMigrationInfo migrationInfo;
        private double lastProgressUpdate;
        private string lastStatusUpdate;
        protected List<Guid> importedUsers;
        protected CancellationToken cancellationToken;

        public event Action<double, string> OnProgressUpdate;

        public AbstractMigration()
        {
            logger = new MigrationLogger();
        }

        protected void ReportProgress(double value, string status)
        {
            lastProgressUpdate = value;
            lastStatusUpdate = status;
            OnProgressUpdate?.Invoke(value, status);
            logger.Log($"{value:0.00} progress: {status}");
        }

        public double GetProgress() => lastProgressUpdate;
        public string GetProgressStatus() => lastStatusUpdate;

        public abstract void Init(string path, CancellationToken cancellationToken);

        public abstract MigrationApiInfo Parse();

        public abstract void Migrate(MigrationApiInfo migrationInfo);

        public void Log(string msg, Exception exception = null)
        {
            logger.Log(msg, exception);
        }
        public virtual void Dispose()
        {
            logger.Dispose();
        }

        public Stream GetLogs()
        {
            return logger.GetStream();
        }

        public virtual List<Guid> GetGuidImportedUsers()
        {
            return importedUsers;
        }
    }
}
