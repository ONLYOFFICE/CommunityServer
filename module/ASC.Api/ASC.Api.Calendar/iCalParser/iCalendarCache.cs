/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
                var path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\addons\\calendar\\ical_cache";
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
