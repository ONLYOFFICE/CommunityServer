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
using System.Collections.Concurrent;
using System.Collections.Generic;
using ASC.Common.DependencyInjection;
using Autofac;
using log4net.Config;
using log4net.Core;
using log4net.Util;
using NLog;

namespace ASC.Common.Logging
{
    public interface ILog
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsTraceEnabled { get; }

        void Trace(object message);
        void TraceFormat(string message, object arg0);

        void DebugWithProps(string message, params KeyValuePair<string, object>[] props);
        void Debug(object message);
        void Debug(object message, Exception exception);
        void DebugFormat(string format, params object[] args);
        void DebugFormat(string format, object arg0);
        void DebugFormat(string format, object arg0, object arg1);
        void DebugFormat(string format, object arg0, object arg1, object arg2);
        void DebugFormat(IFormatProvider provider, string format, params object[] args);


        void Info(object message);
        void Info(string message, Exception exception);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(string format, object arg0);
        void InfoFormat(string format, object arg0, object arg1);
        void InfoFormat(string format, object arg0, object arg1, object arg2);
        void InfoFormat(IFormatProvider provider, string format, params object[] args);

        void Warn(object message);
        void Warn(object message, Exception exception);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(string format, object arg0);
        void WarnFormat(string format, object arg0, object arg1);
        void WarnFormat(string format, object arg0, object arg1, object arg2);
        void WarnFormat(IFormatProvider provider, string format, params object[] args);

        void Error(object message);
        void Error(object message, Exception exception);
        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(string format, object arg0);
        void ErrorFormat(string format, object arg0, object arg1);
        void ErrorFormat(string format, object arg0, object arg1, object arg2);
        void ErrorFormat(IFormatProvider provider, string format, params object[] args);

        void Fatal(object message);
        void Fatal(string message, Exception exception);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(string format, object arg0);
        void FatalFormat(string format, object arg0, object arg1);
        void FatalFormat(string format, object arg0, object arg1, object arg2);
        void FatalFormat(IFormatProvider provider, string format, params object[] args);

