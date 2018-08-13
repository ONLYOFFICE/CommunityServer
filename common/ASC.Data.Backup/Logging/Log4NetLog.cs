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
using ILog4NetLog = log4net.ILog;

namespace ASC.Data.Backup.Logging
{
    internal class Log4NetLog : ILog
    {
        private readonly ILog4NetLog log;

        public Log4NetLog(ILog4NetLog log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            this.log = log;
        }

        public void Debug(string format)
        {
            log.Debug(format);
        }

        public void Debug(string format, object arg0)
        {
            log.DebugFormat(format, arg0);
        }

        public void Debug(string format, object arg0, object arg1)
        {
            log.DebugFormat(format, arg0, arg1);
        }

        public void Debug(string format, object arg0, object arg1, object arg2)
        {
            log.DebugFormat(format, arg0, arg1, arg2);
        }

        public void Debug(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public void Info(string format)
        {
            log.Info(format);
        }

        public void Info(string format, object arg0)
        {
            log.InfoFormat(format, arg0);
        }

        public void Info(string format, object arg0, object arg1)
        {
            log.InfoFormat(format, arg0, arg1);
        }

        public void Info(string format, object arg0, object arg1, object arg2)
        {
            log.InfoFormat(format, arg0, arg1, arg2);
        }

        public void Info(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public void Warn(string format)
        {
            log.Warn(format);
        }

        public void Warn(string format, object arg0)
        {
            log.WarnFormat(format, arg0);
        }

        public void Warn(string format, object arg0, object arg1)
        {
            log.WarnFormat(format, arg0, arg1);
        }

        public void Warn(string format, object arg0, object arg1, object arg2)
        {
            log.WarnFormat(format, arg0, arg1, arg2);
        }

        public void Warn(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public void Warn(Exception error)
        {
            log.Warn(error);
        }

        public void Error(string format)
        {
            log.Error(format);
        }

        public void Error(string format, object arg0)
        {
            log.ErrorFormat(format, arg0);
        }

        public void Error(string format, object arg0, object arg1)
        {
            log.ErrorFormat(format, arg0, arg1);
        }

        public void Error(string format, object arg0, object arg1, object arg2)
        {
            log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void Error(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public void Error(Exception error)
        {
            log.Error(error);
        }

        public void Fatal(string format)
        {
            log.Fatal(format);
        }

        public void Fatal(string format, object arg0)
        {
            log.FatalFormat(format, arg0);
        }

        public void Fatal(string format, object arg0, object arg1)
        {
            log.FatalFormat(format, arg0, arg1);
        }

        public void Fatal(string format, object arg0, object arg1, object arg2)
        {
            log.FatalFormat(format, arg0, arg1, arg2);
        }

        public void Fatal(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }

        public void Fatal(Exception error)
        {
            log.Fatal(error);
        }
    }
}
