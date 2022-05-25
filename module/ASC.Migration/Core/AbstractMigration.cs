using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core
{
    public abstract class AbstractMigration<TMigrationInfo, TUser, TContacts, TCalendar, TFiles, TMail> : IMigration
        where TMigrationInfo : IMigrationInfo
    {
        private MigrationLogger logger;
        protected CancellationToken cancellationToken;
        protected TMigrationInfo migrationInfo;
        private double lastProgressUpdate;
        private string lastStatusUpdate;
        protected List<Guid> importedUsers;

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

        public abstract Task<MigrationApiInfo> Parse();

        public abstract Task Migrate(MigrationApiInfo migrationInfo);

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
