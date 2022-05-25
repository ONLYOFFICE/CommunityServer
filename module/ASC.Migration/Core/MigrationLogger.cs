using System;
using System.IO;

using ASC.Common.Logging;

namespace ASC.Migration.Core
{
    public class MigrationLogger
    {
        private ILog logger;
        private string migrationLogPath;
        private Stream migration;
        private StreamWriter migrationLog;

        public MigrationLogger()
        {
            migrationLogPath = TempPath.GetTempFileName();
            migration = new FileStream(migrationLogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            migrationLog = new StreamWriter(migration);
            logger = LogManager.GetLogger("ASC.Migration");
        }

        public void Log(string msg, Exception exception = null)
        {
            try
            {
                if (exception != null)
                {
                    logger.Warn(msg, exception);
                }
                else
                {
                    logger.Info(msg);
                }
                migrationLog.WriteLine($"{DateTime.Now.ToString("s")}: {msg}");
                if (exception != null)
                {
                    migrationLog.WriteLine($"{exception.Message}");
                }
                migrationLog.Flush();
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                migrationLog.Dispose();
                File.Delete(migrationLogPath);
            }
            catch { }
        }

        public Stream GetStream()
        {
            migration.Position = 0;
            return migration;
        }
    }
}
