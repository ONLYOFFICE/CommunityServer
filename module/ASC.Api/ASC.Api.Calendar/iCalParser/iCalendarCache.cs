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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASC.Api.Calendar.iCalParser
{
    internal class iCalendarCacheParams
    {
        public string FolderCachePath { get; private set; }
        public int ExpiredPeriod { get; private set; }

        public iCalendarCacheParams(string folderCachPath, int expiredPeriod)
        {
            FolderCachePath = folderCachPath.TrimEnd('\\') + "\\";
            ExpiredPeriod = expiredPeriod;
        }

        public static iCalendarCacheParams Default
        {
            get {
                var path = Environment.CurrentDirectory.TrimEnd('\\') + "\\addons\\calendar\\ical_cache";
                return new iCalendarCacheParams(path, 2);
            }
        }
    }

    internal class iCalendarCache
    {
        private iCalendarCacheParams _cacheParams;

        public iCalendarCache() : this(iCalendarCacheParams.Default){}
        public iCalendarCache(iCalendarCacheParams cacheParams)
        {
            _cacheParams = cacheParams;
        }

        public bool UpdateCalendarCache(string calendarId, TextReader textReader)
        {
            var curDate = DateTime.UtcNow;

            string fileName = calendarId+".ics";
            ClearCache(calendarId);

            var buffer = new char[1024*1024];
            try
            {
                if (!Directory.Exists(_cacheParams.FolderCachePath))
                    Directory.CreateDirectory(_cacheParams.FolderCachePath);

                using (var sw = File.CreateText(_cacheParams.FolderCachePath + fileName))
                {
                    while (true)
                    {
                        var count = textReader.Read(buffer, 0, buffer.Length);
                        if (count <= 0)
                            break;

                        sw.Write(buffer, 0, count);
                    }
                }
            }
            catch
            {
                return false;
            }            

            return true;
        }

        public iCalendar GetCalendarFromCache(string calendarId)
        {
            var filePath = _cacheParams.FolderCachePath + calendarId+".ics";
            if (File.Exists(filePath))            
            {                
                var fi = new FileInfo(filePath);
                if ((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalMinutes > _cacheParams.ExpiredPeriod)
                    return null;

                using (var tr = new StreamReader(File.OpenRead(filePath)))
                {
                    return iCalendar.GetFromStream(tr);
                }
            }

            return null;
        }

        public void ClearCache(string calendarId)
        {
            var filePath = _cacheParams.FolderCachePath + calendarId + ".ics";
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }

        public void ClearCache()
        {
            foreach (var file in Directory.GetFiles(_cacheParams.FolderCachePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
        }
    }
}
