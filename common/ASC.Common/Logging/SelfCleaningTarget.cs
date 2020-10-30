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


using System.Collections.Generic;
using NLog;
using NLog.Common;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;

namespace ASC.Common.Logging
{
    [Target("SelfCleaning")] 
    public class SelfCleaningTarget : FileTarget
    {
        private static DateTime _lastCleanDate;

        private static int? _cleanPeriod;

        private static int GetCleanPeriod()
        {
            if (_cleanPeriod != null)
                return _cleanPeriod.Value;

            var value = 30;

            const string key = "cleanPeriod";

            if (NLog.LogManager.Configuration.Variables.Keys.Contains(key))
            {
                var variable = NLog.LogManager.Configuration.Variables[key];

                if (variable != null && !string.IsNullOrEmpty(variable.Text))
                {
                    int.TryParse(variable.Text, out value);
                }
            }

            _cleanPeriod = value;

            return value;
        }

        private void Clean()
        {
            var filePath = string.Empty;
            var dirPath = string.Empty;

            try
            {
                if (FileName == null)
                    return;

                filePath = ((NLog.Layouts.SimpleLayout)FileName).Text;

                if (string.IsNullOrEmpty(filePath))
                    return;

                dirPath = Path.GetDirectoryName(filePath);

                if (string.IsNullOrEmpty(dirPath))
                    return;

                if (!Path.IsPathRooted(dirPath))
                    dirPath = Path.Combine(Environment.CurrentDirectory, dirPath);

                var directory = new DirectoryInfo(dirPath);

                if (!directory.Exists)
                    return;

                var files = directory.GetFiles();

                var cleanPeriod = GetCleanPeriod();

                foreach (var file in files.Where(file => (DateTime.UtcNow.Date - file.CreationTimeUtc.Date).Days > cleanPeriod))
                {
                    file.Delete();
                }
            }
            catch (Exception err)
            {
                base.Write(new LogEventInfo
                    {
                        Exception = err,
                        Level = LogLevel.Error,
                        Message = String.Format("file: {0}, dir: {1}, mess: {2}", filePath, dirPath, err.Message),
                        LoggerName = "SelfCleaningTarget"
                    });
            }
        }

        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (DateTime.UtcNow.Date > _lastCleanDate.Date)
            {
                _lastCleanDate = DateTime.UtcNow.Date;
                Clean();
            }

            var buffer = new List<AsyncLogEventInfo>();

            foreach (var logEvent in logEvents)
            {
                buffer.Add(logEvent);
                if (buffer.Count < 10) continue;
                base.Write(buffer);
                buffer.Clear();
            }

            base.Write(buffer);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (DateTime.UtcNow.Date > _lastCleanDate.Date)
            {
                _lastCleanDate = DateTime.UtcNow.Date;
                Clean();
            }

            base.Write(logEvent);
        } 
    }
}
