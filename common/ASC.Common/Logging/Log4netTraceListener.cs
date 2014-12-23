/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using log4net;
using log4net.Core;
using System.Diagnostics;
using System.Text;

namespace ASC.Common.Logging
{
    public class Log4netTraceListener : TraceListener
    {
        private readonly ILog log;
        private Level level;
        private StringBuilder cache = new StringBuilder();


        public Log4netTraceListener(string name)
        {
            log = LogManager.GetLogger(name);
            log.DebugFormat("Create Log4netTraceListener {0}", name);
        }


        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (SetLevel(eventType))
            {
                base.TraceData(eventCache, source, eventType, id, data);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (SetLevel(eventType))
            {
                base.TraceData(eventCache, source, eventType, id, data);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (SetLevel(eventType))
            {
                base.TraceEvent(eventCache, source, eventType, id);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (SetLevel(eventType))
            {
                base.TraceEvent(eventCache, source, eventType, id, format, args);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (SetLevel(eventType))
            {
                base.TraceEvent(eventCache, source, eventType, id, message);
            }
        }


        public override void Write(string message)
        {
            if (log.Logger.IsEnabledFor(level ?? Level.Debug))
            {
                cache.Append(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (log.Logger.IsEnabledFor(level ?? Level.Debug))
            {
                message = cache.Append(message).ToString();
                cache.Clear();
                if (level == Level.Debug)
                {
                    log.Debug(message);
                }
                else if (level == Level.Info)
                {
                    log.Info(message);
                }
                else if (level == Level.Error)
                {
                    log.Error(message);
                }
                else if (level == Level.Fatal)
                {
                    log.Fatal(message);
                }
            }
        }


        private bool SetLevel(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Critical: level = Level.Fatal; break;
                case TraceEventType.Error: level = Level.Error; break;
                case TraceEventType.Warning: level = Level.Warn; break;
                case TraceEventType.Information: level = Level.Info; break;
                default: level = Level.Debug; break;
            }
            return log.Logger.IsEnabledFor(level);
        }
    }
}