        string LogDirectory { get; }
    }

    public class Log : ILog
    {
        static Log()
        {
            XmlConfigurator.Configure();
        }

        private readonly log4net.ILog loger;

        public bool IsDebugEnabled { get; private set; }

        public bool IsInfoEnabled { get; private set; }

        public bool IsWarnEnabled { get; private set; }

        public bool IsErrorEnabled { get; private set; }

        public bool IsFatalEnabled { get; private set; }

        public bool IsTraceEnabled { get; private set; }

        public Log(string name)
        {
            loger = log4net.LogManager.GetLogger(name);

            IsDebugEnabled = loger.IsDebugEnabled;
            IsInfoEnabled = loger.IsInfoEnabled;
            IsWarnEnabled = loger.IsWarnEnabled;
            IsErrorEnabled = loger.IsErrorEnabled;
            IsFatalEnabled = loger.IsFatalEnabled;
            IsTraceEnabled = loger.Logger.IsEnabledFor(Level.Trace);
        }

        public void Trace(object message)
        {
            if(IsTraceEnabled) loger.Logger.Log(GetType(), Level.Trace, message, null);
        }

        public void TraceFormat(string message, object arg0)
        {
            if (IsTraceEnabled) loger.Logger.Log(GetType(), Level.Trace, string.Format(message, arg0), null);
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled) loger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled) loger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled) loger.DebugFormat(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            if (IsDebugEnabled) loger.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            if (IsDebugEnabled) loger.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsDebugEnabled) loger.DebugFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsDebugEnabled) loger.DebugFormat(provider, format, args);
        }

        public void DebugWithProps(string message, params KeyValuePair<string, object>[] props)
        {
            if (!IsDebugEnabled) return;

            foreach (var p in props)
            {
                log4net.ThreadContext.Properties[p.Key] = p.Value;
            }

            loger.Debug(message);
        }


        public void Info(object message)
        {
            if (IsInfoEnabled) loger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            if (IsInfoEnabled) loger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled) loger.InfoFormat(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            if (IsInfoEnabled) loger.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            if (IsInfoEnabled) loger.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsInfoEnabled) loger.InfoFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsInfoEnabled) loger.InfoFormat(provider, format, args);
        }


        public void Warn(object message)
        {
            if (IsWarnEnabled) loger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled) loger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled) loger.WarnFormat(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            if (IsWarnEnabled) loger.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            if (IsWarnEnabled) loger.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsWarnEnabled) loger.WarnFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsWarnEnabled) loger.WarnFormat(provider, format, args);
        }


        public void Error(object message)
        {
            if (IsErrorEnabled) loger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled) loger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled) loger.ErrorFormat(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            if (IsErrorEnabled) loger.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            if (IsErrorEnabled) loger.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsErrorEnabled) loger.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsErrorEnabled) loger.ErrorFormat(provider, format, args);
        }


        public void Fatal(object message)
        {
            if (IsFatalEnabled) loger.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            if (IsFatalEnabled) loger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled) loger.FatalFormat(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            if (IsFatalEnabled) loger.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            if (IsFatalEnabled) loger.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsFatalEnabled) loger.FatalFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsFatalEnabled) loger.FatalFormat(provider, format, args);
        }

        public string LogDirectory
        {
            get
            {
                return log4net.GlobalContext.Properties["LogDirectory"].ToString();
            }
        }
    }

    public class LogNLog : ILog
    {
        private readonly NLog.ILogger loger;
        private readonly string name;

        public bool IsDebugEnabled { get; private set; }

        public bool IsInfoEnabled { get; private set; }

        public bool IsWarnEnabled { get; private set; }

        public bool IsErrorEnabled { get; private set; }

        public bool IsFatalEnabled { get; private set; }

        public bool IsTraceEnabled { get; private set; }

        static LogNLog()
        {
            var args = Environment.GetCommandLineArgs();

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "--log" && !string.IsNullOrEmpty(args[i + 1]))
                {
                    NLog.LogManager.Configuration.Variables["svcName"] = args[i + 1].Trim().Trim('"');
                }
            }

            NLog.Targets.Target.Register<SelfCleaningTarget>("SelfCleaning");
        }

        public LogNLog(string name)
        {
            this.name = name;

            loger = NLog.LogManager.GetLogger(name);

            IsDebugEnabled = loger.IsDebugEnabled;
            IsInfoEnabled = loger.IsInfoEnabled;
            IsWarnEnabled = loger.IsWarnEnabled;
            IsErrorEnabled = loger.IsErrorEnabled;
            IsFatalEnabled = loger.IsFatalEnabled;
            IsTraceEnabled = loger.IsEnabled(LogLevel.Trace);
        }

        public void Trace(object message)
        {
            if (IsTraceEnabled) loger.Log(LogLevel.Trace, message);
        }

        public void TraceFormat(string message, object arg0)
        {
            if (IsTraceEnabled) loger.Log(LogLevel.Trace, string.Format(message, arg0));
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled) loger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled) loger.Debug(exception, "{0}", message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled) loger.Debug(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            if (IsDebugEnabled) loger.Debug(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            if (IsDebugEnabled) loger.Debug(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsDebugEnabled) loger.Debug(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsDebugEnabled) loger.Debug(provider, format, args);
        }

        public void DebugWithProps(string message, params KeyValuePair<string, object>[] props)
        {
            if (!IsDebugEnabled) return;

            var theEvent = new LogEventInfo { Message = message, LoggerName = name, Level = LogLevel.Debug };

            foreach (var p in props)
            {
                theEvent.Properties[p.Key] = p.Value;
            }

            loger.Log(theEvent);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled) loger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            if (IsInfoEnabled) loger.Info(exception, message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled) loger.Info(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            if (IsInfoEnabled) loger.Info(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            if (IsInfoEnabled) loger.Info(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsInfoEnabled) loger.Info(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsInfoEnabled) loger.Info(provider, format, args);
        }


        public void Warn(object message)
        {
            if (IsWarnEnabled) loger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled) loger.Warn(exception, "{0}", message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled) loger.Warn(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            if (IsWarnEnabled) loger.Warn(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            if (IsWarnEnabled) loger.Warn(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsWarnEnabled) loger.Warn(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsWarnEnabled) loger.Warn(provider, format, args);
        }


        public void Error(object message)
        {
            if (IsErrorEnabled) loger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled) loger.Error(exception, "{0}", message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled) loger.Error(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            if (IsErrorEnabled) loger.Error(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            if (IsErrorEnabled) loger.Error(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsErrorEnabled) loger.Error(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsErrorEnabled) loger.Error(provider, format, args);
        }


        public void Fatal(object message)
        {
            if (IsFatalEnabled) loger.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            if (IsFatalEnabled) loger.Fatal(exception, message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled) loger.Fatal(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            if (IsFatalEnabled) loger.Fatal(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            if (IsFatalEnabled) loger.Fatal(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsFatalEnabled) loger.Fatal(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsFatalEnabled) loger.Fatal(provider, format, args);
        }

        public string LogDirectory { get { return NLog.LogManager.Configuration.Variables["logDirectory"].Text; } }
    }

    public class NullLog : ILog
    {
        public bool IsDebugEnabled { get; set; }
        public bool IsInfoEnabled { get; set; }
        public bool IsWarnEnabled { get; set; }
        public bool IsErrorEnabled { get; set; }
        public bool IsFatalEnabled { get; set; }
        public bool IsTraceEnabled { get; set; }

        public void Trace(object message)
        {
        }

        public void TraceFormat(string message, object arg0)
        {
        }

        public void DebugWithProps(string message, params KeyValuePair<string, object>[] props)
        {
        }

        public void Debug(object message)
        {
        }

        public void Debug(object message, Exception exception)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void DebugFormat(string format, object arg0)
        {
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Info(object message)
        {
        }

        public void Info(string message, Exception exception)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void InfoFormat(string format, object arg0)
        {
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Warn(object message)
        {
        }

        public void Warn(object message, Exception exception)
        {
        }

        public void WarnFormat(string format, params object[] args)
        {
        }

        public void WarnFormat(string format, object arg0)
        {
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Error(object message)
        {
        }

        public void Error(object message, Exception exception)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void ErrorFormat(string format, object arg0)
        {
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Fatal(object message)
        {
        }

        public void Fatal(string message, Exception exception)
        {
        }

        public void FatalFormat(string format, params object[] args)
        {
        }

        public void FatalFormat(string format, object arg0)
        {
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public string LogDirectory { get { return ""; } }
    }

    public class LogManager
    {
        internal static IContainer Builder { get; set; }

        internal static ConcurrentDictionary<string, ILog> Logs;

        static LogManager()
        {
            var container = AutofacConfigLoader.Load("core");
            if (container != null)
            {
                Builder = container.Build();
            }

            Logs = new ConcurrentDictionary<string, ILog>();
        }

        public static ILog GetLogger(string name)
        {
            ILog result;
            if (!Logs.TryGetValue(name, out result))
            {
                result = Logs.AddOrUpdate(name, Builder != null ? Builder.Resolve<ILog>(new TypedParameter(typeof(string), name)) : new NullLog(), (k, v) => v);
            }

            return result;
        }
    }
}
