/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using log4net;
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

            var repo = LogManager.GetRepository();

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

                foreach (var file in files.Where(file => (DateTime.UtcNow.Date - file.CreationTimeUtc.Date).Days > GetCleanPeriod()))
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
