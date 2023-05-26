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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using ASC.Common.Logging;

namespace ASC.Data.Backup.Service
{
    public class BackupCleanerTempFileService
    {
        private bool isStarted;
        private readonly ILog log = LogManager.GetLogger("ASC.Backup.CleanerTempfile");
        private Timer timer;
        private string tempFolder = BackupWorker.TempFolder;
        public void Start()
        {
            if (!isStarted)
            {
                log.Info("staring backup cleaner temp file service...");
                timer = new Timer(_ => CleanerTempFileTask(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
                log.Info("backup cleaner temp file service service started");
                isStarted = true;
            }
        }

        private void CleanerTempFileTask()
        {
            var date = DateTime.UtcNow.AddDays(-7);
            Regex regex = new Regex(@"^\w*_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}.tar.gz$");
            var files = Directory.EnumerateFiles(tempFolder).Where(f => regex.IsMatch(Path.GetFileName(f)) && new FileInfo(f).LastWriteTimeUtc < date);
            foreach (var file in files)
            {
                File.Delete(file);
                log.Info(string.Format("delete file {0}", Path.GetFileName(file)));
            }
        }

        public void Stop()
        {
            if (isStarted)
            {
                log.Info("stoping backup cleaner temp file service...");
                if (timer != null)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose();
                    timer = null;
                }
                log.Info("backup cleaner temp file service stoped");
                isStarted = false;
            }
        }
    }
}
