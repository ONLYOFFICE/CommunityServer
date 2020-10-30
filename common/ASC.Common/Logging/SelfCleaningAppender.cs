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


using log4net.Appender;
using log4net.Core;
using log4net.Util;
using System;
using System.IO;
using System.Linq;

namespace ASC.Common.Logging
{
    public class SelfCleaningAppender : RollingFileAppender
    {
        private static DateTime _lastCleanDate;

        private static int? _cleanPeriod;

        private static int GetCleanPeriod()
        {
            if (_cleanPeriod != null)
                return _cleanPeriod.Value;

            const string key = "CleanPeriod";

            var value = 30;

            var repo = log4net.LogManager.GetRepository();

            if (repo != null && repo.Properties.GetKeys().Contains(key))
            {
                int.TryParse(repo.Properties[key].ToString(), out value);
            }

            _cleanPeriod = value;

            return value;
        }

        private void Clean()
        {
            try
            {
                if(string.IsNullOrEmpty(File))
                    return;
                
                var fileInfo = new FileInfo(File);

                if(!fileInfo.Exists)
                    return;

                var directory = fileInfo.Directory;

                if(directory == null || !directory.Exists)
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
                LogLog.Error(GetType(), err.Message, err);
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (DateTime.UtcNow.Date > _lastCleanDate.Date)
            {
                _lastCleanDate = DateTime.UtcNow.Date;
                Clean();
            }
            
            base.Append(loggingEvent);
        }
        
        protected override void Append(LoggingEvent[] loggingEvents)
        {
            if (DateTime.UtcNow.Date > _lastCleanDate.Date)
            {
                _lastCleanDate = DateTime.UtcNow.Date;
                Clean();
            }
            
            base.Append(loggingEvents);
        }
    }
}
