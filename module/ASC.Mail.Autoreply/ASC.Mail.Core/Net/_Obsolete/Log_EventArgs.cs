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


namespace ASC.Mail.Net
{
    /// <summary>
    /// Provides data for the SessionLog event.
    /// </summary>
    public class Log_EventArgs
    {
        #region Members

        private readonly bool m_FirstLogPart = true;
        private readonly bool m_LastLogPart;
        private readonly SocketLogger m_pLoggger;

        #endregion

        #region Properties

        /// <summary>
        /// Gets log text.
        /// </summary>
        public string LogText
        {
            get { return SocketLogger.LogEntriesToString(m_pLoggger, m_FirstLogPart, m_LastLogPart); }
        }

        /// <summary>
        /// Gets logger.
        /// </summary>
        public SocketLogger Logger
        {
            get { return m_pLoggger; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger">Socket logger.</param>
        /// <param name="firstLogPart">Specifies if first log part of multipart log.</param>
        /// <param name="lastLogPart">Specifies if last log part (logging ended).</param>
        public Log_EventArgs(SocketLogger logger, bool firstLogPart, bool lastLogPart)
        {
            m_pLoggger = logger;
            m_FirstLogPart = firstLogPart;
            m_LastLogPart = lastLogPart;
        }

        #endregion
    }
}