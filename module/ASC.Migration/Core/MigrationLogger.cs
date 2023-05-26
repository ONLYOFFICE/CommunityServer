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
