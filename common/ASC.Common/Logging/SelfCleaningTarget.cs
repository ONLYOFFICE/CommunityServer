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
                    dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dirPath);

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

            base.Write(logEvents);
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
