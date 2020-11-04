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


namespace ASC.Mail.Net.Log
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for <b>Logger.WriteLog</b> event.
    /// </summary>
    public class WriteLogEventArgs : EventArgs
    {
        #region Members

        private readonly LogEntry m_pLogEntry;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logEntry">New log entry.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>logEntry</b> is null.</exception>
        public WriteLogEventArgs(LogEntry logEntry)
        {
            if (logEntry == null)
            {
                throw new ArgumentNullException("logEntry");
            }

            m_pLogEntry = logEntry;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets new log entry.
        /// </summary>
        public LogEntry LogEntry
        {
            get { return m_pLogEntry; }
        }

        #endregion
    }
}