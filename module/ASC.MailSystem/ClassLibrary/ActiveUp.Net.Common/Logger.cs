// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
#if !PocketPC
using System.Windows.Forms;
using Microsoft.Win32;
#endif
using System.Text;
using System.IO;
using log4net;
using log4net.Core;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Provides all logging facilities for any applications.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Logger
    {
        private static string _logFile = string.Empty;
        private static ArrayList _logEntries = new ArrayList();
        private static bool _logInMemory = false, _disabled = false, _useTraceContext = false, _useTraceConsole = false;
        private static int _logLevel = 0;
        //private static bool _isChecked = false;
                
        /// <summary>
        /// The default constructor.
        /// </summary>
        public Logger()
        {
            //_logFile = string.Empty;
            //_logInMemory = true;
            //_logLevel = 0;

//#if TRIAL
//            if (DateTime.Now > new DateTime(2007, 6, 30))
//                throw new Exception("Trial Version Expired. Please register.");
//#endif
        }

        /*/// <summary>
        /// Creates an instance of the logger.
        /// </summary>
        /// <param name="logFile">The log file.</param>
        /// <param name="traceContext">The trace context to use.</param>
        public static Logger(string logFile, System.Web.TraceContext traceContext)
        {
            _logFile = logFile;
            _logInMemory = true;
            _logLevel = 0;
        }*/

        /// <summary>
        /// Gets or sets the log entries that are stored in the memory.
        /// </summary>
        public static ArrayList LogEntries
        {
            get
            {
                if (_logEntries == null)
                    _logEntries = new ArrayList();
                return _logEntries;
            }
            set
            {
                _logEntries = value;
            }
        }

        /// <summary>
        /// Gets or sets the logging level.
        /// </summary>
        public static int LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        /// <summary>
        /// Specify whether if you want to log in memory.
        /// </summary>
        public static bool LogInMemory
        {
            get
            {
                return _logInMemory;
            }
            set
            {
                _logInMemory = value;
            }
        }

        /// <summary>
        /// Gets or sets the full path to the text file to append when logging.
        /// </summary>
        public static string LogFile
        {
            get
            {
                return _logFile;
            }
            set
            {
                _logFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the full path to the text file to append when logging.
        /// </summary>
        public static ILog Log4NetLogger { get; set; }

        /// <summary>
        /// Add a log entry using the logging level.
        /// </summary>
        /// <param name="line">The entry to add.</param>
        /// <param name="level">The log entry level.</param>
        public static void AddEntry(string line, int level)
        {
            if (!_disabled)
            {
                if (level >= _logLevel)
                    AddEntry(line);
            }
        }

        /// <summary>
        /// Add a log entry in all logging objects availables.
        /// </summary>
        /// <param name="line">The entry to add.</param>
        public static void AddEntry(string line)
        {
            if (_disabled) return;

            if (Log4NetLogger != null)
            {
                AddEntryToLogger(line);
            }
            else
            {
                var now = DateTime.UtcNow;
                var logString = new StringBuilder();
                logString.Append(now.Year.ToString());
                logString.Append(".");
                logString.Append(now.Month.ToString().PadLeft(2, '0'));
                logString.Append(".");
                logString.Append(now.Day.ToString().PadLeft(2, '0'));
                logString.Append("-");
                logString.Append(now.Hour.ToString().PadLeft(2, '0'));
                logString.Append(":");
                logString.Append(now.Minute.ToString().PadLeft(2, '0'));
                logString.Append(":");
                logString.Append(now.Second.ToString().PadLeft(2, '0'));
                logString.Append(" ");
                logString.Append(line);

                if (!string.IsNullOrEmpty(_logFile))
                    AddEntryToFile(logString.ToString());

#if !PocketPC
                if (_useTraceContext)
                    AddEntryToTrace(logString.ToString());
#endif

                if (_useTraceConsole)
                    AddEntryToConsole(logString.ToString());

            }

            OnEntryAdded(EventArgs.Empty);
        }

        /// <summary>
        /// Append the logging text file.
        /// </summary>
        /// <param name="line">The entry to add.</param>
        protected static void AddEntryToLogger(string line)
        {
            if (!_disabled)
            {
                Log4NetLogger.Debug(line);
            }
        }

        /// <summary>
        /// Append the logging text file.
        /// </summary>
        /// <param name="line">The entry to add.</param>
        protected static void AddEntryToFile(string line)
        {
            if (!_disabled)
            {
                using (var fileWriter = new StreamWriter(_logFile, true, Encoding.Default))
                {
                    fileWriter.WriteLine(line);
                    fileWriter.Close();
                }
            }
        }

        protected static void AddEntryToConsole(string line)
        {
            if (!_disabled)
            {
                Console.WriteLine("ActiveMail:{0}",line);
            }
        }

#if !PocketPC
        /// <summary>
        /// Append the trace context.
        /// </summary>
        /// <param name="line">The entry to add.</param>
        protected static void AddEntryToTrace(string line)
        {
            if (!_disabled)
            {
                if (System.Web.HttpContext.Current != null)
                    System.Web.HttpContext.Current.Trace.Write("ActiveMail", line);
            }
        }
#endif

        /// <summary>
        /// Gets an ArrayList containing the specified number of last entries.
        /// </summary>
        /// <param name="lines">The max lines to retrieve.</param>
        /// <returns>An ArrayList containing the maximum log entries.</returns>
        public static ArrayList LastEntries(int lines)
        {
            ArrayList entries = new ArrayList();
            int index, count = Count;

            if (count > 0)
            {

                for(index=count-lines;index<=count;index++)
                {
                    if (index>=0)
                    {
                        if (_logEntries != null)
                            entries.Add(_logEntries[index]);
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// Gets a string containing a maximum of 30 log entries.
        /// </summary>
        /// <returns>A maximum of 30 entries separeted by a carriage return.</returns>
        public static string LastEntries()
        {
            ArrayList entries = LastEntries(30);

            var stringEntries = new StringBuilder();

            foreach(string entry in entries)
                stringEntries.Append(entry).Append("\n");

            return stringEntries.ToString();
        }

        /// <summary>
        /// Gets the last entry of the log.
        /// </summary>
        /// <returns>A string containing the last entry.</returns>
        public static string LastEntry()
        {
            if (_logEntries != null)
                return _logEntries[_logEntries.Count-1].ToString();

            return string.Empty;
        }

        /// <summary>
        /// Gets the number of log entries.
        /// </summary>
        public static int Count
        {
            get
            {
                if (_logEntries != null)
                    return _logEntries.Count;

                return 0;
            }
        }

        /// <summary>
        /// Specify whether if the logger needs to append the Trace Context.
        /// </summary>
        public static bool UseTraceContext
        {
            get
            {
                return _useTraceContext;
            }
            set
            {
                _useTraceContext = value;
            }
        }

        public static bool UseTraceConsole
        {
            get 
            {
                return _useTraceConsole;
            }

            set
            {
                _useTraceConsole = value;
            }
        }

        /// <summary>
        /// Specify whether if the logging functions are disabled.
        /// </summary>
        public static bool Disabled
        {
            get
            {
                return _disabled;
            }
            set
            {
                _disabled = value;
            }
        }

        /// <summary>
        /// The EntryAdded event handler.
        /// </summary>
        public static event EventHandler EntryAdded;

        /// <summary>
        /// OnEntryAdded event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected static void OnEntryAdded(EventArgs e) 
        {
            if (EntryAdded != null)
                EntryAdded(null,e);
        }

    }
}
