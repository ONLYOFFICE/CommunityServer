/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository;

namespace ASC.Mail.Aggregator.Common.Logging
{
    public class Log4NetLogger : ILogger
    {
        private readonly log4net.ILog _logger;

        public Log4NetLogger(log4net.ILog logger)
        {
            _logger = logger;
        }

        public void Fatal(string message, params object[] param)
        {
            var str = String.Format(message, param);
            _logger.Fatal(str);
        }

        public void Fatal(Exception ex, string message, params object[] param)
        {
            var original_message = String.Format(message, param);
            _logger.Fatal(original_message + " Exception: " + ex);
        }

        public void Error(string message, object[] param)
        {
            var str = String.Format(message, param);
            _logger.Error(str);
        }

        public void Error(Exception ex, string message, params object[] param)
        {
            var original_message = String.Format(message, param);
            _logger.Error(original_message + " Exception: " + ex);
        }

        public void Warn(string message, object[] param)
        {
            var str = String.Format(message, param);
            _logger.Warn(str);
        }

        public void Debug(string message, object[] param)
        {
            var str = String.Format(message, param);
            _logger.Debug(str);
        }

        public void WarnException(string format, Exception exception)
        {
            _logger.Warn(format, exception);
        }

        public void Info(string message, params object[] param)
        {
            var str = String.Format(message, param);
            _logger.Info(str);
        }

        public void Flush()
        {
            ILoggerRepository rep = LogManager.GetRepository();
            foreach (var buffered in rep.GetAppenders().OfType<BufferingAppenderSkeleton>())
            {
                buffered.Flush();
            }
        }
    }
}
